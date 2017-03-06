using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace RuneApp
{
    public partial class Options : Form
    {
        Dictionary<string, CheckBox> checks = new Dictionary<string, CheckBox>();
        Dictionary<string, TextBox> nums = new Dictionary<string, TextBox>();
        bool loading;

        public void AddCheck(string config, CheckBox box)
        {
            box.Tag = config;
            checks.Add(config, box);
        }

        public void AddNum(string config, TextBox box)
        {
            box.Tag = config;
            
            nums.Add(config, box);
        }

        public Options()
        {
            loading = true;
            InitializeComponent();

            FormClosing += Options_FormClosing;

            AddCheck("locktest", cGenLockTest);
            AddCheck("splitassign", cDisplaySplit);
            AddCheck("noupdate", cOtherUpdate);
            AddCheck("nostats", cOtherStats);
            AddCheck("testgray", cDisplayGray);
            AddCheck("colorteams", cColorTeams);
            AddCheck("startuphelp", cHelpStart);
            cHelpStart.CheckedChanged += CHelpStart_CheckedChanged;

            AddNum("testGen", gTestRun);
            AddNum("testShow", gTestShow);
            AddNum("testTime", gTestTime);

			if (Main.config != null)
            {
                foreach (var p in checks)
                {
                    if (!Main.config.AppSettings.Settings.AllKeys.Contains(p.Key)) continue;

                    bool check;
                    if (bool.TryParse(Main.config.AppSettings.Settings[p.Key].Value, out check))
                        p.Value.Checked = check;
                }
                foreach (var p in nums)
                {
                    if (!Main.config.AppSettings.Settings.AllKeys.Contains(p.Key)) continue;

                    int val;
                    if (int.TryParse(Main.config.AppSettings.Settings[p.Key].Value, out val))
                        p.Value.Text = val.ToString();
                }
            }
            loading = false;
        }

        private void CHelpStart_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            if (Main.help != null)
                Main.help.SetStartupCheck(cHelpStart.Checked);
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            Main.UpdateMakeStats();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void check_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            var ctrl = (CheckBox)sender;
            string key = ctrl.Tag.ToString();

            if (Main.config == null) return;

            Main.config.AppSettings.Settings.Remove(key);
            Main.config.AppSettings.Settings.Add(key, ctrl.Checked.ToString());
            Main.config.Save(ConfigurationSaveMode.Modified);
        }

        private void num_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            var ctrl = (TextBox)sender;
            string key = ctrl.Tag.ToString();
            int val;
            if (Main.config == null || !int.TryParse(ctrl.Text, out val)) return;

            Main.config.AppSettings.Settings.Remove(key);
            Main.config.AppSettings.Settings.Add(key, ctrl.Text);
            Main.config.Save(ConfigurationSaveMode.Modified);
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (Main.help != null)
                Main.help.Close();
            
            Main.help = new Help();
            Main.Instance.OpenHelp(Environment.CurrentDirectory + "\\User Manual\\options.html");//, this);
        }
	}
}
