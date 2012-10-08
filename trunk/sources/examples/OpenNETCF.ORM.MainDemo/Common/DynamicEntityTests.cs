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
        public event EventHandler DataStoreChanged;

        private IDataStore _DataStore;
        private List<EntityInfo> _entities;

        private List<EntityInfo> _unregistered_entities;
        private List<DynamicEntity> _unsaved_items;

        private System.ComponentModel.BindingList<String> _messages;

        public DynamicEntityTests()
        {
            InitializeComponent();
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            this.cmbDynamicEntities.SelectedIndexChanged += new EventHandler(cmbDynamicEntities_SelectedIndexChanged);
            this.lstFields.SelectedIndexChanged += new EventHandler(lstFields_SelectedIndexChanged);
            this.cmbFieldType.SelectedIndexChanged += new EventHandler(cmbFieldType_SelectedIndexChanged);
            this.cmbFieldType.DataSource = Enum.GetNames(typeof(System.Data.DbType));
            DynamicEntityInfo.CanChangeDefinitionsAtRuntime = true;
            this._unregistered_entities = new List<EntityInfo>();
            this.cmbComparison.DataSource = Enum.GetNames(typeof(FilterCondition.FilterOperator));
            this._unsaved_items = new List<DynamicEntity>();
            this.dgvDynamicEntities.MultiSelect = false;
            this.dgvDynamicEntities.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvDynamicEntities.SelectionChanged += new EventHandler(dgvDynamicEntities_SelectionChanged);
            this.pgrItemEditor.PropertyValueChanged += new PropertyValueChangedEventHandler(pgrItemEditor_PropertyValueChanged);
            this._messages = new BindingList<string>();

            var entity = new DynamicEntityInfo("demo_entity", null, null, KeyScheme.GUID);
            entity.AddField("pk", DbType.Guid, 0, true, true, FieldOrder.Ascending);
            entity.AddField("numfield", DbType.Int32, 0, false, false, FieldOrder.Ascending);
            entity.AddField("textfield", DbType.String, 20, false, false, FieldOrder.None);
            entity.AddField("uniquefield", DbType.String, 10, false, true, FieldOrder.None);
            this._unregistered_entities.Add(entity);
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
                            if (this._DataStore.TableExists(entity) && entity is DynamicEntityInfo) this._entities.Add(entity);
                        }
                        if (this._unregistered_entities.Count > 0)
                        {
                            foreach (var entity in this._unregistered_entities)
                                this._entities.Add(entity);
                        }
                        this.cmbDynamicEntities.DataSource = this._entities;
                        this.cmbComparison.DataSource = Enum.GetNames(typeof(FilterCondition.FilterOperator));
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
                CleanCreateEntityUI();
                CleanCreateFieldUI();
                CleanFilterUI();
                this.cmbDynamicEntities.DataSource = null;
                this.lstFields.DataSource = null;
                this.dgvDynamicEntities.DataSource = null;
                this.pgrItemEditor.SelectedObject = null;
                this.cmbFields.DataSource = null;
                this.pnlModified.Visible = false;
                this._unsaved_items.Clear();
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void CleanCreateEntityUI()
        {
            try
            {
                this.txtEntityName.Text = "";
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }
        private void CleanCreateFieldUI()
        {
            try
            {
                this.txtFieldName.Text = "";
                this.numFieldLength.Value = this.numFieldLength.Minimum;
                this.chkPrimaryKey.Checked = false;
                this.chkIndexedField.Checked = false;
                this.chkUniqueValue.Checked = false;
                this.chkPrimaryKey.Enabled = true;
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void CleanFilterUI()
        {
            try
            {
                this.txtFilterValue.Text = "";
                this.cmbComparison.SelectedIndex = 0;
                if (this.cmbFields.DataSource != null && this.cmbFields.Items.Count > 0)
                    this.cmbFields.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void cmbFieldType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var type = this.cmbFieldType.SelectedItem.ToString();
                switch (type)
                {
                    case "String":
                    case "StringFixedLength":
                    case "AnsiString":
                    case "AnsiStringFixedLength":
                        this.numFieldLength.Minimum = 1;
                        if (this.numFieldLength.Value < 10)
                            this.numFieldLength.Value = 20;
                        break;
                    default:
                        this.numFieldLength.Minimum = 0;
                        this.numFieldLength.Value = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void cmbDynamicEntities_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                if (entity != null)
                {
                    CleanCreateFieldUI();
                    this.lstFields.DataSource = entity.Fields.ToList();
                    this.cmbFields.DataSource = entity.Fields.ToList();
                    this.btnRegister.Enabled = !entity.Registered;
                    this.chkPrimaryKey.Enabled = !entity.Registered;
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void lstFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var field = this.lstFields.SelectedItem as FieldAttribute;
                if (field != null)
                {
                    this.pgrItemEditor.SelectedObject = field;
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        void dgvDynamicEntities_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvDynamicEntities.SelectedRows != null && this.dgvDynamicEntities.SelectedRows.Count > 0)
                {
                    var item = this.dgvDynamicEntities.SelectedRows[0].DataBoundItem;
                    if (item is DynamicEntity)
                    {
                        item = new DictionaryPropertyGridAdapter((DynamicEntity)item);
                    }
                    this.pgrItemEditor.SelectedObject = item;
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
                if (this.pgrItemEditor.SelectedObject != null)
                {
                    var adapter = this.pgrItemEditor.SelectedObject as DictionaryPropertyGridAdapter;
                    if (adapter != null && !this._unsaved_items.Contains(adapter.EntityItem))
                    {
                        this._unsaved_items.Add(adapter.EntityItem);
                    }
                }
                this.pnlModified.Visible = (this._unsaved_items.Count > 0);
                this.dgvDynamicEntities.Refresh();
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnAddEntity_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.txtEntityName.Text.Length > 0)
                {
                    var dynamic = (from el in this._entities
                                   where el.EntityName.Equals(this.txtEntityName.Text.Trim(), StringComparison.InvariantCultureIgnoreCase)
                                   select el).FirstOrDefault();
                    if (dynamic == null)
                    {
                        dynamic = new DynamicEntityInfo(this.txtEntityName.Text.Trim(), null, null);
                        this._entities.Add(dynamic);
                        this._unregistered_entities.Add(dynamic);
                        RefreshUI();
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnAddField_Click(object sender, EventArgs e)
        {
            try
            {
                var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                if (entity != null && this.txtFieldName.Text.Length > 0)
                {
                    var searchOrder = FieldOrder.None;
                    if (this.chkIndexedField.Checked)
                        searchOrder = FieldOrder.Ascending;
                    var type = System.Data.DbType.String;
                    try
                    {
                        type = (System.Data.DbType)Enum.Parse(typeof(System.Data.DbType), this.cmbFieldType.SelectedItem.ToString(), true);
                    }
                    catch
                    {
                        type = System.Data.DbType.String;
                        if (this.numFieldLength.Value <= 0)
                            this.numFieldLength.Value = 1;
                    }
                    var field = (from el in entity.Fields
                                 where el.FieldName.Equals(this.txtFieldName.Text.Trim(), StringComparison.InvariantCultureIgnoreCase)
                                 select el).FirstOrDefault();
                    if (field == null)
                    {
                        if (entity.Registered && this.chkPrimaryKey.Checked) this.chkPrimaryKey.Checked = false;
                        entity.AddField(this.txtFieldName.Text.Trim(), type, Convert.ToInt32(Math.Ceiling(this.numFieldLength.Value)), this.chkPrimaryKey.Checked, this.chkUniqueValue.Checked, searchOrder);
                    }
                    else
                    {
                        throw new Exception("Field alreayd exists");
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

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                if (entity != null)
                {
                    var unregistered = (from el in this._unregistered_entities
                                        where el.EntityName.Equals(entity.EntityName, StringComparison.InvariantCultureIgnoreCase)
                                        select el).FirstOrDefault();
                    if (unregistered != null && unregistered.Fields.Count > 0)
                    {
                        this.DataStore.RegisterEntity(entity);
                        this._unregistered_entities.Remove(unregistered);
                        if (DataStoreChanged != null)
                            DataStoreChanged.Invoke(this._DataStore, new EventArgs());
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

        private void btnReverseEngineering_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.DataStore != null)
                {
                    var entities = this.DataStore.ReverseEngineer();
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
                var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                if (entity != null)
                {
                    if (!entity.Registered) throw new Exception("The Entity hasn't been Registered!");
                    var dynamic = new DynamicEntity(entity);
                    var list = this.dgvDynamicEntities.DataSource as BindingList<DynamicEntity>;
                    if (list == null)
                    {
                        list = new BindingList<DynamicEntity>();
                        this.dgvDynamicEntities.DataSource = list;
                    }
                    list.Add(dynamic);
                    this._unsaved_items.Add(dynamic);
                    this.pnlModified.Visible = true;

                    this.pgrItemEditor.SelectedObject = new DictionaryPropertyGridAdapter((DynamicEntity)dynamic);
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
                var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                if (entity != null)
                {
                    if (!entity.Registered) throw new Exception("The Entity hasn't been Registered!");
                    FieldAttribute field = this.cmbFields.SelectedItem as FieldAttribute;
                    List<FilterCondition> filters = null;
                    if (field != null && this.txtFilterValue.Text.Length > 0)
                    {
                        filters = new List<FilterCondition>();
                        filters.Add(new FilterCondition(field.FieldName, GetValue(this.txtFilterValue.Text), (FilterCondition.FilterOperator)Enum.Parse(typeof(FilterCondition.FilterOperator), this.cmbComparison.SelectedItem.ToString())));
                    }
                    var list = new BindingList<int>();
                    DateTime start = DateTime.Now;
                    list.Add(this._DataStore.Count(entity.EntityName, filters));
                    AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                    this.dgvDynamicEntities.DataSource = list;
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbDynamicEntities.SelectedItem != null)
                {
                    var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                    if (entity != null)
                    {
                        if (!entity.Registered) throw new Exception("The Entity hasn't been Registered!");
                        DateTime start = DateTime.Now;
                        var list = this._DataStore.Select(entity.EntityName, false).Cast<DynamicEntity>().ToList();
                        AddMessage(String.Format("Executed in {0}ms (Results:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, list.Count));
                        var listType = typeof(System.ComponentModel.BindingList<>).MakeGenericType(entity.EntityType);
                        var bindList = new BindingList<DynamicEntity>(list);
                        this.dgvDynamicEntities.DataSource = bindList;
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnSelectFilter_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbDynamicEntities.SelectedItem != null)
                {
                    var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                    if (entity != null)
                    {
                        if (!entity.Registered) throw new Exception("The Entity hasn't been Registered!");
                        List<FilterCondition> filters = null;
                        FieldAttribute field = this.cmbFields.SelectedItem as FieldAttribute;
                        if (entity != null && field != null && this.txtFilterValue.Text.Length > 0)
                        {
                            filters = new List<FilterCondition>();
                            filters.Add(new FilterCondition(field.FieldName, GetValue(this.txtFilterValue.Text), (FilterCondition.FilterOperator)Enum.Parse(typeof(FilterCondition.FilterOperator), this.cmbComparison.SelectedItem.ToString())));
                        }
                        DateTime start = DateTime.Now;
                        var list = this._DataStore.Select(entity.EntityName, filters, false).Cast<DynamicEntity>().ToList();
                        AddMessage(String.Format("Executed in {0}ms (Results:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, list.Count));
                        var listType = typeof(System.ComponentModel.BindingList<>).MakeGenericType(entity.EntityType);
                        var bindList = new BindingList<DynamicEntity>(list);
                        this.dgvDynamicEntities.DataSource = bindList;
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                OpenNETCF.ORM.MainDemo.Logger.LogException(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void btnDropAndCreate_Click(object sender, EventArgs e)
        {

        }

        private void btnBulkInsert_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbDynamicEntities.SelectedItem != null)
                {
                    if (this._unsaved_items.Count > 0)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.BulkInsert(this._unsaved_items.ToArray(), false, chkTransactional.Checked);
                        AddMessage(String.Format("Executed in {0}ms (Items:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, this._unsaved_items.Count));
                        this._unsaved_items.Clear();
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
                if (this.cmbDynamicEntities.SelectedItem != null)
                {
                    if (this._unsaved_items.Count > 0)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.BulkInsertOrUpdate(this._unsaved_items.ToArray(), false, chkTransactional.Checked);
                        AddMessage(String.Format("Executed in {0}ms (Items:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, this._unsaved_items.Count));
                        this._unsaved_items.Clear();
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

        private void btnDeleteFiltered_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbDynamicEntities.SelectedItem != null)
                {
                    var entity = this.cmbDynamicEntities.SelectedItem as DynamicEntityInfo;
                    if (entity != null)
                    {
                        if (!entity.Registered) throw new Exception("The Entity hasn't been Registered!");
                        List<FilterCondition> filters = null;
                        FieldAttribute field = this.cmbFields.SelectedItem as FieldAttribute;
                        if (entity != null && field != null && this.txtFilterValue.Text.Length > 0)
                        {
                            filters = new List<FilterCondition>();
                            filters.Add(new FilterCondition(field.FieldName, GetValue(this.txtFilterValue.Text), (FilterCondition.FilterOperator)Enum.Parse(typeof(FilterCondition.FilterOperator), this.cmbComparison.SelectedItem.ToString())));
                        }
                        DateTime start = DateTime.Now;
                        var cnt = this._DataStore.Delete(entity.EntityName, filters, false);
                        AddMessage(String.Format("Executed in {0}ms (Deleted:{1})", DateTime.Now.Subtract(start).TotalMilliseconds, cnt));
                    }
                    RefreshUI();
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
                    var adapter = this.pgrItemEditor.SelectedObject as DictionaryPropertyGridAdapter;
                    if (adapter != null && adapter.EntityItem != null)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.Insert(adapter.EntityItem, false, this.chkTransactional.Checked);
                        AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                        if (this._unsaved_items.Contains(adapter.EntityItem))
                            this._unsaved_items.Remove(adapter.EntityItem);
                        this.pnlModified.Visible = (this._unsaved_items.Count > 0);
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
                    var adapter = this.pgrItemEditor.SelectedObject as DictionaryPropertyGridAdapter;
                    if (adapter != null && adapter.EntityItem != null)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.Update(adapter.EntityItem, false, null, this.chkTransactional.Checked);
                        AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                        if (this._unsaved_items.Contains(adapter.EntityItem))
                            this._unsaved_items.Remove(adapter.EntityItem);
                        this.pnlModified.Visible = (this._unsaved_items.Count > 0);
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
                    var adapter = this.pgrItemEditor.SelectedObject as DictionaryPropertyGridAdapter;
                    if (adapter != null && adapter.EntityItem != null)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.InsertOrUpdate(adapter.EntityItem, false, this.chkTransactional.Checked);
                        AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));
                        if (this._unsaved_items.Contains(adapter.EntityItem))
                            this._unsaved_items.Remove(adapter.EntityItem);
                        this.pnlModified.Visible = (this._unsaved_items.Count > 0);
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
                    var adapter = this.pgrItemEditor.SelectedObject as DictionaryPropertyGridAdapter;
                    if (adapter != null && adapter.EntityItem != null)
                    {
                        DateTime start = DateTime.Now;
                        this._DataStore.Delete(adapter.EntityItem, false);
                        AddMessage(String.Format("Executed in {0}ms", DateTime.Now.Subtract(start).TotalMilliseconds));

                        if (this._unsaved_items.Contains(adapter.EntityItem))
                            this._unsaved_items.Remove(adapter.EntityItem);
                        this.pnlModified.Visible = (this._unsaved_items.Count > 0);

                        var list = this.dgvDynamicEntities.DataSource as BindingList<DynamicEntity>;
                        if (list != null)
                        {
                            if (list.Contains(adapter.EntityItem))
                                list.Remove(adapter.EntityItem);
                        }
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
