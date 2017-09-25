using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Response {
	public class LockUnitResponse : SWResponse {
		[JsonProperty("unit_id")]
		public ulong UnitId;
	}

	public class UnlockUnitResponse : SWResponse {
		[JsonProperty("unit_id")]
		public ulong UnitId;
	}
}
