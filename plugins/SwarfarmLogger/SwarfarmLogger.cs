using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin;
using System.Net;
using System.Web;
using System.IO;

namespace SwarfarmLogger
{
    public class SwarfarmLogger : SWPlugin
    {
        static string base_url = "https://swarfarm.com";
        static string commands_url { get { return base_url + "/data/log/accepted_commands/"; } }
        static string log_url { get { return base_url + "/data/log/upload/"; } }

        static Dictionary<string, SWFCommand> commands = null;
        public static Dictionary<string, SWFCommand> Commands
        {
            get
            {
                if (commands == null)
                {
                    var req = (HttpWebRequest)WebRequest.Create(commands_url);
                    //req.KeepAlive = false;
                    req.AllowAutoRedirect = false;
                    //req.ServicePoint.ConnectionLimit = 1;
                    req.ServicePoint.Expect100Continue = false;
                    var resp = req.GetResponse();
                    using (var stream = new StreamReader(resp.GetResponseStream()))
                    {
                        var raw = stream.ReadToEnd();
                        commands = JsonConvert.DeserializeObject<Dictionary<string, SWFCommand>>(raw);
                    }
                    resp.Close();
                }
                return commands;
            }
        }

        public override void OnLoad()
        {
            if (!Directory.Exists(PluginDataDirectory)) {
                Directory.CreateDirectory(PluginDataDirectory);
            }
            if (!File.Exists(PluginDataDirectory + "\\settings.cfg"))
                File.WriteAllText(PluginDataDirectory + "\\settings.cfg", JsonConvert.SerializeObject(new Dictionary<string, string>() { { "base_url", base_url } }));
            else {
                var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PluginDataDirectory + "\\settings.cfg"));
                base_url = settings["base_url"];
            }

            Console.WriteLine("Loaded " + Commands.Count + " commands from SWFarm " + base_url);
            //System.Net.ServicePointManager.Expect100Continue = false;
        }

        public override void ProcessRequest(object sender, SWEventArgs args)
        {
            var com = args.Request.CommandStr;
            if (Commands.ContainsKey(com))
            {
                var swfcom = Commands[args.Request.CommandStr];
                JObject data = new JObject();
                if (swfcom.RequestMembers != null && swfcom.RequestMembers.Count > 0)
                {
                    JObject req = new JObject();
                    foreach (var key in swfcom.RequestMembers)
                    {
                        req.Add(key, args.RequestJson[key]);
                    }
                    data.Add("request", req);
                }
                if (swfcom.ResponseMembers != null && swfcom.ResponseMembers.Count > 0)
                {
                    JObject resp = new JObject();
                    foreach (var key in swfcom.ResponseMembers)
                    {
                        resp.Add(key, args.ResponseJson[key]);
                    }
                    data.Add("response", resp);
                }
                
                // pretend to be python a little
                var send = "data=" + Uri.EscapeDataString(data.ToString(Formatting.None)).Replace(' ', '+');
                try
                {
                    var post = (HttpWebRequest)WebRequest.Create(log_url);
                    post.Method = "POST";
                    post.ContentType = "application/x-www-form-urlencoded";
                    //post.KeepAlive = false;
                    //post.Timeout = 10000;
                    //post.ReadWriteTimeout = 10000;
                    post.AllowAutoRedirect = false;
                    //post.ServicePoint.ConnectionLimit = 1;
                    post.ServicePoint.Expect100Continue = false;
                    
                    using (var write = new StreamWriter(post.GetRequestStream()))
                    {
                        write.Write(send);
                    }
                    HttpWebResponse resp = (HttpWebResponse)post.GetResponse();
                    Console.WriteLine("Sent " + com + " to SWarFarm");
                    resp.Close();
                }
                catch (Exception e)
                {
                    File.WriteAllText(Environment.CurrentDirectory + "\\plugins\\swarfarmlogger.error.log", e.GetType() + ": " + e.Message + Environment.NewLine + e.StackTrace);
                    Console.WriteLine("Sending " + com + " to SWarFarm failed: " + e.Message);
                }

            }
        }
    }

    public class SWFCommand
    {
        [JsonProperty("request")]
        public List<string> RequestMembers = null;

        [JsonProperty("response")]
        public List<string> ResponseMembers = null;
    }
}
