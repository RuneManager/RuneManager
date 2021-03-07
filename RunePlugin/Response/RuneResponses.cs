using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim.swar;

namespace RunePlugin.Response {
    [SWCommand(SWCommand.UpgradeRune)]
    [SWCommand(SWCommand.RevalueRune)]
    [SWCommand(SWCommand.ConfirmRune)]
    public class GenericRuneResponse : SWResponse {
        [JsonProperty("rune")]
        public Rune Rune;
    }

    public class GenericSellResponse : SWResponse {
        [JsonProperty("sell_mana")]
        public int SellMana;
    }

    [SWCommand(SWCommand.SellRune)]
    public class SellRuneResponse : GenericSellResponse {
        [JsonProperty("runes")]
        public Rune[] SoldRunes; 
    }

    [SWCommand(SWCommand.SellRuneCraftItem)]
    public class SellRuneCraftItemResponse : GenericSellResponse {
        [JsonProperty("rune_craft_item_list")]
        public Craft[] SoldCrafts;
    }

    
}
