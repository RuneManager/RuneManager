using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Request {
	public class UpgradeRuneRequest : SWRequest {
		[JsonProperty("upgrade_curr")]
		public int CurrentLevel;

		[JsonProperty("rune_id")]
		public ulong RuneId;


	}
}
