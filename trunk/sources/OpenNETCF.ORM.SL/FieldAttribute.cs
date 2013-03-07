using System;
using System.Reflection;

namespace OpenNETCF.ORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        public FieldAttribute()
        {
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
        public FieldOrder SearchOrder { get; set; }
        public String Default { get; set; }
        public FieldOrder SortOrder { get; set; }
        public int SortSequence { get; set; }

        /// <summary>
        /// rowversion or timestamp time for Sql Server
        /// </summary>
        public bool IsRowVersion { get; set; }

        public PropertyInfo PropertyInfo { get; internal set; }
        internal bool DataTypeIsValid { get; private set; }

        public System.Data.DbType DataType
        {
            get { return System.Data.DbType.Unknown; }
            set
            {
                DataTypeIsValid = true;
            }
        }

        public override string ToString()
        {
            return FieldName;
        }
    }
}
