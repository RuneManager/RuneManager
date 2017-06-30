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

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);
			var com = obj["command"].ToString();
			switch (com)
			{
				default:
					return obj.ToObject<SWRequest>();
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}