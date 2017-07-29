using System;
using System.ComponentModel;
using System.Windows.Forms;
using RuneOptim;

namespace RuneApp {
	public partial class StatColumn : UserControl
	{
		bool editable = false;
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false)]
		public bool Editable
		{
			get { return editable; }
			set { editable = value; ShowExtras = Editable; refreshControls(); }
		}

		bool locked = false;
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false)]
		public bool Locked
		{
			get { return locked; }
			set { locked = value; refreshControls(); }
		}

		string text;
		[EditorBrowsable(EditorBrowsableState.Always)]
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Bindable(true)]
		[DefaultValue("Title")]
		public override string Text
		{
			get { return text; }
			set { text = value; this.lbTitle.Text = value; }
		}

		bool labels = false;
		[Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DefaultValue(false)]
		public bool IsLabel
		{
			get { return labels; }
			set
			{
				labels = value;
				foreach (var a in Build.statAll)
				{
					if (!labels)
					{
						ChangeLabel(a, 12345.67);
						ChangeTextBox(a, 12345.67);
					}
					else
					{
						ChangeLabel(a, null);
						ChangeTextBox(a, null);
					}
				}
			}
		}
		Stats stats = null;

		public bool ShowExtras = false;

		public Stats Stats
		{
			get { return stats; }
			set
			{
				loading = true;
				stats = value;
				if (stats != null)
					stats.OnStatChanged += Stats_OnStatChanged;
				foreach (var a in Build.statAll)
				{
					ChangeTextBox(a, stats?[a]);
					ChangeLabel(a, stats?[a]);
				}
				if (stats != null && !ShowExtras)
				{
					foreach (var a in Build.extraEnums)
					{
						ChangeLabel(a, stats.ExtraValue(a));
					}
				}
				loading = false;
			}
		}

		bool loading = true;

		public StatColumn()
		{
			InitializeComponent();
			refreshControls();
			this.lbTitle.Text = text;
		}

		private void StatColumn_Load(object sender, EventArgs e)
		{
			loading = false;
		}

		public void RecheckExtras()
		{
			if (stats != null && !ShowExtras)
			{
				foreach (var a in Build.extraEnums)
				{
					ChangeLabel(a, stats.ExtraValue(a));
				}
			}
		}

		private void Stats_OnStatChanged(object sender, StatModEventArgs e)
		{
			if (loading) return;
			ChangeTextBox(e.Attr, e.Value);
			ChangeLabel(e.Attr, e.Value);
		}

		public void ChangeTextBox(Attr a, double? v)
		{
			var astr = a.ToString();
			loading = true;
			foreach (Control c in Controls)
			{
				if (!(c is TextBox)) continue;
				if (c.Tag != null && c.Tag.ToString() == astr)
				{
					if (v != null)
						c.Text = v.ToString();
					else
						c.Text = c.Name.Replace("tb", "");
				}
			}
			loading = false;
		}

		public void ChangeLabel(Attr a, double? v)
		{
			var astr = a.ToString();
			foreach (Control c in Controls)
			{
				if (!(c is Label)) continue;
				if (c.Tag != null && c.Tag.ToString() == astr)
				{
					if (v != null)
						c.Text = v.ToString();
					else
						c.Text = c.Name.Replace("lb", "");
				}
			}
		}

		private void refreshControls()
		{
			foreach (Control c in Controls)
			{
				if (c is Label)
				{
					c.Visible = !editable;
				}
				else if (c is TextBox)
				{
					c.Visible = editable;
					c.Enabled = !locked;
				}
			}
			lbTitle.Show();
		}

		private void tb_TextChanged(object sender, EventArgs e)
		{
			if (loading) return;

			TextBox tb = sender as TextBox;
			if (tb != null && stats != null)
			{
				var tag = tb.Tag;
				if (tag != null)
				{
					Attr attr;
					double v;
					if (Enum.TryParse(tag.ToString(), out attr) && double.TryParse(tb.Text, out v))
					{
						loading = true;
						stats[attr] = v;
						loading = false;
					}
				}
				else
				{
					// TODO: skills
				}
			}
		}
	}
}
