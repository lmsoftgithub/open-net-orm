using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM
{
    public class DynamicEntity
    {
        public string EntityName { get; private set; }
        public DynamicEntityInfo EntityInfo { get; private set; }
        public FieldCollection Fields { get; private set; }

        public DynamicEntity(DynamicEntityInfo entityInfo)
        {
            this.EntityInfo = entityInfo;
            if (entityInfo == null) throw new ArgumentNullException("DynamicEntityInfo", "A DynamicEntity cannot be instanciated without a valid DynamicEntityInfo");
            this.EntityName = entityInfo.EntityName;
            this.Fields = new FieldCollection();

            if (this.EntityName == null || this.EntityName.Length <= 0)
                throw new ArgumentException("EntityName", "A DynamicEntity does not accept a null or empty EntityName");

            if (entityInfo.Fields != null && entityInfo.Fields.Count > 0)
            {
                foreach (var f in entityInfo.Fields)
                {
                    this.Fields.Add(f.FieldName, f.ManagedDataType);
                }
            }
            else
            {
                throw new ArgumentNullException("DynamicEntityInfo", "DynamicEntity does not accept a null field collection");
            }
            if (entityInfo.EntityAttribute.KeyScheme == KeyScheme.GUID
                && entityInfo.Fields.KeyField != null
                && entityInfo.Fields.KeyField.DataType == System.Data.DbType.Guid)
            {
                this[entityInfo.Fields.KeyField.FieldName] = System.Guid.NewGuid();
            }
        }

        public object this[String fieldName]
        {
            get { return Fields[fieldName]; }
            set { Fields[fieldName] = value; }
        }

        public bool HasField(string fieldName)
        {
            return this.Fields.ContainsKey(fieldName);
        }
    }
}
