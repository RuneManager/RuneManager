using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RuneOptim.swar;

namespace RuneApp {
    public partial class MonSelect : Form {
        public Monster retMon = null;

        string lastSearch = "";
        List<ListViewItem> removed = new List<ListViewItem>();

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
                    mon.level.ToString(),
                });
                if (mon.inStorage)
                    item.ForeColor = Color.Gray;

                item.Tag = mon;
                dataMonsterList.Items.Add(item);
                if (mon == c)
                    item.Selected = true;
            }
            
            var sorter = new ListViewSort();
            dataMonsterList.ListViewItemSorter = sorter;
            sorter.OnColumnClick(MonName.Index);
            sorter.OnColumnClick(MonLvl.Index);
            sorter.OnColumnClick(MonPriority.Index);
            dataMonsterList.Sort();

        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e) {
            var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
            sorter.OnColumnClick(e.Column);
            ((ListView)sender).Sort();
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

        private void TextBox1_TextChanged(object sender, EventArgs e) {

            if (lastSearch.Contains(textBox1.Text) || textBox1.Text == string.Empty) {
                var sub = removed.Where(it => it.SubItems[0].Text.Contains(textBox1.Text)).ToArray();

                dataMonsterList.Items.AddRange(sub.ToArray());
                foreach (var s in sub)
                    removed.Remove(s);
            }

            foreach (var i in dataMonsterList.Items.OfType<ListViewItem>()) {
                if (!i.SubItems[0].Text.Contains(textBox1.Text)) {
                    removed.Add(i);
                }
            }

            foreach (var r in removed) {
                dataMonsterList.Items.Remove(r);
            }

            lastSearch = textBox1.Text;
        }
    }
}
