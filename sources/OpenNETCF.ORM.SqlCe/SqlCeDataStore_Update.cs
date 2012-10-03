using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;
using System.Reflection;

namespace OpenNETCF.ORM
{
    partial class SqlCeDataStore
    {
        protected override void Update(object item, bool cascadeUpdates, string fieldName, IDbConnection connection, IDbTransaction transaction)
        {
            object keyValue;
            var itemType = item.GetType();
            string entityName = m_entities.GetNameForType(itemType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(itemType);
            }

            if (Entities[entityName].Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform Updates");
            }

            Boolean bInheritedConnection = connection != null;
            if (transaction == null && connection == null)
                connection = GetConnection(false);
            try
            {
                CheckPrimaryKeyIndex(entityName);

                OnBeforeUpdate(item, cascadeUpdates, fieldName);
                var start = DateTime.Now;

                using (var command = new SqlCeCommand())
                {
                    if (transaction == null)
                        command.Connection = connection as SqlCeConnection;
                    else
                        command.Transaction = transaction as SqlCeTransaction;
                    // TODO: Update doesn't support multiple Primary Keys. Need to be checked before we use TableDirect.
                    command.CommandText = entityName;
                    command.CommandType = CommandType.TableDirect;
                    command.IndexName = Entities[entityName].PrimaryKeyIndexName;
                    using (var results = command.ExecuteResultSet(ResultSetOptions.Scrollable | ResultSetOptions.Updatable))
                    {
                        keyValue = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);
                        var ordinals = GetOrdinals(entityName, results);
                        // seek on the PK
                        var found = results.Seek(DbSeekOptions.BeforeEqual, new object[] { keyValue });

                        if (!found)
                        {
                            // TODO: the PK value has changed - we need to store the original value in the entity or diallow this kind of change
                            throw new RecordNotFoundException("Cannot locate a record with the provided primary key.  You cannot update a primary key value through the Update method");
                        }

                        results.Read();

                        // update the values
                        foreach (var field in Entities[entityName].Fields)
                        {
                            // do not update PK fields
                            if (field.IsPrimaryKey)
                            {
                                continue;
                            }
                            else if (fieldName != null && field.FieldName != fieldName)
                            {
                                continue; // if we pass in a field name, skip over any fields that don't match
                            }
                            else if (field.DataType == DbType.Object)
                            {
                                // get serializer
                                var serializer = GetSerializer(itemType);

                                if (serializer == null)
                                {
                                    throw new MissingMethodException(
                                        string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                        field.FieldName, entityName));
                                }
                                var value = serializer.Invoke(item, new object[] { field.FieldName });
                                results.SetValue(ordinals[field.FieldName], value);
                            }
                            else if (field.IsRowVersion)
                            {
                                // read-only, so do nothing
                            }
                            else if (field.PropertyInfo.PropertyType.UnderlyingTypeIs<TimeSpan>())
                            {
                                // SQL Compact doesn't support Time, so we're convert to ticks in both directions
                                var value = field.PropertyInfo.GetValue(item, null);
                                if (value == null)
                                {
                                    results.SetValue(ordinals[field.FieldName], DBNull.Value);
                                }
                                else
                                {
                                    var ticks = ((TimeSpan)value).Ticks;
                                    results.SetValue(ordinals[field.FieldName], ticks);
                                }
                            }
                            else
                            {
                                var value = field.PropertyInfo.GetValue(item, null);

                                // TODO: should we update only if it's changed?  Does it really matter at this point?
                                results.SetValue(ordinals[field.FieldName], value);
                            }
                        }
                        results.Update();
                    }
                }
                if (cascadeUpdates)
                {
                    CascadeUpdates(item, fieldName, keyValue, m_entities[entityName], connection, transaction);
                }
                OnAfterUpdate(item, cascadeUpdates, fieldName, DateTime.Now.Subtract(start), "tableDirect");
            }
            finally
            {
                if (!bInheritedConnection) DoneWithConnection(connection, false);
            }
        }
    }
}
