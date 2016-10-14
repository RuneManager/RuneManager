using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RuneOptim;

namespace RuneApp
{
    // Specifying a new build, are we?
    public partial class Create : Form
    {
        public Build build = null;

        // controls builds version numbers
        public static int VERSIONNUM = 1;
        
        // Keep track of the runeset groups for speed swaps
        private ListViewGroup rsInc;
        private ListViewGroup rsExc;
        private ListViewGroup rsReq;

        // keep track of the rune hex controls
        private RuneControl[] runes;

        // the current rune to look at
        private Rune runeTest = null;
        // is the form loading? wouldn't want to trigger any OnChanges, eh?
        private bool loading = false;

        private LeaderType[] leadTypes = {
            new LeaderType(Attr.Null),
            new LeaderType(Attr.SpeedPercent).AddRange(new int[] { 0, 10, 13, 15, 16, 19, 23, 24, 28, 30, 33 }),
            new LeaderType(Attr.HealthPercent).AddRange(new int[] { 0, 18, 21, 30, 33, 44, 50 })
		};

        private static string[] tabNames = new string[] { "g", "o", "e", "2", "4", "6", "1", "3", "5" };
        private static string[] statNames = new string[] { "HP", "ATK", "DEF", "SPD", "CR", "CD", "RES", "ACC" };
        private static string[] extraNames = new string[] { "EHP", "EHPDB", "DPS", "AvD", "MxD" };

        private ToolTip tooltipNoSorting = new ToolTip();
        private ToolTip tooltipBadRuneFilter = new ToolTip();
        private ToolTip tooltipSets = new ToolTip();

        #region Leader skills
        public class LeaderType
        {
            public LeaderType(Attr t)
            {
                type = t;
            }

            public Attr type = Attr.Null;
            public class LeaderValue
            {
                public LeaderValue(Attr t, int v)
                {
                    type = t;
                    value = v;
                }
                public int value = 0;
                public Attr type = Attr.Null;
                public override string ToString()
                {
                    return value.ToString() + ((type == Attr.HealthFlat || type == Attr.DefenseFlat || type == Attr.AttackFlat || type == Attr.Speed) ? "" : "%");
                }
            }

            public void Add(int i)
            {
                values.Add(new LeaderValue(type, i));
            }

            public LeaderType AddRange(int[] ii)
            {
                foreach (int i in ii)
                    Add(i);

                return this;
            }

            public List<LeaderValue> values = new List<LeaderValue>();

            public override string ToString()
            {
                if (type == Attr.Null) return "(None)";
                return type.ToString();
            }
        }
    #endregion

