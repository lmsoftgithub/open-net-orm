using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenNETCF.ORM.MainDemo.Common
{
    public partial class DynamicEntityTests : UserControl
    {
        private IDataStore _DataStore;
        public DynamicEntityTests()
        {
            InitializeComponent();
        }

        public IDataStore DataStore
        {
            private get
            {
                return this._DataStore;
            }
            set
            {
                this._DataStore = value;
                RefreshUI();
            }
        }

        public void RefreshUI()
        {

        }

        public void CleanUI()
        {

        }
    }
}
