using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM
{
    public class GenericComparer : IComparer
    {
        private enum CompareDirection
        {
            Ascending,
            Descending
        }
        public GenericComparer()
        {
        }
        public GenericComparer(string propertyName)
        {
            this.PropertyName = propertyName;
            if (this.PropertyName == null || this.PropertyName.Length <= 0) throw new Exception("Invalid property name");
        }

        public string PropertyName { get; set; }

        public int Compare(object x, object y)
        {
            if (this.PropertyName == null || this.PropertyName.Length <= 0) throw new Exception("Invalid property name");
            Type t = x.GetType();
            string[] sortExpressions = this.PropertyName.Trim().Split(',');
            for (int i = 0; i < sortExpressions.Length; i++)
            {
                string fieldName = String.Empty;
                CompareDirection direction = CompareDirection.Ascending;
                if (sortExpressions[i].Trim().Contains(" "))
                {
                    String[] parts = sortExpressions[i].Trim().Split(new[] { ' ' });
                    if (parts.Length > 0) fieldName = parts[0].Trim();
                    if (parts.Length > 1 && parts[1].Trim().ToLower() == "descending") direction = CompareDirection.Descending;
                }
                if (fieldName.Length > 0)
                {
                    System.Reflection.PropertyInfo prop = t.GetProperty(fieldName);
                    if (prop != null)
                    {
                        int iResult = System.Collections.Comparer.DefaultInvariant.Compare(prop.GetValue(x, null), prop.GetValue(y, null));
                        if (iResult != 0)
                        {
                            //Return if not equal
                            if (direction == CompareDirection.Descending)
                            {
                                //Invert order
                                return -iResult;
                            }
                            else
                            {
                                return iResult;
                            }
                        }
                    }
                    else
                    {
                        throw new Exception(fieldName + " doesn't exist in the Class.");
                    }
                }
            }
            //No change to order
            return 0;
        }
    }
}