        public Create()
        {
            InitializeComponent();
            // when show, check we have stuff
            Shown += Create_Shown;

            // declare the truthyness of the groups and track them
            setList.Groups[1].Tag = true;
            rsInc = setList.Groups[1];
            setList.Groups[2].Tag = false;
            rsExc = setList.Groups[2];
            setList.Groups[0].Tag = false;
            rsReq = setList.Groups[0];
            
            leaderTypeBox.Items.AddRange(leadTypes);

            // for each runeset, put it in the list as excluded
            foreach (var rs in Enum.GetNames(typeof(RuneSet)))
			{
                if (rs != "Null" && rs != "Broken")
                {
                    ListViewItem li = new ListViewItem(rs);
                    li.Name = rs;
                    li.Tag = Enum.Parse(typeof(RuneSet), rs);
                    li.Group = setList.Groups[2];
                    setList.Items.Add(li);
                }
			}

            // track all the clickable rune things
            runes = new RuneControl[] { runeControl1, runeControl2, runeControl3, runeControl4, runeControl5, runeControl6 };
            for (int i = 0; i < runes.Length; i++)
            {
                runes[i].Tag = i + 1;
            }

            toolStripButton1.Tag = 0;

            // there are lists on the rune filter tabs 2,4, and 6
            var lists = new ListView[]{ priStat2, priStat4, priStat6};
            for(int j = 0; j < lists.Length; j++)
            {
                // mess 'em up
                var lv = lists[j];

                // for all the attributes that may appears as primaries on runes
                for (int i = 0; i < statNames.Length; i++)
                {
					ListViewItem li = null;

                    string stat = statNames[i];
                    if (i < 3)
                    {
                        li = new ListViewItem(stat);
                        li.Name = stat + "flat";
                        li.Text = stat;
                        li.Tag = stat + "flat";
                        li.Group = lv.Groups[1];
                        
                        lv.Items.Add(li);
                        
                        li = new ListViewItem(stat);
                        li.Name = stat + "perc";
                        li.Text = stat + "%";
                        li.Tag = stat + "perc";
                        li.Group = lv.Groups[1];

                        lv.Items.Add(li);
                        
                    }
                    else
                    {
                        // only allow cool stats on the right slots
                        if (j == 0 && stat != "SPD")
                            continue;
                        if (j == 1 && (stat != "CR" && stat != "CD"))
                            continue;
                        if (j == 2 && (stat != "ACC" && stat != "RES"))
                            continue;

                        li = new ListViewItem(stat);
                        // put the right type on it
                        li.Name = stat + (stat == "SPD" ? "flat" : "perc");
                        li.Text = stat;
                        li.Tag = stat + (stat == "SPD" ? "flat" : "perc");
                        li.Group = lv.Groups[1];
                        
                        lv.Items.Add(li);
                    }
					 
                }
            }

            Control textBox;
            Label label;

            // make a grid for the monsters base, min/max stats and the scoring
            
            int x = 0;
            int y = 0;

            y = 94;

            int colWidth = 50;
            int rowHeight = 24;

            string[] comb = statNames.Concat(extraNames).ToArray();

            int genX = 0;

			int dlCheckX = 0;
			int dlBtnX = 0;

            #region Statbox
            foreach (var stat in comb)
            {
                x = 4; 
                label = new Label();
                groupBox1.Controls.Add(label);
                label.Name = stat + "Label";
                label.Text = stat;
                label.Size = new Size(50, 20);
                label.Location = new Point(x, y);
                x += colWidth;

                label = new Label();
                groupBox1.Controls.Add(label);
                label.Name = stat + "Base";
                label.Size = new Size(50, 20);
                label.Location = new Point(x, y);
				dlCheckX = x;
                x += colWidth;

                label = new Label();
                groupBox1.Controls.Add(label);
                label.Name = stat + "Bonus";
                label.Size = new Size(50, 20);
                label.Location = new Point(x, y);
                x += colWidth;

                textBox = new TextBox();
                groupBox1.Controls.Add(textBox);
                textBox.Name = stat + "Total";
                textBox.Size = new Size(40, 20);
                textBox.Location = new Point(x, y);
                textBox.TextChanged += new System.EventHandler(this.global_TextChanged);
                textBox.TextChanged += Total_TextChanged;
				dlBtnX = x;
                x += colWidth;



                label = new Label();
                groupBox1.Controls.Add(label);
                label.Name = stat + "Current";
                label.Size = new Size(50, 20);
                label.Location = new Point(x, y);
                x += colWidth;

                genX = x;

                textBox = new TextBox();
                groupBox1.Controls.Add(textBox);
                textBox.Name = stat + "Worth";
                textBox.Size = new Size(40, 20);
                textBox.Location = new Point(x, y);
                textBox.TextChanged += new System.EventHandler(this.global_TextChanged);
                x += colWidth;

                label = new Label();
                groupBox1.Controls.Add(label);
                label.Name = stat + "CurrentPts";
                label.Size = new Size((int)(50 * 0.8), 20);
                label.Location = new Point(x, y);
                x += (int)(colWidth * 0.8);

                textBox = new TextBox();
                groupBox1.Controls.Add(textBox);
                textBox.Name = stat + "Thresh";
                textBox.Size = new Size(40, 20);
                textBox.Location = new Point(x, y);
                textBox.TextChanged += new System.EventHandler(this.global_TextChanged);
                x += colWidth;

                textBox = new TextBox();
                groupBox1.Controls.Add(textBox);
                textBox.Name = stat + "Max";
                textBox.Size = new Size(40, 20);
                textBox.Location = new Point(x, y);
                textBox.TextChanged += new System.EventHandler(this.global_TextChanged);

                y += rowHeight;
            }
            #endregion

            testBuildButton.Location = new Point(genX, y);

			btnDL6star.Location = new Point(dlBtnX, y);
			checkDL6star.Location = new Point(dlCheckX, y + 2);

			btnDLawake.Location = new Point(dlBtnX, y + rowHeight);
			checkDLawake.Location = new Point(dlCheckX, y + rowHeight + 2);

			// put the grid on all the tabs
			foreach (var tab in tabNames)
            {
                TabPage page = tabControl1.TabPages["tab" + tab];
                page.Tag = tab;

                label = new Label();
                page.Controls.Add(label);
                label.Text = "Divide stats into points:";
                label.Name = tab + "divprompt";
                label.Size = new Size(140, 14);
                label.Location = new Point(6, 6);

                label = new Label();
                page.Controls.Add(label);
                label.Text = "Inherited";
                label.Name = tab + "inhprompt";
                label.Size = new Size(60, 14);
                label.Location = new Point(134, 6);

                label = new Label();
                page.Controls.Add(label);
                label.Text = "Current";
                label.Name = tab + "curprompt";
                label.Size = new Size(60, 14);
                label.Location = new Point(214, 6);

                ComboBox filterJoin = new ComboBox();
                filterJoin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                filterJoin.FormattingEnabled = true;
                filterJoin.Items.AddRange(new object[] {
                "Or",
                "And",
                "Sum"});
                filterJoin.Location = new System.Drawing.Point(298, 6);
                filterJoin.Name = tab + "join";
                filterJoin.Size = new System.Drawing.Size(72, 21);
                filterJoin.SelectedIndex = 0;

                filterJoin.SelectionChangeCommitted += filterJoin_SelectedIndexChanged;
                filterJoin.Tag = tab;
                page.Controls.Add(filterJoin);

                bool first = true;

                rowHeight = 25;
                colWidth = 42;

                int testX = 0;
                int testY = 0;
                int predX = 0;
                y = 45;
                foreach (var stat in statNames)
                {
                    label = new Label();
                    page.Controls.Add(label);
                    label.Name = tab + stat;
                    label.Location = new Point(5, y);
                    label.Text = stat;
                    label.Size = new Size(30, 20);

                    x = 35;
                    foreach (var pref in new string[] { "", "i", "c" })
                    {
                        foreach (var type in new string[] { "flat", "perc" })
                        {
                            if (first)
                            {
                                label = new Label();
                                page.Controls.Add(label);
                                label.Name = tab + pref + type;
                                label.Location = new Point(x, 25);
                                label.Text = pref + type;
                                if (type == "flat")
                                    label.Text = "Flat";
                                if (type == "perc")
                                    label.Text = "Percent";
                                label.Size = new System.Drawing.Size(45, 16);
                            }
                            
                            if (type == "perc" && stat == "SPD")
                            {
                                x += colWidth;
                                continue;
                            }
                            if (type == "flat" && (stat == "ACC" || stat == "RES" || stat == "CD" || stat == "CR"))
                            {
                                x += colWidth;
                                continue;
                            }

                            if (pref == "")
                            {
                                textBox = new TextBox();
                                textBox.TextChanged += new System.EventHandler(this.global_TextChanged);
                            }
                            else
                                textBox = new Label();
                            page.Controls.Add(textBox);
                            textBox.Name = tab + pref + stat + type;
                            textBox.Location = new Point(x, y - 2);

                            textBox.Size = new System.Drawing.Size(40, 20);
                            x += colWidth;
                        }
                    }

                    predX = x;

                    label = new Label();
                    page.Controls.Add(label);
                    label.Name = tab + stat + "gt";
                    label.Location = new Point(x, y);
                    label.Text = ">=";
                    label.Size = new System.Drawing.Size(30, 20);
                    x += colWidth;

                    testX = x;

                    textBox = new TextBox();
                    page.Controls.Add(textBox);
                    textBox.Name = tab + stat + "test";
                    textBox.Location = new Point(x, y - 2);
                    textBox.Size = new System.Drawing.Size(40, 20);
                    textBox.TextChanged += new System.EventHandler(this.global_TextChanged);
                    x += colWidth;

                    label = new Label();
                    page.Controls.Add(label);
                    label.Name = tab + "r" + stat + "test";
                    label.Location = new Point(x, y);
                    label.Size = new System.Drawing.Size(30, 20);
                    x += colWidth;

                    y += rowHeight;
                    testY = y;
                    first = false;
                }

                y += 8;
                x = predX;

                label = new Label();
                page.Controls.Add(label);
                label.Name = tab + "testGt";
                label.Location = new Point(x - 3, y);
                label.Text = "Sum >=";
                label.Size = new System.Drawing.Size(45, 20);
                x += colWidth;

                textBox = new TextBox();
                page.Controls.Add(textBox);
                textBox.Name = tab + "test";
                textBox.Location = new Point(x, y - 2);
                textBox.Size = new System.Drawing.Size(40, 20);
                textBox.TextChanged += new System.EventHandler(this.global_TextChanged);
                // default scoring is OR which doesn't need this box
                textBox.Enabled = false;
                x += colWidth;

                label = new Label();
                page.Controls.Add(label);
                label.Name = tab + "Check";
                label.Size = new Size(60, 14);
                label.Location = new Point(x, y);


                x = predX;
                y += rowHeight + 8;

                label = new Label();
                page.Controls.Add(label);
                label.Name = tab + "raiseLabel";
                label.Location = new Point(x, y);
                label.Text = "Make+";
                label.Size = new System.Drawing.Size(40, 20);
                x += colWidth;

                textBox = new TextBox();
                page.Controls.Add(textBox);
                textBox.Name = tab + "raise";
                textBox.Location = new Point(x, y-2);
                textBox.Size = new System.Drawing.Size(40, 20);
                textBox.TextChanged += new System.EventHandler(this.global_TextChanged);
                x += colWidth;

                label = new Label();
                page.Controls.Add(label);
                label.Name = tab + "raiseInherit";
                label.Location = new Point(x, y);
                label.Text = "0";
                label.Size = new System.Drawing.Size(40, 20);
                x += colWidth;

                x = predX;
                y += rowHeight;

                CheckBox check = new CheckBox();
                page.Controls.Add(check);
                check.Name = tab + "bonus";
                check.Location = new Point(x, y);
                check.Checked = false;
                check.Size = new System.Drawing.Size(17, 17);
                check.CheckedChanged += new System.EventHandler(this.global_CheckChanged);
                x += 17;

                label = new Label();
                page.Controls.Add(label);
                label.Name = tab + "bonusLabel";
                label.Location = new Point(x, y);
                label.Text = "Predict Subs";
                label.Size = new System.Drawing.Size(67, 20);
                label.Click += (s,e) => { check.Checked = !check.Checked; };
                x += colWidth;

                x += colWidth - 17;

                label = new Label();
                page.Controls.Add(label);
                label.Name = tab + "bonusInherit";
                label.Location = new Point(x, y);
                label.Text = "FT";
                label.Size = new System.Drawing.Size(80, 20);
                x += colWidth;

            }

        }
        
