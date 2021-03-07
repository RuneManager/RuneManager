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
using TrotiNet;

namespace RuneService
{
    class SWProxy
    {
        public EventHandler<SWEventArgs> SWResponse;

        private ICollection<SWPlugin> plugins;

        private static ulong[] whitelistDebugWizards = new ulong[] { 12168103 };

        private static List<string> skipHosts = new List<string>() { "216.58.199.46", // Wifi check
            "pasta.esfile.duapps.com", "analytics.app-adforce.jp", "push.qpyou.cn", "activeuser.qpyou.cn", "mlog.appguard.co.kr" // SW init
        };

        private static List<string> blacklistHosts = new List<string> {
            "hmma.baidu.com", "conf.international.baidu.com", "rts.mobula.sdk.duapps.com", "www.estrongs.com",
        };

        private int listenPort = 8080;
        private TcpServer server;
        
        public SWProxy() { }

        public void Start() {
            LoadPlugins(AppDomain.CurrentDomain.BaseDirectory + "plugins");

            if (Program.Args.ContainsKey("port")) {
                listenPort = (int)Program.Args["port"];
            }
            
            server = new TcpServer(listenPort, false) { BindAddress = IPAddress.Any /* Overrides ipv6=false */ };
            ProxyHandler.Plugins = SWResponse;  // :(
            server.Start(ProxyHandler.OnConnection);

            List<string> ips = new List<string>();
            foreach (var eth in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()) {
                if (eth.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                if (eth.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;
                foreach (var ip in eth.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
                        ips.Add("[" + ip.Address + "]");
                    }
                    else {
                        ips.Add(ip.Address.ToString());
                    }
                }
            }
            ips.Sort();
            foreach (var ip in ips)
                Console.WriteLine("Proxy listening on " + ip + ":" + listenPort);

            server.InitListenFinished.WaitOne();
            if (server.InitListenException != null) throw server.InitListenException;

            //while (true)
            //  System.Threading.Thread.Sleep(1000);
            //server.Stop();
        }

        public void Stop() {
            UnloadPlugins();
            server.Stop();
        }

        private void LoadPlugins(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("No plugin directory: " + path);
                return;
            }
            Console.WriteLine("Loading plugins from " + path);
            try
            {
                string monData = File.ReadAllText(path + "/data/monsterNames.json");
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
                SWPlugin plugin = null;
                try
                {
                    plugin = (SWPlugin)Activator.CreateInstance(type);
                    plugin.OnLoad();
                    plugins.Add(plugin);
                    this.SWResponse += plugin.ProcessRequest;
                    Console.WriteLine($"Successfully loaded plugin: {plugin.GetType().Name}");
                }
                catch (Exception e)
                {
                    if (plugin != null) {
                        plugins.Remove(plugin);
                        this.SWResponse -= plugin.ProcessRequest;
                    }
                    Console.WriteLine($"Failed loading plugin: {type.Name} with exception: {e.GetType().Name}: {e.Message}");
#if DEBUG
                    Console.WriteLine(e.StackTrace);
#endif
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

        class ProxyHandler : BaseProxy {
            public static EventHandler<SWEventArgs> Plugins { get; internal set; }
            private string decRequest;
            private Uri requestUri;
            private JObject req;
            private System.Threading.Timer timer;

            public ProxyHandler(HttpSocket clientSocket) : base(clientSocket) {
            }

            private void OnExpire(object state) {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId + " -- " + this.requestUri.Host + " Expired");
                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                if (SocketBP != null) {
                    SocketBP.CloseSocket();
                    SocketBP = null;
                }
                if (SocketPS != null) {
                    SocketPS.CloseSocket();
                    SocketPS = null;
                }
                State.bPersistConnectionBP = false;
                State.bPersistConnectionPS = false;
                State.NextStep = null;
            }

            public static ProxyHandler OnConnection(HttpSocket clientSocket) {
                return new ProxyHandler(clientSocket);
            }

            protected override void OnReceiveRequest(HttpRequestLine e) {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId + " -> " + RequestLine
                    + (RequestHeaders.Referer != null ? ", Referer: " + RequestHeaders.Referer : "")
                );
                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                requestUri = RequestLine.Uri;   // NOTE: used by ReceiveResponse
                if (blacklistHosts.Contains(requestUri?.Host))
                    SocketBP.CloseSocket();

                var method = e.Method.ToUpper();
                if (method != "CONNECT") {
                    timer = new System.Threading.Timer(new System.Threading.TimerCallback(OnExpire), null, 300 * 1000, System.Threading.Timeout.Infinite);
                }
                if ((method == "POST" || method == "PUT" || method == "PATCH")) {
                    if (skipHosts.Contains(e.Uri.Host))
                        return;

                    // Typical requests endpoint:
                    //http://summonerswar-gb.qpyou.cn/api/gateway_c2.php
                    if (e.Uri.AbsoluteUri.Contains("summonerswar") && e.Uri.AbsoluteUri.Contains("/api/gateway")) {
                        string bodyString = Encoding.ASCII.GetString(SocketBP.Buffer, 0, Array.IndexOf(SocketBP.Buffer, (byte)0));
                        bodyString = bodyString.Substring(bodyString.IndexOf("\r\n\r\n"));      // TODO: FIXME: this needs to match first \r?\n\r?\n

                        decRequest = decryptRequest(bodyString, e.Uri.AbsolutePath.Contains("_c2.php") ? 2 : 1);
                        try {
                            req = JsonConvert.DeserializeObject<JObject>(decRequest);
                            if (!Directory.Exists("Json"))
                                Directory.CreateDirectory("Json");
                            File.WriteAllText($"Json\\{req["command"]}" + 
#if DEBUG
                            $"_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}" + 
#endif
                            ".req.json", JsonConvert.SerializeObject(req, Formatting.Indented));
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine($">{req["command"]}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        catch { };
                    }
                }
            }

            protected override void OnReceiveResponse() {
#if DEBUG
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId + " <- " + ResponseStatusLine + ", " + requestUri.AbsoluteUri
                    + (ResponseHeaders.ContentLength != null ? " Content-Length: " + ResponseHeaders.ContentLength.ToString() : "")
                );
                Console.ForegroundColor = ConsoleColor.Gray;
#endif
                if (ResponseStatusLine.StatusCode == HttpStatus.OK && ResponseHeaders.Headers.ContainsKey("content-type")) {
                    if (skipHosts.Contains(requestUri.Host))
                        return;

                    if (requestUri.AbsoluteUri.Contains("summonerswar") && requestUri.AbsoluteUri.Contains("/api/gateway")) {
                        if (RequestLine.Method == "GET" || RequestLine.Method == "POST") {
                            byte[] response = GetContent();

                            // From now on, the default State.NextStep ( == SendResponse()
                            // at this point) must not be called, since we already read
                            // the response.
                            State.NextStep = null;

                            // Decompress the message stream, if necessary
                            Stream stream = GetResponseMessageStream(response);
                            string body;
                            using (var sr = new StreamReader(stream)) {
                                body = sr.ReadToEnd();
                            }
                            SendResponseStatusAndHeaders();
                            SocketBP.TunnelDataTo(TunnelBP, response);

                            var decResponse = decryptResponse(body, requestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1);

                            try {
                                var resp = JsonConvert.DeserializeObject<JObject>(decResponse);
                                if (!Directory.Exists("Json"))
                                    Directory.CreateDirectory("Json");
                                File.WriteAllText($"Json\\{resp["command"]}" + 
#if DEBUG
                                $"_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}" + 
#endif
                                ".resp.json", JsonConvert.SerializeObject(resp, Formatting.Indented));
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.WriteLine($"<{resp["command"]}");
                                Console.ForegroundColor = ConsoleColor.Gray;

                                // only mangle my wizards who want it, don't crash others.
                                if (resp["command"].ToString() == "GetNoticeChat" && whitelistDebugWizards.Contains((ulong)req["wizard_id"])) {
                                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                                    var jobj = new JObject();
                                    // Add the proxy version number to chat notices to remind people.
                                    jobj["message"] = "Proxy version: " + version;
                                    ///(json["notice_list"] as JArray).Add(jobj);
                                    resp["tzone"] = resp["tzone"].ToString().Replace("/", @"\/");

                                    var ver = requestUri.AbsolutePath.Contains("_c2.php") ? 2 : 1;

                                    var inMsg = Convert.FromBase64String(body);
                                    var outMsg = decryptMessage(inMsg, ver);
                                    var decData = zlibDecompressData(outMsg);

                                    var fix = JsonConvert.SerializeObject(resp);
                                    fix = fix.Replace(@"\\", "\\");
                                    var bytes = zlibCompressData(fix);
                                    var str = encryptMessage(bytes, ver);
                                    var send = Convert.ToBase64String(str);

                                    Console.WriteLine("str:" + (fix == decData));
                                    Console.WriteLine("b64:" + (inMsg.SequenceEqual(str)));
                                    Console.WriteLine("cry:" + (outMsg.SequenceEqual(bytes)));
                                    Console.WriteLine("bytes:" + (body.SequenceEqual(send)));

                                    //encryptResponse(fix, ver);
                                    //if (body.SequenceEqual(send))
                                    //  await e.SetResponseBodyString(send);
                                }
                            }
                            catch { };


                            if (decRequest != null) {
                                System.Threading.Thread thr = new System.Threading.Thread(() =>
                                {
                                    SWEventArgs args = null;
                                    try
                                    {
                                        args = new SWEventArgs(decRequest, decResponse);
                                        Plugins?.Invoke(this, args);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Failed triggering plugin {ex.Source} with {ex.GetType().Name}: {ex.Message}");
#if DEBUG
                                        Console.WriteLine(ex.StackTrace);
#endif
                                    }
                                });
                                thr.Start();
                            }
                        }
                    }
                    else if (RequestHeaders.Headers.ContainsKey("accept-encoding")) {
                        if (!ResponseHeaders.Headers.ContainsKey("content-encoding") && !ResponseHeaders.Headers.ContainsKey("transfer-encoding")) {
                            // read response data
                            // compress
                            // add content-encoding header
                            // add Vary: Accept-Encoding
                            // (re)set content-length header
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