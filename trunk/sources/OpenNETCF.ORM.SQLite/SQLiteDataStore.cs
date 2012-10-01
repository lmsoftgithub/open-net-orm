﻿using System;
using System.Net;
using System.IO;
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
#else
using System.Data.SQLite;
#endif

namespace OpenNETCF.ORM.SQLite
{
    public class SQLiteDataStore : SQLStoreBase<SQLiteEntityInfo>, IDisposable
    {
        private string m_connectionString;

        public string FileName { get; protected set; }

        protected SQLiteDataStore()
            : base()
        {
        }

        public SQLiteDataStore(string fileName)
            : this()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException();
            }

            FileName = fileName;
        }

        private string ConnectionString
        {
            get
            {
                if (m_connectionString == null)
                {
                    m_connectionString = string.Format("Data Source={0}", FileName);

                }
                return m_connectionString;
            }
        }

        protected override IDbCommand GetNewCommandObject()
        {
            return new SQLiteCommand();
        }

        protected override IDbConnection GetNewConnectionObject()
        {
            return new SQLiteConnection(ConnectionString);
        }

        protected override IDataParameter CreateParameterObject(string parameterName, object parameterValue)
        {
            return new SQLiteParameter(parameterName, parameterValue);
        }

        protected override string AutoIncrementFieldIdentifier
        {
            get { return "AUTOINCREMENT"; }
        }

        public override void CreateStore()
        {
            CreateStore(false);
        }

        public override void CreateOrUpdateStore()
        {
            CreateStore(true);
        }

        protected void CreateStore(Boolean UpdateStore)
        {
            if (StoreExists)
            {
                if (!UpdateStore) throw new StoreAlreadyExistsException();
            }

            SQLiteConnection.CreateFile(FileName);

            var connection = GetConnection(true);
            try
            {
                foreach (var entity in this.Entities)
                {
                    CreateTable(connection, entity);
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        public override void DeleteStore()
        {
            if (StoreExists)
            {
                File.Delete(FileName);
            }
        }

        public override bool StoreExists
        {
            get { return File.Exists(FileName); }
        }

        private SQLiteCommand GetInsertCommand(string entityName)
        {
            // TODO: support command caching to improve bulk insert speeds
            //       simply use a dictionary keyed by entityname
            var keyScheme = Entities[entityName].EntityAttribute.KeyScheme;
            var insertCommand = new SQLiteCommand();
            
            var sbFields = new StringBuilder(string.Format("INSERT INTO {0} (", entityName));
            var sbParams = new StringBuilder( " VALUES (");

            foreach (var field in Entities[entityName].Fields)
            {
                // skip auto-increments
                if ((field.IsPrimaryKey) && (keyScheme == KeyScheme.Identity))
                {
                    continue;
                }
                sbFields.Append("[" + field.FieldName + "],");
                sbParams.Append("?,");

                insertCommand.Parameters.Add(new SQLiteParameter(field.FieldName));
            }

            // replace trailing commas
            sbFields[sbFields.Length - 1] = ')';
            sbParams[sbParams.Length - 1] = ')';

            insertCommand.CommandText = sbFields.ToString() + sbParams.ToString();

            return insertCommand;
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
                    command.Transaction = transaction as SQLiteTransaction;

                var keyScheme = Entities[entityName].EntityAttribute.KeyScheme;

                // TODO: fill the parameters
                foreach (var field in Entities[entityName].Fields)
                {
                    if ((field.IsPrimaryKey) && (keyScheme == KeyScheme.Identity))
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

                        var fk = Entities[entityName].Fields[reference.ReferenceField].PropertyInfo.GetValue(item, null);

                        string et = null;

                        // we've already enforced this to be an array when creating the store
                        foreach (var element in valueArray as Array)
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
                }
                OnAfterInsert(item, insertReferences, DateTime.Now.Subtract(start), command.CommandText);
                command.Dispose();
            }
            finally
            {
                DoneWithConnection(connection, false);
            }
        }

        protected override void BulkInsert(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        protected override void BulkInsertOrUpdate(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {
            throw new NotImplementedException();
        }

        private int GetIdentity(IDbConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT last_insert_rowid()", connection as SQLiteConnection))
            {
                object id = command.ExecuteScalar();
                return Convert.ToInt32(id);
            }
        }

        private int GetIdentity(IDbTransaction transaction)
        {
            using (var command = new SQLiteCommand("SELECT last_insert_rowid()", transaction.Connection as SQLiteConnection, transaction as SQLiteTransaction))
            {
                object id = command.ExecuteScalar();
                return Convert.ToInt32(id);
            }
        }

        protected override string GetPrimaryKeyIndexName(string entityName)
        {
            var connection = GetConnection(true);
            try
            {
                string name = null;
                string sql = string.Format("PRAGMA table_info({0})", entityName);

                using (var command = GetNewCommandObject())
                {
                    command.CommandText = sql;
                    command.Connection = connection;
                    using (var reader = command.ExecuteReader() as SQLiteDataReader)
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // pk column is #5
                                if (Convert.ToInt32(reader[5]) != 0)
                                {
                                    return reader[1] as string;
                                }
                            }
                        }
                    }
                }
                return name;
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        private void UpdateIndexCacheForType(string entityName)
        {
            // have we already cached this?
            if (Entities[entityName].IndexNames != null) return;

            // get all iindex names for the type
            var connection = GetConnection(true);
            try
            {
                string sql = string.Format("SELECT name FROM sqlite_master WHERE (tbl_name = '{0}')", entityName);

                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    using (var reader = command.ExecuteReader())
                    {
                        List<string> nameList = new List<string>();

                        while (reader.Read())
                        {
                            nameList.Add(reader.GetString(0));
                        }

                        Entities[entityName].IndexNames = nameList;
                    }
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        protected override object[] Select(Type objectType, IEnumerable<FilterCondition> filters, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences, IDbConnection connection)
        {
            string entityName = m_entities.GetNameForType(objectType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(objectType);
            }

            UpdateIndexCacheForType(entityName);

            var items = new List<object>();

            if (connection == null) connection = GetConnection(false);
            SQLiteCommand command = null;

            try
            {
                CheckOrdinals(entityName);

                OnBeforeSelect(m_entities[entityName], filters, fillReferences);
                var start = DateTime.Now;

                bool tableDirect;
                command = GetSelectCommand<SQLiteCommand, SQLiteParameter>(entityName, filters, firstRowOffset, fetchCount, out tableDirect);
                command.Connection = connection as SQLiteConnection;

                int searchOrdinal = -1;
            //    ResultSetOptions options = ResultSetOptions.Scrollable;

                object matchValue = null;
                string matchField = null;

            // TODO: we need to ensure that the search value does not exceed the length of the indexed
            // field, else we'll get an exception on the Seek call below (see the SQL CE implementation)

                using (var results = command.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (results.HasRows)
                    {
                        ReferenceAttribute[] referenceFields = null;

                        int currentOffset = 0;

                        if (matchValue != null)
                        {
                            // convert enums to an int, else the .Equals later check will fail
                            // this feels a bit kludgey, but for now it's all I can think of
                            if (matchValue.GetType().IsEnum)
                            {
                                matchValue = (int)matchValue;
                            }

                            if (searchOrdinal < 0)
                            {
                                searchOrdinal = results.GetOrdinal(matchField);
                            }
                        }

                        while (results.Read())
                        {
                            if (currentOffset < firstRowOffset)
                            {
                                currentOffset++;
                                continue;
                            }

                            object item = Activator.CreateInstance(objectType);
                            object rowPK = null;

                            // autofill references if desired
                            if (referenceFields == null)
                            {
                                referenceFields = Entities[entityName].References.ToArray();
                            }


                            foreach (var field in Entities[entityName].Fields)
                            {
                                var value = results[field.Ordinal];
                                if (value != DBNull.Value)
                                {
                                    if (field.DataType == DbType.Object)
                                    {
                                        if (fillReferences)
                                        {
                                            // get serializer
                                            var itemType = item.GetType();
                                            var deserializer = GetDeserializer(itemType);

                                            if (deserializer == null)
                                            {
                                                throw new MissingMethodException(
                                                    string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                                    field.FieldName, entityName));
                                            }

                                            var @object = deserializer.Invoke(item, new object[] { field.FieldName, value });
                                            field.PropertyInfo.SetValue(item, @object, null);
                                        }
                                    }
                                    else if (field.IsRowVersion)
                                    {
                                        // sql stores this an 8-byte array
                                        field.PropertyInfo.SetValue(item, BitConverter.ToInt64((byte[])value, 0), null);
                                    }
                                    else if (field.PropertyInfo.PropertyType.UnderlyingTypeIs<TimeSpan>())
                                    {
                                        // SQL Compact doesn't support Time, so we're convert to ticks in both directions
                                        var valueAsTimeSpan = new TimeSpan((long)value);
                                        field.PropertyInfo.SetValue(item, valueAsTimeSpan, null);
                                    }
                                    else if ((field.IsPrimaryKey) && (value is Int64))
                                    {
                                        // SQLite automatically makes auto-increment fields 64-bit, so this works around that behavior
                                        field.PropertyInfo.SetValue(item, Convert.ToInt32(value), null);
                                    }
                                    else
                                    {
                                        field.PropertyInfo.SetValue(item, value, null);
                                    }
                                }
                                //Check if it is reference key to set, not primary.
                                ReferenceAttribute attr = referenceFields.Where(
                                    x => x.ReferenceField == field.FieldName).FirstOrDefault();

                                if (attr != null)
                                {
                                    rowPK = value;
                                }
                                if (field.IsPrimaryKey)
                                {
                                    rowPK = value;
                                }
                            }

                            if ((fillReferences) && (referenceFields.Length > 0))
                            {
                                //FillReferences(item, rowPK, referenceFields, true);
                                FillReferences(item, rowPK, referenceFields, false, filterReferences, connection);
                            }

                            items.Add(item);

                            if ((fetchCount > 0) && (items.Count >= fetchCount))
                            {
                                break;
                            }
                        }
                    }
                }
                OnAfterSelect(m_entities[entityName], filters, fillReferences, DateTime.Now.Subtract(start), command.CommandText);
            }
            finally
            {
                if ((!UseCommandCache) && (command != null))
                {
                    command.Dispose();
                }

                if (UseCommandCache)
                {
                    Monitor.Exit(CommandCache);
                }

                FlushReferenceTableCache();
                DoneWithConnection(connection, false);
            }

            return items.ToArray();
        }
        
        public override void EnsureCompatibility()
        {
            throw new NotImplementedException();
        }

        public override void Update(object item, bool cascadeUpdates, string fieldName)
        {
            Update(item, cascadeUpdates, fieldName, null, null);
        }

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

            if (transaction == null && connection == null)
                connection = GetConnection(false);
            try
            {
                CheckOrdinals(entityName);
                CheckPrimaryKeyIndex(entityName);

                OnBeforeUpdate(item, cascadeUpdates, fieldName);
                var start = DateTime.Now;

                using (var command = GetNewCommandObject())
                {
                    keyValue = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);

                    if (transaction == null)
                        command.Connection = connection;
                    else
                        command.Transaction = transaction;

                    command.CommandText = string.Format("SELECT * FROM {0} WHERE [{1}] = @keyparam",
                        entityName,
                        Entities[entityName].Fields.KeyField.FieldName);

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

                            // only execute if a change occurred
                            if (changeDetected)
                            {
                                // remove the trailing comma and append the filter
                                updateSQL.Length -= 2;
                                updateSQL.AppendFormat(" WHERE {0} = @keyparam", Entities[entityName].Fields.KeyField.FieldName);
                                insertCommand.Parameters.Add(new SQLiteParameter("@keyparam", keyValue));
                                insertCommand.CommandText = updateSQL.ToString();
                                insertCommand.Connection = connection;
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

        public override int Count<T>(IEnumerable<FilterCondition> filters)
        {
            var t = typeof(T);
            string entityName = m_entities.GetNameForType(t);

            if (entityName == null)
            {
                throw new EntityNotFoundException(t);
            }

            var connection = GetConnection(true);
            try
            {
                using (var command = BuildFilterCommand<SQLiteCommand, SQLiteParameter>(entityName, filters, true, 0, 0))
                {
                    command.Connection = connection as SQLiteConnection;
                    return (int)command.ExecuteScalar();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences)
        {
            return Fetch<T>(fetchCount, firstRowOffset, sortField, sortOrder, filter, fillReferences, false);
        }

        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences)
        {
            throw new NotSupportedException("Fetch is not currently supported with this Provider.");
        }

        public override object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences)
        {
            throw new NotSupportedException("Fetch is not currently supported with this Provider.");
        }
    }
}