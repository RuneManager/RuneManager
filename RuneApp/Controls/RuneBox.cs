using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RuneOptim;
using System.Linq;
using RuneOptim.swar;

namespace RuneApp {
    public partial class RuneBox : GroupBox {
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler OnClickHide;

        Rune rune = null;

        public RuneBox() {
            InitializeComponent();
            subs = new[] { lb1, lb2, lb3, lb4 };
            SetRune(null);
        }

        public ulong RuneId { get; set; }
        public bool AllowGrind {
            get {
                return btnGrind.Visible;
            }
            set {
                btnGrind.Visible = value;
            }
        }

        Label[] subs = null;

        public void SetRune(Rune rune) {
            if (rune == null) {

                lbMain.Text = "";
                lbInnate.Text = "";

                for (int i = 0; i < 4; i++) {
                    subs[i].Text = "";
                }

                lbLevel.Text = "";
                lbMon.Text = "";
                RuneId = 0;
                runeControl.Visible = false;
                btnGrind.Visible = false;
                this.rune = null;
                return;
            }


            lbMain.Text = Rune.StringIt(rune.Main.Type, rune.Main.Value);
            lbInnate.Text = Rune.StringIt(rune.Innate.Type, rune.Innate.Value);

            for (int i = 0; i < 4; i++) {
                subs[i].Text = Rune.StringIt(rune.Subs, i);
                if (i < rune.Subs.Count)
                    subs[i].ForeColor = (rune.Subs[i].GrindBonus > 0) ? Color.OrangeRed : Color.Black;
            }
            lbLevel.Text = rune.Level.ToString();
            lbMon.Text = "[" + rune.Id + "] " + rune.AssignedName;
            RuneId = rune.Id;
            runeControl.Visible = true;
            runeControl.SetRune(rune);
            btnGrind.Visible = true;
            this.rune = rune;
        }

        public void SetCraft(Craft craft) {
            lbMain.Text = craft.Set.ToString();
            RuneId = craft.ItemId;
            runeControl.SetCraft(craft);

            lbInnate.Text = craft.Type.ToString();
            lbLevel.Text = craft.Stat.ToString();

            lb1.Text = craft.Value.Min + " - " + craft.Value.Max + " " + craft.Stat.ToShortForm();
            if (craft.Stat != Attr.AttackFlat && craft.Stat != Attr.DefenseFlat && craft.Stat != Attr.HealthFlat && craft.Stat == Attr.Speed) {
                lb1.Text += "%";
            }
            lb2.Text = "";
            lb3.Text = "";
            lb4.Text = "";

            btnGrind.Visible = false;
        }

        private void lbClose_Click(object sender, System.EventArgs e) {
            this.Hide();
            OnClickHide?.Invoke(sender, e);
        }

        private void btnGrind_Click(object sender, EventArgs e) {
            var gr = rune.FilterGrinds(Program.data.Crafts);
            var en = rune.FilterEnchants(Program.data.Crafts);
            using (var f = new GrindPreview()) {
                f.Rune = this.rune;
                f.Crafts = gr.Concat(en);
                f.ShowDialog();
            }
        }
    }
}
