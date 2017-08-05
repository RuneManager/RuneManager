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
			// TODO: onload check if RM is running and ask for defs
			if (args.Request.Command == SWCommand.EquipRune)
			{
				var eqr = args.ResponseAs<EquipRuneResponse>();
				var api = HttpWebRequest.CreateHttp("http://localhost:7676/api/monsters/" + eqr.Monster.Id);
				api.Accept = "application/json";
				api.Method = "POST";
				using (var str = new StreamWriter(api.GetRequestStream())) {
					str.Write(JsonConvert.SerializeObject(eqr.Monster));
				}
				var aresp = api.GetResponse();
				var astr = new StreamReader(aresp.GetResponseStream()).ReadToEnd();
				Console.WriteLine(astr);
			}
			else if (args.Request.Command == SWCommand.UnequipRune)
			{
				var uqr = args.ResponseAs<UnequipRuneResponse>();
				var api = HttpWebRequest.CreateHttp("http://localhost:7676/api/monsters/" + uqr.Monster.Id);
				api.Accept = "application/json";
				api.Method = "POST";
				using (var str = new StreamWriter(api.GetRequestStream())) {
					str.Write(JsonConvert.SerializeObject(uqr.Monster));
				}
				var aresp = api.GetResponse();
				var astr = new StreamReader(aresp.GetResponseStream()).ReadToEnd();
				Console.WriteLine(astr);
			}
			else if (args.Request.Command == SWCommand.EquipRuneList) {
				var eqlr = args.ResponseAs<EquipRuneListResponse>();
				var api = HttpWebRequest.CreateHttp("http://localhost:7676/api/monsters/" + eqlr.TargetMonster.Id);
				api.Accept = "application/json";
				api.Method = "POST";
				using (var str = new StreamWriter(api.GetRequestStream())) {
					str.Write(JsonConvert.SerializeObject(eqlr.TargetMonster));
				}
				var aresp = api.GetResponse();
				var astr = new StreamReader(aresp.GetResponseStream()).ReadToEnd();
				Console.WriteLine(astr);
				foreach (var m in eqlr.SourceMonsters) {
					api = HttpWebRequest.CreateHttp("http://localhost:7676/api/monsters/" + m.Key);
					api.Accept = "application/json";
					api.Method = "POST";
					using (var str = new StreamWriter(api.GetRequestStream())) {
						str.Write(JsonConvert.SerializeObject(m.Value));
					}
					aresp = api.GetResponse();
					astr = new StreamReader(aresp.GetResponseStream()).ReadToEnd();
					Console.WriteLine(astr);
				}
			}
		}
	}

	// TODO: Swagger or swhat?
	public class Api
	{
		[JsonProperty("version")]
		public string Version;
		[JsonProperty("paths")]
		public Dictionary<string, ApiPath> Paths;
		[JsonProperty("host")]
		public string Host;

	}

	public class ApiPath {

	}
}
