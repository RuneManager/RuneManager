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
        TaskCompletionSource<long> readyTcs = new TaskCompletionSource<long>();
        public Task Ready => readyTcs.Task;

        private async void Main_Load(object sender, EventArgs e) {

            Stopwatch sw = new Stopwatch();
            sw.Start();
            #region Watch collections and try loading
            Program.SaveFileTouched += Program_saveFileTouched;
            //Program.data.Runes.CollectionChanged += Runes_CollectionChanged;
            Program.OnRuneUpdate += Program_OnRuneUpdate;
            Program.OnMonsterUpdate += Program_OnMonsterUpdate;
            Program.Loads.CollectionChanged += Loads_CollectionChanged;
            Program.BuildsPrintTo += Program_BuildsPrintTo;
            Program.BuildsProgressTo += Program_BuildsProgressTo;

            buildList.Items.Add("Loading...");
            dataMonsterList.Items.Add("Loading...");

            var loadTask = Task.Run(() =>
            {
                // TODO: this is slow during profiling
#if !DEBUG
                try
#endif
                {
                    // WIZARD DATA
                    LoadSaveResult loadResult = 0;
                    do {
                        loadResult = Program.FindSave();
                        switch (loadResult) {
                            case LoadSaveResult.Success:
                                break;
                            default:
                                if (MessageBox.Show("Couldn't automatically load save.\r\nManually locate a save file?", "Load Save", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                                    loadResult = Invoke(loadSaveDialogue);
                                }
                                else {
                                    Application.Exit();
                                    return;
                                }
                                break;
                        }
                    } while (loadResult != LoadSaveResult.Success);

                    // BUILDS
                    loadResult = 0;
                    do {
                        loadResult = Program.LoadBuilds();
                        switch (loadResult) {
                            case LoadSaveResult.Failure:
                                if (MessageBox.Show("Save was invalid while loading builds.\r\nManually locate a save file?", "Load Builds", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                                    loadResult = Invoke(loadSaveDialogue);
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

                }
#if !DEBUG
                catch (Exception ex) {
                    MessageBox.Show($"Critical Error {ex.GetType()}\r\nDetails in log file.", "Error");
                    LineLog.Fatal($"Fatal during load {ex.GetType()}", ex);
                }
#endif


                #region Shrines

                ToolStripMenuItem[] shrineMenu = new ToolStripMenuItem[] { speedToolStripMenuItem, defenseToolStripMenuItem , attackToolStripMenuItem, healthToolStripMenuItem,
            waterAttackToolStripMenuItem, fireAttackToolStripMenuItem, windAttackToolStripMenuItem, lightAttackToolStripMenuItem, darkAttackToolStripMenuItem, criticalDamageToolStripMenuItem};
                this.Invoke((MethodInvoker)delegate {
                    for (int i = 0; i < 21; i++) {
                        for (int j = 0; j < Deco.ShrineStats.Length; j++) {
                            if (j < 4)
                                addShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(i * Deco.ShrineLevel[j] / 2), shrineMenu[j]);
                            else if (j < 9)
                                addShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(1 + i * Deco.ShrineLevel[j] / 2), shrineMenu[j]);
                            else
                                addShrine(Deco.ShrineStats[j], i, (int)Math.Floor(i * Deco.ShrineLevel[j] / 2), shrineMenu[j]);
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
                Program.Builds.CollectionChanged += Builds_CollectionChanged;

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

            // Add (nested) teams to the UI item
            tsTeamsAdd(teamToolStripMenuItem);

            // Add a button to clear tams
            var tsnone = new ToolStripMenuItem("(Clear)");
            tsnone.Font = new Font(tsnone.Font, FontStyle.Italic);
            tsnone.Click += tsTeamHandler;
            teamToolStripMenuItem.DropDownItems.Add(tsnone);

            if (Program.Settings.StartUpHelp)
                OpenHelp();

            if (Irene == null)
                Irene = new Irene(this);
            if (Program.Settings.ShowIreneOnStart)
                Irene.Show(this);

            await loadTask;

            this.Invoke((MethodInvoker)delegate
            {
                buildList.Sort();
            });
            sw.Stop(); 
            LineLog.Info("Main form loaded in " + sw.ElapsedMilliseconds + "ms");
            loading = false;
            readyTcs.TrySetResult(sw.ElapsedMilliseconds);
        }

        public T Invoke<T>(Func<T> getThis)
        {
            T res = default;

            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    res = getThis();
                });
            }
            else
            {
                res = getThis();
            }

            return res;
        }

        /// <summary>
        /// Update the build status value in the bottom bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e) {
            /*if (lastProg != null) {
                toolStripBuildStatus.Text = "Build Status: " + lastProg.Progress;
                //lastProg = null;
            }
            if (lastPrint != null) {
                ProgressToList(lastPrint.build, lastPrint.Message);
                //lastPrint = null;
            }*/

            if (Program.CurrentBuild?.Runner is IBuildRunner br) {
                toolStripBuildStatus.Text = $"Build Status: {br.Good:N0} ~ {br.Completed:N0} - {br.Skipped:N0}";
                ProgressToList(Program.CurrentBuild, ((double)br.Completed / (double)br.Expected).ToString("P2"));
            }
            else if (Program.CurrentBuild != null && Program.CurrentBuild.IsRunning) {
                toolStripBuildStatus.Text = $"Build Status: {(Program.CurrentBuild.BuildUsage?.passed ?? 0):N0} ~ {Program.CurrentBuild.Count:N0} - {Program.CurrentBuild.Skipped:N0}";
                ProgressToList(Program.CurrentBuild, ((double)Program.CurrentBuild.Count / (double)Program.CurrentBuild.Total).ToString("P2"));
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
            //  blockCol.Add(e);
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
                if (Program.Builds.Any(b => b.Mon == lvim.Tag as Monster)) {
                    lvim.ForeColor = Color.Green;
                }
            }
        }

        private void Loads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var l in e.NewItems.OfType<Loadout>()) {
                        var mm = Program.Builds.FirstOrDefault(b => b.ID == l.BuildID)?.Mon;
                        if (mm != null) {
                            mm.OnRunesChanged += Mm_OnRunesChanged;
                        }

                        CheckLocked();
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
                        var mm = Program.Builds.FirstOrDefault(b => b.ID == l.BuildID)?.Mon;
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
            var bb = Program.Builds.FirstOrDefault(b => b.Mon != null && b.Mon == (sender as Monster));
            if (bb == null)
                return;

            var l = Program.Loads.FirstOrDefault(lo => lo.BuildID == bb.ID);
            if (l == null)
                return;

            Invoke((MethodInvoker)delegate {
                ListViewItem nli = loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout).BuildID == l.BuildID) ?? new ListViewItem();
                ListViewItemLoad(nli, l);
            });
        }


        private void ListViewItemLoad(ListViewItem nli, Loadout l) {
            Build b = Program.Builds.FirstOrDefault(bu => bu.ID == l.BuildID);
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

            nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (l.RunesNew + l.RunesChanged).ToString());
            if (l.Runes.Where(r => r != null && r.IsUnassigned).Any(r => b.Mon.Runes.FirstOrDefault(ru => ru != null && ru.Slot == r.Slot) == null))
                nli.SubItems[3].ForeColor = Color.Green;
            if (b.Type == BuildType.Lock)
                nli.SubItems[0].ForeColor = Color.Gray;
            else if (b.Type == BuildType.Link)
                nli.SubItems[0].ForeColor = Color.Teal;
            if (Program.Settings.SplitAssign)
                nli.SubItems[3].Text = "+" + l.RunesNew.ToString() + " ±" + l.RunesChanged.ToString();
            nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, l.Powerup.ToString());
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
                            if (lv1li.Tag is Monster mon && mon.InStorage)
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

            var r = l?.Runes[runeDial.RuneSelected - 1];
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

        private void tsBtnMonMoveUp_Click(object sender, EventArgs e) {
            if (dataMonsterList?.FocusedItem?.Tag is Monster mon) {
                int maxPri = Program.Data.Monsters.Max(x => x.Priority);
                if (mon.Priority == 0) {
                    mon.Priority = maxPri + 1;
                    dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
                }
                else if (mon.Priority != 1) {
                    int pri = mon.Priority;
                    Monster mon2 = Program.Data.Monsters.FirstOrDefault(x => x.Priority == pri - 1);
                    if (mon2 != null) {
                        ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.FullName);
                        mon2.Priority += 1;
                        listMon.SubItems[colMonPriority.Index].Text = mon2.Priority.ToString();
                    }
                    mon.Priority -= 1;
                    dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (mon.Priority).ToString();
                }
                dataMonsterList.Sort();
            }
        }

        private void tsBtnMonMoveDown_Click(object sender, EventArgs e) {
            if (dataMonsterList?.FocusedItem?.Tag is Monster mon) {
                int maxPri = Program.Data.Monsters.Max(x => x.Priority);
                if (mon.Priority == 0) {
                    mon.Priority = maxPri + 1;
                    dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
                }
                else if (mon.Priority != maxPri) {
                    int pri = mon.Priority;
                    Monster mon2 = Program.Data.Monsters.FirstOrDefault(x => x.Priority == pri + 1);
                    if (mon2 != null) {
                        ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.FullName);
                        mon2.Priority -= 1;
                        listMon.SubItems[colMonPriority.Index].Text = mon2.Priority.ToString();
                    }
                    mon.Priority += 1;
                    dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (mon.Priority).ToString();
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

                    var build = Program.Builds.FirstOrDefault(b => b.ID == bid);

                    Monster mon = null;
                    if (build == null)
                        mon = Program.Data.GetMonster(monid);
                    else
                        mon = build.Mon;

                    ShowMon(mon, load.GetStats(mon));

                    ShowStats(load.GetStats(mon), mon);

                    ShowLoadout(load);

                    var dmon = Program.Data.GetMonster(monid);
                    if (dmon != null) {
                        var dmonld = dmon.Current.Leader;
                        var dmonsh = dmon.Current.Shrines;
                        var dmongu = dmon.Current.Guild;
                        var dmonbu = dmon.Current.Buffs;
                        dmon.Current.Leader = load.Leader;
                        dmon.Current.Shrines = load.Shrines;
                        dmon.Current.Guild = load.Guild;
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
                        dmon.Current.Guild = dmongu;
                        dmon.Current.Buffs = dmonbu;
                        dmon.Current.FakeLevel = dmonfl;
                        dmon.Current.PredictSubs = dmonps;

                    }
                }
                lastFocused = loadoutList.FocusedItem;
            }
            else {
                ShowMon(null);
                ShowLoadout(null);
                ShowDiff(null, null);
            }

            int cost = 0;
            foreach (ListViewItem li in loadoutList.SelectedItems) {
                if (li.Tag is Loadout load) {
                    var mon = Program.Data.GetMonster(ulong.Parse(li.SubItems[2].Text));
                    if (mon != null)
                        cost += mon.SwapCost(load);
                }
            }
            toolStripStatusLabel2.Text = "Unequip: " + cost.ToString();
        }

        private void tsBtnLoadsClear_Click(object sender, EventArgs e) {
            var total = TimeSpan.FromMilliseconds(Program.Loads.Sum(l => l.Time));
            if (MessageBox.Show("Delete *all* loadouts?\r\nThey took " + total.ToString(@"hh\:mm\:ss") + " to generate." , "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                ClearLoadouts();

            }
        }

        private void tsBtnMonMakeBuild_Click(object sender, EventArgs e) {
            if (dataMonsterList.SelectedItems.Count <= 0) return;
            if (!(dataMonsterList.SelectedItems[0].Tag is Monster mon))
                return;

            var nextId = 1;
            if (Program.Builds.Any())
                nextId = Program.Builds.Max(q => q.ID) + 1;

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
                while (Program.Builds.Any(b => b.ID == bb.ID)) {
                    bb.ID++;
                }

                var res = ff.ShowDialog();
                if (res != DialogResult.OK) return;

                if (Program.Builds.Count > 0)
                    bb.Priority = Program.Builds.Max(b => b.Priority) + 1;
                else
                    bb.Priority = 1;

                Program.Builds.Add(bb);

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
                        bb.CopyFrom(Program.Builds.FirstOrDefault(b => b.ID == bb.LinkId));
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
                                    lv1li.ForeColor = before.InStorage ? Color.Gray : Color.Black;

                                lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == ff.Build.Mon.FullName));
                                if (lv1li != null)
                                    lv1li.ForeColor = Color.Green;

                            }
                            if (bb.Type == BuildType.Link) {
                                Program.Builds.FirstOrDefault(b => b.ID == bb.LinkId).CopyFrom(bb);
                            }
                        }
                    }
                }
            }
        }

        private void tsBtnBuildsRunOne_Click(object sender, EventArgs e) {
            var lis = buildList.SelectedItems;
            if (lis.Count == 0)
                MessageBox.Show("Please select a build to run.", "No Build Selected", MessageBoxButtons.OK);
            else if (Program.HasActiveBuild)
                Program.StopBuild();
            else if (resumeTimer != null)
                stopResumeTimer();
            else
            {
                var li = lis[0];
                RunBuild(li, Program.Settings.MakeStats);
                tsBtnBuildsRun.DropDown.Close();
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
                        foreach (var bb in Program.Builds.Where(bu => bu.Type == BuildType.Link && bu.LinkId == b.ID)) {
                            bb.Type = BuildType.Build;
                        }
                        Program.Builds.Remove(b);
                    }
                }
            }
        }

        private void tsBtnBuildsUnlock_Click(object sender, EventArgs e) {
            if (Program.Data == null || Program.Data.Runes == null)
                return;
            foreach (Rune r in Program.Data.Runes) {
                r.UsedInBuild = false;
            }
            CheckLocked();
        }

        private void tsBtnLoadsRemove_Click(object sender, EventArgs e) {
            foreach (ListViewItem li in loadoutList.SelectedItems) {
                Loadout l = (Loadout)li.Tag;
                Program.RemoveLoad(l);
            }
            CheckLocked();
        }

        private void tsBtnBuildsRunAll_Click(object sender, EventArgs e) {
            if (Program.HasActiveBuild)
                Program.StopBuild();
            else if (resumeTimer != null)
                stopResumeTimer();
            else
                Program.RunBuilds(true, -1);
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
            if (Program.Data == null)
                return;

            if (Program.Data.Monsters != null)
                foreach (Monster mon in Program.Data.Monsters) {
                    for (int i = 1; i < 7; i++) {
                        var r = mon.Current.RemoveRune(i);
                        if (r != null) {
                            r.Assigned = null;
                            r.AssignedId = 0;
                            r.AssignedName = "";
                        }
                    }
                }

            if (Program.Data.Runes != null)
                foreach (Rune r in Program.Data.Runes) {
                    r.AssignedId = 0;
                    r.AssignedName = "Inventory";
                }
        }

        private void tsbReloadSave_Click(object sender, EventArgs e) {
            if (Program.HasActiveBuild)
            {
                MessageBox.Show("Cannot refresh file data while builds are running to avoid corruption.  Pleast stop the running build and try again.", "Error", MessageBoxButtons.OK);
            }
            else if (File.Exists(Program.Settings.SaveLocation)) {
                Program.LoadSave(Program.Settings.SaveLocation);
                RebuildLists();
                RefreshLoadouts();
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
            Program.RuneSheet.StatsExcelRunes(false);
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
                    if (rune.UsedInBuild) {
                        rune.UsedInBuild = false;
                        li.BackColor = Color.Transparent;
                    }
                    else {
                        rune.UsedInBuild = true;
                        li.BackColor = Color.Red;
                    }
                }
            }

            CheckLocked();
        }

        private void runetab_savebutton_click(object sender, EventArgs e) {
            Program.SaveData();
        }

        private void unequipMonsterButton_Click(object sender, EventArgs e) {
            if (Program.Data?.Monsters == null)
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
                l.Lock();
                CheckLocked();
            }
        }

        /// <summary>
        /// Run monsters up to (and including) the selected monster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnBuildsRunUpTo_Click(object sender, EventArgs e) {
            if (buildList.SelectedItems.Count == 0)
                MessageBox.Show("Please select a monster to run to.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else if (Program.HasActiveBuild)
                Program.StopBuild();
            else if (resumeTimer != null)
                stopResumeTimer();
            else
            {
                var selected = (buildList.SelectedItems[0].Tag as Build).Priority;
                Program.RunBuilds(true, selected);
                tsBtnBuildsRun.DropDown.Close();
            }
        }

        private void buildList_MouseClick(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                teamBuild = null;
                if (buildList.FocusedItem.Bounds.Contains(e.Location)) {
                    if (buildList.FocusedItem.Tag == null)
                        return;

                    teamBuild = buildList.FocusedItem.Tag as Build;

                    // update the nested tool menu based on the selected build's teams
                    teamChecking = true;
                    tsTeamCheck(teamToolStripMenuItem);
                    teamChecking = false;

                    menu_buildlist.Show(Cursor.Position);
                }
            }
        }

        /// <summary>
        /// Colorize builds and loadoust based on their relationship to the selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buildList_SelectedIndexChanged(object sender, EventArgs e) {
            // don't colorize if more than one build are selected
            // don't return because we may need to clear previous colorization
            bool doColor = buildList.SelectedItems.Count == 1;
            var b1 = doColor ? buildList.SelectedItems[0].Tag as Build : null;

            // Highlight the related loadout
            if (b1 != null) {
                var llvi = loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout)?.BuildID == b1.ID);
                if (llvi != null) {
                    loadoutList.SelectedItems.Clear();
                    // one of these triggers cascading updates
                    llvi.Selected = true;
                    llvi.Focused = true;
                }
            }

            // TODO: Highlight the monster in the left column

            // color monters based on the relationship between their teams
            foreach (ListViewItem li in buildList.Items) {
                // default to white
                li.BackColor = Color.White;
                // conditions requiring no additional colorization
                if (!Program.Settings.ColorTeams || b1 == null || b1.Teams.Count == 0)
                    continue;
                if (li.Tag == null)
                    continue;
                Build b2 = li.Tag as Build;
                if (b2.Teams.Count == 0)
                    continue;

                // determine closest pair of teams between both builds
                int closest = int.MaxValue;
                foreach (var t1 in b1.Teams) {
                    foreach (var t2 in b2.Teams) {
                        var c = GetRel(t1, t2);
                        if (c != -1)
                            closest = Math.Min(c, closest);
                        if (closest == 0)
                            break;
                    }
                    if (closest == 0)
                        break;
                }

                // colorize based on closeness
                if (closest == 0)
                    li.BackColor = Color.Lime;
                else if (closest == 1)
                    li.BackColor = Color.LightGreen;
                else if (closest == 2)
                    li.BackColor = Color.LightGray;
                else if (closest == 3)
                    li.BackColor = Color.DimGray;
            }

            // color linked builds
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
            CheckLocked();
        }

        /// <summary>
        /// Toggle the large rune display window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runeDial1_DoubleClick(object sender, EventArgs e) {
            if (RuneDisplay == null || RuneDisplay.IsDisposed)
                RuneDisplay = new RuneDisplay();
            if (RuneDisplay.Visible) {
                RuneDisplay.Hide();
            }
            else
            {
                RuneDisplay.Show(this);
                var xx = Location.X + 1105 + 8 - 271;//271, 213
                var yy = Location.Y + 49 + 208 - 213;// 8, 208 1105, 49

                RuneDisplay.Location = new Point(xx, yy);
                RuneDisplay.Location = new Point(Location.X + Width, Location.Y);
            }
            if (displayMon != null)
                RuneDisplay.UpdateLoad(displayMon.Current);
            if (loadoutList.SelectedItems.Count == 1) {
                var ll = loadoutList.SelectedItems[0].Tag as Loadout;
                RuneDisplay.UpdateLoad(ll);
            }
        }

        private void findGoodRunes_CheckedChanged(object sender, EventArgs e) {
            if (findGoodRunes.Checked && MessageBox.Show("This runs each test multiple times.\r\nThat means leaving it overnight or something.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK) {
                GoodRunes = true;
            }
            else
                findGoodRunes.Checked = false;
            GoodRunes = findGoodRunes.Checked;
        }

        private void tsBtnFindSpeed_Click(object sender, EventArgs e) {
            foreach (var li in buildList.Items.OfType<ListViewItem>()) {
                if (li.Tag is Build b) {
                    b.RunesUseLocked = false;
                    b.RunesUseEquipped = Program.Settings.UseEquipped;
                    b.BuildSaveStats = false;
                    b.RunesDropHalfSetStat = Program.GoFast;
                    b.RunesOnlyFillEmpty = Program.FillRunes;
                    b.GenRunes(Program.Data);
                    if (b.Runes.Any(rr => rr == null))
                        continue;
                    long c = b.Runes[0].Length;
                    c *= b.Runes[1].Length;
                    c *= b.Runes[2].Length;
                    c *= b.Runes[3].Length;
                    c *= b.Runes[4].Length;
                    c *= b.Runes[5].Length;

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

            var existingLock = Program.Builds.FirstOrDefault(b => b.Mon == mon);
            if (existingLock != null) {
                Program.Builds.Remove(existingLock);
                return;
            }


            var nextId = 1;
            if (Program.Builds.Any())
                nextId = Program.Builds.Max(q => q.ID) + 1;

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

            while (Program.Builds.Any(b => b.ID == bb.ID)) {
                bb.ID++;
            }

            Program.Builds.Add(bb);

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
            if (Program.Builds.Any())
                nextId = Program.Builds.Max(q => q.ID) + 1;

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

            while (Program.Builds.Any(b => b.ID == bb.ID)) {
                bb.ID++;
            }

            Program.Builds.Add(bb);

            Program.BuildPriority(bb, 1);
        }

        private void cbGoFast_CheckedChanged(object sender, EventArgs e) {
            GoFast = cbGoFast.Checked;
        }

        private void cbFillRunes_CheckedChanged(object sender, EventArgs e) {
            FillRunes = cbFillRunes.Checked;
        }

        private void tsBtnSkip_Click(object sender, EventArgs e) {
            if (buildList.SelectedItems.Count > 0) {
                var build = buildList.SelectedItems[0].Tag as Build;
                if (build != null && !Program.Loads.Any(l => l.BuildID == build.ID)) {
                    Program.Loads.Add(new Loadout(build.Mon.Current) {
                        BuildID = build.ID,
                        Leader = build.Leader,
                        Shrines = build.Shrines,
                        Guild = build.Guild,
                        Buffs = build.Buffs,
                        Element = build.Mon.Element,
                    });
                    build.Mon.Current.Lock();
                    CheckLocked();
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
                var fb = Program.Builds.FirstOrDefault(b => b.Best == null);
                var lvi = this.buildList.Items.OfType<ListViewItem>().FirstOrDefault(b => b.Tag == fb);
                if (lvi != null) {
                    // TODO: rename build columns
                    lvi.SubItems[3].Text = ">> " + (resumeTime - DateTime.Now).ToString("mm\\:ss");
                }
            }
        }

        // TODO: abstract this code into TSMI tags, check/uncheck all.
        private void In8HoursToolStripMenuItem_Click(object sender, EventArgs e) {
            delayedRun(8, 0, 0);
        }

        private void In16HoursToolStripMenuItem_Click(object sender, EventArgs e) {
            delayedRun(16, 0, 0);
        }

        private void In30SecondsToolStripMenuItem_Click(object sender, EventArgs e) {
            delayedRun(0, 0, 30);
        }

        private void delayedRun(int hours, int minutes, int seconds)
        {
            if (Program.HasActiveBuild)
                Program.StopBuild();
            else if (resumeTimer != null)
                stopResumeTimer();
            else
                startResumeTimer(DateTime.Now.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds));
        }
    }

}