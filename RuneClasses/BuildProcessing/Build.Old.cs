#define BUILD_PRECHECK_BUILDS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using RuneOptim.swar;
using RuneOptim.Management;

namespace RuneOptim.BuildProcessing {
    public partial class Build {

        // stuff that is being phased out in favour of BuildStrategies

        /// <summary>
        /// if currently running
        /// </summary>
        [JsonIgnore]
        private bool isRunning = false;

        [JsonIgnore]
        private readonly object isRunLock = new object();

        /// <summary>
        /// 'thread-safe' check/set
        /// </summary>
        [JsonIgnore]
        public bool IsRunning {
            get {
                lock (isRunLock) {
                    return isRunning;
                }
            }
            private set {
                lock (isRunLock) {
                    isRunning = value;
                }
            }
        }

        /// <summary>
        /// Get if running without lock
        /// </summary>
        [JsonIgnore]
        public bool IsRunning_Unsafe {
            get {
                return isRunning;
            }
        }

        private bool getRunningHandle() {
            lock (isRunLock) {
                if (isRunning)
                    return false;
                else {
                    isRunning = true;
                    return true;
                }
            }
        }

        [JsonIgnore]
        private readonly object bestLock = new object();

        /// <summary>
        /// Fills the instance with acceptable runes from save
        /// </summary>
        /// <param name="save">The Save data that contais the runes</param>
        /// <param name="useLocked">If it should include locked runes</param>
        /// <param name="useEquipped">If it should include equipped runes (other than the current monster)</param>
        /// <param name="saveStats">If to write information to the runes about usage</param>
        [Obsolete]
        public void GenRunes(Save save) {
            if (save?.Runes == null)
                return;

            if (!getRunningHandle())
                return;
            try {

                if (Type == BuildType.Link && LinkBuild == null) {
                    for (int i = 0; i < 6; i++)
                        Runes[i] = new Rune[0];
                    return;
                }

                // todo: less .ToArray-ing
                ParallelQuery<Rune> rsGlobal = save.Runes.AsParallel();

                if (Type == BuildType.Lock)
                {
                    for (int index = 0; index < Mon.Current.Runes.Count(); index++)
                    {
                        Rune r = Mon.Current.Runes[index];
                        // leaving the rune null is "code" for an intentionally absent rune
                        if (r == null)
                            continue;
                        // make sure to use runes out of rsGlobal (vs. potentially orphaned monster.current runes)
                        Rune rune = rsGlobal.FirstOrDefault(rn => rn.Id == r.Id);
                        if (rune.UsedInBuild)
                            // an empty set will produce an error for a missing rune, appropriate if a rune in a locked build was somehow applied elsewhere
                            Runes[index] = new Rune[0];
                        else
                            Runes[index] = new Rune[] { rune };
                    }
                    return;
                }

                // if not saving stats, cull unusable here
                if (!BuildSaveStats) {
                    // Only using 'inventory' or runes on current mon
                    // also, include runes which have been unequipped (should only look above)
                    if (!RunesUseEquipped || RunesOnlyFillEmpty)
                        rsGlobal = rsGlobal.Where(r => r.IsUnassigned || r.AssignedId == Mon.Id || r.Swapped);
                    // only if the rune isn't currently locked for another purpose
                    if (!RunesUseLocked)
                        rsGlobal = rsGlobal.Where(r => !r.UsedInBuild);
                    rsGlobal = rsGlobal.Where(r => !BannedRuneId.Any(b => b == r.Id) && !BannedRunesTemp.Any(b => b == r.Id));
                }
                if (BuildSets.Any() || RequiredSets.Any()) {
                    // filter based on sets
                    rsGlobal = Rune.FilterBySets(rsGlobal, RequiredSets, BuildSets, AllowBroken);
                }

                if (BuildSaveStats) {
                    foreach (Rune r in rsGlobal) {
                        r.ManageStats.AddOrUpdate("currentBuildPoints", 0, (k, v) => 0);
                        if (!BuildGoodRunes)
                            r.ManageStats.AddOrUpdate("Set", 1, (s, d) => { return d + 1; });
                        else
                            r.ManageStats.AddOrUpdate("Set", 0.001, (s, d) => { return d + 0.001; });
                    }
                }

                int?[] slotFakes = new int?[6];
                bool[] slotPred = new bool[6];
                GetPrediction(slotFakes, slotPred);

                // Set up each runeslot
                for (int i = 0; i < 6; i++) {
                    // put the right ones in
                    Runes[i] = rsGlobal.Where(r => r.Slot == i + 1).ToArray();

                    // makes sure that the primary stat type is in the selection
                    if (i % 2 == 1 && SlotStats[i].Count > 0) // actually evens because off by 1
                    {
                        Runes[i] = Runes[i].AsParallel().Where(r => SlotStats[i].Contains(r.Main.Type.ToForms())).ToArray();
                    }

                    if (BuildSaveStats) {
                        foreach (Rune r in Runes[i]) {
                            if (!BuildGoodRunes)
                                r.ManageStats.AddOrUpdate("TypeFilt", 1, (s, d) => { return d + 1; });
                            else
                                r.ManageStats.AddOrUpdate("TypeFilt", 0.001, (s, d) => { return d + 0.001; });
                        }
                        // cull here instead
                        if (!RunesUseEquipped || RunesOnlyFillEmpty)
                            Runes[i] = Runes[i].AsParallel().Where(r => r.IsUnassigned || r.AssignedId == Mon.Id || r.Swapped).ToArray();
                        if (!RunesUseLocked)
                            Runes[i] = Runes[i].AsParallel().Where(r => !r.UsedInBuild).ToArray();

                    }
                }

                // clean out runes which won't make complete sets
                cleanBroken();

                // clean out runes which cannot meet stat minimums in combination with the maximum value of other slots
                cleanMinimum();

                if (AutoRuneSelect) {
                    // TODO: triple pass: start at needed for min, but each pass reduce the requirements by the average of the chosen runes for that pass, increase it by build scoring

                    var needed = NeededForMin(slotFakes, slotPred);
                    if (needed == null)
                        AutoRuneSelect = false;

                    if (AutoRuneSelect) {
                        var needRune = new Stats(needed) / 6;

                        // Auto-Rune select picking N per RuneSet should be fine to pick more because early-out should keep times low.
                        // reduce number of runes to 10-15

                        // odds first, then evens
                        foreach (int i in new int[] { 0, 2, 4, 5, 3, 1 }) {
                            Rune[] rr = new Rune[0];
                            foreach (var rs in RequiredSets) {
                                rr = rr.Concat(Runes[i].AsParallel().Where(r => r.Set == rs).OrderByDescending(r => runeVsStats(r, needRune) * 10 + runeVsStats(r, Sort)).Take(AutoRuneAmount / 2).ToArray()).ToArray();
                            }
                            if (rr.Length < AutoRuneAmount)
                                rr = rr.Concat(Runes[i].AsParallel().Where(r => !rr.Contains(r)).OrderByDescending(r => runeVsStats(r, needRune) * 10 + runeVsStats(r, Sort)).Take(AutoRuneAmount - rr.Length).ToArray()).Distinct().ToArray();

                            Runes[i] = rr;
                        }

                        cleanBroken();
                    }
                } else { // !AutoSelect
                    // Filter each runeslot
                    for (int i = 0; i < 6; i++) {
                        // default fail OR
                        Predicate<Rune> slotTest = MakeRuneScoring(i + 1, slotFakes[i] ?? 0, slotPred[i]);

                        Runes[i] = Runes[i].AsParallel().Where(r => slotTest.Invoke(r)).OrderByDescending(r => r.ManageStats.GetOrAdd("testScore", 0)).ToArray();
                        var filt = LoadFilters(i + 1);
                        if (filt.Count != null || filt.Type == FilterType.SumN) {

                            var tSets = RequiredSets.Count + BuildSets.Except(RequiredSets).Count();
                            var reqWeight = RequiredSets.Count;
                            // if we only counted required sets, but no 2-size sets
                            if (tSets == RequiredSets.Count && !RequiredSets.Any(s => Rune.Set2.Contains(s))) {
                                reqWeight += 1;
                                reqWeight *= 2;
                                tSets += Rune.RuneSets.Except(RequiredSets).Where(s => Rune.Set2.Contains(s)).Count();
                            }
                            var perc = reqWeight / (float)tSets;
                            var reqLoad = Math.Max(2,(int)((filt.Count ?? AutoRuneAmount ) * perc));

                            var rr = Runes[i].AsParallel().Where(r => RequiredSets.Contains(r.Set)).GroupBy(r => r.Set).SelectMany(r => r).Take(reqLoad).ToArray();

                            var incLoad = (filt.Count ?? AutoRuneAmount) - rr.Count();
                            Runes[i] = rr.Concat(Runes[i].AsParallel().Where(r => !RequiredSets.Contains(r.Set)).Take(incLoad)).ToArray();

                            // TODO: pick 20% per required set
                            // Then fill remaining with the best from included
                            // Go around checking if there are enough runes from each set to complete it (if NonBroken)
                            // Check if removing N other runes of SCORE will permit finishing set
                            // Remove rune add next best in slot
                        }

                        if (BuildSaveStats) {
                            foreach (Rune r in Runes[i]) {
                                if (!BuildGoodRunes)
                                    r.ManageStats.AddOrUpdate("RuneFilt", 1, (s, d) => d + 1);
                                else
                                    r.ManageStats.AddOrUpdate("RuneFilt", 0.001, (s, d) => d + 0.001);
                            }
                        }
                    }
                }
                if (RunesDropHalfSetStat) {
                    for (int i = 0; i < 6; i++) {
                        double rmm = 0;
                        var runesForSlot = Runes[i];
                        var outRunes = new List<Rune>();
                        var runesBySet = runesForSlot.GroupBy(r => r.Set);
                        foreach (var rsg in runesBySet) {
                            var runesByMain = rsg.GroupBy(r => r.Main.Type);
                            foreach (var rmg in runesByMain) {
                                rmm = rmg.Max(r => r.ManageStats.GetOrAdd("testScore", 0)) * 0.6;
                                if (rmm > 0) {
                                    outRunes.AddRange(rmg.Where(r => r.ManageStats.GetOrAdd("testScore", 0) > rmm));
                                }
                            }
                        }
                        if (rmm > 0)
                            Runes[i] = outRunes.ToArray();
                    }
                }
                // if we are only to fill empty slots
                if (RunesOnlyFillEmpty) {
                    for (int i = 0; i < 6; i++) {
                        if (Mon.Current.Runes[i] != null && (!Mon.Current.Runes[i]?.UsedInBuild ?? false)) {
                            Runes[i] = new Rune[0];
                        }
                    }
                }
                // always try to put the current rune back in
                for (int i = 0; i < 6; i++) {
                    var r = Mon.Current.Runes[i];
                    if (r == null)
                        continue;

                    bool isGoodType = true;
                    if (i % 2 == 1 && SlotStats[i].Count > 0) {
                        isGoodType = SlotStats[i].Contains(r.Main.Type.ToForms());
                    }
                    if (!Runes[i].Contains(r) && !r.UsedInBuild && isGoodType) {
                        var tl = Runes[i].ToList();
                        tl.Add(r);
                        Runes[i] = tl.ToArray();
                    }
                }

                Grinds = Runes.SelectMany(rg => rg.SelectMany(r => r.FilterGrinds(save.Crafts).Concat(r.FilterEnchants(save.Crafts)))).Distinct().ToArray();
            }
            catch (Exception e) {
                // cascade the error since higher-level processes update the UI
                throw e;
            }
            finally {
                IsRunning = false;
            }
        }

