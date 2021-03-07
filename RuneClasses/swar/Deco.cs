using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RuneOptim.swar {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Shrine {
        [EnumMember(Value = "Unknown")]
        Unknown = -1,
        [EnumMember(Value = "")]
        Null = 0,

        [EnumMember(Value = "DEF")]
        DEF = 4,
        [EnumMember(Value = "SPD")]
        SPD = 6,
        [EnumMember(Value = "HP")]
        HP = 8,
        [EnumMember(Value = "ATK")]
        ATK = 9,

        [EnumMember(Value = "FireATK")]
        FireATK = 15,
        [EnumMember(Value = "WaterATK")]
        WaterATK = 16,
        [EnumMember(Value = "WindATK")]
        WindATK = 17,
        [EnumMember(Value = "LightATK")]
        LightATK = 18,
        [EnumMember(Value = "DarkATK")]
        DarkATK = 19,

        [EnumMember(Value = "CD")]
        CD = 31,

    }

    public class Deco {
        public readonly static string[] ShrineStats = new string[] { "SPD", "DEF", "ATK", "HP", "WaterATK", "FireATK", "WindATK", "LightATK", "DarkATK", "CD" };
        public readonly static double[] ShrineLevel = new double[] { 1.5, 2, 2, 2, 2, 2, 2, 2, 2, 2.5 };

        [JsonProperty("pos_x")]
        public int X = 0;

        [JsonProperty("pos_y")]
        public int Y = 0;

        [JsonProperty("deco_id")]
        public int ID;

        [JsonProperty("level")]
        public int Level;

        [JsonProperty("wizard_id")]
        public int Owner;

        [JsonProperty("island_id")]
        public int Island;

        [JsonProperty("master_id")]
        public int MasterId;

        [JsonIgnore]
        public Shrine Shrine {
            get {
                switch (MasterId) {
                    case 4:
                    case 6:
                    case 8:
                    case 9:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 31:
                        return (Shrine)MasterId;
                    default:
                        return Shrine.Unknown;
                }
            }
        }

        public override string ToString() {
            return "Lvl. " + Level + " " + Shrine;
        }
    }
}
