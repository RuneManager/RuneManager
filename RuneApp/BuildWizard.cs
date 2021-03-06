using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using RuneOptim;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp {
    public partial class BuildWizard : Form {
        Build build;
        object loadLock = new object();
        bool loading = true;

        bool Loading {
            get { lock (loadLock) { return loading; } }
            set { lock (loadLock) { loading = value; } }
        }

        bool CanLoad {
            get {
                lock (loadLock) {
                    if (loading) return false;
                    else {
                        loading = true;
                        return true;
                    }
                }
            }
        }

        ListViewItem defTemplate = null;

        static List<Build> templateList = null;
        static Dictionary<string, IEnumerable<Build>> customBuilds = new Dictionary<string, IEnumerable<Build>>();

        static List<Build> TemplateList {
            get {
                if (templateList == null) {
                    if (File.Exists(global::RuneApp.Properties.Resources.TemplatesJSON)) {
                        var bstr = File.ReadAllText(global::RuneApp.Properties.Resources.TemplatesJSON);
                        templateList = JsonConvert.DeserializeObject<List<Build>>(bstr).OrderBy(t => t.Priority).ThenBy(t => t.MonName).ToList();
                    }
                    else {
                        Program.LineLog.Error(global::RuneApp.Properties.Resources.TemplatesJSON + " not found?");
                        templateList = new List<Build>();
                    }
                }

                return templateList;
            }
        }

        public BuildWizard(Build newB) {
            InitializeComponent();
            if (newB.Mon == null) {
                MessageBox.Show("Error: Build has no monster!");
                Program.LineLog.Error("Build has no monster!");
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            build = newB;
            this.lbPrebuild.Text = "Prebuild Template for " + build.Mon.FullName;

            build.Mon.RefreshStats();

            defTemplate = addTemplate(new Build() { MonName = "<None>" }, prebuildList.Groups[0]);

            prebuildList.Select();
            defTemplate.Selected = true;
        }

        // Hack for matching unawake to woke
        bool MatchMostlyId(int lhs, int rhs) {
            return lhs == rhs || lhs - 10 == rhs || lhs == rhs - 10;
        }

        string GetNiceName(string name) {
            foreach (var c in Path.GetInvalidFileNameChars()) {
                name = name.Replace(c, '_');
            }
            return name;
        }

        void pullTemplates(IEnumerable<Build> templates, int forceGroup = 0) {
            // load more
            foreach (var b in templates.Where(t => t.MonId == 0 || MatchMostlyId((int)t.MonId, build.Mon.monsterTypeId))) {
                if (b.ID == -1) {
                    try {
                        IEnumerable<Build> temps = null;
                        int fGroup = 0;
                        if (customBuilds.ContainsKey(b.MonName)) {
                            temps = customBuilds[b.MonName];
                        }
                        else {
                            string data = null;
                            if (b.MonName.ToLower().Contains("http")) {
                                Directory.CreateDirectory("cache");
                                fGroup = 2;
                                var fname = "cache/" + GetNiceName(b.MonName);
                                if (File.Exists(fname) && new FileInfo(fname).CreationTime > DateTime.Now.AddDays(-2))
                                    File.Delete(fname);

                                if (File.Exists(fname)) {
                                    data = File.ReadAllText(fname);
                                }
                                else {
                                    using (WebClient client = new WebClient()) {
                                        data = client.DownloadString(b.MonName);
                                        File.WriteAllText(fname, data);
                                    }
                                }
                            }
                            else {
                                if (File.Exists(b.MonName)) {
                                    data = File.ReadAllText(b.MonName);
                                    fGroup = 1;
                                }
                            }
                            if (data != null) {
                                temps = JsonConvert.DeserializeObject<IEnumerable<Build>>(data).OrderBy(t => t.Priority).ThenBy(t => t.MonName);
                                foreach (var t in temps) {
                                    t.ID = fGroup;
                                }
                                customBuilds.Add(b.MonName, temps);
                            }
                        }
                        if (temps != null)
                            pullTemplates(temps, fGroup);
                    }
                    catch (Exception e) {
                        Program.LineLog.Error("Failed " + b.MonName, e);
                        MessageBox.Show("Couldn't parse templates at " + b.MonName + " with " + e.GetType(), "Template Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else {
                    var gid = forceGroup == 0 ? b.ID : forceGroup;
                    ListViewGroup group = null;
                    Invoke((MethodInvoker)delegate {
                        group = prebuildList.Groups.Count > gid ? prebuildList.Groups[gid] : null;
                        if (b.Teams.Count > 0) {
                            group = prebuildList.Groups.OfType<ListViewGroup>().FirstOrDefault(g => g.Header == b.Teams.First());
                            if (group == null) {
                                group = new ListViewGroup(b.Teams.First());
                                prebuildList.Groups.Add(group);
                            }
                        }
                        addTemplate(b, group);
                    });
                }
            }

        }

        ListViewItem addTemplate(Build b, ListViewGroup group) {
            var lvi = new ListViewItem();
            lvi.Text = b.MonName;
            lvi.Tag = b;
            lvi.Group = group;
            this.prebuildList.Items.Add(lvi);
            return lvi;
        }

        private void RecheckPreview() {
            if (!CanLoad) return;

            try {
                runeDial.Loadout = null;
                build.RunesUseLocked = cPreviewLocked.Checked;
                build.AutoRuneSelect = true;
                build.RunesUseEquipped = true;
                build.AutoRuneAmount = 8;

                build.GenRunes(Program.data);

                if (build.runes.Any(rs => rs.Length > 10))
                    return;
                var res = build.GenBuilds();

                if (res == BuildResult.Success && build.Best != null) {
                    runeDial.Loadout = build.Best.Current;
                    statPreview.Stats.SetTo(build.Best.GetStats());
                    statPreview.RecheckExtras();
                }
                else {
                    // TODO: warning
                }

                build.AutoRuneAmount = Build.AutoRuneAmountDefault;
                build.AutoRuneSelect = false;
            }
            catch { }
            finally {
                Loading = false;
            }
        }

        private void BuildWizard_Load(object sender, EventArgs e) {
            cShowWizard.Checked = Program.Settings.ShowBuildWizard;
            statBase.Stats = build.Mon;
            statGoal.Stats = build.Goal;
            statScore.Stats = build.Sort;
            statCurrent.Stats = build.Mon.GetStats();
            statPreview.Stats = new Stats(build.Mon.GetStats(), true);
            statTotal.Stats = new Stats();

            statPreview.Stats.OnStatChanged += CalcStats_OnStatChanged;
            statScore.Stats.OnStatChanged += CalcStats_OnStatChanged;
            statScore.Stats.OnStatChanged += ScoreStats_OnStatChanged;

            new System.Threading.Thread(() => {
                pullTemplates(TemplateList);
            }).Start();

            Loading = false;
        }

        private void BuildWizard_FormClosing(object sender, FormClosingEventArgs e) {
            statPreview.Stats.OnStatChanged -= CalcStats_OnStatChanged;
            statScore.Stats.OnStatChanged -= CalcStats_OnStatChanged;
            statScore.Stats.OnStatChanged -= ScoreStats_OnStatChanged;
        }

        private void ScoreStats_OnStatChanged(object sender, StatModEventArgs e) {
            RecheckPreview();
        }

        private void CalcStats_OnStatChanged(object sender, StatModEventArgs e) {
            build.CalcScore(statPreview.Stats, statTotal.Stats);
        }

        private void btnCreate_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cShowWizard_CheckedChanged(object sender, EventArgs e) {
            if (Loading) return;

            Program.Settings.ShowBuildWizard = cShowWizard.Checked;
            Program.Settings.Save();
        }

        private void prebuildList_SelectedIndexChanged(object sender, EventArgs e) {
            if (!CanLoad) return;
            try {
                var item = prebuildList.SelectedItems.OfType<ListViewItem>().FirstOrDefault();
                if (item == null) return;

                cShowWizard.Enabled = (item == defTemplate);
                if (!cShowWizard.Enabled && !cShowWizard.Checked)
                    cShowWizard.Checked = true;

                var tb = item.Tag as Build;
                if (tb == null) return;

                build.AllowBroken = tb.AllowBroken;
                build.Minimum.SetTo(tb.Minimum);
                build.Maximum.SetTo(tb.Maximum);
                if (tb.Threshold.IsNonZero)
                    build.Threshold.SetTo(tb.Threshold);
                else
                    build.Threshold.SetTo(new Stats() {
                        Accuracy = 85,
                        Resistance = 100,
                        CritRate = 100,
                    });
                build.Goal.SetTo(tb.Goal);
                build.Sort.SetTo(tb.Sort);
                for (int i = 0; i < build.SlotStats.Length; i++) {
                    build.SlotStats[i].Clear();
                    build.SlotStats[i].AddRange(tb.SlotStats[i]);
                }
                build.BuildSets.Clear();
                build.BuildSets.AddRange(tb.BuildSets);
                build.RequiredSets.Clear();
                build.RequiredSets.AddRange(tb.RequiredSets);
            }
            finally {
                Loading = false;
            }
            RecheckPreview();
        }

        private void cPreviewLocked_CheckedChanged(object sender, EventArgs e) {
            RecheckPreview();
        }

        private void runeDial_RuneClick(object sender, RuneClickEventArgs e) {
            runeBox.SetRune(e.Rune);
            runeBox.Show();
        }
    }
}
