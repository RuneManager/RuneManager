using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace RuneOptim
{

    public class RuneUsage
    {
        // the runes legit used 
        public ConcurrentDictionary<Rune, byte> runesUsed = new ConcurrentDictionary<Rune, byte>();
        // runes which where in a passing build
        public ConcurrentDictionary<Rune, byte> runesGood = new ConcurrentDictionary<Rune, byte>();
        
        // the runes that got in the winning builds when runesUsed got banned
        public ConcurrentDictionary<Rune, byte> runesSecond = new ConcurrentDictionary<Rune, byte>();
        // runes which got generated while banning runesUsed --> goodRunes
        public ConcurrentDictionary<Rune, byte> runesOkay = new ConcurrentDictionary<Rune, byte>();
        public ConcurrentDictionary<Rune, byte> runesBetter = new ConcurrentDictionary<Rune, byte>();
    }

    public class BuildUsage
    {
        public int failed = 0;
        public int passed = 0;
        public List<Monster> loads;
    }


    // The heavy lifter
    // Contains most of the data needed to outline build requirements
    public class Build
    {
		// allows iterative code, probably slow but nice to write and integrates with WinForms at a moderate speed
		[Obsolete("Consider changing to statEnums")]
		public static string[] statNames = { "HP", "ATK", "DEF", "SPD", "CR", "CD", "RES", "ACC" };
		public static Attr[] statEnums = { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy };
		public static Attr[] statBoth = { Attr.HealthFlat, Attr.HealthPercent, Attr.AttackFlat, Attr.AttackPercent, Attr.DefenseFlat, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy };
        [Obsolete("Consider changing to extraEnums")]
        public static string[] extraNames = { "EHP", "EHPDB", "DPS", "AvD", "MxD" };
        public static Attr[] extraEnums = { Attr.EffectiveHP, Attr.EffectiveHPDefenseBreak, Attr.DamagePerSpeed, Attr.AverageDamage, Attr.MaxDamage };
        public static Attr[] statAll = { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy, Attr.EffectiveHP, Attr.EffectiveHPDefenseBreak, Attr.DamagePerSpeed, Attr.AverageDamage, Attr.MaxDamage };

        public Build()
        {
            // for all 6 slots, init the list
            for (int i = 0; i < slotStats.Length; i++)
            {
                slotStats[i] = new List<string>();
            }
        }

        public Build(Monster m)
        {
            // for all 6 slots, init the list
            for (int i = 0; i < slotStats.Length; i++)
            {
                slotStats[i] = new List<string>();
            }
            mon = m;
            var load = mon.Current;
            if (load == null)
                return;
            
            // currently equipped stats
            var cstats = load.GetStats(mon);
            // base stats
            var bstats = mon;
            // stat difference
            var dstats = cstats - bstats;
            // percentage of each stat buffed
            var astats = dstats / bstats;
            foreach (Attr a in statEnums)
            {
                if (astats[a] > 0.1)
                {
                    Minimum[a] = Math.Floor(bstats[a] * (1 + astats[a] * 0.8));
                }
            }
            foreach (var s in mon.Current.Sets)
            {
                if (s != RuneSet.Null && Rune.MagicalSets.Contains((s)))
                {
                    RequiredSets.Add(s);
                }
            }
        }

        [JsonProperty("id")]
        public int ID = 0;

        [JsonProperty("version")]
        public int VERSIONNUM;

        [JsonProperty("MonName")]
        public string MonName;

        [JsonProperty("priority")]
        public int priority;

        [JsonIgnore]
        public Monster mon;

        [JsonProperty("new")]
        public bool New;

        [JsonProperty("downloadstats")]
        public bool DownloadStats;

        [JsonProperty("downloadawake")]
        public bool DownloadAwake;

        // Magical (and probably bad) tree structure for rune slot stat filters
        // tab, stat, FILTER
        [JsonProperty("runeFilters")]
        public Dictionary<string, Dictionary<string, RuneFilter>> runeFilters = new Dictionary<string, Dictionary<string, RuneFilter>>();

        public bool ShouldSerializeruneFilters()
        {
            Dictionary<string, Dictionary<string, RuneFilter>> nfilters = new Dictionary<string, Dictionary<string, RuneFilter>>();
            foreach (var tabPair in runeFilters)
            {
                List<string> keep = new List<string>();
                foreach (var statPair in tabPair.Value)
                {
                    if (statPair.Value.NonZero)
                        keep.Add(statPair.Key);
                }
                Dictionary<string, RuneFilter> n = new Dictionary<string, RuneFilter>();
                foreach (var key in keep)
                    n.Add(key, tabPair.Value[key]);
                if (n.Count > 0)
                    nfilters.Add(tabPair.Key, n);
            }
            runeFilters = nfilters;

            return true;
        }

        // For when you want to map 2 pieces of info to a key, just be *really* lazy
        // Contains the scoring type (OR, AND, SUM) and the[(>= SUM] value
        // tab, TYPE, test
        [JsonProperty("runeScoring")]
        public Dictionary<string, KeyValuePair<int, double?>> runeScoring = new Dictionary<string, KeyValuePair<int, double?>>();

        public bool ShouldSerializeruneScoring()
        {
            Dictionary<string, KeyValuePair<int, double?>> nscore = new Dictionary<string, KeyValuePair<int, double?>>();
            foreach (var tabPair in runeScoring)
            {
                if (tabPair.Value.Key != 0 || tabPair.Value.Value != null)
                    nscore.Add(tabPair.Key, new KeyValuePair<int, double?>(tabPair.Value.Key, tabPair.Value.Value));
            }
            runeScoring = nscore;

            return true;
        }

        // if to raise the runes level, and use the appropriate main stat value.
        // also, attempt to give weight to unassigned powerup bonuses
        // tab, RAISE, magic
        [JsonProperty("runePrediction")]
        public Dictionary<string, KeyValuePair<int, bool>> runePrediction = new Dictionary<string, KeyValuePair<int, bool>>();

        public bool ShouldSerializerunePrediction()
        {
            Dictionary<string, KeyValuePair<int, bool>> npred = new Dictionary<string, KeyValuePair<int, bool>>();
            foreach (var tabPair in runePrediction)
            {
                if (tabPair.Value.Key != 0 || tabPair.Value.Value)
                    npred.Add(tabPair.Key, new KeyValuePair<int, bool>(tabPair.Value.Key, tabPair.Value.Value));
            }
            runePrediction = npred;

            return true;
        }

        [JsonProperty("AllowBroken")]
        public bool AllowBroken = false;

        // how much each stat is worth (0 = useless)
        // eg. 300 hp is worth 1 speed
        [JsonProperty("Sort")]
        public Stats Sort = new Stats();

        public bool ShouldSerializeSort()
        {
            return Sort.NonZero();
        }

        // resulting build must have every set in this array
        [JsonProperty("RequiredSets")]
        public List<RuneSet> RequiredSets = new List<RuneSet>();

        // builds *must* have *all* of these stats
        [JsonProperty("Minimum")]
        public Stats Minimum = new Stats();

        // builds *mustn't* exceed *any* of these stats
        [JsonProperty("Maximum")]
        public Stats Maximum = new Stats();

        // builds with individual stats exceeding these values are penalised as wasteful
        [JsonProperty("Threshold")]
        public Stats Threshold = new Stats();

        public bool ShouldSerializeMinimum()
        {
            return Minimum.NonZero();
        }

        public bool ShouldSerializeMaximum()
        {
            return Maximum.NonZero();
        }

        public bool ShouldSerializeThreshold()
        {
            return Threshold.NonZero();
        }

        [JsonProperty]
        public List<string> Teams = new List<string>();
        public bool ShouldSerializeTeams()
        {
            return Teams.Count > 0;
        }

        // Which primary stat types are allowed per slot (should be 2,4,6 only)
        [JsonProperty("slotStats")]
        public List<string>[] slotStats = new List<string>[6];

        // Sets to consider using
        [JsonProperty("BuildSets")]
        public List<RuneSet> BuildSets = new List<RuneSet>();

        [JsonIgnore]
        public BuildUsage buildUsage;

        [JsonIgnore]
        public RuneUsage runeUsage;

		// magically find good runes to use in the build
		public bool autoRuneSelect = false;

		// magically scale Minimum with Sort while the build is running
		public bool autoAdjust = false;

        // Save to JSON
        public List<int> BannedRuneId = new List<int>();

        [JsonIgnore]
        public List<int> bannedRunesTemp = new List<int>();

        /// ---------------

        // These should be generated at runtime, do not store externally

        // The best loadouts
        [JsonIgnore]
        public IEnumerable<Monster> loads;

        // The best loadout in loads
        [JsonIgnore]
        public Monster Best = null;

        [JsonIgnore]
        private object BestLock = new object();

        // The runes to be used to generate builds
        [JsonIgnore]
        public Rune[][] runes = new Rune[6][];

        /// ----------------

        // How to sort the stats
        [JsonIgnore]
        public Func<Stats, int> sortFunc;

        // if currently running
		[JsonIgnore]
        public bool isRun = false;

        [JsonIgnore]
        public long Time;

        [JsonIgnore]
        public Stats shrines = new Stats();

        [JsonProperty("LeaderBonus")]
        public Stats leader = new Stats();

        public bool ShouldSerializeleader()
        {
            return leader.NonZero();
        }

        // Seems to out-of-mem if too many
        private static readonly int MaxBuilds32 = 500000;
        
        public void BanEmTemp(params int[] brunes)
        {
            bannedRunesTemp.Clear();
            foreach (var r in brunes)
            {
                bannedRunesTemp.Add(r);
            }
        }

        // build the scoring function
        public double sort(Stats m)
        {
            double pts = 0;
            if (m == null)
                return pts;

            foreach (Attr stat in statAll)
            {
                // if this stat is used for sorting
                if (!stat.HasFlag(Attr.ExtraStat))
                {
                    if (Sort[stat] != 0)
                    {
                        // sum points for the stat
                        pts += m[stat] / Sort[stat];
                        // if exceeding max, subtracted the gained points and then some
                        if (Threshold[stat] != 0)
                            pts -= Math.Max(0, m[stat] - Threshold[stat]) / Sort[stat];
                    }
                }
                else
                {
                    if (Sort.ExtraGet(stat) != 0)
                    {
                        pts += m.ExtraValue(stat) / Sort.ExtraGet(stat);
                        if (Threshold.ExtraGet(stat) != 0)
                            pts -= Math.Max(0, m.ExtraValue(stat) - Threshold.ExtraGet(stat)) /
                                   Sort.ExtraGet(stat);
                    }
                }
            }
            return pts;
        }

        void GetPrediction(int[] slotFakes, bool[] slotPred)
        {
            // crank the rune prediction
            for (int i = 0; i < 6; i++)
            {
                int raiseTo = 0;
                bool predictSubs = false;

                // find the largest number to raise to
                // if any along the tree say to predict, do it
                if (runePrediction.ContainsKey("g"))
                {
                    int glevel = runePrediction["g"].Key;
                    if (glevel > raiseTo)
                        raiseTo = glevel;
                    predictSubs |= runePrediction["g"].Value;
                }
                if (runePrediction.ContainsKey(((i % 2 == 0) ? "o" : "e")))
                {
                    int mlevel = runePrediction[((i % 2 == 0) ? "o" : "e")].Key;
                    if (mlevel > raiseTo)
                        raiseTo = mlevel;
                    predictSubs |= runePrediction[((i % 2 == 0) ? "o" : "e")].Value;
                }
                if (runePrediction.ContainsKey((i + 1).ToString()))
                {
                    int slevel = runePrediction[(i + 1).ToString()].Key;
                    if (slevel > raiseTo)
                        raiseTo = slevel;
                    predictSubs |= runePrediction[(i + 1).ToString()].Value;
                }

                slotFakes[i] = raiseTo;
                slotPred[i] = predictSubs;
            }
        }

        /// <summary>
        /// Generates builds based on the instances variables.
        /// </summary>
        /// <param name="top">If non-zero, runs until N builds are generated</param>
        /// <param name="time">If non-zero, runs for N seconds</param>
        /// <param name="printTo">Periodically gives progress% and if it failed</param>
        /// <param name="progTo">Periodically gives the progress% as a double</param>
        /// <param name="dumpBads">If true, will only track new builds if they score higher than an other found builds</param>
        /// <param name="saveStats">If to write stats to rune stats</param>
        public void GenBuilds(int top = 0, int time = 0, Action<string> printTo = null, Action<double, int> progTo = null, bool dumpBads = false, bool saveStats = false, bool goodRunes = false)
        {
            if (runes.Any(r => r == null))
                return;

            if (!saveStats)
                goodRunes = false;

            if (!goodRunes)
            {
                runeUsage = new RuneUsage();
                buildUsage = new BuildUsage();
            }

            // if to get awakened
            if (DownloadAwake && !mon.downloaded)
            {
                var mref = MonsterStat.FindMon(mon);
                if (mref != null)
                {
                    // download the current (unawakened monster)
                    var mstat = mref.Download();
                    // if the retrieved mon is unawakened, get the awakened
                    if (!mstat.Awakened && mstat.AwakenRef != null)
                        mon = mstat.AwakenRef.Download().GetMon(mon);
                }
            }
            // getting awakened also gets level 40, so...
            // only get lvl 40 stats if the monster isn't 40, wants to download AND isn't already downloaded (first and last are about the same)
            else if (mon.level < 40 && DownloadStats && !mon.downloaded)
            {
                var mref = MonsterStat.FindMon(mon);
                if (mref != null)
                    mon = mref.Download().GetMon(mon);
            }

            try
            {
                Best = null;
                Stats bstats = null;
                long count = 0;
                long total = runes[0].Length;
                total *= runes[1].Length;
                total *= runes[2].Length;
                total *= runes[3].Length;
                total *= runes[4].Length;
                total *= runes[5].Length;
                long complete = total;

                printTo?.Invoke("...");

                if (total == 0)
                {
                    printTo?.Invoke("0 perms");
                    Console.WriteLine("Zero permuations");
                    return;
                }

                bool hasSort = false;
                foreach (Attr stat in statAll)
                {
                    if ((stat.HasFlag((Attr.ExtraStat)) ? Sort.ExtraGet(stat) : Sort[stat]) != 0)
                    {
                        hasSort = true;
                        break;
                    }
                }
                if (top == 0 && !hasSort)
                {
                    printTo?.Invoke("No sort");
                    Console.WriteLine("No method of determining best");
                    return;
                }

                DateTime begin = DateTime.Now;
                DateTime timer = DateTime.Now;

                Console.WriteLine(count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));

                int[] slotFakes = new int[6];
                bool[] slotPred = new bool[6];
                GetPrediction(slotFakes, slotPred);
                

                // set to running
                isRun = true;

                // Parallel the outer loop
                SynchronizedCollection<Monster> tests = new SynchronizedCollection<Monster>();
                Parallel.ForEach(runes[0], (r0, loopState) =>
                {
                if (!isRun)
                    //break;
                    loopState.Break();

                // number of builds ruled out since last sync
                int kill = 0;
                // number of builds added since last sync
                int plus = 0;

                Monster test = new Monster(mon);
                test.Current.Shrines = shrines;
                test.Current.Leader = leader;

                test.Current.FakeLevel = slotFakes;
                test.Current.PredictSubs = slotPred;
                test.ApplyRune(r0, 6);

                    foreach (Rune r1 in runes[1])
                    {
                        if (!isRun) // Can't break to a label, don't want to goto
                            break;
                        test.ApplyRune(r1, 6);

                        foreach (Rune r2 in runes[2])
                        {
                            if (!isRun)
                                break;
                            test.ApplyRune(r2, 6);

                            foreach (Rune r3 in runes[3])
                            {
                                if (!isRun)
                                    break;
                                test.ApplyRune(r3, 6);

                                foreach (Rune r4 in runes[4])
                                {
                                    if (!isRun)
                                        break;
                                    test.ApplyRune(r4, 6);
                                    foreach (Rune r5 in runes[5])
                                    {
                                        if (!isRun)
                                            break;

                                        test.ApplyRune(r5, 6);

										bool isBad = false;
										//if (test.Current.Runes.All(r => mon.Current.Runes.Contains(r)))
										//	isBad = false;

										var cstats = test.GetStats();

                                        bool maxdead = false;

                                        if (Maximum != null)
                                        {
                                            foreach (var stat in statEnums)
                                            {
                                                if (Maximum[stat] > 0 && cstats[stat] > Maximum[stat])
                                                {
                                                    maxdead = true;
                                                    break;
                                                }
                                            }
                                        }
                                        
                                        // check if build meets minimum
                                        isBad |= (Minimum != null && !(cstats > Minimum));
                                        isBad |= (maxdead);
                                        // if no broken sets, check for broken sets
                                        isBad |= (!AllowBroken && !test.Current.SetsFull);
                                        // if there are required sets, ensure we have them
                                        isBad |= (RequiredSets != null && RequiredSets.Count > 0
                                            // this Linq adds no overhead compared to GetStats() and ApplyRune()
                                            && !RequiredSets.All(s => test.Current.Sets.Contains(s)));
                                        //    && !RequiredSets.All(s => test.Current.Sets.Count(q => q == s) >= RequiredSets.Count(q => q == s)));

                                        if (isBad)
                                        {
                                            kill++;
                                        }

                                        else
                                        {
                                            // we found an okay build!
                                            plus++;

                                            if (saveStats)
                                            {
                                                foreach (Rune r in test.Current.Runes)
                                                {
                                                    if (!goodRunes)
                                                    {
                                                        r.manageStats.AddOrUpdate("LoadFilt", 1, (s, d) => { return d + 1; });
                                                        runeUsage.runesGood.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                                    }
                                                    else
                                                    {
                                                        r.manageStats.AddOrUpdate("LoadFilt", 0.001, (s, d) => { return d + 0.001; });
                                                        runeUsage.runesOkay.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                                    }
                                                }
                                            }

                                            // if we are to track all good builds, keep it
                                            if (!dumpBads)
                                            {
                                                if (tests.Count < MaxBuilds32)
                                                    tests.Add(new Monster(test, true));

                                                var bestScore = sort(bstats);
                                                var curScore = sort(cstats);
                                                lock(BestLock)
                                                {
                                                    if (Best == null || bestScore < curScore)
                                                    {
                                                        Best = new Monster(test, true);
                                                        bstats = Best.GetStats();
                                                    }
                                                }
                                            }
                                            // if we only want to track really good builds
                                            else
                                            {
                                                // if there are currently no good builds, keep it
                                                // or if this build is better than the best, keep it

                                                var bestScore = sort(bstats);
                                                var curScore = sort(cstats);

                                                lock(BestLock)
                                                {
                                                    if (Best == null || bestScore < curScore)
                                                    {
                                                        Best = new Monster(test, true);
                                                        bstats = Best.GetStats();
                                                    }
                                                }
                                                if (tests.Count < MaxBuilds32 && (saveStats || Best == test))
                                                {
                                                    // keep it for spreadsheeting
                                                    tests.Add(new Monster(test, true));
                                                }
                                            }
                                        }

                                        // every second, give a bit of feedback to those watching
                                        if (DateTime.Now > timer.AddSeconds(1))
                                        {
                                            timer = DateTime.Now;
                                            Console.WriteLine(count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));
                                            printTo?.Invoke(string.Format("{0:P2}", (count + complete - total) / (double)complete));
                                            progTo?.Invoke((count + complete - total) / (double)complete, tests.Count);

                                            if (time <= 0) continue;
                                            if (DateTime.Now > begin.AddSeconds(time))
                                            {
                                                Console.WriteLine("Timeout");
                                                printTo?.Invoke("Timeout");
                                                progTo?.Invoke(1, tests.Count);

                                                isRun = false;
                                                break;
                                            }
                                        }
                                    }
                                    // sum up what work we've done
                                    Interlocked.Add(ref total, -kill);
                                    Interlocked.Add(ref buildUsage.failed, kill);
                                    kill = 0;
                                    Interlocked.Add(ref count, plus);
                                    Interlocked.Add(ref buildUsage.passed, plus);
                                    plus = 0;

                                    // if we've got enough, stop
                                    if (top > 0 && count >= top)
                                    {
                                        isRun = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                });

                if (saveStats)
                {
                    foreach (var ra in runes)
                    {
                        foreach (var r in ra)
                        {
                            if (!goodRunes)
                            {
                                runeUsage.runesUsed.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                r.manageStats.AddOrUpdate("LoadGen", total, (s, d) => { return d + total; });
                            }
                            else
                            {
                                runeUsage.runesBetter.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                r.manageStats.AddOrUpdate("LoadGen", total * 0.001, (s, d) => { return d + total * 0.001; });
                            }
                        }
                    }
                }

                // write out completion
                Console.WriteLine(isRun + " " + count + "/" + total + "  " + String.Format("{0:P2}", (count + complete - total) / (double)complete));
                printTo?.Invoke("100%");
                progTo?.Invoke(1, tests.Count);

                // sort *all* the builds
                loads = tests.Where(t => t != null).OrderByDescending(r => sort(r.GetStats())).Take((top > 0 ? top : 1)).ToList();

                if (!goodRunes)
                    buildUsage.loads = tests.ToList();
                
                // dump everything to console, if nothing to print to
                if (printTo == null)
                    foreach (var l in loads)
                    {
                        Console.WriteLine(l.GetStats().Health + "  " + l.GetStats().Attack + "  " + l.GetStats().Defense + "  " + l.GetStats().Speed
                            + "  " + l.GetStats().CritRate + "%" + "  " + l.GetStats().CritDamage + "%" + "  " + l.GetStats().Resistance + "%" + "  " + l.GetStats().Accuracy + "%");
                    }

                // sadface if no builds
                if (!loads.Any())
                {
                    Console.WriteLine("No builds :(");
                    if (printTo != null)
                        printTo.Invoke("Zero :(");
                }
                else
                {
                    // remember the good one
                    Best = loads.First();
                    //Best.Current.runeUsage = usage.runeUsage;
                    //Best.Current.buildUsage = usage.buildUsage;
                    foreach (Rune r in Best.Current.Runes)
                    {
                        if (!goodRunes)
                            r.manageStats.AddOrUpdate("In", 1, (s,e) => 1);
                        else
                        {
                            r.manageStats.AddOrUpdate("In", 2, (s,e) => e);
                            runeUsage.runesSecond.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                        }
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        if (!goodRunes && mon.Current.Runes[i] != null && mon.Current.Runes[i].ID != Best.Current.Runes[i].ID)
                            mon.Current.Runes[i].Swapped = true;
                    }
                }

                //loads = null;
                tests.Clear();
                tests = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e);
                printTo?.Invoke(e.ToString());
            }
        }

        //Dictionary<string, RuneFilter> rfS, Dictionary<string, RuneFilter> rfM, Dictionary<string, RuneFilter> rfG, 
        public bool RunFilters(int slot, out Stats rFlat, out Stats rPerc, out Stats rTest)
        {
            bool blank = true;
            rFlat = new Stats();
            rPerc = new Stats();
            rTest = new Stats();

            // pull the filters (flat, perc, test) for all the tabs and stats
            Dictionary<string, RuneFilter> rfG = new Dictionary<string, RuneFilter>();
            if (runeFilters.ContainsKey("g"))
                rfG = runeFilters["g"];

            Dictionary<string, RuneFilter> rfM = new Dictionary<string, RuneFilter>();
            if (slot != 0 && runeFilters.ContainsKey((slot % 2 == 0 ? "e" : "o")))
                rfM = runeFilters[(slot % 2 == 0 ? "e" : "o")];

            Dictionary<string, RuneFilter> rfS = new Dictionary<string, RuneFilter>();
            if (slot > 0 && runeFilters.ContainsKey(slot.ToString()))
                rfS = runeFilters[slot.ToString()];

            foreach (string stat in statNames)
            {
                RuneFilter rf = new RuneFilter();
                if (rfS.ContainsKey(stat))
                {
                    rf = rfS[stat];
                    if (rfM.ContainsKey(stat))
                        rf = RuneFilter.Dominant(rf, rfM[stat]);

                    if (rfG.ContainsKey(stat))
                        rf = RuneFilter.Dominant(rf, rfG[stat]);
                }
                else
                {
                    if (rfM.ContainsKey(stat))
                    {
                        rf = rfM[stat];
                        if (rfG.ContainsKey(stat))
                            rf = RuneFilter.Dominant(rf, rfG[stat]);
                    }
                    else
                    {
                        if (rfG.ContainsKey(stat))
                            rf = rfG[stat];
                    }
                }
                if (rf.NonZero)
                {
                    // put the most relevant divisor in?
                    if (rf.Flat.HasValue)
                        rFlat[stat] = rf.Flat.Value;
                    if (rf.Percent.HasValue)
                        rPerc[stat] = rf.Percent.Value;
                    if (rf.Test.HasValue)
                        rTest[stat] = rf.Test.Value;
                    blank = false;
                }
            }

            return blank;
        }

        public int GetPredict(Rune r)
        {
            int pred = runePrediction.ContainsKey("g") ? runePrediction["g"].Key : 0;

            if (r.Slot > 0 && runePrediction.ContainsKey(r.Slot % 2 == 0 ? "e" : "o") && runePrediction[r.Slot % 2 == 0 ? "e" : "o"].Key >= 0)
                pred = runePrediction[r.Slot % 2 == 0 ? "e" : "o"].Key;
            if (runePrediction.ContainsKey(r.Slot.ToString()) && runePrediction[r.Slot.ToString()].Key >= 0)
                pred = runePrediction[r.Slot.ToString()].Key;
            
            if (pred == -1)
                pred = 0;

            return pred;
        }

        public double ScoreRune(Rune r, int raiseTo = 0, bool predictSubs = false)
        {
            int slot = r.Slot;
            double? testVal;
            Stats rFlat, rPerc, rTest;

            // if there where no filters with data
            bool blank = RunFilters(slot, out rFlat, out rPerc, out rTest);//true;
            int and = LoadFilters(slot, out testVal);
            double ret = 0;
            if (!blank)
            {
                if (and == 0)
                {
                    if (r.Or(rFlat, rPerc, rTest, raiseTo, predictSubs))
                        ret = 1;
                }
                else if (and == 1)
                {
                    if (r.And(rFlat, rPerc, rTest, raiseTo, predictSubs))
                        ret = 1;
                }
                else if (and == 2)
                {
                    ret = r.Test(rFlat, rPerc, raiseTo, predictSubs);
                }
            }
            return ret;
        }

        public int LoadFilters(int slot, out double? testVal)
        {
            // which tab we pulled the filter from
            testVal = null;
            int and = 0;

            // TODO: check what inheriting SUM (eg. Odd and 3) does
            // TODO: check what inheriting AND/OR then SUM (or visa versa)

            // find the most significant operatand of joining checks
            if (runeScoring.ContainsKey("g") && runeFilters.ContainsKey("g"))// && runeFilters["g"].Any(r => r.Value.NonZero))
            {
                var kv = runeScoring["g"];
                and = kv.Key;
                if (kv.Key == 2)
                {
                    if (kv.Value != null)
                        testVal = kv.Value;
                }
            }
            // is it and odd or even slot?
            string tmk = (slot % 2 == 0 ? "e" : "o");
            if (runeScoring.ContainsKey(tmk) && runeFilters.ContainsKey(tmk))// && runeFilters[tmk].Any(r => r.Value.NonZero))
            {
                var kv = runeScoring[tmk];
                and = kv.Key;
                if (kv.Key == 2)
                {
                    if (kv.Value != null)
                        testVal = kv.Value;
                }
            }
            // turn the 0-5 to a 1-6
            tmk = slot.ToString();
            if (runeScoring.ContainsKey(tmk) && runeFilters.ContainsKey(tmk))// && runeFilters[tmk].Any(r => r.Value.NonZero))
            {
                var kv = runeScoring[tmk];
                and = kv.Key;
                if (kv.Key == 2)
                {
                    if (kv.Value != null)
                        testVal = kv.Value;
                }
            }

            return and;
        }

        public Predicate<Rune> RuneScoring(int slot, int raiseTo = 0, bool predictSubs = false)
        {
            // default fail OR
            Predicate<Rune> slotTest = r => false;

            // the value to test SUM against
            double? testVal;

            int and = LoadFilters(slot, out testVal);

            // this means that runes won't get in unless they meet at least 1 criteria
            
            // if an operand was found, ensure the tab contains filter data
            
            Stats rFlat, rPerc, rTest;

            // if there where no filters with data
            bool blank = RunFilters(slot, out rFlat, out rPerc, out rTest);//true;
            
            // no filter data = use all
            if (blank)
                slotTest = r => true;
            else
            {
                // Set the test based on the type found
                if (and == 0)
                {
                    slotTest = r => r.Or(rFlat, rPerc, rTest, raiseTo, predictSubs);
                }
                else if (and == 1)
                {
                    slotTest = r => r.And(rFlat, rPerc, rTest, raiseTo, predictSubs);
                }
                else if (and == 2)
                {
                    slotTest = r => r.Test(rFlat, rPerc, raiseTo, predictSubs) >= testVal;
                }
            }
            return slotTest;
        }

		// Try to determine the subs required to meet the minimum. Will guess Evens by: Slot, Health%, Attack%, Defense%
		public Stats NeededForMin(int[] slotFakes, bool[] slotPred)
		{
			var smon = (Stats)mon;//.GetStats();
			var smin = Minimum;

            Stats ret = smin - smon;

			var avATK = runes[0].Average(r => r.GetValue(Attr.AttackFlat, slotFakes[0], slotPred[0]));
			var avDEF = runes[2].Average(r => r.GetValue(Attr.DefenseFlat, slotFakes[2], slotPred[2]));
			var avHP = runes[4].Average(r => r.GetValue(Attr.HealthFlat, slotFakes[4], slotPred[4]));

			ret.Attack -= avATK;
			ret.Defense -= avDEF;
			ret.Health -= avHP;

			ret = ret.Of(smon);
			
			var lead = mon.Boost(leader);
			lead -= mon;
			
			ret -= lead;
			
			ret.Attack *= 100;
			ret.Defense *= 100;
			ret.Health *= 100;

			// Check if we have requirements that are unlikey to be met with subs
			Attr[] evenSlots = new Attr[] { Attr.Null, Attr.Null, Attr.Null };

			// get the average MainStats for slots
			var avSel = runes[1].Where(r => r.MainType == Attr.Speed).ToArray();
			var avmSpeed = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Speed, slotFakes[1], slotPred[1]));
			avSel = runes[3].Where(r => r.MainType == Attr.CritRate).ToArray();
			var avmCRate = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritRate, slotFakes[3], slotPred[3]));
			avSel = runes[3].Where(r => r.MainType == Attr.CritDamage).ToArray();
			var avmCDam = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritDamage, slotFakes[3], slotPred[3]));
			avSel = runes[5].Where(r => r.MainType == Attr.Accuracy).ToArray();
			var avmAcc = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Accuracy, slotFakes[5], slotPred[5]));
			avSel = runes[5].Where(r => r.MainType == Attr.Resistance).ToArray();
			var avmRes = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Resistance, slotFakes[5], slotPred[5]));

			if (avmSpeed > 20 && ret.Speed > avmSpeed + 10)
			{
				evenSlots[0] = Attr.Speed;
				ret.Speed -= avmSpeed;
			}

			if (avmAcc > 20 && ret.Accuracy > avmAcc + 10)
			{
				evenSlots[2] = Attr.Accuracy;
				ret.Accuracy -= avmAcc;
			}
			else if (avmRes > 20 && ret.Resistance > avmRes + 10)
			{
				evenSlots[2] = Attr.Resistance;
				ret.Resistance -= avmRes;
			}

			if (avmCDam > 40 && ret.CritDamage > avmCDam + 15)
			{
				evenSlots[1] = Attr.CritDamage;
				ret.CritDamage -= avmCDam;
			}
			else if (avmCRate > 30 && ret.CritRate > avmCRate -  5)
			{
				evenSlots[1] = Attr.CritRate;
				ret.CritRate -= avmCRate;
			}

			// go back 6,4,2 for putting things in
			for (int i = 2; i >= 0; i--)
			{
				if (evenSlots[i] == Attr.Null)
				{
					if (ret.Health > 50)
					{
						evenSlots[i] = Attr.HealthPercent;
						ret.Health -= 50;
					}
					else if (ret.Attack > 50)
					{
						evenSlots[i] = Attr.AttackPercent;
						ret.Attack -= 50;
					}
					else if (ret.Defense > 50)
					{
						evenSlots[i] = Attr.DefensePercent;
						ret.Defense -= 50;
					}
				}
			}

			foreach (Attr a in statEnums)
			{
				if (ret[a] < 0)
					ret[a] = 0;
			}

			return ret;
		}

        /// <summary>
        /// Fills the instance with acceptable runes from save
        /// </summary>
        /// <param name="save">The Save data that contais the runes</param>
        /// <param name="useLocked">If it should include locked runes</param>
        /// <param name="useEquipped">If it should include equipped runes (other than the current monster)</param>
        /// <param name="saveStats">If to write information to the runes about usage</param>
        public void GenRunes(Save save, bool useLocked = false, bool useEquipped = false, bool saveStats = false, bool goodRunes = false)
        {
            if (save?.Runes == null)
                return;

            IEnumerable<Rune> rsGlobal = save.Runes;

            // if not saving stats, cull unusable here
            if (!saveStats)
            {
                // Only using 'inventory' or runes on mon
                // also, include runes which have been unequipped (should only look above)
                if (!useEquipped)
                    rsGlobal = rsGlobal.Where(r => (r.AssignedName == "Unknown name" || r.AssignedName == "Inventory" || r.AssignedName == mon.Name) || r.Swapped);
                // only if the rune isn't currently locked for another purpose
                if (!useLocked)
                    rsGlobal = rsGlobal.Where(r => r.Locked == false);
                rsGlobal = rsGlobal.Where(r => !BannedRuneId.Any(b => b == r.ID) && !bannedRunesTemp.Any(b => b == r.ID));
            }

            
            if (BuildSets.All(s => Rune.SetRequired(s) == 4) && RequiredSets.All(s => Rune.SetRequired(s) == 4))
            {
                // if only include/req 4 sets, include all 2 sets autoRuneSelect && ()
                rsGlobal = rsGlobal.Where(r => BuildSets.Contains(r.Set) || RequiredSets.Contains(r.Set) || Rune.SetRequired(r.Set) == 2);
            }
            else if (BuildSets.Any() || RequiredSets.Any())
            {
                rsGlobal = rsGlobal.Where(r => BuildSets.Contains(r.Set) || RequiredSets.Contains(r.Set));
                // Only runes which we've included
            }

            if (saveStats)
            {
                foreach (Rune r in rsGlobal)
                {
                    if (!goodRunes)
                        r.manageStats.AddOrUpdate("Set", 1, (s,d)=> { return d + 1; });
                    else
                        r.manageStats.AddOrUpdate("Set", 0.001, (s, d) => { return d + 0.001; });

                }
            }

			int[] slotFakes = new int[6];
			bool[] slotPred = new bool[6];
            GetPrediction(slotFakes, slotPred);

			// Set up each runeslot
			for (int i = 0; i < 6; i++)
			{
				// put the right ones in
				runes[i] = rsGlobal.Where(r => r.Slot == i + 1).ToArray();

				if (i % 2 == 1) // actually evens because off by 1
				{
					// makes sure that the primary stat type is in the selection
					if (slotStats[i].Count > 0)
						runes[i] = runes[i].Where(r => slotStats[i].Contains(r.MainType.ToForms())).ToArray();
				}

				if (saveStats)
				{
					foreach (Rune r in runes[i])
					{
                        if (!goodRunes)
						    r.manageStats.AddOrUpdate("TypeFilt", 1, (s, d) => { return d + 1; });
                        else
						    r.manageStats.AddOrUpdate("TypeFilt", 0.001, (s, d) => { return d + 0.001; });
					}
					// cull here instead
					if (!useEquipped)
						runes[i] = runes[i].Where(r => (r.AssignedName == "Unknown name" || r.AssignedName == "Inventory" || r.AssignedName == mon.Name) || r.Swapped).ToArray();
					if (!useLocked)
						runes[i] = runes[i].Where(r => r.Locked == false).ToArray();
				}
			}
			CleanBroken();

			if (autoRuneSelect)
			{
				var needed = NeededForMin(slotFakes, slotPred);
				var needRune = new Stats(needed) / 6;

				// reduce number of runes to 10-15

				// odds first, then evens
				foreach (int i in new int[] { 0, 2, 4, 5, 3, 1 })
				{
                    Rune[] rr = new Rune[0];
                    foreach (var rs in RequiredSets)
                    {
                        rr = rr.Concat(runes[i].Where(r => r.Set == rs).OrderByDescending(r => RuneVsStats(r, needRune) * 10 + RuneVsStats(r, Sort)).Take(7).ToArray()).ToArray();
                    }
                    if (rr.Length < 15)
                        rr = rr.Concat(runes[i].Where(r => !rr.Contains(r)).OrderByDescending(r => RuneVsStats(r, needRune) * 10 + RuneVsStats(r, Sort)).Take(15 - rr.Length).ToArray()).Distinct().ToArray();

                    runes[i] = rr;
                }

                CleanBroken();
			}
			else
			{
				// Filter each runeslot
				for (int i = 0; i < 6; i++)
				{
					// default fail OR
					Predicate<Rune> slotTest = RuneScoring(i + 1, slotFakes[i], slotPred[i]);

					runes[i] = runes[i].Where(r => slotTest.Invoke(r)).ToArray();

					if (saveStats)
					{
						foreach (Rune r in runes[i])
						{
                            if (!goodRunes)
    							r.manageStats.AddOrUpdate("RuneFilt", 1, (s, d) => d + 1);
                            else
    							r.manageStats.AddOrUpdate("RuneFilt", 0.001, (s, d) => d + 0.001);
                        }
                    }
				}
			}
        }

		// Make sure that for each set type, there are enough slots with runes in them
		// Eg. if only 1,4,5 have Violent, remove all violent runes because you need 4
		// for each included set
		private void CleanBroken()
		{
			if (!AllowBroken)
			{
                var used = runes[0].Concat(runes[1]).Concat(runes[2]).Concat(runes[3]).Concat(runes[4]).Concat(runes[5]).Select(r => r.Set).Distinct();
                
				foreach (RuneSet s in used)
				{
					// find how many slots have acceptable runes for it
					int slots = 0;
					for (int i = 0; i < 6; i++)
					{
						if (runes[i].Any(r => r.Set == s))
							slots += 1;
					}
					// if there isn't enough slots
					if (slots < Rune.SetRequired(s))
					{
						// remove that set
						for (int i = 0; i < 6; i++)
						{
							runes[i] = runes[i].Where(r => r.Set != s).ToArray();
						}
					}
				}
			}
		}

		// assumes Stat A/D/H are percent
		private int RuneHasStats(Rune r, Stats s)
		{
			int ret = 0;

			if (r.HealthPercent >= s.Health)
				ret++;
			if (r.AttackPercent >= s.Attack)
				ret++;
			if (r.DefensePercent >= s.Defense)
				ret++;
			if (r.Speed >= s.Speed)
				ret++;

			if (r.CritDamage >= s.CritDamage)
				ret++;
			if (r.CritRate >= s.CritRate)
				ret++;
			if (r.Accuracy >= s.Accuracy)
				ret++;
			if (r.Resistance >= s.Resistance)
				ret++;

			return ret;
		}

		// assumes Stat A/D/H are percent
		private double RuneVsStats(Rune r, Stats s)
		{
			double ret = 0;

			if (s.Health > 0)
				ret += r.HealthPercent / s.Health;
			if (s.Attack > 0)
				ret += r.AttackPercent / s.Attack;
			if (s.Defense > 0)
				ret += r.DefensePercent / s.Defense;
			if (s.Speed > 0)
				ret += r.Speed / s.Speed;

			if (s.CritDamage > 0)
				ret += r.CritDamage / s.CritDamage;
			if (s.CritRate > 0)
				ret += r.CritRate / s.CritRate;
			if (s.Accuracy > 0)
				ret += r.Accuracy / s.Accuracy;
			if (s.Resistance > 0)
				ret += r.Resistance / s.Resistance;
			
			return ret;
		}

	}

}