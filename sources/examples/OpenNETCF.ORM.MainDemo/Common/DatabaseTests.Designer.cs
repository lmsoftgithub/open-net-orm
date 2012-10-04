namespace OpenNETCF.ORM.MainDemo.Common
{
    partial class DatabaseTests
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
            this.grpDBTests = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.pnlModified = new System.Windows.Forms.Panel();
            this.btnReport = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblItemType = new System.Windows.Forms.Label();
            this.grpTableActions = new System.Windows.Forms.GroupBox();
            this.chkTransactional = new System.Windows.Forms.CheckBox();
            this.chkFillReferences = new System.Windows.Forms.CheckBox();
            this.chkFilterReferences = new System.Windows.Forms.CheckBox();
            this.chkCascadeTableActions = new System.Windows.Forms.CheckBox();
            this.btnDeleteFiltered = new System.Windows.Forms.Button();
            this.btnFetch = new System.Windows.Forms.Button();
            this.btnBulkInsertOrUpdate = new System.Windows.Forms.Button();
            this.btnBulkInsert = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnSelectFiltered = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.grpItemActions = new System.Windows.Forms.GroupBox();
            this.chkCascadeItemActions = new System.Windows.Forms.CheckBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnInsertOrUpdate = new System.Windows.Forms.Button();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.txtRowCount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtRowOffset = new System.Windows.Forms.TextBox();
            this.pgrItemEditor = new System.Windows.Forms.PropertyGrid();
            this.txtFilterValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbComparison = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbFields = new System.Windows.Forms.ComboBox();
            this.dgvResultSet = new System.Windows.Forms.DataGridView();
            this.cmbTable = new System.Windows.Forms.ComboBox();
            this.chkLimitResults = new System.Windows.Forms.CheckBox();
            this.grpDBTests.SuspendLayout();
            this.grpTableActions.SuspendLayout();
            this.grpItemActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResultSet)).BeginInit();
            this.SuspendLayout();
            // 
            // grpDBTests
            // 
            this.grpDBTests.Controls.Add(this.chkLimitResults);
            this.grpDBTests.Controls.Add(this.btnRefresh);
            this.grpDBTests.Controls.Add(this.pnlModified);
            this.grpDBTests.Controls.Add(this.btnReport);
            this.grpDBTests.Controls.Add(this.lblMessage);
            this.grpDBTests.Controls.Add(this.lblItemType);
            this.grpDBTests.Controls.Add(this.grpTableActions);
            this.grpDBTests.Controls.Add(this.label6);
            this.grpDBTests.Controls.Add(this.grpItemActions);
            this.grpDBTests.Controls.Add(this.txtRowCount);
            this.grpDBTests.Controls.Add(this.label5);
            this.grpDBTests.Controls.Add(this.label4);
            this.grpDBTests.Controls.Add(this.txtRowOffset);
            this.grpDBTests.Controls.Add(this.pgrItemEditor);
            this.grpDBTests.Controls.Add(this.txtFilterValue);
            this.grpDBTests.Controls.Add(this.label3);
            this.grpDBTests.Controls.Add(this.cmbComparison);
            this.grpDBTests.Controls.Add(this.label2);
            this.grpDBTests.Controls.Add(this.label1);
            this.grpDBTests.Controls.Add(this.cmbFields);
            this.grpDBTests.Controls.Add(this.dgvResultSet);
            this.grpDBTests.Controls.Add(this.cmbTable);
            this.grpDBTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDBTests.Location = new System.Drawing.Point(0, 0);
            this.grpDBTests.Name = "grpDBTests";
            this.grpDBTests.Size = new System.Drawing.Size(655, 500);
            this.grpDBTests.TabIndex = 0;
            this.grpDBTests.TabStop = false;
            this.grpDBTests.Text = "Database Tests";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(125, 18);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(143, 23);
            this.btnRefresh.TabIndex = 32;
            this.btnRefresh.Text = "Refresh Interface";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // pnlModified
            // 
            this.pnlModified.BackColor = System.Drawing.Color.Red;
            this.pnlModified.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlModified.Location = new System.Drawing.Point(6, 400);
            this.pnlModified.Name = "pnlModified";
            this.pnlModified.Size = new System.Drawing.Size(21, 21);
            this.pnlModified.TabIndex = 31;
            this.pnlModified.Visible = false;
            // 
            // btnReport
            // 
            this.btnReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReport.Location = new System.Drawing.Point(629, 476);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new System.Drawing.Size(19, 19);
            this.btnReport.TabIndex = 30;
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblMessage.Location = new System.Drawing.Point(3, 474);
            this.lblMessage.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(649, 23);
            this.lblMessage.TabIndex = 29;
            // 
            // lblItemType
            // 
            this.lblItemType.AutoSize = true;
            this.lblItemType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblItemType.Location = new System.Drawing.Point(399, 18);
            this.lblItemType.Name = "lblItemType";
            this.lblItemType.Size = new System.Drawing.Size(15, 19);
            this.lblItemType.TabIndex = 28;
            this.lblItemType.Text = "-";
            // 
            // grpTableActions
            // 
            this.grpTableActions.Controls.Add(this.chkTransactional);
            this.grpTableActions.Controls.Add(this.chkFillReferences);
            this.grpTableActions.Controls.Add(this.chkFilterReferences);
            this.grpTableActions.Controls.Add(this.chkCascadeTableActions);
            this.grpTableActions.Controls.Add(this.btnDeleteFiltered);
            this.grpTableActions.Controls.Add(this.btnFetch);
            this.grpTableActions.Controls.Add(this.btnBulkInsertOrUpdate);
            this.grpTableActions.Controls.Add(this.btnBulkInsert);
            this.grpTableActions.Controls.Add(this.btnNew);
            this.grpTableActions.Controls.Add(this.btnSelectFiltered);
            this.grpTableActions.Controls.Add(this.btnSelectAll);
            this.grpTableActions.Location = new System.Drawing.Point(6, 280);
            this.grpTableActions.Name = "grpTableActions";
            this.grpTableActions.Size = new System.Drawing.Size(414, 114);
            this.grpTableActions.TabIndex = 26;
            this.grpTableActions.TabStop = false;
            this.grpTableActions.Text = "Table Actions";
            // 
            // chkTransactional
            // 
            this.chkTransactional.AutoSize = true;
            this.chkTransactional.Location = new System.Drawing.Point(178, 87);
            this.chkTransactional.Name = "chkTransactional";
            this.chkTransactional.Size = new System.Drawing.Size(116, 21);
            this.chkTransactional.TabIndex = 33;
            this.chkTransactional.Text = "Transactional";
            this.chkTransactional.UseVisualStyleBackColor = true;
            // 
            // chkFillReferences
            // 
            this.chkFillReferences.AutoSize = true;
            this.chkFillReferences.Location = new System.Drawing.Point(6, 87);
            this.chkFillReferences.Name = "chkFillReferences";
            this.chkFillReferences.Size = new System.Drawing.Size(47, 21);
            this.chkFillReferences.TabIndex = 32;
            this.chkFillReferences.Text = "Fill";
            this.chkFillReferences.UseVisualStyleBackColor = true;
            // 
            // chkFilterReferences
            // 
            this.chkFilterReferences.AutoSize = true;
            this.chkFilterReferences.Location = new System.Drawing.Point(86, 87);
            this.chkFilterReferences.Name = "chkFilterReferences";
            this.chkFilterReferences.Size = new System.Drawing.Size(61, 21);
            this.chkFilterReferences.TabIndex = 31;
            this.chkFilterReferences.Text = "Filter";
            this.chkFilterReferences.UseVisualStyleBackColor = true;
            // 
            // chkCascadeTableActions
            // 
            this.chkCascadeTableActions.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.chkCascadeTableActions.AutoSize = true;
            this.chkCascadeTableActions.Location = new System.Drawing.Point(323, 87);
            this.chkCascadeTableActions.Name = "chkCascadeTableActions";
            this.chkCascadeTableActions.Size = new System.Drawing.Size(85, 21);
            this.chkCascadeTableActions.TabIndex = 30;
            this.chkCascadeTableActions.Text = "Cascade";
            this.chkCascadeTableActions.UseVisualStyleBackColor = true;
            // 
            // btnDeleteFiltered
            // 
            this.btnDeleteFiltered.Location = new System.Drawing.Point(273, 52);
            this.btnDeleteFiltered.Name = "btnDeleteFiltered";
            this.btnDeleteFiltered.Size = new System.Drawing.Size(117, 23);
            this.btnDeleteFiltered.TabIndex = 29;
            this.btnDeleteFiltered.Text = "Delete Filtered";
            this.btnDeleteFiltered.UseVisualStyleBackColor = true;
            this.btnDeleteFiltered.Click += new System.EventHandler(this.btnDeleteFiltered_Click);
            // 
            // btnFetch
            // 
            this.btnFetch.Location = new System.Drawing.Point(200, 23);
            this.btnFetch.Name = "btnFetch";
            this.btnFetch.Size = new System.Drawing.Size(94, 23);
            this.btnFetch.TabIndex = 28;
            this.btnFetch.Text = "Fetch Rows";
            this.btnFetch.UseVisualStyleBackColor = true;
            this.btnFetch.Click += new System.EventHandler(this.btnFetch_Click);
            // 
            // btnBulkInsertOrUpdate
            // 
            this.btnBulkInsertOrUpdate.Location = new System.Drawing.Point(103, 52);
            this.btnBulkInsertOrUpdate.Name = "btnBulkInsertOrUpdate";
            this.btnBulkInsertOrUpdate.Size = new System.Drawing.Size(164, 23);
            this.btnBulkInsertOrUpdate.TabIndex = 27;
            this.btnBulkInsertOrUpdate.Text = "Bulk Insert Or Update";
            this.btnBulkInsertOrUpdate.UseVisualStyleBackColor = true;
            this.btnBulkInsertOrUpdate.Click += new System.EventHandler(this.btnBulkInsertOrUpdate_Click);
            // 
            // btnBulkInsert
            // 
            this.btnBulkInsert.Location = new System.Drawing.Point(6, 52);
            this.btnBulkInsert.Name = "btnBulkInsert";
            this.btnBulkInsert.Size = new System.Drawing.Size(91, 23);
            this.btnBulkInsert.TabIndex = 26;
            this.btnBulkInsert.Text = "Bulk Insert";
            this.btnBulkInsert.UseVisualStyleBackColor = true;
            this.btnBulkInsert.Click += new System.EventHandler(this.btnBulkInsert_Click);
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(300, 23);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(48, 23);
            this.btnNew.TabIndex = 25;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnSelectFiltered
            // 
            this.btnSelectFiltered.Location = new System.Drawing.Point(86, 23);
            this.btnSelectFiltered.Name = "btnSelectFiltered";
            this.btnSelectFiltered.Size = new System.Drawing.Size(108, 23);
            this.btnSelectFiltered.TabIndex = 24;
            this.btnSelectFiltered.Text = "Select Filtered";
            this.btnSelectFiltered.UseVisualStyleBackColor = true;
            this.btnSelectFiltered.Click += new System.EventHandler(this.btnSelectFiltered_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(6, 23);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelectAll.TabIndex = 23;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 164);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 17);
            this.label6.TabIndex = 25;
            this.label6.Text = "Value";
            // 
            // grpItemActions
            // 
            this.grpItemActions.Controls.Add(this.chkCascadeItemActions);
            this.grpItemActions.Controls.Add(this.btnUpdate);
            this.grpItemActions.Controls.Add(this.btnInsertOrUpdate);
            this.grpItemActions.Controls.Add(this.btnInsert);
            this.grpItemActions.Controls.Add(this.btnDelete);
            this.grpItemActions.Location = new System.Drawing.Point(425, 280);
            this.grpItemActions.Name = "grpItemActions";
            this.grpItemActions.Size = new System.Drawing.Size(223, 114);
            this.grpItemActions.TabIndex = 23;
            this.grpItemActions.TabStop = false;
            this.grpItemActions.Text = "Item Actions";
            // 
            // chkCascadeItemActions
            // 
            this.chkCascadeItemActions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCascadeItemActions.AutoSize = true;
            this.chkCascadeItemActions.Location = new System.Drawing.Point(82, 87);
            this.chkCascadeItemActions.Name = "chkCascadeItemActions";
            this.chkCascadeItemActions.Size = new System.Drawing.Size(135, 21);
            this.chkCascadeItemActions.TabIndex = 28;
            this.chkCascadeItemActions.Text = "Cascade Actions";
            this.chkCascadeItemActions.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(88, 23);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnUpdate.TabIndex = 27;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnInsertOrUpdate
            // 
            this.btnInsertOrUpdate.Location = new System.Drawing.Point(7, 52);
            this.btnInsertOrUpdate.Name = "btnInsertOrUpdate";
            this.btnInsertOrUpdate.Size = new System.Drawing.Size(125, 23);
            this.btnInsertOrUpdate.TabIndex = 26;
            this.btnInsertOrUpdate.Text = "Insert Or Update";
            this.btnInsertOrUpdate.UseVisualStyleBackColor = true;
            this.btnInsertOrUpdate.Click += new System.EventHandler(this.btnInsertOrUpdate_Click);
            // 
            // btnInsert
            // 
            this.btnInsert.Location = new System.Drawing.Point(7, 23);
            this.btnInsert.Name = "btnInsert";
            this.btnInsert.Size = new System.Drawing.Size(75, 23);
            this.btnInsert.TabIndex = 25;
            this.btnInsert.Text = "Insert";
            this.btnInsert.UseVisualStyleBackColor = true;
            this.btnInsert.Click += new System.EventHandler(this.btnInsert_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(138, 54);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 24;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // txtRowCount
            // 
            this.txtRowCount.Location = new System.Drawing.Point(119, 217);
            this.txtRowCount.Name = "txtRowCount";
            this.txtRowCount.Size = new System.Drawing.Size(100, 22);
            this.txtRowCount.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 220);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 17);
            this.label5.TabIndex = 20;
            this.label5.Text = "Row Count";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 192);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 17);
            this.label4.TabIndex = 19;
            this.label4.Text = "Row Offset";
            // 
            // txtRowOffset
            // 
            this.txtRowOffset.Location = new System.Drawing.Point(119, 189);
            this.txtRowOffset.Name = "txtRowOffset";
            this.txtRowOffset.Size = new System.Drawing.Size(100, 22);
            this.txtRowOffset.TabIndex = 18;
            // 
            // pgrItemEditor
            // 
            this.pgrItemEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgrItemEditor.HelpVisible = false;
            this.pgrItemEditor.Location = new System.Drawing.Point(317, 15);
            this.pgrItemEditor.Name = "pgrItemEditor";
            this.pgrItemEditor.Size = new System.Drawing.Size(332, 232);
            this.pgrItemEditor.TabIndex = 10;
            // 
            // txtFilterValue
            // 
            this.txtFilterValue.Location = new System.Drawing.Point(119, 161);
            this.txtFilterValue.Name = "txtFilterValue";
            this.txtFilterValue.Size = new System.Drawing.Size(149, 22);
            this.txtFilterValue.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Comparison";
            // 
            // cmbComparison
            // 
            this.cmbComparison.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbComparison.FormattingEnabled = true;
            this.cmbComparison.Location = new System.Drawing.Point(119, 131);
            this.cmbComparison.Name = "cmbComparison";
            this.cmbComparison.Size = new System.Drawing.Size(149, 24);
            this.cmbComparison.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Field";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Table";
            // 
            // cmbFields
            // 
            this.cmbFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFields.FormattingEnabled = true;
            this.cmbFields.Location = new System.Drawing.Point(6, 101);
            this.cmbFields.Name = "cmbFields";
            this.cmbFields.Size = new System.Drawing.Size(262, 24);
            this.cmbFields.TabIndex = 3;
            // 
            // dgvResultSet
            // 
            this.dgvResultSet.AllowUserToAddRows = false;
            this.dgvResultSet.AllowUserToDeleteRows = false;
            this.dgvResultSet.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvResultSet.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResultSet.Location = new System.Drawing.Point(6, 400);
            this.dgvResultSet.Name = "dgvResultSet";
            this.dgvResultSet.ReadOnly = true;
            this.dgvResultSet.RowTemplate.Height = 24;
            this.dgvResultSet.Size = new System.Drawing.Size(643, 69);
            this.dgvResultSet.TabIndex = 2;
            // 
            // cmbTable
            // 
            this.cmbTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTable.FormattingEnabled = true;
            this.cmbTable.Location = new System.Drawing.Point(7, 48);
            this.cmbTable.Name = "cmbTable";
            this.cmbTable.Size = new System.Drawing.Size(261, 24);
            this.cmbTable.TabIndex = 0;
            // 
            // chkLimitResults
            // 
            this.chkLimitResults.AutoSize = true;
            this.chkLimitResults.Checked = true;
            this.chkLimitResults.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitResults.Location = new System.Drawing.Point(9, 250);
            this.chkLimitResults.Name = "chkLimitResults";
            this.chkLimitResults.Size = new System.Drawing.Size(213, 21);
            this.chkLimitResults.TabIndex = 33;
            this.chkLimitResults.Text = "Max 10.000 displayed results";
            this.chkLimitResults.UseVisualStyleBackColor = true;
            // 
            // DatabaseTests
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpDBTests);
            this.MinimumSize = new System.Drawing.Size(655, 500);
            this.Name = "DatabaseTests";
            this.Size = new System.Drawing.Size(655, 500);
            this.grpDBTests.ResumeLayout(false);
            this.grpDBTests.PerformLayout();
            this.grpTableActions.ResumeLayout(false);
            this.grpTableActions.PerformLayout();
            this.grpItemActions.ResumeLayout(false);
            this.grpItemActions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResultSet)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDBTests;
        private System.Windows.Forms.DataGridView dgvResultSet;
        private System.Windows.Forms.ComboBox cmbTable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbFields;
        private System.Windows.Forms.PropertyGrid pgrItemEditor;
        private System.Windows.Forms.TextBox txtFilterValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbComparison;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRowCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtRowOffset;
        private System.Windows.Forms.GroupBox grpItemActions;
        private System.Windows.Forms.CheckBox chkCascadeItemActions;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnInsertOrUpdate;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox grpTableActions;
        private System.Windows.Forms.Button btnDeleteFiltered;
        private System.Windows.Forms.Button btnFetch;
        private System.Windows.Forms.Button btnBulkInsertOrUpdate;
        private System.Windows.Forms.Button btnBulkInsert;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnSelectFiltered;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.CheckBox chkCascadeTableActions;
        private System.Windows.Forms.Label lblItemType;
        private System.Windows.Forms.CheckBox chkFillReferences;
        private System.Windows.Forms.CheckBox chkFilterReferences;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Panel pnlModified;
        private System.Windows.Forms.CheckBox chkTransactional;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox chkLimitResults;
    }
}
