using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenNETCF.ORM;

namespace OpenNETCF.ORM.MainDemo.MSSQL
{
    public partial class frmMainMSSQL : Form
    {
        private MSSqlDataStore _DataStore;

        public frmMainMSSQL()
        {
            InitializeComponent();
            InitializeObjects();
        }

        public void InitializeObjects()
        {
            try
            {
                this.txtConnectionString.Text = "Data Source=192.168.56.52;Initial Catalog=POD; User Id=sa; Password=manager;Timeout=30";
                this._DataStore = new MSSqlDataStore(this.txtConnectionString.Text);
                if (this._DataStore.HasConnection)
                {
                    this._DataStore.DiscoverTypes(System.Reflection.Assembly.GetAssembly(typeof(OpenNETCF.ORM.Model.basictable)));
                    this.dbsStructure.DataStore = this._DataStore;
                    this.dbtTests.DataStore = this._DataStore;
                    this.dbsStructure.DataStoreChanged += new EventHandler(dbsStructure_DataStoreChanged);
                }
                else
                {
                    this._DataStore.Dispose();
                    throw new Exception("Could not connect to the DataStore");
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        void dbsStructure_DataStoreChanged(object sender, EventArgs e)
        {
            this.dbtTests.RefreshUI();
        }

        private void btnCreateDataStore_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.txtConnectionString.Text.Length > 0)
                {
                    String path = System.IO.Path.GetDirectoryName(this.txtConnectionString.Text);
                    if (path.Length == 0 || System.IO.Directory.Exists(path))
                    {
                        this._DataStore = new MSSqlDataStore(this.txtConnectionString.Text);
                        this._DataStore.DiscoverTypes(System.Reflection.Assembly.GetAssembly(typeof(OpenNETCF.ORM.Model.basictable)));

                        if (System.IO.File.Exists(this.txtConnectionString.Text))
                        {
                            this._DataStore.CreateOrUpdateStore();
                        }
                        else
                        {
                            this._DataStore.CreateStore();
                        }
                        this.dbsStructure.DataStore = this._DataStore;
                        this.dbtTests.DataStore = this._DataStore;

                        if (System.IO.File.Exists(this.txtConnectionString.Text))
                        {
                            var fi = new System.IO.FileInfo(this.txtConnectionString.Text);
                        }
                    }
                    else
                    {
                        throw new Exception("Directory Path doesn't exist!");
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnDeleteDataStore_Click(object sender, EventArgs e)
        {
            try
            {
                this.dbsStructure.DataStore = null;
                this.dbtTests.DataStore = null;

                if (this._DataStore != null)
                {
                    this._DataStore.Dispose();
                    this._DataStore = null;
                }

                if (this.txtConnectionString.Text.Length > 0 && System.IO.File.Exists(this.txtConnectionString.Text))
                {
                    System.Threading.Thread.Sleep(500);
                    System.IO.File.Delete(this.txtConnectionString.Text);
                }
                
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }
    }
}
