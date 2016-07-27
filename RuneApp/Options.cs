using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuneApp
{
    public partial class Options : Form
    {
        Dictionary<string, CheckBox> checks = new Dictionary<string, CheckBox>();
        Dictionary<string, TextBox> nums = new Dictionary<string, TextBox>();
        bool loading = true;

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
            InitializeComponent();

            FormClosing += Options_FormClosing;

            AddCheck("locktest", cGenLockTest);
            AddCheck("splitassign", cDisplaySplit);
            AddCheck("noupdate", cOtherUpdate);
            AddCheck("nostats", cOtherStats);
            AddCheck("testgray", cDisplayGray);

            AddNum("testGen", gTestRun);
            AddNum("testShow", gTestShow);

            if (Main.config != null)
            {
                bool check = false;
                int val = 0;

                foreach (var p in checks)
                {
                    if (Main.config.AppSettings.Settings.AllKeys.Contains(p.Key))
                    {
                        if (bool.TryParse(Main.config.AppSettings.Settings[p.Key].Value, out check))
                            p.Value.Checked = check;
                    }
                }
                foreach (var p in nums)
                {
                    if (Main.config.AppSettings.Settings.AllKeys.Contains(p.Key))
                    {
                        if (int.TryParse(Main.config.AppSettings.Settings[p.Key].Value, out val))
                            p.Value.Text = val.ToString();
                    }
                }
            }
            loading = false;
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            var s = Main.MakeStats;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void check_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            var ctrl = (CheckBox)sender;
            string key = ctrl.Tag.ToString();
            if (Main.config != null)
            {
                Main.config.AppSettings.Settings.Remove(key);
                Main.config.AppSettings.Settings.Add(key, ctrl.Checked.ToString());
                Main.config.Save(ConfigurationSaveMode.Modified);
            }
        }

        private void num_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;

            var ctrl = (TextBox)sender;
            string key = ctrl.Tag.ToString();
            int val = 0;
            if (Main.config != null && int.TryParse(ctrl.Text, out val))
            {
                Main.config.AppSettings.Settings.Remove(key);
                Main.config.AppSettings.Settings.Add(key, ctrl.Text);
                Main.config.Save(ConfigurationSaveMode.Modified);
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            if (Main.help != null)
                Main.help.Close();

            Main.help = new Help();
            Main.help.url = Environment.CurrentDirectory + "\\User Manual\\options.html";
            Main.help.Show();
        }
    }
}
