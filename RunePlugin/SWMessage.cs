using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RunePlugin
{
	public class SWMessage
	{
		[JsonProperty("command")]
		public string CommandStr;

		[JsonIgnore]
		public SWCommand Command
		{
			get
			{
				SWCommand c = SWCommand.Unhandled;
				Enum.TryParse<SWCommand>(CommandStr, out c);
				return c;
			}
		}

		[JsonProperty("session_key")]
		protected string SessionKey;

		[JsonProperty("proto_ver")]
		public int ProtoVer;

		[JsonProperty("infocsv")]
		public string InfoCSV;

		[JsonProperty("channel_uid")]
		public int ChannelUID;

		[JsonProperty("ts_val")]
		public int TSVal;

		[JsonProperty("wizard_id")]
		public int WizardId;
	}

	public class ClearRecord {
		[JsonProperty("is_new_record")]
		public bool NewRecord;

		[JsonProperty("current_time")]
		public long Current;

		[JsonProperty("best_time")]
		public long Best;
	}
	
	public class DungeonReward {
		[JsonProperty("mana")]
		public int Mana;

		[JsonProperty("crystal")]
		public int Crystal;

		[JsonProperty("energy")]
		public int Energy;

		[JsonProperty("crate")]
		public RewardCrate Crate;

		[JsonProperty("item_list")]
		public RuneOptim.InventoryItem[] Items;
	}

	public class RewardCrate {
		[JsonProperty("rune")]
		public RuneOptim.Rune Rune;

		[JsonProperty("craft_stuff")]
		public RuneOptim.Craft Craft;

		[JsonProperty("material")]
		public RuneOptim.InventoryItem Material;

		//[JsonProperty("material")]
		//public RuneOptim.InventoryItem[] Materials;

		[JsonProperty("summon_pieces")]
		public RuneOptim.InventoryItem[] SummoningPieces;

		[JsonProperty("unit_info")]
		public RuneOptim.Monster Monster;
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum SWCommand
	{
		Unhandled,
		EquipRune,
		EquipRuneList,
		UnequipRune,
		HubUserLogin,
		GetGuildWarBattleLogByGuildId,
		GetGuildWarBattleLogByWizardId,
		LockUnit,
		UnlockUnit,
		SummonUnit,
		BattleScenarioResult,
		BattleDungeonResult,
		SellRune,
		SacrificeUnit,
		SellRuneCraftItem,
		UpgradeUnit,
		UpdateUnitExpGained,
		BattleRiftDungeonResult,
		GetBestClearRiftDungeon,
		UpgradeRune,
	}
}
