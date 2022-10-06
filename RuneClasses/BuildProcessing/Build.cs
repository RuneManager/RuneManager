#define BUILD_PRECHECK_BUILDS
//#define BUILD_PRECHECK_BUILDS_DEBUG

using Newtonsoft.Json;
using RuneOptim.Management;
using RuneOptim.swar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Combinatorics.Collections;

namespace RuneOptim.BuildProcessing
{


    /// <summary>
    /// Contains most of the data needed to outline build requirements.
    /// The heavy lifter, trying to move logic out of it :S
    /// </summary>
    public partial class Build {
        // allows iterative code, probably slow but nice to write and integrates with WinForms at a moderate speed
        // TODO: have another go at it
        //[Obsolete("Consider changing to statEnums, but HP vs. HP%")]
        public static readonly string[] StatNames = { AttrStr.HP, AttrStr.ATK, AttrStr.DEF, AttrStr.SPD, AttrStr.CR, AttrStr.CD, AttrStr.RES, AttrStr.ACC };
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
        public IBuildRunner Runner;

        [JsonIgnore]
        private TaskCompletionSource<IBuildRunner> tcs = new TaskCompletionSource<IBuildRunner>();

        /// <summary>
        /// awaitable for when the build actually starts running
        /// </summary>
        [JsonIgnore]
        public Task<IBuildRunner> StartedBuild => tcs.Task;

        /// <summary>
        /// Store the score of the last run in here for loading?
        /// </summary>
        private double lastScore = float.MinValue;

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
        /// Sets expicitly selected in the optimizer as optional.
        /// </summary>
        [JsonProperty("BuildSets")]
        public ObservableCollection<RuneSet> BuildSets { get; set; } = new ObservableCollection<RuneSet>();

        [JsonIgnore]
        public IEnumerable<RuneSet> OptionalSets { get => RuneSets.ValidSets(RequiredSets, BuildSets, AllowBroken); }

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
        public Rune[][] Runes { get; set; } = new Rune[6][];

        [JsonIgnore]
        public Craft[] Grinds { get; set; }

        // ----------------

        /// <summary>
        /// How to sort the stats
        /// </summary>
        [JsonIgnore]
        public Func<Stats, int> SortFunc { get; set; }

        [JsonIgnore]
        public long Time { get; set; }

