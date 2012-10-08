using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Data.Common;
using System.Data.SqlTypes;

namespace OpenNETCF.ORM
{
    partial class MSSqlDataStore
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
                // CheckOrdinals(entityName);

                OnBeforeInsert(item, insertReferences);
                var start = DateTime.Now;

                FieldAttribute identity = null;
                var command = GetInsertCommand(entityName);
                if (transaction == null)
                {
                    command.Connection = connection as SqlConnection;
                }
                else
                {
                    command.Connection = transaction.Connection as SqlConnection;
                    command.Transaction = transaction as SqlTransaction;
                }

                var keyScheme = entity.EntityAttribute.KeyScheme;

                // TODO: fill the parameters
                foreach (var field in entity.Fields)
                {
                    if ((field.IsPrimaryKey) && ((keyScheme == KeyScheme.Identity) || field.IsIdentity))
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
                                command.Parameters[String.Format("@{0}", field.FieldName)].Value = DBNull.Value;
                            }
                            else
                            {
                                command.Parameters[String.Format("@{0}", field.FieldName)].Value = value;
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
                                command.Parameters[String.Format("@{0}", field.FieldName)].Value = DBNull.Value;
                            }
                            else
                            {
                                var timespanTicks = ((TimeSpan)value).Ticks;
                                command.Parameters[String.Format("@{0}", field.FieldName)].Value = timespanTicks;
                            }
                        }
                        else
                        {
                            var value = field.PropertyInfo.GetValue(item, null);
                            if (value != null) command.Parameters[String.Format("@{0}", field.FieldName)].Value = value;
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
                    foreach (var reference in entity.References)
                    {
                        var valueArray = reference.PropertyInfo.GetValue(item, null);
                        if (valueArray == null) continue;

                        var fk = entity.Fields.KeyField.PropertyInfo.GetValue(item, null);

                        string et = null;

                        if (reference.IsArray || reference.IsList)
                        {
                            foreach (var element in valueArray as System.Collections.IEnumerable)
                            {
                                if (et == null)
                                {
                                    et = m_entities.GetNameForType(element.GetType());
                                }
                                Entities[et].Fields[reference.ReferenceField].PropertyInfo.SetValue(element, fk, null);
                                if (checkUpdates)
                                    this.InsertOrUpdate(element, insertReferences, connection, transaction);
                                else
                                    this.Insert(element, insertReferences, connection, transaction, false);
                            }
                        }
                        else
                        {
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
