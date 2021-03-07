using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using RuneOptim;
using RuneOptim.BuildProcessing;
using RuneOptim.Management;
using RuneOptim.swar;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {
        [PageAddressRender("monsters")]
        public class MonstersRenderer : PageRenderer {
            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                if (uri.Length == 0) {
                    return returnHtml(new ServedResult[]{
                        new ServedResult("link") { contentDic = { { "rel", "\"stylesheet\"" }, { "type", "\"text/css\"" }, { "href", "\"/css/runes.css\"" } } },
                        new ServedResult("script")
                        {
                            contentDic = { { "type", "\"application/javascript\"" } },
                            contentList = { $@"function showhide(id) {{
    var ee = document.getElementById(id);
    if (ee.style.display == 'none')
        ee.style.display = 'block';
    else
        ee.style.display = 'none';
}}
function popBox(id, ev, ele) {{
if (ev.button == 0 && !ev.ctrlKey && !ev.shiftKey && !ev.altKey) {{
        var frame = document.getElementById(""frame"");
        frame.src = ""monsters/"" + id;
        frame.style.position = ""absolute"";
        frame.style.left = ""500px"";
        frame.style.display = 'block';
        frame.style.top = (ele.getBoundingClientRect().top + window.scrollY) + ""px"";
        ev.preventDefault();
    }}
}}
function closeBox() {{
    window.location.refresh();
}}
" }
                        }
                    }, renderMagicList(), new ServedResult("iframe") { contentDic = { { "id", "frame" }, { "style", "'display: none;'" } } });
                }

                if (uri.Length > 0 && uri.Any(f => f.Contains(".png"))) {
                    var res = uri.FirstOrDefault(f => f.Contains(".png")).Replace(".png", "");//.ToLower();
                    try {
                        using (var stream = new MemoryStream()) {
                            var img = Program.GetMonPortrait(int.Parse(res));
                            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            //return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };

                            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FileContent(res, stream.ToArray(), "image/png"), Headers = { { "Cache-Control", "public, max-age=31536000" } } };
                        }
                    }
                    catch (Exception e) {
                        Program.LineLog.Error(e.GetType() + " " + e.Message);
                    }
                }

                ulong mid = 0;
                if (ulong.TryParse(uri[0], out mid)) {
                    if (Program.data == null)
                        return returnHtml(null, "missingdata");
                    // got Monster Id
                    var m = Program.data.GetMonster(mid);
                    if (m == null)
                        return returnHtml(null, "missingno");
                    return returnHtml(new ServedResult[] {
                        new ServedResult("script") {
                            contentDic = { { "type", "\"application/javascript\"" } },
                            contentList = { $@"function performAction(action, params) {{
var http = new XMLHttpRequest();
var url = ""../api/monsters/{mid}?action="" + action;
http.open(""POST"", url, true);

//Send the proper header information along with the request
http.setRequestHeader(""Content-type"", ""application/json"");
http.setRequestHeader(""Accept"", ""application/json"");

http.onreadystatechange = function() {{//Call a function when the state changes.
    if (http.readyState == 4 && http.status == 200) {{
        window.location.reload();
    }}
}}
http.send(params);
                }}" } }
                    }, renderMonster(m));
                }
                else {
                    if (Program.data == null)
                        return returnHtml(null, "missingdata");

                    var m = Program.data.GetMonster(uri[0]);
                    if (m != null)
                        return new HttpResponseMessage(HttpStatusCode.SeeOther) { Headers = { { "Location", "monsters/" + m.Id } } };
                }
                return return404();
            }
        }

        #region Monster Management
        static ServedResult renderMonster(Monster mon) {
            var div = new ServedResult("div") {
                contentList = {
                    renderMonLink(mon, new ServedResult("img") { contentDic = { { "src", $"\"monsters/{mon.monsterTypeId}.png\"" }, { "width", "50px" } } }, LinkRenderOptions.All ^ LinkRenderOptions.Portrait, false),
                    new ServedResult("br"),
                    new ServedResult("span") { contentList = { mon.Id.ToString() } },
                    new ServedResult("br"),
                }
            };
            if (Program.goals.ReservedIds.Contains(mon.Id))
                div.contentList.Add(new ServedResult("button") { contentDic = { { "onclick", "javascript:performAction('unreserve');" } }, contentList = { "Unreserve" } });
            else
                div.contentList.Add(new ServedResult("button") { contentDic = { { "onclick", "javascript:performAction('reserve');" } }, contentList = { "Reserve" } });

            if (Program.goals.NoSkillIds.Contains(mon.Id)) {
                div.contentList.Add(new ServedResult("br"));
                div.contentList.Add(new ServedResult("button") { contentDic = { { "onclick", "javascript:performAction('doskill');" } }, contentList = { "Not Skilling" } });
                div.contentList.Add(new ServedImage($"../images/mon.png"));
            }
            else {
                div.contentList.Add(new ServedResult("br"));
                div.contentList.Add(new ServedResult("button") { contentDic = { { "onclick", "javascript:performAction('noskill');" } }, contentList = { "Skilling" } });
                div.contentList.Add(new ServedImage($"../images/toMon.png"));
            }

            return div;
            //return (m.Locked ? "L " : "") + m.FullName + " " + m.Id, new ServedResult("img") { contentDic = { { "src", $"\"monsters/{m.monsterTypeId}.png\"" } } };
        }
        #endregion

        #region Monster List
        static ServedResult renderMagicList() {
            // return all completed loads on top, in progress build, unrun builds, mons with no builds
            ServedResult list = new ServedResult("ul");
            list.contentList = new List<ServedResult>();
            if (Program.data == null)
                return "no";

            var pieces = Program.data.InventoryItems.Where(i => i.Type == ItemType.SummoningPieces)
                .Select(p => new InventoryItem() { Id = p.Id, Quantity = p.Quantity, Type = p.Type, WizardId = p.WizardId }).ToDictionary(p => p.Id);
            foreach (var p in pieces) {
                pieces[p.Key].Quantity -= Save.getPiecesRequired(p.Value.Id);
            }
            pieces = pieces.Where(p => p.Value.Quantity > Save.getPiecesRequired(p.Value.Id)).ToDictionary(p => p.Key, p => p.Value);

            var ll = Program.loads;
            var ldic = ll.ToDictionary(l => l.BuildID);

            var bq = Program.builds.Where(b => !ldic.ContainsKey(b.ID));
            var bb = new List<Build>();
            foreach (var b in bq) {
                if (!bb.Any(u => u.MonId == b.MonId))
                    bb.Add(b);
            }

            var bdic = bb.ToDictionary(b => b.MonId);

            var remids = Program.goals.ReservedIds.Where(i => Program.data.GetMonster(i) == null).ToList();
            foreach (var i in remids) {
                Program.goals.ReservedIds.Remove(i);
            }
            var reserved = Program.goals.ReservedIds.Select(id => Program.data.GetMonster(id)).Where(m => m != null).ToList();
            //reserved = reserved.Union(Program.data.Monsters.Where(m => !m.Locked && !bdic.ContainsKey(m.Id) && MonsterStat.FindMon(m).isFusion).GroupBy(m => m.monsterTypeId).Select(m => m.First())).Distinct();

            var monunNull = Program.data.Monsters.Where(m => m != null);


            var fuss = monunNull.Where(m => !m.Locked && !bdic.ContainsKey(m.Id) && MonsterStat.FindMon(m).isFusion).OrderByDescending(m => m.awakened).ThenByDescending(m => m.Grade).ThenByDescending(m => m.level);
            // TODO: pull these from goals
            var nKfg = 1;// 6 - fuss.Count(m => m.Id.ToString().StartsWith("173") && m.Element == Element.Wind);
            var nJojo = 1;
            var dict = new Dictionary<string, int>();

            // TODO: get fuse recipes from somewhere else
            foreach (var m in fuss) {
                var idk = m.monsterTypeId.ToString().Substring(0, 3);
                if (!dict.ContainsKey(idk))
                    dict.Add(idk, 0);
                var mname = m.monsterTypeId.ToString();
                if (dict[idk] < nKfg && !reserved.Contains(m)) {
                    if ((mname.StartsWith("110") && m.Element == Element.Fire) || 
                        (mname.StartsWith("102") && m.Element == Element.Water) || 
                        (mname.StartsWith("195") && m.Element == Element.Wind) || 
                        (mname.StartsWith("160") && m.Element == Element.Wind)) {
                        reserved.Add(m);
                        dict[idk]++;
                    }
                }
                if (dict[idk] < nJojo && !reserved.Contains(m)) {
                    if ((mname.StartsWith("154") && m.Element == Element.Fire) || 
                        (mname.StartsWith("140") && m.Element == Element.Fire) || 
                        (mname.StartsWith("114") && m.Element == Element.Water) || 
                        (mname.StartsWith("132") && m.Element == Element.Wind)) {
                        reserved.Add(m);
                        dict[idk]++;
                    }
                }
                
                if (!reserved.Any(r => r.monsterTypeId.ToString().Substring(0, 3) == m.monsterTypeId.ToString().Substring(0, 3)))
                    reserved.Add(m);
            }

            var mm = monunNull.Where(m => !bdic.ContainsKey(m.Id)).Except(reserved);

            var locked = monunNull.Where(m => m.Locked).Union(bb.Select(b => b.Mon).Where(m => m != null)).Except(reserved).ToList();
            var unlocked = monunNull.Except(locked).Except(reserved).ToList();

            var trashOnes = unlocked.Where(m => m.Grade == 1 && !m.Name.Contains("Devilmon") && !m.FullName.Contains("Angelmon")).ToList();
            unlocked = unlocked.Except(trashOnes).ToList();

            var pairs = new Dictionary<Monster, List<Monster>>();
            var rem = new List<Monster>();
            foreach (var m in locked.Where(m => !Program.goals.NoSkillIds.Contains(m.Id)).OrderByDescending(m => 1 / (bb.FirstOrDefault(b => b.Mon == m)?.Priority ?? m.priority - 0.1)).ThenByDescending(m => m.Grade)
                .ThenByDescending(m => m.level)
                .ThenBy(m => m.Element)
                .ThenByDescending(m => m.awakened)
                .ThenBy(m => m.loadOrder)) {
                pairs.Add(m, new List<Monster>());
                int i = Math.Min(m.Grade, m.SkillupsTotal - m.SkillupsLevel);
                for (; i > 0; i--) {
                    Monster um = null;
                    if (m.level == m.Grade * 5 + 10)
                        um = unlocked
                             .Where(ul => (ul.Grade == m.Grade && ul.level == 1) || ul.Grade == m.Grade - 1)
                             .OrderByDescending(ul => ul.level)
                             .FirstOrDefault(ul => ul.monsterTypeId.ToString().Substring(0, 3) == m.monsterTypeId.ToString().Substring(0, 3));
                    else
                        um = unlocked
                                .Where(ul => ul.level == 1)
                                .OrderBy(ul => ul.Grade)
                                .FirstOrDefault(ul => ul.monsterTypeId.ToString().Substring(0, 3) == m.monsterTypeId.ToString().Substring(0, 3));

                    if (um == null)
                        break;
                    pairs[m].Add(um);
                    rem.Add(um);
                    unlocked.Remove(um);
                }
            }
            foreach (var m in locked.Where(m => !Program.goals.NoSkillIds.Contains(m.Id)).OrderByDescending(m => 1 / (bb.FirstOrDefault(b => b.Mon == m)?.Priority ?? m.priority - 0.1)).ThenByDescending(m => m.Grade)
                .ThenByDescending(m => m.level)
                .ThenBy(m => m.Element)
                .ThenByDescending(m => m.awakened)
                .ThenBy(m => m.loadOrder)) {
                if (!pairs.ContainsKey(m))
                    pairs.Add(m, new List<Monster>());
                int i = m.SkillupsTotal - m.SkillupsLevel - pairs[m].Count;
                for (; i > 0; i--) {
                    Monster um = null;
                    if (m.level == m.Grade * 5 + 10)
                        um = unlocked
                             .OrderByDescending(ul => ul.Grade == m.Grade && ul.level == 1)
                             .ThenByDescending(ul => ul.Grade == m.Grade - 1)
                             .ThenByDescending(ul => ul.level)
                             .FirstOrDefault(ul => ul.monsterTypeId.ToString().Substring(0, 3) == m.monsterTypeId.ToString().Substring(0, 3));
                    else
                        um = unlocked
                                .Where(ul => ul.level == 1)
                                .OrderBy(ul => ul.Grade)
                                .FirstOrDefault(ul => ul.monsterTypeId.ToString().Substring(0, 3) == m.monsterTypeId.ToString().Substring(0, 3));

                    if (um == null)
                        break;
                    pairs[m].Add(um);
                    rem.Add(um);
                    unlocked.Remove(um);
                }
                bool[] zerop = new bool[5];
                while (i > 0 && !zerop.All(p => p)) {
                    for (int j = 1; j < 6; j++) {
                        int monbase = (m.monsterTypeId / 100) * 100;
                        if (pieces.ContainsKey(monbase + j) && pieces[monbase + j].Quantity >= Save.getPiecesRequired(pieces[monbase + j].Id)) {
                            pieces[monbase + j].Quantity -= Save.getPiecesRequired(pieces[monbase + j].Id);
                            pairs[m].Add(new Monster() { Element = pieces[monbase + j].Element, Name = pieces[monbase + j].Name + " Pieces (" + pieces[monbase + j].Quantity + " remain)" });
                            i--;
                        }
                        else
                            zerop[j - 1] = true;
                        if (i <= 0)
                            break;
                    }

                }
            }

            mm = mm.Except(bb.Select(b => b.Mon));
            mm = mm.Except(Program.builds.Select(b => b.Mon));
            mm = mm.Except(pairs.SelectMany(p => p.Value));
            mm = mm.Except(rem);

            trashOnes = trashOnes.Concat(mm.Where(m => !pairs.ContainsKey(m) && !m.Locked && !m.Name.Contains("Devilmon") && !m.FullName.Contains("Angelmon"))).ToList();
            mm = mm.Except(trashOnes);

            mm = mm.OrderByDescending(m => !unlocked.Contains(m))
                .ThenByDescending(m => m.Locked)
                .ThenByDescending(m => m.Grade)
                .ThenByDescending(m => m.level)
                .ThenBy(m => m.Element)
                .ThenByDescending(m => m.awakened)
                .ThenBy(m => m.loadOrder)
                ;

            list.contentList.AddRange(ll.Select(l => renderLoad(l, pairs)));

            list.contentList.AddRange(bb.Select(b => {
                var m = b.Mon;
                var nl = new ServedResult("ul");
                var li = new ServedResult("li")
                {
                    contentList = {
                        new ServedResult("span") { contentList = { renderMonLink(m, "build") } }, nl }
                };
                if (pairs.ContainsKey(m))
                    nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { renderMonLink(mo, "- ", LinkRenderOptions.Grade | LinkRenderOptions.Level) } }));
                if (nl.contentList.Count == 0)
                    nl.name = "br";
                return li;
            }
            ));

            list.contentList.AddRange(mm.Select(m => {
                var nl = new ServedResult("ul");
                var stars = new StringBuilder();
                for (int s = 0; s < m.Grade; s++)
                    stars.Append("<img class='star' src='/runes/star_unawakened.png' >"); //style='left: -" + (0.3*s) + "em' 
                var li = new ServedResult("li")
                {
                    contentList = { ((unlocked.Contains(m)) ?
                    renderMonLink(m, "TRASH: ", LinkRenderOptions.Grade | LinkRenderOptions.Level) :
                    renderMonLink(m, "mon")), nl }
                };
                if (!unlocked.Contains(m) && pairs.ContainsKey(m))
                    nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { renderMonLink(mo, "- ", LinkRenderOptions.Grade | LinkRenderOptions.Level) } }));
                if (nl.contentList.Count == 0)
                    nl.name = "br";
                return li;
            }));
            list.contentList.AddRange(reserved.GroupBy(mg => mg.monsterTypeId).Select(mg => {
                var li = new ServedResult("li");
                li.contentList.Add(new ServedResult(mg.AtLeast(2) ? "a" : "span") {
                    contentDic = { { "href", "\"javascript:showhide('g" + mg.First().Id + "')\"" } },
                    contentList = { "Reserved:" }
                });
                li.contentList.Add(renderMonLink(mg.First(), mg.Count() + "x ", LinkRenderOptions.All));
                if (mg.AtLeast(2)) {
                    li.contentList.Add(new ServedResult("ul") {
                        contentDic = {
                            { "id", "g" + mg.First().Id.ToString() },
                            { "style" , "'display: none;'"}
                        },
                        contentList = mg.Skip(1).Select(m => {
                            return new ServedResult("li") { contentList = { renderMonLink(m) } };
                        }).ToList()
                    });
                }
                return li;
            }));

            //
            var food = trashOnes.OrderBy(t => t.Grade).ThenBy(t => t.Element).ThenByDescending(t => t.monsterTypeId).Select(m => new Food() { mon = m, fakeLevel = m.Grade }).ToList();
            food = makeFood(2, food);
            food = makeFood(3, food);
            food = makeFood(4, food);

            list.contentList.AddRange(food.OrderByDescending(f => f.food.Count).ThenByDescending(f => f.fakeLevel).Select(f => recurseFood(f)).ToList());

            return list;
        }

        static ServedResult recurseFood(Food f) {
            ServedResult sr = new ServedResult("li");
            sr.contentList.Add(renderMonLink(f.mon, null, LinkRenderOptions.None));
            sr.contentList.Add(" " + f.mon.Grade + "*" + "L" + f.mon.level + (f.mon.Grade != f.fakeLevel ? " > " + f.fakeLevel + "*" : ""));
            if (f.food.Any()) {
                var rr = new ServedResult("ul");
                foreach (var o in f.food) {
                    rr.contentList.Add(recurseFood(o));
                }
                sr.contentList.Add(rr);
            }
            return sr;
        }

        static List<Food> makeFood(int lev, List<Food> food) {
            var outFood = new List<Food>();
            Food current = null;
            while (food.Any(f => f.fakeLevel == lev - 1)) {
                if (current == null) {
                    if (food.Count(f => f.fakeLevel == lev - 1) <= lev - 1)
                        break;
                    current = food.OrderByDescending(f => f.mon.level).FirstOrDefault(f => f.fakeLevel == lev - 1);
                    if (current == null)
                        break;
                    food.Remove(current);
                    outFood.Add(current);
                    current.fakeLevel = lev;
                }
                else {
                    // only eat things which are 1 or upgraded
                    var tfood = food.Where(f => f.mon.level == 1 || f.fakeLevel != f.mon.Grade).FirstOrDefault(f => f.fakeLevel == lev - 1);
                    if (tfood == null)
                        break;
                    if (current.food.Count(f => f.fakeLevel == lev - 1) >= lev - 1) {
                        current = null;
                    }
                    else {
                        current.food.Add(tfood);
                        food.Remove(tfood);
                    }
                }
            }

            return outFood.Concat(food).ToList();
        }

        class Food {
            public Monster mon;
            public List<Food> food = new List<Food>();
            public int fakeLevel;
        }

        [Flags]
        public enum LinkRenderOptions {
            None = 0,
            Portrait = 1,
            Grade = 2,
            Level = 4,
            Skillups = 8,
            Locked = 16,
            All = Portrait | Grade | Level | Locked | Skillups
        }
        public static ServedResult renderMonLink(Monster m, ServedResult prefix = null, LinkRenderOptions renderOptions = LinkRenderOptions.All, bool linkify = true) {
            if (m?.Name == null)
                return new ServedResult("span") { contentList = { "error" } };
            var res = new ServedResult("span");
            if (prefix != null)
                res.contentList.Add(prefix);
            if ((renderOptions & LinkRenderOptions.Portrait) == LinkRenderOptions.Portrait)
                res.contentList.Add(new ServedResult("img") { contentDic = { { "class", "\"monster-profile\"" }, { "style", "\"height: 2em;\"" }, { "src", $"\"monsters/{m.monsterTypeId}.png\"" } } });
            var str = m.FullName;
            var suff = "";
            if (m.Name.Contains("Pieces")) {
                res.contentList.Add(new ServedResult("span") {
                    contentList = { str }
                });
            }
            else {
                if ((renderOptions & LinkRenderOptions.Grade) == LinkRenderOptions.Grade)
                    str += " " + m.Grade + "*";
                if ((renderOptions & LinkRenderOptions.Level) == LinkRenderOptions.Level)
                    str += " " + m.level;
                res.contentList.Add(new ServedResult(linkify ? "a" : "span") {
                    contentDic = {
                        { "id", m.Id.ToString() },
                        { "href", "'monsters/" + m.Id + "'" },
                        { "onclick", "'popBox(" + m.Id + ", event, this)'" },
                    },
                    contentList = { str }
                });
                if ((renderOptions & LinkRenderOptions.Locked) == LinkRenderOptions.Locked)
                    suff += (m.Locked ? " <span class=\"locked\">L</span>" : "");
                if ((renderOptions & LinkRenderOptions.Skillups) == LinkRenderOptions.Skillups)
                    suff += " " + m.SkillupsLevel + "/" + m.SkillupsTotal;
            }

            res.contentList.Add(suff);
            return res;
        }

        protected static ServedResult renderLoad(Loadout l, Dictionary<Monster, List<Monster>> pairs) {
            var b = Program.builds.FirstOrDefault(bu => bu.ID == l.BuildID);
            var m = b.Mon;

            var li = new ServedResult("li");

            // render name
            var span = new ServedResult("span")
            {
                contentList = {
                    new ServedResult("a") { contentDic = { { "href", "\"javascript:showhide(" +m.Id.ToString() + ")\"" } }, contentList = { "+ load" } },
                    " ",
                    renderMonLink(m)
            }
            };

            // render rune swaps
            var div = new ServedResult("div")
            {
                contentDic = { { "id", '"' + m.Id.ToString() + '"' },
                    //{ "class", "\"rune-container\"" }
            }
            };
            foreach (var r in l.Runes) {
                if (r == null) continue;
                var rd = RuneRenderer.renderRune(r);
                var hide = rd.contentList.FirstOrDefault(ele => ele.contentDic.Any(pr => pr.Value.ToString().Contains(r.Id.ToString() + "_")));
                hide.contentDic["style"] = (r.AssignedId == m.Id) ? "\"display:none;\"" : "";
                div.contentList.Add(rd);
            }
            if (l.Runes.All(r => r == null || r.AssignedId == m.Id))
                div.contentDic.Add("style", "\"display:none;\"");

            li.contentList.Add(span);
            li.contentList.Add(div);

            // list skillups
            if (pairs.ContainsKey(m)) {
                var nl = new ServedResult("ul");
                nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { renderMonLink(mo, "- ", LinkRenderOptions.Grade | LinkRenderOptions.Level) } }));
                if (nl.contentList.Count == 0)
                    nl.name = "br";
                li.contentList.Add(nl);
            }

            return li;
        }
        #endregion
    }
}