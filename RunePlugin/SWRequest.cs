using System;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin.Request;
using RuneOptim;

namespace RunePlugin {
    public class SWRequest : SWMessage {

    }

    public class SWRequestConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(SWRequest).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var obj = JObject.Load(reader);
            SWCommand com;
            if (Enum.TryParse(obj["command"].ToString(), out com)) {
                var commandTypes = this.GetType().Assembly.GetTypes().Where(t => typeof(SWRequest).IsAssignableFrom(t) && (t.GetCustomAttributes<SWCommandAttribute>()?.Any(a => a.Command == com) ?? false));

                if (commandTypes.Any()) {
                    if (commandTypes.HasCount(1)) {
#if DEBUG
                        Console.WriteLine("Reflecting " + com + " request to " + commandTypes.FirstOrDefault().Name);
#endif
                        return obj.ToObject(commandTypes.FirstOrDefault());
                    }
                    else {
                        Console.WriteLine("Multiple request types found for " + com + ":");
                        foreach (var c in commandTypes) {
                            Console.WriteLine("\t" + c.Name);
                        }
                    }
                }
                else {
                    Console.WriteLine("No request types found for " + com);
                }
            }
            return obj.ToObject<SWRequest>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
