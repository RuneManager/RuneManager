using System;
using Newtonsoft.Json;

namespace RuneOptim.swar {
    public class WizardInfo {
        [JsonProperty("wizard_id")]
        public ulong Id;

        [JsonProperty("wizard_name")]
        public string Name;

        [JsonProperty("wizard_level")]
        public int Level;

        [JsonProperty("experience")]
        public int Experience;

        [JsonProperty("wizard_mana")]
        public int Mana;

        [JsonProperty("wizard_crystal")]
        public int Crystal;

        [JsonProperty("wizard_energy")]
        public int Energy;

        [JsonProperty("arena_energy")]
        public int ArenaEnergy;

        [JsonProperty("arena_energy_max")]
        public int ArenaEnergyMax;

        [JsonProperty("arena_energy_next_gain")]
        public int ArenaEnergyNextGain;

        [JsonProperty("wizard_last_login")]
        public DateTime LastLogin;

        [JsonProperty("unit_slots")]
        public UnitSlots UnitSlots;

        [JsonProperty("energy_max")]
        public int EnergyMax;

        [JsonProperty("energy_per_min")]
        public double EnergyPerMinute;

        [JsonProperty("social_point_current")]
        public int SocialPointCurrent;

        [JsonProperty("social_point_max")]
        public int SocialPointMax;

        [JsonProperty("honor_point")]
        public int HonorPoint;

        [JsonProperty("guild_point")]
        public int GuildPoint;

        [JsonProperty("rep_unit_id")]
        public ulong ReputationMonsterId;

        [JsonProperty("rep_assigned")]
        public bool RepAssigned;

        [JsonProperty("pvp_event")]
        public bool PvpEvent;

        [JsonProperty("mail_box_event")]
        public bool MailBoxEvent;

        [JsonProperty("next_energy_gain")]
        public int EnergyNextGain;

        [JsonProperty("unit_depository_slots")]
        public UnitSlots UnitDepositorySlots;

        [JsonProperty("darkportal_energy")]
        public int DarkPortalEnergy;

        [JsonProperty("darkportal_energy_max")]
        public int DarkPortalEnergyMax;

        [JsonProperty("costume_point")]
        public int CostumePoint;

        [JsonProperty("costume_point_max")]
        public int CostumePointMax;

        [JsonProperty("honor_medal")]
        public int HonorMedal;

        [JsonProperty("gain_exp")]
        public int GainExp;
    }

    public class UnitSlots {
        [JsonProperty("number")]
        public int Number;

        [JsonProperty("upgrade")]
        public SlotsUpgrade Upgrade;
    }

    public class SlotsUpgrade {
        [JsonProperty("number")]
        public int Number;

        [JsonProperty("mana")]
        public int Mana;

        [JsonProperty("crystal")]
        public int Crystal;
    }

    public class DefensePlacement {
        [JsonProperty("pos_id")]
        public int Position;

        [JsonProperty("unit_id")]
        public ulong UnitId;

        [JsonProperty("wizard_id")]
        public ulong WizardId;

        [JsonProperty("battle_round")]
        public int Round;
    }
}
