using System.Windows.Forms;
using RuneOptim;

namespace RuneApp
{
	public partial class RuneDisplay : Form
	{
		RuneBox[] runeBoxes;

		public RuneDisplay()
		{
			InitializeComponent();
			runeBoxes = new RuneBox[] { runeBox1, runeBox2, runeBox3, runeBox4, runeBox5, runeBox6 };
		}
		
		public void UpdateLoad(Loadout load)
		{
			UpdateRunes(load.Runes);
			UpdateSets(load.Sets, !load.SetsFull);
			runeDial.Loadout = load;
		}

		public void UpdateRunes(Rune[] rs)
		{
			runeDial.UpdateRunes(rs);

			foreach (var r in rs)
			{
				if (r != null)
				{
					int i = r.Slot - 1;
					runeBoxes[i].SetRune(r);
				}
			}
		}

		public void UpdateSets(RuneSet[] sets, bool broken)
		{
			runeDial.UpdateSets(sets, broken);
		}
	}
}
