using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using RuneOptim.swar;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {
        [PageAddressRender("loads")]
        public class LoadsRenderer : PageRenderer {
            List<RuneAction> actList = new List<RuneAction>();

            List<MiniMon> mons = new List<MiniMon>();
            List<MiniRune> runes = new List<MiniRune>();
            List<MiniCraft> crafts = new List<MiniCraft>();

            int BoxNum { get { return mons.Count(m => !m.storage); } }
            int StoreNum { get { return mons.Count(m => m.storage); } }
            int RuneNum { get { return runes.Count(r => r.current == null) + crafts.Count(c => c.grindOn == null); } }

            bool HasStoreSpace { get { return StoreNum < Program.data.WizardInfo.UnitDepositorySlots.Number; } }
            bool HasBoxSpace { get { return BoxNum < Program.data.WizardInfo.UnitSlots.Number; } }
            bool HasRuneSpace { get { return RuneNum < 600; } }



            void AddOut(MiniMon mon) {
                actList.Add(new RuneApp.InternalServer.Master.LoadsRenderer.RuneAction(ActionType.MonOut) { mon = mon.mon });
            }

            void AddIn(MiniMon mon) {
                actList.Add(new RuneApp.InternalServer.Master.LoadsRenderer.RuneAction(ActionType.MonIn) { mon = mon.mon });
            }

            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                if (uri.Length > 0 && uri[0].Contains(".png")) {
                    var res = uri[0].Replace(".png", "").ToLower();
                    try {
                        using (var stream = new MemoryStream()) {
                            var mgr = InternalServer.ResourceManager;
                            var obj = mgr.GetObject(res, null);
                            var img = (System.Drawing.Bitmap)obj;
                            img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            //return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };

                            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FileContent(res, stream.ToArray(), "image/png") };
                        }
                    }
                    catch (Exception e) {
                        Program.LineLog.Error(e.GetType() + " " + e.Message, e);
                    }
                }

                var resp = this.Recurse(req, uri);
                if (resp != null)
                    return resp;

                /* TODO:
                Put completed mons in storage
                Put runes on mon
                    Remove rune from Source Mon
                        Take mon out of storage
                    Remove rune from Target Mon
                        Take mon out of storage

                Group by thingo
                */

                string message = "";

                if (Program.data == null)
                    return return404();

                // make a copy of all the data to mess around with
                foreach (var m in Program.data.Monsters) {
                    mons.Add(new MiniMon() { mon = m, storage = m.inStorage, lockedOut = m.OnDefense || m.IsRep || (m.BuildingId != 0 && !m.inStorage) });
                }
                foreach (var r in Program.data.Runes) {
                    var mr = new MiniRune() { rune = r };
                    if (r.Assigned != null) {
                        mr.current = mons.FirstOrDefault(mm => mm.Id == r.AssignedId);
                        mr.current.runesCurrent[r.Slot - 1] = mr;
                    }
                    runes.Add(mr);
                }
                foreach (var b in Program.builds) {
                    var mm = mons.FirstOrDefault(m => m.Id == b.MonId);
                    if (mm != null)
                        mm.priority = b.Priority;
                }
                foreach (var l in Program.loads) {
                    var b = Program.builds.FirstOrDefault(bb => bb.ID == l.BuildID);
                    foreach (var r in l.Runes) {
                        var mm = mons.FirstOrDefault(mo => mo.Id == b.MonId);
                        mm.runesDesired[r.Slot - 1] = runes.FirstOrDefault(mr => mr.Id == r.Id);
                        mm.runesDesired[r.Slot - 1].desired = mm;
                    }
                }

                while (HasStoreSpace && mons.Any(m => m.ActionsLeft == 0 && !m.storage && !m.lockedOut)) {
                    var mo = mons.FirstOrDefault(m => m.ActionsLeft == 0 && !m.storage && !m.lockedOut);
                    if (mo == null)
                        break;
                    actList.Add(new RuneAction(ActionType.MonIn) {
                        mon = mo.mon,
                        amount = StoreNum,
                        maximum = Program.data.WizardInfo.UnitDepositorySlots.Number,
                        message = "Is Good " + BoxNum + "/" + Program.data.WizardInfo.UnitSlots.Number + ", {0}/{1}"
                    });
                    mo.storage = true;
                }

                int giveUp = mons.Count * 2;
                // while things to do
                while (mons.Any(m => !m.IsGood)) {
                    giveUp--;
                    if (giveUp <= 0)
                        break;

                    // put the mons we don't need into storage
                    if (HasStoreSpace && BoxNum >= Program.data.WizardInfo.UnitSlots.Number / 10 * 9) {
                        while (mons.Any(m => m.ActionsLeft == 0 && !m.storage && !m.lockedOut)) {
                            if (!HasStoreSpace)
                                break;
                            var mo = mons.FirstOrDefault(m => m.ActionsLeft == 0 && !m.storage && !m.lockedOut);
                            if (mo == null)
                                break;
                            actList.Add(new RuneAction(ActionType.MonIn) {
                                mon = mo.mon,
                                amount = StoreNum,
                                maximum = Program.data.WizardInfo.UnitDepositorySlots.Number,
                                message = "Is Good " + BoxNum + "/" + Program.data.WizardInfo.UnitSlots.Number + ", {0}/{1}"
                            });
                            mo.storage = true;
                        }
                    }
                    var monPlist = mons.Where(m => m.ActionsLeft > 0).OrderBy(m => m.Score);
                    if (BoxNum < Program.data.WizardInfo.UnitSlots.Number / 10 * 7) {
                        while (BoxNum < Program.data.WizardInfo.UnitSlots.Number / 10 * 9 - 1 && monPlist.Any(m => m.storage)) {
                            var qqlist = monPlist.OrderByDescending(m => m.runesCurrent.Sum(r => r?.desired == null ? 0 : 1 / (double)r.desired.Score));
                            var tmon = qqlist.FirstOrDefault(m => m.storage);
                            actList.Add(new RuneAction(ActionType.MonOut) {
                                mon = tmon.mon,
                                amount = BoxNum,
                                maximum = Program.data.WizardInfo.UnitSlots.Number,
                                message = "Not Good (" + tmon.ActionsLeft + ") {0}/{1}, " + StoreNum + "/" + Program.data.WizardInfo.UnitDepositorySlots.Number
                            });
                            tmon.storage = false;
                        }
                    }
                    var cmon = monPlist.FirstOrDefault(m => !m.IsGood);
                    if (cmon.storage) {
                        if (!HasBoxSpace) {
                            //message = "No box space";
                            //break;
                        }

                        actList.Add(new RuneAction(ActionType.MonOut) {
                            mon = cmon.mon,
                            amount = BoxNum,
                            maximum = Program.data.WizardInfo.UnitSlots.Number,
                            message = "Not Good (" + cmon.ActionsLeft + ") {0}/{1}, " + StoreNum + "/" + Program.data.WizardInfo.UnitDepositorySlots.Number
                        });
                        cmon.storage = false;
                    }
                    if (!cmon.storage) {
                        while (HasRuneSpace && cmon.runesCurrent.Any(r => r != null && r.current != null && r.desired != cmon)) {
                            var rr = cmon.runesCurrent.FirstOrDefault(r => r != null && r.current != null && r.desired != cmon);
                            actList.Add(new RuneAction(ActionType.RuneOut) { rune = rr.rune, mon = rr.current.mon, amount = RuneNum, maximum = 600, message = "{0}/{1}" });
                            rr.current.runesCurrent[rr.rune.Slot - 1] = null;
                            rr.current = null;
                        }
                    }
                    while (!cmon.storage && cmon.runesDesired.Any(rd => rd.current != cmon)) {

                        var r = cmon.runesDesired.FirstOrDefault(rd => rd.current != cmon);
                        // don't need to take out
                        /*if (r.current?.storage ?? false)
                        {
                            if (!HasBoxSpace)
                            {
                                cmon.priority++;
                                break;
                            }
                            actList.Add(new RuneAction(ActionType.MonOut) { mon = r.current.mon,
                                amount = BoxNum,
                                maximum = Program.data.WizardInfo.UnitSlots.Number,
                                message = "Need Rune {0}/{1}, " + StoreNum + "/" + Program.data.WizardInfo.UnitDepositorySlots.Number });
                            r.current.storage = false;
                        }*/
                        if (r.current != null)//!r.current?.storage ?? false)
                        {
                            if (!HasRuneSpace)
                                break;
                            actList.Add(new RuneAction(ActionType.RuneOut) {
                                rune = r.rune,
                                mon = r.current.mon,
                                amount = RuneNum,
                                maximum = 600,
                                message = "{0}/{1}"
                            });

                            var dmon = r.current;
                            while (RuneNum < 570 && dmon.runesCurrent.Any(rr => rr != null && rr != r && rr.current != null && rr.desired != rr.current)) {
                                var dmr = dmon.runesCurrent.FirstOrDefault(rr => rr != null && rr != r && rr.current != null && rr.desired != rr.current);
                                actList.Add(new RuneAction(ActionType.RuneOut) {
                                    rune = dmr.rune,
                                    mon = dmr.current.mon,
                                    amount = RuneNum,
                                    maximum = 600,
                                    message = "{0}/{1}"
                                });
                                dmr.current.runesCurrent[dmr.rune.Slot - 1] = null;
                                dmr.current = null;
                            }

                            r.current.runesCurrent[r.rune.Slot - 1] = null;
                            r.current = null;
                        }

                        if (r.desired.runesCurrent[r.rune.Slot - 1] != null && r.desired.runesCurrent[r.rune.Slot - 1] != r) {
                            if (!HasRuneSpace)
                                break;
                            actList.Add(new RuneAction(ActionType.RuneOut) {
                                rune = r.desired.runesCurrent[r.rune.Slot - 1].rune,
                                mon = r.desired.mon,
                                amount = RuneNum,
                                maximum = 600,
                                message = "{0}/{1}"
                            });

                            var dmon = r.desired;
                            while (RuneNum < 570 && dmon.runesCurrent.Any(rr => rr != null && rr != r && rr.current != null && rr.desired != rr.current)) {
                                var dmr = dmon.runesCurrent.FirstOrDefault(rr => rr != null && rr != r && rr.current != null && rr.desired != rr.current);
                                actList.Add(new RuneAction(ActionType.RuneOut) {
                                    rune = dmr.rune,
                                    mon = dmr.current.mon,
                                    amount = RuneNum,
                                    maximum = 600,
                                    message = "{0}/{1}"
                                });
                                dmr.current.runesCurrent[dmr.rune.Slot - 1] = null;
                                dmr.current = null;
                            }

                            r.desired.runesCurrent[r.rune.Slot - 1].current = null;
                            r.desired.runesCurrent[r.rune.Slot - 1] = null;
                        }

                        if (r.current == null && r.desired.runesCurrent[r.rune.Slot - 1] == null) {
                            r.current = r.desired;
                            r.desired.runesCurrent[r.rune.Slot - 1] = r;
                            actList.Add(new RuneAction(ActionType.RuneIn) {
                                rune = r.rune,
                                mon = r.desired.mon,
                                amount = RuneNum,
                                maximum = 600,
                                message = r.desired.runesCurrent.Count(rr => rr != null) + "/6 {0}/{1}"
                            });
                        }
                    }
                }

                while (HasStoreSpace && mons.Any(m => m.ActionsLeft == 0 && !m.storage && !m.lockedOut)) {
                    var mo = mons.FirstOrDefault(m => m.ActionsLeft == 0 && !m.storage && !m.lockedOut);
                    if (mo == null)
                        break;
                    actList.Add(new RuneAction(ActionType.MonIn) {
                        mon = mo.mon,
                        amount = StoreNum,
                        maximum = Program.data.WizardInfo.UnitDepositorySlots.Number,
                        message = "Is Good " + BoxNum + "/" + Program.data.WizardInfo.UnitSlots.Number + ", {0}/{1}"
                    });
                    mo.storage = true;
                }


                // shuffle
                int lastMonIn = -1;
                for (int i = 0; i < actList.Count; i++) {
                    i = Math.Max(0, i);
                    var act = actList.ElementAt(i);
                    if (act == actList.Last() || act == actList.First())
                        continue;

                    switch (act.action) {
                        case ActionType.MonIn:
                            lastMonIn = i;
                            break;
                        case ActionType.MonOut:
                            actList.Remove(act);
                            actList.Insert(lastMonIn + 1, act);
                            break;
                        case ActionType.RuneIn:
                            for (var j = i + 1; j < actList.Count; j++) {
                                var next = actList.ElementAt(j);
                                if (next.action == ActionType.RuneOut && next.amount > 590)
                                    break;
                                if (next.action == ActionType.RuneIn && next.mon == act.mon) {
                                    for (var k = i + 1; k < j; k++) {
                                        if (actList[k].action == ActionType.RuneIn) {
                                            actList[k].amount--;
                                            act.amount++;
                                        }
                                        else if (actList[k].action == ActionType.RuneOut) {
                                            actList[k].amount--;
                                            act.amount--;
                                        }
                                    }
                                    actList.Remove(act);
                                    next.manyRunes = next.manyRunes.Concat(act.manyRunes).Distinct().ToList();
                                    i--;
                                }
                            }
                            break;
                        case ActionType.RuneOut:
                            for (var j = i - 1; j > 1; j--) {
                                var prev = actList.ElementAt(j);
                                if (prev.action == ActionType.RuneOut && prev.amount > 590 && prev.mon != act.mon)
                                    break;
                                if (prev.action == ActionType.RuneOut && prev.mon == act.mon) {
                                    for (var k = i - 1; k > j; k--) {
                                        if (actList[k].action == ActionType.RuneIn) {
                                            actList[k].amount++;
                                            act.amount++;
                                        }
                                        else if (actList[k].action == ActionType.RuneOut) {
                                            actList[k].amount++;
                                            act.amount--;
                                        }
                                    }
                                    actList.Remove(act);
                                    prev.manyRunes = prev.manyRunes.Concat(act.manyRunes).Distinct().ToList();
                                    i--;
                                }
                            }
                            break;
                    }

                }

                // combine
                for (int i = 0; i < actList.Count; i++) {
                    i = Math.Max(0, i);
                    var act = actList.ElementAt(i);
                    if (act == actList.Last())
                        break;

                    var nact = actList.ElementAt(i + 1);

                    if (act.action != nact.action)
                        continue;
                    if ((act.action == ActionType.RuneIn || act.action == ActionType.RuneOut) && act.mon != nact.mon)
                        continue;

                    switch (act.action) {
                        case ActionType.MonIn:
                        case ActionType.MonOut:
                            act.manyMons.Add(nact.mon);
                            break;
                        case ActionType.RuneIn:
                        case ActionType.RuneOut:
                            act.manyRunes.Add(nact.rune);
                            break;
                    }
                    i -= 2;
                    actList.Remove(nact);
                }

                return returnHtml(new ServedResult[] {
                        new ServedResult("link") { contentDic = { { "rel", "\"stylesheet\"" }, { "type", "\"text/css\"" }, { "href", "\"/css/runes.css\"" } } },
                        new ServedResult("style") {
                        contentList = {
                                @"
div.rune-box {
    display: inline-block;
    vertical-align: middle;
}
.mon-portrait {
    border: 2px solid grey;
    border-radius: 0.5em;
    border-style: ridge;
    vertical-align: middle;
    height: 4em;
}
.action-arrow {
    vertical-align: middle;
    height: 4em;
}
.storage {
    vertical-align: middle;
    height: 4em;
}
@media only screen and (min-resolution: 192dpi),
       only screen and (min-resolution: 2dppx) {
    .mon-portrait {
        height: 6em;
    }
    .action-arrow {
        height: 6em;
    }
    .storage {
        height: 6em;
    }
}
"
                            } },
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
" } }
                    },
                    new ServedResult("ol") {
                        contentList = new ServedResult[] { }
                    .Concat(actList.Select(a =>
                        a.Render()
                    //new ServedResult("li") { contentList = { a.action.ToString(), a.mon.ToString(), RuneRenderer.renderRune(a.rune) } }
                    )).Concat(new ServedResult[] { message }).ToList()
                    });
            }

            class RuneAction {
                public ActionType action;
                public Rune rune {
                    get {
                        return manyRunes.First();
                    }
                    set {
                        manyRunes.Add(value);
                    }
                }

                public Monster mon {
                    get {
                        return manyMons.First();
                    }
                    set {
                        manyMons.Add(value);
                    }
                }

                public List<RuneAction> prereq = new List<RuneAction>();

                public List<Rune> manyRunes = new List<Rune>();
                public List<Monster> manyMons = new List<Monster>();

                public string message = "";

                public int amount;
                public int maximum;

                public RuneAction(ActionType t) {
                    action = t;
                }

                public void Merge(RuneAction rhs) {
                    // add missing prereqs to this
                    foreach (var pr in rhs.prereq) {
                        var fp = prereq.FirstOrDefault(p => p.action == pr.action && p.mon == pr.mon && p.rune == pr.rune);
                        if (fp == null)
                            prereq.Add(fp);
                        else
                            fp.Merge(pr);
                    }
                }

                public ServedResult Render() {
                    var ret = new ServedResult("li");

                    ret.contentList.Add(Master.renderMonLink(mon, action.ToString(), LinkRenderOptions.All ^ LinkRenderOptions.Portrait));
                    ret.contentList.Add(" " + string.Format(message, amount, maximum) + "<br/>");

                    manyMons.Sort();

                    if (action == ActionType.MonIn) {
                        foreach (var m in manyMons)
                            ret.contentList.Add(ServeImageClass($"monsters/{m.monsterTypeId}.png", "mon-portrait"));
                        ret.contentList.Add(ServeImageClass($"loads/move_right.png", "action-arrow"));
                        ret.contentList.Add(ServeImageClass($"loads/building_store_small.png", "storage"));
                    }
                    else if (action == ActionType.MonOut) {
                        ret.contentList.Add(ServeImageClass($"loads/building_store_small.png", "storage"));
                        ret.contentList.Add(ServeImageClass($"loads/move_right.png", "action-arrow"));
                        foreach (var m in manyMons)
                            ret.contentList.Add(ServeImageClass($"monsters/{m.monsterTypeId}.png", "mon-portrait"));
                    }
                    else if (action == ActionType.RuneIn) {
                        foreach (var r in manyRunes)
                            ret.contentList.Add(RuneRenderer.renderRune(r, true));
                        ret.contentList.Add(ServeImageClass($"loads/move_right.png", "action-arrow"));
                        ret.contentList.Add(ServeImageClass($"monsters/{mon.monsterTypeId}.png", "mon-portrait"));
                    }
                    else if (action == ActionType.RuneOut) {
                        return new ServedResult("span");
                    }

                    return ret;
                }

                public ServedImage ServeImageClass(string src, string @class) {
                    var se = new ServedImage(src, null, null);
                    se.contentDic.Add("class", $"\"{@class}\"");
                    return se;
                }
            }

            class MiniMon {
                public Monster mon;
                public MiniRune[] runesCurrent = new MiniRune[6];
                public MiniRune[] runesDesired = new MiniRune[6];
                public bool storage;
                public bool lockedOut;
                internal int priority;

                public override string ToString() {
                    return mon.FullName;
                }

                public ulong Id {
                    get { return mon.Id; }
                }

                public bool IsGood {
                    get {
                        return runesDesired.All(r => r == null) || runesDesired.All(r => runesCurrent.Contains(r));
                    }
                }

                public int ActionsLeft {
                    get {
                        return IsGood ? 0 :
                            // RuneIn + My RuneOut
                            runesDesired.Count(r => r?.current != this) + runesCurrent.Count(r => r != null && r?.desired != this) +
                            // Their RuneOut
                            runesDesired.Count(r => r?.current != null && r.current != this) +
                            // MonOut
                            runesDesired.Where(r => r != null).GroupBy(r => r.current).Select(g => g.FirstOrDefault()).Count(r => r != null && r.current != r.desired && (r.current?.storage ?? false));
                    }
                }

                public int ActionsAvailable {
                    get {
                        return storage ? 0 : runesDesired.Count(r => r != null && (r.current == null || !r.current.storage)) + runesCurrent.Count(r => r != null && r.desired != this);
                    }
                }

                public bool Needed {
                    get {
                        return runesDesired.Any(r => r != null && r.current != this) || runesCurrent.Any(r => r != null && r.desired != r.current);
                    }
                }

                public int Score { get { return 100 * (ActionsLeft - ActionsAvailable) + priority + (lockedOut ? 100 : 0); } }
            }

            class MiniCraft {
                public Craft craft;
                public MiniRune grindOn;

                public ulong Id {
                    get { return craft.ItemId; }
                }
            }

            class MiniRune {
                public Rune rune;
                public MiniMon current;
                public MiniMon desired;

                public ulong Id {
                    get { return rune.Id; }
                }

                public override string ToString() {
                    return rune.ToString();
                }
            }

            enum ActionType {
                MonIn,
                MonOut,
                RuneIn,
                RuneOut,
            }
        }
    }
}
