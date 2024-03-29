﻿namespace OpenNETCF.ORM.MainDemo.Common
{
    partial class DatabaseStructureConfigs
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
            this.txtGenerateBaseAmount = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGenerateChildrenAmount = new System.Windows.Forms.TextBox();
            this.chkInsertTransactions = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtGenerateBaseAmount
            // 
            this.txtGenerateBaseAmount.Location = new System.Drawing.Point(157, 12);
            this.txtGenerateBaseAmount.Name = "txtGenerateBaseAmount";
            this.txtGenerateBaseAmount.Size = new System.Drawing.Size(100, 22);
            this.txtGenerateBaseAmount.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Generate Base Amount";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(153, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Generate Children Max";
            // 
            // txtGenerateChildrenAmount
            // 
            this.txtGenerateChildrenAmount.Location = new System.Drawing.Point(157, 38);
            this.txtGenerateChildrenAmount.Name = "txtGenerateChildrenAmount";
            this.txtGenerateChildrenAmount.Size = new System.Drawing.Size(100, 22);
            this.txtGenerateChildrenAmount.TabIndex = 3;
            // 
            // chkInsertTransactions
            // 
            this.chkInsertTransactions.AutoSize = true;
            this.chkInsertTransactions.Location = new System.Drawing.Point(12, 71);
            this.chkInsertTransactions.Name = "chkInsertTransactions";
            this.chkInsertTransactions.Size = new System.Drawing.Size(203, 21);
            this.chkInsertTransactions.TabIndex = 5;
            this.chkInsertTransactions.Text = "Use transactions for Inserts";
            this.chkInsertTransactions.UseVisualStyleBackColor = true;
            // 
            // DatabaseStructureConfigs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 268);
            this.Controls.Add(this.chkInsertTransactions);
            this.Controls.Add(this.txtGenerateChildrenAmount);
            this.Controls.Add(this.txtGenerateBaseAmount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Name = "DatabaseStructureConfigs";
            this.Text = "DatabaseStructureConfigs";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtGenerateBaseAmount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtGenerateChildrenAmount;
        private System.Windows.Forms.CheckBox chkInsertTransactions;
    }
}