using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using RuneApp.Resources;
using RuneOptim;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {
        [PageAddressRender("runes")]
        public class RuneRenderer : PageRenderer {
            private static global::System.Resources.ResourceManager resourceMan;
            internal static global::System.Resources.ResourceManager ResourceManager {
                get {
                    if (object.ReferenceEquals(resourceMan, null)) {
                        global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RuneApp.Resources.Runes", typeof(Runes).Assembly);
                        resourceMan = temp;
                    }
                    return resourceMan;
                }
            }

            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                if (uri.Length > 0 && uri[0].Contains(".png")) {
                    var res = uri[0].Replace(".png", "").ToLower();
                    try {
                        using (var stream = new MemoryStream()) {
                            var mgr = ResourceManager;
                            var obj = mgr.GetObject(res, null);
                            var img = (System.Drawing.Bitmap)obj;
                            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            //return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };

                            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FileContent(res, stream.ToArray(), "image/png") };
                        }
                    }
                    catch (Exception e) {
                        Program.LineLog.Error(e.GetType() + " " + e.Message);
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
                    .ThenByDescending(r => r.BarionEfficiency * (12 - Math.Min(12, r.Level)))
                    .Select(r => renderRune(r)).ToArray());

                return returnHtml(new ServedResult[] {
                    new ServedResult("link") { contentDic = { { "rel", "\"stylesheet\"" }, { "type", "\"text/css\"" }, { "href", "\"/css/runes.css\"" } } },
                    new ServedResult("script") {contentDic = { { "type", "\"text/css\"" } }, contentList = { @"@media only screen and (min-resolution: 192dpi),
       only screen and (min-resolution: 2dppx) {
    body {
        font-size: 1.5em;
    }
}" } },
                    new ServedResult("script") { contentDic = { { "type", "\"application/javascript\"" } },
                    contentList = { @"function showhide(id) {
    var ee = document.getElementById(id);
    if (ee.style.display == 'none')
        ee.style.display = 'block';
    else
        ee.style.display = 'none';
}
function hackLots(prop, num, on) {
    var as = document.getElementsByClassName('rune-details');
    for (var i = 0; i < as.length; i++) {
        as[i].style.display = on ? 'none' : 'block';
    }
    if (on) {
        var es = document.getElementsByClassName(prop + '-' + num);
        for (var i = 0; i < es.length; i++) {
            es[i].style.display = 'block';
        }
    }
}
" } } }, "<a href=\"javascript:hackLots('stars', 6, true);\">show 6*</a>", rcont
                    );
            }

            private double calcSort(RuneOptim.swar.Rune r) {
                if (r == null)
                    return 0;
                Monster m = null;
                if (!r.manageStats.GetOrAdd("Mon", 0).EqualTo(0)) {
                    m = Program.data.GetMonster((ulong)r.manageStats["Mon"]);
                }

                Build b = null;
                if (m != null) {
                    b = Program.builds.FirstOrDefault(bu => bu.Mon == m);
                }
                double ret = r.manageStats?.GetOrAdd("bestBuildPercent", 0) ?? 0;

                ret *= r.BarionEfficiency;
                ret /= (b?.Priority ?? 0 + 100);
                ret *= 1 + Math.Sqrt(r.manageStats.GetOrAdd("LoadFilt", 0) / (r.manageStats.GetOrAdd("LoadGen", 0) + 1000));
                ret *= 10000;

                return ret;
            }

            public static Random rand = new Random();

            public static ServedResult renderRune(RuneOptim.swar.Rune r, bool forceExpand = false) {
                if (r == null)
                    return "";
                var ret = new ServedResult("div") { contentDic = { { "class", "\"rune-box\"" } } };

                string trashId = r.Id.ToString() + "_" + rand.Next();

                var mainspan = new ServedResult("span")
                {
                    contentList = {
                        new ServedResult("a") {  contentDic = {
                                { "href", "\"javascript:showhide('" +trashId + "')\"" }
                            },
                            contentList = { "+" }
                        },
                        " " + " " + r.Main.Value + " " + r.Main.Type + " +" + r.Level + " (" + r.manageStats?.GetOrAdd("bestBuildPercent", 0).ToString("0.##") + ")"
                    }
                };
                // colour name base on level
                switch (r.Level / 3) {
                    case 5:
                    case 4:
                        mainspan.contentDic.Add("style", "\"color: darkorange\"");
                        break;
                    case 3:
                        mainspan.contentDic.Add("style", "\"color: purple\"");
                        break;
                    case 2:
                        mainspan.contentDic.Add("style", "\"color: cornflourblue\"");
                        break;
                    case 1:
                        mainspan.contentDic.Add("style", "\"color: limegreen\"");
                        break;
                }
                // show the proper background
                var runebackName = "normal";
                switch (r.Rarity) {
                    case 4:
                        runebackName = "legend";
                        break;
                    case 3:
                        runebackName = "hero";
                        break;
                    case 2:
                        runebackName = "rare";
                        break;
                    case 1:
                        runebackName = "magic";
                        break;
                }

                ret.contentList.Add(mainspan);
                var hidediv = new ServedResult("div") { contentDic = { { "id", '"' + trashId + '"' }, { "class", '"' + $"rune-details stars-{r.Grade} rarity-{r.Rarity} level-{r.Level} slot-{r.Slot}" + '"' } } };
                if (!forceExpand && (r.Level == 15 || (r.Slot % 2 == 1 && r.Level >= 12)))
                    hidediv.contentDic.Add("style", "\"display:none\"");
                //hidediv.contentList.Add("<img src=\"/runes/" + r.Set.ToString() + ".png\" style=\"position:relative;left:1em;height:2em;\" />");
                //hidediv.contentList.Add("<img src=\"/runes/rune" + r.Slot.ToString() + ".png\" style=\"z-index:-1;position:relative;left:-2em;\" />");



                hidediv.contentList.Add(
                    new ServedResult("div") {
                        contentDic = { { "class", "\"rune-icon rune-icon-back rune-back " + runebackName + "\"" }, },
                        contentList = {
                        new ServedResult("div") { contentDic = { { "class", "\"rune-icon rune-icon-body rune-body rune-slot" + r.Slot + "\""},  }, contentList = {
                            new ServedResult("div") { contentDic = { { "class", "\"rune-icon rune-icon-set rune-set " + r.Set + "\""}, }, contentList = { " " } }
                            }
                        }
                    }
                    });

                var propdiv = new ServedResult("div") { contentDic = { { "class", "\"rune-box-right\"" } } };
                if (r.Innate != null && r.Innate.Type > RuneOptim.swar.Attr.Null) {
                    propdiv.contentList.Add(new ServedResult("div") {
                        contentDic = { { "class", "\"rune-prop rune-sub rune-innate\"" } },
                        contentList = { "+" + r.Innate.Type + " " + r.Innate.Value }
                    });
                }
                propdiv.contentList.Add(new ServedResult("div") {
                    contentDic = { { "class", "\"monster-name rune-prop rune-monster-name\"" } },
                    contentList = { new ServedResult("a") { contentDic = { { "href", "\"monsters/" + r.AssignedName + "\"" } }, contentList = { r.AssignedName } }
                                }
                });
                hidediv.contentList.Add(propdiv);
                hidediv.contentList.Add("<br/>");
                for (int i = 0; i < 4; i++) {
                    if (r.Subs == null || r.Subs.Count <= i || r.Subs[i].Type <= Attr.Null)
                        continue;
                    var s = r.Subs[i];
                    hidediv.contentList.Add(new ServedResult("span") {
                        contentDic = { { "class", "\"rune-prop rune-sub rune-sub" + i + "\"" } },
                        contentList = { "+" + s.Value + " " + s.Type }
                    });
                    hidediv.contentList.Add(new ServedResult("br"));
                }
                ret.contentList.Add(hidediv);
                return ret;
            }
        }

    }
}
