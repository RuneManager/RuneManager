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
			this.cShowWizard = new System.Windows.Forms.CheckBox();
			this.btnCreate = new System.Windows.Forms.Button();
			this.prebuildList = new System.Windows.Forms.ListView();
			this.lbPrebuild = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// runeDial1
			// 
			this.runeDial1.Loadout = null;
			this.runeDial1.Location = new System.Drawing.Point(675, 305);
			this.runeDial1.Name = "runeDial1";
			this.runeDial1.Size = new System.Drawing.Size(225, 175);
			this.runeDial1.TabIndex = 0;
			// 
			// cShowWizard
			// 
			this.cShowWizard.AutoSize = true;
			this.cShowWizard.Location = new System.Drawing.Point(12, 492);
			this.cShowWizard.Name = "cShowWizard";
			this.cShowWizard.Size = new System.Drawing.Size(89, 17);
			this.cShowWizard.TabIndex = 1;
			this.cShowWizard.Text = "Show Wizard";
			this.cShowWizard.UseVisualStyleBackColor = true;
			// 
			// btnCreate
			// 
			this.btnCreate.Location = new System.Drawing.Point(825, 486);
			this.btnCreate.Name = "btnCreate";
			this.btnCreate.Size = new System.Drawing.Size(75, 23);
			this.btnCreate.TabIndex = 2;
			this.btnCreate.Text = "Create";
			this.btnCreate.UseVisualStyleBackColor = true;
			this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
			// 
			// prebuildList
			// 
			this.prebuildList.Location = new System.Drawing.Point(12, 25);
			this.prebuildList.Name = "prebuildList";
			this.prebuildList.Size = new System.Drawing.Size(121, 422);
			this.prebuildList.TabIndex = 3;
			this.prebuildList.UseCompatibleStateImageBehavior = false;
			this.prebuildList.View = System.Windows.Forms.View.Details;
			// 
			// lbPrebuild
			// 
			this.lbPrebuild.AutoSize = true;
			this.lbPrebuild.Location = new System.Drawing.Point(12, 9);
			this.lbPrebuild.Name = "lbPrebuild";
			this.lbPrebuild.Size = new System.Drawing.Size(92, 13);
			this.lbPrebuild.TabIndex = 4;
			this.lbPrebuild.Text = "Prebuild Template";
			// 
			// BuildWizard
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(912, 521);
			this.Controls.Add(this.lbPrebuild);
			this.Controls.Add(this.prebuildList);
			this.Controls.Add(this.btnCreate);
			this.Controls.Add(this.cShowWizard);
			this.Controls.Add(this.runeDial1);
			this.Name = "BuildWizard";
			this.Text = "BuildWizard";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private RuneDial runeDial1;
		private System.Windows.Forms.CheckBox cShowWizard;
		private System.Windows.Forms.Button btnCreate;
		private System.Windows.Forms.ListView prebuildList;
		private System.Windows.Forms.Label lbPrebuild;
	}
}