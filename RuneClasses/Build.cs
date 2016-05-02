using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RuneOptim
{
    // The heavy lifter
    // Contains most of the data needed to outline build requirements
    public class Build
    {
        // allows iterative code, probably slow but nice to write and integrates with WinForms at a moderate speed
        public static string[] statNames = new string[] { "HP", "ATK", "DEF", "SPD", "CR", "CD", "RES", "ACC" };
        public static string[] extraNames = new string[] { "EHP", "EHPDB", "DPS", "AvD", "MxD" };

        public Build()
        {
            // for all 6 slots, init the list
            for (int i = 0; i < slotStats.Length; i++)
            {
                slotStats[i] = new List<string>();
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
        public Dictionary<string, KeyValuePair<int, int>> runeScoring = new Dictionary<string, KeyValuePair<int, int>>();

        public bool ShouldSerializeruneScoring()
        {
            Dictionary<string, KeyValuePair<int, int>> nscore = new Dictionary<string, KeyValuePair<int, int>>();
            foreach (var tabPair in runeScoring)
            {
                if (tabPair.Value.Key != 0 || tabPair.Value.Value != 0)
                    nscore.Add(tabPair.Key, new KeyValuePair<int, int>(tabPair.Value.Key, tabPair.Value.Value));
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

        // Which primary stat types are allowed per slot (should be 2,4,6 only)
        [JsonProperty("slotStats")]
        public List<string>[] slotStats = new List<string>[6];

        // Sets to consider using
        [JsonProperty("BuildSets")]
        public List<RuneSet> BuildSets = new List<RuneSet>();
        
        /// ---------------

        // These should be generated at runtime, do not store externally

        // The best loadouts
        [JsonIgnore]
        public IEnumerable<Monster> loads;

        // The best loadout in loads
        [JsonIgnore]
        public Monster Best = null;

        // The runes to be used to generate builds
        [JsonIgnore]
        public Rune[][] runes = new Rune[6][];

        /// ----------------
        
        // How to sort the stats
        [JsonIgnore]
        public Func<Stats, int> sortFunc;
        
        // if currently running
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

        /// <summary>
        /// Generates builds based on the instances variables.
        /// </summary>
        /// <param name="top">If non-zero, runs until N builds are generated</param>
        /// <param name="time">If non-zero, runs for N seconds</param>
        /// <param name="printTo">Periodically gives progress% and if it failed</param>
        /// <param name="progTo">Periodically gives the progress% as a double</param>
        /// <param name="dumpBads">If true, will only track new builds if they score higher than an other found builds</param>
        public void GenBuilds(int top = 0, int time = 0, Action<string> printTo = null, Action<double> progTo = null, bool dumpBads = false, bool saveStats = false)
        {
            if (runes.Any(r => r == null))
                return;
            try
            {


                Best = null;
                SynchronizedCollection<Monster> tests = new SynchronizedCollection<Monster>();
                long count = 0;
                long total = runes[0].Count();
                total *= runes[1].Count();
                total *= runes[2].Count();
                total *= runes[3].Count();
                total *= runes[4].Count();
                total *= runes[5].Count();
                long complete = total;

                if (total == 0)
                {
                    if (printTo != null)
                        printTo.Invoke("0 perms");
                    Console.WriteLine("Zero permuations");
                    return;
                }
                if (!AllowBroken && BuildSets.Count == 1 && Rune.SetRequired(BuildSets[0]) == 4)
                {
                    if (printTo != null)
                        printTo.Invoke("Bad sets");
                    Console.WriteLine("Cannot use 4 set with no broken");
                    return;
                }

                bool hasSort = false;
                foreach (string stat in statNames)
                {
                    if (Sort[stat] != 0)
                    {
                        hasSort = true;
                        break;
                    }
                }
                foreach (string extra in extraNames)
                {
                    if (Sort.ExtraGet(extra) != 0)
                    {
                        hasSort = true;
                        break;
                    }
                }
                if (top == 0 && !hasSort)
                {
                    if (printTo != null)
                        printTo.Invoke("No sort");
                    Console.WriteLine("No method of determining best");
                    return;
                }

                DateTime begin = DateTime.Now;
                DateTime timer = DateTime.Now;

                Console.WriteLine(count + "/" + total + "  " + string.Format("{0:P2}", (count + complete - total) / (double)complete));

                // build the scoring function
                Func<Stats, double> sort = (m) =>
                {
                    double pts = 0;

                    foreach (string stat in statNames)
                    {
                    // if this stat is used for sorting
                    if (Sort[stat] != 0)
                        {
                        // sum points for the stat
                        pts += m[stat] / Sort[stat];
                        // if exceeding max, subtracted the gained points and then some
                        if (Threshold[stat] != 0)
                                pts -= Math.Max(0, m[stat] - Threshold[stat]) / Sort[stat];
                        }
                    }
                // look, cool metrics!
                foreach (string extra in extraNames)
                    {
                        if (Sort.ExtraGet(extra) != 0)
                        {
                            pts += m.ExtraValue(extra) / Sort.ExtraGet(extra);
                            if (Threshold.ExtraGet(extra) != 0)
                                pts -= Math.Max(0, m.ExtraValue(extra) - Threshold.ExtraGet(extra)) / Sort.ExtraGet(extra);
                        }
                    }

                    return pts;
                };

                int[] slotFakes = new int[6];
                bool[] slotPred = new bool[6];

                // crank the rune prediction
                for (int i = 0; i < 6; i++)
                {
                    Rune[] rs = runes[i];
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

                // set to running
                isRun = true;

                // Parallel the outer loop
                var loopRes = Parallel.ForEach<Rune>(runes[0], (r0, loopState) =>
                {
                    if (!isRun)
                    //break;
                    loopState.Break();

                // number of builds ruled out since last sync
                int kill = 0;
                // number of builds added since last sync
                int plus = 0;

                    foreach (Rune r1 in runes[1])
                    {
                        if (!isRun) // Can't break to a lable, don't want to goto
                        break;
                        foreach (Rune r2 in runes[2])
                        {
                            if (!isRun)
                                break;
                            foreach (Rune r3 in runes[3])
                            {
                                if (!isRun)
                                    break;
                                foreach (Rune r4 in runes[4])
                                {
                                    if (!isRun)
                                        break;
                                    foreach (Rune r5 in runes[5])
                                    {
                                        if (!isRun)
                                            break;

                                        Monster test = new Monster(mon);
                                        test.Current.shrines = shrines;
                                        test.Current.leader = leader;

                                        test.Current.FakeLevel = slotFakes;
                                        test.Current.PredictSubs = slotPred;

                                        test.ApplyRune(r0);
                                        test.ApplyRune(r1);
                                        test.ApplyRune(r2);
                                        test.ApplyRune(r3);
                                        test.ApplyRune(r4);
                                        test.ApplyRune(r5);

                                        if (saveStats)
                                        {
                                            foreach (Rune r in test.Current.runes)
                                            {
                                                r.manageStats_LoadGen++;
                                            }
                                        }

                                        var cstats = test.GetStats();

                                        bool maxdead = false;

                                        if (Maximum != null)
                                        {
                                            foreach (var stat in statNames)
                                            {
                                                if (Maximum[stat] > 0 && cstats[stat] > Maximum[stat])
                                                {
                                                    maxdead = true;
                                                    break;
                                                }
                                            }
                                        }

                                    // check if build meets minimum
                                    if (Minimum != null && !(cstats > Minimum))
                                        {
                                            kill++;
                                        }
                                        else if (maxdead)
                                        {
                                            kill++;
                                        }
                                    // if no broken sets, check for broken sets
                                    else if (!AllowBroken && !test.Current.SetsFull)
                                        {
                                            kill++;
                                        }
                                    // if there are required sets, ensure we have them
                                    else if (RequiredSets != null && RequiredSets.Count > 0 && !RequiredSets.All(s => test.Current.sets.Contains(s)))
                                        {
                                            kill++;
                                        }
                                        else
                                        {
                                        // we found an okay build!
                                        plus++;

                                            if (saveStats)
                                            {
                                                foreach (Rune r in test.Current.runes)
                                                {
                                                    r.manageStats_LoadFilt++;
                                                }
                                            }

                                            // if we are to track all good builds, keep it
                                            if (!dumpBads)
                                            {
                                                tests.Add(test);
                                            }
                                            // if we only want to track really good builds
                                            else
                                            {
                                                // if there are currently no good builds, keep it
                                                if (tests.FirstOrDefault() == null)
                                                {
                                                    tests.Add(test);
                                                    Best = test;
                                                }
                                                else
                                                {
                                                    // if this build is better than the best, keep it
                                                    if (sort(Best.GetStats()) < sort(test.GetStats()))
                                                    {
                                                        Best = test;
                                                        tests.Add(test);
                                                    }
                                                }
                                            }
                                        }

                                    // every second, give a bit of feedback to those watching
                                    if (DateTime.Now > timer.AddSeconds(1))
                                        {
                                            timer = DateTime.Now;
                                            Console.WriteLine(count + "/" + total + "  " + String.Format("{0:P2}", (double)(count + complete - total) / (double)complete));
                                            if (printTo != null)
                                                printTo.Invoke(String.Format("{0:P2}", (double)(count + complete - total) / (double)complete));
                                            if (progTo != null)
                                                progTo.Invoke((double)(count + complete - total) / (double)complete);

                                            if (time > 0)
                                            {
                                                if (DateTime.Now > begin.AddSeconds(time))
                                                {
                                                    isRun = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                // sum up what work we've done
                                Interlocked.Add(ref total, -kill);
                                    kill = 0;
                                    Interlocked.Add(ref count, plus);
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

                // write out completion
                Console.WriteLine(isRun + " " + count + "/" + total + "  " + String.Format("{0:P2}", (double)(count + complete - total) / (double)complete));
                if (printTo != null)
                    printTo.Invoke("100%");
                if (progTo != null)
                    progTo.Invoke(1);

                // sort *all* the builds
                loads = tests.Where(t => t != null).OrderByDescending(r => sort(r.GetStats())).Take((top > 0 ? top : 1));

                // dump everything to console, if nothing to print to
                if (printTo == null)
                    foreach (var l in loads)
                    {
                        Console.WriteLine(l.GetStats().Health + "  " + l.GetStats().Attack + "  " + l.GetStats().Defense + "  " + l.GetStats().Speed
                            + "  " + l.GetStats().CritRate + "%" + "  " + l.GetStats().CritDamage + "%" + "  " + l.GetStats().Resistance + "%" + "  " + l.GetStats().Accuracy + "%");
                    }

                // sadface if no builds
                if (loads.Count() == 0)
                {
                    Console.WriteLine("No builds :(");
                    if (printTo != null)
                        printTo.Invoke("Zero :(");
                }
                else
                {
                    // remember the good one
                    Best = loads.First();
                    foreach (Rune r in Best.Current.runes)
                    {
                        r.manageStats_In = true;
                    }
					for (int i = 0; i < 6; i++)
					{
						if (mon.Current.runes[i] != null && mon.Current.runes[i].ID != Best.Current.runes[i].ID)
							mon.Current.runes[i].Swapped = true;
					}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + e);
                if (printTo != null)
                    printTo.Invoke(e.ToString());
            }
        }
        
        /// <summary>
        /// Fills the instance with acceptable runes from save
        /// </summary>
        /// <param name="save">The Save data that contais the runes</param>
        /// <param name="useLocked">If it should include locked runes</param>
        /// <param name="useEquipped">If it should include equipped runes (other than the current monster)</param>
        public void GenRunes(Save save, bool useLocked = false, bool useEquipped = false, bool saveStats = false)
        {
            if (save == null)
                return;
            if (save.Runes == null)
                return;

            IEnumerable<Rune> rsGlobal = save.Runes;
            
            // Only using 'inventory' or runes on mon
            if (!useEquipped)
                rsGlobal = rsGlobal.Where(r => (r.AssignedName == "Unknown name" || r.AssignedName == mon.Name));
            // only if the rune isn't currently locked for another purpose
            if (!useLocked)
                rsGlobal = rsGlobal.Where(r => r.Locked == false);
            
            // Only runes which we've included
            rsGlobal = rsGlobal.Where(r => BuildSets.Contains(r.Set));

            if (saveStats)
            {
                foreach (Rune r in rsGlobal)
                {
                    r.manageStats_Set++;
                }
            }

            int[] slotFakes = new int[6];
            bool[] slotPred = new bool[6];

            // For each runeslot
            for (int i = 0; i < 6; i++)
            {
                // put the right ones in
                runes[i] = rsGlobal.Where(r => r.Slot == i + 1).ToArray();

                // crank the rune prediction
                Rune[] rs = runes[i];
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
                

                // default fail OR
                Predicate<Rune> slotTest = r => false;
                int and = 0;
                // this means that runes won't get in unless they meet at least 1 criteria

                // which tab we pulled the filter from
                string gotScore = "";
                // the value to test SUM against
                int testVal = 0;

                // TODO: check what inheriting SUM (eg. Odd and 3) does
                // TODO: check what inheriting AND/OR then SUM (or visa versa)

                // find the most significant operatand of joining checks
                if (runeScoring.ContainsKey("g") && runeFilters.ContainsKey("g") && runeFilters["g"].Any(r => r.Value.NonZero))
                {
                    var kv = runeScoring["g"];
                    gotScore = "g";
                    and = kv.Key;
                    if (kv.Key == 1)
                    {
                        slotTest = r => true;
                    }
                    else if (kv.Key == 2)
                    {
                        testVal = kv.Value;
                    }
                }
                // is it and odd or even slot?
                string tmk = (i % 2 == 1 ? "e" : "o");
                if (runeScoring.ContainsKey(tmk) && runeFilters.ContainsKey(tmk) && runeFilters[tmk].Any(r => r.Value.NonZero))
                {
                    var kv = runeScoring[tmk];
                    gotScore = tmk;
                    and = kv.Key;
                    if (kv.Key == 1)
                    {
                        slotTest = r => true;
					}
                    else if (kv.Key == 2)
                    {
                        testVal = kv.Value;
                    }
                }
                // turn the 0-5 to a 1-6
                tmk = (i + 1).ToString();
                if (runeScoring.ContainsKey(tmk) && runeFilters.ContainsKey(tmk) && runeFilters[tmk].Any(r => r.Value.NonZero))
                {
                    var kv = runeScoring[tmk];
                    gotScore = tmk;
                    and = kv.Key;
                    if (kv.Key == 1)
                    {
                        slotTest = r => true;
                    }
                    else if (kv.Key == 2)
                    {
                        testVal = kv.Value;
                    }
                }
                

				// if an operand was found, ensure the tab contains filter data
				if (gotScore != "")
				{
					if (runeFilters.ContainsKey(gotScore))
					{
                        // if all the filters for the tab are zero
						if (runeFilters[gotScore].All(r => !r.Value.NonZero))
						{
                            // set to OR TRUE
							slotTest = r => true;
							and = 0;
						}
					}
				}
				else
				{
                    // if there wasn't any relevant data for how to pick runes, just take 'em all!
					slotTest = r => true;
				}

                // pull the filters (flat, perc, test) for all the tabs and stats
                Dictionary<string, RuneFilter> rfG = new Dictionary<string, RuneFilter>();
                if (runeFilters.ContainsKey("g"))
                    rfG = runeFilters["g"];

                Dictionary<string, RuneFilter> rfM = new Dictionary<string, RuneFilter>();
                if (runeFilters.ContainsKey((i % 2 == 1 ? "e" : "o")))
                    rfM = runeFilters[(i % 2 == 1 ? "e" : "o")];

                Dictionary<string, RuneFilter> rfS = new Dictionary<string, RuneFilter>();
                if (runeFilters.ContainsKey((i+1).ToString()))
                    rfS = runeFilters[(i + 1).ToString()];

                // if there where no filters with data
                bool blank = true;

                Stats rFlat = new Stats();
                Stats rPerc = new Stats();
                Stats rTest = new Stats();

                foreach (string stat in statNames)
                {
                    RuneFilter rf = new RuneFilter();
                    if (rfS.ContainsKey(stat))
                    {
                        rf = rfS[stat];
                        if (rfM.ContainsKey(stat))
                            rf = RuneFilter.Min(rf, rfM[stat]);

                        if (rfG.ContainsKey(stat))
                            rf = RuneFilter.Min(rf, rfG[stat]);

                    }
                    else
                    {
                        if (rfM.ContainsKey(stat))
                        {
                            rf = rfM[stat];
                            if (rfG.ContainsKey(stat))
                                rf = RuneFilter.Min(rf, rfG[stat]);
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
                        rFlat[stat] = rf.Flat;
                        rPerc[stat] = rf.Percent;
                        rTest[stat] = rf.Test;
                        blank = false;
                    }
                }

                // TODO: seems like it just ignores all the slotTesting before now

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
                runes[i] = runes[i].Where(r => slotTest.Invoke(r)).ToArray();

                if (saveStats)
                {
                    foreach (Rune r in runes[i])
                    {
                        r.manageStats_RuneFilt++;
                    }
                }

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
                        r.manageStats_TypeFilt++;
                    }
                }
            }

            // Make sure that for each set type, there are enough slots with runes in them
            // Eg. if only 1,4,5 have Violent, remove all violent runes because you need 4
            // for each included set
            foreach (RuneSet s in BuildSets)
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

}
