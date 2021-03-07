using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Newtonsoft.Json;
using RuneOptim;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {
        [PageAddressRender("goals")]
        public class GoalsRenderer : PageRenderer {
            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                var resp = this.Recurse(req, uri);
                if (resp != null)
                    return resp;

                var gs = new GoalStack(Program.data);

                wutt fuseSucc = new wutt() {
                    Consumes = {
                        (i) => {
                            return false;
                        }
                    },
                    Produces = {
                        (i) => {
                            var mi = i as MonsterItem;
                            if (mi != null) {
                                if (mi.MonTypeId == 13302) {
                                    return true;
                                }
                            }
                            return false;
                        }
                    }
                };
                wutt awaken = new wutt() {
                    Consumes = {
                        (i) => {
                            return false;
                        }
                    },
                    Produces = {
                        (i) => {
                            return false;
                        }
                    }
                };
                wutt grindMagic = new wutt() {
                    Consumes = {
                        (i) => {
                            return false;
                        }
                    },
                    Produces = {
                        (i) => {
                            return false;
                        }
                    }
                };
                wutt gradeUp = new wutt() {
                    Consumes = {
                        (i) => {
                            return false;
                        }
                    },
                    Produces = {
                        (i) => {
                            return false;
                        }
                    }
                };
                wutt grindFaimon = new wutt() {
                    Consumes = {
                        (i) => {
                            return false;
                        }
                    },
                    Produces = {
                        (i) => {
                            return false;
                        }
                    }
                };
                wutt popUnknown = new wutt() {
                    Consumes = {
                        (i) => {
                            return false;
                        }
                    },
                    Produces = {
                        (i) => {
                            return false;
                        }
                    }
                };
                gs.Moves.Add(new ActionSW("GB10") {
                    Produces = new List<RuneApp.InternalServer.Master.Item> {
                        new CraftItem() {
                             Amount = 0.008,
                             ItemType = ItemType.Scrolls,
                             Id = (int)ScrollType.Mystical
                        },
                        new CraftItem() {
                             Amount = 0.104,
                             ItemType = ItemType.Scrolls,
                             Id = (int)ScrollType.Unknown
                        },
                        new CraftItem() {
                            Amount = 0.008,
                            ItemType = ItemType.WizardInfo,
                            Id = (int)WizardType.CostumePoint
                        }
                    }
                });

                gs.Produce(new CraftItem() {
                    ItemType = ItemType.WizardInfo,
                    Id = (int)WizardType.CostumePoint
                }, 100);

                return returnHtml(new ServedResult[]{
                        new ServedResult("link") { contentDic = { { "rel", "\"stylesheet\"" }, { "type", "\"text/css\"" }, { "href", "\"/css/runes.css\"" } } },
                }, new ServedResult("ul") {
                    contentList = gs.Actions.Select(a => new ServedResult("li") {
                        contentDic = {
                            { "style", "color: " + (a.Fufilled ? "black" : "red") }
                        },
                        contentList = {
                            a.Name
                        }
                    }).ToList()
                });
            }
        }

        class GoalStack {
            RuneOptim.Save data;
            public GoalStack(RuneOptim.Save d) {
                data = d;
                foreach (var m in data.Monsters) {
                    Things.Add(new MonsterItem() {
                        Grade = m.Grade,
                        Level = m.level,
                        Monster = m,
                        MonTypeId = m.monsterTypeId
                    });
                }
                foreach (var i in data.InventoryItems) {
                    Things.Add(new CraftItem() {
                        Amount = i.Quantity,
                        ItemType = i.Type,
                        Id = i.Id
                    });
                }
                Things.Add(new CraftItem() {
                    Amount = data.WizardInfo.CostumePoint,
                    ItemType = ItemType.WizardInfo,
                    Id = (int)WizardType.CostumePoint
                });
                // TODO: the rest
            }

            public List<ActionSW> Moves = new List<ActionSW>();
            public List<Item> Things = new List<Item>();
            public List<ActionSW> Actions = new List<ActionSW>();

            // easymode
            public Fufillment Produce(Item signature, double amount) {
                var ms = signature as MonsterItem;
                var cs = signature as CraftItem;
                if (ms != null) {
                    var act = new ActionSW("Make " + amount + " " + ms.Monster.Name);
                    for (int i = 0; i < amount; i++) {
                        var pp = Produce(signature);
                        var pi = (pp as MonsterItem);
                        var pa = (pp as ActionSW);
                        if (pi != null) {
                            pi.ConsumedBy = act;
                            act.Consumes.Add(pi);
                        }
                        else if (pa != null) {
                            var ppi = pa.Produces.FirstOrDefault(p => p.SameSignature(signature));
                            ppi.ConsumedBy = act;
                            act.Consumes.Add(ppi);
                        }
                    }
                    Actions.Add(act);
                    return act;
                }
                else if (cs != null) {
                    var act = new ActionSW("Make " + amount + " " + cs.Name);
                    cs.Amount = amount;
                    while (cs.Amount > 0) {
                        var pp = Produce(cs);
                        var pi = (pp as CraftItem);
                        var pa = (pp as ActionSW);
                        if (pi != null) {
                            var d = Math.Min(amount, pi.Amount);
                            amount -= d;
                            cs.Amount = amount;
                            if (amount >= 0) {
                                pi.ConsumedBy = act;
                                act.Consumes.Add(pi);
                            }
                            else {
                                pi.Amount -= d;
                            }
                            if (cs.Amount > 0)
                                pa = (Produce(cs) as ActionSW);
                        }
                        if (pa != null) {
                            var pai = pa.Produces.FirstOrDefault(p => p.SameSignature(cs));

                        }
                    }
                    Actions.Add(act);

                }
                return null;
            }

            public Fufillment Produce(Item signature) {
                // What you are looking for already exists
                var thing = Things.FirstOrDefault(t => t.SameSignature(signature) && t.ConsumedBy == null);
                if (thing != null) {
                    var ci = signature as CraftItem;
                    var mi = signature as MonsterItem;
                    if (ci != null) {
                        return thing;
                        // consumption and subtraction *need* to be handled one level up
                        /*
                        var ct = thing as CraftItem;
                        if (ct.Amount >= ci.Amount)
                            return thing;
                        else {
                            var d = Math.Min(ci.Amount, ct.Amount);
                            ct.Amount -= d;
                            ci.Amount -= d;
                        }*/
                    }
                    if (mi != null && !mi.Monster.Locked)
                        return thing;
                }

                // We already have one in the works
                var act = Actions.FirstOrDefault(a => a.Produces.Any(p => p.SameSignature(signature) && p.ConsumedBy == null));
                if (act != null)
                    return act;

                var acts = Moves.Where(a => a.Produces.Any(p => p.SameSignature(signature)));

                // somehow pick "the best" way to produce it
                act = acts.FirstOrDefault();

                var ac = new ActionSW(act.Name);
                // consume and produce

                foreach (var i in act.Consumes) {
                    var fuf = Produce(i);
                    var fi = fuf as Item;
                    var fa = fuf as ActionSW;
                    if (fi != null) {
                        fi.ConsumedBy = ac;
                        ac.Consumes.Add(fi);
                    }
                    else if (fa != null) {
                        var pi = fa.Produces.FirstOrDefault(p => p.SameSignature(i));
                        pi.ConsumedBy = ac;
                        ac.Consumes.Add(pi);
                    }
                    else {
                        var ni = i.Copy();
                        ac.Consumes.Add(ni);
                    }
                }
                ac.Fufilled = ac.Consumes.All(c => c.ConsumedBy == ac);


                Actions.Add(ac);
                return ac;
            }
        }

        abstract class Fufillment {

        }

        abstract class Item : Fufillment {
            public ActionSW ProducedBy;
            public ActionSW ConsumedBy;
            public abstract bool SameSignature(Item rhs);
            public abstract bool IsReplacement(Item rhs);
            public abstract double Diff(Item rhs);
            public abstract Item Copy();
        }

        class MonsterItem : Item {
            public int MonTypeId;
            public int Level;
            public int Grade;

            public RuneOptim.Monster Monster;

            public override Item Copy() {
                return new MonsterItem() {
                    MonTypeId = this.MonTypeId,
                    Monster = this.Monster,
                    Level = this.Level,
                    Grade = this.Grade
                };
            }

            public override bool IsReplacement(Item rhs) {
                var mi = rhs as MonsterItem;
                if (mi == null)
                    return false;
                return (MonTypeId == mi.MonTypeId
                    && Grade >= mi.Grade
                    && Level >= mi.Level);
            }

            public override bool SameSignature(Item rhs) {
                var mi = rhs as MonsterItem;
                if (mi == null)
                    return false;
                return (MonTypeId == mi.MonTypeId);
            }

            public override double Diff(Item rhs) {
                if (!IsReplacement(rhs))
                    return int.MaxValue;
                var mi = rhs as MonsterItem;
                return ((Level - mi.Level)
                    + (Grade - mi.Grade) * 50);
            }
        }

        class CraftItem : Item {
            public int Id;
            public RuneOptim.ItemType ItemType;
            // fractional to handle dungeon drops
            public double Amount;

            public override Item Copy() {
                return new CraftItem() {
                    Amount = this.Amount,
                    Id = this.Id,
                    ItemType = this.ItemType
                };
            }

            public string Name {
                get {
                    switch (ItemType) {
                        case ItemType.WizardInfo:
                            return ((WizardType)Id).ToString();
                        case ItemType.Scrolls:
                            return ((ScrollType)Id).ToString();
                        case ItemType.Essence:
                            return ((EssenceType)Id).ToString();
                        case ItemType.SummoningPieces:
                            if (Id > 10000) {
                                if (Save.MonIdNames.ContainsKey(Id / 100))
                                    return Save.MonIdNames[Id / 100] + " ";
                                return "Missingno " + Id;
                            }
                            break;
                        case ItemType.Material:
                            return ((MaterialType)Id).ToString();
                    }
                    return "N/A" + Id;
                }
            }

            public override bool IsReplacement(Item rhs) {
                var ci = rhs as CraftItem;
                return (Id == ci.Id
                    && ItemType == ci.ItemType
                    && Amount >= ci.Amount);
            }

            public override bool SameSignature(Item rhs) {
                var ci = rhs as CraftItem;
                return (Id == ci.Id
                    && ItemType == ci.ItemType);
            }

            public override double Diff(Item rhs) {
                if (!SameSignature(rhs))
                    return int.MaxValue;
                var ci = rhs as CraftItem;
                return (ci.Amount - Amount);
            }
        }

        class ActionSW : Fufillment {
            public string Name;
            public bool Fufilled;
            public List<Item> Consumes = new List<Item>();
            public List<Item> Produces = new List<Item>();

            public ActionSW(string name) {
                Name = name;
            }
        }

        class wutt {
            public string Name;
            public bool Completed;
            public bool Complete() {
                return false;
            }

            public List<Func<Item, bool>> Produces;
            public List<Func<Item, bool>> Consumes;

            public List<Item> Produced;
            public List<Item> Consumed;

            public wutt Copy() {
                return null;

            }
        }


    }
}