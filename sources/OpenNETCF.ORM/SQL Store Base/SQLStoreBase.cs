using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace OpenNETCF.ORM
{
    public abstract class SQLStoreBase<TEntityInfo> : DataStore<TEntityInfo>, IDisposable
        where TEntityInfo : EntityInfo, new()
    {
        protected List<IndexInfo> m_indexNameCache = new List<IndexInfo>();
        private IDbConnection m_connection;
        private Dictionary<Type, MethodInfo> m_serializerCache = new Dictionary<Type, MethodInfo>();
        private Dictionary<Type, MethodInfo> m_deserializerCache = new Dictionary<Type, MethodInfo>();
        private Dictionary<Type, object[]> m_referenceCache = new Dictionary<Type, object[]>();

        public int DefaultStringFieldSize { get; set; }
        public int DefaultNumericFieldPrecision { get; set; }
        public int DefaultVarBinaryLength { get; set; }
        protected abstract string AutoIncrementFieldIdentifier { get; }

        public ConnectionBehavior ConnectionBehavior { get; set; }

        public abstract override void CreateStore();
        public abstract override void CreateOrUpdateStore();
        public abstract override void DeleteStore();
        public abstract override void EnsureCompatibility();

        public abstract override bool StoreExists { get; }

        public override bool HasConnection
        {
            get
            {
                var conn = GetConnection(true);
                try
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open) return true;
                    return false;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    DoneWithConnection(conn, true);
                }
            }
        }

        protected abstract string GetPrimaryKeyIndexName(string entityName);

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
            IDbConnection connection = null;
            if (transactional)
                transaction = GetTransaction(false);
            else
                connection = GetConnection(false);
            try
            {
                InsertOrUpdate(item, insertReferences, connection, transaction);
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
                DoneWithConnection(connection, false);
            }
        }
        protected virtual void InsertOrUpdate(object item, bool insertReferences, IDbConnection connection, IDbTransaction transaction)
        {
            if (this.Contains(item, connection))
            {
                Update(item, insertReferences, null, connection, transaction);
            }
            else
            {
                Insert(item, insertReferences, connection, transaction, true);
            }
        }

        public override void Insert(object item, bool insertReferences)
        {
            Insert(item, insertReferences, false);
        }
        public override void Insert(object item, bool insertReferences, bool transactional)
        {
            IDbTransaction transaction = null;
            IDbConnection connection = null;
            if (transactional)
                transaction = GetTransaction(false);
            else
                connection = GetConnection(false);
            try
            {
                Insert(item, insertReferences, connection, transaction, false);
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
                DoneWithConnection(connection, false);
            }
        }
        protected abstract void Insert(object item, bool insertReferences, IDbConnection connection, IDbTransaction transaction, bool checkUpdate);


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

        public override void Update(object item, List<string> fieldNames)
        {
            Update(item, false, fieldNames);
        }
        public override void Update(object item, bool cascadeUpdates, List<string> fieldNames)
        {
            Update(item, cascadeUpdates, fieldNames, false);
        }
        public override void Update(object item, bool cascadeUpdates, List<string> fieldNames, bool transactional)
        {
            IDbTransaction transaction = null;
            IDbConnection connection = null;
            if (transactional)
                transaction = GetTransaction(false);
            else
                connection = GetConnection(false);
            try
            {
                Update(item, cascadeUpdates, fieldNames, connection, transaction);
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
                DoneWithConnection(connection, false);
            }
        }
        protected abstract void Update(object item, bool cascadeUpdates, List<string> fieldNames, IDbConnection connection, IDbTransaction transaction);

        public override void BulkInsert(object items, bool insertReferences)
        {
            BulkInsert(items, insertReferences, false);
        }
        public override void BulkInsert(object items, bool insertReferences, bool transactional)
        {
            IDbTransaction transaction = null;
            IDbConnection connection = null;
            if (transactional)
                transaction = GetTransaction(false);
            else
                connection = GetConnection(false);
            try
            {
                BulkInsert(items, insertReferences, connection, transaction);
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
                DoneWithConnection(connection, false);
            }
        }
        protected abstract void BulkInsert(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction);

        public override void BulkInsertOrUpdate(object items, bool insertReferences)
        {
            BulkInsertOrUpdate(items, insertReferences, false);
        }
        public override void BulkInsertOrUpdate(object items, bool insertReferences, bool transactional)
        {
            IDbTransaction transaction = null;
            IDbConnection connection = null;
            if (transactional)
                transaction = GetTransaction(false);
            else
                connection = GetConnection(false);
            try
            {
                BulkInsertOrUpdate(items, insertReferences, connection, transaction);
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
                DoneWithConnection(connection, false);
            }
        }
        protected abstract void BulkInsertOrUpdate(object items, bool insertReferences, IDbConnection connection, IDbTransaction transaction);

        protected abstract IDbCommand GetNewCommandObject();
        protected abstract IDbConnection GetNewConnectionObject();
        protected abstract IDataParameter CreateParameterObject(string parameterName, object parameterValue);

        public SQLStoreBase()
        {
            DefaultStringFieldSize = 255;
            DefaultNumericFieldPrecision = 16;
            DefaultVarBinaryLength = 8000;
        }

        ~SQLStoreBase()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (m_connection != null)
            {
                if (m_connection.State == ConnectionState.Open)
                    m_connection.Close();
                m_connection.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        protected virtual IDbConnection GetConnection(bool maintenance)
        {
            switch (ConnectionBehavior)
            {
                case ConnectionBehavior.AlwaysNew:
                    var connection = GetNewConnectionObject();
                    connection.Open();
                    return connection;
                case ConnectionBehavior.HoldMaintenance:
                    if (m_connection == null)
                    {
                        m_connection = GetNewConnectionObject();
                        m_connection.Open();
                    }
                    if (maintenance) return m_connection;
                    var connection2 = GetNewConnectionObject();
                    connection2.Open();
                    return connection2;
                case ConnectionBehavior.Persistent:
                    if (m_connection == null)
                    {
                        m_connection = GetNewConnectionObject();
                        m_connection.Open();
                    }
                    return m_connection;
                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual IDbTransaction GetTransaction(bool maintenance)
        {
            var connection = GetConnection(maintenance);
            return connection.BeginTransaction();
        }

        protected virtual void DoneWithTransaction(IDbTransaction transaction, bool maintenance)
        {
            if (transaction != null)
            {
                try
                {
                    DoneWithConnection(transaction.Connection, maintenance);
                }
                catch { }
                finally
                {
                     transaction.Dispose();
                }
            }
        }

        protected virtual void DoneWithConnection(IDbConnection connection, bool maintenance)
        {
            if (connection != null)
            {
                switch (ConnectionBehavior)
                {
                    case ConnectionBehavior.AlwaysNew:
                        connection.Close();
                        connection.Dispose();
                        break;
                    case ConnectionBehavior.HoldMaintenance:
                        if (maintenance) return;
                        connection.Close();
                        connection.Dispose();
                        break;
                    case ConnectionBehavior.Persistent:
                        return;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public override bool TableExists(String entityName)
        {
            var entityInfo = this.GetEntityInfo(entityName);
            if (entityInfo != null)
                return TableExists(entityInfo);
            else
                return false;
        }

        public override bool TableExists(EntityInfo entityInfo)
        {
            Boolean exists = false;
            var connection = GetConnection(false);
            try
            {
                string entityName = m_entities.GetNameForType(entityInfo.EntityType);

                if (entityName == null)
                {
                    throw new EntityNotFoundException(entityInfo.EntityType);
                }
                var entity = this.m_entities[entityName];
                exists = this.TableExists(connection, entity);
            }
            catch { }
            finally
            {
                DoneWithConnection(connection, false);
            }
            return exists;
        }

        public override bool FieldExists(String entityName, String fieldName)
        {
            var entityInfo = this.GetEntityInfo(entityName);
            if (entityInfo != null)
            {
                if (entityInfo.Fields.HasField(fieldName))
                {
                    var fieldInfo = entityInfo.Fields[fieldName];
                    return FieldExists(entityInfo, fieldInfo);
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }

        public override bool FieldExists(EntityInfo entityInfo, FieldAttribute fieldInfo)
        {
            Boolean exists = false;
            var connection = GetConnection(false);
            try
            {
                string entityName = m_entities.GetNameForType(entityInfo.EntityType);

                if (entityName == null)
                {
                    throw new EntityNotFoundException(entityInfo.EntityType);
                }
                var entity = this.m_entities[entityName];
                exists = this.FieldExists(connection, entityInfo, fieldInfo);
            }
            catch { }
            finally
            {
                DoneWithConnection(connection, false);
            }
            return exists;
        }

        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, false);
        }

        public int ExecuteNonQuery(string sql, bool throwExceptions)
        {
            var connection = GetConnection(false);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = sql;
                    OnSqlStatementCreated(command, null);
                    command.Connection = connection;
                    return command.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                if (throwExceptions) throw;

                Debug.WriteLine("SQLStoreBase::ExecuteNonQuery threw: " + ex.Message);
                return 0;
            }
            finally
            {
                DoneWithConnection(connection, false);
            }
        }

        public object ExecuteScalar(string sql)
        {
            var connection = GetConnection(false);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = sql;
                    OnSqlStatementCreated(command, null);
                    return command.ExecuteScalar();
                }
            }
            finally
            {
                DoneWithConnection(connection, false);
            }
        }

        protected virtual string[] ReservedWords
        {
            get { return m_ReservedWords; }
        }

        //protected virtual void CreateTable(IDbConnection connection, EntityInfo entity)
        //{
        //    StringBuilder sql = new StringBuilder();

        //    if (ReservedWords.Contains(entity.EntityName, StringComparer.InvariantCultureIgnoreCase))
        //    {
        //        throw new ReservedWordException(entity.EntityName);
        //    }

        //    sql.AppendFormat("CREATE TABLE {0} (", entity.EntityName);

        //    int count = entity.Fields.Count;

        //    foreach (var field in entity.Fields)
        //    {
        //        //if (field is ReferenceFieldAttribute)
        //        //{
        //        //    count--;
        //        //    continue;
        //        //}

        //        if (ReservedWords.Contains(field.FieldName, StringComparer.InvariantCultureIgnoreCase))
        //        {
        //            throw new ReservedWordException(field.FieldName);
        //        }

        //        sql.AppendFormat("[{0}] {1} {2}",
        //            field.FieldName,
        //            GetFieldDataTypeString(entity.EntityName, field),
        //            GetFieldCreationAttributes(entity.EntityAttribute, field));

        //        if (--count > 0) sql.Append(", ");
        //    }

        //    sql.Append(")");

        //    Debug.WriteLine(sql);

            
        //    using (var command = GetNewCommandObject())
        //    {
        //        command.CommandText = sql.ToString();
        //        command.Connection = connection;
        //        int i = command.ExecuteNonQuery();
        //    }

        //    // create indexes
        //    foreach (var field in entity.Fields)
        //    {
        //        if (field.SearchOrder != FieldSearchOrder.NotSearchable)
        //        {
        //            VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
        //        }
        //    }
        //}
        protected override void CreateTable(EntityInfo entity)
        {
            var conn = GetConnection(false);
            try
            {
                CreateTable(conn, entity);
            }
            finally
            {
                DoneWithConnection(conn, false);
            }
        }

        protected virtual void CreateTable(IDbConnection connection, EntityInfo entity)
        {
            CreateTable(connection, entity, false);
        }

        /// <summary>
        /// Creates the table if it does not already exists and create the fields if they don't already exist.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        protected virtual void CreateTable(IDbConnection connection, EntityInfo entity, Boolean forceMultiplePrimaryKeysSyntax)
        {
            Boolean bTableExists = false;
            Boolean bMultiplePrimaryKeys = false;
            if (ReservedWords.Contains(entity.EntityName, StringComparer.InvariantCultureIgnoreCase))
            {
                throw new ReservedWordException(entity.EntityName);
            }

            bTableExists = TableExists(connection, entity);

            bMultiplePrimaryKeys = (entity.Fields.KeyFields.Count > 1) | forceMultiplePrimaryKeysSyntax;

            // Handles the case of tables with multiple primary keys
            if (!bTableExists)
            {
                StringBuilder sql = new StringBuilder();
                StringBuilder keys = new StringBuilder();
                sql.AppendFormat("CREATE TABLE {0} ( ", entity.EntityName);
                int count = entity.Fields.Count;
                int keycount = entity.Fields.KeyFields.Count;
                if (count > 0)
                {
                    foreach (var field in entity.Fields)
                    {
                        sql.AppendFormat(" {0} {1} {2} ",
                            field.FieldName,
                            GetFieldDataTypeString(entity.EntityName, field),
                            GetFieldCreationAttributes(entity.EntityAttribute, field, bMultiplePrimaryKeys));
                        if (field.IsPrimaryKey)
                        {
                            keys.Append(field.FieldName);
                            if (--keycount > 0) keys.Append(", ");
                        }
                        if (--count > 0) sql.Append(", ");
                    }
                    if (bMultiplePrimaryKeys) sql.AppendFormat(", PRIMARY KEY({0})", keys.ToString());
                    sql.Append(" )");
                }
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = sql.ToString();
                    command.Connection = connection;
                    OnSqlStatementCreated(command, null);
                    int i = command.ExecuteNonQuery();
                }
                bTableExists = true;
                foreach (var field in entity.Fields)
                {
                    if (field.SearchOrder != FieldOrder.None)
                    {
                        field.IndexName = VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
                    }
                }
            }
            else
            {
                foreach (var field in entity.Fields)
                {
                    StringBuilder sql = new StringBuilder();
                    if (!FieldExists(connection, entity, field))
                    {
                        if (ReservedWords.Contains(field.FieldName, StringComparer.InvariantCultureIgnoreCase))
                        {
                            throw new ReservedWordException(field.FieldName);
                        }
                        sql.AppendFormat("ALTER TABLE {0} ADD ", entity.EntityName);
                        // ALTER TABLE {TABLENAME} 
                        // ADD {COLUMNNAME} {TYPE} {NULL|NOT NULL} 
                        // CONSTRAINT {CONSTRAINT_NAME} DEFAULT {DEFAULT_VALUE}
                        sql.AppendFormat(" {0} {1} {2} ",
                            field.FieldName,
                            GetFieldDataTypeString(entity.EntityName, field),
                            GetFieldCreationAttributes(entity.EntityAttribute, field, bMultiplePrimaryKeys));
                        using (var command = GetNewCommandObject())
                        {
                            command.CommandText = sql.ToString();
                            command.Connection = connection;
                            OnSqlStatementCreated(command, null);
                            int i = command.ExecuteNonQuery();
                        }
                        // create indexes
                        if (field.SearchOrder != FieldOrder.None)
                        {
                            field.IndexName = VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
                        }
                    }
                    else
                    {
                        // create indexes
                        if (field.SearchOrder != FieldOrder.None)
                        {
                            field.IndexName = VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
                        }
                    }
                }
            }
        }

        protected virtual void CreateTableRecursive(IDbConnection connection, EntityInfo entity)
        {
            CreateTable(connection, entity);
            if (entity.References.Count > 0)
            {
                foreach (var reference in entity.References)
                {
                    string entityName = this.m_entities.GetNameForType(reference.ReferenceEntityType);

                    if (entityName == null)
                    {
                        throw new EntityNotFoundException(reference.ReferenceEntityType);
                    }
                    CreateTableRecursive(connection, this.m_entities[entityName]);
                }
            }
        }

        public override void DropAndCreateTable<T>(bool cascade)
        {
            DropAndCreateTable(typeof(T), cascade);
        }

        public override void DropAndCreateTable(Type entityType, bool cascade)
        {
            if (entityType.Equals(typeof(DynamicEntity)))
            {
                throw new ArgumentException("DynamicEntities must be dropped with one of the other Drop overloads.");
            }

            string entityName = m_entities.GetNameForType(entityType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }
            DropAndCreateTable(entityName, cascade);
        }

        public override void DropAndCreateTable(String entityName, bool cascade)
        {
            if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);

            EntityInfo entity = m_entities[entityName];

            var connection = GetConnection(true);
            try
            {
                DropAndCreateTable(connection, entity, cascade);
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        protected virtual void DropAndCreateTable(IDbConnection connection, EntityInfo entity, bool cascade)
        {
            if (cascade)
                DropRecursive(connection, entity);
            else if (TableExists(connection, entity))
                Drop(connection, entity);
            
            if (cascade)
                CreateTableRecursive(connection, entity);
            else
                CreateTable(connection, entity);
        }

        protected virtual Boolean FieldExists(IDbConnection connection, EntityInfo entity, FieldAttribute field)
        {
            Boolean exists = false;
            try
            {
                // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.  
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = String.Format("select count(*) from Information_SCHEMA.columns where table_name='{0}' and column_name='{1}'", entity.EntityName, field.FieldName);
                    command.Connection = connection;
                    OnSqlStatementCreated(command, null);
                    exists = (int)command.ExecuteScalar() > 0;
                }
            }
            catch
            {
                try
                {
                    exists = true;
                    //TODO: Bad practice, but works across all DBs, need to work on a better test
                    using (var command = GetNewCommandObject())
                    {
                        command.CommandText = String.Format("select {1} from {0} where 1 = 0", entity.EntityName, field.FieldName);
                        command.Connection = connection;
                        OnSqlStatementCreated(command, null);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    exists = false;
                }
            }
            return exists;
        }

        protected virtual Boolean TableExists(IDbConnection connection, EntityInfo entity)
        {
            Boolean exists = false;
            try
            {
                // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.  
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = String.Format("select count(*) from information_schema.tables where table_name = '{0}'", entity.EntityName);
                    command.Connection = connection;
                    OnSqlStatementCreated(command, null);
                    exists = (int)command.ExecuteScalar() == 1;
                }
            }
            catch
            {
                try
                {
                    exists = true;
                    //TODO: Other RDBMS.  Not so graceful degradation
                    using (var command = GetNewCommandObject())
                    {
                        command.CommandText = String.Format("select 1 from {0} where 1 = 0", entity.EntityName);
                        command.Connection = connection;
                        OnSqlStatementCreated(command, null);
                        command.ExecuteNonQuery();
                    }
                }
                catch
                {
                    exists = false;
                }
            }
            return exists;
        }

        protected class IndexInfo
        {
            public IndexInfo()
            {
                MaxCharLength = -1;
            }

            public string Name { get; set; }
            public int MaxCharLength { get; set; }
        }

        protected IndexInfo GetIndexInfo(string indexName)
        {
            return m_indexNameCache.FirstOrDefault(ii => ii.Name == indexName);
        }

        protected virtual string VerifyIndex(string entityName, string fieldName, FieldOrder searchOrder)
        {
            return VerifyIndex(entityName, fieldName, searchOrder, null);
        }

        protected virtual string VerifyIndex(string entityName, string fieldName, FieldOrder searchOrder, IDbConnection connection)
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
                    searchOrder == FieldOrder.Descending ? "DESC" : "ASC");
                
                if (m_indexNameCache.FirstOrDefault(ii => ii.Name == indexName) != null) return indexName;

                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;

                    var sql = string.Format("SELECT COUNT(*) FROM information_schema.indexes WHERE INDEX_NAME = '{0}'", indexName);
                    command.CommandText = sql;
                    OnSqlStatementCreated(command, null);
                    int i = (int)command.ExecuteScalar();

                    if (i == 0)
                    {
                        sql = string.Format("CREATE INDEX {0} ON {1}({2} {3})",
                            indexName,
                            entityName,
                            fieldName,
                            searchOrder == FieldOrder.Descending ? "DESC" : string.Empty);

                        Debug.WriteLine(sql);

                        command.CommandText = sql;
                        OnSqlStatementCreated(command, null);
                        command.ExecuteNonQuery();
                    }

                    var indexinfo = new IndexInfo
                    {
                        Name = indexName
                    };

                    sql = string.Format("SELECT CHARACTER_MAXIMUM_LENGTH FROM information_schema.columns WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'"
                        , entityName, fieldName);

                    command.CommandText = sql;
                    OnSqlStatementCreated(command, null);
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

        protected virtual string GetFieldDataTypeString(string entityName, FieldAttribute field)
        {
            // the SQL RowVersion is a special case
            if (field.IsRowVersion)
            {
                switch (field.DataType)
                {
                    case DbType.UInt64:
                    case DbType.Int64:
                        // no error
                        break;
                    default:
                        throw new FieldDefinitionException(entityName, field.FieldName, "rowversion fields must be an 8-byte data type (Int64 or UInt64)");
                }

                return "rowversion";
            }

            if (field.DataType == DbType.Binary)
            {
                // default to varbinary unless a Length is specifically supplied and it is >= 8000
                if (field.Length >= 8000)
                {
                    return "image";
                }
                // if no length was supplied, default to DefaultVarBinaryLength (8000)
                return string.Format("varbinary({0})", field.Length == 0 ? DefaultVarBinaryLength : field.Length);
            }

            return field.DataType.ToSqlTypeString();
        }

        protected virtual string GetFieldCreationAttributes(EntityAttribute attribute, FieldAttribute field)
        {
            return GetFieldCreationAttributes(attribute, field, false);
        }

        protected virtual string GetFieldCreationAttributes(EntityAttribute attribute, FieldAttribute field, Boolean MultiplePrimaryKeys)
        {
            StringBuilder sb = new StringBuilder();

            switch (field.DataType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.String:
                    if (field.Length > 0)
                    {
                        sb.AppendFormat("({0}) ", field.Length);
                    }
                    else
                    {
                        sb.AppendFormat("({0}) ", DefaultStringFieldSize);
                    }
                    break;
                case DbType.Decimal:
                    int p = field.Precision == 0 ? DefaultNumericFieldPrecision : field.Precision;
                    sb.AppendFormat("({0},{1}) ", p, field.Scale);
                    break;
            }

            if (field.Default != null && field.Default.Length > 0)
            {
                sb.AppendFormat("DEFAULT {0} ", field.Default);
            }

            if (field.IsPrimaryKey)
            {
                if (!MultiplePrimaryKeys) sb.Append("PRIMARY KEY ");

                if (attribute.KeyScheme == KeyScheme.Identity)
                {
                    switch(field.DataType)
                    {
                        case DbType.Int32:
                        case DbType.UInt32:
                        case DbType.Int64:
                        case DbType.UInt64:
                            sb.Append(AutoIncrementFieldIdentifier + " ");
                            break;
                        case DbType.Guid:
                            sb.Append("ROWGUIDCOL ");
                            break;
                        default:
                            throw new FieldDefinitionException(attribute.NameInStore, field.FieldName,
                                string.Format("Data Type '{0}' cannot be marked as an Identity field", field.DataType));
                    }
                }
            }

            if (!field.AllowsNulls & !field.IsPrimaryKey)
            {
                sb.Append("NOT NULL ");
            }

            if (field.RequireUniqueValue & !field.IsPrimaryKey)
            {
                sb.Append("UNIQUE ");
            }

            return sb.ToString();
        }

        [Obsolete("Replace with entity Delegate")]
        protected virtual MethodInfo GetSerializer(Type itemType)
        {
            if (m_serializerCache.ContainsKey(itemType))
            {
                return m_serializerCache[itemType];
            }

            var serializer = itemType.GetMethod("Serialize", BindingFlags.Public | BindingFlags.Instance);

            if (serializer == null) return null;

            m_serializerCache.Add(itemType, serializer);
            return serializer;
        }

        [Obsolete("Replace with entity Delegate")]
        protected virtual MethodInfo GetDeserializer(Type itemType)
        {
            if (m_deserializerCache.ContainsKey(itemType))
            {
                return m_deserializerCache[itemType];
            }

            var deserializer = itemType.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Instance);

            if (deserializer == null) return null;

            m_deserializerCache.Add(itemType, deserializer);
            return deserializer;
        }

        public override bool Contains(object item)
        {
            return Contains(item, null);
        }

        public virtual bool Contains(object item, IDbConnection connection)
        {
            var itemType = item.GetType();
            string entityName = m_entities.GetNameForType(itemType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(item.GetType());
            }

            // Commented - 2012.08.08 - Old unique KeyField working
            //var keyValue = this.Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);
            //var existing = Select(itemType, null, keyValue, -1, -1).FirstOrDefault();

            // Added - 2012.08.08 - More generic way of working
            var filters = new List<FilterCondition>();
            foreach (FieldAttribute field in Entities[entityName].Fields.KeyFields)
            {
                filters.Add(new FilterCondition(field.FieldName, field.PropertyInfo.GetValue(item, null), FilterCondition.FilterOperator.Equals));
            }
            var existing = Select(entityName, filters, -1, -1, false, false, connection).FirstOrDefault();

            return existing != null;
        }

        public override List<DynamicEntityInfo> ReverseEngineer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a single entity instance from the DataStore identified by the specified primary key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public override T Select<T>(object primaryKey)
        {
            return (T)Select(m_entities.GetNameForType(typeof(T)), null, primaryKey, -1, -1, false, false, null).FirstOrDefault();
        }

        public override T Select<T>(object primaryKey, bool fillReferences)
        {
            return (T)Select(m_entities.GetNameForType(typeof(T)), null, primaryKey, -1, -1, fillReferences, false, null).FirstOrDefault();
        }

        public override T Select<T>(object primaryKey, bool fillReferences, bool filterReferences)
        {
            return (T)Select(m_entities.GetNameForType(typeof(T)), null, primaryKey, -1, -1, fillReferences, filterReferences, null).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T[] Select<T>()
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), null, -1, -1, false, false, null).Cast<T>().ToArray();
        }

        /// <summary>
        /// Retrieves all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T[] Select<T>(bool fillReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), null, -1, -1, fillReferences, false, null).Cast<T>().ToArray();
        }

        /// <summary>
        /// Retrieves all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fillReferences"></param>
        /// <param name="filterReferences"></param>
        /// <returns></returns>
        public override T[] Select<T>(bool fillReferences, bool filterReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), null, -1, -1, fillReferences, filterReferences, null).Cast<T>().ToArray();
        }

        /// <summary>
        /// Retrieves all entity instances of the specified type from the DataStore
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public override object[] Select(Type entityType)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(entityType), null, -1, -1, false, false, null);
        }

        public override object[] Select(Type entityType, bool fillReferences)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(entityType), null, -1, -1, fillReferences, false, null);
        }

        public override object[] Select(Type entityType, bool fillReferences, bool filterReferences)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(entityType), null, null, -1, 0, fillReferences, filterReferences, null);
        }

        public override object[] Select(Type entityType, object primaryKey, bool fillReferences, bool filterReferences)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(entityType), null, primaryKey, -1, 0, fillReferences, filterReferences, null);
        }

        public override object[] Select(Type entityType, IEnumerable<FilterCondition> filters, bool fillReferences)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(entityType), filters, -1, -1, fillReferences, false, null);
        }

        public override object[] Select(Type entityType, IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(entityType), filters, -1, -1, fillReferences, filterReferences, null);
        }

        public override T[] Select<T>(string searchFieldName, object matchValue)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), searchFieldName, matchValue, -1, 0, false, false, null).Cast<T>().ToArray();
        }

        public override T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), searchFieldName, matchValue, -1, 0, fillReferences, false, null).Cast<T>().ToArray();
        }

        public override T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences, bool filterReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), searchFieldName, matchValue, -1, 0, fillReferences, filterReferences, null).Cast<T>().ToArray();
        }

        public override T[] Select<T>(IEnumerable<FilterCondition> filters)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), filters, -1, 0, false, false, null).Cast<T>().ToArray();
        }

        public override T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), filters, -1, 0, fillReferences, false, null).Cast<T>().ToArray();
        }

        public override T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(m_entities.GetNameForType(typeof(T)), filters, -1, 0, fillReferences, filterReferences, null).Cast<T>().ToArray();
        }

        public override object[] Select(String entityName)
        {
            return Select(entityName, null, -1, 0, false, false, null);
        }
        public override object[] Select(String entityName, bool fillReferences)
        {
            return Select(entityName, null, -1, 0, fillReferences, false, null);
        }
        public override object[] Select(String entityName, object primaryKey, bool fillReferences)
        {
            return Select(entityName, null, primaryKey, -1, 0, fillReferences, false, null);
        }
        public override object[] Select(String entityName, object primaryKey, bool fillReferences, bool filterReferences)
        {
            return Select(entityName, null, primaryKey, -1, 0, fillReferences, filterReferences, null);
        }
        public override object[] Select(String entityName, IEnumerable<FilterCondition> filters, bool fillReferences)
        {
            return Select(entityName, filters, -1, 0, fillReferences, false, null);
        }
        public override object[] Select(String entityName, IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences)
        {
            return Select(entityName, filters, -1, 0, fillReferences, filterReferences, null);
        }

        protected virtual object[] Select(string entityName, string searchFieldName, object matchValue, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences, IDbConnection connection)
        {
            if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);

            FilterCondition filter = null;

            if (searchFieldName == null)
            {
                if (matchValue != null)
                {
                    CheckPrimaryKeyIndex(entityName);

                    // searching on primary key
                    filter = new SqlFilterCondition
                    {
                        FieldName = (Entities[entityName] as SqlEntityInfo).Fields.KeyField.FieldName,
                        Operator = FilterCondition.FilterOperator.Equals,
                        Value = matchValue,
                        PrimaryKey = true
                    };
                }
            }
            else
            {
                filter = new FilterCondition
                {
                    FieldName = searchFieldName,
                    Operator = FilterCondition.FilterOperator.Equals,
                    Value = matchValue
                };
            }

            return Select(
                entityName,
                (filter == null) ? null :
                    new FilterCondition[]
                    {
                        filter
                    },
                fetchCount,
                firstRowOffset,
                fillReferences
                , filterReferences, connection);
        }

        protected abstract object[] Select(String entityName, IEnumerable<FilterCondition> filters, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences, IDbConnection connection);

        private const int CommandCacheMaxLength = 10;
        protected Dictionary<string, DbCommand> CommandCache = new Dictionary<string, DbCommand>();

        /// <summary>
        /// Determines if the ORM engine should be allowed to cache commands of not.  If you frequently use the same FilterConditions on a Select call to a single entity, 
        /// using the command cache can improve performance by preventing the underlying SQL Compact Engine from recomputing statistics.
        /// </summary>
        public bool UseCommandCache { get; set; }

        public void ClearCommandCache()
        {
            lock (CommandCache)
            {
                foreach (var cmd in CommandCache)
                {
                    cmd.Value.Dispose();
                }
                CommandCache.Clear();
            }
        }

        protected virtual TCommand GetSelectCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters, int rowOffset, int rowCount, out bool tableDirect)
            where TCommand : DbCommand, new()
            where TParameter : IDataParameter, new()
        {
            tableDirect = false;
            return BuildFilterCommand<TCommand, TParameter>(entityName, filters, false, rowOffset, rowCount);
        }

        protected TCommand BuildFilterCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters)
            where TCommand : DbCommand, new()
            where TParameter : IDataParameter, new()
        {
            return BuildFilterCommand<TCommand, TParameter>(entityName, filters, false, 0, 0);
        }

        protected virtual TCommand BuildFilterCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters, bool isCount, int rowOffset, int rowCount)
            where TCommand : DbCommand, new()
            where TParameter : IDataParameter, new()
        {
            var command = new TCommand();
            command.CommandType = CommandType.Text;
            var @params = new List<TParameter>();

            StringBuilder sb;
            sb = new StringBuilder("SELECT ");
            if (isCount)
            {
                FieldAttribute fa = (from FieldAttribute el in Entities[entityName].Fields
                                     where el.IsPrimaryKey || el.SearchOrder != FieldOrder.None
                                    select el).FirstOrDefault<FieldAttribute>();
                if (fa == null)
                {
                    sb.Append(" COUNT(*) ");
                }
                else 
                {
                    sb.AppendFormat(" COUNT({0}.{1}) ",entityName, fa.FieldName);
                }
            }
            else
            {
                sb.Append(BuildFieldsSQL(Entities[entityName], rowOffset, rowCount));
            }
            sb.AppendFormat(" FROM {0} ", entityName);

            if (filters != null)
            {
                for (int i = 0; i < filters.Count(); i++)
                {
                    sb.Append(i == 0 ? " WHERE " : String.Format(" {0} ",FieldAttribute.GetName(typeof(FilterCondition.LogicalOperator), (int)filters.ElementAt(i).WhereOperator)));
                    
                    var filter = filters.ElementAt(i);

                    string paramName = string.Format("@p{0}", i);

                    string paramsql = BuildParameterSQL(paramName, filter);
                    sb.Append(paramsql);
                    // In the case of a IS NULL or so, we don't use the Param, no need to add it.
                    if (paramsql.Contains(paramName))
                    {
                        var param = new TParameter()
                        {
                            ParameterName = paramName,
                            Value = filter.Value ?? DBNull.Value
                        };

                        @params.Add(param);
                    }
                }
            }
            if (Entities[entityName].SortingFields.Count > 0)
            {
                sb.Append(" ORDER BY ");
                foreach (FieldAttribute fa in Entities[entityName].SortingFields.OrderBy(d => d.SortSequence))
                {
                    if (fa.SearchOrder == FieldOrder.Descending)
                        sb.AppendFormat(" {0} DESC,", fa.FieldName);
                    else
                        sb.AppendFormat(" {0} ASC,", fa.FieldName);
                }
                sb.Remove(sb.Length - 1, 1);
            }
            var sql = sb.ToString();
            command.CommandText = sql;
            command.Parameters.AddRange(@params.ToArray());

            if (UseCommandCache)
            {
                lock (CommandCache)
                {
                    if (CommandCache.ContainsKey(sql))
                    {
                        command.Dispose();
                        command = (TCommand)CommandCache[sb.ToString()];

                        // use the cached command object, but we must copy over the new command parameter values
                        // or it will use the old ones
                        for (int p = 0; p < command.Parameters.Count; p++)
                        {
                            command.Parameters[p].Value = @params[p].Value;
                        }
                    }
                    else
                    {
                        CommandCache.Add(sql, command);

                        // trim the cache so it doesn't grow infinitely
                        if (CommandCache.Count > CommandCacheMaxLength)
                        {
                            CommandCache.Remove(CommandCache.First().Key);
                        }
                    }
                }
            }

            return command;
        }

        protected virtual String BuildParameterSQL(string paramName, FilterCondition filter)
        {
            var sb = new StringBuilder();
            sb.Append("[" + filter.FieldName + "]");
            switch (filter.Operator)
            {
                case FilterCondition.FilterOperator.Equals:
                    if ((filter.Value == null) || (filter.Value == DBNull.Value))
                        sb.Append(" IS NULL ");
                    else
                        sb.AppendFormat(" = {0}", paramName);
                    break;
                case FilterCondition.FilterOperator.Like:
                    sb.AppendFormat(" LIKE {0}", paramName);
                    break;
                case FilterCondition.FilterOperator.LessThan:
                    sb.AppendFormat(" < {0}", paramName);
                    break;
                case FilterCondition.FilterOperator.GreaterThan:
                    sb.AppendFormat(" > {0}", paramName);
                    break;
                case FilterCondition.FilterOperator.LessThanOrEqual:
                    sb.AppendFormat(" <= {0}", paramName);
                    break;
                case FilterCondition.FilterOperator.GreaterThanOrEqual:
                    sb.AppendFormat(" >= {0}", paramName);
                    break;
                case FilterCondition.FilterOperator.In:
                    if (filter.Value.GetType().IsArray)
                    {
                        // If we received an array as a parameter, we transform it to a String array
                        // using the ToString() function of the object. This has the benefit that
                        // we can give an array of custom objects and use their overriden ToString method.
                        var arr = filter.Value as Array;
                        String[] strarr = new String[arr.Length];
                        for (int k = 0; k < arr.Length; k++) { strarr[k] = arr.GetValue(k).ToString(); }
                        sb.AppendFormat(" IN ({0})", String.Join(",", strarr));
                    }
                    else
                    {
                        sb.AppendFormat(" IN ({0})", filter.Value.ToString());
                    }
                    break;
                case FilterCondition.FilterOperator.NotEquals:
                    if ((filter.Value == null) || (filter.Value == DBNull.Value))
                        sb.Append(" IS NOT NULL ");
                    else
                        sb.AppendFormat(" <> {0}", paramName);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return sb.ToString();
        }

        protected virtual String BuildFieldsSQL(EntityInfo entity, int rowOffset, int rowCount)
        {
            var sb = new StringBuilder();
            foreach (FieldAttribute fa in entity.Fields)
            {
                sb.AppendFormat(" {0}.{1},", entity.EntityName, fa.FieldName);
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        protected virtual void CascadeUpdates(object item, List<string> fieldNames, object keyValue, EntityInfo entity, IDbConnection connection, IDbTransaction transaction)
        {
            if (keyValue == null && entity.Fields.KeyField != null)
            {
                if (item is DynamicEntity)
                {
                    keyValue = ((DynamicEntity)item)[entity.Fields.KeyField.FieldName];
                }
                else
                {
                    keyValue = entity.Fields.KeyField.PropertyInfo.GetValue(item, null);
                }
            }

            foreach (var reference in entity.References)
            {
                var itemList = reference.PropertyInfo.GetValue(item, null) as Array;
                if (itemList != null)
                {
                    foreach (var refItem in itemList)
                    {
                        if (!this.Contains(refItem, connection))
                        {
                            var foreignKey = refItem.GetType().GetProperty(reference.ReferenceField, BindingFlags.Instance | BindingFlags.Public);
                            foreignKey.SetValue(refItem, keyValue, null);
                            Insert(refItem, true, connection, transaction, true);
                        }
                        else
                        {
                            Update(refItem, true, fieldNames, connection, transaction);
                        }
                    }
                }
            }

        }

        protected virtual void CheckPrimaryKeyIndex(string entityName)
        {
            if ((Entities[entityName] as SqlEntityInfo).PrimaryKeyIndexName != null) return;
            var name = GetPrimaryKeyIndexName(entityName);
            (Entities[entityName] as SqlEntityInfo).PrimaryKeyIndexName = name;
        }

        protected virtual Dictionary<string, int> GetOrdinals(string entityName, IDataReader reader)
        {
            var dic = new Dictionary<string, int>();
            Entities[entityName].Fields.OrdinalsAreValid = false;
            foreach (var field in Entities[entityName].Fields)
            {
                try
                {
                    int ordinal = reader.GetOrdinal(field.FieldName);
                    dic.Add(field.FieldName,ordinal);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new FieldNotFoundException(String.Format("Field '{0}.{1}' not found in resultset ({2})", entityName, field.FieldName, ex.Message));
                }
            }
            Entities[entityName].Fields.OrdinalsAreValid = true;
            return dic;
        }

        /// <summary>
        /// Populates the ReferenceField members of the provided entity instance
        /// </summary>
        /// <param name="instance"></param>
        public override void FillReferences(object instance)
        {
            FillReferences(instance, null, null, false, false, null);
        }
        
        public override void FillReferences(object instance, bool filterReferences)
        {
            FillReferences(instance, null, null, false,filterReferences, null);
        }

        protected void FlushReferenceTableCache()
        {
            m_referenceCache.Clear();
        }

        protected void FillReferences(object instance, object keyValue, ReferenceAttribute[] fieldsToFill, bool cacheReferenceTable, bool filterReferences, IDbConnection connection)
        {
            if (instance == null) return;

            Type type = instance.GetType();
            string entityName = m_entities.GetNameForType(type);

            if (entityName == null)
            {
                throw new EntityNotFoundException(type);
            }

            if (Entities[entityName].References.Count == 0) return;

            Dictionary<ReferenceAttribute, object[]> referenceItems = new Dictionary<ReferenceAttribute, object[]>();

            // query the key if not provided
            if (keyValue == null)
            {
                keyValue = m_entities[entityName].Fields.KeyField.PropertyInfo.GetValue(instance, null);
            }

            // populate reference fields
            foreach (var reference in Entities[entityName].References)
            {
                if (fieldsToFill != null)
                {
                    if (!fieldsToFill.Contains(reference))
                    {
                        continue;
                    }
                }

                // get the lookup values - until we support filtered selects, this may be very expensive memory-wise
                if (!referenceItems.ContainsKey(reference))
                {
                    object[] refData;
                    if (cacheReferenceTable)
                    {
                        // TODO: ref cache needs to be type->reftype->ref's, not type->refs

                        if (!m_referenceCache.ContainsKey(reference.ReferenceEntityType))
                        {
                            refData = Select(reference.ReferenceEntityType.ToString(), null, null, -1, 0, false, false, connection);
                            m_referenceCache.Add(reference.ReferenceEntityType, refData);
                        }
                        else
                        {
                            refData = m_referenceCache[reference.ReferenceEntityType];
                        }
                    }
                    else
                    {
                        if (!filterReferences || reference.ConditionField == null || reference.ConditionField.Length <= 0)
                        {
                            refData = Select(reference.ReferenceEntityType.ToString(), reference.ReferenceField, keyValue, -1, 0, true, filterReferences, connection);
                        }
                        else
                        {
                            var filters = new FilterCondition[2] {
                                  new FilterCondition(reference.ReferenceField, keyValue, FilterCondition.FilterOperator.Equals)
                                 ,new FilterCondition(reference.ConditionField, reference.ConditionValue, FilterCondition.FilterOperator.NotEquals)};
                            refData = Select(reference.ReferenceEntityType.ToString(), filters, -1, 0, true, filterReferences, connection);
                        }
                    }

                    referenceItems.Add(reference, refData);
                }

                // get the lookup field
                var childEntityName = m_entities.GetNameForType(reference.ReferenceEntityType);

                System.Collections.ArrayList childrenArray = null;
                System.Collections.IList childrenList = null;
                if (!reference.IsArray & reference.IsList)
                {
                    var genType = typeof(List<>).MakeGenericType(reference.ReferenceEntityType);
                    childrenList = (System.Collections.IList)Activator.CreateInstance(genType);
                }
                else
                {
                    childrenArray = new System.Collections.ArrayList();
                }

                // now look for those that match our pk
                foreach (var child in referenceItems[reference])
                {
                    var childKey = m_entities[childEntityName].Fields[reference.ReferenceField].PropertyInfo.GetValue(child, null);

                    // this seems "backward" because childKey may turn out null, 
                    // so doing it backwards (keyValue.Equals instead of childKey.Equals) prevents a null referenceexception
                    if (keyValue.Equals(childKey))
                    {
                        if (childrenArray != null)
                            childrenArray.Add(child);
                        else
                            childrenList.Add(child);
                    }
                }
                if (reference.IsArray)
                {
                    reference.PropertyInfo.SetValue(instance, childrenArray.ToArray(reference.ReferenceEntityType), null);
                }
                else if (reference.IsList)
                {
                    reference.PropertyInfo.SetValue(instance, childrenList, null);
                }
                else
                {
                    //var enumerator = carr.GetEnumerator();

                    //if (enumerator.MoveNext())
                    //{
                    //    reference.PropertyInfo.SetValue(instance, children[0], null);
                    //}
                    reference.PropertyInfo.SetValue(instance, childrenArray[0], null);
                }
            }
        }
        public override void Drop<T>(bool cascade)
        {
            Drop(typeof(T), cascade);
        }
        public override void Drop(System.Type entityType, bool cascade)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be dropped with one of the other Drop overloads.");
            }
            string entityName = m_entities.GetNameForType(entityType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }

            Drop(entityName, cascade);
        }

        public override void Drop(String entityName, bool cascade)
        {
            if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);

            EntityInfo entity = m_entities[entityName];

            var connection = GetConnection(true);
            try
            {
                if (cascade)
                    DropRecursive(connection, entity);
                else if (TableExists(connection, entity))
                    Drop(connection, entity);
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        protected void Drop(IDbConnection connection, EntityInfo entity)
        {
            using (var command = GetNewCommandObject())
            {
                command.Connection = connection;
                command.CommandText = string.Format("DROP TABLE {0}", entity.EntityName);
                OnSqlStatementCreated(command, null);
                command.ExecuteNonQuery();
            }
        }

        protected virtual void DropRecursive(IDbConnection connection, EntityInfo entity)
        {
            if (TableExists(connection, entity))
            {
                Drop(connection, entity);
            }
            if (entity.References.Count > 0)
            {
                foreach (var reference in entity.References)
                {
                    string entityName = this.m_entities.GetNameForType(reference.ReferenceEntityType);

                    if (entityName == null)
                    {
                        throw new EntityNotFoundException(reference.ReferenceEntityType);
                    }
                    DropRecursive(connection, this.m_entities[entityName]);
                }
            }
        }

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override int Delete<T>(bool cascade)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Delete overloads.");
            }
            return Delete(typeof(T), null, cascade);
        }

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override int Delete(System.Type entityType, bool cascade)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Delete overloads.");
            }
            return Delete(entityType, null, cascade);
        }


        public override int Delete<T>(IEnumerable<FilterCondition> filters, bool cascade)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Delete overloads.");
            }
            return Delete(typeof(T), filters, cascade);
        }

        public override int Delete(System.Type entityType, IEnumerable<FilterCondition> filters, bool cascade)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be deleted with one of the other Delete overloads.");
            }
            var entityName = m_entities.GetNameForType(entityType);

            int result = 0;
            IDbConnection connection = GetConnection(true);
            IDbTransaction transaction = null;
            try
            {
                if (cascade)
                    transaction = connection.BeginTransaction();
                result += Delete(entityName, filters, connection, transaction, cascade);
                if (transaction != null) transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                DoneWithTransaction(transaction, true);
                DoneWithConnection(connection, true);
            }
            return result;
        }

        protected virtual int Delete(String entityName, IEnumerable<FilterCondition> filters, IDbConnection connection, IDbTransaction transaction, bool cascade)
        {
            if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);
            //TODO : Handle cascade
            int result = 0;
            var entity = m_entities[entityName];
            OnBeforeDelete(entity, filters);
            if (cascade && entity.References.Count > 0)
            {
                if (entity.Fields.KeyField == null)
                {
                    throw new PrimaryKeyRequiredException(String.Format("A primary key is required on an Entity in order to perform a cascade Delete ({0})", entityName));
                }
                if (entity.Fields.KeyFields.Count > 1)
                {
                    throw new PrimaryKeyRequiredException(String.Format("Only one primary key is allowed on an Entity in order to perform a cascade Delete ({0})", entityName));
                }
                if (filters != null)
                {
                    var items = Select(entityName, filters, 0, 0, false, false, connection);
                    var reffilters = new List<FilterCondition>();
                    foreach (var item in items)
                    {
                        object primarykey = null;
                        if (entity is DynamicEntityInfo)
                        {
                            if (item is DynamicEntity) primarykey = ((DynamicEntity)item)[entity.Fields.KeyField.FieldName];
                        }
                        else
                        {
                            primarykey = entity.Fields.KeyField.PropertyInfo.GetValue(item, null);
                        }
                        if (primarykey != null)
                        {
                            foreach (var reference in entity.References)
                            {
                                reffilters.Clear();
                                reffilters.Add(new FilterCondition(reference.ReferenceField,
                                                                    primarykey,
                                                                    FilterCondition.FilterOperator.Equals));
                                if (!reference.CascadeDelete) continue;
                                var refname = m_entities.GetNameForType(reference.ReferenceEntityType);
                                result += Delete(refname, reffilters, connection, transaction, cascade);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var reference in entity.References)
                    {
                        if (!reference.CascadeDelete) continue;
                        var refname = m_entities.GetNameForType(reference.ReferenceEntityType);
                        result += Delete(refname, null, connection, transaction, cascade);
                    }
                }
            }
            result += DeleteFiltered(entityName, filters, connection, transaction);
            return result;
        }

        /// <summary>
        /// Deletes entities of a given type where the specified field name matches a specified value
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="indexName"></param>
        /// <param name="matchValue"></param>
        protected int DeleteFiltered(String entityName, IEnumerable<FilterCondition> filters, IDbConnection connection, IDbTransaction transaction)
        {
            if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);
            using (var command = GetNewCommandObject())
            {
                command.Connection = connection;
                if (transaction != null)
                    command.Transaction = transaction;

                var sb = new StringBuilder();
                sb.AppendFormat("DELETE FROM {0} ", entityName);
                if (filters != null)
                {
                    int iCount = 0;
                    foreach (var filter in filters)
                    {
                        if (iCount == 0) sb.Append(" WHERE ");
                        if (iCount > 1) sb.Append(filter.WhereOperator.ToString());
                        string paramName = String.Format("@{0}", iCount);
                        string paramSQL = BuildParameterSQL(paramName, filter);
                        sb.Append(paramSQL);
                        if (paramSQL.Contains(paramName))
                        {
                            var param = CreateParameterObject(paramName, filter.Value);
                            command.Parameters.Add(param);
                        }
                        iCount++;
                    }
                }
                command.CommandText = sb.ToString();
                OnSqlStatementCreated(command, null);
                var start = DateTime.Now;
                var result = command.ExecuteNonQuery();
                OnAfterDelete(m_entities[entityName], filters, DateTime.Now.Subtract(start), command.CommandText);
                return result;
            }
        }

        /// <summary>
        /// Deletes the specified entity instance from the DataStore
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>
        /// The instance provided must have a valid primary key value
        /// </remarks>
        public override int Delete(object item, bool cascade)
        {
            int result = 0;
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

            if (entity.Fields.KeyField == null && cascade & entity.References.Count > 0)
            {
                throw new PrimaryKeyRequiredException(String.Format("A primary key is required on an Entity in order to perform a cascade Delete ({0})", entityName));
            }
            if (entity.Fields.KeyFields.Count > 1 && cascade & entity.References.Count > 0)
            {
                throw new PrimaryKeyRequiredException(String.Format("Only one primary key is allowed on an Entity in order to perform a cascade Delete ({0})", entityName));
            }

            IDbConnection connection = GetConnection(true);
            IDbTransaction transaction = null;
            try
            {
                if (cascade & entity.Fields.KeyFields.Count == 1)
                {
                    transaction = connection.BeginTransaction();
                    object primaryKey = null;
                    if (entity is DynamicEntityInfo)
                    {
                        if (item is DynamicEntity) primaryKey = ((DynamicEntity)item)[entity.Fields.KeyField.FieldName];
                    }
                    else
                    {
                        primaryKey = entity.Fields.KeyField.PropertyInfo.GetValue(item, null);
                    }
                    if (primaryKey != null)
                    {
                        foreach (var reference in entity.References)
                        {
                            if (!reference.CascadeDelete) continue;
                            var reffilters = new List<FilterCondition>();
                            reffilters.Add(new FilterCondition(
                                reference.ReferenceField,
                                primaryKey,
                                FilterCondition.FilterOperator.Equals));
                            var refname = m_entities.GetNameForType(reference.ReferenceEntityType);
                            result += Delete(refname, reffilters, connection, transaction, cascade);
                        }
                    }
                }

                var filters = new List<FilterCondition>();
                if (entity.Fields.KeyFields == null || entity.Fields.KeyFields.Count <= 0)
                {
                    foreach (var field in entity.Fields)
                    {
                        FilterCondition filter = null;
                        if (entity is DynamicEntityInfo)
                        {
                            if (item is DynamicEntity) filter = new FilterCondition(field.FieldName, ((DynamicEntity)item)[field.FieldName], FilterCondition.FilterOperator.Equals);
                        }
                        else
                        {
                            filter = new FilterCondition(field.FieldName, field.PropertyInfo.GetValue(item, null), FilterCondition.FilterOperator.Equals);
                        }
                        if (filter != null) filters.Add(filter);
                    }
                }
                else
                {
                    foreach (var field in entity.Fields.KeyFields)
                    {
                        FilterCondition filter = null;
                        if (entity is DynamicEntityInfo)
                        {
                            if (item is DynamicEntity) filter = new FilterCondition(field.FieldName, ((DynamicEntity)item)[field.FieldName], FilterCondition.FilterOperator.Equals);
                        }
                        else
                        {
                            filter = new FilterCondition(field.FieldName, field.PropertyInfo.GetValue(item, null), FilterCondition.FilterOperator.Equals);
                        }
                        if (filter == null) throw new ArgumentNullException(field.FieldName, "A primary key cannot be null");
                        if (filter.Value == null || filter.Value == DBNull.Value) throw new ArgumentNullException(field.FieldName, "A primary key cannot be null");
                        filters.Add(filter);
                    }
                }
                if (filters != null && filters.Count > 0)
                {
                    result += DeleteFiltered(entityName, filters, connection, transaction);
                }
                if (transaction != null) transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                DoneWithTransaction(transaction, true);
                DoneWithConnection(connection, true);
            }
            return result;
        }

        /// <summary>
        /// Deletes an entity instance with the specified primary key from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        public override int Delete<T>(object primaryKey, bool cascade)
        {
            return Delete(typeof(T), primaryKey, cascade);
        }

        public override int Delete(String entityName, bool cascade)
        {
            return Delete(entityName, null, cascade);
        }

        public override int Delete(String entityName, IEnumerable<FilterCondition> filters, bool cascade)
        {
            if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);

            int result = 0;
            IDbConnection connection = GetConnection(true);
            IDbTransaction transaction = null;
            try
            {
                if (cascade)
                    transaction = connection.BeginTransaction();
                result += Delete(entityName, filters, connection, transaction, cascade);
                if (transaction != null) transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                DoneWithTransaction(transaction, true);
                DoneWithConnection(connection, true);
            }
            return result;
        }

        public override int Delete(Type entityType, object primaryKey, bool cascade)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be deleted with one of the other Delete overloads.");
            }

            int result = 0;
            string entityName = m_entities.GetNameForType(entityType);

            if (primaryKey == null) return 0;

            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }

            if (Entities[entityName].Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException(String.Format("A primary key is required on an Entity in order to perform a PK Delete ({0})", entityName));
            }

            //TODO: Add transactional delete
            IDbConnection connection = GetConnection(true);
            IDbTransaction transaction = null;
            try
            {
                if (cascade)
                {
                    transaction = connection.BeginTransaction();
                    // handle cascade deletes
                    foreach (var reference in Entities[entityName].References)
                    {
                        if (!reference.CascadeDelete) continue;
                        var reffilters = new List<FilterCondition>();
                        reffilters.Add(new FilterCondition(
                            reference.ReferenceField,
                            primaryKey,
                            FilterCondition.FilterOperator.Equals));
                        var refname = m_entities.GetNameForType(reference.ReferenceEntityType);
                        result += Delete(refname, reffilters, connection, transaction, cascade);
                    }
                }

                var keyFieldName = Entities[entityName].Fields.KeyField.FieldName;
                var filters = new List<FilterCondition>();
                filters.Add(new FilterCondition(
                                keyFieldName,
                                primaryKey,
                                FilterCondition.FilterOperator.Equals));
                result += DeleteFiltered(entityName, filters, connection, transaction);
                if (transaction != null) transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                DoneWithTransaction(transaction, true);
                DoneWithConnection(connection, true);
            }
            return result;
        }


        public override int Count<T>()
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            var entityName = m_entities.GetNameForType(typeof(T));
            if (entityName == null) throw new EntityNotFoundException(typeof(T));
            return Count(entityName, null);
        }

        public override int Count<T>(IEnumerable<FilterCondition> filters)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            var entityName = m_entities.GetNameForType(typeof(T));
            if (entityName == null) throw new EntityNotFoundException(typeof(T));
            return Count(entityName, filters);
        }

        public override int Count(Type entityType)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            string entityName = m_entities.GetNameForType(entityType);
            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }

            return Count(entityName, null);
        }

        public override int Count(Type entityType, IEnumerable<FilterCondition> filters)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            string entityName = m_entities.GetNameForType(entityType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }

            return Count(entityName, null);
        }

        public override int Count(String entityName)
        {
            return Count(entityName, null);
        }

        public override int Count(String entityName, IEnumerable<FilterCondition> filters)
        {
            var connection = GetConnection(true);
            try
            {
                string field = "*";
                if (Entities[entityName].Fields.KeyFields.Count == 1)
                    field = Entities[entityName].Fields.KeyFields[0].FieldName;
                var @params = new List<IDataParameter>(); 
                using (var command = GetNewCommandObject())
                {
                    var sb = new StringBuilder();
                    command.Connection = connection;
                    sb.AppendFormat("SELECT COUNT({0}) FROM {1} ", field, entityName);
                    if (filters != null)
                    {
                        for (int i = 0; i < filters.Count(); i++)
                        {
                            sb.Append(i == 0 ? " WHERE " : String.Format(" {0} ", FieldAttribute.GetName(typeof(FilterCondition.LogicalOperator), (int)filters.ElementAt(i).WhereOperator)));

                            var filter = filters.ElementAt(i);

                            string paramName = string.Format("@p{0}", i);

                            string paramsql = BuildParameterSQL(paramName, filter);
                            sb.Append(paramsql);
                            // In the case of a IS NULL or so, we don't use the Param, no need to add it.
                            if (paramsql.Contains(paramName))
                            {
                                var param = CreateParameterObject(paramName, filter.Value ?? DBNull.Value);
                                @params.Add(param);
                            }
                        }
                    }
                    command.CommandText = sb.ToString();
                    foreach (var param in @params)
                    {
                        command.Parameters.Add(param); 
                    }
                    OnSqlStatementCreated(command, null);
                    var count = command.ExecuteScalar();
                    return Convert.ToInt32(count);
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        /// <summary>
        /// Fetches a sorted list of entities, up to the requested number of entity instances, of the specified type from the DataStore, starting with the specified instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchFieldName"></param>
        /// <param name="fetchCount"></param>
        /// <param name="firstRowOffset"></param>
        /// <returns></returns>
        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Fetch<T>(fetchCount, firstRowOffset, sortField, FieldOrder.Ascending, null, false);
        }

        /// <summary>
        /// Fetches up to the requested number of entity instances of the specified type from the DataStore, starting with the first instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fetchCount"></param>
        /// <returns></returns>
        public override T[] Fetch<T>(int fetchCount)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            var type = typeof(T);
            var items = Select(type.ToString(), null, null, fetchCount, 0, false, false, null);
            return items.Cast<T>().ToArray();
        }

        /// <summary>
        /// Fetches up to the requested number of entity instances of the specified type from the DataStore, starting with the specified instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fetchCount"></param>
        /// <param name="firstRowOffset"></param>
        /// <returns></returns>
        public override T[] Fetch<T>(int fetchCount, int firstRowOffset)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            var type = typeof(T);
            var items = Select(type.ToString(), null, null, fetchCount, firstRowOffset, false, false, null);
            return items.Cast<T>().ToArray();
        }

        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Fetch<T>(fetchCount, firstRowOffset, sortField, sortOrder, filter, fillReferences, false);
        }
        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences)
        {
            if (typeof(T).Equals(typeof(DynamicEntity)) || typeof(T).Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return (T[])Fetch(typeof(T), fetchCount, firstRowOffset, sortField, sortOrder, filter, fillReferences, filterReferences).Cast<T>();
        }

        public override object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, bool fillReferences)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(entityType.ToString(), null, null, fetchCount, firstRowOffset, fillReferences, false, null);
        }
        public override object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences)
        {
            if (entityType.Equals(typeof(DynamicEntity)) || entityType.Equals(typeof(DynamicEntityInfo)))
            {
                throw new ArgumentException("DynamicEntities must be counted with one of the other Count overloads.");
            }
            return Select(entityType.ToString(), null, null, fetchCount, firstRowOffset, fillReferences, filterReferences, null);
        }
        public abstract override object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences);
    }
}
