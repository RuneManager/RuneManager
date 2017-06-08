using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RunePlugin;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace RuneService
{
	class SWProxy
	{
		ProxyServer proxyServer;

		private Dictionary<Guid, string> trackedRequests = new Dictionary<Guid, string>();

		public EventHandler<SWEventArgs> SWResponse;

		private ICollection<SWPlugin> plugins;

		private List<string> skipHosts = new List<string>() { "216.58.199.46", // Wifi check
			"pasta.esfile.duapps.com", "analytics.app-adforce.jp", "push.qpyou.cn", "activeuser.qpyou.cn", "mlog.appguard.co.kr" // SW init
		};

		public SWProxy()
		{
			proxyServer = new ProxyServer();
		}

		public void StartProxy()
		{
			proxyServer.BeforeRequest += OnRequest;
			proxyServer.BeforeResponse += OnResponse;

			// TODO: allow rebinding / more endpoints
			var endpoint = new TransparentProxyEndPoint(IPAddress.Any, 8080, false);
			proxyServer.AddEndPoint(endpoint);
			proxyServer.Start();

			LoadPlugins("plugins");
		}

		private void LoadPlugins(string path)
		{
			if (!Directory.Exists(path))
			{
				Console.WriteLine("No plugin directory");
				return;
			}

			try
			{
				string monData = File.ReadAllText(path + "/monsterNames.json");
				SWReference.MonsterNameMap = JsonConvert.DeserializeObject<Dictionary<string, string>>(monData);
			}
			catch { };

			string[] dllFileNames = Directory.GetFiles(path, "*.dll");

			ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
			foreach (string dllFile in dllFileNames)
			{
				AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
				Assembly assembly = Assembly.Load(an);
				assemblies.Add(assembly);
			}

			Type pluginType = typeof(SWPlugin);
			ICollection<Type> pluginTypes = new List<Type>();
			foreach (Assembly assembly in assemblies)
			{
				if (assembly != null)
				{
					Type[] types = assembly.GetTypes();
					foreach (Type type in types)
					{
						if (type.IsInterface || type.IsAbstract)
						{
							continue;
						}
						else
						{
							//if (type.GetInterface(pluginType.FullName) != null)
							if (pluginType.IsAssignableFrom(type))
							{
								pluginTypes.Add(type);
							}
						}
					}
				}
			}
			plugins = new List<SWPlugin>(pluginTypes.Count);
			foreach (Type type in pluginTypes)
			{
				try
				{
					SWPlugin plugin = (SWPlugin)Activator.CreateInstance(type);
					plugins.Add(plugin);
					plugin.OnLoad();
					this.SWResponse += plugin.ProcessRequest;
					Console.WriteLine($"Successfully loaded plugin: {plugin.GetType().Name}");
				}
				catch (Exception e)
				{
					Console.WriteLine($"Failed loading plugin: {type.Name} with exception: {e.GetType().Name}");
					// TODO: log stacktrace?
				}
			}
		}

		public void UnloadPlugins()
		{
			foreach (var p in plugins)
			{
				try
				{
					this.SWResponse -= p.ProcessRequest;
					p.OnUnload();
				}
				catch (Exception e)
				{
					Console.WriteLine($"Failed unloading plugin: {p.GetType().Name} with exception: {e.GetType().Name}");
					// TODO: log stacktrace?
				}
			}
			plugins.Clear();
		}

		public void Stop()
		{
			UnloadPlugins();

			proxyServer.BeforeRequest -= OnRequest;
			proxyServer.BeforeResponse -= OnResponse;

			proxyServer.Stop();
		}

		private async Task OnRequest(object sender, SessionEventArgs e)
		{
			var requestHeaders = e.WebSession.Request.RequestHeaders;
			
			var method = e.WebSession.Request.Method.ToUpper();
			if ((method == "POST" || method == "PUT" || method == "PATCH"))
			{
				string bodyString = await e.GetRequestBodyAsString();

				if (skipHosts.Contains(e.WebSession.Request.RequestUri.Host))
					return;

				// Typical requests endpoint:
				//http://summonerswar-gb.qpyou.cn/api/gateway_c2.php
				if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("summonerswar") && e.WebSession.Request.RequestUri.AbsoluteUri.Contains("/api/gateway"))
				{
					Console.WriteLine("Request " + e.WebSession.Request.RequestUri.AbsoluteUri);

					var dec = decryptRequest(bodyString, e.WebSession.Request.RequestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1);
					try
					{
						var json = JsonConvert.DeserializeObject<JObject>(dec);
						if (!Directory.Exists("Json"))
							Directory.CreateDirectory("Json");
						File.WriteAllText($"Json\\{json["command"]}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.req.json", dec);
						Console.WriteLine($"Wrote {json["command"]}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}");
					}
					catch { };

					trackedRequests.Add(e.Id, dec);
				}
			}
		}

		private async Task OnResponse(object sender, SessionEventArgs e)
		{
			if (e.WebSession.Request.Method == "GET" || e.WebSession.Request.Method == "POST")
			{
				if (e.WebSession.Response.ResponseStatusCode == "200")
				{
					if (e.WebSession.Response.ContentType != null)
					{

						string body = await e.GetResponseBodyAsString();

						if (skipHosts.Contains(e.WebSession.Request.RequestUri.Host))
							return;
						
						if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("summonerswar") && e.WebSession.Request.RequestUri.AbsoluteUri.Contains("/api/gateway"))
						{
							Console.WriteLine("Response " + e.WebSession.Request.RequestUri.AbsoluteUri);

							var dec = decryptResponse(body, e.WebSession.Request.RequestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1);

							try
							{
								var json = JsonConvert.DeserializeObject<JObject>(dec);
								if (!Directory.Exists("Json"))
									Directory.CreateDirectory("Json");
								File.WriteAllText($"Json\\{json["command"]}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.resp.json", dec);
								Console.WriteLine($"Wrote {json["command"]}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}");

								if (json["command"].Equals("GetNoticeChat"))
								{
									var version = Assembly.GetExecutingAssembly().GetName().Version;
									var jobj = new JObject();
									// Add the proxy version number to chat notices to remind people.
									jobj["message"] = "====[PROXY v" + version + "]====";
									(json["notice_list"] as JArray).Add(jobj);
									await e.SetResponseBodyString(JsonConvert.SerializeObject(json));
								}
							}
							catch { };

							if (trackedRequests.ContainsKey(e.Id))
							{
								System.Threading.Thread thr = new System.Threading.Thread(() =>
								{
									SWEventArgs args = null;
									try
									{
										args = new SWEventArgs(trackedRequests[e.Id], dec);

										SWResponse?.Invoke(this, args);
									}
									catch (Exception ex)
									{
										Console.WriteLine($"Failed triggering plugins with exception: {ex.GetType().Name}");
										// TODO: log stacktrace?
									}
									trackedRequests.Remove(e.Id);
								});
								thr.Start();
							}
						}
					}
				}
			}
		}

		private static string decryptRequest(string bodyString, int version = 1)
		{
			var inMsg = Convert.FromBase64String(bodyString);
			return Encoding.Default.GetString(decryptMessage(inMsg, version));
		}

		private static string decryptResponse(string bodystring, int version = 1)
		{
			var inMsg = Convert.FromBase64String(bodystring);
			var outMsg = decryptMessage(inMsg, version);
			return zlibDecompressData(outMsg);
		}

		// Use inbuilts for zlib Decomp
		// http://stackoverflow.com/questions/17212964/net-zlib-inflate-with-net-4-5
		private static string zlibDecompressData(byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes, 2, bytes.Length - 2))
			using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
			using (var streamReader = new StreamReader(inflater))
			{
				return streamReader.ReadToEnd();
			}
		}

		private static byte[] decryptMessage(byte[] bodyString, int version = 1)
		{
			byte[] key;
			switch (version)
			{
				case 1:
					byte[] kek = new byte[] { 13, 122, 63, 6, 125, 13, 39, 120, 60, 124, 47, 36, 45, 113, 122, 18 };
					key = kek.Select(b => (byte)(b ^ 0x1248)).ToArray();
					break;
				case 2:
					byte[] kek2 = new byte[] { 15, 58, 124, 27, 122, 45, 33, 6, 36, 127, 50, 57, 125, 5, 58, 29 };
					key = kek2.Select(b => (byte)(b ^ 0x1248)).ToArray();
					break;
				default:
					throw new NotImplementedException($"Decrypting version {version} is unsupported.");
			}

			return Decrypt(bodyString, key, new byte[16]);
		}
		
		public static byte[] Decrypt(byte[] buff, byte[] key, byte[] iv)
		{
			using (RijndaelManaged rijndael = new RijndaelManaged())
			{
				rijndael.Padding = PaddingMode.PKCS7;
				rijndael.Mode = CipherMode.CBC;
				rijndael.KeySize = key.Length * 8;
				ICryptoTransform decryptor = rijndael.CreateDecryptor(key, iv);
				MemoryStream memoryStream = new MemoryStream(buff);
				CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
				byte[] output = new byte[buff.Length];
				int readBytes = cryptoStream.Read(output, 0, output.Length);
				return output.Take(readBytes).ToArray();
			}
		}
	}
}