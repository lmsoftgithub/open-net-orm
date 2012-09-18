using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Data.EffiProz;

namespace AdoDotNetSample
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            //isolated storage database
            string connectionString = "connection type=FILE; initial catalog=TestDb; user=SA; password=";
            using (EfzConnection conn = new EfzConnection(connectionString))
            {
                conn.Open();
                string sql = "CREATE TABLE Test(ID int, Name varchar(100));";
                EfzCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                sql = "INSERT INTO Test(ID , Name ) VALUES(2,'Bus');";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                sql = "SELECT * FROM TEST;";
                cmd.CommandText = sql;
                EfzDataReader reader = cmd.ExecuteReader();

                reader.Read();

                tbkText.Text = String.Format("ID = {0}, Name = {1} ", reader.GetInt32(0), reader.GetString(1));

            }
        }

        private void btnMem_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "connection type=MEMORY; initial catalog=TestDb; user=SA; password=";
            using (EfzConnection conn = new EfzConnection(connectionString))
            {
                conn.Open();
                string sql = "CREATE TABLE Test(ID int, Name varchar(100));";
                EfzCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                sql = "INSERT INTO Test(ID , Name ) VALUES(1,'Car');";
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                sql = "SELECT * FROM TEST;";
                cmd.CommandText = sql;
                EfzDataReader reader = cmd.ExecuteReader();

                reader.Read();

                tbkText.Text = String.Format("ID = {0}, Name = {1} ", reader.GetInt32(0), reader.GetString(1));

            }
        } 
    }
}
