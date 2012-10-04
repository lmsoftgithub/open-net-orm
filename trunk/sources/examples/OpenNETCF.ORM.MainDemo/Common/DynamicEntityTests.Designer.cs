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
            this.grpTests = new System.Windows.Forms.GroupBox();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
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
            this.listFields = new System.Windows.Forms.ListBox();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnSelectFilter = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtFilterValue = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbComparison = new System.Windows.Forms.ComboBox();
            this.cmbFields = new System.Windows.Forms.ComboBox();
            this.chkLimitResults = new System.Windows.Forms.CheckBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.grpDynamicEntity.SuspendLayout();
            this.grpTests.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDynamicEntities)).BeginInit();
            this.grpNewEntity.SuspendLayout();
            this.grpNewField.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFieldLength)).BeginInit();
            this.grpFilter.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpDynamicEntity
            // 
            this.grpDynamicEntity.Controls.Add(this.grpTests);
            this.grpDynamicEntity.Controls.Add(this.grpNewEntity);
            this.grpDynamicEntity.Controls.Add(this.grpNewField);
            this.grpDynamicEntity.Controls.Add(this.label2);
            this.grpDynamicEntity.Controls.Add(this.label1);
            this.grpDynamicEntity.Controls.Add(this.cmbDynamicEntities);
            this.grpDynamicEntity.Controls.Add(this.listFields);
            this.grpDynamicEntity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpDynamicEntity.Location = new System.Drawing.Point(0, 0);
            this.grpDynamicEntity.Name = "grpDynamicEntity";
            this.grpDynamicEntity.Size = new System.Drawing.Size(930, 414);
            this.grpDynamicEntity.TabIndex = 0;
            this.grpDynamicEntity.TabStop = false;
            this.grpDynamicEntity.Text = "Dynamic Entity Tests";
            // 
            // grpTests
            // 
            this.grpTests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTests.Controls.Add(this.btnDelete);
            this.grpTests.Controls.Add(this.chkLimitResults);
            this.grpTests.Controls.Add(this.grpFilter);
            this.grpTests.Controls.Add(this.btnNew);
            this.grpTests.Controls.Add(this.btnSelectFilter);
            this.grpTests.Controls.Add(this.btnSelectAll);
            this.grpTests.Controls.Add(this.propertyGrid1);
            this.grpTests.Controls.Add(this.dgvDynamicEntities);
            this.grpTests.Location = new System.Drawing.Point(213, 10);
            this.grpTests.Name = "grpTests";
            this.grpTests.Size = new System.Drawing.Size(706, 395);
            this.grpTests.TabIndex = 6;
            this.grpTests.TabStop = false;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(433, 11);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(267, 222);
            this.propertyGrid1.TabIndex = 1;
            // 
            // dgvDynamicEntities
            // 
            this.dgvDynamicEntities.AllowUserToAddRows = false;
            this.dgvDynamicEntities.AllowUserToDeleteRows = false;
            this.dgvDynamicEntities.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDynamicEntities.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDynamicEntities.Location = new System.Drawing.Point(6, 239);
            this.dgvDynamicEntities.Name = "dgvDynamicEntities";
            this.dgvDynamicEntities.ReadOnly = true;
            this.dgvDynamicEntities.RowTemplate.Height = 24;
            this.dgvDynamicEntities.Size = new System.Drawing.Size(694, 150);
            this.dgvDynamicEntities.TabIndex = 0;
            // 
            // grpNewEntity
            // 
            this.grpNewEntity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.grpNewEntity.Controls.Add(this.btnAddEntity);
            this.grpNewEntity.Controls.Add(this.txtEntityName);
            this.grpNewEntity.Controls.Add(this.label6);
            this.grpNewEntity.Location = new System.Drawing.Point(6, 171);
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
            this.grpNewField.Location = new System.Drawing.Point(3, 253);
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
            // 
            // numFieldLength
            // 
            this.numFieldLength.Location = new System.Drawing.Point(61, 73);
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
            // listFields
            // 
            this.listFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listFields.FormattingEnabled = true;
            this.listFields.ItemHeight = 16;
            this.listFields.Location = new System.Drawing.Point(6, 97);
            this.listFields.Name = "listFields";
            this.listFields.Size = new System.Drawing.Size(201, 68);
            this.listFields.TabIndex = 0;
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(6, 132);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 23);
            this.btnSelectAll.TabIndex = 2;
            this.btnSelectAll.Text = "Select All";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            // 
            // btnSelectFilter
            // 
            this.btnSelectFilter.Location = new System.Drawing.Point(87, 132);
            this.btnSelectFilter.Name = "btnSelectFilter";
            this.btnSelectFilter.Size = new System.Drawing.Size(108, 23);
            this.btnSelectFilter.TabIndex = 3;
            this.btnSelectFilter.Text = "Select Filtered";
            this.btnSelectFilter.UseVisualStyleBackColor = true;
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(201, 132);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(75, 23);
            this.btnNew.TabIndex = 4;
            this.btnNew.Text = "New";
            this.btnNew.UseVisualStyleBackColor = true;
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
            this.grpFilter.Size = new System.Drawing.Size(285, 114);
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
            // chkLimitResults
            // 
            this.chkLimitResults.AutoSize = true;
            this.chkLimitResults.Checked = true;
            this.chkLimitResults.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitResults.Location = new System.Drawing.Point(98, 208);
            this.chkLimitResults.Name = "chkLimitResults";
            this.chkLimitResults.Size = new System.Drawing.Size(213, 21);
            this.chkLimitResults.TabIndex = 40;
            this.chkLimitResults.Text = "Max 10.000 displayed results";
            this.chkLimitResults.UseVisualStyleBackColor = true;
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(6, 161);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 41;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            // 
            // DynamicEntityTests
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpDynamicEntity);
            this.Name = "DynamicEntityTests";
            this.Size = new System.Drawing.Size(930, 414);
            this.grpDynamicEntity.ResumeLayout(false);
            this.grpDynamicEntity.PerformLayout();
            this.grpTests.ResumeLayout(false);
            this.grpTests.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDynamicEntities)).EndInit();
            this.grpNewEntity.ResumeLayout(false);
            this.grpNewEntity.PerformLayout();
            this.grpNewField.ResumeLayout(false);
            this.grpNewField.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFieldLength)).EndInit();
            this.grpFilter.ResumeLayout(false);
            this.grpFilter.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpDynamicEntity;
        private System.Windows.Forms.ListBox listFields;
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
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnSelectFilter;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.CheckBox chkLimitResults;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtFilterValue;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbComparison;
        private System.Windows.Forms.ComboBox cmbFields;
        private System.Windows.Forms.Button btnDelete;
    }
}
