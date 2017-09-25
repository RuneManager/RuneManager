using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim;

namespace RunePlugin.Response {
	public class UpgradeRuneResponse : SWResponse {
		[JsonProperty("rune")]
		public Rune Rune;
	}

	public class SellRuneResponse : SWResponse {
		[JsonProperty("sell_mana")]
		public int SellMana;

		[JsonProperty("runes")]
		public Rune[] SoldRunes; 
	}

	public class SellRuneCraftItemResponse : SWResponse {
		[JsonProperty("sell_mana")]
		public int SellMana;

		[JsonProperty("rune_craft_item_list")]
		public Craft[] SoldCrafts;
	}
}