        /// <summary>
        /// Generate the builds, but only on the given runes
        /// </summary>
        /// <param name="runes"></param>
        /// <returns></returns>
        [Obsolete]
        public Monster GenBuild(params Rune[] runes) {
            if (runes.Length != 6)
                return null;

            // if to get awakened
            if (DownloadAwake && !Mon.Downloaded) {
                var mref = MonsterStat.FindMon(Mon);
                if (mref != null) {
                    // download the current (unawakened monster)
                    var mstat = mref.Download();
                    // if the retrieved mon is unawakened, get the awakened
                    if (!mstat.Awakened && mstat.AwakenTo != null)
                        Mon = mstat.AwakenTo.Download().GetMon(Mon);
                }
            }
            // getting awakened also gets level 40, so...
            // only get lvl 40 stats if the monster isn't 40, wants to download AND isn't already downloaded (first and last are about the same)
            else if (Mon.Level < 40 && DownloadStats && !Mon.Downloaded) {
                var mref = MonsterStat.FindMon(Mon);
                if (mref != null)
                    Mon = mref.Download().GetMon(Mon);
            }

            int?[] slotFakes = new int?[6];
            bool[] slotPred = new bool[6];
            GetPrediction(slotFakes, slotPred);

            Mon.ExtraCritRate = extraCritRate;
            Monster test = new Monster(Mon);
            test.Current.Shrines = Shrines;
            test.Current.Guild = Guild;
            test.Current.Leader = Leader;

            test.Current.FakeLevel = slotFakes.Select(i => i ?? 0).ToArray();
            test.Current.PredictSubs = slotPred;

            test.ApplyRune(runes[0], 7);
            test.ApplyRune(runes[1], 7);
            test.ApplyRune(runes[2], 7);
            test.ApplyRune(runes[3], 7);
            test.ApplyRune(runes[4], 7);
            test.ApplyRune(runes[5], 7);
            test.Current.CheckSets();


            // TODO: Outsource to whoever wants it
            bool isBad = false;

            var cstats = test.GetStats();

            // check if build meets minimum
            isBad |= Minimum != null && (cstats <= Minimum);
            // if no broken sets, check for broken sets
            isBad |= !AllowBroken && !test.Current.SetsFull;
            // if there are required sets, ensure we have them
            isBad |= RequiredSets != null && RequiredSets.Count > 0
                // this Linq adds no overhead compared to GetStats() and ApplyRune()
                && !RequiredSets.All(s => test.Current.Sets.Count(q => q == s) >= RequiredSets.Count(q => q == s));

            if (isBad)
                return null;

            return test;
        }

