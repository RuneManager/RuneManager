using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace RuneOptim.swar {
    public class MonsterStat : StatLoader {
#pragma warning disable IDE1006 // Naming Styles
        [JsonProperty("skills")]
        public SkillDef[] Skills;
        [JsonProperty("homunculus_skills")]
        public HomuSkill[] HomunculusSkills;

        public LeaderSkill leader_skill;

        public bool obtainable;

        [JsonProperty("is_awakened")]
        public bool Awakened;

        public int base_hp;
        public int base_attack;
        public int base_defense;

        [JsonProperty("max_lvl_hp")]
        public int Health;

        [JsonProperty("max_lvl_attack")]
        public int Attack;

        [JsonProperty("max_lvl_defense")]
        public int Defense;

        [JsonProperty("speed")]
        public int Speed;

        [JsonProperty("crit_rate")]
        public int CritRate;

        [JsonProperty("crit_damage")]
        public int CritDamage;

        [JsonProperty("resistance")]
        public int Resistance;

        [JsonProperty("accuracy")]
        public int Accuracy;

        [JsonProperty("awakens_to")]
        public StatReference AwakenTo;

        [JsonProperty("awakens_from")]
        public StatReference AwakenFrom;

        public int awaken_mats_fire_low;
        public int awaken_mats_fire_mid;
        public int awaken_mats_fire_high;
        public int awaken_mats_water_low;
        public int awaken_mats_water_mid;
        public int awaken_mats_water_high;
        public int awaken_mats_wind_low;
        public int awaken_mats_wind_mid;
        public int awaken_mats_wind_high;
        public int awaken_mats_light_low;
        public int awaken_mats_light_mid;
        public int awaken_mats_light_high;
        public int awaken_mats_dark_low;
        public int awaken_mats_dark_mid;
        public int awaken_mats_dark_high;
        public int awaken_mats_magic_low;
        public int awaken_mats_magic_mid;
        public int awaken_mats_magic_high;
#pragma warning restore IDE1006 // Naming Styles

        [JsonIgnore]
        private static List<StatLoader> monStats = null;

        [JsonIgnore]
        public static List<StatLoader> MonStats {
            get {
                if (monStats == null)
                    monStats = AskSWApi<List<StatLoader>>("https://swarfarm.com/api/bestiary");
                return monStats;
            }
        }

        public static int BaseStars(string familyName) {
            var m = MonStats.FirstOrDefault(ms => ms.Name == familyName);
            if (m != null)
                return m.Grade;
            // TODO: lookup?
            return 4; // close enough
        }

        public static StatLoader FindMon(Monster mon) {
            return FindMon(mon.MonsterTypeId) ?? FindMon(mon.Name, mon.Element.ToString());
        }

        public static StatLoader FindMon(int monsterTypeId) {
            return MonStats.FirstOrDefault(m => m.MonsterTypeId == monsterTypeId);
        }

        public static StatLoader FindMon(string name, string element = null) {
            RuneLog.Info($"searching for \"{name} ({element})\"");
            if (element == null)
                return MonStats.FirstOrDefault(m => m.Name == name);
            else
                return MonStats.FirstOrDefault(m => m.Name == name && m.Element.ToString() == element);
        }

        public Monster GetMon(Monster mon) {
            return new Monster() {
                Id = mon.Id,
                Priority = mon.Priority,
                Current = mon.Current,
                Accuracy = Accuracy,
                Attack = Attack,
                CritDamage = CritDamage,
                CritRate = CritRate,
                Defense = Defense,
                Health = Health,
                Level = 40,
                Resistance = Resistance,
                Speed = Speed,
                Element = Element,
                Name = Name,
                Downloaded = true,
                MonsterTypeId = MonsterTypeId,
                SkillList = mon.SkillList.ToList()
            };
        }
    }

    public class LeaderSkill {
#pragma warning disable IDE1006 // Naming Styles
        public string attribute;
        public int amount;
        public string area;
        public Element? element;
#pragma warning restore IDE1006 // Naming Styles
    }

    public class SkillDef {
        [JsonProperty("pk")]
        public int Pk;
        [JsonProperty("com2us_id")]
        public int Com2usId;
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("cooltime")]
        public int? Cooltime;
        [JsonProperty("hits")]
        public int? Hits;
        [JsonProperty("passive")]
        public bool Passive;
        [JsonProperty("level_progress_description")]
        public string LevelProgressDescription;
        [JsonProperty("multiplier_formula_raw")]
        public string MultiplierFormulaRaw;
        [JsonProperty("skill_effect")]
        public SkillEff[] SkillEffect;
    }

    public class SkillEff {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("is_buff")]
        public bool IsBuff;
    }

    public class HomuSkill {
        [JsonProperty("skill")]
        public SkillDef Skill;
        [JsonProperty("craft_materials")]
        public Material[] CraftMaterials;
        [JsonProperty("mana_cost")]
        public int ManaCost;
        [JsonProperty("prerequisites")]
        public int[] Prerequisites;
    }

    public class Material {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("quantity")]
        public int Quantity;
    }

    // Allows me to steal the JSON values into Enum
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Element {
        [EnumMember(Value = "Pure")]
        Pure = 0,

        [EnumMember(Value = "Water")]
        Water = 1,

        [EnumMember(Value = "Fire")]
        Fire = 2,

        [EnumMember(Value = "Wind")]
        Wind = 3,

        [EnumMember(Value = "Light")]
        Light = 4,

        [EnumMember(Value = "Dark")]
        Dark = 5,

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Archetype {
        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Attack")]
        Attack = 1,

        [EnumMember(Value = "HP")]
        HP = 2,

        [EnumMember(Value = "Defense")]
        Defense = 3,

        [EnumMember(Value = "Support")]
        Support = 4,

        [EnumMember(Value = "Material")]
        Material = 5,

    }

    public class StatReference {
        [JsonProperty("url")]
        public string URL;

        [JsonProperty("pk")]
        public int Pk;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("element")]
        public Element Element;

        readonly static Dictionary<string, object> apiObjs = new Dictionary<string, object>();
        readonly static object objLock = new object();

        readonly static object lockSlow = new object();
        private static int backOff = 100;
        private static int baseOff = 10;
        static int goodHits = 0;

        public static T AskSWApi<T>(string location, bool refetch = false) {
            var fpath = location.Replace("https://swarfarm.com/api", "swf_api_cache").Replace("?", "_") + ".json";
            var data = "";
            bool good = false;
            lock (objLock) {
                if (apiObjs.ContainsKey(location)) {
                    return (T)apiObjs[location];
                }
            }
            if (File.Exists(fpath) && new FileInfo(fpath).LastWriteTime < DateTime.Now.AddDays(-30)) {
                //File.Delete(fpath);
                // wait a second for the filesystem to actual refresh the new files Creation Time
                // recreating too fast doesn't reset it >:|
                //System.Threading.Thread.Sleep(1000);
                refetch = true;
            }
            if (!File.Exists(fpath) || refetch) {
                Directory.CreateDirectory(new FileInfo(fpath).Directory.FullName);
                using (WebClient client = new WebClient()) {
                    
                    do {
                        lock (lockSlow) {
                            if (baseOff > 10)
                                Debug.WriteLine("Rate limit " + baseOff + "    " + fpath);
                            System.Threading.Thread.Sleep(baseOff);
                        }

                        try {
                            client.Encoding = Encoding.UTF8;
                            client.Headers["accept"] = "application/json";
                            data = Encoding.UTF8.GetString(client.DownloadData(location));
                            if (backOff >= 150)
                                backOff -= 50;
                            good = !data.Contains("<!DOCTYPE html>");
                            if (!good)
                                baseOff += 5;
                        }
                        catch (WebException wex) when (wex.Response is HttpWebResponse exr) {
                            if (exr.StatusCode == (HttpStatusCode)429) {
                                baseOff += 10;
                                lock (lockSlow) {

                                    int waitTime = backOff;
                                    if (exr.Headers.AllKeys.Contains("Retry-After") && exr.Headers.AllKeys.Contains("Date")) {
                                        var retryAt = DateTime.Parse(exr.Headers["Date"]).AddSeconds(int.Parse(exr.Headers["Retry-After"]));
                                        var delta = retryAt - DateTime.Now;
                                        Debug.WriteLine("Time to backoff " + delta.TotalMilliseconds + "    " + fpath);
                                        waitTime = (int)Math.Max(0, delta.TotalMilliseconds);
                                    }
                                    else {
                                        Debug.WriteLine("Time to backoff " + backOff + "    " + fpath);
                                    }

                                    System.Threading.Thread.Sleep(waitTime + baseOff);
                                    backOff += 150;
                                }
                            }

                        }
                    } while (!good);

                    goodHits++;

                    if (goodHits > 10 && baseOff > 10) {
                        baseOff--;
                        goodHits = 0;
                    }

                }
            }
            else {
                data = File.ReadAllText(fpath, Encoding.UTF8);
            }
            if (string.IsNullOrWhiteSpace(data))
                return default(T);
            lock (objLock) {
                apiObjs.Add(location, JsonConvert.DeserializeObject<T>(data));
                if (good)
                    File.WriteAllText(fpath, data, Encoding.UTF8);
                return (T)apiObjs[location];
            }
        }

        public MonsterStat Download() {
            return AskSWApi<MonsterStat>(URL);
        }

        public override string ToString() {
            return Name + " (" + Element + ")";
        }
    }

    public class StatLoader : StatReference {
        [JsonProperty("image_filename")]
        public string ImageFileName;

        [JsonProperty("archetype")]
        public Archetype Archetype;

        [JsonProperty("base_stars")]
        public int Grade;

        [JsonProperty("fusion_food")]
        public bool IsFusion;

        [JsonProperty("com2us_id")]
        public int MonsterTypeId;
    }
}
