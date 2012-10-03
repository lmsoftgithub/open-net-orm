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
    public partial class FirebirdDataStore : SQLStoreBase<FbEntityInfo>
    {
        private Dictionary<Type, MethodInfo> m_serializerCache = new Dictionary<Type, MethodInfo>();
        private Dictionary<Type, MethodInfo> m_deserializerCache = new Dictionary<Type, MethodInfo>();
        private Dictionary<Type, object[]> m_referenceCache = new Dictionary<Type, object[]>();

        private string m_connectionString;
        private int m_maxSize = 128; // Max Database Size defaults to 128MB

        private string Password { get; set; }

        public string ConnectionStringBase { get; protected set; }

        public override string Name
        {
            get
            {
                //TODO: Add parsing of the Connection String
                throw new NotImplementedException();
            }
        }

        private const int CommandCacheMaxLength = 10;

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
                    m_connectionString = string.Format(ConnectionStringBase, Password);
                }
                return m_connectionString;
            }
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

        protected override string AutoIncrementFieldIdentifier
        {
            get { return String.Empty; }
        }

        protected FirebirdDataStore()
            : base()
        {
            UseCommandCache = false;
            FieldAttributeCollection.AllowMultiplePrimaryKeyFields = true;
            DefaultStringFieldSize = 200;
            DefaultNumericFieldPrecision = 16;
            DefaultVarBinaryLength = 4000;
        }

        public FirebirdDataStore(string connectionString)
            : this(connectionString, null)
        {
        }

        public FirebirdDataStore(string connectionString, string password)
            : this()
        {
            ConnectionStringBase = connectionString;
            Password = password;
        }

        //protected override TCommand BuildFilterCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters, bool isCount)
        //{
        //    var command = new TCommand();
        //    command.CommandType = CommandType.Text;
        //    var @params = new List<TParameter>();

        //    StringBuilder sb;
        //    sb = new StringBuilder("SELECT ");
        //    if (isCount)
        //    {
        //        FieldAttribute fa = (from FieldAttribute el in Entities[entityName].Fields
        //                             where el.IsPrimaryKey
        //                             select el).FirstOrDefault<FieldAttribute>();
        //        if (fa == null)
        //        {
        //            sb.Append(" COUNT(*) ");
        //        }
        //        else
        //        {
        //            sb.AppendFormat(" COUNT({0}.{1}) ", entityName, fa.FieldName);
        //        }
        //    }
        //    else
        //    {
        //        foreach (FieldAttribute fa in Entities[entityName].Fields)
        //        {
        //            sb.AppendFormat(" {0}.{1},", entityName, fa.FieldName);
        //        }
        //        sb.Remove(sb.Length - 1, 1);
        //    }
        //    sb.AppendFormat(" FROM {0} ", entityName);

        //    if (filters != null)
        //    {
        //        for (int i = 0; i < filters.Count(); i++)
        //        {
        //            sb.Append(i == 0 ? " WHERE " : String.Format(" {0} ", FieldAttribute.GetName(typeof(FilterCondition.LogicalOperator), (int)filters.ElementAt(i).WhereOperator)));

        //            var filter = filters.ElementAt(i);
        //            sb.Append("[" + filter.FieldName + "]");

        //            string paramName = string.Format("@p{0}", i);

        //            switch (filters.ElementAt(i).Operator)
        //            {
        //                case FilterCondition.FilterOperator.Equals:
        //                    if ((filter.Value == null) || (filter.Value == DBNull.Value))
        //                    {
        //                        sb.Append(" IS NULL ");
        //                        continue;
        //                    }
        //                    sb.AppendFormat(" = {0}", paramName);
        //                    break;
        //                case FilterCondition.FilterOperator.Like:
        //                    sb.AppendFormat(" LIKE {0}", paramName);
        //                    break;
        //                case FilterCondition.FilterOperator.LessThan:
        //                    sb.AppendFormat(" < {0}", paramName);
        //                    break;
        //                case FilterCondition.FilterOperator.GreaterThan:
        //                    sb.AppendFormat(" > {0}", paramName);
        //                    break;
        //                case FilterCondition.FilterOperator.LessThanOrEqual:
        //                    sb.AppendFormat(" <= {0}", paramName);
        //                    break;
        //                case FilterCondition.FilterOperator.GreaterThanOrEqual:
        //                    sb.AppendFormat(" >= {0}", paramName);
        //                    break;
        //                case FilterCondition.FilterOperator.In:
        //                    if (filter.Value.GetType().IsArray)
        //                    {
        //                        // If we received an array as a parameter, we transform it to a String array
        //                        // using the ToString() function of the object. This has the benefit that
        //                        // we can give an array of custom objects and use their overriden ToString method.
        //                        var arr = filter.Value as Array;
        //                        String[] strarr = new String[arr.Length];
        //                        for (int k = 0; k < arr.Length; k++) { strarr[k] = arr.GetValue(k).ToString(); }
        //                        sb.AppendFormat(" IN ({0})", String.Join(",", strarr));
        //                    }
        //                    else
        //                    {
        //                        sb.AppendFormat(" IN ({0})", filter.Value.ToString());
        //                    }
        //                    break;
        //                case FilterCondition.FilterOperator.NotEquals:
        //                    if ((filter.Value == null) || (filter.Value == DBNull.Value))
        //                    {
        //                        sb.Append(" IS NOT NULL ");
        //                        continue;
        //                    }
        //                    sb.AppendFormat(" <> {0}", paramName);
        //                    break;
        //                default:
        //                    throw new NotSupportedException();
        //            }

        //            var param = new TParameter()
        //            {
        //                ParameterName = paramName,
        //                Value = filter.Value ?? DBNull.Value
        //            };

        //            @params.Add(param);
        //        }
        //    }
        //    if (Entities[entityName].SortingFields.Count > 0)
        //    {
        //        sb.Append(" ORDER BY ");
        //        foreach (FieldAttribute fa in Entities[entityName].SortingFields.OrderBy(d => d.SortSequence))
        //        {
        //            if (fa.SortOrder == FieldSearchOrder.Descending)
        //                sb.AppendFormat(" {0} DESC,", fa.FieldName);
        //            else
        //                sb.AppendFormat(" {0} ASC,", fa.FieldName);
        //        }
        //        sb.Remove(sb.Length - 1, 1);
        //    }
        //    var sql = sb.ToString();
        //    command.CommandText = sql;
        //    command.Parameters.AddRange(@params.ToArray());

        //    if (UseCommandCache)
        //    {
        //        lock (CommandCache)
        //        {
        //            if (CommandCache.ContainsKey(sql))
        //            {
        //                command.Dispose();
        //                command = (TCommand)CommandCache[sb.ToString()];

        //                // use the cached command object, but we must copy over the new command parameter values
        //                // or it will use the old ones
        //                for (int p = 0; p < command.Parameters.Count; p++)
        //                {
        //                    command.Parameters[p].Value = @params[p].Value;
        //                }
        //            }
        //            else
        //            {
        //                CommandCache.Add(sql, command);

        //                // trim the cache so it doesn't grow infinitely
        //                if (CommandCache.Count > CommandCacheMaxLength)
        //                {
        //                    CommandCache.Remove(CommandCache.First().Key);
        //                }
        //            }
        //        }
        //    }

        //    return command;
        //}

        protected override void CheckPrimaryKeyIndex(string entityName)
        {
            if ((Entities[entityName] as FbEntityInfo).PrimaryKeyIndexName != null) return;
            var name = GetPrimaryKeyIndexName(entityName);
            (Entities[entityName] as FbEntityInfo).PrimaryKeyIndexName = name;
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
                using (var command = BuildFilterCommand<FbCommand, FbParameter>(entityName, filters, true, 0, 0))
                {
                    command.Connection = connection as FbConnection;
                    return (int)command.ExecuteScalar();
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        protected override IDataParameter CreateParameterObject(string parameterName, object parameterValue)
        {
            return new FbParameter(parameterName, parameterValue);
        }

        protected override void CreateTable(IDbConnection connection, EntityInfo entity, Boolean forceMultiplePrimaryKeysSyntax)
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
                int count = entity.Fields.Count;
                int keycount = entity.Fields.KeyFields.Count;
                if (count > 0)
                {
                    foreach (var field in entity.Fields)
                    {
                        sql.AppendFormat(" {0} {1} {2} ",
                            field.FieldName,
                            GetFieldDataTypeString(entity.EntityName, field),
                            GetFieldCreationAttributes(entity.EntityAttribute, field, true));
                        if (field.IsPrimaryKey)
                        {
                            keys.Append(field.FieldName);
                            if (--keycount > 0) keys.Append(", ");
                        }
                        if (--count > 0) sql.Append(", ");
                    }
                    sql.AppendFormat(", PRIMARY KEY({0}) )", keys.ToString());
                }
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = sql.ToString();
                    command.Connection = connection;
                    int i = command.ExecuteNonQuery();
                }
                bTableExists = true;
                foreach (var field in entity.Fields)
                {
                    if (field.SearchOrder != FieldSearchOrder.NotSearchable)
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
                            int i = command.ExecuteNonQuery();
                        }
                        // create indexes
                        if (field.SearchOrder != FieldSearchOrder.NotSearchable)
                        {
                            field.IndexName = VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
                        }
                    }
                    else
                    {
                        // create indexes
                        if (field.SearchOrder != FieldSearchOrder.NotSearchable)
                        {
                            field.IndexName = VerifyIndex(entity.EntityName, field.FieldName, field.SearchOrder, connection);
                        }
                    }
                }
            }
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
                    DropAndCreateTable(connection, entity, false);
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

        /// <summary>
        /// Deletes the underlying DataStore
        /// </summary>
        public override void DeleteStore()
        {
            File.Delete(ConnectionStringBase);
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

        protected override Boolean FieldExists(IDbConnection connection, EntityInfo entity, FieldAttribute field)
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

        protected override IDbCommand GetNewCommandObject()
        {
            return new FbCommand();
        }

        protected override string GetFieldDataTypeString(string entityName, FieldAttribute field)
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

        protected override string GetFieldCreationAttributes(EntityAttribute attribute, FieldAttribute field, Boolean MultiplePrimaryKeys)
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
                    throw new NotImplementedException();
                    //switch (field.DataType)
                    //{
                    //    case DbType.Int32:
                    //    case DbType.UInt32:
                    //        sb.Append(AutoIncrementFieldIdentifier + " ");
                    //        break;
                    //    case DbType.Guid:
                    //        sb.Append("ROWGUIDCOL ");
                    //        break;
                    //    default:
                    //        throw new FieldDefinitionException(attribute.NameInStore, field.FieldName,
                    //            string.Format("Data Type '{0}' cannot be marked as an Identity field", field.DataType));
                    //}
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

        private int GetIdentity(IDbConnection connection)
        {
            using (var command = new FbCommand("SELECT @@IDENTITY", connection as FbConnection))
            {
                object id = command.ExecuteScalar();
                return Convert.ToInt32(id);
            }
        }

        private int GetIdentity(IDbTransaction transaction)
        {
            using (var command = new FbCommand("SELECT SCOPE_IDENTITY()", transaction.Connection as FbConnection, transaction as FbTransaction))
            {
                object id = command.ExecuteScalar();
                return Convert.ToInt32(id);
            }
        }

        private FbCommand GetInsertCommand(string entityName)
        {
            // TODO: support command caching to improve bulk insert speeds
            //       simply use a dictionary keyed by entityname
            var keyScheme = Entities[entityName].EntityAttribute.KeyScheme;
            var insertCommand = new FbCommand();

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

                insertCommand.Parameters.Add(new FbParameter(String.Format("@{0}", field.FieldName), DBNull.Value));
            }

            // replace trailing commas
            sbFields[sbFields.Length - 1] = ')';
            sbParams[sbParams.Length - 1] = ')';

            insertCommand.CommandText = sbFields.ToString() + sbParams.ToString();

            return insertCommand;
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

        protected override IDbConnection GetNewConnectionObject()
        {
            return new FbConnection(ConnectionString);
        }

        protected override Boolean TableExists(IDbConnection connection, EntityInfo entity)
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

        protected void ValidateIndex(IDbConnection connection, string indexName, string tableName, string fieldName, bool ascending)
        {
            var valid = false;

            string sql = string.Format("SELECT INDEX_NAME FROM information_schema.indexes WHERE (TABLE_NAME = '{0}') AND (COLUMN_NAME = '{1}')", tableName, fieldName);

            using (FbCommand command = new FbCommand(sql, connection as FbConnection))
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
            using (var command = new FbCommand())
            {
                command.Connection = connection as FbConnection;

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

                                using (var altercmd = new FbCommand(alter.ToString(), connection as FbConnection))
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

        private void UpdateIndexCacheForType(string entityName)
        {
            // have we already cached this?
            if (Entities[entityName].IndexNames != null) return;

            // get all iindex names for the type
            var connection = GetConnection(true);
            try
            {
                string sql = string.Format("" +
                    "SELECT DISTINCT " +
                    " ind.name   " +
                    "FROM sys.indexes ind   " +
                    "INNER JOIN sys.index_columns ic   " +
                    "    ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id   " +
                    "INNER JOIN sys.tables t   " +
                    "    ON ind.object_id = t.object_id   " +
                    "WHERE t.name = '{0}'   " +
                    "    AND ind.is_primary_key = 0   " +
                    "    AND t.is_ms_shipped = 0   ", entityName);

                using (FbCommand command = new FbCommand(sql, connection as FbConnection))
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
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

    }
}
