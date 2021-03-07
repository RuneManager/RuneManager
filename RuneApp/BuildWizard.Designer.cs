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
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Default", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("Custom", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup6 = new System.Windows.Forms.ListViewGroup("Online", System.Windows.Forms.HorizontalAlignment.Left);
            this.cShowWizard = new System.Windows.Forms.CheckBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.prebuildList = new System.Windows.Forms.ListView();
            this.lhTemplate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lbPrebuild = new System.Windows.Forms.Label();
            this.cPreviewLocked = new System.Windows.Forms.CheckBox();
            this.runeBox = new RuneApp.RuneBox();
            this.statScore = new RuneApp.StatColumn();
            this.statTotal = new RuneApp.StatColumn();
            this.statPreview = new RuneApp.StatColumn();
            this.statCurrent = new RuneApp.StatColumn();
            this.statBase = new RuneApp.StatColumn();
            this.statColLabel = new RuneApp.StatColumn();
            this.runeDial = new RuneApp.RuneDial();
            this.statGoal = new RuneApp.StatColumn();
            this.SuspendLayout();
            // 
            // cShowWizard
            // 
            this.cShowWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cShowWizard.AutoSize = true;
            this.cShowWizard.Enabled = false;
            this.cShowWizard.Location = new System.Drawing.Point(12, 447);
            this.cShowWizard.Name = "cShowWizard";
            this.cShowWizard.Size = new System.Drawing.Size(89, 17);
            this.cShowWizard.TabIndex = 1;
            this.cShowWizard.Text = "Show Wizard";
            this.cShowWizard.UseVisualStyleBackColor = true;
            this.cShowWizard.CheckedChanged += new System.EventHandler(this.cShowWizard_CheckedChanged);
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreate.Location = new System.Drawing.Point(959, 441);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 2;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // prebuildList
            // 
            this.prebuildList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.prebuildList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lhTemplate});
            this.prebuildList.FullRowSelect = true;
            listViewGroup4.Header = "Default";
            listViewGroup4.Name = "lvgDefault";
            listViewGroup5.Header = "Custom";
            listViewGroup5.Name = "lvgCustom";
            listViewGroup6.Header = "Online";
            listViewGroup6.Name = "lvgOnline";
            this.prebuildList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup4,
            listViewGroup5,
            listViewGroup6});
            this.prebuildList.Location = new System.Drawing.Point(12, 25);
            this.prebuildList.MultiSelect = false;
            this.prebuildList.Name = "prebuildList";
            this.prebuildList.Size = new System.Drawing.Size(121, 410);
            this.prebuildList.TabIndex = 3;
            this.prebuildList.UseCompatibleStateImageBehavior = false;
            this.prebuildList.View = System.Windows.Forms.View.Details;
            this.prebuildList.SelectedIndexChanged += new System.EventHandler(this.prebuildList_SelectedIndexChanged);
            // 
            // lhTemplate
            // 
            this.lhTemplate.Text = "Template";
            this.lhTemplate.Width = 100;
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
            // cPreviewLocked
            // 
            this.cPreviewLocked.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cPreviewLocked.AutoSize = true;
            this.cPreviewLocked.Location = new System.Drawing.Point(809, 237);
            this.cPreviewLocked.Name = "cPreviewLocked";
            this.cPreviewLocked.Size = new System.Drawing.Size(135, 17);
            this.cPreviewLocked.TabIndex = 8;
            this.cPreviewLocked.Text = "Use Locked in preview";
            this.cPreviewLocked.UseVisualStyleBackColor = true;
            this.cPreviewLocked.CheckedChanged += new System.EventHandler(this.cPreviewLocked_CheckedChanged);
            // 
            // runeBox
            // 
            this.runeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeBox.Location = new System.Drawing.Point(777, 12);
            this.runeBox.Name = "runeBox";
            this.runeBox.RuneId = ((ulong)(0ul));
            this.runeBox.Size = new System.Drawing.Size(257, 186);
            this.runeBox.TabIndex = 9;
            this.runeBox.TabStop = false;
            this.runeBox.Text = "runeBox1";
            this.runeBox.Visible = false;
            // 
            // statScore
            // 
            this.statScore.Editable = true;
            this.statScore.Location = new System.Drawing.Point(600, 12);
            this.statScore.Name = "statScore";
            this.statScore.Size = new System.Drawing.Size(69, 489);
            this.statScore.Stats = null;
            this.statScore.TabIndex = 7;
            this.statScore.Text = "Scoring";
            // 
            // statTotal
            // 
            this.statTotal.Location = new System.Drawing.Point(675, 12);
            this.statTotal.Name = "statTotal";
            this.statTotal.Size = new System.Drawing.Size(69, 489);
            this.statTotal.Stats = null;
            this.statTotal.TabIndex = 6;
            this.statTotal.Text = "Points";
            // 
            // statPreview
            // 
            this.statPreview.Location = new System.Drawing.Point(525, 12);
            this.statPreview.Name = "statPreview";
            this.statPreview.Size = new System.Drawing.Size(69, 489);
            this.statPreview.Stats = null;
            this.statPreview.TabIndex = 6;
            this.statPreview.Text = "Preview";
            // 
            // statCurrent
            // 
            this.statCurrent.Location = new System.Drawing.Point(375, 12);
            this.statCurrent.Name = "statCurrent";
            this.statCurrent.Size = new System.Drawing.Size(69, 489);
            this.statCurrent.Stats = null;
            this.statCurrent.TabIndex = 6;
            this.statCurrent.Text = "Current";
            // 
            // statBase
            // 
            this.statBase.Location = new System.Drawing.Point(300, 12);
            this.statBase.Name = "statBase";
            this.statBase.Size = new System.Drawing.Size(69, 489);
            this.statBase.Stats = null;
            this.statBase.TabIndex = 6;
            this.statBase.Text = "Base";
            // 
            // statColLabel
            // 
            this.statColLabel.IsLabel = true;
            this.statColLabel.Location = new System.Drawing.Point(225, 12);
            this.statColLabel.Name = "statColLabel";
            this.statColLabel.Size = new System.Drawing.Size(69, 489);
            this.statColLabel.Stats = null;
            this.statColLabel.TabIndex = 5;
            this.statColLabel.Text = "Attr";
            // 
            // runeDial
            // 
            this.runeDial.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.runeDial.Loadout = null;
            this.runeDial.Location = new System.Drawing.Point(809, 260);
            this.runeDial.Name = "runeDial";
            this.runeDial.Size = new System.Drawing.Size(225, 175);
            this.runeDial.TabIndex = 0;
            this.runeDial.RuneClick += new System.EventHandler<RuneApp.RuneClickEventArgs>(this.runeDial_RuneClick);
            // 
            // statGoal
            // 
            this.statGoal.Editable = true;
            this.statGoal.Location = new System.Drawing.Point(450, 12);
            this.statGoal.Name = "statGoal";
            this.statGoal.Size = new System.Drawing.Size(69, 489);
            this.statGoal.Stats = null;
            this.statGoal.TabIndex = 6;
            this.statGoal.Text = "Goal";
            // 
            // BuildWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1046, 476);
            this.Controls.Add(this.runeBox);
            this.Controls.Add(this.cPreviewLocked);
            this.Controls.Add(this.statScore);
            this.Controls.Add(this.statTotal);
            this.Controls.Add(this.statPreview);
            this.Controls.Add(this.statGoal);
            this.Controls.Add(this.statCurrent);
            this.Controls.Add(this.statBase);
            this.Controls.Add(this.statColLabel);
            this.Controls.Add(this.lbPrebuild);
            this.Controls.Add(this.prebuildList);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.cShowWizard);
            this.Controls.Add(this.runeDial);
            this.Icon = global::RuneApp.App.Icon;
            this.Name = "BuildWizard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BuildWizard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BuildWizard_FormClosing);
            this.Load += new System.EventHandler(this.BuildWizard_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RuneDial runeDial;
        private System.Windows.Forms.CheckBox cShowWizard;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.ListView prebuildList;
        private System.Windows.Forms.Label lbPrebuild;
        private System.Windows.Forms.ColumnHeader lhTemplate;
        private StatColumn statColLabel;
        private StatColumn statBase;
        private StatColumn statScore;
        private StatColumn statTotal;
        private StatColumn statCurrent;
        private StatColumn statPreview;
        private System.Windows.Forms.CheckBox cPreviewLocked;
        private RuneBox runeBox;
        private StatColumn statGoal;
    }
}