using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RuneApp
{
    /// <summary>
    /// Connects and manages slaves.
    /// Also acts as the server for the remote management app.
    /// </summary>
    public class Master
    {
        public log4net.ILog Log { get { return Program.log; } }
        
        /// <summary>
        /// Dispatches a thread to listen for the incoming Remote App connection
        /// </summary>
        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

                Log.Info($"Starting TCP listener on {ipAddress}");
                TcpListener listener = new TcpListener(ipAddress, 500);
                listener.Start();
                Log.Info("Server is listening on " + listener.LocalEndpoint);

                while (true)
                {
                    Log.Info("Waiting for a connection...");
                    var client = listener.AcceptTcpClient();
                    Task.Factory.StartNew(RemoteManageLoop, client, new CancellationToken());
                }
            });
        }

        public void RemoteManageLoop(object _client)
        {
            if (_client is TcpClient)
            {
                TcpClient client = _client as TcpClient;
                Log.Info("Connection TCP accepted.");

                bool isRunning = true;
                var stream = client.GetStream();

                try
                {
                    using (StreamReader sr = new StreamReader(stream))
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        while (isRunning && client.Connected)
                        {
                            Log.Info("Reading data...");
                            var data = sr.ReadLine();
                            Log.Info($"Recieved data: {data}");
                            try
                            {
                                var comm = JsonConvert.DeserializeObject<RRMRequest>(data, new DeserialCommand());
                                var meth = GetTypes(comm.action).FirstOrDefault();
                                RRMResponse resp = null;
                                if (meth != null)
                                {
                                    resp = (RRMResponse)meth.Invoke(this, new object[] { comm });
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error($"Deserial failed with {ex.GetType()}", ex);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Client failed with {e.GetType()}", e);
                }

                client.Close();
                return;
            }
            throw new Exception("Failed to bind client");
        }


        public static IEnumerable<System.Reflection.MethodInfo> GetTypes(RRMAction act)
        {
            return from m in typeof(Master).GetMethods()
                   let attributes = m.GetCustomAttributes(typeof(RRMAttribute), true).Cast<RRMAttribute>().Where(rrm => rrm.action == act).ToArray()
                   where attributes != null && attributes.Length > 0
                   select m;
        }

        [RRM(RRMAction.RunBuilds)]
        public RRMResponse RunBuilds(RRMRequest req)
        {
            if (req is RunBuildsRequest)
            {
                var request = req as RunBuildsRequest;
                Program.RunBuilds(request.Skip, request.RunTo);
            }
            return null;
        }
    }

    public class RRMAttribute : Attribute
    {
        public RRMAction action;
        public RRMAttribute(RRMAction a)
        {
            action = a;
        }
    }

    public class DeserialCommand : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(RRMRequest).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
            var t = GetTypes((RRMAction)item["action"].Value<Int64>()).FirstOrDefault();
            if (t != null)
            {
                return item.ToObject(t);
            }

            return item.ToObject<RRMRequest>();
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Type> GetTypes(RRMAction act)
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   let attributes = t.GetCustomAttributes(typeof(RRMAttribute), true).Cast<RRMAttribute>().Where(rrm => rrm.action == act).ToArray()
                   where attributes != null && attributes.Length > 0
                   select t;
        }
    }

    public enum RRMAction
    {
        RunBuilds = 1,
        UpdateBuild,
    }

    public class RRMRequest
    {
        public RRMAction action;
        public Dictionary<string, object> data = new Dictionary<string, object>();
    }

    public class RRMResponse
    {
        public Dictionary<string, object> data;
    }

    [RRM(RRMAction.RunBuilds)]
    public class RunBuildsRequest : RRMRequest
    {
        public RunBuildsRequest(RRMRequest rq = null)
        {
            if (rq != null)
                data = rq.data;
        }
        [JsonIgnore]
        public bool Skip { get { return (bool)data["skip"]; } set { data["skip"] = value; } }
        [JsonIgnore]
        public int RunTo { get { return (int)(long)data["runTo"]; } set { data["runTo"] = value; } }
    }
}
