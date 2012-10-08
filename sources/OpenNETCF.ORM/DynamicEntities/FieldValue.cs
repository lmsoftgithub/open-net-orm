using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM
{
    public class FieldValue
    {
        internal FieldValue(string name, object value)
        {
            Name = name;
            Value = value;
        }
        internal FieldValue(string name, Type type, object value)
        {
            Name = name;
            ValueType = type;
            Value = value;
        }

        public string Name { get; private set; }
        private object _value;
        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                if (value != null && ValueType != null && !ValueType.Equals(value.GetType()))
                    throw new InvalidCastException(String.Format("Cannot cast '{0}' to '{1}' for field {2}", ValueType.ToString(), value.GetType().ToString(), Name));
                this._value = value;
            }
        }
        internal Type ValueType { get; set; }

        public override string ToString()
        {
            if (ValueType == null)
                return string.Format("{0}: {1}", Name, Value);
            else
                return string.Format("({2}) {0}: {1}", Name, Value, ValueType);
        } 
    }
}
