using RuneOptim.swar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
    public abstract class BuildRunner<T> : IBuildRunner {

        volatile protected Monster best;
        volatile protected bool isRunning;
        protected readonly object nlock = new object();
        protected long kill;
        protected long plus;
        protected long skip;

        protected Rune[][] runes;
        protected double bestScore = 0;
        protected double currentScore;
        protected long total;
        protected CancellationTokenSource cts;
        public long Expected { get => total; }
        public long Completed { get => kill + plus; }
        public long Good { get => plus; }
        public long Skipped { get => skip; }

        public void Cancel() {
            //cts?.Cancel();
            isRunning = false;
        }

        protected Build build;
        protected BuildSettings settings;

        public void Setup(Build build, BuildSettings settings) {
            this.build = build;
            this.settings = settings;
        }


        public void TearDown() {
            isRunning = false;
        }


        protected class loopData {
            internal Monster Mon;
            internal Stats Minimum;
            internal int kill;
            internal int skip;
            internal int plus;
            internal List<Monster> list;
            internal Stats Maximum;
            internal BuildSettings settings;
            internal int index;
        }

        protected void progIt(loopData data) {
            lock (nlock) {
                kill += data.kill;
                data.kill = 0;
                plus += data.plus;
                data.plus = 0;
                skip += data.skip;
                data.skip = 0;
            }
        }

        protected virtual Rune[][] messupRunes(IEnumerable<Rune> inRunes) {
            return inRunes.GroupBy(r => r.Slot).OrderBy(r => r.Key).Select(r => r.ToArray()).ToArray();

        }

        public virtual void preRun() {

        }

        public virtual Task<Monster> Run(IEnumerable<Rune> inRunes) {

            runes = messupRunes(inRunes);

            total = runes[0].Length;
            total *= runes[1].Length;
            total *= runes[2].Length;
            total *= runes[3].Length;
            total *= runes[4].Length;
            total *= runes[5].Length;

            cts = new CancellationTokenSource();

            var options = new ParallelOptions() {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
            };

            SynchronizedCollection<Monster> bag = new SynchronizedCollection<Monster>();

            currentScore = build.CalcScore(build.Mon);

            var s = build.Mon.GetStats(true);
            var d = build.Mon.DamageFormula;

            preRun();

            isRunning = true;

            // TODO: async
            return Task.Run(() => {
                var c = build.Maximum.NonZeroCached;
                //var result = 
                Parallel.ForEach(runes[0], options,
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

                        data.Mon.Current.FakeLevel = slotFakes;
                        data.Mon.Current.PredictSubs = slotPred;
                        data.Mon.Current.TempLoad = true;

                        return data;
                    },
                    runLoop,
                    data => {
                        foreach (var m in data.list) {
                            bag.Add(m);
                        }
                        data.list.Clear();
                        progIt(data);

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


        protected virtual void bake(loopData data, T tdat) {
            data.Mon.Current.CheckSets();
            var cstats = data.Mon.GetStats();
            bool isBad = false;

            // check if build meets minimum
            isBad |= !data.settings.RunesOnlyFillEmpty && !data.settings.AllowBroken && !data.Mon.Current.SetsFull;

            isBad |= data.Maximum != null && cstats.AnyExceedCached(data.Maximum);

            isBad |= !data.settings.RunesOnlyFillEmpty && data.Minimum != null && !cstats.GreaterEqual(data.Minimum, true);

            double curScore = 0;

            if (!isBad) {
                // try to reduce CalcScore hits
                curScore = build.CalcScore(cstats);
                isBad |= data.settings.IgnoreLess5 && curScore < currentScore * 1.05;
            }

            if (!isBad && curScore > bestScore) {
                bestScore = curScore;
                // we found an okay build!
                data.plus++;
                var mm = new Monster(data.Mon, true);
                mm.score = curScore;
                data.list.Add(mm);
            }
            else {
                data.kill++;
            }
        }


        protected virtual bool prePick(int s, Rune r, ParallelLoopState state, loopData data, T tdat) {
            return false;
        }


        protected const long bigNum = 5 * 7 * 11 * 13 * 15 * 17;

        protected void pick(int s, ParallelLoopState state, loopData data, T tdat) {
            if (!isRunning || state.IsStopped || state.ShouldExitCurrentIteration || state.IsExceptional) {
                state.Stop();
                return;
            }

            if (s < 6) {
                foreach (var r in runes[s]) {
                    if (prePick(s, r, state, data, tdat))
                        continue;

                    data.Mon.ApplyRune(r, 7);
                    pick(s + 1, state, data, tdat);
                }
            }
            else {
                bake(data, tdat);
                if (data.index % bigNum == bigNum - 1) {
                    this.progIt(data);
                }
                data.index++;
            }
        }


        protected virtual loopData runLoop(Rune rune, ParallelLoopState state, long ind, loopData data) {
            if (!isRunning || state.IsStopped || state.ShouldExitCurrentIteration || state.IsExceptional) {
                state.Stop();
                return data;
            }

            data.Mon.Current.Buffs = build.Buffs;
            data.Mon.Current.Shrines = data.settings.Shrines;
            data.Mon.Current.Leader = build.Leader;

            var tt = preLoop();
            
            data.Mon.ApplyRune(rune, 7);

            pick(1, state, data, tt);

            return data;
        }


        protected virtual T preLoop() {
            return default(T);
        }

    }
}
