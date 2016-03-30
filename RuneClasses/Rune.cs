using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace RuneOptim
{
    // Enums up Runesets
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuneSet
    {
        Null, // No set
        Energy, // Health
        Fatal, // Attack
        Blade, // CRate
        Rage, // CDmg
        Swift, // Speed
        Focus, // Acc
        Guard, // Def
        Endure, // Res

        // Here be magic
        Violent,
        Will,
        Nemesis,
        Shield,
        Revenge,
        Despair,
        Vampire,
        Destroy
    }

    // Allows me to steal the JSON values into Enum
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Attr
    {
        [EnumMember(Value = "")]
        Null,

        [EnumMember(Value = "HP flat")]
        HealthFlat,

        [EnumMember(Value = "HP%")]
        HealthPercent,

        [EnumMember(Value = "ATK flat")]
        AttackFlat,

        [EnumMember(Value = "ATK%")]
        AttackPercent,

        [EnumMember(Value = "DEF flat")]
        DefenseFlat,

        [EnumMember(Value = "DEF%")]
        DefensePercent,

        [EnumMember(Value = "SPD")]
        Speed,

        // Thanks Swift -_-
        SpeedPercent,

        [EnumMember(Value = "CRate")]
        CritRate,

        [EnumMember(Value = "CDmg")]
        CritDamage,

        [EnumMember(Value = "ACC")]
        Accuracy,

        [EnumMember(Value = "RES")]
        Resistance,

    }

    public class Rune
    {
        public Rune()
        {

        }

        public Rune(Rune rhs)
        {
            ID = rhs.ID;
            Set = rhs.Set;
            Grade = rhs.Grade;
            Slot = rhs.Slot;
            Level = rhs.Level;
            FakeLevel = rhs.FakeLevel;
            PredictSubs = rhs.PredictSubs;
            Locked = rhs.Locked;
            AssignedId = rhs.AssignedId;
            AssignedName = rhs.AssignedName;
            MainType = rhs.MainType;
            MainValue = rhs.MainValue;
            InnateType = rhs.InnateType;
            InnateValue = rhs.InnateValue;
            Sub1Type = rhs.Sub1Type;
            Sub1Value = rhs.Sub1Value;
            Sub2Type = rhs.Sub2Type;
            Sub2Value = rhs.Sub2Value;
            Sub3Type = rhs.Sub3Type;
            Sub3Value = rhs.Sub3Value;
            Sub4Type = rhs.Sub4Type;
            Sub4Value = rhs.Sub4Value;
            Parent = rhs;
        }

        [JsonProperty("id")]
        public int ID;

        [JsonProperty("set")]
        public RuneSet Set;

        [JsonProperty("grade")]
        public int Grade;

        [JsonProperty("slot")]
        public int Slot;

        [JsonProperty("level")]
        public int Level;

        [JsonIgnore]
        public int FakeLevel = 0;

        [JsonIgnore]
        public bool PredictSubs = false;

        [JsonIgnore]
        public Rune Parent = null;

        [JsonProperty("locked")]
        private bool locked;

        [JsonIgnore]
        public bool Locked
        {
            get
            {
                if (Parent != null && Parent.locked) return true;
                return locked;
            }
            set
            {
                locked = value;
                if (Parent != null)
                    Parent.Locked = value;
            }
        }

        [JsonProperty("monster")]
        public int AssignedId;

        [JsonProperty("monster_n")]
        public string AssignedName;

        [JsonProperty("m_t")]
        public Attr MainType;

        [JsonProperty("m_v")]
        public int MainValue;

        [JsonProperty("i_t")]
        public Attr InnateType;

        [JsonProperty("i_v")]
        public int? InnateValue;

        [JsonProperty("s1_t")]
        public Attr Sub1Type;

        [JsonProperty("s1_v")]
        public int? Sub1Value;

        [JsonProperty("s2_t")]
        public Attr Sub2Type;

        [JsonProperty("s2_v")]
        public int? Sub2Value;

        [JsonProperty("s3_t")]
        public Attr Sub3Type;

        [JsonProperty("s3_v")]
        public int? Sub3Value;

        [JsonProperty("s4_t")]
        public Attr Sub4Type;

        [JsonProperty("s4_v")]
        public int? Sub4Value;
        
        // Nicer getters for stats by type

        public int Accuracy { get { return GetValue(Attr.Accuracy); } }
        public int AttackFlat { get { return GetValue(Attr.AttackFlat); } }
        public int AttackPercent { get { return GetValue(Attr.AttackPercent); } }
        public int CritDamage { get { return GetValue(Attr.CritDamage); } }
        public int CritRate { get { return GetValue(Attr.CritRate); } }
        public int DefenseFlat { get { return GetValue(Attr.DefenseFlat); } }
        public int DefensePercent { get { return GetValue(Attr.DefensePercent); } }
        public int HealthFlat { get { return GetValue(Attr.HealthFlat); } }
        public int HealthPercent { get { return GetValue(Attr.HealthPercent); } }
        public int Resistance { get { return GetValue(Attr.Resistance); } }
        public int Speed { get { return GetValue(Attr.Speed); } }

        public int Rarity
        {
            get
            {
                if (Sub1Type == Attr.Null) return 0; // Normal
                if (Sub2Type == Attr.Null) return 1; // Magic
                if (Sub3Type == Attr.Null) return 2; // Rare
                if (Sub4Type == Attr.Null) return 3; // Hero
                return 4; // Legend
            }
        }

        // Number of sets
        public static int SetCount = Enum.GetNames(typeof(RuneSet)).Length;

        // Number of runes required for set to be complete
        public static int SetRequired(RuneSet set)
        {
            if (set == RuneSet.Energy || set == RuneSet.Blade || set == RuneSet.Focus || set == RuneSet.Guard || set == RuneSet.Shield
               || set == RuneSet.Revenge || set == RuneSet.Nemesis || set == RuneSet.Will || set == RuneSet.Endure || set == RuneSet.Destroy)
                return 2;
            if (set == RuneSet.Swift || set == RuneSet.Fatal || set == RuneSet.Violent || set == RuneSet.Vampire || set == RuneSet.Despair || set == RuneSet.Rage)
                return 4;

            // Unknown set will never be complete!
            return 7;
        }

        // Format rune values okayish
        public string StringIt()
        {
            return StringIt(MainType, MainValue);
        }

        public static string StringIt(Attr type, int? val)
        {
            if (type == Attr.Null || !val.HasValue)
                return "";
            return StringIt(type, val.Value);
        }

        public static string StringIt(Attr type, int val)
        {
            string ret = StringIt(type);

            ret += " +" + val;

            if (type.ToString().Contains("Percent") || type == Attr.CritRate || type == Attr.CritDamage || type == Attr.Accuracy || type == Attr.Resistance)
            {
                ret += "%";
            }

            return ret;
        }

        // Ask the rune for the value of the Attribute type as a string
        public static string StringIt(Attr type, bool suffix = false)
        {
            string ret = "";

            switch (type)
            {
                case Attr.HealthFlat:
                case Attr.HealthPercent:
                    ret += "HP";
                    break;
                case Attr.AttackPercent:
                case Attr.AttackFlat:
                    ret += "ATK";
                    break;
                case Attr.DefenseFlat:
                case Attr.DefensePercent:
                    ret += "DEF";
                    break;
                case Attr.Speed:
                    ret += "SPD";
                    break;
                case Attr.CritRate:
                    ret += "CRI Rate";
                    break;
                case Attr.CritDamage:
                    ret += "CRI Dmg";
                    break;
                case Attr.Accuracy:
                    ret += "Accuracy";
                    break;
                case Attr.Resistance:
                    ret += "Resistance";
                    break;
            }
            if (type.ToString().Contains("Percent") || type == Attr.CritRate || type == Attr.CritDamage || type == Attr.Accuracy || type == Attr.Resistance)
                ret += "%";

            return ret;
        }

        // these sets can't be superceded by really good stats
        // Eg. Blade can be replaced by 12% crit.
        public static RuneSet[] MagicalSets = { RuneSet.Violent, RuneSet.Will, RuneSet.Nemesis, RuneSet.Shield, RuneSet.Revenge, RuneSet.Despair, RuneSet.Vampire, RuneSet.Destroy };

        // Debugger niceness
        public override string ToString()
        {
            return Grade + "* " + Set + " " + StringIt();
        }

        // Gets the value of that Attribute on this rune
        public int GetValue(Attr stat)
        {
            // the stat can only be present once per rune, early exit
            if (MainType == stat)
            {
                if (FakeLevel <= Level || Grade < 3)
                {
                    return MainValue;
                }
                else
                {
                    return MainValues[MainType][Grade - 3][FakeLevel];
                }
            }
            // Need to be able to load in null values (requiring int?) but xType shouldn't be a Type if xValue is null
            if (InnateType == stat) return InnateValue ?? 0;
            // Here, if a subs type is null, there is not further subs (it's how runes work), so we quit early.
            if (PredictSubs == false)
            {
                if (Sub1Type == stat || Sub1Type == Attr.Null) return Sub1Value ?? 0;
                if (Sub2Type == stat || Sub2Type == Attr.Null) return Sub2Value ?? 0;
                if (Sub3Type == stat || Sub3Type == Attr.Null) return Sub3Value ?? 0;
                if (Sub4Type == stat || Sub4Type == Attr.Null) return Sub4Value ?? 0;
            }
            else
            {
                // count how many upgrades have gone into the rune
                int maxUpgrades = Math.Min(4, (int)Math.Floor(FakeLevel / (double)3));
                int upgradesGone = Math.Min(4, (int)Math.Floor(Level / (double)3));
                // how many new sub are to appear (0 legend will be 4 - 4 = 0, 6 rare will be 4 - 3 = 1, 6 magic will be 4 - 2 = 2)
                int subNew = 4 - Rarity;
                // how many subs will go into existing stats (0 legend will be 4 - 0 - 0 = 4, 6 rare will be 4 - 1 - 2 = 1, 6 magic will be 4 - 2 - 2 = 0)
                int subEx = maxUpgrades - upgradesGone - subNew;
                int subVal = (subNew > 0 ? 1 : 0);

                if (Sub1Type == stat || Sub1Type == Attr.Null) return Sub1Value + subEx ?? subVal;
                if (Sub2Type == stat || Sub2Type == Attr.Null) return Sub2Value + subEx ?? subVal;
                if (Sub3Type == stat || Sub3Type == Attr.Null) return Sub3Value + subEx ?? subVal;
                if (Sub4Type == stat || Sub4Type == Attr.Null) return Sub4Value + subEx ?? subVal;
            }
        
            return 0;
        }

        // Does it have this stat at all?
        public bool HasStat(Attr stat)
        {
            if (GetValue(stat) > 0)
                return true;
            return false;
        }

        // For each non-zero stat in flat and percent, divide the runes value and see if any >= test
        public bool Or(Stats rFlat, Stats rPerc, Stats rTest)
        {
            for (int i = 0; i < Build.statNames.Length; i++)
            {
                string stat = Build.statNames[i];
                if (i < 4 && rFlat[stat] != 0 && rTest[stat] != 0)
                    if (this[stat + "flat"] / (double)rFlat[stat] >= rTest[stat])
                        return true;
                if (i != 3 && rPerc[stat] != 0 && rTest[stat] != 0)
                    if (this[stat + "perc"] / (double)rPerc[stat] >= rTest[stat])
                        return true;
            }
            return false;
        }

        // For each non-zero stat in flat and percent, divide the runes value and see if *ALL* >= test
        public bool And(Stats rFlat, Stats rPerc, Stats rTest)
        {
            for (int i = 0; i < Build.statNames.Length; i++)
            {
                string stat = Build.statNames[i];
                if (i < 4 && rFlat[stat] != 0 && rTest[stat] != 0)
                    if (this[stat + "flat"] / (double)rFlat[stat] < rTest[stat])
                        return false;
                if (i != 3 && rPerc[stat] != 0 && rTest[stat] != 0)
                    if (this[stat + "perc"] / (double)rPerc[stat] < rTest[stat])
                        return false;
            }
            return true;
        }
        
        // sum the result of dividing the runes value by flat/percent per stat
        internal double Test(Stats rFlat, Stats rPerc)
        {
            double val = 0;
            for (int i = 0; i < Build.statNames.Length; i++)
            {
                string stat = Build.statNames[i];
                if (i < 4 && rFlat[stat] != 0)
                    val += this[stat + "flat"] / (double)rFlat[stat];
                if (i != 3 && rPerc[stat] != 0)
                    val += this[stat + "perc"] / (double)rPerc[stat];
            }
            return val;
        }

        // NYI rune comparison
        public EquipCompare CompareTo(Rune rhs)
        {
            if (Set != rhs.Set)
                return EquipCompare.Unknown;

            if (HealthPercent < rhs.HealthPercent)
                return EquipCompare.Worse;
            if (AttackPercent < rhs.AttackPercent)
                return EquipCompare.Worse;
            if (DefensePercent < rhs.DefensePercent)
                return EquipCompare.Worse;
            if (Speed < rhs.Speed)
                return EquipCompare.Worse;
            if (CritRate < rhs.CritRate)
                return EquipCompare.Worse;
            if (CritDamage < rhs.CritDamage)
                return EquipCompare.Worse;
            if (Accuracy < rhs.Accuracy)
                return EquipCompare.Worse;
            if (Resistance < rhs.Resistance)
                return EquipCompare.Worse;

            return EquipCompare.Better;
        }

        // fast iterate over rune stat types
        public int this[string stat] 
        {
            get
            {
                switch (stat)
                {
                    case "HPflat":
                        return HealthFlat;
                    case "HPperc":
                        return HealthPercent;
                    case "ATKflat":
                        return AttackFlat;
                    case "ATKperc":
                        return AttackPercent;
                    case "DEFflat":
                        return DefenseFlat;
                    case "DEFperc":
                        return DefensePercent;
                    case "SPDflat":
                        return Speed;
                    case "CDperc":
                        return CritDamage;
                    case "CRperc":
                        return CritRate;
                    case "ACCperc":
                        return Accuracy;
                    case "RESperc":
                        return Resistance;
                }
                return 0;
            }
        }

        #region stats

        public static int[][] MainValues_Speed = new int[][] {
            new int[]{3,4,5,6,8,9,10,12,13,14,16,17,18,19,21,25},
            new int[]{4,5,7,8,10,11,13,14,16,17,19,20,22,23,25,30},
            new int[]{5,7,9,11,13,15,17,19,21,23,25,27,29,31,33,39},
            new int[]{7,9,11,13,15,17,19,21,23,25,27,29,31,33,35,42}
        };

        public static int[][] MainValues_Flat = new int[][] {
            new int[]{7,12,17,22,27,32,37,42,47,52,57,62,67,72,77,92},
            new int[]{10,16,22,28,34,40,46,52,58,64,70,76,82,88,94,112},
            new int[]{15,22,29,36,43,50,57,64,71,78,85,92,99,106,113,135},
            new int[]{22,30,38,46,54,62,70,78,86,94,102,110,118,126,134,160}
        };

        public static int[][] MainValues_HPflat = new int[][] {
            new int[]{100,175,250,325,400,475,550,625,700,775,850,925,1000,1075,1150,1380},
            new int[]{160,250,340,430,520,610,700,790,880,970,1060,1150,1240,1330,1420,1704},
            new int[]{270,375,480,585,690,795,900,1005,1110,1215,1320,1425,1530,1635,1740,2088},
            new int[]{360,480,600,720,840,960,1080,1200,1320,1440,1560,1680,1800,1920,2040,2448}
        };

        public static int[][] MainValues_Percent = new int[][] {
            new int[]{4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,38},
            new int[]{5,7,9,11,13,16,18,20,22,23,27,29,31,33,36,43},
            new int[]{8,10,12,15,17,20,22,24,27,29,32,34,37,40,43,51},
            new int[]{11,14,17,20,23,26,29,32,35,38,41,44,47,50,53,63}
        };
        public static int[][] MainValues_CRate = new int[][] {
            new int[]{3,5,7,9,11,13,15,17,19,21,23,25,27,29,31,37},
            new int[]{4,6,8,11,13,15,17,19,22,24,26,28,30,33,35,41},
            new int[]{5,7,10,12,15,17,19,22,24,27,29,31,34,36,39,47},
            new int[]{7,10,13,16,19,22,25,28,31,34,37,40,43,46,49,58}
        };

        public static int[][] MainValues_CDmg = new int[][] {
            new int[]{4,6,9,11,13,16,18,20,22,25,27,29,32,34,36,43},
            new int[]{6,9,12,15,18,21,24,27,30,33,36,39,42,45,48,57},
            new int[]{8,11,15,18,21,25,28,31,34,38,41,44,48,51,54,65},
            new int[]{11,15,19,23,27,31,35,39,43,47,51,55,59,63,67,80}
        };

        public static int[][] MainValues_ResAcc = new int[][] {
            new int[] {4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,38},
            new int[] {6,8,10,13,15,17,19,21,24,26,28,30,32,35,37,44},
            new int[] {9,11,14,16,19,21,23,26,28,31,33,35,38,40,43,51},
            new int[] {12,15,18,21,24,27,30,33,36,39,42,45,48,51,54,64}
        };

        public static Dictionary<Attr, int[][]> MainValues = new Dictionary<Attr, int[][]>
        {
            {Attr.HealthFlat, MainValues_HPflat },
            {Attr.AttackFlat, MainValues_Flat },
            {Attr.DefenseFlat, MainValues_Flat },
            {Attr.Speed,MainValues_Speed },

            {Attr.HealthPercent, MainValues_Percent },
            {Attr.AttackPercent, MainValues_Percent },
            {Attr.DefensePercent, MainValues_Percent },

            {Attr.CritRate, MainValues_CRate },
            {Attr.CritDamage, MainValues_CDmg },

            {Attr.Accuracy, MainValues_ResAcc },
            {Attr.Resistance, MainValues_ResAcc },
        };

        #endregion
    }
}
