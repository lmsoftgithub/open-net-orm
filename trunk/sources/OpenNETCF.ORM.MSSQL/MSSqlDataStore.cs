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
    public partial class MSSqlDataStore : SQLStoreBase<SqlEntityInfo>
    {
        private string m_connectionString;
        private int m_maxSize = 128; // Max Database Size defaults to 128MB

        private string Password { get; set; }

        public string FileName { get; protected set; }

        protected MSSqlDataStore()
            : base()
        {
            UseCommandCache = false;
            FieldAttributeCollection.AllowMultiplePrimaryKeyFields = true;
        }

        public MSSqlDataStore(string connectionString)
            : this(connectionString, null)
        {
        }

        public MSSqlDataStore(string connectionString, string password)
            : this()
        {
            FileName = connectionString;
            Password = password;
        }

        public override bool StoreExists
        {
            get
            {
                Boolean bConnected = false;
                try
                {
                    IDbConnection conn = GetConnection(false);
                    bConnected = true;
                    conn.Close();
                }
                catch { }
                return bConnected;
            }
        }

        protected override IDbCommand GetNewCommandObject()
        {
            return new SqlCommand();
        }

        protected override string AutoIncrementFieldIdentifier
        {
            get { return "IDENTITY"; }
        }

        /// <summary>
        /// Deletes the underlying DataStore
        /// </summary>
        public override void DeleteStore()
        {
            File.Delete(FileName);
        }

        /// <summary>
        /// Creates the underlying DataStore
        /// </summary>
        public override void CreateStore()
        {
            if (!StoreExists) throw new StoreNotFoundException();

            var connection = GetConnection(true);
            try
            {
                foreach (var entity in this.Entities)
                {
                    DropAndCreateTable(connection, entity);
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        public override void CreateOrUpdateStore()
        {
            if (!StoreExists) throw new StoreNotFoundException();

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

        protected override string VerifyIndex(string entityName, string fieldName, FieldSearchOrder searchOrder, IDbConnection connection)
        {
            bool localConnection = false;
            if (connection == null)
            {
                localConnection = true;
                connection = GetConnection(true);
            }
            try
            {
                var indexName = string.Format("ORM_IDX_{0}_{1}_{2}", entityName, fieldName,
                    searchOrder == FieldSearchOrder.Descending ? "DESC" : "ASC");

                if (m_indexNameCache.FirstOrDefault(ii => ii.Name == indexName) != null) return indexName;

                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;

                    var sql = string.Format("select COUNT(*) from sys.indexes where name = '{0}' and Object_ID = Object_ID(N'{1}')", indexName, entityName);
                    command.CommandText = sql;

                    int i = (int)command.ExecuteScalar();

                    if (i == 0)
                    {
                        sql = string.Format("CREATE INDEX {0} ON {1}({2} {3})",
                            indexName,
                            entityName,
                            fieldName,
                            searchOrder == FieldSearchOrder.Descending ? "DESC" : string.Empty);

                        Debug.WriteLine(sql);

                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }

                    var indexinfo = new IndexInfo
                    {
                        Name = indexName
                    };

                    sql = string.Format("SELECT CHARACTER_MAXIMUM_LENGTH FROM information_schema.columns WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'"
                        , entityName, fieldName);

                    command.CommandText = sql;

                    using (var reader = command.ExecuteReader())
                    {
                        // this should always return true
                        if (reader.Read())
                        {
                            var length = reader[0];
                            if (length != DBNull.Value)
                            {
                                indexinfo.MaxCharLength = Convert.ToInt32(length);
                            }
                        }
                        else
                        {
                            if (Debugger.IsAttached) Debugger.Break();
                        }
                    }

                    m_indexNameCache.Add(indexinfo);
                }

                return indexName;
            }
            finally
            {
                if (localConnection)
                {
                    DoneWithConnection(connection, true);
                }
            }
        }

        /// <summary>
        /// Ensures that the underlying database tables contain all of the Fields to represent the known entities.
        /// This is useful if you need to add a Field to an existing store.  Just add the Field to the Entity, then 
        /// call EnsureCompatibility to have the field added to the database.
        /// </summary>
        public override void EnsureCompatibility()
        {
            if (!StoreExists)
            {
                CreateStore();
                return;
            }

            var connection = GetConnection(true);
            try
            {
                foreach (var entity in this.Entities)
                {
                    ValidateTable(connection, entity);
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
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
                using (var command = BuildFilterCommand<SqlCommand, SqlParameter>(entityName, filters, true))
                {
                    command.Connection = connection as SqlConnection;
                    return (int)command.ExecuteScalar();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        private SqlCommand GetInsertCommand(string entityName)
        {
            // TODO: support command caching to improve bulk insert speeds
            //       simply use a dictionary keyed by entityname
            var keyScheme = Entities[entityName].EntityAttribute.KeyScheme;
            var insertCommand = new SqlCommand();

            var sbFields = new StringBuilder(string.Format("INSERT INTO {0} (", entityName));
            var sbParams = new StringBuilder(" VALUES (");

            foreach (var field in Entities[entityName].Fields)
            {
                // skip auto-increments
                if ((field.IsPrimaryKey) && ((keyScheme == KeyScheme.Identity) || field.IsIdentity))
                {
                    continue;
                }
                sbFields.AppendFormat("[{0}],", field.FieldName);
                sbParams.AppendFormat("@{0},", field.FieldName);

                insertCommand.Parameters.Add(new SqlParameter(String.Format("@{0}",field.FieldName),DBNull.Value));
            }

            // replace trailing commas
            sbFields[sbFields.Length - 1] = ')';
            sbParams[sbParams.Length - 1] = ')';

            insertCommand.CommandText = sbFields.ToString() + sbParams.ToString();

            return insertCommand;
        }

        protected override void Insert(object item, bool insertReferences, IDbTransaction transaction, bool checkUpdates)
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
                    if ((field.IsPrimaryKey) && ((keyScheme == KeyScheme.Identity) || field.IsIdentity) )
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

        protected override void BulkInsert(object items, bool insertReferences, IDbTransaction transaction)
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

        protected override void BulkInsertOrUpdate(object items, bool insertReferences, IDbTransaction transaction)
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

        protected override IDataParameter CreateParameterObject(string parameterName, object parameterValue)
        {
            return new SqlParameter(parameterName, parameterValue);
        }

        private int GetIdentity(IDbConnection connection)
        {
            using (var command = new SqlCommand("SELECT @@IDENTITY", connection as SqlConnection))
            {
                object id = command.ExecuteScalar();
                return Convert.ToInt32(id);
            }
        }

        private int GetIdentity(IDbTransaction transaction)
        {
            using (var command = new SqlCommand("SELECT SCOPE_IDENTITY()", transaction.Connection as SqlConnection, transaction as SqlTransaction))
            {
                object id = command.ExecuteScalar();
                return Convert.ToInt32(id);
            }
        }

        protected override string GetPrimaryKeyIndexName(string entityName)
        {
            var connection = GetConnection(true);
            String result = null;
            try
            {
                string sql = string.Format("" +
                    "SELECT DISTINCT col.name " +
                    "FROM sys.indexes ind  " +
                    "INNER JOIN sys.index_columns ic   " +
                    "   ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id  " +
                    "INNER JOIN sys.tables t  " +
                    "	ON ind.object_id = t.object_id " +
                    "INNER JOIN sys.columns col  " +
                    "    ON ic.object_id = col.object_id and ic.column_id = col.column_id " +
                    "WHERE t.name = '{0}' " +
                    "AND ind.is_primary_key = 1 " +
                    "AND t.is_ms_shipped = 0 ", entityName);
                IDbCommand command = GetNewCommandObject();
                command.CommandText = sql;
                command.Connection = connection;
                using (var reader = command.ExecuteReader())
                {
                    DataTable DT = new DataTable();
                    DT.Load(reader);
                    foreach (DataRow drow in DT.Rows)
                    {
                        if (result == null) result = "";
                        if (result.Length > 0) result += ",";
                        result += drow["name"];
                    }
                    if (DT.Rows.Count != Entities[entityName].Fields.KeyFields.Count) throw new Exception("Missing Primary Key Fields");
                    return result;
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        public int MaxDatabaseSizeInMB 
        {
            get { return m_maxSize; }
            set
            {
                // min of 128MB
                if (value < 128) throw new ArgumentOutOfRangeException();
                // max of 4GB
                if (value > 4096) throw new ArgumentOutOfRangeException();
                m_maxSize = value;
            }
        }

        private string ConnectionString
        {
            get
            {
                if (m_connectionString == null)
                {
                    m_connectionString = string.Format(FileName, Password);
                }
                return m_connectionString;
            }
        }

        protected override IDbConnection GetNewConnectionObject()
        {
            return new SqlConnection(ConnectionString);
        }

        protected void ValidateIndex(IDbConnection connection, string indexName, string tableName, string fieldName, bool ascending)
        {
            var valid = false;

            string sql = string.Format("SELECT INDEX_NAME FROM information_schema.indexes WHERE (TABLE_NAME = '{0}') AND (COLUMN_NAME = '{1}')", tableName, fieldName);

            using (SqlCommand command = new SqlCommand(sql, connection as SqlConnection))
            {
                var name = command.ExecuteScalar() as string;

                if (string.Compare(name, indexName, true) == 0)
                {
                    valid = true;
                }

                if (!valid)
                {
                    sql = string.Format("CREATE INDEX {0} ON {1}({2} {3})",
                        indexName,
                        tableName,
                        fieldName,
                        ascending ? "ASC" : "DESC");

                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
        }

        protected void ValidateTable(IDbConnection connection, EntityInfo entity)
        {
            using (var command = new SqlCommand())
            {
                command.Connection = connection as SqlConnection;

                // first make sure the table exists
                //var sql = string.Format("SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{0}'", entity.EntityAttribute.NameInStore);
                var sql = string.Format("SELECT COUNT(*) FROM sysobjects WHERE xtype='u' AND name='{0}'", entity.EntityAttribute.NameInStore);
                command.CommandText = sql;

                var count = Convert.ToInt32(command.ExecuteScalar());

                if (count == 0)
                {
                    CreateTable(connection, entity);
                }
                else
                {
                    foreach (var field in entity.Fields)
                    {
                        if (ReservedWords.Contains(field.FieldName, StringComparer.InvariantCultureIgnoreCase))
                        {
                            throw new ReservedWordException(field.FieldName);
                        }

                        // yes, I realize hard-coded ordinals are not a good practice, but the SQL isn't changing, it's method specific
                        sql = string.Format("SELECT column_name, "  // 0
                              + "data_type, "                       // 1
                              + "character_maximum_length, "        // 2
                              + "numeric_precision, "               // 3
                              + "numeric_scale, "                   // 4
                              + "is_nullable "
                              + "FROM information_schema.columns "
                              + "WHERE (table_name = '{0}' AND column_name = '{1}')",
                              entity.EntityAttribute.NameInStore, field.FieldName);

                        command.CommandText = sql;

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                // field doesn't exist - we must create it
                                var alter = new StringBuilder(string.Format("ALTER TABLE {0} ", entity.EntityAttribute.NameInStore));
                                alter.Append(string.Format("ADD [{0}] {1} {2}",
                                    field.FieldName,
                                    GetFieldDataTypeString(entity.EntityName, field),
                                    GetFieldCreationAttributes(entity.EntityAttribute, field)));

                                using (var altercmd = new SqlCommand(alter.ToString(), connection as SqlConnection))
                                {
                                    altercmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // TODO: verify field length, etc.
                            }
                        }
                    }
                }
            }
        }

        protected override void CheckOrdinals(string entityName)
        {
            if (Entities[entityName].Fields.OrdinalsAreValid) return;

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("SELECT DISTINCT col.name, col.column_id FROM sys.columns col INNER JOIN sys.tables t 	ON col.object_id = t.object_id WHERE t.name = '{0}'", entityName);
                    DataTable DT;
                    using (var reader = command.ExecuteReader())
                    {
                        DT = new DataTable();
                        DT.Load(reader);
                        foreach (DataRow row in DT.Rows)
                        {
                            String fieldName = ((String)row["name"]).ToUpperInvariant();
                            Int32 fieldOrdinal = ((Int32)row["column_id"]) - 1;
                            FieldAttribute field;
                            field = (from FieldAttribute el in Entities[entityName].Fields
                                     where el.FieldName.ToUpperInvariant() == fieldName
                                     select el).FirstOrDefault<FieldAttribute>();
                            if (field != null) field.Ordinal = fieldOrdinal;
                        }
                        if (DT.Rows.Count == Entities[entityName].Fields.Count)
                            Entities[entityName].Fields.OrdinalsAreValid = true;
                        DT.Dispose();
                    }
                    //using (var reader = command.ExecuteReader())
                    //{
                    //    foreach (var field in Entities[entityName].Fields)
                    //    {
                    //        field.Ordinal = reader.GetOrdinal(field.FieldName);
                    //    }

                    //    Entities[entityName].Fields.OrdinalsAreValid = true;
                    //}
                    command.Dispose();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }
    }
}
