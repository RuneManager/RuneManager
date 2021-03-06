using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RuneOptim;
using RuneOptim.BuildProcessing;
using RuneOptim.Management;
using RuneOptim.swar;

namespace RuneApp {
    /// <summary>
    /// Manages all the stuff for the spreadsheet
    /// </summary>
    public class RuneSheet {
        readonly FileInfo excelFile = new FileInfo(@"runestats.xlsx");
        private ExcelPackage excelPack = null;
        private bool gotExcelPack = false;
        ExcelWorksheets excelSheets = null;
        private int linkCol = 1;

        public void Save() {
            if (StatsExcelBind(true)) {
                StatsExcelRunes();
                StatsExcelSave();
            }
            StatsExcelBind(true);
        }

        void StatsExcelClear() {
            if (Program.data?.Runes == null)
                return;

            FileInfo newFile = new FileInfo(@"runestats.xlsx");
            int status = 0;
            while (status != 1) {
                try {
                    newFile.Delete();
                    status = 1;
                }
                catch (Exception e) {
                    if (status == 0) {
                        if (MessageBox.Show("Please close runestats.xlsx\r\nOr ensure you can overwrite it.", "RuneStats", MessageBoxButtons.RetryCancel) == DialogResult.Cancel) {
                            status = 1;
                        }
                    }
                    else
                        Console.WriteLine(e);
                }
            }

        }

        bool StatsExcelBind(bool passive = false) {
            if (Program.data?.Runes == null)
                return false;

            if (gotExcelPack)
                return true;

            int status = 0;
            do {
                try {
                    excelPack = new ExcelPackage(excelFile);
                    status = 1;
                }
                catch (Exception e) {
                    // don't care if no bind
                    if (passive)
                        return false;
                    if (status == 0 && MessageBox.Show("Please close runestats.xlsx\r\nOr ensure you can overwrite it.", "RuneStats", MessageBoxButtons.RetryCancel) == DialogResult.Cancel) {
                        status = 1;
                        return false;
                    }
                    else
                        Program.LineLog.Error("Failed getting Excel", e);

                }
            }
            while (status != 1);

            excelSheets = excelPack.Workbook.Worksheets;
            makeHome();

            linkCol = 2;

            gotExcelPack = true;

            return true;
        }

        private void makeHome() {
            var hws = excelSheets.FirstOrDefault(w => w.Name == "Home");
            if (hws == null)
                hws = excelSheets.Add("Home");
            excelSheets.MoveBefore(hws.Index, 1);
        }

        public void StatsExcelSave(bool passiveBind) {
            if (StatsExcelBind(passiveBind)) {
                StatsExcelSave();
            }
            StatsExcelBind(true);
        }

