using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM
{
    public class FieldCollection : IEnumerable<FieldValue>
    {
        private Dictionary<string, FieldValue> m_fields;

        internal FieldCollection()
        {
            m_fields = new Dictionary<string, FieldValue>(StringComparer.InvariantCultureIgnoreCase);
        }

        public int Count
        {
            get { return m_fields.Count; }
        }

        public object this[string fieldName]
        {
            get
            {
                lock (m_fields)
                {
                    if (!m_fields.ContainsKey(fieldName.ToLower()))
                    {
                        return null;
                        throw new ArgumentException(string.Format("Field named '{0}' not present", fieldName));
                    }
                    return m_fields[fieldName.ToLower()].Value;
                }
            }
            set
            {
                lock (m_fields)
                {
                    if (!m_fields.ContainsKey(fieldName.ToLower()))
                    {
                        m_fields.Add(fieldName.ToLower(), new FieldValue(fieldName, value));
                    }
                    else
                    {
                        m_fields[fieldName.ToLower()].Value = value;
                    }
                }
            }
        }

        internal void Add(string fieldName)
        {
            Add(fieldName, null);

        }

        internal void Add(string fieldName, Type fieldType)
        {
            Add(fieldName, fieldType, null);
        }

        internal void Add(FieldAttribute field)
        {
            Add(field.FieldName, null, null);
        }

        internal void Add(string fieldName, Type fieldType, object value)
        {
            lock (m_fields)
            {
                m_fields.Add(fieldName.ToLower(), new FieldValue(fieldName, fieldType, value));
            }
        }

        public IEnumerator<FieldValue> GetEnumerator()
        {
            return m_fields.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Type GetFieldType(string fieldName)
        {
            return m_fields[fieldName].ValueType;
        }

        public bool ContainsKey(string key)
        {
            return m_fields.ContainsKey(key.ToLower());
        }
    }
}
