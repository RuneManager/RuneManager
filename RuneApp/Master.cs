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
#if TEST_SLAVE
using SocketSender;
#endif

namespace RuneApp
{
	/// <summary>
	/// Connects and manages slaves.
	/// Also acts as the server for the remote management app.
	/// </summary>
	public class Master
	{
#if !TEST_SLAVE
		public log4net.ILog Log { get { return Program.log; } }
#else
		public Logger Log { get { return Program.log; } }
#endif

		private static readonly RRMResponse genericResponseBad = new GeneralResponse() { ResponseCode = 400, Message = "Request Failed", Exception = new ArgumentException("Method failed to read request.") };
		private static readonly RRMResponse genericResponseGood = new GeneralResponse() { ResponseCode = 200, Message = "Request Succeeded" };

		/// <summary>
		/// Dispatches a thread to listen for the incoming Remote App connection
		/// </summary>
		public void Start()
		{
			Task.Factory.StartNew(() =>
			{
				IPAddress ipAddress = IPAddress.Any;// Parse("[::]");

				Log.Info($"Starting TCP listener on {ipAddress}");
				TcpListener listener = new TcpListener(ipAddress, 7676);
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
				Log.Info("TCP Connection accepted.");

				bool isRunning = true;
				var stream = client.GetStream();

				try
				{
					using (StreamReader sr = new StreamReader(stream))
					using (StreamWriter sw = new StreamWriter(stream))
					{
						while (isRunning && client.Connected && client.Client.IsConnected())
						{
							Log.Info($"Reading data...{((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()}");
							var data = sr.ReadLine();
							Log.Info($"Recieved data: {data}");
							try
							{
								if (data == null)
								{
									sw.WriteLine(new RRMResponse() { data = { { "Error", "received message was null" } } });
								}
								else
								{
									var comm = JsonConvert.DeserializeObject<RRMRequest>(data, new DeserialCommand());
									var meth = GetTypes(comm.action).FirstOrDefault();
									RRMResponse resp = null;
									if (meth != null)
									{
										resp = (RRMResponse)meth.Invoke(this, new object[] { comm });
									}
									sw.WriteLine(JsonConvert.SerializeObject(resp));
								}
								sw.Flush();
							}
							catch (IOException ioex)
							{
								Log.Error($"IO :( {ioex.GetType()}", ioex);
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

				Log.Info("TCP Connection closed.");
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
				Program.RunBuilds(request.Skip, (int)request.RunTo);
				return genericResponseGood;
			}
			return genericResponseBad;
		}

		[RRM(RRMAction.RunTest)]
		public RRMResponse RunTest(RRMRequest req)
		{
			if (req is RunTestRequest)
			{
				var request = req as RunTestRequest;
				var build = Program.builds.FirstOrDefault(b => b.ID == request.buildID);
				if (build != null)
				{
					Program.RunTest(build);
					return genericResponseGood;
				}
			}
			return genericResponseBad;
		}

		[RRM(RRMAction.GetPowerups)]
		public RRMResponse GetPowerups(RRMRequest req)
		{
			if (req is GetPowerupsRequest)
			{
				var request = req as GetPowerupsRequest;
				var build = Program.builds.FirstOrDefault(b => b.ID == request.buildID);
				if (build != null)
				{
					return new GetPowerupsResponse() { Runes = build.GetPowerupRunes() };
					return genericResponseGood;
				}
			}
			return genericResponseBad;
		}

		[RRM(RRMAction.GetLoads)]
		public RRMResponse GetLoads(RRMRequest req)
		{
			if (req is GetLoadsRequest)
			{
				var request = req as GetLoadsRequest;
				return new GetLoadsResponse() { Loads = Program.loads };
			}
			return genericResponseBad;
		}

		[RRM(RRMAction.CancelBuilds)]
		public RRMResponse CancelBuilds(RRMRequest req)
		{
			if (req is CancelBuildsRequest)
			{
				var request = req as CancelBuildsRequest;
				Program.StopBuild();
				return genericResponseGood;
			}
			return genericResponseBad;
		}
	}

	public class DeserialCommand : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(RRMMessage).IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject item = JObject.Load(reader);
			var action = (RRMAction)item["action"].Value<Int64>();
			var ts = GetTypes(action);
			bool? isRequest = item["request"]?.Value<bool?>();
			if (isRequest ?? true)
				ts = ts.Where(q => typeof(RRMRequest).IsAssignableFrom(q));
			else
				ts = ts.Where(q => typeof(RRMResponse).IsAssignableFrom(q));

			var t = ts.FirstOrDefault();
			if (t != null)
			{
				return item.ToObject(t);
			}

			return item.ToObject<RRMMessage>();
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

	public class RRMAttribute : Attribute
	{
		public RRMAction action;
		public RRMAttribute(RRMAction a)
		{
			action = a;
		}
	}

	public enum RRMAction
	{
		RunBuilds = 1,
		RunTest,
		UpdateBuild,
		GetLoads,
		GetPowerups,
		CancelBuilds,
	}

	public class RRMMessage
	{
		public bool request;
		public RRMMessage(RRMMessage rm = null)
		{
			var attr = this.GetType().GetCustomAttributes(typeof(RRMAttribute), false).FirstOrDefault();
			if (attr != null)
				this.action = (attr as RRMAttribute).action;
			if (rm != null)
				data = rm.data;
		}
		public RRMAction action;
		public Dictionary<string, object> data = new Dictionary<string, object>();
		protected T GetValueOrDefault<T>(string key)
		{
			object val;
			if (data.TryGetValue(key, out val))
				return (T)val;
			return default(T);
		}
	}

	public class RRMRequest : RRMMessage
	{
		public RRMRequest(RRMRequest rq = null) : base(rq)
		{
			request = true;
		}
	}

	public class RRMResponse : RRMMessage
	{
		public RRMResponse(RRMRequest rq = null) : base(rq)
		{
			request = false;
		}
	}

	[RRM((RRMAction)(-1))]
	public class GeneralResponse : RRMResponse
	{
		[JsonIgnore]
		public Exception Exception { get { return GetValueOrDefault<Exception>("exception"); } set { data["exception"] = value; } }

		[JsonIgnore]
		public long ResponseCode { get { return GetValueOrDefault<long>("errorCode"); } set { data["errorCode"] = value; } }

		[JsonIgnore]
		public string Message { get { return GetValueOrDefault<string>("message"); } set { data["message"] = value; } }
	}

	[RRM(RRMAction.RunBuilds)]
	public class RunBuildsRequest : RRMRequest
	{
		[JsonIgnore]
		public bool Skip { get { return GetValueOrDefault<bool>("skip"); } set { data["skip"] = value; } }
		[JsonIgnore]
		public long RunTo { get { return GetValueOrDefault<long>("runTo"); } set { data["runTo"] = value; } }
	}

	[RRM(RRMAction.RunTest)]
	public class RunTestRequest : RRMRequest
	{
		[JsonIgnore]
		public long buildID { get { return GetValueOrDefault<long>("buildID"); } set { data["buildID"] = value; } }
	}

	[RRM(RRMAction.GetPowerups)]
	public class GetPowerupsRequest : RRMRequest
	{
		[JsonIgnore]
		public long buildID { get { return GetValueOrDefault<long>("buildID"); } set { data["buildID"] = value; } }
	}

	[RRM(RRMAction.GetPowerups)]
	public class GetPowerupsResponse : RRMResponse
	{
		[JsonIgnore]
		public IEnumerable<RuneOptim.Rune> Runes { get { return GetValueOrDefault<List<RuneOptim.Rune>>("runes"); } set { data["runes"] = value; } }
	}

	[RRM(RRMAction.GetLoads)]
	public class GetLoadsRequest : RRMRequest
	{
	}

	[RRM(RRMAction.GetLoads)]
	public class GetLoadsResponse : RRMResponse
	{
		[JsonIgnore]
		public IEnumerable<RuneOptim.Loadout> Loads { get { return GetValueOrDefault<List<RuneOptim.Loadout>>("runes"); } set { data["runes"] = value; } }
	}

	[RRM(RRMAction.CancelBuilds)]
	public class CancelBuildsRequest : RRMRequest
	{
	}

}
