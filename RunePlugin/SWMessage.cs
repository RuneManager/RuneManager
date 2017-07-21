using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RunePlugin
{
	public class SWMessage
	{
		[JsonProperty("command")]
		public string CommandStr;

		[JsonIgnore]
		public SWCommand Command
		{
			get
			{
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

	[JsonConverter(typeof(StringEnumConverter))]
	public enum SWCommand
	{
		Unhandled,
		EquipRune,
		UnequipRune,
		HubUserLogin,
		GetGuildWarBattleLogByGuildId,
		GetGuildWarBattleLogByWizardId,
	}
}
