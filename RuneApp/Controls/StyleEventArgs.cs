using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RuneApp
{
	public class StyleEventArgs : EventArgs
	{
		public Color ForeColor = Program.Settings.ForeColour;
		public Color BackColor = Program.Settings.BackColour;
		public void ApplyToControl(Control lhs)
		{
			lhs.ForeColor = this.ForeColor;
			lhs.BackColor = this.BackColor;
			System.Windows.Forms.Application.EnableVisualStyles();
			foreach (Control c in lhs.Controls)
			{
				ApplyToControl(c);
				if (c is ListView)
				{
					var lv = c as ListView;
					lv.OwnerDraw = true;
					lv.DrawColumnHeader -= Lv_DrawColumnHeader;
					lv.DrawColumnHeader += Lv_DrawColumnHeader;
					lv.DrawItem -= Lv_DrawItem;
					lv.DrawItem += Lv_DrawItem;
					lv.DrawSubItem -= Lv_DrawSubItem;
					lv.DrawSubItem += Lv_DrawSubItem;
					foreach (ListViewItem lvi in lv.Items)
					{
						if (lv.ForeColor == SystemColors.ControlText)
							lv.ForeColor = this.ForeColor;
						if (lv.BackColor == SystemColors.Window)
							lv.BackColor = this.BackColor;
					}
				}
				else if (c is TabControl)
				{
					foreach (TabPage t in (c as TabControl).TabPages)
					{
						ApplyToControl(t);
					}
				}
				else if (c is ButtonBase)
				{
					(c as ButtonBase).UseVisualStyleBackColor = true;
				}
				else if (c is TabPage)
				{
					(c as TabPage).UseVisualStyleBackColor = true;
				}
			}
		}

		private void Lv_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			TextFormatFlags flags = TextFormatFlags.Left;

			using (StringFormat sf = new StringFormat())
			{
				// Store the column text alignment, letting it default
				// to Left if it has not been set to Center or Right.
				switch (e.Header.TextAlign)
				{
					case HorizontalAlignment.Center:
						sf.Alignment = StringAlignment.Center;
						flags = TextFormatFlags.HorizontalCenter;
						break;
					case HorizontalAlignment.Right:
						sf.Alignment = StringAlignment.Far;
						flags = TextFormatFlags.Right;
						break;
				}
				/*
				// Draw the text and background for a subitem with a 
				// negative value. 
				double subItemValue;
				if (e.ColumnIndex > 0 && Double.TryParse(
					e.SubItem.Text, NumberStyles.Currency,
					NumberFormatInfo.CurrentInfo, out subItemValue) &&
					subItemValue < 0)
				{
					// Unless the item is selected, draw the standard 
					// background to make it stand out from the gradient.
					if ((e.ItemState & ListViewItemStates.Selected) == 0)
					{
						e.DrawBackground();
					}

					// Draw the subitem text in red to highlight it. 
					e.Graphics.DrawString(e.SubItem.Text,
						(sender as ListView).Font, Brushes.Red, e.Bounds, sf);

					return;
				}
				*/
				// Draw normal text for a subitem with a nonnegative 
				// or nonnumerical value.
				e.DrawText(flags);
			}
		}

		private void Lv_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			if ((e.State & ListViewItemStates.Selected) != 0)
			{
				// Draw the background and focus rectangle for a selected item.
				e.Graphics.FillRectangle(Brushes.Maroon, e.Bounds);
				e.DrawFocusRectangle();
			}
			else
			{
				// Draw the background for an unselected item.
				using (LinearGradientBrush brush =
					new LinearGradientBrush(e.Bounds, Color.Orange,
					Color.Maroon, LinearGradientMode.Horizontal))
				{
					e.Graphics.FillRectangle(brush, e.Bounds);
				}
			}

			// Draw the item text for views other than the Details view.
			if ((sender as ListView).View != View.Details)
			{
				e.DrawText();
			}
		}

		public Color AddContrast(Color c, byte amount)
		{
			var r = c.R + (c.R > 128 ? -amount : amount);
			var g = c.G + (c.G > 128 ? -amount : amount);
			var b = c.B + (c.B > 128 ? -amount : amount);
			return Color.FromArgb(c.A, r, g, b);
		}

		private void Lv_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			using (StringFormat sf = new StringFormat())
			{
				e.Graphics.FillRectangle(new SolidBrush(Program.CurrentStyle().BackColor), e.Bounds);
				e.Graphics.DrawRectangle(new Pen(AddContrast(Program.CurrentStyle().BackColor, 50), 2), e.Bounds);
				switch (e.Header.TextAlign)
				{
					case HorizontalAlignment.Center:
						sf.Alignment = StringAlignment.Center;
						break;
					case HorizontalAlignment.Right:
						sf.Alignment = StringAlignment.Far;
						break;
				}
				sf.Trimming = StringTrimming.None;
				sf.FormatFlags = StringFormatFlags.FitBlackBox | StringFormatFlags.LineLimit;
				//using (Font headerFont = new Font("Helvetica", 10, FontStyle.Bold))
				{
					e.Graphics.DrawString(e.Header.Text, (sender as ListView).Font, new SolidBrush(Program.CurrentStyle().ForeColor), e.Bounds, sf);
				}
			}
		}
	}

}
