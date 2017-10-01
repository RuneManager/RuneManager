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

namespace RuneOptim {
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

	public class PrintToEventArgs : EventArgs
	{
		public Build build;
		public string Message;
		public PrintToEventArgs(Build b, string m) { build = b;  Message = m; }
	}

	public class ProgToEventArgs : EventArgs
	{
		public Build build;
		public int Progress; 
		public double Percent;
		public ProgToEventArgs(Build b, double d, int p) { build = b; Percent = d; Progress = p; }
	}

	public class BuildUsage
	{
		public int failed = 0;
		public int passed = 0;
		public List<Monster> loads;
	}

	public enum BuildResult
	{
		Success = 0,
		Failure = 1,
		NoPermutations = 2,
		NoSorting = 3,
		BadRune = 4,
	}

	public enum FilterType {
		None = -1,
		Or = 0,
		And = 1,
		Sum = 2,
		SumN = 3
	}

	// The heavy lifter
	// Contains most of the data needed to outline build requirements
	public class Build
	{
		// allows iterative code, probably slow but nice to write and integrates with WinForms at a moderate speed
		[Obsolete("Consider changing to statEnums")]
		public static readonly string[] statNames = { "HP", "ATK", "DEF", "SPD", "CR", "CD", "RES", "ACC" };
		public static readonly Attr[] statEnums = { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy };
		public static readonly Attr[] statBoth = { Attr.HealthFlat, Attr.HealthPercent, Attr.AttackFlat, Attr.AttackPercent, Attr.DefenseFlat, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy };
		[Obsolete("Consider changing to extraEnums")]
		public static readonly string[] extraNames = { "EHP", "EHPDB", "DPS", "AvD", "MxD" };
		public static readonly Attr[] extraEnums = { Attr.EffectiveHP, Attr.EffectiveHPDefenseBreak, Attr.DamagePerSpeed, Attr.AverageDamage, Attr.MaxDamage };
		public static readonly Attr[] statAll = { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy, Attr.EffectiveHP, Attr.EffectiveHPDefenseBreak, Attr.DamagePerSpeed, Attr.AverageDamage, Attr.MaxDamage };

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
				if (s != RuneSet.Null && RuneProperties.MagicalSets.Contains((s)))
				{
					RequiredSets.Add(s);
				}
			}
		}
		
		public override string ToString()
		{
			return ID + " " + MonName;
		}
		
		[JsonProperty("id")]
		public int ID = 0;

		[JsonProperty("version")]
		public int VERSIONNUM;

		[JsonProperty("MonName")]
		public string MonName;

		[JsonProperty("MonId")]
		public ulong MonId;

		[JsonProperty("priority")]
		public int priority;

		[JsonIgnore]
		public Monster mon;

		[JsonIgnore]
		public int BuildGenerate = 0;

		[JsonIgnore]
		public int BuildTake = 0;

		[JsonIgnore]
		public int BuildTimeout = 0;

		[JsonIgnore]
		public bool BuildDumpBads = false;

		[JsonIgnore]
		public bool BuildSaveStats = false;

		[JsonIgnore]
		public bool BuildGoodRunes = false;

		[JsonIgnore]
		public bool RunesUseLocked = false;

		[JsonIgnore]
		public bool RunesUseEquipped = false;

		public event EventHandler<PrintToEventArgs> BuildPrintTo;

		public event EventHandler<ProgToEventArgs> BuildProgTo;

		[JsonProperty("new")]
		public bool New;

		public bool ShouldSerializeNew()
		{
			return New;
		}

		[JsonProperty("downloadstats")]
		public bool DownloadStats;

		public bool ShouldSerializeDownloadStats()
		{
			return DownloadStats;
		}

		[JsonProperty("downloadawake")]
		public bool DownloadAwake;

		public bool ShouldSerializeDownloadAwake()
		{
			return DownloadAwake;
		}

		[JsonProperty("extra_crit_rate")]
		private int extraCritRate = 0;

		[JsonIgnore]
		public int ExtraCritRate {
			get {
				if (mon != null)
					mon.ExtraCritRate = extraCritRate;
				return extraCritRate;
			}
			set {
				extraCritRate = value;
				if (mon != null)
					mon.ExtraCritRate = value;
			}
		}

		public bool ShouldSerializeextraCritRate() {
			return extraCritRate > 0;
		}
		// Magical (and probably bad) tree structure for rune slot stat filters
		// tab, stat, FILTER
		[JsonProperty("runeFilters")]
		[JsonConverter(typeof(DictionaryWithSpecialEnumKeyConverter))]
		public Dictionary<SlotIndex, Dictionary<string, RuneFilter>> runeFilters = new Dictionary<SlotIndex, Dictionary<string, RuneFilter>>();

		public bool ShouldSerializeruneFilters()
		{
			Dictionary<SlotIndex, Dictionary<string, RuneFilter>> nfilters = new Dictionary<SlotIndex, Dictionary<string, RuneFilter>>();
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

			return runeFilters.Count > 0;
		}

		// For when you want to map 2 pieces of info to a key, just be *really* lazy
		// Contains the scoring type (OR, AND, SUM) and the[(>= SUM] value
		// tab, TYPE, test
		[JsonProperty("runeScoring")]
		[JsonConverter(typeof(DictionaryWithSpecialEnumKeyConverter))]
		public Dictionary<SlotIndex, KeyValuePair<FilterType, double?>> runeScoring = new Dictionary<SlotIndex, KeyValuePair<FilterType, double?>>();

		public bool ShouldSerializeruneScoring()
		{
			Dictionary<SlotIndex, KeyValuePair<FilterType, double?>> nscore = new Dictionary<SlotIndex, KeyValuePair<FilterType, double?>>();
			foreach (var tabPair in runeScoring)
			{
				if (tabPair.Value.Key != 0 || tabPair.Value.Value != null)
					nscore.Add(tabPair.Key, new KeyValuePair<FilterType, double?>(tabPair.Value.Key, tabPair.Value.Value));
			}
			runeScoring = nscore;

			return runeScoring.Count > 0;
		}

		// if to raise the runes level, and use the appropriate main stat value.
		// also, attempt to give weight to unassigned powerup bonuses
		// tab, RAISE, magic
		[JsonProperty("runePrediction")]
		[JsonConverter(typeof(DictionaryWithSpecialEnumKeyConverter))]
		public Dictionary<SlotIndex, KeyValuePair<int?, bool>> runePrediction = new Dictionary<SlotIndex, KeyValuePair<int?, bool>>();

		public bool ShouldSerializerunePrediction()
		{
			Dictionary<SlotIndex, KeyValuePair<int?, bool>> npred = new Dictionary<SlotIndex, KeyValuePair<int?, bool>>();
			foreach (var tabPair in runePrediction)
			{
				if (tabPair.Value.Key != 0 || tabPair.Value.Value)
					npred.Add(tabPair.Key, new KeyValuePair<int?, bool>(tabPair.Value.Key, tabPair.Value.Value));
			}
			runePrediction = npred;

			return runePrediction.Count > 0;
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

		// resulting build must have every set in this collection
		[JsonProperty("RequiredSets")]
		public ObservableCollection<RuneSet> RequiredSets = new ObservableCollection<RuneSet>();

		public bool ShouldSerializeRequiredSets()
		{
			return RequiredSets.Count > 0;
		}

		// builds *must* have *all* of these stats
		[JsonProperty("Minimum")]
		public Stats Minimum = new Stats();
		
		// builds *mustn't* exceed *any* of these stats
		[JsonProperty("Maximum")]
		public Stats Maximum = new Stats();

		// individual stats exceeding these values are capped
		[JsonProperty("Threshold")]
		public Stats Threshold = new Stats();

		// builds with individual stats below these values are penalised as not good enough
		[JsonProperty("Goal")]
		public Stats Goal = new Stats();

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

		public bool ShouldSerializeGoal()
		{
			return Goal.NonZero();
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
		public ObservableCollection<RuneSet> BuildSets = new ObservableCollection<RuneSet>();

		[JsonIgnore]
		public BuildUsage buildUsage;

		[JsonIgnore]
		public RuneUsage runeUsage;

		[JsonIgnore]
		public static readonly int AutoRuneAmountDefault = 30;

		public int autoRuneAmount = AutoRuneAmountDefault;

		public bool ShouldSerializeautoRuneAmount()
		{
			return autoRuneAmount != AutoRuneAmountDefault;
		}

		// magically find good runes to use in the build
		public bool autoRuneSelect = false;

		public bool ShouldSerializeautoRuneSelect()
		{
			return autoRuneSelect;
		}

		// magically scale Minimum with Sort while the build is running
		public bool autoAdjust = false;

		public bool ShouldSerializeautoAdjust()
		{
			return autoAdjust;
		}

		// Save to JSON
		public List<ulong> BannedRuneId = new List<ulong>();

		public bool ShouldSerializeBannedRuneId()
		{
			return BannedRuneId.Any();
		}

		[JsonIgnore]
		public List<ulong> bannedRunesTemp = new List<ulong>();

		/// ---------------

		// These should be generated at runtime, do not store externally

		// The best loadouts
		[JsonIgnore]
		public readonly ObservableCollection<Monster> loads = new ObservableCollection<Monster>();

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
		private bool isRunning = false;

		[JsonIgnore]
		private object isRunLock = new object();
		
		[JsonIgnore]
		public bool IsRunning
		{
			get
			{
				lock (isRunLock)
				{
					return isRunning;
				}
			}
			private set
			{
				lock (isRunLock)
				{
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

		private bool GetRunningHandle()
		{
			lock (isRunLock)
			{
				if (isRunning)
					return false;
				else
				{
					isRunning = true;
					return true;
				}
			}
		}

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
		
		public void BanEmTemp(params ulong[] brunes)
		{
			bannedRunesTemp.Clear();
			foreach (var r in brunes)
			{
				bannedRunesTemp.Add(r);
			}
		}

		// build the scoring function
		public double CalcScore(Stats current, Stats outvals = null, Action<string, int> writeTo = null) {
			if (current == null)
				return 0;
			//if (outvals == null)
			//	outvals = new Stats();
			if (outvals != null)
				outvals.SetTo(0);

			string str;
			double vv, v2, pts = 0;
			// dodgy hack for indexing in Generate ListView
			int i = 2;
			foreach (Attr stat in Build.statEnums) {
				vv = current[stat];
				str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
				if (Sort[stat] != 0) {
					v2 = ((Threshold[stat].EqualTo(0) ? vv : Math.Min(vv, Threshold[stat])) - Goal[stat]) / Sort[stat];
					if (outvals != null)
						outvals[stat] = v2;
					if (writeTo != null)
						str = v2.ToString("0.#") + " (" + current[stat] + ")";
					pts += v2;
				}
				writeTo?.Invoke(str, i);
				i++;
			}

			foreach (Attr stat in Build.extraEnums) {
				vv = current.ExtraValue(stat);
				str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
				if (Sort.ExtraGet(stat) != 0) {
					v2 = ((Threshold.ExtraGet(stat).EqualTo(0) ? vv : Math.Min(vv, Threshold.ExtraGet(stat))) - Goal.ExtraGet(stat)) / Sort.ExtraGet(stat);
					if (outvals != null)
						outvals.ExtraSet(stat, v2);
					if (writeTo != null)
						str = v2.ToString("0.#") + " (" + vv + ")";
					pts += v2;
				}
				writeTo?.Invoke(str, i);
				i++;
			}

			for (int j = 0; j < 4; j++) {
				if (current.SkillFunc[j] != null) {
					vv = current.GetSkillDamage(Attr.AverageDamage, j); //current.SkillFunc[j](current);
					str = vv.ToString(System.Globalization.CultureInfo.CurrentUICulture);
					if (Sort.DamageSkillups[j] != 0) {
						v2 = ((Threshold.DamageSkillups[j].EqualTo(0) ? vv : Math.Min(vv, Threshold.DamageSkillups[j])) - Goal.DamageSkillups[j]) / Sort.DamageSkillups[j];
						if (outvals != null)
							outvals.DamageSkillups[j] = v2;
						if (writeTo != null)
							str = v2.ToString("0.#") + " (" + vv + ")";
						pts += v2;
					}

					writeTo?.Invoke(str, i);
					i++;
				}
			}
			return pts;
		}

		public void toggleIncludedSet(RuneSet set)
		{
			if (BuildSets.Contains(set))
			{
				BuildSets.Remove(set);
			}
			else
			{
				removeRequiredSet(set);
				BuildSets.Add(set);
			}
		}

		public int addRequiredSet(RuneSet set)
		{
			if (BuildSets.Contains(set))
			{
				BuildSets.Remove(set);
			}
			RequiredSets.Add(set);
			if (RequiredSets.Count(s => s == set) > 3)
			{
				while (RequiredSets.Count(s => s == set) > 1)
					RequiredSets.Remove(set);
			}
			return RequiredSets.Count(s => s == set);
		}

		public void addIncludedSet(RuneSet set)
		{
			if (RequiredSets.Any(s => s == set) || BuildSets.Any(s => s == set))
				return;
			BuildSets.Add(set);
		}

		public int removeRequiredSet(RuneSet set)
		{
			int num = 0;
			while (RequiredSets.Any(s => s == set))
			{
				RequiredSets.Remove(set);
				num++;
			}
			return num;
		}

		void GetPrediction(int?[] slotFakes, bool[] slotPred)
		{
			// crank the rune prediction
			for (int i = 0; i < 6; i++)
			{
				int? raiseTo = 0;
				bool predictSubs = false;

				// find the largest number to raise to
				// if any along the tree say to predict, do it
				if (runePrediction.ContainsKey(SlotIndex.Global))
				{
					int? glevel = runePrediction[SlotIndex.Global].Key;
					if (glevel > raiseTo)
						raiseTo = glevel;
					predictSubs |= runePrediction[SlotIndex.Global].Value;
				}
				if (runePrediction.ContainsKey(((i % 2 == 0) ? SlotIndex.Odd : SlotIndex.Even)))
				{
					int? mlevel = runePrediction[((i % 2 == 0) ? SlotIndex.Odd : SlotIndex.Even)].Key;
					if (mlevel > raiseTo)
						raiseTo = mlevel;
					predictSubs |= runePrediction[((i % 2 == 0) ? SlotIndex.Odd : SlotIndex.Even)].Value;
				}
				if (runePrediction.ContainsKey((SlotIndex)(i + 1)))
				{
					int? slevel = runePrediction[(SlotIndex)(i + 1)].Key;
					if (slevel > raiseTo)
						raiseTo = slevel;
					predictSubs |= runePrediction[(SlotIndex)(i + 1)].Value;
				}

				slotFakes[i] = raiseTo;
				slotPred[i] = predictSubs;
			}
		}

		public Monster GenBuild(params Rune[] runes)
		{
			if (runes.Length != 6)
				return null;

			// if to get awakened
			if (DownloadAwake && !mon.downloaded)
			{
				var mref = MonsterStat.FindMon(mon);
				if (mref != null)
				{
					// download the current (unawakened monster)
					var mstat = mref.Download();
					// if the retrieved mon is unawakened, get the awakened
					if (!mstat.Awakened && mstat.AwakenTo != null)
						mon = mstat.AwakenTo.Download().GetMon(mon);
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

			int?[] slotFakes = new int?[6];
			bool[] slotPred = new bool[6];
			GetPrediction(slotFakes, slotPred);

			mon.ExtraCritRate = extraCritRate;
			Monster test = new Monster(mon);
			test.Current.Shrines = shrines;
			test.Current.Leader = leader;

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
			//if (test.Current.Runes.All(r => mon.Current.Runes.Contains(r)))
			//	isBad = false;

			var cstats = test.GetStats();

			// check if build meets minimum
			isBad |= (Minimum != null && !(cstats > Minimum));
			// if no broken sets, check for broken sets
			isBad |= (!AllowBroken && !test.Current.SetsFull);
			// if there are required sets, ensure we have them
			isBad |= (RequiredSets != null && RequiredSets.Count > 0
				// this Linq adds no overhead compared to GetStats() and ApplyRune()
				&& !RequiredSets.All(s => test.Current.Sets.Count(q => q == s) >= RequiredSets.Count(q => q == s)));

			if (isBad)
				return null;

			return test;
		}

		public void Cancel()
		{
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
		public BuildResult GenBuilds(string prefix = "")
		{
			if (runes.Any(r => r == null))
			{
				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Null rune"));
				return BuildResult.BadRune;
			}
			if (!BuildSaveStats)
				BuildGoodRunes = false;

			if (!BuildGoodRunes)
			{
				runeUsage = new RuneUsage();
				buildUsage = new BuildUsage();
			}
			try
			{
				// if to get awakened
				if (DownloadAwake && !mon.downloaded)
				{
					BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Downloading Awake def"));
					var mref = MonsterStat.FindMon(mon);
					if (mref != null)
					{
						// download the current (unawakened monster)
						var mstat = mref.Download();
						BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Reading stats"));
						// if the retrieved mon is unawakened, get the awakened
						if (!mstat.Awakened && mstat.AwakenTo != null)
						{
							BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Awakening"));
							mon = mstat.AwakenTo.Download().GetMon(mon);
						}
					}
					BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Downloaded"));
				}
				// getting awakened also gets level 40, so...
				// only get lvl 40 stats if the monster isn't 40, wants to download AND isn't already downloaded (first and last are about the same)
				else if (mon.level < 40 && DownloadStats && !mon.downloaded)
				{
					BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Downloading 40 def"));
					var mref = MonsterStat.FindMon(mon);
					if (mref != null)
						mon = mref.Download().GetMon(mon);
				}
			}
			catch (Exception e)
			{
				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Failed downloading def: " + e.Message + Environment.NewLine + e.StackTrace));
			}

			if (!GetRunningHandle())
				throw new InvalidOperationException("The build is locked with another action.");

			try
			{
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

				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "cooking"));

				if (total == 0)
				{
					BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "0 perms"));
					RuneLog.Info("Zero permuations");
					return BuildResult.NoPermutations;
				}

				bool hasSort = Sort.NonZero();
				if (BuildTake == 0 && !hasSort)
				{
					BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "No sort"));
					RuneLog.Info("No method of determining best");
					return BuildResult.NoSorting;
				}

				DateTime begin = DateTime.Now;
				DateTime timer = DateTime.Now;

				RuneLog.Debug(count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));

				int?[] slotFakesTemp = new int?[6];
				bool[] slotPred = new bool[6];
				GetPrediction(slotFakesTemp, slotPred);

				int[] slotFakes = slotFakesTemp.Select(i => i ?? 0).ToArray();

				loads.Clear();
				mon.ExtraCritRate = extraCritRate;
				mon.GetStats();
				mon.DamageFormula?.Invoke(mon);

				// set to running
				IsRunning = true;

#if BUILD_PRECHECK_BUILDS_DEBUG
				SynchronizedCollection<string> outstrs = new SynchronizedCollection<string>();
#endif
				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "..."));

				// Parallel the outer loop
				SynchronizedCollection<Monster> tests = new SynchronizedCollection<Monster>();
				Parallel.ForEach(runes[0], (r0, loopState) =>
				{
					//var tempReq = RequiredSets.OrderBy(i => i).ToList();
					var tempReq = RequiredSets.ToList();
					var tempMax = Maximum == null ? null : new Stats(Maximum, true);
					//bool[] tempCheck = new bool[3];
					int tempCheck = 0;
					if (!IsRunning_Unsafe)
						loopState.Break();

					// number of builds ruled out since last sync
					int kill = 0;
					// number of builds added since last sync
					int plus = 0;
					// number of builds skipped
					int skip = 0;

					bool isBad;
					double bestScore = 0, curScore;
					Stats cstats;

					Monster test = new Monster(mon);
					test.Current.Shrines = shrines;
					test.Current.Leader = leader;

					test.Current.FakeLevel = slotFakes;
					test.Current.PredictSubs = slotPred;
					test.ApplyRune(r0, 6);

					RuneSet set4 = r0.SetIs4 ? r0.Set : RuneSet.Null;
					RuneSet set2 = r0.SetIs4 ? RuneSet.Null : r0.Set;
					int pop4 = 0;
					int pop2 = 0;

					foreach (Rune r1 in runes[1])
					{
						if (!IsRunning_Unsafe) // Can't break to a label, don't want to goto
							break;
#if BUILD_PRECHECK_BUILDS
						if (!this.AllowBroken)
						{
							if (r1.SetIs4)
							{
								if (pop2 == 2)
									pop2 = 7;
								if (set4 == RuneSet.Null || pop4 >= 2)
								{
									set4 = r1.Set;
									pop4 = 2;
								}
								else if (set4 != r1.Set)
								{
#if BUILD_PRECHECK_BUILDS_DEBUG
									outstrs.Add($"bad4@2 {set4} {set2} | {r0.Set} {r1.Set}");
#endif
									skip += runes[2].Length * runes[3].Length * runes[4].Length * runes[5].Length;
									continue;
								}
							}
							else
							{
								if (pop4 == 2)
									pop4 = 7;
								if (set2 == RuneSet.Null || pop2 >= 2)
								{
									set2 = r1.Set;
									pop2 = 2;
								}
							}
						}
#endif
						test.ApplyRune(r1, 6);

						foreach (Rune r2 in runes[2])
						{
							if (!IsRunning_Unsafe)
								break;
#if BUILD_PRECHECK_BUILDS
							if (!this.AllowBroken)
							{
								if (r2.SetIs4)
								{
									if (pop2 == 3)
										pop2 = 7;
									if (set4 == RuneSet.Null || pop4 >= 3)
									{
										set4 = r2.Set;
										pop4 = 3;
									}
									else if (set4 != r2.Set)
									{
#if BUILD_PRECHECK_BUILDS_DEBUG
										outstrs.Add($"bad4@3 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set}");
#endif
										skip += runes[3].Length * runes[4].Length * runes[5].Length;
										continue;
									}
								}
								else
								{
									if (pop4 == 3)
										pop4 = 7;
									if (set2 == RuneSet.Null || pop2 >= 3)
									{
										set2 = r2.Set;
										pop2 = 3;
									}
									else if (set4 != RuneSet.Null && set2 != r2.Set)
									{
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

							foreach (Rune r3 in runes[3])
							{
								if (!IsRunning_Unsafe)
									break;
#if BUILD_PRECHECK_BUILDS
								if (!this.AllowBroken)
								{
									if (r3.SetIs4)
									{
										if (pop2 == 4)
											pop2 = 7;
										if (set4 == RuneSet.Null || pop4 >= 4)
										{
											set4 = r3.Set;
											pop4 = 4;
										}
										else if (set4 != r3.Set)
										{
#if BUILD_PRECHECK_BUILDS_DEBUG
											outstrs.Add($"bad4@4 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set}");
#endif
											skip += runes[4].Length * runes[5].Length;
											continue;
										}
									}
									else
									{
										if (pop4 == 4)
											pop4 = 7;
										if (set2 == RuneSet.Null || pop2 >= 4)
										{
											set2 = r3.Set;
											pop2 = 4;
										}
										else if (set4 != RuneSet.Null && set2 != r3.Set)
										{
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

								foreach (Rune r4 in runes[4])
								{
									if (!IsRunning_Unsafe)
									{
										break;
									}
#if BUILD_PRECHECK_BUILDS
									if (!this.AllowBroken)
									{
										if (r4.SetIs4)
										{
											if (pop2 == 5)
												pop2 = 7;
											if (set4 == RuneSet.Null || pop4 >= 5)
											{
												set4 = r4.Set;
												pop4 = 5;
											}
											else if (set4 != r4.Set)
											{
#if BUILD_PRECHECK_BUILDS_DEBUG
												outstrs.Add($"bad4@5 {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set} {r4.Set}");
#endif
												skip += runes[5].Length;
												continue;
											}
										}
										else
										{
											if (pop4 == 5)
												pop4 = 7;
											if (set2 == RuneSet.Null || pop2 >= 5)
											{
												set2 = r4.Set;
												pop2 = 5;
												
											}
											else if (set4 != RuneSet.Null && set2 != r4.Set)
											{
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

									foreach (Rune r5 in runes[5])
									{
										if (!IsRunning_Unsafe)
											break;

										test.ApplyRune(r5, 6);
#if BUILD_PRECHECK_BUILDS_DEBUG
										outstrs.Add($"fine {set4} {set2} | {r0.Set} {r1.Set} {r2.Set} {r3.Set} {r4.Set} {r5.Set}");
#endif
										isBad = false;

										cstats = test.GetStats();

										// check if build meets minimum
										isBad |= (Minimum != null && !(cstats.GreaterEqual(Minimum, true)));
										isBad |= (tempMax != null && cstats.CheckMax(tempMax));
										// if no broken sets, check for broken sets
										isBad |= (!AllowBroken && !test.Current.SetsFull);
										// if there are required sets, ensure we have them
										/*isBad |= (tempReq != null && tempReq.Count > 0
											// this Linq adds no overhead compared to GetStats() and ApplyRune()
											//&& !tempReq.All(s => test.Current.Sets.Count(q => q == s) >= tempReq.Count(q => q == s))
											//&& !tempReq.GroupBy(s => s).All(s => test.Current.Sets.Count(q => q == s.Key) >= s.Count())
											);*/

										if (tempReq != null && tempReq.Count > 0) {
											/*for (int i = 0; i < 3; i++) {
												tempCheck[i] = false;
											}*/
											tempCheck = 0;
											foreach (var r in tempReq) {
												int i;
												for (i = 0; i < 3; i++) {
													if (test.Current.Sets[i] == r && (tempCheck & (1 << i)) != (1 << i)) {
														tempCheck |= (1 << i);
														break;
													}
												}
												if (i >= 3) {
													isBad |= true;
													break;
												}
											}
										}

										if (isBad)
										{
											kill++;
										}

										else
										{
											// we found an okay build!
											plus++;
											curScore = CalcScore(cstats);

											if (BuildSaveStats)
											{
												foreach (Rune r in test.Current.Runes)
												{
													if (!BuildGoodRunes)
													{
														r.manageStats.AddOrUpdate("LoadFilt", 1, (s, d) => { return d + 1; });
														runeUsage.runesGood.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
														r.manageStats.AddOrUpdate("currentBuildPoints", curScore, (k, v) => Math.Max(v, curScore));
														r.manageStats.AddOrUpdate("cbp" + this.ID, curScore, (k, v) => Math.Max(v, curScore));
													}
													else
													{
														r.manageStats.AddOrUpdate("LoadFilt", 0.001, (s, d) => { return d + 0.001; });
														runeUsage.runesOkay.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
														r.manageStats.AddOrUpdate("cbp" + this.ID, curScore, (k, v) => Math.Max(v, curScore * 0.9));
													}
												}
											}

											// if we are to track all good builds, keep it
											if (!BuildDumpBads)
											{
												if (tests.Count < MaxBuilds32)
													tests.Add(new Monster(test, true));

												bestScore = CalcScore(bstats);
												lock (BestLock)
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

												curScore = CalcScore(cstats);

												lock (BestLock)
												{
													if (Best == null || bestScore < curScore)
													{
														Best = new Monster(test, true);
														bestScore = CalcScore(bstats);
														bstats = Best.GetStats();
														tests.Add(Best);
													}
												}
												if (tests.Count < MaxBuilds32 && BuildSaveStats)
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
											RuneLog.Debug(count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));
											BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + string.Format("{0:P2}", (count + complete - total) / (double)complete)));
											BuildProgTo?.Invoke(this, new ProgToEventArgs(this, (count + complete - total) / (double)complete, tests.Count));

											if (BuildTimeout <= 0) continue;
											if (DateTime.Now > begin.AddSeconds(BuildTimeout))
											{
												RuneLog.Info("Timeout");
												BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + "Timeout"));
												BuildProgTo?.Invoke(this, new ProgToEventArgs(this, 1, tests.Count));

												IsRunning = false;
												break;
											}
										}
									}
									// sum up what work we've done
									Interlocked.Add(ref total, -kill);
									Interlocked.Add(ref total, -skip);
									Interlocked.Add(ref actual, kill);
									Interlocked.Add(ref buildUsage.failed, kill);
									kill = 0;
									skip = 0;
									Interlocked.Add(ref count, plus);
									Interlocked.Add(ref actual, plus);
									Interlocked.Add(ref buildUsage.passed, plus);
									plus = 0;

									// if we've got enough, stop
									if (BuildGenerate > 0 && count >= BuildGenerate)
									{
										IsRunning = false;
										break;
									}
								}
							}
						}
					}
				});

				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + "99%+"));

#if BUILD_PRECHECK_BUILDS_DEBUG
				System.IO.File.WriteAllLines("_into_the_bridge.txt", outstrs.ToArray());
#endif
				if (BuildSaveStats)
				{
					foreach (var ra in runes)
					{
						foreach (var r in ra)
						{
							if (!BuildGoodRunes)
							{
								r.manageStats.AddOrUpdate("buildScoreTotal", CalcScore(Best), (k, v) => v + CalcScore(Best));
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
				RuneLog.Debug(IsRunning + " " + count + "/" + total + "  " + String.Format("{0:P2}", (count + complete - total) / (double)complete));
				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + "100%"));
				BuildProgTo?.Invoke(this, new ProgToEventArgs(this, 1, tests.Count));

				// sort *all* the builds
				int takeAmount = 1;
				if (BuildSaveStats)
					takeAmount = 10;
				if (BuildTake > 0)
					takeAmount = BuildTake;


				foreach (var ll in tests.Where(t => t != null).OrderByDescending(r => CalcScore(r.GetStats())).Take(takeAmount))
					loads.Add(ll);

				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, "Found a load " + loads.Count()));

				if (!BuildGoodRunes)
					buildUsage.loads = tests.ToList();

				// dump everything to console, if nothing to print to
				if (BuildPrintTo == null)
					foreach (var l in loads)
					{
						RuneLog.Debug(l.GetStats().Health + "  " + l.GetStats().Attack + "  " + l.GetStats().Defense + "  " + l.GetStats().Speed
							+ "  " + l.GetStats().CritRate + "%" + "  " + l.GetStats().CritDamage + "%" + "  " + l.GetStats().Resistance + "%" + "  " + l.GetStats().Accuracy + "%");
					}

				// sadface if no builds
				if (!loads.Any())
				{
					RuneLog.Info("No builds :(");
					BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + "Zero :("));
				}
				else
				{
					// remember the good one
					Best = loads.First();
					Best.score = CalcScore(Best.GetStats());
					BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + "best " + (Best?.score ?? -1)));
					//Best.Current.runeUsage = usage.runeUsage;
					//Best.Current.buildUsage = usage.buildUsage;
					Best.Current.ActualTests = actual;
					foreach (var bb in loads)
					{
						foreach (Rune r in bb.Current.Runes)
						{
							double val = Best.score;
							if (BuildGoodRunes)
							{
								val *= 0.25;
								if (bb == Best)
									runeUsage.runesSecond.AddOrUpdate(r, (byte)r.Slot, (key, ov) => (byte)r.Slot);
							}

							if (bb != Best)
								val *= 0.1;
							else
								r.manageStats.AddOrUpdate("In", (BuildGoodRunes ? 2 : 1), (s, e) => BuildGoodRunes ? 2 : 1);

							r.manageStats.AddOrUpdate("buildScoreIn", val, (k, v) => v + val);
						}
					}
					for (int i = 0; i < 6; i++)
					{
						if (!BuildGoodRunes && mon.Current.Runes[i] != null && mon.Current.Runes[i].Id != Best.Current.Runes[i].Id)
							mon.Current.Runes[i].Swapped = true;
					}
					foreach (var ra in runes)
					{
						foreach (var r in ra)
						{
							var cbp = r.manageStats.GetOrAdd("currentBuildPoints", 0);
							if (cbp / Best.score < 1)
								r.manageStats.AddOrUpdate("bestBuildPercent", cbp / Best.score, (k, v) => Math.Max(v, cbp / Best.score));
						}
					}
				}

				//loads = null;
				tests.Clear();
				tests = null;
				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + "Test cleared"));
				return BuildResult.Success;
			}
			catch (Exception e)
			{
				RuneLog.Error("Error " + e);
				BuildPrintTo?.Invoke(this, new PrintToEventArgs(this, prefix + e.ToString()));
				return BuildResult.Failure;
			}
			finally
			{
				IsRunning = false;
			}
		}

		//Dictionary<string, RuneFilter> rfS, Dictionary<string, RuneFilter> rfM, Dictionary<string, RuneFilter> rfG, 
		public bool RunFilters(int slot, out Stats rFlat, out Stats rPerc, out Stats rTest)
		{
			bool hasFilter = false;
			rFlat = new Stats();
			rPerc = new Stats();
			rTest = new Stats();

			// pull the filters (flat, perc, test) for all the tabs and stats
			Dictionary<string, RuneFilter> rfG = new Dictionary<string, RuneFilter>();
			if (runeFilters.ContainsKey(SlotIndex.Global))
				rfG = runeFilters[SlotIndex.Global];

			Dictionary<string, RuneFilter> rfM = new Dictionary<string, RuneFilter>();
			if (slot != 0 && runeFilters.ContainsKey((slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd)))
				rfM = runeFilters[(slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd)];

			Dictionary<string, RuneFilter> rfS = new Dictionary<string, RuneFilter>();
			if (slot > 0 && runeFilters.ContainsKey((SlotIndex)slot))
				rfS = runeFilters[(SlotIndex)slot];

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
					hasFilter = true;
				}
			}

			return hasFilter;
		}

		public int GetFakeLevel(Rune r)
		{
			int? pred = runePrediction.ContainsKey(SlotIndex.Global) ? runePrediction[SlotIndex.Global].Key : null;

			if (runePrediction.ContainsKey(r.Slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd))
			{
				var kv = runePrediction[r.Slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd];
				if (pred == null || (kv.Key != null && kv.Key > pred))
					pred = kv.Key;
			}

			if (runePrediction.ContainsKey((SlotIndex)r.Slot))
			{
				var kv = runePrediction[(SlotIndex)r.Slot];
				if (pred == null || (kv.Key != null && kv.Key > pred))
					pred = kv.Key;
			}
			if (pred < 0)
				return 0;

			return pred ?? 0;
		}

		public double ScoreRune(Rune r, int raiseTo = 0, bool predictSubs = false)
		{
			int slot = r.Slot;
			double? testVal;
			Stats rFlat, rPerc, rTest;

			FilterType and = LoadFilters(slot, out testVal);
			if (RunFilters(slot, out rFlat, out rPerc, out rTest))
			{
				switch (and) {
					case FilterType.Or:
						return (r.Or(rFlat, rPerc, rTest, raiseTo, predictSubs)) ? 1 : 0;
					case FilterType.And:
						return (r.And(rFlat, rPerc, rTest, raiseTo, predictSubs)) ? 1 : 0;
					case FilterType.Sum:
					case FilterType.SumN:
						return r.Test(rFlat, rPerc, raiseTo, predictSubs);
				}
			}
			return 0;
		}
		
		public FilterType LoadFilters(int slot, out double? testVal)
		{
			// which tab we pulled the filter from
			testVal = null;
			FilterType and = 0;

			// TODO: check what inheriting SUM (eg. Odd and 3) does
			// TODO: check what inheriting AND/OR then SUM (or visa versa)

			// find the most significant operatand of joining checks
			if (runeScoring.ContainsKey(SlotIndex.Global))
			{
				var kv = runeScoring[SlotIndex.Global];
				if (kv.Key != FilterType.None) {
					if (runeFilters.ContainsKey(SlotIndex.Global))
						and = kv.Key;
					if (kv.Value != null) {
						testVal = kv.Value;
					}
				}
			}
			// is it and odd or even slot?
			var tmk = (slot % 2 == 0 ? SlotIndex.Even : SlotIndex.Odd);
			if (runeScoring.ContainsKey(tmk))
			{
				var kv = runeScoring[tmk];
				if (kv.Key != FilterType.None) {
					if (runeFilters.ContainsKey(tmk))
						and = kv.Key;
					if (kv.Value != null) {
						testVal = kv.Value;
					}
				}
			}
			// turn the 0-5 to a 1-6
			tmk = (SlotIndex)slot;
			if (runeScoring.ContainsKey(tmk))
			{
				var kv = runeScoring[tmk];
				if (kv.Key != FilterType.None) {
					if (runeFilters.ContainsKey(tmk))
						and = kv.Key;
					if (kv.Value != null) {
						testVal = kv.Value;
					}
				}
			}

			return and;
		}

		public Predicate<Rune> RuneScoring(int slot, int raiseTo = 0, bool predictSubs = false)
		{
			// the value to test SUM against
			double? testVal;

			FilterType and = LoadFilters(slot, out testVal);

			// this means that runes won't get in unless they meet at least 1 criteria

			// if an operand was found, ensure the tab contains filter data

			Stats rFlat, rPerc, rTest;

			// if there where no filters with data
			if (!RunFilters(slot, out rFlat, out rPerc, out rTest))
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
		public Stats NeededForMin(int?[] slotFakes, bool[] slotPred)
		{
			foreach (var rs in runes)
			{
				if (rs.Length <= 0)
					return null;
			}

			var smon = (Stats)mon;//.GetStats();
			var smin = Minimum;

			Stats ret = smin - smon;

			var avATK = runes[0].Average(r => r.GetValue(Attr.AttackFlat, (slotFakes[0] ?? 0), slotPred[0]));
			var avDEF = runes[2].Average(r => r.GetValue(Attr.DefenseFlat, (slotFakes[2] ?? 0), slotPred[2]));
			var avHP = runes[4].Average(r => r.GetValue(Attr.HealthFlat, (slotFakes[4] ?? 0), slotPred[4]));

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
			var avSel = runes[1].Where(r => r.Main.Type == Attr.Speed).ToArray();
			var avmSpeed = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Speed, (slotFakes[1] ?? 0), slotPred[1]));
			avSel = runes[3].Where(r => r.Main.Type == Attr.CritRate).ToArray();
			var avmCRate = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritRate, (slotFakes[3] ?? 0), slotPred[3]));
			avSel = runes[3].Where(r => r.Main.Type == Attr.CritDamage).ToArray();
			var avmCDam = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.CritDamage, (slotFakes[3] ?? 0), slotPred[3]));
			avSel = runes[5].Where(r => r.Main.Type == Attr.Accuracy).ToArray();
			var avmAcc = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Accuracy, (slotFakes[5] ?? 0), slotPred[5]));
			avSel = runes[5].Where(r => r.Main.Type == Attr.Resistance).ToArray();
			var avmRes = !avSel.Any() ? 0 : avSel.Average(r => r.GetValue(Attr.Resistance, (slotFakes[5] ?? 0), slotPred[5]));

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
			else if (avmCRate > 30 && ret.CritRate > avmCRate - 5)
			{
				evenSlots[1] = Attr.CritRate;
				ret.CritRate -= avmCRate;
			}

			// go back 6,4,2 for putting things in
			for (int i = 2; i >= 0; i--)
			{
				if (evenSlots[i] <= Attr.Null)
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
		public void GenRunes(Save save)
		{
			if (save?.Runes == null)
				return;

			IEnumerable<Rune> rsGlobal = save.Runes;

			// if not saving stats, cull unusable here
			if (!BuildSaveStats)
			{
				// Only using 'inventory' or runes on mon
				// also, include runes which have been unequipped (should only look above)
				if (!RunesUseEquipped)
					rsGlobal = rsGlobal.Where(r => (r.IsUnassigned || r.AssignedId == mon.Id) || r.Swapped);
				// only if the rune isn't currently locked for another purpose
				if (!RunesUseLocked)
					rsGlobal = rsGlobal.Where(r => !r.Locked);
				rsGlobal = rsGlobal.Where(r => !BannedRuneId.Any(b => b == r.Id) && !bannedRunesTemp.Any(b => b == r.Id));
			}

			if ((BuildSets.Any() || RequiredSets.Any()) && BuildSets.All(s => Rune.SetRequired(s) == 4) && RequiredSets.All(s => Rune.SetRequired(s) == 4))
			{
				// if only include/req 4 sets, include all 2 sets autoRuneSelect && ()
				rsGlobal = rsGlobal.Where(r => BuildSets.Contains(r.Set) || RequiredSets.Contains(r.Set) || Rune.SetRequired(r.Set) == 2);
			}
			else if (BuildSets.Any() || RequiredSets.Any())
			{
				rsGlobal = rsGlobal.Where(r => BuildSets.Contains(r.Set) || RequiredSets.Contains(r.Set));
				// Only runes which we've included
			}

			if (BuildSaveStats)
			{
				foreach (Rune r in rsGlobal)
				{
					r.manageStats.AddOrUpdate("currentBuildPoints", 0, (k, v) => 0);
					if (!BuildGoodRunes)
						r.manageStats.AddOrUpdate("Set", 1, (s, d) => { return d + 1; });
					else
						r.manageStats.AddOrUpdate("Set", 0.001, (s, d) => { return d + 0.001; });
				}
			}
			
			int?[] slotFakes = new int?[6];
			bool[] slotPred = new bool[6];
			GetPrediction(slotFakes, slotPred);

			// Set up each runeslot
			for (int i = 0; i < 6; i++)
			{
				// put the right ones in
				runes[i] = rsGlobal.Where(r => r.Slot == i + 1).ToArray();

				// makes sure that the primary stat type is in the selection
				if (i % 2 == 1 && slotStats[i].Count > 0) // actually evens because off by 1
				{
					runes[i] = runes[i].Where(r => slotStats[i].Contains(r.Main.Type.ToForms())).ToArray();
				}

				if (BuildSaveStats)
				{
					foreach (Rune r in runes[i])
					{
						if (!BuildGoodRunes)
							r.manageStats.AddOrUpdate("TypeFilt", 1, (s, d) => { return d + 1; });
						else
							r.manageStats.AddOrUpdate("TypeFilt", 0.001, (s, d) => { return d + 0.001; });
					}
					// cull here instead
					if (!RunesUseEquipped)
						runes[i] = runes[i].Where(r => (r.IsUnassigned || r.AssignedId == mon.Id) || r.Swapped).ToArray();
					if (!RunesUseLocked)
						runes[i] = runes[i].Where(r => r.Locked == false).ToArray();
				}
			}
			CleanBroken();

			if (autoRuneSelect)
			{
				// TODO: triple pass: start at needed for min, but each pass reduce the requirements by the average of the chosen runes for that pass, increase it by build scoring

				var needed = NeededForMin(slotFakes, slotPred);
				if (needed == null)
					autoRuneSelect = false;

				if (autoRuneSelect)
				{
					var needRune = new Stats(needed) / 6;

					// Auto-Rune select picking N per RuneSet should be fine to pick more because early-out should keep times low.
					// reduce number of runes to 10-15

					// odds first, then evens
					foreach (int i in new int[] { 0, 2, 4, 5, 3, 1 })
					{
						Rune[] rr = new Rune[0];
						foreach (var rs in RequiredSets)
						{
							rr = rr.Concat(runes[i].Where(r => r.Set == rs).OrderByDescending(r => RuneVsStats(r, needRune) * 10 + RuneVsStats(r, Sort)).Take(autoRuneAmount / 2).ToArray()).ToArray();
						}
						if (rr.Length < autoRuneAmount)
							rr = rr.Concat(runes[i].Where(r => !rr.Contains(r)).OrderByDescending(r => RuneVsStats(r, needRune) * 10 + RuneVsStats(r, Sort)).Take(autoRuneAmount - rr.Length).ToArray()).Distinct().ToArray();

						runes[i] = rr;
					}

					CleanBroken();
				}
			}
			if (!autoRuneSelect)
			{
				// TODO: Remove
				//var tmp = RuneLog.logTo;
				//using (var fs = new System.IO.FileStream("sampleselect.log", System.IO.FileMode.Create))
				//using (var sw = new System.IO.StreamWriter(fs))
				{
					//RuneLog.logTo = sw;
					// Filter each runeslot
					for (int i = 0; i < 6; i++)
					{
						// default fail OR
						Predicate<Rune> slotTest = RuneScoring(i + 1, (slotFakes[i] ?? 0), slotPred[i]);

						runes[i] = runes[i].Where(r => slotTest.Invoke(r)).OrderByDescending(r => r.manageStats.GetOrAdd("testScore", 0)).ToArray();
						double? n;
						if (LoadFilters(i + 1, out n) == FilterType.SumN) {
							var nInc = Math.Max(runes[i].GroupBy(r => r.Set).Count(rs => !RequiredSets.Contains(rs.Key)),
								runes[i].GroupBy(r => r.Set).Count(rs => RequiredSets.Contains(rs.Key)));
							runes[i] = runes[i].Where(r => !RequiredSets.Contains(r.Set)).GroupBy(r => r.Set).SelectMany(r => r.Take(Math.Max(1, (int)(n ?? 30) / nInc)))
								.Concat(runes[i].Where(r => RequiredSets.Contains(r.Set)).GroupBy(r => r.Set).SelectMany(r => r.Take(Math.Max(2, 2 * (int)(n ?? 30) / nInc)))).Distinct().ToArray();

							//runes[i] = runes[i].GroupBy(r => r.Set).SelectMany(rg => rg.Take((int)(n ?? 30))).ToArray();
							//runes[i] = runes[i].Take((int)(n ?? 30)).ToArray();
						}

						if (BuildSaveStats)
						{
							foreach (Rune r in runes[i])
							{
								if (!BuildGoodRunes)
									r.manageStats.AddOrUpdate("RuneFilt", 1, (s, d) => d + 1);
								else
									r.manageStats.AddOrUpdate("RuneFilt", 0.001, (s, d) => d + 0.001);
							}
						}
					}
					//RuneLog.logTo = tmp;
				}
			}
		}

		public IEnumerable<Rune> GetPowerupRunes()
		{
			if (!loads.Any())
				return new Rune[] { };
			double max = loads.Max(g => g.score);
			foreach (var r in loads.SelectMany(m => m.Current.Runes))
			{
				r.manageStats.AddOrUpdate("besttestscore", 0, (k, v) => 0);
			}

			foreach (var g in loads)
			{
				foreach (var r in g.Current.Runes)
				{
					r.manageStats.AddOrUpdate("besttestscore", g.score / max, (k, v) => v < g.score / max ? g.score / max : v);
				}
			}

			return loads.SelectMany(b => b.Current.Runes.Where(r => Math.Max(12, r.Level) < r.Rarity * 3 || r.Level < 12 || r.Level < GetFakeLevel(r))).Distinct();
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
		private double RuneVsStats(Rune r, Stats s)
		{
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
			if (s.EffectiveHP > 0)
			{
				var sh = 6000 * (100 + r.HealthPercent[0] + 20) / 100.0 + r.HealthFlat[0] + 1200;
				var sd = 300 * (100 + r.DefensePercent[0] + 10) / 100.0 + r.DefenseFlat[0] + 70;

				double delt = 0;
				delt += sh / ((1000 / (1000 + sd * 3)));
				delt -= 6000 / ((1000 / (1000 + 300.0 * 3)));
				ret += delt / s.EffectiveHP;
			}

			//Health / ((1000 / (1000 + Defense * 3 * 0.3)))
			if (s.EffectiveHPDefenseBreak > 0)
			{
				var sh = 6000 * (100 + r.HealthPercent[0] + 20) / 100.0 + r.HealthFlat[0] + 1200;
				var sd = 300 * (100 + r.DefensePercent[0] + 10) / 100.0 + r.DefenseFlat[0] + 70;

				double delt = 0;
				delt += sh / ((1000 / (1000 + sd * 0.9)));
				delt -= 6000 / ((1000 / (1000 + 300 * 0.9)));
				ret += delt / s.EffectiveHPDefenseBreak;
			}

			// (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage + CritDamage / 100)
			if (s.MaxDamage > 0)
			{
				var sa = 300 * (100 + r.AttackPercent[0] + 20) / 100.0 + r.AttackFlat[0] + 100;
				var cd = 50 + r.CritDamage[0] + 20;

				double delt = 0;
				delt += sa * (1 + cd / 100);
				delt -= 460 * (1 + 0.7);
				ret += delt / s.MaxDamage;
			}

			// (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage + CritDamage / 100 * Math.Min(CritRate, 100) / 100)
			if (s.AverageDamage > 0)
			{
				var sa = 300 * (100 + r.AttackPercent[0] + 20) / 100.0 + r.AttackFlat[0] + 100;
				var cd = 50 + r.CritDamage[0] + 20;
				var cr = 15 + r.CritRate[0] + 15;

				double delt = 0;
				delt += sa * (1 + cd / 100 * Math.Min(cr, 100) / 100);
				delt -= 460 * (1 + 0.7 * 0.3);
				ret += delt / s.AverageDamage;
			}

			// ExtraValue(Attr.AverageDamage) * Speed / 100
			if (s.DamagePerSpeed > 0)
			{
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