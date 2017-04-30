using System.Windows.Forms;
using RuneOptim;

namespace RuneApp
{
    public partial class RuneBox : GroupBox
    {
        public RuneBox()
        {
            InitializeComponent();
        }

        public void SetRune(Rune rune)
        {
            lbMain.Text = Rune.StringIt(rune.Main.Type, rune.Main.Value);
            lbInnate.Text = Rune.StringIt(rune.Innate.Type, rune.Innate.Value);
            lb1.Text = Rune.StringIt(rune.Subs, 0);
            lb2.Text = Rune.StringIt(rune.Subs, 1);
            lb3.Text = Rune.StringIt(rune.Subs, 2);
            lb4.Text = Rune.StringIt(rune.Subs, 3);
            lbLevel.Text = rune.Level.ToString();
            lbMon.Text = "[" + rune.Id + "] " + rune.AssignedName;
            runeControl1.SetRune(rune);
        }
    }
}
