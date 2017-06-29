using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin.Response;

namespace RunePlugin
{
	[JsonConverter(typeof(SWResponseConverter))]
	public class SWResponse : SWMessage
	{
		//public int ret_code;
		//public int tvalue;
		//public int tvaluelocal;
		//public string tzone;

		public string Command;

		[JsonProperty("ret_code")]
		public int ReturnCode;

		[JsonProperty("tvalue")]
		public int TValue;

		[JsonProperty("tvaluelocal")]
		public int TValueLocal;

		[JsonProperty("tzone")]
		public string TZone;

		[JsonProperty("wizard_info")]
		public WizardInfo WizardInfo;
	}

	public class SWResponseConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(SWResponse).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JObject.Load(reader);
			var com = obj["command"].ToString();
			switch (com)
			{
				case "BattleRiftDungeonResult":
					return obj.ToObject<BattleRiftDungeonResultResponse>();
				default:
					return obj.ToObject<SWResponse>();
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}

	public class WizardInfo
	{
		[JsonProperty("wizard_id")]
		public int Id;

		[JsonProperty("wizard_name")]
		public string Name;

		[JsonProperty("wizard_level")]
		public int Level;

		[JsonProperty("experience")]
		public ulong Experience;

		[JsonProperty("wizard_mana")]
		public ulong Mana;

		[JsonProperty("wizard_crystal")]
		public ulong Crystal;

		[JsonProperty("wizard_energy")]
		public ulong Energy;

		[JsonProperty("arena_energy")]
		public ulong ArenaEnergy;

		[JsonProperty("arena_energy_max")]
		public ulong ArenaEnergyMax;

		[JsonProperty("arena_energy_next_gain")]
		public ulong ArenaEnergyNextGain;

		[JsonProperty("wizard_last_login")]
		public DateTime LastLogin;

		[JsonProperty("unit_slots")]
		public UnitSlots UnitSlots;

		[JsonProperty("energy_max")]
		public ulong EnergyMax;

		[JsonProperty("energy_per_min")]
		public ulong EnergyPerMinute;

		[JsonProperty("social_point_current")]
		public ulong SocialPointCurrent;

		[JsonProperty("social_point_max")]
		public ulong SocialPointMax;

		[JsonProperty("honor_point")]
		public ulong HonorPoint;

		[JsonProperty("guild_point")]
		public ulong GuildPoint;

		[JsonProperty("rep_unit_id")]
		public ulong ReputatiopnMonsterId;

		[JsonProperty("rep_assigned")]
		public bool RepAssigned;

		[JsonProperty("pvp_event")]
		public bool PvpEvent;

		[JsonProperty("mail_box_event")]
		public bool MailBoxEvent;

		[JsonProperty("next_energy_gain")]
		public ulong EnergyNextGain;

		[JsonProperty("unit_depository_slots")]
		public UnitDepositorySlots UnitDepositorySlots;

		[JsonProperty("darkportal_energy")]
		public ulong DarkPortalEnergy;

		[JsonProperty("darkportal_energy_max")]
		public ulong DarkPortalEnergyMax;

		[JsonProperty("costume_point")]
		public int CostumePoint;

		[JsonProperty("costume_point_max")]
		public int CostumePointMax;

		[JsonProperty("honor_medal")]
		public int HonorMedal;

		[JsonProperty("gain_exp")]
		public int GainExp;
	}

	public class UnitSlots
	{
		[JsonProperty("number")]
		public ulong Number;

		[JsonProperty("upgrade")]
		public SlotsUpgrade Upgrade;
	}

	public class UnitDepositorySlots
	{
		[JsonProperty("number")]
		public ulong Number;

		[JsonProperty("upgrade")]
		public SlotsUpgrade Upgrade;
	}

	public class SlotsUpgrade
	{
		[JsonProperty("number")]
		public ulong Number;

		[JsonProperty("mana")]
		public ulong Mana;

		[JsonProperty("crystal")]
		public ulong Crystal;
	}
}