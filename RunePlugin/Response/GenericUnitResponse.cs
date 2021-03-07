using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Response {
    [SWCommand(SWCommand.LockUnit)]
    [SWCommand(SWCommand.UnlockUnit)]
    public class GenericUnitResponse : SWResponse {
        [JsonProperty("unit_id")]
        public ulong UnitId;
    }
    
}
