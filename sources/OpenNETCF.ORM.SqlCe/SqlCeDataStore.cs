﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Runtime.InteropServices;
using System.Data.Common;
using System.Data.SqlTypes;

namespace OpenNETCF.ORM
{
    public partial class SqlCeDataStore : SQLStoreBase<SqlEntityInfo>
    {
        private string m_connectionString;
        private int m_maxSize = 128; // Max Database Size defaults to 128MB

        private string Password { get; set; }

        public string FileName { get; protected set; }

        public override string Name
        {
            get
            {
                return FileName;
            }
        }

        protected SqlCeDataStore()
            : base()
        {
            UseCommandCache = true;
            ConnectionBehavior = ConnectionBehavior.Persistent;
        }

        public SqlCeDataStore(string fileName)
            : this(fileName, null)
        {
        }

        public SqlCeDataStore(string fileName, string password)
            : this()
        {
            FileName = fileName;
            Password = password;
        }

        public override bool StoreExists
        {
            get
            {
                return File.Exists(FileName);
            }
        }

        protected override IDbCommand GetNewCommandObject()
        {
            return new SqlCeCommand();
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

        public override void CreateStore()
        {
            CreateStore(false);
        }

        public override void CreateOrUpdateStore()
        {
            CreateStore(true);
        }

        /// <summary>
        /// Creates the underlying DataStore
        /// </summary>
        protected void CreateStore(Boolean UpdateStore)
        {
            if (StoreExists)
            {
                if (!UpdateStore) throw new StoreAlreadyExistsException();
            }
            else
            {
                // create the file
                using (SqlCeEngine engine = new SqlCeEngine(ConnectionString))
                {
                    engine.CreateDatabase();
                }
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

        protected override IDataParameter CreateParameterObject(string parameterName, object parameterValue)
        {
            return new SqlCeParameter(parameterName, parameterValue);
        }

        private int GetIdentity(IDbConnection connection)
        {
            using (var command = new SqlCeCommand("SELECT @@IDENTITY", connection as SqlCeConnection))
            {
                object id = command.ExecuteScalar();
                return Convert.ToInt32(id);
            }
        }

        private int GetIdentity(IDbTransaction transaction)
        {
            using (var command = new SqlCeCommand("SELECT SCOPE_IDENTITY()", transaction.Connection as SqlCeConnection, transaction as SqlCeTransaction))
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
                string sql = string.Format("SELECT INDEX_NAME FROM information_schema.indexes WHERE (TABLE_NAME = '{0}') AND (PRIMARY_KEY = 1)", entityName);

                using (var command = GetNewCommandObject())
                {
                    command.CommandText = sql;
                    OnSqlStatementCreated(command, null);
                    command.Connection = connection;
                    return command.ExecuteScalar() as string;
                }
            }
            finally
            {
                DoneWithConnection(connection, true);
            }
        }

        protected override string GetFieldDataTypeString(string entityName, FieldAttribute field)
        {
            // SqlCe doesn't support varchar or char. Only the N variants.
            switch (field.DataType)
            {
                case DbType.AnsiString:
                    return "nvarchar";
                case DbType.AnsiStringFixedLength:
                    return "nchar";
            }
            return base.GetFieldDataTypeString(entityName, field);
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
                    m_connectionString = string.Format("Data Source={0};Persist Security Info=False;Max Database Size={1};", FileName, MaxDatabaseSizeInMB);

                    if (!string.IsNullOrEmpty(Password))
                    {
                        m_connectionString += string.Format("Password={0};", Password);
                    }
                }
                return m_connectionString;
            }
        }

        protected override IDbConnection GetNewConnectionObject()
        {
            return new SqlCeConnection(ConnectionString);
        }

        protected void ValidateIndex(IDbConnection connection, string indexName, string tableName, string fieldName, bool ascending)
        {
            var valid = false;

            string sql = string.Format("SELECT INDEX_NAME FROM information_schema.indexes WHERE (TABLE_NAME = '{0}') AND (COLUMN_NAME = '{1}')", tableName, fieldName);

            using (SqlCeCommand command = new SqlCeCommand(sql, connection as SqlCeConnection))
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
            using (var command = new SqlCeCommand())
            {
                command.Connection = connection as SqlCeConnection;

                // first make sure the table exists
                var sql = string.Format("SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{0}'", entity.EntityAttribute.NameInStore);

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

                                using (var altercmd = new SqlCeCommand(alter.ToString(), connection as SqlCeConnection))
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
