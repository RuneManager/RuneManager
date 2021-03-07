using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuneOptim;
using RuneOptim.swar;
using RunePlugin.Response;

namespace RunePlugin {
    public class SWResponse : SWMessage {
        [JsonProperty("ret_code")]
        public int ReturnCode;

        [JsonProperty("tvalue")]
        public int TValue;

        [JsonProperty("tvaluelocal")]
        public int TValueLocal;

        [JsonProperty("tzone")]
        public string TZone;

        [JsonProperty("wizard_info")]
        public WizardInfo WizardInfo;
    }

    public class SWResponseConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(SWResponse).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var obj = JObject.Load(reader);
            SWCommand com;
            if (Enum.TryParse(obj["command"].ToString(), out com)) {
                var commandTypes = this.GetType().Assembly.GetTypes().Where(t => typeof(SWResponse).IsAssignableFrom(t) && (t.GetCustomAttributes<SWCommandAttribute>()?.Any(a => a.Command == com) ?? false));

                if (commandTypes.Any()) {
                    if (commandTypes.HasCount(1)) {
#if DEBUG
                        Console.WriteLine("Reflecting " + com + " response to " + commandTypes.FirstOrDefault().Name);
#endif
                        return obj.ToObject(commandTypes.FirstOrDefault());
                    }
                    else {
                        Console.WriteLine("Multiple response types found for " + com + ":");
                        foreach (var c in commandTypes) {
                            Console.WriteLine("\t" + c.Name);
                        }
                    }
                }
                else {
                    Console.WriteLine("No response types found for " + com);
                }
            }
            return obj.ToObject<SWResponse>();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
