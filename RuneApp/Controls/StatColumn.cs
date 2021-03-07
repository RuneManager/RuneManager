using System;
using System.ComponentModel;
using System.Windows.Forms;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp {
    public partial class StatColumn : UserControl {
        bool editable = false;
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false)]
        public bool Editable {
            get { return editable; }
            set { editable = value; ShowExtras = Editable; refreshControls(); }
        }

        bool locked = false;
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false)]
        public bool Locked {
            get { return locked; }
            set { locked = value; refreshControls(); }
        }

        string text;
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        [DefaultValue("Title")]
        public override string Text {
            get { return text; }
            set { text = value; this.lbTitle.Text = value; }
        }

        bool labels = false;
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false)]
        public bool IsLabel {
            get { return labels; }
            set {
                labels = value;
                foreach (var a in Build.StatAll) {
                    if (!labels) {
                        ChangeLabel(a, 12345.67);
                        ChangeTextBox(a, 12345.67);
                    }
                    else {
                        ChangeLabel(a, null);
                        ChangeTextBox(a, null);
                    }
                }
            }
        }
        Stats stats = null;

        public bool ShowExtras = false;

        public Stats Stats {
            get { return stats; }
            set {
                loading = true;
                stats = value;
                if (stats != null) {
                    stats.OnStatChanged += Stats_OnStatChanged;
                }
                foreach (var a in Build.StatAll) {
                    ChangeTextBox(a, stats?[a]);
                    ChangeLabel(a, stats?[a]);
                }
                foreach (var a in new Attr[] { Attr.Skill1, Attr.Skill2, Attr.Skill3, Attr.Skill4 }) {
                    ChangeTextBox(a, stats?.DamageSkillups[a - Attr.Skill1]);
                    //ChangeLabel(a, stats?.DamageSkillups[(int)a - 22]);
                    ChangeLabel(a, stats?.GetSkillDamage(Attr.AverageDamage, a - Attr.Skill1, stats));
                }
                if (stats != null && !ShowExtras) {
                    foreach (var a in Build.ExtraEnums) {
                        ChangeLabel(a, stats.ExtraValue(a));
                    }
                }
                loading = false;
            }
        }

        bool loading = true;

        public StatColumn() {
            InitializeComponent();
            refreshControls();
            this.lbTitle.Text = text;
        }

        private void StatColumn_Load(object sender, EventArgs e) {
            loading = false;
        }

        public void RecheckExtras() {
            if (stats != null && !ShowExtras) {
                foreach (var a in Build.ExtraEnums) {
                    ChangeLabel(a, stats.ExtraValue(a));
                }
            }
        }

        private void Stats_OnStatChanged(object sender, StatModEventArgs e) {
            if (loading) return;
            ChangeTextBox(e.Attr, e.Value);
            ChangeLabel(e.Attr, e.Value);
        }

        public void ChangeTextBox(Attr a, double? v) {
            var astr = a.ToString();
            loading = true;
            foreach (Control c in Controls) {
                if (!(c is TextBox)) continue;
                if (c.Tag != null && c.Tag.ToString() == astr) {
                    if (v != null)
                        c.Text = v != 0 ? v.ToString() : "";
                    else
                        c.Text = c.Name.Replace("tb", "");
                }
            }
            loading = false;
        }

        public void ChangeLabel(Attr a, double? v) {
            var astr = a.ToString();
            foreach (Control c in Controls) {
                if (!(c is Label)) continue;
                if (c.Tag != null && c.Tag.ToString() == astr) {
                    if (v != null)
                        c.Text = v != 0 ? v.ToString() : "";
                    else
                        c.Text = c.Name.Replace("lb", "");
                }
            }
        }

        private void refreshControls() {
            foreach (Control c in Controls) {
                if (c is Label) {
                    c.Visible = !editable;
                }
                else if (c is TextBox) {
                    c.Visible = editable;
                    c.Enabled = !locked;
                }
            }
            lbTitle.Show();
        }

        private void tb_TextChanged(object sender, EventArgs e) {
            if (loading) return;

            TextBox tb = sender as TextBox;
            if (tb != null && stats != null) {
                var tag = tb.Tag;
                Attr attr;
                double v;
                if (tag != null && Enum.TryParse(tag.ToString(), out attr) && (double.TryParse(tb.Text, out v) || string.IsNullOrEmpty(tb.Text))) {
                    loading = true;
                    if (attr >= Attr.Skill1) {
                        //stats.DamageSkillups[(attr - Attr.Skill1)] = v;
                        stats.DamageSkillupsSet(attr - Attr.Skill1, v);
                    }
                    else {
                        stats[attr] = v;
                    }
                    loading = false;
                }
            }
        }

        public void SetSkills(int num) {
            for (int i = 0; i < 4; i++) {
                var astr = (Attr.Skill1 + i).ToString();
                loading = true;
                foreach (Control c in Controls) {
                    if ((c.Tag as string) != astr) continue;
                    c.Visible = i < num && ((c is TextBox && editable) || (c is Label && !editable));
                }
            }
        }
    }
}
