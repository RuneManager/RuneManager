using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim.swar;

namespace RunePlugin.Response {
    public class Gauge {
        public int original;
        public int bonus;
    }

    [SWCommand(SWCommand.UpdateUnitExpGained)]
    public class GenericUnitListResponse : SWResponse {
        [JsonProperty("unit_list")]
        public Monster[] Monsters;
    }


    [SWCommand(SWCommand.SacrificeUnit)]
    public class SacrificeUnitResponse : SWResponse {
        [JsonProperty("gauge")]
        public Gauge Gauge;

        [JsonProperty("target_unit")]
        public Monster Target;
    }

    [SWCommand(SWCommand.SummonUnit)]
    public class SummonUnitResponse : GenericUnitListResponse {
        [JsonProperty("item_info")]
        public InventoryItem RemainingItem;
    }
    
    [SWCommand(SWCommand.UpgradeUnit)]
    public class UpgradeUnitResponse : SWResponse {
        [JsonProperty("unit_info")]
        public Monster Target;
    }

}
