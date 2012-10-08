namespace OpenNETCF.ORM.MainDemo.Common
{
    partial class DynamicEntityTests
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
            this.grpDynamicEntity = new System.Windows.Forms.GroupBox();
            this.btnReport = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.btnReverseEngineering = new System.Windows.Forms.Button();
            this.btnRegister = new System.Windows.Forms.Button();
            this.grpTests = new System.Windows.Forms.GroupBox();
            this.pnlModified = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDeleteFiltered = new System.Windows.Forms.Button();
            this.btnBulkInsertOrUpdate = new System.Windows.Forms.Button();
            this.btnBulkInsert = new System.Windows.Forms.Button();
            this.btnDropAndCreate = new System.Windows.Forms.Button();
            this.btnCount = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnSelectFilter = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.grpItemActions = new System.Windows.Forms.GroupBox();
            this.chkCascadeItemActions = new System.Windows.Forms.CheckBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnInsertOrUpdate = new System.Windows.Forms.Button();
            this.btnInsert = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.chkLimitResults = new System.Windows.Forms.CheckBox();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtFilterValue = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbComparison = new System.Windows.Forms.ComboBox();
            this.cmbFields = new System.Windows.Forms.ComboBox();
            this.pgrItemEditor = new System.Windows.Forms.PropertyGrid();
            this.dgvDynamicEntities = new System.Windows.Forms.DataGridView();
            this.grpNewEntity = new System.Windows.Forms.GroupBox();
            this.btnAddEntity = new System.Windows.Forms.Button();
            this.txtEntityName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.grpNewField = new System.Windows.Forms.GroupBox();
            this.btnAddField = new System.Windows.Forms.Button();
            this.numFieldLength = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.chkIndexedField = new System.Windows.Forms.CheckBox();
            this.chkUniqueValue = new System.Windows.Forms.CheckBox();
            this.chkPrimaryKey = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbFieldType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtFieldName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbDynamicEntities = new System.Windows.Forms.ComboBox();
            this.lstFields = new System.Windows.Forms.ListBox();
            this.chkTransactional = new System.Windows.Forms.CheckBox();
            this.grpDynamicEntity.SuspendLayout();
            this.grpTests.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpItemActions.SuspendLayout();
            this.grpFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDynamicEntities)).BeginInit();
            this.grpNewEntity.SuspendLayout();
            this.grpNewField.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFieldLength)).BeginInit();
            this.SuspendLayout();
            // 
            // grpDynamicEntity
            // 
            this.grpDynamicEntity.Controls.Add(this.btnReport);
            this.grpDynamicEntity.Controls.Add(this.lblMessage);
            this.grpDynamicEntity.Controls.Add(this.btnReverseEngineering);
            this.grpDynamicEntity.Controls.Add(this.btnRegister);
            this.grpDynamicEntity.Controls.Add(this.grpTests);
            this.grpDynamicEntity.Controls.Add(this.grpNewEntity);
            this.grpDynamicEntity.Controls.Add(this.grpNewField);
            this.grpDynamicEntity.Controls.Add(this.label2);
            this.grpDynamicEntity.Controls.Add(this.label1);
            this.grpDynamicEntity.Controls.Add(this.cmbDynamicEntities);
            this.grpDynamicEntity.Controls.Add(this.lstFields);
            this.grpDynamicEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDynamicEntity.Location = new System.Drawing.Point(0, 0);
            this.grpDynamicEntity.Name = "grpDynamicEntity";
            this.grpDynamicEntity.Size = new System.Drawing.Size(826, 535);
            this.grpDynamicEntity.TabIndex = 0;
            this.grpDynamicEntity.TabStop = false;
            this.grpDynamicEntity.Text = "Dynamic Entity Tests";
            // 
            // btnReport
            // 
            this.btnReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReport.Location = new System.Drawing.Point(793, 506);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new System.Drawing.Size(23, 23);
            this.btnReport.TabIndex = 10;
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblMessage.Location = new System.Drawing.Point(5, 506);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(810, 23);
            this.lblMessage.TabIndex = 9;
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnReverseEngineering
            // 
            this.btnReverseEngineering.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReverseEngineering.Location = new System.Drawing.Point(6, 225);
            this.btnReverseEngineering.Name = "btnReverseEngineering";
            this.btnReverseEngineering.Size = new System.Drawing.Size(82, 23);
            this.btnReverseEngineering.TabIndex = 8;
            this.btnReverseEngineering.Text = "Reverse";
            this.btnReverseEngineering.UseVisualStyleBackColor = true;
            this.btnReverseEngineering.Click += new System.EventHandler(this.btnReverseEngineering_Click);
            // 
            // btnRegister
            // 
            this.btnRegister.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRegister.Enabled = false;
            this.btnRegister.Location = new System.Drawing.Point(125, 225);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(82, 23);
            this.btnRegister.TabIndex = 7;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // grpTests
            // 
            this.grpTests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTests.Controls.Add(this.pnlModified);
            this.grpTests.Controls.Add(this.groupBox1);
            this.grpTests.Controls.Add(this.grpItemActions);
            this.grpTests.Controls.Add(this.chkLimitResults);
            this.grpTests.Controls.Add(this.grpFilter);
            this.grpTests.Controls.Add(this.pgrItemEditor);
            this.grpTests.Controls.Add(this.dgvDynamicEntities);
            this.grpTests.Location = new System.Drawing.Point(213, 10);
            this.grpTests.Name = "grpTests";
            this.grpTests.Size = new System.Drawing.Size(602, 488);
            this.grpTests.TabIndex = 6;
            this.grpTests.TabStop = false;
            // 
            // pnlModified
            // 
            this.pnlModified.BackColor = System.Drawing.Color.Red;
            this.pnlModified.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlModified.Location = new System.Drawing.Point(6, 312);
            this.pnlModified.Name = "pnlModified";
            this.pnlModified.Size = new System.Drawing.Size(21, 21);
            this.pnlModified.TabIndex = 51;
            this.pnlModified.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkTransactional);
            this.groupBox1.Controls.Add(this.btnDeleteFiltered);
            this.groupBox1.Controls.Add(this.btnBulkInsertOrUpdate);
            this.groupBox1.Controls.Add(this.btnBulkInsert);
            this.groupBox1.Controls.Add(this.btnDropAndCreate);
            this.groupBox1.Controls.Add(this.btnCount);
            this.groupBox1.Controls.Add(this.btnNew);
            this.groupBox1.Controls.Add(this.btnSelectFilter);
            this.groupBox1.Controls.Add(this.btnSelectAll);
            this.groupBox1.Location = new System.Drawing.Point(6, 192);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(363, 114);
            this.groupBox1.TabIndex = 50;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            // 
            // btnDeleteFiltered
            // 
            this.btnDeleteFiltered.Location = new System.Drawing.Point(6, 73);
            this.btnDeleteFiltered.Name = "btnDeleteFiltered";
            this.btnDeleteFiltered.Size = new System.Drawing.Size(108, 23);
            this.btnDeleteFiltered.TabIndex = 59;
            this.btnDeleteFiltered.Text = "Delete Filtered";
            this.btnDeleteFiltered.UseVisualStyleBackColor = true;
            this.btnDeleteFiltered.Click += new System.EventHandler(this.btnDeleteFiltered_Click);
            // 
            // btnBulkInsertOrUpdate
            // 
            this.btnBulkInsertOrUpdate.Location = new System.Drawing.Point(201, 47);
            this.btnBulkInsertOrUpdate.Name = "btnBulkInsertOrUpdate";
            this.btnBulkInsertOrUpdate.Size = new System.Drawing.Size(155, 23);
            this.btnBulkInsertOrUpdate.TabIndex = 58;
            this.btnBulkInsertOrUpdate.Text = "Bulk Insert or Update";
            this.btnBulkInsertOrUpdate.UseVisualStyleBackColor = true;
            this.btnBulkInsertOrUpdate.Click += new System.EventHandler(this.btnBulkInsertOrUpdate_Click);
            // 
            // btnBulkInsert
            // 
            this.btnBulkInsert.Location = new System.Drawing.Point(113, 47);
            this.btnBulkInsert.Name = "btnBulkInsert";
            this.btnBulkInsert.Size = new System.Drawing.Size(82, 23);
            this.btnBulkInsert.TabIndex = 57;
            this.btnBulkInsert.Text = "Bulk Insert";
            this.btnBulkInsert.UseVisualStyleBackColor = true;
            this.btnBulkInsert.Click += new System.EventHandler(this.btnBulkInsert_Click);
            // 
            // btnDropAndCreate
            // 
            this.btnDropAndCreate.Location = new System.Drawing.Point(6, 47);
            this.btnDropAndCreate.Name = "btnDropAndCreate";
            this.btnDropAndCreate.Size = new System.Drawing.Size(101, 23);
            this.btnDropAndCreate.TabIndex = 56;
            this.btnDropAndCreate.Text = "Drop/Create";
            this.btnDropAndCreate.UseVisualStyleBackColor = true;
            this.btnDropAndCreate.Click += new System.EventHandler(this.btnDropAndCreate_Click);
            // 
            // btnCount
            // 
            this.btnCount.Location = new System.Drawing.Point(256, 20);
            this.btnCount.Name = "btnCount";
            this.btnCount.Size = new System.Drawing.Size(57, 23);
            this.btnCount.TabIndex = 55;
            this.btnCount.Text = "Count";
            this.btnCount.UseVisualStyleBackColor = true;
            this.btnCount.Click += new System.EventHandler(this.btnCount_Click);
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(201, 21);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(49, 23);
            this.btnNew.TabIndex = 54;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnSelectFilter
            // 
            this.btnSelectFilter.Location = new System.Drawing.Point(87, 21);
            this.btnSelectFilter.Name = "btnSelectFilter";
            this.btnSelectFilter.Size = new System.Drawing.Size(108, 23);
            this.btnSelectFilter.TabIndex = 53;
            this.btnSelectFilter.Text = "Select Filtered";
            this.btnSelectFilter.UseVisualStyleBackColor = true;
            this.btnSelectFilter.Click += new System.EventHandler(this.btnSelectFilter_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(6, 21);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelectAll.TabIndex = 52;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // grpItemActions
            // 
            this.grpItemActions.Controls.Add(this.chkCascadeItemActions);
            this.grpItemActions.Controls.Add(this.btnUpdate);
            this.grpItemActions.Controls.Add(this.btnInsertOrUpdate);
            this.grpItemActions.Controls.Add(this.btnInsert);
            this.grpItemActions.Controls.Add(this.btnDelete);
            this.grpItemActions.Location = new System.Drawing.Point(373, 192);
            this.grpItemActions.Name = "grpItemActions";
            this.grpItemActions.Size = new System.Drawing.Size(223, 114);
            this.grpItemActions.TabIndex = 49;
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
            // chkLimitResults
            // 
            this.chkLimitResults.AutoSize = true;
            this.chkLimitResults.Checked = true;
            this.chkLimitResults.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitResults.Location = new System.Drawing.Point(6, 165);
            this.chkLimitResults.Name = "chkLimitResults";
            this.chkLimitResults.Size = new System.Drawing.Size(213, 21);
            this.chkLimitResults.TabIndex = 40;
            this.chkLimitResults.Text = "Max 10.000 displayed results";
            this.chkLimitResults.UseVisualStyleBackColor = true;
            // 
            // grpFilter
            // 
            this.grpFilter.Controls.Add(this.label7);
            this.grpFilter.Controls.Add(this.txtFilterValue);
            this.grpFilter.Controls.Add(this.label8);
            this.grpFilter.Controls.Add(this.cmbComparison);
            this.grpFilter.Controls.Add(this.cmbFields);
            this.grpFilter.Location = new System.Drawing.Point(6, 11);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Size = new System.Drawing.Size(285, 148);
            this.grpFilter.TabIndex = 5;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "Filter";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 84);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 17);
            this.label7.TabIndex = 38;
            this.label7.Text = "Value";
            // 
            // txtFilterValue
            // 
            this.txtFilterValue.Location = new System.Drawing.Point(126, 81);
            this.txtFilterValue.Name = "txtFilterValue";
            this.txtFilterValue.Size = new System.Drawing.Size(149, 22);
            this.txtFilterValue.TabIndex = 37;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 54);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(83, 17);
            this.label8.TabIndex = 36;
            this.label8.Text = "Comparison";
            // 
            // cmbComparison
            // 
            this.cmbComparison.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbComparison.FormattingEnabled = true;
            this.cmbComparison.Location = new System.Drawing.Point(126, 51);
            this.cmbComparison.Name = "cmbComparison";
            this.cmbComparison.Size = new System.Drawing.Size(149, 24);
            this.cmbComparison.TabIndex = 35;
            // 
            // cmbFields
            // 
            this.cmbFields.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFields.FormattingEnabled = true;
            this.cmbFields.Location = new System.Drawing.Point(13, 21);
            this.cmbFields.Name = "cmbFields";
            this.cmbFields.Size = new System.Drawing.Size(262, 24);
            this.cmbFields.TabIndex = 34;
            // 
            // pgrItemEditor
            // 
            this.pgrItemEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgrItemEditor.HelpVisible = false;
            this.pgrItemEditor.Location = new System.Drawing.Point(297, 11);
            this.pgrItemEditor.Name = "pgrItemEditor";
            this.pgrItemEditor.Size = new System.Drawing.Size(299, 175);
            this.pgrItemEditor.TabIndex = 1;
            // 
            // dgvDynamicEntities
            // 
            this.dgvDynamicEntities.AllowUserToAddRows = false;
            this.dgvDynamicEntities.AllowUserToDeleteRows = false;
            this.dgvDynamicEntities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDynamicEntities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDynamicEntities.Location = new System.Drawing.Point(6, 312);
            this.dgvDynamicEntities.Name = "dgvDynamicEntities";
            this.dgvDynamicEntities.ReadOnly = true;
            this.dgvDynamicEntities.RowTemplate.Height = 24;
            this.dgvDynamicEntities.Size = new System.Drawing.Size(590, 170);
            this.dgvDynamicEntities.TabIndex = 0;
            // 
            // grpNewEntity
            // 
            this.grpNewEntity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.grpNewEntity.Controls.Add(this.btnAddEntity);
            this.grpNewEntity.Controls.Add(this.txtEntityName);
            this.grpNewEntity.Controls.Add(this.label6);
            this.grpNewEntity.Location = new System.Drawing.Point(6, 258);
            this.grpNewEntity.Name = "grpNewEntity";
            this.grpNewEntity.Size = new System.Drawing.Size(197, 76);
            this.grpNewEntity.TabIndex = 5;
            this.grpNewEntity.TabStop = false;
            this.grpNewEntity.Text = "Create Entity";
            // 
            // btnAddEntity
            // 
            this.btnAddEntity.Location = new System.Drawing.Point(6, 47);
            this.btnAddEntity.Name = "btnAddEntity";
            this.btnAddEntity.Size = new System.Drawing.Size(185, 24);
            this.btnAddEntity.TabIndex = 10;
            this.btnAddEntity.Text = "Create Entity";
            this.btnAddEntity.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnAddEntity.UseVisualStyleBackColor = true;
            this.btnAddEntity.Click += new System.EventHandler(this.btnAddEntity_Click);
            // 
            // txtEntityName
            // 
            this.txtEntityName.Location = new System.Drawing.Point(58, 19);
            this.txtEntityName.Name = "txtEntityName";
            this.txtEntityName.Size = new System.Drawing.Size(133, 22);
            this.txtEntityName.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(45, 17);
            this.label6.TabIndex = 0;
            this.label6.Text = "Name";
            // 
            // grpNewField
            // 
            this.grpNewField.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.grpNewField.Controls.Add(this.btnAddField);
            this.grpNewField.Controls.Add(this.numFieldLength);
            this.grpNewField.Controls.Add(this.label5);
            this.grpNewField.Controls.Add(this.chkIndexedField);
            this.grpNewField.Controls.Add(this.chkUniqueValue);
            this.grpNewField.Controls.Add(this.chkPrimaryKey);
            this.grpNewField.Controls.Add(this.label4);
            this.grpNewField.Controls.Add(this.cmbFieldType);
            this.grpNewField.Controls.Add(this.label3);
            this.grpNewField.Controls.Add(this.txtFieldName);
            this.grpNewField.Location = new System.Drawing.Point(3, 340);
            this.grpNewField.Name = "grpNewField";
            this.grpNewField.Size = new System.Drawing.Size(200, 158);
            this.grpNewField.TabIndex = 4;
            this.grpNewField.TabStop = false;
            this.grpNewField.Text = "Create Field";
            // 
            // btnAddField
            // 
            this.btnAddField.Location = new System.Drawing.Point(9, 128);
            this.btnAddField.Name = "btnAddField";
            this.btnAddField.Size = new System.Drawing.Size(185, 24);
            this.btnAddField.TabIndex = 9;
            this.btnAddField.Text = "Create Field";
            this.btnAddField.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnAddField.UseVisualStyleBackColor = true;
            this.btnAddField.Click += new System.EventHandler(this.btnAddField_Click);
            // 
            // numFieldLength
            // 
            this.numFieldLength.Location = new System.Drawing.Point(61, 73);
            this.numFieldLength.Maximum = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            this.numFieldLength.Name = "numFieldLength";
            this.numFieldLength.Size = new System.Drawing.Size(132, 22);
            this.numFieldLength.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 17);
            this.label5.TabIndex = 7;
            this.label5.Text = "Length";
            // 
            // chkIndexedField
            // 
            this.chkIndexedField.AutoSize = true;
            this.chkIndexedField.Location = new System.Drawing.Point(124, 101);
            this.chkIndexedField.Name = "chkIndexedField";
            this.chkIndexedField.Size = new System.Drawing.Size(63, 21);
            this.chkIndexedField.TabIndex = 6;
            this.chkIndexedField.Text = "Index";
            this.chkIndexedField.UseVisualStyleBackColor = true;
            // 
            // chkUniqueValue
            // 
            this.chkUniqueValue.AutoSize = true;
            this.chkUniqueValue.Location = new System.Drawing.Point(63, 101);
            this.chkUniqueValue.Name = "chkUniqueValue";
            this.chkUniqueValue.Size = new System.Drawing.Size(55, 21);
            this.chkUniqueValue.TabIndex = 5;
            this.chkUniqueValue.Text = "Uni.";
            this.chkUniqueValue.UseVisualStyleBackColor = true;
            // 
            // chkPrimaryKey
            // 
            this.chkPrimaryKey.AutoSize = true;
            this.chkPrimaryKey.Location = new System.Drawing.Point(9, 101);
            this.chkPrimaryKey.Name = "chkPrimaryKey";
            this.chkPrimaryKey.Size = new System.Drawing.Size(48, 21);
            this.chkPrimaryKey.TabIndex = 4;
            this.chkPrimaryKey.Text = "PK";
            this.chkPrimaryKey.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "Type";
            // 
            // cmbFieldType
            // 
            this.cmbFieldType.FormattingEnabled = true;
            this.cmbFieldType.Location = new System.Drawing.Point(61, 43);
            this.cmbFieldType.Name = "cmbFieldType";
            this.cmbFieldType.Size = new System.Drawing.Size(133, 24);
            this.cmbFieldType.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(45, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "Name";
            // 
            // txtFieldName
            // 
            this.txtFieldName.Location = new System.Drawing.Point(61, 15);
            this.txtFieldName.Name = "txtFieldName";
            this.txtFieldName.Size = new System.Drawing.Size(133, 22);
            this.txtFieldName.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Fields";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Entities";
            // 
            // cmbDynamicEntities
            // 
            this.cmbDynamicEntities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDynamicEntities.FormattingEnabled = true;
            this.cmbDynamicEntities.Location = new System.Drawing.Point(6, 50);
            this.cmbDynamicEntities.Name = "cmbDynamicEntities";
            this.cmbDynamicEntities.Size = new System.Drawing.Size(201, 24);
            this.cmbDynamicEntities.TabIndex = 1;
            // 
            // lstFields
            // 
            this.lstFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstFields.FormattingEnabled = true;
            this.lstFields.ItemHeight = 16;
            this.lstFields.Location = new System.Drawing.Point(6, 97);
            this.lstFields.Name = "lstFields";
            this.lstFields.Size = new System.Drawing.Size(201, 116);
            this.lstFields.TabIndex = 0;
            // 
            // chkTransactional
            // 
            this.chkTransactional.AutoSize = true;
            this.chkTransactional.Location = new System.Drawing.Point(247, 87);
            this.chkTransactional.Name = "chkTransactional";
            this.chkTransactional.Size = new System.Drawing.Size(116, 21);
            this.chkTransactional.TabIndex = 60;
            this.chkTransactional.Text = "Transactional";
            this.chkTransactional.UseVisualStyleBackColor = true;
            // 
            // DynamicEntityTests
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpDynamicEntity);
            this.Name = "DynamicEntityTests";
            this.Size = new System.Drawing.Size(826, 535);
            this.grpDynamicEntity.ResumeLayout(false);
            this.grpDynamicEntity.PerformLayout();
            this.grpTests.ResumeLayout(false);
            this.grpTests.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpItemActions.ResumeLayout(false);
            this.grpItemActions.PerformLayout();
            this.grpFilter.ResumeLayout(false);
            this.grpFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDynamicEntities)).EndInit();
            this.grpNewEntity.ResumeLayout(false);
            this.grpNewEntity.PerformLayout();
            this.grpNewField.ResumeLayout(false);
            this.grpNewField.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFieldLength)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDynamicEntity;
        private System.Windows.Forms.ListBox lstFields;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbDynamicEntities;
        private System.Windows.Forms.GroupBox grpNewField;
        private System.Windows.Forms.CheckBox chkIndexedField;
        private System.Windows.Forms.CheckBox chkUniqueValue;
        private System.Windows.Forms.CheckBox chkPrimaryKey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbFieldType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtFieldName;
        private System.Windows.Forms.Button btnAddField;
        private System.Windows.Forms.NumericUpDown numFieldLength;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox grpNewEntity;
        private System.Windows.Forms.Button btnAddEntity;
        private System.Windows.Forms.TextBox txtEntityName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox grpTests;
        private System.Windows.Forms.DataGridView dgvDynamicEntities;
        private System.Windows.Forms.PropertyGrid pgrItemEditor;
        private System.Windows.Forms.CheckBox chkLimitResults;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtFilterValue;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbComparison;
        private System.Windows.Forms.ComboBox cmbFields;
        private System.Windows.Forms.GroupBox grpItemActions;
        private System.Windows.Forms.CheckBox chkCascadeItemActions;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnInsertOrUpdate;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnDeleteFiltered;
        private System.Windows.Forms.Button btnBulkInsertOrUpdate;
        private System.Windows.Forms.Button btnBulkInsert;
        private System.Windows.Forms.Button btnDropAndCreate;
        private System.Windows.Forms.Button btnCount;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnSelectFilter;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnReverseEngineering;
        private System.Windows.Forms.Panel pnlModified;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.CheckBox chkTransactional;
    }
}
