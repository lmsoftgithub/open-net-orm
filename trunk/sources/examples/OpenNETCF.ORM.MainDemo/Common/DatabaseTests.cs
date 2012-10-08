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
    public partial class DatabaseTests : UserControl
    {
        private IDataStore _DataStore;
        private List<EntityInfo> _entities;
        private EntityInfo _currentEntity;
        private System.ComponentModel.BindingList<String> _messages;
        private List<object> _unsavedItems;

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


        public DatabaseTests()
        {
            InitializeComponent();
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            this.cmbTable.SelectedIndexChanged += new EventHandler(cmbTable_SelectedIndexChanged);
            this.dgvResultSet.MultiSelect = false;
            this.dgvResultSet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvResultSet.SelectionChanged += new EventHandler(dgvResultSet_SelectionChanged);
            this.pgrItemEditor.PropertyValueChanged += new PropertyValueChangedEventHandler(pgrItemEditor_PropertyValueChanged);
            this._messages = new System.ComponentModel.BindingList<string>();
            this._unsavedItems = new List<object>();
            this.cmbComparison.DataSource = Enum.GetNames(typeof(FilterCondition.FilterOperator));
        }

        public void RefreshUI()
        {
            try
            {
                CleanUI();
                if (this._DataStore != null)
                {
                    if (this._DataStore.StoreExists)
                    {
                        this._entities = new List<EntityInfo>();
                        foreach (var entity in this._DataStore.GetEntities())
                        {
                            if (this._DataStore.TableExists(entity)) this._entities.Add(entity);
                        }
                        
                        this.cmbTable.DisplayMember = "EntityName";
                        this.cmbTable.DataSource = this._entities;
                    }
                    else
                    {
                        throw new OpenNETCF.ORM.StoreNotFoundException();
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public void CleanUI()
        {
            try
            {
                this.cmbTable.DataSource = null;
                this.cmbFields.DataSource = null;
                this.cmbComparison.DataSource = null;
                this.txtFilterValue.Text = "";
                this.txtRowOffset.Text = "";
                this.txtRowCount.Text = "";
                this.pgrItemEditor.SelectedObject = null;
                this.dgvResultSet.DataSource = null;
                this._entities = null;
                this._unsavedItems.Clear();
                this._currentEntity = null;
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void CleanUISelect()
        {
            try
            {
                this.pgrItemEditor.SelectedObject = null;
                this.dgvResultSet.DataSource = null;
                this._unsavedItems.Clear();
                this.pnlModified.Visible = false;
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void cmbTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this._entities != null && this._entities.Count > 0)
                {
                    if (this._currentEntity == null || !this._currentEntity.Equals(this.cmbTable.SelectedItem))
                    {
                        this._currentEntity = this.cmbTable.SelectedItem as EntityInfo;
                        if (this._currentEntity != null)
                        {
                            this.cmbFields.DataSource = this._currentEntity.Fields.ToList();
                            CleanUISelect();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshUI();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbTable.SelectedItem != null)
                {
                    CleanUISelect();
                    EntityInfo entity = this.cmbTable.SelectedItem as EntityInfo;
                    if (entity != null)
                    {
                        DateTime start = DateTime.Now;
                        System.Collections.IList list = (System.Collections.IList)this._DataStore.Select(entity.EntityType, this.chkFillReferences.Checked, this.chkFilterReferences.Checked).ToList();
                        AddMessage(String.Format("Executed in {0}ms (Results:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, list.Count));
                        var listType = typeof(System.ComponentModel.BindingList<>).MakeGenericType(entity.EntityType);
                        IBindingList bindList = (System.ComponentModel.IBindingList)Activator.CreateInstance(listType);
                        this.dgvResultSet.DataSource = bindList;
                        Application.DoEvents();
                        if (list.Count > 0)
                        {
                            int iCount = 0;
                            foreach (var item in list)
                            {
                                bindList.Add(item);
                                if (iCount++ >= 10000 & chkLimitResults.Checked) break;
                            }
                        }
                        //var conn = new System.Data.SqlServerCe.SqlCeConnection(String.Format("Data Source={0};Persist Security Info=False;Max Database Size=500;", "demo.sdf"));
                        //var items = new List<object>();
                        //using (var cmd = new System.Data.SqlServerCe.SqlCeCommand(entity.EntityName, conn))
                        //{
                        //    conn.Open();

                        //    cmd.CommandType = CommandType.TableDirect;
                        //    start = DateTime.Now;
                        //    var r = cmd.ExecuteResultSet(System.Data.SqlServerCe.ResultSetOptions.None);
                        //    System.Diagnostics.Debug.WriteLine(String.Format("Table direct in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));

                        //    var ordinals = new Dictionary<string, int>();
                        //    foreach (var field in entity.Fields)
                        //    {
                        //        ordinals.Add(field.FieldName, r.GetOrdinal(field.FieldName));
                        //    }
                        //    start = DateTime.Now;
                        //    while (r.Read())
                        //    {
                        //        items.Add(entity.CreateProxy.Invoke(r, ordinals));
                        //    }
                        //    System.Diagnostics.Debug.WriteLine(String.Format("populated in {0} ({1})", DateTime.Now.Subtract(start).TotalMilliseconds, items.Count));
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private Model.hierarchy3a GetInstance(System.Data.IDataReader result, System.Collections.Generic.IDictionary<string, int> ordinals)
        {
            var obj = new Model.hierarchy3a()
            {
                pid = result.IsDBNull(ordinals["pid"]) ? new Guid() : result.GetGuid(ordinals["pid"]),
                fid = result.IsDBNull(ordinals["fid"]) ? 0 : result.GetInt64(ordinals["fid"]),
                uid = result.IsDBNull(ordinals["uid"]) ? new Guid() : result.GetGuid(ordinals["uid"]),
                numfield1 = result.IsDBNull(ordinals["numfield1"]) ? 0 : result.GetInt32(ordinals["numfield1"]),
                numfield2 = result.IsDBNull(ordinals["numfield2"]) ? 0 : result.GetInt32(ordinals["numfield2"]),
                numfield3 = result.IsDBNull(ordinals["numfield3"]) ? 0 : result.GetInt32(ordinals["numfield3"]),
                textfield1 = result.IsDBNull(ordinals["textfield1"]) ? null : result.GetString(ordinals["textfield1"]),
                textfield2 = result.IsDBNull(ordinals["textfield2"]) ? null : result.GetString(ordinals["textfield2"]),
                textfield3 = result.IsDBNull(ordinals["textfield3"]) ? null : result.GetString(ordinals["textfield3"])
            };
            return obj;
        }

        private void btnSelectFiltered_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbTable.SelectedItem != null && this.cmbFields.SelectedItem != null)
                {
                    CleanUISelect();
                    EntityInfo entity = this.cmbTable.SelectedItem as EntityInfo;
                    FieldAttribute field = this.cmbFields.SelectedItem as FieldAttribute;
                    if (entity != null && field != null && this.txtFilterValue.Text.Length > 0)
                    {
                        var filters = new List<FilterCondition>();
                        filters.Add(new FilterCondition(field.FieldName, GetValue(this.txtFilterValue.Text), (FilterCondition.FilterOperator)Enum.Parse(typeof(FilterCondition.FilterOperator), this.cmbComparison.SelectedItem.ToString())));
                        DateTime start = DateTime.Now;
                        var list = this._DataStore.Select(entity.EntityType, filters, this.chkFillReferences.Checked, this.chkFilterReferences.Checked);
                        AddMessage(String.Format("Executed in {0}ms (Results:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, list.Length));
                        var listType = typeof(System.ComponentModel.BindingList<>).MakeGenericType(entity.EntityType);
                        IBindingList bindList = (System.ComponentModel.IBindingList)Activator.CreateInstance(listType);
                        this.dgvResultSet.DataSource = bindList;
                        if (list.Length > 0)
                        {
                            int iCount = 0;
                            foreach (var item in list)
                            {
                                bindList.Add(item);
                                if (iCount++ >= 10000 & chkLimitResults.Checked) break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        object GetValue(String txt)
        {
            txt = txt.Trim();
            object result = txt;
            if (!(txt.StartsWith("\"") | txt.EndsWith("\"")))
            {
                int intval = 0;
                if (int.TryParse(txt, out intval)) return intval;
                DateTime dateval;
                if (DateTime.TryParse(txt, out dateval)) return dateval;
                Boolean boolval;
                if (Boolean.TryParse(txt, out boolval)) return boolval;
            }
            return result;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbTable.SelectedItem != null)
                {
                    EntityInfo entity = this.cmbTable.SelectedItem as EntityInfo;
                    if (entity != null)
                    {
                        var item = Activator.CreateInstance(entity.EntityType) as Model.modelbase;
                        if (item != null)
                        {
                            if (dgvResultSet.DataSource == null)
                            {
                                var listType = typeof(BindingList<>).MakeGenericType(entity.EntityType);
                                dgvResultSet.DataSource = Activator.CreateInstance(listType);
                            }
                            dgvResultSet.ClearSelection();
                            var list = dgvResultSet.DataSource as System.ComponentModel.IBindingList;
                            list.Add(item);
                            this._unsavedItems.Add(item);
                            this.pgrItemEditor.SelectedObject = item;
                            this.pnlModified.Visible = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnFetch_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbTable.SelectedItem != null)
                {
                    CleanUISelect();
                    EntityInfo entity = this.cmbTable.SelectedItem as EntityInfo;
                    if (entity != null && this.txtRowOffset.Text.Length > 0 && this.txtRowCount.Text.Length > 0)
                    {
                        bool bIsNumeric = true;
                        int val = 0;
                        bIsNumeric = bIsNumeric & int.TryParse(this.txtRowCount.Text, out val);
                        bIsNumeric = bIsNumeric & int.TryParse(this.txtRowOffset.Text, out val);
                        if (bIsNumeric)
                        {
                            DateTime start = DateTime.Now;
                            var list = this._DataStore.Fetch(entity.EntityType, int.Parse(this.txtRowCount.Text), int.Parse(this.txtRowOffset.Text), this.chkFillReferences.Checked, this.chkFilterReferences.Checked);
                            AddMessage(String.Format("Executed in {0}ms (Results:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, list.Length));
                            var listType = typeof(System.ComponentModel.BindingList<>).MakeGenericType(entity.EntityType);
                            IBindingList bindList = (System.ComponentModel.IBindingList)Activator.CreateInstance(listType);
                            this.dgvResultSet.DataSource = bindList;
                            if (list.Length > 0)
                            {
                                int iCount = 0;
                                foreach (var item in list)
                                {
                                    bindList.Add(item);
                                    if (iCount++ >= 10000 & chkLimitResults.Checked) break;
                                }
                            }
                        }
                        else
                        {
                            AddMessage(String.Format("Row Count or Row Offset is not numeric"));
                        }
                    }
                    else
                    {
                        AddMessage(String.Format("One of the required value is not set correctly"));
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnBulkInsert_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbTable.SelectedItem != null)
                {
                    if (this._unsavedItems.Count > 0)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.BulkInsert(this._unsavedItems.ToArray(), this.chkFillReferences.Checked, this.chkTransactional.Checked);
                        AddMessage(String.Format("Executed in {0}ms (Items:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, this._unsavedItems.Count));
                        this._unsavedItems.Clear();
                        this.pnlModified.Visible = false;
                    }
                    else
                    {
                        AddMessage(String.Format("No new items to insert"));
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnBulkInsertOrUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbTable.SelectedItem != null)
                {
                    if (this._unsavedItems.Count > 0)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.BulkInsertOrUpdate(this._unsavedItems.ToArray(), this.chkFillReferences.Checked, this.chkTransactional.Checked);
                        AddMessage(String.Format("Executed in {0}ms (Items:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, this._unsavedItems.Count));
                        this._unsavedItems.Clear();
                        this.pnlModified.Visible = false;
                    }
                    else
                    {
                        AddMessage(String.Format("No new or modified items to insert"));
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnDeleteFiltered_Click(object sender, EventArgs e)
        {
            try
            {
                EntityInfo entity = this.cmbTable.SelectedItem as EntityInfo;
                FieldAttribute field = this.cmbFields.SelectedItem as FieldAttribute;
                if (entity != null && field != null && this.txtFilterValue.Text.Length > 0)
                {
                    var filters = new List<FilterCondition>();
                    filters.Add(new FilterCondition(field.FieldName, GetValue(this.txtFilterValue.Text), (FilterCondition.FilterOperator)Enum.Parse(typeof(FilterCondition.FilterOperator), this.cmbComparison.SelectedItem.ToString())));
                    DateTime start = DateTime.Now;
                    var cnt = this._DataStore.Delete(entity.EntityType, filters, this.chkCascadeTableActions.Checked);
                    AddMessage(String.Format("Executed in {0}ms (Affected:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, cnt));
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.pgrItemEditor.SelectedObject != null)
                {
                    DateTime start = DateTime.Now;
                    this._DataStore.Insert(this.pgrItemEditor.SelectedObject, this.chkFillReferences.Checked, this.chkTransactional.Checked);
                    AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                    if (this._unsavedItems.Count > 0 && this._unsavedItems.Contains(this.pgrItemEditor.SelectedObject))
                    {
                        this._unsavedItems.Remove(this.pgrItemEditor.SelectedObject);
                        this.pnlModified.Visible = (this._unsavedItems.Count > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.pgrItemEditor.SelectedObject != null)
                {
                    DateTime start = DateTime.Now;
                    this._DataStore.Update(this.pgrItemEditor.SelectedObject, this.chkFillReferences.Checked, null, this.chkTransactional.Checked);
                    AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                    if (this._unsavedItems.Count > 0 && this._unsavedItems.Contains(this.pgrItemEditor.SelectedObject))
                    {
                        this._unsavedItems.Remove(this.pgrItemEditor.SelectedObject);
                        this.pnlModified.Visible = (this._unsavedItems.Count > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnInsertOrUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.pgrItemEditor.SelectedObject != null)
                {
                    DateTime start = DateTime.Now;
                    this._DataStore.InsertOrUpdate(this.pgrItemEditor.SelectedObject, this.chkFillReferences.Checked, this.chkTransactional.Checked);
                    AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                    if (this._unsavedItems.Count > 0 && this._unsavedItems.Contains(this.pgrItemEditor.SelectedObject))
                    {
                        this._unsavedItems.Remove(this.pgrItemEditor.SelectedObject);
                        this.pnlModified.Visible = (this._unsavedItems.Count > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.pgrItemEditor.SelectedObject != null)
                {
                    DateTime start = DateTime.Now;
                    this._DataStore.Delete(this.pgrItemEditor.SelectedObject, this.chkCascadeItemActions.Checked);
                    AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                    if (this._unsavedItems.Count > 0 && this._unsavedItems.Contains(this.pgrItemEditor.SelectedObject))
                    {
                        this._unsavedItems.Remove(this.pgrItemEditor.SelectedObject);
                        this.pnlModified.Visible = (this._unsavedItems.Count > 0);
                    }
                    var list = dgvResultSet.DataSource as IBindingList;
                    if (list != null && list.Contains(this.pgrItemEditor.SelectedObject))
                    {
                        list.Remove(this.pgrItemEditor.SelectedObject);
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnCount_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbTable.SelectedItem != null)
                {
                    CleanUISelect();
                    EntityInfo entity = this.cmbTable.SelectedItem as EntityInfo;
                    FieldAttribute field = this.cmbFields.SelectedItem as FieldAttribute;
                    List<FilterCondition> filters = null;
                    if (field != null && this.txtFilterValue.Text.Length > 0)
                    {
                        filters = new List<FilterCondition>();
                        filters.Add(new FilterCondition(field.FieldName, GetValue(this.txtFilterValue.Text), (FilterCondition.FilterOperator)Enum.Parse(typeof(FilterCondition.FilterOperator), this.cmbComparison.SelectedItem.ToString())));
                    }
                    var list = new BindingList<int>();
                    DateTime start = DateTime.Now;
                    list.Add(this._DataStore.Count(entity.EntityType, filters));
                    AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                    dgvResultSet.DataSource = list;
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void dgvResultSet_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.lblItemType.Text = "-";
                if (dgvResultSet.SelectedRows != null && dgvResultSet.SelectedRows.Count > 0)
                {
                    this.pgrItemEditor.SelectedObject = dgvResultSet.SelectedRows[0].DataBoundItem;
                    var modelbase = this.pgrItemEditor.SelectedObject as Model.modelbase;
                    if (modelbase != null)
                        this.lblItemType.Text = modelbase.ToString();
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void pgrItemEditor_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            try
            {
                if (this.pgrItemEditor.SelectedObject != null && !this._unsavedItems.Contains(this.pgrItemEditor.SelectedObject))
                {
                    this._unsavedItems.Add(this.pgrItemEditor.SelectedObject);
                }
                this.pnlModified.Visible = (this._unsavedItems.Count > 0);
                this.dgvResultSet.Refresh();
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void AddMessage(string msg)
        {
            if (this.lblMessage.InvokeRequired)
            {
                this.lblMessage.BeginInvoke(new Action<string>(AddMessage), msg);
            }
            else
            {
                this.lblMessage.Text = msg;
                this._messages.Add(DateTime.Now.ToString() + ": " + msg);
                while (this._messages.Count > 5000)
                    this._messages.RemoveAt(0);
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

    }
}
