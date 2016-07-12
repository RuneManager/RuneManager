using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

		private Task runTask = null;
		private CancellationToken runToken;
		private CancellationTokenSource runSource = null;
		bool plsDie = false;
		public static Help help = null;

		public Main()
        {
            InitializeComponent();
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
                if (Main.config.AppSettings.Settings.AllKeys.Contains("noupdate"))
                    bool.TryParse(Main.config.AppSettings.Settings["noupdate"].Value, out dontupdate);

            }

            runes = new RuneControl[] { runeControl1, runeControl2, runeControl3, runeControl4, runeControl5, runeControl6 };
            var sorter = new ListViewSort();
            sorter.OnColumnClick(MonPriority.Index);
            listView1.ListViewItemSorter = sorter;
            listView2.ListViewItemSorter = new ListViewSort();

            sorter = new ListViewSort();
            sorter.OnColumnClick(0);
            listView5.ListViewItemSorter = sorter;

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

            for (int i = 0; i < 11; i++)
            {
                ToolStripItem it = new ToolStripMenuItem(i.ToString() + (i > 0 ? " (" + Math.Ceiling(i * 1.5).ToString() + "%)" : ""));
                it.Tag = (int)Math.Floor(i * 1.5);
                it.Click += ShrineClickSpeed;
                speedToolStripMenuItem.DropDownItems.Add(it);
            }

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
            foreach (var s in Build.statNames.Concat(Build.extraNames))
            {
                l = new Label();
                l.Location = new Point(4 + xx, 400 + yy);
                l.Name = s + "compStat";
                l.Text = s;
                l.Size = new Size(50, 14);
                groupBox1.Controls.Add(l);
                xx += 50;

                l = new Label();
                l.Location = new Point(4 + xx, 400 + yy);
                l.Name = s + "compBefore";
                l.Text = "15000";
                l.Size = new Size(50, 14);
                groupBox1.Controls.Add(l);
                xx += 50;

                l = new Label();
                l.Location = new Point(4 + xx, 400 + yy);
                l.Name = s + "compAfter";
                l.Text = "30000";
                l.Size = new Size(50, 14);
                groupBox1.Controls.Add(l);
                xx += 50;

                l = new Label();
                l.Location = new Point(4 + xx, 400 + yy);
                l.Name = s + "compDiff";
                l.Text = "+15000";
                l.Size = new Size(50, 14);
                groupBox1.Controls.Add(l);
                xx += 50;

                if (s == "SPD")
                    yy += 4;
                if (s == "ACC")
                    yy += 8;

                yy += 16;
                xx = 0;
            }
        }
        
        private void ShrineClickSpeed(object sender, EventArgs e)
        {
            var it = (ToolStripMenuItem)sender;
            foreach (ToolStripMenuItem i in speedToolStripMenuItem.DropDownItems)
            {
                i.Checked = false;
            }
            it.Checked = true;
            data.shrines.Speed = (int)it.Tag;
            if (config != null)
            {
                config.AppSettings.Settings.Remove("shrineSpeed");
                config.AppSettings.Settings.Add("shrineSpeed", it.Tag.ToString());
                config.Save(ConfigurationSaveMode.Modified);
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
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
            {
                var item = listView1.FocusedItem;
                if (item.Tag != null)
                {
                    Monster mon = (Monster)item.Tag;

                    ShowMon(mon);
                }
            }
        }

        private void ShowMon(Monster mon)
        {
            Stats cur = mon.GetStats();

            statName.Text = mon.Name;
            statID.Text = mon.ID.ToString();
            statLevel.Text = mon.level.ToString();

            ShowStats(cur, mon);
            ShowRunes(mon.Current.runes);
        }

        private void ShowStats(Stats cur, Stats mon)
        {
            foreach (string stat in new string[] { "HP", "ATK", "DEF", "SPD" })
            {
                groupBox1.Controls.Find(stat + "Base", false).FirstOrDefault().Text = mon[stat].ToString();
                groupBox1.Controls.Find(stat + "Total", false).FirstOrDefault().Text = cur[stat].ToString();
                groupBox1.Controls.Find(stat + "Bonus", false).FirstOrDefault().Text = "+" + (cur[stat] - mon[stat]).ToString();
            }

            foreach (string stat in new string[] { "CR", "CD", "RES", "ACC" })
            {
                groupBox1.Controls.Find(stat + "Base", false).FirstOrDefault().Text = mon[stat].ToString() + "%";
                groupBox1.Controls.Find(stat + "Total", false).FirstOrDefault().Text = cur[stat].ToString() + "%";
                groupBox1.Controls.Find(stat + "Bonus", false).FirstOrDefault().Text = "+" + (cur[stat] - mon[stat]).ToString();
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
                }
                else
                {
                    tc.Hide();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int size = -1;
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
                                if (MessageBox.Show("Do you want to override the existing startup save?", "Load Save" , MessageBoxButtons.YesNo) == DialogResult.Yes)
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
            Console.WriteLine(size); // <-- Shows file size in debugging mode.
            Console.WriteLine(result); // <-- For debugging use.
			
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
            listView1.Items.Clear();
            listView2.Items.Clear();
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
            builds = JsonConvert.DeserializeObject<List<Build>>(text);
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
                    button1_Click(null, new EventArgs());
                }
                else
                {
                    MessageBox.Show("Couldn't load monster stats, builds may not work");
                }
            }

            foreach (Build b in builds)
            {
                if (data != null)
                    b.mon = data.GetMonster(b.MonName);
                else
                {
                    b.mon = new Monster();
                    b.mon.Name = b.MonName;
                }

				int id = b.ID;
				if (b.ID == 0)
				{
					id = listView5.Items.Count + 1;
					b.ID = id;
				}
                if (b.priority == 0)
                {
                    b.priority = listView5.Items.Count + 1;
                }
				ListViewItem li = new ListViewItem(new string[] { b.priority.ToString(), id.ToString(), b.mon.Name, "" });
                li.Tag = b;
                listView5.Items.Add(li);
				
				// ask the enumerable to eat Linq. Unsure why Find(b.mon.Name, false/true) failed here.
				var lv1li = listView1.Items.Cast<ListViewItem>().Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == b.mon.Name).Count() > 0).FirstOrDefault();
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
            //string save = System.IO.File.ReadAllText("..\\..\\save.json");
            data = JsonConvert.DeserializeObject<Save>(text);
            foreach (Monster mon in data.Monsters)
            {
                var equipedRunes = data.Runes.Where(r => r.AssignedId == mon.ID);

				bool wasStored = (mon.Name.IndexOf("In Storage") >= 0);
                mon.Name = mon.Name.Replace(" (In Storage)", "");
                
                foreach (Rune rune in equipedRunes)
                {
                    mon.ApplyRune(rune);
                    rune.AssignedName = mon.Name;
                }

                if (mon.priority == 0 && mon.Current.runeCount > 0)
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
				if (wasStored)
					item.ForeColor = Color.Gray;
				
                item.Tag = mon;
                listView1.Items.Add(item);
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
                listView2.Items.Add(item);
            }

            if (data.shrines == null)
                data.shrines = new Stats();
            
            if (config != null)
            {
                if (config.AppSettings.Settings.AllKeys.Contains("shrineSpeed"))
                {
                    int val = 0;
                    int.TryParse(config.AppSettings.Settings["shrineSpeed"].Value, out val);
                    data.shrines.Speed = val;
                    int level = (int)Math.Floor(val / (double)1.5);
                    ((ToolStripMenuItem)speedToolStripMenuItem.DropDownItems[level]).Checked = true;
                }
            }
        }

        public void checkLocked()
        {
            if (data == null)
                return;
            if (data.Runes != null)
                toolStripStatusLabel1.Text = "Locked: " + data.Runes.Where(r => r.Locked == true).Count().ToString();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // confirm maybe?
            this.Close();
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
                rune_Stats((Rune)tc.Tag);
                runeBox.Show();
                runeShown.SetRune((Rune)tc.Tag);
            }
            else
            {
                tc.Hide();
                runeBox.Hide();
            }
        }

        private void rune_Stats(Rune rune)
        {
            SRuneMain.Text = Rune.StringIt(rune.MainType, rune.MainValue) + " " + rune.ID;
            SRuneInnate.Text = Rune.StringIt(rune.InnateType, rune.InnateValue);
            SRuneSub1.Text = Rune.StringIt(rune.Sub1Type, rune.Sub1Value);
            SRuneSub2.Text = Rune.StringIt(rune.Sub2Type, rune.Sub2Value);
            SRuneSub3.Text = Rune.StringIt(rune.Sub3Type, rune.Sub3Value);
            SRuneSub4.Text = Rune.StringIt(rune.Sub4Type, rune.Sub4Value);
            SRuneLevel.Text = rune.Level.ToString();
            SRuneMon.Text = rune.AssignedName;
        }

        private void label8_Click(object sender, EventArgs e)
        {
            runeBox.Hide();
            foreach (RuneControl r in runes)
            {
                r.Gamma = 1;
                r.Refresh();
            }
        }

        private void label8_Click_1(object sender, EventArgs e)
        {
            runeBox2.Hide();
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.FocusedItem != null)
            {
                var item = listView2.FocusedItem;
                if (item.Tag != null)
                {
                    Rune rune = (Rune)item.Tag;

                    runeBox2.Show();
                    runeInventory.SetRune(rune);

                    IRuneMain.Text = Rune.StringIt(rune.MainType, rune.MainValue);
                    IRuneInnate.Text = Rune.StringIt(rune.InnateType, rune.InnateValue);
                    IRuneSub1.Text = Rune.StringIt(rune.Sub1Type, rune.Sub1Value);
                    IRuneSub2.Text = Rune.StringIt(rune.Sub2Type, rune.Sub2Value);
                    IRuneSub3.Text = Rune.StringIt(rune.Sub3Type, rune.Sub3Value);
                    IRuneSub4.Text = Rune.StringIt(rune.Sub4Type, rune.Sub4Value);
                    IRuneMon.Text = rune.AssignedName;
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
            if (listView1.FocusedItem != null)
            {
                if (listView1.FocusedItem.Tag != null)
                {
                    Monster mon = (Monster)listView1.FocusedItem.Tag;
                    int maxPri = (int)data.Monsters.Max(x => x.priority);
                    if (mon.priority == 0)
                    {
                        mon.priority = maxPri + 1;
                        listView1.FocusedItem.SubItems[MonPriority.Index].Text = (maxPri + 1).ToString();
                    }
                    else if (mon.priority != 1)
                    {
                        int pri = mon.priority;
                        Monster mon2 = data.Monsters.Where(x => x.priority == pri - 1).FirstOrDefault();
                        if (mon2 != null)
                        {
                            var items = listView1.Items;
                            ListViewItem listMon = listView1.FindItemWithText(mon2.Name);
                            mon2.priority += 1;
                            listMon.SubItems[MonPriority.Index].Text = mon2.priority.ToString();
                        }
                        mon.priority -= 1;
                        listView1.FocusedItem.SubItems[MonPriority.Index].Text = (mon.priority).ToString();                        
                    }
                    listView1.Sort();
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listView1.FocusedItem != null)
            {
                if (listView1.FocusedItem.Tag != null)
                {
                    Monster mon = (Monster)listView1.FocusedItem.Tag;
                    int maxPri = (int)data.Monsters.Max(x => x.priority);
                    if (mon.priority == 0)
                    {
                        mon.priority = maxPri + 1;
                        listView1.FocusedItem.SubItems[MonPriority.Index].Text = (maxPri + 1).ToString();
                    }
                    else if (mon.priority != maxPri)
                    {
                        int pri = mon.priority;
                        Monster mon2 = data.Monsters.Where(x => x.priority == pri + 1).FirstOrDefault();
                        if (mon2 != null)
                        {
                            var items = listView1.Items;
                            ListViewItem listMon = listView1.FindItemWithText(mon2.Name);
                            mon2.priority -= 1;
                            listMon.SubItems[MonPriority.Index].Text = mon2.priority.ToString();
                        }
                        mon.priority += 1;
                        listView1.FocusedItem.SubItems[MonPriority.Index].Text = (mon.priority).ToString();
                    }
                    listView1.Sort();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(tabRunes.Name);
            filterList(listView2, x => ((Rune)x).Slot == ((Rune)runeShown.Tag).Slot);
        }

        private void filterList(ListView list, Predicate<object> p)
        {
            listView2.Items.Clear();
            if (data == null)
                return;

            if (data.Runes != null)
            foreach (Rune rune in data.Runes.Where(x => p.Invoke(x)))
            {
                ListViewItem item = new ListViewItem(new string[]{
                    rune.Set.ToString(),
                    rune.ID.ToString(),
                    rune.Grade.ToString(),
                    Rune.StringIt(rune.MainType, true),
                    rune.MainValue.ToString()
                });
                item.Tag = rune;
                listView2.Items.Add(item);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            filterList(listView2, x => true);
        }
        
        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.FocusedItem != null)
            {
                var item = listView3.FocusedItem;
                if (item.Tag != null)
                {
                    Loadout load = (Loadout)item.Tag;

					var monid = int.Parse(item.SubItems[2].Text);
					var bid = int.Parse(item.SubItems[0].Text);

					var build = builds.Where(b => b.ID == bid).FirstOrDefault();

					Monster mon = null;
					if (build == null)
						mon = data.GetMonster(monid);
					else
						mon = build.mon;

					ShowMon(mon);

                    ShowStats(load.GetStats(mon), mon);

                    ShowRunes(load.runes);
                    ShowDiff(data.GetMonster(monid).GetStats(), load.GetStats(mon));
                }
            }
            int cost = 0;
            foreach (ListViewItem li in listView3.SelectedItems)
            {
                if (li.Tag != null)
                {
                    Loadout load = (Loadout)li.Tag;
					if (load != null)
					{
						var mon = data.GetMonster(int.Parse(li.SubItems[2].Text));
						if (mon != null)
							cost += mon.SwapCost(load);
					}
                }
            }
            toolStripStatusLabel2.Text = "Unequip: " + cost.ToString();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem li in listView3.Items)
            {
                Loadout l = (Loadout)li.Tag;

                foreach (Rune r in l.runes)
                {
                    r.Locked = false;
                }

                listView3.Items.Remove(li);
            }
            //builds.Clear();
            checkLocked();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (var ff = new Create())
                {
                    var bb = new Build();
                    bb.New = true;
                    bb.mon = (Monster)listView1.SelectedItems[0].Tag;
					bb.ID = listView5.Items.Count + 1;
                    ff.Tag = bb;
                    var res = ff.ShowDialog();
                    if (res == System.Windows.Forms.DialogResult.OK)
                    {
						if (builds.Count > 0)
							ff.build.priority = builds.Max(b => b.priority) + 1;
						else
							ff.build.priority = 1;

                        ListViewItem li = new ListViewItem(new string[]{ff.build.priority.ToString(), ff.build.ID.ToString(), ff.build.mon.Name,"0%"});
                        li.Tag = ff.build;
                        listView5.Items.Add(li);
                        builds.Add(ff.build);

						var lv1li = listView1.Items.Cast<ListViewItem>().Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == ff.build.mon.Name).Count() > 0).FirstOrDefault();
						if (lv1li != null)
							lv1li.ForeColor = Color.Green;
					
					}
				}
            }
        }

        private void listView5_DoubleClick(object sender, EventArgs e)
        {
            var items = listView5.SelectedItems;
            if (items.Count > 0)
            {
                var item = items[0];
                if (item.Tag != null)
                {
                    Build bb = (Build)item.Tag;
                    using (var ff = new Create())
                    {
                        ff.Tag = bb;
                        var res = ff.ShowDialog();
                        if (res == System.Windows.Forms.DialogResult.OK)
                        {
                        }
                    }
                }
            }
        }

        private void RunBuild(ListViewItem pli, bool isBatch = false)
        {
            if (plsDie)
                return;

            if (currentBuild != null)
                currentBuild.isRun = false;

            if (pli.Tag != null)
            {
				Stopwatch buildTime = Stopwatch.StartNew();
                Build b = (Build)pli.Tag;
                currentBuild = b;

                ListViewItem[] olvs = null;
                Invoke((MethodInvoker)delegate { olvs = listView3.Items.Find(b.ID.ToString(), false); });

                if (olvs.Length > 0)
                {
                    var olv = olvs.First();
                    Loadout ob = (Loadout)olv.Tag;
                    foreach (Rune r in ob.runes)
                    {
                        r.Locked = false;
                    }
                }

                b.GenRunes(data, false, useEquipped, isBatch);
                b.shrines = data.shrines;

                string nR = "";

                for (int i = 0; i < b.runes.Length; i++)
                {
                    if (b.runes[i] != null && b.runes[i].Length == 0)
                        nR += (i + 1) + " ";
                }

                if (nR != "")
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        pli.SubItems[3].Text = ":( " + nR + "Runes";
                    });
                    return;
                }
                
                b.GenBuilds(0, 0, (str) =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        pli.SubItems[3].Text = str;
                    });
                }, null, true, isBatch);

                if (b.Best == null)
                {
                    return;
                }

                int numchanged = 0;
                int numnew = 0;
                int powerup = 0;
                int upgrades = 0;
                foreach (Rune r in b.Best.Current.runes)
                {
                    r.Locked = true;
                    if (r.AssignedName != b.Best.Name)
                    {
                        //if (b.mon.Current.runes[r.Slot - 1] == null)
                        if (r.AssignedName == "Unknown name")
                            numnew++;
                        else
                            numchanged++;
                    }
                    powerup += Math.Max(0, b.Best.Current.FakeLevel[r.Slot - 1] - r.Level);
                    if (b.Best.Current.FakeLevel[r.Slot -1] != 0)
                    {
                        int tup = (int)Math.Floor(Math.Min(12, b.Best.Current.FakeLevel[r.Slot - 1]) / (double)3);
                        int cup = (int)Math.Floor(Math.Min(12, r.Level) / (double)3);
                        upgrades += Math.Max(0, tup - cup);
                    }
                }
                currentBuild = null;
				b.Time = buildTime.ElapsedMilliseconds;
				buildTime.Stop();

				this.Invoke((MethodInvoker)delegate
                {
					checkLocked();

					ListViewItem nli;

                    var lvs = listView3.Items.Find(b.ID.ToString(), false);

					if (lvs.Length == 0)
                        nli = new ListViewItem();
                    else
                        nli = lvs.First();

					nli.Tag = b.Best.Current;
					nli.Text = b.ID.ToString();
					nli.Name = b.ID.ToString();
					while (nli.SubItems.Count < 6)
						nli.SubItems.Add("");
					nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, b.ID.ToString());
					nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, b.Best.Name);
					nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, b.Best.ID.ToString());
                    nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (numnew + numchanged).ToString());
                    if (Main.config.AppSettings.Settings.AllKeys.Contains("splitassign"))
                    {
                        bool check = false;
                        bool.TryParse(Main.config.AppSettings.Settings["splitassign"].Value, out check);
                        if (check)
                            nli.SubItems[3].Text = numnew.ToString() + "/" + numchanged.ToString();
                    }
					nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, powerup.ToString());
					//nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, upgrades.ToString());
					nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, (b.Time/(double)1000).ToString("0.##"));

					if (lvs.Length == 0)
						listView3.Items.Add(nli);

				});

            }

        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            plsDie = false;
            var lis = listView5.SelectedItems;
            if (lis.Count > 0)
            {
                var li = lis[0];
                Task.Factory.StartNew(() =>
                {
                    RunBuild(li);
                });
            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            builds.Clear();

            var lbs = listView5.Items;

            foreach (ListViewItem li in lbs)
            {
                var bb = (Build)li.Tag;
                bb.MonName = bb.mon.Name;
                builds.Add(bb);
            }

            var str = JsonConvert.SerializeObject(builds);
            File.WriteAllText("builds.json", str);
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            var lis = listView5.SelectedItems;
            if (lis.Count > 0)
            {
				foreach (ListViewItem li in lis)
				{
					listView5.Items.Remove(li);
					Build b = (Build)li.Tag;
					if (b != null)
					{
						var lv1li = listView1.Items.Cast<ListViewItem>().Where(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Where(s => s.Text == b.mon.Name).Count() > 0).FirstOrDefault();
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
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            var lis = listView3.SelectedItems;
            if (lis.Count > 0)
            {
                var li = lis[0];

                Loadout l = (Loadout)li.Tag;

                foreach (Rune r in l.runes)
                {
                    r.Locked = false;
                }

                listView3.Items.Remove(li);
            }
            checkLocked();
		}

        private void toolStripButton12_Click(object sender, EventArgs e)
		{
            if (data == null)
                return;
            if (runTask != null && runTask.Status == TaskStatus.Running)
            {
                runSource.Cancel();
                if (currentBuild != null)
                    currentBuild.isRun = false;
                plsDie = true;
                return;
            }
            plsDie = false;

            // unlock and remove all current builds
			foreach (ListViewItem li in listView3.Items)
			{
				Loadout l = (Loadout)li.Tag;

				foreach (Rune r in l.runes)
				{
					r.Locked = false;
				}

				listView3.Items.Remove(li);
			}
            builds.Clear();

            // collect the builds
			List<ListViewItem> list5 = new List<ListViewItem>();
			foreach (ListViewItem li in listView5.Items)
			{
				list5.Add(li);

                li.SubItems[3].Text = "";
			}

            runSource = new CancellationTokenSource();
            runToken = runSource.Token;
			runTask = Task.Factory.StartNew(() =>
			{
                if (data.Runes != null)
                foreach (Rune r in data.Runes)
                {
					r.Swapped = false;
                    r.ResetStats();
                }

				foreach (ListViewItem li in list5)
				{
                    RunBuild(li, true);
				}

                GenerateStats();
                this.Invoke((MethodInvoker) delegate { GenerateExcel(); });

            }, runSource.Token);
		}

        void GenerateExcel()
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
                        if (MessageBox.Show("Please close runestats.xlsx\r\nOr ensure you can delete it.", "RuneStats", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                        {
                            status = 1;
                        }
                    }
                }
            }

            ExcelPackage pck = new ExcelPackage(newFile);
            var ws = pck.Workbook.Worksheets.Add("Runes");
            int row = 1;
            int col = 1;
            foreach (var th in "Id,Grade,Set,Slot,MainType,Level,Select,Rune,Type,Load,Gen,Eff,Used,Points,FlatCount,FlatPts,SellScore,Action".Split(','))
            {
                ws.Cells[row, col].Value = th; col++;
            }
            row++;
            col = 1;

            foreach (Rune r in data.Runes.OrderByDescending(r => r.ScoringBad))
            {
                ws.Cells[row, col].Value = r.ID; col++;

                ws.Cells[row, col].Value = r.Grade; col++;

                ws.Cells[row, col].Value = r.Set; col++;
                ws.Cells[row, col].Value = r.Slot; col++;
                ws.Cells[row, col].Value = r.MainType; col++;

                ws.Cells[row, col].Value = r.Level; col++;

                ws.Cells[row, col].Value = r.manageStats_Set; col++;
                ws.Cells[row, col].Value = r.manageStats_RuneFilt; col++;
                ws.Cells[row, col].Value = r.manageStats_TypeFilt; col++;

                ws.Cells[row, col].Value = r.manageStats_LoadFilt; col++;
                ws.Cells[row, col].Value = r.manageStats_LoadGen; col++;

                ws.Cells[row, col].Value = r.Efficiency.ToString("0.####"); col++;

                ws.Cells[row, col].Value = (r.manageStats_In ? "TRUE" : "FALSE"); col++;

                ws.Cells[row, col].Value = r.ScoringBad.ToString(); col++;

                ws.Cells[row, col].Value = r.FlatCount().ToString(); col++;
                ws.Cells[row, col].Value = r.FlatPoints().ToString("0.####"); col++;

                ws.Cells[row, col].Value = r.ScoringSell.ToString(); col++;

                ws.Cells[row, col].Value = r.ScoringAct; col++;
                
                row++;
                col = 1;
            }
            // write rune stats

            foreach (ListViewItem li in listView3.Items)
            {
                if (li.Tag != null)
                {
                    Loadout load = (Loadout)li.Tag;
                    if (load != null)
                    {
                        var mon = data.GetMonster(int.Parse(li.SubItems[2].Text));

                        ws = pck.Workbook.Worksheets.Add(mon.Name);
                        col = 1;
                        ws.Cells[1, 2].Value = "Pass";
                        ws.Cells[1, 3].Value = "Good";
                        ws.Cells[1, 4].Value = "Std Dev";
                        ws.Cells[1, 5].Value = "'# This time";
                        ws.Cells[1, 6].Value = "'# Next time";
                        ws.Cells[1, 7].Value = "Limit";
                        row = 2;
                        for (int i = 0; i < Build.statNames.Length; i++)
                        {
                            var stat = Build.statNames[i];

                            if (i <= 3) //SPD
                            {
                                ws.Cells[row, col].Value = stat;
                                col++;
                                ws.Cells[row, col].Value = load.runeUsage.runesUsed.Select(r => r.Key).Where(r => r[stat + "flat", 0, false] != 0).Average(r => r[stat + "flat", 0, false]);
                                col++;
                                double av = load.runeUsage.runesGood.Select(r => r.Key).Where(r => r[stat + "flat", 0, false] != 0).Average(r => r[stat + "flat", 0, false]);
                                ws.Cells[row, col].Value = av;
                                col++;
                                double std = load.runeUsage.runesGood.Select(r => r.Key).Where(r => r[stat + "flat", 0, false] != 0).StandardDeviation(r => r[stat + "flat", 0, false]);
                                ws.Cells[row, col].Value = std;
                                col++;
                                ws.Cells[row, col].Value = load.runeUsage.runesUsed.Select(r => r.Key).Where(r => r[stat + "flat", 0, false] != 0).Count();
                                col++;
                                ws.Cells[row, col].Value = load.runeUsage.runesUsed.Select(r => r.Key).Where(r => r[stat + "flat", 0, false] > av - std).Count();
                                col++;
                                ws.Cells[row, col].Value = (av - std).ToString("0.##");
                                
                                row++;
                                col = 1;
                            }
                            if (i != 3)
                            {
                                ws.Cells[row, col].Value = stat + " %";
                                col++;
                                ws.Cells[row, col].Value = load.runeUsage.runesUsed.Select(r => r.Key).Where(r => r[stat + "perc", 0, false] != 0).Average(r => r[stat + "perc", 0, false]);
                                col++;
                                double av = load.runeUsage.runesGood.Select(r => r.Key).Where(r => r[stat + "perc", 0, false] != 0).Average(r => r[stat + "perc", 0, false]);
                                ws.Cells[row, col].Value = av;
                                col++;
                                double std = load.runeUsage.runesGood.Select(r => r.Key).Where(r => r[stat + "perc", 0, false] != 0).StandardDeviation(r => r[stat + "perc", 0, false]);
                                ws.Cells[row, col].Value = std;
                                col++;
                                ws.Cells[row, col].Value = load.runeUsage.runesUsed.Select(r => r.Key).Where(r => r[stat + "perc", 0, false] != 0).Count();
                                col++;
                                ws.Cells[row, col].Value = load.runeUsage.runesUsed.Select(r => r.Key).Where(r => r[stat + "perc", 0, false] > av - std).Count();
                                col++;
                                ws.Cells[row, col].Value = (av - std).ToString("0.##");

                                row++;
                                col = 1;
                            }

                        }

                    }
                }
            }

            pck.Save();

        }

        void GenerateStats()
        {
            if (data == null || data.Runes == null)
                return;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Id,Grade,Set,Slot,MainType,Level,Select,Rune,Type,Load,Gen,Eff,Used,Points,FlatCount,FlatPts,SellScore,Action");

            foreach (Rune r in data.Runes.OrderByDescending(r => r.ScoringBad))
            {
                sb.Append(r.ID + ",");

                sb.Append(r.Grade + ",");

                sb.Append(r.Set + ",");
                sb.Append(r.Slot + ",");
                sb.Append(r.MainType + ",");

                sb.Append(r.Level + ",");

                sb.Append(r.manageStats_Set + ",");
                sb.Append(r.manageStats_RuneFilt + ",");
                sb.Append(r.manageStats_TypeFilt + ",");

                sb.Append(r.manageStats_LoadFilt + ",");
                sb.Append(r.manageStats_LoadGen + ",");

                sb.Append(r.Efficiency.ToString("0.####") + ",");

                sb.Append((r.manageStats_In ? "TRUE" : "FALSE") + ",");
                
                sb.Append(r.ScoringBad.ToString() + ",");

				sb.Append(r.FlatCount().ToString() + ",");
				sb.Append(r.FlatPoints().ToString("0.####") + ",");

				sb.Append(r.ScoringSell.ToString() + ",");

				sb.Append(r.ScoringAct + ",");

				sb.AppendLine();
            }
            int status = 0;
            while (status != 1)
            {
                try
                {
                    File.WriteAllText("runestats.csv", sb.ToString());
                    status = 1;
                }
                catch (Exception e)
                {
                    if (status == 0)
                    {
                        if (MessageBox.Show("Please close runestats.csv\r\nOr ensure you can create a file here.", "RuneStats", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
                        {
                            status = 1;
                        }
                    }
                }
            }
        }

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			builds.Clear();

			var lbs = listView5.Items;

			foreach (ListViewItem li in lbs)
			{
				var bb = (Build)li.Tag;
				bb.MonName = bb.mon.Name;
				builds.Add(bb);
			}

			var str = JsonConvert.SerializeObject(builds);
			File.WriteAllText("builds.json", str);
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
                r.AssignedName = "Unknown name";
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
            if (listView5.FocusedItem != null)
            {
                if (listView5.FocusedItem.Tag != null)
                {
                    Build build = (Build)listView5.FocusedItem.Tag;

                    var bb = builds.OrderBy(b => b.priority).ToList();
                    var bi = bb.FindIndex(b => b == build);
                    
                    if (bi != 0)
                    {
                        var b2 = bb[bi - 1];
                        var sid = build.priority;
                        build.priority = b2.priority;
                        b2.priority = sid;

                        listView5.FocusedItem.SubItems[0].Text = build.priority.ToString();

                        ListViewItem listmon = listView5.FindItemWithText(b2.mon.Name);
                        listmon.SubItems[0].Text = b2.priority.ToString();

                        listView5.Sort();
                    }
                }
            }
        }

        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            if (listView5.FocusedItem != null)
            {
                if (listView5.FocusedItem.Tag != null)
                {
                    Build build = (Build)listView5.FocusedItem.Tag;

                    var bb = builds.OrderBy(b => b.priority).ToList();
                    var bi = bb.FindIndex(b => b == build);

                    if (bi != builds.Max(b => b.priority) && bi+1 < bb.Count)
                    {
                        var b2 = bb[bi + 1];
                        var sid = build.priority;
                        build.priority = b2.priority;
                        b2.priority = sid;

                        listView5.FocusedItem.SubItems[0].Text = build.priority.ToString();

                        ListViewItem listmon = listView5.FindItemWithText(b2.mon.Name);
                        listmon.SubItems[0].Text = b2.priority.ToString();

                        listView5.Sort();
                    }
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
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
                try {
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
                        //MessageBox.Show("You have a newer version than available");
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
                catch(Exception ex)
                {
                    updateComplain.Text = e.Error.ToString();
                    updateComplain.Visible = false;
                }
            });
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

            // return 0;
        }

        private void updateDownload_Click(object sender, EventArgs e)
        {
            if (filelink != "")
            {
                Process.Start(filelink);
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
            GenerateStats();
            GenerateExcel();
		}

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new Options())
            {
                f.ShowDialog();
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
	}
}
