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
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;
using RuneOptim.Management;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace RuneApp {


	public partial class Main : Form {
		// Here we keep all the WinForm callbacks, because of the way the Designer works
		// TODO: empty the logic into functions, instead of inside the callbacks

		BlockingCollection<EventArgs> blockCol = new BlockingCollection<EventArgs>();

		private void Main_Load(object sender, EventArgs e) {

			#region Watch collections and try loading
			Program.saveFileTouched += Program_saveFileTouched;
			//Program.data.Runes.CollectionChanged += Runes_CollectionChanged;
			Program.OnRuneUpdate += Program_OnRuneUpdate;
			Program.OnMonsterUpdate += Program_OnMonsterUpdate;
			Program.loads.CollectionChanged += Loads_CollectionChanged;
			Program.BuildsPrintTo += Program_BuildsPrintTo;
			Program.BuildsProgressTo += Program_BuildsProgressTo;

			buildList.Items.Add("Loading...");
			dataMonsterList.Items.Add("Loading...");

			Task.Run(() => {
				// TODO: this is slow during profiling
#if !DEBUG
				try {
#endif
					LoadSaveResult loadResult = 0;
					do {
						loadResult = Program.FindSave();
						switch (loadResult) {
							case LoadSaveResult.Success:
								break;
							default:
								if (MessageBox.Show("Couldn't automatically load save.\r\nManually locate a save file?", "Load Save", MessageBoxButtons.YesNo) == DialogResult.Yes) {
									loadResult = loadSaveDialogue();
								}
								else {
									Application.Exit();
									return;
								}
								break;
						}
					} while (loadResult != LoadSaveResult.Success);

					loadResult = 0;
					do {
						loadResult = Program.LoadBuilds();
						switch (loadResult) {
							case LoadSaveResult.Failure:
								if (MessageBox.Show("Save was invalid while loading builds.\r\nManually locate a save file?", "Load Builds", MessageBoxButtons.YesNo) == DialogResult.Yes) {
									loadResult = loadSaveDialogue();
								}
								break;
							case LoadSaveResult.EmptyFile:
							case LoadSaveResult.FileNotFound:
								loadResult = LoadSaveResult.Success;
								break;
							default:
								break;
						}
					} while (loadResult != LoadSaveResult.Success);

					this.Invoke((MethodInvoker)delegate {

						RebuildLists();
					});

#if !DEBUG
				}
				catch (Exception ex) {
					MessageBox.Show($"Critical Error {ex.GetType()}\r\nDetails in log file.", "Error");
					LineLog.Fatal($"Fatal during load {ex.GetType()}", ex);
				}
#endif


				#region Shrines

				ToolStripMenuItem[] shrineMenu = new ToolStripMenuItem[] { speedToolStripMenuItem, defenseToolStripMenuItem , attackToolStripMenuItem, healthToolStripMenuItem,
			waterAttackToolStripMenuItem, fireAttackToolStripMenuItem, windAttackToolStripMenuItem, lightAttackToolStripMenuItem, darkAttackToolStripMenuItem, criticalDamageToolStripMenuItem};
				this.Invoke((MethodInvoker)delegate {
					for (int i = 0; i < 11; i++) {
						for (int j = 0; j < Deco.ShrineStats.Length; j++) {
							if (j < 4)
								addShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(i * Deco.ShrineLevel[j]), shrineMenu[j]);
							else if (j < 9)
								addShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(1 + i * Deco.ShrineLevel[j]), shrineMenu[j]);
							else
								addShrine(Deco.ShrineStats[j], i, (int)Math.Floor(i * Deco.ShrineLevel[j]), shrineMenu[j]);
						}
					}
				});
				#endregion

				#region Sorter
				var sorter = new ListViewSort();
				this.Invoke((MethodInvoker)delegate {
					dataMonsterList.ListViewItemSorter = sorter;
					sorter.OnColumnClick(colMonGrade.Index);
					sorter.OnColumnClick(colMonPriority.Index);
					dataMonsterList.Sort();

					dataRuneList.ListViewItemSorter = new ListViewSort();

					sorter = new ListViewSort();
					sorter.OnColumnClick(1);
					dataCraftList.ListViewItemSorter = sorter;

					sorter = new ListViewSort();
					sorter.OnColumnClick(1);
					buildList.ListViewItemSorter = sorter;
				});
				#endregion

				RebuildBuildList();
				Program.builds.CollectionChanged += Builds_CollectionChanged;

			});
			#endregion

			//buildList.SelectedIndexChanged += buildList_SelectedIndexChanged;

			LineLog.Debug("Preparing teams");

			this.FormClosing += (a, b) => {
				blockCol.CompleteAdding();
			};
			backgroundWorker1.WorkerReportsProgress = true;
			backgroundWorker1.DoWork += Main_DoWork;
			backgroundWorker1.ProgressChanged += Main_ProgChanged;

			//backgroundWorker1.RunWorkerAsync();
			timer1.Tick += Timer1_Tick;
			timer1.Interval = 5;
			timer1.Start();

			tsTeamAdd(teamToolStripMenuItem, "PvE");
			tsTeamAdd(teamToolStripMenuItem, "Dungeon");
			tsTeamAdd(teamToolStripMenuItem, "Raid");
			tsTeamAdd(teamToolStripMenuItem, "PvP");

			var tsnone = new ToolStripMenuItem("(Clear)");
			tsnone.Font = new Font(tsnone.Font, FontStyle.Italic);
			tsnone.Click += tsTeamHandler;

			teamToolStripMenuItem.DropDownItems.Add(tsnone);

			if (Program.Settings.StartUpHelp)
				OpenHelp();

			if (irene == null)
				irene = new Irene(this);
			if (Program.Settings.ShowIreneOnStart)
				irene.Show(this);

			LineLog.Info("Main form loaded");
			loading = false;
			buildList.Sort();
		}

		private void Timer1_Tick(object sender, EventArgs e) {
			/*if (lastProg != null) {
				toolStripBuildStatus.Text = "Build Status: " + lastProg.Progress;
				//lastProg = null;
			}
			if (lastPrint != null) {
				ProgressToList(lastPrint.build, lastPrint.Message);
				//lastPrint = null;
			}*/

			if (Program.CurrentBuild?.runner is IBuildRunner br) {
				toolStripBuildStatus.Text = $"Build Status: {br.Good:N0} ~ {br.Completed:N0} - {br.Skipped:N0}";
				ProgressToList(Program.CurrentBuild, ((double)br.Completed / (double)br.Expected).ToString("P2"));
			}
			else if (Program.CurrentBuild != null && Program.CurrentBuild.IsRunning) {
				toolStripBuildStatus.Text = $"Build Status: {(Program.CurrentBuild.BuildUsage?.passed ?? 0):N0} ~ {Program.CurrentBuild.count:N0} - {Program.CurrentBuild.skipped:N0}";
				ProgressToList(Program.CurrentBuild, ((double)Program.CurrentBuild.count / (double)Program.CurrentBuild.total).ToString("P2"));
			}
		}

		ProgToEventArgs lastProg;
		PrintToEventArgs lastPrint;

		private void Main_ProgChanged(object a, ProgressChangedEventArgs b) {
			if (b.UserState is ProgToEventArgs e2) {
				toolStripBuildStatus.Text = "Build Status: " + e2.Progress;
				ProgressToList(e2.build, e2.Percent.ToString("P2"));
			}
			else if (b.UserState is PrintToEventArgs e3) {
				ProgressToList(e3.build, e3.Message);
			}
		}

		private void Main_DoWork(object a, DoWorkEventArgs b) {
			foreach (var w in blockCol.GetConsumingEnumerable()) {
				backgroundWorker1.ReportProgress(1, w);
			}
		}

		private void Program_BuildsProgressTo(object sender, ProgToEventArgs e) {
			//ProgressToList(e.build, e.Percent.ToString("P2"));
			//backgroundWorker1.RunWorkerAsync(e);
			//blockCol.Add(e);
			if (lastProg == null || e.Progress >= lastProg.Progress)
				lastProg = e;
			//Application.DoEvents();
			if (!this.IsDisposed)
				this.Invoke((MethodInvoker)delegate {
					toolStripBuildStatus.Text = "Build Status: " + e.Progress;
			});
		}

		private void Program_BuildsPrintTo(object sender, PrintToEventArgs e) {
			LineLog.Debug(e.Message, e.Line, e.Caller, e.File);
			//if (!blockCol.IsAddingCompleted)
			//	blockCol.Add(e);
			if (lastPrint == null || e.build != lastPrint.build || e.Order >= lastPrint.Order)
				lastPrint = e;
			//Application.DoEvents();
			//backgroundWorker1.RunWorkerAsync(e);
			ProgressToList(e.build, e.Message);
		}

		private void Runes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			switch (e.Action) {
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
				default:
					throw new NotImplementedException();
			}
		}

		private void Program_saveFileTouched(object sender, EventArgs e) {
			this.fileBox.Visible = true;
		}

		private void ColorMonsWithBuilds() {
			foreach (ListViewItem lvim in dataMonsterList.Items) {
				if (Program.builds.Any(b => b.Mon == lvim.Tag as Monster)) {
					lvim.ForeColor = Color.Green;
				}
			}
		}

		private void Loads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			switch (e.Action) {
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					foreach (var l in e.NewItems.OfType<Loadout>()) {
						var mm = Program.builds.FirstOrDefault(b => b.ID == l.BuildID)?.Mon;
						if (mm != null) {
							mm.OnRunesChanged += Mm_OnRunesChanged;
						}

						checkLocked();
						Invoke((MethodInvoker)delegate {
							ListViewItem nli = loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout).BuildID == l.BuildID) ?? new ListViewItem();

							var bli = buildList.Items.OfType<ListViewItem>().FirstOrDefault(bi => (bi.Tag as Build).ID == l.BuildID);
							if (bli != null) {
								var ahh = bli.SubItems[3].Text;
								if (ahh == "" || ahh == "!") {
									bli.SubItems[3].Text = "Loaded";
								}
							}

							ListViewItemLoad(nli, l);
							if (!loadoutList.Items.Contains(nli))
								loadoutList.Items.Add(nli);
						});
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					loadoutList.Items.Clear();
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					foreach (var l in e.OldItems.OfType<Loadout>()) {
						var mm = Program.builds.FirstOrDefault(b => b.ID == l.BuildID)?.Mon;
						if (mm != null) {
							mm.OnRunesChanged -= Mm_OnRunesChanged;
						}

						Invoke((MethodInvoker)delegate {
							loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => li.Tag == l).Remove();
						});
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
				default:
					throw new NotImplementedException();
			}
		}

		private void Mm_OnRunesChanged(object sender, EventArgs e) {
			var bb = Program.builds.FirstOrDefault(b => b.Mon != null && b.Mon == (sender as Monster));
			if (bb == null)
				return;

			var l = Program.loads.FirstOrDefault(lo => lo.BuildID == bb.ID);
			if (l == null)
				return;

			Invoke((MethodInvoker)delegate {
				ListViewItem nli = loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout).BuildID == l.BuildID) ?? new ListViewItem();
				ListViewItemLoad(nli, l);
			});
		}


		private void ListViewItemLoad(ListViewItem nli, Loadout l) {
			Build b = Program.builds.FirstOrDefault(bu => bu.ID == l.BuildID);
			nli.Tag = l;
			nli.Text = b.ID.ToString();
			nli.Name = b.ID.ToString();
			nli.UseItemStyleForSubItems = false;
			while (nli.SubItems.Count < 6)
				nli.SubItems.Add("");
			nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, b.MonName);
			nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, b.ID.ToString());
			nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, b.Mon.Id.ToString());

			l.RecountDiff(b.Mon.Id);

			nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (l.RunesNew + l.runesChanged).ToString());
			if (l.Runes.Where(r => r != null && r.IsUnassigned).Any(r => b.Mon.Runes.FirstOrDefault(ru => ru != null && ru.Slot == r.Slot) == null))
				nli.SubItems[3].ForeColor = Color.Green;
			if (b.Type == BuildType.Lock)
				nli.SubItems[0].ForeColor = Color.Gray;
			else if (b.Type == BuildType.Link)
				nli.SubItems[0].ForeColor = Color.Teal;
			if (Program.Settings.SplitAssign)
				nli.SubItems[3].Text = l.RunesNew.ToString() + "/" + l.runesChanged.ToString();
			nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, l.powerup.ToString());
			nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, (l.Time / (double)1000).ToString("0.##"));
			if (l.Time / (double)1000 > 60)
				nli.SubItems[5].BackColor = Color.Red;
			else if (l.Time / (double)1000 > 30)
				nli.SubItems[5].BackColor = Color.Orange;
		}

		private void Program_OnMonsterUpdate(object sender, bool deleted) {
			if (sender is Monster mon) {
				Invoke((MethodInvoker)delegate {
					var nli = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Monster).Id == mon.Id);
					if (deleted) {
						if (nli != null)
							dataMonsterList.Items.Remove(nli);
						return;
					}

					bool add = nli == null;
					nli = ListViewItemMonster(mon, nli);
					if (add)
						dataMonsterList.Items.Add(nli);

					if (displayMon?.Id == mon.Id)
						ShowMon(displayMon);
				});
			}
		}

		private void Program_OnRuneUpdate(object sender, bool deleted) {
			if (sender is Rune rune) {
				Invoke((MethodInvoker)delegate {
					var nli = dataRuneList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Rune).Id == rune.Id);
					if (deleted) {
						if (nli != null)
							dataRuneList.Items.Remove(nli);
						return;
					}

					bool add = nli == null;
					nli = ListViewItemRune(rune, nli);
					if (add)
						dataRuneList.Items.Add(nli);

					if (this.runeInventory.RuneId == rune.Id)
						runeInventory.SetRune(rune);
					if (this.runeEquipped.RuneId == rune.Id)
						runeEquipped.SetRune(rune);
					if (displayMon?.Id == rune.AssignedId)
						ShowMon(displayMon);
				});
			}
		}

		private void Builds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
			switch (e.Action) {
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					List<ListViewItem> tempMons = null;
					this.Invoke((MethodInvoker)delegate { tempMons = dataMonsterList.Items.OfType<ListViewItem>().ToList(); });

					foreach (var b in e.NewItems.OfType<Build>()) {
						ListViewItem li = new ListViewItem();
						this.Invoke((MethodInvoker)delegate {
							ListViewItemBuild(li, b);
							buildList.Items.Add(li);
							if (!loading)
								buildList.Sort();
						});
						var lv1li = tempMons.FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == (b.Mon?.Id ?? b.MonId).ToString()));
						if (lv1li != null) {
							lv1li.ForeColor = Color.Green;
						}
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					foreach (var b in e.OldItems.OfType<Build>()) {
						var bli = buildList.Items.OfType<ListViewItem>().FirstOrDefault(lvi => b.Equals(lvi.Tag));
						buildList.Items.Remove(bli);

						var lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == b.Mon.FullName));
						if (lv1li != null) {
							lv1li.ForeColor = Color.Black;
							if (lv1li.Tag is Monster mon && mon.inStorage)
								lv1li.ForeColor = Color.Gray;
						}
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
				default:
					throw new NotImplementedException();
			}
		}

		private void tsTeamHandler(object sender, EventArgs e) {
			if (teamChecking || teamBuild == null)
				return;

			ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
			if (tsmi.Text == "(Clear)") {
				teamBuild.Teams.Clear();
			}
			else {
				if (tsmi.Checked) {
					if (!teamBuild.Teams.Contains(tsmi.Text))
						teamBuild.Teams.Add(tsmi.Text);
				}
				else {
					teamBuild.Teams.Remove(tsmi.Text);
				}
			}
			var bli = buildList.Items.OfType<ListViewItem>().FirstOrDefault(it => it.Tag == teamBuild);
			if (bli != null) {
				while (bli.SubItems.Count < 6) bli.SubItems.Add("");
				bli.SubItems[5].Text = getTeamStr(teamBuild);
			}
			menu_buildlist.Close();
		}

		private void monstertab_list_select(object sender, EventArgs e) {
			if (dataMonsterList?.FocusedItem?.Tag is Monster mon) {
				ShowMon(mon);
				lastFocused = null;
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			// confirm maybe?
			Close();
		}

		private void runeDial_RuneClick(object sender, RuneClickEventArgs e) {
			if (e.Rune != null) {
				runeEquipped.Show();
				runeEquipped.SetRune(e.Rune);
			}
			else {
				runeEquipped.Hide();
			}
		}

		private void runeDial1_LoadoutChanged(object sender, Loadout l) {
			if (runeDial.RuneSelected == 0)
				return;

			var r = l.Runes[runeDial.RuneSelected - 1];
			runeEquipped.SetRune(r);
		}

		private void lbCloseEquipped_Click(object sender, EventArgs e) {
			runeEquipped.Hide();
			runeDial.ResetRuneClicked();
		}

		private void lbCloseInventory_Click(object sender, EventArgs e) {
			runeInventory.Hide();
		}

		private void runetab_list_select(object sender, EventArgs e) {
			if (dataRuneList.SelectedItems.OfType<ListViewItem>().FirstOrDefault()?.Tag is Rune rune) {
				runeInventory.Show();
				runeInventory.SetRune(rune);
			}
		}

		private void crafttab_list_select(object sender, EventArgs e) {
			if (dataCraftList.SelectedItems.OfType<ListViewItem>().FirstOrDefault()?.Tag is Craft craft) {
				runeInventory.Show();
				runeInventory.SetCraft(craft);
			}
		}

		private void listView2_ColumnClick(object sender, ColumnClickEventArgs e) {
			var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
			sorter.OnColumnClick(e.Column);
			((ListView)sender).Sort();
		}

		private void tsbIncreasePriority_Click(object sender, EventArgs e) {
			if (dataMonsterList?.FocusedItem?.Tag is Monster mon) {
				int maxPri = Program.data.Monsters.Max(x => x.priority);
				if (mon.priority == 0) {
					mon.priority = maxPri + 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
				}
				else if (mon.priority != 1) {
					int pri = mon.priority;
					Monster mon2 = Program.data.Monsters.FirstOrDefault(x => x.priority == pri - 1);
					if (mon2 != null) {
						ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.FullName);
						mon2.priority += 1;
						listMon.SubItems[colMonPriority.Index].Text = mon2.priority.ToString();
					}
					mon.priority -= 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (mon.priority).ToString();
				}
				dataMonsterList.Sort();
			}
		}

		private void tsbDecreasePriority_Click(object sender, EventArgs e) {
			if (dataMonsterList?.FocusedItem?.Tag is Monster mon) {
				int maxPri = Program.data.Monsters.Max(x => x.priority);
				if (mon.priority == 0) {
					mon.priority = maxPri + 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
				}
				else if (mon.priority != maxPri) {
					int pri = mon.priority;
					Monster mon2 = Program.data.Monsters.FirstOrDefault(x => x.priority == pri + 1);
					if (mon2 != null) {
						ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.FullName);
						mon2.priority -= 1;
						listMon.SubItems[colMonPriority.Index].Text = mon2.priority.ToString();
					}
					mon.priority += 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (mon.priority).ToString();
				}
				dataMonsterList.Sort();
			}

		}

		private void button2_Click(object sender, EventArgs e) {
			tabControl1.SelectTab(tabRunes.Name);
			filterRunesList(x => ((Rune)x).Slot == ((Rune)runeEquipped.Tag).Slot);
		}

		private void runetab_clearfilter(object sender, EventArgs e) {
			filterRunesList(x => true);
		}

		private void loadoutlist_SelectedIndexChanged(object sender, EventArgs e) {
			if (loadoutList.SelectedItems.Count == 0)
				lastFocused = null;
			if (loadoutList.FocusedItem != null && lastFocused != loadoutList.FocusedItem && loadoutList.SelectedItems.Count == 1) {
				var item = loadoutList.FocusedItem;
				if (item.Tag != null) {
					Loadout load = (Loadout)item.Tag;

					var monid = ulong.Parse(item.SubItems[2].Text);
					var bid = int.Parse(item.SubItems[1].Text);

					var build = Program.builds.FirstOrDefault(b => b.ID == bid);

					Monster mon = null;
					if (build == null)
						mon = Program.data.GetMonster(monid);
					else
						mon = build.Mon;

					ShowMon(mon);

					ShowStats(load.GetStats(mon), mon);

					ShowLoadout(load);

					var dmon = Program.data.GetMonster(monid);
					if (dmon != null) {
						var dmonld = dmon.Current.Leader;
						var dmonsh = dmon.Current.Shrines;
						var dmonbu = dmon.Current.Buffs;
						dmon.Current.Leader = load.Leader;
						dmon.Current.Shrines = load.Shrines;
						dmon.Current.Buffs = load.Buffs;
						var dmonfl = dmon.Current.FakeLevel;
						var dmonps = dmon.Current.PredictSubs;
						dmon.Current.FakeLevel = load.FakeLevel;
						dmon.Current.PredictSubs = load.PredictSubs;

						if (build != null) {
							var beforeScore = build.CalcScore(dmon.GetStats());
							var afterScore = build.CalcScore(load.GetStats(mon));
							groupBox1.Controls.Find("PtscompBefore", false).FirstOrDefault().Text = beforeScore.ToString("0.##");
							groupBox1.Controls.Find("PtscompAfter", false).FirstOrDefault().Text = afterScore.ToString("0.##");
							var dScore = load.DeltaPoints;
							if (dScore == 0)
								dScore = afterScore - beforeScore;
							string str = dScore.ToString("0.##");
							if (dScore != 0)
								str += " (" + (afterScore - beforeScore).ToString("0.##") + ")";
							groupBox1.Controls.Find("PtscompDiff", false).FirstOrDefault().Text = str;
						}
						ShowDiff(dmon.GetStats(), load.GetStats(mon), build);

						dmon.Current.Leader = dmonld;
						dmon.Current.Shrines = dmonsh;
						dmon.Current.Buffs = dmonbu;
						dmon.Current.FakeLevel = dmonfl;
						dmon.Current.PredictSubs = dmonps;

					}
				}
				lastFocused = loadoutList.FocusedItem;
			}

			int cost = 0;
			foreach (ListViewItem li in loadoutList.SelectedItems) {
				if (li.Tag is Loadout load) {
					var mon = Program.data.GetMonster(ulong.Parse(li.SubItems[2].Text));
					if (mon != null)
						cost += mon.SwapCost(load);
				}
			}
			toolStripStatusLabel2.Text = "Unequip: " + cost.ToString();
		}

		private void tsBtnLoadsClear_Click(object sender, EventArgs e) {
			ClearLoadouts();
		}

		private void toolStripButton7_Click(object sender, EventArgs e) {
			if (dataMonsterList.SelectedItems.Count <= 0) return;
			if (!(dataMonsterList.SelectedItems[0].Tag is Monster mon))
				return;

			var nextId = 1;
			if (Program.builds.Any())
				nextId = Program.builds.Max(q => q.ID) + 1;

			Build bb = new Build(mon)
			{
				New = true,
				ID = nextId,
				MonId = mon.Id,
				MonName = mon.FullName
			};

			if (Program.Settings.ShowBuildWizard) {
				using (var bwiz = new BuildWizard(bb)) {
					var res = bwiz.ShowDialog();
					if (res != DialogResult.OK) return;
				}
			}

			using (var ff = new Create(bb)) {
				while (Program.builds.Any(b => b.ID == bb.ID)) {
					bb.ID++;
				}

				var res = ff.ShowDialog();
				if (res != DialogResult.OK) return;

				if (Program.builds.Count > 0)
					bb.Priority = Program.builds.Max(b => b.Priority) + 1;
				else
					bb.Priority = 1;

				Program.builds.Add(bb);

				var lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == bb.Mon.FullName));
				if (lv1li != null)
					lv1li.ForeColor = Color.Green;
			}
		}

		private void buildList_DoubleClick(object sender, EventArgs e) {
			var items = buildList.SelectedItems;
			if (items.Count > 0) {
				var item = items[0];
				if (item.Tag != null) {
					Build bb = (Build)item.Tag;
					Monster before = bb.Mon;
					if (bb.Type == BuildType.Link) {
						bb.CopyFrom(Program.builds.FirstOrDefault(b => b.ID == bb.LinkId));
					}
					using (var ff = new Create(bb)) {
						var res = ff.ShowDialog();
						if (res == DialogResult.OK) {
							item.SubItems[0].Text = bb.Mon.FullName;
							item.SubItems[4].Text = bb.Mon.Id.ToString();
							item.ForeColor = bb.RunePrediction.Any(p => p.Value.Value) ? Color.Purple : Color.Black;
							if (bb.Mon != before) {
								// TODO: check tag?
								var lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == before.FullName));
								if (lv1li != null)
									lv1li.ForeColor = before.inStorage ? Color.Gray : Color.Black;

								lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == ff.build.Mon.FullName));
								if (lv1li != null)
									lv1li.ForeColor = Color.Green;

							}
							if (bb.Type == BuildType.Link) {
								Program.builds.FirstOrDefault(b => b.ID == bb.LinkId).CopyFrom(bb);
							}
						}
					}
				}
			}
		}

		private void tsBtnBuildsRunOne_Click(object sender, EventArgs e) {
			var lis = buildList.SelectedItems;
			if (lis.Count > 0) {
				var li = lis[0];
				RunBuild(li, Program.Settings.MakeStats);
			}
			else {
				Program.StopBuild();
			}
		}

		private void tsBtnBuildsSave_Click(object sender, EventArgs e) {
			Program.SaveBuilds();
		}

		private void tsBtnBuildsRemove_Click(object sender, EventArgs e) {
			if (MessageBox.Show("This will delete a build (not a loadout)." + Environment.NewLine + "How many minutes of work will be undone?", "Delete Build?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				return;

			var lis = buildList.SelectedItems;
			if (lis.Count > 0) {
				foreach (ListViewItem li in lis) {
					Build b = (Build)li.Tag;
					if (b != null) {
						foreach (var bb in Program.builds.Where(bu => bu.Type == BuildType.Link && bu.LinkId == b.ID)) {
							bb.Type = BuildType.Build;
						}
						Program.builds.Remove(b);
					}
				}
			}
		}

		private void tsBtnBuildsUnlock_Click(object sender, EventArgs e) {
			if (Program.data == null || Program.data.Runes == null)
				return;
			foreach (Rune r in Program.data.Runes) {
				r.Locked = false;
			}
			checkLocked();
		}

		private void tsBtnLoadsRemove_Click(object sender, EventArgs e) {
			foreach (ListViewItem li in loadoutList.SelectedItems) {
				Loadout l = (Loadout)li.Tag;

				Program.loads.Remove(l);
				//loadoutList.Items.Remove(li);
			}
			checkLocked();
		}

		private void tsBtnBuildsRunAll_Click(object sender, EventArgs e) {
			Program.RunBuilds(false, -1);
		}

		private void Main_FormClosing(object sender, FormClosingEventArgs e) {
			this.isClosing = true;
			LineLog.Info("Closing...");
			if (CheckSaveChanges() == DialogResult.Cancel) {
				e.Cancel = true;
				return;
			}
			Program.BuildsPrintTo -= Program_BuildsPrintTo;
			Program.SaveBuilds();
		}

		private void tsbUnequipAll_Click(object sender, EventArgs e) {
			if (Program.data == null)
				return;

			if (Program.data.Monsters != null)
				foreach (Monster mon in Program.data.Monsters) {
					for (int i = 1; i < 7; i++) {
						var r = mon.Current.RemoveRune(i);
						if (r != null) {
							r.Assigned = null;
							r.AssignedId = 0;
							r.AssignedName = "";
						}
					}
				}

			if (Program.data.Runes != null)
				foreach (Rune r in Program.data.Runes) {
					r.AssignedId = 0;
					r.AssignedName = "Inventory";
				}
		}

		private void tsbReloadSave_Click(object sender, EventArgs e) {
			if (File.Exists(Program.Settings.SaveLocation)) {
				Program.LoadSave(Program.Settings.SaveLocation);
				RebuildLists();
			}
		}

		private void tsBtnBuildsMoveUp_Click(object sender, EventArgs e) {
			if (buildList.SelectedItems.Count > 0) {
				foreach (ListViewItem sli in buildList.SelectedItems.OfType<ListViewItem>().Where(l => l.Tag != null).OrderBy(l => (l.Tag as Build).Priority)) {
					if (sli.Tag is Build build)
						Program.BuildPriority(build, -1);
				}

				RegenBuildList();

				buildList.Sort();
			}
		}

		private void tsBtnBuildsMoveDown_Click(object sender, EventArgs e) {
			if (buildList.SelectedItems.Count > 0) {
				foreach (ListViewItem sli in buildList.SelectedItems.OfType<ListViewItem>().Where(l => l.Tag != null).OrderByDescending(l => (l.Tag as Build).Priority)) {
					if (sli.Tag is Build build)
						Program.BuildPriority(build, 1);
				}

				RegenBuildList();

				buildList.Sort();
			}
		}

		private void useRunesCheck_CheckedChanged(object sender, EventArgs e) {
			Program.Settings.UseEquipped = ((CheckBox)sender).Checked;
			Program.Settings.Save();
		}

		void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			Invoke((MethodInvoker)delegate {
				updateBox.Visible = true;
				try {
					string result = e.Result.Replace("\r\n", "\n");
					int firstline = result.IndexOf('\n');

					Version newver = new Version((firstline != -1) ? result.Substring(0, firstline) : result);

					int ind1 = result.IndexOf('\n');

					if (result.IndexOf('\n') != -1) {
						int ind2 = result.IndexOf('\n', ind1 + 1);
						if (ind2 == -1)
							filelink = e.Result.Substring(ind1 + 1);
						else {
							filelink = e.Result.Substring(ind1 + 1, ind2 - ind1 - 1);
							whatsNewText = e.Result.Substring(ind2 + 1);
						}

					}

					Version oldver = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
					updateCurrent.Text = "Current: " + oldver.ToString();
					updateNew.Text = "New: " + newver.ToString();

					if (oldver > newver) {
						updateComplain.Text = "You hacker";
					}
					else if (oldver < newver) {
						updateComplain.Text = "Update available!";
						if (filelink != "") {
							updateDownload.Enabled = true;
							if (whatsNewText != "")
								updateWhat.Visible = true;
						}
					}
					else {
						updateComplain.Visible = false;
					}
				}
				catch (Exception ex) {
					updateComplain.Text = e.Error.ToString();
					updateComplain.Visible = false;
					Console.WriteLine(ex);
				}
			});
		}

		private void updateDownload_Click(object sender, EventArgs e) {
			if (filelink != "") {
				Process.Start(new Uri(filelink).ToString());
			}
		}

		private void updateWhat_Click(object sender, EventArgs e) {
			if (whatsNewText != "") {
				MessageBox.Show(whatsNewText, "What's New");
			}
		}

		private void tsBtnRuneStats_Click(object sender, EventArgs e) {
			Program.runeSheet.StatsExcelRunes(false);
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
			ShowOptions();
		}

		private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e) {
			updateBox.Show();
			updateComplain.Text = "Checking...";
			Task.Factory.StartNew(() => {
				using (WebClient client = new WebClient()) {
					client.DownloadStringCompleted += client_DownloadStringCompleted;
					client.DownloadStringAsync(new Uri("https://raw.github.com/Skibisky/RuneManager/master/version.txt"));
				}
			});
		}

		private void aboutToolStripMenuItem1_Click(object sender, EventArgs e) {
			var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			string oldvernum = ver.ProductVersion;

			MessageBox.Show("Rune Manager\r\nBy Skibisky\r\nVersion " + oldvernum, "About", MessageBoxButtons.OK);
		}

		private void userManualHelpToolStripMenuItem_Click(object sender, EventArgs e) {
			OpenHelp();
		}

		private void runelistSwapLocked(object sender, EventArgs e) {
			// swap the selected runes locked state
			foreach (ListViewItem li in dataRuneList.SelectedItems) {
				if (li.Tag is Rune rune) {
					if (rune.Locked) {
						rune.Locked = false;
						li.BackColor = Color.Transparent;
					}
					else {
						rune.Locked = true;
						li.BackColor = Color.Red;
					}
				}
			}

			checkLocked();
		}

		private void runetab_savebutton_click(object sender, EventArgs e) {
			Program.SaveData();
		}

		private void unequipMonsterButton_Click(object sender, EventArgs e) {
			if (Program.data?.Monsters == null)
				return;

			foreach (ListViewItem li in dataMonsterList.SelectedItems) {
				if (!(li.Tag is Monster mon))
					continue;

				for (int i = 1; i < 7; i++) {
					var r = mon.Current.RemoveRune(i);
					if (r == null)
						continue;

					r.AssignedId = 0;
					r.Assigned = null;
					r.AssignedName = "Inventory";
				}
			}
		}

		private void tsBtnLoadsLock_Click(object sender, EventArgs e) {
			foreach (ListViewItem li in loadoutList.SelectedItems) {
				Loadout l = (Loadout)li.Tag;

				foreach (Rune r in l.Runes) {
					r.Locked = true;
				}
				checkLocked();
			}
		}

		private void tsBtnBuildsResume_Click(object sender, EventArgs e) {
			Program.RunBuilds(true, -1);
			tsBtnBuildsRunOne.DropDown.Close();
		}

		private void tsBtnBuildsRunUpTo_Click(object sender, EventArgs e) {
			int selected = -1;
			if (buildList.SelectedItems.Count > 0)
				selected = (buildList.SelectedItems[0].Tag as Build).Priority;
			Program.RunBuilds(true, selected);
		}

		


		private void buildList_MouseClick(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Right) {
				teamBuild = null;
				if (buildList.FocusedItem.Bounds.Contains(e.Location)) {
					if (buildList.FocusedItem.Tag == null)
						return;

					teamBuild = buildList.FocusedItem.Tag as Build;

					teamChecking = true;

					foreach (ToolStripMenuItem tsmi in teamToolStripMenuItem.DropDownItems) {
						tsmi.Image = null;
						if (tsTeamCheck(tsmi))
							tsmi.Image = App.add;
					}
					teamChecking = false;

					menu_buildlist.Show(Cursor.Position);
				}
			}
		}

		private void buildList_SelectedIndexChanged(object sender, EventArgs e) {
			bool doColor = buildList.SelectedItems.Count == 1;
			var b1 = doColor ? buildList.SelectedItems[0].Tag as Build : null;

			if (b1 != null) {
				var llvi = loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout)?.BuildID == b1.ID);
				if (llvi != null) {
					loadoutList.SelectedItems.Clear();
					llvi.Selected = true;
					llvi.Focused = true;
				}
			}

			foreach (ListViewItem li in buildList.Items) {
				li.BackColor = Color.White;
				if (Program.Settings.ColorTeams && b1 != null && b1.Teams.Count > 0) {
					if (li.Tag is Build b2 && b2.Teams.Count > 0) {
						int close = -1;
						foreach (var t1 in b1.Teams) {
							foreach (var t2 in b2.Teams) {
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

			if (b1 != null && b1.Type == BuildType.Link) {
				var lis = buildList.Items.OfType<ListViewItem>();
				lis = lis.Where(l => l?.Tag == b1.LinkBuild);
				var li = lis.FirstOrDefault();
				li.BackColor = Color.Fuchsia;
				foreach (var lvi in buildList.Items.OfType<ListViewItem>().Where(l => {
					if (l.Tag is Build b && b.Type == BuildType.Link && b.LinkId == b1.LinkId)
						return true;
					return false;
				})) {
					lvi.BackColor = Color.Purple;
				}
			}
		}

		private void tsBtnLoadsSave_Click(object sender, EventArgs e) {
			Program.SaveLoadouts();
		}

		private void tsBtnLoadsLoad_Click(object sender, EventArgs e) {
			Program.LoadLoadouts();
			checkLocked();
		}

		private void runeDial1_DoubleClick(object sender, EventArgs e) {
			if (runeDisplay == null || runeDisplay.IsDisposed)
				runeDisplay = new RuneDisplay();
			if (!runeDisplay.Visible) {
				runeDisplay.Show(this);
				var xx = Location.X + 1105 + 8 - 271;//271, 213
				var yy = Location.Y + 49 + 208 - 213;// 8, 208 1105, 49

				runeDisplay.Location = new Point(xx, yy);
				runeDisplay.Location = new Point(Location.X + Width, Location.Y);
			}
			if (displayMon != null)
				runeDisplay.UpdateLoad(displayMon.Current);
			if (loadoutList.SelectedItems.Count == 1) {
				var ll = loadoutList.SelectedItems[0].Tag as Loadout;
				runeDisplay.UpdateLoad(ll);
			}
		}

		private void findGoodRunes_CheckedChanged(object sender, EventArgs e) {
			if (findGoodRunes.Checked && MessageBox.Show("This runs each test multiple times.\r\nThat means leaving it overnight or something.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK) {
				goodRunes = true;
			}
			else
				findGoodRunes.Checked = false;
			goodRunes = findGoodRunes.Checked;
		}

		private void tsBtnFindSpeed_Click(object sender, EventArgs e) {
			foreach (var li in buildList.Items.OfType<ListViewItem>()) {
				if (li.Tag is Build b) {
					b.RunesUseLocked = false;
					b.RunesUseEquipped = Program.Settings.UseEquipped;
					b.BuildSaveStats = false;
					b.RunesDropHalfSetStat = Program.goFast;
					b.RunesOnlyFillEmpty = Program.fillRunes;
					b.GenRunes(Program.data);
					if (b.runes.Any(rr => rr == null))
						continue;
					long c = b.runes[0].Length;
					c *= b.runes[1].Length;
					c *= b.runes[2].Length;
					c *= b.runes[3].Length;
					c *= b.runes[4].Length;
					c *= b.runes[5].Length;

					// I get about 4mil/sec, and can be bothered to wait 10 - 25 seconds
					li.SubItems[0].ForeColor = Color.Black;
					if (c > 1000 * 1000 * (long)1000 * 100) {
						li.SubItems[0].ForeColor = Color.DarkBlue;
						li.SubItems[0].BackColor = Color.Red;
					}
					else if (c > 1000 * 1000 * (long)1000 * 10) {
						li.SubItems[0].ForeColor = Color.DarkBlue;
						li.SubItems[0].BackColor = Color.OrangeRed;
					}
					else if (c > 1000 * 1000 * (long)1000 * 1) {
						li.SubItems[0].ForeColor = Color.DarkBlue;
						li.SubItems[0].BackColor = Color.Orange;
					}
					else if (c > 4000000 * 25)
						li.SubItems[0].ForeColor = Color.Red;
					else if (c > 4000000 * 10)
						li.SubItems[0].ForeColor = Color.Orange;
				}
			}
		}

		private void btnRefreshSave_Click(object sender, EventArgs e) {
			Program.LoadSave(Program.Settings.SaveLocation);
		}

		private void buildList_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e) {
			if (e.ColumnIndex == buildCHTeams.Index) {
				foreach (var lvi in buildList.Items.OfType<ListViewItem>()) {
					if (lvi.SubItems.Count > buildCHTeams.Index)
						lvi.SubItems[buildCHTeams.Index].Text = getTeamStr(lvi.Tag as Build);
				}
			}
		}

		private void tsBtnLockMon_Click(object sender, EventArgs e) {
			if (dataMonsterList.SelectedItems.Count <= 0) return;
			if (!(dataMonsterList.SelectedItems[0].Tag is Monster mon))
				return;

			var existingLock = Program.builds.FirstOrDefault(b => b.Mon == mon);
			if (existingLock != null) {
				Program.builds.Remove(existingLock);
				return;
			}


			var nextId = 1;
			if (Program.builds.Any())
				nextId = Program.builds.Max(q => q.ID) + 1;

			Build bb = new Build(mon)
			{
				New = true,
				ID = nextId,
				MonId = mon.Id,
				MonName = mon.FullName,
				Type = BuildType.Lock,
				Priority = 0,
				AllowBroken = true,
			};

			while (Program.builds.Any(b => b.ID == bb.ID)) {
				bb.ID++;
			}

			Program.builds.Add(bb);

			var lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == bb.Mon.FullName));
			if (lv1li != null)
				lv1li.ForeColor = Color.Green;
		}

		private void tsBtnLink_Click(object sender, EventArgs e) {
			if (buildList.SelectedItems.Count <= 0) return;

			if (!(buildList.SelectedItems[0].Tag is Build build ))
				return;

			if (build.Type == BuildType.Lock)
				return;
			else if (build.Type == BuildType.Link) {
				build.Type = BuildType.Build;
				ListViewItemBuild(buildList.SelectedItems[0], build);
				return;
			}

			if (buildList.SelectedItems.Count > 1) {
				if (MessageBox.Show($"Link {buildList.SelectedItems.Count - 1} builds to [{build.ID} - {build.MonName}]?", "Link Builds", MessageBoxButtons.YesNo) == DialogResult.Yes) {
					foreach (var lvi in buildList.SelectedItems.OfType<ListViewItem>().Skip(1)) {
						var b = lvi.Tag as Build;
						if (b != null && b != build) {
							b.Type = BuildType.Link;
							b.LinkId = build.ID;
							b.LinkBuild = build;
							ListViewItemBuild(lvi, b);
						}
					}
				}
				return;
			}
			var nextId = 1;
			if (Program.builds.Any())
				nextId = Program.builds.Max(q => q.ID) + 1;

			Build bb = new Build(build.Mon)
			{
				New = true,
				ID = nextId,
				MonId = build.Mon.Id,
				MonName = build.Mon.FullName,
				Type = BuildType.Link,
				Priority = build.Priority,
				LinkBuild = build,
				LinkId = build.ID,
			};

			while (Program.builds.Any(b => b.ID == bb.ID)) {
				bb.ID++;
			}

			Program.builds.Add(bb);

			Program.BuildPriority(bb, 1);
		}

		private void cbGoFast_CheckedChanged(object sender, EventArgs e) {
			goFast = cbGoFast.Checked;
		}

		private void cbFillRunes_CheckedChanged(object sender, EventArgs e) {
			fillRunes = cbFillRunes.Checked;
		}

		private void tsBtnSkip_Click(object sender, EventArgs e) {
			if (buildList.SelectedItems.Count > 0) {
				var build = buildList.SelectedItems[0].Tag as Build;
				if (build != null && !Program.loads.Any(l => l.BuildID == build.ID)) {
					Program.loads.Add(new Loadout(build.Mon.Current) {
						BuildID = build.ID,
						Leader = build.Leader,
						Shrines = build.Shrines,
						Buffs = build.Buffs,
						Element = build.Mon.Element,
					});
					foreach (var r in build.Mon.Current.Runes.Where(r => r != null)) {
						r.Locked = true;
					}
					checkLocked();
				}
			}
		}
		private void ResumeTimer_Tick(object sender, EventArgs e) {
			if (resumeTime <= DateTime.MinValue) return;
			if (DateTime.Now >= resumeTime) {
				Program.RunBuilds(true, -1);
				stopResumeTimer();
			}
			else {
				var fb = Program.builds.FirstOrDefault(b => b.Best == null);
				var lvi = this.buildList.Items.OfType<ListViewItem>().FirstOrDefault(b => b.Tag == fb);
				if (lvi != null) {
					// TODO: rename build columns
					lvi.SubItems[3].Text = ">> " + (resumeTime - DateTime.Now).ToString("mm\\:ss");
				}
			}
		}

		// TODO: abstract this code into TSMI tags, check/uncheck all.
		private void In8HoursToolStripMenuItem_Click(object sender, EventArgs e) {
			if (!(sender is ToolStripMenuItem tsmi)) return;
			if (resumeTimer == null) {
				startResumeTimer(DateTime.Now.AddHours(8));
				tsmi.Checked = true;
			}
			else {
				stopResumeTimer();
				tsmi.Checked = false;
			}
		}

		private void In16HoursToolStripMenuItem_Click(object sender, EventArgs e) {
			if (!(sender is ToolStripMenuItem tsmi)) return;
			if (resumeTimer == null) {
				startResumeTimer(DateTime.Now.AddHours(16));
				tsmi.Checked = true;
			}
			else {
				stopResumeTimer();
				tsmi.Checked = false;
			}
		}

		private void In30SecondsToolStripMenuItem_Click(object sender, EventArgs e) {
			if (!(sender is ToolStripMenuItem tsmi)) return;
			if (resumeTimer == null) {
				startResumeTimer(DateTime.Now.AddSeconds(30));
				tsmi.Checked = true;
			}
			else {
				stopResumeTimer();
				tsmi.Checked = false;
			}
		}
	}

}