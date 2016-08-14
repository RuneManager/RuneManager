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
    public partial class MonSelect : Form
    {
        Monster current = null;
        public Monster retMon = null;

        public MonSelect(Monster c)
        {
            InitializeComponent();
            current = c;

            foreach (Monster mon in Main.data.Monsters)
            {
                string pri = "";
                if (mon.priority != 0)
                    pri = mon.priority.ToString();

                ListViewItem item = new ListViewItem(new string[]{
                    mon.Name,
                    mon.ID.ToString(),
                    pri,
                });
                if (mon.inStorage)
                    item.ForeColor = Color.Gray;

                item.Tag = mon;
                dataMonsterList.Items.Add(item);
            }
        }

        private void btn_select_Click(object sender, EventArgs e)
        {
            if (dataMonsterList.SelectedItems.Count > 0)
            {
                retMon = (Monster)dataMonsterList.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dataMonsterList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataMonsterList.SelectedItems.Count > 0)
            {
                retMon = (Monster)dataMonsterList.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
