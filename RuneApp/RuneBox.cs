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
            lbMain.Text = Rune.StringIt(rune.MainType, rune.MainValue);
            lbInnate.Text = Rune.StringIt(rune.InnateType, rune.InnateValue);
            lb1.Text = Rune.StringIt(rune.Sub1Type, rune.Sub1Value);
            lb2.Text = Rune.StringIt(rune.Sub2Type, rune.Sub2Value);
            lb3.Text = Rune.StringIt(rune.Sub3Type, rune.Sub3Value);
            lb4.Text = Rune.StringIt(rune.Sub4Type, rune.Sub4Value);
            lbLevel.Text = rune.Level.ToString();
            lbMon.Text = "[" + rune.ID + "] " + rune.AssignedName;
            runeControl1.SetRune(rune);
        }
    }
}
