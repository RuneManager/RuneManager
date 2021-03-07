using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using RuneOptim.BuildProcessing;
using System.Diagnostics;

namespace RuneOptim.swar {


    public class RuneSetConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var date = (RuneSet)value;

            var i = Enum.GetValues(typeof(RuneSet)).OfType<RuneSet>().ToList().IndexOf(date) - 2;

            writer.WriteValue(i.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.Value is int i) {
                return (RuneSet)(1 << (i - 1));
            }
            else if (reader.Value is Int64 l) {
                return (RuneSet)(1 << (int)(l - 1));
            }
            else {
                throw new Exception();
            }

        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(RuneSet);
        }

    }

    [DebuggerTypeProxy(typeof(RuneProxy))]
    public partial class Rune : RuneLink {

        #region JSON Props

        [JsonProperty("set_id")]
        [JsonConverter(typeof(RuneSetConverter))]
        public RuneSet Set;

        [JsonProperty("class")]
        public int Grade;
        
        /// <summary>
        /// The 1-index slot number
        /// </summary>
        [JsonProperty("slot_no")]
        public int Slot;

        [JsonProperty("upgrade_curr")]
        public int Level;

        [JsonProperty("rank")]
        public int _rank;

        [JsonProperty("locked")]
        protected bool locked;

        [JsonIgnore]
        public bool Locked {
            get {
                return locked;
            }
            set {
                locked = value;
                if (EnchantOf != null)
                    EnchantOf.locked = value;
            }
        }

        [JsonIgnore]
        public Rune EnchantOf;

        [JsonProperty("occupied_type")]
        public int _occupiedType;

        [JsonProperty("sell_value")]
        public int SellValue;

        [JsonProperty("monster_n")]
        public string AssignedName;

        [JsonProperty("pri_eff")]
        public RuneAttr Main;

        [JsonProperty("prefix_eff")]
        public RuneAttr Innate;

        [JsonProperty("sec_eff")]
        public List<RuneAttr> Subs;

        [JsonProperty("wizard_id")]
        public ulong WizardId = 0;

        [JsonProperty("extra")]
        public int _extra;

        #endregion

        // this stuff makes the build generation run faster, trust
        #region Nicer getters for stats by type

        [JsonIgnore]
        public int[] HealthFlat = new int[32];

        [JsonIgnore]
        public int[] HealthPercent = new int[32];

        [JsonIgnore]
        public int[] AttackFlat = new int[32];

        [JsonIgnore]
        public int[] AttackPercent = new int[32];

        [JsonIgnore]
        public int[] DefenseFlat = new int[32];

        [JsonIgnore]
        public int[] DefensePercent = new int[32];

        [JsonIgnore]
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public int[] Speed = new int[32];

        [JsonIgnore]
        public int[] SpeedPercent = new int[32];

        [JsonIgnore]
        public int[] CritRate = new int[32];

        [JsonIgnore]
        public int[] CritDamage = new int[32];

        [JsonIgnore]
        public int[] Accuracy = new int[32];

        [JsonIgnore]
        public int[] Resistance = new int[32];

        #endregion

        public Rune() {
            Main = new RuneAttr();
            Innate = new RuneAttr();
            Main.OnChanged += (a, b) => { PrebuildAttributes(); };
            Innate.OnChanged += (a, b) => { PrebuildAttributes(); };
            Subs = new List<RuneAttr>();
        }

        public void CopyTo(Rune rhs, bool keepLocked, Monster newAssigned) {
            // TODO
            rhs.Freeze();

            rhs.Set = Set;
            rhs.Grade = Grade;
            rhs.Slot = Slot;
            rhs.Level = Level;
            rhs._rank = _rank;
            if (!keepLocked)
                rhs.Locked = Locked;
            rhs._occupiedType = _occupiedType;
            rhs.SellValue = SellValue;

            Main.CopyTo(ref rhs.Main);
            //rhs.Main.CopyFrom(Main);
            Innate.CopyTo(ref rhs.Innate);
            while (rhs.Subs.Count < Subs.Count)
                rhs.Subs.Add(new RuneAttr());
            for (int i = 0; i < Subs.Count; i++) {
                rhs.Subs[i].CopyFrom(Subs[i]);
            }

            rhs.Unfreeze();
            rhs.PrebuildAttributes();

            if (rhs.AssignedId != AssignedId) {
                if (AssignedId == 0) {
                    rhs.AssignedName = "Inventory";
                    rhs.Assigned = null;
                }
                else if (rhs.Assigned != null && newAssigned != null) {
                    if (rhs.Assigned.Current.Runes[rhs.Slot - 1] == rhs)
                        rhs.Assigned.Current.RemoveRune(rhs.Slot - 1);
                    newAssigned.Current.AddRune(rhs);
                    rhs.Assigned = newAssigned;
                }
            }

            if (rhs.Assigned != null) {
                rhs.Assigned.RefreshStats();
            }
        }

        // fast iterate over rune stat types
        public int this[string stat, int fake, bool pred] {
            get {
                int ind = pred ? fake + 16 : fake;
                switch (stat) {
                    case "HPflat":
                        return HealthFlat[ind];
                    case "HPperc":
                        return HealthPercent[ind];
                    case "ATKflat":
                        return AttackFlat[ind];
                    case "ATKperc":
                        return AttackPercent[ind];
                    case "DEFflat":
                        return DefenseFlat[ind];
                    case "DEFperc":
                        return DefensePercent[ind];
                    case "SPDflat":
                        return Speed[ind];
                    case "CDperc":
                        return CritDamage[ind];
                    case "CRperc":
                        return CritRate[ind];
                    case "ACCperc":
                        return Accuracy[ind];
                    case "RESperc":
                        return Resistance[ind];
                    default:
                        return 0;
                }
            }
        }

        // fast iterate over rune stat types
        public int this[Attr stat, int fake, bool pred] {
            get {
                int ind = pred ? fake + 16 : fake;
                switch (stat) {
                    case Attr.HealthFlat:
                        return HealthFlat[ind];
                    case Attr.HealthPercent:
                        return HealthPercent[ind];
                    case Attr.AttackFlat:
                        return AttackFlat[ind];
                    case Attr.AttackPercent:
                        return AttackPercent[ind];
                    case Attr.DefenseFlat:
                        return DefenseFlat[ind];
                    case Attr.DefensePercent:
                        return DefensePercent[ind];
                    case Attr.Speed:
                    case Attr.SpeedPercent:
                        return Speed[ind];
                    case Attr.CritDamage:
                        return CritDamage[ind];
                    case Attr.CritRate:
                        return CritRate[ind];
                    case Attr.Accuracy:
                        return Accuracy[ind];
                    case Attr.Resistance:
                        return Resistance[ind];
                }
                return 0;
            }
        }

        [JsonIgnore]
        public bool IsUnassigned {
            get {
                if (this.AssignedId == 0) return true;
                return new string[] { "Unknown name", "Inventory", "Unknown name (???[0])" }.Any(s => s.Equals(this.AssignedName));
            }
        }

        // Number of sets
        public static readonly int SetCount = Enum.GetNames(typeof(RuneSet)).Length;

        // todo: consider hashSet.Contains
        // Number of runes required for set to be complete
        public static int SetRequired(RuneSet set) {
            if ((set & RuneSet.Set4) == set)
                return 4;
            // Not a 4 set => is a 2 set
            return 2;
        }

        // Format rune values okayish
        public string StringIt() {
            return StringIt(Main.Type, Main.Value);
        }

        public static string StringIt(Attr type, int? val) {
            if (type <= Attr.Null || !val.HasValue)
                return "";
            return StringIt(type, val.Value);
        }

        public static string StringIt(Attr type, int val) {
            if (type <= Attr.Null)
                return "";

            string ret = StringIt(type);

            ret += " +" + val;

            if (type.ToString().Contains("Percent") || type == Attr.CritRate || type == Attr.CritDamage || type == Attr.Accuracy || type == Attr.Resistance) {
                ret += "%";
            }

            return ret;
        }

        public static string StringIt(List<RuneAttr> subs, int v) {
            if (subs.Count > v)
                return StringIt(subs[v].Type, subs[v].Value) + (subs[v].IsEnchanted ? "↺" : "");
            return "";
        }

        // Ask the rune for the value of the Attribute type as a string
        public static string StringIt(Attr type, bool suffix = false) {
            string ret = "";

            switch (type) {
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

        // Debugger niceness
        public override string ToString() {
            return Grade + "* " + Set + " " + StringIt();
        }

        // Gets the value of that Attribute on this rune
        public int GetValue(Attr stat, int FakeLevel = -1, bool PredictSubs = false) {
            if (Main == null) return -1;
            // the stat can only be present once per rune, early exit
            if (Main.Type == stat && FakeLevel <= 15 && FakeLevel > Level)
                return RuneProperties.MainValues[Main.Type][Grade - 1][FakeLevel];
            else if (Main.Type == stat)
                return Main.Value;

            // Need to be able to load in null values (requiring int?) but xType shouldn't be a Type if xValue is null
            if (Innate.Type == stat) return Innate.Value;
            // Here, if a subs type is null, there is not further subs (it's how runes work), so we quit early.
            if (!PredictSubs) {
                if (Subs.Count > 0 && (Subs[0].Type == stat || Subs[0].Type <= Attr.Null)) return Subs[0].Value;
                else if (Subs.Count > 1 && (Subs[1].Type == stat || Subs[1].Type <= Attr.Null)) return Subs[1].Value;
                else if (Subs.Count > 2 && (Subs[2].Type == stat || Subs[2].Type <= Attr.Null)) return Subs[2].Value;
                else if (Subs.Count > 3 && (Subs[3].Type == stat || Subs[3].Type <= Attr.Null)) return Subs[3].Value;
            }
            else {
                // count how many upgrades have gone into the rune
                int maxUpgrades = Math.Min(Rarity, Math.Max(Level, FakeLevel) / 3);
                int upgradesGone = Math.Min(4, Level / 3);
                // how many new sub are to appear (0 legend will be 4 - 4 = 0, 6 rare will be 4 - 3 = 1, 6 magic will be 4 - 2 = 2)
                int subNew = 4 - Rarity;
                // how many subs will go into existing stats (0 legend will be 4 - 0 - 0 = 4, 6 rare will be 4 - 1 - 2 = 1, 6 magic will be 4 - 2 - 2 = 0)
                int subEx = maxUpgrades - upgradesGone;// - subNew;
                int subVal = subNew * RuneProperties.MainValues[stat][this.Grade - 1][0] / 8;

                // TODO: sub prediction
                if (Subs.Count == 0) return subVal;
                if (Subs[0].Type == stat) return Subs[0].Value + subEx * (RuneProperties.MainValues[stat][this.Grade - 1][0] / Subs.Count);
                if (Subs.Count == 1) return subVal;
                if (Subs[1].Type == stat) return Subs[1].Value + subEx * (RuneProperties.MainValues[stat][this.Grade - 1][0] / Subs.Count);
                if (Subs.Count == 2) return subVal;
                if (Subs[2].Type == stat) return Subs[2].Value + subEx * (RuneProperties.MainValues[stat][this.Grade - 1][0] / Subs.Count);
                if (Subs.Count == 3) return subVal;
                if (Subs[3].Type == stat) return Subs[3].Value + subEx * (RuneProperties.MainValues[stat][this.Grade - 1][0] / Subs.Count);
                return subVal;
            }

            return 0;
        }

        // Does it have this stat at all?
        // TODO: should I listen to fake/pred?
        public bool HasStat(Attr stat, int fake = -1, bool pred = false) {
            if (GetValue(stat, fake, pred) > 0)
                return true;
            return false;
        }

        public void PrebuildAttributes() {
            Accuracy = PrebuildAttribute(Attr.Accuracy);
            AttackFlat = PrebuildAttribute(Attr.AttackFlat);
            AttackPercent = PrebuildAttribute(Attr.AttackPercent);
            CritDamage = PrebuildAttribute(Attr.CritDamage);
            CritRate = PrebuildAttribute(Attr.CritRate);
            DefenseFlat = PrebuildAttribute(Attr.DefenseFlat);
            DefensePercent = PrebuildAttribute(Attr.DefensePercent);
            HealthFlat = PrebuildAttribute(Attr.HealthFlat);
            HealthPercent = PrebuildAttribute(Attr.HealthPercent);
            Resistance = PrebuildAttribute(Attr.Resistance);
            Speed = PrebuildAttribute(Attr.Speed);
            OnUpdate?.Invoke(this, EventArgs.Empty);
        }

        private int[] PrebuildAttribute(Attr a) {
            int[] vs = new int[32];
            for (int i = 0; i < 16; i++) {
                vs[i] = GetValue(a, i, false);
                vs[i + 16] = GetValue(a, i, true);
            }
            return vs;
        }

    }
}