        private TabPage GetTab(string tabName)
        {
            // figure out which tab it's on
            int tabId = tabName == "g" ? 0 : tabName == "e" ? -2 : tabName == "o" ? -1 : int.Parse(tabName);
            TabPage tab = null;
            if (tabId <= 0)
                tab = tabControl1.TabPages[-tabId];
            if (tabId > 0)
            {
                if (tabId % 2 == 0)
                    tab = tabControl1.TabPages[2 + tabId / 2];
                else
                    tab = tabControl1.TabPages[6 + tabId / 2];
            }
            return tab;
        }

        // when the scoring type changes
        void filterJoin_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            string tabName = (string)box.Tag;
            TabPage tab = GetTab(tabName);
            Control ctrl;

            foreach (var stat in statNames)
            {
                foreach (var type in new string[] { "flat", "perc" })
                {
                    if (type == "perc" && stat == "SPD")
                        continue;
                    if (type == "flat" && (stat == "ACC" || stat == "RES" || stat == "CD" || stat == "CR"))
                        continue;
                    ctrl = tab.Controls.Find(tabName + stat + type, false).FirstOrDefault();
                    //ctrl.Enabled = (box.SelectedIndex == 2);
                }
                ctrl = tab.Controls.Find(tabName + stat + "test", false).FirstOrDefault();
                ctrl.Enabled = (box.SelectedIndex != 2);
                ctrl = tab.Controls.Find(tabName + "test", false).FirstOrDefault();
                ctrl.Enabled = (box.SelectedIndex == 2);

            }

            double test = 0;
            ctrl = tab.Controls.Find(tabName + "test", false).FirstOrDefault();
            double.TryParse(ctrl.Text, out test);
            
            if (!build.runeScoring.ContainsKey(tabName))
                build.runeScoring.Add(tabName, new KeyValuePair<int, double>(box.SelectedIndex, test));
            var kv = build.runeScoring[tabName] = new KeyValuePair<int, double>(box.SelectedIndex, test);

            // TODO: trim the ZERO nodes on the tree

            // retest the rune
            TestRune(runeTest);
        }

        // When the window is told to appear (hopefully we have everything)
        void Create_Shown(object sender, EventArgs e)
        {
            if (Tag == null)
            {
                // don't have. :(
                Close();
                return;
            }
            // warning, now loading
            loading = true;

            build = (Build)Tag;
            Monster mon = (Monster)build.mon;
            Stats cur = mon.GetStats();
            if (mon.ID != -1)
                monLabel.Text = "Build for " + mon.Name + " (" + mon.ID + ")";
            else
                monLabel.Text = "Build for " + build.MonName + " (" + mon.ID + ")";

            build.VERSIONNUM = VERSIONNUM;

			checkDL6star.Checked = build.DownloadStats;
			checkDLawake.Checked = build.DownloadAwake;
			checkDL6star.Enabled = !checkDLawake.Checked;

			check_autoRunes.Checked = build.autoRuneSelect;

            if (build.leader.NonZero())
            {
                Attr t = build.leader.FirstNonZero();
                if (t != Attr.Null)
                {
                    leaderTypeBox.SelectedIndex = leadTypes.ToList().FindIndex(lt => lt.type == t);
                }
            }

            // move the sets around in the list a little
            foreach (RuneSet s in build.BuildSets)
            {
                ListViewItem li = setList.Items.Find(s.ToString(), true).FirstOrDefault();
                if (li == null)
                    li.Group = rsExc;
                if (li != null)
                    li.Group = rsInc;
            }
            foreach (RuneSet s in build.RequiredSets)
            {
                ListViewItem li = setList.Items.Find(s.ToString(), true).FirstOrDefault();
                if (li != null)
                    li.Group = rsReq;
				int num = build.RequiredSets.Count(r => r == s);
				if (num > 1)
					li.Text = s.ToString() + " x" + num;
				
            }

            // for 2,4,6 - make sure that the Attrs are set up
            var lists = new ListView[]{ priStat2, priStat4, priStat6};
            for (int j = 0; j < lists.Length; j++)
            {
                var lv = lists[j];

				var attrs = Enum.GetValues(typeof(Attr));
				for (int i = 0; i < statNames.Length; i++)
                {
                    var bl = build.slotStats[(j+1)*2 - 1];

					
                    string stat = statNames[i];
                    if (i < 3)
                    {
                        if (bl.Contains(stat + "flat"))
                            lv.Items.Find(stat + "flat", true).FirstOrDefault().Group = lv.Groups[0];

                        if (bl.Contains(stat + "perc") || build.New)
                            lv.Items.Find(stat + "perc", true).FirstOrDefault().Group = lv.Groups[0];
                    }
                    else
                    {
                        if (j == 0 && stat != "SPD")
                            continue;
                        if (j == 1 && (stat != "CR" && stat != "CD"))
                            continue;
                        if (j == 2 && (stat != "ACC" && stat != "RES"))
                            continue;

                        if (bl.Contains(stat + (stat == "SPD" ? "flat" : "perc")) || build.New)
                            lv.Items.Find(stat + (stat == "SPD" ? "flat" : "perc"), true).FirstOrDefault().Group = lv.Groups[0];
                    }
                }

            }

			refreshStats(mon, cur);

            // do we allow broken sets?
            ChangeBroken(build.AllowBroken);

            // for each tabs filter
            foreach (var rs in build.runeFilters)
            {
                var tab = rs.Key;
                TabPage ctab = GetTab(tab);
                // for each stats filter
                foreach (var f in rs.Value)
                {
                    var stat = f.Key;
                    // for each stat type
                    foreach (var type in new string[] { "flat", "perc", "test" })
                    {
                        // find the controls and shove the value in it
                        var ctrl = ctab.Controls.Find(tab + stat + type, true).FirstOrDefault();
                        if (ctrl != null)
                        {
                            double? val = f.Value[type];
                            // unless it's zero, I don't want zeros
                            if (val != null)
                                ctrl.Text = val.Value.ToString("0.##");
                            else
                                ctrl.Text = "";
                        }
                    }
                }
            }
            foreach (var tab in build.runePrediction)
            {
                TabPage ctab = GetTab(tab.Key);
                TextBox tb = (TextBox)ctab.Controls.Find(tab.Key + "raise", true).FirstOrDefault();
                if (tb != null)
                {
                    if (tab.Value.Key != 0)
                        tb.Text = tab.Value.Key.ToString();
                }
                CheckBox cb = (CheckBox)ctab.Controls.Find(tab.Key + "bonus", true).FirstOrDefault();
                if (cb != null)
                {
                    cb.Checked = tab.Value.Value;
                }
            }

            // for each tabs scoring
            foreach (var tab in build.runeScoring)
            {
                TabPage ctab = GetTab(tab.Key);
                // find that box
                ComboBox box = (ComboBox)ctab.Controls.Find(tab.Key + "join", true).FirstOrDefault();
                if (box != null)
                {
                    // manually kajigger it
                    box.SelectedIndex = tab.Value.Key;
                    foreach (var stat in statNames)
                    {
                        Control ctrl;
                        foreach (var type in new string[] { "flat", "perc" })
                        {
                            if (type == "perc" && stat == "SPD")
                                continue;
                            if (type == "flat" && (stat == "ACC" || stat == "RES" || stat == "CD" || stat == "CR"))
                                continue;
                            ctrl = ctab.Controls.Find(tab.Key + stat + type, true).FirstOrDefault();
                        }
                        ctrl = ctab.Controls.Find(tab.Key + stat + "test", true).FirstOrDefault();
                        ctrl.Enabled = (box.SelectedIndex != 2);
                        ctrl = ctab.Controls.Find(tab.Key + "test", true).FirstOrDefault();
                        ctrl.Enabled = (box.SelectedIndex == 2);
                    }
                }

                var tb = (TextBox)ctab.Controls.Find(tab.Key + "test", true).FirstOrDefault();
                if (tab.Value.Value != 0)
                    tb.Text = tab.Value.Value.ToString();
            }
            // done loading!
            loading = false;
            // this build is no longer considered new
            build.New = false;
            // oh yeah, update everything now that it's finally loaded
            UpdateGlobal();
            CalcPerms();
        }

