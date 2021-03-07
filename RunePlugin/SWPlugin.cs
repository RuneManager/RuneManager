using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RuneOptim.swar;

namespace RunePlugin {
    public abstract class SWPlugin
    {
        public string PluginDataDirectory {
            get {
                if (!Directory.Exists(Environment.CurrentDirectory + "\\Plugins\\" + this.GetType().Name))
                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\Plugins\\" + this.GetType().Name);
                return Environment.CurrentDirectory + "\\Plugins\\" + this.GetType().Name;
            }
        }

        public virtual void OnLoad() { }

        public virtual void OnUnload() { }
        
        public virtual ConsoleColor FavouriteColour { get { return ConsoleColor.Gray; } }

        public abstract void ProcessRequest(object sender, SWEventArgs args);

        public static JObject MapCraft(JObject craft, int craft_id)
        {
            string type_str = ((string)craft["craft_type_id"]).PadLeft(6, '0'); //100802
            return new JObject
            {
                { "id", craft_id },
                { "item_id", craft["craft_item_id"] },
                {"type", ((int)craft["craft_type"] == 1) ? "E" : "G" },
                {"set", RuneSetId(int.Parse(type_str.Substring(0,2))) },
                {"stat", RuneEffectType(int.Parse(type_str.Substring(2,2))) },
                {"grade", int.Parse(type_str.Substring(4)) }
            };
        }

        public static string RuneEffectType(int id)
        {
            var memInfo = typeof(Attr).GetMember(((Attr)id).ToString());
            var attr = memInfo[0].GetCustomAttributes(false).OfType<System.Runtime.Serialization.EnumMemberAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Value;
            }
            
            return Enum.GetName(typeof(Attr), id);
        }

        public static JToken RuneSetId(int id)
        {
            return Enum.GetName(typeof(RuneOptim.swar.RuneSet), id);
        }

        public static JObject MapMonster(JObject monster, Dictionary<long, long> monster_id_mapping, long? storage_id)
        {
            JObject optimizer_monster = null;
            if (monster_id_mapping != null)
            {
                optimizer_monster = new JObject()
                {
                    { "id", monster_id_mapping[(long)monster["unit_id"]] },
                    { "name", $"{MonsterName((long)monster["unit_master_id"], "Unknown name")}{(((long)monster["building_id"] == storage_id) ? " (In Storage)" : "")}" },
                    { "level", monster["unit_level"] },
                    { "unit_id", monster["unit_id"] },
                    { "master_id", monster["unit_master_id"] },
                    { "stars", monster["class"] },
                    { "attribute", MonsterAttribute((int)monster["attribute"])},
                    { "b_hp", 15 * (long)monster["con"] },
                    { "b_atk", monster["atk"] },
                    { "b_def", monster["def"] },
                    { "b_spd", monster["spd"] },
                    { "b_crate", monster["critical_rate"] },
                    { "b_cdmg", monster["critical_damage"] },
                    { "b_res", monster["resist"] },
                    { "b_acc", monster["accuracy"] },
            };
            }
            return optimizer_monster;
        }

        private static string MonsterAttribute(int id)
        {
            var memInfo = typeof(Element).GetMember(((Element)id).ToString());
            var attr = memInfo[0].GetCustomAttributes(false).OfType<System.Runtime.Serialization.EnumMemberAttribute>().FirstOrDefault();
            string name = "";
            if (attr != null)
            {
                name = attr.Value;
            }

            if (name.Length > 3)
                return name;
            return $"???[{id}]";
        }

