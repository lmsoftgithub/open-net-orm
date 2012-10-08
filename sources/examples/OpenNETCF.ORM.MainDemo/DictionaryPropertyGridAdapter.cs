using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace OpenNETCF.ORM.MainDemo
{
    class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        DynamicEntity _dynamic;

        public DynamicEntity EntityItem { get { return _dynamic; } }

        public DictionaryPropertyGridAdapter(DynamicEntity d)
        {
            _dynamic = d;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return _dynamic.EntityName;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            foreach (var f in _dynamic.Fields)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dynamic, f.Name));
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dynamic;
        }
    }

    class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        DynamicEntity _dynamic;
        string _key;

        internal DictionaryPropertyDescriptor(DynamicEntity d, string key)
            : base(key.ToString(), null)
        {
            _dynamic = d;
            _key = key;
        }
        public override Type PropertyType
        {
            get {
                if (_dynamic.Fields.GetFieldType(_key) != null)
                    return _dynamic.Fields.GetFieldType(_key);
                if (_dynamic[_key] == null)
                    return typeof(object);
                return _dynamic[_key].GetType();
            }
        }
        public override void SetValue(object component, object value)
        {
            _dynamic[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dynamic[_key];
        }
        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