        [Obsolete]
        public void Cancel() {
            IsRunning = false;
        }

        [JsonIgnore]
        public long Count = 0;
        [JsonIgnore]
        public long Skipped = 0;
        [JsonIgnore]
        public long Actual = 0;
        [JsonIgnore]
        public long Total = 0;
        [JsonIgnore]
        public long Complete = 0;
        [JsonIgnore]
        public long Good = 0;
        [JsonIgnore]
        public long Bad = 0;

        /// <summary>
        /// Generates builds based on the instances variables.
        /// </summary>
        /// <param name="top">If non-zero, runs until N builds are generated</param>
        /// <param name="time">If non-zero, runs for N seconds</param>
        /// <param name="printTo">Periodically gives progress% and if it failed</param>
        /// <param name="progTo">Periodically gives the progress% as a double</param>
        /// <param name="dumpBads">If true, will only track new builds if they score higher than an other found builds</param>
        /// <param name="saveStats">If to write stats to rune stats</param>
        [Obsolete]
        public BuildResult GenBuilds(string prefix = "") {
            if (Type == BuildType.Lock) {
                Monster temp = new Monster(Mon);
                foreach (var slot in Runes)
                {
                    // null indicates an intentionally empty slot
                    // a zero length slot is a rune that is needed but is not available
                    if (slot != null && slot.Length > 0)
                        temp.ApplyRune(slot[0]);
                }
                Best = new Monster(temp, true);
                return BuildResult.Success;
            }
            else if (Type == BuildType.Link) {
                if (LinkBuild == null) {
                    for (int i = 0; i < 6; i++)
                        Runes[i] = new Rune[0];
                    return BuildResult.Failure;
                }
                else {
                    CopyFrom(LinkBuild);
                }
            }

            if (Runes.Any(r => r == null)) {
                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Null rune"));
                return BuildResult.BadRune;
            }
            if (!BuildSaveStats)
                BuildGoodRunes = false;

            if (!BuildGoodRunes) {
                RuneUsage = new RuneUsage();
                BuildUsage = new BuildUsage();
            }
            try {
                // if to get awakened
                if (DownloadAwake && !Mon.Downloaded) {
                    BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Downloading Awake def"));
                    var mref = MonsterStat.FindMon(Mon);
                    if (mref != null) {
                        // download the current (unawakened monster)
                        var mstat = mref.Download();
                        BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Reading stats"));
                        // if the retrieved mon is unawakened, get the awakened
                        if (!mstat.Awakened && mstat.AwakenTo != null) {
                            BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Awakening"));
                            Mon = mstat.AwakenTo.Download().GetMon(Mon);
                        }
                    }
                    BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Downloaded"));
                }
                // getting awakened also gets level 40, so...
                // only get lvl 40 stats if the monster isn't 40, wants to download AND isn't already downloaded (first and last are about the same)
                else if (Mon.Level < 40 && DownloadStats && !Mon.Downloaded) {
                    BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Downloading 40 def"));
                    var mref = MonsterStat.FindMon(Mon);
                    if (mref != null)
                        Mon = mref.Download().GetMon(Mon);
                }
            }
            catch (Exception e) {
                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Failed downloading def: " + e.Message + Environment.NewLine + e.StackTrace));
            }

            if (!getRunningHandle())
                throw new InvalidOperationException("The build is locked with another action.");
            
            Loads.Clear();

            if (!Sort[Attr.Speed].EqualTo(0) && Sort[Attr.Speed] <= 1 // 1 SPD is too good to pass
                || Mon.Current.Runes.Any(r => r == null)
                || !Mon.Current.Runes.All(r => Runes[r.Slot - 1].Contains(r)) // only IgnoreLess5 if I have my own runes
                || Sort.NonZeroStats.HasCount(1)) // if there is only 1 sorting, must be too important to drop???
                IgnoreLess5 = false;

            Thread timeThread = null;

            if (!string.IsNullOrWhiteSpace(BuildStrategy)) {

                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes().Where(t => typeof(IBuildStrategyDefinition).IsAssignableFrom(t)));

                var type = types.FirstOrDefault(t => t.AssemblyQualifiedName.Contains(BuildStrategy));
                if (type != null) {
                    RunStrategy();
                }
            }

