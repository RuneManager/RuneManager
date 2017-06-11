using System.Windows.Forms;
using RuneOptim;

namespace RuneApp
{
    public partial class RuneDisplay : Form
    {
        RuneControl[] runes;
        RuneBox[] runeBoxes;

        public RuneDisplay()
        {
            InitializeComponent();
            runes = new RuneControl[] { runeControl1, runeControl2, runeControl3, runeControl4, runeControl5, runeControl6 };
            runeBoxes = new RuneBox[] { runeBox1, runeBox2, runeBox3, runeBox4, runeBox5, runeBox6 };
        }
        
        public void UpdateLoad(Loadout load)
        {
            UpdateRunes(load.Runes);
            UpdateSets(load.Sets, !load.SetsFull);
        }

        public void UpdateRunes(Rune[] rs)
        {
            for (int i = 0; i < 6; i++)
            {
                runes[i].Hide();
            }

            foreach (var r in rs)
            {
                if (r != null)
                {
                    int i = r.Slot - 1;
                    runes[i].Show();
                    runes[i].SetRune(r);
                    runeBoxes[i].SetRune(r);
                }
            }
        }

        public void UpdateSets(RuneSet[] sets, bool broken)
        {
            if (sets.Length > 0)
                Set1Label.Text = sets[0] == RuneSet.Null ? "" : sets[0].ToString();
            if (sets.Length > 1)
                Set2Label.Text = sets[1] == RuneSet.Null ? "" : sets[1].ToString();
            if (sets.Length > 2)
                Set3Label.Text = sets[2] == RuneSet.Null ? "" : sets[2].ToString();

            if (broken)
            {
                if (sets[0] == RuneSet.Null)
                    Set1Label.Text = "Broken";
                else if (sets[1] == RuneSet.Null)
                    Set2Label.Text = "Broken";
                else if (sets[2] == RuneSet.Null)
                    Set3Label.Text = "Broken";
            }
        }
    }
}
