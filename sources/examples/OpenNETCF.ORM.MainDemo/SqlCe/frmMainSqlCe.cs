using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenNETCF.ORM;

namespace OpenNETCF.ORM.MainDemo.SqlCe
{
    public partial class frmMainSqlCe : Form
    {
        private SqlCeDataStore _DataStore;

        public frmMainSqlCe()
        {
            InitializeComponent();
            InitializeObjects();
        }

        public void InitializeObjects()
        {
            try
            {
                this.txtFileName.Text = "demo.sdf";
                if (System.IO.File.Exists(this.txtFileName.Text))
                {
                    var fi = new System.IO.FileInfo(this.txtFileName.Text);
                    this.lblSize.Text = String.Format("{0} Kb", (fi.Length / 1024));
                    this.lblCreatedOn.Text = String.Format("{0}", fi.CreationTime.ToString());

                    this._DataStore = new SqlCeDataStore(this.txtFileName.Text);
                    this._DataStore.MaxDatabaseSizeInMB = 500;
                    this._DataStore.ConnectionBehavior = ConnectionBehavior.Persistent;
                    this._DataStore.DiscoverTypes(System.Reflection.Assembly.GetAssembly(typeof(OpenNETCF.ORM.Model.basictable)));
                    this.dbsStructure.DataStore = this._DataStore;
                    this.dbtTests.DataStore = this._DataStore;
                    this.dbsStructure.DataStoreChanged += new EventHandler(dbsStructure_DataStoreChanged);
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
                this.lblSize.Text = "0 Kb";
                this.lblCreatedOn.Text = "-";
                if (this.txtFileName.Text.Length > 0)
                {
                    String path = System.IO.Path.GetDirectoryName(this.txtFileName.Text);
                    if (path.Length == 0 || System.IO.Directory.Exists(path))
                    {
                        this._DataStore = new SqlCeDataStore(this.txtFileName.Text);
                        this._DataStore.MaxDatabaseSizeInMB = 500;
                        this._DataStore.ConnectionBehavior = ConnectionBehavior.Persistent;
                        this._DataStore.DiscoverTypes(System.Reflection.Assembly.GetAssembly(typeof(OpenNETCF.ORM.Model.basictable)));

                        if (System.IO.File.Exists(this.txtFileName.Text))
                        {
                            this._DataStore.CreateOrUpdateStore();
                        }
                        else
                        {
                            this._DataStore.CreateStore();
                        }
                        this.dbsStructure.DataStore = this._DataStore;
                        this.dbtTests.DataStore = this._DataStore;

                        if (System.IO.File.Exists(this.txtFileName.Text))
                        {
                            var fi = new System.IO.FileInfo(this.txtFileName.Text);
                            this.lblSize.Text = String.Format("{0} Kb", (fi.Length / 1024));
                            this.lblCreatedOn.Text = String.Format("{0}", fi.CreationTime.ToString());
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
                this.lblSize.Text = "0 Kb";
                this.lblCreatedOn.Text = "-";

                this.dbsStructure.DataStore = null;
                this.dbtTests.DataStore = null;

                if (this._DataStore != null)
                {
                    this._DataStore.Dispose();
                    this._DataStore = null;
                }

                if (this.txtFileName.Text.Length > 0 && System.IO.File.Exists(this.txtFileName.Text))
                {
                    System.Threading.Thread.Sleep(500);
                    System.IO.File.Delete(this.txtFileName.Text);
                }
                
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            try
            {
                this.lblSize.Text = "0 Kb";
                this.lblCreatedOn.Text = "-";
                var opnFile = new OpenFileDialog();
                opnFile.AddExtension = true;
                opnFile.CheckPathExists = true;
                opnFile.DefaultExt = ".sdf";
                opnFile.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                opnFile.Multiselect = false;
                if (this.txtFileName.Text.Length > 0) opnFile.FileName = this.txtFileName.Text;
                DialogResult result = opnFile.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    if (opnFile.FileName.Length > 0 && System.IO.File.Exists(opnFile.FileName))
                    {
                        this.txtFileName.Text = opnFile.FileName;
                        this.txtFileName.BackColor = Color.FromArgb(255, 177, 255, 132);
                    }
                    else
                    {
                        this.txtFileName.BackColor = Color.FromArgb(255, 255, 169, 71);
                    }
                }
                else
                {
                    if (this.txtFileName.Text.Length > 0 && System.IO.File.Exists(this.txtFileName.Text))
                    {
                        this.txtFileName.BackColor = Color.FromArgb(255, 177, 255, 132);
                    }
                    else
                    {
                        this.txtFileName.BackColor = Color.FromArgb(255, 255, 169, 71);
                    }
                }
                opnFile.Dispose();

                if (System.IO.File.Exists(this.txtFileName.Text))
                {
                    var fi = new System.IO.FileInfo(this.txtFileName.Text);
                    this.lblSize.Text = String.Format("{0} Kb", (fi.Length / 1024));
                    this.lblCreatedOn.Text = String.Format("{0}", fi.CreationTime.ToString());
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }
    }
}
