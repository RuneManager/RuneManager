using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RunePlugin;
using RunePlugin.Response;

namespace RuneManagerBridge
{
	public class RuneManagerBridge : SWPlugin
	{
		public override void ProcessRequest(object sender, SWEventArgs args)
		{
			if (args.Request.Command == SWCommand.EquipRune)
			{
				var eqr = args.ResponseAs<EquipRuneResponse>();
				var apir = HttpWebRequest.CreateHttp("http://localhost:7676/api");
				apir.Accept = "application/json";
				var aresp = apir.GetResponse();
				var astr = new StreamReader(aresp.GetResponseStream()).ReadToEnd();
				var api = JsonConvert.DeserializeObject<Api>(astr);
			}
			else if (args.Request.Command == SWCommand.UnequipRune)
			{
				var uqr = args.ResponseAs<UnequipRuneResponse>();
				var api = HttpWebRequest.CreateHttp("http://localhost:7676/api");
				api.Accept = "application/json";

			}
		}
	}

	public class Api
	{
		[JsonProperty("version")]
		public string Version;
		[JsonProperty("endpoints")]
		public string[] Endpoints;
		[JsonProperty("baseurl")]
		public string BaseUrl;

	}
}
