using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using RuneOptim;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp {
    // Specifying a new build, are we?
    public partial class Create : Form {
        public Build Build = null;

        // controls builds version numbers
        public static readonly int VERSIONNUM = 3;

        // Keep track of the runeset groups for speed swaps
        private ListViewGroup rsInc;
        private ListViewGroup rsExc;
        private ListViewGroup rsReq;

        // the current rune to look at
        private Rune runeTest = null;
        // is the form loading? wouldn't want to trigger any OnChanges, eh?
        private bool loading = false;

        private GenerateLive testWindow = null;

        static readonly string[] tabNames = { "g", "o", "e", "2", "4", "6", "1", "3", "5" };
        static readonly SlotIndex[] tabIndexes = { SlotIndex.Global, SlotIndex.Odd, SlotIndex.Even, SlotIndex.Two, SlotIndex.Four, SlotIndex.Six, SlotIndex.One, SlotIndex.Three, SlotIndex.Five };

        private ToolTip tooltipNoSorting = new ToolTip();
        private ToolTip tooltipBadRuneFilter = new ToolTip();
        private ToolTip tooltipSets = new ToolTip();

        protected override CreateParams CreateParams {
            get {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;    // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        class StatRow {
            public Label Label;
            public Label Base;
            public Label Bonus;
            public TextBox Min;
            public Label Current;
            public TextBox Goal;
            public TextBox Worth;
            public Label CurrentPts;
            public TextBox Thresh;
            public TextBox Max;
        }

        Dictionary<Attr, StatRow> statRows = new Dictionary<Attr, StatRow>();

        public Create(Build bb) {
            InitializeComponent();

            SuspendLayout();
            groupBox1.SuspendLayout();

            this.SetDoubleBuffered();
            // when show, check we have stuff

            if (bb == null) {
                MessageBox.Show("Build is null", "Create Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            Build = bb;

            // declare the truthyness of the groups and track them
            setList.Groups[1].Tag = true;
            rsInc = setList.Groups[1];
            setList.Groups[2].Tag = false;
            rsExc = setList.Groups[2];
            setList.Groups[0].Tag = false;
            rsReq = setList.Groups[0];

            if (leadTypes != null)
                leaderTypeBox.Items.AddRange(leadTypes);

            // for each runeset, put it in the list as excluded
            foreach (var rs in Enum.GetNames(typeof(RuneSet))) {
                if (rs != "Null" && rs != "Broken" && rs != "Unknown" && rs[0] != '_') {
                    ListViewItem li = new ListViewItem(rs);
                    li.Name = rs;
                    li.Tag = Enum.Parse(typeof(RuneSet), rs);
                    li.Group = setList.Groups[2];
                    setList.Items.Add(li);
                }
            }

            tBtnSetLess.Tag = 0;
            addAttrsToEvens();

            //Control textBox;

            // make a grid for the monsters base, min/max stats and the scoring

            int x = 0;
            int y = 94;

            int colWidth = 50;
            int rowHeight = 24;

            var comb = Build.StatAll;

            int genX = 0;

            int dlCheckX = 0;
            int dlBtnX = 0;

            Program.LineLog.Debug("comb " + comb.Length + " " + string.Join(",", comb));

            #region Statbox
            foreach (var attr in comb) {
                statRows[attr] = new StatRow();
                string strStat = attr.ToShortForm();
                x = 4;
                statRows[attr].Label = groupBox1.Controls.MakeControl<Label>(strStat, "Label", x, y, 50, 20, strStat);
                x += colWidth;

                statRows[attr].Base = groupBox1.Controls.MakeControl<Label>(strStat, "Base", x, y, 50, 20, strStat);
                dlCheckX = x;
                x += colWidth;
                
                statRows[attr].Bonus = groupBox1.Controls.MakeControl<Label>(strStat, "Bonus", x, y, 50, 20, strStat);
                x += colWidth;

                statRows[attr].Min = groupBox1.Controls.MakeControl<TextBox>(strStat, "Total", x, y, 40, 20);
                statRows[attr].Min.TextChanged += global_TextChanged;
                statRows[attr].Min.TextChanged += Total_TextChanged;
                dlBtnX = x;
                x += colWidth;

                statRows[attr].Current = groupBox1.Controls.MakeControl<Label>(strStat, "Current", x, y, 50, 20, strStat);
                x += colWidth;

                statRows[attr].Goal = groupBox1.Controls.MakeControl<TextBox>(strStat, "Goal", x, y, 40, 20);
                statRows[attr].Goal.TextChanged += global_TextChanged;
                x += colWidth;


                genX = x;

                statRows[attr].Worth = groupBox1.Controls.MakeControl<TextBox>(strStat, "Worth", x, y, 40, 20);
                statRows[attr].Worth.TextChanged += global_TextChanged;
                x += colWidth;

                statRows[attr].CurrentPts = groupBox1.Controls.MakeControl<Label>(strStat, "CurrentPts", x, y, (int)(50 * 0.8), 20, strStat);
                x += (int)(colWidth * 0.8);

                statRows[attr].Thresh = groupBox1.Controls.MakeControl<TextBox>(strStat, "Thresh", x, y, 40, 20);
                statRows[attr].Thresh.TextChanged += global_TextChanged;
                x += colWidth;

                statRows[attr].Max = groupBox1.Controls.MakeControl<TextBox>(strStat, "Max", x, y, 40, 20);
                statRows[attr].Max.TextChanged += global_TextChanged;

                y += rowHeight;
            }
            #endregion

            Build.Mon.GetStats();

            for (int i = 0; i < 4; i++) {
                if (Build?.Mon?.SkillsFunction?[i] != null) {
                    statRows[Attr.Skill1 + i] = new StatRow();

                    //var ff = build.mon.SkillFunc[i]; build.mon.GetSkillDamage(Attr.AverageDamage, i);
                    string stat = "Skill" + i;
                    x = 4;
                    statRows[Attr.Skill1 + i].Label = groupBox1.Controls.MakeControl<Label>(stat, "Label", x, y, 50, 20, "Skill " + (i + 1));
                    x += colWidth;

                    double aa = Build.Mon.GetSkillDamage(Attr.AverageDamage, i); //ff(build.mon);
                    double cc = Build.Mon.GetStats().GetSkillDamage(Attr.AverageDamage, i); //ff(build.mon.GetStats());

                    statRows[Attr.Skill1 + i].Base = groupBox1.Controls.MakeControl<Label>(stat, "Base", x, y, 50, 20, aa.ToString());
                    x += colWidth;

                    statRows[Attr.Skill1 + i].Bonus = groupBox1.Controls.MakeControl<Label>(stat, "Bonus", x, y, 50, 20, (cc - aa).ToString());
                    x += colWidth;

                    statRows[Attr.Skill1 + i].Min = groupBox1.Controls.MakeControl<TextBox>(stat, "Total", x, y, 40, 20);
                    statRows[Attr.Skill1 + i].Min.TextChanged += global_TextChanged;
                    statRows[Attr.Skill1 + i].Min.TextChanged += Total_TextChanged;
                    x += colWidth;

                    statRows[Attr.Skill1 + i].Current = groupBox1.Controls.MakeControl<Label>(stat, "Current", x, y, 50, 20, cc.ToString());
                    x += colWidth;

                    statRows[Attr.Skill1 + i].Goal = groupBox1.Controls.MakeControl<TextBox>(stat, "Goal", x, y, 40, 20);
                    statRows[Attr.Skill1 + i].Goal.TextChanged += global_TextChanged;
                    x += colWidth;

                    genX = x;

                    statRows[Attr.Skill1 + i].Worth = groupBox1.Controls.MakeControl<TextBox>(stat, "Worth", x, y, 40, 20);
                    statRows[Attr.Skill1 + i].Worth.TextChanged += global_TextChanged;
                    x += colWidth;

                    statRows[Attr.Skill1 + i].CurrentPts = groupBox1.Controls.MakeControl<Label>(stat, "CurrentPts", x, y, (int)(50 * 0.8), 20, "0");
                    x += (int)(colWidth * 0.8);

                    statRows[Attr.Skill1 + i].Thresh = groupBox1.Controls.MakeControl<TextBox>(stat, "Thresh", x, y, 40, 20);
                    statRows[Attr.Skill1 + i].Thresh.TextChanged += global_TextChanged;
                    x += colWidth;

                    statRows[Attr.Skill1 + i].Max = groupBox1.Controls.MakeControl<TextBox>(stat, "Max", x, y, 40, 20);
                    statRows[Attr.Skill1 + i].Max.TextChanged += global_TextChanged;

                    y += rowHeight;
                }
            }

            testBuildButton.Location = new Point(genX, y);

            /*
            btnDL6star.Location = new Point(dlBtnX, y);
            checkDL6star.Location = new Point(dlCheckX, y + 2);

            btnDLawake.Location = new Point(dlBtnX, y + rowHeight);
            checkDLawake.Location = new Point(dlCheckX, y + rowHeight + 2);
            */

            genTabRuneGrid(ref x, ref y, ref colWidth, ref rowHeight);


            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes()
                .Where(t => typeof(IBuildStrategyDefinition).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface))
                .Select(t => (IBuildStrategyDefinition)Activator.CreateInstance(t))
                .OrderBy(t => t.Order)
                .ToList();

            cbBuildStrat.Items.AddRange(types.ToArray());
            var ind = types.FindIndex(bs => bs.GetType().AssemblyQualifiedName == Build.BuildStrategy);
            cbBuildStrat.SelectedIndex = ind;

            ResumeLayout();
            groupBox1.ResumeLayout();
        }

        // when the scoring type changes
        private void filterJoin_SelectedIndexChanged(object sender, EventArgs e) {
            ComboBox box = (ComboBox)sender;
            string tabName = (string)box.Tag;
            TabPage tab = GetTab(tabName);
            Control ctrl;

            foreach (var stat in Build.StatNames) {
                ctrl = tab.Controls.Find(tabName + stat + "test", false).FirstOrDefault();
                if (ctrl != null) ctrl.Enabled = RuneAttributeFilterEnabled((FilterType)box.SelectedItem);
            }
            ctrl = tab.Controls.Find(tabName + "test", false).FirstOrDefault();
            if (ctrl != null) ctrl.Enabled = RuneSumFilterEnabled((FilterType)box.SelectedItem);

            int count = 30;
            double? test = null;
            double temp;
            ctrl = tab.Controls.Find(tabName + "test", false).FirstOrDefault();
            if (double.TryParse(ctrl?.Text, out temp))
                test = temp;

            ctrl = tab.Controls.Find(tabName + "count", false).FirstOrDefault();
            if (double.TryParse(ctrl?.Text, out temp))
                count = (int)temp;

            if (!Build.RuneScoring.ContainsKey(LibExtensionMethods.GetIndex(tabName)))
                Build.RuneScoring.Add(LibExtensionMethods.GetIndex(tabName), new Build.RuneScoreFilter((FilterType)box.SelectedItem, test, count));
            Build.RuneScoring[LibExtensionMethods.GetIndex(tabName)] = new Build.RuneScoreFilter((FilterType)box.SelectedItem, test, count);

            // TODO: trim the ZERO nodes on the tree

            // retest the rune
            //TestRune(runeTest);

            UpdateGlobal();
        }

        // When the window is told to appear (hopefully we have everything)
        async void Create_Shown(object sender, EventArgs e) {
            if (Build == null) {
                // don't have. :(
                Close();
                return;
            }

            SuspendLayout();
            groupBox1.SuspendLayout();

            // warning, now loading
            loading = true;

            if (Program.Settings.ShowIreneOnStart)
                Main.Irene.Show();

            Monster mon = Build.Mon;
            mon.Current.Leader = Build.Leader;
            mon.Current.Shrines = Build.Shrines;
            mon.Current.Guild = Build.Guild;
            Stats cur = mon.GetStats();
            if (mon.Id != 0)
                monLabel.Text = "Build for " + mon.FullName + " (" + mon.Id + ")";
            else
                monLabel.Text = "Build for " + Build.MonName + " (" + mon.Id + ")";

            Build.VERSIONNUM = VERSIONNUM;

            checkDL6star.Checked = Build.DownloadStats;
            checkDLawake.Checked = Build.DownloadAwake;
            checkDL6star.Enabled = !checkDLawake.Checked;
            extraCritBox.SelectedIndex = Build.ExtraCritRate / 15;

            check_autoRunes.Checked = Build.AutoRuneSelect;

            if (Build.Leader.IsNonZero) {
                Attr a = Build.Leader.NonZeroStats.FirstOrDefault();
                if (a != Attr.Null) {
                    leaderTypeBox.SelectedIndex = leadTypes.ToList().FindIndex(lt => lt.type == a);
                }
            }

            // move the sets around in the list a little
            foreach (RuneSet s in Build.BuildSets) {
                ListViewItem li = setList.Items.Find(s.ToString(), true).FirstOrDefault();
                //if (li == null)
                //    li.Group = rsExc;
                if (li != null)
                    li.Group = rsInc;
            }
            foreach (RuneSet s in Build.RequiredSets) {
                ListViewItem li = setList.Items.Find(s.ToString(), true).FirstOrDefault();
                if (li != null)
                    li.Group = rsReq;
                int num = Build.RequiredSets.Count(r => r == s);
                if (num > 1)
                    li.Text = s.ToString() + " x" + num;

            }

            Build.BuildSets.CollectionChanged += BuildSets_CollectionChanged;

            Build.RequiredSets.CollectionChanged += RequiredSets_CollectionChanged;

            // for 2,4,6 - make sure that the Attrs are set up
            var lists = new ListView[] { priStat2, priStat4, priStat6 };
            for (int j = 0; j < lists.Length; j++) {
                var lv = lists[j];

                var attrs = Enum.GetValues(typeof(Attr));
                for (int i = 0; i < Build.StatNames.Length; i++) {
                    var bl = Build.SlotStats[(j + 1) * 2 - 1];

                    string stat = Build.StatNames[i];
                    if (i < 3) {
                        if (bl.Contains(stat + "flat") || bl.Count == 0)
                            lv.Items.Find(stat + "flat", true).FirstOrDefault().Group = lv.Groups[0];

                        if (bl.Contains(stat + "perc") || bl.Count == 0)
                            lv.Items.Find(stat + "perc", true).FirstOrDefault().Group = lv.Groups[0];
                    }
                    else {
                        if (j == 0 && stat != "SPD")
                            continue;
                        if (j == 1 && (stat != "CR" && stat != "CD"))
                            continue;
                        if (j == 2 && (stat != "ACC" && stat != "RES"))
                            continue;

                        if (bl.Contains(stat + (stat == "SPD" ? "flat" : "perc")) || bl.Count == 0)
                            lv.Items.Find(stat + (stat == "SPD" ? "flat" : "perc"), true).FirstOrDefault().Group = lv.Groups[0];
                    }
                }
            }

            refreshStats(mon, cur);

            RegenSetList();

            // do we allow broken sets?
            ChangeBroken(Build.AllowBroken);

            // for each tabs filter
            foreach (var rs in Build.RuneFilters) {
                var tab = rs.Key;
                TabPage ctab = GetTab(tab.ToString());
                // for each stats filter
                foreach (var f in rs.Value) {
                    var stat = f.Key;
                    // for each stat type
                    foreach (var type in new string[] { "flat", "perc", "test" }) {
                        if (type == "perc" && stat == "SPD")
                            continue;
                        if (type == "flat" && (stat == "CR" || stat == "CD" || stat == "RES" || stat == "ACC"))
                            continue;

                        // find the controls and shove the value in it
                        var ctrl = ctab.Controls.Find(ctab.Tag + stat + type, true).FirstOrDefault();
                        double? val = f.Value[type];
                        // unless it's zero, I don't want zeros
                        if (val != null)
                            ctrl.Text = val.Value.ToString("0.##");
                        else
                            ctrl.Text = "";
                    }
                }
            }

            foreach (var tab in Build.RunePrediction) {
                TabPage ctab = GetTab(tab.Key.ToString());
                TextBox tb = (TextBox)ctab.Controls.Find(ctab.Tag + "raise", true).FirstOrDefault();
                if (tab.Value.Key != 0)
                    tb.Text = tab.Value.Key.ToString();
                CheckBox cb = (CheckBox)ctab.Controls.Find(ctab.Tag + "bonus", true).FirstOrDefault();
                cb.Checked = tab.Value.Value;
            }

            // for each tabs scoring
            foreach (var tab in Build.RuneScoring) {
                TabPage ctab = GetTab(tab.Key.ToString());
                // find that box
                ComboBox box = (ComboBox)ctab.Controls.Find(ctab.Tag + "join", true).FirstOrDefault();
                // manually kajigger it
                box.SelectedItem = tab.Value.Type;
                Control ctrl;
                foreach (var stat in Build.StatNames) {
                    foreach (var type in new string[] { "flat", "perc" }) {
                        if (type == "perc" && stat == "SPD")
                            continue;
                        if (type == "flat" && (stat == "ACC" || stat == "RES" || stat == "CD" || stat == "CR"))
                            continue;
                        //ctrl = ctab.Controls.Find(ctab.Tag + stat + type, true).FirstOrDefault();
                    }
                    ctrl = ctab.Controls.Find(ctab.Tag + stat + "test", true).FirstOrDefault();
                    ctrl.Enabled = RuneAttributeFilterEnabled((FilterType)box.SelectedItem);
                }
                ctrl = ctab.Controls.Find(ctab.Tag + "test", true).FirstOrDefault();
                ctrl.Enabled = RuneSumFilterEnabled((FilterType)box.SelectedItem);

                var tb = (TextBox)ctab.Controls.Find(ctab.Tag + "test", true).FirstOrDefault();
                if (tab.Value.Value != 0)
                    tb.Text = tab.Value.Value.ToString();


                tb = (TextBox)ctab.Controls.Find(ctab.Tag + "count", true).FirstOrDefault();
                if (tab.Value.Count != 0)
                    tb.Text = tab.Value.Count.ToString();
            }

            checkBuffAtk.Checked = Build.Buffs.Attack;

            tcATK.Gamma = Build.Buffs.Attack ? 1.0f : 0.2f;
            tcDEF.Gamma = Build.Buffs.Defense ? 1.0f : 0.2f;
            tcSPD.Gamma = Build.Buffs.Speed ? 1.0f : 0.2f;

            var t = Task.Delay(200).ContinueWith(i => {
                tcATK.Invalidate();
                tcDEF.Invalidate();
                tcSPD.Invalidate();
            });

            // done loading!
            loading = false;
            // this build is no longer considered new
            Build.New = false;
            // oh yeah, update everything now that it's finally loaded
            UpdateGlobal();
            //CalcPerms();


            ResumeLayout();
            groupBox1.ResumeLayout();

            await t;
        }

        private void RequiredSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var set in e.NewItems.OfType<RuneSet>()) {
                        var lvi = setList.Items.OfType<ListViewItem>().FirstOrDefault(ll => ((RuneSet)ll.Tag) == set);
                        lvi.Group = rsReq;
                        var num = (sender as IEnumerable<RuneSet>).Count(s => s == set);
                        lvi.Text = set.ToString();
                        if (num > 1)
                            lvi.Text = $"{set.ToString()} x{num}";
                    }
                    RegenSetList();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var set in e.OldItems.OfType<RuneSet>()) {
                        var lvi = setList.Items.OfType<ListViewItem>().FirstOrDefault(ll => ((RuneSet)ll.Tag) == set);
                        lvi.Group = rsExc;
                        var num = (sender as IEnumerable<RuneSet>).Count(s => s == set);
                        lvi.Text = set.ToString();
                        if (num > 0)
                            lvi.Group = rsReq;
                        if (num > 1)
                            lvi.Text = $"{set.ToString()} x{num}";
                    }
                    RegenSetList();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RegenSetList();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                default:
                    throw new NotImplementedException();
            }
        }

        private void BuildSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    /*foreach (var set in e.NewItems.OfType<RuneSet>())
                    {
                        var lvi = setList.Items.OfType<ListViewItem>().FirstOrDefault(ll => ((RuneSet)ll.Tag) == set);
                        lvi.Group = rsInc;
                    }*/
                    RegenSetList();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    /*foreach (var set in e.OldItems.OfType<RuneSet>())
                    {
                        var lvi = setList.Items.OfType<ListViewItem>().FirstOrDefault(ll => ((RuneSet)ll.Tag) == set);
                        lvi.Group = rsExc;
                    }*/
                    RegenSetList();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    RegenSetList();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                default:
                    throw new NotImplementedException();
            }
        }

        // The minimum value was changed
        void Total_TextChanged(object sender, EventArgs e) {
            var total = (TextBox)sender;

            if (total.Tag == null)
                return;

            // pull the BASE and BONUS controls from the tag
            var kv = (KeyValuePair<Label, Label>)total.Tag;
            var bonus = kv.Value;
            var mBase = kv.Key;

            // did someone put a k in?
            if (total.Text.ToLower().Contains("k")) {
                total.Text = (double.Parse(total.Text.ToLower().Replace("k", "")) * 1000).ToString();
            }

            // calculate the difference between base and minimum, put it in bonus
            var hasPercent = mBase.Text.Contains("%");
            double val;
            if (double.TryParse(total.Text.Replace("%", ""), out val)) {
                double sub;
                double.TryParse(mBase.Text.Replace("%", ""), out sub);
                val -= sub;
            }
            if (val < 0)
                val = 0;

            bonus.Text = "+" + (val) + (hasPercent ? "%" : "");
        }

        void tBtnBreak_Click(object sender, EventArgs e) {
            ChangeBroken(!(bool)((ToolStripButton)sender).Tag);
        }

        void tBtnSetMore_Click(object sender, EventArgs e) {
            if (Build == null)
                return;

            foreach (var sl in setList.SelectedItems.OfType<ListViewItem>()) {
                var ss = (RuneSet)sl.Tag;
                if (Build.RequiredSets.Contains(ss)) {
                    int num = Build.RequiredSets.Count(s => s == ss);
                    if (Rune.SetRequired(ss) == 2 && num < 3) {
                        Build.RequiredSets.Add(ss);
                    }
                }
                else if (Build.BuildSets.Contains(ss)) {
                    Build.AddRequiredSet(ss);
                }
                else {
                    Build.AddIncludedSet(ss);
                }
            }

            RegenSetList();
        }

        void tBtnSetLess_Click(object sender, EventArgs e) {
            if (Build == null)
                return;

            foreach (var sl in setList.SelectedItems.OfType<ListViewItem>()) {
                var ss = (RuneSet)sl.Tag;
                if (Build.RequiredSets.Contains(ss)) {
                    Build.RemoveRequiredSet(ss);
                    Build.AddIncludedSet(ss);
                }
                else if (Build.BuildSets.Contains(ss)) {
                    Build.ToggleIncludedSet(ss);
                }
            }

            RegenSetList();
        }

        void tBtnSetShuffle(object sender, EventArgs e) {
            if (Build == null)
                return;

            foreach (var sl in setList.SelectedItems.OfType<ListViewItem>()) {
                var ss = (RuneSet)sl.Tag;
                if (Build.RequiredSets.Contains(ss)) {
                    int num = Build.RequiredSets.Count(s => s == ss);
                    if (Rune.SetRequired(ss) == 2 && num < 3) {
                        Build.RequiredSets.Add(ss);
                    }
                    else {
                        Build.RemoveRequiredSet(ss);
                    }
                }
                else if (Build.BuildSets.Contains(ss)) {
                    Build.AddRequiredSet(ss);
                }
                else {
                    Build.AddIncludedSet(ss);
                }
            }

            RegenSetList();
        }

        // shuffle runesets between: included <-> excluded
        void tBtnInclude_Click(object sender, EventArgs e) {
            if (Build == null)
                return;

            var list = setList;

            var items = list.SelectedItems;
            if (items.Count == 0)
                return;

            var selGrp = items[0].Group;
            var indGrp = selGrp.Items.IndexOf(items[0]);

            // shift all the selected things to the place
            foreach (ListViewItem item in items) {
                var set = (RuneSet)item.Tag;
                Build.ToggleIncludedSet(set);
                /*if (build.BuildSets.Contains(set))
                {
                    build.BuildSets.Remove(set);
                    item.Group = rsExc;
                }
                else
                {
                    build.BuildSets.Add(set);
                    item.Group = rsInc;
                }*/
            }

            var ind = items[0].Index;

            // TODO: maybe try cool reselecting the next entry
            if (selGrp.Items.Count > 0) {
                int i = indGrp;
                if (i > 0 && (i == selGrp.Items.Count || !(bool)selGrp.Tag))
                    i -= 1;
                while (selGrp.Items[i].Selected) {
                    i++;
                    i %= selGrp.Items.Count;
                    if (i == indGrp)
                        break;

                }

                ind = selGrp.Items[i].Index;
            }
            setList.SelectedIndices.Clear();

            setList.SelectedIndices.Add(ind);
        }

        void tBtnRequire_Click(object sender, EventArgs e) {
            if (setList.SelectedItems.Count == 1) {
                var li = setList.SelectedItems[0];
                RuneSet set = (RuneSet)li.Tag;
                // whatever, just add it to required if not
                int num = Build.AddRequiredSet(set);
                /*if (num > 1)
                    li.Text = set.ToString() + " x" + num;
                if (num > 3 || num <= 1)
                    li.Text = set.ToString();*/
            }
        }

        // shuffle rnuesets between: excluded > required <-> included
        void tBtnShuffle_Click(object sender, EventArgs e) {
            if (Build == null)
                return;

            var list = setList;

            var items = list.SelectedItems;
            if (items.Count == 0)
                return;

            var selGrp = items[0].Group;
            var indGrp = selGrp.Items.IndexOf(items[0]);

            foreach (ListViewItem item in items) {
                var set = (RuneSet)item.Tag;

                if (Build.RemoveRequiredSet(set) > 0) {
                    Build.AddIncludedSet(set);
                }
                else {
                    Build.AddRequiredSet(set);
                }

                if (Build.RequiredSets.Contains(set)) {
                    item.Group = rsInc;
                    item.Text = set.ToString();
                }
                else {
                    item.Group = rsReq;
                }
            }

            var ind = items[0].Index;

            if (selGrp.Items.Count > 0) {
                int i = indGrp;
                int selcount = 0;
                if (i > 0 && (i == selGrp.Items.Count || !(bool)selGrp.Tag))
                    i -= 1;
                while (selGrp.Items[i].Selected) {
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
        }

        private void runeDial_RuneClick(object sender, RuneClickEventArgs e) {
            Build.RunesUseEquipped = Program.Settings.UseEquipped;
            Build.RunesUseLocked = false;
            Build.RunesDropHalfSetStat = Program.GoFast;
            Build.RunesOnlyFillEmpty = Program.FillRunes;
            // good idea, generate right now whenever the user clicks a... whatever
            Build.GenRunes(Program.Data);

            using (RuneSelect rs = new RuneSelect()) {
                rs.ReturnedRune = Build.Mon.Current.Runes[e.Slot - 1];
                rs.Build = Build;
                rs.Slot = (SlotIndex)e.Slot;
                rs.Runes = Build.Runes[e.Slot - 1];
                rs.ShowDialog();
            }
        }

        void button1_Click(object sender, EventArgs e) {
            Build.RunesUseLocked = false;
            Build.RunesUseEquipped = true;
            Build.RunesDropHalfSetStat = Program.GoFast;
            Build.RunesOnlyFillEmpty = Program.FillRunes;
            Build.GenRunes(Program.Data);
            using (var ff = new RuneSelect()) {
                ff.ReturnedRune = runeTest;
                ff.Build = Build;

                switch (tabControl1.SelectedTab.Text) {
                    case "Evens":
                        ff.Runes = Program.Data.Runes.Where(r => r.Slot % 2 == 0);
                        List<Rune> fr = new List<Rune>();
                        fr.AddRange(ff.Runes.Where(r => r.Slot == 2 && Build.SlotStats[1].Contains(r.Main.Type.ToForms())));
                        fr.AddRange(ff.Runes.Where(r => r.Slot == 4 && Build.SlotStats[3].Contains(r.Main.Type.ToForms())));
                        fr.AddRange(ff.Runes.Where(r => r.Slot == 6 && Build.SlotStats[5].Contains(r.Main.Type.ToForms())));
                        ff.Runes = fr;
                        break;
                    case "Odds":
                        ff.Runes = Program.Data.Runes.Where(r => r.Slot % 2 == 1);
                        break;
                    case "Global":
                        ff.Runes = Program.Data.Runes;
                        break;
                    default:
                        int slot = int.Parse(tabControl1.SelectedTab.Text);
                        ff.Runes = Program.Data.Runes.Where(r => r.Slot == slot);
                        break;
                }

                ff.Slot = (SlotIndex)Enum.Parse(typeof(SlotIndex), tabControl1.SelectedTab.Text);

                ff.Runes = ff.Runes.Where(r => Build.BuildSets.Contains(r.Set));

                var res = ff.ShowDialog();
                if (res == DialogResult.OK) {
                    Rune rune = ff.ReturnedRune;
                    if (rune != null) {
                        runeTest = rune;
                        TestRune(rune);
                    }
                }
            }
        }

        void button2_Click(object sender, EventArgs e) {
            if (AnnoyUser()) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        void global_TextChanged(object sender, EventArgs e) {
            TextBox text = (TextBox)sender;

            if (text.Text.ToLower().Contains("k"))
                text.Text = (double.Parse(text.Text.ToLower().Replace("k", "")) * 1000).ToString();

            UpdateGlobal();
        }

        void global_CheckChanged(object sender, EventArgs e) {
            UpdateGlobal();
        }

        void testBuildClick(object sender, EventArgs e) {
            if (Build == null)
                return;

            if (AnnoyUser()) return;

            // make a new form that generates builds into it
            // have a weight panel near the stats
            // sort live on change
            // copy weights back to here

            if (!Program.Config.AppSettings.Settings.AllKeys.Contains("generateLive")) {
                var ff = new Generate(Build);
                var backupSort = new Stats(Build.Sort);
                var res = ff.ShowDialog();
                if (res == DialogResult.OK) {
                    loading = true;
                    foreach (var stat in Build.StatEnums) {
                        statRows[stat].Worth.Text = Build.Sort[stat] != 0 ? Build.Sort[stat].ToString() : "";
                    }
                    foreach (var extra in Build.ExtraEnums) {
                        statRows[extra].Worth.Text = Build.Sort.ExtraGet(extra) != 0 ? Build.Sort.ExtraGet(extra).ToString() : "";
                    }
                    for (int i = 0; i < 4; i++) {
                        if (Build?.Mon?.SkillsFunction?[i] != null) {
                            var skilla = Attr.Skill1 + i;
                            statRows[skilla].Worth.Text = Build.Sort.DamageSkillups[i] != 0 ? Build.Sort.DamageSkillups[i].ToString() : "";
                        }
                    }
                    loading = false;
                }
                else {
                    double val;
                    foreach (var stat in Build.StatEnums) {
                        double.TryParse(statRows[stat].Worth.Text, out val);
                        Build.Sort[stat] = val;
                    }
                    foreach (var extra in Build.ExtraEnums) {
                        double.TryParse(statRows[extra].Worth.Text, out val);
                        Build.Sort.ExtraSet(extra, val);
                    }
                    for (int i = 0; i < 4; i++) {
                        if (Build?.Mon?.SkillsFunction?[i] != null) {
                            var skilla = Attr.Skill1 + i;
                            double.TryParse(statRows[skilla].Worth.Text, out val);
                            Build.Sort.DamageSkillupsSet(i, val);
                        }
                    }

                }
                UpdateGlobal();
            }
            else {
                if (testWindow == null || testWindow.IsDisposed) {
                    testWindow = new GenerateLive(Build);
                    testWindow.Owner = this;
                }
                if (!testWindow.Visible)
                    testWindow.Show();
                testWindow.Location = new Point(Location.X + Width, Location.Y);
            }
        }

        void listView2_DoubleClick(object sender, EventArgs e) {
            ListView lv = (ListView)sender;

            if (lv.SelectedItems.Count > 0) {
                ListViewItem li = lv.SelectedItems[0];

                if (li.Group == lv.Groups[0])
                    li.Group = lv.Groups[1];
                else
                    li.Group = lv.Groups[0];

                UpdateGlobal();
            }
        }

        void button4_Click(object sender, EventArgs e) {
            CalcPerms();
        }

        void leaderTypeBox_SelectedIndexChanged(object sender, EventArgs e) {
            leaderAmountBox.Items.Clear();
            if (leaderTypeBox.SelectedIndex == 0) {
                leaderAmountBox.Enabled = false;
                Build.Leader.SetTo(0);
            }
            else {
                LeaderType lt = (LeaderType)leaderTypeBox.SelectedItem;
                foreach (var o in lt.values)
                    leaderAmountBox.Items.Add(o);
                leaderAmountBox.Enabled = true;
                if (loading)
                    leaderAmountBox.SelectedIndex = lt.values.FindIndex(l => l.value == Build.Leader.Sum());
                else
                    leaderAmountBox.SelectedIndex = 0;
            }
        }

        void leaderAmountBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (loading)
                return;
            Build.Leader.SetTo(0);
            var lv = (LeaderType.LeaderValue)leaderAmountBox.SelectedItem;
            switch (lv.type) {
                case Attr.Speed:
                    Build.Leader.Speed = lv.value;
                    break;
                case Attr.HealthPercent:
                    Build.Leader.Health = lv.value;
                    break;
                case Attr.AttackPercent:
                    Build.Leader.Attack = lv.value;
                    break;
                default:
                    Build.Leader[lv.type] = lv.value;
                    break;
            }
            var mon = Build.Mon;
            mon.Current.Leader = Build.Leader;
            refreshStats(mon, mon.GetStats());
        }

        void btnHelp_Click(object sender, EventArgs e) {
            if (Main.Help != null)
                Main.Help.Close();

            Main.Help = new Help();
            Main.Help.url = Environment.CurrentDirectory + "\\User Manual\\build.html";
            Main.Help.Show();
        }

        void button4_Click_1(object sender, EventArgs e) {
            btnDL6star.Enabled = false;
            var mref = MonsterStat.FindMon(Build.Mon);
            if (mref != null) {
                var mstat = mref.Download();
                var newmon = mstat.GetMon(Build.Mon);
                Build.Mon = newmon;
                refreshStats(newmon, newmon.GetStats());
            }
            btnDL6star.Enabled = true;
        }

        void button5_Click_1(object sender, EventArgs e) {
            btnDLawake.Enabled = false;
            var mref = MonsterStat.FindMon(Build.Mon);
            if (mref != null) {
                var mstat = mref.Download();
                if (!mstat.Awakened && mstat.AwakenTo != null) {
                    mstat = mstat.AwakenTo.Download();
                    var newmon = mstat.GetMon(Build.Mon);
                    Build.Mon = newmon;
                    refreshStats(newmon, newmon.GetStats());
                }
            }
            btnDLawake.Enabled = true;
        }

        void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if (loading) return;
            Build.DownloadStats = checkDL6star.Checked;
        }

        void checkBox2_CheckedChanged(object sender, EventArgs e) {
            if (loading) return;
            Build.DownloadAwake = checkDLawake.Checked;
            checkDL6star.Enabled = !checkDLawake.Checked;
        }

        void monLabel_Click(object sender, EventArgs e) {
            using (MonSelect ms = new MonSelect(Build.Mon)) {
                if (ms.ShowDialog() == DialogResult.OK) {
                    Build.Mon = ms.retMon;
                    Build.MonId = Build.Mon.Id;
                    Build.MonName = Build.Mon.FullName;
                    monLabel.Text = "Build for " + Build.Mon.FullName + " (" + Build.Mon.Id + ")";
                    refreshStats(Build.Mon, Build.Mon.GetStats());
                }
            }
        }

        void check_magic_CheckedChanged(object sender, EventArgs e) {
            btnPerms.Visible = check_autoRunes.Checked;
            Build.AutoRuneSelect = check_autoRunes.Checked;
            updatePerms();
        }

        void check_autoBuild_CheckedChanged(object sender, EventArgs e) {
            MessageBox.Show("NYI");
            // automatically adjust the minimum and runefilters during the build?
        }

        private void Create_FormClosing(object sender, FormClosingEventArgs e) {
            if (testWindow != null && !testWindow.IsDisposed)
                testWindow.Close();
            Build.RequiredSets.CollectionChanged -= RequiredSets_CollectionChanged;
            Build.BuildSets.CollectionChanged -= BuildSets_CollectionChanged;
        }

        private void extraCritBox_SelectedIndexChanged(object sender, EventArgs e) {
            // TODO: uncheese
            var mon = Build.Mon;
            Build.ExtraCritRate = extraCritBox.SelectedIndex * 15;
            mon.ExtraCritRate = Build.ExtraCritRate;
            refreshStats(mon, mon.GetStats());
        }

        private void checkBuffAtk_CheckedChanged(object sender, EventArgs e) {
            if (loading)
                return;
            var buffs = Build.Buffs;
            buffs.Attack = checkBuffAtk.Checked;
            Build.Buffs = buffs;
        }

        private void tcATK_Click(object sender, EventArgs e) {
            var buffs = Build.Buffs;
            buffs.Attack = !Build.Buffs.Attack;
            Build.Buffs = buffs;
            checkBuffAtk.Checked = Build.Buffs.Attack;
            tcATK.Gamma = Build.Buffs.Attack ? 1.0f : 0.2f;
            tcATK.Invalidate();

            Monster mon = Build.Mon;
            mon.Current.Buffs = Build.Buffs;
            Stats cur = mon.GetStats(true);
            refreshStats(mon, cur);
        }

        private void tcDEF_Click(object sender, EventArgs e) {
            var buffs = Build.Buffs;
            buffs.Defense = !Build.Buffs.Defense;
            Build.Buffs = buffs;
            tcDEF.Gamma = Build.Buffs.Defense ? 1.0f : 0.2f;
            tcDEF.Invalidate();

            Monster mon = Build.Mon;
            mon.Current.Buffs = Build.Buffs;
            Stats cur = mon.GetStats(true);
            refreshStats(mon, cur);
        }

        private void tcSPD_Click(object sender, EventArgs e) {
            var buffs = Build.Buffs;
            buffs.Speed = !Build.Buffs.Speed;
            Build.Buffs = buffs;
            tcSPD.Gamma = Build.Buffs.Speed ? 1.0f : 0.2f;
            tcSPD.Invalidate();

            Monster mon = Build.Mon;
            mon.Current.Buffs = Build.Buffs;
            Stats cur = mon.GetStats(true);
            refreshStats(mon, cur);
        }

        private void CbBuildStrat_SelectedIndexChanged(object sender, EventArgs e) {
            var bs = cbBuildStrat.SelectedItem as IBuildStrategyDefinition;
            Build.BuildStrategy = bs.GetType().AssemblyQualifiedName;
        }


        private void CbBuildStrat_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index < 0) {
                e.Graphics.DrawString(string.Empty, cbBuildStrat.Font, Brushes.Black, e.Bounds);
                return;
            }
            if (!(cbBuildStrat.Items[e.Index] is IBuildStrategyDefinition item)) {
                e.Graphics.DrawString(cbBuildStrat.Items[e.Index].ToString(), cbBuildStrat.Font, Brushes.Black, e.Bounds);
                return;
            }

            var nn = item.Name;
            if (string.IsNullOrWhiteSpace(nn))
                nn = item.GetType().Name;

            if (!item.IsValid(Build)) //We are disabling item based on Index, you can have your logic here
            {
                e.Graphics.FillRectangle(Brushes.PaleVioletRed, e.Bounds);
                e.Graphics.DrawString(nn, cbBuildStrat.Font, Brushes.DimGray, e.Bounds);
            }
            else {
                e.DrawBackground();
                e.Graphics.DrawString(nn, cbBuildStrat.Font, Brushes.Black, e.Bounds);
                e.DrawFocusRectangle();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e) {
            Close();
        }
    }
}

