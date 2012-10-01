using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace OpenNETCF.ORM
{
    public interface IDataStore
    {
        event EventHandler<EntityTypeAddedArgs> EntityTypeAdded;
        event EventHandler<EntitySelectArgs> BeforeSelect;
        event EventHandler<EntitySelectArgs> AfterSelect;
        event EventHandler<EntityInsertArgs> BeforeInsert;
        event EventHandler<EntityInsertArgs> AfterInsert;
        event EventHandler<EntityUpdateArgs> BeforeUpdate;
        event EventHandler<EntityUpdateArgs> AfterUpdate;
        event EventHandler<EntityDeleteArgs> BeforeDelete;
        event EventHandler<EntityDeleteArgs> AfterDelete;

        string Name { get; }

        void AddType<T>();
        void AddType(Type entityType);

        void DiscoverTypes(Assembly containingAssembly);

        void CreateStore();
        void DeleteStore();
        bool StoreExists { get; }
        void EnsureCompatibility();


        EntityInfoCollection<EntityInfo> GetEntities();

        EntityInfo GetEntityInfo(string entityName);

        void Insert(object item);
        void Insert(object item, bool insertReferences);
        void Insert(object item, bool insertReferences, bool transactional);
        void InsertOrUpdate(object item, bool insertReferences);
        void InsertOrUpdate(object item, bool insertReferences, bool transactional);

        void BulkInsert(object items, bool insertReferences);
        void BulkInsert(object items, bool insertReferences, bool transactional);
        void BulkInsertOrUpdate(object items, bool insertReferences);
        void BulkInsertOrUpdate(object items, bool insertReferences, bool transactional);

        T[] Select<T>() where T : new();
        T[] Select<T>(bool fillReferences) where T : new();
        T[] Select<T>(bool fillReferences, bool filterReferences) where T : new();
        T Select<T>(object primaryKey) where T : new();
        T Select<T>(object primaryKey, bool fillReferences) where T : new();
        T Select<T>(object primaryKey, bool fillReferences, bool filterReferences) where T : new();
        T[] Select<T>(string searchFieldName, object matchValue) where T : new();
        T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences) where T : new();
        T[] Select<T>(string searchFieldName, object matchValue, bool fillReferences, bool filterReferences) where T : new();
        T[] Select<T>(IEnumerable<FilterCondition> filters) where T : new();
        T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences) where T : new();
        T[] Select<T>(IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences) where T : new();
        T[] Select<T>(Func<T, bool> selector) where T : new();
        object[] Select(Type entityType);
        object[] Select(Type entityType, bool fillReferences);
        object[] Select(Type entityType, bool fillReferences, bool filterReferences);
        object[] Select(Type entityType, object primaryKey, bool fillReferences, bool filterReferences);
        object[] Select(Type objectType, IEnumerable<FilterCondition> filters, bool fillReferences);
        object[] Select(Type objectType, IEnumerable<FilterCondition> filters, bool fillReferences, bool filterReferences); 

        void Update(object item);
        void Update(object item, bool cascadeUpdates, string fieldName);
        void Update(object item, bool cascadeUpdates, string fieldName, bool transactional);
        void Update(object item, string fieldName);

        int Delete(object item, bool cascade = false);
        int Delete<T>(object primaryKey, bool cascade = false);
        int Delete(Type entityType, object primaryKey, bool cascade = false);
        int Delete<T>(bool cascade = false);
        int Delete(Type entityType, bool cascade = false);
        int Delete<T>(IEnumerable<FilterCondition> filters, bool cascade = false);
        int Delete(Type entityType, IEnumerable<FilterCondition> filters, bool cascade = false);

        void Drop<T>(bool cascade = false);
        void Drop(Type entityType, bool cascade = false);
        void DropAndCreateTable<T>(bool cascade = false);
        void DropAndCreateTable(Type entityType, bool cascade = false);

        T[] Fetch<T>(int fetchCount) where T : new();
        T[] Fetch<T>(int fetchCount, int firstRowOffset) where T : new();
        T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField) where T : new();
        T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences) where T : new();
        object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, bool fillReferences);
        object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences);
        object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences);

        int Count<T>();
        int Count(Type entityType);

        bool Contains(object item);

        void FillReferences(object instance);

        bool TableExists(String entityName);
        bool TableExists(EntityInfo entityInfo);
        bool FieldExists(String entityName, String fieldName);
        bool FieldExists(EntityInfo entityInfo, FieldAttribute fieldInfo);
    }
}