		private void refreshStats(Monster mon, Stats cur)
		{
			statName.Text = mon.Name;
			statID.Text = mon.ID.ToString();
			statLevel.Text = mon.level.ToString();

			// read a bunch of numbers
			foreach (var stat in statNames)
			{
				var ctrlBase = (Label)groupBox1.Controls.Find(stat + "Base", true).FirstOrDefault();
				ctrlBase.Text = mon[stat].ToString();

				var ctrlBonus = (Label)groupBox1.Controls.Find(stat + "Bonus", true).FirstOrDefault();
				var ctrlTotal = (TextBox)groupBox1.Controls.Find(stat + "Total", true).FirstOrDefault();

				ctrlTotal.Tag = new KeyValuePair<Label, Label>(ctrlBase, ctrlBonus);

				var ctrlCurrent = groupBox1.Controls.Find(stat + "Current", true).FirstOrDefault();
				ctrlCurrent.Text = cur[stat].ToString();

				var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();

				var ctrlThresh = groupBox1.Controls.Find(stat + "Thresh", true).FirstOrDefault();

				var ctrlMax = groupBox1.Controls.Find(stat + "Max", true).FirstOrDefault();

				if (build.Minimum[stat] > 0)
					ctrlTotal.Text = build.Minimum[stat].ToString();
				if (build.Sort[stat] != 0)
					ctrlWorth.Text = build.Sort[stat].ToString();
				if (build.Maximum[stat] != 0)
					ctrlMax.Text = build.Maximum[stat].ToString();
				if (build.Threshold[stat] != 0)
					ctrlThresh.Text = build.Threshold[stat].ToString();

			}

			foreach (var extra in extraNames)
			{
				var ctrlBase = (Label)groupBox1.Controls.Find(extra + "Base", true).FirstOrDefault();
				ctrlBase.Text = mon.ExtraValue(extra).ToString();

				var ctrlBonus = (Label)groupBox1.Controls.Find(extra + "Bonus", true).FirstOrDefault();
				var ctrlTotal = (TextBox)groupBox1.Controls.Find(extra + "Total", true).FirstOrDefault();

				ctrlTotal.Tag = new KeyValuePair<Label, Label>(ctrlBase, ctrlBonus);

				var ctrlCurrent = groupBox1.Controls.Find(extra + "Current", true).FirstOrDefault();
				ctrlCurrent.Text = cur.ExtraValue(extra).ToString();

				var ctrlWorth = groupBox1.Controls.Find(extra + "Worth", true).FirstOrDefault();

				var ctrlThresh = groupBox1.Controls.Find(extra + "Thresh", true).FirstOrDefault();

				var ctrlMax = groupBox1.Controls.Find(extra + "Max", true).FirstOrDefault();

				if (build.Minimum.ExtraGet(extra) > 0)
					ctrlTotal.Text = build.Minimum.ExtraGet(extra).ToString();
				if (build.Sort.ExtraGet(extra) != 0)
					ctrlWorth.Text = build.Sort.ExtraGet(extra).ToString();
				if (build.Maximum.ExtraGet(extra) != 0)
					ctrlMax.Text = build.Maximum.ExtraGet(extra).ToString();
				if (build.Threshold.ExtraGet(extra) != 0)
					ctrlThresh.Text = build.Threshold.ExtraGet(extra).ToString();
			}

		}

		// switch the cool icon on the button (and the bool in the build)
		private void ChangeBroken(bool state)
        {
            toolStripButton2.Tag = state;
            if (state)
            {
                toolStripButton2.Image = global::RuneApp.App.broken;
            }
            else
            {
                toolStripButton2.Image = global::RuneApp.App.whole;
            }
            if (build != null)
                build.AllowBroken = state;
        }

        // The minimum value was changed
        private void Total_TextChanged(object sender, EventArgs e)
        {
            var total = (TextBox)sender;

            if (total.Tag == null)
                return;

            // pull the BASE and BONUS controls from the tag
            var kv = (KeyValuePair<Label, Label>)total.Tag;
            var bonus = kv.Value;
            var mBase = kv.Key;

            // did someone put a k in?
            if (total.Text.ToLower().Contains("k"))
            {
                total.Text = (double.Parse(total.Text.ToLower().Replace("k", "")) * 1000).ToString();
            }

            // calculate the difference between base and minimum, put it in bonus
            var hasPercent = mBase.Text.Contains("%");
            double val = 0;
			if (double.TryParse(total.Text.Replace("%", ""), out val))
			{
                double sub = 0;
                double.TryParse(mBase.Text.Replace("%", ""), out sub);
				val -= sub;
			}
            if (val < 0)
                val = 0;

            bonus.Text = "+" + (val) + (hasPercent ? "%" : "");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (build == null)
                return;

            var list = (ListView)sender;

            var items = list.SelectedItems;
            if (items.Count == 0)
                return;

            ListViewItem item = items[0];

            ChangeMove(build.BuildSets.Contains((RuneSet)item.Tag));
            ChangeReq(build.RequiredSets.Contains((RuneSet)item.Tag));
        }

        // flip the icon for the INCLUDE set button
        private void ChangeMove(bool dir)
        {
            toolStripButton1.Tag = dir;
            if (!dir)
            {
                toolStripButton1.Image = global::RuneApp.App.add;
                toolStripButton1.Text = "Include selected set";
            }
            else
            {
                toolStripButton1.Image = global::RuneApp.App.subtract;
                toolStripButton1.Text = "Exclude selected set";
            }
            updatePerms();
        }

        // flip the icon for the REQUIRED set button
        private void ChangeReq(bool dir)
        {
            toolStripButton3.Tag = dir;
            if (!dir)
            {
                toolStripButton3.Image = global::RuneApp.App.up;
                toolStripButton3.Text = "Require selected set";
            }
            else
            {
                toolStripButton3.Image = global::RuneApp.App.down;
                toolStripButton3.Text = "Option selected set";
            }
            updatePerms();

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ChangeBroken(!(bool)((ToolStripButton)sender).Tag);
        }

        // shuffle runesets between: included <-> excluded
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (build == null)
                return;

            var list = setList;

            var items = list.SelectedItems;
            if (items.Count == 0)
                return;

            var selGrp = items[0].Group;
            var indGrp = selGrp.Items.IndexOf(items[0]);

