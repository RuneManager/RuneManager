using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunePlugin.Response {
    [SWCommand(SWCommand.GetGuildSiegeBattleLogByWizardId)]
    public class GetGuildSiegeBattleLogByWizardId : SWRequest {
    }

    [SWCommand(SWCommand.GetGuildSiegeMatchupInfo)]
    public class GetGuildSiegeMatchupInfo : SWRequest {

    }

    [SWCommand(SWCommand.GetGuildSiegeDefenseDeckByWizardId)]
    public class GetGuildSiegeDefenseDeckByWizardId : SWRequest {

    }

    [SWCommand(SWCommand.GetTargetUnitInfo)]
    public class GetTargetUnitInfo : SWRequest {

    }

    [SWCommand(SWCommand.GetGuildSiegeRankingInfo)]
    public class GetGuildSiegeRankingInfo : SWRequest {

    }

    [SWCommand(SWCommand.GetGuildSiegeBaseDefenseUnitList)]
    public class GetGuildSiegeBaseDefenseUnitList : SWRequest {

    }

    [SWCommand(SWCommand.GetGuildSiegeBattleLogByDeckId)]
    public class GetGuildSiegeBattleLogByDeckId : SWRequest {
        public int LogType;


        public List<SiegeInfo> SiegeLog;
    }

    public class SiegeInfo {
        public List<SiegeGuildInfo> GuildInfo;
        public List<SiegeBattleLog> BattleLog;
    }

    public class SiegeGuildInfo {

    }

    public class SiegeBattleLog {

    }

}
