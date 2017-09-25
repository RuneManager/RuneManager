using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim;

namespace RunePlugin.Response {
	public enum WinLose {
		Win = 1,
		Lose = 2,
	}

	public class BattleDungeonResultResponse : SWResponse {
		[JsonProperty("win_lose")]
		public WinLose Win;

		[JsonProperty("clear_time")]
		public ClearRecord Time;

		[JsonProperty("reward")]
		public DungeonReward Reward;

		[JsonProperty("clear_bonus")]
		public DungeonReward ClearBonus;

		[JsonProperty("unit_list")]
		public RuneOptim.Monster[] Monsters;
	}

	public class BattleRiftDungeonResultResponse : SWResponse {
		[JsonProperty("rift_dungeon_box_id")]
		public int RiftDungeonBoxId;

		[JsonProperty("total_damage")]
		public ulong TotalDamage;

		[JsonProperty("best_clear_rift_dungeon_info")]
		public BestRiftDeck BestClearRiftDungeonInfo;

		[JsonProperty("bestdeck_rift_dungeon_info")]
		public RiftDeck BestDeckRiftDungeonInfo;

		[JsonProperty("item_list")]
		public RuneOptim.InventoryItem[] ItemList;
	}

	public class ScenarioInfo {
		[JsonProperty("wizard_id")]
		public ulong WizardId;

		[JsonProperty("region_id")]
		public int RegionId;

		[JsonProperty("difficulty")]
		public int Difficulty;

		[JsonProperty("stage_no")]
		public int StageNumber;

		[JsonProperty("cleared")]
		public bool Cleared;
	}

	public class BattleScenarioResultResponse : SWResponse {
		[JsonProperty("win_lose")]
		public WinLose Win;

		[JsonProperty("clear_time")]
		public ClearRecord Time;

		[JsonProperty("reward")]
		public DungeonReward Reward;

		[JsonProperty("unit_list")]
		public RuneOptim.Monster[] Monsters;
	}
}
