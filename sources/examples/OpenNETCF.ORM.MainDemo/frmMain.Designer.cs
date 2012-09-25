namespace OpenNETCF.ORM.MainDemo
{
    partial class frmMain
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
            this.btnSqlCeDemo = new System.Windows.Forms.Button();
            this.btnSQLiteDemo = new System.Windows.Forms.Button();
            this.btnMSSQLDemo = new System.Windows.Forms.Button();
            this.btnFirebirdDemo = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnMySQLDemo = new System.Windows.Forms.Button();
            this.btnOracleDemo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSqlCeDemo
            // 
            this.btnSqlCeDemo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSqlCeDemo.Location = new System.Drawing.Point(47, 29);
            this.btnSqlCeDemo.Name = "btnSqlCeDemo";
            this.btnSqlCeDemo.Size = new System.Drawing.Size(199, 33);
            this.btnSqlCeDemo.TabIndex = 0;
            this.btnSqlCeDemo.Text = "SqlCe Demo";
            this.btnSqlCeDemo.UseVisualStyleBackColor = true;
            this.btnSqlCeDemo.Click += new System.EventHandler(this.btnSqlCeDemo_Click);
            // 
            // btnSQLiteDemo
            // 
            this.btnSQLiteDemo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSQLiteDemo.Location = new System.Drawing.Point(47, 68);
            this.btnSQLiteDemo.Name = "btnSQLiteDemo";
            this.btnSQLiteDemo.Size = new System.Drawing.Size(199, 33);
            this.btnSQLiteDemo.TabIndex = 1;
            this.btnSQLiteDemo.Text = "SQLite Demo";
            this.btnSQLiteDemo.UseVisualStyleBackColor = true;
            this.btnSQLiteDemo.Click += new System.EventHandler(this.btnSQLiteDemo_Click);
            // 
            // btnMSSQLDemo
            // 
            this.btnMSSQLDemo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMSSQLDemo.Location = new System.Drawing.Point(47, 107);
            this.btnMSSQLDemo.Name = "btnMSSQLDemo";
            this.btnMSSQLDemo.Size = new System.Drawing.Size(199, 33);
            this.btnMSSQLDemo.TabIndex = 2;
            this.btnMSSQLDemo.Text = "MSSQL Demo";
            this.btnMSSQLDemo.UseVisualStyleBackColor = true;
            this.btnMSSQLDemo.Click += new System.EventHandler(this.btnMSSQLDemo_Click);
            // 
            // btnFirebirdDemo
            // 
            this.btnFirebirdDemo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFirebirdDemo.Location = new System.Drawing.Point(47, 146);
            this.btnFirebirdDemo.Name = "btnFirebirdDemo";
            this.btnFirebirdDemo.Size = new System.Drawing.Size(199, 33);
            this.btnFirebirdDemo.TabIndex = 3;
            this.btnFirebirdDemo.Text = "FireBird Demo";
            this.btnFirebirdDemo.UseVisualStyleBackColor = true;
            this.btnFirebirdDemo.Click += new System.EventHandler(this.btnFirebirdDemo_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(47, 293);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(199, 33);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnMySQLDemo
            // 
            this.btnMySQLDemo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMySQLDemo.Location = new System.Drawing.Point(47, 185);
            this.btnMySQLDemo.Name = "btnMySQLDemo";
            this.btnMySQLDemo.Size = new System.Drawing.Size(199, 33);
            this.btnMySQLDemo.TabIndex = 5;
            this.btnMySQLDemo.Text = "MySQL Demo";
            this.btnMySQLDemo.UseVisualStyleBackColor = true;
            this.btnMySQLDemo.Click += new System.EventHandler(this.btnMySQLDemo_Click);
            // 
            // btnOracleDemo
            // 
            this.btnOracleDemo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOracleDemo.Location = new System.Drawing.Point(47, 224);
            this.btnOracleDemo.Name = "btnOracleDemo";
            this.btnOracleDemo.Size = new System.Drawing.Size(199, 33);
            this.btnOracleDemo.TabIndex = 6;
            this.btnOracleDemo.Text = "Oracle Demo";
            this.btnOracleDemo.UseVisualStyleBackColor = true;
            this.btnOracleDemo.Click += new System.EventHandler(this.btnOracleDemo_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 338);
            this.ControlBox = false;
            this.Controls.Add(this.btnOracleDemo);
            this.Controls.Add(this.btnMySQLDemo);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnFirebirdDemo);
            this.Controls.Add(this.btnMSSQLDemo);
            this.Controls.Add(this.btnSQLiteDemo);
            this.Controls.Add(this.btnSqlCeDemo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "frmMain";
            this.Text = "OpenNETCF Main Demo";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSqlCeDemo;
        private System.Windows.Forms.Button btnSQLiteDemo;
        private System.Windows.Forms.Button btnMSSQLDemo;
        private System.Windows.Forms.Button btnFirebirdDemo;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnMySQLDemo;
        private System.Windows.Forms.Button btnOracleDemo;
    }
}

