using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using RuneOptim;
using RuneOptim.swar;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {
        [PageAddressRender("css")]
        public class CssRenderer : PageRenderer {
            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                var themeSet = Themes.Themes.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);

                if (uri.Length > 0 && uri[0].Contains(".css")) {
                    var theme = themeSet.OfType<DictionaryEntry>().FirstOrDefault(kv => kv.Key.ToString() == uri[0].Replace(".css", ""));
                    if (theme.Key != null)
                        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(theme.Value.ToString()) };
                }

                var resp = this.Recurse(req, uri);
                if (resp != null)
                    return resp;

                var sr = new List<ServedResult>
                {
                    "Select a theme<br/>",
                    new ServedResult("a") { contentDic = { { "href", "/" } }, contentList = { "Return Home" } },
                    "<br/>",
                    new ServedResult("button") { contentDic = { { "onclick", "javascript:window.location.href=\"/css/set?theme=/css/none.css\"" } }, contentList = { "Reset to default" } }
                };

                foreach (DictionaryEntry t in themeSet) {
                    sr.Add(new ServedResult("iframe") { contentDic = { { "src", "\"css/" + t.Key + ".html\"" }, { "style", "\"display:block;\"" } }, contentList = { " " } });
                    sr.Add(new ServedResult("button") {
                        contentDic = { { "onclick", "\"javascript:window.location.href='/css/set?theme=/css/" + t.Key + ".css'\"" } },
                        contentList = { "Use this theme" }
                    });
                }

                return returnHtml(null, sr.ToArray());
            }

            [PageAddressRender("preview.html")]
            public class ThemePreviewer : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return returnHtml(null, new ThemePreview());
                }
            }

            public class ThemePreview : ServedResult {
                public override string ToJson() {
                    return "HTML previewer";
                }

                public override string ToHtml() {
                    return InternalServer.theme_preview.Replace("{guid}", new Guid().ToString());
                }
            }

            [PageAddressRender("set")]
            public class SetCss : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    currentTheme = req.getHeadOrParam("theme");
                    return new HttpResponseMessage(HttpStatusCode.SeeOther) { Headers = { { "Location", "/" } } };
                }
            }

            [PageAddressRender("swagger.css")]
            public class SwaggerCss : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Swagger.Swagger.swagger_css) };
                }
            }

            [PageAddressRender("runes.css")]
            public class RuneCss : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    var cssStr = new StringBuilder(InternalServer.runes_css);

                    foreach (var s in new string[] { "normal", "magic", "rare", "hero", "legend" })
                        cssStr.Append("\r\n.rune-back." + s + " {\r\n\tbackground-image: url(/runes/bg_" + s + ".png);\r\r}");

                    for (int i = 1; i < 7; i++)
                        cssStr.Append("\r\n.rune-body.rune-slot" + i + " {\r\n\tbackground-image: url(/runes/rune" + i + ".png);\r\n}");

                    foreach (RuneSet rs in Rune.RuneSets)
                        cssStr.Append("\r\n.rune-set." + rs + " {\r\n\tbackground-image: url(/runes/" + rs + ".png);\r\n}");

                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(cssStr.ToString()) };
                }
            }

            [PageAddressRender("light.html")]
            public class LightPreview : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return returnHtml(new[] { new ServedResult("link") { contentDic = { { "rel", "stylesheet" }, { "type", "text/css" }, { "href", "light.css" } } } }, new ThemePreview());
                }
            }

            [PageAddressRender("none.css")]
            public class NoneTheme : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"") };
                }
            }

            [PageAddressRender("dark.html")]
            public class DarkPreview : PageRenderer {
                public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                    return returnHtml(new[] { new ServedResult("link") { contentDic = { { "rel", "stylesheet" }, { "type", "text/css" }, { "href", "dark.css" } } } }, new ThemePreview());
                }
            }
        }

    }
}
