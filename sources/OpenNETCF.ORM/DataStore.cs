using System;
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

        protected SequentialGuidTypes _sequentialGuidType = SequentialGuidTypes.NotSequential;
        protected bool _sequentialGuidCrypto = false; // Whether we base our guids on the Crypto lib (true) or on NewGuid()
        public enum SequentialGuidTypes
        {
            NotSequential = 0,
            SortedAsString = 1, // Oracle / MySQL
            SortedAsBinary = 2,
            SortedAtEnd = 3     // MS-SQL Server
        }

        public event EventHandler<EntityTypeAddedArgs> EntityTypeAdded;
        public event EventHandler<EntityTypeChangedArgs> EntityTypeModified;
        public event EventHandler<EntitySelectArgs> BeforeSelect;
        public event EventHandler<EntitySelectArgs> AfterSelect;
        public event EventHandler<EntityInsertArgs> BeforeInsert;
        public event EventHandler<EntityInsertArgs> AfterInsert;
        public event EventHandler<EntityUpdateArgs> BeforeUpdate;
        public event EventHandler<EntityUpdateArgs> AfterUpdate;
        public event EventHandler<EntityDeleteArgs> BeforeDelete;
        public event EventHandler<EntityDeleteArgs> AfterDelete;

        // TODO: maybe move these to another object since they're more "admin" related?
        public abstract void CreateStore();
        public abstract void CreateOrUpdateStore();
        public abstract void DeleteStore();
        public abstract bool StoreExists { get; }
        public abstract void EnsureCompatibility();
        public abstract bool TableExists(String entityName);
        public abstract bool TableExists(EntityInfo entityInfo);
        public abstract bool FieldExists(String entityName, String fieldName);
        public abstract bool FieldExists(EntityInfo entityInfo, FieldAttribute fieldInfo);
        protected abstract void CreateTable(EntityInfo entity);

        public abstract List<DynamicEntityInfo> ReverseEngineer();

        public void Insert(object item)
        {
            Insert(item, false);
        }
        public abstract void InsertOrUpdate(object item, bool insertReferences, bool transactional);
        public abstract void Insert(object item, bool insertReferences, bool transactional);
        public abstract void InsertOrUpdate(object item, bool insertReferences);
        public abstract void Insert(object item, bool insertReferences);

        public abstract void BulkInsertOrUpdate(object items, bool insertReferences, bool transactional);
        public abstract void BulkInsert(object items, bool insertReferences, bool transactional);
        public abstract void BulkInsertOrUpdate(object items, bool insertReferences);
        public abstract void BulkInsert(object items, bool insertReferences);

        public T[] Select<T>(Func<T, bool> selector)
            where T : new()
        {
            return (from e in Select<T>(false)
                    where selector(e)
                    select e).ToArray();
        }
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
        public abstract object[] Select(Type entityType, object primaryKey, bool fillReferences, bool filterReferences);
        public abstract object[] Select(Type objectType, IEnumerable<FilterCondition> filters, bool fillReferences);
        public abstract object[] Select(Type objectType, IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences);
        // Those are meant to be used with Dynamic Entities only, but will actually work with all object types.
        public abstract object[] Select(String entityName);
        public abstract object[] Select(String entityName, bool fillReferences);
        public abstract object[] Select(String entityName, object primaryKey, bool fillReferences);
        public abstract object[] Select(String entityName, object primaryKey, bool fillReferences, bool filterReferences);
        public abstract object[] Select(String entityName, IEnumerable<FilterCondition> filters, bool fillReferences);
        public abstract object[] Select(String entityName, IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences);

        public abstract void Update(object item);
        public abstract void Update(object item, bool cascadeUpdates, string fieldName);
        public abstract void Update(object item, bool cascadeUpdates, string fieldName,  bool transactional);
        public abstract void Update(object item, string fieldName);
        
        public abstract void FillReferences(object instance);
        public abstract void FillReferences(object instance, bool filterReferences);

        public abstract T[] Fetch<T>(int fetchCount) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences) where T : new();
        public abstract T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences) where T : new();
        public abstract object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, bool fillReferences);
        public abstract object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences);
        public abstract object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences);

        public abstract int Count<T>();
        public abstract int Count<T>(IEnumerable<FilterCondition> filters);
        public abstract int Count(Type entityType);
        public abstract int Count(Type entityType, IEnumerable<FilterCondition> filters);
        public abstract int Count(String entityName);
        public abstract int Count(String entityName, IEnumerable<FilterCondition> filters);

        public abstract int Delete(object item, bool cascade);
        public abstract int Delete<T>(object primaryKey, bool cascade);
        public abstract int Delete(Type entityType, object primaryKey, bool cascade);
        public abstract int Delete<T>(bool cascade);
        public abstract int Delete(Type entityType, bool cascade);
        public abstract int Delete<T>(IEnumerable<FilterCondition> filters, bool cascade);
        public abstract int Delete(Type entityType, IEnumerable<FilterCondition> filters, bool cascade);
        public abstract int Delete(String entityName, bool cascade);
        public abstract int Delete(String entityName, IEnumerable<FilterCondition> filters, bool cascade);

        public abstract bool Contains(object item);

        public abstract void Drop<T>(bool cascade);
        public abstract void Drop(Type entityType, bool cascade);
        public abstract void Drop(String entityName, bool cascade);
        public abstract void DropAndCreateTable<T>(bool cascade);
        public abstract void DropAndCreateTable(Type entityType, bool cascade);
        public abstract void DropAndCreateTable(String entityName, bool cascade);

        public DataStore()
        {
            DynamicEntityInfo.EntityDefinitionChanged += new EventHandler<EntityTypeChangedArgs>(DynamicEntityInfo_EntityDefinitionChanged);
        }

        public virtual string Name { get { return "Unnamed DataStore"; } }

        public abstract bool HasConnection { get; }

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

        public EntityInfoCollection<TEntityInfo> Entities 
        {
            get { return m_entities; }
        }

        public EntityInfoCollection<EntityInfo> GetEntities()
        {
            var coll = new EntityInfoCollection<EntityInfo>();
            foreach (var entity in m_entities)
            {
                coll.Add(entity);
            }
            return coll;
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
                    attribute.FieldName = attribute.FieldName.Replace(" ", "_");
                    
                    if (!attribute.DataTypeIsValid)
                    {
                        // TODO: add custom converter support here
                        attribute.DataType = prop.PropertyType.ToDbType();
                    }
                    // Ensures the IsIdentity is set on the field, otherwise it could have side effects at table creation
                    if (attribute.IsPrimaryKey & map.EntityAttribute.KeyScheme == KeyScheme.Identity)
                        attribute.IsIdentity = true;
                    map.Fields.Add(attribute);
                    if (attribute.SortOrder != FieldOrder.None)
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
                        reference.IsArray = prop.PropertyType.IsArray;
                        var interfaces = prop.PropertyType.GetInterfaces();
                        reference.IsList = (from el in prop.PropertyType.GetInterfaces()
                                            where el.Name.Equals("IList")
                                            select el).FirstOrDefault() != null;
                        map.References.Add(reference);
                    }
                }
            }
            if (map.Fields.Count == 0)
            {
                throw new OpenNETCF.ORM.EntityDefinitionException(map.EntityName, string.Format("Entity '{0}' Contains no Field definitions.", map.EntityName));
            }
            if (map.References != null && map.References.Count > 0 && map.Fields.KeyFields.Count != 1)
            {
                throw new OpenNETCF.ORM.EntityDefinitionException(map.EntityName, string.Format("Entity '{0}' Contains references but does have an invalid Primary Key count ({1}).", map.EntityName, map.Fields.KeyFields.Count));
            }
            var createProxyMethod = entityType.GetMethod("ORM_CreateProxy", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (createProxyMethod != null)
            {
                var parameters = createProxyMethod.GetParameters();
                if (parameters.Length >= 2 && parameters[1].ParameterType.Equals(typeof(System.Collections.Generic.IDictionary<string, int>)))
                {
                    map.CreateProxy = (EntityInfo.CreateProxyDelegate)Delegate.CreateDelegate(typeof(EntityInfo.CreateProxyDelegate),null, createProxyMethod);
                }
            }

            var ctor = entityType.GetConstructor(new Type[]{});
            if (ctor != null)
                map.DefaultConstructor = ctor;
            else // We want a default constructor because we want the ability to instantiate new empty objects to fill in.
                throw new EntityDefinitionException(entityType.ToString(), "The Type doesn't have a parameterless default constructor");

            var serializerMethod = entityType.GetMethod("Serialize", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (serializerMethod != null)
            {
                var serParameters = serializerMethod.GetParameters();
                if (serParameters.Length >= 2 &&
                    serParameters[0].ParameterType.Equals(typeof(object)) &&
                    serParameters[1].ParameterType.Equals(typeof(string)))
                {
                    var deserializerMethod = entityType.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                    if (deserializerMethod != null)
                    {
                        var desParameters = deserializerMethod.GetParameters();
                        if (desParameters.Length >= 3 &&
                            desParameters[0].ParameterType.Equals(typeof(object)) &&
                            desParameters[1].ParameterType.Equals(typeof(string)) &&
                            desParameters[2].ParameterType.Equals(typeof(object)))
                        {
                            map.Serializer = (EntityInfo.SerializerDelegate)Delegate.CreateDelegate(typeof(EntityInfo.SerializerDelegate), null, serializerMethod);
                            map.Deserializer = (EntityInfo.DeserializerDelegate)Delegate.CreateDelegate(typeof(EntityInfo.DeserializerDelegate), null, deserializerMethod);
                        }
                    }
                }
            }

            m_entities.Add(map);
            OnTypeAdded(map);
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

        public virtual void RegisterEntity(EntityInfo entity)
        {
            if (!(entity is TEntityInfo)) throw new InvalidCastException(string.Format("The entity '{0}' does not match the EntityInfo type ({1})", entity.EntityName, typeof(TEntityInfo).ToString()));
            if (m_entities.HasEntity(entity.EntityName))
            {
                // We check if the existing entity is a DynamicEntityInfo, if not, then we cannot replace/update it.
                if (!(m_entities[entity.EntityName] is DynamicEntityInfo)) throw new EntityDefinitionException(entity.EntityName, "A static Entity with the same name already exists in the DataStore");
                // Next, we check if all the fields of the existing entity are the same as those from the current entity.
                foreach (var f in m_entities[entity.EntityName].Fields)
                {
                    if (!entity.Fields.HasField(f.FieldName))
                        throw new EntityDefinitionException(entity.EntityName, String.Format("The new Entity Definition is missing the  '{0}' field", f.FieldName));
                    if (!entity.Fields[f.FieldName].Equals(f))
                        throw new EntityDefinitionException(entity.EntityName, String.Format("The existing field '{0}' does not match the new Entity Definition", f.FieldName));
                }
                m_entities[entity.EntityName] = (TEntityInfo)entity;
            }
            else
            {
                m_entities.Add((TEntityInfo)entity);
            }
            if (StoreExists) CreateTable(entity);
            if (entity is DynamicEntityInfo)
            {
                ((DynamicEntityInfo)entity).Registered = true;
            }
            OnTypeAdded(entity);
        }

        protected virtual void DynamicEntityInfo_EntityDefinitionChanged(object sender, EntityTypeChangedArgs e)
        {
            try
            {
                if (e.EntityInfo != null && e.FieldAttribute != null)
                {
                    if (!FieldExists(e.EntityInfo, e.FieldAttribute))
                    {
                        if (!e.TableCreated && StoreExists)
                        {
                            CreateTable(e.EntityInfo);
                            e.TableCreated = true;
                        }
                        OnTypeModified(e.EntityInfo, e.FieldAttribute, e.TableCreated);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!FieldExists(e.EntityInfo, e.FieldAttribute)) e.EntityInfo.Fields.Remove(e.FieldAttribute);
                throw ex;
            }
        }

        protected void AddFieldToEntity(EntityInfo entity, FieldAttribute field)
        {
            try
            {
                if (entity != null && field != null && !entity.Fields.HasField(field.FieldName))
                {
                    if (StoreExists && TableExists(entity))
                    {
                        if (!FieldExists(entity, field))
                        {
                            entity.Fields.Add(field);
                            CreateTable(entity);
                            OnTypeModified(entity, field, true);
                        }
                    }
                    else
                    {
                        entity.Fields.Add(field);
                        OnTypeModified(entity, field, false);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!FieldExists(entity, field)) entity.Fields.Remove(field);
                throw ex;
            }
        }

        protected virtual String ProtectFieldName(String objectName)
        {
            return objectName;
        }

        protected virtual String ProtectTableName(String objectName)
        {
            return objectName;
        }

        public virtual void OnTypeAdded(EntityInfo entity)
        {
            if (EntityTypeAdded != null)
            {
                EntityTypeAdded(this, new EntityTypeAddedArgs(entity));
            }
        }

        public virtual void OnTypeModified(EntityInfo entity, FieldAttribute attribute, bool tableCreated)
        {
            if (EntityTypeModified != null)
            {
                EntityTypeModified(this, new EntityTypeChangedArgs(entity, attribute) { TableCreated = tableCreated });
            }
        }

        public virtual void OnBeforeInsert(object item, bool insertReferences)
        {
            if (BeforeInsert != null)
            {
                BeforeInsert(this, new EntityInsertArgs(item, insertReferences));
            }
        }

        public virtual void OnAfterInsert(object item, bool insertReferences, TimeSpan executionTime, string sqlQuery)
        {
            if (AfterInsert != null)
            {
                AfterInsert(this, new EntityInsertArgs(item, insertReferences) { Timespan = executionTime, Data = sqlQuery });
            }
        }

        public virtual void OnBeforeUpdate(object item, bool cascadeUpdates, string fieldName)
        {
            if (BeforeUpdate != null)
            {
                BeforeUpdate(this, new EntityUpdateArgs(item, cascadeUpdates, fieldName));
            }
        }

        public virtual void OnAfterUpdate(object item, bool cascadeUpdates, string fieldName, TimeSpan executionTime, string sqlQuery)
        {
            if (AfterUpdate != null)
            {
                AfterUpdate(this, new EntityUpdateArgs(item, cascadeUpdates, fieldName) { Timespan = executionTime, Data = sqlQuery });
            }
        }

        public virtual void OnBeforeDelete(object item)
        {
            if (BeforeDelete != null)
            {
                BeforeDelete(this, new EntityDeleteArgs(item));
            }
        }

        public virtual void OnBeforeDelete(EntityInfo entity, IEnumerable<FilterCondition> filters)
        {
            if (BeforeDelete != null)
            {
                BeforeDelete(this, new EntityDeleteArgs(entity, filters));
            }
        }

        public virtual void OnAfterDelete(object item, TimeSpan executionTime, string sqlQuery)
        {
            if (AfterDelete != null)
            {
                AfterDelete(this, new EntityDeleteArgs(item) { Timespan = executionTime, Data = sqlQuery });
            }
        }

        public virtual void OnAfterDelete(EntityInfo entity, IEnumerable<FilterCondition> filters, TimeSpan executionTime, string sqlQuery)
        {
            if (AfterDelete != null)
            {
                AfterDelete(this, new EntityDeleteArgs(entity, filters) { Timespan = executionTime, Data = sqlQuery });
            }
        }

        public virtual void OnBeforeSelect(EntityInfo entity, IEnumerable<FilterCondition> filters, bool fillReferences)
        {
            if (BeforeSelect != null)
            {
                BeforeSelect(this, new EntitySelectArgs(entity, filters, fillReferences));
            }
        }

        public virtual void OnAfterSelect(EntityInfo entity, IEnumerable<FilterCondition> filters, bool fillReferences, TimeSpan executionTime, string sqlQuery)
        {
            if (AfterSelect != null)
            {
                AfterSelect(this, new EntitySelectArgs(entity, filters, fillReferences) { Timespan = executionTime, Data = sqlQuery });
            }
        }

        public virtual Guid NewGuidComb()
        {
            Guid result = Guid.Empty;
            if (this._sequentialGuidType != SequentialGuidTypes.NotSequential)
            {
                byte[] guid = null;
                if (this._sequentialGuidCrypto)
                {
                    var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
                    byte[] random = new byte[10];
                    rng.GetBytes(random);
                    Buffer.BlockCopy(random, 0, guid, 6, 10);
                }
                else
                {
                    guid = Guid.NewGuid().ToByteArray();
                }
                long timestamp = DateTime.Now.Ticks / 10000L;
                byte[] timestampBytes = BitConverter.GetBytes(timestamp);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(timestampBytes);
                }
                switch (this._sequentialGuidType)
                {
                    case SequentialGuidTypes.SortedAsString:
                    case SequentialGuidTypes.SortedAsBinary:
                        Buffer.BlockCopy(timestampBytes, 2, guid, 0, 6);
                        break;
                    case SequentialGuidTypes.SortedAtEnd:
                        Buffer.BlockCopy(timestampBytes, 2, guid, 10, 6);
                        break;
                }
                result = new Guid(guid);
            }
            else
            {
                result = Guid.NewGuid();
            }
            return result;
        }
    }
}
