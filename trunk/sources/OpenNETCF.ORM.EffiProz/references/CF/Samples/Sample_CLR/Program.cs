using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.Data.EffiProz;

namespace Sample_CLR
{
    class Program
    {
        static void Main(string[] args)
        {          
            ClrFunctionTest();
        }

        public static void ClrFunctionTest()
        {
            string connString = "Connection Type=Memory ;Initial Catalog=CLRSampleDB; User=sa; Password=;";

            using (DbConnection cnn = new EfzConnection(connString))
            {
                cnn.Open();

                using (DbCommand cmd = cnn.CreateCommand())
                {
                    string sql = "CREATE FUNCTION add_num(x INT,  y INT)\n" +
                                         "RETURNS INT\n NO SQL\n" +
                                         "LANGUAGE DOTNET\n EXTERNAL NAME 'ClrRoutines:EffiProz.Samples.ClrRoutines.Add'";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT add_num(3,4) from dual;";
                    int result = (int)cmd.ExecuteScalar();

                    Console.WriteLine("Result: {0}", result);
                }
            }
        }
    }
}
