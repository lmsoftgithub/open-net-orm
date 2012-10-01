using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM.MainDemo
{
    public static class Logger
    {
        public static void LogException(String methodname, System.Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(
                String.Format("{0}():\n{1}\n\n{2}",
                    methodname,
                    ex.Message,
                    ex.StackTrace)
                , "Error"
                , System.Windows.Forms.MessageBoxButtons.OK
                , System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
