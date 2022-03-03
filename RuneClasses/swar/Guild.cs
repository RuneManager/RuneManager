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
        public GuildInfo GuildInfo = new GuildInfo();

        public Guild() { }

        // copy constructor, amrite?
        public Guild(Guild rhs, bool copyExtra = false)
        {
            CopyFrom(rhs, copyExtra);
        }

        public void CopyFrom(Guild rhs, bool copyExtra = false)
        {
            Price = rhs.Price;
            GuildInfo = new GuildInfo(rhs.GuildInfo);

        }

        [JsonIgnore]
        public long Health => GuildInfo.Health;
        public long Defense => GuildInfo.Defense;
        public long Attack => GuildInfo.Attack;
        public long Speed => GuildInfo.Speed;
        public long CritRate => GuildInfo.CritRate;
        public long CritDamage => GuildInfo.CritDamage;
        public long Accuracy => GuildInfo.Accuracy;
        public long Resistance => GuildInfo.Resistance;

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

        [JsonIgnore]
        public long Health => Skills == null ? 0 : Dungeon == null ? 0 : Dungeon.Hp;
        public long Defense => Skills == null ? 0 : Dungeon == null ? 0 : Dungeon.Def;
        public long Attack => Skills == null ? 0 : Dungeon == null ? 0 : Dungeon.Atk;
        public long Speed = 0;
        public long CritRate = 0;
        public long CritDamage = 0;
        public long Accuracy = 0;
        public long Resistance = 0;

        public GuildInfo() { }

        // copy constructor, amrite?
        public GuildInfo(GuildInfo rhs, bool copyExtra = false)
        {
            CopyFrom(rhs, copyExtra);
        }

        public void CopyFrom(GuildInfo rhs, bool copyExtra = false)
        {
            Id = rhs.Id;
            Skills = rhs.Skills.ToDictionary(entry => entry.Key,
                                             entry => new GuildSkill(entry.Value));
        }

    }

    [JsonConverter(typeof(GuildSkillConverter))]
    public class GuildSkill
    {
        public bool HasStats;

        public long Amount;

        public long Hp;
        public long Def;
        public long Atk;

        public GuildSkill() { }

        // copy constructor, amrite?
        public GuildSkill(GuildSkill rhs, bool copyExtra = false)
        {
            CopyFrom(rhs, copyExtra);
        }

        public void CopyFrom(GuildSkill rhs, bool copyExtra = false)
        {
            HasStats = rhs.HasStats;
            Amount = rhs.Amount;
            Hp = rhs.Hp;
            Def = rhs.Def;
            Atk = rhs.Atk;
        }
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
