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

        public Main()
        {
            InitializeComponent();
            runes = new RuneControl[] { runeControl1, runeControl2, runeControl3, runeControl4, runeControl5, runeControl6 };
            var sorter = new ListViewSort();
            sorter.OnColumnClick(MonPriority.Index);
            listView1.ListViewItemSorter = sorter;
            listView2.ListViewItemSorter = new ListViewSort();

            sorter = new ListViewSort();
            sorter.OnColumnClick(0);
            listView5.ListViewItemSorter = sorter;

            Task.Factory.StartNew(() =>
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadStringCompleted += client_DownloadStringCompleted;
                    client.DownloadStringAsync(new Uri("https://www.dropbox.com/s/nrfmjzsw9cltorf/version.txt?dl=1"));
                }
            });
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
                try
                {
                    size = LoadFile(file);
                }
                catch (IOException)
                {
                }
            }
            Console.WriteLine(size); // <-- Shows file size in debugging mode.
            Console.WriteLine(result); // <-- For debugging use.
			
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
            foreach (Build b in builds)
            {
                b.mon = data.GetMonster(b.MonName);
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
                
            }
        }

        public void LoadJSON(string text)
        {
            //string save = System.IO.File.ReadAllText("..\\..\\save.json");
            data = JsonConvert.DeserializeObject<Save>(text);
            foreach (Monster mon in data.Monsters)
            {
                var equipedRunes = data.Runes.Where(r => r.AssignedId == mon.ID);

                mon.Name = mon.Name.Replace(" (In Storage)", "");
                
                foreach (Rune rune in equipedRunes)
                {
                    mon.ApplyRune(rune);
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
        }

        public void checkLocked()
        {
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
                    Loadout build = (Loadout)item.Tag;

                    var mon = data.GetMonster(int.Parse(item.SubItems[2].Text));

                    ShowStats(build.GetStats(mon), mon);

                    ShowRunes(build.runes);

                }
            }
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
                        ff.build.priority = (listView5.Items.Count + 1);
                        ListViewItem li = new ListViewItem(new string[]{ff.build.priority.ToString(), ff.build.ID.ToString(), ff.build.mon.Name,"0%"});
                        li.Tag = ff.build;
                        listView5.Items.Add(li);
                        builds.Add(ff.build);
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

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            var lis = listView5.SelectedItems;
            if (lis.Count > 0)
            {
                var li = lis[0];
                if (li.Tag != null)
                {
                    Build b = (Build)li.Tag;

					var olvs = listView3.Items.Find(b.ID.ToString(), false);
                    if (olvs.Length > 0)
                    {
                        var olv = olvs.First();
                        Loadout ob = (Loadout)olv.Tag;
                        foreach (Rune r in ob.runes)
                        {
                            r.Locked = false;
                        }
                    }
                    
                    Task.Factory.StartNew(() => {
                        b.GenRunes(data, false, useEquipped);
                        b.GenBuilds(0, 0, (str) => {
                            this.Invoke((MethodInvoker) delegate {
                                li.SubItems[3].Text = str;
                            }); 
                        }, null, true);

                        if (b.Best == null)
                        {
                            this.Invoke((MethodInvoker)delegate {
                                li.SubItems[3].Text = "No builds :(";
                            });
                            return;
                        }

                        foreach (Rune r in b.Best.Current.runes)
                        {
                            r.Locked = true;
                        }
                        checkLocked();
                        
                        this.Invoke((MethodInvoker)delegate
                        {
                            var lvs = listView3.Items.Find(b.ID.ToString(), false);
                            if (lvs.Length == 0)
                            {
                                li = new ListViewItem();
                                li.Tag = b.Best.Current;
                                li.Text = b.ID.ToString();
                                li.SubItems.Add(b.Best.Name);
                                li.SubItems.Add(b.Best.ID.ToString());
                                li.Name = b.ID.ToString();
                                listView3.Items.Add(li);
                            }
                            else
                            {
                                li = lvs.First();
                                li.Tag = b.Best.Current;
                                li.Text = b.ID.ToString();
                                li.SubItems.Add(b.Best.Name);
                                li.SubItems.Add(b.Best.ID.ToString());
                                li.Name = b.ID.ToString();
                            }
                        }); 

                    });

                }
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
                    listView5.Items.Remove(li);
            }
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
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
		}

        private Task runTask = null;
        private CancellationToken runToken;
        private CancellationTokenSource runSource = null;
        bool plsDie = false;

		private void toolStripButton12_Click(object sender, EventArgs e)
		{
            if (runTask != null && runTask.Status == TaskStatus.Running)
            {
                runSource.Cancel();
                //runTask = null;
                plsDie = true;
                return;
            }
            plsDie = false;

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
				foreach (ListViewItem li in list5)
				{
                    if (plsDie)
                        return;

					if (li.Tag != null)
					{
						Build b = (Build)li.Tag;

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

						b.GenRunes(data, false, useEquipped);

                        string nR = "";

                        for(int i = 0; i < b.runes.Length; i++)
                        {
                            if (b.runes[i].Length == 0)
                                nR += (i+1) + " ";
                        }

						if (nR != "")
						{
                            this.Invoke((MethodInvoker)delegate
                            {
                                li.SubItems[3].Text = ":( " + nR + "Runes";
                            });
                            continue;
						}


						b.GenBuilds(0, 0, (str) =>
						{
							this.Invoke((MethodInvoker)delegate
							{
								li.SubItems[3].Text = str;
							});
						});

                        if (b.Best == null)
                        {
                            continue;
                        }

						foreach (Rune r in b.Best.Current.runes)
						{
							r.Locked = true;
						}
                        checkLocked();

						this.Invoke((MethodInvoker)delegate
						{
							var lvs = listView3.Items.Find(b.ID.ToString(), false);
							if (lvs.Length == 0)
							{
								var nli = new ListViewItem();
								nli.Tag = b.Best.Current;
								nli.Text = b.ID.ToString();
								nli.SubItems.Add(b.Best.Name);
                                nli.SubItems.Add(b.Best.ID.ToString());
								nli.Name = b.ID.ToString();
								listView3.Items.Add(nli);
							}
							else
							{
								var nli = lvs.First();
								nli.Tag = b.Best.Current;
								nli.Text = b.ID.ToString();
								nli.SubItems.Add(b.Best.Name);
                                nli.SubItems.Add(b.Best.ID.ToString());
                                nli.Name = b.ID.ToString();
							}
						});

					}

				}
            }, runSource.Token);
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
            foreach (Monster mon in data.Monsters)
            {
                for (int i = 1; i < 7; i++)
                    mon.Current.RemoveRune(i);
            }

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
        }


        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                updateBox.Visible = true;
                string result = e.Result;
                string newvernum = result.Substring(0, result.IndexOf('\r'));
                if (newvernum.IndexOf('\n') != -1)
                    newvernum = newvernum.Substring(0, newvernum.IndexOf('\n'));
                Console.WriteLine(newvernum);
                int ind1 = result.IndexOf('\n');
                
                if (result.IndexOf('\n') != -1)
                {
                    int ind2 = result.IndexOf('\n', ind1 + 1);
                    if (ind2 == -1)
                        filelink = e.Result.Substring(ind1 + 1);
                    else
                    {
                        int offset = (e.Result.Contains('\r') ? 2 : 1);
                        filelink = e.Result.Substring(ind1 + 1, ind2 - ind1 - offset);
                        whatsNewText = e.Result.Substring(ind2 + 1);
                    }

                }
                var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                string oldvernum = ver.ProductVersion;
                updateNew.Text = "New: " + newvernum;
                updateCurrent.Text = "Current: " + oldvernum;
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
                MessageBox.Show(whatsNewText);
            }
        }
    }
}
