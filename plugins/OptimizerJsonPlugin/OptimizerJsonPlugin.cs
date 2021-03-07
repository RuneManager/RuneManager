using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin;

namespace OptimizerJsonPlugin
{
    class Optimizer
    {
        public List<object> runes = new List<object>();
        public List<object> mons = new List<object>();
        public List<object> crafts = new List<object>();
        public List<object> savedBuilds = new List<object>();
        public object tvalue;
        public long wizard_id;
        public object deco_list;
    }

    public class OptimizerJsonPlugin : SWPlugin
    {
        public override void ProcessRequest(object sender, SWEventArgs args)
        {
            if (!new string[] { "HubUserLogin", "GuestLogin" }.Contains((string)args.ResponseJson["command"]))
                return;

            JObject wizard = (JObject)args.ResponseJson["wizard_info"];
            List<JObject> monsters = ((JArray)args.ResponseJson["unit_list"]).Cast<JObject>().ToList();
            List<JObject> runes = ((JArray)args.ResponseJson["runes"]).Cast<JObject>().ToList();
            List<JObject> crafts = ((JArray)args.ResponseJson["rune_craft_item_list"]).Cast<JObject>().ToList();

            Optimizer optimizer = new Optimizer();
            optimizer.tvalue = args.Response.TValue;
            optimizer.wizard_id = (long)wizard["wizard_id"];
            optimizer.deco_list = args.ResponseJson["deco_list"];

            long? storage_id = null;
            foreach (JObject building in args.ResponseJson["building_list"])
            {
                if ((long)building["building_master_id"] == 25)
                {
                    storage_id = (long)building["building_id"];
                    break;
                }
            }

            Dictionary<long, long> monster_id_mapping = new Dictionary<long, long>();
            Dictionary<long, long> rune_id_mapping = new Dictionary<long, long>();

            int craft_id = 1;
            foreach (var craft in crafts
                .OrderBy(c => (long)c["craft_type"])
                .ThenBy(c => (long)c["craft_item_id"]))
            {
                optimizer.crafts.Add(SWPlugin.MapCraft(craft, craft_id));
                craft_id++;
            }

            int rune_id = 1;

            foreach (var rune in runes
                .OrderBy(r => (long)r["set_id"])
                .ThenBy(r => (long)r["slot_no"]))
            {
                rune_id_mapping[(long)rune["rune_id"]] = rune_id;
                rune_id++;
            }

            int monster_id = 1;
            foreach (var monster in monsters
                .OrderBy(m => (m["building_id"].Equals(storage_id) ? 1 : 0))
                .ThenBy(m => 6 - (long)m["class"])
                .ThenBy(m => 40 - (long)m["unit_level"])
                .ThenBy(m => (long)m["attribute"])
                .ThenBy(m => 1 - ((((long)m["unit_master_id"])/10)%10))
                .ThenBy(m => (long)m["unit_id"]))
            {
                monster_id_mapping[(long)monster["unit_id"]] = monster_id;
                monster_id++;

                JEnumerable<JToken> monster_runes;
                var rr = monster["runes"];

                JObject jo = rr as JObject;
                if (jo != null)
                    monster_runes = (JEnumerable<JToken>)jo.Values();
                else
                    monster_runes = rr.Children();

                foreach (var rune in monster_runes
                    .OrderBy(r => (long)r["slot_no"]))
                {
                    rune_id_mapping[(long)rune["rune_id"]] = rune_id;
                    rune_id++;
                }
            }

            foreach (var rune in runes)
            {
                var optimizer_rune = MapRune(rune, rune_id_mapping[(long)rune["rune_id"]]);

                optimizer.runes.Add(optimizer_rune);
            }

            foreach (var monster in monsters)
            {
                var optimizer_monster = MapMonster(monster, monster_id_mapping, storage_id);

                optimizer.mons.Add(optimizer_monster);

                JEnumerable<JToken> monster_runes;
                var rr = monster["runes"];

                JObject jo = rr as JObject;
                if (jo != null)
                    monster_runes = (JEnumerable<JToken>)jo.Values();
                else
                    monster_runes = rr.Children();
                
                foreach (var rune in monster_runes)
                {
                    var optimizer_rune = MapRune((JObject)rune, rune_id_mapping[(long)rune["rune_id"]], monster_id_mapping[(long)monster["unit_id"]], (long)monster["unit_master_id"]);

                    optimizer_rune["monster_n"] = $"{MonsterName((long)monster["unit_master_id"], "Unknown name")}{(((long)monster["building_id"] == storage_id) ? " (In Storage)" : "")}";

                    optimizer.runes.Add(optimizer_rune);
                }
            }

            File.WriteAllText($"{(int)wizard["wizard_id"]}-optimizer.json", JsonConvert.SerializeObject(optimizer, Formatting.Indented));
            Console.WriteLine("Generated optimizer.json");
        }
    }
    
}
