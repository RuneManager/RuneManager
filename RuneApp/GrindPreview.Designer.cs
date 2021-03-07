namespace RuneApp {
    partial class GrindPreview {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GrindPreview));
            this.listRunes = new System.Windows.Forms.ListView();
            this.runesSet = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesAttr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesRarity = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runesValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.runeBox3 = new RuneApp.RuneBox();
            this.runeBox2 = new RuneApp.RuneBox();
            this.runeBox1 = new RuneApp.RuneBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbSub = new System.Windows.Forms.ComboBox();
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
            this.runesAttr,
            this.runesRarity,
            this.runesType,
            this.runesValue});
            this.listRunes.FullRowSelect = true;
            this.listRunes.Location = new System.Drawing.Point(11, 11);
            this.listRunes.Margin = new System.Windows.Forms.Padding(2);
            this.listRunes.MultiSelect = false;
            this.listRunes.Name = "listRunes";
            this.listRunes.Size = new System.Drawing.Size(392, 623);
            this.listRunes.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listRunes.TabIndex = 22;
            this.listRunes.UseCompatibleStateImageBehavior = false;
            this.listRunes.View = System.Windows.Forms.View.Details;
            this.listRunes.SelectedIndexChanged += new System.EventHandler(this.listRunes_SelectedIndexChanged);
            // 
            // runesSet
            // 
            this.runesSet.DisplayIndex = 2;
            this.runesSet.Text = "Set";
            this.runesSet.Width = 90;
            // 
            // runesAttr
            // 
            this.runesAttr.Text = "Attr";
            this.runesAttr.Width = 91;
            // 
            // runesRarity
            // 
            this.runesRarity.DisplayIndex = 0;
            this.runesRarity.Text = "Rarity";
            this.runesRarity.Width = 40;
            // 
            // runesType
            // 
            this.runesType.Text = "Type";
            this.runesType.Width = 59;
            // 
            // runesValue
            // 
            this.runesValue.Text = "Value";
            this.runesValue.Width = 69;
            // 
            // runeBox3
            // 
            this.runeBox3.AllowGrind = true;
            this.runeBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeBox3.Location = new System.Drawing.Point(408, 396);
            this.runeBox3.Name = "runeBox3";
            this.runeBox3.RuneId = ((ulong)(0ul));
            this.runeBox3.Size = new System.Drawing.Size(215, 186);
            this.runeBox3.TabIndex = 25;
            this.runeBox3.TabStop = false;
            this.runeBox3.Text = "Rune";
            // 
            // runeBox2
            // 
            this.runeBox2.AllowGrind = true;
            this.runeBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeBox2.Location = new System.Drawing.Point(408, 204);
            this.runeBox2.Name = "runeBox2";
            this.runeBox2.RuneId = ((ulong)(0ul));
            this.runeBox2.Size = new System.Drawing.Size(215, 186);
            this.runeBox2.TabIndex = 24;
            this.runeBox2.TabStop = false;
            this.runeBox2.Text = "Rune";
            // 
            // runeBox1
            // 
            this.runeBox1.AllowGrind = true;
            this.runeBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runeBox1.Location = new System.Drawing.Point(408, 12);
            this.runeBox1.Name = "runeBox1";
            this.runeBox1.RuneId = ((ulong)(0ul));
            this.runeBox1.Size = new System.Drawing.Size(215, 186);
            this.runeBox1.TabIndex = 23;
            this.runeBox1.TabStop = false;
            this.runeBox1.Text = "Rune";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(408, 591);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Enchant";
            // 
            // cbSub
            // 
            this.cbSub.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSub.FormattingEnabled = true;
            this.cbSub.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.cbSub.Location = new System.Drawing.Point(461, 588);
            this.cbSub.Name = "cbSub";
            this.cbSub.Size = new System.Drawing.Size(121, 21);
            this.cbSub.TabIndex = 27;
            this.cbSub.SelectedIndexChanged += new System.EventHandler(this.cbSub_SelectedIndexChanged);
            // 
            // GrindPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 645);
            this.Controls.Add(this.cbSub);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.runeBox3);
            this.Controls.Add(this.runeBox2);
            this.Controls.Add(this.runeBox1);
            this.Controls.Add(this.listRunes);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GrindPreview";
            this.Text = "GrindPreview";
            this.Shown += new System.EventHandler(this.GrindPreview_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RuneBox runeBox1;
        private System.Windows.Forms.ListView listRunes;
        private System.Windows.Forms.ColumnHeader runesSet;
        private System.Windows.Forms.ColumnHeader runesAttr;
        private System.Windows.Forms.ColumnHeader runesRarity;
        private System.Windows.Forms.ColumnHeader runesType;
        private System.Windows.Forms.ColumnHeader runesValue;
        private RuneBox runeBox2;
        private RuneBox runeBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbSub;
    }
}