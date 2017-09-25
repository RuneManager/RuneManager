using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim;

namespace RunePlugin.Response {
	public class Gauge {
		public int original;
		public int bonus;
	}

	public class SacrificeUnitResponse : SWResponse {
		[JsonProperty("gauge")]
		public Gauge Gauge;

		[JsonProperty("target_unit")]
		public Monster Target;
	}

	public class SummonUnitResponse : SWResponse {
		[JsonProperty("item_info")]
		public InventoryItem RemainingItem;

		[JsonProperty("unit_list")]
		public Monster[] Monsters;
	}
	
	public class UpgradeUnitResponse : SWResponse {
		[JsonProperty("unit_info")]
		public Monster Target;
	}

	public class UpdateUnitExpGainedResponse : SWResponse {
		[JsonProperty("unit_list")]
		public Monster[] UpdatedMonsters;
	}
}
