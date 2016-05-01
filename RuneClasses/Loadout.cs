using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Rune[] runes = new Rune[6];
        public int runeCount = 0;
        public RuneSet[] sets = new RuneSet[3];
        public bool SetsFull = false;

        public int[] FakeLevel = new int[6];
        public bool[] PredictSubs = new bool[6];

        public Stats shrines = new Stats();
        public Stats leader = new Stats();
        
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
        public int HealthFlat
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.HealthFlat, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.HealthFlat, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.HealthFlat, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.HealthFlat, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.HealthFlat, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.HealthFlat, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.HealthFlat);
            }
        }
        public int HealthPercent
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.HealthPercent, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.HealthPercent, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.HealthPercent, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.HealthPercent, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.HealthPercent, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.HealthPercent, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.HealthPercent);
            }
        }

        public int AttackFlat
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.AttackFlat, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.AttackFlat, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.AttackFlat, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.AttackFlat, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.AttackFlat, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.AttackFlat, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.AttackFlat);
            }
        }
        public int AttackPercent
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.AttackPercent, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.AttackPercent, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.AttackPercent, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.AttackPercent, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.AttackPercent, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.AttackPercent, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.AttackPercent);
            }
        }

        public int DefenseFlat
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.DefenseFlat, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.DefenseFlat, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.DefenseFlat, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.DefenseFlat, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.DefenseFlat, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.DefenseFlat, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.DefenseFlat);
            }
        }
        public int DefensePercent
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.DefensePercent, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.DefensePercent, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.DefensePercent, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.DefensePercent, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.DefensePercent, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.DefensePercent, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.DefensePercent);
            }
        }

        public int Speed
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.Speed, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.Speed, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.Speed, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.Speed, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.Speed, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.Speed, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.Speed);
            }
        }

        // Runes don't get SPD%
        public int SpeedPercent
        {
            get
            {
                return SetStat(Attr.SpeedPercent) + (int)shrines.Speed + (int)leader.Speed;
            }
        }

        public int CritRate
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.CritRate, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.CritRate, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.CritRate, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.CritRate, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.CritRate, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.CritRate, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.CritRate);
            }
        }

        public int CritDamage
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.CritDamage, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.CritDamage, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.CritDamage, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.CritDamage, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.CritDamage, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.CritDamage, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.CritDamage);
            }
        }

        public int Accuracy
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.Accuracy, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.Accuracy, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.Accuracy, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.Accuracy, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.Accuracy, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.Accuracy, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.Accuracy);
            }
        }

        public int Resistance
        {
            get
            {
                return
                    (runes[0] != null ? runes[0].GetValue(Attr.Resistance, FakeLevel[0], PredictSubs[0]) : 0) +
                    (runes[1] != null ? runes[1].GetValue(Attr.Resistance, FakeLevel[1], PredictSubs[1]) : 0) +
                    (runes[2] != null ? runes[2].GetValue(Attr.Resistance, FakeLevel[2], PredictSubs[2]) : 0) +
                    (runes[3] != null ? runes[3].GetValue(Attr.Resistance, FakeLevel[3], PredictSubs[3]) : 0) +
                    (runes[4] != null ? runes[4].GetValue(Attr.Resistance, FakeLevel[4], PredictSubs[4]) : 0) +
                    (runes[5] != null ? runes[5].GetValue(Attr.Resistance, FakeLevel[5], PredictSubs[5]) : 0) +
                    SetStat(Attr.Resistance);
            }
        }

        #endregion

        // Put the rune on the build
        public void AddRune(Rune rune)
        {
            // don't bother if not a rune
            if (rune == null)
                return;

            if (runes[rune.Slot - 1] == null)
                runeCount++;

            runes[rune.Slot - 1] = rune;//new Rune(rune);
            if (runeCount % 2 == 0)
                CheckSets();
        }

        // Removes the rune from slot
        public void RemoveRune(int slot)
        {
            // don't bother if there was none
            if (runes[slot - 1] == null)
                return;

            runes[slot - 1] = null;
            runeCount--;
            CheckSets();
        }

        // Check what sets are completed in this build
        public void CheckSets()
        {
			SetsFull = false;
            
            // If there are an odd number of runes, don't bother (maybe even check < 6?)
            if (runeCount % 2 == 1)
                return;

            // can only have 3 sets max (eg. energy / energy / blade)
            sets = new RuneSet[3];

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
				SetsFull = true;
            // notify hackers their attempt has failed
			else if (setNums > 6)
				throw new Exception("Wut");

        }

        // pull how much Attr is in the equipped sets
        public int SetStat(Attr attr)
        {
            switch (attr)
            {
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
            }
            return 0;
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
