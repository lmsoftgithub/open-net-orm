namespace OpenNETCF.ORM.MainDemo.Common
{
    partial class DatabaseStructure
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
            this.btnDropTable = new System.Windows.Forms.Button();
            this.btnRefreshDBStructure = new System.Windows.Forms.Button();
            this.treeDBStructure = new System.Windows.Forms.TreeView();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnDeleteTable = new System.Windows.Forms.Button();
            this.btnDropAndCreate = new System.Windows.Forms.Button();
            this.chkCascade = new System.Windows.Forms.CheckBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnReport = new System.Windows.Forms.Button();
            this.prgProgress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnDropTable
            // 
            this.btnDropTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDropTable.Location = new System.Drawing.Point(5, 371);
            this.btnDropTable.Name = "btnDropTable";
            this.btnDropTable.Size = new System.Drawing.Size(75, 27);
            this.btnDropTable.TabIndex = 19;
            this.btnDropTable.Text = "Drop";
            this.btnDropTable.UseVisualStyleBackColor = true;
            this.btnDropTable.Click += new System.EventHandler(this.btnDropTable_Click);
            // 
            // btnRefreshDBStructure
            // 
            this.btnRefreshDBStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshDBStructure.Location = new System.Drawing.Point(129, 3);
            this.btnRefreshDBStructure.Name = "btnRefreshDBStructure";
            this.btnRefreshDBStructure.Size = new System.Drawing.Size(75, 29);
            this.btnRefreshDBStructure.TabIndex = 18;
            this.btnRefreshDBStructure.Text = "Refresh";
            this.btnRefreshDBStructure.UseVisualStyleBackColor = true;
            this.btnRefreshDBStructure.Click += new System.EventHandler(this.btnRefreshDBStructure_Click);
            // 
            // treeDBStructure
            // 
            this.treeDBStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeDBStructure.Location = new System.Drawing.Point(5, 38);
            this.treeDBStructure.Name = "treeDBStructure";
            this.treeDBStructure.Size = new System.Drawing.Size(365, 327);
            this.treeDBStructure.TabIndex = 17;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(1, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(122, 17);
            this.lblTitle.TabIndex = 16;
            this.lblTitle.Text = "Database Content";
            this.lblTitle.DoubleClick += new System.EventHandler(this.lblTitle_DblClick);
            // 
            // btnDeleteTable
            // 
            this.btnDeleteTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteTable.Location = new System.Drawing.Point(86, 371);
            this.btnDeleteTable.Name = "btnDeleteTable";
            this.btnDeleteTable.Size = new System.Drawing.Size(75, 27);
            this.btnDeleteTable.TabIndex = 20;
            this.btnDeleteTable.Text = "Delete";
            this.btnDeleteTable.UseVisualStyleBackColor = true;
            this.btnDeleteTable.Click += new System.EventHandler(this.btnDeleteTable_Click);
            // 
            // btnDropAndCreate
            // 
            this.btnDropAndCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDropAndCreate.Location = new System.Drawing.Point(167, 371);
            this.btnDropAndCreate.Name = "btnDropAndCreate";
            this.btnDropAndCreate.Size = new System.Drawing.Size(125, 27);
            this.btnDropAndCreate.TabIndex = 21;
            this.btnDropAndCreate.Text = "Drop and Create";
            this.btnDropAndCreate.UseVisualStyleBackColor = true;
            this.btnDropAndCreate.Click += new System.EventHandler(this.btnDropAndCreate_Click);
            // 
            // chkCascade
            // 
            this.chkCascade.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkCascade.AutoSize = true;
            this.chkCascade.Location = new System.Drawing.Point(5, 404);
            this.chkCascade.Name = "chkCascade";
            this.chkCascade.Size = new System.Drawing.Size(135, 21);
            this.chkCascade.TabIndex = 22;
            this.chkCascade.Text = "Cascade Actions";
            this.chkCascade.UseVisualStyleBackColor = true;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(210, 3);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(118, 29);
            this.btnGenerate.TabIndex = 23;
            this.btnGenerate.Text = "Generate Data";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // btnReport
            // 
            this.btnReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReport.Location = new System.Drawing.Point(350, 402);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new System.Drawing.Size(20, 23);
            this.btnReport.TabIndex = 24;
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
            // 
            // prgProgress
            // 
            this.prgProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.prgProgress.Location = new System.Drawing.Point(146, 402);
            this.prgProgress.Name = "prgProgress";
            this.prgProgress.Size = new System.Drawing.Size(198, 23);
            this.prgProgress.TabIndex = 25;
            // 
            // DatabaseStructure
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.prgProgress);
            this.Controls.Add(this.btnReport);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.chkCascade);
            this.Controls.Add(this.btnDropAndCreate);
            this.Controls.Add(this.btnDeleteTable);
            this.Controls.Add(this.btnDropTable);
            this.Controls.Add(this.btnRefreshDBStructure);
            this.Controls.Add(this.treeDBStructure);
            this.Controls.Add(this.lblTitle);
            this.Name = "DatabaseStructure";
            this.Size = new System.Drawing.Size(377, 436);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDropTable;
        private System.Windows.Forms.Button btnRefreshDBStructure;
        private System.Windows.Forms.TreeView treeDBStructure;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnDeleteTable;
        private System.Windows.Forms.Button btnDropAndCreate;
        private System.Windows.Forms.CheckBox chkCascade;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.ProgressBar prgProgress;
    }
}
