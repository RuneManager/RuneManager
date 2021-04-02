using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuneOptim.swar
{
    public class Guild
    {
        [JsonProperty("price")]
        public int Price;

        [JsonProperty("guild_info")]
        public GuildInfo GuildInfo;

    }

    public class GuildInfo
    {
        [JsonProperty("guild_id")]
        public ulong Id;


        [JsonProperty("skill_info")]
        public Dictionary<string, GuildSkill> Skills;

        [JsonIgnore]
        public GuildSkill Dungeon => Skills["1001"];

        [JsonIgnore]
        public GuildSkill RaidBeast => Skills["2001"];

        [JsonIgnore]
        public GuildSkill Labyrinth => Skills["3001"];

        [JsonIgnore]
        public GuildSkill RiftRaid => Skills["4001"];

    }

    [JsonConverter(typeof(GuildSkillConverter))]
    public class GuildSkill
    {
        public bool HasStats;

        public long Amount;

        public long Hp;
        public long Def;
        public long Atk;
    }


    public class GuildSkillConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is GuildSkill gs)
            {
                if (gs.HasStats)
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("1");
                    writer.WriteValue(gs.Hp);
                    writer.WritePropertyName("2");
                    writer.WriteValue(gs.Def);
                    writer.WritePropertyName("3");
                    writer.WriteValue(gs.Atk);

                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStartArray();
                    writer.WriteValue(gs.Amount);
                    writer.WriteEndArray();

                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            GuildSkill gs = new GuildSkill();
            object v;
            switch (reader.TokenType)
            {
                case JsonToken.StartArray:
                    {
                        reader.Read();
                        if (reader.TokenType == JsonToken.EndArray)
                            break;
                        gs.Amount = (long)reader.Value;
                        reader.Read();
                        break;
                    }
                case JsonToken.StartObject:
                    {
                        gs.HasStats = true;
                        reader.Read(); // propName
                        if (reader.TokenType == JsonToken.EndObject)
                            break;
                        reader.Read(); // value
                        gs.Hp = (long)reader.Value;

                        reader.Read(); // propName
                        if (reader.TokenType == JsonToken.EndObject)
                            break;
                        reader.Read(); // value
                        gs.Def = (long)reader.Value;

                        reader.Read(); // propName
                        if (reader.TokenType == JsonToken.EndObject)
                            break;
                        reader.Read(); // value
                        gs.Atk = (long)reader.Value;
                        reader.Read();
                        break;
                    }
                default:
                    {
                        throw new TypeLoadException("I don't like " + reader.TokenType);
                    }
            }
            return gs;
        }
        
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GuildSkill);
        }

    }

}
