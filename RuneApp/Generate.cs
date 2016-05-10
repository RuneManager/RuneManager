using RuneOptim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace RuneApp
{
    // Generates a bunch of builds to preview the stats
    public partial class Generate : Form
    {
		private static string[] statNames = new string[] { "HP", "ATK", "DEF", "SPD", "CR", "CD", "RES", "ACC" };
        private static string[] extraNames = new string[] { "EHP", "EHPDB", "DPS", "AvD", "MxD" };

        public static int buildsGen = 5000;
        public static int buildsShow = 100;

        // the build to use
		public Build build = null;

        // if making builds
		bool building = false;

        bool grayLocked = false;
        bool noLocked = false;
		
		public Generate(Build bb)
        {
            InitializeComponent();
            if (Main.config.AppSettings.Settings.AllKeys.Contains("locktest"))
                bool.TryParse(Main.config.AppSettings.Settings["locktest"].Value, out noLocked);
            if (Main.config.AppSettings.Settings.AllKeys.Contains("testgray"))
                bool.TryParse(Main.config.AppSettings.Settings["testgray"].Value, out grayLocked);
            if (Main.config.AppSettings.Settings.AllKeys.Contains("testGen"))
                int.TryParse(Main.config.AppSettings.Settings["testGen"].Value, out buildsGen);
            if (Main.config.AppSettings.Settings.AllKeys.Contains("testShow"))
                int.TryParse(Main.config.AppSettings.Settings["testShow"].Value, out buildsShow);

            // master has given Gener a Build?
            build = bb;
			Label label = null;
            TextBox textBox = null;

            // cool clicky thing
            var sorter = new ListViewSort();
            // sort decending on POINTS
            sorter.OnColumnClick(0, false);
            listView1.ListViewItemSorter = sorter;
            
            // place controls in a nice grid-like manner
			int x, y;

			y = 20;
			foreach (string stat in statNames)
			{
				x = 25;
				label = new Label();
				label.Name = stat + "Label";
				label.Location = new Point(x, y);
				label.Size = new Size(40, 20);
				label.Text = stat;
				groupBox1.Controls.Add(label);
				x += 45;

				textBox = new TextBox();
				textBox.Name = stat + "Worth";
				textBox.Location = new Point(x, y);
				textBox.Size = new Size(40, 20);
				if (build.Sort[stat] != 0)
					textBox.Text = build.Sort[stat].ToString();
                textBox.TextChanged += textBox_TextChanged;
                groupBox1.Controls.Add(textBox);

				y += 22;

                listView1.Columns.Add(stat).Width = 80;
			}
            foreach (string extra in extraNames)
            {
                x = 25;
                label = new Label();
                label.Name = extra + "Label";
                label.Location = new Point(x, y);
                label.Size = new Size(40, 20);
                label.Text = extra;
                groupBox1.Controls.Add(label);
                x += 45;

                textBox = new TextBox();
                textBox.Name = extra + "Worth";
                textBox.Location = new Point(x, y);
                textBox.Size = new Size(40, 20);
                if (build.Sort.ExtraGet(extra) != 0)
                    textBox.Text = build.Sort.ExtraGet(extra).ToString();
                textBox.TextChanged += textBox_TextChanged;
                groupBox1.Controls.Add(textBox);

                y += 22;

                listView1.Columns.Add(extra).Width = 80;
            }

			toolStripStatusLabel1.Text = "Generating...";
			building = true;
            toolStripProgressBar1.Maximum = buildsShow * 2;
            
            Task.Factory.StartNew(() =>
            {
                // Allow the window to draw before destroying the CPU
                Thread.Sleep(100);

                
                // Disregard locked, but honor equippedness checking
                build.GenRunes(Main.data, noLocked, Main.useEquipped);

                // generate 5000 builds
				build.GenBuilds(buildsGen, 0, (s) => { }, (d) =>
				{
					Invoke((MethodInvoker)delegate
					{
						toolStripProgressBar1.Value = (int)(d * buildsShow);
					});
				});
				if (build.loads == null)
				{
					toolStripStatusLabel1.Text = "Error: no builds";
					return;
				}

                int num = 0;
                var takenLoads = build.loads.Take(buildsShow);
                int tbuilds = takenLoads.Count();
                // pick the top 100
                // Believe it or not, putting 100 into the list takes a *lot* longer than making 5000
                foreach (var b in takenLoads)
				{
					ListViewItem li = new ListViewItem();
					var Cur = b.GetStats();

                    double pts = GetPoints(Cur, (str, i)=> { li.SubItems.Add(str); });
                    
                    // put the sum points into the first item
					li.SubItems[0].Text = pts.ToString("0.##");
					li.Tag = b;
                    if (grayLocked && b.Current.runes.Any(r => r.Locked))
                        li.ForeColor = Color.Gray;

					Invoke((MethodInvoker)delegate
					{
                        // put the thing in on the main thread and bump the progress bar
						listView1.Items.Add(li);
                        num++;
						toolStripProgressBar1.Value = buildsShow + (int)(buildsShow * num / (double)tbuilds);
					});
				}

				Invoke((MethodInvoker)delegate
				{
					toolStripStatusLabel1.Text = "Generated " + listView1.Items.Count + " builds";
					building = false;
				});

			});
        }

        void textBox_TextChanged(object sender, EventArgs e)
        {
            // if we are generating builds, don't recalculate all the builds
			if (building) return;

            // TODO: try to only mangle the column which is changing?

			foreach (string stat in statNames)
            {
                TextBox tb = (TextBox)Controls.Find(stat + "Worth", true).FirstOrDefault();
                double val = 0;
                double.TryParse(tb.Text, out val);
                build.Sort[stat] = val;
            }
            foreach (string extra in extraNames)
            {
                TextBox tb = (TextBox)Controls.Find(extra + "Worth", true).FirstOrDefault();
                double val = 0;
                double.TryParse(tb.Text, out val);
                build.Sort.ExtraSet(extra, val);
            }
            // "sort" as in, recalculate the whole number
            foreach (ListViewItem li in listView1.Items)
            {
                ListItemSort(li);
            }
            var lv = (ListView)listView1;
            var lvs = (ListViewSort)(lv).ListViewItemSorter;
            lvs.OnColumnClick(0, false, true);
            // actually sort the list, on points
			lv.Sort();
		}

        private double GetPoints(Stats Cur, Action<string, int> w = null)
        {
            double pts = 0;
            double p;
            int i = 1;
            foreach (var stat in statNames)
            {
                string str = Cur[stat].ToString();
                if (build.Sort[stat] != 0)
                {
                    p = Cur[stat] / build.Sort[stat];
                    if (build.Threshold[stat] != 0)
                        p -= Math.Max(0, Cur[stat] - build.Threshold[stat]) / build.Sort[stat];
                    str = p.ToString("0.#") + " (" + Cur[stat].ToString() + ")";
                    pts += p;
                }
                w.Invoke(str, i);
                i++;
            }
            foreach (var extra in extraNames)
            {
                string str = Cur.ExtraValue(extra).ToString();
                if (build.Sort.ExtraGet(extra) != 0)
                {
                    p = Cur.ExtraValue(extra) / build.Sort.ExtraGet(extra);
                    if (build.Threshold.ExtraGet(extra) != 0)
                        p -= Math.Max(0, Cur.ExtraValue(extra) - build.Threshold.ExtraGet(extra)) / build.Sort.ExtraGet(extra);
                    str = p.ToString("0.#") + " (" + Cur.ExtraValue(extra).ToString() + ")";
                    pts += p;
                }
                w.Invoke(str, i);
                i++;
            }
            return pts;
        }

        // recalculate all the points for this monster
        // TODO: consider hiding point values in the subitem tags and only recalcing the changed column
        // TODO: pull the scoring algorithm into a neater function
        public void ListItemSort(ListViewItem li)
        {
            Monster load = (Monster)li.Tag;
            var Cur = load.GetStats();

            double pts = GetPoints(Cur, (str, num) => { li.SubItems[num].Text = str; });
            
            li.SubItems[0].Text = pts.ToString("0.##");

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
			if (building) return;

            var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
            sorter.OnColumnClick(e.Column, false, true);
            ((ListView)sender).Sort();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            // Things went okay
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            // things were :(
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
		}

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem lit = listView1.Items[0];
                ListViewItem lis = listView1.SelectedItems[0];

                if (lit == lis)
                    return;

                Monster mY = (Monster)lis.Tag;
                Monster mN = (Monster)lit.Tag;

                List<string> better = new List<string>();

                building = true;

                foreach (var stat in statNames)
                {
                    if (mY.GetStats()[stat] > mN.GetStats()[stat])
                        better.Add(stat);
                }
                double totalsort = 0;
                foreach (var stat in statNames)
                {
                    if (build.Sort[stat] != 0)
                        totalsort += Math.Abs(build.Sort[stat]);
                }
                foreach (var extra in extraNames)
                {
                    if (build.Sort.ExtraGet(extra) != 0)
                        totalsort += Math.Abs(build.Sort.ExtraGet(extra));
                }
                if (totalsort == 0)
                {
                    double totalstats = 0;
                    foreach (var stat in better)
                    {
                        if (statNames.Contains(stat))
                            totalstats += mY.GetStats()[stat];
                        else
                            totalstats += mY.GetStats().ExtraValue(stat);
                    }
                    int amount = (int)Math.Max(30, Math.Sqrt(Math.Max(100, totalstats)));
                    foreach (var stat in better)
                    {
                        if (statNames.Contains(stat))
                        {
                            build.Sort[stat] = (int)(amount * (double)(mY.GetStats()[stat] / (double)totalstats));
                            TextBox tb = (TextBox)Controls.Find(stat + "Worth", true).FirstOrDefault();
                            tb.Text = build.Sort[stat].ToString();
                        }
                        else
                        {
                            build.Sort.ExtraSet(stat, (int)(amount * (double)(mY.GetStats().ExtraValue(stat) / (double)totalstats)));
                            TextBox tb = (TextBox)Controls.Find(stat + "Worth", true).FirstOrDefault();
                            tb.Text = build.Sort.ExtraGet(stat).ToString();
                        }
                    }
                }
                else
                {
                    // to do
                }

                building = false;
                textBox_TextChanged(null, null);
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (Main.help != null)
                Main.help.Close();

            Main.help = new Help();
            Main.help.url = Environment.CurrentDirectory + "\\User Manual\\test.html";
            Main.help.Show();
        }
    }
}
