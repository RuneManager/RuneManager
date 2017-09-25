using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin.Request;

namespace RunePlugin
{
	public class SWRequest : SWMessage
	{

	}

	public class SWRequestConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(SWRequest).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
			var obj = JObject.Load(reader);
			SWCommand com;
			if (Enum.TryParse(obj["command"].ToString(), out com)) {
				switch (com) {
					case SWCommand.SacrificeUnit:
						return obj.ToObject<SacrificeUnitRequest>();
					case SWCommand.UpgradeUnit:
						return obj.ToObject<UpgradeUnitRequest>();
					case SWCommand.UpgradeRune:
						return obj.ToObject<UpgradeRuneRequest>();
					default:
						return obj.ToObject<SWRequest>();
				}
			}
			return obj.ToObject<SWRequest>();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}