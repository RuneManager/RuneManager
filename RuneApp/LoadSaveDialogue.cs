using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RuneApp {
    public partial class LoadSaveDialogue : Form
    {
        private string file = null;
        public string Filename
        {
            [DebuggerStepThrough]
            get { return file; }
            set
            {
                file = value;
                btnOpen.Enabled = !string.IsNullOrWhiteSpace(file);
            }
        }

        private string lookupFile = null;
        private string LookupFile
        {
            [DebuggerStepThrough]
            get
            {
                return lookupFile;
            }
            set
            {
                lookupFile = value;
                lblFile.Text = lookupFile;
                if (lookupFile.Length > 40)
                {
                    string[] paths = lookupFile.Split('\\');
                    StringBuilder sb = new StringBuilder();
                    int i = 0;
                    while (sb.Length < 15)
                    {
                        sb.Append(paths[i] + "\\");
                        i++;
                    }
                    sb.Append("...");
                    StringBuilder sbEnd = new StringBuilder();
                    i = paths.Length - 1;
                    while (sbEnd.Length < 25)
                    {
                        sbEnd.Insert(0, "\\" + paths[i]);
                        i--;
                    }
                    lblFile.Text = sb.ToString() + sbEnd.ToString();
                }
                radLookup.Enabled = !string.IsNullOrWhiteSpace(lookupFile);
                if (!string.IsNullOrWhiteSpace(lookupFile))
                    radLookup.Checked = true;
            }
        }
        private string swarfarmFile = null;

        string[] localFiles = null;

        public LoadSaveDialogue()
        {
            InitializeComponent();

            if (File.Exists("save.json"))
                radSave.Enabled = true;

            localFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.json");

            bool isLocal = false;

            if (localFiles.Any())
            {
                radSwarfarm.Enabled = true;
                cboxSwarfarm.Items.AddRange(localFiles.Select(s => s.Replace(Environment.CurrentDirectory + "\\", "")).ToArray());
                cboxSwarfarm.SelectedIndex = 0;
                cboxSwarfarm.Enabled = true;
                if (cboxSwarfarm.Items.Contains(Program.Settings.SaveLocation))
                {
                    isLocal = true;
                    cboxSwarfarm.SelectedIndex = cboxSwarfarm.Items.IndexOf(Program.Settings.SaveLocation);
                }
            }

            if (!isLocal)
                LookupFile = Program.Settings.SaveLocation;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(file))
            {
                //Program.WatchSave = (MessageBox.Show("Watch file for live updates?", "Load Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) ;

                if (radLookup.Checked && !string.IsNullOrWhiteSpace(lookupFile))
                    Filename = lookupFile;
                else if (radSwarfarm.Checked && !string.IsNullOrWhiteSpace(swarfarmFile))
                    Filename = swarfarmFile;
                else if (radSave.Checked)
                    Filename = "save.json";

                Program.Settings.SaveLocation = Filename;
                Program.Settings.Save();

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                openFileDialog.Filter = "JSON Data|*.json|SWarFarm JSON|*-swarfarm.json|All Files|*.*";
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LookupFile = openFileDialog.FileName;
                }
            }
        }

        private void cboxSwarfarm_SelectedIndexChanged(object sender, EventArgs e)
        {
            // do things
            if (cboxSwarfarm.SelectedIndex == -1)
            {
                swarfarmFile = null;
                radSwarfarm.Checked = false;
                return;
            }

            swarfarmFile = cboxSwarfarm.Items[cboxSwarfarm.SelectedIndex].ToString();

            radSwarfarm.Checked = true;
        }

        private void radLookup_CheckedChanged(object sender, EventArgs e)
        {
            if (radLookup.Checked)
                Filename = lookupFile;
        }

        private void radSwarfarm_CheckedChanged(object sender, EventArgs e)
        {
            Filename = swarfarmFile;
        }

        private void radSave_CheckedChanged(object sender, EventArgs e)
        {
            Filename = "save.json";
        }
    }
}
