namespace OpenNETCF.ORM.MainDemo.SQLite
{
    partial class frmMainSQLite
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblCreatedOn = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblSize = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDeleteDataStore = new System.Windows.Forms.Button();
            this.btnCreateDataStore = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.dbsStructure = new OpenNETCF.ORM.MainDemo.Common.DatabaseStructure();
            this.dbtTests = new OpenNETCF.ORM.MainDemo.Common.DatabaseTests();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
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
            this.splitMain.Panel1.Controls.Add(this.btnOpenFile);
            this.splitMain.Panel1.Controls.Add(this.txtFileName);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.dbtTests);
            this.splitMain.Size = new System.Drawing.Size(1012, 568);
            this.splitMain.SplitterDistance = 370;
            this.splitMain.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblCreatedOn);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.lblSize);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnDeleteDataStore);
            this.groupBox1.Controls.Add(this.btnCreateDataStore);
            this.groupBox1.Location = new System.Drawing.Point(11, 74);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(345, 94);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Database Info";
            // 
            // lblCreatedOn
            // 
            this.lblCreatedOn.AutoSize = true;
            this.lblCreatedOn.Location = new System.Drawing.Point(102, 61);
            this.lblCreatedOn.Name = "lblCreatedOn";
            this.lblCreatedOn.Size = new System.Drawing.Size(13, 17);
            this.lblCreatedOn.TabIndex = 5;
            this.lblCreatedOn.Text = "-";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 61);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 17);
            this.label4.TabIndex = 4;
            this.label4.Text = "Created On:";
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(102, 30);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(37, 17);
            this.lblSize.TabIndex = 3;
            this.lblSize.Text = "0 Kb";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "File Size:";
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
            this.btnCreateDataStore.Location = new System.Drawing.Point(258, 14);
            this.btnCreateDataStore.Name = "btnCreateDataStore";
            this.btnCreateDataStore.Size = new System.Drawing.Size(75, 33);
            this.btnCreateDataStore.TabIndex = 0;
            this.btnCreateDataStore.Text = "Create";
            this.btnCreateDataStore.UseVisualStyleBackColor = true;
            this.btnCreateDataStore.Click += new System.EventHandler(this.btnCreateDataStore_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Database File";
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(328, 35);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(28, 23);
            this.btnOpenFile.TabIndex = 9;
            this.btnOpenFile.Text = "...";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(11, 35);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(311, 22);
            this.txtFileName.TabIndex = 8;
            // 
            // dbsStructure
            // 
            this.dbsStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dbsStructure.Location = new System.Drawing.Point(3, 174);
            this.dbsStructure.Name = "dbsStructure";
            this.dbsStructure.Size = new System.Drawing.Size(364, 394);
            this.dbsStructure.TabIndex = 12;
            // 
            // dbtTests
            // 
            this.dbtTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbtTests.Location = new System.Drawing.Point(0, 0);
            this.dbtTests.MinimumSize = new System.Drawing.Size(615, 420);
            this.dbtTests.Name = "dbtTests";
            this.dbtTests.Size = new System.Drawing.Size(638, 568);
            this.dbtTests.TabIndex = 0;
            // 
            // frmMainSqlCe
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1012, 568);
            this.Controls.Add(this.splitMain);
            this.MinimumSize = new System.Drawing.Size(1020, 600);
            this.Name = "frmMainSqlCe";
            this.Text = "OpenNETCF ORM SqlCe Demo";
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel1.PerformLayout();
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblCreatedOn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDeleteDataStore;
        private System.Windows.Forms.Button btnCreateDataStore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.TextBox txtFileName;
        private Common.DatabaseStructure dbsStructure;
        private Common.DatabaseTests dbtTests;

    }
}