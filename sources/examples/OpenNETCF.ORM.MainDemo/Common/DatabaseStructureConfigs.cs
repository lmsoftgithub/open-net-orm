using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenNETCF.ORM.MainDemo.Common
{
    public partial class DatabaseStructureConfigs : Form
    {
        public class Configuration
        {
            public int GenerateDataBaseAmount { get; set; }
            public int GenerateChildrenBaseAmount { get; set; }
        }

        public Configuration Config { get; set; }

        public DatabaseStructureConfigs(Configuration config)
        {
            InitializeComponent();
            this.Config = config;
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            if (this.Config != null)
            {
                this.txtGenerateBaseAmount.Text = this.Config.GenerateDataBaseAmount.ToString();
                this.txtGenerateChildrenAmount.Text = this.Config.GenerateChildrenBaseAmount.ToString();
            }

            this.FormClosing += new FormClosingEventHandler(DatabaseStructureConfigs_FormClosing);
        }

        void DatabaseStructureConfigs_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Config != null)
            {
                int intval = 0;
                if (int.TryParse(this.txtGenerateBaseAmount.Text, out intval)) this.Config.GenerateDataBaseAmount = intval;
                if (int.TryParse(this.txtGenerateChildrenAmount.Text, out intval)) this.Config.GenerateChildrenBaseAmount = intval;
            }
        }
    }
}
