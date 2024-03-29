﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace OpenNETCF.ORM
{
    public class FieldAttributeCollection : IEnumerable<FieldAttribute>
    {
        private Dictionary<string, FieldAttribute> m_fields = new Dictionary<string, FieldAttribute>();
        static public Boolean AllowMultiplePrimaryKeyFields = false;

        public bool OrdinalsAreValid { get; set; }
        public FieldAttribute KeyField { get; private set; }
        public List<FieldAttribute> KeyFields { get; private set; }
        
        internal FieldAttributeCollection()
        {
            OrdinalsAreValid = false;
            KeyField = null;
            KeyFields = new List<FieldAttribute>();
        }

        internal void Add(FieldAttribute attribute)
        {
            if (attribute.IsPrimaryKey)
            {
                if (KeyField == null)
                {
                    KeyField = attribute;
                    KeyFields.Add(attribute);
                }
                else if (AllowMultiplePrimaryKeyFields)
                {
                    KeyFields.Add(attribute);
                }
                else
                {
                    throw new MutiplePrimaryKeyException(KeyField.FieldName);
                }
            }

            m_fields.Add(attribute.FieldName.ToLower(), attribute);
        }

        internal void AddRange(IEnumerable<FieldAttribute> fields)
        {
            lock (m_fields)
            {
                foreach (var f in fields)
                {
                    Add(f);
                }
            }
        }

        public int Count
        {
            get { return m_fields.Count; }
        }

        public FieldAttribute this[string fieldName]
        {
            get { return m_fields[fieldName.ToLower()]; }
        }

        public IEnumerator<FieldAttribute> GetEnumerator()
        {
            return m_fields.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_fields.Values.GetEnumerator();
        }

        internal void Remove(FieldAttribute field)
        {
            if (m_fields.ContainsKey(field.FieldName.ToLower()))
                m_fields.Remove(field.FieldName.ToLower());
        }

        public bool HasField(String fieldName)
        {
            return m_fields.ContainsKey(fieldName.ToLower());
        }
    }
}
