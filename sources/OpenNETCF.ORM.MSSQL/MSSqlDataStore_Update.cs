using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace OpenNETCF.ORM
{
    partial class MSSqlDataStore
    {

        protected override void Update(object item, bool cascadeUpdates, string fieldName, IDbConnection connection, IDbTransaction transaction)
        {
            object keyValue;
            var changeDetected = false;
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

            if (transaction == null && connection == null) connection = GetConnection(false);
            try
            {
                CheckOrdinals(entityName);
                CheckPrimaryKeyIndex(entityName);

                OnBeforeUpdate(item, cascadeUpdates, fieldName);
                var start = DateTime.Now;

                using (var command = GetNewCommandObject())
                {

                    if (transaction == null)
                    {
                        command.Connection = connection as SqlConnection;
                    }
                    else
                    {
                        command.Connection = transaction.Connection as SqlConnection;
                        command.Transaction = transaction as SqlTransaction;
                    }
                    StringBuilder sql = new StringBuilder();
                    StringBuilder where = new StringBuilder(" WHERE ");

                    sql.Append("SELECT ");
                    int count = Entities[entityName].Fields.Count;
                    int keycount = Entities[entityName].Fields.KeyFields.Count;
                    foreach (FieldAttribute field in Entities[entityName].Fields)
                    {
                        sql.Append(field.FieldName);
                        if (Entities[entityName].Fields.KeyFields.Contains(field))
                        {
                            where.AppendFormat(" [{0}] = @{0} ", field.FieldName);
                            keyValue = field.PropertyInfo.GetValue(item, null);
                            command.Parameters.Add(new SqlParameter(String.Format("@{0}", field.FieldName), keyValue));
                            if (--keycount > 0) where.Append(" AND ");
                        }
                        if (--count > 0) sql.Append(", ");
                    }
                    sql.AppendFormat(" FROM {0} {1}", entityName, where.ToString());

                    command.CommandText = sql.ToString();
                    command.CommandType = CommandType.Text;

                    var updateSQL = new StringBuilder(string.Format("UPDATE {0} SET ", entityName));

                    using (var reader = command.ExecuteReader() as SqlDataReader)
                    {

                        if (!reader.HasRows)
                        {
                            // TODO: the PK value has changed - we need to store the original value in the entity or diallow this kind of change
                            throw new RecordNotFoundException("Cannot locate a record with the provided primary key.  You cannot update a primary key value through the Update method");
                        }

                        reader.Read();

                        using (var insertCommand = GetNewCommandObject())
                        {
                            keyValue = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);
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
                                else if (field.IsRowVersion)
                                {
                                    // read-only, so do nothing
                                }
                                else if (field.DataType == DbType.Object)
                                {
                                    changeDetected = true;
                                    // get serializer
                                    var serializer = GetSerializer(itemType);

                                    if (serializer == null)
                                    {
                                        throw new MissingMethodException(
                                            string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                            field.FieldName, entityName));
                                    }
                                    var value = serializer.Invoke(item, new object[] { field.FieldName });

                                    if (value == null)
                                    {
                                        updateSQL.AppendFormat("{0}=NULL, ", field.FieldName);
                                    }
                                    else
                                    {
                                        updateSQL.AppendFormat("{0}=@{0}, ", field.FieldName);
                                        insertCommand.Parameters.Add(new SqlParameter("@" + field.FieldName, value));
                                    }
                                }
                                else if (field.PropertyInfo.PropertyType.UnderlyingTypeIs<TimeSpan>())
                                {
                                    changeDetected = true;
                                    // SQL Compact doesn't support Time, so we're convert to ticks in both directions
                                    var value = field.PropertyInfo.GetValue(item, null);
                                    if (value == null)
                                    {
                                        updateSQL.AppendFormat("{0}=NULL, ", field.FieldName);
                                    }
                                    else
                                    {
                                        var ticks = ((TimeSpan)value).Ticks;
                                        updateSQL.AppendFormat("{0}=@{0}, ", field.FieldName);
                                        insertCommand.Parameters.Add(new SqlParameter("@" + field.FieldName, ticks));
                                    }
                                }
                                else
                                {
                                    var value = field.PropertyInfo.GetValue(item, null);
                                    var readerval = reader[field.FieldName];
                                    if (DBNull.Value.Equals(readerval) || !value.Equals(readerval))
                                    {
                                        changeDetected = true;

                                        if (value == null)
                                        {
                                            updateSQL.AppendFormat("{0}=NULL, ", field.FieldName);
                                        }
                                        else
                                        {
                                            updateSQL.AppendFormat("{0}=@{0}, ", field.FieldName);
                                            insertCommand.Parameters.Add(new SqlParameter("@" + field.FieldName, value));
                                        }
                                    }
                                }
                            }
                            reader.Close();

                            // only execute if a change occurred
                            if (changeDetected)
                            {
                                // remove the trailing comma and append the filter
                                updateSQL.Length -= 2;
                                updateSQL.Append(where.ToString());
                                foreach (FieldAttribute field in Entities[entityName].Fields.KeyFields)
                                {
                                    keyValue = field.PropertyInfo.GetValue(item, null);
                                    insertCommand.Parameters.Add(new SqlParameter(String.Format("@{0}", field.FieldName), keyValue));
                                }
                                insertCommand.CommandText = updateSQL.ToString();
                                if (transaction == null)
                                {
                                    insertCommand.Connection = connection as SqlConnection;
                                }
                                else
                                {
                                    insertCommand.Connection = transaction.Connection as SqlConnection;
                                    insertCommand.Transaction = transaction as SqlTransaction;
                                }
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    if (cascadeUpdates)
                    {
                        CascadeUpdates(item, fieldName, keyValue, m_entities[entityName], connection, transaction);
                    }
                    if (changeDetected)
                        OnAfterUpdate(item, cascadeUpdates, fieldName, DateTime.Now.Subtract(start), updateSQL.ToString());
                    else
                        OnAfterUpdate(item, cascadeUpdates, fieldName, DateTime.Now.Subtract(start), null);
                }
            }
            finally
            {
                DoneWithConnection(connection, false);
            }
        }
    }
}
