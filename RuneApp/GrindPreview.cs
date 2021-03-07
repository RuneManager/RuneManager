using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RuneOptim.swar;

namespace RuneApp {
    public partial class GrindPreview : Form {

        public IEnumerable<Craft> Crafts = null;
        public Rune Rune = null;
        int grindInd = 0;
        Craft selCraft = null;

        public GrindPreview() {
            InitializeComponent();
            var sorter = new ListViewSort();
            listRunes.ListViewItemSorter = sorter;
            this.runeBox1.AllowGrind = false;
            this.runeBox2.AllowGrind = false;
            cbSub.SelectedIndex = 0;
        }

        private void GrindPreview_Shown(object sender, EventArgs e) {

            this.runeBox1.SetRune(Rune);

            foreach (var craft in Crafts) {
                ListViewItem item = new ListViewItem(new string[]{
                    craft.Set.ToString(),
                    craft.Stat.ToString(),
                    craft.Rarity.ToString(),
                    craft.Type.ToString(),
                    craft.Value.Min.ToString() + "-" + craft.Value.Max.ToString(),
                });
                // stash the rune into the tag
                item.Tag = craft;


                listRunes.Items.Add(item);
            }
        }

        void Craftify() {
            if (selCraft != null) {
                this.runeBox2.SetCraft(selCraft);
                var r = Rune.Grind(selCraft, grindInd);
                this.runeBox3.SetRune(r);
            }
        }

        private void listRunes_SelectedIndexChanged(object sender, EventArgs e) {
            var fq = listRunes.SelectedItems.OfType<ListViewItem>().FirstOrDefault();
            selCraft = fq?.Tag as Craft;
            Craftify();
        }

        private void cbSub_SelectedIndexChanged(object sender, EventArgs e) {
            grindInd = cbSub.SelectedIndex;
            Craftify();
        }
    }
}
