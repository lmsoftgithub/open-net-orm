using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenNETCF.ORM.MainDemo
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSqlCeDemo_Click(object sender, EventArgs e)
        {
            var frm = new SqlCe.frmMainSqlCe();
            this.Hide();
            frm.ShowDialog();
            this.Show();
        }

        private void btnSQLiteDemo_Click(object sender, EventArgs e)
        {
            var frm = new SQLite.frmMainSQLite();
            this.Hide();
            frm.ShowDialog();
            this.Show();
        }

        private void btnMSSQLDemo_Click(object sender, EventArgs e)
        {
            var frm = new MSSQL.frmMainMSSQL();
            this.Hide();
            frm.ShowDialog();
            this.Show();
        }

        private void btnFirebirdDemo_Click(object sender, EventArgs e)
        {

            this.Show();
        }

        private void btnMySQLDemo_Click(object sender, EventArgs e)
        {

            this.Show();
        }

        private void btnOracleDemo_Click(object sender, EventArgs e)
        {

            this.Show();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
