namespace RuneApp {
    partial class Irene {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Irene));
            this.btnOk = new System.Windows.Forms.Button();
            this.cboxResponse = new System.Windows.Forms.ComboBox();
            this.lnkResponse = new System.Windows.Forms.LinkLabel();
            this.richTextSpeech = new RuneApp.Controls.RichTextBoxEx();
            this.transparentControl1 = new RuneApp.TransparentControl();
            this.imageIrene = new RuneApp.TransparentControl();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(207, 194);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 2;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // cboxResponse
            // 
            this.cboxResponse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxResponse.FormattingEnabled = true;
            this.cboxResponse.Items.AddRange(new object[] {
            "Who are you?",
            "Explain what I point at",
            "Go away for now",
            "Leave until called"});
            this.cboxResponse.Location = new System.Drawing.Point(12, 196);
            this.cboxResponse.Name = "cboxResponse";
            this.cboxResponse.Size = new System.Drawing.Size(189, 21);
            this.cboxResponse.TabIndex = 3;
            this.cboxResponse.SelectedIndexChanged += new System.EventHandler(this.cboxResponse_SelectedIndexChanged);
            // 
            // lnkResponse
            // 
            this.lnkResponse.AutoSize = true;
            this.lnkResponse.LinkArea = new System.Windows.Forms.LinkArea(16, 4);
            this.lnkResponse.Location = new System.Drawing.Point(12, 199);
            this.lnkResponse.Name = "lnkResponse";
            this.lnkResponse.Size = new System.Drawing.Size(142, 17);
            this.lnkResponse.TabIndex = 5;
            this.lnkResponse.TabStop = true;
            this.lnkResponse.Text = "I would like to sell my runes";
            this.lnkResponse.UseCompatibleTextRendering = true;
            this.lnkResponse.Visible = false;
            this.lnkResponse.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkResponse_LinkClicked);
            // 
            // richTextSpeech
            // 
            this.richTextSpeech.BackColor = System.Drawing.SystemColors.Window;
            this.richTextSpeech.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextSpeech.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextSpeech.Location = new System.Drawing.Point(14, 11);
            this.richTextSpeech.Name = "richTextSpeech";
            this.richTextSpeech.ReadOnly = true;
            this.richTextSpeech.Size = new System.Drawing.Size(268, 173);
            this.richTextSpeech.TabIndex = 1;
            this.richTextSpeech.Text = "Make sure you sign up to http://swarfarm.com.\nYou can share all your cool monster" +
    "s there.";
            this.richTextSpeech.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.richTextSpeech_LinkClicked);
            // 
            // transparentControl1
            // 
            this.transparentControl1.BackColor = System.Drawing.Color.Transparent;
            this.transparentControl1.Gamma = 1F;
            this.transparentControl1.Image = ((System.Drawing.Image)(resources.GetObject("transparentControl1.Image")));
            this.transparentControl1.Location = new System.Drawing.Point(-3, -7);
            this.transparentControl1.Name = "transparentControl1";
            this.transparentControl1.Size = new System.Drawing.Size(344, 225);
            this.transparentControl1.TabIndex = 6;
            this.transparentControl1.Text = "transparentControl1";
            // 
            // imageIrene
            // 
            this.imageIrene.BackColor = System.Drawing.Color.Transparent;
            this.imageIrene.Gamma = 1F;
            this.imageIrene.Image = ((System.Drawing.Image)(resources.GetObject("imageIrene.Image")));
            this.imageIrene.Location = new System.Drawing.Point(0, 0);
            this.imageIrene.Name = "imageIrene";
            this.imageIrene.Size = new System.Drawing.Size(512, 512);
            this.imageIrene.TabIndex = 0;
            this.imageIrene.Text = "transparentControl1";
            this.imageIrene.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imageIrene_MouseDown);
            // 
            // Irene
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(512, 512);
            this.Controls.Add(this.lnkResponse);
            this.Controls.Add(this.cboxResponse);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.richTextSpeech);
            this.Controls.Add(this.transparentControl1);
            this.Controls.Add(this.imageIrene);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Irene";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Irene";
            this.Load += new System.EventHandler(this.Irene_Load);
            this.Shown += new System.EventHandler(this.Irene_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TransparentControl imageIrene;
        private RuneApp.Controls.RichTextBoxEx richTextSpeech;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ComboBox cboxResponse;
        private System.Windows.Forms.LinkLabel lnkResponse;
        private TransparentControl transparentControl1;
    }
}