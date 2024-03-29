﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.ORM
{
    public class EntityInfo
    {
        protected EntityInfo()
        {
            Fields = new FieldAttributeCollection();
            References = new ReferenceAttributeCollection();
            SortingFields = new FieldAttributeCollection();
        }

        internal void Initialize(EntityAttribute entityAttribute, Type entityType)
        {
            EntityAttribute = entityAttribute;
            EntityType = entityType;
        }

        public Type EntityType { get; protected set; }

        public FieldAttributeCollection Fields { get; private set; }
        public ReferenceAttributeCollection References { get; private set; }
        public FieldAttributeCollection SortingFields { get; private set; }

        public EntityAttribute EntityAttribute { get; set; }

        public delegate object CreateProxyDelegate(System.Data.IDataReader reader, Dictionary<string, int> ordinals);
        public CreateProxyDelegate CreateProxy { get; internal set; }

        public delegate object SerializerDelegate(object item, string fieldName);
        public SerializerDelegate Serializer { get; internal set; }

        public delegate object DeserializerDelegate(object item, string fieldName, object value);
        public DeserializerDelegate Deserializer { get; internal set; }

        public System.Reflection.ConstructorInfo DefaultConstructor { get; internal set; }

        public string EntityName 
        {
            get
            {
                return EntityAttribute.NameInStore;
            }
        }

        public override string ToString()
        {
            return EntityName;
        }

        protected void AddField(FieldAttribute field)
        {
            Fields.Add(field);
        }

        public object CreateInstance()
        {
            object result = null;
            if (this.DefaultConstructor != null)
            {
                result = this.DefaultConstructor.Invoke(null);
            }
            return result;
        }
    }
}
