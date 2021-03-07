using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim.swar;

namespace RunePlugin.Response {
    [SWCommand(SWCommand.UpgradeDeco)]
    public class UpgradeDecoResponse : SWResponse {
        [JsonProperty("deco_info")]
        public Deco Deco;
    }
}
