using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin.Response;

namespace RunePlugin
{
	public class SWResponse : SWMessage
	{
		[JsonProperty("ret_code")]
		public int ReturnCode;

		[JsonProperty("tvalue")]
		public int TValue;

		[JsonProperty("tvaluelocal")]
		public int TValueLocal;

		[JsonProperty("tzone")]
		public string TZone;

		[JsonProperty("wizard_info")]
		public RuneOptim.WizardInfo WizardInfo;
	}

	public class SWResponseConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(SWResponse).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);
			SWCommand com;
			if (Enum.TryParse(obj["command"].ToString(), out com)) {
				switch (com) {
					case SWCommand.BattleDungeonResult:
						return obj.ToObject<BattleDungeonResultResponse>();
					case SWCommand.BattleRiftDungeonResult:
						return obj.ToObject<BattleRiftDungeonResultResponse>();
					case SWCommand.BattleScenarioResult:
						return obj.ToObject<BattleScenarioResultResponse>();
					case SWCommand.EquipRune:
						return obj.ToObject<EquipRuneResponse>();
					case SWCommand.EquipRuneList:
						return obj.ToObject<EquipRuneListResponse>();
					case SWCommand.GetBestClearRiftDungeon:
						return obj.ToObject<GetBestClearRiftDungeonResponse>();
					case SWCommand.LockUnit:
						return obj.ToObject<LockUnitResponse>();
					case SWCommand.SacrificeUnit:
						return obj.ToObject<SacrificeUnitResponse>();
					case SWCommand.SellRune:
						return obj.ToObject<SellRuneResponse>();
					case SWCommand.SellRuneCraftItem:
						return obj.ToObject<SellRuneResponse>();
					case SWCommand.SummonUnit:
						return obj.ToObject<SummonUnitResponse>();
					case SWCommand.UnequipRune:
						return obj.ToObject<UnequipRuneResponse>();
					case SWCommand.UnlockUnit:
						return obj.ToObject<UnlockUnitResponse>();
					case SWCommand.UpdateUnitExpGained:
						return obj.ToObject<UpdateUnitExpGainedResponse>();
					case SWCommand.UpgradeRune:
						return obj.ToObject<UpgradeRuneResponse>();
					case SWCommand.UpgradeUnit:
						return obj.ToObject<UpgradeUnitResponse>();
					default:
						return obj.ToObject<SWResponse>();
				}
			}
			return obj.ToObject<SWResponse>();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}