using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RuneOptim;
using RuneApp;
using System.Collections;
using RunePlugin;
#if TEST_SLAVE
using SocketSender;
#endif

namespace RuneApp.InternalServer
{
	/// <summary>
	/// Connects and manages slaves.
	/// Also acts as the server for the remote management app.
	/// </summary>
	public partial class Master : PageRenderer
	{
#if !TEST_SLAVE
		public static log4net.ILog Log { get { return Program.log; } }
#else
		public Logger Log { get { return Program.log; } }
#endif

		private static readonly RRMResponse genericResponseBad = new GeneralResponse() { ResponseCode = 400, Message = "Request Failed", Exception = new ArgumentException("Method failed to read request.") };
		private static readonly RRMResponse genericResponseGood = new GeneralResponse() { ResponseCode = 200, Message = "Request Succeeded" };

		private HttpListener listener;

		bool isRunning = false;

		public static string currentTheme = "/css/none.css";

		/// <summary>
		/// Dispatches a thread to listen for the incoming Remote App connection
		/// </summary>
		public void Start()
		{
			try
			{
				try
				{
					listener = new HttpListener();
					listener.Prefixes.Add("http://*:7676/");
					listener.Start();
				}
				catch
				{
					Log.Error("Failed to bind to *, binding to localhost");
					listener = new HttpListener();
					listener.Prefixes.Add("http://localhost:7676/");
					listener.Start();
				}

				Log.Info("Server is listening on " + listener.Prefixes.First());
				isRunning = true;
			}
			catch (Exception e)
			{
				Log.Error("Failed to start server", e);
				throw;
			}

			Task.Factory.StartNew(() =>
			{
				try
				{
					while (isRunning)
					{
						Log.Info("Waiting for a connection...");
						var context = listener.GetContext();
						new Thread(() => RemoteManageLoop(context)).Start();
					}
				}
				catch (Exception e)
				{
					Log.Error("Failed while running server", e);
				}
				isRunning = false;
			}, TaskCreationOptions.LongRunning);
		}

		public void Stop()
		{
			listener.Stop();
			listener.Close();

			DateTime start = DateTime.Now;
			while (DateTime.Now - start < new TimeSpan(0,0,5))
			{
				Thread.Sleep(100);
				if (!isRunning)
					return;
			}
			throw new TaskCanceledException("Failed to stop server!");
		}

