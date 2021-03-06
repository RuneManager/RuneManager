using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using RuneOptim;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;
using RuneOptim.Management;

namespace RuneApp {
    public partial class Main {

        // Functions for working with the Swar Save data

        private void loadSaveDialogue(object sender, EventArgs e) {
            loadSaveDialogue();
        }

        private LoadSaveResult loadSaveDialogue() {
            LoadSaveResult loadres = LoadSaveResult.Failure;
            using (var lsd = new LoadSaveDialogue()) {
                if (lsd.ShowDialog() == DialogResult.OK) // Test result.
                {
                    try {
                        loadres = Program.LoadSave(lsd.Filename);
                        RebuildLists();
                    }
                    catch (IOException ex) {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            return loadres;
        }

        private void addShrine(string stat, int num, int value, ToolStripMenuItem owner) {
            ToolStripMenuItem it = new ToolStripMenuItem(num.ToString() + (num > 0 ? " (" + value + "%)" : "")) {
                Tag = new KeyValuePair<string, int>(stat, value)
            };
            it.Click += ShrineClick;
            owner.DropDownItems.Add(it);
            if (!shrineMap.ContainsKey(stat))
                shrineMap.Add(stat, new List<ToolStripMenuItem>());
            shrineMap[stat].Add(it);
            if (Program.data.shrines[stat].EqualTo(value))
                it.Checked = true;
        }

        private void ShrineClick(object sender, EventArgs e) {
            var it = (ToolStripMenuItem)sender;
            if (it != null) {
                var tag = (KeyValuePair<string, int>)it.Tag;
                var stat = tag.Key;

                foreach (ToolStripMenuItem i in shrineMap[stat]) {
                    i.Checked = false;
                }
                it.Checked = true;
                if (Program.data == null)
                    return;

                if (string.IsNullOrWhiteSpace(stat))
                    return;

                Program.data.shrines[stat] = tag.Value;
                File.WriteAllText("shrine_overwrite.json", JsonConvert.SerializeObject(Program.data.shrines));
            }
        }

        private ListViewItem ListViewItemRune(Rune rune, ListViewItem nli = null) {
            if (nli == null)
                nli = new ListViewItem();
            nli.Tag = rune;
            nli.BackColor = rune.Locked ? Color.Red : Color.Transparent;

            while (nli.SubItems.Count < 8)
                nli.SubItems.Add("");

            nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, rune.Set.ToString());
            if (RuneProperties.setUnicode.ContainsKey(rune.Set))
                nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, RuneProperties.setUnicode[rune.Set]);
            nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, rune.Id.ToString());
            nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, rune.Grade.ToString());
            nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, Rune.StringIt(rune.Main.Type, true));
            nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, rune.Main.Value.ToString());
            nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, rune.Level.ToString());
            nli.SubItems[6] = new ListViewItem.ListViewSubItem(nli, rune.BarionEfficiency.ToString("0%"));
            nli.SubItems[7] = new ListViewItem.ListViewSubItem(nli, rune.MaxEfficiency.ToString("0%"));

            return nli;
        }

        private ListViewItem ListViewItemMonster(Monster mon, ListViewItem nli = null) {
            if (nli == null)
                nli = new ListViewItem();
            nli.Tag = mon;
            nli.Text = mon.FullName;

            while (nli.SubItems.Count < 6)
                nli.SubItems.Add("");

            nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, mon.FullName);
            nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, mon.Grade.ToString());
            nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, mon.priority.ToString("#"));
            nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, mon.Id.ToString());
            nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, mon.monsterTypeId.ToString());
            nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, mon.level.ToString());
            if (Program.builds.Any(b => b.MonId == mon.Id))
                nli.ForeColor = Color.Green;
            else if (mon.inStorage)
                nli.ForeColor = Color.Gray;
            return nli;
        }

        private void filterRunesList(Predicate<object> p) {
            dataRuneList.Items.Clear();

            if (Program.data?.Runes == null) return;

            foreach (Rune rune in Program.data.Runes.Where(p.Invoke)) {
                dataRuneList.Items.Add(ListViewItemRune(rune));
            }
        }

        private DialogResult CheckSaveChanges() {
            if (!Program.data.isModified)
                return DialogResult.Yes;

            var res = MessageBox.Show("Would you like to save changes to your imported data?", "Save Data", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
                Program.SaveData();
            return res;
        }

        public void ShowOptions() {
            using (var f = new Options()) {
                f.ShowDialog();
                findGoodRunes.Enabled = Program.Settings.MakeStats;
                if (!Program.Settings.MakeStats)
                    findGoodRunes.Checked = false;
            }
        }

        public void OpenHelp(string url = null, Form owner = null) {
            if (help == null || help.IsDisposed)
                help = new Help();
            help.url = url;

            if (!help.Visible)
                help.Show(owner ?? this);

            var xx = Location.X + 1105 + 8 - 271;//271, 213
            var yy = Location.Y + 49 + 208 - 213;// 8, 208 1105, 49
            help.Height = this.Height;
            help.Location = new Point(xx, yy);
            help.Location = new Point(Location.X + Width, Location.Y);
        }

        private void ShowMon(Monster mon, Stats cur = null) {
            displayMon = mon;
            if (mon != null) {
                cur = cur ?? mon.GetStats();

                statName.Text = mon.FullName;
                statID.Text = mon.Id.ToString();
                statLevel.Text = mon.level.ToString();

                ShowStats(cur, mon);
                ShowLoadout(mon.Current);

                var fname = Environment.CurrentDirectory.Replace("\\", "/") + "/data/unit/" + Program.GetMonIconName(mon.monsterTypeId) + ".png";
                if (File.Exists(fname))
                    monImage.ImageLocation = fname;
                else
                    monImage.Image = RuneApp.InternalServer.InternalServer.mon_spot;
            }
            else {
                statName.Text = "";
                statID.Text = "";
                statLevel.Text = "";
                ShowStats(null, null);
                ShowLoadout(null);
                monImage.Image = RuneApp.InternalServer.InternalServer.mon_spot;

            }
        }

        private void ShowLoadout(Loadout l) {
            runeDial.Loadout = l;

            if (runeDisplay != null && !runeDisplay.IsDisposed)
                runeDisplay.UpdateLoad(l);
        }

        private void ShowStats(Stats cur, Monster mon) {
            foreach (Attr a in new Attr[] { Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy }) {
                if (statCtrls[(int)a] == null)
                    statCtrls[(int)a] = groupBox1.Controls.Find(a.ToShortForm() + "Base", false).FirstOrDefault();

                if (mon == null)
                    statCtrls[(int)a].Text = "";
                else
                    statCtrls[(int)a].Text = mon[a] + (((int)a) > 8 ? "%" : "");

                if (statCtrls[12 + (int)a] == null)
                    statCtrls[12 + (int)a] = groupBox1.Controls.Find(a.ToShortForm() + "Total", false).FirstOrDefault();

                if (cur == null)
                    statCtrls[12 + (int)a].Text = "";
                else
                    statCtrls[12 + (int)a].Text = cur[a] + (((int)a) > 8 ? "%" : "");

                if (statCtrls[24 + (int)a] == null)
                    statCtrls[24 + (int)a] = groupBox1.Controls.Find(a.ToShortForm() + "Bonus", false).FirstOrDefault();

                if (cur == null || mon == null)
                    statCtrls[24 + (int)a].Text = "";
                else if (a != Attr.Speed)
                    statCtrls[24 + (int)a].Text = "+" + (cur[a] - mon[a]);
                else
                    statCtrls[24 + (int)a].Text = "+" + (cur[a] - mon[a]) + " (" + mon.GameSpeedBonus + ")";
            }
        }

        private void ShowDiff(Stats old, Stats load, Build build = null) {
            foreach (Attr stat in new Attr[] { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed }) {
                string sstat = stat.ToShortForm();
                if (old != null && load != null) {
                    groupBox1.Controls.Find(sstat + "compBefore", false).FirstOrDefault().Text = old[stat].ToString();
                    groupBox1.Controls.Find(sstat + "compAfter", false).FirstOrDefault().Text = load[stat].ToString();
                    string pts = (load[stat] - old[stat]).ToString();
                    if (build != null && !build.Sort[stat].EqualTo(0)) {
                        var left = build.ScoreStat(load, stat);
                        var right = build.ScoreStat(old, stat);
                        pts += " (" + (left - right).ToString("0.##") + ")";
                    }
                    groupBox1.Controls.Find(sstat + "compDiff", false).FirstOrDefault().Text = pts;
                }
                else {
                    groupBox1.Controls.Find(sstat + "compBefore", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(sstat + "compAfter", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(sstat + "compDiff", false).FirstOrDefault().Text = "";
                }
            }

            foreach (Attr stat in new Attr[] { Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy }) {
                string sstat = stat.ToShortForm();
                if (old != null && load != null) {
                    groupBox1.Controls.Find(sstat + "compBefore", false).FirstOrDefault().Text = old[stat].ToString() + "%";
                    groupBox1.Controls.Find(sstat + "compAfter", false).FirstOrDefault().Text = load[stat].ToString() + "%";
                    var before = old[stat];
                    var after = load[stat];
                    if (stat != Attr.CritDamage) {
                        before = Math.Min(100, before);
                        after = Math.Min(100, after);
                    }
                    string pts = (after - before).ToString();
                    if (build != null && !build.Sort[stat].EqualTo(0)) {
                        var left = build.ScoreStat(load, stat);
                        var right = build.ScoreStat(old, stat);
                        pts += " (" + (left - right).ToString("0.##") + ")";
                    }
                    groupBox1.Controls.Find(sstat + "compDiff", false).FirstOrDefault().Text = pts;
                }
                else {
                    groupBox1.Controls.Find(sstat + "compBefore", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(sstat + "compAfter", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(sstat + "compDiff", false).FirstOrDefault().Text = "";
                }
            }

            foreach (Attr extra in Build.ExtraEnums) {
                string sstat = extra.ToShortForm();
                if (old != null && load != null) {
                    groupBox1.Controls.Find(sstat + "compBefore", false).FirstOrDefault().Text = old.ExtraValue(extra).ToString("0");
                    groupBox1.Controls.Find(sstat + "compAfter", false).FirstOrDefault().Text = load.ExtraValue(extra).ToString("0");
                    string pts = (load.ExtraValue(extra) - old.ExtraValue(extra)).ToString("0");
                    if (build != null && !build.Sort.ExtraGet(extra).EqualTo(0)) {
                        var aa = load.ExtraValue(extra);
                        var cc = old.ExtraValue(extra);
                        Console.WriteLine("" + aa + ", " + cc);
                        var ss = build.Sort.ExtraGet(extra);

                        var left = build.ScoreExtra(load, extra);
                        var right = build.ScoreExtra(old, extra);

                        pts += " (" + (left - right).ToString("0.##") + ")";
                    }

                    groupBox1.Controls.Find(sstat + "compDiff", false).FirstOrDefault().Text = pts;
                }
                else {
                    groupBox1.Controls.Find(sstat + "compBefore", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(sstat + "compAfter", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(sstat + "compDiff", false).FirstOrDefault().Text = "";
                }

            }

            for (int i = 0; i < 4; i++) {
                var stat = "Skill" + (i + 1);
                if (old != null && load != null) {
                    groupBox1.Controls.Find(stat + "compBefore", false).FirstOrDefault().Text = old.GetSkillDamage(Attr.AverageDamage, i).ToString("0.##");
                    groupBox1.Controls.Find(stat + "compAfter", false).FirstOrDefault().Text = load.GetSkillDamage(Attr.AverageDamage, i).ToString("0.##");
                    string pts = (load.GetSkillDamage(Attr.AverageDamage, i) - old.GetSkillDamage(Attr.AverageDamage, i)).ToString("0.##");
                    if (build != null && !build.Sort.DamageSkillups[i].EqualTo(0)) {

                        var left = build.ScoreSkill(load, i);
                        var right = build.ScoreSkill(old, i);

                        pts += " (" + (left - right).ToString("0.##") + ")";
                    }
                    groupBox1.Controls.Find(stat + "compDiff", false).FirstOrDefault().Text = pts;
                }
                else {
                    groupBox1.Controls.Find(stat + "compBefore", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(stat + "compAfter", false).FirstOrDefault().Text = "";
                    groupBox1.Controls.Find(stat + "compDiff", false).FirstOrDefault().Text = "";
                }
            }
        }

        public void RebuildLists() {
            // TODO: comment it up a little?

            var oldMonSort = dataMonsterList.ListViewItemSorter;
            dataMonsterList.ListViewItemSorter = null;
            var oldRuneSort = dataRuneList.ListViewItemSorter;
            dataRuneList.ListViewItemSorter = null;

            dataMonsterList.Items.Clear();
            dataRuneList.Items.Clear();
            listView4.Items.Clear();
            if (Program.data == null)
                return;
            int maxPri = 0;
            if (Program.builds.Count > 0)
                maxPri = Program.builds.Max(b => b.Priority) + 1;
            foreach (var mon in Program.data.Monsters) {
                mon.priority = (Program.builds?.FirstOrDefault(b => b.MonId == mon.Id)?.Priority) ?? (mon.Current?.RuneCount > 0 ? (maxPri++) : 0);
            }
            dataMonsterList.Items.AddRange(Program.data.Monsters.Select(mon => ListViewItemMonster(mon)).ToArray());

            dataCraftList.Items.AddRange(Program.data.Crafts.Select(craft => new ListViewItem() {
                Text = craft.ItemId.ToString(),
                SubItems =
                {
                    craft.Set.ToString(),
                    craft.Stat.ToString(),
                    craft.Rarity.ToString(),
                    craft.Type.ToString(),
                }
            }).ToArray());

            foreach (Rune rune in Program.data.Runes) {
                dataRuneList.Items.Add(ListViewItemRune(rune));
            }
            checkLocked();
            ColorMonsWithBuilds();

            dataMonsterList.ListViewItemSorter = oldMonSort;
            if (dataMonsterList.ListViewItemSorter != null) {
                var mlvs = (ListViewSort)dataMonsterList.ListViewItemSorter;
                mlvs.ShouldSort = true;
                mlvs.OrderBy(colMonGrade.Index, false);
                mlvs.ThenBy(colMonPriority.Index, true);
                dataMonsterList.Sort();
            }
            dataRuneList.ListViewItemSorter = oldRuneSort;
            if (dataRuneList.ListViewItemSorter != null) {
                ((ListViewSort)dataRuneList.ListViewItemSorter).ShouldSort = true;
                dataRuneList.Sort();
            }
        }
        public void refreshLoadouts()
        {
            foreach (ListViewItem item in loadoutList.Items)
            {
                Loadout load = (Loadout)item.Tag;

                var monid = ulong.Parse(item.SubItems[2].Text);

                load.RecountDiff(monid);
                ListViewItemLoad(item, load);
            }
        }

        public void checkLocked() {
            if (Program.data?.Runes == null)
                return;

            Invoke((MethodInvoker)delegate {
                toolStripStatusLabel1.Text = "Locked: " + Program.data.Runes.Count(r => r.Locked);
                foreach (ListViewItem li in dataRuneList.Items) {
                    if (li.Tag is Rune rune)
                        li.BackColor = rune.Locked ? Color.Red : Color.Transparent;
                }
            });
        }

        private void startResumeTimer(DateTime when) {
            resumeTime = when;
            resumeTimer = new System.Windows.Forms.Timer() {
                Interval = 1000,
            };
            resumeTimer.Tick += ResumeTimer_Tick;
            resumeTimer.Start();

        }


        private void stopResumeTimer() {
            if (resumeTimer != null) {
                resumeTimer.Stop();
            }
            resumeTimer = null;

            var fb = Program.builds.FirstOrDefault(b => b.Best == null);
            var lvi = this.buildList.Items.OfType<ListViewItem>().FirstOrDefault(b => b.Tag == fb);
            if (lvi != null) {
                // TODO: rename build columns
                lvi.SubItems[3].Text = "";
            }
        }
    }
}
