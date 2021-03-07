using System.Windows.Forms;
using RuneOptim.Management;
using RuneOptim.swar;

namespace RuneApp {
    public partial class RuneDisplay : Form {
        RuneBox[] runeBoxes;

        public RuneDisplay() {
            InitializeComponent();
            runeBoxes = new RuneBox[] { runeBox1, runeBox2, runeBox3, runeBox4, runeBox5, runeBox6 };
            foreach (var rb in runeBoxes) {
                rb.AllowGrind = false;
            }
        }

        public void UpdateLoad(Loadout load) {
            if (load != null) {
                UpdateRunes(load.Runes);
                UpdateSets(load.Sets, !load.SetsFull);
            }
            else {
                UpdateRunes();
                UpdateSets();
            }
            runeDial.Loadout = load;
        }

        public void UpdateRunes(Rune[] rs = null) {
            runeDial.UpdateRunes(rs);

            if (rs == null) {
                foreach (var rb in runeBoxes) {
                    rb.SetRune(null);
                }
            }
            else {
                foreach (var r in rs) {
                    if (r != null) {
                        int i = r.Slot - 1;
                        runeBoxes[i].SetRune(r);
                        runeBoxes[i].AllowGrind = false;
                    }
                }
            }
        }

        public void UpdateSets(RuneSet[] sets = null, bool broken = false) {
            runeDial.UpdateSets(sets, broken);
        }
    }
}
