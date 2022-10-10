using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Drawing.Text;

namespace RuneApp {


    public partial class Main : Form {
        // Here we keep all the WinForm callbacks, because of the way the Designer works
        // TODO: empty the logic into functions, instead of inside the callbacks

        BlockingCollection<EventArgs> blockCol = new BlockingCollection<EventArgs>();
        TaskCompletionSource<long> readyTcs = new TaskCompletionSource<long>();
        public Task Ready => readyTcs.Task;

        private async void Main_Load(object sender, EventArgs e) {

            // DEBUG Initialization Timer
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
                try
                {
                    // First load exported runes json file
                    LoadSaveResult loadResult = 0;
                    do {
                        loadResult = Program.FindExportedRunesJSON();
                        switch (loadResult) {
                            case LoadSaveResult.Success:
                                break;
                            default:
                                if (MessageBox.Show("Couldn't automatically load exported runes.\r\nManually locate file?\r\n(\"No\" will exit app.)", "Load exported runes", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                                    loadResult = Invoke(loadSaveDialogue);
                                }
                                else {
                                    Application.Exit();
                                    return;
                                }
                                break;
                        }
                    } while (loadResult != LoadSaveResult.Success);

                    // Then load saved builds (may be empty first time)
                    Program.LoadBuilds();
                        // we previously had some ability to load a different build.json file, if we ever want that again, we should build a specific loader dialog for it since the current one is very rune specific

                    this.Invoke((MethodInvoker)delegate {
                        RebuildLists();
                    });

                }
                catch (Exception ex) {
                    var logFileName = Program.GetLogFileName("MyFileAppender");
                    if (string.IsNullOrEmpty(logFileName)) { logFileName = "log file"; }
                    MessageBox.Show($"Critical Error {ex.GetType()}\r\nDetails in {logFileName}.", "Error");
                    LineLog.Fatal($"Fatal during load {ex.GetType()}", ex);
                    throw ex;
                }


                #region Shrines

                List<KeyValuePair<string, ToolStripMenuItem>> shrineMenus = new List<KeyValuePair<string, ToolStripMenuItem>> () {
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.SPD, speedToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.DEF, defenseToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.ATK, attackToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.HP, healthToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.WATER_ATK, waterAttackToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.FIRE_ATK, fireAttackToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.WIND_ATK, windAttackToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.LIGHT_ATK, lightAttackToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.DARK_ATK, darkAttackToolStripMenuItem),
                    new KeyValuePair<string, ToolStripMenuItem>(Deco.CD, criticalDamageToolStripMenuItem),
                };

                // Create level entries (0-20) for each shrine
                this.Invoke((MethodInvoker)delegate {
                    for (int i = 0; i < 21; i++) {
                        foreach (KeyValuePair<string, ToolStripMenuItem> item in shrineMenus) {
                            addShrine(item.Key, i, Deco.ShrineStats[item.Key][i], item.Value);
                        }
                    }
                });
                #endregion

                #region Sorter
                // initialize sorters (with sane ordering)
                var sorter = new ListViewSort();
                this.Invoke((MethodInvoker)delegate {
                    dataMonsterList.ListViewItemSorter = sorter;
                    // work backwards; like clicking, the last sort is the most prominent
                    // seems to support only two levels without calling `Sort`
                    sorter.OnColumnClick(colMonID.Index); // ascending ID (so first acquired is earlier among non-prioritized units)
                    dataMonsterList.Sort();
                    sorter.OnColumnClick(colMonName.Index); // ascending name
                    dataMonsterList.Sort();
                    sorter.OnColumnClick(colMonLevel.Index, false); // descending level
                    dataMonsterList.Sort();
                    sorter.OnColumnClick(colMonGrade.Index, false); // descending grade
                    dataMonsterList.Sort();
                    sorter.OnColumnClick(colMonPriority.Index); // ascending priority
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

            #region Teams
            LineLog.Debug("Preparing teams context menu");

            // Add teams tree to the UI item
            tsTeamsAdd(teamToolStripMenuItem);

            // Add a button to clear teams
            var tsnone = new ToolStripMenuItem("(Clear)");
            tsnone.Font = new Font(tsnone.Font, FontStyle.Italic);
            tsnone.Click += tsTeamHandler;
            teamToolStripMenuItem.DropDownItems.Add(tsnone);
            #endregion

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

        ProgToEventArgs lastProg;
        PrintToEventArgs lastPrint;

        private void Main_ProgChanged(object a, ProgressChangedEventArgs b) {
            if (b.UserState is ProgToEventArgs e2) {
                SetBuildStatusBar(e2.Progress.ToString());
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

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.isClosing = true;
            LineLog.Info("Closing...");
            if (CheckSaveChanges() == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            Program.BuildsPrintTo -= Program_BuildsPrintTo;
            Program.SaveBuilds();
        }

        private void Mm_OnRunesChanged(object sender, EventArgs e)
        {
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

        private void Program_saveFileTouched(object sender, EventArgs e) {
            this.fileBox.Visible = true;
        }

        #region Menu

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // confirm maybe?
            Close();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowOptions();
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

        #endregion

        #region RuneList

        private void Runes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                default:
                    throw new NotImplementedException();
            }
        }

        private void Program_OnRuneUpdate(object sender, bool deleted)
        {
            if (sender is Rune rune)
            {
                Invoke((MethodInvoker)delegate {
                    var nli = dataRuneList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Rune).Id == rune.Id);
                    if (deleted)
                    {
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

        private void runetab_list_select(object sender, EventArgs e)
        {
            if (dataRuneList.SelectedItems.OfType<ListViewItem>().FirstOrDefault()?.Tag is Rune rune)
            {
                runeInventory.Show();
                runeInventory.SetRune(rune);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(tabRunes.Name);
            filterRunesList(x => ((Rune)x).Slot == ((Rune)runeEquipped.Tag).Slot);
        }

        private void runetab_clearfilter(object sender, EventArgs e)
        {
            filterRunesList(x => true);
        }

        private void runetab_savebutton_click(object sender, EventArgs e)
        {
            Program.SaveData();
        }

        private void runelistSwapLocked(object sender, EventArgs e)
        {
            // swap the selected runes locked state
            foreach (ListViewItem li in dataRuneList.SelectedItems)
            {
                if (li.Tag is Rune rune)
                {
                    if (rune.UsedInBuild)
                    {
                        rune.UsedInBuild = false;
                        li.BackColor = Color.Transparent;
                    }
                    else
                    {
                        rune.UsedInBuild = true;
                        li.BackColor = Color.Red;
                    }
                }
            }

            ColorizeRuneList();
        }

        #endregion

        #region CraftList

        private void crafttab_list_select(object sender, EventArgs e)
        {
            if (dataCraftList.SelectedItems.OfType<ListViewItem>().FirstOrDefault()?.Tag is Craft craft)
            {
                runeInventory.Show();
                runeInventory.SetCraft(craft);
            }
        }

        #endregion

        #region RuneInventoryDisplay

        private void lbCloseInventory_Click(object sender, EventArgs e)
        {
            runeInventory.Hide();
        }

        #endregion

        #region MonsterList

        private void ColorMonsWithBuilds()
        {
            foreach (ListViewItem lvim in dataMonsterList.Items)
            {
                if (Program.Builds.Any(b => b.Mon == lvim.Tag as Monster))
                {
                    lvim.ForeColor = Color.Green;
                }
            }
        }

        private void Program_OnMonsterUpdate(object sender, bool deleted)
        {
            if (sender is Monster mon)
            {
                Invoke((MethodInvoker)delegate {
                    var nli = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Monster).Id == mon.Id);
                    if (deleted)
                    {
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

        private void monstertab_list_select(object sender, EventArgs e)
        {
            if (dataMonsterList?.FocusedItem?.Tag is Monster mon)
            {
                ShowMon(mon);
                lastFocused = null;
            }
        }

        private void tsBtnMonMoveUp_Click(object sender, EventArgs e)
        {
            if (dataMonsterList?.FocusedItem?.Tag is Monster mon)
            {
                int maxPri = Program.Data.Monsters.Max(x => x.Priority);
                if (mon.Priority == 0)
                {
                    mon.Priority = maxPri + 1;
                    dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
                }
                else if (mon.Priority != 1)
                {
                    int pri = mon.Priority;
                    Monster mon2 = Program.Data.Monsters.FirstOrDefault(x => x.Priority == pri - 1);
                    if (mon2 != null)
                    {
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

        private void tsBtnMonMoveDown_Click(object sender, EventArgs e)
        {
            if (dataMonsterList?.FocusedItem?.Tag is Monster mon)
            {
                int maxPri = Program.Data.Monsters.Max(x => x.Priority);
                if (mon.Priority == 0)
                {
                    mon.Priority = maxPri + 1;
                    dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
                }
                else if (mon.Priority != maxPri)
                {
                    int pri = mon.Priority;
                    Monster mon2 = Program.Data.Monsters.FirstOrDefault(x => x.Priority == pri + 1);
                    if (mon2 != null)
                    {
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

        private void btnRefreshSave_Click(object sender, EventArgs e)
        {
            Program.LoadExportedRunesJSON(Program.Settings.SaveLocation);
        }

        private void tsBtnLockMon_Click(object sender, EventArgs e)
        {
            if (dataMonsterList.SelectedItems.Count <= 0) return;
            if (!(dataMonsterList.SelectedItems[0].Tag is Monster mon))
                return;

            var existingLock = Program.Builds.FirstOrDefault(b => b.Mon == mon);
            if (existingLock != null)
            {
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

            while (Program.Builds.Any(b => b.ID == bb.ID))
            {
                bb.ID++;
            }

            Program.Builds.Add(bb);

            var lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == bb.Mon.FullName));
            if (lv1li != null)
                lv1li.ForeColor = Color.Green;
        }

        private void tsBtnLink_Click(object sender, EventArgs e)
        {
            if (buildList.SelectedItems.Count <= 0) return;

            if (!(buildList.SelectedItems[0].Tag is Build build))
                return;

            if (build.Type == BuildType.Lock)
                return;
            else if (build.Type == BuildType.Link)
            {
                build.Type = BuildType.Build;
                ListViewItemBuild(buildList.SelectedItems[0], build);
                return;
            }

            if (buildList.SelectedItems.Count > 1)
            {
                if (MessageBox.Show($"Link {buildList.SelectedItems.Count - 1} builds to [{build.ID} - {build.MonName}]?", "Link Builds", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (var lvi in buildList.SelectedItems.OfType<ListViewItem>().Skip(1))
                    {
                        var b = lvi.Tag as Build;
                        if (b != null && b != build)
                        {
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

            while (Program.Builds.Any(b => b.ID == bb.ID))
            {
                bb.ID++;
            }

            Program.Builds.Add(bb);

            Program.BuildPriority(bb, 1);
        }

        private void unequipMonsterButton_Click(object sender, EventArgs e)
        {
            if (Program.Data?.Monsters == null)
                return;

            foreach (ListViewItem li in dataMonsterList.SelectedItems)
            {
                if (!(li.Tag is Monster mon))
                    continue;

                for (int i = 1; i < 7; i++)
                {
                    var r = mon.Current.RemoveRune(i);
                    if (r == null)
                        continue;

                    r.AssignedId = 0;
                    r.Assigned = null;
                    r.AssignedName = "Inventory";
                }
            }
        }

        private void tsBtnMonMakeBuild_Click(object sender, EventArgs e)
        {
            if (dataMonsterList.SelectedItems.Count <= 0) return;
            if (!(dataMonsterList.SelectedItems[0].Tag is Monster mon))
                return;
            if (Program.Builds.Any(b => b.MonId == mon.Id))
            {
                MessageBox.Show("Error: Monster already has a build.");
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
                MonName = mon.FullName
            };

            if (Program.Settings.ShowBuildWizard)
            {
                using (var bwiz = new BuildWizard(bb))
                {
                    var res = bwiz.ShowDialog();
                    if (res != DialogResult.OK) return;
                }
            }

            using (var ff = new Create(bb))
            {
                while (Program.Builds.Any(b => b.ID == bb.ID))
                {
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

        #endregion

        #region BuildList

        /// <summary>
        /// Updates a specific build (line) with the status of the build
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Program_BuildsPrintTo(object sender, PrintToEventArgs e)
        {
            LineLog.Debug(e.Message, e.Line, e.Caller, e.File);
            //if (!blockCol.IsAddingCompleted)
            //  blockCol.Add(e);
            if (lastPrint == null || e.build != lastPrint.build || e.Order >= lastPrint.Order)
                lastPrint = e;
            //Application.DoEvents();
            //backgroundWorker1.RunWorkerAsync(e);
            ProgressToList(e.build, e.Message);
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
            var bli = buildList.Items.OfType<ListViewItem>().FirstOrDefault(it => it.Tag == teamBuild);
            if (bli != null)
            {
                while (bli.SubItems.Count < 6) bli.SubItems.Add("");
                bli.SubItems[5].Text = getTeamStr(teamBuild);
            }
            menu_buildlist.Close();
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
        private void buildList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // don't colorize if more than one build are selected
            // don't return because we may need to clear previous colorization
            bool doColor = buildList.SelectedItems.Count == 1;
            var b1 = doColor ? buildList.SelectedItems[0].Tag as Build : null;

            // Highlight the related loadout
            if (b1 != null)
            {
                var llvi = loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout)?.BuildID == b1.ID);
                if (llvi != null)
                {
                    loadoutList.SelectedItems.Clear();
                    // one of these triggers cascading updates
                    llvi.Selected = true;
                    llvi.Focused = true;
                }
            }

            // TODO: Highlight the monster in the left column

            // color monters based on the relationship between their teams
            foreach (ListViewItem li in buildList.Items)
            {
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
                foreach (var t1 in b1.Teams)
                {
                    foreach (var t2 in b2.Teams)
                    {
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
            if (b1 != null && b1.Type == BuildType.Link)
            {
                var lis = buildList.Items.OfType<ListViewItem>();
                lis = lis.Where(l => l?.Tag == b1.LinkBuild);
                var li = lis.FirstOrDefault();
                li.BackColor = Color.Fuchsia;
                foreach (var lvi in buildList.Items.OfType<ListViewItem>().Where(l => {
                    if (l.Tag is Build b && b.Type == BuildType.Link && b.LinkId == b1.LinkId)
                        return true;
                    return false;
                }))
                {
                    lvi.BackColor = Color.Purple;
                }
            }
        }

        private void buildList_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex == buildCHTeams.Index)
            {
                foreach (var lvi in buildList.Items.OfType<ListViewItem>())
                {
                    if (lvi.SubItems.Count > buildCHTeams.Index)
                        lvi.SubItems[buildCHTeams.Index].Text = getTeamStr(lvi.Tag as Build);
                }
            }
        }

        private void tsBtnBuildsMoveUp_Click(object sender, EventArgs e)
        {
            if (buildList.SelectedItems.Count > 0)
            {
                foreach (ListViewItem sli in buildList.SelectedItems.OfType<ListViewItem>().Where(l => l.Tag != null).OrderBy(l => (l.Tag as Build).Priority))
                {
                    if (sli.Tag is Build build)
                        Program.BuildPriority(build, -1);
                }

                RegenBuildList();

                buildList.Sort();
            }
        }

        private void tsBtnBuildsMoveDown_Click(object sender, EventArgs e)
        {
            if (buildList.SelectedItems.Count > 0)
            {
                foreach (ListViewItem sli in buildList.SelectedItems.OfType<ListViewItem>().Where(l => l.Tag != null).OrderByDescending(l => (l.Tag as Build).Priority))
                {
                    if (sli.Tag is Build build)
                        Program.BuildPriority(build, 1);
                }

                RegenBuildList();

                buildList.Sort();
            }
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
                    Build b = (Build)li.Tag;
                    if (b != null)
                    {
                        foreach (var bb in Program.Builds.Where(bu => bu.Type == BuildType.Link && bu.LinkId == b.ID))
                        {
                            bb.Type = BuildType.Build;
                        }
                        Program.Builds.Remove(b);
                    }
                }
            }
        }

        private void tsBtnBuildsUnlock_Click(object sender, EventArgs e)
        {
            if (Program.Data == null || Program.Data.Runes == null)
                return;
            foreach (Rune r in Program.Data.Runes)
            {
                r.UsedInBuild = false;
            }
            ColorizeRuneList();
        }

        private void tsBtnSkip_Click(object sender, EventArgs e)
        {
            if (buildList.SelectedItems.Count > 0)
            {
                var build = buildList.SelectedItems[0].Tag as Build;
                if (build != null && !Program.Loads.Any(l => l.BuildID == build.ID))
                {
                    Program.Loads.Add(new Loadout(build.Mon.Current)
                    {
                        BuildID = build.ID,
                        Leader = build.Leader,
                        Shrines = build.Shrines,
                        Guild = build.Guild,
                        Buffs = build.Buffs,
                        Element = build.Mon.Element,
                    });
                    build.Mon.Current.Lock();
                    ColorizeRuneList();
                }
            }
        }

        private void tsBtnBuildsSave_Click(object sender, EventArgs e)
        {
            Program.SaveBuilds();
        }

        private void tsbUnequipAll_Click(object sender, EventArgs e)
        {
            if (Program.Data == null)
                return;

            if (Program.Data.Monsters != null)
                foreach (Monster mon in Program.Data.Monsters)
                {
                    for (int i = 1; i < 7; i++)
                    {
                        var r = mon.Current.RemoveRune(i);
                        if (r != null)
                        {
                            r.Assigned = null;
                            r.AssignedId = 0;
                            r.AssignedName = "";
                        }
                    }
                }

            if (Program.Data.Runes != null)
                foreach (Rune r in Program.Data.Runes)
                {
                    r.AssignedId = 0;
                    r.AssignedName = "Inventory";
                }
        }

        private void tsbReloadSave_Click(object sender, EventArgs e)
        {
            if (Program.HasActiveBuild)
            {
                MessageBox.Show("Cannot refresh file data while builds are running to avoid corruption.  Pleast stop the running build and try again.", "Error", MessageBoxButtons.OK);
            }
            else if (File.Exists(Program.Settings.SaveLocation))
            {
                Program.LoadExportedRunesJSON(Program.Settings.SaveLocation);
                RebuildLists();
                RefreshLoadouts();
            }
        }

        private void buildList_DoubleClick(object sender, EventArgs e)
        {
            var items = buildList.SelectedItems;
            if (items.Count > 0)
            {
                var item = items[0];
                if (item.Tag != null)
                {
                    Build bb = (Build)item.Tag;
                    Monster before = bb.Mon;
                    if (bb.Type == BuildType.Link)
                    {
                        bb.CopyFrom(Program.Builds.FirstOrDefault(b => b.ID == bb.LinkId));
                    }
                    using (var ff = new Create(bb))
                    {
                        var res = ff.ShowDialog();
                        if (res == DialogResult.OK)
                        {
                            item.SubItems[0].Text = bb.Mon.FullName;
                            item.SubItems[4].Text = bb.Mon.Id.ToString();
                            item.ForeColor = bb.RunePrediction.Any(p => p.Value.Value) ? Color.Purple : Color.Black;
                            if (bb.Mon != before)
                            {
                                // TODO: check tag?
                                var lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == before.FullName));
                                if (lv1li != null)
                                    lv1li.ForeColor = before.InStorage ? Color.Gray : Color.Black;

                                lv1li = dataMonsterList.Items.OfType<ListViewItem>().FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == ff.Build.Mon.FullName));
                                if (lv1li != null)
                                    lv1li.ForeColor = Color.Green;

                            }
                            if (bb.Type == BuildType.Link)
                            {
                                Program.Builds.FirstOrDefault(b => b.ID == bb.LinkId).CopyFrom(bb);
                            }
                        }
                    }
                }
            }
        }

        private void tsBtnBuildsRunOne_Click(object sender, EventArgs e)
        {
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

        /// <summary>
        /// Run monsters up to (and including) the selected monster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnBuildsRunUpTo_Click(object sender, EventArgs e)
        {
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

        private void tsBtnBuildsRunAll_Click(object sender, EventArgs e)
        {
            if (Program.HasActiveBuild)
                Program.StopBuild();
            else if (resumeTimer != null)
                stopResumeTimer();
            else
                Program.RunBuilds(true, -1);
        }

        // TODO: abstract this code into TSMI tags, check/uncheck all.
        private void In8HoursToolStripMenuItem_Click(object sender, EventArgs e)
        {
            delayedRun(8, 0, 0);
        }

        private void In16HoursToolStripMenuItem_Click(object sender, EventArgs e)
        {
            delayedRun(16, 0, 0);
        }

        private void In30SecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            delayedRun(0, 0, 30);
        }

        /// <summary>
        /// Schedules the run to resume after a certain amount of time.  Implemented instead of a "pause" feature which is what's really needed.
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        private void delayedRun(int hours, int minutes, int seconds)
        {
            if (Program.HasActiveBuild)
                Program.StopBuild();
            else if (resumeTimer != null)
                stopResumeTimer();
            else
                startResumeTimer(DateTime.Now.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds));
        }

        /// <summary>
        /// Method to decide whether or not to resume builds after specified delay
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResumeTimer_Tick(object sender, EventArgs e)
        {
            if (resumeTime <= DateTime.MinValue) return;
            if (DateTime.Now >= resumeTime)
            {
                Program.RunBuilds(true, -1);
                stopResumeTimer();
            }
            else
            {
                var fb = Program.Builds.FirstOrDefault(b => b.Best == null);
                var lvi = this.buildList.Items.OfType<ListViewItem>().FirstOrDefault(b => b.Tag == fb);
                if (lvi != null)
                {
                    // TODO: rename build columns
                    lvi.SubItems[3].Text = ">> " + (resumeTime - DateTime.Now).ToString("mm\\:ss");
                }
            }
        }

        /// <summary>
        /// Highlights builds that have a large number of permustations (currently rune dial icon above builds)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsBtnFindSpeed_Click(object sender, EventArgs e)
        {
            foreach (var li in buildList.Items.OfType<ListViewItem>())
            {
                if (li.Tag is Build b)
                {
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
                    if (c > 1000 * 1000 * (long)1000 * 100)
                    {
                        li.SubItems[0].ForeColor = Color.DarkBlue;
                        li.SubItems[0].BackColor = Color.Red;
                    }
                    else if (c > 1000 * 1000 * (long)1000 * 10)
                    {
                        li.SubItems[0].ForeColor = Color.DarkBlue;
                        li.SubItems[0].BackColor = Color.OrangeRed;
                    }
                    else if (c > 1000 * 1000 * (long)1000 * 1)
                    {
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

        #endregion

        #region LoadoutList

        private void Loads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var l in e.NewItems.OfType<Loadout>())
                    {
                        // Add event handler to Mon.OnRunesChanged
                        var mm = Program.Builds.FirstOrDefault(b => b.ID == l.BuildID)?.Mon;
                        if (mm != null)
                        {
                            mm.OnRunesChanged += Mm_OnRunesChanged;
                        }
                        // Update UI
                        Invoke((MethodInvoker)delegate {
                            // update build list label to "Loaded" (if it exists)
                            var bli = buildList.Items.OfType<ListViewItem>().FirstOrDefault(bi => (bi.Tag as Build).ID == l.BuildID);
                            if (bli != null)
                            {
                                var ahh = bli.SubItems[3].Text;
                                if (ahh == "" || ahh == "!")
                                {
                                    bli.SubItems[3].Text = "Loaded";
                                }
                            }
                            // Get or create laodout item (matching on BuildID)
                            ListViewItem nli = loadoutList.Items.OfType<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout).BuildID == l.BuildID) ?? new ListViewItem();
                            ListViewItemLoad(nli, l);
                            if (!loadoutList.Items.Contains(nli))
                                loadoutList.Items.Add(nli);
                        });
                    }
                    // Colorize the Loadouts List
                    ColorizeRuneList();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    loadoutList.Items.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var l in e.OldItems.OfType<Loadout>())
                    {
                        var mm = Program.Builds.FirstOrDefault(b => b.ID == l.BuildID)?.Mon;
                        if (mm != null)
                        {
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

        /// <summary>
        /// Adds an item to the Loadout list, colorizing as approprite
        /// </summary>
        /// <param name="nli"></param>
        /// <param name="l"></param>
        private void ListViewItemLoad(ListViewItem nli, Loadout l)
        {
            Build b = Program.Builds.FirstOrDefault(bu => bu.ID == l.BuildID);
            nli.Tag = l;
            // also required when build is null
            while (nli.SubItems.Count < 6)
                nli.SubItems.Add("");
            // must suppor missing build for "load loadouts"
            if (b == null)
            {
                nli.Text = "Build Missing";
                return;
            }
            nli.Text = b.ID.ToString();
            nli.Name = b.ID.ToString();
            nli.UseItemStyleForSubItems = false;
            nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, b.MonName);
            nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, b.ID.ToString());
            nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, b.Mon.Id.ToString());
            l.RecountDiff(b.Mon.Id);

            // For colors, see http://www.flounder.com/csharp_color_table.htm
            nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (l.RunesNew + l.RunesChanged).ToString());
            // Colorize name if build is locked or linked
            if (b.Type == BuildType.Lock)
                nli.SubItems[0].ForeColor = Color.Gray;
            else if (b.Type == BuildType.Link)
                nli.SubItems[0].ForeColor = Color.Teal;
            if (Program.Settings.SplitAssign)
                nli.SubItems[3].Text = "+" + l.RunesNew.ToString() + " ±" + l.RunesChanged.ToString();
            if (Program.Settings.ColorLoadChanges)
            {
                int emptySlots = 6 - b.Mon.Runes.Count();
                List<Rune> inventoryRunes = l.Runes.Where(r => r != null && r.IsUnassigned).ToList();
                // Dim loadouts with no changes
                if (l.RunesNew + l.RunesChanged == 0) ; // no coloring exit condition
                else if (emptySlots >= l.RunesChanged)
                    // swap is inventory netural (or better)
                    nli.SubItems[3].BackColor = Color.LightGreen;
                else if (emptySlots + inventoryRunes.Count > 2)
                    // many runes can be swapped without increasing inventory
                    nli.SubItems[3].BackColor = Color.Khaki;
                else if (emptySlots + inventoryRunes.Count > 0)
                    // few runes can be swapped without increasing inventory
                    nli.SubItems[3].BackColor = Color.Bisque;
                else
                    // no runes can be swapped without increasing inventory
                    nli.SubItems[3].BackColor = Color.LightPink;
                // (LEGACY) Has Inventory Rune going into Empty Slot i.e. always free to equip
                // need to use "slot" explicitly
                if (inventoryRunes.Any(ru => !b.Mon.Runes.Any(r => r.Slot == ru.Slot)))
                    nli.SubItems[3].ForeColor = Color.Green;
            }
            nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, l.Powerup.ToString());
            // Highlight upgrades that have additional substat improvements
            if (Program.Settings.ColorLoadUpgrades && l.Powerup > 0)
                if (l.HasRunesUnder12())
                    nli.SubItems[4].BackColor = Color.LightPink;
                else
                    nli.SubItems[4].BackColor = Color.LightGreen;
            nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, (l.Time / (double)1000).ToString("0.##"));
            // Colorize long build times
            if (Program.Settings.ColorLoadTimes)
            {
                var t = l.Time / (double)1000;
                if (t > 60)
                    nli.SubItems[5].BackColor = Color.LightPink;
                else if (t > 30)
                    nli.SubItems[5].BackColor = Color.Wheat;
            }
        }

        private void loadoutlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadoutList.SelectedItems.Count == 0)
                lastFocused = null;
            // only update on changes
            if (loadoutList.FocusedItem != null && loadoutList.SelectedItems.Count == 1)
            {
                if (lastFocused == loadoutList.FocusedItem)
                    return;
                lastFocused = loadoutList.FocusedItem;

                var item = loadoutList.FocusedItem;
                if (item.Tag != null)
                {
                    Loadout load = (Loadout)item.Tag;
                    Build build = Program.Builds.FirstOrDefault(b => b.ID == load.BuildID);
                    Monster mon = null;

                    if (build != null)
                        mon = build.Mon;
                    else if (item.SubItems[2].Text != "")
                        mon = Program.Data.GetMonster(ulong.Parse(item.SubItems[2].Text));

                    if (mon != null)
                    {
                        // internally, ShowStats ShowLoadout
                        ShowMon(mon, load);
                        ShowLoadout(load, build);
                    }
                    else
                    {
                        // e.g. imported load where build has been deleted
                        ShowMon(null);
                        ShowLoadout(load);
                    }
                }
            }
            else
            {
                ShowMon(null);
                ShowLoadout(null);
            }

            // show mana cost to unequip all selected mons
            int cost = 0;
            foreach (ListViewItem li in loadoutList.SelectedItems)
            {
                if (li.Tag is Loadout load)
                {
                    if (li.SubItems[2].Text == "")
                        continue;
                    var mon = Program.Data.GetMonster(ulong.Parse(li.SubItems[2].Text));
                    if (mon != null)
                        cost += mon.SwapCost(load);
                }
            }
            toolStripStatusLabel2.Text = "Unequip: " + cost.ToString();
        }

        private void ShowLoadout(Loadout load, Build build = null) {
            if (load == null)
            {
                ShowDiff(null, null);
                return;
            }
            Monster mon = build.Mon;
            if (mon == null)
                mon = Program.Data.GetMonster(build.MonId);
            if (mon == null)
            {
                ShowDiff(null, null);
                return;
            }

            var dmonld = mon.Current.Leader;
            var dmonsh = mon.Current.Shrines;
            var dmongu = mon.Current.Guild;
            var dmonbu = mon.Current.Buffs;
            var dmonfl = mon.Current.FakeLevel;
            var dmonps = mon.Current.PredictSubs;
            mon.Current.Leader = load.Leader;
            mon.Current.Shrines = load.Shrines;
            mon.Current.Guild = load.Guild;
            mon.Current.Buffs = load.Buffs;
            mon.Current.FakeLevel = load.FakeLevel;
            mon.Current.PredictSubs = load.PredictSubs;

            if (build != null)
            {
                var beforeScore = build.CalcScore(mon.GetStats());
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
            ShowDiff(mon.GetStats(), load.GetStats(mon), build);

            mon.Current.Leader = dmonld;
            mon.Current.Shrines = dmonsh;
            mon.Current.Guild = dmongu;
            mon.Current.Buffs = dmonbu;
            mon.Current.FakeLevel = dmonfl;
            mon.Current.PredictSubs = dmonps;
        }

        private void ClearLoadoutUI()
        {
        }

        private void tsBtnLoadsClear_Click(object sender, EventArgs e)
        {
            var total = TimeSpan.FromMilliseconds(Program.Loads.Sum(l => l.Time));
            if (MessageBox.Show("Delete *all* loadouts?\r\nThey took " + total.ToString(@"hh\:mm\:ss") + " to generate.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ClearLoadouts();

            }
        }

        private void tsBtnLoadsRemove_Click(object sender, EventArgs e) {
            foreach (ListViewItem li in loadoutList.SelectedItems) {
                Loadout l = (Loadout)li.Tag;
                Program.RemoveLoad(l);
            }
            ColorizeRuneList();
        }

        private void tsBtnLoadsLock_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem li in loadoutList.SelectedItems)
            {
                Loadout l = (Loadout)li.Tag;
                l.Lock();
                ColorizeRuneList();
            }
        }

        private void tsBtnLoadsSave_Click(object sender, EventArgs e)
        {
            Program.SaveLoadouts();
        }

        private void tsBtnLoadsLoad_Click(object sender, EventArgs e)
        {
            Program.LoadLoadouts();
            ColorizeRuneList();
        }

        private void tsBtnRuneStats_Click(object sender, EventArgs e)
        {
            Program.RuneSheet.StatsExcelRunes(false);
        }

        #endregion

        #region BuildSettings

        /// <summary>
        /// Stores setting for "use equipped runes"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void useEquippedRunes_CheckedChanged(object sender, EventArgs e)
        {
            Program.Settings.UseEquipped = ((CheckBox)sender).Checked;
            Program.Settings.Save();
        }

        /// <summary>
        /// Stores setting for "find good runes"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findGoodRunes_CheckedChanged(object sender, EventArgs e)
        {
            if (findGoodRunes.Checked && MessageBox.Show("This runs each test multiple times.\r\nThat means leaving it overnight or something.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                GoodRunes = true;
            }
            else
                findGoodRunes.Checked = false;
            GoodRunes = findGoodRunes.Checked;
        }

        /// <summary>
        /// Stores setting for "go fast and break things"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGoFast_CheckedChanged(object sender, EventArgs e)
        {
            GoFast = cbGoFast.Checked;
        }

        /// <summary>
        /// Stores settings for "Fill Only" i.e. don't remove any runes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFillOnly_CheckedChanged(object sender, EventArgs e)
        {
            FillRunes = cbFillRunes.Checked;
        }

        #endregion

        #region Version

        /// <summary>
        /// Extract update data from the Github API response event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Invoke((MethodInvoker)delegate {
                updateBox.Visible = true;
                try
                {
                    dynamic payload = JsonConvert.DeserializeObject(e.Result);

                    // version number (v#.#.#)
                    string tag = payload.tag_name;
                    string result = tag.Replace("v", "");
                    Version newver = new Version(result);
                    updateNew.Text = "New: " + newver.ToString();

                    // what's new link
                    whatsNewLink = payload.html_url;
                    filelink = payload.assets[0].browser_download_url;

                    // local version
                    Version oldver = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                    updateCurrent.Text = "Current: " + oldver.ToString();

                    // update UI based on versions
                    if (oldver > newver)
                    {
                        updateComplain.Text = "You hacker";
                    }
                    else if (oldver < newver)
                    {
                        updateComplain.Text = "Update available!";
                        if (filelink != "")
                        {
                            updateDownload.Enabled = true;
                            if (whatsNewLink != "")
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

        /// <summary>
        /// Trigger async call to get Newest Release data from Github API
        /// </summary>
        private void checkForUpdates()
        {
            updateBox.Show();
            updateComplain.Text = "Checking...";
            updateComplain.Show();
            Task.Factory.StartNew(() => {
                using (WebClient client = new WebClient())
                {
                    LineLog.Info("Checking for updates");
                    client.DownloadStringCompleted += client_DownloadStringCompleted;
                    client.Headers.Add(HttpRequestHeader.UserAgent, "RuneManager");
                    client.DownloadStringAsync(new Uri("https://api.github.com/repos/RuneManager/RuneManager/releases/latest"));
                }
            });
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkForUpdates();
        }

        /// <summary>
        /// Prompt the default browser to download the zip version of the newest release
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateDownload_Click(object sender, EventArgs e)
        {
            if (filelink != "")
            {
                Process.Start(new Uri(filelink).ToString());
            }
        }

        /// <summary>
        /// Prompt the default browser to open the information page for the latest relase
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateWhatsNew_Click(object sender, EventArgs e)
        {
            if (whatsNewLink != "")
            {
                Process.Start(new Uri(whatsNewLink).ToString());
            }
        }

        #endregion

        #region RuneDial

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

        /// <summary>
        /// Toggle the large rune display window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void runeDial1_DoubleClick(object sender, EventArgs e)
        {
            if (RuneDisplay == null || RuneDisplay.IsDisposed)
                RuneDisplay = new RuneDisplay();
            if (RuneDisplay.Visible)
            {
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
            if (loadoutList.SelectedItems.Count == 1)
            {
                var ll = loadoutList.SelectedItems[0].Tag as Loadout;
                RuneDisplay.UpdateLoad(ll);
            }
        }

        #endregion

        #region BuildProgressBar

        /// <summary>
        /// Update the build status value in the bottom bar
        /// </summary>
        /// <param name="message"></param>
        private void SetBuildStatusBar(string message)
        {
            // if the main window is closed while a build is running, UI updates will generate errors
            // stop the build so the program can cleanup promptly
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    toolStripBuildStatus.Text = "Build Status: " + message;
                });
            }
            catch (System.InvalidOperationException e)
            {
                Program.StopBuild();
                LineLog.Info("Ignored System.InvalidOperationException, presumably during close");
            }
            catch (System.ComponentModel.InvalidAsynchronousStateException e)
            {
                Program.StopBuild();
                LineLog.Info("Ignored System.ComponentModel.InvalidAsynchronousStateException, presumably during close");
            }
        }

        /// <summary>
        /// Updates build status based on the state in the Runner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            /*if (lastProg != null) {
                toolStripBuildStatus.Text = "Build Status: " + lastProg.Progress;
                //lastProg = null;
            }
            if (lastPrint != null) {
                ProgressToList(lastPrint.build, lastPrint.Message);
                //lastPrint = null;
            }*/

            if (Program.CurrentBuild?.Runner is IBuildRunner br)
            {
                SetBuildStatusBar($"{br.Good:N0} ~ {br.Completed:N0} - {br.Skipped:N0}");
                ProgressToList(Program.CurrentBuild, ((double)br.Completed / (double)br.Expected).ToString("P2"));
            }
            else if (Program.CurrentBuild != null && Program.CurrentBuild.IsRunning)
            {
                SetBuildStatusBar($"{(Program.CurrentBuild.BuildUsage?.passed ?? 0):N0} ~ {Program.CurrentBuild.Count:N0} - {Program.CurrentBuild.Skipped:N0}");
                ProgressToList(Program.CurrentBuild, ((double)Program.CurrentBuild.Count / (double)Program.CurrentBuild.Total).ToString("P2"));
            }
        }

        /// <summary>
        /// Wraps build stats update with a lastProg[ress]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Program_BuildsProgressTo(object sender, ProgToEventArgs e)
        {
            //ProgressToList(e.build, e.Percent.ToString("P2"));
            //backgroundWorker1.RunWorkerAsync(e);
            //blockCol.Add(e);
            if (lastProg == null || e.Progress >= lastProg.Progress)
                lastProg = e;
            //Application.DoEvents();
            SetBuildStatusBar(e.Progress.ToString());
        }

        #endregion

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e) {
            var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
            sorter.OnColumnClick(e.Column);
            ((ListView)sender).Sort();
        }
    }

}