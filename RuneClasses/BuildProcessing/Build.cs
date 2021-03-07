#define BUILD_PRECHECK_BUILDS
//#define BUILD_PRECHECK_BUILDS_DEBUG

using Newtonsoft.Json;
using RuneOptim.Management;
using RuneOptim.swar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {


    /// <summary>
    /// Contains most of the data needed to outline build requirements.
    /// The heavy lifter, trying to move logic out of it :S
    /// </summary>
    public partial class Build {
        // allows iterative code, probably slow but nice to write and integrates with WinForms at a moderate speed
        // TODO: have another go at it
        //[Obsolete("Consider changing to statEnums")]
        public static readonly string[] StatNames = { "HP", "ATK", "DEF", "SPD", "CR", "CD", "RES", "ACC" };
        public static readonly Attr[] StatEnums = { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy };
        public static readonly Attr[] StatBoth = { Attr.HealthFlat, Attr.HealthPercent, Attr.AttackFlat, Attr.AttackPercent, Attr.DefenseFlat, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy };
        //[Obsolete("Consider changing to extraEnums")]
        public static readonly string[] ExtraNames = { "EHP", "EHPDB", "DPS", "AvD", "MxD" };
        public static readonly Attr[] ExtraEnums = { Attr.EffectiveHP, Attr.EffectiveHPDefenseBreak, Attr.DamagePerSpeed, Attr.AverageDamage, Attr.MaxDamage };
        public static readonly Attr[] StatAll = { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy, Attr.EffectiveHP, Attr.EffectiveHPDefenseBreak, Attr.DamagePerSpeed, Attr.AverageDamage, Attr.MaxDamage };

        /// <summary>
        /// Current running build strategy, ask it for progress?
        /// </summary>
        [JsonIgnore]
        public IBuildRunner runner;

        [JsonIgnore]
        private TaskCompletionSource<IBuildRunner> tcs = new TaskCompletionSource<IBuildRunner>();

        /// <summary>
        /// awaitable for when the build actually starts running
        /// </summary>
        [JsonIgnore]
        public Task<IBuildRunner> startedBuild => tcs.Task;

        /// <summary>
        /// Store the score of the last run in here for loading?
        /// </summary>
        public double lastScore = float.MinValue;

        public Build() {
            // for all 6 slots, init the list
            for (int i = 0; i < SlotStats.Length; i++) {
                SlotStats[i] = new List<string>();
            }
        }

        public Build(Monster m) {
            // for all 6 slots, init the list
            for (int i = 0; i < SlotStats.Length; i++) {
                SlotStats[i] = new List<string>();
            }
            Mon = m;
            var load = Mon.Current;
            if (load == null)
                return;

            // currently equipped stats
            var cstats = load.GetStats(Mon);
            // base stats
            var bstats = Mon;
            // stat difference
            var dstats = cstats - bstats;
            // percentage of each stat buffed
            var astats = dstats / bstats;

            // TODO: template?
            if (false) {
                foreach (Attr a in StatEnums) {
                    if (astats[a] > 0.1) {
                        Minimum[a] = Math.Floor(bstats[a] * (1 + astats[a] * 0.8));
                    }
                }
                foreach (var s in Mon.Current.Sets) {
                    if (s != RuneSet.Null && RuneProperties.MagicalSets.Contains(s)) {
                        RequiredSets.Add(s);
                    }
                }
            }
        }

        public override string ToString() {
            return ID + " " + MonName;
        }

        public BuildType Type { get; set; } = BuildType.Build;

        [JsonProperty("id")]
        public int ID { get; set; } = 0;

        [JsonProperty("version")]
        public int VERSIONNUM { get; set; }

        [JsonProperty("MonName")]
        public string MonName { get; set; }

        [JsonProperty("MonId")]
        public ulong MonId { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("buffs")]
        public Buffs Buffs { get; set; }

        [JsonIgnore]
        public Monster Mon { get; set; }

        #region Rune-build time properties
        [JsonIgnore]
        public int BuildGenerate { get; set; } = 0;

        [JsonIgnore]
        public int BuildTake { get; set; } = 0;

        [JsonIgnore]
        public int BuildTimeout { get; set; } = 0;

        [JsonIgnore]
        public bool BuildDumpBads { get; set; } = false;

        [JsonIgnore]
        public bool BuildSaveStats { get; set; } = false;

        [JsonIgnore]
        public bool BuildGoodRunes { get; set; } = false;

        [JsonIgnore]
        public bool RunesUseLocked { get; set; } = false;

        [JsonIgnore]
        public bool RunesUseEquipped { get; set; } = false;

        [JsonIgnore]
        public bool RunesDropHalfSetStat { get; set; } = false;

        [JsonIgnore]
        public bool RunesOnlyFillEmpty { get; set; } = false;

        [JsonIgnore]
        public bool GrindLoads { get; set; } = false;

        [JsonIgnore]
        public bool IgnoreLess5 { get; set; } = false;
        #endregion

        [JsonProperty]
        public string BuildStrategy { get; set; }

        [Obsolete("Wrap this in a method")]
        public event EventHandler<PrintToEventArgs> BuildPrintTo;

        [Obsolete("Wrap this in a method")]
        public event EventHandler<ProgToEventArgs> BuildProgTo;

        [JsonProperty("new")]
        public bool New { get; set; }

        public bool ShouldSerializeNew() {
            return New;
        }

        [JsonProperty("downloadstats")]
        public bool DownloadStats { get; set; }

        public bool ShouldSerializeDownloadStats() {
            return DownloadStats;
        }

        [JsonProperty("downloadawake")]
        public bool DownloadAwake { get; set; }

        public bool ShouldSerializeDownloadAwake() {
            return DownloadAwake;
        }

        [JsonProperty("extra_crit_rate")]
        private int extraCritRate = 0;

        /// <summary>
        /// Extra crit rate from Ele Advantage/Passive
        /// </summary>
        [JsonIgnore]
        public int ExtraCritRate {
            get {
                if (Mon != null)
                    Mon.ExtraCritRate = extraCritRate;
                return extraCritRate;
            }
            set {
                extraCritRate = value;
                if (Mon != null)
                    Mon.ExtraCritRate = value;
            }
        }

        public bool ShouldSerializeExtraCritRate() {
            return extraCritRate > 0;
        }

        /// <summary>
        /// Magical (and probably bad) tree structure for rune slot stat filters.
        /// Tab -> Stat -> FILTER
        /// </summary>
        [JsonProperty("runeFilters")]
        [JsonConverter(typeof(DictionaryWithSpecialEnumKeyConverter))]
        public Dictionary<SlotIndex, Dictionary<string, RuneFilter>> RuneFilters { get; set; } = new Dictionary<SlotIndex, Dictionary<string, RuneFilter>>();

        public bool ShouldSerializeRuneFilters() {
            Dictionary<SlotIndex, Dictionary<string, RuneFilter>> nfilters = new Dictionary<SlotIndex, Dictionary<string, RuneFilter>>();
            foreach (var tabPair in RuneFilters) {
                List<string> keep = new List<string>();
                foreach (var statPair in tabPair.Value) {
                    if (statPair.Value.NonZero)
                        keep.Add(statPair.Key);
                }
                Dictionary<string, RuneFilter> n = new Dictionary<string, RuneFilter>();
                foreach (var key in keep)
                    n.Add(key, tabPair.Value[key]);
                if (n.Count > 0)
                    nfilters.Add(tabPair.Key, n);
            }
            RuneFilters = nfilters;

            return RuneFilters.Count > 0;
        }

        /// <summary>
        /// For when you want to map 2 pieces of info to a key, just be *really* lazy.
        /// Contains the scoring type (OR, AND, SUM) and the[(>= SUM] value.
        /// Tab -> TYPE -> Test
        /// </summary>
        [JsonProperty("runeScoring")]
        [JsonConverter(typeof(DictionaryWithSpecialEnumKeyConverter))]
        public Dictionary<SlotIndex, RuneScoreFilter> RuneScoring { get; set; } = new Dictionary<SlotIndex, RuneScoreFilter>();

        public struct RuneScoreFilter {
            [Obsolete("Used for upgrading JSON files")]
            public FilterType Key { set => Type = value; }

            public FilterType Type;
            public double? Value;
            public int? Count;

            public RuneScoreFilter(FilterType t = FilterType.None, double? v = null, int? c = null) {
                Type = t;
                Value = v;
                Count = c;
            }

            public override bool Equals(object obj) {
                if (obj is null || !(obj is RuneScoreFilter rhs))
                    return base.Equals(obj);
                return this == rhs;
            }

            public static bool operator ==(RuneScoreFilter lhs, RuneScoreFilter rhs) {
                return lhs.Count == rhs.Count && lhs.Type == rhs.Type && lhs.Value == rhs.Value;
            }


            public static bool operator !=(RuneScoreFilter lhs, RuneScoreFilter rhs) {
                return !(lhs == rhs);
            }
        }

        public bool ShouldSerializeRuneScoring() {
            Dictionary<SlotIndex, RuneScoreFilter> nscore = new Dictionary<SlotIndex, RuneScoreFilter>();
            // filter out all the "default" values
            foreach (var tabPair in RuneScoring) {
                if (tabPair.Value.Type != 0 || tabPair.Value.Value != null || tabPair.Value.Count != null)
                    nscore.Add(tabPair.Key, new RuneScoreFilter(tabPair.Value.Type, tabPair.Value.Value, tabPair.Value.Count));
            }
            RuneScoring = nscore;

            return RuneScoring.Count > 0;
        }

        /// <summary>
        /// if to raise the runes level, and use the appropriate main stat value.
        /// also, attempt to give weight to unassigned powerup bonuses.
        /// Tab -> RAISE -> Magic.
        /// </summary>
        [JsonProperty("runePrediction")]
        [JsonConverter(typeof(DictionaryWithSpecialEnumKeyConverter))]
        public Dictionary<SlotIndex, KeyValuePair<int?, bool>> RunePrediction { get; set; } = new Dictionary<SlotIndex, KeyValuePair<int?, bool>>();

        public bool ShouldSerializeRunePrediction() {
            Dictionary<SlotIndex, KeyValuePair<int?, bool>> npred = new Dictionary<SlotIndex, KeyValuePair<int?, bool>>();
            foreach (var tabPair in RunePrediction) {
                if (tabPair.Value.Key != 0 || tabPair.Value.Value)
                    npred.Add(tabPair.Key, new KeyValuePair<int?, bool>(tabPair.Value.Key, tabPair.Value.Value));
            }
            RunePrediction = npred;

            return RunePrediction.Count > 0;
        }

        /// <summary>
        /// If to allow broken sets on this Monster.
        /// </summary>
        [JsonProperty("AllowBroken")]
        public bool AllowBroken { get; set; } = false;

        /// <summary>
        /// How much each stat is worth (0 = useless)
        /// eg. 300 hp is worth 1 speed
        /// </summary>
        [JsonProperty("Sort")]
        public Stats Sort { get; set; } = new Stats();

        public bool ShouldSerializeSort() {
            return Sort.IsNonZero;
        }

        /// <summary>
        /// Resulting build must have every set in this collection.
        /// </summary>
        [JsonProperty("RequiredSets")]
        public ObservableCollection<RuneSet> RequiredSets { get; set; } = new ObservableCollection<RuneSet>();

        public bool ShouldSerializeRequiredSets() {
            return RequiredSets.Count > 0;
        }

        /// <summary>
        /// Builds *must* have *all* of these stat values or higher.
        /// </summary>
        [JsonProperty("Minimum")]
        public Stats Minimum { get; set; } = new Stats();

        /// <summary>
        /// Builds *mustn't* exceed *any* of these stats.
        /// </summary>
        [JsonProperty("Maximum")]
        public Stats Maximum { get; set; } = new Stats();

        /// <summary>
        /// Individual stats exceeding these values are capped.
        /// </summary>
        [JsonProperty("Threshold")]
        public Stats Threshold { get; set; } = new Stats();

        /// <summary>
        /// Individual stats above these values have reduced weight to encourage 'balanced' builds.
        /// </summary>
        [JsonProperty("Goal")]
        public Stats Goal { get; set; } = new Stats();

        public bool ShouldSerializeMinimum() {
            return Minimum.IsNonZero;
        }

        public bool ShouldSerializeMaximum() {
            return Maximum.IsNonZero;
        }

        public bool ShouldSerializeThreshold() {
            return Threshold.IsNonZero;
        }

        public bool ShouldSerializeGoal() {
            return Goal.IsNonZero;
        }

        [JsonProperty]
        public List<string> Teams { get; set; } = new List<string>();

        public bool ShouldSerializeTeams() {
            return Teams.Count > 0;
        }

        /// <summary>
        /// Which primary stat types are allowed per slot (should be 2,4,6 only)
        /// </summary>
        [JsonProperty("slotStats")]
        public List<string>[] SlotStats { get; set; } = new List<string>[6];

        /// <summary>
        /// Sets to consider using.
        /// </summary>
        [JsonProperty("BuildSets")]
        public ObservableCollection<RuneSet> BuildSets { get; set; } = new ObservableCollection<RuneSet>();

        [JsonIgnore]
        public BuildUsage BuildUsage { get; set; }

        [JsonIgnore]
        public RuneUsage RuneUsage { get; set; }

        [JsonIgnore]
        public static readonly int AutoRuneAmountDefault = 30;

        [JsonProperty("autoRuneAmount")]
        public int AutoRuneAmount { get; set; } = AutoRuneAmountDefault;

        public bool ShouldSerializeAutoRuneAmount() {
            return AutoRuneAmount != AutoRuneAmountDefault;
        }

        /// <summary>
        /// Magically find good runes to use in the build.
        /// This will use Minimum and Score as a heuristic to pick the runes.
        /// </summary>
        [JsonProperty("autoRuneSelect")]
        public bool AutoRuneSelect { get; set; } = false;

        public bool ShouldSerializeAutoRuneSelect() {
            return AutoRuneSelect;
        }

        /// <summary>
        /// Magically scale Minimum with Sort while the build is running.
        /// NOT YET IMPLEMENTED.
        /// </summary>
        [JsonProperty("autoAdjust")]
        public bool AutoAdjust { get; set; } = false;

        public bool ShouldSerializeAutoAdjust() {
            return AutoAdjust;
        }

        // Save to JSON
        public List<ulong> BannedRuneId { get; set; } = new List<ulong>();

        public bool ShouldSerializeBannedRuneId() {
            return BannedRuneId.Any();
        }

        [JsonIgnore]
        public List<ulong> BannedRunesTemp { get; set; } = new List<ulong>();

        // ----------------

        /// <summary>
        /// The best loadouts.
        ///  These should be generated at runtime, do not store externally
        /// </summary>
        [JsonIgnore]
        public readonly ObservableCollection<Monster> Loads = new ObservableCollection<Monster>();

        /// <summary>
        /// The best loadout in loads
        /// </summary>
        [JsonIgnore]
        public Monster Best { get; set; } = null;

        /// <summary>
        /// The runes to be used to generate builds
        /// </summary>
        [JsonIgnore]
        public Rune[][] runes { get; set; } = new Rune[6][];

        [JsonIgnore]
        public Craft[] grinds { get; set; }

        // ----------------

        /// <summary>
        /// How to sort the stats
        /// </summary>
        [JsonIgnore]
        public Func<Stats, int> sortFunc { get; set; }

        [JsonIgnore]
        public long Time { get; set; }

        [JsonIgnore]
        public Stats Shrines { get; set; } = new Stats();

        [JsonProperty("LeaderBonus")]
        public Stats Leader { get; set; } = new Stats();

        public bool ShouldSerializeLeader() {
            return Leader.IsNonZero;
        }

        /// <summary>
        /// Start dropping stored builds here. Seems to out-of-mem if too many
        /// </summary>
        private static readonly int MaxBuilds32 = 500000;

        public int LinkId { get; set; }

        [JsonIgnore]
        public Build LinkBuild { get; set; }

        public void CopyFrom(Build rhs) {
            if (rhs == null) return;

            RunePrediction.Clear();
            RuneFilters.Clear();
            RuneScoring.Clear();
            BuildSets.Clear();
            RequiredSets.Clear();

            AllowBroken = rhs.AllowBroken;
            AutoAdjust = rhs.AutoAdjust;
            AutoRuneAmount = rhs.AutoRuneAmount;
            AutoRuneSelect = rhs.AutoRuneSelect;
            Buffs = rhs.Buffs;
            DownloadAwake = rhs.DownloadAwake;
            DownloadStats = rhs.DownloadStats;
            extraCritRate = rhs.extraCritRate;

            Leader = rhs.Leader;
            Shrines = rhs.Shrines;

            Goal.CopyFrom(rhs.Goal, true);
            Maximum.CopyFrom(rhs.Maximum, true);
            Minimum.CopyFrom(rhs.Minimum, true);
            Sort.CopyFrom(rhs.Sort, true);
            Threshold.CopyFrom(rhs.Threshold, true);

            foreach (var kv in rhs.RuneFilters) {
                RuneFilters[kv.Key] = new Dictionary<string, RuneFilter>();
                foreach (var vp in kv.Value) {
                    RuneFilters[kv.Key].Add(vp.Key, new RuneFilter(vp.Value));
                }
            }

            foreach (var kv in rhs.RunePrediction) {
                RunePrediction[kv.Key] = new KeyValuePair<int?, bool>(kv.Value.Key, kv.Value.Value);
            }

            foreach (var kv in rhs.RuneScoring) {
                RuneScoring[kv.Key] = new RuneScoreFilter(kv.Value.Type, kv.Value.Value, kv.Value.Count);
            }

            for (int i = 0; i < SlotStats.Length; i++) {
                SlotStats[i] = rhs.SlotStats[i].ToList();
            }

            foreach (var bs in rhs.BuildSets) {
                BuildSets.Add(bs);
            }

            foreach (var bs in rhs.RequiredSets) {
                RequiredSets.Add(bs);
            }
        }

        /// <summary>
        /// Warning: Synchronous, uses the builds current settings.
        /// </summary>
        /// <returns></returns>
        public BuildResult RunStrategy() {
            return RunStrategy(new BuildSettings() {
                AllowBroken = AllowBroken,
                BuildDumpBads = BuildDumpBads,
                BuildGenerate = BuildGenerate,
                BuildGoodRunes = BuildGoodRunes,
                BuildSaveStats = BuildSaveStats,
                BuildTake = BuildTake,
                BuildTimeout = BuildTimeout,
                IgnoreLess5 = IgnoreLess5,
                RunesDropHalfSetStat = RunesDropHalfSetStat,
                RunesOnlyFillEmpty = RunesOnlyFillEmpty,
                RunesUseEquipped = RunesUseEquipped,
                RunesUseLocked = RunesUseLocked,
                Shrines = Shrines
            });
        }

        /// <summary>
        /// Warning: Synchronous
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public BuildResult RunStrategy(BuildSettings settings) {
            // TODO: put in the old prerun checks

            if (!string.IsNullOrWhiteSpace(BuildStrategy)) {

                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes().Where(t => typeof(IBuildStrategyDefinition).IsAssignableFrom(t)));

                var type = types.FirstOrDefault(t => t.AssemblyQualifiedName.Contains(BuildStrategy));
                if (type != null) {
                    RunUsingStrategy(type, settings);
                }
            }

            return BuildResult.Failure;

        }

        public BuildResult RunUsingStrategy<T>(BuildSettings settings) where T : IBuildStrategyDefinition {
            return RunUsingStrategy(typeof(T), settings);
        }

        public BuildResult RunUsingStrategy(Type strategy, BuildSettings settings) {
            var def = (IBuildStrategyDefinition)Activator.CreateInstance(strategy);
            runner = null;
            try {
                runner = def.GetRunner();
                // TODO: fixme

                if (runner != null) {
                    runner.Setup(this, settings);
                    tcs.TrySetResult(runner);
                    this.Best = runner.Run(runes.SelectMany(r => r)).Result;

                    return BuildResult.Success;
                }
                return BuildResult.Failure;
            }
            catch (Exception ex) {
                tcs.TrySetException(ex);
                IsRunning = false;
                return BuildResult.Failure;
            }
            finally {
                tcs = new TaskCompletionSource<IBuildRunner>();
                IsRunning = false;
                runner?.TearDown();
                runner = null;
            }

        }


        public void BanEmTemp(params ulong[] brunes) {
            BannedRunesTemp.Clear();
            foreach (var r in brunes) {
                BannedRunesTemp.Add(r);
            }
        }

        /// <summary>
        /// Use the builds score function to return the score contribution for a single Attribute.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stat"></param>
        /// <returns></returns>
        public double ScoreStat(Stats current, Attr stat) {
            if (current == null)
                return 0;
            double vv = current[stat];

            if (vv > 85 && (stat == Attr.Accuracy))
                vv = 85; 
            if (vv > 100 && (stat == Attr.CritRate || stat == Attr.Resistance))
                vv = 100;
            if (Sort[stat] != 0) {
                var tg = Threshold[stat];
                var gg = Goal[stat];
                if (tg != 0 && tg < vv)
                    vv = tg;
                if (vv > gg && gg > 0)
                    vv = (vv - gg) / 2 + gg;
                vv /= Sort[stat];
                return vv;
            }
            return 0;
        }

        /// <summary>
        /// Overload for providing breakdown, but with the null check path taking out.
        /// Gotta save ops.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stat"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double ScoreStat(Stats current, Attr stat, Stats outvals) {
            if (current == null)
                return 0;
            double vv = current[stat];
            double v2 = 0;

            if (vv > 85 && (stat == Attr.Accuracy))
                vv = 85;
            if (vv > 100 && (stat == Attr.CritRate || stat == Attr.Resistance))
                vv = 100;
            if (Sort[stat] != 0) {
                v2 = Threshold[stat].EqualTo(0) ? vv : Math.Min(vv, Threshold[stat]);
                if (v2 > Goal[stat] && Goal[stat] > 0)
                    v2 = (v2 - Goal[stat]) / 2 + Goal[stat];
                v2 /= Sort[stat];
                if (outvals != null)
                    outvals[stat] = v2;
            }

            return v2;
        }

        /// <summary>
        /// Overload that provides a nice string.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stat"></param>
        /// <param name="str"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double ScoreStat(Stats current, Attr stat, out string str, Stats outvals = null) {
            str = string.Empty;
            if (current == null)
                return 0;
            double vv = current[stat];
            double v2 = 0;

            if (vv > 85 && (stat == Attr.Accuracy))
                vv = 85;
            if (vv > 100 && (stat == Attr.CritRate || stat == Attr.Resistance))
                vv = 100;
            if (Sort[stat] != 0) {
                v2 = Threshold[stat].EqualTo(0) ? vv : Math.Min(vv, Threshold[stat]);
                if (v2 > Goal[stat] && Goal[stat] > 0)
                    v2 = (v2 - Goal[stat]) / 2 + Goal[stat];
                v2 /= Sort[stat];
                if (outvals != null)
                    outvals[stat] = v2;
                str = v2.ToString("0.#") + " (" + current[stat] + ")";
            }
            else {
                str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
            }

            return v2;
        }

        /// <summary>
        /// Use the builds score function to return the score contribution for a single Extra.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stat"></param>
        /// <returns></returns>
        public double ScoreExtra(Stats current, Attr stat) {
            if (current == null)
                return 0;
            double vv = current.ExtraValue(stat);

            if (Sort.ExtraGet(stat) != 0) {
                var tg = Threshold.ExtraGet(stat);
                var gg = Goal.ExtraGet(stat);
                if (tg != 0 && tg < vv)
                    vv = tg;
                if (vv > gg && gg > 0)
                    vv = (vv - gg) / 2 + gg;
                vv /= Sort.ExtraGet(stat);
                return vv;
            }
            return 0;
        }

        /// <summary>
        /// Overload that takes the branching null-check op out
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stat"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double ScoreExtra(Stats current, Attr stat, Stats outvals) {
            if (current == null)
                return 0;
            double vv = current.ExtraValue(stat);
            double v2 = 0;

            if (Sort.ExtraGet(stat) != 0) {
                var tg = Threshold.ExtraGet(stat);
                var gg = Goal.ExtraGet(stat);
                v2 = tg.EqualTo(0) ? vv : Math.Min(vv, tg);
                if (v2 > gg && gg > 0)
                    v2 = (v2 - gg) / 2 + gg;
                v2 /= Sort.ExtraGet(stat);
                if (outvals != null)
                    outvals.ExtraSet(stat, v2);
            }
            return v2;
        }

        /// <summary>
        /// Overload that outputs a nice string.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="stat"></param>
        /// <param name="str"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double ScoreExtra(Stats current, Attr stat, out string str, Stats outvals = null) {
            str = "";
            if (current == null)
                return 0;
            double vv = current.ExtraValue(stat);
            double v2 = 0;

            str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
            if (Sort.ExtraGet(stat) != 0) {
                var tg = Threshold.ExtraGet(stat);
                var gg = Goal.ExtraGet(stat);
                v2 = tg.EqualTo(0) ? vv : Math.Min(vv, tg);
                if (v2 > gg && gg > 0)
                    v2 = (v2 - gg) / 2 + gg;
                v2 /= Sort.ExtraGet(stat);
                if (outvals != null)
                    outvals.ExtraSet(stat, v2);
                str = v2.ToString("0.#") + " (" + vv + ")";
            }
            return v2;
        }

        /// <summary>
        /// Use the builds score function to return the score contribution for a single skill.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public double ScoreSkill(Stats current, int j) {
            if (current == null)
                return 0;
            double vv = current.GetSkillDamage(Attr.AverageDamage, j);

            if (Sort.DamageSkillups[j] != 0) {
                var tg = Threshold.DamageSkillups[j];
                var gg = Goal.DamageSkillups[j];
                if (tg != 0 && tg < vv)
                    vv = tg;
                if (vv > gg && gg > 0)
                    vv = (vv - gg) / 2 + gg;
                vv /= Sort.DamageSkillups[j];
                return vv;
            }
            return 0;
        }

        /// <summary>
        /// Overload that reduces the branching op count in critical loops
        /// </summary>
        /// <param name="current"></param>
        /// <param name="j"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double ScoreSkill(Stats current, int j, Stats outvals) {
            if (current == null)
                return 0;
            double vv = current.GetSkillDamage(Attr.AverageDamage, j);
            double v2 = 0;

            if (Sort.DamageSkillups[j] != 0) {
                v2 = Threshold.DamageSkillups[j].EqualTo(0) ? vv : Math.Min(vv, Threshold.DamageSkillups[j]);
                if (v2 > Goal.DamageSkillups[j] && Goal.DamageSkillups[j] > 0)
                    v2 = (v2 - Goal.DamageSkillups[j]) / 2 + Goal.DamageSkillups[j];
                v2 /= Sort.DamageSkillups[j];
                if (outvals != null)
                    outvals.DamageSkillups[j] = v2;
            }

            return v2;
        }

        /// <summary>
        /// Overload that provides a nice string
        /// </summary>
        /// <param name="current"></param>
        /// <param name="j"></param>
        /// <param name="str"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double ScoreSkill(Stats current, int j, out string str, Stats outvals = null) {
            str = "";
            if (current == null)
                return 0;
            double vv = current.GetSkillDamage(Attr.AverageDamage, j);
            double v2 = 0;

            str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
            if (Sort.DamageSkillups[j] != 0) {
                v2 = Threshold.DamageSkillups[j].EqualTo(0) ? vv : Math.Min(vv, Threshold.DamageSkillups[j]);
                if (v2 > Goal.DamageSkillups[j] && Goal.DamageSkillups[j] > 0)
                    v2 = (v2 - Goal.DamageSkillups[j]) / 2 + Goal.DamageSkillups[j];
                v2 /= Sort.DamageSkillups[j];
                if (outvals != null)
                    outvals.DamageSkillups[j] = v2;
                str = v2.ToString("0.#") + " (" + vv + ")";
            }

            return v2;
        }

        /// <summary>
        /// Calculate and cache the score
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public double LastScore(Stats current ) {
            if (lastScore == float.MinValue)
                CalcScore(current);
            return lastScore;
        }

        /// <summary>
        /// Overload that will calc score providing a breakdown
        /// </summary>
        /// <param name="current"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double LastScore(Stats current, Stats outvals) {
            if (lastScore == float.MinValue)
                CalcScore(current, outvals);
            return lastScore;
        }

        /// <summary>
        /// Use the scoring function on the given Stats, writing string out.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="writeTo"></param>
        /// <returns></returns>
        public double CalcScore(Stats current, Action<string, int> writeTo) {
            if (current == null)
                return 0;

            string str;
            double pts = 0;
            // dodgy hack for indexing in Generate ListView

            int i = 2;
            foreach (Attr stat in StatEnums) {
                pts += ScoreStat(current, stat, out str);
                writeTo?.Invoke(str, i);
                i++;
            }

            foreach (Attr stat in ExtraEnums) {
                pts += ScoreExtra(current, stat, out str);
                writeTo?.Invoke(str, i);
                i++;
            }

            for (int j = 0; j < 4; j++) {
                if (current.SkillFunc[j] != null) {
                    pts += ScoreSkill(current, j, out str);
                    writeTo?.Invoke(str, i);
                    i++;
                }
            }
            lastScore = pts;
            return pts;
        }

        /// <summary>
        /// Run the scoring function on given stats, providing formatted strings and a breakdown.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="writeTo"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double CalcScore(Stats current, Action<string, int> writeTo, Stats outvals) {
            if (current == null)
                return 0;
            if (outvals != null)
                outvals.SetTo(0);

            string str;
            double pts = 0;
            // dodgy hack for indexing in Generate ListView

            // TODO: instead of -Goal, make everything >Goal /2
            int i = 2;
            foreach (Attr stat in StatEnums) {
                pts += ScoreStat(current, stat, out str, outvals);
                writeTo?.Invoke(str, i);
                i++;
            }

            foreach (Attr stat in ExtraEnums) {
                pts += ScoreExtra(current, stat, out str, outvals);
                writeTo?.Invoke(str, i);
                i++;
            }

            for (int j = 0; j < 4; j++) {
                if (current.SkillFunc[j] != null) {
                    pts += ScoreSkill(current, j, out str, outvals);
                    writeTo?.Invoke(str, i);
                    i++;
                }
            }
            lastScore = pts;
            return pts;
        }

        /// <summary>
        /// Use the scoring function to evaluate a Rune on a Monster given a guess of the average stats of the other runes.
        /// </summary>
        /// <param name="rune"></param>
        /// <param name="current"></param>
        /// <param name="avg"></param>
        /// <returns></returns>
        public double CalcScore(Rune rune, Monster current, Stats avg = null) {
            if (rune == null)
                return 0;
            if (avg == null)
                avg = new Stats();
            double pts = 0;

            Stats s = current;
            Monster t = new Monster(current, false);

            t.ApplyRune(rune, 7);

            var a = (t.GetStats() + avg) - s;
            current.SkillFunc.CopyTo(a.SkillFunc, 0);


            int i = 2;
            foreach (Attr stat in StatEnums) {
                pts += ScoreStat(a, stat);
                i++;
            }

            foreach (Attr stat in ExtraEnums) {
                pts += ScoreExtra(a, stat);
                i++;
            }

            for (int j = 0; j < 4; j++) {
                if (a.SkillFunc[j] != null) {
                    pts += ScoreSkill(a, j);
                    i++;
                }
            }
            return pts;

        }

        /// <summary>
        /// Calculate the score of a rune applied to a monster, providing a breakdown.
        /// </summary>
        /// <param name="rune"></param>
        /// <param name="current"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double CalcScore(Rune rune, Monster current, ref Stats outvals) {
            if (rune == null)
                return 0;
            if (outvals != null)
                outvals.SetTo(0);
            double pts = 0;

            Stats s = current;
            Monster t = new Monster(current, false);

            t.ApplyRune(rune, 7);
            
            var a = t.GetStats() - s;
            current.SkillFunc.CopyTo(a.SkillFunc, 0);

            int i = 2;
            foreach (Attr stat in StatEnums) {
                pts += ScoreStat(a, stat, outvals);
                i++;
            }

            foreach (Attr stat in ExtraEnums) {
                pts += ScoreExtra(a, stat, outvals);
                i++;
            }

            for (int j = 0; j < 4; j++) {
                if (a.SkillFunc[j] != null) {
                    pts += ScoreSkill(a, j, outvals);
                    i++;
                }
            }
            return pts;

        }

        /// <summary>
        /// Score the given set of stats.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public double CalcScore(Stats current) {
            if (current == null)
                return 0;

            double pts = 0;
            // dodgy hack for indexing in Generate ListView

            // TODO: instead of -Goal, make everything >Goal /2
            int i = 2;
            foreach (Attr stat in StatEnums) {
                pts += ScoreStat(current, stat);
                i++;
            }

            foreach (Attr stat in ExtraEnums) {
                pts += ScoreExtra(current, stat);
                i++;
            }

            for (int j = 0; j < 4; j++) {
                if (current.SkillFunc[j] != null) {
                    pts += ScoreSkill(current, j);
                    i++;
                }
            }
            lastScore = pts;
            return pts;
        }

        /// <summary>
        /// Score the given stats, providing a breakdown.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="outvals"></param>
        /// <returns></returns>
        public double CalcScore(Stats current, Stats outvals) {
            if (current == null)
                return 0;
            if (outvals != null)
                outvals.SetTo(0);

            double pts = 0;
            // dodgy hack for indexing in Generate ListView

            // TODO: instead of -Goal, make everything >Goal /2
            int i = 2;
            foreach (Attr stat in StatEnums) {
                pts += ScoreStat(current, stat, outvals);
                i++;
            }

            foreach (Attr stat in ExtraEnums) {
                pts += ScoreExtra(current, stat, outvals);
                i++;
            }

            for (int j = 0; j < 4; j++) {
                if (current.SkillFunc[j] != null) {
                    pts += ScoreSkill(current, j, outvals);
                    i++;
                }
            }
            lastScore = pts;
            return pts;
        }

        /// <summary>
        /// Downgrade the includedness of a RuneSet.
        /// If a set is Included -> Uninclude. Required -> Included.
        /// </summary>
        /// <param name="set"></param>
        public void ToggleIncludedSet(RuneSet set) {
            if (BuildSets.Contains(set)) {
                BuildSets.Remove(set);
            }
            else {
                RemoveRequiredSet(set);
                BuildSets.Add(set);
            }
        }

        /// <summary>
        /// Cycle the requiredness of a set.
        /// Eg. 0 -> 1 -> 2 -> 3 -> 0.
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public int AddRequiredSet(RuneSet set) {
            if (BuildSets.Contains(set)) {
                BuildSets.Remove(set);
            }
            RequiredSets.Add(set);
            if (RequiredSets.Count(s => s == set) > 3) {
                while (RequiredSets.Count(s => s == set) > 1)
                    RequiredSets.Remove(set);
            }
            return RequiredSets.Count(s => s == set);
        }

        /// <summary>
        /// Ensure that this rune is included (ignored if Required).
        /// </summary>
        /// <param name="set"></param>
        public void AddIncludedSet(RuneSet set) {
            if (RequiredSets.Any(s => s == set) || BuildSets.Any(s => s == set))
                return;
            BuildSets.Add(set);
        }

        /// <summary>
        /// Remove any count of this RuneSet from being required.
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public int RemoveRequiredSet(RuneSet set) {
            int num = 0;
            while (RequiredSets.Any(s => s == set)) {
                RequiredSets.Remove(set);
                num++;
            }
            return num;
        }

        /// <summary>
        /// Fills in the given arrays based on the builds configuration, including fallthrough from Global > Odd/Even > Slot#.
        /// </summary>
        /// <param name="slotFakes"></param>
        /// <param name="slotPred"></param>
        public void getPrediction(int?[] slotFakes, bool[] slotPred) {
            // crank the rune prediction
            for (int i = 0; i < 6; i++) {

                getPrediction((SlotIndex)(i + 1), out int? raiseTo, out bool predictSubs);

                slotFakes[i] = raiseTo;
                slotPred[i] = predictSubs;
            }
        }

        public void getPrediction(SlotIndex ind, out int? raiseTo, out bool predictSubs) {
            raiseTo = null;
            predictSubs = false;

            int i = (int)ind;

            // find the largest number to raise to
            // if any along the tree say to predict, do it
            if (RunePrediction.ContainsKey(SlotIndex.Global)) {
                int? glevel = RunePrediction[SlotIndex.Global].Key;
                if (raiseTo == null || glevel > raiseTo)
                    raiseTo = glevel;
                predictSubs |= RunePrediction[SlotIndex.Global].Value;
            }
            if (i < 0) {
                if (RunePrediction.ContainsKey((-i) % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd)) {
                    int? mlevel = RunePrediction[(-i) % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd].Key;
                    if (raiseTo == null || mlevel > raiseTo)
                        raiseTo = mlevel;
                    predictSubs |= RunePrediction[(-i) % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd].Value;
                }
            }
            else {
                if(RunePrediction.ContainsKey(i % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd)) {
                    int? mlevel = RunePrediction[i % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd].Key;
                    if (raiseTo == null || mlevel > raiseTo)
                        raiseTo = mlevel;
                    predictSubs |= RunePrediction[i % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd].Value;
                }
                if (RunePrediction.ContainsKey((SlotIndex)(i + 1))) {
                    int? slevel = RunePrediction[(SlotIndex)(i + 1)].Key;
                    if (raiseTo == null || slevel > raiseTo)
                        raiseTo = slevel;
                    predictSubs |= RunePrediction[(SlotIndex)(i + 1)].Value;
                }
            }

        }

        //Dictionary<string, RuneFilter> rfS, Dictionary<string, RuneFilter> rfM, Dictionary<string, RuneFilter> rfG, 

        /// <summary>
        /// Go through all the Global|Odd/Even|Slot# filters and create the dominant functions.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="rFlat"></param>
        /// <param name="rPerc"></param>
        /// <param name="rTest"></param>
        /// <returns></returns>
        public bool RunFilters(int slot, out Stats rFlat, out Stats rPerc, out Stats rTest) {
            bool hasFilter = false;
            rFlat = new Stats();
            rPerc = new Stats();
            rTest = new Stats();

            // pull the filters (flat, perc, test) for all the tabs and stats
            Dictionary<string, RuneFilter> rfG = new Dictionary<string, RuneFilter>();
            if (RuneFilters.ContainsKey(SlotIndex.Global))
                rfG = RuneFilters[SlotIndex.Global];

            Dictionary<string, RuneFilter> rfM = new Dictionary<string, RuneFilter>();
            if (slot != 0 && RuneFilters.ContainsKey(slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd))
                rfM = RuneFilters[slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd];

            Dictionary<string, RuneFilter> rfS = new Dictionary<string, RuneFilter>();
            if (slot > 0 && RuneFilters.ContainsKey((SlotIndex)slot))
                rfS = RuneFilters[(SlotIndex)slot];

            foreach (string stat in StatNames) {
                RuneFilter rf = new RuneFilter();
                if (rfS.ContainsKey(stat)) {
                    rf = rfS[stat];
                    if (rfM.ContainsKey(stat))
                        rf = RuneFilter.Dominant(rf, rfM[stat]);

                    if (rfG.ContainsKey(stat))
                        rf = RuneFilter.Dominant(rf, rfG[stat]);
                }
                else {
                    if (rfM.ContainsKey(stat)) {
                        rf = rfM[stat];
                        if (rfG.ContainsKey(stat))
                            rf = RuneFilter.Dominant(rf, rfG[stat]);
                    }
                    else {
                        if (rfG.ContainsKey(stat))
                            rf = rfG[stat];
                    }
                }
                if (rf.NonZero) {
                    // put the most relevant divisor in?
                    if (rf.Flat.HasValue)
                        rFlat[stat] = rf.Flat.Value;
                    if (rf.Percent.HasValue)
                        rPerc[stat] = rf.Percent.Value;
                    if (rf.Test.HasValue)
                        rTest[stat] = rf.Test.Value;
                    hasFilter = true;
                }
            }

            return hasFilter;
        }

        /// <summary>
        /// Check the heirarchy for what FakeLevel this Rune should be.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public int GetFakeLevel(Rune r) {
            int? pred = RunePrediction.ContainsKey(SlotIndex.Global) ? RunePrediction[SlotIndex.Global].Key : null;

            if (RunePrediction.ContainsKey(r.Slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd)) {
                var kv = RunePrediction[r.Slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd];
                if (pred == null || kv.Key != null && kv.Key > pred)
                    pred = kv.Key;
            }

            if (RunePrediction.ContainsKey((SlotIndex)r.Slot)) {
                var kv = RunePrediction[(SlotIndex)r.Slot];
                if (pred == null || kv.Key != null && kv.Key > pred)
                    pred = kv.Key;
            }
            if (pred < 0)
                return 0;

            return pred ?? 0;
        }

        /// <summary>
        /// Run the RuneScoring function on a Rune with fakeness and prediction.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="raiseTo"></param>
        /// <param name="predictSubs"></param>
        /// <returns></returns>
        public double ScoreRune(Rune r, int raiseTo = 0, bool predictSubs = false) {
            RuneScoreFilter filt = LoadFilters(r.Slot);
            if (RunFilters(r.Slot, out Stats rFlat, out Stats rPerc, out Stats rTest)) {
                switch (filt.Type) {
                    case FilterType.Or:
                        return r.Or(rFlat, rPerc, rTest, raiseTo, predictSubs) ? 1 : 0;
                    case FilterType.And:
                        return r.And(rFlat, rPerc, rTest, raiseTo, predictSubs) ? 1 : 0;
                    case FilterType.Sum:
                    case FilterType.SumN:
                        return r.Test(rFlat, rPerc, raiseTo, predictSubs);
                }
            }
            return 0;
        }

        /// <summary>
        /// Determine the FilterType for a slot via the heirarchy and return the testValue.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="testVal"></param>
        /// <returns></returns>
        public RuneScoreFilter LoadFilters(int slot) {
            RuneScoreFilter filt = new RuneScoreFilter();
            // which tab we pulled the filter from

            // TODO: check what inheriting SUM (eg. Odd and 3) does
            // TODO: check what inheriting AND/OR then SUM (or visa versa)

            // find the most significant operatand of joining checks
            if (RuneScoring.ContainsKey(SlotIndex.Global)) {
                var kv = RuneScoring[SlotIndex.Global];
                if (kv.Type != FilterType.None) {
                    if (RuneFilters.ContainsKey(SlotIndex.Global) || kv.Type == FilterType.SumN)
                        filt.Type = kv.Type;
                    if (kv.Value != null) {
                        filt.Value = kv.Value;
                    }
                    if (kv.Count != null)
                        filt.Count = kv.Count;
                }
            }
            // is it and odd or even slot?
            var tmk = slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd;
            if (RuneScoring.ContainsKey(tmk)) {
                var kv = RuneScoring[tmk];
                if (kv.Type != FilterType.None) {
                    if (RuneFilters.ContainsKey(tmk) || kv.Type == FilterType.SumN)
                        filt.Type = kv.Type;
                    if (kv.Value != null) {
                        filt.Value = kv.Value;
                    }
                    if (kv.Count != null)
                        filt.Count = kv.Count;
                }
            }
            // turn the 0-5 to a 1-6
            tmk = (SlotIndex)slot;
            if (RuneScoring.ContainsKey(tmk)) {
                var kv = RuneScoring[tmk];
                if (kv.Type != FilterType.None) {
                    if (RuneFilters.ContainsKey(tmk) || kv.Type == FilterType.SumN)
                        filt.Type = kv.Type;
                    if (kv.Value != null) {
                        filt.Value = kv.Value;
                    }
                    if (kv.Count != null)
                        filt.Count = kv.Count;
                }
            }

            return filt;
        }

        /// <summary>
        /// Generate the RuneScoring function for the given slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="raiseTo"></param>
        /// <param name="predictSubs"></param>
        /// <returns></returns>
        public Predicate<Rune> MakeRuneScoring(int slot, int raiseTo = 0, bool predictSubs = false) {
            // the value to test SUM against
            RuneScoreFilter filt = LoadFilters(slot);

            // this means that runes won't get in unless they meet at least 1 criteria
            // if an operand was found, ensure the tab contains filter data
            // if there where no filters with data
            if (!RunFilters(slot, out Stats rFlat, out Stats rPerc, out Stats rTest))
                return r => true;

            // no filter data = use all
            // Set the test based on the type found
            switch (filt.Type) {
                case FilterType.None:
                    return r => true;
                case FilterType.Or:
                    return r => r.Or(rFlat, rPerc, rTest, raiseTo, predictSubs);
                case FilterType.And:
                    return r => r.And(rFlat, rPerc, rTest, raiseTo, predictSubs);
                case FilterType.Sum:
                case FilterType.SumN:
                    return r => {
                        if (!System.Diagnostics.Debugger.IsAttached)
                            RuneLog.Debug("[" + slot + "] Checking " + r.Id + " {");
                        var vv = r.Test(rFlat, rPerc, raiseTo, predictSubs);
                        if (!System.Diagnostics.Debugger.IsAttached)
                            RuneLog.Debug("\t\t" + vv + " against " + filt.Value + "}" + (vv >= filt.Value));
                        return filt.Value == null || vv >= filt.Value;
                    };
            }
            return r => false;
        }

        /// <summary>
        /// Try to determine the subs required to meet the minimum. Will guess Evens by: Slot, Health%, Attack%, Defense%
        /// </summary>
        /// <param name="slotFakes"></param>
        /// <param name="slotPred"></param>
        /// <returns></returns>
        public Stats NeededForMin(int?[] slotFakes, bool[] slotPred) {
            foreach (var rs in runes) {
                if (rs.Length <= 0)
                    return null;
            }

            var smon = (Stats)Mon;
            var smin = Minimum;

            Stats ret = smin - smon;

            var avATK = runes[0].Average(r => r.GetValue(Attr.AttackFlat, slotFakes[0] ?? 0, slotPred[0]));
            var avDEF = runes[2].Average(r => r.GetValue(Attr.DefenseFlat, slotFakes[2] ?? 0, slotPred[2]));
            var avHP = runes[4].Average(r => r.GetValue(Attr.HealthFlat, slotFakes[4] ?? 0, slotPred[4]));

            ret.Attack -= avATK;
            ret.Defense -= avDEF;
            ret.Health -= avHP;

            ret = ret.Of(smon);

            var lead = Mon.Boost(Leader);
            lead -= Mon;

            ret -= lead;

            ret.Attack *= 100;
            ret.Defense *= 100;
            ret.Health *= 100;

            // Check if we have requirements that are unlikey to be met with subs
            Attr[] evenSlots = new Attr[] { Attr.Null, Attr.Null, Attr.Null };

            // get the average MainStats for slots
            var avSel = runes[1].Where(r => r.Main.Type == Attr.Speed).ToArray();
            var avmSpeed = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Speed, slotFakes[1] ?? 0, slotPred[1]));
            avSel = runes[3].Where(r => r.Main.Type == Attr.CritRate).ToArray();
            var avmCRate = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritRate, slotFakes[3] ?? 0, slotPred[3]));
            avSel = runes[3].Where(r => r.Main.Type == Attr.CritDamage).ToArray();
            var avmCDam = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritDamage, slotFakes[3] ?? 0, slotPred[3]));
            avSel = runes[5].Where(r => r.Main.Type == Attr.Accuracy).ToArray();
            var avmAcc = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Accuracy, slotFakes[5] ?? 0, slotPred[5]));
            avSel = runes[5].Where(r => r.Main.Type == Attr.Resistance).ToArray();
            var avmRes = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Resistance, slotFakes[5] ?? 0, slotPred[5]));

            if (avmSpeed > 20 && ret.Speed > avmSpeed + 10) {
                evenSlots[0] = Attr.Speed;
                ret.Speed -= avmSpeed;
            }

            if (avmAcc > 20 && ret.Accuracy > avmAcc + 10) {
                evenSlots[2] = Attr.Accuracy;
                ret.Accuracy -= avmAcc;
            }
            else if (avmRes > 20 && ret.Resistance > avmRes + 10) {
                evenSlots[2] = Attr.Resistance;
                ret.Resistance -= avmRes;
            }

            if (avmCDam > 40 && ret.CritDamage > avmCDam + 15) {
                evenSlots[1] = Attr.CritDamage;
                ret.CritDamage -= avmCDam;
            }
            else if (avmCRate > 30 && ret.CritRate > avmCRate - 5) {
                evenSlots[1] = Attr.CritRate;
                ret.CritRate -= avmCRate;
            }

            // go back 6,4,2 for putting things in
            for (int i = 2; i >= 0; i--) {
                if (evenSlots[i] <= Attr.Null) {
                    if (ret.Health > 50) {
                        evenSlots[i] = Attr.HealthPercent;
                        ret.Health -= 50;
                    }
                    else if (ret.Attack > 50) {
                        evenSlots[i] = Attr.AttackPercent;
                        ret.Attack -= 50;
                    }
                    else if (ret.Defense > 50) {
                        evenSlots[i] = Attr.DefensePercent;
                        ret.Defense -= 50;
                    }
                }
            }

            foreach (Attr a in StatEnums) {
                if (ret[a] < 0)
                    ret[a] = 0;
            }

            return ret;
        }

        /// <summary>
        /// Uses the ManageStats to find runes which almost came close to being picked.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Rune> GetPowerupRunes() {
            if (!Loads.Any())
                return new Rune[] { };
            double max = Loads.Max(g => g.score);
            foreach (var r in Loads.SelectMany(m => m.Current.Runes)) {
                r.manageStats.AddOrUpdate("besttestscore", 0, (k, v) => 0);
            }

            foreach (var g in Loads) {
                foreach (var r in g.Current.Runes) {
                    r.manageStats.AddOrUpdate("besttestscore", g.score / max, (k, v) => v < g.score / max ? g.score / max : v);
                }
            }

            return Loads.SelectMany(b => b.Current.Runes.Where(r => Math.Max(12, r.Level) < r.Rarity * 3 || r.Level < 12 || r.Level < GetFakeLevel(r))).Distinct();
        }

        /// <summary>
        /// Return the best increase of <paramref name="attr"/> of the subset of RuneSets <paramref name="ofThese"/>
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="ofThese"></param>
        /// <returns>Percentage stat increase</returns>
        public static double BestEffect(Attr attr, IEnumerable<RuneSet> ofThese = null) {
            switch (attr) {
                case Attr.HealthFlat:
                case Attr.HealthPercent:
                    if (ofThese == null || ofThese.Contains(RuneSet.Energy))
                        return 15 * 3;
                    if (ofThese.Contains(RuneSet.Enhance))
                        return 8 * 3;
                    break;
                case Attr.AttackFlat:
                case Attr.AttackPercent:
                    if (ofThese == null || ofThese.Contains(RuneSet.Fatal))
                        return 35;
                    if (ofThese.Contains(RuneSet.Fight))
                        return 8 * 3;
                    break;
                case Attr.DefenseFlat:
                case Attr.DefensePercent:
                    if (ofThese == null || ofThese.Contains(RuneSet.Guard))
                        return 15 * 3;
                    if (ofThese.Contains(RuneSet.Determination))
                        return 8 * 3;
                    break;
                case Attr.SpeedPercent:
                case Attr.Speed:
                    if (ofThese == null || ofThese.Contains(RuneSet.Swift))
                        return 25;
                    break;
                case Attr.CritRate:
                    if (ofThese == null || ofThese.Contains(RuneSet.Blade))
                        return 12 * 3;
                    break;
                case Attr.CritDamage:
                    if (ofThese == null || ofThese.Contains(RuneSet.Rage))
                        return 40;
                    break;
                case Attr.Resistance:
                    if (ofThese == null || ofThese.Contains(RuneSet.Endure))
                        return 20 * 3;
                    if (ofThese.Contains(RuneSet.Tolerance))
                        return 10 * 3;
                    break;
                case Attr.Accuracy:
                    if (ofThese == null || ofThese.Contains(RuneSet.Focus))
                        return 20 * 3;
                    if (ofThese.Contains(RuneSet.Accuracy))
                        return 10 * 3;
                    break;
            }
            return 0;
        }

        /// <summary>
        /// This will work through runes[][] to remove runes which would <i>never</i> be able to meet the minimum requirements.
        /// </summary>
        private void cleanMinimum() {

            if (Minimum == null || !Minimum.IsNonZero) {
                return;
            }

            int?[] fake = new int?[6];
            bool[] pred = new bool[6];

            getPrediction(fake, pred);

            var hasSets = runes.SelectMany(r => r).Select(r => r.Set).Distinct();

            bool removedOne = true;
            while (removedOne) {
                // we haven't removed any on this pass
                removedOne = false;

                // go through each non-zero stat
                foreach( var attr in new Attr[] { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed }) {

                    if (Minimum[attr].EqualTo(0))
                        continue;

                    var bb = Mon[attr];

                    for (int i = 0; i < 6; i++) {
                        // start the counting at the best runeset bonus we could get
                        double totEff = bb * (1 + BestEffect(attr, hasSets) * 0.01f) ;

                        for (int j = 0; j < 6; j++) {
                            if (i == j)
                                continue;

                            double maxEff = 0;

                            double eff = 0;
                            foreach (var u in runes[j]) {
                                var p = u[attr, fake[j] ?? 0, pred[j]];
                                var f = u[attr - 1, fake[j] ?? 0, pred[j]];
                                if (attr == Attr.Speed) {
                                    f = p;
                                    p = 0;
                                }
                                eff = bb * ( p * 0.01) + f;

                                if (eff > maxEff) {
                                    maxEff = eff;
                                }
                            }
                            totEff += maxEff;
                        }

                        // pick the runes which really won't make the cut
                        var garbo = runes[i].Where(r => bb * ( 0.01 * (r[attr, fake[i] ?? 0, pred[i]] + BestEffect(attr, new[] { r.Set }))) + r[attr - 1, fake[i] ?? 0, pred[i]] + totEff < Minimum[attr]).ToArray();

                        if (garbo.Length > 0)
                            removedOne = true;

                        runes[i] = runes[i].Except(garbo).ToArray();
                    }
                }

                foreach (var attr in new Attr[] {  Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy }) {

                    if (Minimum[attr].EqualTo(0))
                        continue;

                    var bb = Mon[attr];

                    for (int i = 0; i < 6; i++) {
                        double totEff = bb + BestEffect(attr, hasSets);

                        for (int j = 0; j < 6; j++) {
                            if (i == j)
                                continue;

                            double maxEff = 0;

                            double eff = 0;
                            foreach (var u in runes[j]) {
                                var f = u[attr, fake[j] ?? 0, pred[j]];
                                eff = bb + f;

                                if (eff > maxEff) {
                                    maxEff = eff;
                                }
                            }
                            totEff += maxEff;
                        }

                        var garbo = runes[i].Where(r => bb + r[attr, fake[i] ?? 0, pred[i]] + BestEffect(attr, new[] { r.Set }) + totEff < Minimum[attr]).ToArray();

                        if (garbo.Length > 0)
                            removedOne = true;

                        runes[i] = runes[i].Except(garbo).ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Make sure that for each set type, there are enough slots with runes in them
        /// Eg. if only 1,4,5 have Violent, remove all violent runes because you need 4
        /// for each included set
        /// </summary>
        private void cleanBroken() {
            if (!AllowBroken) {
                var used = runes[0].Concat(runes[1]).Concat(runes[2]).Concat(runes[3]).Concat(runes[4]).Concat(runes[5]).Select(r => r.Set).Distinct();

                foreach (RuneSet s in used) {
                    // find how many slots have acceptable runes for it
                    int slots = 0;
                    for (int i = 0; i < 6; i++) {
                        if (runes[i].Any(r => r.Set == s))
                            slots += 1;
                    }
                    // if there isn't enough slots
                    if (slots < Rune.SetRequired(s)) {
                        // remove that set
                        for (int i = 0; i < 6; i++) {
                            runes[i] = runes[i].Where(r => r.Set != s).ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks how many of the stat scores in s are met or exceeded by r. 
        /// Assumes Stat A/D/H are percent
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private int runeHasStats(Rune r, Stats s) {
            int ret = 0;

            if (r.HealthPercent[0] >= s.Health)
                ret++;
            if (r.AttackPercent[0] >= s.Attack)
                ret++;
            if (r.DefensePercent[0] >= s.Defense)
                ret++;
            if (r.Speed[0] >= s.Speed)
                ret++;

            if (r.CritDamage[0] >= s.CritDamage)
                ret++;
            if (r.CritRate[0] >= s.CritRate)
                ret++;
            if (r.Accuracy[0] >= s.Accuracy)
                ret++;
            if (r.Resistance[0] >= s.Resistance)
                ret++;

            return ret;
        }

        /// <summary>
        /// Returns the sum of how much the rune fufills the Stat requirement.
        /// Eg. if the stat has 10% ATK & 20% HP, and the rune only has 5% ATK, it will return 0.5.
        /// Assumes Stat A/D/H are percent
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private double runeVsStats(Rune r, Stats s) {
            double ret = 0;

            if (s.Health > 0)
                ret += r.HealthPercent[0] / s.Health;
            if (s.Attack > 0)
                ret += r.AttackPercent[0] / s.Attack;
            if (s.Defense > 0)
                ret += r.DefensePercent[0] / s.Defense;
            if (s.Speed > 0)
                ret += r.Speed[0] / s.Speed;

            if (s.CritDamage > 0)
                ret += r.CritDamage[0] / s.CritDamage;
            if (s.CritRate > 0)
                ret += r.CritRate[0] / s.CritRate;
            if (s.Accuracy > 0)
                ret += r.Accuracy[0] / s.Accuracy;
            if (s.Resistance > 0)
                ret += r.Resistance[0] / s.Resistance;

            //Health / ((1000 / (1000 + Defense * 3)))
            if (s.EffectiveHP > 0) {
                var sh = 6000 * (100 + r.HealthPercent[0] + 20) / 100.0 + r.HealthFlat[0] + 1200;
                var sd = 300 * (100 + r.DefensePercent[0] + 10) / 100.0 + r.DefenseFlat[0] + 70;

                double delt = 0;
                delt += sh / (1000 / (1000 + sd * 3));
                delt -= 6000 / (1000 / (1000 + 300.0 * 3));
                ret += delt / s.EffectiveHP;
            }

            //Health / ((1000 / (1000 + Defense * 3 * 0.3)))
            if (s.EffectiveHPDefenseBreak > 0) {
                var sh = 6000 * (100 + r.HealthPercent[0] + 20) / 100.0 + r.HealthFlat[0] + 1200;
                var sd = 300 * (100 + r.DefensePercent[0] + 10) / 100.0 + r.DefenseFlat[0] + 70;

                double delt = 0;
                delt += sh / (1000 / (1000 + sd * 0.9));
                delt -= 6000 / (1000 / (1000 + 300 * 0.9));
                ret += delt / s.EffectiveHPDefenseBreak;
            }

            // (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage + CritDamage / 100)
            if (s.MaxDamage > 0) {
                var sa = 300 * (100 + r.AttackPercent[0] + 20) / 100.0 + r.AttackFlat[0] + 100;
                var cd = 50 + r.CritDamage[0] + 20;

                double delt = 0;
                delt += sa * (1 + cd / 100);
                delt -= 460 * (1 + 0.7);
                ret += delt / s.MaxDamage;
            }

            // (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage + CritDamage / 100 * Math.Min(CritRate, 100) / 100)
            if (s.AverageDamage > 0) {
                var sa = 300 * (100 + r.AttackPercent[0] + 20) / 100.0 + r.AttackFlat[0] + 100;
                var cd = 50 + r.CritDamage[0] + 20;
                var cr = 15 + r.CritRate[0] + 15;

                double delt = 0;
                delt += sa * (1 + cd / 100 * Math.Min(cr, 100) / 100);
                delt -= 460 * (1 + 0.7 * 0.3);
                ret += delt / s.AverageDamage;
            }

            // ExtraValue(Attr.AverageDamage) * Speed / 100
            if (s.DamagePerSpeed > 0) {
                var sa = 300 * (100 + r.AttackPercent[0] + 20) / 100.0 + r.AttackFlat[0] + 100;
                var cd = 50 + r.CritDamage[0] + 20;
                var cr = 15 + r.CritRate[0] + 15;
                var sp = 100 + r.Speed[0] + 15;

                double delt = 0;
                delt += sa * (1 + cd / 100 * Math.Min(cr, 100) / 100) * sp / 100;
                delt -= 460 * (1 + 0.70 * 0.30) * 1.15;
                ret += delt / s.DamagePerSpeed;
            }

            return ret;
        }

    }

}
