using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Response
{
	public class UnequipRuneResponse : SWResponse
	{
		[JsonProperty("rune")]
		public RuneOptim.Rune Rune;

		[JsonProperty("unit_info")]
		public RuneOptim.Monster Monster;
	}
}
