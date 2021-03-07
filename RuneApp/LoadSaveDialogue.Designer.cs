namespace RuneApp
{
    partial class LoadSaveDialogue
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadSaveDialogue));
            this.radLookup = new System.Windows.Forms.RadioButton();
            this.radSwarfarm = new System.Windows.Forms.RadioButton();
            this.radSave = new System.Windows.Forms.RadioButton();
            this.lblFile = new System.Windows.Forms.Label();
            this.btnFile = new System.Windows.Forms.Button();
            this.cboxSwarfarm = new System.Windows.Forms.ComboBox();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radLookup
            // 
            this.radLookup.AutoSize = true;
            this.radLookup.Location = new System.Drawing.Point(44, 31);
            this.radLookup.Name = "radLookup";
            this.radLookup.Size = new System.Drawing.Size(14, 13);
            this.radLookup.TabIndex = 0;
            this.radLookup.TabStop = true;
            this.radLookup.UseVisualStyleBackColor = true;
            this.radLookup.CheckedChanged += new System.EventHandler(this.radLookup_CheckedChanged);
            // 
            // radSwarfarm
            // 
            this.radSwarfarm.AutoSize = true;
            this.radSwarfarm.Location = new System.Drawing.Point(44, 58);
            this.radSwarfarm.Name = "radSwarfarm";
            this.radSwarfarm.Size = new System.Drawing.Size(14, 13);
            this.radSwarfarm.TabIndex = 0;
            this.radSwarfarm.TabStop = true;
            this.radSwarfarm.UseVisualStyleBackColor = true;
            this.radSwarfarm.CheckedChanged += new System.EventHandler(this.radSwarfarm_CheckedChanged);
            // 
            // radSave
            // 
            this.radSave.AutoSize = true;
            this.radSave.Enabled = false;
            this.radSave.Location = new System.Drawing.Point(44, 82);
            this.radSave.Name = "radSave";
            this.radSave.Size = new System.Drawing.Size(70, 17);
            this.radSave.TabIndex = 0;
            this.radSave.TabStop = true;
            this.radSave.Text = "save.json";
            this.radSave.UseVisualStyleBackColor = true;
            this.radSave.CheckedChanged += new System.EventHandler(this.radSave_CheckedChanged);
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(150, 31);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(16, 13);
            this.lblFile.TabIndex = 1;
            this.lblFile.Text = "...";
            // 
            // btnFile
            // 
            this.btnFile.Location = new System.Drawing.Point(64, 26);
            this.btnFile.Name = "btnFile";
            this.btnFile.Size = new System.Drawing.Size(75, 23);
            this.btnFile.TabIndex = 2;
            this.btnFile.Text = "Open...";
            this.btnFile.UseVisualStyleBackColor = true;
            this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
            // 
            // cboxSwarfarm
            // 
            this.cboxSwarfarm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxSwarfarm.Enabled = false;
            this.cboxSwarfarm.FormattingEnabled = true;
            this.cboxSwarfarm.Location = new System.Drawing.Point(64, 55);
            this.cboxSwarfarm.Name = "cboxSwarfarm";
            this.cboxSwarfarm.Size = new System.Drawing.Size(182, 21);
            this.cboxSwarfarm.TabIndex = 3;
            this.cboxSwarfarm.SelectedIndexChanged += new System.EventHandler(this.cboxSwarfarm_SelectedIndexChanged);
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpen.Enabled = false;
            this.btnOpen.Location = new System.Drawing.Point(267, 113);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 4;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(348, 113);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // LoadSaveDialogue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 148);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.cboxSwarfarm);
            this.Controls.Add(this.btnFile);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.radSave);
            this.Controls.Add(this.radSwarfarm);
            this.Controls.Add(this.radLookup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoadSaveDialogue";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "LoadSaveDialogue";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radLookup;
        private System.Windows.Forms.RadioButton radSwarfarm;
        private System.Windows.Forms.RadioButton radSave;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.Button btnFile;
        private System.Windows.Forms.ComboBox cboxSwarfarm;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnCancel;
    }
}