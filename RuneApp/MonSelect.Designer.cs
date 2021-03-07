namespace RuneApp
{
    partial class MonSelect
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
            this.dataMonsterList = new System.Windows.Forms.ListView();
            this.MonName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MonID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.MonPriority = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btn_select = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.MonLvl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // dataMonsterList
            // 
            this.dataMonsterList.AllowColumnReorder = true;
            this.dataMonsterList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataMonsterList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.MonName,
            this.MonID,
            this.MonPriority,
            this.MonLvl});
            this.dataMonsterList.FullRowSelect = true;
            this.dataMonsterList.HideSelection = false;
            this.dataMonsterList.Location = new System.Drawing.Point(11, 37);
            this.dataMonsterList.Margin = new System.Windows.Forms.Padding(2);
            this.dataMonsterList.MultiSelect = false;
            this.dataMonsterList.Name = "dataMonsterList";
            this.dataMonsterList.Size = new System.Drawing.Size(271, 514);
            this.dataMonsterList.TabIndex = 1;
            this.dataMonsterList.UseCompatibleStateImageBehavior = false;
            this.dataMonsterList.View = System.Windows.Forms.View.Details;
            this.dataMonsterList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_ColumnClick);
            this.dataMonsterList.DoubleClick += new System.EventHandler(this.dataMonsterList_SelectedIndexChanged);
            // 
            // MonName
            // 
            this.MonName.DisplayIndex = 3;
            this.MonName.Text = "Name";
            this.MonName.Width = 130;
            // 
            // MonID
            // 
            this.MonID.Text = "ID";
            this.MonID.Width = 40;
            // 
            // MonPriority
            // 
            this.MonPriority.DisplayIndex = 0;
            this.MonPriority.Text = "Priority";
            this.MonPriority.Width = 40;
            // 
            // btn_select
            // 
            this.btn_select.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_select.Location = new System.Drawing.Point(125, 556);
            this.btn_select.Name = "btn_select";
            this.btn_select.Size = new System.Drawing.Size(75, 23);
            this.btn_select.TabIndex = 2;
            this.btn_select.Text = "Select";
            this.btn_select.UseVisualStyleBackColor = true;
            this.btn_select.Click += new System.EventHandler(this.btn_select_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(206, 556);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 2;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
            // 
            // MonLvl
            // 
            this.MonLvl.DisplayIndex = 2;
            this.MonLvl.Text = "Lvl";
            this.MonLvl.Width = 30;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(269, 20);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextChanged += new System.EventHandler(this.TextBox1_TextChanged);
            // 
            // MonSelect
            // 
            this.AcceptButton = this.btn_select;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(293, 591);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_select);
            this.Controls.Add(this.dataMonsterList);
            this.Icon = global::RuneApp.App.Icon;
            this.Name = "MonSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MonSelect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView dataMonsterList;
        private System.Windows.Forms.ColumnHeader MonName;
        private System.Windows.Forms.ColumnHeader MonID;
        private System.Windows.Forms.ColumnHeader MonPriority;
        private System.Windows.Forms.Button btn_select;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.ColumnHeader MonLvl;
        private System.Windows.Forms.TextBox textBox1;
    }
}