            tcs.TrySetResult(null);

            try {
                Best = null;
                Stats bstats = null;
                Count = 0;
                Actual = 0;
                Total = Runes[0].Length;
                Total *= Runes[1].Length;
                Total *= Runes[2].Length;
                Total *= Runes[3].Length;
                Total *= Runes[4].Length;
                Total *= Runes[5].Length;
                Complete = Total;

                Mon.ExtraCritRate = extraCritRate;
                Mon.GetStats();
                Mon.DamageMethod?.Invoke(Mon);

                int?[] slotFakesTemp = new int?[6];
                bool[] slotPred = new bool[6];
                GetPrediction(slotFakesTemp, slotPred);

                int[] slotFakes = slotFakesTemp.Select(i => i ?? 0).ToArray();

                var currentLoad = new Monster(Mon, true);
                currentLoad.Current.TempLoad = true;
                currentLoad.Current.Buffs = Buffs;
                currentLoad.Current.Shrines = Shrines;
                currentLoad.Current.Guild = Guild;
                currentLoad.Current.Leader = Leader;

                currentLoad.Current.FakeLevel = slotFakes;
                currentLoad.Current.PredictSubs = slotPred;

                double currentScore = CalcScore(currentLoad.GetStats(true));

                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "cooking"));

                if (Total == 0) {
                    BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "0 perms"));
                    RuneLog.Info("Zero permuations");
                    return BuildResult.NoPermutations;
                }

                bool hasSort = Sort.IsNonZero;
                if (BuildTake == 0 && !hasSort) {
                    BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "No sort"));
                    RuneLog.Info("No method of determining best");
                    return BuildResult.NoSorting;
                }

                DateTime begin = DateTime.Now;

                RuneLog.Debug(Count + "/" + Total + "  " + string.Format("{0:P2}", (Count + Complete - Total) / (double)Complete));


                // set to running
                IsRunning = true;

