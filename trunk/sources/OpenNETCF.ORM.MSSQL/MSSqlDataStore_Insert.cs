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
                if (items.GetType().IsArray)
                {
                    foreach (var item in items as Array)
                    {
                        Insert(item, insertReferences, connection, transaction, false);
                    }
                }
                else if (items is System.Collections.IEnumerable)
                {
                    foreach (var item in items as System.Collections.IEnumerable)
                    {
                        Insert(item, insertReferences, connection, transaction, false);
                    }
                }
            }
        }

        protected override void BulkInsertOrUpdate(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {
            if (items != null)
            {
                if (items.GetType().IsArray)
                {
                    foreach (var item in items as Array)
                    {
                        InsertOrUpdate(item, insertReferences, connection, transaction);
                    }
                }
                else if (items is System.Collections.IEnumerable)
                {
                    foreach (var item in items as System.Collections.IEnumerable)
                    {
                        InsertOrUpdate(item, insertReferences, connection, transaction);
                    }
                }
            }
        }

        protected override void Insert(object item, bool insertReferences, IDbConnection connection, IDbTransaction transaction, bool checkUpdates)
        {
            var itemType = item.GetType();
            string entityName = m_entities.GetNameForType(itemType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(item.GetType());
            }

            Boolean bInheritedConnection = connection != null;
            if (transaction == null && connection == null) connection = GetConnection(false);
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

                var keyScheme = Entities[entityName].EntityAttribute.KeyScheme;

                // TODO: fill the parameters
                foreach (var field in Entities[entityName].Fields)
                {
                    if ((field.IsPrimaryKey) && ((keyScheme == KeyScheme.Identity) || field.IsIdentity))
                    {
                        identity = field;
                        continue;
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
                    identity.PropertyInfo.SetValue(item, id, null);
                }

                if (insertReferences)
                {
                    // cascade insert any References
                    // do this last because we need the PK from above
                    foreach (var reference in Entities[entityName].References)
                    {
                        var valueArray = reference.PropertyInfo.GetValue(item, null);
                        if (valueArray == null) continue;

                        var fk = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);

                        string et = null;

                        // we've already enforced this to be an array when creating the store
                        foreach (var element in valueArray as Array)
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
