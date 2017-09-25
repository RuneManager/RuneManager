using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Request {
	public class SacrificeUnitRequest : SWRequest {
		[JsonProperty("target_id")]
		public ulong TargetId;

		[JsonProperty("source_list")]
		public Source[] Sources;
	}

	public class UpgradeUnitRequest : SWRequest {
		[JsonProperty("target_id")]
		public ulong TargetId;

		[JsonProperty("source_list")]
		public Source[] Sources;
	}

	public class Source {
		[JsonProperty("source_id")]
		public ulong Id;
	}
}
