using System;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;

namespace RuneOptim
{
    public enum EquipCompare
    {
        Unknown,
        Worse,
        Better
    }

    public class Loadout
    {
        private Rune[] runes = new Rune[6];
        private int runeCount = 0;
        private RuneSet[] sets = new RuneSet[3];
        private bool setsFull = false;

        private int[] fakeLevel = new int[6];
        private bool[] predictSubs = new bool[6];

        private int buildID;
        public int BuildID { get { return buildID; } set { buildID = value; } }

        [JsonIgnore]
        public Rune[] Runes { get { return runes; } }
        [JsonIgnore]
        public int RuneCount { get { return runeCount; } }
        public RuneSet[] Sets { get { return sets; } }
        [JsonIgnore]
        public bool SetsFull { get { return setsFull; } }

        public double Time;

        public ConcurrentDictionary<string, double>[] manageStats;
        public ConcurrentDictionary<string, double>[] ManageStats
        {
            get
            {
                if (runes.All(r => r != null))
                    return runes.Select(r => r.manageStats).ToArray();
                return null;
            }
            set
            {
                manageStats = value;
            }
        }


        private int[] runeIDs;
        public int[] RuneIDs
        {
            get
            {
                if (runes.All(r => r != null))
                    runeIDs = runes.Select(r => r.ID).ToArray();
                return runeIDs;
            }
            set
            {
                runeIDs = value;
            }
        }

        public int[] FakeLevel { get { return fakeLevel; } set { fakeLevel = value; changed = true; } }
        public bool[] PredictSubs { get { return predictSubs; } set { predictSubs = value; changed = true; } }

        private Stats shrines = new Stats();
        private Stats leader = new Stats();

        private bool changed = false;
        [JsonIgnore]
        public bool Changed { get { return changed; } }

        [JsonIgnore]
        public Stats Shrines
        {
            get
            {
                return shrines;
            }
            set
            {
                shrines = value;
                changed = true;
            }
        }
        public Stats Leader
        {
            get
            {
                return leader;
            }
            set
            {
                leader = value;
                changed = true;
            }
        }

        public Loadout()
        {
        }

        public Loadout(Loadout rhs = null)
        {
            if (rhs != null)
            {
                shrines = rhs.shrines;
                leader = rhs.leader;
                fakeLevel = rhs.fakeLevel;
                predictSubs = rhs.predictSubs;
                buildID = rhs.buildID;
                foreach (var r in rhs.Runes)
                {
                    AddRune(r);
                }
                // TODO: do we even need to?
                manageStats = rhs.manageStats;
            }
        }

        // Debugging niceness
        public override string ToString()
        {
            string str = "";
            foreach (Rune r in runes)
            {
                str += r.ID + "  ";
            }
            str += "|  ";
            foreach (RuneSet s in sets)
            {
                str += s + " ";
            }
            return str;
        }

        // Lock all the runes on the build
        public void Lock()
        {
            foreach (Rune r in runes)
            {
                r.Locked = true;
            }
        }

        #region AttrGetters

        // Could have used runes.Where(x => x != null).Sum(x => x.STAT) + SetStat(Attr.STAT), but it was a bottleneck
        [JsonIgnore]
        public int HealthFlat
        {
            get
            {
				return 
					(runes[0]?.HealthFlat[fakeLevel[0], predictSubs[0]] ?? 0) +
					(runes[1]?.HealthFlat[fakeLevel[1], predictSubs[1]] ?? 0) +
					(runes[2]?.HealthFlat[fakeLevel[2], predictSubs[2]] ?? 0) +
					(runes[3]?.HealthFlat[fakeLevel[3], predictSubs[3]] ?? 0) +
					(runes[4]?.HealthFlat[fakeLevel[4], predictSubs[4]] ?? 0) +
					(runes[5]?.HealthFlat[fakeLevel[5], predictSubs[5]] ?? 0) +
					SetStat(Attr.HealthFlat); ;
            }
        }
        [JsonIgnore]
        public int HealthPercent
        {
            get
            {
				return
					(runes[0]?.HealthPercent[fakeLevel[0], predictSubs[0]] ?? 0) +
					(runes[1]?.HealthPercent[fakeLevel[1], predictSubs[1]] ?? 0) +
					(runes[2]?.HealthPercent[fakeLevel[2], predictSubs[2]] ?? 0) +
					(runes[3]?.HealthPercent[fakeLevel[3], predictSubs[3]] ?? 0) +
					(runes[4]?.HealthPercent[fakeLevel[4], predictSubs[4]] ?? 0) +
					(runes[5]?.HealthPercent[fakeLevel[5], predictSubs[5]] ?? 0) +
					SetStat(Attr.HealthPercent);
            }
        }

