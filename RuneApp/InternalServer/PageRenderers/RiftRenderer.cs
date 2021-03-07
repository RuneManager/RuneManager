using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using RunePlugin;

namespace RuneApp.InternalServer {
    public partial class Master : PageRenderer {

        [PageAddressRender("rift")]
        public class RiftRenderer : PageRenderer {
            public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri) {
                var resp = this.Recurse(req, uri);
                if (resp != null)
                    return resp;

                Master.LineLog.Debug("getting best clear");
                var best = Directory.GetFiles(Environment.CurrentDirectory, "GetBestClearRiftDungeon*.resp.json").OrderByDescending(s => s);
                if (best.Any()) {
                    var bestRift = JsonConvert.DeserializeObject<RunePlugin.Response.GetBestClearRiftDungeonResponse>(File.ReadAllText(best.First()), new SWResponseConverter());
                    Master.LineLog.Debug("deserialised " + bestRift.BestDeckRiftDungeons.Count() + " best teams");

                    Master.LineLog.Debug("can do name " + RuneOptim.swar.Save.MonIdNames.FirstOrDefault());
                    Master.LineLog.Debug("can do mon " + Program.data.Monsters.FirstOrDefault());
                    var sr = new List<ServedResult>();
                    foreach (var br in bestRift.BestDeckRiftDungeons) {
                        sr.Add("<h1>" + br.RiftDungeonId + "</h1>");
                        sr.Add("<h3>" + br.ClearRating + " " + br.ClearDamage + "</h3>");
                        var table = "<table>";

                        table += "<tr>";
                        for (int i = 1; i < 5; i++) {
                            if (i == br.LeaderIndex)
                                table += "<td style='background-color: yellow' >";
                            else
                                table += "<td>";
                            var mp = br.Monsters.FirstOrDefault(p => p.Position == i);
                            Master.LineLog.Debug("retrieving " + i + " mon " + mp?.MonsterId);
                            if (mp != null) {
                                var mon = Program.data.GetMonster((ulong)mp.MonsterId);
                                Master.LineLog.Debug("mon " + mon?.FullName);
                                if (mon == null) {
                                    var name = mp.MonsterTypeId.ToString();
                                    if (RuneOptim.swar.Save.MonIdNames.ContainsKey((int)mp.MonsterTypeId))
                                        name = RuneOptim.swar.Save.MonIdNames[(int)mp.MonsterTypeId];
                                    else
                                        name = RuneOptim.swar.Save.MonIdNames[(int)(mp.MonsterTypeId / 100)];

                                    table += name;
                                }
                                else {
                                    table += mon.FullName + " " + mon.Grade + "*";
                                }
                            }
                            table += "</td>";
                        }

                        table += "</tr>";
                        table += "<tr>";
                        for (int i = 5; i < 9; i++) {
                            if (i == br.LeaderIndex)
                                table += "<td style='background-color: yellow' >";
                            else
                                table += "<td>";
                            var mp = br.Monsters.FirstOrDefault(p => p.Position == i);
                            Master.LineLog.Debug("retrieving " + i + " mon " + mp?.MonsterId);
                            if (mp != null) {
                                var mon = Program.data.GetMonster((ulong)mp.MonsterId);
                                Master.LineLog.Debug("mon " + mon?.FullName);
                                if (mon == null) {
                                    var name = mp.MonsterTypeId.ToString();
                                    if (RuneOptim.swar.Save.MonIdNames.ContainsKey((int)mp.MonsterTypeId))
                                        name = RuneOptim.swar.Save.MonIdNames[(int)mp.MonsterTypeId];
                                    else
                                        name = RuneOptim.swar.Save.MonIdNames[(int)(mp.MonsterTypeId / 100)];

                                    table += name;
                                }
                                else {
                                    table += mon.FullName + " " + mon.Grade + "*";
                                }
                            }
                            table += "</td>";
                        }
                        table += "</tr>";

                        table += "</table>";

                        sr.Add(table);
                    }
                    return returnHtml(new ServedResult[] { new ServedResult("style") { contentList = { "td {border: 1px solid black;}" } } }, sr.ToArray());
                }
                else {
                    return returnHtml(null, "Please put GetBestClearRiftDungeon_*.resp.json");
                }
            }
        }

    }
}
