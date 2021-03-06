using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using RunePlugin;
using RuneOptim.swar;

namespace RiftTrackerPlugin {
    public class RiftTrackerPlugin : RunePlugin.SWPlugin
    {
        Dictionary<string, Dictionary<ulong, KeyValuePair<RiftDeck, Monster>>> allMons = new Dictionary<string, Dictionary<ulong, KeyValuePair<RiftDeck, Monster>>>();

        Dictionary<RiftDungeon, Dictionary<string, Dictionary<string, int>>> matchCount = new Dictionary<RiftDungeon, Dictionary<string, Dictionary<string, int>>>();

        bool loading = true;

        public override void OnLoad()
        {
            if (!Directory.Exists(PluginDataDirectory))
                Directory.CreateDirectory(PluginDataDirectory);
            if (!Directory.Exists(PluginDataDirectory + "\\Rift Teams"))
                Directory.CreateDirectory(PluginDataDirectory + "\\Rift Teams");
            if (!Directory.Exists(PluginDataDirectory + "\\Rift Teams\\Archive"))
                Directory.CreateDirectory(PluginDataDirectory + "\\Rift Teams\\Archive");

            if (File.Exists(PluginDataDirectory + "\\allMons.gz"))
            {
                using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(PluginDataDirectory + "\\allMons.gz")))
                {
                    using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Decompress))
                    {
                        using (MemoryStream os = new MemoryStream())
                        {
                            gz.CopyTo(os);
                            var ostr = Encoding.ASCII.GetString(os.ToArray());
                            allMons = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<ulong, KeyValuePair<RiftDeck, Monster>>>>(ostr);
                        }
                    }
                }
                foreach (var m in allMons.SelectMany(q => q.Value.Select(r => r.Value.Value)))
                {
                    var tid = long.Parse(m.monsterTypeId.ToString().Substring(0, m.monsterTypeId.ToString().Length - 2) + "1" + m.monsterTypeId.ToString().Last());
                    m.Name = MonsterName(tid);

                    foreach (var r in m.Runes)
                    {
                        r.PrebuildAttributes();
                        m.ApplyRune(r);
                        r.AssignedName = m.Name;
                        r.Assigned = m;
                    }
                }
            }
            if (File.Exists(PluginDataDirectory + "\\matchCount.json"))
                matchCount = JsonConvert.DeserializeObject<Dictionary<RiftDungeon, Dictionary<string, Dictionary<string, int>>>>(File.ReadAllText(PluginDataDirectory + "\\matchCount.json"));

            var files = Directory.GetFiles(PluginDataDirectory + "\\Rift Teams", "*.json");
            foreach (var f in files)
            {
                ProcessDeck(File.ReadAllText(f));
                File.Move(f, PluginDataDirectory + "\\Rift Teams\\Archive\\" + new FileInfo(f).Name);
            }
            loading = false;
            SaveStuff();
        }

        public override void ProcessRequest(object sender, SWEventArgs args)
        {
            if (args.Response.CommandStr != "GetRiftDungeonCommentDeck")
                return;

            //var riftstats = JsonConvert.DeserializeObject<RiftDeck>(args.ResponseJson["bestdeck_rift_dungeon"].ToString());
            //File.WriteAllText(teamsdir + "\\" + riftstats.RiftDungeonId + "_" + riftstats.WizardId + ".json", args.ResponseRaw);
            ProcessDeck(args.ResponseRaw);
            SaveStuff();
        }

        public void ProcessDeck(string json)
        {
            var jobj = JsonConvert.DeserializeObject<JObject>(json);
            var riftstats = JsonConvert.DeserializeObject<RiftDeck>(jobj["bestdeck_rift_dungeon"].ToString());
            var mons = JsonConvert.DeserializeObject<Monster[]>(jobj["unit_list"].ToString());
            foreach (var m in mons)
            {
                var tid = long.Parse(m.monsterTypeId.ToString().Substring(0, m.monsterTypeId.ToString().Length - 2) + "1" + m.monsterTypeId.ToString().Last());
                m.Name = MonsterName(tid);
                
                foreach (var r in m.Runes)
                {
                    r.PrebuildAttributes();
                    m.ApplyRune(r);
                    r.AssignedName = m.Name;
                    r.Assigned = m;
                }

                if (!matchCount.ContainsKey(riftstats.RiftDungeonId))
                    matchCount.Add(riftstats.RiftDungeonId, new Dictionary<string, Dictionary<string, int>>());
                if (!matchCount[riftstats.RiftDungeonId].ContainsKey(m.Name))
                    matchCount[riftstats.RiftDungeonId].Add(m.Name, new Dictionary<string, int>());
                foreach (var mm in mons)
                {
                    if (mm.Name == "Missingno")
                    {
                        tid = long.Parse(mm.monsterTypeId.ToString().Substring(0, mm.monsterTypeId.ToString().Length-2) + "1" + mm.monsterTypeId.ToString().Last());
                        mm.Name = MonsterName(tid);
                    }

                    if (m.Name == mm.Name)
                        continue;

                    if (!matchCount[riftstats.RiftDungeonId][m.Name].ContainsKey(mm.Name))
                        matchCount[riftstats.RiftDungeonId][m.Name].Add(mm.Name, 0);
                    if (!matchCount[riftstats.RiftDungeonId].ContainsKey(mm.Name))
                        matchCount[riftstats.RiftDungeonId].Add(mm.Name, new Dictionary<string, int>());
                    if (!matchCount[riftstats.RiftDungeonId][mm.Name].ContainsKey(m.Name))
                        matchCount[riftstats.RiftDungeonId][mm.Name].Add(m.Name, 0);


                    matchCount[riftstats.RiftDungeonId][m.Name][mm.Name]++;
                    //matchCount[riftstats.RiftDungeonId][mm.Name][m.Name]++;
                }

                //if (!allMons.ContainsKey(riftstats.RiftDungeonId))
                //  allMons[riftstats.RiftDungeonId] = new Dictionary<string, Dictionary<ulong, KeyValuePair<RiftDeck, RuneOptim.Monster>>>();
                //var riftMons = allMons[riftstats.RiftDungeonId];

                if (!allMons.ContainsKey(m.Name))
                    allMons.Add(m.Name, new Dictionary<ulong, KeyValuePair<RiftDeck, Monster>>());
                var monList = allMons[m.Name];
                monList[riftstats.WizardId] = new KeyValuePair<RiftDeck, Monster>(riftstats, m);
            }

        }

        public void SaveStuff()
        {
            if (loading) return;

            var allMonStr = JsonConvert.SerializeObject(allMons);
            using (MemoryStream ms = new MemoryStream())
            {
                using (System.IO.Compression.GZipStream gz = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress))
                {
                    var bytes = Encoding.ASCII.GetBytes(allMonStr);
                    gz.Write(bytes, 0, bytes.Length);
                    gz.Flush();
                }
                ms.Flush();
                File.WriteAllBytes(PluginDataDirectory + "\\allMons.gz", ms.ToArray());
            }

            File.WriteAllText(PluginDataDirectory + "\\matchCount.json", JsonConvert.SerializeObject(matchCount));

            FileInfo excelFile = new FileInfo(PluginDataDirectory + @"\riftstats.xlsx");
            ExcelPackage excelPack = null;
            excelPack = new ExcelPackage(excelFile);
            Dictionary<string, int> mcount = new Dictionary<string, int>();

            foreach (var montype in allMons) {
                int row = 1;
                int col = 1;
                    
                var page = excelPack.Workbook.Worksheets.FirstOrDefault(p => p.Name == montype.Key);
                if (page == null)
                    page = excelPack.Workbook.Worksheets.Add(montype.Key);

                List<string> colHead = new List<string>();

                foreach (var th in "Raid,Grade,Points,Pos,Lead, ,HP,ATK,DEF,SPD,CR,CD,RES,ACC,Set1,Set2,Set3,EHP,EHPDB,DPS,MxD,AvD".Split(','))
                {
                    colHead.Add(th);
                    page.Cells[row, col].Value = th; col++;
                }
                row++;

                foreach (var kvm in montype.Value.OrderByDescending(mv => mv.Value.Key.ClearDamage))
                {
                    if (!mcount.ContainsKey(montype.Key))
                        mcount[montype.Key] = 1;
                    else
                        mcount[montype.Key]++;

                    var mon = kvm.Value.Value;
                    var stats = mon.GetStats();
                    for (col = 1; col <= colHead.Count; col++)
                    {
                        switch (colHead[col - 1])
                        {
                            case "Raid":
                                page.Cells[row, col].Value = kvm.Value.Key.RiftDungeonId;
                                break;
                            case "Grade":
                                page.Cells[row, col].Value = kvm.Value.Key.ClearRating;
                                break;
                            case "Points":
                                page.Cells[row, col].Value = kvm.Value.Key.ClearDamage;
                                break;
                            case "Pos":
                                page.Cells[row, col].Value = kvm.Value.Key.Monsters.FirstOrDefault(l => l.MonsterId == (long)mon.Id)?.Position;
                                break;
                            case "Lead":
                                var rifty = kvm.Value.Key.Monsters.FirstOrDefault(l => l.MonsterId == (long)mon.Id);
                                page.Cells[row, col].Value = (rifty?.Position == kvm.Value.Key.LeaderIndex);
                                break;
                            case "HP":
                                page.Cells[row, col].Value = stats.Health;
                                break;
                            case "ATK":
                                page.Cells[row, col].Value = stats.Attack;
                                break;
                            case "DEF":
                                page.Cells[row, col].Value = stats.Defense;
                                break;
                            case "SPD":
                                page.Cells[row, col].Value = stats.Speed;
                                break;
                            case "CR":
                                page.Cells[row, col].Value = stats.CritRate;
                                break;
                            case "CD":
                                page.Cells[row, col].Value = stats.CritDamage;
                                break;
                            case "RES":
                                page.Cells[row, col].Value = stats.Resistance;
                                break;
                            case "ACC":
                                page.Cells[row, col].Value = stats.Accuracy;
                                break;
                            case "Set1":
                                if (Rune.SetRequired(mon.Current.Sets[1]) == 4)
                                    page.Cells[row, col].Value = mon.Current.Sets[1];
                                else
                                    page.Cells[row, col].Value = mon.Current.Sets[0];
                                break;
                            case "Set2":
                                if (Rune.SetRequired(mon.Current.Sets[1]) == 4)
                                    page.Cells[row, col].Value = mon.Current.Sets[0];
                                else
                                    page.Cells[row, col].Value = mon.Current.Sets[1];
                                break;
                            case "Set3":
                                page.Cells[row, col].Value = mon.Current.Sets[2];
                                break;
                            case "EHP":
                                page.Cells[row, col].Value = stats.ExtraValue(Attr.EffectiveHP);
                                break;
                            case "EHPDB":
                                page.Cells[row, col].Value = stats.ExtraValue(Attr.EffectiveHPDefenseBreak);
                                break;
                            case "DPS":
                                page.Cells[row, col].Value = stats.ExtraValue(Attr.DamagePerSpeed);
                                break;
                            case "MxD":
                                page.Cells[row, col].Value = stats.ExtraValue(Attr.MaxDamage);
                                break;
                            case "AvD":
                                page.Cells[row, col].Value = stats.ExtraValue(Attr.AverageDamage);
                                break;

                        }
                    }
                    row++;
                }
            }

            var sheets = excelPack.Workbook.Worksheets;
            foreach (var cc in mcount.OrderBy(v => v.Value))
            {
                var thissheet = sheets[cc.Key];
                try
                {
                    var tables = thissheet.Tables;
                    int row = 1;
                    while (!string.IsNullOrWhiteSpace(thissheet.Cells[row, 1].Value?.ToString()))
                        row++;
                    int headsize = 1;
                    while (headsize < 10 || !string.IsNullOrWhiteSpace(thissheet.Cells[1, headsize].Value?.ToString()))
                        headsize++;

                    var range = thissheet.Cells[1, 1, row - 1, headsize - 1];
                    if (tables.FirstOrDefault(t => t.Name == cc.Key.Replace(" ", "_")) == null)
                        tables.Add(range, cc.Key.Replace(" ", "_"));
                    else {
                        var newRange = new ExcelAddress(1, 1, row - 1, headsize - 1).ToString();

                        var tableElement = tables.FirstOrDefault(t => t.Name == cc.Key.Replace(" ", "_")).TableXml.DocumentElement;
                        tableElement.Attributes["ref"].Value = newRange;
                        tableElement["autoFilter"].Attributes["ref"].Value = newRange;

                    }
                }
                catch { }
                excelPack.Workbook.Worksheets.MoveToStart(cc.Key);
            }

            foreach (var rd in matchCount)
            {
                
                var page = excelPack.Workbook.Worksheets.FirstOrDefault(p => p.Name == rd.Key.ToString());
                if (page == null)
                    page = excelPack.Workbook.Worksheets.Add(rd.Key.ToString());

                var rmons = allMons.SelectMany(q => q.Value.Select(r => r.Value)).Where(d => d.Key.RiftDungeonId == rd.Key);
                var monm = rmons.Select(d => d.Value);
                //var mp = rmons.Select(d => d.Value._monsterTypeId.ToString().Substring(0,3)).Select(i => monm.FirstOrDefault(m => m._monsterTypeId.ToString().Substring(0,3) == i));
                
                int row = 2;
                int col = 2;

                var monsX = rd.Value.OrderByDescending(q => q.Value.Sum(v => v.Value)).Select(q => q.Key).ToArray();

                //var monsY = rd.Value.OrderByDescending(q => q.Value.Sum(v => v.Value)).SelectMany(q => q.Value.Select(w => w.Key)).Distinct().ToArray();
                var monsdY = new Dictionary<string, int>();
                foreach (var mq in rd.Value)
                {
                    foreach (var mw in mq.Value)
                    {
                        if (!monsdY.ContainsKey(mw.Key))
                            monsdY.Add(mw.Key, 0);
                        monsdY[mw.Key]++;
                    }
                }
                var monsY = monsdY.OrderByDescending(v => v.Value).Select(v => v.Key).ToArray();


                foreach (var mx in monsX)
                {
                    page.Cells[1, col].Value = mx; col++;
                }
                foreach (var my in monsY)
                {
                    page.Cells[row, 1].Value = my; row++;
                }

                row = 2;
                col = 2;

                foreach (var mx in monsX)
                {
                    row = 2;
                    foreach (var my in monsY)
                    {
                        if (rd.Value.ContainsKey(monsX[col - 2]) && rd.Value[mx].ContainsKey(monsY[row - 2]))
                        {
                            var c = rd.Value[mx][my];
                            c = rd.Value[monsX[col - 2]][monsY[row - 2]];
                            //var c = rds.Count(r => r.Monsters.Any(p => p.unit_master_id == mx._monsterTypeId) && r.Monsters.Any(p => p.unit_master_id == my._monsterTypeId)); ;
                            page.Cells[row, col].Value = c;
                        }
                        else
                            page.Cells[row, col].Value = "";


                        row++;
                    }
                    col++;
                }
                excelPack.Workbook.Worksheets.MoveToStart(rd.Key.ToString());
            }
            try {
                if (excelPack.Workbook.Worksheets.Count > 0)
                    excelPack.Save();
                Console.WriteLine("Saved riftstats");
            }
            catch (Exception e) {
                Console.WriteLine("Failed saving riftstats because " + e.GetType() + ": " + e.Message);
            }
        }
    }
}
