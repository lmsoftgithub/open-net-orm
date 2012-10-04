using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM
{
    public class DynamicEntity
    {
        public string EntityName { get; private set; }
        public FieldCollection Fields { get; private set; }

        public DynamicEntity(string entityName)
            : this(entityName, null)
        {
        }

        public object this[String fieldName]
        {
            get { return Fields[fieldName]; }
            set { Fields[fieldName] = value; }
        }

        public DynamicEntity(string entityName, FieldAttributeCollection fields)
        {
            EntityName = entityName;
            Fields = new FieldCollection();

            if (fields != null && fields.Count > 0)
            {
                foreach (var f in fields)
                {
                    this.Fields.Add(f.FieldName, f.ManagedDataType);
                }
            }
            else
            {
                throw new ArgumentNullException("Fields", "DynamicEntity does not accept a null field collection");
            }
        }

        public DynamicEntity(DynamicEntityInfo entityInfo)
        {
            if (entityInfo == null) throw new ArgumentNullException("DynamicEntityInfo", "A DynamicEntity cannot be instanciated without a valid DynamicEntityInfo");
            EntityName = entityInfo.EntityName;
            Fields = new FieldCollection();

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
        }

    }
}
