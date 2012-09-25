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

        public override void BulkInsert(object items, bool insertReferences)
        {
            BulkInsert(items, insertReferences, false);
        }
        public override void BulkInsert(object items, bool insertReferences, bool transactional)
        {
            IDbTransaction transaction = null;
            if (transactional)
            {
                transaction = GetTransaction(false);
            }
            try
            {
                BulkInsert(items, insertReferences, transaction);
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
        protected void BulkInsert(object items, bool insertReferences, IDbTransaction transaction)
        {
            if (items != null)
            {
                if (items.GetType().IsArray)
                {
                    foreach (var item in items as Array)
                    {
                        Insert(item, insertReferences, transaction, false);
                    }
                }
                else if (items is System.Collections.IEnumerable)
                {
                    foreach (var item in items as System.Collections.IEnumerable)
                    {
                        Insert(item, insertReferences, transaction, false);
                    }
                }
            }
        }

        public override void BulkInsertOrUpdate(object items, bool insertReferences)
        {
            BulkInsertOrUpdate(items, insertReferences, false);
        }
        public override void BulkInsertOrUpdate(object items, bool insertReferences, bool transactional)
        {
            IDbTransaction transaction = null;
            if (transactional)
            {
                transaction = GetTransaction(false);
            }
            try
            {
                BulkInsertOrUpdate(items, insertReferences, transaction);
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
        protected void BulkInsertOrUpdate(object items, bool insertReferences, IDbTransaction transaction)
        {
            if (items != null)
            {
                if (items.GetType().IsArray)
                {
                    foreach (var item in items as Array)
                    {
                        InsertOrUpdate(item, insertReferences, transaction);
                    }
                }
                else if (items is System.Collections.IEnumerable)
                {
                    foreach (var item in items as System.Collections.IEnumerable)
                    {
                        InsertOrUpdate(item, insertReferences, transaction);
                    }
                }
            }
        }

        public override void Insert(object item, bool insertReferences)
        {
            Insert(item, insertReferences, false);
        }
        public override void Insert(object item, bool insertReferences, bool transactional)
        {
            //IDbConnection connection = null;
            IDbTransaction transaction = null;
            if (transactional)
            {
                transaction = GetTransaction(false);
            }
            try
            {
                Insert(item, insertReferences, transaction, false);
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
        protected void Insert(object item, bool insertReferences, IDbTransaction transaction, bool checkUpdates)
        {
            var itemType = item.GetType();
            string entityName = m_entities.GetNameForType(itemType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(item.GetType());
            }

            IDbConnection connection = null;
            if (transaction == null && connection == null) connection = GetConnection(false);
            try
            {
                // CheckOrdinals(entityName);
                FieldAttribute identity = null;
                var command = GetInsertCommand(entityName);
                if (transaction == null)
                {
                    command.Connection = connection as FbConnection;
                }
                else
                {
                    command.Connection = transaction.Connection as FbConnection;
                    command.Transaction = transaction as FbTransaction;
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
                                this.InsertOrUpdate(element, insertReferences, transaction);
                            else
                                this.Insert(element, insertReferences, transaction, false);
                        }
                    }
                }
            }
            finally
            {
                DoneWithConnection(connection, false);
            }
        }

        public virtual void InsertOrUpdate(object item)
        {
            InsertOrUpdate(item, false);
        }
        public override void InsertOrUpdate(object item, bool insertReferences)
        {
            InsertOrUpdate(item, insertReferences, false);
        }
        public override void InsertOrUpdate(object item, bool insertReferences, bool transactional)
        {
            IDbTransaction transaction = null;
            if (transactional)
            {
                transaction = GetTransaction(false);
            }
            try
            {
                InsertOrUpdate(item, insertReferences, transaction);
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
        protected virtual void InsertOrUpdate(object item, bool insertReferences, IDbTransaction transaction)
        {
            if (this.Contains(item))
            {
                Update(item, insertReferences, null, transaction);
            }
            else
            {
                Insert(item, insertReferences, transaction, true);
            }
        }

    }
}
