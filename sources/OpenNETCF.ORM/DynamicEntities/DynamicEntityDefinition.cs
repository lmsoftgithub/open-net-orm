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

        public DynamicEntityInfo(System.Xml.XmlDocument xdoc)
        {
            //TODO: Implement XML Support for Dynamic entities
            throw new NotImplementedException();
        }

        public DynamicEntityInfo(string entityName, IEnumerable<FieldAttribute> fields, IEnumerable<ReferenceAttribute> references)
            : this(entityName, fields, references, KeyScheme.None, null, null)
        {
        }

        public DynamicEntityInfo(string entityName, IEnumerable<FieldAttribute> fields, IEnumerable<ReferenceAttribute> references, KeyScheme keyScheme)
            : this(entityName, fields, references, keyScheme, null, null)
        {
        }

        public DynamicEntityInfo(string entityName, IEnumerable<FieldAttribute> fields, IEnumerable<ReferenceAttribute> references, KeyScheme keyScheme, EntityInfo.SerializerDelegate serializer, EntityInfo.DeserializerDelegate deserializer)
        {
            this.Registered = false;
            this.AllowRuntimeChanges = DynamicEntityInfo.CanChangeDefinitionsAtRuntime;

            if (entityName == null || entityName.Length <= 0)
                throw new ArgumentException("EntityName", "A DynamicEntityInfo does not accept a null or empty EntityName");

            var entityAttribute = new EntityAttribute(keyScheme);
            entityAttribute.NameInStore = entityName;
            this.EntityType = typeof(DynamicEntityInfo);
            this.Initialize(entityAttribute, this.EntityType);
            this.Serializer = serializer;
            this.Deserializer = deserializer;
            if (fields != null) this.Fields.AddRange(fields);
            if (references != null) this.References.AddRange(references);
        }

        public void AddField(string fieldName, System.Data.DbType fieldType, int length)
        {
            AddField(fieldName, fieldType, length, false, false, FieldOrder.None);
        }

        public void AddField(string fieldName, System.Data.DbType fieldType, int length, bool isPrimaryKey, bool requireUnique, FieldOrder searchOrder)
        {
            if (this.Registered & !this.AllowRuntimeChanges) throw new InvalidOperationException("The Entity is already registered, it cannot be modified");
            if (this.Registered & isPrimaryKey) throw new ArgumentException("The Entity is already registered, you cannot add a primary key", "isPrimaryKey");
            var attr = new FieldAttribute();
            attr.FieldName = fieldName;
            attr.DataType = fieldType;
            attr.AllowsNulls = !isPrimaryKey;
            attr.IsPrimaryKey = isPrimaryKey;
            attr.IsIdentity = EntityAttribute.KeyScheme == KeyScheme.Identity & isPrimaryKey & this.Fields.KeyFields.Count <= 0;
            attr.Length = length;
            attr.RequireUniqueValue = requireUnique;
            attr.SearchOrder = searchOrder;
            this.Fields.Add(attr);
            if (this.Registered && EntityDefinitionChanged != null)
                EntityDefinitionChanged.Invoke(this, new EntityTypeChangedArgs(this, attr));
        }

        public void AddReference(Type referenceType, string referenceField, bool isArray, ReferenceType refType)
        {
            if (this.Registered & !this.AllowRuntimeChanges) throw new InvalidOperationException("The Entity is already registered, it cannot be modified");

            var attr = new ReferenceAttribute(referenceType, referenceField);
            attr.ReferenceType = ReferenceType.OneToMany;
            if (refType == ReferenceType.ManyToMany) attr.ReferenceType = refType;
            attr.IsArray = isArray;
            attr.IsList = !isArray;

            this.References.Add(attr);
            if (this.Registered && EntityDefinitionChanged != null)
                EntityDefinitionChanged.Invoke(this, new EntityTypeChangedArgs(this, attr));
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.EntityName);
            if (!this.Registered)
                sb.Append("*");
            sb.AppendFormat(" ({0})", this.Fields.Count);
            return sb.ToString();
        }

        public System.Xml.XmlDocument ToXML()
        {
            //System.Xml.XmlDocument xdoc = null;
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                //xdoc = null;
                throw ex;
            }
            //return xdoc;
        }
    }
}
