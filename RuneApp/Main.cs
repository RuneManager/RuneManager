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
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace RuneApp
{
    public partial class Main : Form
    {
        public static Save data;
        public static List<Build> builds = new List<Build>();
        int priority = 1;
        public static bool useEquipped = false;
        string filelink = "";
        string whatsNewText = "";
        Build currentBuild = null;
        public static Configuration config = null;

        private Dictionary<string, List<ToolStripMenuItem>> shrineMap = new Dictionary<string, List<ToolStripMenuItem>>();
        readonly string[] shrineStats = new string[] { "SPD", "DEF", "ATK", "HP", "WaterATK", "FireATK", "WindATK", "LightATK", "DarkATK", "CD" };
        readonly double[] shrineLevel = new double[] { 1.5, 2, 2, 2, 2, 2, 2, 2, 2, 2.5 };

        private Task runTask = null;
        private CancellationToken runToken;
        private CancellationTokenSource runSource = null;
        bool plsDie = false;
        bool isRunning = false;
        public static Help help = null;

        public static bool makeStats = true;
        private int linkCol = 1;
        public static bool goodRunes = false;

        FileInfo excelFile = new FileInfo(@"runestats.xlsx");
        public static ExcelPackage excelPack = null;
        private bool gotExcelPack = false;
        ExcelWorksheets excelSheets = null;

        RuneDial runeDial = null;
        Monster displayMon = null;

        bool teamChecking = false;
        Build teamBuild = null;
        Dictionary<string, List<string>> toolmap = null;
        List<string> knownTeams = new List<string>();
        List<string> extraTeams = new List<string>();

        public static bool MakeStats
        {
            get
            {
                if (config == null || !config.AppSettings.Settings.AllKeys.Contains("nostats")) return makeStats;
                return UpdateMakeStats();
            }
        }

        public static bool UpdateMakeStats()
        {
            bool tstats;
            if (bool.TryParse(config.AppSettings.Settings["nostats"].Value, out tstats))
                makeStats = !tstats;
            return makeStats;
        }

        public Main()
        {
            InitializeComponent();

            #region Config
            config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            bool dontupdate = false;
            if (config != null)
            {
                // it's stored as string, what is fasted yescompare?
                if (config.AppSettings.Settings.AllKeys.Contains("useEquipped") && config.AppSettings.Settings["useEquipped"].Value == true.ToString())
                {
                    useRunesCheck.Checked = true;
                    useEquipped = true;
                }
                // this?
                if (config.AppSettings.Settings.AllKeys.Contains("noupdate"))
                    bool.TryParse(config.AppSettings.Settings["noupdate"].Value, out dontupdate);
                if (config.AppSettings.Settings.AllKeys.Contains("nostats"))
                {
                    bool tstats;
                    if (bool.TryParse(config.AppSettings.Settings["nostats"].Value, out tstats))
                        makeStats = !tstats;
                }
            }

            findGoodRunes.Enabled = MakeStats;
            if (!MakeStats)
                findGoodRunes.Checked = false;

            #endregion

            runes = new RuneControl[] { runeControl1, runeControl2, runeControl3, runeControl4, runeControl5, runeControl6 };

            # region Sorter
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

            if (!dontupdate)
            {
                Task.Factory.StartNew(() =>
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadStringCompleted += client_DownloadStringCompleted;
                        client.DownloadStringAsync(new Uri("https://raw.github.com/Skibisky/RuneManager/master/version.txt"));
                    }
                });
            }
            else
            {
                updateBox.Show();
                updateComplain.Text = "Updates Disabled";
                var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                string oldvernum = ver.ProductVersion;
                updateCurrent.Text = "Current: " + oldvernum;
                updateNew.Text = "";
            }

            #endregion

            #region Shrines

            ToolStripMenuItem[] shrineMenu = new ToolStripMenuItem[] { speedToolStripMenuItem, defenseToolStripMenuItem , attackToolStripMenuItem, healthToolStripMenuItem,
            waterAttackToolStripMenuItem, fireAttackToolStripMenuItem, windAttackToolStripMenuItem, lightAttackToolStripMenuItem, darkAttackToolStripMenuItem, criticalDamageToolStripMenuItem};
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < shrineStats.Length; j++)
                {
                    if (j < 4)
                        AddShrine(shrineStats[j], i, (int)Math.Ceiling(i * shrineLevel[j]), shrineMenu[j]);
                    else if (j < 9)
                        AddShrine(shrineStats[j], i, (int)Math.Ceiling(1 + i * shrineLevel[j]), shrineMenu[j]);
                    else
                        AddShrine(shrineStats[j], i, (int)Math.Floor(i * shrineLevel[j]), shrineMenu[j]);
                }
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
                if (data == null)
                    return;

                if (string.IsNullOrWhiteSpace(stat))
                    return;

                data.shrines[stat] = tag.Value;
                if (config != null)
                {
                    config.AppSettings.Settings.Remove("shrine" + stat);
                    if (tag.Value > 0)
                        config.AppSettings.Settings.Add("shrine" + stat, tag.Value.ToString());
                    config.Save(ConfigurationSaveMode.Modified);
                }
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (File.Exists("save.json"))
            {
                LoadFile("save.json");
            }
            if (File.Exists("builds.json"))
            {
                LoadBuilds("builds.json");
            }
            if (File.Exists("basestats.json"))
            {
                LoadMons("basestats.json");
            }

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
            int size;
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
                        size = LoadFile(file);
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
                    int maxPri = data.Monsters.Max(x => x.priority);
                    if (mon.priority == 0)
                    {
                        mon.priority = maxPri + 1;
                        dataMonsterList.FocusedItem.SubItems[ColMonPriority.Index].Text = (maxPri + 1).ToString();
                    }
                    else if (mon.priority != 1)
                    {
                        int pri = mon.priority;
                        Monster mon2 = data.Monsters.Where(x => x.priority == pri - 1).FirstOrDefault();
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
                    int maxPri = data.Monsters.Max(x => x.priority);
                    if (mon.priority == 0)
                    {
                        mon.priority = maxPri + 1;
                        dataMonsterList.FocusedItem.SubItems[ColMonPriority.Index].Text = (maxPri + 1).ToString();
                    }
                    else if (mon.priority != maxPri)
                    {
                        int pri = mon.priority;
                        Monster mon2 = data.Monsters.Where(x => x.priority == pri + 1).FirstOrDefault();
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

            if (data?.Runes == null) return;

            foreach (Rune rune in data.Runes.Where(p.Invoke))
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

                    var build = builds.FirstOrDefault(b => b.ID == bid);

                    Monster mon = null;
                    if (build == null)
                        mon = data.GetMonster(monid);
                    else
                        mon = build.mon;

                    ShowMon(mon);

                    ShowStats(load.GetStats(mon), mon);

                    ShowRunes(load.Runes);

                    var dmon = data.GetMonster(monid);
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
                    var mon = data.GetMonster(int.Parse(li.SubItems[2].Text));
                    if (mon != null)
                        cost += mon.SwapCost(load);
                }
            }
            toolStripStatusLabel2.Text = "Unequip: " + cost.ToString();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
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
                while (builds.Any(b => b.ID == bb.ID))
                {
                    bb.ID++;
                }

                ff.build = bb;
                var res = ff.ShowDialog();
                if (res != DialogResult.OK) return;

                if (builds.Count > 0)
                    ff.build.priority = builds.Max(b => b.priority) + 1;
                else
                    ff.build.priority = 1;

                ListViewItem li = new ListViewItem(new string[] { ff.build.priority.ToString(), ff.build.ID.ToString(), ff.build.mon.Name, "", ff.build.mon.ID.ToString(), "" });
                li.Tag = ff.build;
                buildList.Items.Add(li);
                builds.Add(ff.build);

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

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            plsDie = false;
            var lis = buildList.SelectedItems;
            if (lis.Count > 0)
            {
                var li = lis[0];
                Task.Factory.StartNew(() =>
                {
                    RunBuild(li, makeStats);
                });
            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            SaveBuilds();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
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

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            if (data == null || data.Runes == null)
                return;
            foreach (Rune r in data.Runes)
            {
                r.Locked = false;
            }
            checkLocked();
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
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

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            RunBuilds(false);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveBuilds();
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            if (data == null)
                return;

            if (data.Monsters != null)
                foreach (Monster mon in data.Monsters)
                {
                    for (int i = 1; i < 7; i++)
                        mon.Current.RemoveRune(i);
                }

            if (data.Runes != null)
                foreach (Rune r in data.Runes)
                {
                    r.AssignedId = 0;
                    r.AssignedName = "Inventory";
                }
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            if (File.Exists("save.json"))
            {
                LoadFile("save.json");
            }
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (buildList.SelectedItems.Count > 0)
            {
                foreach (ListViewItem sli in buildList.SelectedItems.Cast<ListViewItem>().Where(l => l.Tag != null).OrderBy(l => (l.Tag as Build).priority))
                {
                    Build build = (Build)sli.Tag;

                    var bb = builds.OrderBy(b => b.priority).ToList();
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

        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            if (buildList.SelectedItems.Count > 0)
            {
                foreach (ListViewItem sli in buildList.SelectedItems.Cast<ListViewItem>().Where(l => l.Tag != null).OrderByDescending(l => (l.Tag as Build).priority))
                {
                    Build build = (Build)sli.Tag;

                    var bb = builds.OrderBy(b => b.priority).ToList();
                    var bi = bb.FindIndex(b => b == build);

                    if (bi != builds.Max(b => b.priority) && bi + 1 < bb.Count)
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
            useEquipped = ((CheckBox)sender).Checked;
            if (config != null)
            {
                config.AppSettings.Settings.Remove("useEquipped");
                config.AppSettings.Settings.Add("useEquipped", useEquipped.ToString());
                config.Save(ConfigurationSaveMode.Modified);
            }
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

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            if (StatsExcelBind(true))
            {
                StatsExcelRunes();
                StatsExcelSave();
            }
            StatsExcelBind(true);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new Options())
            {
                f.ShowDialog();
                findGoodRunes.Enabled = MakeStats;
                if (!MakeStats)
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
            help = new Help();
            help.Show();
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
            if (!data.isModified && MessageBox.Show("Changes saved will be lost if you import a new save from the proxy.\n\n", "Save save?", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                data.isModified = true;

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
            var str = JsonConvert.SerializeObject(data);
            File.WriteAllText("save.json", str);
        }

        private void unequipMonsterButton_Click(object sender, EventArgs e)
        {
            if (data?.Monsters == null)
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

        private void toolStripButton3_Click(object sender, EventArgs e)
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

        private void build_btn_resume_Click(object sender, EventArgs e)
        {
            RunBuilds(true);
        }

        private void build_btn_resumeto_Click(object sender, EventArgs e)
        {
            int selected = -1;
            if (buildList.SelectedItems.Count > 0)
                selected = (buildList.SelectedItems[0].Tag as Build).ID;
            RunBuilds(true, selected);
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

        public int LoadMons(string fname)
        {
            string text = File.ReadAllText(fname);

            MonsterStat.monStats = JsonConvert.DeserializeObject<List<MonsterStat>>(text);

            return text.Length;
        }

        public int LoadFile(string fname)
        {
            string text = File.ReadAllText(fname);
            dataMonsterList.Items.Clear();
            dataRuneList.Items.Clear();
            listView4.Items.Clear();

            LoadJSON(text);

            checkLocked();
            return text.Length;
        }

        public int LoadBuilds(string fname)
        {
            string text = File.ReadAllText(fname);
            LoadBuildJSON(text);
            return text.Length;
        }

        public void LoadBuildJSON(string text)
        {
            try
            {
                builds = JsonConvert.DeserializeObject<List<Build>>(text);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error occurred loading Build JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_build.txt");
                File.WriteAllText("error_build.txt", e.ToString());
                return;
            }

            if (builds.Count > 0 && (data == null || data.Monsters == null))
            {
                // backup, just in case
                string destFile = Path.Combine("", string.Format("{0}.backup{1}", "builds", ".json"));
                int num = 2;
                while (File.Exists(destFile))
                {
                    destFile = Path.Combine("", string.Format("{0}.backup{1}{2}", "builds", num, ".json"));
                    num++;
                }

                File.Copy("builds.json", destFile);
                if (MessageBox.Show("Couldn't locate save.json loading builds.json.\r\nManually locate file?", "Load Builds", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    loadSaveDialogue(null, new EventArgs());
                }
                else
                {
                    MessageBox.Show("Couldn't load monster stats, builds may not work");
                }
            }

            int current_pri = 1;
            foreach (Build b in builds.OrderBy(bu => bu.priority))
            {
                if (data != null)
                {
                    var bnum = buildList.Items.Cast<ListViewItem>().Select(it => it.Tag as Build).Where(d => d.MonName == b.MonName).Count();
                    // if there is a build with this monname, maybe I have 2 mons with that name?!
                    b.mon = data.GetMonster(b.MonName, bnum + 1);
                }
                else
                {
                    b.mon = new Monster();
                    b.mon.Name = b.MonName;
                }

                int id = b.ID;
                if (b.ID == 0 || builds.Where(bu => bu != b).Select(bu => bu.ID).Any(bid => bid == b.ID))
                {
                    //id = buildList.Items.Count + 1;
                    id = 1;
                    while (builds.Any(bu => bu.ID == id))
                        id++;
                    b.ID = id;
                }
                b.priority = current_pri++;
                var teamstr = (b.Teams == null || b.Teams.Count == 0) ? "" : (b.Teams.Count > 2 ? b.Teams.Count.ToString() : b.Teams[0] + (b.Teams.Count == 2 ? ", " + b.Teams[1] : ""));
                ListViewItem li = new ListViewItem(new string[] { b.priority.ToString(), id.ToString(), b.mon.Name, "", b.mon.ID.ToString(), teamstr });
                li.Tag = b;
                buildList.Items.Add(li);

                // ask the enumerable to eat Linq. Unsure why Find(b.mon.Name, false/true) failed here.
                var lv1li = dataMonsterList.Items.Cast<ListViewItem>().Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == b.mon.ID.ToString()).Count() > 0).FirstOrDefault();
                if (lv1li != null)
                {
                    lv1li.ForeColor = Color.Green;
                }

                // upgrade builds, hopefully
                while (b.VERSIONNUM < Create.VERSIONNUM)
                {
                    switch (b.VERSIONNUM)
                    {
                        case 0: // unversioned to 1
                            b.Threshold = b.Maximum;
                            b.Maximum = new Stats();
                            break;

                    }
                    b.VERSIONNUM++;
                }
            }
        }

        public void LoadJSON(string text)
        {
            try
            {
                data = JsonConvert.DeserializeObject<Save>(text);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error occurred loading Save JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_save.txt");
                File.WriteAllText("error_save.txt", e.ToString());
                return;
            }
            foreach (Monster mon in data.Monsters)
            {
                var equipedRunes = data.Runes.Where(r => r.AssignedId == mon.ID);

                mon.inStorage = (mon.Name.IndexOf("In Storage") >= 0);
                mon.Name = mon.Name.Replace(" (In Storage)", "");

                foreach (Rune rune in equipedRunes)
                {
                    mon.ApplyRune(rune);
                    rune.AssignedName = mon.Name;
                }

                if (mon.priority == 0 && mon.Current.RuneCount > 0)
                {
                    mon.priority = priority++;
                }

                var stats = mon.GetStats();

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
            foreach (Rune rune in data.Runes)
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

            if (data.shrines == null)
                data.shrines = new Stats();

            if (config == null)
                return;

            for (int i = 0; i < shrineStats.Length; i++)
            {
                var stat = shrineStats[i];

                if (config.AppSettings.Settings.AllKeys.Contains("shrine" + stat))
                {
                    int val = 0;
                    int.TryParse(config.AppSettings.Settings["shrine" + stat].Value, out val);
                    data.shrines[stat] = val;
                    int level = (int)Math.Floor(val / shrineLevel[i]);
                    shrineMap[stat][level].Checked = true;
                }
                else
                {
                    var shrine = data.Decorations?.FirstOrDefault(d => d.Shrine.ToString() == stat);
                    if (shrine == null) continue;

                    data.shrines[stat] = Math.Ceiling(shrine.Level * shrineLevel[i]);
                    shrineMap[stat][shrine.Level].Checked = true;
                }
            }

        }

        public void checkLocked()
        {
            if (data?.Runes == null)
                return;

            Invoke((MethodInvoker)delegate
            {
                toolStripStatusLabel1.Text = "Locked: " + data.Runes.Count(r => r.Locked);
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

            RunBuild((Build)pli.Tag, saveStats, (s) => Invoke((MethodInvoker) delegate { pli.SubItems[3].Text = s; }));
        }

        private void RunBuild(Build b, bool saveStats = false, Action<string> printTo = null)
        {
            if (plsDie)
                return;

            if (currentBuild != null)
                currentBuild.isRun = false;

            if (b == null)
                return;

            while (isRunning)
            {
                plsDie = true;
                b.isRun = false;
                Thread.Sleep(100);
            }
            if (plsDie)
            {
                plsDie = false;
                return;
            }

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

            b.GenRunes(data, false, useEquipped, saveStats);
            b.shrines = data.shrines;

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

            b.GenBuilds(0, 0, (str) =>
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    printTo?.Invoke(str);
                }
            }, null, true, saveStats);

            #region Save null build
            if (b.Best == null)
            {
                Invoke((MethodInvoker)delegate
                {
                    if (saveStats)
                    {
                        if (StatsExcelBind(true))
                        {
                            StatsExcelBuild(b, b.mon, null);
                            StatsExcelSave();
                        }
                        StatsExcelBind(true);
                    }
                });
                goto finishBuild;
            }
            #endregion

            b.Best.Current.BuildID = b.ID;
            builds.Add(b);

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
                    if (r.AssignedName == "Unknown name" || r.AssignedName == "Inventory")
                        numnew++;
                    else
                        numchanged++;
                }
                powerup += Math.Max(0, (b.Best.Current.FakeLevel[r.Slot - 1] ?? 0) - r.Level);
                if (b.Best.Current.FakeLevel[r.Slot - 1] != 0)
                {
                    int tup = (int)Math.Floor(Math.Min(12, (b.Best.Current.FakeLevel[r.Slot - 1] ?? 0)) / (double)3);
                    int cup = (int)Math.Floor(Math.Min(12, r.Level) / (double)3);
                    upgrades += Math.Max(0, tup - cup);
                }
            }
            #endregion

            currentBuild = null;
            b.Time = buildTime.ElapsedMilliseconds;
            buildTime.Stop();

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
                if (config.AppSettings.Settings.AllKeys.Contains("splitassign"))
                {
                    bool check = false;
                    bool.TryParse(config.AppSettings.Settings["splitassign"].Value, out check);
                    if (check)
                        nli.SubItems[3].Text = numnew.ToString() + "/" + numchanged.ToString();
                }
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
                if (StatsExcelBind(true))
                {
                    StatsExcelBuild(b, b.mon, b.Best.Current);
                    StatsExcelSave();
                }
                StatsExcelBind(true);
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

            isRunning = false;
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
                doneIds = doneIds.Concat(new int[]{ r.ID}).ToArray();

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
            b.BanEmTemp(doneIds);

            b.GenRunes(data, false, useEquipped, true, goodRunes);

            b.GenBuilds(0, 0, (str) =>
            {
                if (!IsDisposed && IsHandleCreated)
                {
                    printTo?.Invoke(c + " " + str);
                }
            }, null, true, true, goodRunes);
        }

        private void SaveBuilds(string fname = "builds.json")
        {
            builds.Clear();

            var lbs = buildList.Items;

            foreach (ListViewItem li in lbs)
            {
                var bb = (Build)li.Tag;
                if (bb.mon.Name != "Missingno")
                {
                    if (!bb.DownloadAwake || data.GetMonster(bb.mon.Name).Name != "Missingno")
                        bb.MonName = bb.mon.Name;
                    else
                    {
                        if (data.GetMonster(bb.mon.ID).Name != "Missingno")
                            bb.MonName = data.GetMonster(bb.mon.ID).Name;
                    }
                }
                builds.Add(bb);
            }

            // only write if there are builds, may save some files
            if (builds.Count > 0)
            {
                try
                {
                    // keep a single recent backup
                    if (File.Exists(fname))
                        File.Copy(fname, fname + ".backup", true);
                    var str = JsonConvert.SerializeObject(builds);
                    File.WriteAllText(fname, str);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        private void ClearLoadouts()
        {
            foreach (ListViewItem li in loadoutList.Items)
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

        private void RunBuilds(bool skipLoaded, int runTo = -1)
        {
            if (data == null)
                return;
            if (runTask != null && runTask.Status == TaskStatus.Running)
            {
                runSource.Cancel();
                if (currentBuild != null)
                    currentBuild.isRun = false;
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

            if (makeStats)
                StatsExcelBind();

            runSource = new CancellationTokenSource();
            runToken = runSource.Token;
            runTask = Task.Factory.StartNew(() =>
            {
                if (data.Runes != null && !skipLoaded)
                {
                    foreach (Rune r in data.Runes)
                    {
                        r.Swapped = false;
                        r.ResetStats();
                    }
                }

#warning consider making it nicer by using the List<Build>
                foreach (ListViewItem li in list5)
                {
                    if (plsDie) break;
                    RunBuild(li, makeStats);
                }

                if (makeStats)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        if (!skipLoaded)
                            StatsExcelRunes();
                        try
                        {
                            StatsExcelSave();
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

        void StatsExcelClear()
        {
            if (data == null || data.Runes == null)
                return;

            FileInfo newFile = new FileInfo(@"runestats.xlsx");
            int status = 0;
            while (status != 1)
            {
                try
                {
                    newFile.Delete();
                    status = 1;
                }
                catch (Exception e)
                {
                    if (status == 0)
                    {
                        if (MessageBox.Show("Please close runestats.xlsx\r\nOr ensure you can overwrite it.", "RuneStats", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                        {
                            status = 1;
                        }
                    }
                    else
                        Console.WriteLine(e);
                }
            }

        }

        bool StatsExcelBind(bool passive = false)
        {
            if (data == null || data.Runes == null)
                return false;

            if (gotExcelPack)
                return true;

            int status = 0;
            do
            {
                try
                {
                    excelPack = new ExcelPackage(excelFile);
                    status = 1;
                }
                catch (Exception e)
                {
                    // don't care if no bind
                    if (passive)
                        return false;
                    if (status == 0)
                    {
                        if (MessageBox.Show("Please close runestats.xlsx\r\nOr ensure you can overwrite it.", "RuneStats", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                        {
                            status = 1;
                            return false;
                        }
                    }
                    else
                        Console.WriteLine(e);

                }
            }
            while (status != 1);

            excelSheets = excelPack.Workbook.Worksheets;

            var hws = excelSheets.Where(w => w.Name == "Home").FirstOrDefault();
            if (hws == null)
                hws = excelSheets.Add("Home");
            excelSheets.MoveBefore(hws.Index, 1);

            linkCol = 2;

            gotExcelPack = true;

            return true;
        }

        public void StatsExcelSave()
        {
            try
            {
                excelPack.Save();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void StatsExcelBuild(Build build, Monster mon, Loadout load)
        {
            // TODO
            Console.WriteLine("Writing build");

            ExcelWorksheet ws = null;

            int ptries = 0;
            int ind = -1;

            do
            {
                ws = excelSheets.Where(w => w.Name == mon.Name + (ptries == 0 ? "" : ptries.ToString())).FirstOrDefault();

                if (ws != null)
                {
                    bool trashit = true;
                    if (ws.Cells[1, 9].Value != null)
                    {
                        string pids = ws.Cells[1, 9].Value.ToString();
                        if (pids != "")
                        {
                            int pid = 0;
                            if (int.TryParse(pids, out pid))
                            {
                                if (pid == mon.ID)
                                    trashit = true;
                                else
                                    trashit = false;
                            }
                            else
                                trashit = true;
                        }
                        else
                            trashit = true;
                    }

                    if (trashit)
                    {
                        ind = ws.Index;
                        excelSheets.Delete(mon.Name + (ptries == 0 ? "" : ptries.ToString()));
                        ws = null;
                    }
                    else
                        ptries++;
                }
            }
            while (ws != null);

            ws = excelPack.Workbook.Worksheets.Add(mon.Name + (ptries == 0 ? "" : ptries.ToString()));
            if (ind != -1)
                excelSheets.MoveBefore(ws.Index, ind);

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

            ws.Cells[1, 9].Value = mon.ID;

            var runeSheet = excelSheets.Where(w => w.Name == "Home").FirstOrDefault();
            if (runeSheet != null)
            {
                string hl = ws.Name + "!A1";
                if (hl.IndexOf(' ') != -1)
                    hl = "'" + ws.Name + "'!A1";
                runeSheet.Cells[build.priority + 1, linkCol].Hyperlink = new ExcelHyperLink(hl, mon.Name);
                runeSheet.Cells[build.priority + 1, linkCol].Style.Font.UnderLine = true;
                runeSheet.Cells[build.priority + 1, linkCol].Style.Font.Color.SetColor(Color.Blue);

                runeSheet.Cells[build.priority + 1, linkCol + 1].Value = build.Time / (double)1000;
                runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Numberformat.Format = "0.00";
                if (build.Time / 1000 < 2)
                {
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.MediumTurquoise);
                }
                else if (build.Time / 1000 < 15)
                {
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                }
                else if (build.Time / 1000 < 60)
                {
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                }
                else
                {
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    runeSheet.Cells[build.priority + 1, linkCol + 1].Style.Fill.BackgroundColor.SetColor(Color.Red);
                }

            }
            else
            {
                Console.WriteLine("No Home sheet");
            }

            row++;

            ws.Cells[row, 2].Value = build.buildUsage.failed;
            ws.Cells[row, 3].Value = build.buildUsage.passed;

            if (build.buildUsage != null && build.buildUsage.loads != null)
                build.buildUsage.loads = build.buildUsage.loads.OrderByDescending(m => build.sort(m.GetStats())).ToList();

            double scoreav = 0;
            int c = 0;
            Stats minav = new Stats();
            foreach (var b in build.buildUsage.loads)
            {
                double sc = build.sort(b.GetStats());
                b.score = sc;
                scoreav += sc;
                minav += b.GetStats();
                foreach (var s in Build.extraNames)
                {
                    minav.ExtraSet(s, minav.ExtraGet(s) + b.GetStats().ExtraValue(s));
                }
                c++;
            }
            scoreav /= c;
            minav /= c;

            ws.Cells[row - 1, 4].Value = scoreav;

            Stats lowQ = new Stats();
            foreach (var s in Build.statEnums)
            {
                c = 0;
                foreach (var b in build.buildUsage.loads)
                {
                    if (minav[s] > b.GetStats()[s])
                    {
                        lowQ[s] += b.GetStats()[s];
                        c++;
                    }
                }
                lowQ[s] /= c;
            }
            foreach (var s in Build.extraNames)
            {
                c = 0;
                foreach (var b in build.buildUsage.loads)
                {
                    if (minav.ExtraGet(s) > b.GetStats().ExtraValue(s))
                    {
                        lowQ.ExtraSet(s, b.GetStats().ExtraValue(s));
                        c++;
                    }
                }
                lowQ[s] /= c;
            }

            Stats versus = new Stats();
            bool enough = false;

            foreach (Attr s in Build.statAll)
            {
                if (!s.HasFlag(Attr.ExtraStat))
                {
                    if (build.Minimum[s] != 0)
                    {
                        versus[s] = lowQ[s];
                        if (build.buildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.buildUsage.passed)
                        {
                            enough = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (build.Minimum.ExtraGet(s) != 0)
                    {
                        versus.ExtraSet(s, lowQ.ExtraGet(s));
                        if (build.buildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.buildUsage.passed)
                        {
                            enough = true;
                            break;
                        }
                    }
                }
            }

            if (!enough)
            {
                foreach (Attr s in Build.statAll)
                {
                    if (!s.HasFlag(Attr.ExtraStat))
                    {
                        if (build.Sort[s] != 0)
                        {
                            versus[s] = lowQ[s];
                            if (build.buildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.buildUsage.passed)
                            {
                                enough = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (build.Sort.ExtraGet(s) != 0)
                        {
                            versus.ExtraSet(s, lowQ.ExtraGet(s));
                            if (build.buildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true)) < 0.25 * build.buildUsage.passed)
                            {
                                enough = true;
                                break;
                            }
                        }
                    }
                }
            }


            ws.Cells[row, 4].Value = build.buildUsage.loads.Count(m => m.GetStats().GreaterEqual(versus, true));

            var trow = row;
            row++;

            foreach (Attr stat in Build.statAll)
            {
                ws.Cells[row, 1].Value = stat;
                if (!stat.HasFlag(Attr.ExtraStat))
                {
                    if (build.Minimum[stat] > 0 || build.Sort[stat] != 0)
                    {
                        ws.Cells[row, 2].Value = build.Minimum[stat];
                        ws.Cells[row, 3].Value = build.Sort[stat];
                        ws.Cells[row, 4].Value = versus[stat];
                    }
                }
                else
                {
                    if (build.Minimum.ExtraGet(stat) > 0 || build.Sort.ExtraGet(stat) != 0)
                    {
                        ws.Cells[row, 2].Value = build.Minimum.ExtraGet(stat);
                        ws.Cells[row, 3].Value = build.Sort.ExtraGet(stat);
                        ws.Cells[row, 4].Value = versus.ExtraGet(stat);
                    }
                }
                row++;
            }

            col = 5;
            row = trow;

            foreach (var b in build.buildUsage.loads.Take(20))
            {
                row = trow;
                ws.Cells[row, col].Value = b.score;// Build.sort(build, b.GetStats());
                if (b.score < scoreav)
                {
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                }
                row++;
                foreach (Attr stat in Build.statAll)
                {
                    if (!stat.HasFlag(Attr.ExtraStat))
                    {
                        ws.Cells[row, col].Value = b.GetStats()[stat];
                    }
                    else
                    {
                        ws.Cells[row, col].Value = b.GetStats().ExtraValue(stat);
                    }
                    row++;
                }
                col++;
            }

            row++;
            col = 1;

            StatsExcelRuneBoard(ws, ref row, ref col, build, load);
            /*
            ws.Cells[row, 2].Value = "Good Av";
            ws.Cells[row, 3].Value = "Bad Av";
            ws.Cells[row, 4].Value = "Good Mult";

            row++;
            StatsExcelRune(ws, ref row, ref col, 0, build, load);
            row++;
            StatsExcelRune(ws, ref row, ref col, -1, build, load);
            row++;
            StatsExcelRune(ws, ref row, ref col, -2, build, load);
            row++;
            for (int slot = 1; slot < 7; slot++)
            {
                StatsExcelRune(ws, ref row, ref col, slot, build, load);
                row++;
            }*/

            Console.WriteLine("Finished Writing");
        }

        public void StatsExcelRuneBoard(ExcelWorksheet ws, ref int row, ref int col, Build build, Loadout load)
        {
            // write each slots scoring weights
            // reference and gray out things which inherit
            var cmax = 1;
            var rowstat = row + 2;
            var abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            foreach (int i in new int[] {-1, -2, -3, 0, 1, 2, 3, 4, 5 })
            {
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


                var rf = build.runeFilters.ContainsKey(sslot) ? build.runeFilters[sslot] : null;

                var btest = build.runeScoring.ContainsKey(sslot) ? build.runeScoring[sslot] : new KeyValuePair<int, double?>(-1, null);
                double? test = btest.Value;
                bool isTestInherited = false;
                string testForm = null;

                if (test == null)
                {
                    if (slot > 0)
                    {
                        isTestInherited = true;
                        testForm = "=A" + (rowstat - (slot) % 2);
                        
                    }
                    else if (slot < 0)
                    {
                        isTestInherited = true;
                        testForm = "=A" + (rowstat - 2);
                    }
                }
                if (testForm == null)
                    ws.Cells[row, 1].Value = test;
                else
                    ws.Cells[row, 1].Formula = testForm;

                if (isTestInherited)
                {
                    ws.Cells[row, 1].Style.Font.Color.SetColor(Color.Gray);
                }
                ws.Cells[row, 1].Style.Numberformat.Format = "#";

                for (int j = 0; j < Build.statNames.Length; j++)
                {
                    var stat = Build.statNames[j];

                    foreach (var sstr in new string[] { "flat", "perc" })
                    {
                        if (!((sstr == "flat" && j < 4) || (sstr == "perc" && j != 3)))
                            continue;
                        
                        double? vvv = rf?.ContainsKey(stat) ?? false ? rf[stat][sstr] : null;
                        string sssForm = null;
                        if (vvv == null)
                        {
                            if (slot > 0)
                                sssForm = "=" + abc[col - 1] + (rowstat - (slot) % 2);
                            else if (slot < 0)
                                sssForm = "=" + abc[col - 1] + (rowstat - 2);
                        }
                        if (sssForm == null)
                            ws.Cells[row, col].Value = vvv;
                        else
                        {
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

            // maximum column (may need to be after writing table)

            // table
            // PTS, set, slot, primary, HP, %, ATK, %, DEF, %, SPD, CR, CD, RES, ACC
            col = 1;
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
            foreach (Attr stat in Build.statBoth)
            {
                ws.Cells[row, col].Value = stat.ToString();
                col++;
                if (col > cmax)
                    cmax = col;
            }
            row++;

            // for each rune
            // make the pts use the hardwriten values and the appropriate slot weights

            // slot 0 = global, -1 = odd, -2 even

            var used = build.runeUsage.runesUsed.Select(r => r.Key);
            var good = build.runeUsage.runesGood.Select(r => r.Key);
            var second = build.runeUsage.runesSecond.Select(r => r.Key);
            var bad = used.Except(good);

            Console.WriteLine("Used runes: " + used.Count());

            //string[] cellStrings = Enumerable.Repeat("=", 6).ToArray();
            
            foreach (var r in used.OrderByDescending(r => good.Contains(r)).ThenByDescending(r => build.ScoreRune(r, build.GetPredict(r), false)))
            {
                col = 1;
                ws.Cells[row, col].Formula = ""; // build.ScoreRune(r, build.GetPredict(r), false);
                for (int i = 0; i < Build.statBoth.Length; i++)
                {
                    var attr = Build.statBoth[i];
                    if (i != 0) ws.Cells[row, col].Formula += "+";
                    var statCell = "$" + abc[i + 5] + "$" + (rowstat + r.Slot);
                    ws.Cells[row, col].Formula += "if(" + statCell + "<>0, runesFor" + build.ID + "[[#This Row],[" + attr + "]]/" + statCell + ", 0)";
                }

                col++;
                ws.Cells[row, col].Value = r.Set;
                col++;
                ws.Cells[row, col].Value = r.Slot;
                col++;
                ws.Cells[row, col].Value = r.MainType;
                col++;
                ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                if (load?.Runes != null)
                {
                    if (load.Runes.Contains(r))
                    {
                        ws.Cells[row, col].Value = "Best";
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.MediumTurquoise);
                    }
                    else if (second.Contains(r))
                    {
                        ws.Cells[row, col].Value = "Second";
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Teal);
                    }
                    else if (good.Contains(r))
                    {
                        ws.Cells[row, col].Value = "Good";
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                    }
                    else
                    {
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    }
                }

                col++;

                foreach (Attr stat in Build.statBoth)
                {
                    var fake = build.GetPredict(r);

                    var rval = r[stat, fake, false];

                    if (rval > 0)
                    {
                        ws.Cells[row, col].Value = rval;
                    }
                    col++;
                }
                if (col > cmax)
                    cmax = col;
                row++;
            }
            
            var table = ws.Tables.Where(t => t.Name == "runesFor" + build.ID).FirstOrDefault();
            if (table == null)
                table = ws.Tables.Add(ws.Cells[rstart, 1, row - 1, cmax - 1], "runesFor" + build.ID);

            if (table.Address.Columns != cmax - 1 || table.Address.Rows != row - 1)
            {
                var start = table.Address.Start;
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

        public void StatsExcelRunes()
        {
            if (data == null || data.Runes == null || excelPack == null)
                return;

            var ws = excelSheets.Where(w => w.Name == "Runes").FirstOrDefault();
            if (ws == null)
            {
                ws = excelSheets.Add("Runes");
                excelSheets.MoveAfter(ws.Index, 1);
            }
            else
            {
                var ind = ws.Index;
                excelSheets.Delete("Runes");
                ws = excelPack.Workbook.Worksheets.Add("Runes");
                excelSheets.MoveBefore(ws.Index, ind);
            }

            int row = 1;
            int col = 1;

            List<string> colHead = new List<string>();

            // ,MType,Points,FlatPts
            foreach (var th in "Id,Grade,Set,Slot,Main,Innate,1,2,3,4,Level,Select,Rune,Type,Load,Gen,Eff,Used,CurMon,Mon,RatingScore,Keep,Action, ,HPpts,ATKpts,Pts,_,Rarity,Flats,SPD,HPP,ACC".Split(','))
            {
                colHead.Add(th);
                ws.Cells[row, col].Value = th; col++;
            }
            row++;
            col = 1;

            int cmax = 1;

            // calculate the stats
            foreach (Rune r in data.Runes)
            {
                double keep = StatsKeepRune(r, colHead);
                r.manageStats.AddOrUpdate("Keep", keep, (s, d) => keep);
            }

            int rr = 0;
            // from keepscore 1 to 100
            foreach (Rune r in data.Runes.Where(r => r.manageStats["In"] == 0).OrderBy(r => r.manageStats.GetOrAdd("Keep", 0)))
            {
                rr++;
                if (rr < data.Runes.Where(ru => ru.manageStats["In"] == 0).Count() * 0.25)
                {
                    //if (r.manageStats["Action"] == -2)
                    if (!r.manageStats.ContainsKey("Action"))
                        r.manageStats["Action"] = -2;
                }
                else if (rr < data.Runes.Where(ru => ru.manageStats["In"] == 0).Count() * 0.5)
                {
                    if (r.manageStats["Action"] == 0)
                        r.manageStats["Action"] = -3;
                }
                else if (rr < data.Runes.Where(ru => ru.manageStats["In"] == 0).Count() * 0.75)
                {
                    if (r.manageStats["Action"] == 0)
                        r.manageStats["Action"] = -1;
                }
                else
                {
                    r.manageStats["Action"] = -1;
                    if (r.Level < 6)
                        r.manageStats["Action"] = 6;
                    else if (r.Level < 9)
                        r.manageStats["Action"] = 9;
                    else if (r.Level < 12)
                        r.manageStats["Action"] = 9;
                }
            }

            foreach (Rune r in data.Runes.OrderBy(r => r.manageStats.GetOrAdd("Keep", 0)))
            {
                for (col = 1; col <= colHead.Count; col++)
                {
                    switch (colHead[col - 1])
                    {
                        case "Id":
                            ws.Cells[row, col].Value = r.ID;
                            break;
                        case "Grade":
                            ws.Cells[row, col].Value = r.Grade;
                            break;
                        case "Rarity":
                            ws.Cells[row, col].Value = r.Rarity;
                            break;
                        case "Set":
                            if (r.Rarity > 0)
                            {
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
                            ws.Cells[row, col].Value = r.MainType.ToGameString();
                            break;
                        case "Level":
                            ws.Cells[row, col].Value = r.Level;
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
                        case "Eff":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.Efficiency;
                            break;
                        case "Used":
                            switch ((int)r.manageStats.GetOrAdd("In", 0))
                            {
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
                        case "CurMon":
                            if (r.AssignedName != "Unknown name" && r.AssignedName != "Inventory")
                                ws.Cells[row, col].Value = r.AssignedName;
                            break;
                        case "Mon":
                            if (r.manageStats.GetOrAdd("Mon", -1) != -1)
                            {
                                ws.Cells[row, col].Value = data.GetMonster((int)r.manageStats["Mon"]).Name;
                            }
                            break;
                        case "Flats":
                            ws.Cells[row, col].Value = r.FlatCount();
                            break;
                        case "FlatPts":
                            ws.Cells[row, col].Style.Numberformat.Format = "[>0]0.00;";
                            break;
                        case "RatingScore":
                            ws.Cells[row, col].Value = r.RatingScore;
                            break;
                        case "Keep":
                            //ws.Cells[row, col].Value = r.manageStats.GetOrAdd("Keep", 0);
                            string fstr;
                            StatsKeepRune(r, colHead, out fstr);
                            ws.Cells[row, col].Formula = fstr;
                            break;
                        case "Action":
                            if (r.manageStats.GetOrAdd("Action", 0) == 0)
                            {
                                if (r.ScoringATK < 0.5
                                    && r.ScoringHP < 0.5
                                    && r.ScoringRune < 0.5
                                    && r.Efficiency < 0.5
                                    && r.manageStats.GetOrAdd("Keep", 0) < 40)
                                {
                                    ws.Cells[row, col].Value = "Sell";
                                }
                            }
                            else if (r.manageStats.GetOrAdd("Action", 0) > 0)
                                ws.Cells[row, col].Value = "To " + r.manageStats.GetOrAdd("Action", 0);
                            else if (r.manageStats.GetOrAdd("Action", 0) == -1)
                                ws.Cells[row, col].Value = "Keep";
                            else if (r.manageStats.GetOrAdd("Action", 0) == -2)
                                ws.Cells[row, col].Value = "Sell";
                            else if (r.manageStats.GetOrAdd("Action", 0) == -3)
                                ws.Cells[row, col].Value = "Consider";
                            break;
                        case "Main":
                            ws.Cells[row, col].Value = r.MainValue.ToString() + " " + r.MainType.ToGameString();
                            break;
                        case "Innate":
                            if (r.InnateType != Attr.Null)
                                ws.Cells[row, col].Value = r.InnateValue.ToString() + " " + r.InnateType.ToGameString();
                            break;
                        case "1":
                            if (r.Sub1Type != Attr.Null)
                                ws.Cells[row, col].Value = r.Sub1Value.ToString() + " " + r.Sub1Type.ToGameString();
                            break;
                        case "2":
                            if (r.Sub2Type != Attr.Null)
                                ws.Cells[row, col].Value = r.Sub2Value.ToString() + " " + r.Sub2Type.ToGameString();
                            break;
                        case "3":
                            if (r.Sub3Type != Attr.Null)
                                ws.Cells[row, col].Value = r.Sub3Value.ToString() + " " + r.Sub3Type.ToGameString();
                            break;
                        case "4":
                            if (r.Sub4Type != Attr.Null)
                                ws.Cells[row, col].Value = r.Sub4Value.ToString() + " " + r.Sub4Type.ToGameString();
                            break;
                        case "HPpts":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.ScoringHP;
                            break;
                        case "ATKpts":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.ScoringATK;
                            break;
                        case "Pts":
                            ws.Cells[row, col].Style.Numberformat.Format = "0.00%";
                            ws.Cells[row, col].Value = r.ScoringRune;
                            break;
                        case "SPD":
                            ws.Cells[row, col].Value = r.Speed.Value;
                            break;
                        case "HPP":
                            ws.Cells[row, col].Value = r.HealthPercent.Value;
                            break;
                        case "ACC":
                            ws.Cells[row, col].Value = r.Accuracy.Value;
                            break;

                    }
                }
                row++;
            }

            cmax = colHead.Count + 1;

            var table = ws.Tables.Where(t => t.Name == "RuneTable").FirstOrDefault();
            if (table == null)
                table = ws.Tables.Add(ws.Cells[1, 1, row - 1, cmax - 1], "RuneTable");

            if (table.Address.Columns != cmax - 1 || table.Address.Rows != row - 1)
            {
                var start = table.Address.Start;
                var newRange = string.Format("{0}:{1}", start.Address, new ExcelAddress(1, 1, row - 1, cmax - 1));

                var tableElement = table.TableXml.DocumentElement;
                tableElement.Attributes["ref"].Value = newRange;
                tableElement["autoFilter"].Attributes["ref"].Value = newRange;

            }

            table.ShowHeader = true;
            table.StyleName = "TableStyleMedium2";
            // write rune stats

        }

        double StatsKeepRune(Rune r, List<string> heads)
        {
            string t;
            return StatsKeepRune(r, heads, out t);
        }

        double StatsKeepRune(Rune r, List<string> heads, out string formula)
        {
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
            
            r.manageStats.GetOrAdd("Mon", -1);
            if (r.manageStats["In"] > 0)
            {
                var b = builds.Where(bu => bu.Best != null && bu.Best.Current.Runes.Contains(r)).FirstOrDefault();
                r.manageStats["Action"] = -1;
                if (b == null)
                {
                    r.manageStats["Priority"] = 2;
                    if (r.Slot % 2 == 0 && r.Level < 15)
                        r.manageStats["Action"] = 15;
                    if (r.Slot % 2 == 1 && r.Level < 12)
                        r.manageStats["Action"] = 12;
                }
                else
                {
                    r.manageStats["Mon"] = b.mon.ID;
                    r.manageStats["Priority"] = b.priority / (double)builds.Max(bu => bu.priority);
                    int p = b.GetPredict(r);
                    if (r.Level < p)
                        r.manageStats["Action"] = p;
                }
                keep += 10;
            }
            formula += heads.Contains("Used") ? "+if(RuneTable[[#This Row],[Used]]<>\"No\",10,0)" : ((r.manageStats["In"] > 0) ? "+10" : "");

            // TODO: skip upgrading if rune is trash

            if (r.Rarity > Math.Floor(r.Level / (double)3))
            {
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

            keep += r.Efficiency * 5;
            formula += "+" + (heads.Contains("Eff") ? "RuneTable[[#This Row],[Eff]]" : r.Efficiency.ToString()) + "*5";

            keep += r.ScoringRune * Math.Max(r.ScoringHP, r.ScoringATK) * 20;
            /**/
            formula += "+" + (heads.Contains("Pts") ? "RuneTable[[#This Row],[Pts]]" : r.ScoringRune.ToString()) + "*max("
            + (heads.Contains("HPpts") ? "RuneTable[[#This Row],[HPpts]]" : r.ScoringHP.ToString()) + "," + (heads.Contains("ATKpts") ? "RuneTable[[#This Row],[ATKpts]]" : r.ScoringATK.ToString()) + ")*20";//*/

            keep += r.Speed;
            formula += "+" + (heads.Contains("SPD") ? "RuneTable[[#This Row],[SPD]]" : r.Speed.ToString()) + "";
            keep += r.HealthPercent * 0.3;
            formula += "+" + (heads.Contains("HPP") ? "RuneTable[[#This Row],[HPP]]" : r.HealthPercent.ToString()) + "*0.3";
            keep += r.Accuracy * 0.4;
            formula += "+" + (heads.Contains("ACC") ? "RuneTable[[#This Row],[ACC]]" : r.Accuracy.ToString()) + "*0.4";

            keep += (Math.Pow(1.004, r.manageStats.GetOrAdd("Set", 0)) - 1) * 10;
            formula += "+(power(1.004, " + (heads.Contains("Select") ? "RuneTable[[#This Row],[Select]]" : r.manageStats.GetOrAdd("Set", 0).ToString()) + ")-1)*10";

            keep += (Math.Pow(1.007, r.manageStats.GetOrAdd("RuneFilt", 0)) - 1) * 10;
            formula += "+(power(1.007, " + (heads.Contains("Rune") ? "RuneTable[[#This Row],[Rune]]" : r.manageStats.GetOrAdd("RuneFilt", 0).ToString()) + ")-1)*10";

            keep += (Math.Pow(1.01, r.manageStats.GetOrAdd("TypeFilt", 0)) - 1) * 10;
            formula += "+(power(1.01, " + (heads.Contains("Type") ? "RuneTable[[#This Row],[Type]]" : r.manageStats.GetOrAdd("TypeFilt", 0).ToString()) + ")-1)*10";

            if (r.manageStats.GetOrAdd("LoadGen", 0) > 0)
            {
                keep += Math.Pow(r.manageStats.GetOrAdd("LoadFilt", 0) / r.manageStats["LoadGen"], 1.1) * 10;
            }
            /**/
            formula += "+if(" + (heads.Contains("Gen") ? "RuneTable[[#This Row],[Gen]]" : r.manageStats["LoadGen"].ToString()) + ">0,power("
            + (heads.Contains("Load") ? "RuneTable[[#This Row],[Load]]" : r.manageStats.GetOrAdd("LoadFilt", 0).ToString()) + "/"
            + (heads.Contains("Gen") ? "RuneTable[[#This Row],[Gen]]" : r.manageStats["LoadGen"].ToString()) + ",1.1)*10,0)";//*/

            r.manageStats.AddOrUpdate("Keep", keep, (s, d) => keep);
            return keep;
        }

        void StatsExcelRune(ExcelWorksheet ws, ref int row, ref int col, int slot, Build build, Loadout load)
        {
            string sslot = slot.ToString();
            if (slot == -1)
                sslot = "o";
            else if (slot == -2)
                sslot = "e";
            else if (slot == 0)
                sslot = "g";

            int wrote = 0;

            Func<int, bool> aslot = (a) => { return true; };
            if (slot > 0)
            {
                aslot = (a) => { return a == slot; };
            }
            else if (slot == -1)
            {
                aslot = (a) => { return a % 2 == 1; };
            }
            else if (slot == -2)
            {
                aslot = (a) => { return a % 2 == 0; };
            }
            var used = build.runeUsage.runesUsed.Select(r => r.Key);
            var usedSlot = used.Where(r => aslot(r.Slot));
            var good = build.runeUsage.runesGood.Select(r => r.Key);
            var goodSlot = good.Where(r => aslot(r.Slot));

            var okay = build.runeUsage.runesOkay.Select(r => r.Key);
            var okaySlot = okay.Where(r => aslot(r.Slot));
            var second = build.runeUsage.runesSecond.Select(r => r.Key);
            var secondSlot = second.Where(r => aslot(r.Slot));


            foreach (var r in usedSlot)
            {
                r.manageStats["buildScore"] = build.ScoreRune(r, build.GetPredict(r), false);
            }

            col = 5;
            ws.Cells[row, 1].Value = "Set";
            ws.Cells[row + 1, 1].Value = "Primary";
            row--;

            foreach (var r in usedSlot.OrderByDescending(r => goodSlot.Contains(r)).ThenByDescending(r => r.manageStats["buildScore"]))
            {
                if (secondSlot.Contains(r))
                {
                    ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Teal);
                }
                ws.Cells[row, col].Value = r.manageStats["buildScore"];
                row++;
                ws.Cells[row, col].Value = r.Set.ToString();
                row++;
                ws.Cells[row, col].Value = r.MainType.ToGameString();
                row--;
                row--;
                col++;
            }
            row += 3;
            col = 1;


            for (int i = 0; i < Build.statNames.Length; i++)
            {
                var stat = Build.statNames[i];

                if (i <= 3) //SPD
                {
                    var qqw = new Dictionary<string, RuneFilter>();
                    bool writeIt = false;
                    if (build.runeFilters.TryGetValue(sslot, out qqw))
                    {
                        if (qqw.ContainsKey(stat))
                        {
                            writeIt = qqw[stat].Flat > 0;
                        }
                    }

                    if (true)
                    {
                        WriteRune(ws, ref row, ref col, stat, "", "flat", load, build, slot);
                        row++;
                        wrote++;
                    }
                    col = 1;
                }
                if (i != 3)
                {
                    var qqw = new Dictionary<string, RuneFilter>();
                    bool writeIt = false;
                    if (build.runeFilters.TryGetValue(sslot, out qqw))
                    {
                        if (qqw.ContainsKey(stat))
                        {
                            writeIt = qqw[stat].Percent > 0;
                        }
                    }

                    if (true)
                    {
                        WriteRune(ws, ref row, ref col, stat, " %", "perc", load, build, slot);
                        row++;
                        wrote++;
                    }
                    col = 1;
                }
            }
            if (wrote > 0)
            {
                if (slot == 0)
                    ws.Cells[row - wrote - 3, col].Value = "Global";
                else if (slot == -1)
                    ws.Cells[row - wrote - 3, col].Value = "Odds";
                else if (slot == -2)
                    ws.Cells[row - wrote - 3, col].Value = "Evens";
                else
                    ws.Cells[row - wrote - 3, col].Value = slot;
            }
        }

        void WriteRune(ExcelWorksheet ws, ref int row, ref int col, string stat, string hpref, string ssuff, Loadout load, Build build, int slot)
        {
            // slot 0 = global, -1 = odd, -2 even
            Func<int, bool> aslot = (a) => { return true; };
            string sslot = "g";
            if (slot > 0)
            {
                aslot = (a) => { return a == slot; };
                sslot = slot.ToString();
            }
            else if (slot == -1)
            {
                aslot = (a) => { return a % 2 == 1; };
                sslot = "o";
            }
            else if (slot == -2)
            {
                aslot = (a) => { return a % 2 == 0; };
                sslot = "e";
            }

            ws.Cells[row, col].Value = stat + hpref;
            col++;


            var used = build.runeUsage.runesUsed.Select(r => r.Key);
            var usedSlot = used.Where(r => aslot(r.Slot));
            var usedFilt = usedSlot.Where(r => r[stat + ssuff, build.GetPredict(r), false] != 0);

            var good = build.runeUsage.runesGood.Select(r => r.Key);
            var goodSlot = good.Where(r => aslot(r.Slot));
            var goodFilt = goodSlot.Where(r => r[stat + ssuff, build.GetPredict(r), false] != 0);

            var badFilt = usedFilt.Except(goodFilt);

            double gav = 0;
            if (goodFilt.Count() > 0)
                gav = goodFilt.Average(r => r[stat + ssuff, build.GetPredict(r), false]);
            ws.Cells[row, col].Value = gav;
            col++;

            double bav = 0;
            if (badFilt.Count() > 0)
                bav = badFilt.Average(r => r[stat + ssuff, build.GetPredict(r), false]);
            ws.Cells[row, col].Value = bav;
            col++;

            double mul = 0;

            if (bav != 0)
                mul = gav / bav;
            ws.Cells[row, col].Value = mul;
            col++;

            double av = 0;
            double std = 0;
            if (goodFilt.Count() > 0)
            {
                av = goodFilt.Average(r => r[stat + ssuff, build.GetPredict(r), false]);
                std = goodFilt.StandardDeviation(r => r[stat + ssuff, build.GetPredict(r), false]);
            }

            if (mul >= 2)
            {
                ws.Cells[row, 4].Style.Fill.PatternType = ExcelFillStyle.MediumGray;
                ws.Cells[row, 4].Style.Fill.BackgroundColor.SetColor(Color.LawnGreen);
            }

            foreach (var r in usedSlot.OrderByDescending(r => goodSlot.Contains(r)).ThenByDescending(r => r.manageStats["buildScore"]))
            {
                var rval = r[stat + ssuff, build.GetPredict(r), false];

                if (rval > 0)
                {
                    ws.Cells[row, col].Value = rval;
                    if (load != null && load.Runes.Contains(r))
                    {
                        ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.MediumTurquoise);
                        if (rval < av - std)
                            ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.MediumGray;
                    }
                    else if (build.runeUsage.runesGood.ContainsKey(r))
                    {
                        ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                        if (rval < av - std)
                            ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.MediumGray;

                    }
                    else if (rval < av - std)
                    {
                        ws.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    }
                }
                col++;
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
            bool check = false;
            if (config.AppSettings.Settings.AllKeys.Contains("colorteams"))
            {
                bool.TryParse(config.AppSettings.Settings["colorteams"].Value, out check);
            }

            bool doColor = buildList.SelectedItems.Count == 1;
            var b1 = doColor ? buildList.SelectedItems[0].Tag as Build : null;

            foreach (ListViewItem li in buildList.Items)
            {
                li.BackColor = Color.White;
                if (check && b1 != null && b1.Teams.Count > 0)
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

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            SaveLoadouts();
        }

        private void SaveLoadouts(string fname = "loads.json")
        {
            var lbs = loadoutList.Items;
            var lloads = new List<Loadout>();

            foreach (ListViewItem li in lbs)
            {
                var bb = (Loadout)li.Tag;
                lloads.Add(bb);
            }

            // only write if there are builds, may save some files
            if (lloads.Count > 0)
            {
                try
                {
                    // keep a single recent backup
                    if (File.Exists(fname))
                        File.Copy(fname, fname + ".backup", true);
                    var str = JsonConvert.SerializeObject(lloads);
                    File.WriteAllText(fname, str);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        private void toolStripButton19_Click(object sender, EventArgs e)
        {
            LoadLoadouts();
        }
        
        private void LoadLoadouts(string fname = "loads.json")
        {
            try
            {
                string text = File.ReadAllText(fname);
                var lloads = JsonConvert.DeserializeObject<Loadout[]>(text);

                foreach (var load in lloads)
                {
                    ListViewItem nli;

                    var b = builds.Where(bu => bu.ID == load.BuildID).FirstOrDefault();

                    var lvs = loadoutList.Items.Find(b.ID.ToString(), false);

                    if (lvs.Length == 0)
                        nli = new ListViewItem();
                    else
                        nli = lvs.First();

                    foreach (var rid in load.RuneIDs)
                    {
                        load.AddRune(data.Runes.Where(r => r.ID == rid).FirstOrDefault());
                    }

                    int numchanged = 0;
                    int numnew = 0;
                    int powerup = 0;
                    int upgrades = 0;
                    foreach (Rune r in load.Runes)
                    {
                        r.manageStats = load.manageStats[r.Slot - 1];
                        r.Locked = true;
                        r.manageStats["Mon"] = b.mon.ID;
                        if (r.AssignedName != b.MonName)
                        {
                            if (r.AssignedName == "Unknown name" || r.AssignedName == "Inventory")
                                numnew++;
                            else
                                numchanged++;
                        }
                        powerup += Math.Max(0, (load.FakeLevel[r.Slot - 1] ?? 0) - r.Level);
                        if (load.FakeLevel[r.Slot - 1] != 0)
                        {
                            int tup = (int)Math.Floor(Math.Min(12, (load.FakeLevel[r.Slot - 1] ?? 0)) / (double)3);
                            int cup = (int)Math.Floor(Math.Min(12, r.Level) / (double)3);
                            upgrades += Math.Max(0, tup - cup);
                        }
                    }

                    nli.Tag = load;
                    nli.Text = b.ID.ToString();
                    nli.Name = b.ID.ToString();
                    while (nli.SubItems.Count < 6)
                        nli.SubItems.Add("");
                    nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, b.ID.ToString());
                    nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, b.MonName);
                    nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, b.mon.ID.ToString());
                    nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (numnew + numchanged).ToString());
                    if (numnew > 0 && load.RuneCount < 6)
                        nli.SubItems[3].ForeColor = Color.Green;
                    if (config.AppSettings.Settings.AllKeys.Contains("splitassign"))
                    {
                        bool check;
                        bool.TryParse(config.AppSettings.Settings["splitassign"].Value, out check);
                        if (check)
                            nli.SubItems[3].Text = numnew + "/" + numchanged;
                    }
                    nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, powerup.ToString());
                    nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, (load.Time / 1000).ToString("0.##"));
                    nli.UseItemStyleForSubItems = false;
                    if (load.Time / (double)1000 > 60)
                        nli.SubItems[5].BackColor = Color.Red;
                    else if (load.Time / (double)1000 > 20)
                        nli.SubItems[5].BackColor = Color.Orange;

                    if (lvs.Length == 0)
                        loadoutList.Items.Add(nli);
                }
                checkLocked();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error occurred loading Save JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_save.txt");
                File.WriteAllText("error_loads.txt", e.ToString());
            }
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
