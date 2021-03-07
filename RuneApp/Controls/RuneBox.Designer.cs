namespace RuneApp
{
    partial class RuneBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbMain = new System.Windows.Forms.Label();
            this.lbInnate = new System.Windows.Forms.Label();
            this.runeControl = new RuneApp.RuneControl();
            this.lbLevel = new System.Windows.Forms.Label();
            this.lb1 = new System.Windows.Forms.Label();
            this.lb2 = new System.Windows.Forms.Label();
            this.lb3 = new System.Windows.Forms.Label();
            this.lb4 = new System.Windows.Forms.Label();
            this.lbMon = new System.Windows.Forms.Label();
            this.lbClose = new System.Windows.Forms.Label();
            this.btnGrind = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbMain
            // 
            this.lbMain.AutoSize = true;
            this.lbMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMain.Location = new System.Drawing.Point(69, 17);
            this.lbMain.Name = "lbMain";
            this.lbMain.Size = new System.Drawing.Size(40, 18);
            this.lbMain.TabIndex = 0;
            this.lbMain.Text = "Main";
            // 
            // lbInnate
            // 
            this.lbInnate.AutoSize = true;
            this.lbInnate.Location = new System.Drawing.Point(69, 37);
            this.lbInnate.Name = "lbInnate";
            this.lbInnate.Size = new System.Drawing.Size(37, 13);
            this.lbInnate.TabIndex = 0;
            this.lbInnate.Text = "Innate";
            // 
            // runeControl
            // 
            this.runeControl.BackColor = System.Drawing.Color.Transparent;
            this.runeControl.BackImage = global::RuneApp.Resources.Runes.bg_normal;
            this.runeControl.Coolness = 0;
            this.runeControl.Gamma = 1F;
            this.runeControl.Grade = 2;
            this.runeControl.Location = new System.Drawing.Point(4, 17);
            this.runeControl.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl.Name = "runeControl";
            this.runeControl.SetImage = global::RuneApp.Resources.Runes.despair;
            this.runeControl.ShowBack = true;
            this.runeControl.ShowStars = true;
            this.runeControl.Size = new System.Drawing.Size(58, 58);
            this.runeControl.SlotImage = global::RuneApp.Resources.Runes.rune2;
            this.runeControl.StarImage = global::RuneApp.Resources.Runes.star_unawakened;
            this.runeControl.TabIndex = 11;
            this.runeControl.Text = "Rune";
            // 
            // lbLevel
            // 
            this.lbLevel.AutoSize = true;
            this.lbLevel.Location = new System.Drawing.Point(69, 52);
            this.lbLevel.Name = "lbLevel";
            this.lbLevel.Size = new System.Drawing.Size(33, 13);
            this.lbLevel.TabIndex = 0;
            this.lbLevel.Text = "Level";
            // 
            // lb1
            // 
            this.lb1.AutoSize = true;
            this.lb1.Location = new System.Drawing.Point(4, 77);
            this.lb1.Name = "lb1";
            this.lb1.Size = new System.Drawing.Size(32, 13);
            this.lb1.TabIndex = 0;
            this.lb1.Text = "Sub1";
            // 
            // lb2
            // 
            this.lb2.AutoSize = true;
            this.lb2.Location = new System.Drawing.Point(4, 91);
            this.lb2.Name = "lb2";
            this.lb2.Size = new System.Drawing.Size(32, 13);
            this.lb2.TabIndex = 0;
            this.lb2.Text = "Sub2";
            // 
            // lb3
            // 
            this.lb3.AutoSize = true;
            this.lb3.Location = new System.Drawing.Point(4, 105);
            this.lb3.Name = "lb3";
            this.lb3.Size = new System.Drawing.Size(32, 13);
            this.lb3.TabIndex = 0;
            this.lb3.Text = "Sub3";
            // 
            // lb4
            // 
            this.lb4.AutoSize = true;
            this.lb4.Location = new System.Drawing.Point(4, 119);
            this.lb4.Name = "lb4";
            this.lb4.Size = new System.Drawing.Size(32, 13);
            this.lb4.TabIndex = 0;
            this.lb4.Text = "Sub4";
            // 
            // lbMon
            // 
            this.lbMon.AutoSize = true;
            this.lbMon.Location = new System.Drawing.Point(4, 159);
            this.lbMon.Name = "lbMon";
            this.lbMon.Size = new System.Drawing.Size(45, 13);
            this.lbMon.TabIndex = 0;
            this.lbMon.Text = "Monster";
            // 
            // lbClose
            // 
            this.lbClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbClose.AutoSize = true;
            this.lbClose.Location = new System.Drawing.Point(225, 15);
            this.lbClose.Name = "lbClose";
            this.lbClose.Size = new System.Drawing.Size(14, 13);
            this.lbClose.TabIndex = 22;
            this.lbClose.Text = "X";
            this.lbClose.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lbClose.Click += new System.EventHandler(this.lbClose_Click);
            // 
            // btnGrind
            // 
            this.btnGrind.Location = new System.Drawing.Point(144, 119);
            this.btnGrind.Name = "btnGrind";
            this.btnGrind.Size = new System.Drawing.Size(65, 23);
            this.btnGrind.TabIndex = 23;
            this.btnGrind.Text = "Grind";
            this.btnGrind.UseVisualStyleBackColor = true;
            this.btnGrind.Click += new System.EventHandler(this.btnGrind_Click);
            // 
            // RuneBox
            // 
            this.Controls.Add(this.btnGrind);
            this.Controls.Add(this.lbClose);
            this.Controls.Add(this.runeControl);
            this.Controls.Add(this.lbMain);
            this.Controls.Add(this.lbInnate);
            this.Controls.Add(this.lbLevel);
            this.Controls.Add(this.lb1);
            this.Controls.Add(this.lb2);
            this.Controls.Add(this.lb3);
            this.Controls.Add(this.lb4);
            this.Controls.Add(this.lbMon);
            this.Size = new System.Drawing.Size(241, 148);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RuneControl runeControl;
        private System.Windows.Forms.Label lbMain;
        private System.Windows.Forms.Label lbInnate;
        private System.Windows.Forms.Label lbLevel;
        private System.Windows.Forms.Label lb1;
        private System.Windows.Forms.Label lb2;
        private System.Windows.Forms.Label lb3;
        private System.Windows.Forms.Label lb4;
        private System.Windows.Forms.Label lbMon;
        private System.Windows.Forms.Label lbClose;
        private System.Windows.Forms.Button btnGrind;
    }
}