        void StatsExcelSave() {
            try {
                excelPack.Save();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        public void StatsExcelBuild(Build build, Monster mon, Loadout load, bool passiveBind) {
            if (StatsExcelBind(passiveBind)) {
                StatsExcelBuild(build, mon, load);
                StatsExcelSave();
            }
            StatsExcelBind(true);
        }

        void StatsExcelBuild(Build build, Monster mon, Loadout load) {
            // TODO
            Console.WriteLine("Writing build");
            ExcelWorksheet ws = getSheet(mon);

            int row = 1;
            int col = 1;

            // number of good builds?
            ws.Cells[row, 1].Value = "Builds";
            ws.Cells[row, 2].Value = "Bad";
            ws.Cells[row, 3].Value = "Good";

            ws.Cells[row, 6].Value = DateTime.Now;
            ws.Cells[row, 6].Style.Numberformat.Format = "dd-MM-yy";

            ws.Cells[row, 7].Value = build.Time / 1000;
            ws.Cells[row, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
            if (build.Time / 1000 < 2)
                ws.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.MediumTurquoise);
            else if (build.Time / 1000 < 15)
                ws.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
            else if (build.Time / 1000 < 60)
                ws.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.Orange);
            else
                ws.Cells[row, 7].Style.Fill.BackgroundColor.SetColor(Color.Red);

            ws.Cells[row, 8].Hyperlink = new ExcelHyperLink("Home!A1", "Home");
            ws.Cells[row, 8].Style.Font.UnderLine = true;
            ws.Cells[row, 8].Style.Font.Color.SetColor(Color.Blue);

            ws.Cells[1, 9].Value = mon.Id;

            var runeSheet = excelSheets.FirstOrDefault(w => w.Name == "Home");
            if (runeSheet != null) {
                string hl = ws.Name + "!A1";
                if (hl.IndexOf(' ') != -1)
                    hl = "'" + ws.Name + "'!A1";
                runeSheet.Cells[build.Priority + 1, linkCol].Hyperlink = new ExcelHyperLink(hl, mon.FullName);
                runeSheet.Cells[build.Priority + 1, linkCol].Style.Font.UnderLine = true;
                runeSheet.Cells[build.Priority + 1, linkCol].Style.Font.Color.SetColor(Color.Blue);

                runeSheet.Cells[build.Priority + 1, linkCol + 1].Value = build.Time / (double)1000;
                runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Numberformat.Format = "0.00";
                if (build.Time / 1000 < 2) {
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.MediumTurquoise);
                }
                else if (build.Time / 1000 < 15) {
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                }
                else if (build.Time / 1000 < 60) {
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                }
                else {
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.Priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                }

                int combinations = build.runes[0].Length;
                combinations *= build.runes[1].Length;
                combinations *= build.runes[2].Length;
                combinations *= build.runes[3].Length;
                combinations *= build.runes[4].Length;
                combinations *= build.runes[5].Length;

                runeSheet.Cells[build.Priority + 1, linkCol + 2].Value = load.ActualTests;
                runeSheet.Cells[build.Priority + 1, linkCol + 3].Value = combinations;

            }
            else {
                Console.WriteLine("No Home sheet");
            }

            row++;

            ws.Cells[row, 2].Value = build.BuildUsage.failed;
            ws.Cells[row, 3].Value = build.BuildUsage.passed;

            if (build.BuildUsage != null && build.BuildUsage.loads != null)
                build.BuildUsage.loads = build.BuildUsage.loads.OrderByDescending(m => build.CalcScore(m.GetStats())).ToList();

            double scoreav = 0;
            int c = 0;
            Stats minav = new Stats();
            foreach (var b in build.BuildUsage.loads) {
                double sc = build.CalcScore(b.GetStats());
                b.score = sc;
                scoreav += sc;
                minav += b.GetStats();
                foreach (var s in Build.ExtraNames) {
                    minav.ExtraSet(s, minav.ExtraGet(s) + b.GetStats().ExtraValue(s));
                }
                c++;
            }
            scoreav /= c;
            minav /= c;

            ws.Cells[row - 1, 4].Value = scoreav;

            Stats lowQ = new Stats();
            foreach (var s in Build.StatEnums) {
                c = 0;
                foreach (var b in build.BuildUsage.loads) {
                    if (minav[s] > b.GetStats()[s]) {
                        lowQ[s] += b.GetStats()[s];
                        c++;
                    }
                }
                lowQ[s] /= c;
            }
            foreach (var s in Build.ExtraNames) {
                c = 0;
                foreach (var b in build.BuildUsage.loads) {
                    if (minav.ExtraGet(s) > b.GetStats().ExtraValue(s)) {
                        lowQ.ExtraSet(s, b.GetStats().ExtraValue(s));
                        c++;
                    }
                }
                lowQ[s] /= c;
            }

            Stats versus = new Stats();
            bool enough = false;

            foreach (Attr s in Build.StatAll) {
                if (!s.HasFlag(Attr.ExtraStat)) {
                    if (!build.Minimum[s].EqualTo(0)) {
                        versus[s] = lowQ[s];
                        if (build.BuildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.BuildUsage.passed) {
                            enough = true;
                            break;
                        }
                    }
                }
                else {
                    if (!build.Minimum.ExtraGet(s).EqualTo(0)) {
                        versus.ExtraSet(s, lowQ.ExtraGet(s));
                        if (build.BuildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.BuildUsage.passed) {
                            enough = true;
                            break;
                        }
                    }
                }
            }

            if (!enough) {
                foreach (Attr s in Build.StatAll) {
                    if (!s.HasFlag(Attr.ExtraStat)) {
                        if (!build.Sort[s].EqualTo(0)) {
                            versus[s] = lowQ[s];
                            if (build.BuildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.BuildUsage.passed) {
                                break;
                            }
                        }
                    }
                    else {
                        if (!build.Sort.ExtraGet(s).EqualTo(0)) {
                            versus.ExtraSet(s, lowQ.ExtraGet(s));
                            if (build.BuildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.BuildUsage.passed) {
                                break;
                            }
                        }
                    }
                }
            }


            ws.Cells[row, 4].Value = build.BuildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true));

            var trow = row;
            row++;

            foreach (Attr stat in Build.StatAll) {
                ws.Cells[row, 1].Value = stat;
                if (!stat.HasFlag(Attr.ExtraStat)) {
                    if (build.Minimum[stat] > 0 || !build.Sort[stat].EqualTo(0)) {
                        ws.Cells[row, 2].Value = build.Minimum[stat];
                        ws.Cells[row, 3].Value = build.Sort[stat];
                        ws.Cells[row, 4].Value = versus[stat];
                    }
                }
                else {
                    if (build.Minimum.ExtraGet(stat) > 0 || !build.Sort.ExtraGet(stat).EqualTo(0)) {
                        ws.Cells[row, 2].Value = build.Minimum.ExtraGet(stat);
                        ws.Cells[row, 3].Value = build.Sort.ExtraGet(stat);
                        ws.Cells[row, 4].Value = versus.ExtraGet(stat);
                    }
                }
                row++;
            }

            col = 5;
            row = trow;

            foreach (var b in build.BuildUsage.loads.Take(20)) {
                row = trow;
                ws.Cells[row, col].Value = b.score;
                if (b.score < scoreav) {
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                }
                row++;
                foreach (Attr stat in Build.StatAll) {
                    if (!stat.HasFlag(Attr.ExtraStat)) {
                        ws.Cells[row, col].Value = b.GetStats()[stat];
                    }
                    else {
                        ws.Cells[row, col].Value = b.GetStats().ExtraValue(stat);
                    }
                    row++;
                }
                col++;
            }

            row++;
            col = 1;

