	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Windows.Forms;
	using System.IO;
	using Newtonsoft.Json;
	using RuneOptim;
	using System.Threading;
	using System.Diagnostics;
	using System.Net;
	using System.Reflection;
	using System.Configuration;

namespace RuneApp
{
	public partial class Main : Form
	{
		string filelink = "";
		string whatsNewText = "";
		Build currentBuild = null;

		private Dictionary<string, List<ToolStripMenuItem>> shrineMap = new Dictionary<string, List<ToolStripMenuItem>>();

		private Task runTask = null;
		private CancellationToken runToken;
		private CancellationTokenSource runSource = null;
		bool plsDie = false;
		bool isRunning = false;
		public static Help help = null;

		public static bool goodRunes = false;

		public static Main currentMain = null;
		public static RuneDial runeDial = null;
		Monster displayMon = null;

		bool teamChecking = false;
		Build teamBuild = null;
		Dictionary<string, List<string>> toolmap = null;
		List<string> knownTeams = new List<string>();
		List<string> extraTeams = new List<string>();

		public static Main Instance = null;

		//public static Configuration config {  get { return Program.config; } }
		public static log4net.ILog Log { get { return Program.log; } }

		public Main()
		{
			InitializeComponent();
			Log.Info("Initialized Main");
			Instance = this;

			currentMain = this;

			useRunesCheck.Checked = Program.Settings.UseEquipped;

			findGoodRunes.Enabled = Program.Settings.MakeStats;
			if (!Program.Settings.MakeStats)
				findGoodRunes.Checked = false;

			runes = new RuneControl[] { runeControl1, runeControl2, runeControl3, runeControl4, runeControl5, runeControl6 };

			#region Sorter
			var sorter = new ListViewSort();
			dataMonsterList.ListViewItemSorter = sorter;
			sorter.OnColumnClick(ColMonID.Index);
			sorter.OnColumnClick(ColMonPriority.Index);
			dataRuneList.ListViewItemSorter = new ListViewSort();

			sorter = new ListViewSort();
			sorter.OnColumnClick(0);
			buildList.ListViewItemSorter = sorter;
			#endregion

			#region Update

			if (Program.Settings.CheckUpdates)
			{
				Task.Factory.StartNew(() =>
				{
					using (WebClient client = new WebClient())
					{
						Log.Info("Checking for updates");
						client.DownloadStringCompleted += client_DownloadStringCompleted;
						client.DownloadStringAsync(new Uri("https://raw.github.com/Skibisky/RuneManager/master/version.txt"));
					}
				});
			}
			else
			{
				updateBox.Show();
				Log.Info("Updates Disabled");
				updateComplain.Text = "Updates Disabled";
				var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				string oldvernum = ver.ProductVersion;
				updateCurrent.Text = "Current: " + oldvernum;
				updateNew.Text = "";
			}

			#endregion

			#region Labels

			Label l = new Label();
			l.Location = new Point(4 + 50, 400 - 18);
			l.Name = "compBefore";
			l.Text = "Before";
			l.Size = new Size(50, 14);
			groupBox1.Controls.Add(l);

			l = new Label();
			l.Location = new Point(4 + 100, 400 - 18);
			l.Name = "compAfter";
			l.Text = "After";
			l.Size = new Size(50, 14);
			groupBox1.Controls.Add(l);

			l = new Label();
			l.Location = new Point(4 + 150, 400 - 18);
			l.Name = "compDiff";
			l.Text = "Difference";
			l.Size = new Size(60, 14);
			groupBox1.Controls.Add(l);

			int xx = 0;
			int yy = 0;
			var labelPrefixes = new string[] { "Pts" }.Concat(Build.statNames).Concat(Build.extraNames);
			foreach (var s in labelPrefixes)
			{
				groupBox1.Controls.MakeControl<Label>(s, "compStat", 4 + xx, 400 + yy, 50, 14, s);
				xx += 50;

				groupBox1.Controls.MakeControl<Label>(s, "compBefore", 4 + xx, 400 + yy, 50, 14, "");
				xx += 50;

				groupBox1.Controls.MakeControl<Label>(s, "compAfter", 4 + xx, 400 + yy, 50, 14, "");
				xx += 50;

				groupBox1.Controls.MakeControl<Label>(s, "compDiff", 4 + xx, 400 + yy, 50, 14, "");
				xx += 50;

				if (s == "SPD")
					yy += 4;
				if (s == "ACC")
					yy += 8;

				yy += 16;
				xx = 0;
			}

			#endregion
		}

		private void AddShrine(string stat, int num, int value, ToolStripMenuItem owner)
		{
			ToolStripMenuItem it = new ToolStripMenuItem(num.ToString() + (num > 0 ? " (" + value + "%)" : ""));
			it.Tag = new KeyValuePair<string, int>(stat, value);
			it.Click += ShrineClick;
			owner.DropDownItems.Add(it);
			if (!shrineMap.ContainsKey(stat))
				shrineMap.Add(stat, new List<ToolStripMenuItem>());
			shrineMap[stat].Add(it);
			if (value == Program.data.shrines[stat])
				it.Checked = true;
		}

		private void ShrineClick(object sender, EventArgs e)
		{
			var it = (ToolStripMenuItem)sender;
			if (it != null)
			{
				var tag = (KeyValuePair<string, int>)it.Tag;
				var stat = tag.Key;

				foreach (ToolStripMenuItem i in shrineMap[stat])
				{
					i.Checked = false;
				}
				it.Checked = true;
				if (Program.data == null)
					return;

				if (string.IsNullOrWhiteSpace(stat))
					return;

				Program.data.shrines[stat] = tag.Value;

				Program.Settings["shrine" + stat] = tag.Value;
				Program.Settings.Save();
			}
		}

