using System;
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

        protected override void BulkInsert(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {
            if (items != null)
            {
                if (items is System.Collections.IEnumerable)
                {
                    foreach (var item in items as System.Collections.IEnumerable)
                    {
                        Insert(item, insertReferences, connection, transaction, false);
                    }
                }
                else
                {
                    throw new NotSupportedException(String.Format("The given item collection type is not supported: {0}", items.GetType()));
                }
            }
        }

        protected override void BulkInsertOrUpdate(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {
            if (items != null)
            {
                if (items is System.Collections.IEnumerable)
                {
                    foreach (var item in items as System.Collections.IEnumerable)
                    {
                        InsertOrUpdate(item, insertReferences, connection, transaction);
                    }
                }
                else
                {
                    throw new NotSupportedException(String.Format("The given item collection type is not supported: {0}", items.GetType()));
                }
            }
        }

        /// <summary>
        /// Inserts the provided entity instance into the underlying data store.
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>
        /// If the entity has an identity field, calling Insert will populate that field with the identity vale vefore returning
        /// </remarks>
        protected override void Insert(object item, bool insertReferences, IDbConnection connection, IDbTransaction transaction, bool checkUpdates)
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

            Boolean bInheritedConnection = connection != null;
            if (transaction == null && connection == null)
                connection = GetConnection(false);
            try
            {
                //                CheckOrdinals(entityName);

                OnBeforeInsert(item, insertReferences);
                var start = DateTime.Now;

                FieldAttribute identity = null;
                var command = GetInsertCommand(entityName);
                if (transaction == null)
                    command.Connection = connection as SQLiteConnection;
                else
                {
                    command.Connection = transaction.Connection as SQLiteConnection;
                    command.Transaction = transaction as SQLiteTransaction;
                }
                var keyScheme = Entities[entityName].EntityAttribute.KeyScheme;

                // TODO: fill the parameters
                foreach (var field in Entities[entityName].Fields)
                {
                    if ((field.IsPrimaryKey) && (keyScheme == KeyScheme.Identity))
                    {
                        identity = field;
                        continue;
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
                            command.Parameters[String.Format("@{0}", field.FieldName)].Value = DBNull.Value;
                        }
                        else
                        {
                            command.Parameters[String.Format("@{0}", field.FieldName)].Value = value;
                        }
                    }
                    else
                    {
                        if (field.DataType == DbType.Object)
                        {
                           if (entity.Serializer == null)
                            {
                                throw new MissingMethodException(
                                    string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                    field.FieldName, entityName));
                            }
                           var value = entity.Serializer.Invoke(item, field.FieldName);
                            if (value == null)
                            {
                                command.Parameters[field.FieldName].Value = DBNull.Value;
                            }
                            else
                            {
                                command.Parameters[field.FieldName].Value = value;
                            }
                        }
                        else if (field.IsRowVersion)
                        {
                            // read-only, so do nothing
                        }
                        else if (field.PropertyInfo.PropertyType.UnderlyingTypeIs<TimeSpan>())
                        {
                            // SQL Compact doesn't support Time, so we're convert to a DateTime both directions
                            var value = field.PropertyInfo.GetValue(item, null);

                            if (value == null)
                            {
                                command.Parameters[field.FieldName].Value = DBNull.Value;
                            }
                            else
                            {
                                var timespanTicks = ((TimeSpan)value).Ticks;
                                command.Parameters[field.FieldName].Value = timespanTicks;
                            }
                        }
                        else
                        {
                            var value = field.PropertyInfo.GetValue(item, null);
                            command.Parameters[field.FieldName].Value = value;
                        }
                    }
                }

                command.ExecuteNonQuery();

                // did we have an identity field?  If so, we need to update that value in the item
                if (identity != null)
                {
                    int id = 0;
                    if (transaction == null)
                    {
                        id = GetIdentity(connection);
                    }
                    else
                    {
                        id = GetIdentity(transaction);
                    }
                    if (isDynamicEntity)
                    {
                        ((DynamicEntity)item)[identity.FieldName] = id;
                    }
                    else
                    {
                        identity.PropertyInfo.SetValue(item, id, null);
                    }
                }

                if (insertReferences)
                {
                    // cascade insert any References
                    // do this last because we need the PK from above
                    foreach (var reference in Entities[entityName].References)
                    {
                        var valueArray = reference.PropertyInfo.GetValue(item, null);
                        if (valueArray == null) continue;

                        //var fk = Entities[entityName].Fields[reference.ReferenceField].PropertyInfo.GetValue(item, null);
                        var fk = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);

                        string et = null;

                        if (reference.IsArray || reference.IsList)
                        {
                            foreach (var element in valueArray as System.Collections.IEnumerable)
                            {
                                if (et == null)
                                {
                                    et = m_entities.GetNameForType(element.GetType());
                                }

                                // Added - 2012.08.08 - Added for robustness in updates
                                Entities[et].Fields[reference.ReferenceField].PropertyInfo.SetValue(element, fk, null);
                                if (checkUpdates)
                                    this.InsertOrUpdate(element, insertReferences, connection, transaction);
                                else
                                    this.Insert(element, insertReferences, connection, transaction, false);
                            }
                        }
                        else
                        {
                            //if (et == null)
                            //{
                            //    et = m_entities.GetNameForType(element.GetType());
                            //}

                            //// Added - 2012.08.08 - Added for robustness in updates
                            //Entities[et].Fields[reference.ReferenceField].PropertyInfo.SetValue(element, fk, null);
                            //if (checkUpdates)
                            //    this.InsertOrUpdate(element, insertReferences, connection, transaction);
                            //else
                            //    this.Insert(element, insertReferences, connection, transaction, false);
                            throw new NotImplementedException();
                        }
                    }
                }
                OnAfterInsert(item, insertReferences, DateTime.Now.Subtract(start), command.CommandText);
                command.Dispose();
            }
            finally
            {
                if (!bInheritedConnection) DoneWithConnection(connection, false);
            }
        }
    }
}
