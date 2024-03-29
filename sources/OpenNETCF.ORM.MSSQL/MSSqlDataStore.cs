﻿using System;
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

        public string ConnectionStringBase { get; protected set; }

        public override string Name
        {
            get
            {
                //TODO: Add parsing of the Connection String
                throw new NotImplementedException();
            }
        }

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
            ConnectionStringBase = connectionString;
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
            File.Delete(ConnectionStringBase);
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

        protected override string VerifyIndex(string entityName, string fieldName, FieldOrder searchOrder, IDbConnection connection)
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

                    var sql = string.Format("select COUNT(*) from sys.indexes where name = '{0}' and Object_ID = Object_ID(N'{1}')", indexName, entityName);
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
                OnSqlStatementCreated(command, null);
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
                    m_connectionString = string.Format(ConnectionStringBase, Password);
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

            string sql = string.Format("SELECT DISTINCT ind.name FROM sys.indexes ind INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id " +
                                       " INNER JOIN sys.tables t ON ind.object_id = t.object_id INNER JOIN sys.columns col  ON ic.object_id = col.object_id and ic.column_id = col.column_id  WHERE t.name = '{0}' AND col.name = '{1}'",
                                       tableName, fieldName);

            using (SqlCommand command = new SqlCommand(sql, connection as SqlConnection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Compare(reader.GetString(0), indexName, true) == 0)
                        {
                            valid = true;
                            break;
                        }
                    }
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
    }
}