        [JsonIgnore]
        public int AttackFlat
        {
            get
            {
                return
                    (runes[0]?.AttackFlat[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.AttackFlat[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.AttackFlat[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.AttackFlat[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.AttackFlat[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.AttackFlat[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.AttackFlat);
            }
        }
        [JsonIgnore]
        public int AttackPercent
        {
            get
            {
                return
                    (runes[0]?.AttackPercent[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.AttackPercent[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.AttackPercent[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.AttackPercent[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.AttackPercent[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.AttackPercent[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.AttackPercent);
            }
        }

        [JsonIgnore]
        public int DefenseFlat
        {
            get
            {
                return
                    (runes[0]?.DefenseFlat[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.DefenseFlat[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.DefenseFlat[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.DefenseFlat[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.DefenseFlat[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.DefenseFlat[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.DefenseFlat);
            }
        }
        [JsonIgnore]
        public int DefensePercent
        {
            get
            {
                return
                    (runes[0]?.DefensePercent[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.DefensePercent[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.DefensePercent[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.DefensePercent[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.DefensePercent[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.DefensePercent[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.DefensePercent);
            }
        }

        [JsonIgnore]
        public int Speed
        {
            get
            {
                return
                    (runes[0]?.Speed[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.Speed[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.Speed[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.Speed[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.Speed[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.Speed[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.Speed);
            }
        }

        // Runes don't get SPD%
        [JsonIgnore]
        public int SpeedPercent
        {
            get
            {
                return SetStat(Attr.SpeedPercent) + (int)shrines.Speed + (int)leader.Speed;
            }
        }

        [JsonIgnore]
        public int CritRate
        {
            get
            {
                return
                    (runes[0]?.CritRate[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.CritRate[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.CritRate[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.CritRate[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.CritRate[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.CritRate[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.CritRate);
            }
        }

        [JsonIgnore]
        public int CritDamage
        {
            get
            {
                return
                    (runes[0]?.CritDamage[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.CritDamage[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.CritDamage[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.CritDamage[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.CritDamage[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.CritDamage[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.CritDamage);
            }
        }

        [JsonIgnore]
        public int Accuracy
        {
            get
            {
                return
                    (runes[0]?.Accuracy[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.Accuracy[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.Accuracy[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.Accuracy[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.Accuracy[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.Accuracy[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.Accuracy);
            }
        }

        [JsonIgnore]
        public int Resistance
        {
            get
            {
                return
                    (runes[0]?.Resistance[fakeLevel[0], predictSubs[0]] ?? 0) +
                    (runes[1]?.Resistance[fakeLevel[1], predictSubs[1]] ?? 0) +
                    (runes[2]?.Resistance[fakeLevel[2], predictSubs[2]] ?? 0) +
                    (runes[3]?.Resistance[fakeLevel[3], predictSubs[3]] ?? 0) +
                    (runes[4]?.Resistance[fakeLevel[4], predictSubs[4]] ?? 0) +
                    (runes[5]?.Resistance[fakeLevel[5], predictSubs[5]] ?? 0) +
                    SetStat(Attr.Resistance);
            }
        }

        #endregion

        // Put the rune on the build
        public void AddRune(Rune rune, int checkOn = 2)
        {
            // don't bother if not a rune
            if (rune == null)
                return;

            changed = true;

            if (runes[rune.Slot - 1] == null)
                runeCount++;

            runes[rune.Slot - 1] = rune;//new Rune(rune);
            if (runeCount % checkOn == 0)
                CheckSets();
        }

        // Removes the rune from slot
        public Rune RemoveRune(int slot)
        {
            // don't bother if there was none
            if (runes[slot - 1] == null)
                return null;

            changed = true;

            var r = runes[slot - 1];

            runes[slot - 1] = null;
            runeCount--;
            CheckSets();
            return r;
        }

        // Check what sets are completed in this build
        public void CheckSets()
        {
			setsFull = false;
            
            // If there are an odd number of runes, don't bother (maybe even check < 6?)
            if (runeCount % 2 == 1)
                return;

            // can only have 3 sets max (eg. energy / energy / blade)
            //if (sets == null)
            //    sets = new RuneSet[3];
            sets[0] = 0;
            sets[1] = 0;
            sets[2] = 0;

            // which set we are looking for
            int setInd = 0;
            // what slot we are looking at
            int slotInd = 0;
            // if we have used this slot in a set yet
            bool[] used = new bool[6];

            // how many runes are in sets
			int setNums = 0;

            // Check slots 1 - 5, minimum set size is 2.
            // Because it will search forward for sets, can't find more from 6
            // Eg. starting from 6, working around, find 2 runes?
            for (; slotInd < 5; slotInd++)
            {
                //if there is a uncounted rune in this slot
                Rune rune = runes[slotInd];
                if (rune != null && used[slotInd] == false)
                {
                    // look for more in the set
                    RuneSet set = rune.Set;
                    // how many runes we need to get
                    int getNum = Rune.SetRequired(set);
                    // how many we got
                    int gotNum = 1;
                    // we have now used this slot
                    used[slotInd] = true;

                    // for the runes after this rune
                    for (int ind = slotInd + 1; ind < 6; ind++)
                    {
                        // if there is a rune in this slot
                        if (runes[ind] != null)
                            // that hasn't been counted
                            if (used[ind] == false)
                                // that is the type I want
                                if (runes[ind].Set == set)
                                {
                                    used[ind] = true;
                                    gotNum++;
                                }

                        // if we have more than 1 rune
                        if (gotNum > 1)
                        {
                            // if we have enough runes for a set
                            if (gotNum == getNum)
                            {
                                // log this set
                                sets[setInd] = set;
                                // increase the number of runes in sets
								setNums += getNum;
                                // look for the next set
                                setInd++;
                                // stop looking forward
                                break;
                            }
                        }
                    }
                }
            }

            // if all runes are in sets
			if (setNums == 6)
				setsFull = true;
            // notify hackers their attempt has failed
			else if (setNums > 6)
				throw new Exception("Wut");

        }

        // pull how much Attr is in the equipped sets
        public int SetStat(Attr attr)
        {
            switch (attr)
            {
                case Attr.Null:
                case Attr.HealthFlat:
                case Attr.AttackFlat:
                case Attr.DefenseFlat:
                case Attr.Speed:
                case Attr.ExtraStat:
                case Attr.EffectiveHP:
                case Attr.EffectiveHPDefenseBreak:
                case Attr.DamagePerSpeed:
                case Attr.AverageDamage:
                case Attr.MaxDamage:
                    return 0;

                // I could use sets.Where(s => s.Equals(RuneSet.SET)).Count() * BONUS, but it was too slow
                case Attr.HealthPercent:
                    return (sets[0] == RuneSet.Energy ? 15 : 0) + (sets[1] == RuneSet.Energy ? 15 : 0) + (sets[2] == RuneSet.Energy ? 15 : 0);
                // 4 slot sets aren't going to be in [2]
                case Attr.AttackPercent:
                    return sets[0] == RuneSet.Fatal ? 35 : sets[1] == RuneSet.Fatal ? 35 : 0;
                case Attr.CritRate:
                    return (sets[0] == RuneSet.Blade ? 12 : 0) + (sets[1] == RuneSet.Blade ? 12 : 0) + (sets[2] == RuneSet.Blade ? 12 : 0);
                case Attr.CritDamage:
                    return sets[0] == RuneSet.Rage ? 40 : sets[1] == RuneSet.Rage ? 40 : 0;
                case Attr.SpeedPercent:
                    return sets[0] == RuneSet.Swift ? 25 : sets[1] == RuneSet.Swift ? 25 : 0;
                case Attr.Accuracy:
                    return (sets[0] == RuneSet.Focus ? 20 : 0) + (sets[1] == RuneSet.Focus ? 20 : 0) + (sets[2] == RuneSet.Focus ? 20 : 0);
                case Attr.DefensePercent:
                    return (sets[0] == RuneSet.Guard ? 15 : 0) + (sets[1] == RuneSet.Guard ? 15 : 0) + (sets[2] == RuneSet.Guard ? 15 : 0);
                case Attr.Resistance:
                    return (sets[0] == RuneSet.Endure ? 20 : 0) + (sets[1] == RuneSet.Endure ? 20 : 0) + (sets[2] == RuneSet.Endure ? 20 : 0);
                default:
                    return 0;
            }
        }

        // Using the given stats as a base, apply the modifiers
        public Stats GetStats(Stats baseStats)
        {
            Stats value = new Stats(baseStats);
            
            // Apply percent before flat
            value.Health += (int)Math.Ceiling(baseStats.Health * HealthPercent / 100.0) + HealthFlat;
            value.Attack += (int)Math.Ceiling(baseStats.Attack * AttackPercent / 100.0) + AttackFlat;
            value.Defense += (int)Math.Ceiling(baseStats.Defense * DefensePercent / 100.0) + DefenseFlat;
            value.Speed += (int)Math.Ceiling(baseStats.Speed * SpeedPercent / 100.0) + Speed;

            value.CritDamage += CritDamage;
            value.CritRate += CritRate;

            value.Accuracy += Accuracy;
            value.Resistance += Resistance;

            changed = false;

            return value;
        }

        /*// NYI comparison
        public EquipCompare CompareTo(Loadout rhs)
        {
            // check if the sets are comparable
            if (CompareSets(sets, rhs.sets) == 0)
                return EquipCompare.Unknown;

            int side = 0;
            if (HealthPercent > rhs.HealthPercent)
                side++;
            else
                side--;

            if (AttackPercent > rhs.AttackPercent)
                side++;
            else
                side--;

            if (DefensePercent > rhs.DefensePercent)
                side++;
            else
                side--;

            if (Speed > rhs.Speed)

            return EquipCompare.Unknown;
        }
        */

        // TODO: return an enum?
        // NYI comparison
        // 0 = differing magical sets
        // 1 = exact match
        // 2 = none or no differing magic sets
        public static int CompareSets(RuneSet[] a, RuneSet[] b)
        {
            if (a == b)
                return 1;

            if (a.Length == b.Length)
            {
                int same = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] == b[i])
                        same++;
                }
                if (same == a.Length)
                    return 1;
            }
            // different lengths, or not the same sets

            // count the number of magical sets, and make sure both loadout have the same number of sets
            foreach (RuneSet s in Rune.MagicalSets)
            {
                if (a.Where(x => x == s).Count() != b.Where(x => x == s).Count())
                    return 0;
            }

            return 1;
        }

    }
}