        [JsonIgnore]
        public Stats Shrines { get; set; } = new Stats();
        public Guild Guild { get; set; }

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
            Guild = rhs.Guild;

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
                Shrines = Shrines,
                Guild = Guild
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
            Runner = null;
            try {
                Runner = def.GetRunner();
                // TODO: fixme

                if (Runner != null) {
                    Runner.Setup(this, settings);
                    tcs.TrySetResult(Runner);
                    this.Best = Runner.Run(Runes.SelectMany(r => r)).Result;

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
                Runner?.TearDown();
                Runner = null;
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

        public double LastScore() {
            if (lastScore == float.MinValue)
                CalcScore(this.Best ?? this.Mon ?? throw new NullReferenceException("No monster to calculate score for build " + ID + ": " + MonName));
            return lastScore;
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
                if (current.SkillsFunction[j] != null) {
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
                if (current.SkillsFunction[j] != null) {
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
            current.SkillsFunction.CopyTo(a.SkillsFunction, 0);


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
                if (a.SkillsFunction[j] != null) {
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
            current.SkillsFunction.CopyTo(a.SkillsFunction, 0);

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
                if (a.SkillsFunction[j] != null) {
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
                if (current.SkillsFunction[j] != null) {
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
                if (current.SkillsFunction[j] != null) {
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
        public void GetPrediction(int?[] slotFakes, bool[] slotPred) {
            // crank the rune prediction
            for (int i = 0; i < 6; i++) {

                GetPrediction((SlotIndex)(i + 1), out int? raiseTo, out bool predictSubs);

                slotFakes[i] = raiseTo;
                slotPred[i] = predictSubs;
            }
        }

        public void GetPrediction(SlotIndex ind, out int? raiseTo, out bool predictSubs) {
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
            foreach (var rs in Runes) {
                if (rs.Length <= 0)
                    return null;
            }

            var smon = (Stats)Mon;
            var smin = Minimum;

            Stats ret = smin - smon;

            var avATK = Runes[0].Average(r => r.GetValue(Attr.AttackFlat, slotFakes[0] ?? 0, slotPred[0]));
            var avDEF = Runes[2].Average(r => r.GetValue(Attr.DefenseFlat, slotFakes[2] ?? 0, slotPred[2]));
            var avHP = Runes[4].Average(r => r.GetValue(Attr.HealthFlat, slotFakes[4] ?? 0, slotPred[4]));

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
            var avSel = Runes[1].Where(r => r.Main.Type == Attr.Speed).ToArray();
            var avmSpeed = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Speed, slotFakes[1] ?? 0, slotPred[1]));
            avSel = Runes[3].Where(r => r.Main.Type == Attr.CritRate).ToArray();
            var avmCRate = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritRate, slotFakes[3] ?? 0, slotPred[3]));
            avSel = Runes[3].Where(r => r.Main.Type == Attr.CritDamage).ToArray();
            var avmCDam = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritDamage, slotFakes[3] ?? 0, slotPred[3]));
            avSel = Runes[5].Where(r => r.Main.Type == Attr.Accuracy).ToArray();
            var avmAcc = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Accuracy, slotFakes[5] ?? 0, slotPred[5]));
            avSel = Runes[5].Where(r => r.Main.Type == Attr.Resistance).ToArray();
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
            double max = Loads.Max(g => g.Score);
            foreach (var r in Loads.SelectMany(m => m.Current.Runes)) {
                r.ManageStats.AddOrUpdate("besttestscore", 0, (k, v) => 0);
            }

            foreach (var g in Loads) {
                foreach (var r in g.Current.Runes) {
                    r.ManageStats.AddOrUpdate("besttestscore", g.Score / max, (k, v) => v < g.Score / max ? g.Score / max : v);
                }
            }

            return Loads.SelectMany(b => b.Current.Runes.Where(r => Math.Max(12, r.Level) < r.Rarity * 3 || r.Level < 12 || r.Level < GetFakeLevel(r))).Distinct();
        }

        /// <summary>
        /// Return the best increase of <paramref name="attr"/> from the possible RuneSets <paramref name="maxSets"/>
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="maxSets"></param>
        /// <returns>Percentage stat increase</returns>
        public static double BestSetEffect(Attr attr, Dictionary<RuneSet, int> maxSets = null) {
            switch (attr) {
                case Attr.HealthFlat:
                case Attr.HealthPercent:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Energy))
                        return 15 * maxSets[RuneSet.Energy];
                    if (maxSets.ContainsKey(RuneSet.Enhance))
                        return 8 * maxSets[RuneSet.Enhance];
                    break;
                case Attr.AttackFlat:
                case Attr.AttackPercent:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Fatal))
                        return 35;
                    if (maxSets.ContainsKey(RuneSet.Fight))
                        return 8 * maxSets[RuneSet.Fight];
                    break;
                case Attr.DefenseFlat:
                case Attr.DefensePercent:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Guard))
                        return 15 * maxSets[RuneSet.Guard];
                    if (maxSets.ContainsKey(RuneSet.Determination))
                        return 8 * maxSets[RuneSet.Guard];
                    break;
                case Attr.SpeedPercent:
                case Attr.Speed:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Swift))
                        return 25;
                    break;
                case Attr.CritRate:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Blade))
                        return 12 * maxSets[RuneSet.Blade];
                    break;
                case Attr.CritDamage:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Rage))
                        return 40;
                    break;
                case Attr.Resistance:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Endure))
                        return 20 * maxSets[RuneSet.Endure];
                    if (maxSets.ContainsKey(RuneSet.Tolerance))
                        return 10 * maxSets[RuneSet.Tolerance];
                    break;
                case Attr.Accuracy:
                    if (maxSets == null || maxSets.ContainsKey(RuneSet.Focus))
                        return 20 * maxSets[RuneSet.Focus];
                    if (maxSets.ContainsKey(RuneSet.Accuracy))
                        return 10 * maxSets[RuneSet.Accuracy];
                    break;
            }
            return 0;
        }

        /// <summary>
        /// Calculates the maximum number of each set that could be included based on the specific RequiredSets and BuildSets
        /// </summary>
        /// <param name="requiredSets"></param>
        /// <param name="buildSets"></param>
        /// <returns></returns>
        private Dictionary<RuneSet, int> GetMaxSetCount(IEnumerable<RuneSet> requiredSets, IEnumerable<RuneSet> buildSets)
        {
            Dictionary<RuneSet, int> setCounts = new Dictionary<RuneSet, int>();
            int required = 0;
            foreach (var rs in requiredSets)
            {
                required += rs.Size();
                if (setCounts.ContainsKey(rs))
                {
                    setCounts[rs]++;
                }
                else
                {
                    setCounts[rs] = 1;
                };
            }
            if (required == 6)
                return setCounts;
            foreach (var bs in buildSets)
            {
                setCounts[bs] = (int)Math.Floor((decimal)(6-required) / bs.Size());
            }
            return setCounts;
        }

        /// <summary>
        /// This will work through runes[][] to remove runes which would <i>never</i> be able to meet the minimum requirements.
        /// </summary>
        private void cleanMinimum()
        {
            if (AllowBroken)
                cleanMinimumLegacy();
            else
                cleanMinimumSetAware();
        }

        /// <summary>
        /// Issue #197 was accellerated due to some undesirable side-effects from fixes in #200
        /// </summary>
        private void cleanMinimumSetAware()
        {
            // attributes that require tracking for minimum analysis
            List<Attr> attrWithMin = AttrWithMin(Minimum);
            // filtering not practical without minimums
            if (attrWithMin.Count() == 0)
                return;

            // split runes by set and slot
            Dictionary<RuneSet, Rune[][]> runesBySet = SplitRunesBySet(Runes);

            while (true)
            {
                // get maximums for each slot combination
                Dictionary<RuneSet, Stats[]> maxBySlot = new Dictionary<RuneSet, Stats[]>();
                Dictionary<RuneSet, Stats[]> maxByFullSet = new Dictionary<RuneSet, Stats[]>();
                foreach (var set in runesBySet)
                {
                    maxBySlot[set.Key] = new Stats[6];
                    for (var i = 0; i < 6; i++)
                    {
                        maxBySlot[set.Key][i] = GetMaxBySlot(runesBySet[set.Key][i], attrWithMin.ToArray(), i);
                    }
                    maxByFullSet[set.Key] = GetMaxByFullSet(maxBySlot[set.Key], set.Key.Size() == 4);
                }

                // deep copy rune lists as set of ineligible runes
                Dictionary<RuneSet, Rune[][]> ineligible = new Dictionary<RuneSet, Rune[][]>();
                foreach (var set in runesBySet)
                {
                    ineligible[set.Key] = new Rune[6][];
                    for (var slot = 0; slot < 6; slot++)
                    {
                        ineligible[set.Key][slot] = (Rune[])runesBySet[set.Key][slot]?.Clone();
                    }
                }

                var optionalSets = RuneSets.ValidSets(RequiredSets, BuildSets, AllowBroken);
                // iterate through possible set combinations, removing eligible runes from ineligible
                foreach (var sets in ValidSets(RequiredSets, optionalSets))
                {
                    RemoveEligible(sets, maxBySlot, maxByFullSet, ineligible);
                }

                // break if no changes
                if (!ineligible.Any(s => s.Value.Any(slot => slot != null && slot.Count() > 0)))
                {
                    for (var slot = 0; slot < 6; slot++)
                    {
                        Runes[slot] = (Rune[])runesBySet.SelectMany(s => s.Value[slot] == null ? new Rune[0] : s.Value[slot]).ToArray();
                    }
                    break;
                }

                // remove ineligible runes
                foreach (var entry in ineligible)
                {
                    for (var slot = 0; slot < 6; slot++)
                    {
                        // if a slot never had runes, ther'es nothing to do
                        if (runesBySet[entry.Key][slot] == null)
                            continue;
                        runesBySet[entry.Key][slot] = runesBySet[entry.Key][slot].Except(ineligible[entry.Key][slot]).ToArray();
                    }
                }
            }

        }

        /// <summary>
        /// Removes any eligible runes from the ineligible list
        /// </summary>
        /// <param name="sets"></param>
        /// <param name="maxBySetAndSlot"></param>
        /// <param name="ineligible"></param>
        private void RemoveEligible(RuneSet[] sets, Dictionary<RuneSet, Stats[]> maxBySlot, Dictionary<RuneSet, Stats[]> maxBySetAndSlot, Dictionary<RuneSet, Rune[][]> ineligible)
        {
            if (ineligible.Count == 0)
                return;
            var percentBonuses = Shrines + Leader + Guild.AsStats();
            foreach (var set in sets)
                percentBonuses += set.AsStats();

            if (sets.Any(s => s.Size() == 4))
            {
                // 4+2
                RuneSet set4 = sets.First(s => s.Size() == 4);
                RuneSet set2 = sets.First(s => s.Size() == 2);
                for (int first = 0; first < 5; first++)
                {
                    for (int second = first+1; second < 6; second++)
                    {
                        int index = FullSetIndex(first, second);

                        // one of the sets is missing a rune so nothing can be eligible
                        if (maxBySetAndSlot[set4][index] == null || maxBySetAndSlot[set2][index] == null)
                            continue;

                        // Mon[attr] already applied to get raw stat numbers
                        var runeMax = maxBySetAndSlot[set4][index] + maxBySetAndSlot[set2][index];
                        for (int slot = 0; slot < 6; slot ++)
                        {
                            RuneSet slotSet = slot == first || slot == second ? set2 : set4;
                            if (ineligible[slotSet][slot] == null || ineligible[slotSet][slot].Length == 0)
                                continue;
                            ineligible[slotSet][slot] = RemoveEligibleBySlot(
                                ineligible[slotSet][slot],
                                percentBonuses,
                                // the best runes for all 6 slots minus the best rune for this slot (so we can test other runes in its place)
                                runeMax - maxBySlot[slotSet][slot],
                                // we have to pass around slot numbers for calls to predictions
                                slot
                            ).ToArray();
                        }
                    }
                }
            }
            else
            {
                Permutations<int> permutations = new Permutations<int>(new int[] { 0, 1, 2, 3, 4, 5 }, GenerateOption.WithoutRepetition);

                foreach (IList<int> indexes in permutations)
                {
                    // we don't care which order the set is presented so ignore "backwards" versions
                    if (indexes[1] < indexes[0] || indexes[3] < indexes[2] || indexes[5] < indexes[4])
                        continue;
                    int set1index = FullSetIndex(indexes[0], indexes[1]);
                    int set2index = FullSetIndex(indexes[2], indexes[3]);
                    int set3index = FullSetIndex(indexes[4], indexes[5]);

                    // one of the sets is missing a rune so nothing can be eligible
                    if (maxBySetAndSlot[sets[0]][set1index] == null || maxBySetAndSlot[sets[1]][set2index] == null || maxBySetAndSlot[sets[2]][set3index] == null)
                        continue;

                    var setMax = maxBySetAndSlot[sets[0]][set1index] + maxBySetAndSlot[sets[1]][set2index] + maxBySetAndSlot[sets[2]][set3index];
                    for (int slot = 0; slot < 6; slot++)
                    {
                        RuneSet slotSet = sets[(int)indexes.IndexOf(slot) / 2];
                        if (ineligible[slotSet][slot] == null)
                            continue;
                        ineligible[slotSet][slot] = RemoveEligibleBySlot(
                            ineligible[slotSet][slot],
                            percentBonuses,
                            setMax - maxBySlot[slotSet][slot],
                            slot
                        ).ToArray();
                    }
                }
            }
        }

        public IEnumerable<Rune> RemoveEligibleBySlot(IEnumerable<Rune> ineligible, Stats percentBonuses, Stats flatRuneStats, int slot)
        {
            if (ineligible == null || ineligible.Count() == 0)
                return ineligible;

            int?[] fake = new int?[6];
            bool[] pred = new bool[6];
            GetPrediction(fake, pred);

            // stats that are diven by a flat and a percentage
            foreach (var attr in new Attr[] { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.SpeedPercent})
            {
                // these sets are analyzed using a float where 0.01 represents 1%
                if (Minimum[attr].EqualTo(0))
                    continue;

                // pick the runes which can't achieve the minimum
                var eligible = ineligible.Where(r => Math.Ceiling(
                    // monster base
                    Mon[attr]
                    // rune percentage bonus
                    + Mon[attr] * r[attr, fake[slot] ?? 0, pred[slot]] * 0.01f
                    // rune flat bonus
                    + r[attr - 1, fake[slot] ?? 0, pred[slot]]
                    // other runes
                    + flatRuneStats[attr]
                    // other (percentage) bonus
                    + Mon[attr] * percentBonuses[attr] * 0.01f
                    ) >= Minimum[attr]
                );

                ineligible = ineligible.Except(eligible);
            }

            // stats that are only a percentage
            foreach (var attr in new Attr[] { Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy })
            {

                if (Minimum[attr].EqualTo(0))
                    continue;

                var eligible = ineligible.Where(r =>
                    // monster base
                    Mon[attr]
                    // flat bonus
                    + r[attr, fake[slot] ?? 0, pred[slot]]
                    // rune bonus
                    + flatRuneStats[attr]
                    // other percentage bonuses
                    + percentBonuses[attr]
                    >= Minimum[attr]
                );

                ineligible = ineligible.Except(eligible);
            }

            return ineligible;
        }

        /// <summary>
        /// Gets a list of attributes required to evaluate minimums
        /// </summary>
        /// <param name="minimum"></param>
        /// <returns></returns>
        private List<Attr> AttrWithMin(Stats minimum)
        {
            List<Attr> minStats = new List<Attr>();
            foreach (var attr in new Attr[] { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy })
            {
                // these sets are analyzed using a float where 0.01 represents 1%
                if (!Minimum[attr].EqualTo(0))
                {
                    minStats.Add(attr);
                    if (attr == Attr.HealthPercent)
                        minStats.Add(Attr.HealthFlat);
                    else if (attr == Attr.AttackPercent)
                        minStats.Add(Attr.AttackFlat);
                    else if (attr == Attr.DefensePercent)
                        minStats.Add(Attr.DefenseFlat);
                }
            }
            return minStats;
        }

        static int[] FirstSlotOffsets = { 0, 5, 9, 12, 14 };
        int FullSetIndex(int first, int second)
        {
            return FirstSlotOffsets[first] + second - first - 1;
        }

        /// <summary>
        /// Splits runes by set
        /// </summary>
        /// <param name="runes"></param>
        /// <returns></returns>
        private Dictionary<RuneSet, Rune[][]> SplitRunesBySet(Rune[][] runes)
        {
            Dictionary<RuneSet, Rune[][]> runesBySet = new Dictionary<RuneSet, Rune[][]>();

            // initialize set arrays
            var hasSets = Runes.SelectMany(r => r).Select(r => r.Set).Distinct();
            foreach (var set in hasSets)
            {
                runesBySet[set] = new Rune[6][];
            }
            // split runes by set
            for (var i = 0; i < 6; i++)
            {
                foreach (var runesInSet in Runes[i].GroupBy(r => r.Set))
                {
                    runesBySet[runesInSet.Key][i] = runesInSet.ToArray();
                }
            }

            return runesBySet;
        }

        /// <summary>
        /// Outer set generator converts required and optional sets into valid sets of sets
        /// </summary>
        /// <param name="required"></param>
        /// <param name="included"></param>
        /// <returns></returns>
        private IEnumerable<RuneSet[]> ValidSets(IEnumerable<RuneSet> required, IEnumerable<RuneSet> included)
        {
            int remaining = 6 - required.Select(s => s.Size()).Sum();

            // required sets exhaust space
            if (remaining == 0)
                yield return required.ToArray();

            // conbine required and included
            foreach (var runeSet in ExtendSets(required, included, remaining))
                yield return runeSet;
        }

        /// <summary>
        /// Inner generator recursively adds optional sets and returns 6-rune sets of sets
        /// </summary>
        /// <param name="baseline"></param>
        /// <param name="optional"></param>
        /// <param name="remaining"></param>
        /// <returns></returns>
        private IEnumerable<RuneSet[]> ExtendSets(IEnumerable<RuneSet> baseline, IEnumerable<RuneSet> optional, int remaining)
        {
            if (remaining == 0)
                yield return baseline.ToArray();
            List<RuneSet[]> sets = new List<RuneSet[]>();
            foreach (var set in optional.Where(s => s.Size() <= remaining)) {
                foreach (var result in ExtendSets(baseline.Concat(new[] { set }), optional, remaining - set.Size()))
                    yield return result;
            }
        }

        private Stats GetMaxBySlot(Rune[] runes, Attr[] attrs, int slot)
        {
            int?[] fake = new int?[6];
            bool[] pred = new bool[6];

            GetPrediction(fake, pred);

            // if no rune exist we need to prevent the system from treating the set as completable
            if (runes == null || runes.Length == 0)
                return null;
            Stats slotMaximum = new Stats();
            foreach (var rune in runes)
            {
                foreach (var attr in attrs)
                {
                    var val = rune[attr, fake[slot] ?? 0, pred[slot]];
                    if (slotMaximum[attr] < val)
                        slotMaximum[attr] = val;
                }
            }
             
            return slotMaximum;
        }

        /// <summary>
        /// Calculates the maximum possible values of attributes in <paramref name="attrs"/> possible for
        /// complete sets of <paramref name="set"/> among <paramref name="runes"/> 
        /// </summary>
        /// <param name="runes"></param>
        /// <param name="set"></param>
        /// <param name="attrs"></param>
        /// <returns></returns>
        private Stats[] GetMaxByFullSet(Stats[] slotMaximums, bool set4)
        {

            Stats[] setMaximums = new Stats[15];
            // for each set
            for (var first = 0; first < 5; first ++)
            {
                for (var second = first + 1; second < 6; second++)
                {
                    // if any of the slots are null, the combination isn't valid
                    // otherwise, the maximum value is calculated
                    int index = FullSetIndex(first, second);
                    if (!set4)
                    {
                        if (slotMaximums[first] == null || slotMaximums[second] == null)
                            // no runes exist in a slot
                            setMaximums[index] = null;
                        else
                            setMaximums[index] = slotMaximums[first] + slotMaximums[second];
                    } else
                    {
                        setMaximums[index] = new Stats();
                        for (var slot = 0; slot < 6; slot++) {
                            if (slot == first || slot == second)
                                continue;
                            // no runes exist in a slot
                            if (slotMaximums[slot] == null)
                            {
                                setMaximums[index] = null;
                                break;
                            }
                            setMaximums[index] += slotMaximums[slot];
                        }
                    }
                }
            }
            return setMaximums;

        }

        /// <summary>
        /// The version resulting from issue #200 which was intented to address issue #198 but drew attention to missing
        /// bonuses like leaders and guilds.
        /// </summary>
        private void cleanMinimumLegacy() {
            
            if (Minimum == null || !Minimum.IsNonZero) {
                return;
            }

            int?[] fake = new int?[6];
            bool[] pred = new bool[6];

            GetPrediction(fake, pred);

            var hasSets = Runes.SelectMany(r => r).Select(r => r.Set).Distinct();
            var requiredSets = RequiredSets.Where(r => hasSets.Contains(r));
            var buildSets = BuildSets.Where(r => hasSets.Contains(r));

            var maxSets = GetMaxSetCount(requiredSets, buildSets);

            bool removedOne = true;
            while (removedOne) {
                // we haven't removed any on this pass
                removedOne = false;

                // stats that are diven by a flat and a percentage
                foreach( var attr in new Attr[] { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed }) {
                    // these sets are analyzed using a float where 0.01 represents 1%

                    if (Minimum[attr].EqualTo(0))
                        continue;

                    var monBase = Mon[attr];
                    double bestSetEffect = BestSetEffect(attr, maxSets);
                    double guildEffect = Guild.ByStat(attr);
                    double shrineEffect = Shrines[attr];
                    double leaderEffect = Leader[attr];

                    // best rune for each slot
                    double[] bestRuneEffect = new double[] { 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < 6; i++)
                    {
                        double maxEff = 0;
                        double eff = 0;
                        foreach (var u in Runes[i])
                        {
                            var percentageBonus = u[attr, fake[i] ?? 0, pred[i]];
                            var flatBonus = u[attr - 1, fake[i] ?? 0, pred[i]];
                            // normally we use a percentage `attr` and the flat `attr - 1`
                            // speed is only flat but we refer to it as `attr` so we need to move the effect to flat
                            if (attr == Attr.Speed)
                            {
                                flatBonus = percentageBonus;
                                percentageBonus = 0;
                            }
                            eff = monBase * (percentageBonus * 0.01) + flatBonus;

                            if (eff > maxEff)
                            {
                                maxEff = eff;
                            }
                        }
                        // cache best effect in slot
                        bestRuneEffect[i] = maxEff;
                    }

                    // filter runes in slot
                    for (int i = 0; i < 6; i++) {

                        // get the best runes from other slots
                        double otherRuneEffect = 0;
                        for (int j = 0; j < 6; j++)
                        {
                            if (i == j)
                                continue;
                            otherRuneEffect += bestRuneEffect[j];
                        }

                        // pick the runes which can't achieve the minimum
                        var insufficientBonus = Runes[i].Where(r =>
                            // monster base
                            monBase
                            // rune percentage bonus
                            + monBase * r[attr, fake[i] ?? 0, pred[i]] * 0.01f
                            // rune flat bonus
                            + r[attr - 1, fake[i] ?? 0, pred[i]] 
                            // other runes
                            + otherRuneEffect
                            // set effect
                            + monBase * bestSetEffect * 0.01f
                            // leader effect
                            + monBase * leaderEffect * 0.01f
                            // guild effect
                            + monBase * guildEffect * 0.01f
                            // tower effect
                            + monBase * shrineEffect * 0.01f
                            < Minimum[attr]
                        ).ToArray();

                        if (insufficientBonus.Length > 0)
                            removedOne = true;

                        Runes[i] = Runes[i].Except(insufficientBonus).ToArray();
                    }
                }

                // stats that are only a percentage
                foreach (var attr in new Attr[] {  Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy }) {

                    if (Minimum[attr].EqualTo(0))
                        continue;

                    var monBase = Mon[attr];
                    double bestSetEffect = BestSetEffect(attr, maxSets);
                    double guildEffect = Guild.ByStat(attr);
                    double shrineEffect = Shrines[attr];
                    double leaderEffect = Leader[attr];

                    // best rune for each slot
                    double[] bestRuneEffect = new double[] { 0, 0, 0, 0, 0, 0 };
                    for (int i = 0; i < 6; i++)
                    {
                        double maxEff = 0;
                        double eff = 0;
                        foreach (var u in Runes[i])
                        {
                            eff = u[attr, fake[i] ?? 0, pred[i]];
                            if (eff > maxEff)
                            {
                                maxEff = eff;
                            }
                        }
                        // cache best effect in slot
                        bestRuneEffect[i] = maxEff;
                    }

                    // filter runes in slot
                    for (int i = 0; i < 6; i++) {

                        // get the best runes from other slots
                        double otherRuneEffect = 0;
                        for (int j = 0; j < 6; j++)
                        {
                            if (i == j)
                                continue;
                            otherRuneEffect += bestRuneEffect[j];
                        }

                        var insufficientBonus = Runes[i].Where(r => 
                            // monster base
                            monBase
                            // flat bonus
                            + r[attr, fake[i] ?? 0, pred[i]]
                            // other rune bonus
                            + otherRuneEffect
                            // set bonuses
                            + bestSetEffect
                            // leader bonus
                            + leaderEffect
                            // guild bonus
                            + guildEffect
                            // shrine bonus
                            + shrineEffect
                            < Minimum[attr]
                        ).ToArray();

                        if (insufficientBonus.Length > 0)
                            removedOne = true;

                        Runes[i] = Runes[i].Except(insufficientBonus).ToArray();
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
            if (AllowBroken)
                return;

            var used = Runes[0].Concat(Runes[1]).Concat(Runes[2]).Concat(Runes[3]).Concat(Runes[4]).Concat(Runes[5]).Select(r => r.Set).Distinct();
            foreach (RuneSet s in used) {
                // find how many slots have acceptable runes for it
                int slots = 0;
                for (int i = 0; i < 6; i++) {
                    if (Runes[i].Any(r => r.Set == s))
                        slots += 1;
                }
                // if there isn't enough slots
                if (slots < s.Size()) {
                    // remove that set
                    for (int i = 0; i < 6; i++) {
                        Runes[i] = Runes[i].Where(r => r.Set != s).ToArray();
                    }
                }
            }
        }

    }

}
