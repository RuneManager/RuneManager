namespace RuneApp
{
    partial class RuneDisplay
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
            this.runeBox6 = new RuneApp.RuneBox();
            this.runeBox1 = new RuneApp.RuneBox();
            this.runeBox2 = new RuneApp.RuneBox();
            this.runeBox3 = new RuneApp.RuneBox();
            this.runeBox4 = new RuneApp.RuneBox();
            this.runeBox5 = new RuneApp.RuneBox();
            this.runeDial = new RuneApp.RuneDial();
            this.SuspendLayout();
            // 
            // runeBox6
            // 
            this.runeBox6.Location = new System.Drawing.Point(10, 107);
            this.runeBox6.Name = "runeBox6";
            this.runeBox6.Size = new System.Drawing.Size(171, 179);
            this.runeBox6.TabIndex = 59;
            this.runeBox6.TabStop = false;
            this.runeBox6.Text = "6";
            // 
            // runeBox1
            // 
            this.runeBox1.Location = new System.Drawing.Point(187, 10);
            this.runeBox1.Name = "runeBox1";
            this.runeBox1.Size = new System.Drawing.Size(171, 179);
            this.runeBox1.TabIndex = 59;
            this.runeBox1.TabStop = false;
            this.runeBox1.Text = "1";
            // 
            // runeBox2
            // 
            this.runeBox2.Location = new System.Drawing.Point(364, 107);
            this.runeBox2.Name = "runeBox2";
            this.runeBox2.Size = new System.Drawing.Size(171, 179);
            this.runeBox2.TabIndex = 59;
            this.runeBox2.TabStop = false;
            this.runeBox2.Text = "2";
            // 
            // runeBox3
            // 
            this.runeBox3.Location = new System.Drawing.Point(364, 292);
            this.runeBox3.Name = "runeBox3";
            this.runeBox3.Size = new System.Drawing.Size(171, 179);
            this.runeBox3.TabIndex = 59;
            this.runeBox3.TabStop = false;
            this.runeBox3.Text = "3";
            // 
            // runeBox4
            // 
            this.runeBox4.Location = new System.Drawing.Point(187, 369);
            this.runeBox4.Name = "runeBox4";
            this.runeBox4.Size = new System.Drawing.Size(171, 179);
            this.runeBox4.TabIndex = 59;
            this.runeBox4.TabStop = false;
            this.runeBox4.Text = "4";
            // 
            // runeBox5
            // 
            this.runeBox5.Location = new System.Drawing.Point(12, 292);
            this.runeBox5.Name = "runeBox5";
            this.runeBox5.Size = new System.Drawing.Size(169, 179);
            this.runeBox5.TabIndex = 59;
            this.runeBox5.TabStop = false;
            this.runeBox5.Text = "5";
            // 
            // runeDial
            // 
            this.runeDial.IsVertical = false;
            this.runeDial.Loadout = null;
            this.runeDial.Location = new System.Drawing.Point(185, 195);
            this.runeDial.Name = "runeDial";
            this.runeDial.Size = new System.Drawing.Size(177, 175);
            this.runeDial.TabIndex = 60;
            // 
            // RuneDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 556);
            this.Controls.Add(this.runeBox5);
            this.Controls.Add(this.runeBox4);
            this.Controls.Add(this.runeBox3);
            this.Controls.Add(this.runeBox2);
            this.Controls.Add(this.runeBox1);
            this.Controls.Add(this.runeBox6);
            this.Controls.Add(this.runeDial);
            this.Icon = App.Icon;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "RuneDisplay";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "RuneDial";
            this.ResumeLayout(false);

        }

        #endregion
        private RuneApp.RuneBox runeBox6;
        private RuneApp.RuneBox runeBox1;
        private RuneApp.RuneBox runeBox2;
        private RuneApp.RuneBox runeBox3;
        private RuneApp.RuneBox runeBox4;
        private RuneApp.RuneBox runeBox5;
        private RuneDial runeDial;
    }
}