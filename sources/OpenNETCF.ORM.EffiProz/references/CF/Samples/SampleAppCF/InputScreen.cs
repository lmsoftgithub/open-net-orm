using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.EffiProz;

namespace SampleAppCF
{
    public partial class InputScreen : Form
    {

        private DataSet m_oDs = null;
        private EfzDataAdapter m_oDA = null;
        private EfzConnection m_oCn = null;
        private string m_sDataSource;


        public InputScreen()
        {
            InitializeComponent();

            //Initialize DataSet and SqlCeConnection.
            this.m_oDs = new DataSet();
            this.m_sDataSource = "Connection Type=file ; Initial Catalog=/Storage Card/Northwind/NorthwindEF; User=sa; Password=;";
            //this.m_sDataSource = "\\my documents\\TestDB1.sdf";
            this.m_oCn = new EfzConnection(m_sDataSource); ;

            // Create the SELECT Command
            string sSelectSQL = "SELECT p.ProductID, c.CategoryID, c.CategoryName, p.ProductName, " +
                " p.UnitPrice, p.UnitsInStock, p.UnitsOnOrder, p.ReorderLevel " +
                " FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID " +
                " ORDER BY c.CategoryName, p.ProductName";
            this.m_oDA = new EfzDataAdapter();
            this.m_oDA.SelectCommand = new EfzCommand(sSelectSQL);
            this.m_oDA.SelectCommand.Connection = this.m_oCn;

            // Create the UPDATE Command
            string sUpdateSQL = "UPDATE Products SET UnitPrice = @UnitPrice, UnitsInStock = @UnitsInStock, UnitsOnOrder = @UnitsOnOrder, ReorderLevel = @ReorderLevel WHERE ProductID = @ProductID";
            this.m_oDA.UpdateCommand = new EfzCommand(sUpdateSQL);
            this.m_oDA.UpdateCommand.Connection = this.m_oCn;
            this.m_oDA.UpdateCommand.Parameters.Add(new EfzParameter("@UnitPrice", EfzType.Money, 8, "UnitPrice"));
            this.m_oDA.UpdateCommand.Parameters.Add(new EfzParameter("@UnitsInStock", EfzType.SmallInt, 2, "UnitsInStock"));
            this.m_oDA.UpdateCommand.Parameters.Add(new EfzParameter("@UnitsOnOrder", EfzType.SmallInt, 2, "UnitsOnOrder"));
            this.m_oDA.UpdateCommand.Parameters.Add(new EfzParameter("@ReorderLevel", EfzType.SmallInt, 2, "ReorderLevel"));
            this.m_oDA.UpdateCommand.Parameters.Add(new EfzParameter("@ProductID", EfzType.Int,  "PRODUCTID"));

            LoadData(false);
        }


        private void LoadData(bool bIgnoreErrors)
        {
            try
            {
                GetProducts();
                GetCategories();
            }
            catch (EfzException exSql)
            {
                if (bIgnoreErrors != true)
                {
                    throw (new Exception(exSql.Message, exSql));
                }
            }
            catch (Exception ex)
            {
                if (bIgnoreErrors != true)
                {
                    throw (new Exception(ex.Message, ex));
                }
            }
        }


