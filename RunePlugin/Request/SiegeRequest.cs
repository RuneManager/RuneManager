using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Request {
    [SWCommand(SWCommand.GetTargetUnitInfo)]
    public class GetTargetUnitInfoRequest : SWRequest {
        [JsonProperty("target_req_type")]
        public int RequestType;

        [JsonProperty("target_unit_list")]
        public List<Unit> UnitList;
    }

    [SWCommand(SWCommand.GetGuildSiegeBattleLogByWizardId)]
    public class GetGuildSiegeBattleLogByWizardId : SWRequest {
        [JsonProperty("log_type")]
        public int LogType;
    }

    [SWCommand(SWCommand.GetGuildSiegeBaseDefenseUnitList)]
    public class GetGuildSiegeBaseDefenseUnitList : SWRequest {
        [JsonProperty("base_number")]
        public int BaseNumber;
    }

    public class Unit {
        [JsonProperty("unit_id")]
        public long UnitId;
    }
}