            StatsExcelRuneBoard(ws, ref row, ref col, build, load);
            Program.LineLog.Info("Finished Writing statsheet");
        }

        private ExcelWorksheet getSheet(Monster mon) {
            ExcelWorksheet ws;
            string wsname;
            int ptries = 0;
            int ind = -1;

            do {
                wsname = mon.FullName + (ptries == 0 ? "" : ptries.ToString());
                ws = excelSheets.FirstOrDefault(w => w.Name == wsname);

                if (ws != null) {
                    ulong pid;
                    string pids = ws.Cells[1, 9].Value.ToString();
                    if ((!string.IsNullOrWhiteSpace(pids) && ulong.TryParse(pids, out pid) && pid == mon.Id)) {
                        ind = ws.Index;
                        excelSheets.Delete(mon.FullName + (ptries == 0 ? "" : ptries.ToString()));
                        ws = null;
                    }
                    else
                        ptries++;
                }
            }
            while (ws != null);

            ws = excelPack.Workbook.Worksheets.Add(wsname);
            if (ind != -1)
                excelSheets.MoveBefore(ws.Index, ind);
            return ws;
        }

        void StatsExcelRuneBoard(ExcelWorksheet ws, ref int row, ref int col, Build build, Loadout load) {
            // write each slots scoring weights
            // reference and gray out things which inherit
            var cmax = 1;
            var rowstat = row + 2;
            var abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            foreach (int i in new int[] { -1, -2, -3, 0, 1, 2, 3, 4, 5 }) {
                col = 5;
                var slot = i + 1;
                string sslot = "g";
                if (slot > 0)
                    sslot = slot.ToString();
                else if (slot == -1)
                    sslot = "o";
                else if (slot == -2)
                    sslot = "e";

                ws.Cells[row, col].Value = sslot;
                col++;


                var rf = build.RuneFilters.ContainsKey((SlotIndex)slot) ? build.RuneFilters[(SlotIndex)slot] : null;

                var btest = build.RuneScoring.ContainsKey((SlotIndex)slot) ? build.RuneScoring[(SlotIndex)slot] : new Build.RuneScoreFilter();
                double? test = btest.Value;
                bool isTestInherited = false;
                string testForm = null;

                if (test == null) {
                    if (slot > 0) {
                        isTestInherited = true;
                        testForm = "=A" + (rowstat - (slot) % 2);

                    }
                    else if (slot < 0) {
                        isTestInherited = true;
                        testForm = "=A" + (rowstat - 2);
                    }
                }
                if (testForm == null)
                    ws.Cells[row, 1].Value = test;
                else
                    ws.Cells[row, 1].Formula = testForm;

                if (isTestInherited) {
                    ws.Cells[row, 1].Style.Font.Color.SetColor(Color.Gray);
                }
                ws.Cells[row, 1].Style.Numberformat.Format = "#";

                for (int j = 0; j < Build.StatNames.Length; j++) {
                    var stat = Build.StatNames[j];

                    foreach (var sstr in new string[] { "flat", "perc" }) {
                        if (!((sstr == "flat" && j < 4) || (sstr == "perc" && j != 3)))
                            continue;

                        double? vvv = rf?.ContainsKey(stat) ?? false ? rf[stat][sstr] : null;
                        string sssForm = null;
                        if (vvv == null) {
                            if (slot > 0)
                                sssForm = "=" + abc[col - 1] + (rowstat - (slot) % 2);
                            else if (slot < 0)
                                sssForm = "=" + abc[col - 1] + (rowstat - 2);
                        }
                        if (sssForm == null)
                            ws.Cells[row, col].Value = vvv;
                        else {
                            ws.Cells[row, col].Formula = sssForm;
                            ws.Cells[row, col].Style.Font.Color.SetColor(Color.Gray);
                        }
                        ws.Cells[row, col].Style.Numberformat.Format = "#";
                        col++;
                    }
                }

                if (col > cmax)
                    cmax = col;

                row++;
            }

            var rstart = row;

            col = 6;
            foreach (var a in Build.StatBoth) {
                row = rstart;
                ws.Cells[row, col].CreateArrayFormula($"({abc[col - 1]}{row + 1}-{abc[col - 1]}{row + 2})/{abc[col - 1]}{row + 1}/MIN(ABS((F{row + 1}:P{row + 1}-F{row + 2}:P{row + 2})/F{row + 1}:P{row + 1}))");
                row++;
                ws.Cells[row, col].Formula = $"AVERAGEIF(runesFor{build.ID}[Good], \"{build.Best.score}\", runesFor{build.ID}[{a.ToString()}])";
                row++;
                ws.Cells[row, col].Formula = $"AVERAGEA(runesFor{build.ID}[{a.ToString()}])";
                row++;

                col++;
            }

            // table
            rstart = row;

            // maximum column (may need to be after writing table)
            col = runeBoardHeader(ws, ref row, ref cmax);

            // for each rune
            // make the pts use the hardwriten values and the appropriate slot weights

            // slot 0 = global, -1 = odd, -2 even

            var used = build.RuneUsage.runesUsed.Select(r => r.Key);
            var good = build.RuneUsage.runesGood.Select(r => r.Key);
            var second = build.RuneUsage.runesSecond.Select(r => r.Key);

            Console.WriteLine("Used runes: " + used.Count());

            foreach (var r in used.OrderByDescending(r => good.Contains(r)).ThenByDescending(r => build.ScoreRune(r, build.GetFakeLevel(r), false))) {
                col = 1;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Build.StatBoth.Length; i++) {
                    var attr = Build.StatBoth[i];
                    if (i != 0) sb.Append("+");
                    var statCell = "$" + abc[i + 5] + "$" + (rowstat + r.Slot);
                    sb.Append($"if({statCell}<>0, runesFor{build.ID}[[#This Row],[{attr}]]/{statCell}, 0)");
                }
                ws.Cells[row, col].Formula = sb.ToString();

                col++;
                ws.Cells[row, col].Value = r.Set;
                col++;
                ws.Cells[row, col].Value = r.Slot;
                col++;
                ws.Cells[row, col].Value = r.Main.Type;
                col++;
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (load?.Runes != null) {
                    double d;
                    r.manageStats.TryGetValue("cbp" + build.ID, out d);
                    ws.Cells[row, col].Value = d;
                    if (load.Runes.Contains(r)) {
                        //ws.Cells[row, col].Value = "Best";
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.MediumTurquoise);
                    }
                    else if (second.Contains(r)) {
                        //ws.Cells[row, col].Value = "Second";
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Teal);
                    }
                    else if (good.Contains(r)) {
                        //ws.Cells[row, col].Value = "Good";
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                    }
                    else {
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    }
                }

                col++;

                foreach (Attr stat in Build.StatBoth) {
                    var fake = build.GetFakeLevel(r);

                    var rval = r[stat, fake, false];

                    //if (rval > 0)
                    {
                        ws.Cells[row, col].Value = rval;
                    }
                    ws.Cells[row, col].Style.Numberformat.Format = "#";
                    col++;
                }
                if (col > cmax)
                    cmax = col;
                row++;
            }

            OfficeOpenXml.Table.ExcelTable table = null;
            try {
                table = ws.Tables.FirstOrDefault(t => t.Name == "runesFor" + build.ID);
                if (table == null)
                    table = ws.Tables.Add(ws.Cells[rstart, 1, row - 1, cmax - 1], "runesFor" + build.ID);
            }
            catch (Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (table.Address.Columns != cmax - 1 || table.Address.Rows != row - 1) {
                var newRange = new ExcelAddress(rstart, 1, row - 1, cmax - 1).ToString();

                var tableElement = table.TableXml.DocumentElement;
                tableElement.Attributes["ref"].Value = newRange;
                tableElement["autoFilter"].Attributes["ref"].Value = newRange;

            }

            table.ShowHeader = true;
            table.StyleName = "TableStyleMedium2";

            var cond = ws.ConditionalFormatting.AddExpression(new ExcelAddress(rstart + 1, 1, row - 1, 1));
            cond.Formula = "A" + (rstart + 1) + ">=INDIRECT(ADDRESS(" + (rstart - 7) + "+C" + (rstart + 1) + ",1))";
            cond.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cond.Style.Fill.BackgroundColor.Color = Color.LimeGreen;

            cond = ws.ConditionalFormatting.AddExpression(new ExcelAddress(rstart + 1, 1, row - 1, 1));
            cond.Formula = "A" + (rstart + 1) + "<INDIRECT(ADDRESS(" + (rstart - 7) + "+C" + (rstart + 1) + ",1))";
            cond.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cond.Style.Fill.BackgroundColor.Color = Color.Red;
        }

        private static int runeBoardHeader(ExcelWorksheet ws, ref int row, ref int cmax) {
            // PTS, set, slot, primary, HP, %, ATK, %, DEF, %, SPD, CR, CD, RES, ACC
            int col = 1;
            ws.Cells[row, col].Value = "Pts";
            col++;
            ws.Cells[row, col].Value = "Set";
            col++;
            ws.Cells[row, col].Value = "Slot";
            col++;
            ws.Cells[row, col].Value = "Main";
            col++;
            ws.Cells[row, col].Value = "Good";
            col++;
            foreach (Attr stat in Build.StatBoth) {
                ws.Cells[row, col].Value = stat.ToString();
                col++;
                if (col > cmax)
                    cmax = col;
            }
            row++;
            return col;
        }

        public void StatsExcelRunes(bool passiveBind) {
            if (StatsExcelBind(passiveBind)) {
                StatsExcelRunes();
                StatsExcelSave();
            }
            StatsExcelBind(true);
        }

        void StatsExcelRunes() {
            if (Program.data?.Runes == null || excelPack == null)
                return;

            var ws = excelSheets.FirstOrDefault(w => w.Name == "Runes");
            if (ws == null) {
                ws = excelSheets.Add("Runes");
                excelSheets.MoveAfter(ws.Index, 1);
            }
            else {
                var ind = ws.Index;
                excelSheets.Delete("Runes");
                ws = excelPack.Workbook.Worksheets.Add("Runes");
                excelSheets.MoveBefore(ws.Index, ind);
            }

            int tTop = (int)Math.Log((int)RuneSet.Broken, 2) + 2;
            int tLeft = 1;

            int row = tTop;
            int col = tLeft;

            List<string> colHead = new List<string>();

            // ,MType,Points,FlatPts
            foreach (var th in "Id,Grade,Set,Slot,Main,Innate,1,2,3,4,Level,SellVal,Select,Rune,Type,Load,Gen,Eff,EffMax,VPM,Used,Priority,CurMon,Mon,RatingScore,Keep,Action, ,BuildPercent,HPpts,ATKpts,Pts,_,Rarity,Orig,Flats,HPF,HPP,ATKF,ATKP,DEFF,DEFP,SPD,CR,CD,RES,ACC,BuildG,BuildT".Split(',')) {
                colHead.Add(th);
                ws.Cells[row, col].Value = th; col++;
            }
            row++;
            col = 1;


            // calculate the stats
            foreach (Rune r in Program.data.Runes) {
                double keep = StatsKeepRune(r, colHead);
                r.manageStats.AddOrUpdate("Keep", keep, (s, d) => keep);
            }

            int rr = 0;
            // from keepscore 1 to 100
            foreach (Rune r in Program.data.Runes.Where(r => r.manageStats["In"].EqualTo(0)).OrderBy(r => r.manageStats.GetOrAdd("Keep", 0))) {
                rr++;
                if (rr < Program.data.Runes.Count(ru => ru.manageStats["In"].EqualTo(0)) * 0.25) {
                    if (!r.manageStats.ContainsKey("Action"))
                        r.manageStats["Action"] = -2;
                }
                else if (rr < Program.data.Runes.Count(ru => ru.manageStats["In"].EqualTo(0)) * 0.5) {
                    if (r.manageStats["Action"].EqualTo(0))
                        r.manageStats["Action"] = -3;
                }
                else if (rr < Program.data.Runes.Count(ru => ru.manageStats["In"].EqualTo(0)) * 0.75) {
                    if (r.manageStats["Action"].EqualTo(0))
                        r.manageStats["Action"] = -1;
                }
                else {
                    r.manageStats["Action"] = -1;
                    if (r.Level < 6)
                        r.manageStats["Action"] = 6;
                    else if (r.Level < 9)
                        r.manageStats["Action"] = 9;
                    else if (r.Level < 12)
                        r.manageStats["Action"] = 12;
                }
            }

            foreach (Rune r in Program.data.Runes.OrderBy(r => r.manageStats.GetOrAdd("Keep", 0))) {
                Monster m = null;
                if (!r.manageStats.GetOrAdd("Mon", 0).EqualTo(0)) {
                    m = Program.data.GetMonster((ulong)r.manageStats["Mon"]);
                }

                Build b = null;
                if (m != null) {
                    b = Program.builds.FirstOrDefault(bu => bu.Mon == m);
                }

                for (col = 1; col <= colHead.Count; col++) {
                    switch (colHead[col - 1]) {
                        case "Id":
                            if (r._extra > 0) {
                                Color color = Color.FromArgb(255, 190, 190, 190);
                                if (r._extra == 5) color = Color.FromArgb(255, 255, 153, 0);
                                else if (r._extra == 4) color = Color.FromArgb(255, 204, 0, 153);
                                else if (r._extra == 3) color = Color.FromArgb(255, 102, 205, 255);
                                else if (r._extra == 2) color = Color.FromArgb(255, 146, 208, 80);
                                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
                            }
                            ws.Cells[row, col].Value = r.Id;
                            break;
                        case "Grade":
                            ws.Cells[row, col].Value = r.Grade;
                            break;
                        case "Rarity":
                            ws.Cells[row, col].Value = r.Rarity;
                            break;
                        case "Orig":
                            ws.Cells[row, col].Value = r._extra;
                            break;
                        case "Set":
                            if (r.Rarity > 0) {
                                Color color = Color.FromArgb(255, 146, 208, 80);
                                if (r.Rarity == 4) color = Color.FromArgb(255, 255, 153, 0);
                                else if (r.Rarity == 3) color = Color.FromArgb(255, 204, 0, 153);
                                else if (r.Rarity == 2) color = Color.FromArgb(255, 102, 205, 255);

                                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
                            }
                            ws.Cells[row, col].Value = r.Set;
                            break;
                        case "Slot":
                            ws.Cells[row, col].Value = r.Slot;
                            break;
                        case "MType":
                            ws.Cells[row, col].Value = r.Main.Type.ToGameString();
                            break;
                        case "Level":
                            ws.Cells[row, col].Value = r.Level;
                            break;
                        case "SellVal":
                            ws.Cells[row, col].Value = r.SellValue;
                            break;
                        case "Select":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("Set", 0);
                            break;
                        case "Rune":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("RuneFilt", 0);
                            break;
                        case "Type":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("TypeFilt", 0);
                            break;
                        case "Load":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("LoadFilt", 0);
                            break;
                        case "Gen":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("LoadGen", 0);
                            break;
                        case "BuildG":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("buildScoreIn", 0);
                            break;
                        case "BuildT":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("buildScoreTotal", 0);
                            break;
                        case "BuildPercent":
                            ws.Cells[row, col].Value = r.manageStats.GetOrAdd("bestBuildPercent", 0);
                            break;
                        case "Eff":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.BarionEfficiency;
                            break;
                        case "EffMax":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.MaxEfficiency;
                            break;
                        case "VPM":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.VivoPrestoModel;
                            break;
                        case "Used":
                            switch ((int)r.manageStats.GetOrAdd("In", 0)) {
                                case 1:
                                    ws.Cells[row, col].Value = "Best";
                                    break;
                                case 2:
                                    ws.Cells[row, col].Value = "Second";
                                    break;
                                default:
                                    ws.Cells[row, col].Value = "No";
                                    break;
                            }
                            break;
                        case "Points":
                            break;
                        case "Priority":
                            ws.Cells[row, col].Value = b?.Priority;
                            break;
                        case "CurMon":
                            if (!r.IsUnassigned)
                                ws.Cells[row, col].Value = r.AssignedName;
                            break;
                        case "Mon":
                            ws.Cells[row, col].Value = m?.FullName;
                            break;
                        case "Flats":
                            ws.Cells[row, col].Value = r.FlatCount();
                            break;
                        case "FlatPts":
                            ws.Cells[row, col].Style.Numberformat.Format = "[>0]0.00;";
                            break;
                        case "RatingScore":
                            ws.Cells[row, col].Value = r.ComputeRating();
                            break;
                        case "Keep":
                            string fstr;
                            StatsKeepRune(r, colHead, out fstr);
                            ws.Cells[row, col].Formula = fstr;
                            break;
                        case "Action":
                            if (r.manageStats.GetOrAdd("Action", 0).EqualTo(0)) {
                                if (r.ScoreATK() < 0.5
                                    && r.ScoreHP() < 0.5
                                    && r.ScoreRune() < 0.5
                                    && r.BarionEfficiency < 0.5
                                    && r.manageStats.GetOrAdd("Keep", 0) < 40) {
                                    ws.Cells[row, col].Value = "Sell";
                                }
                            }
                            else if (r.manageStats.GetOrAdd("Action", 0) > 0)
                                ws.Cells[row, col].Value = "To " + r.manageStats.GetOrAdd("Action", 0);
                            else if (r.manageStats.GetOrAdd("Action", 0).EqualTo(-1))
                                ws.Cells[row, col].Value = "Keep";
                            else if (r.manageStats.GetOrAdd("Action", 0).EqualTo(-2))
                                ws.Cells[row, col].Value = "Sell";
                            else if (r.manageStats.GetOrAdd("Action", 0).EqualTo(-3))
                                ws.Cells[row, col].Value = "Consider";
                            break;
                        case "Main":
                            ws.Cells[row, col].Value = r.Main.Value.ToString() + " " + r.Main.Type.ToGameString();
                            break;
                        case "Innate":
                            if (r.Innate != null && r.Innate.Type > Attr.Null)
                                ws.Cells[row, col].Value = r.Innate.Value.ToString() + " " + r.Innate.Type.ToGameString();
                            break;
                        case "1":
                            if (r.Subs.Count > 0)
                                ws.Cells[row, col].Value = r.Subs[0].Value.ToString() + " " + r.Subs[0].Type.ToGameString();
                            break;
                        case "2":
                            if (r.Subs.Count > 1)
                                ws.Cells[row, col].Value = r.Subs[1].Value.ToString() + " " + r.Subs[1].Type.ToGameString();
                            break;
                        case "3":
                            if (r.Subs.Count > 2)
                                ws.Cells[row, col].Value = r.Subs[2].Value.ToString() + " " + r.Subs[2].Type.ToGameString();
                            break;
                        case "4":
                            if (r.Subs.Count > 3)
                                ws.Cells[row, col].Value = r.Subs[3].Value.ToString() + " " + r.Subs[3].Type.ToGameString();
                            break;
                        case "HPpts":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.ScoreHP();
                            break;
                        case "ATKpts":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.ScoreATK();
                            break;
                        case "Pts":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.ScoreRune();
                            break;
                        case "HPF":
                            ws.Cells[row, col].Value = r.HealthFlat[0];
                            break;
                        case "HPP":
                            ws.Cells[row, col].Value = r.HealthPercent[0];
                            break;
                        case "ATKF":
                            ws.Cells[row, col].Value = r.AttackFlat[0];
                            break;
                        case "ATKP":
                            ws.Cells[row, col].Value = r.AttackPercent[0];
                            break;
                        case "DEFF":
                            ws.Cells[row, col].Value = r.DefenseFlat[0];
                            break;
                        case "DEFP":
                            ws.Cells[row, col].Value = r.DefensePercent[0];
                            break;
                        case "SPD":
                            ws.Cells[row, col].Value = r.Speed[0];
                            break;
                        case "CR":
                            ws.Cells[row, col].Value = r.CritRate[0];
                            break;
                        case "CD":
                            ws.Cells[row, col].Value = r.CritDamage[0];
                            break;
                        case "RES":
                            ws.Cells[row, col].Value = r.Resistance[0];
                            break;
                        case "ACC":
                            ws.Cells[row, col].Value = r.Accuracy[0];
                            break;

                    }
                }
                row++;
            }

            int cmax = colHead.Count + 1;

            var table = ws.Tables.FirstOrDefault(t => t.Name == "RuneTable");
            if (table == null)
                table = ws.Tables.Add(ws.Cells[tTop, tLeft, row - 1, cmax - 1], "RuneTable");

            if (table.Address.Columns != cmax - tLeft || table.Address.Rows != row - tTop) {
                var start = table.Address.Start;
                var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(tTop, tLeft, row - 1, cmax - 1));

                var tableElement = table.TableXml.DocumentElement;
                tableElement.Attributes["ref"].Value = newRange;
                tableElement["autoFilter"].Attributes["ref"].Value = newRange;
            }

            table.ShowHeader = true;
            table.StyleName = "TableStyleMedium2";
            // write rune stats

            // Breakdown by Set
            row = 1;
            col = 2;

            ws.Cells[row, col].Formula = "COUNT(RuneTable[Id])"; col++;
            ws.Cells[row, col].Formula = "SUM(C3:C23)"; col++;
            ws.Cells[row, col].Formula = "SUM(D3:D23)"; col++;

            tLeft = 2;
            tTop = 2;

            row = tTop;
            col = tLeft;
            cmax = tLeft;

            ws.Cells[row, col].Value = "Set"; col++;
            ws.Cells[row, col].Value = "Used"; col++;
            ws.Cells[row, col].Value = "Stored"; col++;
            ws.Cells[row, col].Value = "Usage"; col++;
            ws.Cells[row, col].Value = "Storage"; col++;
            ws.Cells[row, col].Value = "Total"; col++;
            ws.Cells[row, col].Value = "Average Eff"; col++;
            ws.Cells[row, col].Value = "Main2"; col++;
            ws.Cells[row, col].Value = "Main4"; col++;
            ws.Cells[row, col].Value = "Main6"; col++;
            ws.Cells[row, col].Value = "S1"; col++;
            ws.Cells[row, col].Value = "S2"; col++;
            ws.Cells[row, col].Value = "S3"; col++;
            ws.Cells[row, col].Value = "S4"; col++;

            foreach (var attr in "HPF,HPP,ATKF,ATKP,DEFF,DEFP,SPD,CR,CD,RES,ACC".Split(',')) {
                ws.Cells[row, col].Value = attr; col++;
            }

            row = 3;
            cmax = Math.Max(cmax, col);
            col = 2;

            string getType = "MID({0}, FIND(\" \",{0})+1, 10)";
            string matchString = "MATCH({0}, {0}, 0)";
            string modeText = "INDEX({0}, MODE({1}))";
            string ifblank = "IF({0},{1},\"\")";
            string ifError = "IFERROR({0},\"\")";

            Func<string, int, string> makeHardcore = (a, b) =>
            {
                string runetableSub = string.Format(getType, $"RuneTable[{a}]");
                string runetableMatch = string.Format(matchString, runetableSub);
                string ifs = string.Format(ifblank, "RuneTable[Set]=B" + row, runetableMatch);
                if (b > 0)
                    ifs = string.Format(ifblank, $"RuneTable[Slot]={b}", ifs);
                ifs = string.Format(ifblank, "RuneTable[Used]=\"Best\"", ifs);
                string indMode = string.Format(modeText, $"RuneTable[{a}]", ifs);
                string finalCut = string.Format(getType, indMode);
                return string.Format(ifError, finalCut);
            };

            foreach (var rs in Enum.GetNames(typeof(RuneSet))) {
                cmax = Math.Max(cmax, col);
                col = 2;
                if (rs[0] != '_' && rs != "Null" && rs != "Broken" && rs != "Unknown") {
                    ws.Cells[row, col].Value = rs.ToString(); col++;
                    ws.Cells[row, col].Formula = $"COUNTIFS(RuneTable[Set],B{row},RuneTable[CurMon],\"<>\")+COUNTIFS(RuneTable[Set],B{row},RuneTable[Mon],\"<>\")-COUNTIFS(RuneTable[Set],B{row},RuneTable[CurMon],\"<>\",RuneTable[Mon],\"<>\")"; col++;
                    ws.Cells[row, col].Formula = $"COUNTIFS(RuneTable[Set],B{row},RuneTable[CurMon],\"\",RuneTable[Mon],\"\")"; col++;
                    ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                    ws.Cells[row, col].Formula = $"C{row}/C$1"; col++;
                    ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                    ws.Cells[row, col].Formula = $"D{row}/D$1"; col++;
                    ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                    ws.Cells[row, col].Formula = $"(C{row}+D{row})/B$1"; col++;
                    // =IF(D3=0,"",AVERAGEIFS(RuneTable[Eff],RuneTable[Set],C3,RuneTable[Used],"Best"))
                    ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                    ws.Cells[row, col].Formula = $"IF(C{row}=0,\"\",(SUMIFS(RuneTable[Eff],RuneTable[Set],B{row},RuneTable[CurMon],\"<>\")+SUMIFS(RuneTable[Eff],RuneTable[Set],B{row},RuneTable[Mon],\"<>\")-SUMIFS(RuneTable[Eff],RuneTable[Set],B{row},RuneTable[CurMon],\"<>\",RuneTable[Mon],\"<>\"))/DetailedRunes[[#This Row],[Used]])"; col++;

                    ws.Cells[row, col].CreateArrayFormula(makeHardcore("Main", 2)); col++;
                    ws.Cells[row, col].CreateArrayFormula(makeHardcore("Main", 4)); col++;
                    ws.Cells[row, col].CreateArrayFormula(makeHardcore("Main", 6)); col++;
                    ws.Cells[row, col].CreateArrayFormula(makeHardcore("1", 0)); col++;
                    ws.Cells[row, col].CreateArrayFormula(makeHardcore("2", 0)); col++;
                    ws.Cells[row, col].CreateArrayFormula(makeHardcore("3", 0)); col++;
                    ws.Cells[row, col].CreateArrayFormula(makeHardcore("4", 0)); col++;

                    foreach (var attr in "HPF,HPP,ATKF,ATKP,DEFF,DEFP,SPD,CR,CD,RES,ACC".Split(',')) {
                        ws.Cells[row, col].Formula = $"IFERROR(AVERAGEIFS(RuneTable[{attr}], RuneTable[Set], B{row},RuneTable[{attr}],\"<>0\", RuneTable[Used], \"Best\"),\"\")"; col++;
                    }

                    row++;
                }
            }

            table = ws.Tables.FirstOrDefault(t => t.Name == "DetailedRunes");
            if (table == null)
                table = ws.Tables.Add(ws.Cells[tTop, tLeft, row - 1, cmax - 1], "DetailedRunes");

            if (table.Address.Columns != cmax - tLeft || table.Address.Rows != row - tTop) {
                var start = table.Address.Start;
                var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(tTop, tLeft, row - 1, cmax - 1));

                var tableElement = table.TableXml.DocumentElement;
                tableElement.Attributes["ref"].Value = newRange;
                tableElement["autoFilter"].Attributes["ref"].Value = newRange;
            }

            table.ShowHeader = true;
            table.StyleName = "TableStyleMedium2";

        }

        double StatsKeepRune(Rune r, List<string> heads) {
            string t;
            return StatsKeepRune(r, heads, out t);
        }

        double StatsKeepRune(Rune r, List<string> heads, out string formula) {
            double keep = 0;
            formula = "";

            //foreach (var th in "Id,Grade,Set,Slot,Main,Innate,1,2,3,4,Level,Select,Rune,Type,Load,Gen,Eff,Used,Mon,Keep,Action, ,HPpts,ATKpts,Pts,_,Rarity,Flats,SPD,HPP,ACC".Split(','))

            keep += Math.Pow(r.Level, 0.7);
            formula += "power(" + (heads.Contains("Level") ? "RuneTable[[#This Row],[Level]]" : r.Level.ToString()) + ",0.7)";

            keep += Math.Pow(r.Grade, 1.4);
            formula += "+power(" + (heads.Contains("Grade") ? "RuneTable[[#This Row],[Grade]]" : r.Grade.ToString()) + ",1.4)";

            r.manageStats["Action"] = 0;
            if (!r.manageStats.ContainsKey("In"))
                r.manageStats["In"] = 0;

            r.manageStats.GetOrAdd("Mon", 0);
            if (r.manageStats["In"] > 0) {
                var b = Program.builds.FirstOrDefault(bu => bu.Best != null && bu.Best.Current.Runes.Contains(r));
                r.manageStats["Action"] = -1;
                if (b == null) {
                    r.manageStats["Priority"] = 2;
                    if (r.Slot % 2 == 0 && r.Level < 15)
                        r.manageStats["Action"] = 15;
                    if (r.Slot % 2 == 1 && r.Level < 12)
                        r.manageStats["Action"] = 12;
                }
                else {
                    r.manageStats["Mon"] = b.Mon.Id;
                    r.manageStats["Priority"] = b.Priority / (double)Program.builds.Max(bu => bu.Priority);
                    int p = b.GetFakeLevel(r);
                    if (r.Level < p)
                        r.manageStats["Action"] = p;
                }
                keep += 10;
            }
            formula += heads.Contains("Used") ? "+if(RuneTable[[#This Row],[Used]]<>\"No\",10,0)" : ((r.manageStats["In"] > 0) ? "+10" : "");

            // TODO: skip upgrading if rune is trash

            if (r.Rarity > Math.Floor(r.Level / (double)3)) {
                keep += Math.Pow(r.Rarity - Math.Min(4, Math.Floor(r.Level / (double)3)), 1.1) * 6;
                if (r.Rarity > Math.Floor(r.Level / (double)3) + 1)
                    r.manageStats["Action"] = r.Rarity * 3;
            }
            /**/
            formula += "+if(" + (heads.Contains("Rarity") ? "RuneTable[[#This Row],[Rarity]]" : r.Rarity.ToString())
            + ">floor(" + (heads.Contains("Level") ? "RuneTable[[#This Row],[Level]]" : r.Level.ToString()) + "/3,1),power("
            + (heads.Contains("Rarity") ? "RuneTable[[#This Row],[Rarity]]" : r.Rarity.ToString()) + "-min(4,floor(" + (heads.Contains("Level") ? "RuneTable[[#This Row],[Level]]" : r.Level.ToString()) + "/3,1)),1.1)*6,0)";//*/

            keep -= r.FlatCount();
            formula += "-" + (heads.Contains("Flats") ? "RuneTable[[#This Row],[Flats]]" : r.FlatCount().ToString()) + "";

            keep += r.BarionEfficiency * 5;
            formula += "+" + (heads.Contains("Eff") ? "RuneTable[[#This Row],[Eff]]" : r.BarionEfficiency.ToString(System.Globalization.CultureInfo.CurrentUICulture)) + "*5";

            keep += r.ScoreRune() * Math.Max(r.ScoreHP(), r.ScoreATK()) * 20;
            /**/
            formula += "+" + (heads.Contains("Pts") ? "RuneTable[[#This Row],[Pts]]" : r.ScoreRune().ToString(System.Globalization.CultureInfo.CurrentUICulture)) + "*max("
            + (heads.Contains("HPpts") ? "RuneTable[[#This Row],[HPpts]]" : r.ScoreHP().ToString(System.Globalization.CultureInfo.CurrentUICulture)) + "," + (heads.Contains("ATKpts") ? "RuneTable[[#This Row],[ATKpts]]" : r.ScoreATK().ToString(System.Globalization.CultureInfo.CurrentUICulture)) + ")*20";//*/

            keep += r.Speed[0];
            formula += "+" + (heads.Contains("SPD") ? "RuneTable[[#This Row],[SPD]]" : r.Speed[0].ToString()) + "";
            keep += r.HealthPercent[0] * 0.3;
            formula += "+" + (heads.Contains("HPP") ? "RuneTable[[#This Row],[HPP]]" : r.HealthPercent[0].ToString()) + "*0.3";
            keep += r.Accuracy[0] * 0.4;
            formula += "+" + (heads.Contains("ACC") ? "RuneTable[[#This Row],[ACC]]" : r.Accuracy[0].ToString()) + "*0.4";

            keep += (Math.Pow(1.004, r.manageStats.GetOrAdd("Set", 0)) - 1) * 10;
            formula += "+(power(1.004, " + (heads.Contains("Select") ? "RuneTable[[#This Row],[Select]]" : r.manageStats.GetOrAdd("Set", 0).ToString(System.Globalization.CultureInfo.CurrentUICulture)) + ")-1)*10";

            keep += (Math.Pow(1.007, r.manageStats.GetOrAdd("RuneFilt", 0)) - 1) * 10;
            formula += "+(power(1.007, " + (heads.Contains("Rune") ? "RuneTable[[#This Row],[Rune]]" : r.manageStats.GetOrAdd("RuneFilt", 0).ToString(System.Globalization.CultureInfo.CurrentUICulture)) + ")-1)*10";

            keep += (Math.Pow(1.01, r.manageStats.GetOrAdd("TypeFilt", 0)) - 1) * 10;
            formula += "+(power(1.01, " + (heads.Contains("Type") ? "RuneTable[[#This Row],[Type]]" : r.manageStats.GetOrAdd("TypeFilt", 0).ToString(System.Globalization.CultureInfo.CurrentUICulture)) + ")-1)*10";

            if (r.manageStats.GetOrAdd("LoadGen", 0) > 0) {
                keep += Math.Pow(r.manageStats.GetOrAdd("LoadFilt", 0) / r.manageStats["LoadGen"], 1.1) * 10;
            }
            /**/
            formula += "+if(" + (heads.Contains("Gen") ? "RuneTable[[#This Row],[Gen]]" : r.manageStats["LoadGen"].ToString(System.Globalization.CultureInfo.CurrentUICulture)) + ">0,power("
            + (heads.Contains("Load") ? "RuneTable[[#This Row],[Load]]" : r.manageStats.GetOrAdd("LoadFilt", 0).ToString(System.Globalization.CultureInfo.CurrentUICulture)) + "/"
            + (heads.Contains("Gen") ? "RuneTable[[#This Row],[Gen]]" : r.manageStats["LoadGen"].ToString(System.Globalization.CultureInfo.CurrentUICulture)) + ",1.1)*10,0)";//*/

            r.manageStats.AddOrUpdate("Keep", keep, (s, d) => keep);
            return keep;
        }

    }
}
