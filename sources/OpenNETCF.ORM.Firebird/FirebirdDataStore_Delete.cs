using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;

namespace OpenNETCF.ORM
{
    public partial class FirebirdDataStore
    {

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Delete<T>()
        {
            Delete(typeof(T));
        }

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Delete(System.Type entityType)
        {
            string entityName = m_entities.GetNameForType(entityType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }

            // TODO: handle cascade deletes?

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("DELETE FROM {0}", entityName);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }


        public override void Delete<T>(string fieldName, object matchValue)
        {
            Delete(typeof(T), fieldName, matchValue);
        }

        /// <summary>
        /// Deletes entities of a given type where the specified field name matches a specified value
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="indexName"></param>
        /// <param name="matchValue"></param>
        protected void Delete(Type entityType, string fieldName, object matchValue)
        {
            string entityName = m_entities.GetNameForType(entityType);

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("DELETE FROM {0} WHERE {1} = {2}", entityName, fieldName, "@" + fieldName);
                    var param = CreateParameterObject("@" + fieldName, matchValue);
                    command.Parameters.Add(param);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        /// <summary>
        /// Deletes entities of a given type where the specified field names match the specified values
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="fieldNames"></param>
        /// <param name="matchValues"></param>
        protected void Delete(Type entityType, List<String> fieldNames, List<object> matchValues)
        {
            string entityName = m_entities.GetNameForType(entityType);

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    String sql = string.Format("DELETE FROM {0} WHERE ", entityName);
                    for (int i = 0; i < fieldNames.Count; i++)
                    {
                        if (i > 0) sql += " AND ";
                        sql += fieldNames[i] + " = @" + fieldNames[i];
                        var param = CreateParameterObject("@" + fieldNames[i], matchValues[i]);
                        command.Parameters.Add(param);
                    }
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        /// <summary>
        /// Deletes the specified entity instance from the DataStore
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>
        /// The instance provided must have a valid primary key value
        /// </remarks>
        public override void Delete(object item)
        {
            var type = item.GetType();
            string entityName = m_entities.GetNameForType(type);

            if (entityName == null)
            {
                throw new EntityNotFoundException(type);
            }

            if (Entities[entityName].Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform a Delete");
            }

            if (FieldAttributeCollection.AllowMultiplePrimaryKeyFields && Entities[entityName].Fields.KeyFields.Count > 1)
            {
                List<String> keys = new List<String>();
                List<object> values = new List<object>();
                foreach (FieldAttribute field in Entities[entityName].Fields.KeyFields)
                {
                    keys.Add(field.FieldName);
                    values.Add(field.PropertyInfo.GetValue(item, null));
                }
                Delete(type, keys, values);
            }
            else
            {
                var keyValue = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);
                Delete(type, keyValue);
            }
        }

        /// <summary>
        /// Deletes an entity instance with the specified primary key from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        public override void Delete<T>(object primaryKey)
        {
            Delete(typeof(T), primaryKey);
        }

        protected virtual void Delete(Type t, object primaryKey)
        {
            string entityName = m_entities.GetNameForType(t);

            if (entityName == null)
            {
                throw new EntityNotFoundException(t);
            }

            if (Entities[entityName].Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform a Delete");
            }

            // handle cascade deletes
            foreach (var reference in Entities[entityName].References)
            {
                if (!reference.CascadeDelete) continue;

                Delete(reference.ReferenceEntityType, reference.ReferenceField, primaryKey);
            }

            var keyFieldName = Entities[entityName].Fields.KeyField.FieldName;
            Delete(t, keyFieldName, primaryKey);
        }

    }
}