        private void GetCategories()
        {
            try
            {
                // Get the categories via a DataReader
                EfzCommand oCmd = new EfzCommand("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", this.m_oCn);
                this.m_oCn.Open();
                EfzDataReader oDR = oCmd.ExecuteReader();
                // Clear the category list and fill it
                cboCategory.Items.Clear();
                while (oDR.Read())
                {
                    cboCategory.Items.Add(new Category((string)oDR["CategoryName"], (int)oDR["CategoryID"]));
                }
                oDR.Close();
                this.m_oCn.Close();
                // Select the first category
                cboCategory.SelectedIndex = 0;
            }
            catch (EfzException exSql)
            {
                throw (new Exception(exSql.Message, exSql));
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message, ex));
            }
        }

        private void GetProducts()
        {
            try
            {
                // Fill the Products DataSet.
                if (this.m_oDs.Tables["Products"] == null)
                {
                    this.m_oDA.Fill(this.m_oDs, "Products");
                }
                else
                {
                    this.m_oDs.Clear();
                    this.m_oDA.Fill(this.m_oDs, "Products");
                }

            }
            catch (EfzException exSql)
            {
                throw (new Exception(exSql.Message, exSql));
            }
            catch (Exception ex)
            {
                throw (new Exception(ex.Message, ex));
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                FilterProducts();
            }          
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


      

        private void FilterProducts()
        {
            try
            {
                // Get the selected category and cast it back to the Category class
                Category oCat = (Category)cboCategory.SelectedItem;

                // Create a DataView that filters the Products DataTable by the selected category
                DataView oDV = new DataView(this.m_oDs.Tables["Products"], "CategoryID = " + Convert.ToString(oCat.CategoryID), "ProductName", DataViewRowState.CurrentRows);

                // Clear all bindings
                cboProduct.DataBindings.Clear();
                txtUnitPrice.DataBindings.Clear();
                txtUnitsInStock.DataBindings.Clear();
                txtUnitsOnOrder.DataBindings.Clear();
                txtReorderLevel.DataBindings.Clear();

                // Bind the DataView to the controls

                cboProduct.DisplayMember = "PRODUCTNAME";
                cboProduct.ValueMember = "PRODUCTID";
                cboProduct.DataSource = oDV;
                //cboProduct.ValueMember = "ProductID";
                txtUnitPrice.DataBindings.Add("Text", oDV, "UnitPrice");
                txtUnitsInStock.DataBindings.Add("Text", oDV, "UnitsInStock");
                txtUnitsOnOrder.DataBindings.Add("Text", oDV, "UnitsOnOrder");
                txtReorderLevel.DataBindings.Add("Text", oDV, "ReorderLevel");

                // Select the first Product
                cboProduct.SelectedIndex = 0;

                txtReorderLevel.Enabled = true;
                txtUnitPrice.Enabled = true;
                txtUnitsOnOrder.Enabled = true;
                txtUnitsInStock.Enabled = true;
                btnCancel.Enabled = true;
                btnSave.Enabled = true;
            }
            catch (EfzException exSql)
            {
                txtReorderLevel.Enabled = false;
                txtUnitPrice.Enabled = false;
                txtUnitsOnOrder.Enabled = false;
                txtUnitsInStock.Enabled = false;
                btnCancel.Enabled = false;
                btnSave.Enabled = false;
                throw (new Exception(exSql.Message, exSql));
            }
            catch (Exception ex)
            {
                txtReorderLevel.Enabled = false;
                txtUnitPrice.Enabled = false;
                txtUnitsOnOrder.Enabled = false;
                txtUnitsInStock.Enabled = false;
                btnCancel.Enabled = false;
                btnSave.Enabled = false;
                throw (new Exception(ex.Message, ex));
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                int nPosition = BindingContext[this.m_oDs.Tables["Products"]].Position;
                this.m_oDs.Tables["Products"].Rows[nPosition].EndEdit();
                if (this.m_oDs.HasChanges())
                {
                    string sMsg = "Are you sure you want to cancel all changes?";
                    DialogResult oResponse = DialogResult.Yes;
                    oResponse = MessageBox.Show(sMsg, "Inventory", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                    if (oResponse == DialogResult.Yes)
                    {
                        this.m_oDs.RejectChanges();
                    }
                }
            }           
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            int nPosition = BindingContext[this.m_oDs.Tables["Products"]].Position;
            this.m_oDs.Tables["Products"].Rows[nPosition].EndEdit();

            if (this.m_oDs.HasChanges())
            {
                string sMsg = "Are you sure you want to save?";
                DialogResult oResponse = DialogResult.Yes;
                oResponse = MessageBox.Show(sMsg, "Inventory", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
                if (oResponse == DialogResult.Yes)
                {
                    try
                    {
                        //Update the DataSet and accept the changes.
                        this.m_oDA.Update(this.m_oDs, "Products");
                        this.m_oDs.AcceptChanges();
                    }                   
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

       

        private void button1_Click(object sender, EventArgs e)
        {
           
        }
    }


   



    public class Category
    {
      
        public int CategoryID;
        public string CategoryName;
       
        /// <summary>
        /// This method will setup the Category class for use. 
        /// </summary>
        /// <remarks overloaded="no">
        /// </remarks>
        /// <param/>
        public Category(string sCategoryName, int nCategoryID)
        {
            this.CategoryID = nCategoryID;
            this.CategoryName = sCategoryName;
        }
       
        public override string ToString()
        {
            return CategoryName;
        }
      
    }
}