		public async void RemoteManageLoop(HttpListenerContext context)
		{
			Log.Debug("serving: " + context.Request.RawUrl);
			var req = context.Request;
			var resp = context.Response;

			var msg = getResponse(req);
			resp.StatusCode = (int)msg.StatusCode;
			Log.Debug("returning: " + resp.StatusCode);
			foreach (var h in msg.Headers)
			{
				foreach (var v in h.Value)
				{
					resp.Headers.Add(h.Key, v);
				}
			}

			if (resp.StatusCode != 303)
			{
				using (var output = resp.OutputStream)
				{
					byte[] outBytes = Encoding.UTF8.GetBytes("Critical Failure");
					bool expires = false;
					if (msg.Content is StringContent)
					{
						var qw = msg.Content as StringContent;
						string qq = await qw.ReadAsStringAsync();
						outBytes = Encoding.UTF8.GetBytes(qq);
					}
					else if (msg.Content is FileContent)
					{
						var qw = msg.Content as FileContent;
						resp.ContentType = qw.Type;
						resp.Headers.Add("Content-Disposition", $"attachment; filename=\"{qw.FileName}\"");
						outBytes = await qw.ReadAsByteArrayAsync();
					}
					else if (msg.Content is ByteArrayContent)
					{
						var qw = msg.Content as ByteArrayContent;
						resp.ContentType = "application/octet-stream";
						outBytes = await qw.ReadAsByteArrayAsync();
					}
					else if (msg.Content is StreamContent)
					{
						var qw = msg.Content as StreamContent;
						using (var ms = new MemoryStream())
						{
							var stream = await qw.ReadAsStreamAsync();
							stream.CopyTo(ms);
							outBytes = ms.ToArray();
						}

					}
					resp.Headers.Add("Content-language", "en-au");

					if (expires)
					{
						resp.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
						resp.Headers.Add("Pragma", "no-cache");
						resp.Headers.Add("Expires", "Wed, 16 Jul 1969 13:32:00 UTC");
					}

					var enc = req.Headers.GetValues("Accept-Encoding");
					// 
					if (enc.Any(a => a.ToLowerInvariant() == "deflate"))
					{
						resp.Headers.Add("Content-Encoding", "deflate");
						using (MemoryStream ms = new MemoryStream())
						{
							using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress))
							{
								ds.Write(outBytes, 0, outBytes.Length);
								ds.Flush();
							}
							ms.Flush();
							outBytes = ms.ToArray();
						}
					}

					resp.ContentLength64 = outBytes.Length;
					try
					{
						output.Write(outBytes, 0, outBytes.Length);
					}
					catch (Exception ex)
					{
						Program.log.Error("Failed to write " + ex.GetType().ToString() + " " + ex.Message);
					}
				}
			}
			else
			{
				resp.OutputStream.Close();
			}
		}

		public HttpResponseMessage getResponse(HttpListenerRequest req)
		{
			if (req.AcceptTypes == null)
			{
				//TODO: topkek return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
			}

			var msg = new HttpResponseMessage();

			var ruri = req.RawUrl;
			if (ruri.Contains("?"))
				ruri = ruri.Remove(ruri.IndexOf("?"));
			var locList = ruri.Split('/').ToList();
			locList.RemoveAll(a => a == "");

			msg.StatusCode = HttpStatusCode.OK;
			Log.Debug("rendering response...");
			return this.Render(req, locList.ToArray());

			//<html><head><script src='/script.js'></script></head><body><button id='button1' style='width:50px' onclick='javascript:startProgress();'>Start</button></body></html>
			
		}

		private string getUrlComp(string url, int comp)
		{
			if (url.Contains("?"))
				url = url.Remove(url.IndexOf("?"));
			var array = url.Split('/');
			if (array.Length > comp && !string.IsNullOrWhiteSpace(array[comp]))
				return array[comp];
			
			return null;
		}
		
		public static IEnumerable<MethodInfo> GetTypes(RRMAction act)
		{
			return from m in typeof(Master).GetMethods()
				   let attributes = m.GetCustomAttributes(typeof(RRMAttribute), true).Cast<RRMAttribute>().Where(rrm => rrm.action == act).ToArray()
				   where attributes != null && attributes.Length > 0
				   select m;
		}

		#region Address Rendering

		public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
		{
			if (uri == null || uri.Length == 0 || uri[0] == "/")
			{
				return returnHtml(null, "Check my thingo!<br/>",
					new ServedResult("a") { contentDic = { { "href", "api" } }, contentList = { "Api docs" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "runes" } }, contentList = { "Rune list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "monsters" } }, contentList = { "Monster list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "builds" } }, contentList = { "Build list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "loads" } }, contentList = { "Load list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "rift" } }, contentList = { "Rift Best Clear" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "css" } }, contentList = { "Choose a theme!" } }, "<br/>",
					"<br/>");
			}
			else
			{
				return Recurse(req, uri);
			}
		}

		protected static HttpResponseMessage return404()
		{
			// TODO:
			return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("404 lol") };
		}

		protected static HttpResponseMessage returnHtml(ServedResult[] head = null, params ServedResult[] body)
		{
			var bb = new StringBuilder();
			if (body != null)
				foreach (var b in body)
					bb.AppendLine(b.ToHtml());

			var hh = new StringBuilder();
			if (head != null)
				foreach (var h in head)
					hh.AppendLine(h.ToHtml());

			var html = InternalServer.default_tpl
			.Replace("{title}", "TopKek")
			.Replace("{theme}", Master.currentTheme)
			.Replace("{head}", hh.ToString())
			.Replace("{body}", bb.ToString())
			;

			return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html) };
		}
		
		[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
		public class PageAddressRenderAttribute : Attribute
		{
			// See the attribute guidelines at 
			//  http://go.microsoft.com/fwlink/?LinkId=85236
			readonly string positionalString;

			// This is a positional argument
			public PageAddressRenderAttribute(string positionalString)
			{
				this.positionalString = positionalString;
			}

			public string PositionalString
			{
				get { return positionalString; }
			}
		}

		#endregion


		#region commands

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
		#endregion

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

	#region Messages

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
	#endregion
}