#if BUILD_PRECHECK_BUILDS_DEBUG
                SynchronizedCollection<string> outstrs = new SynchronizedCollection<string>();
#endif
                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "..."));

                List<Monster> tests = new List<Monster>();

                timeThread = new Thread(() => {
                    while (IsRunning) {
                        if (RunesOnlyFillEmpty)
                            Thread.Sleep(30 / ((Mon?.Current?.RuneCount ?? 1) + 1));
                        else
                            Thread.Sleep(400);
                        // every second, give a bit of feedback to those watching
                        RuneLog.Debug(Count + "/" + Total + "  " + string.Format("{0:P2}", (Count + Complete - Total) / (double)Complete));
                        if (tests != null)
                            BuildProgTo?.Invoke(this, ProgToEventArgs.GetEvent(this, (Count + Complete - Total) / (double)Complete, tests.Count));

                        if (BuildTimeout > 0 && DateTime.Now > begin.AddSeconds(BuildTimeout)) {
                            RuneLog.Info("Timeout");
                            BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + "Timeout"));
                            BuildProgTo?.Invoke(this, ProgToEventArgs.GetEvent(this, 1, tests.Count));

                            IsRunning = false;
                        }
                    }
                });
                timeThread.Start();

                double bestScore = double.MinValue;

                var opts = new ParallelOptions() {
                    MaxDegreeOfParallelism = Environment.ProcessorCount - 1
                };

                var mmm = Maximum.NonZeroCached;

                // Parallel the outer loop
                // TODO: setup the begin/finish Actions with syncList.
                void body (Rune r0, ParallelLoopState loopState) {
                    var tempReq = RequiredSets.ToList();
                    var tempMax = Maximum == null || !Maximum.IsNonZero ? null : new Stats(Maximum, true);
                    int tempCheck = 0;

                    Monster myBest = null;
                    List<Monster> syncList = new List<Monster>();

                    void syncMyList() {
                        lock (bestLock) {
#if DEBUG_SYNC_BUILDS
                            foreach (var s in syncList) {
                                if (s.Current.Runes.All(r => r.Assigned == mon)) {
                                    Console.WriteLine("!");
                                }
                            }
#endif
                            tests.AddRange(syncList);

                        }
                        //syncList.ForEach(_ => tests.Add(_));
                        syncList.Clear();
                        if (tests.Count > Math.Max(BuildGenerate, 250000)) {
#if DEBUG_SYNC_BUILDS
                                var rems = tests.OrderByDescending(b => b.score).Skip(75000).ToList();
                                foreach (var bbb in rems) {
                                    if (bbb.Current.Runes.All(r => r.Assigned == mon)) {
                                        Console.WriteLine("!");
                                    }
                                }
#endif
                            lock (bestLock) {
                                tests = tests.OrderByDescending(b => b.Score).Take(75000).ToList();
                            }
                        }

                        if (tests.Count > MaxBuilds32)
                            IsRunning = false;
                    }

                    if (!IsRunning_Unsafe) {
                        syncMyList();
                        loopState.Break();
                    }

                    // number of builds ruled out since last sync
                    int kill = 0;
                    // number of builds added since last sync
                    int plus = 0;
                    // number of builds skipped
                    int skip = 0;


                    bool isBad;
                    double myBestScore = double.MinValue, curScore, lastBest = double.MinValue;
                    Stats cstats, myStats;


                    Monster test = new Monster(Mon);
                    test.Current.TempLoad = true;
                    test.Current.Buffs = Buffs;
                    test.Current.Shrines = Shrines;
                    test.Current.Guild = Guild;
                    test.Current.Leader = Leader;

                    test.Current.FakeLevel = slotFakes;
                    test.Current.PredictSubs = slotPred;
                    test.ApplyRune(r0, 7);

                    RuneSet set4 = r0.SetIs4 ? r0.Set : RuneSet.Null;
                    RuneSet set2 = r0.SetIs4 ? RuneSet.Null : r0.Set;
                    int pop4 = 0;
                    int pop2 = 0;

                    foreach (Rune r1 in Runes[1]) {
                        // TODO: refactor to local method
                        if (!IsRunning_Unsafe) // Can't break to a label, don't want to goto
                            break;
                        // TODO: break into multiple implementations that have less branching
#if BUILD_PRECHECK_BUILDS
                        if (!AllowBroken && !RunesOnlyFillEmpty) {
                            if (r1.SetIs4) {
                                if (pop2 == 2)
                                    pop2 = 7;
                                if (set4 == RuneSet.Null || pop4 >= 2) {
                                    set4 = r1.Set;
                                    pop4 = 2;
                                }
                                else if (set4 != r1.Set) {
#if BUILD_PRECHECK_BUILDS_DEBUG
                                    outstrs.Add($"bad4@2 {set4} {set2} | {r0.Set} {r1.Set}");
#endif
                                    skip += Runes[2].Length * Runes[3].Length * Runes[4].Length * Runes[5].Length;
                                    continue;
                                }
                            }
                            else {
                                if (pop4 == 2)
                                    pop4 = 7;
                                if (set2 == RuneSet.Null || pop2 >= 2) {
                                    set2 = r1.Set;
                                    pop2 = 2;
                                }
                            }
                        }
#endif
                        test.ApplyRune(r1, 7);

                        foreach (Rune r2 in Runes[2]) {
                            if (!IsRunning_Unsafe)
                                break;
#if BUILD_PRECHECK_BUILDS
                            if (!AllowBroken && !RunesOnlyFillEmpty) {
                                if (r2.SetIs4) {
                                    if (pop2 == 3)
                                        pop2 = 7;
                                    if (set4 == RuneSet.Null || pop4 >= 3) {
                                        set4 = r2.Set;
                                        pop4 = 3;
                                    }
                                    else if (set4 != r2.Set) {
#if BUILD_PRECHECK_BUILDS_DEBUG
                                        outstrs.Add($"bad4@3 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set}");
#endif
                                        skip += Runes[3].Length * Runes[4].Length * Runes[5].Length;
                                        continue;
                                    }
                                }
                                else {
                                    if (pop4 == 3)
                                        pop4 = 7;
                                    if (set2 == RuneSet.Null || pop2 >= 3) {
                                        set2 = r2.Set;
                                        pop2 = 3;
                                    }
                                    else if (set4 != RuneSet.Null && set2 != r2.Set) {
#if BUILD_PRECHECK_BUILDS_DEBUG
                                        outstrs.Add($"bad2@3 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set}");
#endif
                                        skip += Runes[3].Length * Runes[4].Length * Runes[5].Length;
                                        continue;
                                    }
                                }
                            }
#endif
                            test.ApplyRune(r2, 7);

                            foreach (Rune r3 in Runes[3]) {
                                if (!IsRunning_Unsafe)
                                    break;
#if BUILD_PRECHECK_BUILDS
                                if (!AllowBroken && !RunesOnlyFillEmpty) {
                                    if (r3.SetIs4) {
                                        if (pop2 == 4)
                                            pop2 = 7;
                                        if (set4 == RuneSet.Null || pop4 >= 4) {
                                            set4 = r3.Set;
                                            pop4 = 4;
                                        }
                                        else if (set4 != r3.Set) {
#if BUILD_PRECHECK_BUILDS_DEBUG
                                            outstrs.Add($"bad4@4 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set}");
#endif
                                            skip += Runes[4].Length * Runes[5].Length;
                                            continue;
                                        }
                                    }
                                    else {
                                        if (pop4 == 4)
                                            pop4 = 7;
                                        if (set2 == RuneSet.Null || pop2 >= 4) {
                                            set2 = r3.Set;
                                            pop2 = 4;
                                        }
                                        else if (set4 != RuneSet.Null && set2 != r3.Set) {
#if BUILD_PRECHECK_BUILDS_DEBUG
                                            outstrs.Add($"bad2@4 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set}");
#endif
                                            skip += Runes[4].Length * Runes[5].Length;
                                            continue;
                                        }
                                    }
                                }
#endif
                                test.ApplyRune(r3, 7);

                                foreach (Rune r4 in Runes[4]) {
                                    if (!IsRunning_Unsafe) {
                                        break;
                                    }
#if BUILD_PRECHECK_BUILDS
                                    if (!AllowBroken && !RunesOnlyFillEmpty) {
                                        if (r4.SetIs4) {
                                            if (pop2 == 5)
                                                pop2 = 7;
                                            if (set4 == RuneSet.Null || pop4 >= 5) {
                                                set4 = r4.Set;
                                                pop4 = 5;
                                            }
                                            else if (set4 != r4.Set) {
#if BUILD_PRECHECK_BUILDS_DEBUG
                                                outstrs.Add($"bad4@5 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set} {r4.Set}");
#endif
                                                skip += Runes[5].Length;
                                                continue;
                                            }
                                        }
                                        else {
                                            if (pop4 == 5)
                                                pop4 = 7;
                                            if (set2 == RuneSet.Null || pop2 >= 5) {
                                                set2 = r4.Set;
                                                pop2 = 5;

                                            }
                                            else if (set4 != RuneSet.Null && set2 != r4.Set) {
#if BUILD_PRECHECK_BUILDS_DEBUG
                                                outstrs.Add($"bad2@5 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set} {r4.Set}");
#endif
                                                skip += Runes[5].Length;
                                                continue;
                                            }
                                        }
                                    }
#endif
                                    test.ApplyRune(r4, 7);

                                    foreach (Rune r5 in Runes[5]) {
                                        if (!IsRunning_Unsafe)
                                            break;

                                        test.ApplyRune(r5, 7);
                                        test.Current.CheckSets();
#if BUILD_PRECHECK_BUILDS_DEBUG
                                        outstrs.Add($"fine {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set} {r4.Set} {r5.Set}");
#endif
                                        isBad = false;

                                        cstats = test.GetStats();

                                        // check if build meets minimum
                                        isBad |= !RunesOnlyFillEmpty && !AllowBroken && !test.Current.SetsFull;

                                        isBad |= tempMax != null && cstats.AnyExceedCached(tempMax);

                                        if (!isBad && GrindLoads) {
                                            var mahGrinds = Grinds.ToList();
                                            for (int rg = 0; rg < 6; rg++) {
                                                var lgrinds = test.Runes[rg].FilterGrinds(mahGrinds);
                                                foreach (var g in lgrinds) {
                                                    var tr = test.Runes[rg].Grind(g);
                                                }
                                                // TODO: 
                                            }
                                        }

                                        isBad |= !RunesOnlyFillEmpty && Minimum != null && !cstats.GreaterEqual(Minimum, true);
                                        // if no broken sets, check for broken sets
                                        // if there are required sets, ensure we have them
                                        /*isBad |= (tempReq != null && tempReq.Count > 0
                                            // this Linq adds no overhead compared to GetStats() and ApplyRune()
                                            //&& !tempReq.All(s => test.Current.Sets.Count(q => q == s) >= tempReq.Count(q => q == s))
                                            //&& !tempReq.GroupBy(s => s).All(s => test.Current.Sets.Count(q => q == s.Key) >= s.Count())
                                            );*/
                                        // TODO: recheck this code is correct
                                        if (tempReq != null && tempReq.Count > 0) {
                                            tempCheck = 0;
                                            foreach (var r in tempReq) {
                                                int i;
                                                for (i = 0; i < 3; i++) {
                                                    if (test.Current.Sets[i] == r && (tempCheck & 1 << i) != 1 << i) {
                                                        tempCheck |= 1 << i;
                                                        break;
                                                    }
                                                }
                                                if (i >= 3) {
                                                    isBad |= true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (isBad) {
                                            kill++;
                                            curScore = 0;
                                        }
                                        else {
                                            // try to reduce CalcScore hits
                                            curScore = CalcScore(cstats);
                                            isBad |= IgnoreLess5 && curScore < currentScore * 1.05;
                                            if (isBad)
                                                kill++;
                                        }

                                        if (!isBad) {
                                            // we found an okay build!
                                            plus++;
                                            test.Score = curScore;

                                            if (BuildSaveStats) {
                                                foreach (Rune r in test.Current.Runes) {
                                                    if (!BuildGoodRunes) {
                                                        r.ManageStats.AddOrUpdate("LoadFilt", 1, (s, d) => { return d + 1; });
                                                        RuneUsage.runesGood.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                                        r.ManageStats.AddOrUpdate("currentBuildPoints", curScore, (k, v) => Math.Max(v, curScore));
                                                        r.ManageStats.AddOrUpdate("cbp" + ID, curScore, (k, v) => Math.Max(v, curScore));
                                                    }
                                                    else {
                                                        r.ManageStats.AddOrUpdate("LoadFilt", 0.001, (s, d) => { return d + 0.001; });
                                                        RuneUsage.runesOkay.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                                        r.ManageStats.AddOrUpdate("cbp" + ID, curScore, (k, v) => Math.Max(v, curScore * 0.9));
                                                    }
                                                }
                                            }

                                            if (syncList.Count >= 500) {
                                                syncMyList();
                                            }

                                            // if we are to track all good builds, keep it
                                            if (!BuildDumpBads) {

                                                syncList.Add(new Monster(test, true));

                                                // locally track my best
                                                if (myBest == null || curScore > myBestScore) {
                                                    myBest = new Monster(test, true);
                                                    myStats = myBest.GetStats();
                                                    myBestScore = CalcScore(myStats);
                                                    myBest.Score = myBestScore;
                                                }

                                                // if mine is better than what I last saw
                                                if (myBestScore > lastBest) {
                                                    lock (bestLock) {
                                                        if (Best == null) {
                                                            Best = new Monster(myBest, true);
                                                            bstats = Best.GetStats();
                                                            bestScore = CalcScore(bstats);
                                                            Best.Score = bestScore;
                                                        }
                                                        else {
                                                            // sync best score
                                                            lastBest = bestScore;
                                                            // double check
                                                            if (myBestScore > lastBest) {
                                                                Best = new Monster(myBest, true);
                                                                bestScore = CalcScore(bstats);
                                                                Best.Score = bestScore;
                                                                bstats = Best.GetStats();
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                            // if we only want to track really good builds
                                            else {
                                                // if there are currently no good builds, keep it
                                                // or if this build is better than the best, keep it

                                                // locally track my best
                                                if (myBest == null || curScore > myBestScore) {
                                                    myBest = new Monster(test, true);
                                                    myStats = myBest.GetStats();
                                                    myBestScore = CalcScore(myStats);
                                                    myBest.Score = myBestScore;
                                                    syncList.Add(myBest);
                                                }
                                                else if (BuildSaveStats) {
                                                    // keep it for spreadsheeting
                                                    syncList.Add(new Monster(test, true) {
                                                        Score = curScore
                                                    });
                                                }
                                            }
                                        }
                                    }
                                    // sum up what work we've done
                                    Interlocked.Add(ref Count, kill);
                                    Interlocked.Add(ref Count, skip);
                                    Interlocked.Add(ref Skipped, skip);
                                    Interlocked.Add(ref Actual, kill);
                                    Interlocked.Add(ref BuildUsage.failed, kill);
                                    kill = 0;
                                    skip = 0;
                                    Interlocked.Add(ref Count, plus);
                                    Interlocked.Add(ref Actual, plus);
                                    Interlocked.Add(ref BuildUsage.passed, plus);
                                    plus = 0;

                                    // if we've got enough, stop
                                    if (BuildGenerate > 0 && BuildUsage.passed >= BuildGenerate) {
                                        IsRunning = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    // just before dying
                    syncMyList();
                }
                Parallel.ForEach(Runes[0], opts, body);


                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + "finalizing..."));
                BuildProgTo?.Invoke(this, ProgToEventArgs.GetEvent(this, 0.99, -1));

#if BUILD_PRECHECK_BUILDS_DEBUG
                System.IO.File.WriteAllLines("_into_the_bridge.txt", outstrs.ToArray());
#endif
                if (BuildSaveStats) {
                    foreach (var ra in Runes) {
                        foreach (var r in ra) {
                            if (!BuildGoodRunes) {
                                r.ManageStats.AddOrUpdate("buildScoreTotal", CalcScore(Best), (k, v) => v + CalcScore(Best));
                                RuneUsage.runesUsed.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                r.ManageStats.AddOrUpdate("LoadGen", Total, (s, d) => { return d + Total; });

                            }
                            else {
                                RuneUsage.runesBetter.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                                r.ManageStats.AddOrUpdate("LoadGen", Total * 0.001, (s, d) => { return d + Total * 0.001; });
                            }
                        }
                    }
                }

                // write out completion
                RuneLog.Debug(IsRunning + " " + Count + "/" + Total + "  " + string.Format("{0:P2}", (Count + Complete - Total) / (double)Complete));
                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + " completed"));
                BuildProgTo?.Invoke(this, ProgToEventArgs.GetEvent(this, 1, tests.Count));

                // sort *all* the builds
                int takeAmount = 1;
                if (BuildSaveStats)
                    takeAmount = 10;
                if (BuildTake > 0)
                    takeAmount = BuildTake;

                if (IgnoreLess5)
                    tests.Add(new Monster(Mon, true));

                foreach (var ll in tests.Where(t => t != null).OrderByDescending(r => CalcScore(r.GetStats())).Take(takeAmount))
                    Loads.Add(ll);

                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "Found a load " + Loads.Count()));

                if (!BuildGoodRunes)
                    BuildUsage.loads = tests.ToList();

                // dump everything to console, if nothing to print to
                if (BuildPrintTo == null)
                    foreach (var l in Loads) {
                        RuneLog.Debug(l.GetStats().Health + "  " + l.GetStats().Attack + "  " + l.GetStats().Defense + "  " + l.GetStats().Speed
                            + "  " + l.GetStats().CritRate + "%" + "  " + l.GetStats().CritDamage + "%" + "  " + l.GetStats().Resistance + "%" + "  " + l.GetStats().Accuracy + "%");
                    }

                // sadface if no builds
                if (!Loads.Any()) {
                    RuneLog.Info("No builds :(");
                    BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + "Zero :("));
                }
                else {
                    // remember the good one
                    Best = Loads.First();
                    Best.Current.TempLoad = false;
                    Best.Score = CalcScore(Best.GetStats());
                    BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + "best " + (Best?.Score ?? -1)));
                    Best.Current.ActualTests = Actual;
                    foreach (var bb in Loads) {
                        foreach (Rune r in bb.Current.Runes) {
                            double val = Best.Score;
                            if (BuildGoodRunes) {
                                val *= 0.25;
                                if (bb == Best)
                                    RuneUsage.runesSecond.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
                            }

                            if (bb != Best)
                                val *= 0.1;
                            else
                                r.ManageStats.AddOrUpdate("In", BuildGoodRunes ? 2 : 1, (s, e) => BuildGoodRunes ? 2 : 1);

                            r.ManageStats.AddOrUpdate("buildScoreIn", val, (k, v) => v + val);
                        }
                    }
                    for (int i = 0; i < 6; i++) {
                        if (!BuildGoodRunes && Mon.Current.Runes[i] != null && Mon.Current.Runes[i].Id != Best.Current.Runes[i].Id)
                            Mon.Current.Runes[i].Swapped = true;
                    }
                    foreach (var ra in Runes) {
                        foreach (var r in ra) {
                            var cbp = r.ManageStats.GetOrAdd("currentBuildPoints", 0);
                            if (cbp / Best.Score < 1)
                                r.ManageStats.AddOrUpdate("bestBuildPercent", cbp / Best.Score, (k, v) => Math.Max(v, cbp / Best.Score));
                        }
                    }
                }

                tests.Clear();
                tests = null;
                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + "Test cleared"));
                return BuildResult.Success;
            }
            catch (Exception e) {
                RuneLog.Error("Error " + e);
                BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + e.ToString()));
                return BuildResult.Failure;
            }
            finally {
                tcs = new TaskCompletionSource<IBuildRunner>();
                IsRunning = false;
                if (timeThread != null)
                    timeThread.Join();
            }
        }

    }
}
