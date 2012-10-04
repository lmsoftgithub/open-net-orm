namespace OpenNETCF.ORM.MainDemo.MSSQL
{
    partial class frmMainMSSQL
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
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.dbsStructure = new OpenNETCF.ORM.MainDemo.Common.DatabaseStructure();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDeleteDataStore = new System.Windows.Forms.Button();
            this.btnCreateDataStore = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.tabTests = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dbtTests = new OpenNETCF.ORM.MainDemo.Common.DatabaseTests();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.deTests = new OpenNETCF.ORM.MainDemo.Common.DynamicEntityTests();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabTests.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.IsSplitterFixed = true;
            this.splitMain.Location = new System.Drawing.Point(0, 0);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.dbsStructure);
            this.splitMain.Panel1.Controls.Add(this.groupBox1);
            this.splitMain.Panel1.Controls.Add(this.label1);
            this.splitMain.Panel1.Controls.Add(this.txtConnectionString);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.tabTests);
            this.splitMain.Size = new System.Drawing.Size(1012, 568);
            this.splitMain.SplitterDistance = 370;
            this.splitMain.TabIndex = 0;
            // 
            // dbsStructure
            // 
            this.dbsStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dbsStructure.Location = new System.Drawing.Point(3, 174);
            this.dbsStructure.Name = "dbsStructure";
            this.dbsStructure.Size = new System.Drawing.Size(364, 394);
            this.dbsStructure.StopGenerator = false;
            this.dbsStructure.TabIndex = 12;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDeleteDataStore);
            this.groupBox1.Controls.Add(this.btnCreateDataStore);
            this.groupBox1.Location = new System.Drawing.Point(11, 74);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(345, 94);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database Info";
            // 
            // btnDeleteDataStore
            // 
            this.btnDeleteDataStore.Location = new System.Drawing.Point(258, 53);
            this.btnDeleteDataStore.Name = "btnDeleteDataStore";
            this.btnDeleteDataStore.Size = new System.Drawing.Size(75, 32);
            this.btnDeleteDataStore.TabIndex = 1;
            this.btnDeleteDataStore.Text = "Delete";
            this.btnDeleteDataStore.UseVisualStyleBackColor = true;
            this.btnDeleteDataStore.Click += new System.EventHandler(this.btnDeleteDataStore_Click);
            // 
            // btnCreateDataStore
            // 
            this.btnCreateDataStore.Location = new System.Drawing.Point(215, 14);
            this.btnCreateDataStore.Name = "btnCreateDataStore";
            this.btnCreateDataStore.Size = new System.Drawing.Size(118, 33);
            this.btnCreateDataStore.TabIndex = 0;
            this.btnCreateDataStore.Text = "Create / Update";
            this.btnCreateDataStore.UseVisualStyleBackColor = true;
            this.btnCreateDataStore.Click += new System.EventHandler(this.btnCreateDataStore_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(185, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Database Connection String";
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(11, 35);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(345, 22);
            this.txtConnectionString.TabIndex = 8;
            // 
            // tabTests
            // 
            this.tabTests.Controls.Add(this.tabPage1);
            this.tabTests.Controls.Add(this.tabPage2);
            this.tabTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabTests.Location = new System.Drawing.Point(0, 0);
            this.tabTests.Name = "tabTests";
            this.tabTests.SelectedIndex = 0;
            this.tabTests.Size = new System.Drawing.Size(638, 568);
            this.tabTests.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dbtTests);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(630, 539);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Entities";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dbtTests
            // 
            this.dbtTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbtTests.Location = new System.Drawing.Point(3, 3);
            this.dbtTests.MinimumSize = new System.Drawing.Size(615, 420);
            this.dbtTests.Name = "dbtTests";
            this.dbtTests.Size = new System.Drawing.Size(624, 533);
            this.dbtTests.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.deTests);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(630, 539);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Dynamic Entities";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // deTests
            // 
            this.deTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deTests.Location = new System.Drawing.Point(3, 3);
            this.deTests.Name = "deTests";
            this.deTests.Size = new System.Drawing.Size(624, 533);
            this.deTests.TabIndex = 0;
            // 
            // frmMainMSSQL
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 568);
            this.Controls.Add(this.splitMain);
            this.MinimumSize = new System.Drawing.Size(1020, 600);
            this.Name = "frmMainMSSQL";
            this.Text = "OpenNETCF ORM MSSQL Demo";
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel1.PerformLayout();
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tabTests.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnDeleteDataStore;
        private System.Windows.Forms.Button btnCreateDataStore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConnectionString;
        private Common.DatabaseStructure dbsStructure;
        private System.Windows.Forms.TabControl tabTests;
        private System.Windows.Forms.TabPage tabPage1;
        private Common.DatabaseTests dbtTests;
        private System.Windows.Forms.TabPage tabPage2;
        private Common.DynamicEntityTests deTests;

    }
}