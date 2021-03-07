using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Request {
    [SWCommand(SWCommand.RevalueRune)]
    public class GenericRuneRequest : SWRequest {
        [JsonProperty("rune_id")]
        public ulong RuneId;
    }

    [SWCommand(SWCommand.UpgradeRune)]
    public class UpgradeRuneRequest : GenericRuneRequest {
        [JsonProperty("upgrade_curr")]
        public int CurrentLevel;
    }

    [SWCommand(SWCommand.ConfirmRune)]
    public class ConfirmRuneRequest : GenericRuneRequest {
        [JsonProperty("roll_back")]
        public bool Rollback;
    }
}
