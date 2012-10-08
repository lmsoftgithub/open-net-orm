﻿using System;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

#if ANDROID
// note the case difference between the System.Data.SQLite and Mono's implementation
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
#elif WINDOWS_PHONE
// ah the joys of an open-source project changing cases on us
using SQLiteConnection = Community.CsharpSqlite.SQLiteClient.SqliteConnection;
using SQLiteCommand = Community.CsharpSqlite.SQLiteClient.SqliteCommand;
using SQLiteParameter = Community.CsharpSqlite.SQLiteClient.SqliteParameter;
using SQLiteDataReader = Community.CsharpSqlite.SQLiteClient.SqliteDataReader;
#else
using System.Data.SQLite;
#endif

namespace OpenNETCF.ORM
{
    public partial class SQLiteDataStore
    {

        protected override void Update(object item, bool cascadeUpdates, string fieldName, IDbConnection connection, IDbTransaction transaction)
        {
            var isDynamicEntity = item is DynamicEntity;
            string entityName = null;
            if (isDynamicEntity)
            {
                entityName = ((DynamicEntity)item).EntityName;
                if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);
            }
            else
            {
                entityName = m_entities.GetNameForType(item.GetType());
            }

            if (entityName == null)
            {
                throw new EntityNotFoundException(item.GetType());
            }
            var entity = m_entities[entityName];

            if (entity.Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform Updates");
            }

            object keyValue;
            var changeDetected = false;

            Boolean bInheritedConnection = connection != null;
            if (transaction == null && connection == null)
                connection = GetConnection(false);
            try
            {
                CheckPrimaryKeyIndex(entityName);

                OnBeforeUpdate(item, cascadeUpdates, fieldName);
                var start = DateTime.Now;

                using (var command = GetNewCommandObject())
                {
                    keyValue = entity.Fields.KeyField.PropertyInfo.GetValue(item, null);

                    if (transaction == null)
                        command.Connection = connection;
                    else
                        command.Transaction = transaction;
                    StringBuilder sql = new StringBuilder();
                    StringBuilder where = new StringBuilder(" WHERE ");

                    sql.Append("SELECT ");
                    int count = entity.Fields.Count;
                    int keycount = entity.Fields.KeyFields.Count;
                    foreach (FieldAttribute field in entity.Fields)
                    {
                        sql.Append(field.FieldName);
                        if (entity.Fields.KeyFields.Contains(field))
                        {
                            where.AppendFormat(" [{0}] = @{0} ", field.FieldName);
                            if (isDynamicEntity)
                            {
                                keyValue = ((DynamicEntity)item)[field.FieldName];
                            }
                            else
                            {
                                keyValue = field.PropertyInfo.GetValue(item, null);
                            }
                            command.Parameters.Add(new SQLiteParameter(String.Format("@{0}", field.FieldName), keyValue));
                            if (--keycount > 0) where.Append(" AND ");
                        }
                        if (--count > 0) sql.Append(", ");
                    }
                    sql.AppendFormat(" FROM {0} {1}", entityName, where.ToString());

                    command.CommandText = sql.ToString();
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SQLiteParameter("@keyparam", keyValue));

                    var updateSQL = new StringBuilder(string.Format("UPDATE {0} SET ", entityName));

                    using (var reader = command.ExecuteReader() as SQLiteDataReader)
                    {

                        if (!reader.HasRows)
                        {
                            // TODO: the PK value has changed - we need to store the original value in the entity or diallow this kind of change
                            throw new RecordNotFoundException("Cannot locate a record with the provided primary key.  You cannot update a primary key value through the Update method");
                        }

                        reader.Read();

                        using (var insertCommand = GetNewCommandObject())
                        {
                            // update the values
                            foreach (var field in entity.Fields)
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
                                else if (isDynamicEntity)
                                {
                                    object value = null;
                                    if (entity.Fields[field.FieldName].DataType == DbType.Object)
                                    {
                                        if (entity.Serializer == null)
                                        {
                                            throw new MissingMethodException(
                                                string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                                field.FieldName, entity.EntityName));
                                        }
                                        value = entity.Serializer.Invoke(item, field.FieldName);
                                    }
                                    else
                                    {
                                        value = ((DynamicEntity)item)[field.FieldName];
                                    }
                                    if (value == null)
                                    {
                                        updateSQL.AppendFormat("{0}=NULL, ", field.FieldName);
                                    }
                                    else
                                    {
                                        updateSQL.AppendFormat("{0}=@{0}, ", field.FieldName);
                                        insertCommand.Parameters.Add(new SQLiteParameter("@" + field.FieldName, value));
                                    }
                                }
                                else
                                {
                                    if (field.IsRowVersion)
                                    {
                                        // read-only, so do nothing
                                    }
                                    else if (field.DataType == DbType.Object)
                                    {
                                        changeDetected = true;
                                        if (entity.Serializer == null)
                                        {
                                            throw new MissingMethodException(
                                                string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                                field.FieldName, entityName));
                                        }
                                        var value = entity.Serializer.Invoke(item, field.FieldName);

                                        if (value == null)
                                        {
                                            updateSQL.AppendFormat("{0}=NULL, ", field.FieldName);
                                        }
                                        else
                                        {
                                            updateSQL.AppendFormat("{0}=@{0}, ", field.FieldName);
                                            insertCommand.Parameters.Add(new SQLiteParameter("@" + field.FieldName, value));
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
                                            insertCommand.Parameters.Add(new SQLiteParameter("@" + field.FieldName, ticks));
                                        }
                                    }
                                    else
                                    {
                                        var value = field.PropertyInfo.GetValue(item, null);

                                        if (reader[field.FieldName] != value)
                                        {
                                            changeDetected = true;

                                            if (value == null)
                                            {
                                                updateSQL.AppendFormat("{0}=NULL, ", field.FieldName);
                                            }
                                            else
                                            {
                                                updateSQL.AppendFormat("{0}=@{0}, ", field.FieldName);
                                                insertCommand.Parameters.Add(new SQLiteParameter("@" + field.FieldName, value));
                                            }
                                        }
                                    }
                                }
                            }

                            // only execute if a change occurred
                            if (changeDetected)
                            {
                                // remove the trailing comma and append the filter
                                updateSQL.Length -= 2;
                                updateSQL.Append(where.ToString());
                                foreach (FieldAttribute field in entity.Fields.KeyFields)
                                {
                                    if (isDynamicEntity)
                                    {
                                        keyValue = ((DynamicEntity)item)[field.FieldName];
                                    }
                                    else
                                    {
                                        keyValue = field.PropertyInfo.GetValue(item, null);
                                    }
                                    insertCommand.Parameters.Add(new SQLiteParameter(String.Format("@{0}", field.FieldName), keyValue));
                                }
                                insertCommand.CommandText = updateSQL.ToString();
                                if (transaction == null)
                                {
                                    insertCommand.Connection = connection as SQLiteConnection;
                                }
                                else
                                {
                                    insertCommand.Connection = transaction.Connection as SQLiteConnection;
                                    insertCommand.Transaction = transaction as SQLiteTransaction;
                                }
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    if (cascadeUpdates)
                    {
                        CascadeUpdates(item, fieldName, null, entity, connection, transaction);
                    }

                    if (changeDetected)
                        OnAfterUpdate(item, cascadeUpdates, fieldName, DateTime.Now.Subtract(start), updateSQL.ToString());
                    else
                        OnAfterUpdate(item, cascadeUpdates, fieldName, DateTime.Now.Subtract(start), null);

                }
            }
            finally
            {
                if (!bInheritedConnection) DoneWithConnection(connection, false);
            }
        }
    }
}
