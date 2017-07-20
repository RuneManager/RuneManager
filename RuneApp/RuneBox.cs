using System;
using System.ComponentModel;
using System.Windows.Forms;
using RuneOptim;

namespace RuneApp
{
	public partial class RuneBox : GroupBox
	{
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
		public event EventHandler OnClickHide;

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
			runeControl.SetRune(rune);
		}

		public void SetCraft(Craft craft)
		{
			lbMain.Text = craft.Set.ToString() + " " + craft.Stat + " " + craft.Type.ToString();
			runeControl.SetCraft(craft);
		}

		private void lbClose_Click(object sender, System.EventArgs e)
		{
			this.Hide();
			OnClickHide?.Invoke(sender, e);
		}
	}
}
