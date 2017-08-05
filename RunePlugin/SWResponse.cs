using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin.Response;

namespace RunePlugin
{
	public class SWResponse : SWMessage
	{
		//public string Command;
		//public int ret_code;
		//public int tvalue;
		//public int tvaluelocal;
		//public string tzone;

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
			var com = obj["command"].ToString();
			switch (com)
			{
				case "BattleRiftDungeonResult":
					return obj.ToObject<BattleRiftDungeonResultResponse>();
				case "GetBestClearRiftDungeon":
					return obj.ToObject<GetBestClearRiftDungeonResponse>();
				case "EquipRune":
					return obj.ToObject<EquipRuneResponse>();
				case "UnequipRune":
					return obj.ToObject<UnequipRuneResponse>();
				case "EquipRuneList":
					return obj.ToObject<EquipRuneListResponse>();
				default:
					return obj.ToObject<SWResponse>();
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}