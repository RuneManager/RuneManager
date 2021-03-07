using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RuneOptim.swar;

namespace RunePlugin {
    public class SWMessage {
        [JsonProperty("command")]
        public string CommandStr;

        [JsonIgnore]
        public SWCommand Command {
            get {
                SWCommand c = SWCommand.Unhandled;
                Enum.TryParse<SWCommand>(CommandStr, out c);
                return c;
            }
        }

        [JsonProperty("session_key")]
        protected string SessionKey;

        [JsonProperty("proto_ver")]
        public int ProtoVer;

        [JsonProperty("infocsv")]
        public string InfoCSV;

        [JsonProperty("channel_uid")]
        public int ChannelUID;

        [JsonProperty("ts_val")]
        public int TSVal;

        [JsonProperty("wizard_id")]
        public int WizardId;
    }

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class SWCommandAttribute : Attribute {
        public SWCommand Command { get; private set; }

        // This is a positional argument
        public SWCommandAttribute(SWCommand command) {
            this.Command = command;
        }
    }

    public class ClearRecord {
        [JsonProperty("is_new_record")]
        public bool NewRecord;

        [JsonProperty("current_time")]
        public long Current;

        [JsonProperty("best_time")]
        public long Best;
    }

    public class DungeonReward {
        [JsonProperty("mana")]
        public int Mana;

        [JsonProperty("crystal")]
        public int Crystal;

        [JsonProperty("energy")]
        public int Energy;

        [JsonProperty("crate")]
        public RewardCrate Crate;

        [JsonProperty("item_list")]
        public InventoryItem[] Items;
    }

    public class RewardCrate {
        [JsonProperty("rune")]
        public RuneOptim.swar.Rune Rune;

        [JsonProperty("craft_stuff")]
        public Craft Craft;

        [JsonProperty("material")]
        public InventoryItem Material;

        //[JsonProperty("material")]
        //public RuneOptim.InventoryItem[] Materials;

        [JsonProperty("summon_pieces")]
        public InventoryItem[] SummoningPieces;

        [JsonProperty("unit_info")]
        public Monster Monster;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SWCommand {
        Unhandled,
        EquipRune,
        EquipRuneList,
        UnequipRune,
        HubUserLogin,
        LockUnit,
        UnlockUnit,
        SummonUnit,
        BattleScenarioResult,
        BattleDungeonResult,
        SellRune,
        SacrificeUnit,
        SellRuneCraftItem,
        UpgradeUnit,
        UpdateUnitExpGained,
        BattleRiftDungeonResult,
        GetBestClearRiftDungeon,
        UpgradeRune,
        RevalueRune,
        ConfirmRune,
        UpgradeDeco,

        // Guild War
        GetGuildWarMatchupInfo,
        GetGuildWarParticipationInfo,
        GetGuildInfo,
        GetGuildWarBattleLogByGuildId,
        GetGuildWarBattleLogByWizardId,

        // Siege Battle
        GetTargetUnitInfo,
        GetGuildSiegeDefenseDeckByWizardId,
        GetGuildSiegeRankingInfo,
        GetGuildSiegeBaseDefenseUnitList,
        GetGuildSiegeBattleLogByDeckId,
        GetGuildSiegeMatchupInfo,
        GetGuildSiegeBattleLogByWizardId,
    }
}
