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
        private DbType db_type;
        private Type m_type;

        public FieldAttribute()
        {
            // set up defaults
            AllowsNulls = true;
            IsPrimaryKey = false;
            SearchOrder = FieldOrder.None;
            RequireUniqueValue = false;
            IsRowVersion = false;
            SortOrder = FieldOrder.None;
            SortSequence = 0;
        }

        private string _fieldName;
        public string FieldName {
            get { return _fieldName; }
            set
            {
                if (!Extensions.MatchFieldNamingConstraints(value)) throw new InvalidElementNameException(value);
                _fieldName = value;
            }
        }
        public int Length { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool AllowsNulls { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool RequireUniqueValue { get; set; }
        public FieldOrder SearchOrder { get; set; }
        public String Default { get; set; }
        public FieldOrder SortOrder { get; set; }
        public int SortSequence { get; set; }
        public string IndexName { get; set; }

        /// <summary>
        /// rowversion or timestamp time for Sql Server
        /// </summary>
        public bool IsRowVersion { get; set; }
 
        public PropertyInfo PropertyInfo { get; internal set; }
        internal bool DataTypeIsValid { get; private set; }

        public DbType DataType 
        {
            get { return db_type; }
            set
            {
                db_type = value;
                m_type = Extensions.ToManagedType(db_type);
                DataTypeIsValid = true;
            }
        }

        public Type ManagedDataType
        {
            get { return m_type; }
            internal set
            {
                m_type = value;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})",FieldName,this.DataType);
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

        public override bool Equals(object obj)
        {
            var f = obj as FieldAttribute;
            if (f != null)
            {
                if (!(this.AllowsNulls & f.AllowsNulls)) return false;
                if (!(this.IsPrimaryKey & f.IsPrimaryKey)) return false;
                if (!(this.IsRowVersion & f.IsRowVersion)) return false;
                if (!(this.DataType == f.DataType)) return false;
                if (!(this.Default == f.Default)) return false;
                if (!(this.FieldName.Equals(FieldName, StringComparison.InvariantCultureIgnoreCase))) return false;
                if (!(this.Length == f.Length)) return false;
                if (!(this.RequireUniqueValue & f.RequireUniqueValue)) return false;
                if (!(this.SearchOrder == f.SearchOrder)) return false;
                if (!(this.SortOrder == f.SortOrder)) return false;
                if (!(this.Scale == f.Scale)) return false;
                if (!(this.Precision == f.Precision)) return false;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
