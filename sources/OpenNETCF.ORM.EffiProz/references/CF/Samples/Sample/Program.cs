using System;
using System.Data.Common;
using System.Data.EffiProz;

namespace InMemorySample
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string connString = "Connection Type=File ; Initial Catalog=/Storage Card/TestDB; User=sa; Password=;"; //for file DB
            //string connString = "Connection Type=Memory ; Initial Catalog=TestDB; User=sa; Password=;";           
            using (DbConnection conn = new EfzConnection(connString))
            {    
                conn.Open();

                DbCommand command = conn.CreateCommand();
                command.CommandText = "CREATE TABLE Test(ID INT PRIMARY KEY, Name VARCHAR(100));";
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO Test(ID , Name) VALUES(@ID , @Name);";
                DbParameter id = command.CreateParameter();
                id.ParameterName = "@ID";
                id.Value = 1;
                command.Parameters.Add(id);

                DbParameter name = command.CreateParameter();
                name.ParameterName = "@NAME";
                name.Value = "Van";
                command.Parameters.Add(name);
                command.ExecuteNonQuery();

                id.Value = 2;
                name.Value = "Car";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT * FROM TEST;";
                DbDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    System.Console.WriteLine(String.Format("ID= {0} , Name= {1}",
                        reader.GetInt32(0), reader.GetString(1)));
                }

                Console.WriteLine("Press Any Key to Continue...");
              

            }
        }
    }
}
