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

		private static ulong[] whitelistDebugWizards = new ulong[]{ 12168103 };

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

			foreach (var eth in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
				if (eth.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
				if (eth.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;
				foreach (var ip in eth.GetIPProperties().UnicastAddresses) {
					if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
						Console.WriteLine("Proxy listening on [" + ip.Address + "]:" + endpoint.Port);
					}
					else {
						Console.WriteLine("Proxy listening on " + ip.Address + ":" + endpoint.Port);
					}
				}
			}
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
			var method = e.WebSession.Request.Method.ToUpper();
			if ((method == "POST" || method == "PUT" || method == "PATCH"))
			{
				if (skipHosts.Contains(e.WebSession.Request.RequestUri.Host))
					return;

				// Typical requests endpoint:
				//http://summonerswar-gb.qpyou.cn/api/gateway_c2.php
				if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("summonerswar") && e.WebSession.Request.RequestUri.AbsoluteUri.Contains("/api/gateway"))
				{
					string bodyString = await e.GetRequestBodyAsString();

					var dec = decryptRequest(bodyString, e.WebSession.Request.RequestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1);
					try
					{
						var json = JsonConvert.DeserializeObject<JObject>(dec);
						if (!Directory.Exists("Json"))
							Directory.CreateDirectory("Json");
						File.WriteAllText($"Json\\{json["command"]}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.req.json", dec);
						Console.ForegroundColor = ConsoleColor.DarkGray;
						Console.WriteLine($">{json["command"]}");
						Console.ForegroundColor = ConsoleColor.Gray;
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
				if (e.WebSession.Response.ResponseStatusCode == "200" && e.WebSession.Response.ContentType != null)
				{
					if (skipHosts.Contains(e.WebSession.Request.RequestUri.Host))
						return;

					if (e.WebSession.Request.RequestUri.AbsoluteUri.Contains("summonerswar") && e.WebSession.Request.RequestUri.AbsoluteUri.Contains("/api/gateway"))
					{

						string body = await e.GetResponseBodyAsString();
						var dec = decryptResponse(body, e.WebSession.Request.RequestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1);
						string req = null;
						if (trackedRequests.ContainsKey(e.Id))
							req = trackedRequests[e.Id];

						try
						{
							var json = JsonConvert.DeserializeObject<JObject>(dec, new JsonSerializerSettings() { Formatting = Formatting.Indented });
							var reqjson = JsonConvert.DeserializeObject<JObject>(req, new JsonSerializerSettings() { Formatting = Formatting.Indented });
							if (!Directory.Exists("Json"))
								Directory.CreateDirectory("Json");
							File.WriteAllText($"Json\\{json["command"]}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.resp.json", dec);
							Console.ForegroundColor = ConsoleColor.DarkGray;
							Console.WriteLine($"<{json["command"]}");
							Console.ForegroundColor = ConsoleColor.Gray;

							// only mangle my wizards who want it, don't crash others.
							if (json["command"].ToString() == "GetNoticeChat" && whitelistDebugWizards.Contains((ulong)reqjson["wizard_id"]))
							{
								var version = Assembly.GetExecutingAssembly().GetName().Version;
								var jobj = new JObject();
								// Add the proxy version number to chat notices to remind people.
								jobj["message"] = "Proxy version: " + version;
								///(json["notice_list"] as JArray).Add(jobj);
								json["tzone"] = json["tzone"].ToString().Replace("/", @"\/");

								var ver = e.WebSession.Request.RequestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1;

								var inMsg = Convert.FromBase64String(body);
								var outMsg = decryptMessage(inMsg, ver);
								var decData = zlibDecompressData(outMsg);

								var fix = JsonConvert.SerializeObject(json);
								fix = fix.Replace(@"\\", "\\");
								var bytes = zlibCompressData(fix);
								var str = encryptMessage(bytes, ver);
								var send = Convert.ToBase64String(str);

								Console.WriteLine("str:" + (fix == decData));
								Console.WriteLine("b64:" + (inMsg.SequenceEqual(str)));
								Console.WriteLine("cry:" + (outMsg.SequenceEqual(bytes)));
								Console.WriteLine("bytes:" + (body.SequenceEqual(send)));

								//encryptResponse(fix, ver);
								if (body.SequenceEqual(send))
									await e.SetResponseBodyString(send);
							}
						}
						catch { };


						if (req != null)
						{
							System.Threading.Thread thr = new System.Threading.Thread(() =>
							{
								SWEventArgs args = null;
								try
								{
									args = new SWEventArgs(req, dec);

									SWResponse?.Invoke(this, args);
								}
								catch (Exception ex)
								{
									Console.WriteLine($"Failed triggering plugin {ex.Source} with exception: {ex.GetType().Name}");
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

		public class Adler32Computer
		{
			private int a = 1;
			private int b = 0;

			public int Checksum
			{
				get
				{
					return ((b * 65536) + a);
				}
			}

			private static readonly int Modulus = 65521;

			public void Update(byte[] data, int offset, int length)
			{
				for (int counter = 0; counter < length; ++counter)
				{
					a = (a + (data[offset + counter])) % Modulus;
					b = (b + a) % Modulus;
				}
			}
		}

		private static byte[] zlibCompressData(string body)
		{
			using (var stream = new MemoryStream())
			{
				using (var inflater = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Compress))
				using (var streamWriter = new StreamWriter(inflater))
				{
					streamWriter.Write(body);
				}
				var res = stream.ToArray();
				/*var ret = new byte[res.Length + 6];
				ret[0] = 120;
				ret[1] = 156;
				res.CopyTo(ret, 2);
				var adl = new Adler32Computer();
				adl.Update(res, 0, res.Length);
				BitConverter.GetBytes(adl.Checksum).CopyTo(ret, 2 + res.Length);*/
				return res;
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
		
		public static byte[] encryptMessage(byte[] bodyString, int version = 1)
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

			return Encrypt(bodyString, key, new byte[16]);
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

		public static byte[] Encrypt(byte[] buff, byte[] key, byte[] iv)
		{
			using (RijndaelManaged rijndael = new RijndaelManaged())
			{
				rijndael.Padding = PaddingMode.PKCS7;
				rijndael.Mode = CipherMode.CBC;
				rijndael.KeySize = key.Length * 8;
				ICryptoTransform decryptor = rijndael.CreateEncryptor(key, iv);
				MemoryStream memoryStream = new MemoryStream();
				CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write);
				byte[] output = new byte[buff.Length];
				//int readBytes = cryptoStream.Read(output, 0, output.Length);
				cryptoStream.Write(buff, 0, buff.Length);
				return memoryStream.ToArray();
			}
		}
	}
}