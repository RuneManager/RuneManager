namespace RuneApp
{
	partial class BuildWizard
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.runeDial1 = new RuneApp.RuneDial();
			this.SuspendLayout();
			// 
			// runeDial1
			// 
			this.runeDial1.Location = new System.Drawing.Point(622, 313);
			this.runeDial1.Name = "runeDial1";
			this.runeDial1.ShownBuild = null;
			this.runeDial1.Size = new System.Drawing.Size(225, 175);
			this.runeDial1.TabIndex = 0;
			// 
			// BuildWizard
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(912, 521);
			this.Controls.Add(this.runeDial1);
			this.Name = "BuildWizard";
			this.Text = "BuildWizard";
			this.ResumeLayout(false);

		}

		#endregion

		private RuneDial runeDial1;
	}
}