using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RuneOptim;

namespace RuneApp
{
	public partial class BuildWizard : Form
	{
		Build build;

		public BuildWizard(Build b)
		{
			InitializeComponent();
			if (b.mon == null)
			{
				MessageBox.Show("Error: Build has no monster!");
				Program.log.Error("Build has no monster!");
				this.DialogResult = DialogResult.Abort;
				this.Close();
			}
			build = b;
			this.lbPrebuild.Text = "Prebuild Template for " + build.mon.Name;
		}

		private void btnCreate_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