        public static JObject MapRune(JObject rune, long rune_id, long monster_id = 0, long monster_uid = 0)
        {
            Dictionary<string, long?> subs = new Dictionary<string, long?>()
            {
                { "ATK flat", null },
                { "ATK%", null },
                { "DEF flat", null },
                { "DEF%", null },
                { "HP flat", null },
                { "HP%", null },
                { "SPD", null },
                { "CRate", null },
                { "CDam", null },
                { "RES", null },
                { "ACC", null },
            };

            foreach (JArray sec_eff in rune["sec_eff"].OfType<JArray>())
            {
                subs[RuneEffectType((int)sec_eff[0])] = (int)sec_eff[1] + ((sec_eff.Count > 2) ? (int)sec_eff[3] : 0);
            }

            JObject optimizer_map = new JObject
            {
                { "id", rune_id },
                { "unique_id", rune["rune_id"] },
                { "monster", monster_id },
                { "monster_n", MonsterName(monster_uid, "Unknown name") },
                { "set", RuneSetId((int)rune["set_id"]) },
                { "slot", rune["slot_no"] },
                { "grade", rune["class"] },
                { "level", rune["upgrade_curr"] },
                { "m_t", RuneEffectType((int)(rune["pri_eff"][0])) },
                { "m_v", rune["pri_eff"][1] },
                { "i_t", RuneEffectType((int)(rune["prefix_eff"][0])) },
                { "i_v", rune["prefix_eff"][1] },
                { "locked", 0 },
                { "sub_atkf", subs["ATK flat"] },
                { "sub_atkp", subs["ATK%"] },
                { "sub_deff", subs["DEF flat"] },
                { "sub_defp", subs["DEF%"] },
                { "sub_hpf", subs["HP flat"] },
                { "sub_hpp", subs["HP%"] },
                { "sub_spd", subs["SPD"] },
                { "sub_crate", subs["CRate"] },
                { "sub_cdam", subs["CDam"] },
                { "sub_res", subs["RES"] },
                { "sub_acc", subs["ACC"] },
            };

            foreach (var prop in new string[] { "atkf", "atkp", "deff", "defp", "hpf", "hpp", "spd", "crate", "cdam", "res", "acc" })
            {
                if ((int?)optimizer_map[$"sub_{prop}"] == null)
                    optimizer_map[$"sub_{prop}"] = "-";
            }

            for (int i = 0; i < 4; i++)
            {
                optimizer_map[$"s{i + 1}_t"] = (((JArray)rune["sec_eff"]).Count >= (i + 1)) ? RuneEffectType((int)(rune["sec_eff"][i][0])) : "";

                int s_v = 0;
                if (((JArray)rune["sec_eff"]).Count >= i + 1)
                {
                    s_v = (int)rune["sec_eff"][i][1];
                    if (((JArray)rune["sec_eff"][i]).Count > 2)
                        s_v += (int)rune["sec_eff"][i][3];
                }
                optimizer_map[$"s{i + 1}_v"] = s_v;

                JObject enchanted = new JObject{ };

                if (((JArray)rune["sec_eff"]).Count >= i+1 && ((JArray)rune["sec_eff"][i]).Count > 2)
                {
                    enchanted["gvalue"] = rune["sec_eff"][i][3];
                    enchanted["enchanted"] = ((int)rune["sec_eff"][i][2]) == 1;
                }

                optimizer_map[$"s{i + 1}_data"] = enchanted;
            }

            return optimizer_map;
        }

        public static string MonsterName(long uid, string default_unknown = "???", bool full = true)
        {
            try
            {
                string suid = uid.ToString().PadRight(5, '0');
                if (default_unknown == "???")
                    default_unknown += $"[{suid.Substring(0, suid.Length - 2)}]";

                if (SWReference.MonsterNameMap == null)
                    return "MonsterNameMap is null";

                if (SWReference.MonsterNameMap.ContainsKey(suid) && SWReference.MonsterNameMap[suid].Length > 0)
                {
                    return SWReference.MonsterNameMap[suid];
                }

                var awakened = int.Parse(suid.Substring(3, 1)) > 0;
                string name = default_unknown;

                if (SWReference.MonsterNameMap.ContainsKey(suid.Substring(0, 3)) && SWReference.MonsterNameMap[suid.Substring(0, 3)].Length > 0)
                {
                    name = SWReference.MonsterNameMap[suid.Substring(0, 3)];
                }

                if (full)
                {
                    var attribute = int.Parse("" + suid.Last());
                    return $"{(awakened ? "AWAKENED" : "")}{name} ({MonsterAttribute(attribute)})";
                }
                else if (!awakened)
                {
                    return name;
                }

                return default_unknown;
            }
            catch (Exception e)
            {
                return uid + " failed with " + e.GetType();
            }
        }
    }
}
