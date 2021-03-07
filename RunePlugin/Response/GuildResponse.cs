using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Response {
    [SWCommand(SWCommand.GetGuildWarBattleLogByGuildId)]
    public class GetGuildWarBattleLogByGuildIdResponse : SWResponse {
        [JsonProperty("log_type")]
        public int LogType;

        [JsonProperty("battle_log_list_group")]
        public List<BattleLogGroup> BattleLogs;
    }

    [SWCommand(SWCommand.GetGuildWarMatchupInfo)]
    public class GetGuildWarMatchupInfoResponse : SWResponse {
        //guildwar_match_info
        //guildwar_my_dead_unit_id_list
        //my_atkdef_list

        [JsonProperty("my_attack_list")]
        public List<GuildWarAttacker> AttackerList;

        //opp_participation_info

        [JsonProperty("opp_guild_info")]
        public GuildInfo OppGuildInfo;

        [JsonProperty("opp_guild_member_list")]
        public List<GuildMember> OppGuildMembers;
        //opp_defense_list

    }

    [SWCommand(SWCommand.GetGuildWarParticipationInfo)]
    public class GetGuildWarParticipationInfoResponse : SWResponse {
        //guildwar_participation_info

        [JsonProperty("guildwar_member_list")]
        public List<GuildMember> MemberList;

        //guild_member_defense_list
        //guildwar_ranking_info
        //guildwar_ranking_stat
        //guildwar_reserve
    }

    [SWCommand(SWCommand.GetGuildInfo)]
    public class GetGuildInfoResponse : SWResponse {
        [JsonProperty("guild")]
        public Guild Guild;
    }

    public class Guild {
        [JsonProperty("guild_members")]
        public Dictionary<string, GuildMember> Members;
    }

    public class GuildInfo {
        [JsonProperty("guild_id")]
        public long GuildId;

        [JsonProperty("name")]
        public string Name;
    }

    public class GuildWarMatchInfo {
        [JsonProperty("match_id")]
        public long MatchId;

        [JsonProperty("guild_id")]
        public long GuildId;

        [JsonProperty("opp_guild_id")]
        public long OppGuildId;

    }

    public class GuildWarAttacker {
        [JsonProperty("match_id")]
        public long MatchId;

        [JsonProperty("wizard_id")]
        public long WizardId;

        [JsonProperty("guild_id")]
        public long GuildId;

        [JsonProperty("guild_point_var")]
        public int GuildPoints;

        [JsonProperty("energy")]
        public int Swords;
    }

    public class GuildMember {
        [JsonProperty("guild_id")]
        public long GuildId;

        [JsonProperty("wizard_id")]
        public long WizardId;

        [JsonProperty("grade")]
        public int Grade;

        [JsonProperty("channel_uid")]
        public long ChannelUid;

        [JsonProperty("wizard_name")]
        public string WizardName;

        [JsonProperty("wizard_level")]
        public int WizardLevel;

        [JsonProperty("rating_id")]
        public int RatingId;

        [JsonProperty("arena_score")]
        public int ArenaScore;

        [JsonProperty("last_login_time")]
        public long LastLogin;

    }

    public class BattleLogGroup {
        [JsonProperty("opp_guild_info")]
        public OppGuildInfo EnemyGuild;

        [JsonProperty("battle_log_list")]
        public List<BattleLog> BattleLogList;
    }

    public class OppGuildInfo {
        [JsonProperty("guild_id")]
        public long GuildId;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("master_wizard_id")]
        public long MasterWizardId;

        [JsonProperty("master_channel_uid")]
        public long MasterChannelUid;

        [JsonProperty("master_wizard_name")]
        public string MasterWizardName;

        [JsonProperty("master_wizard_level")]
        public int MasterWizardLevel;
    }

    public class BattleLog {
        [JsonProperty("rid")]
        public long RID;

        [JsonProperty("battle_time")]
        public int BattleTime;

        [JsonProperty("log_type")]
        public int LogType;

        [JsonProperty("match_id")]
        public long MatchId;

        [JsonProperty("league_type")]
        public int LeagueType;

        [JsonProperty("wizard_id")]
        public long WizardId;

        [JsonProperty("wizard_level")]
        public int WizardLevel;

        [JsonProperty("channel_uid")]
        public long ChannelUid;

        [JsonProperty("wizard_name")]
        public string WizardName;

        [JsonProperty("win_count")]
        public int WinCount;

        [JsonProperty("draw_count")]
        public int DrawCount;

        [JsonProperty("lose_count")]
        public int LoseCount;

        [JsonProperty("result")]
        public WinLose[] Result;

        [JsonProperty("guild_id")]
        public long GuildId;

        [JsonProperty("guild_name")]
        public string GuildName;

        [JsonProperty("guild_point_var")]
        public int GuildPoints;

        [JsonProperty("opp_wizard_id")]
        public long OppWizardId;

        [JsonProperty("opp_wizard_level")]
        public int OppWizardLevel;

        [JsonProperty("opp_wizard_name")]
        public string OppWizardName;

        [JsonProperty("opp_guild_id")]
        public long OppGuildId;

        [JsonProperty("opp_guild_name")]
        public string OppGuildName;

        [JsonProperty("battle_end")]
        public long BattleEnd;

        public DateTime Time { get { return new DateTime().AddSeconds(BattleEnd); } }
    }
}
