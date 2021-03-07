using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RunePlugin
{
    public class SWEventArgs : EventArgs
    {
        public readonly string RequestRaw;
        public readonly string ResponseRaw;

        public readonly JObject RequestJson;
        public readonly JObject ResponseJson;

        public readonly SWRequest Request;
        public readonly SWResponse Response;

        public SWEventArgs(string req, string resp)
        {
            RequestRaw = req;
            ResponseRaw = resp;
            RequestJson = JsonConvert.DeserializeObject<JObject>(req);
            ResponseJson = JsonConvert.DeserializeObject<JObject>(resp);
            Request = JsonConvert.DeserializeObject<SWRequest>(req, new SWRequestConverter());
            Response = JsonConvert.DeserializeObject<SWResponse>(resp, new SWResponseConverter());
        }

        public T RequestAs<T>() where T : SWRequest {
            return (T)Request;
        }

        public T ResponseAs<T>() where T : SWResponse {
            return (T)Response;
        }
    }
}