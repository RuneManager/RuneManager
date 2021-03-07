using RuneOptim;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp {
    // Generates a bunch of builds to preview the stats
    public partial class GenerateLive : Form {
        // the build to use
        public Build build;

        // if making builds
        bool building;

        bool changedSpec = true;

        bool listGenCancelled = false;
        object listGenLock = new object();

        bool buildGenCancelled = false;
        object buildGenLock = new object();

        bool monSetsCancelled = false;
        object monSetsLock = new object();
        ConcurrentDictionary<RuneKey, Monster> monsList = new ConcurrentDictionary<RuneKey, Monster>();

        BlockingCollection<RuneKey> buildsToGen = new BlockingCollection<RuneKey>();
        BlockingCollection<Monster> freshMons = new BlockingCollection<Monster>();

        int progState = 0;

        List<Monster> currentList = new List<Monster>();

        class RuneKey : IEquatable<RuneKey> {
            public Rune[] rs;
            public RuneKey(params Rune[] ii) {
                rs = ii;
            }

            public bool Equals(RuneKey other) {
                return !(other.rs[0].Id != rs[0].Id ||
                    other.rs[1].Id != rs[1].Id ||
                    other.rs[2].Id != rs[2].Id ||
                    other.rs[3].Id != rs[3].Id ||
                    other.rs[4].Id != rs[4].Id ||
                    other.rs[5].Id != rs[5].Id);
            }

            public override bool Equals(object obj) {
                RuneKey rk;
                if ((rk = obj as RuneKey) != null)
                    return !(rk.rs[0].Id != rs[0].Id ||
                            rk.rs[1].Id != rs[1].Id ||
                            rk.rs[2].Id != rs[2].Id ||
                            rk.rs[3].Id != rs[3].Id ||
                            rk.rs[4].Id != rs[4].Id ||
                            rk.rs[5].Id != rs[5].Id);

                return (GetHashCode() == obj.GetHashCode());
            }

            public override int GetHashCode() {
                var res = 0;
                res = (res * 397) ^ (int)rs[0].Id;
                res = (res * 397) ^ (int)rs[1].Id;
                res = (res * 397) ^ (int)rs[2].Id;
                res = (res * 397) ^ (int)rs[3].Id;
                res = (res * 397) ^ (int)rs[4].Id;
                res = (res * 397) ^ (int)rs[5].Id;
                return res;
            }

            public override string ToString() {
                return rs[0].Id + "," + rs[1].Id + "," + rs[2].Id + "," + rs[3].Id + "," + rs[4].Id + "," + rs[5].Id;
            }
        }

        public bool IsOkayBuild(Monster test) {

            bool isBad = false;

            var cstats = test.GetStats();

            // check if build meets minimum
            isBad |= (build.Minimum != null && !(cstats > build.Minimum));
            // if no broken sets, check for broken sets
            isBad |= (!build.AllowBroken && !test.Current.SetsFull);
            // if there are required sets, ensure we have them
            isBad |= (build.RequiredSets != null && build.RequiredSets.Count > 0
                // this Linq adds no overhead compared to GetStats() and ApplyRune()
                && !build.RequiredSets.All(s => test.Current.Sets.Contains(s)));

            return !isBad;
        }

        public GenerateLive(Build bb) {
            InitializeComponent();

            // master has given Gener a Build?
            build = bb;

            // cool clicky thing
            var sorter = new ListViewSort();
            // sort decending on POINTS
            sorter.OnColumnClick(0, false);
            loadoutList.ListViewItemSorter = sorter;

            foreach (Attr stat in Build.StatAll) {
                loadoutList.Columns.Add(stat.ToShortForm()).Width = 80;
            }

            toolStripStatusLabel1.Text = "Generating...";
            building = true;
            toolStripProgressBar1.Maximum = Program.Settings.TestShow * 2;

            //for (int i = 0; i < 20; i++)
            {
                Task.Factory.StartNew(() => {
                    DateTime start = DateTime.Now;
                    while (true) {
                        long total = getTotal();
                        var partitioner = Partitioner.Create(buildsToGen.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
                        Parallel.ForEach(partitioner, (batch, state) => {
                            //string compKey = batch;// buildsToGen.Take();
                            if (batch.rs[0].Id == 638 &&
                                batch.rs[1].Id == 465 &&
                                batch.rs[2].Id == 763 &&
                                batch.rs[3].Id == 467 &&
                                batch.rs[4].Id == 612 &&
                                batch.rs[5].Id == 487)
                                total -= 1;


                            var tb = build.GenBuild(batch.rs);
                            if (tb != null) {
                                //Console.WriteLine("Created build " + compKey);
                                freshMons.Add(tb);
                                monsList.AddOrUpdate(batch, tb, (k, v) => tb);
                            }

                            if (buildsToGen.Count == 0 || DateTime.Now > start.AddSeconds(1)) {
                                if (!IsDisposed && progState == 1) {
                                    Invoke((MethodInvoker)delegate {
                                        int num = buildsToGen.Count;
                                        // put the thing in on the main thread and bump the progress bar
                                        int v = (int)(toolStripProgressBar1.Maximum * num / (double)total);
                                        if (v >= toolStripProgressBar1.Minimum && v <= toolStripProgressBar1.Maximum)
                                            toolStripProgressBar1.Value = v;
                                        toolStripStatusLabel1.Text = "Remaining Sets " + num.ToString("#,##0");
                                    });
                                }
                                //Thread.Sleep(20);
                                start = DateTime.Now;
                            }
                        });
                        Invoke((MethodInvoker)delegate {
                            int num = buildsToGen.Count;
                            // put the thing in on the main thread and bump the progress bar
                            int v = (int)(toolStripProgressBar1.Maximum * num / (double)total);
                            if (v >= toolStripProgressBar1.Minimum && v <= toolStripProgressBar1.Maximum)
                                toolStripProgressBar1.Value = v;
                            toolStripStatusLabel1.Text = "Remaining Sets " + num.ToString("#,##0");
                        });
                    }
                });
            }

            Task.Factory.StartNew(() => {
                while (true) {
                    bool hasChanged = false;
                    Monster fresh;
                    if (freshMons.TryTake(out fresh)) {
                        if (fresh.score == 0)
                            fresh.score = build.CalcScore(fresh);
                        //Console.WriteLine("Checking build " + string.Join(",", fresh.Current.Runes.Select(r => r.ID.ToString())));
                        if (currentList.Count < Program.Settings.TestShow || fresh.score > currentList.Min(qq => qq.score)) {
                            currentList.Add(fresh); //.Where(IsOkayBuild)
                            currentList = currentList.OrderByDescending(m => m.score).Take(Program.Settings.TestShow).ToList();
                            hasChanged = true;
                        }
                    }

                    if (changedSpec && monsList.Count > 0) {
                        // todo: recheck min/max //  && IsOkayBuild(m.Value)
                        currentList = monsList.Where(m => m.Value != null).Select(m => m.Value).OrderByDescending(m => m.score).Take(Program.Settings.TestShow).ToList();
                        if (currentList.Count > 0)
                            hasChanged = true;
                    }

                    if (!hasChanged) {
                        //Thread.Sleep(50);
                        continue;
                    }

                    IEnumerable<Monster> llist = currentList.ToList();
                    List<ListViewItem> ilist = null;
                    Invoke((MethodInvoker)delegate {
                        ilist = loadoutList.Items.OfType<ListViewItem>().ToList();
                    });

                    foreach (var li in ilist) {
                        if (listGenCancelled)
                            break;

                        var mon = li.Tag as Monster;
                        if (mon == null)
                            continue;

                        var compKey = string.Join(",", mon.Current.Runes.Select(r => r.Id.ToString()));
                        if (!llist.Any(m => string.Join(",", m.Current.Runes.Select(r => r.Id.ToString())) == compKey)) {
                            // new list doen't contain old build
                            Invoke((MethodInvoker)delegate {
                                li.Remove();
                            });
                        }
                        else {
                            currentList.Add(mon);
                            // new list does contain old build
                            llist = llist.Except(llist.Where(m => string.Join(",", m.Current.Runes.Select(r => r.Id.ToString())) == compKey));
                        }
                    }

                    if (listGenCancelled)
                        return;

                    foreach (var b in llist) {
                        if (listGenCancelled)
                            break;

                        ListViewItem li = new ListViewItem();
                        var Cur = b.GetStats();

                        //currentList.Add(b);

                        int underSpec = 0;
                        int under12 = 0;
                        foreach (var r in b.Current.Runes) {
                            if (r.Level < 12)
                                under12 += 12 - r.Level;
                            if (build.RunePrediction.ContainsKey((SlotIndex)r.Slot) && r.Level < (build.RunePrediction[(SlotIndex)r.Slot].Key ?? 0))
                                underSpec += (build.RunePrediction[(SlotIndex)r.Slot].Key ?? 0) - r.Level;
                        }

                        li.SubItems.Add(underSpec + "/" + under12);
                        double pts = GetPoints(Cur, (str, i) => { li.SubItems.Add(str); });
                        b.score = pts;

                        // put the sum points into the first item
                        li.SubItems[0].Text = pts.ToString("0.##");

                        li.Tag = b;
                        if (Program.Settings.TestGray && b.Current.Runes.Any(r => r.Locked))
                            li.ForeColor = Color.Gray;
                        else {
                            if (b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 2) &&
                                b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 4)) {
                                li.ForeColor = Color.Green;
                            }
                            else if (b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 2)) {
                                li.ForeColor = Color.Goldenrod;
                            }
                            else if (b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 4)) {
                                li.ForeColor = Color.DarkBlue;
                            }
                        }

                        if (!IsDisposed && IsHandleCreated) {
                            Invoke((MethodInvoker)delegate {
                                // put the thing in on the main thread and bump the progress bar
                                loadoutList.Items.Add(li);
                            });
                        }
                    }
                    changedSpec = false;

                }
            });

            RegenSets();


        }

        public void textBox_TextChanged(object sender, EventArgs e) {
            // if we are generating builds, don't recalculate all the builds
            //if (building) return;

            // "sort" as in, recalculate the whole number
            foreach (ListViewItem li in loadoutList.Items) {
                ListItemSort(li);
            }
            var lv = loadoutList;
            var lvs = (ListViewSort)(lv).ListViewItemSorter;
            lvs.OnColumnClick(0, false, true);
            // actually sort the list, on points
            lv.Sort();
        }

        private double GetPoints(Stats Cur, Action<string, int> w = null) {
            double pts = 0;
            double p;
            int i = 2;
            foreach (Attr stat in Build.StatAll) {
                if (!stat.HasFlag(Attr.ExtraStat)) {
                    string str = Cur[stat].ToString();
                    if (build.Sort[stat] != 0) {
                        p = Cur[stat] / build.Sort[stat];
                        if (build.Threshold[stat] != 0)
                            p -= Math.Max(0, Cur[stat] - build.Threshold[stat]) / build.Sort[stat];
                        str = p.ToString("0.#") + " (" + Cur[stat] + ")";
                        pts += p;
                    }
                    w?.Invoke(str, i);
                    i++;
                }
                else {
                    string str = Cur.ExtraValue(stat).ToString();
                    if (build.Sort.ExtraGet(stat) != 0) {
                        p = Cur.ExtraValue(stat) / build.Sort.ExtraGet(stat);
                        if (build.Threshold.ExtraGet(stat) != 0)
                            p -= Math.Max(0, Cur.ExtraValue(stat) - build.Threshold.ExtraGet(stat)) /
                                 build.Sort.ExtraGet(stat);
                        str = p.ToString("0.#") + " (" + Cur.ExtraValue(stat) + ")";
                        pts += p;
                    }
                    w?.Invoke(str, i);
                    i++;
                }
            }
            return pts;
        }

        // recalculate all the points for this monster
        // TODO: consider hiding point values in the subitem tags and only recalcing the changed column
        // TODO: pull the scoring algorithm into a neater function
        public void ListItemSort(ListViewItem li) {
            Monster load = (Monster)li.Tag;
            var Cur = load.GetStats();

            double pts = GetPoints(Cur, (str, num) => { li.SubItems[num].Text = str; });

            li.SubItems[0].Text = pts.ToString("0.##");

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e) {
            if (building) return;

            var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
            sorter.OnColumnClick(e.Column, false, true);
            ((ListView)sender).Sort();
        }

        private void button1_Click(object sender, EventArgs e) {
            // Things went okay
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e) {
            // things were :(
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // TODO: update, fix, make work forrealz
        public void TryMakeTheBest() {
            if (loadoutList.SelectedItems.Count > 0) {
                ListViewItem lit = loadoutList.Items[0];
                ListViewItem lis = loadoutList.SelectedItems[0];

                if (lit == lis)
                    return;

                Monster mY = (Monster)lis.Tag;
                Monster mN = (Monster)lit.Tag;

                List<Attr> better = new List<Attr>();

                building = true;

                foreach (Attr stat in Build.StatAll) {
                    if (!stat.HasFlag(Attr.ExtraStat) && mY.GetStats()[stat] > mN.GetStats()[stat])
                        better.Add(stat);
                }
                double totalsort = 0;
                foreach (var stat in Build.StatAll) {
                    if (!stat.HasFlag(Attr.ExtraStat) && build.Sort[stat] != 0)
                        totalsort += Math.Abs(build.Sort[stat]);

                    if (stat.HasFlag(Attr.ExtraStat) && build.Sort.ExtraGet(stat) != 0)
                        totalsort += Math.Abs(build.Sort.ExtraGet(stat));
                }
                if (totalsort == 0) {
                    double totalstats = 0;
                    foreach (var stat in better) {
                        if (!stat.HasFlag(Attr.ExtraStat))
                            totalstats += mY.GetStats()[stat];
                        else
                            totalstats += mY.GetStats().ExtraValue(stat);
                    }
                    int amount = (int)Math.Max(30, Math.Sqrt(Math.Max(100, totalstats)));
                    foreach (var stat in better) {
                        if (!stat.HasFlag(Attr.ExtraStat)) {
                            build.Sort[stat] = (int)(amount * (mY.GetStats()[stat] / totalstats));
                            TextBox tb = (TextBox)Controls.Find(stat + "Worth", true).FirstOrDefault();
                            if (tb != null)
                                tb.Text = build.Sort[stat].ToString();
                        }
                        else {
                            build.Sort.ExtraSet(stat, (int)(amount * (mY.GetStats().ExtraValue(stat) / totalstats)));
                            TextBox tb = (TextBox)Controls.Find(stat + "Worth", true).FirstOrDefault();
                            if (tb != null)
                                tb.Text = build.Sort.ExtraGet(stat).ToString();
                        }
                    }
                }
                else {
                    // todo
                }

                building = false;
                textBox_TextChanged(null, null);
            }
        }

        private int getTotal() {
            return build.runes[0].Length
                * build.runes[1].Length
                * build.runes[2].Length
                * build.runes[3].Length
                * build.runes[4].Length
                * build.runes[5].Length;
        }

        public void RegenSets() {
            progState = 0;
            // delegate
            Task.Factory.StartNew(() => {
                // cancel the current worker
                monSetsCancelled = true;
                // cancel anyone making builds
                buildGenCancelled = true;
                // wait for anyone to stop
                lock (monSetsLock) {
                    // stop cancelling
                    monSetsCancelled = false;
                    build.RunesUseLocked = Program.Settings.LockTest;
                    build.RunesUseEquipped = Program.Settings.UseEquipped;
                    build.RunesDropHalfSetStat = Program.goFast;
                    build.RunesOnlyFillEmpty = Program.fillRunes;
                    build.GenRunes(Program.data);

                    long num = 0;
                    long total = getTotal();
                    DateTime start = DateTime.Now;

                    var pfer = Parallel.ForEach(build.runes[0], (r0, loop) =>
                    {
                        if (monSetsCancelled)
                            loop.Break();

                        RuneSet? c4set = null;
                        if (!build.AllowBroken && r0.SetIs4)
                            c4set = r0.Set;

                        foreach (var r1 in build.runes[1])
                        {
                            if (monSetsCancelled)
                                break;

                            if (!build.AllowBroken && r1.SetIs4)
                            {
                                if (c4set == null)
                                    c4set = r1.Set;
                                else if (c4set != r1.Set)
                                    continue;
                            }

                            foreach (var r2 in build.runes[2])
                            {
                                if (monSetsCancelled)
                                    break;

                                if (!build.AllowBroken && r2.SetIs4)
                                {
                                    if (c4set == null)
                                        c4set = r2.Set;
                                    else if (c4set != r2.Set)
                                        continue;
                                }

                                foreach (var r3 in build.runes[3])
                                {
                                    if (monSetsCancelled)
                                        break;

                                    if (!build.AllowBroken && r3.SetIs4)
                                    {
                                        if (c4set != r3.Set)
                                            continue;
                                    }

                                    foreach (var r4 in build.runes[4])
                                    {
                                        if (monSetsCancelled)
                                            break;

                                        if (!build.AllowBroken && r4.SetIs4)
                                        {
                                            if (c4set != r4.Set)
                                                continue;
                                        }

                                        foreach (var r5 in build.runes[5])
                                        {
                                            if (monSetsCancelled)
                                                break;

                                            if (!build.AllowBroken && r5.SetIs4)
                                            {
                                                if (c4set != r5.Set)
                                                    continue;
                                            }

                                            var key = new RuneKey(r0, r1, r2, r3, r4, r5);
                                            /*
                                            var compKey = r0.ID.ToString() + "," +
                                                            r1.ID.ToString() + "," +
                                                            r2.ID.ToString() + "," +
                                                            r3.ID.ToString() + "," +
                                                            r4.ID.ToString() + "," +
                                                            r5.ID.ToString();*/

                                            //if (!buildsToGen.Contains(compKey) && !monsList.ContainsKey(compKey))
                                            {
                                                //monsList.AddOrUpdate(compKey, (Monster)null, (k, v) => v);
                                                buildsToGen.Add(key);
                                                Interlocked.Increment(ref num);
                                                // don't lag the comp
                                            }

                                            if (DateTime.Now > start.AddSeconds(1))
                                            {
                                                if (!IsDisposed)
                                                {
                                                    Invoke((MethodInvoker)delegate
                                                    {
                                                        // put the thing in on the main thread and bump the progress bar
                                                        toolStripProgressBar1.Value = (int)(toolStripProgressBar1.Maximum * num / (double)total);
                                                        toolStripStatusLabel1.Text = "Generating Sets " + num.ToString("#,##0") + "/" + total.ToString("#,##0") + " [" + buildsToGen.Count.ToString("#,##0") + "]";

                                                    });
                                                }
                                                Thread.Sleep(20);
                                                start = DateTime.Now;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                    if (!IsDisposed) {
                        Invoke((MethodInvoker)delegate {
                            toolStripProgressBar1.Value = (int)(toolStripProgressBar1.Maximum * num / (double)total);
                            toolStripStatusLabel1.Text = "Generating Sets " + num.ToString("#,##0") + "/" + total.ToString("#,##0");
                        });
                    }
                    progState = 1;
                }
            });
        }

        public void RegenBuilds() {
            Task.Factory.StartNew(() => {
                // cancel the current worker
                buildGenCancelled = true;
                // wait for anyone to stop
                lock (buildGenLock) {
                    // stop cancelling
                    buildGenCancelled = false;
                    int num = 0;
                    int total = monsList.Count;
                    DateTime start = DateTime.Now;
                    var pfer = Parallel.ForEach(monsList, (kv, loop) =>
                        {
                            if (buildGenCancelled)
                                loop.Break();
                            if (kv.Value == null)
                            {
                                var tb = build.GenBuild(kv.Key.rs);
                                if (tb != null)
                                {
                                    monsList[kv.Key] = tb;
                                }
                                Invoke((MethodInvoker)delegate
                                {
                                    // put the thing in on the main thread and bump the progress bar
                                    Interlocked.Increment(ref num);
                                    toolStripProgressBar1.Value = (int)(toolStripProgressBar1.Maximum * num / (double)total);
                                    toolStripStatusLabel1.Text = "Generating Builds " + num + "/" + total;
                                });
                                if (DateTime.Now > start.AddSeconds(1))
                                {
                                    Thread.Sleep(20);
                                    start = DateTime.Now;
                                }
                            }
                        });
                }
                RegenList();
            });
        }

        private void AddBuild(Monster mon) {
            //string compKey = string.Join(",", mon.Current.Runes.Select(r => r.ID.ToString()));
            var key = new RuneKey(mon.Current.Runes);
            if (!monsList.ContainsKey(key)) {
                monsList.AddOrUpdate(key, mon, (k, v) => mon);
                RegenList();
            }
        }

        private void RegenList() {
            Task.Factory.StartNew(() => {
                listGenCancelled = true;
                lock (listGenLock) {
                    listGenCancelled = false;
                    var llist = monsList.Where(m => m.Value != null).Select(m => m.Value).OrderByDescending(m => m.score).Take(Program.Settings.TestShow);
                    var ilist = loadoutList.Items.OfType<ListViewItem>();

                    int num = 0;
                    int total = ilist.Count() + llist.Count();

                    foreach (var li in ilist) {
                        if (listGenCancelled)
                            break;

                        var mon = li.Tag as Monster;
                        if (mon == null)
                            continue;
                        var compKey = string.Join(",", mon.Current.Runes.Select(r => r.Id.ToString()));
                        if (!llist.Any(m => string.Join(",", m.Current.Runes.Select(r => r.Id.ToString())) == compKey)) {
                            // new list doen't contain old build
                            li.Remove();
                        }
                        else {
                            // new list does contain old build
                            llist = llist.Except(llist.Where(m => string.Join(",", m.Current.Runes.Select(r => r.Id.ToString())) == compKey));
                            total--;
                        }
                        Invoke((MethodInvoker)delegate {
                            // put the thing in on the main thread and bump the progress bar
                            Interlocked.Increment(ref num);
                            toolStripProgressBar1.Value = (int)(toolStripProgressBar1.Maximum * num / (double)total);
                            toolStripStatusLabel1.Text = "Cleaning List " + num + "/" + total;
                        });
                    }

                    if (listGenCancelled)
                        return;

                    foreach (var b in llist) {
                        if (listGenCancelled)
                            break;

                        ListViewItem li = new ListViewItem();
                        var Cur = b.GetStats();


                        int underSpec = 0;
                        int under12 = 0;
                        foreach (var r in b.Current.Runes) {
                            if (r.Level < 12)
                                under12 += 12 - r.Level;
                            if (build.RunePrediction.ContainsKey((SlotIndex)r.Slot) && r.Level < (build.RunePrediction[(SlotIndex)r.Slot].Key ?? 0))
                                underSpec += (build.RunePrediction[(SlotIndex)r.Slot].Key ?? 0) - r.Level;
                        }

                        li.SubItems.Add(underSpec + "/" + under12);
                        double pts = GetPoints(Cur, (str, i) => { li.SubItems.Add(str); });
                        b.score = pts;

                        // put the sum points into the first item
                        li.SubItems[0].Text = pts.ToString("0.##");

                        li.Tag = b;
                        if (Program.Settings.TestGray && b.Current.Runes.Any(r => r.Locked))
                            li.ForeColor = Color.Gray;
                        else {
                            if (b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 2) &&
                                b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 4)) {
                                li.ForeColor = Color.Green;
                            }
                            else if (b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 2)) {
                                li.ForeColor = Color.Goldenrod;
                            }
                            else if (b.Current.Sets.Any(rs => RuneProperties.MagicalSets.Contains(rs) && Rune.SetRequired(rs) == 4)) {
                                li.ForeColor = Color.DarkBlue;
                            }
                        }

                        if (!IsDisposed && IsHandleCreated) {
                            Invoke((MethodInvoker)delegate {
                                // put the thing in on the main thread and bump the progress bar
                                loadoutList.Items.Add(li);
                                Interlocked.Increment(ref num);
                                toolStripProgressBar1.Value = (int)(toolStripProgressBar1.Maximum * num / (double)total);
                                toolStripStatusLabel1.Text = "Adding to list " + num + "/" + total;
                            });
                        }
                    }
                }
            });
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {

        }

        private void btnHelp_Click(object sender, EventArgs e) {
            if (Main.help != null)
                Main.help.Close();

            Main.help = new Help();
            Main.help.url = Environment.CurrentDirectory + "\\User Manual\\test.html";
            Main.help.Show();
        }

        private void loadoutList_SelectedIndexChanged(object sender, EventArgs e) {
            if (loadoutList.FocusedItem != null) {
                var item = loadoutList.FocusedItem;
                if (item.Tag != null) {
                    Monster mon = item.Tag as Monster;
                    if (mon != null) {
                        if (Main.runeDisplay == null || Main.runeDisplay.IsDisposed)
                            Main.runeDisplay = new RuneDisplay();
                        if (!Main.runeDisplay.Visible) {
                            Main.runeDisplay.Show(this);
                        }
                        Main.runeDisplay.Owner = this;
                        Main.runeDisplay.Location = new Point(0, 0);
                        Main.runeDisplay.UpdateRunes(mon.Current.Runes);
                    }
                }
            }
        }

        private void btn_powerrunes_Click(object sender, EventArgs e) {
            if (!building) {
                var mons = loadoutList.Items.OfType<ListViewItem>().Select(lvi => lvi.Tag as Monster).Where(m => m != null);
                List<Rune> lrunes = new List<Rune>();
                foreach (var g in mons) {
                    foreach (var r in g.Current.Runes) {
                        r.manageStats.AddOrUpdate("besttestscore", g.score, (k, v) => v < g.score ? g.score : v);

                        if (!lrunes.Contains(r) && (r.Level < 12 || r.Level < build.GetFakeLevel(r)))
                            lrunes.Add(r);
                    }
                }

                using (var qq = new RuneSelect()) {
                    qq.runes = lrunes;
                    qq.sortFunc = r => -(int)r.manageStats.GetOrAdd("besttestscore", 0);
                    qq.runeStatKey = "besttestscore";
                    qq.ShowDialog();
                }
            }
        }

        private void Generate_FormClosing(object sender, FormClosingEventArgs e) {
            if (Main.runeDisplay != null && !Main.runeDisplay.IsDisposed && Main.runeDisplay.Owner == this) {
                Main.runeDisplay.Owner = Main.currentMain;
                Main.runeDisplay.Location = new Point(Main.currentMain.Location.X + Main.currentMain.Width, Main.currentMain.Location.Y);
            }
        }
    }
}
