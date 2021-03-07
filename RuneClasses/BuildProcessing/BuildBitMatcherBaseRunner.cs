using RuneOptim.swar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
    public class BuildBitMatcherBaseRunner : BuildRunner<bool> {
        public static int NumberOfSetBits(int i) {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        // Enumerate all possible m-size combinations of [0, 1, ..., n-1] array
        // in lexicographic order (first [0, 1, 2, ..., m-1]).
        private static IEnumerable<int[]> CombinationsRosettaWoRecursion(int m, int n) {
            int[] result = new int[m];
            Stack<int> stack = new Stack<int>(m);
            stack.Push(0);
            while (stack.Count > 0) {
                int index = stack.Count - 1;
                int value = stack.Pop();
                while (value < n) {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index != m) continue;
                    yield return (int[])result.Clone(); // thanks to @xanatos
                                                        //yield return result;
                    break;
                }
            }
        }

        public static IEnumerable<T[]> CombinationsRosettaWoRecursion<T>(T[] array, int m) {
            if (array.Length < m)
                throw new ArgumentException("Array length can't be less than number of selected elements");
            if (m < 1)
                throw new ArgumentException("Number of selected elements can't be less than 1");
            return combinationsRosettaWoRecursion(array, m);
        }

        private static IEnumerable<T[]> combinationsRosettaWoRecursion<T>(T[] array, int m) {
            T[] result = new T[m];
            foreach (int[] j in CombinationsRosettaWoRecursion(m, array.Length)) {
                for (int i = 0; i < m; i++) {
                    result[i] = array[j[i]];
                }
                yield return (T[])result.Clone();
            }
        }

        /// <summary>
        /// Find all the pairs of 2 and sets of 4 that fit together to make a build.
        /// Yield-returns because it can be too big to fit into memory.
        /// </summary>
        /// <param name="b2"></param>
        /// <param name="b4"></param>
        /// <returns></returns>
        public IEnumerable<LoadBits> getMatches(IEnumerable<BitLoad> b2, IEnumerable<BitLoad> b4) {

            foreach (var b_2 in b2) {
                foreach (var b_4 in b4) {
                    if ((b_2.bit | b_4.bit) == 63)
                        yield return new LoadBits() { a = b_2, b = b_4 };
                }
            }

            var doTwos = !build.BuildSets.Any(se => Rune.SetRequired(se) == 4);
            if (build.RequiredSets.Any() && !build.RequiredSets.Any(se => Rune.SetRequired(se) == 4))
                doTwos = true;

            // try to get 3 sets of 2s
            if (doTwos) {
                var group2s = b2.GroupBy(k => k.bit).ToArray();
                if (group2s.Length < 3)
                    yield break;

                foreach (var thar in CombinationsRosettaWoRecursion(group2s, 3)) {
                    if ((thar[0].Key | thar[1].Key | thar[2].Key) == 63) {
                        foreach (var a in thar[0]) {
                            foreach (var b in thar[1]) {
                                foreach (var c in thar[2])
                                    yield return new LoadBits() { a = a, b = b, c = c };
                            }
                        }
                    }
                }
            }
        }

        protected loopData runPicker(LoadBits load, ParallelLoopState state, loopData data) {

            if (!isRunning || state.IsStopped || state.ShouldExitCurrentIteration || state.IsExceptional) {
                state.Stop();
                return data;
            }

            foreach (var rune in load.a.Runes) {
                data.Mon.ApplyRune(rune, 7);
            }
            foreach (var rune in load.b.Runes) {
                data.Mon.ApplyRune(rune, 7);
            }
            if (load.c != null)
                foreach (var rune in load.c.Runes) {
                    data.Mon.ApplyRune(rune, 7);
                }

            bake(data, true);

            return data;
        }
        
        protected virtual void MakeBitSets(IEnumerable<Rune> inRunes, out BitLoad[] twos, out BitLoad[] fours) {

            var rr = this.messupRunes(inRunes);

            var sets = rr.SelectMany(r => r).GroupBy(r => r.Set);

            HashSet<BitLoad> bLoads = new HashSet<BitLoad>();

            foreach (var gb in sets) {
                var ii = Rune.SetRequired(gb.Key);
                var gba = gb.ToArray();
                if (gba.Length < ii)
                    continue;
                this.total += PermutationsAndCombinations.nCr(gba.Length, ii);

                // yield-return sort-of works in parallel
                Parallel.ForEach(CombinationsRosettaWoRecursion<Rune>(gba, ii), () => new HashSet<BitLoad>(), (rc, i, a) => {
                    var bl = new BitLoad(gb.Key);
                    bl.Runes = rc;
                    foreach (var r in rc) {
                        bl.bit |= 1 << (r.Slot - 1);
                    }
                    if (NumberOfSetBits(bl.bit) == ii) {
                        a.Add(bl);
                        this.plus++;
                    }
                    else
                        this.kill++;
                    return a;
                }, hs => {
                    lock (bLoads) {
                        foreach (var r in hs)
                            bLoads.Add(r);
                    }
                }
                );
            }
            
            this.total = 0;
            this.kill = 0;
            this.plus = 0;

            twos = bLoads.Where(b => Rune.SetRequired(b.Set) == 2).ToArray();
            fours = bLoads.Where(b => Rune.SetRequired(b.Set) == 4).ToArray();
        }

        
        public override Task<Monster> Run(IEnumerable<Rune> inRunes) {

            MakeBitSets(inRunes, out BitLoad[] b2, out BitLoad[] b4);

            long tt = 0;

            skip = b2.AsParallel().Sum(__b => b4.Count(__c => (__b.bit | __c.bit) == 63));

            var doTwos = !build.BuildSets.Any(se => Rune.SetRequired(se) == 4);
            if (build.RequiredSets.Any() && !build.RequiredSets.Any(se => Rune.SetRequired(se) == 4))
                doTwos = true;


            if (doTwos) {

                var qrhad = b2.GroupBy(k => k.bit).ToArray();

                if (qrhad.Length >= 3) {

                    foreach (var thar in CombinationsRosettaWoRecursion(qrhad, 3)) {
                        if ((thar[0].Key | thar[1].Key | thar[2].Key) == 63)
                            skip += thar[0].Count() * thar[1].Count() * thar[2].Count();
                    }
                }
            }

            tt = skip;
            total = tt;

            var matches = getMatches(b2, b4);

            cts = new CancellationTokenSource();

            var options = new ParallelOptions() {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
            };

            SynchronizedCollection<Monster> bag = new SynchronizedCollection<Monster>();

            currentScore = build.CalcScore(build.Mon);

            build.Mon.GetStats(true);
            build.Mon.BakeDamageFormula();

            preRun();

            isRunning = true;

            return Task.Run(() => {
                var c = build.Maximum.NonZeroCached;
                //var result = 
                Parallel.ForEach(matches, options,
                    () => {
                        var data = new loopData {
                            Mon = new Monster(build.Mon),
                            Minimum = new Stats(build.Minimum, true),
                            kill = 0,
                            plus = 0,
                            index = 0,
                            list = new List<Monster>(),
                            settings = this.settings,
                            Maximum = new Stats(build.Maximum, true),
                        };

                        data.Mon.ExtraCritRate = build.ExtraCritRate;

                        int?[] slotFakesTemp = new int?[6];
                        bool[] slotPred = new bool[6];
                        build.getPrediction(slotFakesTemp, slotPred);

                        int[] slotFakes = slotFakesTemp.Select(i => i ?? 0).ToArray();


                        data.Mon.Current.Buffs = build.Buffs;
                        data.Mon.Current.Shrines = data.settings.Shrines;
                        data.Mon.Current.Leader = build.Leader;

                        data.Mon.Current.FakeLevel = slotFakes;
                        data.Mon.Current.PredictSubs = slotPred;
                        data.Mon.Current.TempLoad = true;

                        return data;
                    },
                    runPicker,
                    data => {
                        progIt(data);
                        foreach (var m in data.list) {
                            bag.Add(m);
                        }
                        data.list.Clear();

                        lock (bag.SyncRoot) {
                            var tn = Math.Max(settings.BuildGenerate, 250000);
                            if (bag.Count > tn) {
                                var bb = bag.OrderByDescending(b => b.score).Take(25000).ToList();
                                bag.Clear();
                                bag.AddRange(bb);

                            }
                        }

                        if (settings.BuildGenerate > 0) {
                            if (bag.Count >= settings.BuildGenerate) {
                                isRunning = false;
                            }
                        }
                    });
                try {
                    var bgood = bag.OrderByDescending(a => build.CalcScore(a.GetStats()));
                    best = bgood.FirstOrDefault();

                    foreach (var ll in bgood.Take(settings.BuildTake))
                        build.Loads.Add(ll);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }

                return best;
            });
        }

    }

    /// <summary>
    /// Turn the slots the given runes into a bitmask
    /// </summary>
    public class BitLoad {
        public int bit;

        public Rune[] Runes;

        public RuneSet Set;
        public BitLoad(RuneSet set) {
            Set = set;
        }

        public override bool Equals(object obj) {
            return obj is BitLoad load &&
                   EqualityComparer<Rune[]>.Default.Equals(Runes, load.Runes) &&
                   Set == load.Set;
        }

        public override int GetHashCode() {
            var hashCode = -504466931;
            hashCode = hashCode * -1521134295 + EqualityComparer<Rune[]>.Default.GetHashCode(Runes);
            hashCode = hashCode * -1521134295 + Set.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            return bit + ":" + (Runes?.FirstOrDefault()?.Set.ToString() ?? "") + " " + string.Join(", ", Runes?.Select(r => r.Id));
        }

    }
    
    /// <summary>
    /// A set of runes which bitmask is 111111 => 63
    /// </summary>
    public class LoadBits {
        public BitLoad a;
        public BitLoad b;
        public BitLoad c;
    }

}
