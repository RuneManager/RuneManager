namespace RuneApp
{
    partial class Generate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Generate));
            this.loadoutList = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.runeBuild = new System.Windows.Forms.GroupBox();
            this.SRuneLevel = new System.Windows.Forms.Label();
            this.SRuneMon = new System.Windows.Forms.Label();
            this.hideBuildRune = new System.Windows.Forms.Label();
            this.runeShown = new RuneApp.RuneControl();
            this.SRuneSub4 = new System.Windows.Forms.Label();
            this.SRuneSub3 = new System.Windows.Forms.Label();
            this.SRuneSub2 = new System.Windows.Forms.Label();
            this.SRuneSub1 = new System.Windows.Forms.Label();
            this.SRuneInnate = new System.Windows.Forms.Label();
            this.SRuneMain = new System.Windows.Forms.Label();
            this.runeControl6 = new RuneApp.RuneControl();
            this.runeControl5 = new RuneApp.RuneControl();
            this.runeControl4 = new RuneApp.RuneControl();
            this.runeControl3 = new RuneApp.RuneControl();
            this.runeControl2 = new RuneApp.RuneControl();
            this.runeControl1 = new RuneApp.RuneControl();
            this.Set3Label = new System.Windows.Forms.Label();
            this.Set2Label = new System.Windows.Forms.Label();
            this.Set1Label = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1.SuspendLayout();
            this.runeBuild.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // loadoutList
            // 
            this.loadoutList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadoutList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.loadoutList.FullRowSelect = true;
            this.loadoutList.Location = new System.Drawing.Point(12, 12);
            this.loadoutList.Name = "loadoutList";
            this.loadoutList.Size = new System.Drawing.Size(718, 693);
            this.loadoutList.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.loadoutList.TabIndex = 0;
            this.loadoutList.UseCompatibleStateImageBehavior = false;
            this.loadoutList.View = System.Windows.Forms.View.Details;
            this.loadoutList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.loadoutList.SelectedIndexChanged += new System.EventHandler(this.loadoutList_SelectedIndexChanged);
            this.loadoutList.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Points";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.runeBuild);
            this.groupBox1.Controls.Add(this.runeControl6);
            this.groupBox1.Controls.Add(this.runeControl5);
            this.groupBox1.Controls.Add(this.runeControl4);
            this.groupBox1.Controls.Add(this.runeControl3);
            this.groupBox1.Controls.Add(this.runeControl2);
            this.groupBox1.Controls.Add(this.runeControl1);
            this.groupBox1.Controls.Add(this.Set3Label);
            this.groupBox1.Controls.Add(this.Set2Label);
            this.groupBox1.Controls.Add(this.Set1Label);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.btnHelp);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(736, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 693);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Scoring";
            // 
            // runeBuild
            // 
            this.runeBuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeBuild.Controls.Add(this.SRuneLevel);
            this.runeBuild.Controls.Add(this.SRuneMon);
            this.runeBuild.Controls.Add(this.hideBuildRune);
            this.runeBuild.Controls.Add(this.runeShown);
            this.runeBuild.Controls.Add(this.SRuneSub4);
            this.runeBuild.Controls.Add(this.SRuneSub3);
            this.runeBuild.Controls.Add(this.SRuneSub2);
            this.runeBuild.Controls.Add(this.SRuneSub1);
            this.runeBuild.Controls.Add(this.SRuneInnate);
            this.runeBuild.Controls.Add(this.SRuneMain);
            this.runeBuild.Location = new System.Drawing.Point(5, 480);
            this.runeBuild.Margin = new System.Windows.Forms.Padding(2);
            this.runeBuild.Name = "runeBuild";
            this.runeBuild.Padding = new System.Windows.Forms.Padding(2);
            this.runeBuild.Size = new System.Drawing.Size(217, 179);
            this.runeBuild.TabIndex = 100;
            this.runeBuild.TabStop = false;
            this.runeBuild.Text = "Rune";
            this.runeBuild.Visible = false;
            // 
            // SRuneLevel
            // 
            this.SRuneLevel.AutoSize = true;
            this.SRuneLevel.Location = new System.Drawing.Point(69, 52);
            this.SRuneLevel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneLevel.Name = "SRuneLevel";
            this.SRuneLevel.Size = new System.Drawing.Size(32, 13);
            this.SRuneLevel.TabIndex = 20;
            this.SRuneLevel.Text = "Sub1";
            // 
            // SRuneMon
            // 
            this.SRuneMon.AutoSize = true;
            this.SRuneMon.Location = new System.Drawing.Point(4, 159);
            this.SRuneMon.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneMon.Name = "SRuneMon";
            this.SRuneMon.Size = new System.Drawing.Size(52, 13);
            this.SRuneMon.TabIndex = 19;
            this.SRuneMon.Text = "Equipped";
            // 
            // hideBuildRune
            // 
            this.hideBuildRune.AutoSize = true;
            this.hideBuildRune.Location = new System.Drawing.Point(199, 15);
            this.hideBuildRune.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hideBuildRune.Name = "hideBuildRune";
            this.hideBuildRune.Size = new System.Drawing.Size(14, 13);
            this.hideBuildRune.TabIndex = 10;
            this.hideBuildRune.Text = "X";
            this.hideBuildRune.Click += new System.EventHandler(this.hideRuneBox);
            // 
            // runeShown
            // 
            this.runeShown.BackColor = System.Drawing.Color.Transparent;
            this.runeShown.BackImage = global::RuneApp.Runes.bg_normal;
            this.runeShown.Coolness = 0;
            this.runeShown.Gamma = 1F;
            this.runeShown.Grade = 2;
            this.runeShown.Location = new System.Drawing.Point(4, 17);
            this.runeShown.Margin = new System.Windows.Forms.Padding(2);
            this.runeShown.Name = "runeShown";
            this.runeShown.SetImage = global::RuneApp.Runes.despair;
            this.runeShown.ShowBack = true;
            this.runeShown.ShowStars = true;
            this.runeShown.Size = new System.Drawing.Size(58, 58);
            this.runeShown.SlotImage = global::RuneApp.Runes.rune2;
            this.runeShown.StarImage = global::RuneApp.Runes.star_unawakened;
            this.runeShown.TabIndex = 11;
            this.runeShown.Text = "runeControl2";
            // 
            // SRuneSub4
            // 
            this.SRuneSub4.AutoSize = true;
            this.SRuneSub4.Location = new System.Drawing.Point(4, 119);
            this.SRuneSub4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneSub4.Name = "SRuneSub4";
            this.SRuneSub4.Size = new System.Drawing.Size(32, 13);
            this.SRuneSub4.TabIndex = 17;
            this.SRuneSub4.Text = "Sub4";
            // 
            // SRuneSub3
            // 
            this.SRuneSub3.AutoSize = true;
            this.SRuneSub3.Location = new System.Drawing.Point(4, 105);
            this.SRuneSub3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneSub3.Name = "SRuneSub3";
            this.SRuneSub3.Size = new System.Drawing.Size(32, 13);
            this.SRuneSub3.TabIndex = 16;
            this.SRuneSub3.Text = "Sub3";
            // 
            // SRuneSub2
            // 
            this.SRuneSub2.AutoSize = true;
            this.SRuneSub2.Location = new System.Drawing.Point(4, 91);
            this.SRuneSub2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneSub2.Name = "SRuneSub2";
            this.SRuneSub2.Size = new System.Drawing.Size(32, 13);
            this.SRuneSub2.TabIndex = 15;
            this.SRuneSub2.Text = "Sub2";
            // 
            // SRuneSub1
            // 
            this.SRuneSub1.AutoSize = true;
            this.SRuneSub1.Location = new System.Drawing.Point(4, 77);
            this.SRuneSub1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneSub1.Name = "SRuneSub1";
            this.SRuneSub1.Size = new System.Drawing.Size(32, 13);
            this.SRuneSub1.TabIndex = 14;
            this.SRuneSub1.Text = "Sub1";
            // 
            // SRuneInnate
            // 
            this.SRuneInnate.AutoSize = true;
            this.SRuneInnate.Location = new System.Drawing.Point(69, 37);
            this.SRuneInnate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneInnate.Name = "SRuneInnate";
            this.SRuneInnate.Size = new System.Drawing.Size(37, 13);
            this.SRuneInnate.TabIndex = 13;
            this.SRuneInnate.Text = "Innate";
            // 
            // SRuneMain
            // 
            this.SRuneMain.AutoSize = true;
            this.SRuneMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SRuneMain.Location = new System.Drawing.Point(69, 17);
            this.SRuneMain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SRuneMain.Name = "SRuneMain";
            this.SRuneMain.Size = new System.Drawing.Size(40, 18);
            this.SRuneMain.TabIndex = 12;
            this.SRuneMain.Text = "Main";
            // 
            // runeControl6
            // 
            this.runeControl6.BackColor = System.Drawing.Color.Transparent;
            this.runeControl6.BackImage = null;
            this.runeControl6.Coolness = 0;
            this.runeControl6.Gamma = 1F;
            this.runeControl6.Grade = 1;
            this.runeControl6.Location = new System.Drawing.Point(8, 360);
            this.runeControl6.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl6.Name = "runeControl6";
            this.runeControl6.SetImage = ((System.Drawing.Image)(resources.GetObject("runeControl6.SetImage")));
            this.runeControl6.ShowBack = false;
            this.runeControl6.ShowStars = false;
            this.runeControl6.Size = new System.Drawing.Size(56, 41);
            this.runeControl6.SlotImage = global::RuneApp.Runes.rune6;
            this.runeControl6.StarImage = null;
            this.runeControl6.TabIndex = 99;
            this.runeControl6.Text = "runeControl6";
            this.runeControl6.Visible = false;
            this.runeControl6.Click += new System.EventHandler(this.rune_Click);
            // 
            // runeControl5
            // 
            this.runeControl5.BackColor = System.Drawing.Color.Transparent;
            this.runeControl5.BackImage = null;
            this.runeControl5.Coolness = 0;
            this.runeControl5.Gamma = 1F;
            this.runeControl5.Grade = 1;
            this.runeControl5.Location = new System.Drawing.Point(9, 396);
            this.runeControl5.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl5.Name = "runeControl5";
            this.runeControl5.SetImage = global::RuneApp.Runes.revenge;
            this.runeControl5.ShowBack = false;
            this.runeControl5.ShowStars = false;
            this.runeControl5.Size = new System.Drawing.Size(54, 42);
            this.runeControl5.SlotImage = global::RuneApp.Runes.rune5;
            this.runeControl5.StarImage = null;
            this.runeControl5.TabIndex = 98;
            this.runeControl5.Text = "runeControl5";
            this.runeControl5.Visible = false;
            this.runeControl5.Click += new System.EventHandler(this.rune_Click);
            // 
            // runeControl4
            // 
            this.runeControl4.BackColor = System.Drawing.Color.Transparent;
            this.runeControl4.BackImage = null;
            this.runeControl4.Coolness = 0;
            this.runeControl4.Gamma = 1F;
            this.runeControl4.Grade = 1;
            this.runeControl4.Location = new System.Drawing.Point(51, 413);
            this.runeControl4.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl4.Name = "runeControl4";
            this.runeControl4.SetImage = global::RuneApp.Runes.revenge;
            this.runeControl4.ShowBack = false;
            this.runeControl4.ShowStars = false;
            this.runeControl4.Size = new System.Drawing.Size(44, 55);
            this.runeControl4.SlotImage = global::RuneApp.Runes.rune4;
            this.runeControl4.StarImage = null;
            this.runeControl4.TabIndex = 97;
            this.runeControl4.Text = "runeControl4";
            this.runeControl4.Visible = false;
            this.runeControl4.Click += new System.EventHandler(this.rune_Click);
            // 
            // runeControl3
            // 
            this.runeControl3.BackColor = System.Drawing.Color.Transparent;
            this.runeControl3.BackImage = null;
            this.runeControl3.Coolness = 0;
            this.runeControl3.Gamma = 1F;
            this.runeControl3.Grade = 1;
            this.runeControl3.Location = new System.Drawing.Point(83, 396);
            this.runeControl3.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl3.Name = "runeControl3";
            this.runeControl3.SetImage = ((System.Drawing.Image)(resources.GetObject("runeControl3.SetImage")));
            this.runeControl3.ShowBack = false;
            this.runeControl3.ShowStars = false;
            this.runeControl3.Size = new System.Drawing.Size(53, 41);
            this.runeControl3.SlotImage = global::RuneApp.Runes.rune3;
            this.runeControl3.StarImage = null;
            this.runeControl3.TabIndex = 96;
            this.runeControl3.Text = "runeControl3";
            this.runeControl3.Visible = false;
            this.runeControl3.Click += new System.EventHandler(this.rune_Click);
            // 
            // runeControl2
            // 
            this.runeControl2.BackColor = System.Drawing.Color.Transparent;
            this.runeControl2.BackImage = null;
            this.runeControl2.Coolness = 0;
            this.runeControl2.Gamma = 1F;
            this.runeControl2.Grade = 1;
            this.runeControl2.Location = new System.Drawing.Point(83, 359);
            this.runeControl2.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl2.Name = "runeControl2";
            this.runeControl2.SetImage = ((System.Drawing.Image)(resources.GetObject("runeControl2.SetImage")));
            this.runeControl2.ShowBack = false;
            this.runeControl2.ShowStars = false;
            this.runeControl2.Size = new System.Drawing.Size(53, 41);
            this.runeControl2.SlotImage = global::RuneApp.Runes.rune2;
            this.runeControl2.StarImage = null;
            this.runeControl2.TabIndex = 95;
            this.runeControl2.Text = "runeControl2";
            this.runeControl2.Visible = false;
            this.runeControl2.Click += new System.EventHandler(this.rune_Click);
            // 
            // runeControl1
            // 
            this.runeControl1.BackColor = System.Drawing.Color.Transparent;
            this.runeControl1.BackImage = null;
            this.runeControl1.Coolness = 0;
            this.runeControl1.Gamma = 1F;
            this.runeControl1.Grade = 1;
            this.runeControl1.Location = new System.Drawing.Point(53, 328);
            this.runeControl1.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl1.Name = "runeControl1";
            this.runeControl1.SetImage = ((System.Drawing.Image)(resources.GetObject("runeControl1.SetImage")));
            this.runeControl1.ShowBack = false;
            this.runeControl1.ShowStars = false;
            this.runeControl1.Size = new System.Drawing.Size(41, 55);
            this.runeControl1.SlotImage = global::RuneApp.Runes.rune1;
            this.runeControl1.StarImage = null;
            this.runeControl1.TabIndex = 94;
            this.runeControl1.Text = "runeControl2";
            this.runeControl1.Visible = false;
            this.runeControl1.Click += new System.EventHandler(this.rune_Click);
            // 
            // Set3Label
            // 
            this.Set3Label.AutoSize = true;
            this.Set3Label.Location = new System.Drawing.Point(167, 376);
            this.Set3Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Set3Label.Name = "Set3Label";
            this.Set3Label.Size = new System.Drawing.Size(63, 13);
            this.Set3Label.TabIndex = 92;
            this.Set3Label.Text = "SampleText";
            // 
            // Set2Label
            // 
            this.Set2Label.AutoSize = true;
            this.Set2Label.Location = new System.Drawing.Point(167, 362);
            this.Set2Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Set2Label.Name = "Set2Label";
            this.Set2Label.Size = new System.Drawing.Size(40, 13);
            this.Set2Label.TabIndex = 91;
            this.Set2Label.Text = "Energy";
            // 
            // Set1Label
            // 
            this.Set1Label.AutoSize = true;
            this.Set1Label.Location = new System.Drawing.Point(167, 348);
            this.Set1Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Set1Label.Name = "Set1Label";
            this.Set1Label.Size = new System.Drawing.Size(30, 13);
            this.Set1Label.TabIndex = 90;
            this.Set1Label.Text = "Swift";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::RuneApp.Runes.runes;
            this.pictureBox1.Location = new System.Drawing.Point(5, 325);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(138, 151);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 93;
            this.pictureBox1.TabStop = false;
            // 
            // btnHelp
            // 
            this.btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp.Location = new System.Drawing.Point(209, 19);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(45, 23);
            this.btnHelp.TabIndex = 89;
            this.btnHelp.Text = "Help?";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(179, 664);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 0;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(98, 664);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 708);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1008, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Maximum = 1000;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(300, 16);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(118, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Levels";
            // 
            // Generate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 730);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.loadoutList);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Generate";
            this.Text = "Generate";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.runeBuild.ResumeLayout(false);
            this.runeBuild.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.ListView loadoutList;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label Set3Label;
        private System.Windows.Forms.Label Set2Label;
        private System.Windows.Forms.Label Set1Label;
        private System.Windows.Forms.PictureBox pictureBox1;
        private RuneControl runeControl6;
        private RuneControl runeControl5;
        private RuneControl runeControl4;
        private RuneControl runeControl3;
        private RuneControl runeControl2;
        private RuneControl runeControl1;
        private RuneControl[] runes;
        private System.Windows.Forms.GroupBox runeBuild;
        private System.Windows.Forms.Label SRuneLevel;
        private System.Windows.Forms.Label SRuneMon;
        private System.Windows.Forms.Label hideBuildRune;
        private RuneControl runeShown;
        private System.Windows.Forms.Label SRuneSub4;
        private System.Windows.Forms.Label SRuneSub3;
        private System.Windows.Forms.Label SRuneSub2;
        private System.Windows.Forms.Label SRuneSub1;
        private System.Windows.Forms.Label SRuneInnate;
        private System.Windows.Forms.Label SRuneMain;
        private System.Windows.Forms.ColumnHeader columnHeader2;
    }
}