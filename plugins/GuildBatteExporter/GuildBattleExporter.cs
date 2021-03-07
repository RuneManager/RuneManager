using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunePlugin;
using OfficeOpenXml;
using System.IO;
using System.Text.RegularExpressions;

namespace GuildBattleExporter {
    public class GuildBattleExporter : RunePlugin.SWPlugin {

        object fileLock = new object();

        public override void ProcessRequest(object sender, SWEventArgs args) {
            try {
                if (args.Response.Command == SWCommand.GetGuildWarMatchupInfo) {
                    lock (fileLock) {
                        bakeMatchup(args);
                    }
                }

                if (args.Response.Command == SWCommand.GetGuildWarParticipationInfo) {
                    lock (fileLock) {
                        bakeParticipation(args);
                    }
                }

                if (args.Response.Command == SWCommand.GetGuildInfo) {
                    lock (fileLock) {
                        bakeMembers(args);
                    }
                }

                if (args.Response.Command == SWCommand.GetGuildWarBattleLogByGuildId) {
                    lock (fileLock) {
                        bakeByGuildId(args);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.GetType() + ": " + e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        void bakeMatchup(SWEventArgs args) {
            var resp = args.ResponseAs<RunePlugin.Response.GetGuildWarMatchupInfoResponse>();

            if (!resp.AttackerList.Any())
                return;

            FileInfo excelFile = new FileInfo(PluginDataDirectory + @"\GuildBattle.xlsx");
            ExcelPackage excelPack = new ExcelPackage(excelFile);

            var safeGuildName = new Regex("[^A-Za-z0-9 _.]").Replace(resp.OppGuildInfo.Name, "");
            if ("0123456789".Contains(safeGuildName.First()))
                safeGuildName = "_" + safeGuildName;

            var safeId = resp.AttackerList.First().MatchId;
            var tablePrefix = safeGuildName.Replace(" ", "_") + "_" + safeId;

            List<string> guildies = new List<string>();
            Dictionary<string, long> guildyId = new Dictionary<string, long>();

            int row = 1;
            int col = 1;

            var mods = excelPack.Workbook.Worksheets.FirstOrDefault(s => s.Name == "modifiers");
            if (mods != null) {
                var memTab = mods.Tables.FirstOrDefault(t => t.Name == "Members");
                if (memTab != null) {
                    var memCol = memTab.Columns.FirstOrDefault(c => c.Name == "Member");
                    if (memCol != null) {
                        row = memTab.Address.Start.Row;
                        col = memTab.Address.Start.Column;

                        while (row != memTab.Address.End.Row) {
                            row++;
                            guildies.Add(mods.Cells[row, col].Value.ToString());
                            guildyId.Add(mods.Cells[row, col].Value.ToString(), long.Parse(mods.Cells[row, col + 1].Value.ToString()));
                        }

                    }
                }
            }
            else {
                return;
            }

            var guildSheet = excelPack.Workbook.Worksheets.FirstOrDefault(s => s.Cells[1, 1].Value != null && s.Cells[1, 1].Value.ToString() == safeId.ToString());
            if (guildSheet == null)
                return;

            var atkTab = guildSheet.Tables.FirstOrDefault(t => t.Name == tablePrefix + "");
            if (atkTab == null)
                return;

            var atkCol = atkTab.Columns.FirstOrDefault(c => c.Name == "Attacker?");
            if (atkCol == null)
                return;

            var nameCol = atkTab.Columns.FirstOrDefault(c => c.Name == "Member");
            if (nameCol == null)
                return;

            col = atkTab.Address.Start.Column + nameCol.Position;

            foreach (var m in guildyId) {
                row = atkTab.Address.Start.Row + 1;
                while (guildSheet.Cells[row, col].Value.ToString() != m.Key) {
                    row++;
                }

                if (resp.AttackerList.Any(a => a.WizardId == m.Value)) {
                    guildSheet.Cells[row, atkTab.Address.Start.Column + atkCol.Position].Value = "Yes";
                }
                else {
                    guildSheet.Cells[row, atkTab.Address.Start.Column + atkCol.Position].Value = "No";
                }
            }


            var enTab = guildSheet.Tables.FirstOrDefault(t => t.Name == tablePrefix + "_mem");
            if (enTab == null)
                return;

            col = enTab.Address.Start.Column;
            row = enTab.Address.Start.Row + 1;

            List<string> enCur = new List<string>();
            while (!string.IsNullOrWhiteSpace(guildSheet.Cells[row, col].Value?.ToString())) {
                enCur.Add(guildSheet.Cells[row, col].Value.ToString());
                row++;
            }

            foreach (var enemy in resp.OppGuildMembers.Where(e => !enCur.Contains(e.WizardName))) {
                guildSheet.Cells[row, col].Value = enemy.WizardName;
                row++;
            }


            if (enTab.Address.Rows != row - 1) {
                var start = enTab.Address.Start;
                var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(enTab.Address.Start.Row, enTab.Address.Start.Column, row - 1, enTab.Address.Start.Column + enTab.Address.Columns).End.Address);

                var tableElement = enTab.TableXml.DocumentElement;
                tableElement.Attributes["ref"].Value = newRange;
                tableElement["autoFilter"].Attributes["ref"].Value = newRange;
            }

            enTab.ShowHeader = true;
            enTab.StyleName = "TableStyleMedium2";

            try {
                excelPack.Save();
            }
            catch (Exception e) {
                Console.WriteLine(e.GetType() + ": " + e.Message);
            }
        }

        void bakeParticipation(SWEventArgs args) {
            var resp = args.ResponseAs<RunePlugin.Response.GetGuildWarParticipationInfoResponse>();

        }

        void bakeMembers(SWEventArgs args) {
            var resp = args.ResponseAs<RunePlugin.Response.GetGuildInfoResponse>();

            FileInfo excelFile = new FileInfo(PluginDataDirectory + @"\GuildBattle.xlsx");
            ExcelPackage excelPack = new ExcelPackage(excelFile);

            List<string> guildies = new List<string>();

            int row = 1;
            int col = 1;


            var mods = excelPack.Workbook.Worksheets.FirstOrDefault(s => s.Name == "modifiers");
            if (mods != null) {
                var memTab = mods.Tables.FirstOrDefault(t => t.Name == "Members");
                if (memTab != null) {
                    var memCol = memTab.Columns.FirstOrDefault(c => c.Name == "Member");
                    if (memCol != null) {
                        row = memTab.Address.Start.Row;
                        col = memTab.Address.Start.Column;

                        while (row != memTab.Address.End.Row) {
                            row++;
                            guildies.Add(mods.Cells[row, col].Value.ToString());
                        }

                    }
                }
            }
            else {
                mods = excelPack.Workbook.Worksheets.Add("modifiers");
            }

            row = 3;
            col = 6;

            string tableMembers = "Members";
            int tLeft = col;
            int tTop = row;

            string[] headMembers = { "Member", "Id", "Login" };

            foreach (var h in headMembers) {
                mods.Cells[row, col].Value = headMembers[col - 6];
                col++;
            }
            row++;

            foreach (var p in resp.Guild.Members.Values) {
                col = 6;
                foreach (var h in headMembers) {
                    switch (h) {
                        case "Member":
                            mods.Cells[row, col].Value = p.WizardName;
                            break;
                        case "Id":
                            mods.Cells[row, col].Value = p.WizardId;
                            break;
                        case "Login":
                            mods.Cells[row, col].Value = p.LastLogin;
                            break;
                    }
                    col++;
                }
                row++;
            }

            int cmax = headMembers.Length + 1;

            var table = mods.Tables.FirstOrDefault(t => t.Name == tableMembers);
            if (table == null)
                table = mods.Tables.Add(mods.Cells[tTop, tLeft, row - 1, tLeft + cmax - 2], tableMembers);

            if (table.Address.Columns != cmax - 1 || table.Address.Rows != row - 1) {
                var start = table.Address.Start;
                var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(tTop, tLeft, row - 1, tLeft + cmax - 2).End.Address);

                var tableElement = table.TableXml.DocumentElement;
                tableElement.Attributes["ref"].Value = newRange;
                tableElement["autoFilter"].Attributes["ref"].Value = newRange;
            }

            table.ShowHeader = true;
            table.StyleName = "TableStyleMedium2";

            try {
                excelPack.Save();
            }
            catch (Exception e) {
                Console.WriteLine(e.GetType() + ": " + e.Message);
            }
        }

        void bakeByGuildId(SWEventArgs args) {
            var resp = args.ResponseAs<RunePlugin.Response.GetGuildWarBattleLogByGuildIdResponse>();

            FileInfo excelFile = new FileInfo(PluginDataDirectory + @"\GuildBattle.xlsx");
            ExcelPackage excelPack = new ExcelPackage(excelFile);

            List<string> guildies = new List<string>();

            int row = 1;
            int col = 1;


            var mods = excelPack.Workbook.Worksheets.FirstOrDefault(s => s.Name == "modifiers");
            if (mods != null) {
                var memTab = mods.Tables.FirstOrDefault(t => t.Name == "Members");
                if (memTab != null) {
                    var memCol = memTab.Columns.FirstOrDefault(c => c.Name == "Member");
                    if (memCol != null) {
                        row = memTab.Address.Start.Row;
                        col = memTab.Address.Start.Column;

                        while (row != memTab.Address.End.Row) {
                            row++;
                            guildies.Add(mods.Cells[row, col].Value.ToString());
                        }

                    }
                }
            }
            else {
                mods = excelPack.Workbook.Worksheets.Add("modifiers");
            }

            var nameDic = new Dictionary<string, ExcelRange>();

            nameDic.Add("Dealt_Points", mods.Cells["C3"]);
            nameDic.Add("Dealt_Power", mods.Cells["C4"]);
            nameDic.Add("Attempt_Points", mods.Cells["C5"]);
            nameDic.Add("Attempt_Power", mods.Cells["C6"]);

            nameDic.Add("Free_Points", mods.Cells["C8"]);
            nameDic.Add("Off_Points", mods.Cells["C9"]);
            nameDic.Add("GP_Modifier", mods.Cells["C10"]);

            nameDic.Add("Last_Mod", mods.Cells["C12"]);
            nameDic.Add("_2nd_Last_Mod", mods.Cells["C13"]);

            nameDic.Add("Sword_Point", mods.Cells["C15"]);
            nameDic.Add("Sword_Power", mods.Cells["C16"]);

            foreach (var n in nameDic) {
                if (!excelPack.Workbook.Names.ContainsKey(n.Key))
                    excelPack.Workbook.Names.Add(n.Key, n.Value);
            }

            foreach (var battlelog in resp.BattleLogs.Where(b => b.BattleLogList.Any())) {
                var safeGuildName = new Regex("[^A-Za-z0-9 ._]").Replace(battlelog.EnemyGuild.Name, "");
                if ("0123456789".Contains(safeGuildName.First()))
                    safeGuildName = "_" + safeGuildName;

                var safeId = battlelog.BattleLogList.First().MatchId;
                var tablePrefix = safeGuildName.Replace(" ", "_") + "_" + safeId;

                var guildSheet = excelPack.Workbook.Worksheets.FirstOrDefault(s => s.Cells[1, 1].Value != null && s.Cells[1, 1].Value.ToString() == safeId.ToString());
                if (guildSheet == null) {
                    var ngss = excelPack.Workbook.Worksheets.Count(s => s.Name.Contains(safeGuildName)).ToString();
                    if (ngss == "0")
                        ngss = "";
                    guildSheet = excelPack.Workbook.Worksheets.Add(safeGuildName + ngss);
                }

                guildSheet.Cells[1, 1].Value = safeId;

                row = 2;
                col = 1;


                var headBattle = new string[] { "Attacker", "Defender", "Bonus", "Win", "Draw", "Loss", "Last Damage", "Hit 1", "Hit 2", "Miss 1", "Miss 2", "Current Damage" };
                var headScore = new string[] { "Member", "Attacker?", "Dealt", "Attempted", "Damage", "GP_base", "GP", "Swords", "2nd Last", "Last_Base", "Previous", "Score" };
                var headEnemy = new string[] { "Enemy", "Bonus" };

                #region Attack Table

                string tableAtk = tablePrefix + "_atk";
                int tLeft = col;
                int tTop = row;

                foreach (var h in headBattle) {
                    guildSheet.Cells[row, col].Value = headBattle[col - 1];
                    col++;
                }
                row++;

                var sorted = battlelog.BattleLogList.OrderBy(bl => bl.BattleEnd);
                foreach (var b in sorted) {
                    col = 1;
                    foreach (var h in headBattle) {
                        switch (h) {
                            case "Attacker":
                                guildSheet.Cells[row, col].Value = b.WizardName;
                                break;
                            case "Defender":
                                guildSheet.Cells[row, col].Value = b.OppWizardName;
                                break;
                            case "Bonus":
                                guildSheet.Cells[row, col].Formula = $"=VLOOKUP({tableAtk}[[#This Row],[Defender]], {tablePrefix}_mem,2,FALSE)*{tableAtk}[[#This Row],[Win]]";
                                break;
                            case "Win":
                                guildSheet.Cells[row, col].Value = b.WinCount;
                                break;
                            case "Draw":
                                guildSheet.Cells[row, col].Value = b.DrawCount;
                                break;
                            case "Loss":
                                guildSheet.Cells[row, col].Value = b.LoseCount;
                                break;
                            case "Last Damage":
                                guildSheet.Cells[row, col].CreateArrayFormula($"=IF(OR(B$2:B{row - 1}=B{row}),INDEX(L$2:L{row - 1},1+LARGE(IF(B$2:B{row - 1}=B{row},ROW(B$2:B{row - 1})-2,0),1)),100)");
                                break;
                            case "Hit 1":
                                guildSheet.Cells[row, col].Formula = $"=IF({tableAtk}[[#This Row],[Win]]>0,ROUND({tableAtk}[[#This Row],[Last Damage]]*0.3,0),0)";
                                break;
                            case "Hit 2":
                                guildSheet.Cells[row, col].Formula =
                                    $"=IF({tableAtk}[[#This Row],[Win]]>1,ROUND(({tableAtk}[[#This Row],[Last Damage]]-{tableAtk}[[#This Row],[Hit 1]])*0.3,0),0)+5*{tableAtk}[[#This Row],[Draw]]";
                                break;
                            case "Miss 1":
                                guildSheet.Cells[row, col].Formula = $"=IF({tableAtk}[[#This Row],[Loss]]>0,ROUND(({tableAtk}[[#This Row],[Last Damage]]-{tableAtk}[[#This Row],[Hit 1]])*0.3,0),0)";
                                break;
                            case "Miss 2":
                                guildSheet.Cells[row, col].Formula =
                                    $"=IF({tableAtk}[[#This Row],[Loss]]>1,ROUND(({tableAtk}[[#This Row],[Last Damage]]-{tableAtk}[[#This Row],[Hit 1]])*0.3,0),0)+10*{tableAtk}[[#This Row],[Draw]]";
                                break;
                            case "Current Damage":
                                guildSheet.Cells[row, col].Formula = $"={tableAtk}[[#This Row],[Last Damage]]-{tableAtk}[[#This Row],[Hit 1]]-{tableAtk}[[#This Row],[Hit 2]]";
                                break;
                        }
                        col++;
                    }
                    row++;
                }

                int cmax = headBattle.Length + 1;

                var table = guildSheet.Tables.FirstOrDefault(t => t.Name == tableAtk);
                if (table == null)
                    table = guildSheet.Tables.Add(guildSheet.Cells[tTop, tLeft, row - 1, cmax - 1], tableAtk);

                if (table.Address.Columns != cmax - 1 || table.Address.Rows != row - 1) {
                    var start = table.Address.Start;
                    var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(tTop, tLeft, row - 1, cmax - 1).End.Address);

                    var tableElement = table.TableXml.DocumentElement;
                    tableElement.Attributes["ref"].Value = newRange;
                    tableElement["autoFilter"].Attributes["ref"].Value = newRange;
                }

                table.ShowHeader = true;
                table.StyleName = "TableStyleMedium2";

                #endregion

                #region Score Table

                row = 2;
                col = 1 + headBattle.Length + 1;

                string tableScore = tablePrefix + "";
                tLeft = col;
                tTop = row;

                foreach (var h in headScore) {
                    guildSheet.Cells[row, col].Value = headScore[col - headBattle.Length - 2];
                    col++;
                }
                row++;

                //var sorted = battlelog.BattleLogList.OrderBy(bl => bl.BattleEnd);
                foreach (var g in guildies) {
                    col = 1 + headBattle.Length + 1;
                    // "Member", "Attacker?", "Dealt", "Attempted", "Damage", "GP_base", "GP", "Swords", "2nd Last", "Last_Base", "Previous", "Score"
                    var gbattles = battlelog.BattleLogList.Where(b => b.WizardName == g);
                    foreach (var h in headScore) {
                        var tbm = tableScore + "[[#This Row],[Member]]";
                        switch (h) {
                            case "Member":
                                guildSheet.Cells[row, col].Value = g;
                                break;
                            case "Attacker?":
                                if (string.IsNullOrWhiteSpace(guildSheet.Cells[row, col].Value?.ToString()))
                                    guildSheet.Cells[row, col].Value = gbattles.Any() ? "Yes" : "";
                                break;
                            case "Dealt":
                                guildSheet.Cells[row, col].Formula = $"=SUMIFS({tableAtk}[[Hit 2]], {tableAtk}[[Attacker]], {tbm})+SUMIFS({tableAtk}[[Hit 1]], {tableAtk}[[Attacker]], {tbm})";
                                break;
                            case "Attempted":
                                guildSheet.Cells[row, col].Formula = $"=SUMIFS({tableAtk}[[Miss 2]], {tableAtk}[[Attacker]], {tbm})+SUMIFS({tableAtk}[[Miss 1]], {tableAtk}[[Attacker]], {tbm})";
                                break;
                            case "Damage":
                                guildSheet.Cells[row, col].Formula = $"=ROUND(POWER({tableScore}[[#This Row],[Dealt]]*Dealt_Points,Dealt_Power),0)+ROUND(POWER({tableScore}[[#This Row],[Attempted]]*Attempt_Points,Attempt_Power),0)";
                                break;
                            case "GP_base":
                                guildSheet.Cells[row, col].Formula = $"=SUMIFS({tableAtk}[[Draw]],{tableAtk}[[Attacker]],{tbm})+SUMIFS({tableAtk}[[Win]],{tableAtk}[[Attacker]],{tbm})*3+SUMIFS({tableAtk}[[Bonus]],{tableAtk}[[Attacker]],{tbm})";
                                break;
                            case "GP":
                                guildSheet.Cells[row, col].Formula = $"={tableScore}[[#This Row],[GP_base]]*GP_Modifier";
                                break;
                            case "Swords":
                                guildSheet.Cells[row, col].Formula =
                                    $"=IF({tableScore}[[#This Row],[Attacker?]]=\"No\", Off_Points, ROUND(Sword_Point*POWER(6-SUMIFS({tableAtk}[[Win]],{tableAtk}[[Attacker]],{tbm})-SUMIFS({tableAtk}[[Draw]],{tableAtk}[[Attacker]],{tbm})-SUMIFS({tableAtk}[[Loss]],{tableAtk}[[Attacker]],{tbm}),Sword_Power),0))";
                                break;
                            case "2nd Last":
                                guildSheet.Cells[row, col].Formula = $"=IFERROR(VLOOKUP({tbm},tablename,12,FALSE),0)";
                                break;
                            case "Last_Base":
                                guildSheet.Cells[row, col].Formula = $"=IFERROR(VLOOKUP({tbm},tablename,12,FALSE),0)";
                                break;
                            case "Previous":
                                guildSheet.Cells[row, col].Formula = $"=ROUND(Last_Mod*{tableScore}[[#This Row],[Last_Base]]+_2nd_Last_Mod*{tableScore}[[#This Row],[2nd Last]],0)";
                                break;
                            case "Score":
                                guildSheet.Cells[row, col].Formula = $"={tableScore}[[#This Row],[Damage]]+{tableScore}[[#This Row],[Previous]]+{tableScore}[[#This Row],[Swords]]+{tableScore}[[#This Row],[GP]]";
                                break;
                        }
                        col++;
                    }
                    row++;
                }

                cmax = headScore.Length + 1;

                table = guildSheet.Tables.FirstOrDefault(t => t.Name == tableScore);
                if (table == null)
                    table = guildSheet.Tables.Add(guildSheet.Cells[tTop, tLeft, row - 1, tLeft + cmax - 2], tableScore);

                if (table.Address.Columns != cmax - 1 || table.Address.Rows != row - 1) {
                    var start = table.Address.Start;
                    var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(tTop, tLeft, row - 1, tLeft + cmax - 2).End.Address);

                    var tableElement = table.TableXml.DocumentElement;
                    tableElement.Attributes["ref"].Value = newRange;
                    tableElement["autoFilter"].Attributes["ref"].Value = newRange;
                }

                table.ShowHeader = true;
                table.StyleName = "TableStyleMedium2";

                #endregion

                #region Enemy Table

                row = 2;
                col = 1 + headBattle.Length + 1 + headScore.Length + 1;

                string tableEnemy = tablePrefix + "_mem";
                tLeft = col;
                tTop = row;

                foreach (var h in headEnemy) {
                    guildSheet.Cells[row, col].Value = headEnemy[col - 3 - headBattle.Length - headScore.Length];
                    col++;
                }
                row++;

                var baseGP = (int)Math.Round(battlelog.BattleLogList.GroupBy(b => b.OppWizardName).Select(b => b.Max(q => q.GuildPoints / 2)).Average() - 1.5);

                foreach (var en in battlelog.BattleLogList.Select(b => b.OppWizardName).Distinct()) {
                    col = 1 + headBattle.Length + 1 + headScore.Length + 1;
                    // "Enemy", "Bonus" 
                    var wins = battlelog.BattleLogList.Where(b => b.OppWizardName == en && b.WinCount > 0);
                    var escore = wins.Any() ? wins.Max(b => b.GuildPoints / b.WinCount) - baseGP : 0;
                    foreach (var h in headEnemy) {
                        switch (h) {
                            case "Enemy":
                                guildSheet.Cells[row, col].Value = en;
                                break;
                            case "Bonus":
                                guildSheet.Cells[row, col].Value = escore;
                                break;
                        }
                        col++;
                    }
                    row++;
                }

                cmax = headEnemy.Length + 1;

                table = guildSheet.Tables.FirstOrDefault(t => t.Name == tableEnemy);
                if (table == null)
                    table = guildSheet.Tables.Add(guildSheet.Cells[tTop, tLeft, row - 1, tLeft + cmax - 2], tableEnemy);

                if (table.Address.Columns != cmax - 1 || table.Address.Rows != row - 1) {
                    var start = table.Address.Start;
                    var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(tTop, tLeft, row - 1, tLeft + cmax - 2).End.Address);

                    var tableElement = table.TableXml.DocumentElement;
                    tableElement.Attributes["ref"].Value = newRange;
                    tableElement["autoFilter"].Attributes["ref"].Value = newRange;
                }

                table.ShowHeader = true;
                table.StyleName = "TableStyleMedium2";

                #endregion
            }
            try {
                excelPack.Save();
            }
            catch (Exception e) {
                Console.WriteLine(e.GetType() + ": " + e.Message);
            }
        }
    }
}