            // shift all the selected things to the place
            foreach (ListViewItem item in items)
            {
                var set = (RuneSet)item.Tag;
                if (build.BuildSets.Contains(set))
                {
                    build.BuildSets.Remove(set);
                    item.Group = rsExc;
                }
                else
                {
                    build.BuildSets.Add(set);
                    item.Group = rsInc;
                }
            }

            var ind = items[0].Index;
            
            // maybe try to do cool reselecting the next entry
            if (selGrp.Items.Count > 0)
            {
                int i = indGrp;
                if (i > 0 && (i == selGrp.Items.Count || !(bool)selGrp.Tag))
                    i -= 1;
                while (selGrp.Items[i].Selected)
                {
                    i++;
                    i %= selGrp.Items.Count;
                    if (i == indGrp)
                        break;

                }

                ind = selGrp.Items[i].Index;
            }
            setList.SelectedIndices.Clear();
            
            setList.SelectedIndices.Add(ind);
            listView1_SelectedIndexChanged(setList, null);
        }

        // shuffle rnuesets between: excluded > required <-> included
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (build == null)
                return;

            var list = setList;

            var items = list.SelectedItems;
            if (items.Count == 0)
                return;

            var selGrp = items[0].Group;
            var indGrp = selGrp.Items.IndexOf(items[0]);

            foreach (ListViewItem item in items)
            {
                var set = (RuneSet)item.Tag;
                if (build.RequiredSets.Contains(set))
                {
                    build.RequiredSets.RemoveAll(s => s == set);
                    if (!build.BuildSets.Contains(set))
                        build.BuildSets.Add(set);
                    item.Group = rsInc;
					item.Text = set.ToString();
                }
                else
                {
                    build.RequiredSets.Add(set);
                    if (!build.BuildSets.Contains(set))
                        build.BuildSets.Add(set);
                    item.Group = rsReq;
                }
            }

            var ind = items[0].Index;

            if (selGrp.Items.Count > 0)
            {
                int i = indGrp;
				int selcount = 0;
                if (i > 0 && (i == selGrp.Items.Count || !(bool)selGrp.Tag))
                    i -= 1;
                while (selGrp.Items[i].Selected)
                {
					selcount++;
                    i++;
                    i %= selGrp.Items.Count;
					if (selcount == selGrp.Items.Count)
						break;
                }

                ind = selGrp.Items[i].Index;
            }
            setList.SelectedIndices.Clear();

            setList.SelectedIndices.Add(ind);
            listView1_SelectedIndexChanged(setList, null);
        }

        // if you click on a little rune
        private void runeControl_Click(object sender, EventArgs e)
        {
            // reset all the gammas
            foreach (RuneControl t in runes)
            {
                t.Gamma = 1;
                t.Refresh();
            }

            RuneControl tc = ((RuneControl)sender);
            
            // darken? wut
            tc.Gamma = 1.4f;
            // redraw that
            tc.Refresh();

            // good idea, generate right now whenever the user clicks a... whatever
            build.GenRunes(Main.data, false, Main.useEquipped);

            using (RuneSelect rs = new RuneSelect())
            {
                rs.returnedRune = build.mon.Current.Runes[(int)tc.Tag - 1];
                rs.build = build;
                rs.slot = ((int)tc.Tag).ToString();
                rs.runes = build.runes[(int)tc.Tag-1];
                rs.ShowDialog();
            }

        }

        private void UpdateGlobal()
        {
            // if the window is loading, try not to save the window
            if (loading)
                return;

            foreach (string tab in tabNames)
            {
                foreach (string stat in statNames)
                {
                    UpdateStat(tab, stat);
                }
                if (!build.runeScoring.ContainsKey(tab) && build.runeFilters.ContainsKey(tab))
                {
                    // if there is a non-zero
                    if (build.runeFilters[tab].Any(r => r.Value.NonZero))
                    {
                        build.runeScoring.Add(tab, new KeyValuePair<int, double>(0,0));
                    }
                }
                if (build.runeScoring.ContainsKey(tab))
                {
                    var kv = build.runeScoring[tab];
                    var ctrlTest = Controls.Find(tab + "test", true).FirstOrDefault();
                    if (ctrlTest.Text != "")
                        build.runeScoring[tab] = new KeyValuePair<int, double>(kv.Key, double.Parse(ctrlTest.Text));
                }
                TextBox tb = (TextBox)this.Controls.Find(tab + "raise", true).FirstOrDefault();
                int raiseLevel = 0;
                if (!int.TryParse(tb.Text, out raiseLevel))
                    raiseLevel = -1;
                CheckBox cb = (CheckBox)this.Controls.Find(tab + "bonus", true).FirstOrDefault();
                /*if (!build.runePrediction.ContainsKey(tab))
                {
                    build.runePrediction.Add(tab, new KeyValuePair<int, bool>(raiseLevel, cb.Checked));
                }
                if (build.runePrediction.ContainsKey(tab))
                {
                    var kv = build.runePrediction[tab];*/
                    if (raiseLevel == -1 && !cb.Checked)
                {
                    build.runePrediction.Remove(tab);
                }
                    else
                    build.runePrediction[tab] = new KeyValuePair<int, bool>(raiseLevel, cb.Checked);
                //}
                if (build.runePrediction.ContainsKey(tab))
                {
                    var kv = build.runePrediction[tab];
                    var ctrlRaise = Controls.Find(tab + "raiseInherit", true).FirstOrDefault();
                    ctrlRaise.Text = kv.Key.ToString();
                    var ctrlPred = Controls.Find(tab + "bonusInherit", true).FirstOrDefault();
                    ctrlPred.Text = (kv.Value ? "T" : "F");

                }

            }
            foreach (string stat in statNames)
            {
                double val = 0; double total = 0;
                var ctrlTotal = groupBox1.Controls.Find(stat + "Total", true).FirstOrDefault();
                double.TryParse(ctrlTotal.Text, out val);
                    total = val;
                build.Minimum[stat] = val;

                var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();
                val = 0; double worth = 0;
                if (double.TryParse(ctrlWorth.Text, out val))
                    worth = val;
                build.Sort[stat] = val;

                var ctrlMax = groupBox1.Controls.Find(stat + "Max", true).FirstOrDefault();
                val = 0; double max = 0;
                if (double.TryParse(ctrlMax.Text, out val))
                    max = val;
                build.Maximum[stat] = val;

                var ctrlThresh = groupBox1.Controls.Find(stat + "Thresh", true).FirstOrDefault();
                val = 0; double thr = 0;
                if (double.TryParse(ctrlThresh.Text, out val))
                    thr = val;
                build.Threshold[stat] = val;

                var ctrlCurrent = groupBox1.Controls.Find(stat + "Current", true).FirstOrDefault();
                val = 0; double current = 0;
                if (double.TryParse(ctrlCurrent.Text, out val))
                    current = val;
                var ctrlWorthPts = groupBox1.Controls.Find(stat + "CurrentPts", true).FirstOrDefault();
                if (worth != 0 && current != 0)
                {
                    double pts = current;
                    if (max != 0)
                        pts = Math.Min(max, current);
                    ctrlWorthPts.Text = ((double)(pts / (double)worth)).ToString("0.##");
                }
            }

            foreach (string extra in extraNames)
            {
                var ctrlTotal = groupBox1.Controls.Find(extra + "Total", true).FirstOrDefault();
                double val = 0; double total = 0;
                if (double.TryParse(ctrlTotal.Text, out val))
                    total = val;
                build.Minimum.ExtraSet(extra, val);
                var ctrlWorth = groupBox1.Controls.Find(extra + "Worth", true).FirstOrDefault();
                val = 0; double worth = 0;
                if (double.TryParse(ctrlWorth.Text, out val))
                    worth = val;
                build.Sort.ExtraSet(extra, val);

                var ctrlMax = groupBox1.Controls.Find(extra + "Max", true).FirstOrDefault();
                val = 0; double max = 0;
                if (double.TryParse(ctrlMax.Text, out val))
                    max = val;
                build.Maximum.ExtraSet(extra, val);

                var ctrlThresh = groupBox1.Controls.Find(extra + "Thresh", true).FirstOrDefault();
                val = 0; double thr = 0;
                if (double.TryParse(ctrlThresh.Text, out val))
                    thr = val;
                build.Threshold.ExtraSet(extra, val);

                var ctrlCurrent = groupBox1.Controls.Find(extra + "Current", true).FirstOrDefault();
                val = 0; double current = 0;
                if (double.TryParse(ctrlCurrent.Text, out val))
                    current = val;
                var ctrlWorthPts = groupBox1.Controls.Find(extra + "CurrentPts", true).FirstOrDefault();
                if (worth != 0 && current != 0)
                {
                    double pts = current;
                    if (max != 0)
                        pts = Math.Min(max, current);
                    ctrlWorthPts.Text = ((double)(pts / (double)worth)).ToString("0.##");
                }
                else
                {
                    ctrlWorthPts.Text = "";
                }
            }

            var lists = new ListView[]{ priStat2, priStat4, priStat6};
            for (int j = 0; j < lists.Length; j++)
            {
                var lv = lists[j];
                var bl = build.slotStats[(j + 1) * 2 - 1];
                bl.Clear();

                for (int i = 0; i < statNames.Length; i++)
                {
                    string stat = statNames[i];
                    if (i < 3)
                    {
                        if (lv.Items.Find(stat + "flat", true).FirstOrDefault().Group == lv.Groups[0])
                            bl.Add(stat + "flat");
                        if (lv.Items.Find(stat + "perc", true).FirstOrDefault().Group == lv.Groups[0])
                            bl.Add(stat + "perc");

                    }
                    else
                    {
                        if (j == 0 && stat != "SPD")
                            continue;
                        if (j == 1 && (stat != "CR" && stat != "CD"))
                            continue;
                        if (j == 2 && (stat != "ACC" && stat != "RES"))
                            continue;

                        if (lv.Items.Find(stat + (stat == "SPD" ? "flat" : "perc"), true).FirstOrDefault().Group == lv.Groups[0])
                            bl.Add(stat + (stat == "SPD" ? "flat" : "perc"));
                    }
                }
            }

            TestRune(runeTest);

			updatePerms();
		}

		private void UpdateStat(string tab, string stat)
        {
            TabPage ctab = GetTab(tab);
            var ctest = ctab.Controls.Find(tab + stat + "test", true).First();
            double tt;
            double? test = 0;
            double.TryParse(ctest.Text, out tt);
            test = tt;
            if (ctest.Text.Length == 0)
                test = null;

            if (!build.runeFilters.ContainsKey(tab))
            {
                build.runeFilters.Add(tab, new Dictionary<string, RuneFilter>());
            }
            var fd = build.runeFilters[tab];
            if (!fd.ContainsKey(stat))
            {
                fd.Add(stat, new RuneFilter());
            }
            var fi = fd[stat];
            
            foreach (string type in new string[] { "flat", "perc" })
            {
                if (type == "perc" && stat == "SPD")
                {
                    continue;
                }
                if (type == "flat" && (stat == "ACC" || stat == "RES" || stat == "CD" || stat == "CR"))
                {
                    continue;
                }

                if (tab == "g")
                    ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = "";
                else if (tab == "e" || tab == "o")
                {
                    ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = tabg.Controls.Find("gc" + stat + type, true).First().Text;
                }
                else
                {
                    int s = int.Parse(tab);
                    if (s % 2 == 0)
                        ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = tabe.Controls.Find("ec" + stat + type, true).First().Text;
                    else
                        ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = tabo.Controls.Find("oc" + stat + type, true).First().Text;
                }

                var c = ctab.Controls.Find(tab + "c" + stat + type, true).First();

                double i = 0;
                double t = 0;
                var ip = double.TryParse(ctab.Controls.Find(tab + "i" + stat + type, true).First().Text, out i);
                var tp = double.TryParse(ctab.Controls.Find(tab + stat + type, true).First().Text, out t);

                if (ip)
                {
                    if (tp)
                        c.Text = t.ToString();//Math.Min(i, t).ToString();
                    else
                        c.Text = i.ToString();
                }
                else
                {
                    if (tp)
                        c.Text = t.ToString();
                    else
                        c.Text = "";
                }
                
                fi[type] = t;
                
                if (ctab.Controls.Find(tab + stat + type, true).First().Text.Length == 0)
                {
                    fi[type] = null;
                }

            }

            fi.Test = test;
        }

        private void TestRune(Rune rune)
        {
            if (rune == null)
                return;

            // consider moving to the slot tab for the rune
            foreach (var tab in tabNames)
            {
                TestRuneTab(rune, tab);
            }
        }

        private double DivCtrl(double val, string tab, string stat, string type)
        {
            var ctrls = this.Controls.Find(tab + "c" + stat + type, true);
            if (ctrls.Length == 0)
                return 0;

            var ctrl = ctrls[0];
            double num = 1;
            if (double.TryParse(ctrl.Text, out num))
            {
                if (num == 0)
                    return 0;

                return val / num;
            }
            else
                return 0;
        }

        private bool GetPts(Rune rune, string tab, string stat, ref double points, int fake, bool pred)
        {
            double pts = 0;

            PropertyInfo[] props = typeof(Rune).GetProperties();
            foreach (var prop in props)
            {
                
            }

            pts += DivCtrl(rune[stat + "flat", fake, pred], tab, stat, "flat");
            pts += DivCtrl(rune[stat + "perc", fake, pred], tab, stat, "perc");
            points += pts;

            var lCtrls = this.Controls.Find(tab + "r" + stat + "test", true);
            var tbCtrls = this.Controls.Find(tab + stat + "test", true);
            if (lCtrls.Length != 0 && tbCtrls.Length != 0)
            {
                var tLab = (Label)lCtrls[0];
                var tBox = (TextBox)tbCtrls[0];
                tLab.Text = pts.ToString();
                tLab.ForeColor = Color.Black;
                double vs = 1;
                if (double.TryParse(tBox.Text, out vs))
                {
                    if (pts >= vs)
                    {
                        tLab.ForeColor = Color.Green;
                        return true;
                    }
                    else
                    {
                        tLab.ForeColor = Color.Red;
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        private void TestRuneTab(Rune rune, string tab)
        {
            bool res = false;
            if (!build.runeScoring.ContainsKey(tab))
                return;

            int fake = 0;
            bool pred = false;
            if (build.runePrediction.ContainsKey(tab))
            {
                fake = build.runePrediction[tab].Key;
                pred = build.runePrediction[tab].Value;
            }

            var kv = build.runeScoring[tab];
            int scoring = kv.Key;
            if (scoring == 1)
                res = true;

            double points = 0;
            foreach (var stat in statNames)
            {
                bool s = GetPts(rune, tab, stat, ref points, fake, pred);
                if (scoring == 1)
                    res &= s;
                else if (scoring == 0)
                    res |= s;
            }

            var ctrl = Controls.Find(tab + "Check", true).FirstOrDefault();

            ctrl.Text = res.ToString();

            if (scoring == 2)
            {
                ctrl.Text = points.ToString("#.##");
                ctrl.ForeColor = Color.Red;
                if (points >= build.runeScoring[tab].Value)
                    ctrl.ForeColor = Color.Green;
            }

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            build.GenRunes(Main.data, false, true);
            using (var ff = new RuneSelect())
            {
                ff.returnedRune = runeTest;
                ff.build = build;

                switch (tabControl1.SelectedTab.Text)
                {
                    case "Evens":
                        ff.runes = Main.data.Runes.Where(r => r.Slot % 2 == 0);
                        List<Rune> fr = new List<Rune>();
                        fr.AddRange(ff.runes.Where(r => r.Slot == 2 && build.slotStats[1].Contains(r.MainType.ToForms())));
                        fr.AddRange(ff.runes.Where(r => r.Slot == 4 && build.slotStats[3].Contains(r.MainType.ToForms())));
                        fr.AddRange(ff.runes.Where(r => r.Slot == 6 && build.slotStats[5].Contains(r.MainType.ToForms())));
                        ff.runes = fr;
                        break;
                    case "Odds":
                        ff.runes = Main.data.Runes.Where(r => r.Slot % 2 == 1);
                        break;
                    case "Global":
                        ff.runes = Main.data.Runes;
                        break;
                    default:
                        int slot = int.Parse(tabControl1.SelectedTab.Text);
                        ff.runes = Main.data.Runes.Where(r => r.Slot == slot);
                        break;
                }

                ff.slot = tabControl1.SelectedTab.Text;

                ff.runes = ff.runes.Where(r => build.BuildSets.Contains(r.Set));

                var res = ff.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    Rune rune = ff.returnedRune;
                    if (rune != null)
                    {
                        runeTest = rune;
                        TestRune(rune);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (AnnoyUser()) return;
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void global_TextChanged(object sender, EventArgs e)
        {
            TextBox text = (TextBox)sender;

            if (text.Text.ToLower().Contains("k"))
                text.Text = (double.Parse(text.Text.ToLower().Replace("k", "")) * 1000).ToString();

            UpdateGlobal();
        }

        private bool AnnoyUser()
        {

            if (build.runeFilters == null)
            {
                tabg.Select();
                var ctrl = tabg.Controls.Find("gSPDflat", false).FirstOrDefault();
                tooltipBadRuneFilter.IsBalloon = true;
                tooltipBadRuneFilter.Show(string.Empty, ctrl);
                tooltipBadRuneFilter.Show("Filters are nice", ctrl);
                return true;
            }

            foreach (var tbf in build.runeFilters)
            {
                if (build.runeScoring.ContainsKey(tbf.Key))
                {
                    int and = build.runeScoring[tbf.Key].Key;
                    double sum = 0;

                    foreach (var rbf in tbf.Value)
                    {
                        if (rbf.Value.Flat.HasValue)
                            sum += rbf.Value.Flat.Value;
                        if (rbf.Value.Percent.HasValue)
                            sum += rbf.Value.Percent.Value;
                        if (and != 2)
                        {
                            if (rbf.Value.Test == 0)
                            {
                                if (rbf.Value.Flat > 0 || rbf.Value.Percent > 0)
                                {
                                    if (tabControl1.TabPages.ContainsKey("tab" + tbf.Key))
                                    {
                                        var tab = tabControl1.TabPages["tab" + tbf.Key];
                                        tabControl1.SelectTab(tab);
                                        var ctrl = tab.Controls.Find(tbf.Key + rbf.Key + "test", false).FirstOrDefault();
                                        tooltipBadRuneFilter.IsBalloon = true;
                                        tooltipBadRuneFilter.Show(string.Empty, ctrl);
                                        tooltipBadRuneFilter.Show("GEQ how much?", ctrl, 0);
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                if (rbf.Value.Flat + rbf.Value.Percent == 0)
                                {
                                    var tab = tabControl1.TabPages["tab" + tbf.Key];
                                    tabControl1.SelectTab(tab);
                                    var ctrl = tab.Controls.Find(tbf.Key + rbf.Key, false).FirstOrDefault();
                                    tooltipBadRuneFilter.IsBalloon = true;
                                    tooltipBadRuneFilter.Show(string.Empty, ctrl);
                                    tooltipBadRuneFilter.Show("Counts for what?", ctrl, 0);
                                    return true;
                                }
                            }
                        }
                    }
                    if (and == 2 && sum > 0 && build.runeScoring[tbf.Key].Value == 0)
                    {
                        if (tabControl1.TabPages.ContainsKey("tab" + tbf.Key))
                        {
                            var tab = tabControl1.TabPages["tab" + tbf.Key];
                            tabControl1.SelectTab(tab);
                            var ctrl = tab.Controls.Find(tbf.Key + "test", false).FirstOrDefault();
                            tooltipBadRuneFilter.IsBalloon = true;
                            tooltipBadRuneFilter.Show(string.Empty, ctrl);
                            tooltipBadRuneFilter.Show("GEQ how much?", ctrl, 0);
                            return true;
                        }
                    }
                }
            }

            if (!build.Sort.NonZero())
            {
                var ctrl = groupBox1.Controls.Find("SPDWorth", false).FirstOrDefault();
                if (ctrl != null)
                {
                    tooltipNoSorting.IsBalloon = true;
                    tooltipNoSorting.Show(string.Empty, ctrl);
                    tooltipNoSorting.Show("Enter a value somewhere, please.\nLike 1 or 3.14", ctrl, 0);
                    return true;
                }
            }

            if (build.BuildSets == null && build.RequiredSets == null)
            {
                tooltipSets.IsBalloon = true;
                tooltipSets.Show(string.Empty, setList);
                tooltipSets.Show("No sets", setList, 0);
                return true;
            }
            else
            {
                if (build.BuildSets.Count + build.RequiredSets.Count == 0)
                {
                    tooltipSets.IsBalloon = true;
                    tooltipSets.Show(string.Empty, setList);
                    tooltipSets.Show("No sets", setList, 0);
                    return true;
                }
                if (!build.AllowBroken)
                {
                    bool has2 = false;
                    bool has4 = false;
                    foreach (var s in build.BuildSets)
                    {
                        if (Rune.SetRequired(s) == 2)
                            has2 = true;
                        else if (Rune.SetRequired(s) == 4)
                            has4 = true;
                    }
                    // never annoy because only 4 will include all 2
                    if (false && !has2 && has4 && !build.autoRuneSelect)
                    {
                        tooltipSets.IsBalloon = true;
                        tooltipSets.Show(string.Empty, setList);
                        tooltipSets.Show("Need 2 and 4 set for non-broken", setList, 0);
                        return true;
                    }
                }
            }

            if (CalcPerms() == 0)
            {
                tooltipBadRuneFilter.IsBalloon = true;
                tooltipBadRuneFilter.Show(string.Empty, runeNums);
                tooltipBadRuneFilter.Show("No builds\nAdd more sets, lax your rune filters, or unlock runes.", runeNums, 0);
                return true;
            }
            return false;
        }

        private void global_CheckChanged(object sender, EventArgs e)
        {
            CheckBox check = (CheckBox)sender;
            
            UpdateGlobal();
        }

        private void testBuildClick(object sender, EventArgs e)
        {
            if (build == null)
                return;

            if (AnnoyUser()) return;

            if (MessageBox.Show("This will generate builds", "Continue?", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                // make a new form that generates builds into it
                // have a weight panel near the stats
                // sort live on change
                // copy weights back to here

				var ff = new Generate(build);
				var res = ff.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    loading = true;
                    foreach (var stat in statNames)
                    {
                        var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();

                        if (build.Sort[stat] > 0)
                            ctrlWorth.Text = build.Sort[stat].ToString();
                        else
                            ctrlWorth.Text = "";
                    }
					foreach (var extra in extraNames)
					{
						var ctrlWorth = groupBox1.Controls.Find(extra + "Worth", true).FirstOrDefault();

						if (build.Sort.ExtraGet(extra) > 0)
							ctrlWorth.Text = build.Sort.ExtraGet(extra).ToString();
						else
							ctrlWorth.Text = "";
					}
					loading = false;
                }
                else
                {
                    foreach (var stat in statNames)
                    {
                        var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();
                        int val = 0;
                        int.TryParse(ctrlWorth.Text, out val);
                        build.Sort[stat] = val;
                    }
					foreach (var extra in extraNames)
					{
						var ctrlWorth = groupBox1.Controls.Find(extra + "Worth", true).FirstOrDefault();
						int val = 0;
						int.TryParse(ctrlWorth.Text, out val);
						build.Sort.ExtraSet(extra, val);
					}

				}
				UpdateGlobal();
            }
        }

        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            ListView lv = (ListView)sender;

            if (lv.SelectedItems.Count > 0)
            {
                ListViewItem li = lv.SelectedItems[0];

                if (li.Group == lv.Groups[0])
                    li.Group = lv.Groups[1];
                else
                    li.Group = lv.Groups[0];

                UpdateGlobal();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CalcPerms();
        }

        private void updatePerms()
        {
            if (btnPerms.Visible)
                btnPerms.Enabled = true;
            else
                CalcPerms();
        }

        private long CalcPerms()
        { // good idea, generate right now whenever the user clicks a... whatever
            build.GenRunes(Main.data, false, Main.useEquipped);

            // figure stuff out
            long perms = 0;
            Label ctrl;
            for (int i = 0; i < 6; i++)
            {
                int num = build.runes[i].Length;

                if (perms == 0)
                    perms = num;
                else
                    perms *= num;
                if (num == 0)
                    perms = 0;

                ctrl = (Label)Controls.Find("runeNum" + (i + 1).ToString(), true).FirstOrDefault();
                ctrl.Text = num.ToString();
                ctrl.ForeColor = Color.Black;

                // arbitrary colours for goodness/badness
                if (num < 12)
                    ctrl.ForeColor = Color.Green;
                if (num > 24)
                    ctrl.ForeColor = Color.Orange;
                if (num > 32)
                    ctrl.ForeColor = Color.Red;
            }
            ctrl = (Label)Controls.Find("runeNums", true).FirstOrDefault();
            ctrl.Text = String.Format("{0:#,##0}", perms);
            ctrl.ForeColor = Color.Black;

            // arbitrary colours for goodness/badness
            if (perms < 500000) // 500k
                ctrl.ForeColor = Color.Green;
            if (perms > 7000000) // 7m
                ctrl.ForeColor = Color.Orange;
            if (perms > 21000000 || perms == 0) // 21m
                ctrl.ForeColor = Color.Red;

            btnPerms.Enabled = false;

            return perms;
        }

        private void leaderTypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            leaderAmountBox.Items.Clear();
            if (leaderTypeBox.SelectedIndex == 0)
            {
                leaderAmountBox.Enabled = false;
                build.leader.SetZero();
            }
            else
            {
                LeaderType lt = (LeaderType)leaderTypeBox.SelectedItem;
                foreach (var o in lt.values)
                    leaderAmountBox.Items.Add(o);
                leaderAmountBox.Enabled = true;
                if (loading)
                    leaderAmountBox.SelectedIndex = lt.values.FindIndex(l => l.value == build.leader.Sum());
                else
                    leaderAmountBox.SelectedIndex = 0;
            }
        }

        private void leaderAmountBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            build.leader.SetZero();
            var lv = (LeaderType.LeaderValue)leaderAmountBox.SelectedItem;
            switch (lv.type)
            {
                case Attr.SpeedPercent:
                    build.leader.Speed = lv.value; 
                    break;
				case Attr.HealthPercent:
					build.leader.Health = lv.value;
					break;

			}
		}

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (Main.help != null)
                Main.help.Close();

            Main.help = new Help();
            Main.help.url = Environment.CurrentDirectory + "\\User Manual\\build.html";
            Main.help.Show();
        }

		private void button4_Click_1(object sender, EventArgs e)
		{
			btnDL6star.Enabled = false;
			var mref = MonsterStat.FindMon(build.mon);
			if (mref != null)
			{
				var mstat = mref.Download();
				var newmon = mstat.GetMon(build.mon);
				build.mon = newmon;
				refreshStats(newmon, newmon.GetStats());
			}
			btnDL6star.Enabled = true;
		}

		private void button5_Click_1(object sender, EventArgs e)
		{
			btnDLawake.Enabled = false;
			var mref = MonsterStat.FindMon(build.mon);
			if (mref != null)
			{
				var mstat = mref.Download();
				if (!mstat.Awakened && mstat.AwakenRef != null)
				{
					mstat = mstat.AwakenRef.Download();
					var newmon = mstat.GetMon(build.mon);
					build.mon = newmon;
					refreshStats(newmon, newmon.GetStats());
				}
			}
			btnDLawake.Enabled = true;
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (loading) return;
			build.DownloadStats = checkDL6star.Checked;
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{
			if (loading) return;
			build.DownloadAwake = checkDLawake.Checked;
			checkDL6star.Enabled = !checkDLawake.Checked;
		}

		private void listView1_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				toolStripButton4_Click(sender, e);
		}

		private void toolStripButton4_Click(object sender, EventArgs e)
		{
			if (setList.SelectedItems.Count == 1)
			{
				var li = setList.SelectedItems[0];
				RuneSet set = (RuneSet)li.Tag;
				// whatever, just add it to required if not
				if (li.Group != rsReq)
				{
					li.Group = rsReq;
					build.RequiredSets.Add(set);
					if (!build.BuildSets.Contains(set))
						build.BuildSets.Add(set);
				}
				else
				{
					build.RequiredSets.Add(set);
					int num = build.RequiredSets.Count(s => s == set);
					if (num > 3)
					{
						build.RequiredSets.RemoveAll(s => s == set);
						build.RequiredSets.Add(set);
					}
					if (num > 1)
						li.Text = set.ToString() + " x" + num;
					if (num > 3 || num == 1)
						li.Text = set.ToString();
				}
			}
		}

        private void monLabel_Click(object sender, EventArgs e)
        {
            using (MonSelect ms = new MonSelect(build.mon))
            {
                if (ms.ShowDialog() == DialogResult.OK)
                {
                    build.mon = ms.retMon;
                    monLabel.Text = "Build for " + build.mon.Name + " (" + build.mon.ID + ")";
                    refreshStats(build.mon, build.mon.GetStats());
                }
            }
        }

		private void check_magic_CheckedChanged(object sender, EventArgs e)
		{
			btnPerms.Visible = check_autoRunes.Checked;
			build.autoRuneSelect = check_autoRunes.Checked;
			updatePerms();
		}

        private void check_autoBuild_CheckedChanged(object sender, EventArgs e)
        {
            MessageBox.Show("NYI");
           // sampletext
        }
    }
}