		private void Main_Load(object sender, EventArgs e)
		{
			#region Watch collections and try loading
			Program.builds.CollectionChanged += Builds_CollectionChanged;
			Program.loads.CollectionChanged += Loads_CollectionChanged;
			Program.BuildsProgressTo += Program_BuildsProgressTo;

			try {
				LoadSaveResult loadResult = 0;
				while ((loadResult = Program.FindSave()) != LoadSaveResult.Success)
				{
					if (loadResult == LoadSaveResult.FileNotFound && MessageBox.Show("Couldn't automatically load save.\r\nManually locate a save file?", "Load Save", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						loadSaveDialogue(null, new EventArgs());
					}
				}
				loadResult = Program.LoadMonStats();
				if (loadResult != LoadSaveResult.Success)
				{
					// TODO: pulling from SWarFarm API might be sad
					MessageBox.Show("Error loading base monsters stats.\r\nThings could go bad.", "Base Monster Stats");
				}
				loadResult = 0;
				while ((loadResult = Program.LoadBuilds()) != LoadSaveResult.Success)
				{
					if (loadResult == LoadSaveResult.Failure && MessageBox.Show("Save was invalid while loading builds.\r\nManually locate a save file?", "Load Builds", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						loadSaveDialogue(null, new EventArgs());
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Critical Error {ex.GetType()}\r\nDetails in log file.", "Error");
				Log.Fatal($"Fatal during load {ex.GetType()}", ex);
			}
			#endregion

			RegenLists();

			#region Shrines

			ToolStripMenuItem[] shrineMenu = new ToolStripMenuItem[] { speedToolStripMenuItem, defenseToolStripMenuItem , attackToolStripMenuItem, healthToolStripMenuItem,
			waterAttackToolStripMenuItem, fireAttackToolStripMenuItem, windAttackToolStripMenuItem, lightAttackToolStripMenuItem, darkAttackToolStripMenuItem, criticalDamageToolStripMenuItem};
			for (int i = 0; i < 11; i++)
			{
				for (int j = 0; j < Deco.ShrineStats.Length; j++)
				{
					if (j < 4)
						AddShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(i * Deco.ShrineLevel[j]), shrineMenu[j]);
					else if (j < 9)
						AddShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(1 + i * Deco.ShrineLevel[j]), shrineMenu[j]);
					else
						AddShrine(Deco.ShrineStats[j], i, (int)Math.Floor(i * Deco.ShrineLevel[j]), shrineMenu[j]);
				}
			}

			#endregion

			buildList.SelectedIndexChanged += buildList_SelectedIndexChanged;


			foreach (ToolStripItem ii in menu_buildlist.Items)
			{
				if (ii.Text == "Team")
				{
					/*{ "Farmer", "Dungeon", "Giant", "Dragon", "Necro", "Secret",
						"Elemental", "Magic", "Light D", "Dark D", "Fire D", "Water D", "Wind D",
						"ToA", "ToAN", "ToAH", "Raid", "Normal", "Light R", "Dark R", "Fire R", "Water R", "Wind R",
						"PvP", "AO", "AD", "GWO", "GWD", "World Boss"}*/

					toolmap = new Dictionary<string, List<string>>()
					{
						{ "PvE", new List<string> { "Farmer", "World Boss", "ToA" } },
						{ "Dungeon", new List<string> { "Giant", "Dragon", "Necro", "Secret", "HoH", "Elemental" } },
						{ "Raid", new List<string> {"Group", "Light R", "Dark R", "Fire R", "Water R", "Wind R" } },
						{ "PvP", new List<string> { "AO", "AD", "GWO", "GWD" } },

						{ "Elemental", new List<string> {"Magic", "Light D", "Dark D", "Fire D", "Water D", "Wind D" } },
						{ "ToA", new List<string> { "ToAN", "ToAH" } }
					};

					ToolStripMenuItem tsmi = ii as ToolStripMenuItem;

					tsTeamAdd(tsmi, "PvE");
					tsTeamAdd(tsmi, "Dungeon");
					tsTeamAdd(tsmi, "Raid");
					tsTeamAdd(tsmi, "PvP");

					var tsnone = new ToolStripMenuItem("(Clear)");
					tsnone.Font = new Font(tsnone.Font, FontStyle.Italic);
					tsnone.Click += tsTeamHandler;

					tsmi.DropDownItems.Add(tsnone);
				}
			}

			if (Program.Settings.StartUpHelp)
				OpenHelp();
		}

		private void Program_BuildsProgressTo(object sender, PrintToEventArgs e)
		{
			ProgressToList(e.build, e.Message);
		}

		private void Loads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					foreach (var l in e.NewItems.Cast<Loadout>())
					{
						Invoke((MethodInvoker)delegate
						{
							checkLocked();
							Build b = Program.builds.FirstOrDefault(bu => bu.ID == l.BuildID);

							ListViewItem nli;

							var lvs = loadoutList.Items.Find(b.ID.ToString(), false);

							if (lvs.Length == 0)
								nli = new ListViewItem();
							else
								nli = lvs.First();

							nli.Tag = l;
							nli.Text = b.ID.ToString();
							nli.Name = b.ID.ToString();
							while (nli.SubItems.Count < 6)
								nli.SubItems.Add("");
							nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, b.ID.ToString());
							nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, b.Best.Name);
							nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, b.Best.ID.ToString());
							nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (l.runesNew + l.runesChanged).ToString());
							if (l.runesNew > 0 && b.mon.Current.RuneCount < 6)
								nli.SubItems[3].ForeColor = Color.Green;
							if (Program.Settings.SplitAssign)
								nli.SubItems[3].Text = l.runesNew.ToString() + "/" + l.runesChanged.ToString();
							nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, b.Best.Current.powerup.ToString());
							nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, (b.Time / (double)1000).ToString("0.##"));
							nli.UseItemStyleForSubItems = false;
							if (b.Time / (double)1000 > 60)
								nli.SubItems[5].BackColor = Color.Red;
							else if (b.Time / (double)1000 > 30)
								nli.SubItems[5].BackColor = Color.Orange;

