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
            this.listView2 = new System.Windows.Forms.ListView();
            this.runesSet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesGrade = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesMType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesMValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.runeBox2 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.runeInventory = new RuneApp.RuneControl();
            this.IRuneSub4 = new System.Windows.Forms.Label();
            this.IRuneSub3 = new System.Windows.Forms.Label();
            this.IRuneSub2 = new System.Windows.Forms.Label();
            this.IRuneSub1 = new System.Windows.Forms.Label();
            this.IRuneInnate = new System.Windows.Forms.Label();
            this.IRuneMain = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.IRuneMon = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            this.runeBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView2
            // 
            this.listView2.AllowColumnReorder = true;
            this.listView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.runesSet,
            this.runesID,
            this.runesGrade,
            this.runesMType,
            this.runesMValue});
            this.listView2.FullRowSelect = true;
            this.listView2.Location = new System.Drawing.Point(11, 27);
            this.listView2.Margin = new System.Windows.Forms.Padding(2);
            this.listView2.MultiSelect = false;
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(346, 542);
            this.listView2.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listView2.TabIndex = 1;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            this.listView2.SelectedIndexChanged += new System.EventHandler(this.listView2_SelectedIndexChanged);
            this.listView2.DoubleClick += new System.EventHandler(this.listView2_DoubleClick);
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
            this.runesMType.Text = "MainType";
            this.runesMType.Width = 80;
            // 
            // runesMValue
            // 
            this.runesMValue.Text = "MainValue";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(589, 25);
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
            // runeBox2
            // 
            this.runeBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeBox2.Controls.Add(this.IRuneMon);
            this.runeBox2.Controls.Add(this.label8);
            this.runeBox2.Controls.Add(this.runeInventory);
            this.runeBox2.Controls.Add(this.IRuneSub4);
            this.runeBox2.Controls.Add(this.IRuneSub3);
            this.runeBox2.Controls.Add(this.IRuneSub2);
            this.runeBox2.Controls.Add(this.IRuneSub1);
            this.runeBox2.Controls.Add(this.IRuneInnate);
            this.runeBox2.Controls.Add(this.IRuneMain);
            this.runeBox2.Location = new System.Drawing.Point(361, 27);
            this.runeBox2.Margin = new System.Windows.Forms.Padding(2);
            this.runeBox2.Name = "runeBox2";
            this.runeBox2.Padding = new System.Windows.Forms.Padding(2);
            this.runeBox2.Size = new System.Drawing.Size(217, 167);
            this.runeBox2.TabIndex = 13;
            this.runeBox2.TabStop = false;
            this.runeBox2.Text = "Rune";
            this.runeBox2.Visible = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(199, 15);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(14, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "X";
            // 
            // runeInventory
            // 
            this.runeInventory.BackColor = System.Drawing.Color.Transparent;
            this.runeInventory.BackImage = global::RuneApp.Runes.bg_normal;
            this.runeInventory.Coolness = 0;
            this.runeInventory.Gamma = 1F;
            this.runeInventory.Grade = 2;
            this.runeInventory.Location = new System.Drawing.Point(4, 17);
            this.runeInventory.Margin = new System.Windows.Forms.Padding(2);
            this.runeInventory.Name = "runeInventory";
            this.runeInventory.SetImage = global::RuneApp.Runes.despair;
            this.runeInventory.ShowBack = true;
            this.runeInventory.ShowStars = true;
            this.runeInventory.Size = new System.Drawing.Size(58, 58);
            this.runeInventory.SlotImage = global::RuneApp.Runes.rune2;
            this.runeInventory.StarImage = global::RuneApp.Runes.star_unawakened;
            this.runeInventory.TabIndex = 11;
            this.runeInventory.Text = "runeControl2";
            // 
            // IRuneSub4
            // 
            this.IRuneSub4.AutoSize = true;
            this.IRuneSub4.Location = new System.Drawing.Point(4, 119);
            this.IRuneSub4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IRuneSub4.Name = "IRuneSub4";
            this.IRuneSub4.Size = new System.Drawing.Size(32, 13);
            this.IRuneSub4.TabIndex = 17;
            this.IRuneSub4.Text = "Sub4";
            // 
            // IRuneSub3
            // 
            this.IRuneSub3.AutoSize = true;
            this.IRuneSub3.Location = new System.Drawing.Point(4, 105);
            this.IRuneSub3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IRuneSub3.Name = "IRuneSub3";
            this.IRuneSub3.Size = new System.Drawing.Size(32, 13);
            this.IRuneSub3.TabIndex = 16;
            this.IRuneSub3.Text = "Sub3";
            // 
            // IRuneSub2
            // 
            this.IRuneSub2.AutoSize = true;
            this.IRuneSub2.Location = new System.Drawing.Point(4, 91);
            this.IRuneSub2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IRuneSub2.Name = "IRuneSub2";
            this.IRuneSub2.Size = new System.Drawing.Size(32, 13);
            this.IRuneSub2.TabIndex = 15;
            this.IRuneSub2.Text = "Sub2";
            // 
            // IRuneSub1
            // 
            this.IRuneSub1.AutoSize = true;
            this.IRuneSub1.Location = new System.Drawing.Point(4, 77);
            this.IRuneSub1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IRuneSub1.Name = "IRuneSub1";
            this.IRuneSub1.Size = new System.Drawing.Size(32, 13);
            this.IRuneSub1.TabIndex = 14;
            this.IRuneSub1.Text = "Sub1";
            // 
            // IRuneInnate
            // 
            this.IRuneInnate.AutoSize = true;
            this.IRuneInnate.Location = new System.Drawing.Point(69, 37);
            this.IRuneInnate.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IRuneInnate.Name = "IRuneInnate";
            this.IRuneInnate.Size = new System.Drawing.Size(37, 13);
            this.IRuneInnate.TabIndex = 13;
            this.IRuneInnate.Text = "Innate";
            // 
            // IRuneMain
            // 
            this.IRuneMain.AutoSize = true;
            this.IRuneMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IRuneMain.Location = new System.Drawing.Point(69, 17);
            this.IRuneMain.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IRuneMain.Name = "IRuneMain";
            this.IRuneMain.Size = new System.Drawing.Size(40, 18);
            this.IRuneMain.TabIndex = 12;
            this.IRuneMain.Text = "Main";
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(517, 545);
            this.button3.Margin = new System.Windows.Forms.Padding(2);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(61, 24);
            this.button3.TabIndex = 19;
            this.button3.Text = "Select";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(452, 545);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(61, 24);
            this.button1.TabIndex = 20;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // IRuneMon
            // 
            this.IRuneMon.AutoSize = true;
            this.IRuneMon.Location = new System.Drawing.Point(14, 143);
            this.IRuneMon.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.IRuneMon.Name = "IRuneMon";
            this.IRuneMon.Size = new System.Drawing.Size(52, 13);
            this.IRuneMon.TabIndex = 18;
            this.IRuneMon.Text = "Equipped";
            // 
            // RuneSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 580);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.runeBox2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView2);
            this.Name = "RuneSelect";
            this.Text = "RuneSelect";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.runeBox2.ResumeLayout(false);
            this.runeBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ColumnHeader runesSet;
        private System.Windows.Forms.ColumnHeader runesID;
        private System.Windows.Forms.ColumnHeader runesGrade;
        private System.Windows.Forms.ColumnHeader runesMType;
        private System.Windows.Forms.ColumnHeader runesMValue;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.GroupBox runeBox2;
        private System.Windows.Forms.Label label8;
        private RuneControl runeInventory;
        private System.Windows.Forms.Label IRuneSub4;
        private System.Windows.Forms.Label IRuneSub3;
        private System.Windows.Forms.Label IRuneSub2;
        private System.Windows.Forms.Label IRuneSub1;
        private System.Windows.Forms.Label IRuneInnate;
        private System.Windows.Forms.Label IRuneMain;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label IRuneMon;
    }
}