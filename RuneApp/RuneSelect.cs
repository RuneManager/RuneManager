using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RuneOptim;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp {
    public partial class RuneSelect : Form {
        public IEnumerable<Rune> runes;

        public Rune returnedRune = null;

        public Build build = null;

        public Func<Rune, long> sortFunc = r => (long)r.Id;
        public string runeStatKey = null;

        public SlotIndex slot = SlotIndex.Global;

        public RuneSelect() {
            InitializeComponent();
            // when show, check if we have been given cool things
            Shown += RuneSelect_Shown;

            var sorter = new ListViewSort();
            listRunes.ListViewItemSorter = sorter;

        }

        void RuneSelect_Shown(object sender, EventArgs e) {
            if (runeStatKey == null)
                this.runeStats.Width = 0;

            // dump out the rune details
            foreach (Rune rune in runes.OrderBy(sortFunc)) {
                double points = 0;
                if (build != null) {
                    points = build.ScoreRune(rune, build.GetFakeLevel(rune), false);

                    if (rune.Slot % 2 == 0 && build.SlotStats[rune.Slot - 1].Any() && !build.SlotStats[rune.Slot - 1].Contains(rune.Main.Type.ToForms()))
                        continue;
                }

                ListViewItem item = new ListViewItem(new string[]{
                    rune.Set.ToString(),
                    rune.Id.ToString(),
                    rune.Grade.ToString(),
                    Rune.StringIt(rune.Main.Type, true),
                    rune.Main.Value.ToString(),
                    points.ToString(),
                    runeStatKey == null ? null : rune.manageStats.GetOrAdd(runeStatKey, 0).ToString()
                });
                if (build != null)
                    item.ForeColor = Color.Gray;
                if ((rune.IsUnassigned) && !Program.Settings.UseEquipped)
                    item.ForeColor = Color.Red;
                // stash the rune into the tag
                item.Tag = rune;


                listRunes.Items.Add(item);
            }
            if (build != null) {
                listRunes.Columns[5].Width = 50;

                // Find all the runes in the build for the slot
                List<Rune> fr = new List<Rune>();
                switch (slot) {
                    case SlotIndex.Even:
                        fr.AddRange(build.runes[1]);
                        fr.AddRange(build.runes[3]);
                        fr.AddRange(build.runes[5]);
                        break;
                    case SlotIndex.Odd:
                        fr.AddRange(build.runes[0]);
                        fr.AddRange(build.runes[2]);
                        fr.AddRange(build.runes[4]);
                        break;
                    case SlotIndex.Global:
                        fr.AddRange(build.runes[0]);
                        fr.AddRange(build.runes[1]);
                        fr.AddRange(build.runes[2]);
                        fr.AddRange(build.runes[3]);
                        fr.AddRange(build.runes[4]);
                        fr.AddRange(build.runes[5]);
                        break;
                    default:
                        fr.AddRange(build.runes[(int)slot - 1]);
                        break;
                }
                // find the chosen runes in the list and colour them in
                foreach (ListViewItem li in listRunes.Items) {
                    Rune rt = (Rune)li.Tag;
                    if (rt != null && fr.Contains(rt) && (Program.Settings.UseEquipped || (rt.IsUnassigned) || rt.AssignedName == build.MonName)) {
                        li.ForeColor = Color.Black;
                    }
                }
            }

            // if we are given a rune, see if we can pre-select it
            if (returnedRune != null) {
                ShowRune(returnedRune);
                // find the precious
                foreach (ListViewItem li in listRunes.Items) {
                    if (li.Tag != null && ((Rune)li.Tag).Equals(returnedRune))
                        listRunes.SelectedIndices.Add(li.Index);
                }
            }

            ((ListViewSort)listRunes.ListViewItemSorter).OrderBy(runesPoints.Index, false);
            listRunes.Sort();
        }

        // preview the selected rune
        private void listView2_SelectedIndexChanged(object sender, EventArgs e) {
            if (listRunes.FocusedItem != null) {
                var item = listRunes.FocusedItem;
                if (item.Tag != null) {
                    Rune rune = (Rune)item.Tag;
                    ShowRune(rune);
                }
            }
        }

        // Show the little preview window of the given rune
        private void ShowRune(Rune rune) {
            runeBox1.Show();
            runeBox1.SetRune(rune);
        }

        // return the highlighted rune
        private void button3_Click(object sender, EventArgs e) {
            if (listRunes.SelectedItems.Count > 0) {
                returnedRune = (Rune)listRunes.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        // cancel selecting a rune
        private void button1_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // return the selected rune
        private void listView2_DoubleClick(object sender, EventArgs e) {
            if (listRunes.SelectedItems.Count > 0) {
                returnedRune = (Rune)listRunes.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e) {
            var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
            sorter.OnColumnClick(e.Column);
            ((ListView)sender).Sort();
        }

    }
}
