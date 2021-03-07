using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuneApp.InternalServer {
    /// <summary>
    /// Connects and manages slaves.
    /// Also acts as the server for the remote management app.
    /// </summary>
    public partial class Master : PageRenderer {
#if !TEST_SLAVE
        [Obsolete("Try LineLog")]
        public static log4net.ILog Log { [DebuggerStepThrough] get { return Program.log; } }
        public static LineLogger LineLog { [DebuggerStepThrough] get { return Program.LineLog; } }
#else
        public Logger Log { get { return Program.log; } }
#endif

        private static readonly Dictionary<string, string> mimeTypes = new Dictionary<string, string>() {
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".ico", "image/x-icon" },
            //{ "*", "text/html" },
        };

        private HttpListener listener;

        bool isRunning = false;

        public static string currentTheme = "/css/none.css";

        /// <summary>
        /// Dispatches a thread to listen for the incoming Remote App connection
        /// </summary>
        public void Start() {
            
            try {
                try {

                    //if (!NetAclChecker.HasFirewall("WebSocketMulti", true, true, true, 80))
                    //  NetAclChecker.AddFirewall("WebSocketMulti", true, true, true, 80);
                    //if (!NetAclChecker.HasFirewall("WebSocketMulti", true, true, true, 81))
                    //  NetAclChecker.AddFirewall("WebSocketMulti", true, true, true, 81);

                    NetAclChecker.AddAddress($"http://*:7676/");


                    listener = new HttpListener();
                    listener.Prefixes.Add("http://*:7676/");
                    listener.Start();
                }
                catch {
                    LineLog.Error("Failed to bind to *, binding to localhost");
                    listener = new HttpListener();
                    listener.Prefixes.Add("http://localhost:7676/");
                    listener.Start();
                }

                LineLog.Info("Server is listening on " + listener.Prefixes.First());
                isRunning = true;
            }
            catch (Exception e) {
                LineLog.Error("Failed to start server", e);
                throw;
            }

            Task.Factory.StartNew(() => {
                try {
                    while (isRunning) {
                        //LineLog.Info("Waiting for a connection...");
                        var context = listener.GetContext();
                        new Thread(() => RemoteManageLoop(context)).Start();
                    }
                }
                catch (Exception e) {
                    LineLog.Error("Failed while running server", e);
                }
                isRunning = false;
            }, TaskCreationOptions.LongRunning);
        }

        public void Stop() {
            listener.Stop();
            listener.Close();

            DateTime start = DateTime.Now;
            while (DateTime.Now - start < new TimeSpan(0, 0, 5)) {
                Thread.Sleep(100);
                if (!isRunning)
                    return;
            }
            throw new TaskCanceledException("Failed to stop server!");
        }

        public async void RemoteManageLoop(HttpListenerContext context) {
            try {
                LineLog.Debug("serving: " + context.Request.RawUrl);
                var req = context.Request;
                var resp = context.Response;

                var msg = getResponse(req);
                resp.StatusCode = (int)msg.StatusCode;
                LineLog.Debug("returning: " + resp.StatusCode);
                foreach (var h in msg.Headers) {
                    foreach (var v in h.Value) {
                        resp.Headers.Add(h.Key, v);
                    }
                }

                if (resp.StatusCode != 303) {
                    using (var output = resp.OutputStream) {
                        byte[] outBytes = Encoding.UTF8.GetBytes("Critical Failure");
                        bool expires = false;
                        if (msg.Content is StringContent) {
                            var qw = msg.Content as StringContent;
                            string qq = await qw.ReadAsStringAsync();
                            resp.ContentType = mimeTypes.Where(t => req.Url.AbsolutePath.EndsWith(t.Key)).FirstOrDefault().Value;
                            if (resp.ContentType == null)
                                resp.ContentType = "text/html";                 // Default to HTML
                            outBytes = Encoding.UTF8.GetBytes(qq);
                        }
                        else if (msg.Content is FileContent) {
                            var qw = msg.Content as FileContent;
                            resp.ContentType = qw.Type;
                            if (resp.ContentType == null)
                                resp.ContentType = "application/octet-stream";  // Should always be set, but bin just incase
                                                                                //resp.Headers.Add("Content-Disposition", $"attachment; filename=\"{qw.FileName}\"");
                            outBytes = await qw.ReadAsByteArrayAsync();
                        }
                        else if (msg.Content is ByteArrayContent) {
                            var qw = msg.Content as ByteArrayContent;
                            resp.ContentType = mimeTypes.Where(t => req.Url.AbsolutePath.EndsWith(t.Key)).FirstOrDefault().Value;
                            if (resp.ContentType == null)
                                resp.ContentType = "application/octet-stream";  // Default to binary
                            outBytes = await qw.ReadAsByteArrayAsync();
                        }
                        else if (msg.Content is StreamContent) {
                            var qw = msg.Content as StreamContent;
                            using (var ms = new MemoryStream()) {
                                var stream = await qw.ReadAsStreamAsync();
                                stream.CopyTo(ms);
                                //resp.ContentType = "application/octet-stream"
                                outBytes = ms.ToArray();
                            }
                        }
                        //resp.Headers.Add("Content-language", "en-au");
                        resp.Headers.Add("Access-Control-Allow-Origin: *");
                        resp.Headers.Add("Access-Control-Allow-Methods: *");

                        if (expires) {
                            resp.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                            resp.Headers.Add("Pragma", "no-cache");
                            resp.Headers.Add("Expires", "Wed, 16 Jul 1969 13:32:00 UTC");
                        }

                        if ((outBytes.Length > 0) && (resp.ContentType != "image/png")) {   // Don't compress empty responses or compressed file types
                            var enc = req.Headers.GetValues("Accept-Encoding");

                            if (enc?.Contains("gzip") ?? false) {
                                using (MemoryStream ms = new MemoryStream())
                                using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress)) {
                                    gs.Write(outBytes, 0, outBytes.Length);
                                    gs.Flush();
                                    gs.Close();     // https://stackoverflow.com/questions/3722192/how-do-i-use-gzipstream-with-system-io-memorystream#comment3929538_3722263
                                    ms.Flush();
                                    outBytes = ms.ToArray();
                                }
                                resp.Headers.Add("Content-Encoding", "gzip");
                            }
                            else if (enc?.Any(a => a.ToLowerInvariant() == "deflate") ?? false) {
                                using (MemoryStream ms = new MemoryStream()) {
                                    using (System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress)) {
                                        ds.Write(outBytes, 0, outBytes.Length);
                                        ds.Flush();
                                    }
                                    ms.Flush();
                                    outBytes = ms.ToArray();
                                }
                                resp.Headers.Add("Content-Encoding", "deflate");
                            }
                        }

                        resp.ContentLength64 = outBytes.Length;
                        try {
                            output.Write(outBytes, 0, outBytes.Length);
                        }
                        catch (Exception ex) {
                            Program.LineLog.Error("Failed to write " + ex.GetType().ToString() + " " + ex.Message);
                        }
                    }
                }
                else {
                    resp.OutputStream.Close();
                }
            }
            catch (Exception e) {
                LineLog.Error("Something went wrong.", e);
                var resp = context.Response;
                resp.StatusCode = 500;
                using (var output = resp.OutputStream) {
                    byte[] outBytes = Encoding.UTF8.GetBytes(e.GetType() + ": " + e.Message);
                    resp.ContentLength64 = outBytes.Length;
                    try {
                        output.Write(outBytes, 0, outBytes.Length);
                    }
                    catch (Exception ex) {
                        Program.LineLog.Error("Failed to write " + ex.GetType().ToString() + " " + ex.Message);
                    }
                }
            }
        }

        public HttpResponseMessage getResponse(HttpListenerRequest req) {
            if (req.AcceptTypes == null) {
                //TODO: topkek return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            }

            var msg = new HttpResponseMessage();

            var ruri = req.RawUrl;
            if (ruri.Contains("?"))
                ruri = ruri.Remove(ruri.IndexOf("?"));
            var locList = ruri.Split('/').ToList();
            locList.RemoveAll(a => a == "");

            msg.StatusCode = HttpStatusCode.OK;
            LineLog.Debug("rendering response...");
            return this.Render(req, locList.ToArray());

            //<html><head><script src='/script.js'></script></head><body><button id='button1' style='width:50px' onclick='javascript:startProgress();'>Start</button></body></html>

        }

        private string getUrlComp(string url, int comp) {
            if (url.Contains("?"))
                url = url.Remove(url.IndexOf("?"));
            var array = url.Split('/');
            if (array.Length > comp && !string.IsNullOrWhiteSpace(array[comp]))
                return array[comp];

            return null;
        }

        #region Address Rendering

        public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
            if (uri == null || uri.Length == 0 || uri[0] == "/") {
                return returnHtml(null, "Check my thingo!<br/>",
                    new ServedResult("a") { contentDic = { { "href", "api" } }, contentList = { "Api docs" } }, "<br/>",
                    new ServedResult("a") { contentDic = { { "href", "runes" } }, contentList = { "Rune list" } }, "<br/>",
                    new ServedResult("a") { contentDic = { { "href", "monsters" } }, contentList = { "Monster list" } }, "<br/>",
                    new ServedResult("a") { contentDic = { { "href", "builds" } }, contentList = { "Build list" } }, "<br/>",
                    new ServedResult("a") { contentDic = { { "href", "loads" } }, contentList = { "Load list" } }, "<br/>",
                    new ServedResult("a") { contentDic = { { "href", "goals" } }, contentList = { "Goal tracking" } }, "<br/>",
                    new ServedResult("a") { contentDic = { { "href", "rift" } }, contentList = { "Rift Best Clear" } }, "<br/>",
                    new ServedResult("a") { contentDic = { { "href", "css" } }, contentList = { "Choose a theme!" } }, "<br/>",
                    "<br/>");
            }
            if (uri != null && uri.Length > 1 && uri[0] == "images" && uri[1].Contains(".png")) {
                var res = uri[1].Replace(".png", "");//.ToLower();
                try {
                    using (var stream = new MemoryStream()) {
                        var mgr = App.ResourceManager;
                        var obj = mgr.GetObject(res, null);
                        var img = (System.Drawing.Bitmap)obj;
                        img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        //return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };

                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FileContent(res, stream.ToArray(), "image/png") };
                    }
                }
                catch (Exception e) {
                    Program.LineLog.Error(e.GetType() + " " + e.Message);
                    return returnException(e);
                }
            }
            else {
                return Recurse(req, uri);
            }
        }

        protected static HttpResponseMessage return404() {
            // TODO:
            return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("404 lol") };
        }

        protected static HttpResponseMessage returnException(Exception e) {
            // TODO:
            return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("500! " + e.GetType() + ": " + e.Message + "<br/>" + e.StackTrace) };
        }

        protected static HttpResponseMessage returnHtml(ServedResult[] head = null, params ServedResult[] body) {
            var bb = new StringBuilder();
            if (body != null)
                foreach (var b in body)
                    bb.AppendLine(b.ToHtml());

            var hh = new StringBuilder();
            if (head != null)
                foreach (var h in head)
                    hh.AppendLine(h.ToHtml());

            var html = InternalServer.default_tpl
            .Replace("{title}", "Rune Manager")
            .Replace("{theme}", Master.currentTheme)
            .Replace("{head}", hh.ToString())
            .Replace("{body}", bb.ToString())
            ;

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html) };
        }

        [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        public class PageAddressRenderAttribute : Attribute {
            readonly string pageAddress;

            public PageAddressRenderAttribute(string pageAddress) {
                this.pageAddress = pageAddress;
            }

            public string PageAddress {
                get { return pageAddress; }
            }
        }

        [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        sealed class HttpMethodAttribute : Attribute {
            readonly string method;

            public HttpMethodAttribute(string method) {
                this.method = method;
            }

            public string Method {
                get { return method; }
            }
        }

        #endregion
    }

}
