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
#if TEST_SLAVE
using SocketSender;
#endif

namespace RuneApp
{
	public abstract class PageRenderer
	{
		public abstract HttpResponseMessage Render(HttpListenerRequest req, string[] uri);

		protected virtual HttpResponseMessage Recurse(HttpListenerRequest req, string[] uri)
		{
			if (uri.Length == 0)
				return null;

			var types = this.GetType().GetNestedTypes();
			var atypes = types.Where(t => (t.GetCustomAttribute(typeof(Master.PageAddressRenderAttribute)) is Master.PageAddressRenderAttribute));
			var type = atypes.FirstOrDefault(t => (t.GetCustomAttribute(typeof(Master.PageAddressRenderAttribute)) as Master.PageAddressRenderAttribute).PositionalString == uri.First());
			if (type != null)
			{
				var tt = type.GetConstructor(new Type[] { }).Invoke(new object[] { });
				return (HttpResponseMessage)type.GetMethod("Render").Invoke(tt, new object[] { req, uri.Skip(1).ToArray() });
			}
			return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("<html><body>404: " + req.RawUrl + " not found. <a href='/'>Return.</a></body></html>") };
		}
	}

	public class ServedResult
	{
		public string name;

		public bool isList = false;
		public Dictionary<string, ServedResult> contentDic = new Dictionary<string, ServedResult>();
		public List<ServedResult> contentList = new List<ServedResult>();

		public ServedResult()
		{
		}

		public ServedResult(bool listify) : base()
		{
			isList = listify;
		}
		
		public ServedResult(string n) : base()
		{
			name = n;
			if (name == "script")
				contentList.Add("");
		}

		public virtual string ToJson()
		{
			if (isList)
				return "[" + string.Join(",", string.Join("", contentList.Select(li => li.ToJson()))) + "]";
			else
				return "{" + string.Join(",", contentDic.Select(kv => '"' + kv.Key + "\":" + kv.Value.ToJson())) + "}";
		}

		public virtual string ToHtml()
		{
			return "<" + name 
					+ (contentDic.Count > 0 ? " " : "") + string.Join(" ", contentDic.Select(kv => kv.Key + "=" + kv.Value)) 
					+ (contentList.Count == 0 ? "/" : "") + ">" + (contentList.Count > 0 ? "\n" : "") 
					+ string.Join("\n", contentList.Select(li => li.ToHtml())) + (contentList.Count > 0 ? "\n" : "")
					+ (contentList.Count > 0 ? ("</" + name + ">") : "");
		}
		
		public static implicit operator ServedResult(string rhs)
		{
			return (ServedString)rhs;
		}
	}

	public class ServedString : ServedResult
	{
		readonly string value = null;

		public ServedString(string v)
		{
			value = v;
		}

		public static implicit operator ServedString(string rhs)
		{
			return new ServedString(rhs);
		}

		public static implicit operator string(ServedString rhs)
		{
			return rhs.value;
		}

		public override string ToHtml()
		{
			return value;
		}

		public override string ToJson()
		{
			return "\"" + value + "\"";
		}

		public override string ToString()
		{
			return value;
		}
	}

	/// <summary>
	/// Connects and manages slaves.
	/// Also acts as the server for the remote management app.
	/// </summary>
	public class Master : PageRenderer
	{
#if !TEST_SLAVE
		public log4net.ILog Log { get { return Program.log; } }
#else
		public Logger Log { get { return Program.log; } }
#endif

		private static readonly RRMResponse genericResponseBad = new GeneralResponse() { ResponseCode = 400, Message = "Request Failed", Exception = new ArgumentException("Method failed to read request.") };
		private static readonly RRMResponse genericResponseGood = new GeneralResponse() { ResponseCode = 200, Message = "Request Succeeded" };

		private HttpListener listener;

		bool isRunning = false;

		public static string currentTheme = "css/none.css";

		/// <summary>
		/// Dispatches a thread to listen for the incoming Remote App connection
		/// </summary>
		public void Start()
		{
			Task.Factory.StartNew(() =>
			{
				IPAddress ipAddress = IPAddress.Any;

				Log.Info($"Starting TCP listener on {ipAddress}");

				listener = new HttpListener();
				listener.Prefixes.Add("http://*:7676/");
				listener.Start();
				Log.Info("Server is listening on " + listener.Prefixes.First());

				isRunning = true;

				while (isRunning)
				{
					Log.Info("Waiting for a connection...");
					var context = listener.GetContext();
					new Thread(() => RemoteManageLoop(context)).Start();
				}
			});
		}

		public async void RemoteManageLoop(HttpListenerContext context)
		{
			var req = context.Request;
			var resp = context.Response;

			var msg = getResponse(req);
			resp.StatusCode = (int)msg.StatusCode;
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
				return returnPage(false, null, "Check my thingo!<br/>",
					new ServedResult("a") { contentDic = { { "href", "api" } }, contentList = { "Api docs" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "runes" } }, contentList = { "Rune list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "builds" } }, contentList = { "Monster list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "builds" } }, contentList = { "Build list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "loads" } }, contentList = { "Load list" } }, "<br/>",
					new ServedResult("a") { contentDic = { { "href", "css" } }, contentList = { "Choose a theme!" } }, "<br/>",
					renderMagicList(),
					"<br/>");
			}
			else
			{
				return Recurse(req, uri);
			}
		}

		ServedResult renderMagicList()
		{
			// return all completed loads on top, in progress build, unrun builds, mons with no builds
			ServedResult list = new ServedResult("ul");
			list.contentList = new List<ServedResult>();

			var ll = Program.loads;
			var bb = Program.builds.Where(b => !ll.Any(l => l.BuildID == b.ID));
			var mm = Program.data.Monsters.Where(m => !ll.Any(l => bb.FirstOrDefault(b => b.ID == l.BuildID)?.mon == m) && !bb.Any(b => b.mon == m));

			list.contentList.AddRange(ll.Select(l => new ServedResult("li") { contentList = { "load " + Program.builds.FirstOrDefault(b => b.ID == l.BuildID)?.mon.Name } }));
			list.contentList.AddRange(bb.Select(b => new ServedResult("li") { contentList = { "build " + b.MonName } }));
			list.contentList.AddRange(mm.Select(m => new ServedResult("li") { contentList = { "mon " + m.Name } }));

			return list;
		}

		protected static HttpResponseMessage returnPage(bool asJson = false, ServedResult[] head = null, params ServedResult[] body)
		{
			var hh = new ServedResult("head") { contentList = {
					new ServedResult("title") { contentList = { "TopKek" } },
					new ServedResult("meta") {contentDic = { { "http-equiv", "\"Content-Type\"" }, { "content", "\"text/html; charset=utf-8\""} } },
					new ServedResult("link") { contentDic = { { "rel", "\"stylesheet\"" }, { "type", "\"text/css\"" }, { "href", "\"" + Master.currentTheme + "\"" } } },
					
				} };
			if (head != null)
				hh.contentList.AddRange(head);
			var bb = new ServedResult("body");
			bb.contentList.AddRange(body);
			var rr = new ServedResult("html") { contentList = { hh, bb } };
			if (asJson)
				return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(rr.ToJson()) };
			return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<!DOCTYPE html>\r\n" + rr.ToHtml()) };
		}

		[PageAddressRender("runes")]
		public class RuneRenderer : PageRenderer
		{
			private static global::System.Resources.ResourceManager resourceMan;
			internal static global::System.Resources.ResourceManager ResourceManager
			{
				get
				{
					if (object.ReferenceEquals(resourceMan, null))
					{
						global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RuneApp.Runes", typeof(Runes).Assembly);
						resourceMan = temp;
					}
					return resourceMan;
				}
			}

			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				if (uri.Length > 0 && uri[0].Contains(".png"))
				{
					var res = uri[0].Replace(".png", "").ToLower();
					try
					{
						using (var stream = new MemoryStream())
						{
							var mgr = ResourceManager;
							var obj = mgr.GetObject(res, null);
							var img = (System.Drawing.Bitmap)obj;
							img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
							//return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };

							return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FileContent(res, stream.ToArray(), "image/png") };
						}
					}
					catch (Exception e)
					{
						Program.log.Error(e.GetType() + " " + e.Message);
					}
				}

				var resp = this.Recurse(req, uri);
				if (resp != null)
					return resp;

				var rcont = new ServedResult("div")
				{
					contentDic = { { "class", "\"rune-container\"" } },
				};
				rcont.contentList.AddRange(Program.data.Runes
					.Where(r => r != null)
					.OrderByDescending(r => calcSort(r))
					.ThenByDescending(r => r.Efficiency * (12 - Math.Min(12, r.Level)))
					.Select(r => renderRune(r)).ToArray());

				return returnPage(false, new ServedResult[] {
					new ServedResult("link") { contentDic = { { "rel", "\"stylesheet\"" }, { "type", "\"text/css\"" }, { "href", "\"/css/runes.css\"" } } },
					new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" } },
					contentList = { @"function showhide(id) {
	var ee = document.getElementById(id);
	if (ee.style.display == 'none')
		ee.style.display = 'block';
	else
		ee.style.display = 'none';
}" } } }, rcont
					);
			}

			private double calcSort(RuneOptim.Rune r)
			{
				if (r == null)
					return 0;
				Monster m = null;
				if (!r.manageStats.GetOrAdd("Mon", 0).EqualTo(0))
				{
					m = Program.data.GetMonster((ulong)r.manageStats["Mon"]);
				}

				Build b = null;
				if (m != null)
				{
					b = Program.builds.FirstOrDefault(bu => bu.mon == m);
				}
				double ret = r.manageStats?.GetOrAdd("bestBuildPercent", 0) ?? 0;

				ret *= r.Efficiency;
				ret /= (b?.priority ?? 0 + 100);
				ret *= 1+Math.Sqrt(r.manageStats.GetOrAdd("LoadFilt", 0)/(r.manageStats.GetOrAdd("LoadGen", 0) + 1000));
				ret *= 10000;

				return ret;
			}

			private ServedResult renderRune(RuneOptim.Rune r)
			{
				var ret = new ServedResult("div") { contentDic = { { "class", "\"rune-box\"" } } };
				var mainspan = new ServedResult("span") { contentList = {
						new ServedResult("a") {  contentDic = {
								{ "href", "\"javascript:showhide(" +r.Id.ToString() + ")\"" }
							},
							contentList = { "+" }
						},
						" " + " " + r.Main.Value + " " + r.Main.Type + " +" + r.Level + " (" + r.manageStats?.GetOrAdd("bestBuildPercent", 0).ToString("0.##") + ")"
					}
				};
				var runebackName = "normal";
				switch (r.Rarity)
				{
					case 4:
						mainspan.contentDic.Add("style", "\"color: darkorange\"");
						runebackName = "legend";
						break;
					case 3:
						mainspan.contentDic.Add("style", "\"color: purple\"");
						runebackName = "hero";
						break;
					case 2:
						mainspan.contentDic.Add("style", "\"color: cornflourblue\"");
						runebackName = "rare";
						break;
					case 1:
						mainspan.contentDic.Add("style", "\"color: limegreen\"");
						runebackName = "magic";
						break;

				}
				ret.contentList.Add(mainspan);
				var hidediv = new ServedResult("div") { contentDic = { { "id", '"' + r.Id.ToString() + '"' } } };
				if (r.Level == 15 || (r.Slot % 2 == 1 && r.Level >= 12))
					hidediv.contentDic.Add("style", "\"display:none\"");
				//hidediv.contentList.Add("<img src=\"/runes/" + r.Set.ToString() + ".png\" style=\"position:relative;left:1em;height:2em;\" />");
				//hidediv.contentList.Add("<img src=\"/runes/rune" + r.Slot.ToString() + ".png\" style=\"z-index:-1;position:relative;left:-2em;\" />");



				hidediv.contentList.Add(
					new ServedResult("div") { contentDic = { { "class", "\"rune-icon rune-icon-back rune-back " + runebackName + "\""}, }, contentList = {
						new ServedResult("div") { contentDic = { { "class", "\"rune-icon rune-icon-body rune-body rune-slot" + r.Slot + "\""},  }, contentList = {
							new ServedResult("div") { contentDic = { { "class", "\"rune-icon rune-icon-set rune-set " + r.Set + "\""}, }, contentList = { " " } }
							}
						}
					}
				});

				if (r.Innate != null && r.Innate.Type != RuneOptim.Attr.Null)
				{
					hidediv.contentList.Add(new ServedResult("div") {contentDic = { { "class", "\"rune-box-right\""} },
						contentList = {
							new ServedResult("div") { contentDic = { { "class", "\"rune-prop rune-sub rune-innate\"" } },
								contentList = {
									"+" + r.Innate.Type + " " + r.Innate.Value
								} },
							new ServedResult("div") { contentDic = { { "class", "\"monster-name rune-prop rune-monster-name\"" } },
								contentList = { new ServedResult("a") { contentDic = { { "href", "\"monsters/" + r.AssignedName + "\"" } }, contentList = { r.AssignedName } }
								} }
						} });
				}
				hidediv.contentList.Add("<br/>");
				for (int i = 0; i < 4; i++)
				{
					if (r.Subs == null || r.Subs.Count <= i || r.Subs[i].Type == Attr.Null)
						continue;
					var s = r.Subs[i];
					hidediv.contentList.Add(new ServedResult("span") { contentDic = { { "class", "\"rune-prop rune-sub rune-sub" + i + "\"" } },
						contentList = { "+" + s.Value + " " + s.Type } });
					hidediv.contentList.Add(new ServedResult("br"));
				}
				ret.contentList.Add(hidediv);
				return ret;
			}
		}

		[PageAddressRender("css")]
		public class CssRenderer : PageRenderer
		{
			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				var resp = this.Recurse(req, uri);
				if (resp != null)
					return resp;

				return returnPage(false, null,
					"Select a theme<br/>",
					new ServedResult("a") { contentDic = { { "href", "/" } }, contentList = { "Return Home" } },
					"<br/>",
					new ServedResult("button") { contentDic = { { "onclick", "javascript:window.location.href=\"/css/set?theme=/css/none.css\"" } }, contentList = { "Reset to default" } },
					new ServedResult("iframe") { contentDic = { { "src", "css/light.html" }, { "style", "display:block;" } } },
					new ServedResult("button") { contentDic = { { "onclick", "javascript:window.location.href=\"/css/set?theme=/css/light.css\"" } }, contentList = { "Use this theme" } },
					new ServedResult("iframe") { contentDic = { { "src", "css/dark.html" }, { "style", "display:block;" } } },
					new ServedResult("button") { contentDic = { { "onclick", "javascript:window.location.href=\"/css/set?theme=/css/dark.css\"" } }, contentList = { "Use this theme" } }
				);
			}

			[PageAddressRender("preview.html")]
			public class ThemePreviewer : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return returnPage(false, null, new ThemePreview());
				}
			}

			public class ThemePreview : ServedResult
			{
				public override string ToJson()
				{
					return "HTML previewer";
				}

				public override string ToHtml()
				{
					return @"Theme preview!<br/>
<button>Click</button><br/>
<a href=""" + Guid.NewGuid() + @""" class=""link"">Sample Link</a>&nbsp;<a class=""link_visited"" href=""/css"">Visited link</a><br/>
<br/>
<ul><li>List item</li>
<li>Another</li></ul>
";
				}
			}

			[PageAddressRender("set")]
			public class SetCss : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					currentTheme = req.getHeadOrParam("theme");
					return new HttpResponseMessage(HttpStatusCode.SeeOther) { Headers = { { "Location", "/" } } };
				}
			}

			[PageAddressRender("swagger.css")]
			public class SwaggerCss : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText("swagger.css")) };
				}
			}

			[PageAddressRender("light.html")]
			public class LightPreview : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return returnPage(false, new[] { new ServedResult("link") { contentDic = { { "rel", "stylesheet" }, { "type", "text/css" }, { "href", "light.css" } } }}, new ThemePreview());
				}
			}

			[PageAddressRender("runes.css")]
			public class RuneCss : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					var cssStr = new StringBuilder(@"
@media only screen and (min-resolution: 192dpi),
	   only screen and (min-resolution: 2dppx) {
	body {
		font-size: 1.5em;
	}
}
.rune-container {
	display: flex;
	flex-wrap: wrap;
	justify-content: space-between;
	align-items: flex-start;
}
.rune-box {
	display: inline-flex;
	flex-direction: column;
	border: 1px solid black;
	border-radius: 5pt;
	margin: 1pt;
	padding: 1pt;
}
.rune-icon {
	text-align: center;
	vertical-align: middle;
	background-position-x: center;
	background-position-y: center;
	background-repeat-x: no-repeat;
	background-repeat-y: no-repeat;
}
.rune-icon-back {
	display: inline-block;
	background-size: 100%;
}
.rune-icon-body {
	background-size: 100%;
}
.rune-icon-set {
	background-size: 55%;
	height: 3em;
	width: 3em;
}
.rune-box-right {
	display: inline-block;
	vertical-align: middle;
}
.rune-innate {
	font-style: italic;
}
.rune-monster-name {
	text-align: right;
}
");
					foreach (var s in new string[] { "normal", "magic", "rare", "hero", "legend" })
						cssStr.Append(@"
.rune-back." + s + @" {
	background-image: url(/runes/bg_" + s + @".png);
}");
					for (int i = 1; i < 7; i++)
						cssStr.Append(@"
					
.rune-body.rune-slot" + i + @" {
	background-image: url(/runes/rune" + i + @".png);
}");
					foreach (RuneSet rs in Rune.RuneSets)
					{
						cssStr.Append(@"
.rune-set." + rs + @" {
	background-image: url(/runes/" + rs + @".png);
}");
					}

					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(cssStr.ToString()) };
				}
			}

			[PageAddressRender("none.css")]
			public class NoneTheme : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"") };
				}
			}
			[PageAddressRender("light.css")]
			public class LightTheme : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"
