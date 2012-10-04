using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM
{
    public class DynamicEntityInfo : SqlEntityInfo
    {
        public static bool CanChangeDefinitionsAtRuntime { get; set; }

        public static event EventHandler<EntityTypeChangedArgs> EntityDefinitionChanged;

        public bool AllowRuntimeChanges { get; set; }

        public bool Registered { get; internal set; }

        public DynamicEntityInfo(string entityName, IEnumerable<FieldAttribute> fields)
            : this(entityName, fields, KeyScheme.None)
        {
        }

        public DynamicEntityInfo(string entityName, IEnumerable<FieldAttribute> fields, KeyScheme keyScheme)
        {
            this.Registered = false;
            this.AllowRuntimeChanges = DynamicEntityInfo.CanChangeDefinitionsAtRuntime;

            var entityAttribute = new EntityAttribute(keyScheme);
            entityAttribute.NameInStore = entityName;
            this.EntityType = typeof(DynamicEntityInfo);
            this.Initialize(entityAttribute, this.EntityType);
            if (fields != null) this.Fields.AddRange(fields);
        }

        public void AddField(string fieldName, System.Data.DbType fieldType, int length)
        {
            AddField(fieldName, fieldType, length, false, false, FieldSearchOrder.NotSearchable);
        }

        public void AddField(string fieldName, System.Data.DbType fieldType, int length, bool isPrimaryKey, bool requireUnique, FieldSearchOrder searchOrder)
        {
            if (this.Registered & !this.AllowRuntimeChanges) throw new InvalidOperationException("The Entity is already registered, it cannot be modified");

            var attr = new FieldAttribute();
            attr.FieldName = fieldName;
            attr.DataType = fieldType;
            attr.AllowsNulls = !isPrimaryKey & !this.Registered;
            attr.IsPrimaryKey = isPrimaryKey;
            attr.IsIdentity = EntityAttribute.KeyScheme == KeyScheme.Identity & isPrimaryKey;
            attr.Length = length;
            attr.RequireUniqueValue = requireUnique;
            attr.SearchOrder = searchOrder;
            this.Fields.Add(attr);
            if (EntityDefinitionChanged != null)
                EntityDefinitionChanged.Invoke(this, new EntityTypeChangedArgs(this, attr));
        }
    }
}
