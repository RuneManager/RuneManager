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


	// The heavy lifter
	// Contains most of the data needed to outline build requirements
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

		[JsonIgnore]
		public IBuildRunner runner;

		[JsonIgnore]
		private TaskCompletionSource<IBuildRunner> tcs = new TaskCompletionSource<IBuildRunner>();

		[JsonIgnore]
		public Task<IBuildRunner> startedBuild => tcs.Task;

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
		// Magical (and probably bad) tree structure for rune slot stat filters
		// tab, stat, FILTER
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

		// For when you want to map 2 pieces of info to a key, just be *really* lazy
		// Contains the scoring type (OR, AND, SUM) and the[(>= SUM] value
		// tab, TYPE, test
		[JsonProperty("runeScoring")]
		[JsonConverter(typeof(DictionaryWithSpecialEnumKeyConverter))]
		public Dictionary<SlotIndex, KeyValuePair<FilterType, double?>> RuneScoring { get; set; } = new Dictionary<SlotIndex, KeyValuePair<FilterType, double?>>();

		public bool ShouldSerializeRuneScoring() {
			Dictionary<SlotIndex, KeyValuePair<FilterType, double?>> nscore = new Dictionary<SlotIndex, KeyValuePair<FilterType, double?>>();
			foreach (var tabPair in RuneScoring) {
				if (tabPair.Value.Key != 0 || tabPair.Value.Value != null)
					nscore.Add(tabPair.Key, new KeyValuePair<FilterType, double?>(tabPair.Value.Key, tabPair.Value.Value));
			}
			RuneScoring = nscore;

			return RuneScoring.Count > 0;
		}

		// if to raise the runes level, and use the appropriate main stat value.
		// also, attempt to give weight to unassigned powerup bonuses
		// tab, RAISE, magic
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

		[JsonProperty("AllowBroken")]
		public bool AllowBroken { get; set; } = false;

		// how much each stat is worth (0 = useless)
		// eg. 300 hp is worth 1 speed
		[JsonProperty("Sort")]
		public Stats Sort { get; set; } = new Stats();

		public bool ShouldSerializeSort() {
			return Sort.IsNonZero;
		}

		// resulting build must have every set in this collection
		[JsonProperty("RequiredSets")]
		public ObservableCollection<RuneSet> RequiredSets { get; set; } = new ObservableCollection<RuneSet>();

		public bool ShouldSerializeRequiredSets() {
			return RequiredSets.Count > 0;
		}

		// builds *must* have *all* of these stats
		[JsonProperty("Minimum")]
		public Stats Minimum { get; set; } = new Stats();

		// builds *mustn't* exceed *any* of these stats
		[JsonProperty("Maximum")]
		public Stats Maximum { get; set; } = new Stats();

		// individual stats exceeding these values are capped
		[JsonProperty("Threshold")]
		public Stats Threshold { get; set; } = new Stats();

		// builds with individual stats below these values are penalised as not good enough
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

		// Which primary stat types are allowed per slot (should be 2,4,6 only)
		[JsonProperty("slotStats")]
		public List<string>[] SlotStats { get; set; } = new List<string>[6];

		// Sets to consider using
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

		// magically find good runes to use in the build
		[JsonProperty("autoRuneSelect")]
		public bool AutoRuneSelect { get; set; } = false;

		public bool ShouldSerializeAutoRuneSelect() {
			return AutoRuneSelect;
		}

		// magically scale Minimum with Sort while the build is running
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

		/// ---------------

		// These should be generated at runtime, do not store externally

		// The best loadouts
		[JsonIgnore]
		public readonly ObservableCollection<Monster> Loads = new ObservableCollection<Monster>();

		// The best loadout in loads
		[JsonIgnore]
		public Monster Best { get; set; } = null;

		// The runes to be used to generate builds
		[JsonIgnore]
		public Rune[][] runes { get; set; } = new Rune[6][];

		[JsonIgnore]
		public Craft[] grinds { get; set; }

		/// ----------------

		// How to sort the stats
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

		// Seems to out-of-mem if too many
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
				RuneScoring[kv.Key] = new KeyValuePair<FilterType, double?>(kv.Value.Key, kv.Value.Value);
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

		public void BanEmTemp(params ulong[] brunes) {
			BannedRunesTemp.Clear();
			foreach (var r in brunes) {
				BannedRunesTemp.Add(r);
			}
		}


		public double ScoreStat(Stats current, Attr stat) {
			if (current == null)
				return 0;
			double vv = current[stat];

			if (vv > 100 && (stat == Attr.Accuracy || stat == Attr.CritRate || stat == Attr.Resistance))
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


		public double ScoreStat(Stats current, Attr stat, Stats outvals) {
			if (current == null)
				return 0;
			double vv = current[stat];
			double v2 = 0;

			if (vv > 100 && (stat == Attr.Accuracy || stat == Attr.CritRate || stat == Attr.Resistance))
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

		public double ScoreStat(Stats current, Attr stat, out string str, Stats outvals = null) {
			str = "";
			if (current == null)
				return 0;
			double vv = current[stat];
			double v2 = 0;

			if (vv > 100 && (stat == Attr.Accuracy || stat == Attr.CritRate || stat == Attr.Resistance))
				vv = 100;
			str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
			if (Sort[stat] != 0) {
				v2 = Threshold[stat].EqualTo(0) ? vv : Math.Min(vv, Threshold[stat]);
				if (v2 > Goal[stat] && Goal[stat] > 0)
					v2 = (v2 - Goal[stat]) / 2 + Goal[stat];
				v2 /= Sort[stat];
				if (outvals != null)
					outvals[stat] = v2;
				str = v2.ToString("0.#") + " (" + current[stat] + ")";
			}

			return v2;
		}


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

		public double LastScore(Stats current ) {
			if (lastScore == float.MinValue)
				CalcScore(current);
			return lastScore;
		}

		public double LastScore(Stats current, Stats outvals) {
			if (lastScore == float.MinValue)
				CalcScore(current, outvals);
			return lastScore;
		}

		// build the scoring function
		public double CalcScore(Stats current, Action<string, int> writeTo) {
			if (current == null)
				return 0;

			string str;
			double pts = 0;
			// dodgy hack for indexing in Generate ListView

			// TODO: instead of -Goal, make everything >Goal /2
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

		// build the scoring function
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

		// build the scoring function
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

		public void ToggleIncludedSet(RuneSet set) {
			if (BuildSets.Contains(set)) {
				BuildSets.Remove(set);
			}
			else {
				RemoveRequiredSet(set);
				BuildSets.Add(set);
			}
		}

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

		public void AddIncludedSet(RuneSet set) {
			if (RequiredSets.Any(s => s == set) || BuildSets.Any(s => s == set))
				return;
			BuildSets.Add(set);
		}

		public int RemoveRequiredSet(RuneSet set) {
			int num = 0;
			while (RequiredSets.Any(s => s == set)) {
				RequiredSets.Remove(set);
				num++;
			}
			return num;
		}

		public void getPrediction(int?[] slotFakes, bool[] slotPred) {
			// crank the rune prediction
			for (int i = 0; i < 6; i++) {
				int? raiseTo = 0;
				bool predictSubs = false;

				// find the largest number to raise to
				// if any along the tree say to predict, do it
				if (RunePrediction.ContainsKey(SlotIndex.Global)) {
					int? glevel = RunePrediction[SlotIndex.Global].Key;
					if (glevel > raiseTo)
						raiseTo = glevel;
					predictSubs |= RunePrediction[SlotIndex.Global].Value;
				}
				if (RunePrediction.ContainsKey(i % 2 == 0 ? SlotIndex.Odd : SlotIndex.Even)) {
					int? mlevel = RunePrediction[i % 2 == 0 ? SlotIndex.Odd : SlotIndex.Even].Key;
					if (mlevel > raiseTo)
						raiseTo = mlevel;
					predictSubs |= RunePrediction[i % 2 == 0 ? SlotIndex.Odd : SlotIndex.Even].Value;
				}
				if (RunePrediction.ContainsKey((SlotIndex)(i + 1))) {
					int? slevel = RunePrediction[(SlotIndex)(i + 1)].Key;
					if (slevel > raiseTo)
						raiseTo = slevel;
					predictSubs |= RunePrediction[(SlotIndex)(i + 1)].Value;
				}

				slotFakes[i] = raiseTo;
				slotPred[i] = predictSubs;
			}
		}

		//Dictionary<string, RuneFilter> rfS, Dictionary<string, RuneFilter> rfM, Dictionary<string, RuneFilter> rfG, 
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

		public double ScoreRune(Rune r, int raiseTo = 0, bool predictSubs = false) {
			FilterType and = LoadFilters(r.Slot, out _);
			if (RunFilters(r.Slot, out Stats rFlat, out Stats rPerc, out Stats rTest)) {
				switch (and) {
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

		public FilterType LoadFilters(int slot, out double? testVal) {
			// which tab we pulled the filter from
			testVal = null;
			FilterType and = 0;

			// TODO: check what inheriting SUM (eg. Odd and 3) does
			// TODO: check what inheriting AND/OR then SUM (or visa versa)

			// find the most significant operatand of joining checks
			if (RuneScoring.ContainsKey(SlotIndex.Global)) {
				var kv = RuneScoring[SlotIndex.Global];
				if (kv.Key != FilterType.None) {
					if (RuneFilters.ContainsKey(SlotIndex.Global) || kv.Key == FilterType.SumN)
						and = kv.Key;
					if (kv.Value != null) {
						testVal = kv.Value;
					}
				}
			}
			// is it and odd or even slot?
			var tmk = slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd;
			if (RuneScoring.ContainsKey(tmk)) {
				var kv = RuneScoring[tmk];
				if (kv.Key != FilterType.None) {
					if (RuneFilters.ContainsKey(tmk) || kv.Key == FilterType.SumN)
						and = kv.Key;
					if (kv.Value != null) {
						testVal = kv.Value;
					}
				}
			}
			// turn the 0-5 to a 1-6
			tmk = (SlotIndex)slot;
			if (RuneScoring.ContainsKey(tmk)) {
				var kv = RuneScoring[tmk];
				if (kv.Key != FilterType.None) {
					if (RuneFilters.ContainsKey(tmk) || kv.Key == FilterType.SumN)
						and = kv.Key;
					if (kv.Value != null) {
						testVal = kv.Value;
					}
				}
			}

			return and;
		}

		public Predicate<Rune> MakeRuneScoring(int slot, int raiseTo = 0, bool predictSubs = false) {
			// the value to test SUM against
			FilterType and = LoadFilters(slot, out double? testVal);

			// this means that runes won't get in unless they meet at least 1 criteria
			// if an operand was found, ensure the tab contains filter data
			// if there where no filters with data
			if (!RunFilters(slot, out Stats rFlat, out Stats rPerc, out Stats rTest))
				return r => true;

			// no filter data = use all
			// Set the test based on the type found
			switch (and) {
				case FilterType.Or:
					return r => r.Or(rFlat, rPerc, rTest, raiseTo, predictSubs);
				case FilterType.And:
					return r => r.And(rFlat, rPerc, rTest, raiseTo, predictSubs);
				case FilterType.Sum:
					return r => {
						if (!System.Diagnostics.Debugger.IsAttached)
							RuneLog.Debug("[" + slot + "] Checking " + r.Id + " {");
						var vv = r.Test(rFlat, rPerc, raiseTo, predictSubs);
						if (!System.Diagnostics.Debugger.IsAttached)
							RuneLog.Debug("\t\t" + vv + " against " + testVal + "}" + (vv >= testVal));
						return vv >= testVal;
					};
				case FilterType.SumN:
					return r => {
						if (!System.Diagnostics.Debugger.IsAttached)
							RuneLog.Debug("[" + slot + "] Checking " + r.Id + " {");
						var vv = r.Test(rFlat, rPerc, raiseTo, predictSubs);
						if (!System.Diagnostics.Debugger.IsAttached)
							RuneLog.Debug("\t\t" + vv + " against " + testVal + "}" + (vv >= testVal));
						return true;
					};
			}
			return r => false;
		}

		// Try to determine the subs required to meet the minimum. Will guess Evens by: Slot, Health%, Attack%, Defense%
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

		// Make sure that for each set type, there are enough slots with runes in them
		// Eg. if only 1,4,5 have Violent, remove all violent runes because you need 4
		// for each included set
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

		// assumes Stat A/D/H are percent
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

		// assumes Stat A/D/H are percent
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
