using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RuneOptim
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CraftType
    {
        [EnumMember(Value = "")]
        Null = 0,

        [EnumMember(Value = "E")]
        Enchant = 1,

        [EnumMember(Value = "G")]
        Grind = 2,
    }

    public class Craft
    {
        [JsonProperty("stat")]
        public Attr Stat;

        [JsonProperty("set")]
        public RuneSet Set;

        [JsonProperty("grade")]
        public int grade;

        [JsonProperty("item_id")]
        public long ItemID;

        [JsonProperty("type")]
        public CraftType Type;

        [JsonProperty("id")]
        public long ID;

    }
}