body{background:#ccc;color:#733;}
") };
				}
			}

			[PageAddressRender("dark.html")]
			public class DarkPreview : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return returnPage(false, new[] { new ServedResult("link") { contentDic = { { "rel", "stylesheet" }, { "type", "text/css" }, { "href", "dark.css" } } }}, new ThemePreview());
				}
			}

			[PageAddressRender("dark.css")]
			public class DarkTheme : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"
body{background:#000;color:#ccc;}
a,.link{color:#aaf;}
a:visited,.link_visited{color:#88c;}
") };
				}
			}
		}

		[PageAddressRender("api")]
		public class ApiRenderer : PageRenderer
		{
			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				var resp = this.Recurse(req, uri);
				if (resp != null)
					return resp;

				return returnPage(false, 
					new ServedResult[] {
						new ServedResult("link") { contentDic = { { "type", "\"text/css\"" }, { "rel", "\"stylesheet\"" }, { "href", "\"/css/swagger.css\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"http://code.jquery.com/jquery-1.8.0.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/underscore.js/1.3.3/underscore-min.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/handlebars.js/1.0.0/handlebars.min.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"https://cdnjs.cloudflare.com/ajax/libs/backbone.js/0.9.2/backbone-min.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"/scripts/swagger.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"/scripts/swagger-ui.js\"" } } },
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" }, { "src", "\"/scripts/swagger-client.js\"" } } },
						/*
	spec: JSON.parse('" + "{ \"swagger\": \"2.0\", \"info\": { \"description\": \"This is a sample server Petstore server.  You can find out more about Swagger at [http://swagger.io](http://swagger.io) or on [irc.freenode.net, #swagger](http://swagger.io/irc/).  For this sample, you can use the api key `special-key` to test the authorization filters.\", \"version\": \"1.0.0\", \"title\": \"Swagger Petstore\", \"termsOfService\": \"http://swagger.io/terms/\", \"contact\": { \"email\": \"apiteam@swagger.io\" }, \"license\": { \"name\": \"Apache 2.0\", \"url\": \"http://www.apache.org/licenses/LICENSE-2.0.html\" } }, \"host\": \"petstore.swagger.io\", \"basePath\": \"/v2\", \"tags\": [{ \"name\": \"pet\", \"description\": \"Everything about your Pets\", \"externalDocs\": { \"description\": \"Find out more\", \"url\": \"http://swagger.io\" } }, { \"name\": \"store\", \"description\": \"Access to Petstore orders\" }, { \"name\": \"user\", \"description\": \"Operations about user\", \"externalDocs\": { \"description\": \"Find out more about our store\", \"url\": \"http://swagger.io\" } }], \"schemes\": [\"http\"], \"paths\": { \"/pet\": { \"post\": { \"tags\": [\"pet\"], \"summary\": \"Add a new pet to the store\", \"description\": \"\", \"operationId\": \"addPet\", \"consumes\": [\"application/json\", \"application/xml\"], \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"in\": \"body\", \"name\": \"body\", \"description\": \"Pet object that needs to be added to the store\", \"required\": true, \"schema\": { \"$ref\": \"#/definitions/Pet\" } }], \"responses\": { \"405\": { \"description\": \"Invalid input\" } }, \"security\": [{ \"petstore_auth\": [\"write:pets\", \"read:pets\"] }] }, \"put\": { \"tags\": [\"pet\"], \"summary\": \"Update an existing pet\", \"description\": \"\", \"operationId\": \"updatePet\", \"consumes\": [\"application/json\", \"application/xml\"], \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"in\": \"body\", \"name\": \"body\", \"description\": \"Pet object that needs to be added to the store\", \"required\": true, \"schema\": { \"$ref\": \"#/definitions/Pet\" } }], \"responses\": { \"400\": { \"description\": \"Invalid ID supplied\" }, \"404\": { \"description\": \"Pet not found\" }, \"405\": { \"description\": \"Validation exception\" } }, \"security\": [{ \"petstore_auth\": [\"write:pets\", \"read:pets\"] }] } }, \"/user/{username}\": { \"put\": { \"tags\": [\"user\"], \"summary\": \"Updated user\", \"description\": \"This can only be done by the logged in user.\", \"operationId\": \"updateUser\", \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"name\": \"username\", \"in\": \"path\", \"description\": \"name that need to be updated\", \"required\": true, \"type\": \"string\" }, { \"in\": \"body\", \"name\": \"body\", \"description\": \"Updated user object\", \"required\": true, \"schema\": { \"$ref\": \"#/definitions/User\" } }], \"responses\": { \"400\": { \"description\": \"Invalid user supplied\" }, \"404\": { \"description\": \"User not found\" } } }, \"delete\": { \"tags\": [\"user\"], \"summary\": \"Delete user\", \"description\": \"This can only be done by the logged in user.\", \"operationId\": \"deleteUser\", \"produces\": [\"application/xml\", \"application/json\"], \"parameters\": [{ \"name\": \"username\", \"in\": \"path\", \"description\": \"The name that needs to be deleted\", \"required\": true, \"type\": \"string\" }], \"responses\": { \"400\": { \"description\": \"Invalid username supplied\" }, \"404\": { \"description\": \"User not found\" } } } } }, \"securityDefinitions\": { \"petstore_auth\": { \"type\": \"oauth2\", \"authorizationUrl\": \"http://petstore.swagger.io/oauth/dialog\", \"flow\": \"implicit\", \"scopes\": { \"write:pets\": \"modify pets in your account\", \"read:pets\": \"read your pets\" } }, \"api_key\": { \"type\": \"apiKey\", \"name\": \"api_key\", \"in\": \"header\" } }, \"definitions\": { \"Pet\": { \"type\": \"object\", \"required\": [\"name\", \"photoUrls\"], \"properties\": { \"id\": { \"type\": \"integer\", \"format\": \"int64\" }, \"category\": { \"$ref\": \"#/definitions/Category\" }, \"name\": { \"type\": \"string\", \"example\": \"doggie\" }, \"photoUrls\": { \"type\": \"array\", \"xml\": { \"name\": \"photoUrl\", \"wrapped\": true }, \"items\": { \"type\": \"string\" } }, \"tags\": { \"type\": \"array\", \"xml\": { \"name\": \"tag\", \"wrapped\": true }, \"items\": { \"$ref\": \"#/definitions/Tag\" } }, \"status\": { \"type\": \"string\", \"description\": \"pet status in the store\", \"enum\": [\"available\", \"pending\", \"sold\"] } }, \"xml\": { \"name\": \"Pet\" } } }, \"externalDocs\": { \"description\": \"Find out more about Swagger\", \"url\": \"http://swagger.io\" } }" + @"'),
						 * */
						new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" } }, contentList = { @"setTimeout(function() {
window.swaggerUi = new SwaggerUi({
	url: ""http://" + req.UserHostName + @"/api/swagger.json"",
	dom_id: ""swagger-ui-container"",
	supportedSubmitMethods: [""get"", ""post"", ""put"", ""delete""],
	useJQuery: true,
	onComplete: function(swaggerApi, swaggerUi) {
	},
	onFailure: function(data) {
		console.log(""Unable to Load SwaggerUI"");
	},
	docExpansion: ""list"",
	sorter : ""alpha""
});

window.swaggerUi.load();
document.getElementById(""out"").className = ""swagger-section"";
document.getElementById(""out"").innerHTML = ""<div class=\""swagger-ui-wrap\"" id=\""swagger-ui-container\"">"" + document.getElementById(""out"").innerHTML + ""</div>"";
},10);" } },
					},
					new ServedResult("div") { contentDic = { {  "id", "\"out\"" } }, contentList = { " " } });
			}

			[PageAddressRender("swagger.json")]
			public class SwaggerJsRenderer : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new ServedResult() {
						contentDic =
						{
							{ "swagger", "2.0" },
							{ "info", new ServedResult()
								{
									contentDic =
									{
										{ "description", "RuneManager sample Swag" },
										{ "version", Assembly.GetExecutingAssembly().ImageRuntimeVersion },
										{ "title", "RuneManager API" },
										{ "contact", "skibisky@outlook.com.au" }
									}
								}
							},
							{ "host", req.UserHostName },
							{ "schemes", new ServedResult(true) { contentList = { "http" } } },
							{ "paths", new ServedResult()
								{
									contentDic =
									{
										{ "/pet", new ServedResult()
											{
											contentDic =
												{ { "post", new ServedResult()
												{
												contentDic =
													{
														{"summary", "Add a new pet" }
													}
												} }
												}
											}
										}	
									}
								}
							}
						}
					}.ToJson()) };
				}
			}


			[PageAddressRender("rune")]
			public class RuneRenderer : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
				}
			}
		}

		[PageAddressRender("scripts")]
		public class ScriptRenderer : PageRenderer
		{
			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				var resp = this.Recurse(req, uri);
				if (resp != null)// && resp.StatusCode != HttpStatusCode.NotFound)
					return resp;
				
				// allows downloading files
				if (uri.Length > 0 && File.Exists(uri[0]))
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText(uri[0])) };
				}

				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(@"function nextProgress() {
	var xmlHttp = new XMLHttpRequest();
	xmlHttp.onreadystatechange = function() {
		if (xmlHttp.readyState == 4 && xmlHttp.status == 200) {
			document.getElementById('button1').setAttribute('style', 'width:' + (Number(xmlHttp.responseText) + 50) + 'px;');
			console.log(xmlHttp.responseText);
			if (xmlHttp.responseText < 100) {
				setTimeout(nextProgress, 20);
			}
		}
	}
	xmlHttp.open('GET', '/api/getProgress?id=1', true); // true for asynchronous 
	xmlHttp.send(null);
}")
				};
			}

			[PageAddressRender("swagger.js")]
			public class SwaggerRenderer : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText("swagger.js")) };
				}
			}

			[PageAddressRender("swagger-ui.js")]
			public class SwaggerUiRenderer : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText("swagger-ui.js")) };
				}
			}

			[PageAddressRender("swagger-client.js")]
			public class SwaggerClientRenderer : PageRenderer
			{
				public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
				{
					return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(File.ReadAllText("swagger-client.js")) };
				}
			}

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

	public static class ServerExtensions
	{
		public static string getHeadOrParam(this HttpListenerRequest req, string name)
		{
			if (req.QueryString.AllKeys.Contains(name))
				return req.QueryString[name];

			if (req.Headers.AllKeys.Contains(name))
				return req.Headers[name];

			return null;
		}
	}

	public class FileContent : ByteArrayContent
	{
		public readonly string FileName;
		public readonly string Type;

		public FileContent(string fname, byte[] content, string type = "application/octet-stream") : base(content)
		{
			this.FileName = fname;
			this.Type = type;
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
