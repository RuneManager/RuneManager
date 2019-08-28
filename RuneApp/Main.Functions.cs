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
using RuneOptim.BuidProcessing;
using RuneOptim.swar;
using RuneOptim.Management;

namespace RuneApp {
	public partial class Main {

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

		private void ListViewItemBuild(ListViewItem lvi, Build b) {
			lvi.Text = b.ID.ToString();

			while (lvi.SubItems.Count < 6)
				lvi.SubItems.Add("");

			int i = 0;
			lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, b.Mon?.FullName ?? b.MonName);
			lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, b.Priority.ToString());
			lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, b.ID.ToString());
			lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, "");
			lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, (b.Mon?.Id ?? b.MonId).ToString());
			if (b.Type == BuildType.Lock) {
				lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, "Locked");
				lvi.ForeColor = Color.Gray;
			}
			else if (b.Type == BuildType.Link) {
				lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, "Linked");
				lvi.ForeColor = Color.Teal;
			}
			else
				lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, getTeamStr(b));

			lvi.Tag = b;

			if (b.RunePrediction.Any(p => p.Value.Value))
				lvi.ForeColor = Color.Purple;
		}

		private string getTeamStr(Build b) {
			if (b.Teams == null || b.Teams.Count == 0)
				return "";

			var sz = buildCHTeams.Width;
			var str = "";
			for (int i = 0; i < b.Teams.Count; i++) {
				var sb = new StringBuilder(string.Join(", ", b.Teams.Take(i)));
				if (!string.IsNullOrWhiteSpace(sb.ToString()))
					sb.Append(", ");
				sb.Append(b.Teams.Count - i);
				var tstr = string.Join(", ", b.Teams.Take(i + 1));
				if (this.CreateGraphics().MeasureString(tstr + "...", buildList.Font).Width > sz - 10)
					return sb.ToString();
				str = tstr;
			}

			return str;
		}

		private void tsTeamAdd(ToolStripMenuItem parent, string item) {
			knownTeams.Add(item);
			ToolStripMenuItem n = new ToolStripMenuItem(item);
			parent.DropDownItems.Add(n);
			n.CheckedChanged += tsTeamHandler;
			n.CheckOnClick = true;

			if (toolmap[item] != null) {
				foreach (var smi in toolmap[item]) {
					if (toolmap.ContainsKey(smi)) {
						tsTeamAdd(n, smi);
					}
					else {
						ToolStripMenuItem s = new ToolStripMenuItem(smi);
						s.CheckedChanged += tsTeamHandler;
						s.CheckOnClick = true;
						n.DropDownItems.Add(s);
					}
				}
			}
		}

		private void filterRunesList(Predicate<object> p) {
			dataRuneList.Items.Clear();

			if (Program.data?.Runes == null) return;

			foreach (Rune rune in Program.data.Runes.Where(p.Invoke)) {
				dataRuneList.Items.Add(ListViewItemRune(rune));
			}
		}

		public void ProgressToList(Build b, string str) {
			//Program.log.Info("_" + str);
			this.Invoke((MethodInvoker)delegate {
				if (!IsDisposed) {
					var lvi = buildList.Items.OfType<ListViewItem>().FirstOrDefault(ll => (ll.Tag as Build)?.ID == b.ID);
					if (lvi == null)
						return;
					while (lvi.SubItems.Count < 4)
						lvi.SubItems.Add("");
					lvi.SubItems[3].Text = str;
				}
			});
		}

		private DialogResult CheckSaveChanges() {
			if (!Program.data.isModified)
				return DialogResult.Yes;

			var res = MessageBox.Show("Would you like to save changes to your imported data?", "Save Data", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (res == DialogResult.Yes)
				Program.SaveData();
			return res;
		}

		private void RegenBuildList() {
			foreach (var lvi in buildList.Items.OfType<ListViewItem>()) {
				if (lvi.Tag is Build b) {
					lvi.SubItems[1].Text = b.Priority.ToString();
				}
			}
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

		private void ShowMon(Monster mon) {
			displayMon = mon;
			var cur = mon.GetStats();

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

		private void ShowLoadout(Loadout l) {
			runeDial.Loadout = l;

			if (runeDisplay != null && !runeDisplay.IsDisposed)
				runeDisplay.UpdateLoad(l);
		}

		private void ShowStats(Stats cur, Stats mon) {
			foreach (Attr a in new Attr[] { Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy }) {
				if (statCtrls[(int)a] == null)
					statCtrls[(int)a] = groupBox1.Controls.Find(a.ToShortForm() + "Base", false).FirstOrDefault();
				statCtrls[(int)a].Text = mon[a] + (((int)a) > 8 ? "%" : "");

				if (statCtrls[12 + (int)a] == null)
					statCtrls[12 + (int)a] = groupBox1.Controls.Find(a.ToShortForm() + "Total", false).FirstOrDefault();
				statCtrls[12 + (int)a].Text = cur[a] + (((int)a) > 8 ? "%" : "");

				if (statCtrls[24 + (int)a] == null)
					statCtrls[24 + (int)a] = groupBox1.Controls.Find(a.ToShortForm() + "Bonus", false).FirstOrDefault();
				statCtrls[24 + (int)a].Text = "+" + (cur[a] - mon[a]);
			}
		}

		private void ShowDiff(Stats old, Stats load, Build build = null) {
			foreach (Attr stat in new Attr[] { Attr.HealthPercent, Attr.AttackPercent, Attr.DefensePercent, Attr.Speed }) {
				string sstat = stat.ToShortForm();
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

			foreach (Attr stat in new Attr[] { Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy }) {
				string sstat = stat.ToShortForm();
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

			foreach (Attr extra in Build.ExtraEnums) {
				string sstat = extra.ToShortForm();
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

			for (int i = 0; i < 4; i++) {
				var stat = "Skill" + (i + 1);
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

		public void RebuildBuildList() {
			List<ListViewItem> tempMons = null;
			this.Invoke((MethodInvoker)delegate {
				tempMons = dataMonsterList.Items.OfType<ListViewItem>().ToList();
				buildList.Items.Clear();
			});

			var lviList = new List<ListViewItem>();

			foreach (var b in Program.builds) {
				ListViewItem li = new ListViewItem();
				this.Invoke((MethodInvoker)delegate {
					ListViewItemBuild(li, b);
				});
				lviList.Add(li);
				var lv1li = tempMons.FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == (b.Mon?.Id ?? b.MonId).ToString()));
				if (lv1li != null) {
					lv1li.ForeColor = Color.Green;
				}
			}

			this.Invoke((MethodInvoker)delegate {
				buildList.Items.AddRange(lviList.ToArray());
				buildList.Sort();
			});
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

		private void RunBuild(ListViewItem pli, bool saveStats = false) {
			if (pli?.Tag is Build)
				Program.RunBuild((Build)pli.Tag, saveStats);
		}

		private void ClearLoadouts() {
			Program.ClearLoadouts();
			checkLocked();
		}

		private bool tsTeamCheck(ToolStripMenuItem t) {
			bool ret = false;
			t.Checked = false;
			t.Image = null;
			if (teamBuild.Teams.Contains(t.Text)) {
				t.Checked = true;
				ret = true;
			}
			foreach (ToolStripMenuItem smi in t.DropDownItems) {
				if (tsTeamCheck(smi)) {
					t.Image = App.add;
					ret = true;
				}
			}
			return ret;
		}

		int GetRel(string first, string second) {
			if (first == second)
				return 0;

			string p1 = null;
			string p2 = null;

			if (toolmap.Keys.Contains(first) && toolmap.Keys.Contains(second))
				return 1;

			foreach (var k in toolmap) {
				if (k.Value.Contains(first) && k.Value.Contains(second))
					return 1;
				if (k.Value.Contains(first))
					p1 = k.Key;
				if (k.Value.Contains(second))
					p2 = k.Key;
			}

			if (toolmap.Keys.Contains(first) && toolmap[first].Contains(second))
				return 1;
			if (toolmap.Keys.Contains(second) && toolmap[second].Contains(first))
				return 1;

			if (p2 != null && toolmap[p2].Contains(p1))
				return 2;
			if (p1 != null && toolmap[p1].Contains(p2))
				return 2;

			if (p2 != null && toolmap[p2].Contains(first))
				return 3;
			if (p1 != null && toolmap[p1].Contains(second))
				return 3;

			return -1;
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
