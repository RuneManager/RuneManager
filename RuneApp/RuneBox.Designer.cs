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
            this.runeControl1 = new RuneApp.RuneControl();
            this.lbLevel = new System.Windows.Forms.Label();
            this.lb1 = new System.Windows.Forms.Label();
            this.lb2 = new System.Windows.Forms.Label();
            this.lb3 = new System.Windows.Forms.Label();
            this.lb4 = new System.Windows.Forms.Label();
            this.lbMon = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // runeControl1
            // 
            this.runeControl1.BackColor = System.Drawing.Color.Transparent;
            this.runeControl1.BackImage = global::RuneApp.Runes.bg_normal;
            this.runeControl1.Coolness = 0;
            this.runeControl1.Gamma = 1F;
            this.runeControl1.Grade = 2;
            this.runeControl1.Location = new System.Drawing.Point(4, 17);
            this.runeControl1.Margin = new System.Windows.Forms.Padding(2);
            this.runeControl1.Name = "runeControl1";
            this.runeControl1.SetImage = global::RuneApp.Runes.despair;
            this.runeControl1.ShowBack = true;
            this.runeControl1.ShowStars = true;
            this.runeControl1.Size = new System.Drawing.Size(58, 58);
            this.runeControl1.SlotImage = global::RuneApp.Runes.rune2;
            this.runeControl1.StarImage = global::RuneApp.Runes.star_unawakened;
            this.runeControl1.TabIndex = 11;
            this.runeControl1.Text = "Rune";
            // 
            // lbMain
            // 
            this.lbMain.AutoSize = true;
            this.lbMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMain.Location = new System.Drawing.Point(69, 17);
            this.lbMain.Name = "lbMain";
            this.lbMain.Size = new System.Drawing.Size(30, 13);
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
            // lbLevel
            // 
            this.lbLevel.AutoSize = true;
            this.lbLevel.Location = new System.Drawing.Point(69, 52);
            this.lbLevel.Name = "lbLevel";
            this.lbLevel.Size = new System.Drawing.Size(100, 23);
            this.lbLevel.TabIndex = 0;
            this.lbLevel.Text = "Level";
            // 
            // lb1
            // 
            this.lb1.AutoSize = true;
            this.lb1.Location = new System.Drawing.Point(4, 77);
            this.lb1.Name = "lb1";
            this.lb1.Size = new System.Drawing.Size(100, 23);
            this.lb1.TabIndex = 0;
            this.lb1.Text = "Sub1";
            // 
            // lb2
            // 
            this.lb2.AutoSize = true;
            this.lb2.Location = new System.Drawing.Point(4, 91);
            this.lb3.Name = "lb3";
            this.lb2.Size = new System.Drawing.Size(100, 23);
            this.lb2.TabIndex = 0;
            this.lb2.Text = "Sub2";
            // 
            // lb3
            // 
            this.lb3.AutoSize = true;
            this.lb3.Location = new System.Drawing.Point(4, 105);
            this.lb3.Name = "lb3";
            this.lb3.Size = new System.Drawing.Size(100, 23);
            this.lb3.TabIndex = 0;
            this.lb3.Text = "Sub3";
            // 
            // lb4
            // 
            this.lb4.AutoSize = true;
            this.lb4.Location = new System.Drawing.Point(4, 119);
            this.lb4.Name = "lb4";
            this.lb4.Size = new System.Drawing.Size(100, 23);
            this.lb4.TabIndex = 0;
            this.lb4.Text = "Sub4";
            // 
            // lbMon
            // 
            this.lbMon.AutoSize = true;
            this.lbMon.Location = new System.Drawing.Point(4, 159);
            this.lbMon.Name = "lbMon";
            this.lbMon.Size = new System.Drawing.Size(100, 23);
            this.lbMon.TabIndex = 0;
            this.lbMon.Text = "Monster";
            // 
            // RuneBox
            // 
            this.Controls.Add(this.runeControl1);
            this.Controls.Add(this.lbMain);
            this.Controls.Add(this.lbInnate);
            this.Controls.Add(this.lbLevel);
            this.Controls.Add(this.lb1);
            this.Controls.Add(this.lb2);
            this.Controls.Add(this.lb3);
            this.Controls.Add(this.lb4);
            this.Controls.Add(this.lbMon);
            this.Size = new System.Drawing.Size(257, 186);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RuneControl runeControl1;
        private System.Windows.Forms.Label lbMain;
        private System.Windows.Forms.Label lbInnate;
        private System.Windows.Forms.Label lbLevel;
        private System.Windows.Forms.Label lb1;
        private System.Windows.Forms.Label lb2;
        private System.Windows.Forms.Label lb3;
        private System.Windows.Forms.Label lb4;
        private System.Windows.Forms.Label lbMon;
    }
}
