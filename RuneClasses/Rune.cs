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

        [JsonProperty("locked")]
        public bool Locked;

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
            if (MainType == stat) return MainValue;
            // Need to be able to load in null values (requiring int?) but xType shouldn't be a Type if xValue is null
            if (InnateType == stat) return InnateValue ?? 0;
            // Here, if a subs type is null, there is not further subs (it's how runes work), so we quit early.
            if (Sub1Type == stat || Sub1Type == Attr.Null) return Sub1Value ?? 0;
            if (Sub2Type == stat || Sub2Type == Attr.Null) return Sub2Value ?? 0;
            if (Sub3Type == stat || Sub3Type == Attr.Null) return Sub3Value ?? 0;
            if (Sub4Type == stat || Sub4Type == Attr.Null) return Sub4Value ?? 0;
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
    }
}
