using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RunePlugin;

namespace SwarfarmExportPlugin
{
    // Conversion of:
    // https://github.com/kakaroto/SWProxy/blob/master/plugins/SwarfarmExportPlugin.py
    public class SwarfarmExportPlugin : SWPlugin
    {
        public override void ProcessRequest(object sender, SWEventArgs args)
        {
            if (((string)args.ResponseJson["command"]).Equals("HubUserLogin"))
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                data["inventory_info"] = args.ResponseJson["inventory_info"];
                data["unit_list"] = args.ResponseJson["unit_list"];
                data["runes"] = args.ResponseJson["runes"];
                data["building_list"] = args.ResponseJson["building_list"];
                data["deco_list"] = args.ResponseJson["deco_list"];
                data["wizard_info"] = args.ResponseJson["wizard_info"];
                data["unit_lock_list"] = args.ResponseJson["unit_lock_list"];
                data["helper_list"] = args.ResponseJson["helper_list"];
                data["rune_craft_item_list"] = args.ResponseJson["rune_craft_item_list"];
                data["defense_unit_list"] = args.ResponseJson["defense_unit_list"];
                data["guildwar_defense_unit_list"] = args.ResponseJson["guildwar_defense_unit_list"];

                File.WriteAllText($"{(int)args.ResponseJson["wizard_info"]["wizard_id"]}-swarfarm.json", JsonConvert.SerializeObject(data));
                Console.WriteLine("Generated swarfarm.json");
            }
        }
    }
}