							if (lvs.Length == 0)
								loadoutList.Items.Add(nli);
						});
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					throw new NotImplementedException();
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException();
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
					throw new NotImplementedException();
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					loadoutList.Items.Clear();
					break;
				//throw new NotImplementedException();
				default:
					throw new NotImplementedException();
			}
		}

		private void Builds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					List<ListViewItem> tempMons = null;
					this.Invoke((MethodInvoker)delegate { tempMons = dataMonsterList.Items.Cast<ListViewItem>().ToList(); });

					foreach (var b in e.NewItems.Cast<Build>())
					{
						var teamstr = (b.Teams == null || b.Teams.Count == 0) ? "" : (b.Teams.Count > 2 ? b.Teams.Count.ToString() : b.Teams[0] + (b.Teams.Count == 2 ? ", " + b.Teams[1] : ""));
						ListViewItem li = new ListViewItem(new string[] { b.priority.ToString(), b.ID.ToString(), b.mon.Name, "", b.mon.ID.ToString(), teamstr });
						li.Tag = b;
						this.Invoke((MethodInvoker)delegate { buildList.Items.Add(li); });
						var lv1li = tempMons.Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == b.mon.ID.ToString()).Count() > 0).FirstOrDefault();
						if (lv1li != null)
						{
							lv1li.ForeColor = Color.Green;
						}
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					throw new NotImplementedException();
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					throw new NotImplementedException();
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
					throw new NotImplementedException();
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					throw new NotImplementedException();
				default:
					throw new NotImplementedException();
			}
		}

		private void tsTeamAdd(ToolStripMenuItem parent, string item)
		{
			knownTeams.Add(item);
			ToolStripMenuItem n = new ToolStripMenuItem(item);
			parent.DropDownItems.Add(n);
			n.CheckedChanged += tsTeamHandler;
			n.CheckOnClick = true;

			if (toolmap[item] != null)
			{
				foreach (var smi in toolmap[item])
				{
					if (toolmap.ContainsKey(smi))
					{
						tsTeamAdd(n, smi);
					}
					else
					{
						ToolStripMenuItem s = new ToolStripMenuItem(smi);
						s.CheckedChanged += tsTeamHandler;
						s.CheckOnClick = true;
						n.DropDownItems.Add(s);
					}
				}
			}
		}

		private void tsTeamHandler(object sender, EventArgs e)
		{
			if (teamChecking || teamBuild == null)
				return;

			ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
			if (tsmi.Text == "(Clear)")
			{
				teamBuild.Teams.Clear();
			}
			else
			{
				if (tsmi.Checked)
				{
					if (!teamBuild.Teams.Contains(tsmi.Text))
						teamBuild.Teams.Add(tsmi.Text);
				}
				else
				{
					teamBuild.Teams.Remove(tsmi.Text);
				}
			}
			var teamstr = (teamBuild.Teams == null || teamBuild.Teams.Count == 0) ? "" : (teamBuild.Teams.Count > 2 ? teamBuild.Teams.Count.ToString() : teamBuild.Teams[0] + (teamBuild.Teams.Count == 2 ? ", " + teamBuild.Teams[1] : ""));
			var bli = buildList.Items.Cast<ListViewItem>().Where(it => it.Tag == teamBuild).FirstOrDefault();
			if (bli != null)
			{
				while (bli.SubItems.Count < 6) bli.SubItems.Add("");
				bli.SubItems[5].Text = teamstr;
			}
			menu_buildlist.Close();
		}

		private void monstertab_list_select(object sender, EventArgs e)
		{
			if (dataMonsterList.FocusedItem != null)
			{
				var item = dataMonsterList.FocusedItem;
				if (item.Tag != null)
				{
					Monster mon = (Monster)item.Tag;

					ShowMon(mon);
				}
			}
		}

		private void loadSaveDialogue(object sender, EventArgs e)
		{
			LoadSaveResult size;
			OpenFileDialog openFileDialog1 = new OpenFileDialog();
			openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();
			DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
			if (result == DialogResult.OK) // Test result.
			{
				string file = openFileDialog1.FileName;
				if (file != null)
				{
					try
					{
						size = Program.LoadSave(file);
						if (size > 0)
						{
							if (File.Exists("save.json"))
							{
								if (MessageBox.Show("Do you want to override the existing startup save?", "Load Save", MessageBoxButtons.YesNo) == DialogResult.Yes)
								{
									File.Copy(file, "save.json", true);
								}
							}
							else
							{
								if (MessageBox.Show("Do you want to load this save on startup?", "Load Save", MessageBoxButtons.YesNo) == DialogResult.Yes)
								{
									File.Copy(file, "save.json");
								}
							}
						}
					}
					catch (IOException ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// confirm maybe?
			Close();
		}

		private void rune_Click(object sender, EventArgs e)
		{
			foreach (RuneControl t in runes)
			{
				t.Gamma = 1;
				t.Refresh();
			}

			RuneControl tc = ((RuneControl)sender);
			if (tc.Tag != null)
			{
				tc.Gamma = 1.4f;
				tc.Refresh();
				runeEquipped.Show();
				runeEquipped.SetRune((Rune)tc.Tag);
				lbCloseEquipped.Show();
			}
			else
			{
				tc.Hide();
				runeEquipped.Hide();
				lbCloseEquipped.Hide();
			}
		}

		private void lbCloseEquipped_Click(object sender, EventArgs e)
		{
			lbCloseEquipped.Hide();
			runeEquipped.Hide();
			foreach (RuneControl r in runes)
			{
				r.Gamma = 1;
				r.Refresh();
			}
		}

		private void lbCloseInventory_Click(object sender, EventArgs e)
		{
			lbCloseInventory.Hide();
			runeInventory.Hide();
		}

		private void runetab_list_select(object sender, EventArgs e)
		{
			if (dataRuneList.FocusedItem != null)
			{
				var item = dataRuneList.FocusedItem;
				if (item.Tag != null)
				{
					Rune rune = (Rune)item.Tag;

					lbCloseInventory.Show();
					runeInventory.Show();
					runeInventory.SetRune(rune);
				}
			}

		}

		private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
			sorter.OnColumnClick(e.Column);
			((ListView)sender).Sort();
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			if (dataMonsterList.FocusedItem != null)
			{
				if (dataMonsterList.FocusedItem.Tag != null)
				{
					Monster mon = (Monster)dataMonsterList.FocusedItem.Tag;
					int maxPri = Program.data.Monsters.Max(x => x.priority);
					if (mon.priority == 0)
					{
						mon.priority = maxPri + 1;
						dataMonsterList.FocusedItem.SubItems[ColMonPriority.Index].Text = (maxPri + 1).ToString();
					}
					else if (mon.priority != 1)
					{
						int pri = mon.priority;
						Monster mon2 = Program.data.Monsters.Where(x => x.priority == pri - 1).FirstOrDefault();
						if (mon2 != null)
						{
							ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.Name);
							mon2.priority += 1;
							listMon.SubItems[ColMonPriority.Index].Text = mon2.priority.ToString();
						}
						mon.priority -= 1;
						dataMonsterList.FocusedItem.SubItems[ColMonPriority.Index].Text = (mon.priority).ToString();
					}
					dataMonsterList.Sort();
				}
			}
		}

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			if (dataMonsterList.FocusedItem != null)
			{
				if (dataMonsterList.FocusedItem.Tag != null)
				{
					Monster mon = (Monster)dataMonsterList.FocusedItem.Tag;
					int maxPri = Program.data.Monsters.Max(x => x.priority);
					if (mon.priority == 0)
					{
						mon.priority = maxPri + 1;
						dataMonsterList.FocusedItem.SubItems[ColMonPriority.Index].Text = (maxPri + 1).ToString();
					}
					else if (mon.priority != maxPri)
					{
						int pri = mon.priority;
						Monster mon2 = Program.data.Monsters.Where(x => x.priority == pri + 1).FirstOrDefault();
						if (mon2 != null)
						{
							var items = dataMonsterList.Items;
							ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.Name);
							mon2.priority -= 1;
							listMon.SubItems[ColMonPriority.Index].Text = mon2.priority.ToString();
						}
						mon.priority += 1;
						dataMonsterList.FocusedItem.SubItems[ColMonPriority.Index].Text = (mon.priority).ToString();
					}
					dataMonsterList.Sort();
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			tabControl1.SelectTab(tabRunes.Name);
			filterRunesList(x => ((Rune)x).Slot == ((Rune)runeEquipped.Tag).Slot);
		}

		private void filterRunesList(Predicate<object> p)
		{
			dataRuneList.Items.Clear();

			if (Program.data?.Runes == null) return;

			foreach (Rune rune in Program.data.Runes.Where(p.Invoke))
			{
				ListViewItem item = new ListViewItem(new string[]{
					rune.Set.ToString(),
					rune.ID.ToString(),
					rune.Grade.ToString(),
					Rune.StringIt(rune.MainType, true),
					rune.MainValue.ToString()
				});
				item.Tag = rune;
				item.BackColor = rune.Locked ? Color.Red : Color.Transparent;
				dataRuneList.Items.Add(item);
			}
		}

		private void runetab_clearfilter(object sender, EventArgs e)
		{
			filterRunesList(x => true);
		}

		private void loadout_list_select(object sender, EventArgs e)
		{
			if (loadoutList.FocusedItem != null)
			{
				var item = loadoutList.FocusedItem;
				if (item.Tag != null)
				{
					Loadout load = (Loadout)item.Tag;

					var monid = int.Parse(item.SubItems[2].Text);
					var bid = int.Parse(item.SubItems[0].Text);

					var build = Program.builds.FirstOrDefault(b => b.ID == bid);

					Monster mon = null;
					if (build == null)
						mon = Program.data.GetMonster(monid);
					else
						mon = build.mon;

					ShowMon(mon);

					ShowStats(load.GetStats(mon), mon);

					ShowRunes(load.Runes);

					var dmon = Program.data.GetMonster(monid);
					if (dmon != null)
					{
						var dmonld = dmon.Current.Leader;
						var dmonsh = dmon.Current.Shrines;
						dmon.Current.Leader = load.Leader;
						dmon.Current.Shrines = load.Shrines;

						if (build != null)
						{
							var beforeScore = build.sort(dmon.GetStats());
							var afterScore = build.sort(load.GetStats(mon));
							groupBox1.Controls.Find("PtscompBefore", false).FirstOrDefault().Text = beforeScore.ToString();
							groupBox1.Controls.Find("PtscompAfter", false).FirstOrDefault().Text = afterScore.ToString();
							groupBox1.Controls.Find("PtscompDiff", false).FirstOrDefault().Text = (afterScore - beforeScore).ToString();
						}

						ShowDiff(dmon.GetStats(), load.GetStats(mon));

						dmon.Current.Leader = dmonld;
						dmon.Current.Shrines = dmonsh;
					}
					ShowSets(load);
				}
			}
			int cost = 0;
			foreach (ListViewItem li in loadoutList.SelectedItems)
			{
				Loadout load = li.Tag as Loadout;
				if (load != null)
				{
					var mon = Program.data.GetMonster(int.Parse(li.SubItems[2].Text));
					if (mon != null)
						cost += mon.SwapCost(load);
				}
			}
			toolStripStatusLabel2.Text = "Unequip: " + cost.ToString();
		}

		private void tsBtnLoadsClear_Click(object sender, EventArgs e)
		{
			ClearLoadouts();
		}

		private void toolStripButton7_Click(object sender, EventArgs e)
		{
			if (dataMonsterList.SelectedItems.Count <= 0) return;
			var mon = dataMonsterList.SelectedItems[0].Tag as Monster;
			if (mon == null)
				return;

			using (var ff = new Create())
			{
				var bb = new Build((Monster)dataMonsterList.SelectedItems[0].Tag)
				{
					New = true,
					ID = buildList.Items.Count + 1
				};
				while (Program.builds.Any(b => b.ID == bb.ID))
				{
					bb.ID++;
				}

				ff.build = bb;
				var res = ff.ShowDialog();
				if (res != DialogResult.OK) return;

				if (Program.builds.Count > 0)
					ff.build.priority = Program.builds.Max(b => b.priority) + 1;
				else
					ff.build.priority = 1;

				ListViewItem li = new ListViewItem(new string[] { ff.build.priority.ToString(), ff.build.ID.ToString(), ff.build.mon.Name, "", ff.build.mon.ID.ToString(), "" });
				li.Tag = ff.build;
				buildList.Items.Add(li);
				Program.builds.Add(ff.build);

				var lv1li = dataMonsterList.Items.Cast<ListViewItem>().FirstOrDefault(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Any(s => s.Text == ff.build.mon.Name));
				if (lv1li != null)
					lv1li.ForeColor = Color.Green;
			}
		}

		private void listView5_DoubleClick(object sender, EventArgs e)
		{
			var items = buildList.SelectedItems;
			if (items.Count > 0)
			{
				var item = items[0];
				if (item.Tag != null)
				{
					Build bb = (Build)item.Tag;
					Monster before = bb.mon;
					using (var ff = new Create())
					{
						ff.build = bb;
						var res = ff.ShowDialog();
						if (res == DialogResult.OK)
						{
							item.SubItems[2].Text = bb.mon.Name;
							item.SubItems[4].Text = bb.mon.ID.ToString();
							if (bb.mon != before)
							{
								var lv1li = dataMonsterList.Items.Cast<ListViewItem>().Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == before.Name).Count() > 0).FirstOrDefault();
								if (lv1li != null)
									lv1li.ForeColor = before.inStorage ? Color.Gray : Color.Black;

								lv1li = dataMonsterList.Items.Cast<ListViewItem>().Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == ff.build.mon.Name).Count() > 0).FirstOrDefault();
								if (lv1li != null)
									lv1li.ForeColor = Color.Green;

							}
						}
					}
				}
			}
		}

		private void tsBtnBuildsRunOne_Click(object sender, EventArgs e)
		{
			plsDie = false;
			var lis = buildList.SelectedItems;
			if (lis.Count > 0)
			{
				var li = lis[0];
				Task.Factory.StartNew(() =>
				{
					RunBuild(li, Program.Settings.MakeStats);
				});
			}
		}

		private void tsBtnBuildsSave_Click(object sender, EventArgs e)
		{
			Program.SaveBuilds();
		}

		private void tsBtnBuildsRemove_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("This will delete a build (not a loadout)." + Environment.NewLine + "How many minutes of work will be undone?", "Delete Build?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				return;

			var lis = buildList.SelectedItems;
			if (lis.Count > 0)
			{
				foreach (ListViewItem li in lis)
				{
					buildList.Items.Remove(li);
					Build b = (Build)li.Tag;
					if (b != null)
					{
						var lv1li = dataMonsterList.Items.Cast<ListViewItem>().Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == b.mon.Name).Count() > 0).FirstOrDefault();
						if (lv1li != null)
						{
							lv1li.ForeColor = Color.Black;
							// can't tell is was in storage, name is mangled on load
						}
					}
				}
			}
		}

		private void tsBtnBuildsUnlock_Click(object sender, EventArgs e)
		{
			if (Program.data == null || Program.data.Runes == null)
				return;
			foreach (Rune r in Program.data.Runes)
			{
				r.Locked = false;
			}
			checkLocked();
		}

		private void tsBtnLoadsRemove_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem li in loadoutList.SelectedItems)
			{
				Loadout l = (Loadout)li.Tag;

				foreach (Rune r in l.Runes)
				{
					r.Locked = false;
				}

				loadoutList.Items.Remove(li);
			}
			checkLocked();
		}

		public void ProgressToList(Build b, string str)
		{
			Program.log.Info("_" + str);
			this.Invoke((MethodInvoker)delegate
			{
				if (!IsDisposed)
				{
					var lvi = buildList.Items.Cast<ListViewItem>().FirstOrDefault(ll => (ll.Tag as Build).ID == b.ID);
					lvi.SubItems[3].Text = str;
				}
			});
		}

		private void tsBtnBuildsRunAll_Click(object sender, EventArgs e)
		{
			Program.RunBuilds(false, -1);
		}

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			Program.SaveBuilds();
		}

		private void toolStripButton15_Click(object sender, EventArgs e)
		{
			if (Program.data == null)
				return;

			if (Program.data.Monsters != null)
				foreach (Monster mon in Program.data.Monsters)
				{
					for (int i = 1; i < 7; i++)
						mon.Current.RemoveRune(i);
				}

			if (Program.data.Runes != null)
				foreach (Rune r in Program.data.Runes)
				{
					r.AssignedId = 0;
					r.AssignedName = "Inventory";
				}
		}

		private void toolStripButton14_Click(object sender, EventArgs e)
		{
			if (File.Exists("save.json"))
			{
				Program.LoadSave("save.json");
				RegenLists();
			}
		}

		private void tsBtnBuildsMoveUp_Click(object sender, EventArgs e)
		{
			if (buildList.SelectedItems.Count > 0)
			{
				foreach (ListViewItem sli in buildList.SelectedItems.Cast<ListViewItem>().Where(l => l.Tag != null).OrderBy(l => (l.Tag as Build).priority))
				{
					Build build = (Build)sli.Tag;

					var bb = Program.builds.OrderBy(b => b.priority).ToList();
					var bi = bb.FindIndex(b => b == build);

					if (bi != 0)
					{
						var b2 = bb[bi - 1];
						var sid = build.priority;
						build.priority = b2.priority;
						b2.priority = sid;

						sli.SubItems[0].Text = build.priority.ToString();

						var monname = b2.MonName;
						if (monname == null || monname == "Missingno")
						{
							if (b2.DownloadAwake)
								return;
							monname = b2.mon.Name;
						}

						ListViewItem listmon = buildList.Items.Cast<ListViewItem>().Where(li => li.Tag != null && (li.Tag as Build) == b2).FirstOrDefault();
						if (listmon != null)
							listmon.SubItems[0].Text = b2.priority.ToString();

						buildList.Sort();
					}
				}
			}
		}

		private void tsBtnBuildsMoveDown_Click(object sender, EventArgs e)
		{
			if (buildList.SelectedItems.Count > 0)
			{
				foreach (ListViewItem sli in buildList.SelectedItems.Cast<ListViewItem>().Where(l => l.Tag != null).OrderByDescending(l => (l.Tag as Build).priority))
				{
					Build build = (Build)sli.Tag;

					var bb = Program.builds.OrderBy(b => b.priority).ToList();
					var bi = bb.FindIndex(b => b == build);

					if (bi != Program.builds.Max(b => b.priority) && bi + 1 < bb.Count)
					{
						var b2 = bb[bi + 1];
						var sid = build.priority;
						build.priority = b2.priority;
						b2.priority = sid;

						sli.SubItems[0].Text = build.priority.ToString();

						ListViewItem listmon = buildList.Items.Cast<ListViewItem>().Where(li => li.Tag != null && (li.Tag as Build) == b2).FirstOrDefault();
						if (listmon != null)
							listmon.SubItems[0].Text = b2.priority.ToString();

						buildList.Sort();
					}
				}
			}
		}

		private void useRunesCheck_CheckedChanged(object sender, EventArgs e)
		{
			Program.Settings.UseEquipped = ((CheckBox)sender).Checked;
			Program.Settings.Save();
		}

		void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			Invoke((MethodInvoker)delegate
			{
				updateBox.Visible = true;
				try
				{
					string result = e.Result.Replace("\r\n", "\n");
					int firstline = result.IndexOf('\n');

					string newvernum = result;
					if (firstline != -1)
						newvernum = newvernum.Substring(0, firstline);

					Console.WriteLine(newvernum);
					int ind1 = result.IndexOf('\n');

					if (result.IndexOf('\n') != -1)
					{
						int ind2 = result.IndexOf('\n', ind1 + 1);
						if (ind2 == -1)
							filelink = e.Result.Substring(ind1 + 1);
						else
						{
							filelink = e.Result.Substring(ind1 + 1, ind2 - ind1 - 1);
							whatsNewText = e.Result.Substring(ind2 + 1);
						}

					}

					var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
					string oldvernum = ver.ProductVersion;
					updateCurrent.Text = "Current: " + oldvernum;
					updateNew.Text = "New: " + newvernum;
					int newver = VersionCompare(oldvernum, newvernum);
					if (newver > 0)
					{
						updateComplain.Text = "You hacker";
					}
					else if (newver < 0)
					{
						updateComplain.Text = "Update available!";
						if (filelink != "")
						{
							updateDownload.Enabled = true;
							if (whatsNewText != "")
								updateWhat.Visible = true;
						}
					}
					else
					{
						updateComplain.Visible = false;
					}
				}
				catch (Exception ex)
				{
					updateComplain.Text = e.Error.ToString();
					updateComplain.Visible = false;
					Console.WriteLine(ex);
				}
			});
		}

		private void updateDownload_Click(object sender, EventArgs e)
		{
			if (filelink != "")
			{
				Process.Start(new Uri(filelink).ToString());
			}
		}

		private void updateWhat_Click(object sender, EventArgs e)
		{
			if (whatsNewText != "")
			{
				MessageBox.Show(whatsNewText, "What's New");
			}
		}

		private void tsBtnRuneStats_Click(object sender, EventArgs e)
		{
			Program.runeSheet.StatsExcelRunes(false);
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var f = new Options())
			{
				f.ShowDialog();
				findGoodRunes.Enabled = Program.Settings.MakeStats;
				if (!Program.Settings.MakeStats)
					findGoodRunes.Checked = false;
			}
		}

		private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateBox.Show();
			updateComplain.Text = "Checking...";
			Task.Factory.StartNew(() =>
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadStringCompleted += client_DownloadStringCompleted;
					client.DownloadStringAsync(new Uri("https://raw.github.com/Skibisky/RuneManager/master/version.txt"));
				}
			});
		}

		private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			string oldvernum = ver.ProductVersion;

			MessageBox.Show("Rune Manager\r\nBy Skibisky\r\nVersion " + oldvernum, "About", MessageBoxButtons.OK);
		}

		private void userManualHelpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenHelp();
		}

		private void runelistSwapLocked(object sender, EventArgs e)
		{
			// swap the selected runes locked state
			foreach (ListViewItem li in dataRuneList.SelectedItems)
			{
				var rune = li.Tag as Rune;
				if (rune != null)
				{
					if (rune.Locked)
					{
						rune.Locked = false;
						li.BackColor = Color.Transparent;
					}
					else
					{
						rune.Locked = true;
						li.BackColor = Color.Red;
					}
				}
			}
		}

		private void runetab_savebutton_click(object sender, EventArgs e)
		{
			// only prompt and backup if the save is str8 outta proxy
			if (!Program.data.isModified && MessageBox.Show("Changes saved will be lost if you import a new save from the proxy.\n\n", "Save save?", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				Program.data.isModified = true;

				if (File.Exists("save.json"))
				{
					// backup, just in case
					string destFile = Path.Combine("", string.Format("{0}.backup{1}", "save", ".json"));
					int num = 2;
					while (File.Exists(destFile))
					{
						destFile = Path.Combine("", string.Format("{0}.backup{1}{2}", "save", num, ".json"));
						num++;
					}

					File.Copy("save.json", destFile);
				}
			}
			var str = JsonConvert.SerializeObject(Program.data);
			File.WriteAllText("save.json", str);
		}

		private void unequipMonsterButton_Click(object sender, EventArgs e)
		{
			if (Program.data?.Monsters == null)
				return;

			foreach (ListViewItem li in dataMonsterList.SelectedItems)
			{
				Monster mon = li.Tag as Monster;
				if (mon == null)
					continue;

				for (int i = 1; i < 7; i++)
				{
					var r = mon.Current.RemoveRune(i);
					if (r == null)
						continue;

					r.AssignedId = 0;
					r.AssignedName = "Inventory";
				}
			}
		}

		private void tsBtnLoadsLock_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem li in loadoutList.SelectedItems)
			{
				Loadout l = (Loadout)li.Tag;

				foreach (Rune r in l.Runes)
				{
					r.Locked = true;
				}
				checkLocked();
			}
		}

		private void tsBtnBuildsResume_Click(object sender, EventArgs e)
		{
			Program.RunBuilds(true, -1);
		}

		private void tsBtnBuildsRunUpTo_Click(object sender, EventArgs e)
		{
			int selected = -1;
			if (buildList.SelectedItems.Count > 0)
				selected = (buildList.SelectedItems[0].Tag as Build).priority;
			Program.RunBuilds(true, selected);
		}

		static int VersionCompare(string v1, string v2)
		{
			Version ver1;
			Version ver2;
			if (!Version.TryParse(v1, out ver1))
				return 1;
			if (!Version.TryParse(v2, out ver2))
				return -1;

			return ver1.CompareTo(ver2);
		}

		public void OpenHelp(string url = null, Form owner = null)
		{
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

		private void ShowMon(Monster mon)
		{
			displayMon = mon;
			var cur = mon.GetStats();

			statName.Text = mon.Name;
			statID.Text = mon.ID.ToString();
			statLevel.Text = mon.level.ToString();

			ShowStats(cur, mon);
			ShowRunes(mon.Current.Runes);
			ShowSets(mon.Current);

		}

		private void ShowSets(Loadout load)
		{
			if (load.Sets != null)
			{
				if (load.Sets.Length > 0)
					Set1Label.Text = load.Sets[0] == RuneSet.Null ? "" : load.Sets[0].ToString();
				if (load.Sets.Length > 1)
					Set2Label.Text = load.Sets[1] == RuneSet.Null ? "" : load.Sets[1].ToString();
				if (load.Sets.Length > 2)
					Set3Label.Text = load.Sets[2] == RuneSet.Null ? "" : load.Sets[2].ToString();

				if (!load.SetsFull)
				{
					if (load.Sets[0] == RuneSet.Null)
						Set1Label.Text = "Broken";
					else if (load.Sets[1] == RuneSet.Null)
						Set2Label.Text = "Broken";
					else if (load.Sets[2] == RuneSet.Null)
						Set3Label.Text = "Broken";
				}
			}
			if (runeDial != null && !runeDial.IsDisposed)
				runeDial.UpdateSets(load.Sets, !load.SetsFull);
		}

		private void ShowStats(Stats cur, Stats mon)
		{
			foreach (string stat in new string[] { "HP", "ATK", "DEF", "SPD" })
			{
				groupBox1.Controls.Find(stat + "Base", false).FirstOrDefault().Text = mon[stat].ToString();
				groupBox1.Controls.Find(stat + "Total", false).FirstOrDefault().Text = cur[stat].ToString();
				groupBox1.Controls.Find(stat + "Bonus", false).FirstOrDefault().Text = "+" + (cur[stat] - mon[stat]);
			}

			foreach (string stat in new string[] { "CR", "CD", "RES", "ACC" })
			{
				groupBox1.Controls.Find(stat + "Base", false).FirstOrDefault().Text = mon[stat] + "%";
				groupBox1.Controls.Find(stat + "Total", false).FirstOrDefault().Text = cur[stat] + "%";
				groupBox1.Controls.Find(stat + "Bonus", false).FirstOrDefault().Text = "+" + (cur[stat] - mon[stat]);
			}
		}

		private void ShowDiff(Stats old, Stats load)
		{
			foreach (string stat in new string[] { "HP", "ATK", "DEF", "SPD" })
			{
				groupBox1.Controls.Find(stat + "compBefore", false).FirstOrDefault().Text = old[stat].ToString();
				groupBox1.Controls.Find(stat + "compAfter", false).FirstOrDefault().Text = load[stat].ToString();
				groupBox1.Controls.Find(stat + "compDiff", false).FirstOrDefault().Text = (load[stat] - old[stat]).ToString();
			}

			foreach (string stat in new string[] { "CR", "CD", "RES", "ACC" })
			{
				groupBox1.Controls.Find(stat + "compBefore", false).FirstOrDefault().Text = old[stat].ToString() + "%";
				groupBox1.Controls.Find(stat + "compAfter", false).FirstOrDefault().Text = load[stat].ToString() + "%";
				groupBox1.Controls.Find(stat + "compDiff", false).FirstOrDefault().Text = (load[stat] - old[stat]).ToString();
			}

			foreach (string extra in Build.extraNames)
			{
				groupBox1.Controls.Find(extra + "compBefore", false).FirstOrDefault().Text = old.ExtraValue(extra).ToString();
				groupBox1.Controls.Find(extra + "compAfter", false).FirstOrDefault().Text = load.ExtraValue(extra).ToString();
				groupBox1.Controls.Find(extra + "compDiff", false).FirstOrDefault().Text = (load.ExtraValue(extra) - old.ExtraValue(extra)).ToString();

			}
		}

		private void ShowRunes(Rune[] rune)
		{
			runeControl1.SetRune(rune[0]);
			runeControl2.SetRune(rune[1]);
			runeControl3.SetRune(rune[2]);
			runeControl4.SetRune(rune[3]);
			runeControl5.SetRune(rune[4]);
			runeControl6.SetRune(rune[5]);

			foreach (RuneControl tc in runes)
			{
				if (tc.Tag != null)
				{
					tc.Show();
					// if the RuneControl is clicked, update the preview
					if (tc.Gamma > 1.1)
					{
						tc.Gamma = 1.4f;
						tc.Refresh();
						runeEquipped.Show();
						runeEquipped.SetRune((Rune)tc.Tag);
						lbCloseEquipped.Show();
					}
				}
				else
				{
					tc.Hide();
				}
			}
			if (runeDial != null && !runeDial.IsDisposed)
				runeDial.UpdateRunes(rune);
		}

		public void RegenLists()
		{
			dataMonsterList.Items.Clear();
			dataRuneList.Items.Clear();
			listView4.Items.Clear();
			if (Program.data == null)
				return;

			foreach (Monster mon in Program.data.Monsters)
			{
				string pri = "";
				if (mon.priority != 0)
					pri = mon.priority.ToString();

				ListViewItem item = new ListViewItem(new string[]{
					mon.Name,
					mon.ID.ToString(),
					pri,
				});
				if (mon.inStorage)
					item.ForeColor = Color.Gray;

				item.Tag = mon;
				dataMonsterList.Items.Add(item);
			}

			foreach (Rune rune in Program.data.Runes)
			{
				ListViewItem item = new ListViewItem(new string[]{
					rune.Set.ToString(),
					rune.ID.ToString(),
					rune.Grade.ToString(),
					Rune.StringIt(rune.MainType, true),
					rune.MainValue.ToString()
				});
				item.Tag = rune;
				if (rune.Locked)
					item.BackColor = Color.Red;
				dataRuneList.Items.Add(item);
			}
			checkLocked();
		}

		public void checkLocked()
		{
			if (Program.data?.Runes == null)
				return;

			Invoke((MethodInvoker)delegate
			{
				toolStripStatusLabel1.Text = "Locked: " + Program.data.Runes.Count(r => r.Locked);
				foreach (ListViewItem li in dataRuneList.Items)
				{
					var rune = li.Tag as Rune;
					if (rune != null)
						li.BackColor = rune.Locked ? Color.Red : Color.Transparent;
				}
			});
		}

		private void RunBuild(ListViewItem pli, bool saveStats = false)
		{
			if ((pli?.Tag as Build) == null)
				return;

			Program.RunBuild((Build)pli.Tag, saveStats);/*, (bb, s) => Invoke((MethodInvoker) delegate
			{
				Log.Info(s);
				if (!this.IsDisposed)
					pli.SubItems[3].Text = s;
			}));*/
		}

		[Obsolete("Please run this on Program, not Main", true)]
		private void RunBuild(Build b, bool saveStats = false, Action<string> printTo = null)
		{
			try
			{
				if (b == null)
				{
					Log.Info("Build is null");
					return;
				}

				if (plsDie)
				{
					Log.Info("Cancelling build " + b.ID + " " + b.MonName);
					//plsDie = false;
					return;
				}

				if (currentBuild != null)
				{
					Log.Info("Force stopping " + currentBuild.ID + " " + currentBuild.MonName);
					// TODO: consider things
					//currentBuild.IsRunning = false;
				}

				if (isRunning)
					Log.Info("Looping...");

				while (isRunning)
				{
					plsDie = true;
					// TODO: consider things
					//b.isRun = false;
					Thread.Sleep(100);
				}

				Log.Info("Starting watch " + b.ID + " " + b.MonName);

				Stopwatch buildTime = Stopwatch.StartNew();
				currentBuild = b;

				ListViewItem[] olvs = null;
				Invoke((MethodInvoker)delegate { olvs = loadoutList.Items.Find(b.ID.ToString(), false); });

				if (olvs.Length > 0)
				{
					var olv = olvs.First();
					Loadout ob = (Loadout)olv.Tag;
					foreach (Rune r in ob.Runes)
					{
						r.Locked = false;
					}
				}

				b.RunesUseLocked = false;
				b.RunesUseEquipped = Program.Settings.UseEquipped;
				b.BuildSaveStats = saveStats;
				b.GenRunes(Program.data);
				b.shrines = Program.data.shrines;

				#region Check enough runes
				string nR = "";
				for (int i = 0; i < b.runes.Length; i++)
				{
					if (b.runes[i] != null && b.runes[i].Length == 0)
						nR += (i + 1) + " ";
				}

				if (nR != "")
				{
					printTo?.Invoke(":( " + nR + "Runes");
					return;
				}
				#endregion

				isRunning = true;

				b.BuildTimeout = 0;
				b.BuildTake = 0;
				b.BuildGenerate = 0;
				b.BuildPrintTo += this.Program_BuildsProgressTo;
				b.BuildDumpBads = true;

				b.GenBuilds();

				b.BuildPrintTo -= this.Program_BuildsProgressTo;

				#region Save null build
				if (b.Best == null)
				{
					Invoke((MethodInvoker)delegate
					{
						if (saveStats)
						{
							Program.runeSheet.StatsExcelBuild(b, b.mon, null, true);
						}
					});
					goto finishBuild;
				}
				#endregion

				b.Best.Current.BuildID = b.ID;
				//Program.builds.Add(b);

				#region Get the rune diff
				int numchanged = 0;
				int numnew = 0;
				int powerup = 0;
				int upgrades = 0;
				foreach (Rune r in b.Best.Current.Runes)
				{
					r.Locked = true;
					if (r.AssignedName != b.Best.Name)
					{
						if (r.IsUnassigned)
							numnew++;
						else
							numchanged++;
					}
					powerup += Math.Max(0, (b.Best.Current.FakeLevel[r.Slot - 1]) - r.Level);
					if (b.Best.Current.FakeLevel[r.Slot - 1] != 0)
					{
						int tup = (int)Math.Floor(Math.Min(12, (b.Best.Current.FakeLevel[r.Slot - 1])) / (double)3);
						int cup = (int)Math.Floor(Math.Min(12, r.Level) / (double)3);
						upgrades += Math.Max(0, tup - cup);
					}
				}
				#endregion

				currentBuild = null;
				b.Time = buildTime.ElapsedMilliseconds;
				buildTime.Stop();
				Log.Info("Stopping watch " + b.ID + " " + b.MonName + " @ " + buildTime.ElapsedMilliseconds);

				Invoke((MethodInvoker)delegate
				{
					checkLocked();

					ListViewItem nli;

					var lvs = loadoutList.Items.Find(b.ID.ToString(), false);

					if (lvs.Length == 0)
						nli = new ListViewItem();
					else
						nli = lvs.First();

					b.Best.Current.Time = b.Time;

					nli.Tag = b.Best.Current;
					nli.Text = b.ID.ToString();
					nli.Name = b.ID.ToString();
					while (nli.SubItems.Count < 6)
						nli.SubItems.Add("");
					nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, b.ID.ToString());
					nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, b.Best.Name);
					nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, b.Best.ID.ToString());
					nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (numnew + numchanged).ToString());
					if (numnew > 0 && b.mon.Current.RuneCount < 6)
						nli.SubItems[3].ForeColor = Color.Green;
					if (Program.Settings.SplitAssign)
						nli.SubItems[3].Text = numnew.ToString() + "/" + numchanged.ToString();

					nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, powerup.ToString());
					nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, (b.Time / (double)1000).ToString("0.##"));
					nli.UseItemStyleForSubItems = false;
					if (b.Time / (double)1000 > 60)
						nli.SubItems[5].BackColor = Color.Red;
					else if (b.Time / (double)1000 > 30)
						nli.SubItems[5].BackColor = Color.Orange;

					if (lvs.Length == 0)
						loadoutList.Items.Add(nli);
				});

				// if we are on the hunt of good runes.
				if (goodRunes && saveStats)
				{
					var theBest = b.Best;
					int count = 0;
					// we must progressively ban more runes from the build to find second-place runes.
					//GenDeep(b, 0, printTo, ref count);
					RunBanned(b, printTo, ++count, theBest.Current.Runes.Where(r => r.Slot % 2 != 0).Select(r => r.ID).ToArray());
					RunBanned(b, printTo, ++count, theBest.Current.Runes.Where(r => r.Slot % 2 == 0).Select(r => r.ID).ToArray());
					RunBanned(b, printTo, ++count, theBest.Current.Runes.Select(r => r.ID).ToArray());

					// after messing all that shit up
					b.Best = theBest;
				}

				#region Save Build stats

				if (saveStats)
				{
					Program.runeSheet.StatsExcelBuild(b, b.mon, null, true);
				}

				// clean up for GC
				if (b.buildUsage != null)
					b.buildUsage.loads.Clear();
				if (b.runeUsage != null)
				{
					b.runeUsage.runesGood.Clear();
					b.runeUsage.runesUsed.Clear();
				}
				b.runeUsage = null;
				b.buildUsage = null;

				#endregion

				finishBuild:
				if (plsDie)
					printTo?.Invoke("Canned");
				else if (b.Best != null)
					printTo?.Invoke("Done");
				else
					printTo?.Invoke("Zero :(");

				Log.Info("Cleaning up");
				isRunning = false;
				//b.isRun = false;
				currentBuild = null;
				Log.Info("Cleaned");
			}
			catch (Exception e)
			{
				Log.Error("Error during build " + b.ID + " " + e.Message + Environment.NewLine + e.StackTrace);
			}
		}

		private void GenDeep(Build b, int slot0, Action<string> printTo, ref int count, params int[] doneIds)
		{
			if (plsDie)
				return;

			if (doneIds == null)
				doneIds = new int[] { };

			for (int i = slot0; i < 6; i++)
			{
				if (plsDie)
					return;

				count++;
				var c = count;
				Rune r = b.Best.Current.Runes[i];
				doneIds = doneIds.Concat(new int[] { r.ID }).ToArray();

				RunBanned(b, printTo, c, doneIds);

				for (int j = i + 1; j < 6; j++)
				{
					if (plsDie)
						return;
					GenDeep(b, j, printTo, ref count, doneIds);
				}
			}
		}

		private void RunBanned(Build b, Action<string> printTo, int c, params int[] doneIds)
		{
			Log.Info("Running ban");
			b.BanEmTemp(doneIds);

			b.RunesUseLocked = false;
			b.RunesUseEquipped = Program.Settings.UseEquipped;
			b.BuildSaveStats = true;
			b.BuildGoodRunes = goodRunes;
			b.GenRunes(Program.data);

			b.BuildTimeout = 0;
			b.BuildTake = 0;
			b.BuildGenerate = 0;
			b.BuildPrintTo += this.Program_BuildsProgressTo;
			b.BuildDumpBads = true;
			b.GenBuilds();
			b.BuildPrintTo -= this.Program_BuildsProgressTo;
			Log.Info("Ban finished");
		}

		private void ClearLoadouts()
		{
			Program.ClearLoadouts();
			checkLocked();
		}

		[Obsolete("Use Program. not Main.")]
		private void RunBuilds(bool skipLoaded, int runTo = -1)
		{
			if (Program.data == null)
				return;

			try {

				if (runTask != null && runTask.Status == TaskStatus.Running)
				{
					runSource.Cancel();
					//if (currentBuild != null)
					//    currentBuild.isRun = false;
					plsDie = true;
					isRunning = false;
					return;
				}
				plsDie = false;

				List<int> loady = new List<int>();

				if (skipLoaded)
				{
					// collect loadouts
					foreach (ListViewItem li in loadoutList.Items)
					{
						Loadout load = li.Tag as Loadout;

						var monid = int.Parse(li.SubItems[2].Text);
						var bid = int.Parse(li.SubItems[0].Text);

						if (load != null)
							loady.Add(bid);
					}
				}
				else
				{
					ClearLoadouts();
					foreach (var r in Program.data.Runes)
					{
						r.manageStats.AddOrUpdate("buildScoreIn", 0, (k, v) => 0);
						r.manageStats.AddOrUpdate("buildScoreTotal", 0, (k, v) => 0);
					}
				}

				bool collect = true;
				int newPri = 1;
				// collect the builds
				List<ListViewItem> list5 = new List<ListViewItem>();
				foreach (ListViewItem li in buildList.Items)
				{
					li.SubItems[0].Text = newPri.ToString();
					(li.Tag as Build).priority = newPri++;

					if (loady.Contains((li.Tag as Build).ID))
						continue;

					if ((li.Tag as Build).ID == runTo)
						collect = false;

					if (collect)
						list5.Add(li);

					li.SubItems[3].Text = "";
				}

				runSource = new CancellationTokenSource();
				runToken = runSource.Token;
				runTask = Task.Factory.StartNew(() =>
				{
					if (Program.data.Runes != null && !skipLoaded)
					{
						foreach (Rune r in Program.data.Runes)
						{
							r.Swapped = false;
							r.ResetStats();
						}
					}

#warning consider making it nicer by using the List<Build>
					foreach (ListViewItem li in list5)
					{
						if (plsDie) break;
						RunBuild(li, Program.Settings.MakeStats);
					}

					if (Program.Settings.MakeStats)
					{
						Invoke((MethodInvoker)delegate
						{
							if (!skipLoaded)
								Program.runeSheet.StatsExcelRunes(true);
							try
							{
								Program.runeSheet.StatsExcelSave(true);
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex);
							}
						});
					}
					checkLocked();
				}, runSource.Token);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, e.GetType().ToString());
			}
		}

		private void buildList_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				teamBuild = null;
				if (buildList.FocusedItem.Bounds.Contains(e.Location))
				{
					if (buildList.FocusedItem.Tag == null)
						return;

					teamBuild = buildList.FocusedItem.Tag as Build;

					teamChecking = true;

					foreach (ToolStripMenuItem tsmi in teamToolStripMenuItem.DropDownItems)
					{
						tsmi.Image = null;
						if (tsTeamCheck(tsmi))
							tsmi.Image = App.add;
					}
					teamChecking = false;

					menu_buildlist.Show(Cursor.Position);
				}
			}
		}

		private bool tsTeamCheck(ToolStripMenuItem t)
		{
			bool ret = false;
			t.Checked = false;
			t.Image = null;
			if (teamBuild.Teams.Contains(t.Text))
			{
				t.Checked = true;
				ret = true;
			}
			foreach (ToolStripMenuItem smi in t.DropDownItems)
			{
				if (tsTeamCheck(smi))
				{
					t.Image = App.add;
					ret = true;
				}
			}
			return ret;
		}

		int GetRel(string first, string second)
		{
			if (first == second)
				return 0;

			string p1 = null;
			string p2 = null;

			if (toolmap.Keys.Contains(first) && toolmap.Keys.Contains(second))
				return 1;

			foreach (var k in toolmap)
			{
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

		private void buildList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool doColor = buildList.SelectedItems.Count == 1;
			var b1 = doColor ? buildList.SelectedItems[0].Tag as Build : null;

			foreach (ListViewItem li in buildList.Items)
			{
				li.BackColor = Color.White;
				if (Program.Settings.ColorTeams && b1 != null && b1.Teams.Count > 0)
				{
					var b2 = li.Tag as Build;
					if (b2 != null && b2.Teams.Count > 0)
					{
						int close = -1;
						foreach (var t1 in b1.Teams)
						{
							foreach (var t2 in b2.Teams)
							{
								var c = GetRel(t1, t2);
								if (c != -1)
									close = close == -1 ? c : (c < close ? c : close);
								if (close == 0)
									break;
							}
							if (close == 0)
								break;
						}
						if (close == 0)
							li.BackColor = Color.Lime;
						else if (close == 1)
							li.BackColor = Color.LightGreen;
						else if (close == 2)
							li.BackColor = Color.DimGray;
						else if (close == 3)
							li.BackColor = Color.LightGray;
					}
				}
			}
		}

		private void tsBtnLoadsSave_Click(object sender, EventArgs e)
		{
			Program.SaveLoadouts();
		}

		private void tsBtnLoadsLoad_Click(object sender, EventArgs e)
		{
			Program.LoadLoadouts();
			checkLocked();
		}

		private void pictureBox1_DoubleClick(object sender, EventArgs e)
		{
			if (runeDial == null || runeDial.IsDisposed)
				runeDial = new RuneDial();
			if (!runeDial.Visible)
			{
				runeDial.Show(this);
				var xx = Location.X + 1105 + 8 - 271;//271, 213
				var yy = Location.Y + 49 + 208 - 213;// 8, 208 1105, 49

				runeDial.Location = new Point(xx, yy);
				runeDial.Location = new Point(Location.X + Width, Location.Y);
			}
			if (displayMon != null)
				runeDial.UpdateLoad(displayMon.Current);
		}

		private void findGoodRunes_CheckedChanged(object sender, EventArgs e)
		{
			if (findGoodRunes.Checked && MessageBox.Show("This runs each test multiple times.\r\nThat means leaving it overnight or something.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
			{
				goodRunes = true;
			}
			else
				findGoodRunes.Checked = false;
			goodRunes = findGoodRunes.Checked;
		}
	}

}