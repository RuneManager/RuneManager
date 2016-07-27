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
    public partial class RuneSelect : Form
    {
        public IEnumerable<Rune> runes;

        public Rune returnedRune = null;

        public Build build = null;

        public string slot = "";

        public RuneSelect()
        {
            InitializeComponent();
            // when show, check if we have been given cool things
            Shown += RuneSelect_Shown;
        }

        void RuneSelect_Shown(object sender, EventArgs e)
        {
            // dump out the rune details
            foreach (Rune rune in runes.OrderBy(r => r.ID))
            {
                ListViewItem item = new ListViewItem(new string[]{
                    rune.Set.ToString(),
                    rune.ID.ToString(),
                    rune.Grade.ToString(),
                    Rune.StringIt(rune.MainType, true),
                    rune.MainValue.ToString()
                });
                if (build != null)
                    item.ForeColor = Color.Gray;
                if ((rune.AssignedName != "Unknown name" || rune.AssignedName == "Inventory") && !Main.useEquipped)
                    item.ForeColor = Color.Red;
                // stash the rune into the tag
                item.Tag = rune;
                listView2.Items.Add(item);
            }
            if (build != null)
            {
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
                foreach (Rune r in fr.OrderBy(r => r.ID))
                {
                    foreach (ListViewItem li in listView2.Items)
                    {
                        Rune rt = (Rune)li.Tag;
                        if (rt != null)
                        {
                            if (rt.ID < r.ID)
                                continue;

                            if (rt.ID == r.ID && (Main.useEquipped || (rt.AssignedName == "Unknown name" || rt.AssignedName == "Inventory") || rt.AssignedName == build.MonName))
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
            runeBox2.Show();
            runeInventory.SetRune(rune);

            IRuneMain.Text = Rune.StringIt(rune.MainType, rune.MainValue);
            IRuneInnate.Text = Rune.StringIt(rune.InnateType, rune.InnateValue);
            IRuneSub1.Text = Rune.StringIt(rune.Sub1Type, rune.Sub1Value);
            IRuneSub2.Text = Rune.StringIt(rune.Sub2Type, rune.Sub2Value);
            IRuneSub3.Text = Rune.StringIt(rune.Sub3Type, rune.Sub3Value);
            IRuneSub4.Text = Rune.StringIt(rune.Sub4Type, rune.Sub4Value);
            IRuneMon.Text = rune.AssignedName;
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

    }
}
