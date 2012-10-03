using System;
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
    public partial class SQLiteDataStore : SQLStoreBase<SQLiteEntityInfo>, IDisposable
    {
        private string m_connectionString;

        private int m_maxSize = 500;

        public string FileName { get; protected set; }

        public override string Name { get{ return FileName; } }

        public enum TransactionSynchronization
        {
            OFF = 0,
            NORMAL = 1,
            FULL = 2
        }

        protected SQLiteDataStore()
            : base()
        {
            ConnectionBehavior = ORM.ConnectionBehavior.Persistent;
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
            else
            {
#if(!WINDOWS_PHONE)
                SQLiteConnection.CreateFile(FileName);
#endif
            }

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

        public int MaxDatabaseSizeInMB
        {
            get { return m_maxSize; }
            set { m_maxSize = value; }
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
            else if (field.IsPrimaryKey & field.IsIdentity)
            {
                // Only the 
                switch (field.DataType)
                {
                    case DbType.Int64:
                    case DbType.UInt64:
                    case DbType.Int32:
                    case DbType.UInt32:
                    case DbType.Int16:
                    case DbType.UInt16:
                    case DbType.Byte:
                        return "integer";
                }
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
        
        public override void EnsureCompatibility()
        {
            throw new NotImplementedException();
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

        protected override Boolean FieldExists(IDbConnection connection, EntityInfo entity, FieldAttribute field)
        {
            Boolean exists = false;
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = String.Format("PRAGMA table_info('{0}')", entity.EntityName, field.FieldName);
                    command.Connection = connection;
                    using (var results = command.ExecuteReader())
                    {
                        while (results.Read())
                        {
                            if (results["name"].ToString().Equals(field.FieldName))
                            {
                                exists = true;
                                break;
                            }
                        }
                    }
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

        protected override Boolean TableExists(IDbConnection connection, EntityInfo entity)
        {
            Boolean exists = false;
            try
            {
                // ANSI SQL way.  Works in PostgreSQL, MSSQL, MySQL.  
                using (var command = GetNewCommandObject())
                {
                    command.CommandText = String.Format("SELECT count(type) FROM sqlite_master where type = 'table' and tbl_name = '{0}'", entity.EntityName);
                    command.Connection = connection;
                    exists = (Int64)command.ExecuteScalar() == 1;
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

                    var sql = string.Format("SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = '{0}'", indexName);
                    command.CommandText = sql;

                    var i = (long)command.ExecuteScalar();

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
                        Name = indexName,
                        MaxCharLength = -1
                    };

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

        protected virtual TransactionSynchronization SetTransactionSynchronization(TransactionSynchronization value)
        {
            TransactionSynchronization result = TransactionSynchronization.OFF;
            var connection = GetConnection(true);
            try
            {
                using (var command = GetNewCommandObject())
                {
                    command.Connection = connection;

                    command.CommandText = string.Format("PRAGMA synchronous = {0}", (int)value);

                    var i = (long)command.ExecuteScalar();

                    result = (TransactionSynchronization)Enum.Parse(typeof(TransactionSynchronization), i.ToString(), true);
                }

                return result;
            }
            finally
            {
                 DoneWithConnection(connection, true);
            }
        }
    }
}
