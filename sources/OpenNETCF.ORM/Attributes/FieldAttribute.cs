using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;

namespace OpenNETCF.ORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        private DbType m_type;
        
        public FieldAttribute()
        {
            // set up defaults
            AllowsNulls = true;
            IsPrimaryKey = false;
            SearchOrder = FieldSearchOrder.NotSearchable;
            RequireUniqueValue = false;
            Ordinal = -1;
            IsRowVersion = false;
            SortOrder = FieldSearchOrder.NotSearchable;
            SortSequence = 0;
        }

        public string FieldName { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool AllowsNulls { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool RequireUniqueValue { get; set; }
        public int Ordinal { get; set; }
        public FieldSearchOrder SearchOrder { get; set; }
        public String Default { get; set; }
        public FieldSearchOrder SortOrder { get; set; }
        public int SortSequence { get; set; }

        /// <summary>
        /// rowversion or timestamp time for Sql Server
        /// </summary>
        public bool IsRowVersion { get; set; }
 
        public PropertyInfo PropertyInfo { get; internal set; }
        internal bool DataTypeIsValid { get; private set; }

        public DbType DataType 
        {
            get { return m_type; }
            set
            {
                m_type = value;
                DataTypeIsValid = true;
            }
        }

        public override string ToString()
        {
            return FieldName;
        }

        public static string[] GetNames(Type enumType)  
        {  
            FieldInfo[] fieldInfo = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);  
            return fieldInfo.Select(f => f.Name).ToArray();  
        }

        public static string GetName(Type enumType, int value)  
        {  
            FieldInfo[] fieldInfo = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            return fieldInfo[value].Name;
        }
    }
}
