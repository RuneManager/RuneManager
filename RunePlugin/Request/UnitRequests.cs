using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Request {
    public class GenericUnitRequest : SWRequest {
        [JsonProperty("target_id")]
        public ulong TargetId;
    }

    [SWCommand(SWCommand.SacrificeUnit)]
    [SWCommand(SWCommand.UpgradeUnit)]
    public class SourceUnitRequest : GenericUnitRequest {
        [JsonProperty("source_list")]
        public Source[] Sources;
    }
    
    public class Source {
        [JsonProperty("source_id")]
        public ulong Id;
    }
}
