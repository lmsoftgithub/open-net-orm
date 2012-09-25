﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace OpenNETCF.ORM
{
    public abstract class DataStore<TEntityInfo> : IDataStore
        where TEntityInfo : EntityInfo, new()
    {
        protected EntityInfoCollection<TEntityInfo> m_entities = new EntityInfoCollection<TEntityInfo>();

        // TODO: maybe move these to another object since they're more "admin" related?
        public abstract void CreateStore();
        public abstract void CreateOrUpdateStore();
        public abstract void DeleteStore();
        public abstract bool StoreExists { get; }
        public abstract void EnsureCompatibility();

        public abstract void InsertOrUpdate(object item, bool insertReferences, bool transactional);
        public abstract void Insert(object item, bool insertReferences, bool transactional);
        public abstract void InsertOrUpdate(object item, bool insertReferences);
        public abstract void Insert(object item, bool insertReferences);

        public abstract void BulkInsertOrUpdate(object items, bool insertReferences, bool transactional);
        public abstract void BulkInsert(object items, bool insertReferences, bool transactional);
        public abstract void BulkInsertOrUpdate(object items, bool insertReferences);
        public abstract void BulkInsert(object items, bool insertReferences);

        public abstract T[] Select<T>() where T : new();
        public abstract T[] Select<T>(bool fillReferences) where T : new();
        public abstract T[] Select<T>(bool fillReferences, bool filterReferences) where T : new();
        public abstract T Select<T>(object primaryKey) where T : new();
        public abstract T Select<T>(object primaryKey, bool fillReferences) where T : new();
        public abstract T Select<T>(object primaryKey, bool fillReferences, bool filterReferences) where T : new();
        public abstract T[] Select<T>(string searchFieldName, object matchValue) where T : new();
        public abstract T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences) where T : new();
        public abstract T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences, bool filterReferences) where T : new();
        public abstract T[] Select<T>(IEnumerable<FilterCondition> filters) where T : new();
        public abstract T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences) where T : new();
        public abstract T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences) where T : new();
        public abstract object[] Select(Type entityType);
        public abstract object[] Select(Type entityType, bool fillReferences);
        public abstract object[] Select(Type entityType, bool fillReferences, bool filterReferences);
        
        public abstract void Update(object item);
        public abstract void Update(object item, bool cascadeUpdates, string fieldName);
        public abstract void Update(object item, bool cascadeUpdates, string fieldName,  bool transactional);
        public abstract void Update(object item, string fieldName);
        
        public abstract void Delete(object item);
        public abstract void Delete<T>(object primaryKey);
        
        public abstract void FillReferences(object instance);
        public abstract void FillReferences(object instance, bool filterReferences);
        public abstract T[] Fetch<T>(int fetchCount) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences) where T : new();

        public abstract int Count<T>();
        public abstract int Count<T>(IEnumerable<FilterCondition> filters);
        public abstract void Delete<T>();
        public abstract void Delete(System.Type entityType);
        public abstract void Delete<T>(string fieldName, object matchValue);
        public abstract bool Contains(object item);

        public abstract void Drop<T>();
        public abstract void Drop(System.Type entityType);

        public DataStore()
        {
        }

        public EntityInfoCollection<TEntityInfo> Entities 
        {
            get { return m_entities; }
        }

        public EntityInfo GetEntityInfo(string entityName)
        {
            return Entities[entityName];
        }

        public void AddType<T>()
        {
            AddType(typeof(T), true);
        }

        public void AddType(Type entityType)
        {
            AddType(entityType, true);
        }

        private void AddType(Type entityType, bool verifyInterface)
        {
            var attr = (from a in entityType.GetCustomAttributes(true)
                        where a.GetType().Equals(typeof(EntityAttribute))
                        select a).FirstOrDefault() as EntityAttribute;

            if (verifyInterface)
            {
                if (attr == null)
                {
                    throw new ArgumentException(
                        string.Format("Type '{0}' does not have an EntityAttribute", entityType.Name));
                }                
            }

            var map = new TEntityInfo();

            // store the NameInStore if  not explicitly set
            if (attr.NameInStore == null)
            {
                attr.NameInStore = entityType.Name;
            }

            //TODO: validate NameInStore

            map.Initialize(attr, entityType);

            // see if we have any entity 
            // get all field definitions
            foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var attribute = prop.GetCustomAttributes(true)
                    .Where(a => 
                        (a.GetType().Equals(typeof(FieldAttribute)))
                        ).FirstOrDefault() as FieldAttribute;

                if (attribute != null)
                {

                    attribute.PropertyInfo = prop;

                    // construct the true FieldAttribute by merging the propertyinfo and the fileldattribute overrides
                    if (attribute.FieldName == null)
                    {
                        attribute.FieldName = prop.Name;
                    }

                    if (!attribute.DataTypeIsValid)
                    {
                        // TODO: add custom converter support here
                        attribute.DataType = prop.PropertyType.ToDbType();
                    }

                    map.Fields.Add(attribute);
                    if (attribute.SortOrder != FieldSearchOrder.NotSearchable)
                        map.SortingFields.Add(attribute);
                }
                else
                {
                    var reference = prop.GetCustomAttributes(true).Where(a => a.GetType().Equals(typeof(ReferenceAttribute))).FirstOrDefault() as ReferenceAttribute;

                    if (reference != null)
                    {
                        //if (!prop.PropertyType.IsArray)
                        //{
                        //    throw new InvalidReferenceTypeException(reference.ReferenceEntityType, reference.ReferenceField,
                        //        "Reference fields must be arrays");
                        //}

                        reference.PropertyInfo = prop;
                        map.References.Add(reference);
                    }
                }
            }
            if (map.Fields.Count == 0)
            {
                throw new OpenNETCF.ORM.EntityDefinitionException(map.EntityName, string.Format("Entity '{0}' Contains no Field definitions.", map.EntityName));
            }

            m_entities.Add(map);
        }

        public void DiscoverTypes(Assembly containingAssembly)
        {
            var entities = from t in containingAssembly.GetTypes()
                           where t.GetCustomAttributes(true).Where(a => a.GetType().Equals(typeof(EntityAttribute))).FirstOrDefault() != null
                           select t;

            foreach (var entity in entities)
            {
                // the interface has already been verified by our LINQ
                AddType(entity, false);
            }
        }

        protected void AddFieldToEntity(EntityInfo entity, FieldAttribute field)
        {
            entity.Fields.Add(field);
        }

        public void Insert(object item)
        {
            // TODO: should this default to true or false?
            // right now it is false since we don't look for duplicate references
            Insert(item, false);
        }

        public T[] Select<T>(Func<T, bool> selector)
            where T : new()
        {
            return (from e in Select<T>(false)
                   where selector(e)
                   select e).ToArray();
        }

        /// <summary>
        /// List of reserved words including a cross-reference of SQL, SQLite, Firebird, Oracle, MySQL.
        /// </summary>
        protected static string[] m_ReservedWords = new string[] {
            "IDENTITY","ENCRYPTION","ORDER","ADD","END","OUTER","ALL","ERRLVL","OVER","ALTER","ESCAPE","PERCENT","AND","EXCEPT","PLAN","ANY","EXEC",
            "PRECISION","AS","EXECUTE","PRIMARY","ASC","EXISTS","PRINT","AUTHORIZATION","EXIT","PROC","AVG","EXPRESSION","PROCEDURE","BACKUP","FETCH",
            "PUBLIC","BEGIN","FILE","RAISERROR","BETWEEN","FILLFACTOR","READ","BREAK","FOR","READTEXT","BROWSE","FOREIGN","RECONFIGURE","BULK","FREETEXT",
            "REFERENCES","BY","FREETEXTTABLE","REPLICATION","CASCADE","FROM","RESTORE","CASE","FULL","RESTRICT","CHECK","FUNCTION","RETURN","CHECKPOINT",
            "GOTO","REVOKE","CLOSE","GRANT","RIGHT","CLUSTERED","GROUP","ROLLBACK","COALESCE","HAVING","ROWCOUNT","COLLATE","HOLDLOCK","ROWGUIDCOL",
            "COLUMN","RULE","COMMIT","IDENTITY_INSERT","SAVE","COMPUTE","IDENTITYCOL","SCHEMA","CONSTRAINT","IF","SELECT","CONTAINS","IN","SESSION_USER",
            "CONTAINSTABLE","INDEX","SET","CONTINUE","INNER","SETUSER","CONVERT","INSERT","SHUTDOWN","COUNT","INTERSECT","SOME","CREATE","INTO",
            "STATISTICS","CROSS","IS","SUM","CURRENT","JOIN","SYSTEM_USER","CURRENT_DATE","KEY","TABLE","CURRENT_TIME","KILL","TEXTSIZE",
            "CURRENT_TIMESTAMP","LEFT","THEN","CURRENT_USER","LIKE","TO","CURSOR","LINENO","TOP","DATABASE","LOAD","TRAN","DATABASEPASSWORD","MAX",
            "TRANSACTION","DATEADD","MIN","TRIGGER","DATEDIFF","NATIONAL","TRUNCATE","DATENAME","NOCHECK","TSEQUAL","DATEPART","NONCLUSTERED","UNION",
            "DBCC","NOT","UNIQUE","DEALLOCATE","NULL","UPDATE","DECLARE","NULLIF","UPDATETEXT","DEFAULT","OF","USE","DELETE","OFF","USER","DENY","OFFSETS",
            "VALUES","DESC","ON","VARYING","DISK","OPEN","VIEW","DISTINCT","OPENDATASOURCE","WAITFOR","DISTRIBUTED","OPENQUERY","WHEN","DOUBLE",
            "OPENROWSET","WHERE","DROP","OPENXML","WHILE","DUMP","OPTION","WITH","ELSE","OR","WRITETEXT","ABSOLUTE","ACTION","AFTER","ALLOCATE","ARE",
            "ARRAY","ASENSITIVE","ASSERTION","ASYMMETRIC","AT","ATOMIC","BEFORE","BIGINT","BINARY","BIT","BIT_LENGTH","BLOB","BOOLEAN","BOTH","BREADTH",
            "CALL","CALLED","CASCADED","CAST","CATALOG","CHAR","CHAR_LENGTH","CHARACTER","CHARACTER_LENGTH","CLOB","COLLATION","CONDITION","CONNECT",
            "CONNECTION","CONSTRAINTS","CONSTRUCTOR","CORRESPONDING","CUBE","CURRENT_DEFAULT_TRANSFORM_GROUP","CURRENT_PATH","CURRENT_ROLE",
            "CURRENT_TRANSFORM_GROUP_FOR_TYPE","CYCLE","DATA","DATE","DAY","DEC","DECIMAL","DEFERRABLE","DEFERRED","DEPTH","DEREF","DESCRIBE",
            "DESCRIPTOR","DETERMINISTIC","DIAGNOSTICS","DISCONNECT","DO","DOMAIN","DYNAMIC","EACH","ELEMENT","ELSEIF","EQUALS","EXCEPTION","EXTERNAL",
            "EXTRACT","FALSE","FILTER","FIRST","FLOAT","FOUND","FREE","GENERAL","GET","GLOBAL","GO","GROUPING","HANDLER","HOLD","HOUR","IMMEDIATE",
            "INDICATOR","INITIALLY","INOUT","INPUT","INSENSITIVE","INT","INTEGER","INTERVAL","ISOLATION","ITERATE","LANGUAGE","LARGE","LAST","LATERAL",
            "LEADING","LEAVE","LEVEL","LOCAL","LOCALTIME","LOCALTIMESTAMP","LOCATOR","LOOP","LOWER","MAP","MATCH","MEMBER","MERGE","METHOD","MINUTE",
            "MODIFIES","MODULE","MONTH","MULTISET","NAMES","NATURAL","NCHAR","NCLOB","NEW","NEXT","NO","NONE","NUMERIC","OBJECT","OCTET_LENGTH","OLD",
            "ONLY","ORDINALITY","OUT","OUTPUT","OVERLAPS","PAD","PARAMETER","PARTIAL","PARTITION","PATH","POSITION","PREPARE","PRESERVE","PRIOR",
            "PRIVILEGES","RANGE","READS","REAL","RECURSIVE","REF","REFERENCING","RELATIVE","RELEASE","REPEAT","RESIGNAL","RESULT","RETURNS","ROLE",
            "ROLLUP","ROUTINE","ROW","ROWS","SAVEPOINT","SCOPE","SCROLL","SEARCH","SECOND","SECTION","SENSITIVE","SESSION","SETS","SIGNAL","SIMILAR",
            "SIZE","SMALLINT","SPACE","SPECIFIC","SPECIFICTYPE","SQL","SQLCODE","SQLERROR","SQLEXCEPTION","SQLSTATE","SQLWARNING","START","STATE","STATIC",
            "SUBMULTISET","SUBSTRING","SYMMETRIC","SYSTEM","TABLESAMPLE","TEMPORARY","TIME","TIMESTAMP","TIMEZONE_HOUR","TIMEZONE_MINUTE","TRAILING",
            "TRANSLATE","TRANSLATION","TREAT","TRIM","TRUE","UNDER","UNDO","UNKNOWN","UNNEST","UNTIL","UPPER","USAGE","USING","VALUE","VARCHAR","WHENEVER",
            "WINDOW","WITHIN","WITHOUT","WORK","WRITE","YEAR","ZONE","ACCESS","ACCOUNT","ACTIVATE","ADMIN","ADVISE","ALL_ROWS","ANALYZE","ARCHIVE",
            "ARCHIVELOG","AUDIT","AUTHENTICATED","AUTOEXTEND","AUTOMATIC","BECOME","BFILE","BITMAP","BLOCK","BODY","CACHE","CACHE_INSTANCES","CANCEL",
            "CFILE","CHAINED","CHANGE","CHAR_CS","CHOOSE","CHUNK","CLEAR","CLONE","CLOSE_CACHED_OPEN_CURSORS","CLUSTER","COLUMNS","COMMENT","COMMITTED",
            "COMPATIBILITY","COMPILE","COMPLETE","COMPOSITE_LIMIT","COMPRESS","CONNECT_TIME","CONTENTS","CONTROLFILE","COST","CPU_PER_CALL",
            "CPU_PER_SESSION","CURRENT_SCHEMA","CURREN_USER","DANGLING","DATAFILE","DATAFILES","DATAOBJNO","DBA","DBHIGH","DBLOW","DBMAC","DEBUG","DEGREE",
            "DIRECTORY","DISABLE","DISMOUNT","DML","ENABLE","ENFORCE","ENTRY","EXCEPTIONS","EXCHANGE","EXCLUDING","EXCLUSIVE","EXPIRE","EXPLAIN","EXTENT",
            "EXTENTS","EXTERNALLY","FAILED_LOGIN_ATTEMPTS","FAST","FIRST_ROWS","FLAGGER","FLOB","FLUSH","FORCE","FREELIST","FREELISTS","GLOBALLY",
            "GLOBAL_NAME","GROUPS","HASH","HASHKEYS","HEADER","HEAP","IDENTIFIED","IDGENERATORS","IDLE_TIME","INCLUDING","INCREMENT","INDEXED","INDEXES",
            "IND_PARTITION","INITIAL","INITRANS","INSTANCE","INSTANCES","INSTEAD","INTERMEDIATE","ISOLATION_LEVEL","KEEP","LABEL","LAYER","LESS","LIBRARY",
            "LIMIT","LINK","LIST","LOB","LOCK","LOCKED","LOG","LOGFILE","LOGGING","LOGICAL_READS_PER_CALL","LOGICAL_READS_PER_SESSION","LONG","MANAGE",
            "MASTER","MAXARCHLOGS","MAXDATAFILES","MAXEXTENTS","MAXINSTANCES","MAXLOGFILES","MAXLOGHISTORY","MAXLOGMEMBERS","MAXSIZE","MAXTRANS",
            "MAXVALUE","MINIMUM","MINEXTENTS","MINUS","MINVALUE","MLSLABEL","MLS_LABEL_FORMAT","MODE","MODIFY","MOUNT","MOVE","MTS_DISPATCHERS","NCHAR_CS",
            "NEEDED","NESTED","NETWORK","NOARCHIVELOG","NOAUDIT","NOCACHE","NOCOMPRESS","NOCYCLE","NOFORCE","NOLOGGING","NOMAXVALUE","NOMINVALUE",
            "NOORDER","NOOVERRIDE","NOPARALLEL","NOREVERSE","NORMAL","NOSORT","NOTHING","NOWAIT","NUMBER","NVARCHAR2","OBJNO","OBJNO_REUSE","OFFLINE",
            "OID","OIDINDEX","ONLINE","OPCODE","OPTIMAL","OPTIMIZER_GOAL","ORGANIZATION","OSLABEL","OVERFLOW","OWN","PACKAGE","PARALLEL","PASSWORD",
            "PASSWORD_GRACE_TIME","PASSWORD_LIFE_TIME","PASSWORD_LOCK_TIME","PASSWORD_REUSE_MAX","PASSWORD_REUSE_TIME","PASSWORD_VERIFY_FUNCTION",
            "PCTFREE","PCTINCREASE","PCTTHRESHOLD","PCTUSED","PCTVERSION","PERMANENT","PLSQL_DEBUG","POST_TRANSACTION","PRIVATE","PRIVATE_SGA","PRIVILEGE",
            "PROFILE","PURGE","QUEUE","QUOTA","RAW","RBA","READUP","REBUILD","RECOVER","RECOVERABLE","RECOVERY","REFRESH","RENAME","REPLACE","RESET",
            "RESETLOGS","RESIZE","RESOURCE","RESTRICTED","RETURNING","REUSE","REVERSE","ROLES","ROWID","ROWNUM","SAMPLE","SB4","SCAN_INSTANCES","SCN",
            "SD_ALL","SD_INHIBIT","SD_SHOW","SEGMENT","SEG_BLOCK","SEG_FILE","SEQUENCE","SERIALIZABLE","SESSION_CACHED_CURSORS","SESSIONS_PER_USER",
            "SHARE","SHARED","SHARED_POOL","SHRINK","SKIP","SKIP_UNUSABLE_INDEXES","SNAPSHOT","SORT","SPECIFICATION","SPLIT","SQL_TRACE","STANDBY",
            "STATEMENT_ID","STOP","STORAGE","STORE","STRUCTURE","SUCCESSFUL","SWITCH","SYS_OP_ENFORCE_NOT_NULL$","SYS_OP_NTCIMG$","SYNONYM","SYSDATE",
            "SYSDBA","SYSOPER","TABLES","TABLESPACE","TABLESPACE_NO","TABNO","THAN","THE","THREAD","TOPLEVEL","TRACE","TRACING","TRANSITIONAL","TRIGGERS",
            "TX","TYPE","UB2","UBA","UID","UNARCHIVED","UNLIMITED","UNLOCK","UNRECOVERABLE","UNUSABLE","UNUSED","UPDATABLE","VALIDATE","VALIDATION",
            "VARCHAR2","WRITEDOWN","WRITEUP","XID","ACCESSIBLE","DATABASES","DAY_HOUR","DAY_MICROSECOND","DAY_MINUTE","DAY_SECOND","DELAYED","DISTINCTROW",
            "DIV","DUAL","ENCLOSED","ESCAPED","FLOAT4","FLOAT8","FULLTEXT","HIGH_PRIORITY","HOUR_MICROSECOND","HOUR_MINUTE","HOUR_SECOND","IGNORE",
            "INFILE","INT1","INT2","INT3","INT4","INT8","KEYS","LINEAR","LINES","LONGBLOB","LONGTEXT","LOW_PRIORITY","MASTER_SSL_VERIFY_SERVER_CERT",
            "MEDIUMBLOB","MEDIUMINT","MEDIUMTEXT","MIDDLEINT","MINUTE_MICROSECOND","MINUTE_SECOND","MOD","NO_WRITE_TO_BINLOG","OPTIMIZE","OPTIONALLY",
            "OUTFILE","READ_WRITE","REGEXP","REQUIRE","RLIKE","SCHEMAS","SECOND_MICROSECOND","SEPARATOR","SHOW","SPATIAL","SQL_BIG_RESULT",
            "SQL_CALC_FOUND_ROWS","SQL_SMALL_RESULT","SSL","STARTING","STRAIGHT_JOIN","TERMINATED","TINYBLOB","TINYINT","TINYTEXT","UNSIGNED","UTC_DATE",
            "UTC_TIME","UTC_TIMESTAMP","VARBINARY","VARCHARACTER","XOR","YEAR_MONTH","ZEROFILL","IGNORE_SERVER_IDS","MASTER_HEARTBEAT_PERIOD","SLOW",
            "ABORT","ATTACH","AUTOINCREMENT","CONFLICT","DETACH","FAIL","GLOB","ISNULL","NOTNULL","OFFSET","PRAGMA","QUERY","RAISE","REINDEX","TEMP",
            "VACUUM","VIRTUAL"
        };
    }
}