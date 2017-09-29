using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim;

namespace RuneManagerBridge {
	// TODO: Swagger or swhat?
	class RuneManagerApi {
		readonly string baseUri;
		public RuneManagerApi(string baseUri) {
			this.baseUri = baseUri;
		}

		public string MonsterPost(Monster mon) {
			var api = WebRequest.CreateHttp(baseUri + "/api/monsters/" + mon.Id.ToString());
			api.Accept = "application/json";
			api.Method = "POST";
			using (var str = new StreamWriter(api.GetRequestStream())) {
				str.Write(JsonConvert.SerializeObject(mon));
			}
			return new StreamReader(api.GetResponse().GetResponseStream()).ReadToEnd();
		}

		public string MonsterDelete(ulong id) {
			var api = WebRequest.CreateHttp(baseUri + "/api/monsters/" +id.ToString());
			api.Accept = "application/json";
			api.Method = "DELETE";
			try {
				return new StreamReader(api.GetResponse().GetResponseStream()).ReadToEnd();
			}
			catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError && ex.Message.Contains("404")) {
				return new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
			}
		}

		public string TestConnection() {
			var api = WebRequest.CreateHttp(baseUri + "/api");
			api.Accept = "application/json";
			return new StreamReader(api.GetResponse().GetResponseStream()).ReadToEnd();
		}

		public string MonsterAction(ulong id, string action) {
			var api = WebRequest.CreateHttp(baseUri + "/api/monsters/" + id.ToString() + "?action=" + action);
			api.Accept = "application/json";
			api.Method = "POST";
			using (var str = new StreamWriter(api.GetRequestStream())) {
				str.Write("{id:" + id.ToString() + "}");
			}
			return new StreamReader(api.GetResponse().GetResponseStream()).ReadToEnd();
		}

		public string RunePost(Rune rune) {
			var api = WebRequest.CreateHttp(baseUri + "/api/runes/" + rune.Id.ToString());
			api.Accept = "application/json";
			api.Method = "POST";
			using (var str = new StreamWriter(api.GetRequestStream())) {
				str.Write(JsonConvert.SerializeObject(rune));
			}
			return new StreamReader(api.GetResponse().GetResponseStream()).ReadToEnd();
		}

		public string RuneDelete(ulong id) {
			var api = WebRequest.CreateHttp(baseUri + "/api/runes/" +id.ToString());
			api.Accept = "application/json";
			api.Method = "DELETE";
			try {
				return new StreamReader(api.GetResponse().GetResponseStream()).ReadToEnd();
			}
			catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError && ex.Message.Contains("404")) {
				return new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
			}
		}

	}
}
