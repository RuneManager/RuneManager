using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin
{
	public class SWMessage
	{
		[JsonProperty("command")]
		public string Command;

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
}
