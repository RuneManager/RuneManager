using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Response
{
	public class EquipRuneResponse : SWResponse
	{
		[JsonProperty("rune_id")]
		public ulong RuneId;

		[JsonProperty("unit_info")]
		public RuneOptim.Monster Monster;
	}
}
