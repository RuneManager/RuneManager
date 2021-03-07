using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RuneOptim.Management;
using RuneOptim.swar;

namespace RuneApp {
    public partial class RuneDial : UserControl {
        bool isVertical = true;
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(true)]
        public bool IsVertical {
            get { return isVertical; }
            set { isVertical = value; checkLabels(); }
        }

        bool showSetIcons = true;
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(true)]
        public bool ShowSetIcons {
            get { return showSetIcons; }
            set { showSetIcons = value; refreshControls(); }
        }

        bool alwaysShowBases = false;
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false)]
        public bool AlwaysShowBases {
            get { return alwaysShowBases; }
            set { alwaysShowBases = value; refreshControls(); }
        }

        Loadout load = null;

        public Loadout Loadout {
            get { return load; }
            set { load = value; refreshControls(); LoadChanged?.Invoke(this, value); }
        }
        RuneControl[] runes;

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<RuneClickEventArgs> RuneClick;

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler DialDoubleClick;

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler<Loadout> LoadChanged;

        public int RuneSelected = 0;

        public RuneDial() {
            InitializeComponent();
            runes = new RuneControl[] { runeControl1, runeControl2, runeControl3, runeControl4, runeControl5, runeControl6 };
            checkLabels();
        }

        public void ResetRuneClicked() {
            RuneSelected = 0;
            foreach (RuneControl t in runes) {
                t.Gamma = 1;
                t.Refresh();
            }
        }

        private void rune_Click(object sender, EventArgs e) {
            ResetRuneClicked();

            RuneControl tc = ((RuneControl)sender);
            tc.Gamma = 1.4f;
            tc.Refresh();

            var ind = runes.ToList().IndexOf(tc) + 1;
            RuneSelected = ind;
            if (alwaysShowBases || (ind > 0 && tc.Tag != null)) {
                RuneClick?.Invoke(sender, new RuneClickEventArgs(ind, (Rune)tc.Tag));
            }
        }

        private void checkLabels() {
            if (isVertical) {
                Set1Label_v.Visible = true;
                Set2Label_v.Visible = true;
                Set3Label_v.Visible = true;
                Set1Label_h.Visible = false;
                Set2Label_h.Visible = false;
                Set3Label_h.Visible = false;

                Set1Label_v.ForeColor = Color.Black;
                Set2Label_v.ForeColor = Color.Black;
                Set3Label_v.ForeColor = Color.Black;
                Set1Label_h.ForeColor = Color.Gray;
                Set2Label_h.ForeColor = Color.Gray;
                Set3Label_h.ForeColor = Color.Gray;

                this.Width = 225;
            }
            else {
                Set1Label_v.Visible = false;
                Set2Label_v.Visible = false;
                Set3Label_v.Visible = false;
                Set1Label_h.Visible = true;
                Set2Label_h.Visible = true;
                Set3Label_h.Visible = true;

                Set1Label_v.ForeColor = Color.Gray;
                Set2Label_v.ForeColor = Color.Gray;
                Set3Label_v.ForeColor = Color.Gray;
                Set1Label_h.ForeColor = Color.Black;
                Set2Label_h.ForeColor = Color.Black;
                Set3Label_h.ForeColor = Color.Black;

                this.Width = 177;
            }
        }

        private void refreshControls() {
            if (load == null) {
                UpdateRunes();
                UpdateSets();
            }
            else {
                UpdateRunes(load.Runes);
                UpdateSets(load.Sets, !load.SetsFull);
            }
        }

        public void UpdateRunes(Rune[] rs = null) {
            for (int i = 0; i < 6; i++) {
                runes[i].SetImage = null;
                if (AlwaysShowBases)
                    runes[i].Show();
                else
                    runes[i].Hide();
            }

            if (rs != null) {
                foreach (var r in rs) {
                    if (r != null) {
                        int i = r.Slot - 1;
                        runes[i].Show();
                        runes[i].SetRune(r);
                        if (!ShowSetIcons)
                            runes[i].SetImage = null;
                    }
                }
            }
        }

        public void UpdateSets(RuneSet[] sets = null, bool broken = false) {
            Set1Label_v.Text = "";
            Set1Label_h.Text = "";
            Set2Label_v.Text = "";
            Set2Label_h.Text = "";
            Set3Label_v.Text = "";
            Set3Label_h.Text = "";
            if (sets == null)
                return;

            if (sets.Length > 0) {
                Set1Label_v.Text = sets[0] == RuneSet.Null ? "" : sets[0].ToString();
                Set1Label_h.Text = sets[0] == RuneSet.Null ? "" : sets[0].ToString();
            }
            if (sets.Length > 1) {
                Set2Label_v.Text = sets[1] == RuneSet.Null ? "" : sets[1].ToString();
                Set2Label_h.Text = sets[1] == RuneSet.Null ? "" : sets[1].ToString();
            }
            if (sets.Length > 2) {
                Set3Label_v.Text = sets[2] == RuneSet.Null ? "" : sets[2].ToString();
                Set3Label_h.Text = sets[2] == RuneSet.Null ? "" : sets[2].ToString();
            }

            if (broken) {
                if (sets[0] == RuneSet.Null) {
                    Set1Label_v.Text = "Broken";
                    Set1Label_h.Text = "Broken";
                }
                else if (sets[1] == RuneSet.Null) {
                    Set2Label_v.Text = "Broken";
                    Set2Label_h.Text = "Broken";
                }
                else if (sets[2] == RuneSet.Null) {
                    Set3Label_v.Text = "Broken";
                    Set3Label_h.Text = "Broken";
                }
            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e) {
            DialDoubleClick?.Invoke(sender, e);
        }
    }

    public class RuneClickEventArgs : EventArgs {
        public readonly Rune Rune;
        public readonly int Slot;

        public RuneClickEventArgs(int index, Rune r) : base() {
            this.Slot = index;
            this.Rune = r;
        }
    }

}
