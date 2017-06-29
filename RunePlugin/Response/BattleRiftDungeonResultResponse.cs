using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim;

namespace RunePlugin.Response
{
	public class BattleRiftDungeonResultResponse : SWResponse
	{
		//public string command;
		//public int ret_code;

		[JsonProperty("rift_dungeon_box_id")]
		public int RiftDungeonBoxId;

		[JsonProperty("total_damage")]
		public ulong TotalDamage;

		[JsonProperty("best_clear_rift_dungeon_info")]
		public BestRiftDeck BestClearRiftDungeonInfo;

		[JsonProperty("bestdeck_rift_dungeon_info")]
		public RiftDeck BestDeckRiftDungeonInfo;

		//public WizardInfo WizardInfo;

		[JsonProperty("item_list")]
		public RuneOptim.InventoryItem[] ItemList;

		//public int tvalue;
		//public int tvaluelocal;
		//public string tzone;
	}
}
