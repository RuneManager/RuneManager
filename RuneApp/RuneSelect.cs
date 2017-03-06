using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using RuneOptim;

namespace RuneApp
{
    public partial class RuneSelect : Form
    {
        public IEnumerable<Rune> runes;

        public Rune returnedRune = null;

        public Build build = null;

        public Func<Rune, int> sortFunc = r => r.ID;
        public string runeStatKey = null;

        public string slot = "";

        public RuneSelect()
        {
            InitializeComponent();
            // when show, check if we have been given cool things
            Shown += RuneSelect_Shown;

            var sorter = new ListViewSort();
            listView2.ListViewItemSorter = sorter;

        }

        void RuneSelect_Shown(object sender, EventArgs e)
        {
            if (runeStatKey == null)
                this.runeStats.Width = 0;

            // dump out the rune details
            foreach (Rune rune in runes.OrderBy(sortFunc))
            {
                double points = 0;
                if (build != null)
                {
                    points = build.ScoreRune(rune, build.GetFakeLevel(rune), false);

                    if (rune.Slot % 2 == 0)
                    {
                        if (build.slotStats[rune.Slot - 1].Count > 0)
                        {
                            if (!build.slotStats[rune.Slot - 1].Contains(rune.MainType.ToForms()))
                                continue;
                        }
                    }
                }

                ListViewItem item = new ListViewItem(new string[]{
                    rune.Set.ToString(),
                    rune.ID.ToString(),
                    rune.Grade.ToString(),
                    Rune.StringIt(rune.MainType, true),
                    rune.MainValue.ToString(),
                    points.ToString(),
                    runeStatKey == null ? null : rune.manageStats.GetOrAdd(runeStatKey, 0).ToString()
                });
                if (build != null)
                    item.ForeColor = Color.Gray;
                if ((rune.IsUnassigned) && !Program.useEquipped)
                    item.ForeColor = Color.Red;
                // stash the rune into the tag
                item.Tag = rune;
                

                listView2.Items.Add(item);
            }
            if (build != null)
            {
                listView2.Columns[5].Width = 50;

                // Find all the runes in the build for the slot
                List<Rune> fr = new List<Rune>();
                switch (slot)
                {
                    case "Evens":
                        fr.AddRange(build.runes[1]);
                        fr.AddRange(build.runes[3]);
                        fr.AddRange(build.runes[5]);
                        break;
                    case "Odds":
                        fr.AddRange(build.runes[0]);
                        fr.AddRange(build.runes[2]);
                        fr.AddRange(build.runes[4]);
                        break;
                    case "Global":
                        fr.AddRange(build.runes[0]);
                        fr.AddRange(build.runes[1]);
                        fr.AddRange(build.runes[2]);
                        fr.AddRange(build.runes[3]);
                        fr.AddRange(build.runes[4]);
                        fr.AddRange(build.runes[5]);
                        break;
                    default:
                        int slotn = int.Parse(slot)-1;
                        fr.AddRange(build.runes[slotn]);
                        
                        break;
                }
                // find the chosen runes in the list and colour them in
                foreach (Rune r in fr.OrderBy(sortFunc))
                {
                    foreach (ListViewItem li in listView2.Items)
                    {
                        Rune rt = (Rune)li.Tag;
                        if (rt != null)
                        {
                            if (rt.ID < r.ID)
                                continue;

                            if (rt.ID == r.ID && (Program.useEquipped || (rt.IsUnassigned) || rt.AssignedName == build.MonName))
                            {
                                li.ForeColor = Color.Black;

                                break;
                            }
                        }
                    }
                }
            }

            // if we are given a rune, see if we can pre-select it
            if (returnedRune != null)
            {
                ShowRune(returnedRune);
                // find the precious
                foreach (ListViewItem li in listView2.Items)
                {
                    if (li.Tag != null && ((Rune)li.Tag).Equals(returnedRune))
                        listView2.SelectedIndices.Add(li.Index);
                }
            }
        }

        // preview the selected rune
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.FocusedItem != null)
            {
                var item = listView2.FocusedItem;
                if (item.Tag != null)
                {
                    Rune rune = (Rune)item.Tag;
                    ShowRune(rune);
                }
            }
        }

        // Show the little preview window of the given rune
        private void ShowRune(Rune rune)
        {
            runeBox1.Show();
            runeBox1.SetRune(rune);
            /*
            runeBox2.Show();
            runeInventory.SetRune(rune);

            IRuneMain.Text = Rune.StringIt(rune.MainType, rune.MainValue);
            IRuneInnate.Text = Rune.StringIt(rune.InnateType, rune.InnateValue);
            IRuneSub1.Text = Rune.StringIt(rune.Sub1Type, rune.Sub1Value);
            IRuneSub2.Text = Rune.StringIt(rune.Sub2Type, rune.Sub2Value);
            IRuneSub3.Text = Rune.StringIt(rune.Sub3Type, rune.Sub3Value);
            IRuneSub4.Text = Rune.StringIt(rune.Sub4Type, rune.Sub4Value);
            IRuneMon.Text = rune.AssignedName;*/
        }

        // return the highlighted rune
        private void button3_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                returnedRune = (Rune)listView2.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        // cancel selecting a rune
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // return the selected rune
        private void listView2_DoubleClick(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                returnedRune = (Rune)listView2.SelectedItems[0].Tag;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        
        private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
            sorter.OnColumnClick(e.Column);
            ((ListView)sender).Sort();
        }

    }
}
