using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenNETCF.ORM;

namespace OpenNETCF.ORM.MainDemo.Common
{
    public partial class DatabaseStructure : UserControl
    {
        public event EventHandler DataStoreChanged;

        private IDataStore _DataStore;
        private System.Collections.Generic.List<String> _messages = new List<string>();
        private DatabaseStructureConfigs.Configuration _config;
        private System.Threading.Thread _generator;
        private Boolean _stopGenerator;
        private DateTime _lastGeneration;

        public IDataStore DataStore {
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

        public Boolean StopGenerator
        {
            get
            {
                return this._stopGenerator;
            }
            set
            {
                this._stopGenerator = value;
                if (value)
                    this.btnGenerate.Text = "Generate Data";
                else
                    this.btnGenerate.Text = "Stop Generator";
            }
        }

        public DatabaseStructure()
        {
            InitializeComponent();
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            this._config = new DatabaseStructureConfigs.Configuration();
            this._config.GenerateDataBaseAmount = 1000;
            this._config.GenerateChildrenBaseAmount = 20;
            this.Disposed += new EventHandler(DatabaseStructure_Disposed);
            this._stopGenerator = false;
            this._lastGeneration = DateTime.MinValue;
        }

        void DatabaseStructure_Disposed(object sender, EventArgs e)
        {
            try
            {
                this._stopGenerator = true;
                if (this._generator != null && this._generator.ThreadState == System.Threading.ThreadState.Running)
                    this._generator.Abort();
            }
            catch
            {
            }
        }



        private void btnRefreshDBStructure_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshUI();
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnDropTable_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.treeDBStructure.SelectedNode != null && this.treeDBStructure.SelectedNode.Tag != null)
                {
                    var currNode = this.treeDBStructure.SelectedNode;
                    if (this.treeDBStructure.SelectedNode.Parent != null)
                        currNode = this.treeDBStructure.SelectedNode.Parent;
                    var entity = currNode.Tag as EntityInfo;
                    if (entity != null)
                    {
                        AddMessage(String.Format("Drop Table: {0}   Recursive: {1}", entity.EntityName, this.chkCascade.Checked));
                        this._DataStore.Drop(entity.EntityType, this.chkCascade.Checked);
                    }
                    RefreshUI();
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnDropAndCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.treeDBStructure.SelectedNode != null && this.treeDBStructure.SelectedNode.Tag != null)
                {
                    var currNode = this.treeDBStructure.SelectedNode;
                    if (this.treeDBStructure.SelectedNode.Parent != null)
                        currNode = this.treeDBStructure.SelectedNode.Parent;
                    var entity = currNode.Tag as EntityInfo;
                    if (entity != null)
                    {
                        AddMessage(String.Format("Drop and Create Table: {0}   Recursive: {1}", entity.EntityName, this.chkCascade.Checked));
                        this._DataStore.DropAndCreateTable(entity.EntityType, this.chkCascade.Checked);
                    }
                    RefreshUI();
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnDeleteTable_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.treeDBStructure.SelectedNode != null && this.treeDBStructure.SelectedNode.Tag != null)
                {
                    var currNode = this.treeDBStructure.SelectedNode;
                    if (this.treeDBStructure.SelectedNode.Parent != null)
                        currNode = this.treeDBStructure.SelectedNode.Parent;
                    var entity = currNode.Tag as EntityInfo;
                    if (entity != null)
                    {
                        AddMessage(String.Format("Delete Table: {0}   Recursive: {1}", entity.EntityName, this.chkCascade.Checked));
                        this._DataStore.Delete(entity.EntityType, this.chkCascade.Checked);
                    }
                    RefreshUI();
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        public void RefreshUI()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(RefreshUI));
            }
            else
            {
                try
                {
                    CleanUI();
                    if (this._DataStore != null)
                    {
                        if (this._DataStore.StoreExists)
                        {
                            foreach (var entity in this._DataStore.GetEntities())
                            {
                                try
                                {
                                    int iCount = 0;
                                    var entityNode = this.treeDBStructure.Nodes.Add(String.Format("{0} ({1}) - {2}", entity.EntityName, entity.Fields.Count, iCount));
                                    entityNode.Tag = entity;
                                    entityNode.Collapse();
                                    if (!this._DataStore.TableExists(entity))
                                        entityNode.BackColor = Color.FromArgb(255, 255, 169, 71);
                                    else
                                        iCount = this._DataStore.Count(entity.EntityType);
                                    entityNode.Text = String.Format("{0} ({1}) - {2}", entity.EntityName, entity.Fields.Count, iCount);
                                    foreach (var field in entity.Fields)
                                    {
                                        var fieldNode = new TreeNode(String.Format("{0} ({1})", field.FieldName, field.DataType.ToSqlTypeString()));
                                        if (!this._DataStore.FieldExists(entity, field))
                                        {
                                            fieldNode.BackColor = Color.FromArgb(255, 255, 169, 71);
                                            entityNode.BackColor = Color.FromArgb(255, 255, 169, 71);
                                        }
                                        entityNode.Nodes.Add(fieldNode);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                                }
                            }
                        }
                        else
                        {
                            throw new OpenNETCF.ORM.StoreNotFoundException();
                        }
                        if (DataStoreChanged != null)
                            DataStoreChanged.Invoke(this._DataStore, new EventArgs());
                    }
                }
                catch (Exception ex)
                {
                    OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
        }

        public void Progress(Double value)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<Double>(Progress), value);
            }
            else
            {
                try
                {
                    if (value >= 1) value = 0.99;
                    if (value < 0) value = 0;
                    Double current = value * 100;
                    this.prgProgress.Value = (int)Math.Floor(current);
                }
                catch (Exception ex)
                {
                    OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
        }

        public void CleanUI()
        {
            try
            {
                this.treeDBStructure.Nodes.Clear();
                this.prgProgress.Value = 0;
                this.StopGenerator = true;
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (DateTime.Now.Subtract(this._lastGeneration).TotalSeconds > 5)
                {
                    this._lastGeneration = DateTime.Now;
                    if (this._generator == null || this._generator.ThreadState != System.Threading.ThreadState.Running)
                    {
                        this._generator = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadedCreation));
                        this.StopGenerator = false;
                        this._generator.Start();
                    }
                    else
                    {
                        this.StopGenerator = true;
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void ThreadedCreation()
        {
            try
            {
                if (this._DataStore != null)
                {
                    Boolean useTransations = this._config.UseTransactionsForInserts;
                    var entities = this._DataStore.GetEntities();
                    int baseAmount = this._config.GenerateDataBaseAmount;
                    var list = new List<EntityInfo>();
                    int iEntity = 0;
                    foreach (var entity in entities)
                    {
                        Type type = entity.EntityType;
                        // Filtering only topmost objects
                        var parent = (from el in entities
                                      where el.References.Any(p => p.ReferenceEntityType.Equals(type))
                                      select el).FirstOrDefault();
                        if (parent == null)
                        {
                            list.Add(entity);
                        }
                    }
                    if (list.Count > 0)
                    {
                        double maxValue = list.Count * 2.5 * baseAmount;
                        double speed = 0;
                        foreach (var entity in list)
                        {
                            if (this._stopGenerator) break;
                            Random rand = new Random(DateTime.Now.Millisecond * DateTime.Now.Minute);
                            int iCount = baseAmount + rand.Next(2 * baseAmount);
                            Model.modelbase item = (Model.modelbase)Activator.CreateInstance(entity.EntityType);
                            Boolean bHasReferences = (entity.References.Count > 0);
                            var start = DateTime.Now;
                            for (int i = 0; i < iCount; i++)
                            {
                                if (this._stopGenerator) break;
                                var newelement = item.CreateRandomObject();
                                AddMessage(newelement.ToString());
                                RecurseCreation(newelement, entities);
                                this._DataStore.Insert(newelement, bHasReferences, useTransations);
                                Progress(iEntity++ / maxValue);
                            }
                            speed = DateTime.Now.Subtract(start).TotalMilliseconds / iCount;
                            System.Diagnostics.Debug.WriteLine(String.Format("{1} average speed: {0}ms/item", speed, entity.EntityName));
                            AddMessage(String.Format("{1} average speed: {0}ms/item", speed, entity.EntityName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            finally
            {
                RefreshUI();
            }
        }

        private void RecurseCreation(object item, EntityInfoCollection<EntityInfo> entities)
        {
            Random rand = new Random(DateTime.Now.Millisecond * DateTime.Now.Minute);
            string entityName = entities.GetNameForType(item.GetType());
            if (entities.HasEntity(entityName) && entities[entityName].References.Count > 0)
            {
                object primarykey = entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);
                foreach (var refentity in entities[entityName].References)
                {
                    if (this._stopGenerator) break;
                    var genType = typeof(List<>).MakeGenericType(refentity.ReferenceEntityType);
                    var items = (System.Collections.IList)Activator.CreateInstance(genType);
                    Model.modelbase reference = (Model.modelbase)Activator.CreateInstance(refentity.ReferenceEntityType);
                    int iCount = rand.Next(this._config.GenerateChildrenBaseAmount);
                    for (int i = 0; i < iCount; i++)
                    {
                        if (this._stopGenerator) break;
                        var newelement = reference.CreateRandomObject(primarykey);
                        AddMessage(newelement.ToString());
                        items.Add(newelement);
                        //this._DataStore.Insert(newelement);
                        RecurseCreation(newelement, entities);
                    }
                    if (refentity.IsArray)
                    {
                        var arr = Array.CreateInstance(refentity.ReferenceEntityType, items.Count);
                        items.CopyTo(arr, 0);
                        refentity.PropertyInfo.SetValue(item, arr, null);
                    }
                    else if (refentity.IsList)
                    {
                        refentity.PropertyInfo.SetValue(item, items, null);
                    }
                    else
                    {
                        refentity.PropertyInfo.SetValue(item, items[0], null);
                    }
                }
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            try
            {
                var frm = new System.Windows.Forms.Form();
                frm.Text = "Report";
                frm.Width = 800;
                frm.Height = 600;
                var lst = new System.Windows.Forms.ListBox();
                lst.Dock = DockStyle.Fill;
                frm.Controls.Add(lst);
                lst.DataSource = this._messages;
                frm.FormClosing += new FormClosingEventHandler(frm_FormClosing);
                frm.Show();
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                var form = sender as System.Windows.Forms.Form;
                if (form != null)
                {
                    form.FormClosing -= new FormClosingEventHandler(frm_FormClosing);
                    var lst = form.Tag as System.Windows.Forms.ListBox;
                    if (lst != null)
                    {
                        lst.DataSource = null;
                        lst.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void AddMessage(string msg)
        {
            try
            {
                System.Threading.Monitor.TryEnter(this._messages);
                this._messages.Add(DateTime.Now.ToString() + ": " + msg);
                while (this._messages.Count > 5000)
                    this._messages.RemoveAt(0);
            }
            finally
            {
                System.Threading.Monitor.Exit(this._messages);
            }
        }

        private void lblTitle_DblClick(object sender, EventArgs e)
        {
            var frm = new DatabaseStructureConfigs(this._config);
            frm.ShowDialog();
            AddMessage("Modified Config");
        }
    }
}
