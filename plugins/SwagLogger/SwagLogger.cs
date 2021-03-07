using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RunePlugin;

namespace SwagLogger
{
    // Conversion of:
    // https://github.com/Xzandro/sw-exporter/blob/master/app/plugins/swag-logger.js
    public class SwagLogger : SWPlugin
    {
        static string log_url = "https://gw.swop.one/data/upload/";

        public override void ProcessRequest(object sender, SWEventArgs args)
        {
            if (args.Request.Command == SWCommand.GetGuildWarBattleLogByGuildId || args.Request.Command ==  SWCommand.GetGuildWarBattleLogByWizardId)
            {
                try
                {
                    var post = HttpWebRequest.CreateHttp(log_url);
                    post.Method = "POST";
                    post.ContentType = "application/json";

                    using (var write = new StreamWriter(post.GetRequestStream()))
                    {
                        write.Write(args.ResponseRaw);
                    }
                    var resp = post.GetResponse();
                    Console.WriteLine("Sent " + args.Request.Command + " to SWAG");
                }
                catch (Exception e)
                {
                    File.WriteAllText(Environment.CurrentDirectory + "\\plugins\\swaglogger.error.log", e.GetType() + ": " + e.Message + Environment.NewLine + e.StackTrace);
                    Console.WriteLine("Sending " + args.Request.Command + " to SWAG failed :(");
                }
            }
        }
    }
}
