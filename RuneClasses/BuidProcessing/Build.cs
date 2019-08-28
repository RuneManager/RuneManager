#define BUILD_PRECHECK_BUILDS
//#define BUILD_PRECHECK_BUILDS_DEBUG

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

namespace RuneOptim.BuidProcessing {


	// The heavy lifter
	// Contains most of the data needed to outline build requirements
	public class Build {
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

		[JsonIgnore]
		private readonly object bestLock = new object();

		// The runes to be used to generate builds
		[JsonIgnore]
		public Rune[][] runes { get; set; } = new Rune[6][];

		[JsonIgnore]
		public Craft[] grinds { get; set; }

		/// ----------------

		// How to sort the stats
		[JsonIgnore]
		public Func<Stats, int> sortFunc { get; set; }

		// if currently running
		[JsonIgnore]
		private bool isRunning = false;

		[JsonIgnore]
		private readonly object isRunLock = new object();

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

		public double ScoreStat(Stats current, Attr attr) {
			return ScoreStat(current, attr, out _);
		}

		public double ScoreExtra(Stats current, Attr attr) {
			return ScoreExtra(current, attr, out _);
		}

		public double ScoreSkill(Stats current, int j) {
			return ScoreSkill(current, j, out _);
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

		public double ScoreExtra(Stats current, Attr stat, out string str, Stats outvals = null) {
			str = "";
			if (current == null)
				return 0;
			double vv = current.ExtraValue(stat);
			double v2 = 0;

			str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
			if (Sort.ExtraGet(stat) != 0) {
				v2 = Threshold.ExtraGet(stat).EqualTo(0) ? vv : Math.Min(vv, Threshold.ExtraGet(stat));
				if (v2 > Goal.ExtraGet(stat) && Goal.ExtraGet(stat) > 0)
					v2 = (v2 - Goal.ExtraGet(stat)) / 2 + Goal.ExtraGet(stat);
				v2 /= Sort.ExtraGet(stat);
				if (outvals != null)
					outvals.ExtraSet(stat, v2);
				str = v2.ToString("0.#") + " (" + vv + ")";
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

		// build the scoring function
		public double CalcScore(Stats current, Stats outvals = null, Action<string, int> writeTo = null) {
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

		void getPrediction(int?[] slotFakes, bool[] slotPred) {
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

		public Monster GenBuild(params Rune[] runes) {
			if (runes.Length != 6)
				return null;

			// if to get awakened
			if (DownloadAwake && !Mon.downloaded) {
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
			else if (Mon.level < 40 && DownloadStats && !Mon.downloaded) {
				var mref = MonsterStat.FindMon(Mon);
				if (mref != null)
					Mon = mref.Download().GetMon(Mon);
			}

			int?[] slotFakes = new int?[6];
			bool[] slotPred = new bool[6];
			getPrediction(slotFakes, slotPred);

			Mon.ExtraCritRate = extraCritRate;
			Monster test = new Monster(Mon);
			test.Current.Shrines = Shrines;
			test.Current.Leader = Leader;

			test.Current.FakeLevel = slotFakes.Select(i => i ?? 0).ToArray();
			test.Current.PredictSubs = slotPred;

			test.ApplyRune(runes[0], 6);
			test.ApplyRune(runes[1], 6);
			test.ApplyRune(runes[2], 6);
			test.ApplyRune(runes[3], 6);
			test.ApplyRune(runes[4], 6);
			test.ApplyRune(runes[5], 6);


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

		public void Cancel() {
			IsRunning = false;
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
		public BuildResult GenBuilds(string prefix = "") {
			if (Type == BuildType.Lock) {
				Best = new Monster(Mon, true);
				return BuildResult.Success;
			}
			else if (Type == BuildType.Link) {
				if (LinkBuild == null) {
					for (int i = 0; i < 6; i++)
						runes[i] = new Rune[0];
					return BuildResult.Failure;
				}
				else {
					CopyFrom(LinkBuild);
				}
			}

			if (runes.Any(r => r == null)) {
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
				if (DownloadAwake && !Mon.downloaded) {
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
				else if (Mon.level < 40 && DownloadStats && !Mon.downloaded) {
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

			Thread timeThread = null;

			try {
				Best = null;
				Stats bstats = null;
				long count = 0;
				long actual = 0;
				long total = runes[0].Length;
				total *= runes[1].Length;
				total *= runes[2].Length;
				total *= runes[3].Length;
				total *= runes[4].Length;
				total *= runes[5].Length;
				long complete = total;

				Mon.ExtraCritRate = extraCritRate;
				Mon.GetStats();
				Mon.DamageFormula?.Invoke(Mon);

				int?[] slotFakesTemp = new int?[6];
				bool[] slotPred = new bool[6];
				getPrediction(slotFakesTemp, slotPred);

				int[] slotFakes = slotFakesTemp.Select(i => i ?? 0).ToArray();

				var currentLoad = new Monster(Mon, true);
				currentLoad.Current.TempLoad = true;
				currentLoad.Current.Buffs = Buffs;
				currentLoad.Current.Shrines = Shrines;
				currentLoad.Current.Leader = Leader;

				currentLoad.Current.FakeLevel = slotFakes;
				currentLoad.Current.PredictSubs = slotPred;

				double currentScore = CalcScore(currentLoad.GetStats(true));

				if (!Sort[Attr.Speed].EqualTo(0) && Sort[Attr.Speed] <= 1 // 1 SPD is too good to pass
					|| Mon.Current.Runes.Any(r => r == null)
					|| !Mon.Current.Runes.All(r => runes[r.Slot - 1].Contains(r)) // only IgnoreLess5 if I have my own runes
					|| Sort.NonZeroStats.HasCount(1)) // if there is only 1 sorting, must be too important to drop???
					IgnoreLess5 = false;

				BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, "cooking"));

				if (total == 0) {
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

				RuneLog.Debug(count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));

				Loads.Clear();

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
						RuneLog.Debug(count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));
						if (tests != null)
							BuildProgTo?.Invoke(this, ProgToEventArgs.GetEvent(this, (count + complete - total) / (double)complete, tests.Count));

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

				// Parallel the outer loop
				// TODO: setup the begin/finish Actions with syncList.
				Parallel.ForEach(runes[0], opts, (r0, loopState) => {
					var tempReq = RequiredSets.ToList();
					var tempMax = Maximum == null || !Maximum.IsNonZero ? null : new Stats(Maximum, true);
					int tempCheck = 0;

					Monster myBest = null;
					List<Monster> syncList = new List<Monster>();

					void syncMyList () {
						lock (bestLock) {
#if DEBUG_SYNC_BUILDS
							foreach (var s in syncList) {
								if (s.Current.Runes.All(r => r.Assigned == mon)) {
									Console.WriteLine("!");
								}
							}
#endif
							tests.AddRange(syncList);
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
								tests = tests.OrderByDescending(b => b.score).Take(75000).ToList();
							}

							if (tests.Count > MaxBuilds32)
								IsRunning = false;
						}
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
					test.Current.Leader = Leader;

					test.Current.FakeLevel = slotFakes;
					test.Current.PredictSubs = slotPred;
					test.ApplyRune(r0, 6);

					RuneSet set4 = r0.SetIs4 ? r0.Set : RuneSet.Null;
					RuneSet set2 = r0.SetIs4 ? RuneSet.Null : r0.Set;
					int pop4 = 0;
					int pop2 = 0;

					foreach (Rune r1 in runes[1]) {
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
									skip += runes[2].Length * runes[3].Length * runes[4].Length * runes[5].Length;
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
						test.ApplyRune(r1, 6);

						foreach (Rune r2 in runes[2]) {
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
										skip += runes[3].Length * runes[4].Length * runes[5].Length;
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
										skip += runes[3].Length * runes[4].Length * runes[5].Length;
										continue;
									}
								}
							}
#endif
							test.ApplyRune(r2, 6);

							foreach (Rune r3 in runes[3]) {
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
											skip += runes[4].Length * runes[5].Length;
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
											skip += runes[4].Length * runes[5].Length;
											continue;
										}
									}
								}
#endif
								test.ApplyRune(r3, 6);

								foreach (Rune r4 in runes[4]) {
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
												skip += runes[5].Length;
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
												skip += runes[5].Length;
												continue;
											}
										}
									}
#endif
									test.ApplyRune(r4, 6);

									foreach (Rune r5 in runes[5]) {
										if (!IsRunning_Unsafe)
											break;

										test.ApplyRune(r5, 6);
#if BUILD_PRECHECK_BUILDS_DEBUG
										outstrs.Add($"fine {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set} {r4.Set} {r5.Set}");
#endif
										isBad = false;

										cstats = test.GetStats();

										// check if build meets minimum
										isBad |= !RunesOnlyFillEmpty && !AllowBroken && !test.Current.SetsFull;

										isBad |= tempMax != null && cstats.AnyExceed(tempMax);

										if (!isBad && GrindLoads) {
											var mahGrinds = grinds.ToList();
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
											test.score = curScore;

											if (BuildSaveStats) {
												foreach (Rune r in test.Current.Runes) {
													if (!BuildGoodRunes) {
														r.manageStats.AddOrUpdate("LoadFilt", 1, (s, d) => { return d + 1; });
														RuneUsage.runesGood.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
														r.manageStats.AddOrUpdate("currentBuildPoints", curScore, (k, v) => Math.Max(v, curScore));
														r.manageStats.AddOrUpdate("cbp" + ID, curScore, (k, v) => Math.Max(v, curScore));
													}
													else {
														r.manageStats.AddOrUpdate("LoadFilt", 0.001, (s, d) => { return d + 0.001; });
														RuneUsage.runesOkay.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
														r.manageStats.AddOrUpdate("cbp" + ID, curScore, (k, v) => Math.Max(v, curScore * 0.9));
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
													myBest.score = myBestScore;
												}

												// if mine is better than what I last saw
												if (myBestScore > lastBest) {
													lock (bestLock) {
														if (Best == null) {
															Best = new Monster(myBest, true);
															bstats = Best.GetStats();
															bestScore = CalcScore(bstats);
															Best.score = bestScore;
														}
														else {
															// sync best score
															lastBest = bestScore;
															// double check
															if (myBestScore > lastBest) {
																Best = new Monster(myBest, true);
																bestScore = CalcScore(bstats);
																Best.score = bestScore;
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
													myBest.score = myBestScore;
													syncList.Add(myBest);
												}
												else if (BuildSaveStats) {
													// keep it for spreadsheeting
													syncList.Add(new Monster(test, true) {
														score = curScore
													});
												}
											}
										}
									}
									// sum up what work we've done
									Interlocked.Add(ref count, kill);
									Interlocked.Add(ref count, skip);
									Interlocked.Add(ref actual, kill);
									Interlocked.Add(ref BuildUsage.failed, kill);
									kill = 0;
									skip = 0;
									Interlocked.Add(ref count, plus);
									Interlocked.Add(ref actual, plus);
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
				});


				BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + "finalizing..."));
				BuildProgTo?.Invoke(this, ProgToEventArgs.GetEvent(this, 0.99, -1));

#if BUILD_PRECHECK_BUILDS_DEBUG
				System.IO.File.WriteAllLines("_into_the_bridge.txt", outstrs.ToArray());
#endif
				if (BuildSaveStats) {
					foreach (var ra in runes) {
						foreach (var r in ra) {
							if (!BuildGoodRunes) {
								r.manageStats.AddOrUpdate("buildScoreTotal", CalcScore(Best), (k, v) => v + CalcScore(Best));
								RuneUsage.runesUsed.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
								r.manageStats.AddOrUpdate("LoadGen", total, (s, d) => { return d + total; });

							}
							else {
								RuneUsage.runesBetter.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
								r.manageStats.AddOrUpdate("LoadGen", total * 0.001, (s, d) => { return d + total * 0.001; });
							}
						}
					}
				}

				// write out completion
				RuneLog.Debug(IsRunning + " " + count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));
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
					Best.score = CalcScore(Best.GetStats());
					BuildPrintTo?.Invoke(this, PrintToEventArgs.GetEvent(this, prefix + "best " + (Best?.score ?? -1)));
					Best.Current.ActualTests = actual;
					foreach (var bb in Loads) {
						foreach (Rune r in bb.Current.Runes) {
							double val = Best.score;
							if (BuildGoodRunes) {
								val *= 0.25;
								if (bb == Best)
									RuneUsage.runesSecond.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
							}

							if (bb != Best)
								val *= 0.1;
							else
								r.manageStats.AddOrUpdate("In", BuildGoodRunes ? 2 : 1, (s, e) => BuildGoodRunes ? 2 : 1);

							r.manageStats.AddOrUpdate("buildScoreIn", val, (k, v) => v + val);
						}
					}
					for (int i = 0; i < 6; i++) {
						if (!BuildGoodRunes && Mon.Current.Runes[i] != null && Mon.Current.Runes[i].Id != Best.Current.Runes[i].Id)
							Mon.Current.Runes[i].Swapped = true;
					}
					foreach (var ra in runes) {
						foreach (var r in ra) {
							var cbp = r.manageStats.GetOrAdd("currentBuildPoints", 0);
							if (cbp / Best.score < 1)
								r.manageStats.AddOrUpdate("bestBuildPercent", cbp / Best.score, (k, v) => Math.Max(v, cbp / Best.score));
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
				IsRunning = false;
				if (timeThread != null)
					timeThread.Join();
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

		/// <summary>
		/// Fills the instance with acceptable runes from save
		/// </summary>
		/// <param name="save">The Save data that contais the runes</param>
		/// <param name="useLocked">If it should include locked runes</param>
		/// <param name="useEquipped">If it should include equipped runes (other than the current monster)</param>
		/// <param name="saveStats">If to write information to the runes about usage</param>
		public void GenRunes(Save save) {
			if (save?.Runes == null)
				return;

			if (!getRunningHandle())
				return;
			try {

				if (Type == BuildType.Lock) {
					foreach (var r in Mon.Current.Runes) {
						if (r != null)
							runes[r.Slot - 1] = new Rune[] { r };
					}
					return;
				}

				if (Type == BuildType.Link && LinkBuild == null) {
					for (int i = 0; i < 6; i++)
						runes[i] = new Rune[0];
					return;
				}

				IEnumerable<Rune> rsGlobal = save.Runes;

				// if not saving stats, cull unusable here
				if (!BuildSaveStats) {
					// Only using 'inventory' or runes on mon
					// also, include runes which have been unequipped (should only look above)
					if (!RunesUseEquipped || RunesOnlyFillEmpty)
						rsGlobal = rsGlobal.Where(r => r.IsUnassigned || r.AssignedId == Mon.Id || r.Swapped);
					// only if the rune isn't currently locked for another purpose
					if (!RunesUseLocked)
						rsGlobal = rsGlobal.Where(r => !r.Locked);
					rsGlobal = rsGlobal.Where(r => !BannedRuneId.Any(b => b == r.Id) && !BannedRunesTemp.Any(b => b == r.Id));
				}

				if ((BuildSets.Any() || RequiredSets.Any()) && BuildSets.All(s => Rune.SetRequired(s) == 4) && RequiredSets.All(s => Rune.SetRequired(s) == 4)) {
					// if only include/req 4 sets, include all 2 sets autoRuneSelect && ()
					rsGlobal = rsGlobal.Where(r => BuildSets.Contains(r.Set) || RequiredSets.Contains(r.Set) || Rune.SetRequired(r.Set) == 2);
				}
				else if (BuildSets.Any() || RequiredSets.Any()) {
					rsGlobal = rsGlobal.Where(r => BuildSets.Contains(r.Set) || RequiredSets.Contains(r.Set));
					// Only runes which we've included
				}

				if (BuildSaveStats) {
					foreach (Rune r in rsGlobal) {
						r.manageStats.AddOrUpdate("currentBuildPoints", 0, (k, v) => 0);
						if (!BuildGoodRunes)
							r.manageStats.AddOrUpdate("Set", 1, (s, d) => { return d + 1; });
						else
							r.manageStats.AddOrUpdate("Set", 0.001, (s, d) => { return d + 0.001; });
					}
				}

				int?[] slotFakes = new int?[6];
				bool[] slotPred = new bool[6];
				getPrediction(slotFakes, slotPred);

				// Set up each runeslot
				for (int i = 0; i < 6; i++) {
					// put the right ones in
					runes[i] = rsGlobal.Where(r => r.Slot == i + 1).ToArray();

					// makes sure that the primary stat type is in the selection
					if (i % 2 == 1 && SlotStats[i].Count > 0) // actually evens because off by 1
					{
						runes[i] = runes[i].Where(r => SlotStats[i].Contains(r.Main.Type.ToForms())).ToArray();
					}

					if (BuildSaveStats) {
						foreach (Rune r in runes[i]) {
							if (!BuildGoodRunes)
								r.manageStats.AddOrUpdate("TypeFilt", 1, (s, d) => { return d + 1; });
							else
								r.manageStats.AddOrUpdate("TypeFilt", 0.001, (s, d) => { return d + 0.001; });
						}
						// cull here instead
						if (!RunesUseEquipped || RunesOnlyFillEmpty)
							runes[i] = runes[i].Where(r => r.IsUnassigned || r.AssignedId == Mon.Id || r.Swapped).ToArray();
						if (!RunesUseLocked)
							runes[i] = runes[i].Where(r => !r.Locked).ToArray();

					}
				}
				cleanBroken();

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
								rr = rr.Concat(runes[i].Where(r => r.Set == rs).OrderByDescending(r => runeVsStats(r, needRune) * 10 + runeVsStats(r, Sort)).Take(AutoRuneAmount / 2).ToArray()).ToArray();
							}
							if (rr.Length < AutoRuneAmount)
								rr = rr.Concat(runes[i].Where(r => !rr.Contains(r)).OrderByDescending(r => runeVsStats(r, needRune) * 10 + runeVsStats(r, Sort)).Take(AutoRuneAmount - rr.Length).ToArray()).Distinct().ToArray();

							runes[i] = rr;
						}

						cleanBroken();
					}
				}
				if (!AutoRuneSelect) {
					// TODO: Remove
#if BUILD_RUNE_LOGGING
					//var tmp = RuneLog.logTo;
					//using (var fs = new System.IO.FileStream("sampleselect.log", System.IO.FileMode.Create))
					//using (var sw = new System.IO.StreamWriter(fs)) {
						RuneLog.logTo = sw;
#else
					{
#endif
						// Filter each runeslot
						for (int i = 0; i < 6; i++) {
							// default fail OR
							Predicate<Rune> slotTest = MakeRuneScoring(i + 1, slotFakes[i] ?? 0, slotPred[i]);

							runes[i] = runes[i].Where(r => slotTest.Invoke(r)).OrderByDescending(r => r.manageStats.GetOrAdd("testScore", 0)).ToArray();
							if (LoadFilters(i + 1, out double? n) == FilterType.SumN) {
								
								var rr = runes[i].Where(r => RequiredSets.Contains(r.Set)).GroupBy(r => r.Set).SelectMany(r => r.Take(Math.Max(2, (int)((n ?? 30) * 0.25))));
								runes[i] = rr.Concat(runes[i].Where(r => !RequiredSets.Contains(r.Set)).Take((int)(n ?? 30) - rr.Count())).ToArray();

								// TODO: pick 20% per required set
								// Then fill remaining with the best from included
								// Go around checking if there are enough runes from each set to complete it (if NonBroken)
								// Check if removing N other runes of SCORE will permit finishing set
								// Remove rune add next best in slot
							}

							if (BuildSaveStats) {
								foreach (Rune r in runes[i]) {
									if (!BuildGoodRunes)
										r.manageStats.AddOrUpdate("RuneFilt", 1, (s, d) => d + 1);
									else
										r.manageStats.AddOrUpdate("RuneFilt", 0.001, (s, d) => d + 0.001);
								}
							}
						}
					}
				}
				if (RunesDropHalfSetStat) {
					for (int i = 0; i < 6; i++) {
						double rmm = 0;
						var runesForSlot = runes[i];
						var outRunes = new List<Rune>();
						var runesBySet = runesForSlot.GroupBy(r => r.Set);
						foreach (var rsg in runesBySet) {
							var runesByMain = rsg.GroupBy(r => r.Main.Type);
							foreach (var rmg in runesByMain) {
								rmm = rmg.Max(r => r.manageStats.GetOrAdd("testScore", 0)) * 0.6;
								if (rmm > 0) {
									outRunes.AddRange(rmg.Where(r => r.manageStats.GetOrAdd("testScore", 0) > rmm));
								}
							}
						}
						if (rmm > 0)
							runes[i] = outRunes.ToArray();
					}
				}
				// if we are only to fill empty slots
				if (RunesOnlyFillEmpty) {
					for (int i = 0; i < 6; i++) {
						if (Mon.Current.Runes[i] != null && (!Mon.Current.Runes[i]?.Locked ?? false)) {
							runes[i] = new Rune[0];
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
					if (!runes[i].Contains(r) && !r.Locked && isGoodType) {
						var tl = runes[i].ToList();
						tl.Add(r);
						runes[i] = tl.ToArray();
					}
				}

				grinds = runes.SelectMany(rg => rg.SelectMany(r => r.FilterGrinds(save.Crafts).Concat(r.FilterEnchants(save.Crafts)))).Distinct().ToArray();
			}
			finally {
				IsRunning = false;
			}
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
