using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuneApp
{
	public partial class LoadSaveDialogue : Form
	{
		private string file = null;
		public string Filename
		{
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

			localFiles = Directory.GetFiles(Environment.CurrentDirectory, "*-swarfarm.json");

			if (localFiles.Any())
			{
				radSwarfarm.Enabled = true;
				cboxSwarfarm.Items.AddRange(localFiles.Select(s => s.Replace(Environment.CurrentDirectory + "\\", "")).ToArray());
				cboxSwarfarm.SelectedIndex = 0;
				cboxSwarfarm.Enabled = true;
			}

			LookupFile = Program.Settings.SaveLocation;
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(file))
			{
				Program.WatchSave = (MessageBox.Show("Watch file for live updates?", "Load Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) ;

				if (radLookup.Checked && !string.IsNullOrWhiteSpace(lookupFile))
					Program.Settings.SaveLocation = lookupFile;
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
				openFileDialog.Filter = "SWarFarm JSON|*-swarfarm.json|JSON Data|*.json|All Files|*.*";
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
			swarfarmFile = cboxSwarfarm.SelectedItem.ToString();

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
