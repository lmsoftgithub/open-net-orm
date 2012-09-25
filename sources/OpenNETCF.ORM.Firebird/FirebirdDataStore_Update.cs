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
        /// Updates the backing DataStore with the values in the specified entity instance
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>
        /// The instance provided must have a valid primary key value
        /// </remarks>
        public override void Update(object item)
        {
            //TODO: is a cascading default of true a good idea?
            Update(item, true, null);
        }

        public override void Update(object item, string fieldName)
        {
            Update(item, false, fieldName);
        }

        public override void Update(object item, bool cascadeUpdates, string fieldName)
        {
            Update(item, cascadeUpdates, fieldName, false);
        }
        public override void Update(object item, bool cascadeUpdates, string fieldName, bool transactional)
        {
            IDbTransaction transaction = null;
            if (transactional)
            {
                transaction = GetTransaction(false);
            }
            try
            {
                Update(item, cascadeUpdates, fieldName, transaction);
                if (transaction != null) transaction.Commit();
            }
            catch
            {
                if (transaction != null) transaction.Rollback();
                throw;
            }
            finally
            {
                DoneWithTransaction(transaction, false);
            }
        }

        protected void Update(object item, bool cascadeUpdates, string fieldName, IDbTransaction transaction)
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

            IDbConnection connection = null;
            if (transaction == null) connection = GetConnection(false);
            try
            {
                CheckOrdinals(entityName);
                CheckPrimaryKeyIndex(entityName);

                using (var command = GetNewCommandObject())
                {

                    if (transaction == null)
                    {
                        command.Connection = connection as FbConnection;
                    }
                    else
                    {
                        command.Connection = transaction.Connection as FbConnection;
                        command.Transaction = transaction as FbTransaction;
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
                            command.Parameters.Add(new FbParameter(String.Format("@{0}", field.FieldName), keyValue));
                            if (--keycount > 0) where.Append(" AND ");
                        }
                        if (--count > 0) sql.Append(", ");
                    }
                    sql.AppendFormat(" FROM {0} {1}", entityName, where.ToString());

                    command.CommandText = sql.ToString();
                    command.CommandType = CommandType.Text;

                    var updateSQL = new StringBuilder(string.Format("UPDATE {0} SET ", entityName));

                    using (var reader = command.ExecuteReader() as FbDataReader)
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
                                        insertCommand.Parameters.Add(new FbParameter("@" + field.FieldName, value));
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
                                        insertCommand.Parameters.Add(new FbParameter("@" + field.FieldName, ticks));
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
                                            insertCommand.Parameters.Add(new FbParameter("@" + field.FieldName, value));
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
                                    insertCommand.Parameters.Add(new FbParameter(String.Format("@{0}", field.FieldName), keyValue));
                                }
                                insertCommand.CommandText = updateSQL.ToString();
                                if (transaction == null)
                                {
                                    insertCommand.Connection = connection as FbConnection;
                                }
                                else
                                {
                                    insertCommand.Connection = transaction.Connection as FbConnection;
                                    insertCommand.Transaction = transaction as FbTransaction;
                                }
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            finally
            {
                DoneWithConnection(connection, false);
            }

            if (cascadeUpdates)
            {
                // TODO: move this into the base DataStore class as it's not SqlCe-specific
                foreach (var reference in Entities[entityName].References)
                {
                    var itemList = reference.PropertyInfo.GetValue(item, null) as Array;
                    if (itemList != null)
                    {
                        foreach (var refItem in itemList)
                        {
                            var foreignKey = refItem.GetType().GetProperty(reference.ReferenceField, BindingFlags.Instance | BindingFlags.Public);
                            foreignKey.SetValue(refItem, keyValue, null);
                            if (!this.Contains(refItem))
                            {
                                Insert(refItem, cascadeUpdates, transaction, true);
                            }
                            else
                            {
                                Update(refItem, cascadeUpdates, fieldName, transaction);
                            }
                        }
                    }
                }
            }
        }
    }
}
