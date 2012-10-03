using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;

namespace OpenNETCF.ORM
{
    partial class SqlCeDataStore
    {

        protected override void BulkInsert(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {

            if (items != null && items.GetType().IsArray && (items as Array).Length > 0)
            {
                object firstitem = (items as Array).GetValue(0);
                var itemType = firstitem.GetType();
                string entityName = m_entities.GetNameForType(itemType);

                if (entityName == null)
                {
                    throw new EntityNotFoundException(firstitem.GetType());
                }

                Boolean bInheritedConnection = connection != null;
                if (transaction == null && connection == null) connection = GetConnection(false);

                try
                {
                    OnBeforeInsert(items, insertReferences);
                    var start = DateTime.Now;

                    FieldAttribute identity = null;
                    using (var command = new SqlCeCommand())
                    {
                        if (transaction == null)
                            command.Connection = connection as SqlCeConnection;
                        else
                        {
                            command.Transaction = transaction as SqlCeTransaction;
                            command.Connection = transaction.Connection as SqlCeConnection;
                        }
                        command.CommandText = entityName;
                        command.CommandType = CommandType.TableDirect;

                        using (var results = command.ExecuteResultSet(ResultSetOptions.Updatable))
                        {
                            var record = results.CreateRecord();
                            var ordinals = GetOrdinals(entityName, results);

                            var keyScheme = Entities[entityName].EntityAttribute.KeyScheme;

                            foreach (object item in items as Array)
                            {
                                foreach (var field in Entities[entityName].Fields)
                                {
                                    if ((keyScheme == KeyScheme.Identity) && field.IsPrimaryKey)
                                    {
                                        identity = field;
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
                                            record.SetValue(ordinals[field.FieldName], DBNull.Value);
                                        }
                                        else
                                        {
                                            record.SetValue(ordinals[field.FieldName], value);
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
                                            record.SetValue(ordinals[field.FieldName], DBNull.Value);
                                        }
                                        else
                                        {
                                            var timespanTicks = ((TimeSpan)value).Ticks;
                                            record.SetValue(ordinals[field.FieldName], timespanTicks);
                                        }
                                    }
                                    else
                                    {
                                        var value = field.PropertyInfo.GetValue(item, null);
                                        record.SetValue(ordinals[field.FieldName], value);
                                    }
                                }

                                results.Insert(record);

                                // did we have an identity field?  If so, we need to update that value in the item
                                if (identity != null)
                                {
                                    var id = GetIdentity(connection);
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

                                        // Modified - 2012.08.16 - Corrected an error where the bad keyfield was used.
                                        //var fk = Entities[entityName].Fields[reference.ReferenceField].PropertyInfo.GetValue(item, null);
                                        var fk = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);

                                        string et = null;

                                        // we've already enforced this to be an array when creating the store
                                        foreach (var element in valueArray as Array)
                                        {
                                            if (et == null)
                                            {
                                                et = m_entities.GetNameForType(element.GetType());
                                            }

                                            // Added - 2012.08.08 - Replacing the old code to handle existing objects which were added
                                            Entities[et].Fields[reference.ReferenceField].PropertyInfo.SetValue(element, fk, null);
                                        }
                                        this.BulkInsert((object[])valueArray, insertReferences, connection, transaction);
                                    }
                                }
                            }
                        }
                    }
                    OnAfterInsert(items, insertReferences, DateTime.Now.Subtract(start), null);
                }
                finally
                {
                    if (!bInheritedConnection) DoneWithConnection(connection, false);
                }
            }
        }

        protected override void BulkInsertOrUpdate(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {
            if (items != null && items.GetType().IsArray && (items as Array).Length > 0)
            {
                object firstitem = (items as Array).GetValue(0);
                var itemType = firstitem.GetType();
                string entityName = m_entities.GetNameForType(itemType);

                if (entityName == null)
                {
                    throw new EntityNotFoundException(firstitem.GetType());
                }

                foreach (var item in items as Array)
                {
                    if (this.Contains(item, connection))
                        this.Update(item, insertReferences, null, connection, transaction);
                    else
                        this.Insert(item, insertReferences, connection, transaction, insertReferences);
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
            var itemType = item.GetType();
            string entityName = m_entities.GetNameForType(itemType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(item.GetType());
            }
            EntityInfo entity = m_entities[entityName];

            Boolean bInheritedConnection = connection != null;
            if (transaction == null && connection == null)
                connection = GetConnection(false);
            try
            {
                OnBeforeInsert(item, insertReferences);
                var start = DateTime.Now;

                FieldAttribute identity = null;
                using (var command = new SqlCeCommand())
                {
                    if (transaction == null)
                        command.Connection = connection as SqlCeConnection;
                    else
                    {
                        command.Connection = transaction.Connection as SqlCeConnection;
                        command.Transaction = transaction as SqlCeTransaction;
                    }
                    command.CommandText = entity.EntityName;
                    command.CommandType = CommandType.TableDirect;

                    using (var results = command.ExecuteResultSet(ResultSetOptions.Updatable))
                    {
                        var ordinals = GetOrdinals(entityName, results);

                        var record = results.CreateRecord();

                        var keyScheme = Entities[entity.EntityName].EntityAttribute.KeyScheme;

                        foreach (var field in Entities[entity.EntityName].Fields)
                        {
                            if ((keyScheme == KeyScheme.Identity) && field.IsPrimaryKey)
                            {
                                identity = field;
                            }
                            else if (field.DataType == DbType.Object)
                            {
                                // get serializer
                                var serializer = GetSerializer(itemType);

                                if (serializer == null)
                                {
                                    throw new MissingMethodException(
                                        string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                        field.FieldName, entity.EntityName));
                                }
                                var value = serializer.Invoke(item, new object[] { field.FieldName });
                                if (value == null)
                                {
                                    record.SetValue(ordinals[field.FieldName], DBNull.Value);
                                }
                                else
                                {
                                    record.SetValue(ordinals[field.FieldName], value);
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
                                    record.SetValue(ordinals[field.FieldName], DBNull.Value);
                                }
                                else
                                {
                                    var timespanTicks = ((TimeSpan)value).Ticks;
                                    record.SetValue(ordinals[field.FieldName], timespanTicks);
                                }
                            }
                            else
                            {
                                var value = field.PropertyInfo.GetValue(item, null);
                                record.SetValue(ordinals[field.FieldName], value);
                            }
                        }

                        results.Insert(record);

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
                            identity.PropertyInfo.SetValue(item, id, null);
                        }

                        if (insertReferences)
                        {
                            // cascade insert any References
                            // do this last because we need the PK from above
                            foreach (var reference in Entities[entity.EntityName].References)
                            {
                                var valueArray = reference.PropertyInfo.GetValue(item, null);
                                if (valueArray == null) continue;

                                // Modified - 2012.08.16 - Corrected an error where the bad keyfield was used.
                                //var fk = Entities[entityName].Fields[reference.ReferenceField].PropertyInfo.GetValue(item, null);
                                var fk = Entities[entity.EntityName].Fields.KeyField.PropertyInfo.GetValue(item, null);

                                string et = null;

                                // we've already enforced this to be an array when creating the store
                                foreach (var element in valueArray as Array)
                                {
                                    if (et == null)
                                    {
                                        et = m_entities.GetNameForType(element.GetType());
                                    }

                                    // Added - 2012.08.08 - Replacing the old code to handle existing objects which were added
                                    Entities[et].Fields[reference.ReferenceField].PropertyInfo.SetValue(element, fk, null);
                                    if (checkUpdates)
                                        this.InsertOrUpdate(element, insertReferences, connection, transaction);
                                    else
                                        this.Insert(element, insertReferences, connection, transaction, checkUpdates);
                                }
                            }
                        }
                    }
                }
                OnAfterInsert(item, insertReferences, DateTime.Now.Subtract(start), null);
            }
            finally
            {
                if (!bInheritedConnection) DoneWithConnection(connection, false);
            }
        }

    }
}
