using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim.swar;

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

        public bool TestConnection() {

            TcpClient tc = new TcpClient();
            try {
                var tcp = baseUri.Replace("http://", "").Replace("https://", "").Split(':');
                int port = 80;
                if (tcp.Length > 1)
                    int.TryParse(tcp.Last(), out port);
                tc.Connect(tcp.First(), port);
                bool stat = tc.Connected;
                if (stat)
                    return true;

                tc.Close();
            }
            catch (Exception ex) {
                tc.Close();
                return false;
            }

            var api = WebRequest.CreateHttp(baseUri + "/api");
            api.Accept = "application/json";
            try {
                var resp = api.GetResponse();
                var stream = resp.GetResponseStream();
                using (var sr = new StreamReader(stream)) {
                    Console.WriteLine("Connection: " + sr.ReadToEnd());
                    return true;
                }
            }
            //catch (WebException e) when (e.InnerException is System.Net.Sockets.SocketException) {
            catch (Exception e) { 
                return false;
            }
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
