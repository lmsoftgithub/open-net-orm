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
        public abstract override void DeleteStore();
        public abstract override void EnsureCompatibility();

        public abstract override bool StoreExists { get; }

        protected abstract string GetPrimaryKeyIndexName(string entityName);

        public abstract override void InsertOrUpdate(object item, bool insertReferences);
        public abstract override void InsertOrUpdate(object item, bool insertReferences, bool transactional);
        protected abstract void InsertOrUpdate(object item, bool insertReferences, IDbTransaction transaction);

        public abstract override void Insert(object item, bool insertReferences);
        public abstract override void Insert(object item, bool insertReferences, bool transactional);
        protected abstract void Insert(object item, bool insertReferences, IDbTransaction transaction);

        protected abstract object[] Select(Type objectType, IEnumerable<FilterCondition> filters, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences);

        public abstract override void Update(object item, bool cascadeUpdates, string fieldName);
        public abstract override void Update(object item, bool cascadeUpdates, string fieldName, bool transactional);
        protected abstract void Update(object item, bool cascadeUpdates, string fieldName, IDbTransaction transaction);

        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences)
        {
            return Fetch<T>(fetchCount, firstRowOffset, sortField, sortOrder, filter, fillReferences, false);
        }
        public abstract override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences);

        public abstract override int Count<T>(IEnumerable<FilterCondition> filters);

        protected abstract IDbCommand GetNewCommandObject();
        protected abstract IDbConnection GetNewConnectionObject();
        protected abstract IDataParameter CreateParameterObject(string parameterName, object parameterValue);

        public SQLStoreBase()
        {
            DefaultStringFieldSize = 200;
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

        public bool TableExists(EntityInfo entityInfo)
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

        public bool FieldExists(EntityInfo entityInfo, FieldAttribute fieldInfo)
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
            get { return m_sqlReserved; }
        }

        private static string[] m_sqlReserved = new string[]
        {
            "IDENTITY" ,"ENCRYPTION" ,"ORDER" ,"ADD" ,"END" ,"OUTER" ,"ALL" ,"ERRLVL" ,"OVER" ,"ALTER" ,"ESCAPE" ,"PERCENT" ,"AND" ,"EXCEPT" ,"PLAN" ,"ANY" ,"EXEC" ,"PRECISION" ,"AS" ,"EXECUTE" ,"PRIMARY" ,"ASC",
            "EXISTS" ,"PRINT" ,"AUTHORIZATION" ,"EXIT" ,"PROC" ,"AVG" ,"EXPRESSION" ,"PROCEDURE" ,"BACKUP" ,"FETCH" ,"PUBLIC" ,"BEGIN" ,"FILE" ,"RAISERROR" ,"BETWEEN" ,"FILLFACTOR" ,"READ" ,"BREAK" ,"FOR" ,"READTEXT",
            "BROWSE" ,"FOREIGN" ,"RECONFIGURE" ,"BULK" ,"FREETEXT" ,"REFERENCES" ,"BY" ,"FREETEXTTABLE" ,"REPLICATION" ,"CASCADE" ,"FROM" ,"RESTORE" ,"CASE" ,"FULL" ,"RESTRICT" ,"CHECK" ,"FUNCTION" ,"RETURN" ,"CHECKPOINT",
            "GOTO" ,"REVOKE" ,"CLOSE" ,"GRANT" ,"RIGHT" ,"CLUSTERED" ,"GROUP" ,"ROLLBACK" ,"COALESCE" ,"HAVING" ,"ROWCOUNT" ,"COLLATE" ,"HOLDLOCK" ,"ROWGUIDCOL" ,"COLUMN" ,"IDENTITY" ,"RULE",
            "COMMIT" ,"IDENTITY_INSERT" ,"SAVE" ,"COMPUTE" ,"IDENTITYCOL" ,"SCHEMA" ,"CONSTRAINT" ,"IF" ,"SELECT" ,"CONTAINS" ,"IN" ,"SESSION_USER" ,"CONTAINSTABLE" ,"INDEX" ,"SET" ,"CONTINUE" ,"INNER" ,"SETUSER",
            "CONVERT" ,"INSERT" ,"SHUTDOWN" ,"COUNT" ,"INTERSECT" ,"SOME" ,"CREATE" ,"INTO" ,"STATISTICS" ,"CROSS" ,"IS" ,"SUM" ,"CURRENT" ,"JOIN" ,"SYSTEM_USER" ,"CURRENT_DATE" ,"KEY" ,"TABLE" ,"CURRENT_TIME" ,"KILL",
            "TEXTSIZE" ,"CURRENT_TIMESTAMP" ,"LEFT" ,"THEN" ,"CURRENT_USER" ,"LIKE" ,"TO" ,"CURSOR" ,"LINENO" ,"TOP" ,"DATABASE" ,"LOAD" ,"TRAN" ,"DATABASEPASSWORD" ,"MAX" ,"TRANSACTION" ,"DATEADD" ,"MIN" ,"TRIGGER",
            "DATEDIFF" ,"NATIONAL" ,"TRUNCATE" ,"DATENAME" ,"NOCHECK" ,"TSEQUAL" ,"DATEPART" ,"NONCLUSTERED" ,"UNION" ,"DBCC" ,"NOT" ,"UNIQUE" ,"DEALLOCATE", "NULL", "UPDATE", "DECLARE", "NULLIF", "UPDATETEXT",
            "DEFAULT", "OF", "USE", "DELETE", "OFF", "USER", "DENY", "OFFSETS", "VALUES", "DESC", "ON", "VARYING", "DISK", "OPEN", "VIEW", "DISTINCT", "OPENDATASOURCE", "WAITFOR", "DISTRIBUTED", "OPENQUERY", "WHEN", 
            "DOUBLE", "OPENROWSET", "WHERE", "DROP", "OPENXML", "WHILE", "DUMP", "OPTION", "WITH", "ELSE", "OR", "WRITETEXT" 
        };

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

        /// <summary>
        /// Creates the table if it does not already exists and create the fields if they don't already exist.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entity"></param>
        protected virtual void CreateTable(IDbConnection connection, EntityInfo entity)
        {
            Boolean bTableExists = false;
            Boolean bMultiplePrimaryKeys = false;
            if (ReservedWords.Contains(entity.EntityName, StringComparer.InvariantCultureIgnoreCase))
            {
                throw new ReservedWordException(entity.EntityName);
            }

            bTableExists = TableExists(connection, entity);

            bMultiplePrimaryKeys = entity.Fields.KeyFields.Count > 1;

            // Handles the case of tables with multiple primary keys
            if (!bTableExists)
            {
                StringBuilder sql = new StringBuilder();
                StringBuilder keys = new StringBuilder();
                sql.AppendFormat("CREATE TABLE {0} ( ", entity.EntityName);
                int count = entity.Fields.KeyFields.Count;
                if (count > 0)
                {
                    foreach (var field in entity.Fields.KeyFields)
                    {
                        sql.AppendFormat(" {0} {1} {2} ",
                            field.FieldName,
                            GetFieldDataTypeString(entity.EntityName, field),
                            GetFieldCreationAttributes(entity.EntityAttribute, field, true));
                        keys.Append(field.FieldName);
                        if (--count > 0)
                        {
                            sql.Append(", ");
                            keys.Append(", ");
                        }
                    }
                    sql.AppendFormat(", PRIMARY KEY({0}) )", keys.ToString());
                }
                else
                {
                    var field = entity.Fields.First();
                    sql.AppendFormat(" {0} {1} {2} )",
                            field.FieldName,
                            GetFieldDataTypeString(entity.EntityName, field),
                            GetFieldCreationAttributes(entity.EntityAttribute, field, true));
                }
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = sql.ToString();
                    command.Connection = connection;
                    int i = command.ExecuteNonQuery();
                }
                bTableExists = true;
            }

            foreach (var field in entity.Fields)
            {
                StringBuilder sql = new StringBuilder();
                if (!FieldExists(connection, entity, field))
                {
                    if (ReservedWords.Contains(field.FieldName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        throw new ReservedWordException(field.FieldName);
                    }
                    if (bTableExists)
                    {
                        sql.AppendFormat("ALTER TABLE {0} ADD ", entity.EntityName);
                    }
                    else
                    {
                        sql.AppendFormat("CREATE TABLE {0} ( ", entity.EntityName);
                    }
                    // ALTER TABLE {TABLENAME} 
                    // ADD {COLUMNNAME} {TYPE} {NULL|NOT NULL} 
                    // CONSTRAINT {CONSTRAINT_NAME} DEFAULT {DEFAULT_VALUE}
                    sql.AppendFormat(" {0} {1} {2} ",
                        field.FieldName,
                        GetFieldDataTypeString(entity.EntityName, field),
                        GetFieldCreationAttributes(entity.EntityAttribute, field, bMultiplePrimaryKeys));

                    if (bTableExists)
                    {
                    }
                    else
                    {
                        sql.AppendFormat(") ");
                    }

                    using (var command = GetNewCommandObject())
                    {
                        command.CommandText = sql.ToString();
                        command.Connection = connection;
                        int i = command.ExecuteNonQuery();
                    }
                    bTableExists = true;

                    // create indexes
                    if (field.SearchOrder != FieldSearchOrder.NotSearchable)
                    {
                        VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
                    }
                }
                else
                {
                    // create indexes
                    if (field.SearchOrder != FieldSearchOrder.NotSearchable)
                    {
                        VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
                    }
                }
            }
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

        protected virtual string VerifyIndex(string entityName, string fieldName, FieldSearchOrder searchOrder)
        {
            return VerifyIndex(entityName, fieldName, searchOrder, null);
        }

        protected virtual string VerifyIndex(string entityName, string fieldName, FieldSearchOrder searchOrder, IDbConnection connection)
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

                    var sql = string.Format("SELECT COUNT(*) FROM information_schema.indexes WHERE INDEX_NAME = '{0}'", indexName);
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

            if (!field.AllowsNulls)
            {
                sb.Append("NOT NULL ");
            }

            if (field.RequireUniqueValue)
            {
                sb.Append("UNIQUE ");
            }

            return sb.ToString();
        }

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

        /// <summary>
        /// Determines if the specified object already exists in the Store (by primary key value)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool Contains(object item)
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
            var existing = Select(itemType, filters, 0, 0, false, false).FirstOrDefault();

            return existing != null;
        }

        /// <summary>
        /// Retrieves a single entity instance from the DataStore identified by the specified primary key value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public override T Select<T>(object primaryKey)
        {
            return Select<T>(primaryKey, false, false);
        }

        public override T Select<T>(object primaryKey, bool fillReferences)
        {
            return Select<T>(primaryKey, fillReferences, false);
        }

        public override T Select<T>(object primaryKey, bool fillReferences, bool filterReferences)
        {
            return (T)Select(typeof(T), null, primaryKey, -1, -1, fillReferences, false).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T[] Select<T>()
        {
            var type = typeof(T);
            var items = Select(type, null, null, -1, 0);
            return items.Cast<T>().ToArray();
        }

        /// <summary>
        /// Retrieves all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T[] Select<T>(bool fillReferences)
        {
            return Select<T>(fillReferences, false);
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
            var type = typeof(T);
            var items = Select(type, null, null, -1, 0, fillReferences, filterReferences);
            return items.Cast<T>().ToArray();
        }

        /// <summary>
        /// Retrieves all entity instances of the specified type from the DataStore
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public override object[] Select(Type entityType)
        {
            return Select(entityType, true);
        }

        public override object[] Select(Type entityType, bool fillReferences)
        {
            return Select(entityType, fillReferences, false);
        }

        public override object[] Select(Type entityType, bool fillReferences, bool filterReferences)
        {
            var items = Select(entityType, null, null, -1, 0, fillReferences, filterReferences);
            return items.ToArray();
        }

        public override T[] Select<T>(string searchFieldName, object matchValue)
        {
            return Select<T>(searchFieldName, matchValue, true);
        }

        public override T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences)
        {
            return Select<T>(searchFieldName, matchValue, fillReferences, false);
        }

        public override T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences, bool filterReferences)
        {
            var type = typeof(T);
            var items = Select(type, searchFieldName, matchValue, -1, 0, fillReferences, filterReferences);
            return items.Cast<T>().ToArray();
        }

        public override T[] Select<T>(IEnumerable<FilterCondition> filters)
        {
            return Select<T>(filters, true);
        }

        public override T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences)
        {
            return Select<T>(filters, fillReferences, false);
        }

        public override T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences)
        {
            var objectType = typeof(T);
            return Select(objectType, filters, -1, 0, fillReferences, filterReferences).Cast<T>().ToArray();
        }

        private object[] Select(Type objectType, string searchFieldName, object matchValue, int fetchCount, int firstRowOffset)
        {
            return Select(objectType, searchFieldName, matchValue, fetchCount, firstRowOffset, true, false);
        }

        protected virtual object[] Select(Type objectType, string searchFieldName, object matchValue, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences)
        {
            string entityName = m_entities.GetNameForType(objectType);
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
                objectType,
                (filter == null) ? null :
                    new FilterCondition[]
                    {
                        filter
                    },
                fetchCount,
                firstRowOffset,
                fillReferences
                , filterReferences);
        }

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

        protected virtual TCommand GetSelectCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters, out bool tableDirect)
            where TCommand : DbCommand, new()
            where TParameter : IDataParameter, new()
        {
            tableDirect = false;
            return BuildFilterCommand<TCommand, TParameter>(entityName, filters);
        }

        protected TCommand BuildFilterCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters)
            where TCommand : DbCommand, new()
            where TParameter : IDataParameter, new()
        {
            return BuildFilterCommand<TCommand, TParameter>(entityName, filters, false);
        }

        protected TCommand BuildFilterCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters, bool isCount)
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
                                    where el.IsPrimaryKey
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
                foreach (FieldAttribute fa in Entities[entityName].Fields)
                {
                    sb.AppendFormat(" {0}.{1},", entityName, fa.FieldName);
                }
                sb.Remove(sb.Length - 1, 1);
            }
            sb.AppendFormat(" FROM {0} ", entityName);

            if (filters != null)
            {
                for (int i = 0; i < filters.Count(); i++)
                {
                    sb.Append(i == 0 ? " WHERE " : String.Format(" {0} ",FieldAttribute.GetName(typeof(FilterCondition.LogicalOperator), (int)filters.ElementAt(i).WhereOperator)));
                    
                    var filter = filters.ElementAt(i);
                    sb.Append("[" + filter.FieldName + "]");

                    string paramName = string.Format("@p{0}", i);

                    switch (filters.ElementAt(i).Operator)
                    {
                        case FilterCondition.FilterOperator.Equals:
                            if ((filter.Value == null) || (filter.Value == DBNull.Value))
                            {
                                sb.Append(" IS NULL ");
                                continue;
                            }
                            sb.AppendFormat(" = {0}",paramName);
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
                                sb.AppendFormat(" IN ({0})", String.Join(",",strarr));
                            }
                            else
                            {
                                sb.AppendFormat(" IN ({0})", filter.Value.ToString());
                            }
                            break;
                        case FilterCondition.FilterOperator.NotEquals:
                            if ((filter.Value == null) || (filter.Value == DBNull.Value))
                            {
                                sb.Append(" IS NOT NULL ");
                                continue;
                            }
                            sb.AppendFormat(" <> {0}", paramName);
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    var param = new TParameter()
                    {
                        ParameterName = paramName,
                        Value = filter.Value ?? DBNull.Value
                    };

                    @params.Add(param);
                }
            }
            if (Entities[entityName].SortingFields.Count > 0)
            {
                sb.Append(" ORDER BY ");
                foreach (FieldAttribute fa in Entities[entityName].SortingFields.OrderBy(d => d.SortSequence))
                {
                    if (fa.SortOrder == FieldSearchOrder.Descending)
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

        protected void CheckPrimaryKeyIndex(string entityName)
        {
            if ((Entities[entityName] as SqlEntityInfo).PrimaryKeyIndexName != null) return;
            var name = GetPrimaryKeyIndexName(entityName);
            (Entities[entityName] as SqlEntityInfo).PrimaryKeyIndexName = name;
        }

        protected virtual void CheckOrdinals(string entityName)
        {
            if (Entities[entityName].Fields.OrdinalsAreValid) return;

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("SELECT * FROM {0}", entityName);

                    using (var reader = command.ExecuteReader())
                    {
                        foreach (var field in Entities[entityName].Fields)
                        {
                            field.Ordinal = reader.GetOrdinal(field.FieldName);
                        }

                        Entities[entityName].Fields.OrdinalsAreValid = true;
                    }

                    command.Dispose();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        /// <summary>
        /// Populates the ReferenceField members of the provided entity instance
        /// </summary>
        /// <param name="instance"></param>
        public override void FillReferences(object instance)
        {
            FillReferences(instance, null, null, false, false);
        }

        
        public override void FillReferences(object instance, bool filterReferences)
        {
            FillReferences(instance, null, null, false,filterReferences);
        }

        protected void FlushReferenceTableCache()
        {
            m_referenceCache.Clear();
        }

        protected void FillReferences(object instance, object keyValue, ReferenceAttribute[] fieldsToFill, bool cacheReferenceTable, bool filterReferences)
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
                            refData = Select(reference.ReferenceEntityType, null, null, -1, 0);
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
                            refData = Select(reference.ReferenceEntityType, reference.ReferenceField, keyValue, -1, 0, true, filterReferences);
                        }
                        else
                        {
                            var filters = new FilterCondition[2] {
                                  new FilterCondition(reference.ReferenceField, keyValue, FilterCondition.FilterOperator.Equals)
                                 ,new FilterCondition(reference.ConditionField, reference.ConditionValue, FilterCondition.FilterOperator.NotEquals)};
                            refData = Select(reference.ReferenceEntityType, filters, -1, 0, true, filterReferences);
                        }
                    }

                    referenceItems.Add(reference, refData);
                }

                // get the lookup field
                var childEntityName = m_entities.GetNameForType(reference.ReferenceEntityType);

                System.Collections.ArrayList children = new System.Collections.ArrayList();

                // now look for those that match our pk
                foreach (var child in referenceItems[reference])
                {
                    var childKey = m_entities[childEntityName].Fields[reference.ReferenceField].PropertyInfo.GetValue(child, null);

                    // this seems "backward" because childKey may turn out null, 
                    // so doing it backwards (keyValue.Equals instead of childKey.Equals) prevents a null referenceexception
                    if (keyValue.Equals(childKey))
                    {
                        children.Add(child);
                    }
                }
                var carr = children.ToArray(reference.ReferenceEntityType);
                if (reference.PropertyInfo.PropertyType.IsArray)
                {
                    reference.PropertyInfo.SetValue(instance, carr, null);
                }
                else
                {
                    var enumerator = carr.GetEnumerator();

                    if (enumerator.MoveNext())
                    {
                        reference.PropertyInfo.SetValue(instance, children[0], null);
                    }
                }
            }
        }

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

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Drop<T>()
        {
            Drop(typeof(T));
        }

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Drop(System.Type entityType)
        {
            string entityName = m_entities.GetNameForType(entityType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }

            // TODO: handle cascade deletes?

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("DROP TABLE {0}", entityName);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Delete<T>()
        {
            Delete(typeof(T));
        }

        /// <summary>
        /// Deletes all entity instances of the specified type from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public override void Delete(System.Type entityType)
        {
            string entityName = m_entities.GetNameForType(entityType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(entityType);
            }

            // TODO: handle cascade deletes?

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("DELETE FROM {0}", entityName);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }


        public override void Delete<T>(string fieldName, object matchValue)
        {
            Delete(typeof(T), fieldName, matchValue);
        }

        /// <summary>
        /// Deletes entities of a given type where the specified field name matches a specified value
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="indexName"></param>
        /// <param name="matchValue"></param>
        protected void Delete(Type entityType, string fieldName, object matchValue)
        {
            string entityName = m_entities.GetNameForType(entityType);

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("DELETE FROM {0} WHERE {1} = {2}", entityName, fieldName, "@" + fieldName);
                    var param = CreateParameterObject("@" + fieldName, matchValue);
                    command.Parameters.Add(param);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        /// <summary>
        /// Deletes entities of a given type where the specified field names match the specified values
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="fieldNames"></param>
        /// <param name="matchValues"></param>
        protected void Delete(Type entityType, List<String> fieldNames, List<object> matchValues)
        {
            string entityName = m_entities.GetNameForType(entityType);

            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    String sql = string.Format("DELETE FROM {0} WHERE ", entityName);
                    for (int i = 0; i < fieldNames.Count; i++)
                    {
                        if (i > 0) sql += " AND ";
                        sql += fieldNames[i] + " = @" + fieldNames[i];
                        var param = CreateParameterObject("@" + fieldNames[i], matchValues[i]);
                        command.Parameters.Add(param);
                    }
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        /// <summary>
        /// Deletes the specified entity instance from the DataStore
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>
        /// The instance provided must have a valid primary key value
        /// </remarks>
        public override void Delete(object item)
        {
            var type = item.GetType();
            string entityName = m_entities.GetNameForType(type);

            if (entityName == null)
            {
                throw new EntityNotFoundException(type);
            }

            if (Entities[entityName].Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform a Delete");
            }

            if (FieldAttributeCollection.AllowMultiplePrimaryKeyFields && Entities[entityName].Fields.KeyFields.Count > 1)
            {
                List<String> keys = new List<String>();
                List<object> values = new List<object>();
                foreach (FieldAttribute field in Entities[entityName].Fields.KeyFields)
                {
                    keys.Add(field.FieldName);
                    values.Add(field.PropertyInfo.GetValue(item, null));
                }
                Delete(type, keys, values);
            }
            else
            {
                var keyValue = Entities[entityName].Fields.KeyField.PropertyInfo.GetValue(item, null);
                Delete(type, keyValue);
            }
        }

        /// <summary>
        /// Deletes an entity instance with the specified primary key from the DataStore
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        public override void Delete<T>(object primaryKey)
        {
            Delete(typeof(T), primaryKey);
        }

        protected virtual void Delete(Type t, object primaryKey)
        {
            string entityName = m_entities.GetNameForType(t);

            if (entityName == null)
            {
                throw new EntityNotFoundException(t);
            }

            if (Entities[entityName].Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform a Delete");
            }

            // handle cascade deletes
            foreach (var reference in Entities[entityName].References)
            {
                if (!reference.CascadeDelete) continue;

                Delete(reference.ReferenceEntityType, reference.ReferenceField, primaryKey);
            }

            var keyFieldName = Entities[entityName].Fields.KeyField.FieldName;
            Delete(t, keyFieldName, primaryKey);
        }

        /// <summary>
        /// Returns the number of instances of the given type in the DataStore
        /// </summary>
        /// <typeparam name="T">Entity type to count</typeparam>
        /// <returns>The number of instances in the store</returns>
        public override int Count<T>()
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
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;
                    command.CommandText = string.Format("SELECT COUNT(*) FROM {0}", entityName);
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
            return Fetch<T>(fetchCount, firstRowOffset, sortField, FieldSearchOrder.Ascending, null, false);
        }

        /// <summary>
        /// Fetches up to the requested number of entity instances of the specified type from the DataStore, starting with the first instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fetchCount"></param>
        /// <returns></returns>
        public override T[] Fetch<T>(int fetchCount)
        {
            var type = typeof(T);
            var items = Select(type, null, null, fetchCount, 0, false, false);
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
            var type = typeof(T);
            var items = Select(type, null, null, fetchCount, firstRowOffset, false, false);
            return items.Cast<T>().ToArray();
        }
    }
}
