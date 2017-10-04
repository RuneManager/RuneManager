﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RunePlugin.Response {
	[SWCommand(SWCommand.UpgradeDeco)]
	class UpgradeDecoResponse : SWResponse {
		[JsonProperty("deco_info")]
		RuneOptim.Deco Deco;
	}
}
