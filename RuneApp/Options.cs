using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RuneApp {
    public partial class Options : Form {
        Dictionary<string, CheckBox> checks = new Dictionary<string, CheckBox>();
        Dictionary<string, TextBox> nums = new Dictionary<string, TextBox>();
        bool loading;
        bool isUnchecking = false;

        public void AddCheck(string config, CheckBox box) {
            box.Tag = config;
            checks.Add(config, box);
        }

        public void AddNum(string config, TextBox box) {
            box.Tag = config;

            nums.Add(config, box);
        }

        public Options() {
            loading = true;
            InitializeComponent();

            FormClosing += Options_FormClosing;

            AddCheck("LockTest", cGenLockTest);
            AddCheck("SplitAssign", cDisplaySplit);
            AddCheck("CheckUpdates", cOtherUpdate);
            AddCheck("MakeStats", cOtherStats);
            AddCheck("TestGray", cDisplayGray);
            AddCheck("ColorTeams", cColorTeams);
            AddCheck("StartUpHelp", cHelpStart);
            cHelpStart.CheckedChanged += CHelpStart_CheckedChanged;
            AddCheck("InternalServer", cInternalServer);
            cInternalServer.CheckedChanged += CInternalServer_CheckedChanged;
            AddCheck("ShowBuildWizard", cBuildWizard);
            AddCheck("WatchSave", cWatchSave);
            AddCheck("ShowIreneOnStart", cIreneStart);
            AddCheck("IgnoreLess5", cIgnore5Less);

            AddNum("TestGen", gTestRun);
            AddNum("TestShow", gTestShow);
            AddNum("TestTime", gTestTime);

            foreach (var p in checks) {
                p.Value.Checked = (bool)Program.Settings[p.Key];
            }
            foreach (var p in nums) {
                p.Value.Text = ((int)Program.Settings[p.Key]).ToString();
            }

            loading = false;
        }

        private void CInternalServer_CheckedChanged(object sender, EventArgs e) {
            if (loading || isUnchecking)
                return;

            isUnchecking = true;
            cInternalServer.Checked = !cInternalServer.Checked;
            cInternalServer.Enabled = false;

            if (!cInternalServer.Checked) {
                try {
                    Program.master.Start();
                    cInternalServer.Checked = true;
                }
                catch (Exception ex) {
                    MessageBox.Show("Failed starting server\r\n" + ex.GetType() + ": " + ex.Message);
                }
            }
            else {
                try {
                    Program.master.Stop();
                    cInternalServer.Checked = false;
                }
                catch (Exception ex) {
                    MessageBox.Show("Failed stopping server\r\n" + ex.GetType() + ": " + ex.Message);
                }
            }
            cInternalServer.Enabled = true;
            isUnchecking = false;
        }

        private void CHelpStart_CheckedChanged(object sender, EventArgs e) {
            if (loading)
                return;

            if (Main.help != null)
                Main.help.SetStartupCheck(cHelpStart.Checked);
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e) {
        }

        private void button1_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void check_CheckedChanged(object sender, EventArgs e) {
            if (loading)
                return;

            var ctrl = (CheckBox)sender;
            string key = ctrl.Tag.ToString();

            Program.Settings[key] = ctrl.Checked;
            Program.Settings.Save();
        }

        private void num_TextChanged(object sender, EventArgs e) {
            if (loading)
                return;

            var ctrl = (TextBox)sender;
            string key = ctrl.Tag.ToString();
            int val;

            if (!int.TryParse(ctrl.Text, out val)) return;
            Program.Settings[key] = val;
            Program.Settings.Save();
        }

        private void btnHelp_Click(object sender, EventArgs e) {
            if (Main.help != null)
                Main.help.Close();

            Main.help = new Help();
            Main.Instance.OpenHelp(Environment.CurrentDirectory + "\\User Manual\\options.html");//, this);
        }

    }
}
