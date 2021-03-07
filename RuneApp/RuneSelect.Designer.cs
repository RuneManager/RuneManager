namespace RuneApp
{
    partial class RuneSelect
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
            this.listRunes = new System.Windows.Forms.ListView();
            this.runesSet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesGrade = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesMType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesMValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesPoints = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runeStats = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.runeBox1 = new RuneApp.RuneBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listRunes
            // 
            this.listRunes.AllowColumnReorder = true;
            this.listRunes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listRunes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.runesSet,
            this.runesID,
            this.runesGrade,
            this.runesMType,
            this.runesMValue,
            this.runesPoints,
            this.runeStats});
            this.listRunes.FullRowSelect = true;
            this.listRunes.Location = new System.Drawing.Point(11, 27);
            this.listRunes.Margin = new System.Windows.Forms.Padding(2);
            this.listRunes.MultiSelect = false;
            this.listRunes.Name = "listRunes";
            this.listRunes.Size = new System.Drawing.Size(385, 542);
            this.listRunes.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listRunes.TabIndex = 1;
            this.listRunes.UseCompatibleStateImageBehavior = false;
            this.listRunes.View = System.Windows.Forms.View.Details;
            this.listRunes.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView2_ColumnClick);
            this.listRunes.SelectedIndexChanged += new System.EventHandler(this.listView2_SelectedIndexChanged);
            this.listRunes.DoubleClick += new System.EventHandler(this.listView2_DoubleClick);
            // 
            // runesSet
            // 
            this.runesSet.DisplayIndex = 2;
            this.runesSet.Text = "Set";
            this.runesSet.Width = 100;
            // 
            // runesID
            // 
            this.runesID.Text = "ID";
            this.runesID.Width = 40;
            // 
            // runesGrade
            // 
            this.runesGrade.DisplayIndex = 0;
            this.runesGrade.Text = "Grade";
            this.runesGrade.Width = 40;
            // 
            // runesMType
            // 
            this.runesMType.DisplayIndex = 4;
            this.runesMType.Text = "Main.Type";
            this.runesMType.Width = 80;
            // 
            // runesMValue
            // 
            this.runesMValue.DisplayIndex = 5;
            this.runesMValue.Text = "Main.Value";
            // 
            // runesPoints
            // 
            this.runesPoints.DisplayIndex = 3;
            this.runesPoints.Text = "Score";
            this.runesPoints.Width = 0;
            // 
            // runeStats
            // 
            this.runeStats.Text = "Custom";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(628, 25);
            this.toolStrip1.TabIndex = 12;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = global::RuneApp.App.refresh;
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "toolStripButton3";
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Location = new System.Drawing.Point(491, 545);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(2);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(61, 24);
            this.btnSelect.TabIndex = 19;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(556, 545);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(61, 24);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.button1_Click);
            // 
            // runeBox1
            // 
            this.runeBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeBox1.Location = new System.Drawing.Point(401, 28);
            this.runeBox1.Name = "runeBox1";
            this.runeBox1.Size = new System.Drawing.Size(215, 186);
            this.runeBox1.TabIndex = 21;
            this.runeBox1.TabStop = false;
            this.runeBox1.Text = "Rune";
            // 
            // RuneSelect
            // 
            this.AcceptButton = this.btnSelect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(628, 580);
            this.Controls.Add(this.runeBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listRunes);
            this.Icon = global::RuneApp.App.Icon;
            this.Name = "RuneSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RuneSelect";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listRunes;
        private System.Windows.Forms.ColumnHeader runesSet;
        private System.Windows.Forms.ColumnHeader runesID;
        private System.Windows.Forms.ColumnHeader runesGrade;
        private System.Windows.Forms.ColumnHeader runesMType;
        private System.Windows.Forms.ColumnHeader runesMValue;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnCancel;
        private RuneBox runeBox1;
        private System.Windows.Forms.ColumnHeader runesPoints;
        private System.Windows.Forms.ColumnHeader runeStats;
    }
}