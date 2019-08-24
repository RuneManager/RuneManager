using System;
using System.Drawing;
using System.Windows.Forms;
using RuneOptim.swar;

namespace RuneApp {
	public partial class MonSelect : Form {
		public Monster retMon = null;

		public MonSelect(Monster c) {
			InitializeComponent();

			foreach (Monster mon in Program.data.Monsters) {
				string pri = "";
				if (mon.priority != 0)
					pri = mon.priority.ToString();

				ListViewItem item = new ListViewItem(new string[]{
					mon.FullName,
					mon.Id.ToString(),
					pri,
				});
				if (mon.inStorage)
					item.ForeColor = Color.Gray;

				item.Tag = mon;
				dataMonsterList.Items.Add(item);
				if (mon == c)
					item.Selected = true;
			}
		}

		void btn_select_Click(object sender, EventArgs e) {
			if (dataMonsterList.SelectedItems.Count > 0) {
				retMon = (Monster)dataMonsterList.SelectedItems[0].Tag;
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void btn_cancel_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void dataMonsterList_SelectedIndexChanged(object sender, EventArgs e) {
			if (dataMonsterList.SelectedItems.Count > 0) {
				retMon = (Monster)dataMonsterList.SelectedItems[0].Tag;
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
