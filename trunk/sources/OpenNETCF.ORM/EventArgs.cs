using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace OpenNETCF.ORM
{
    public class EntityTypeAddedArgs : EventArgs
    {
        internal EntityTypeAddedArgs(EntityInfo info)
        {
            EntityInfo = info;
        }

        public EntityInfo EntityInfo { get; set; }
    }

    public class EntitySelectArgs : EventArgs
    {
        internal EntitySelectArgs(EntityInfo info, IEnumerable<FilterCondition> filters, bool fillReferences)
        {
            Info = info;
            FillReferences = fillReferences;
            Filters = filters;
            Data = null;
            Timespan = new TimeSpan(0);
        }

        public EntityInfo Info { get; set; }
        public bool FillReferences { get; set; }
        public IEnumerable<FilterCondition> Filters { get; set; }
        public object Data { get; set; }
        public TimeSpan Timespan { get; set; }
    }

    public class EntityUpdateArgs : EventArgs
    {
        internal EntityUpdateArgs(object item, bool cascadeUpdates, string fieldName)
        {
            Item = item;
            CascadeUpdates = cascadeUpdates;
            FieldName = fieldName;
            Data = null;
            Timespan = new TimeSpan(0);
        }

        public object Item { get; set; }
        public bool CascadeUpdates { get; set; }
        public string FieldName { get; set; }
        public object Data { get; set; }
        public TimeSpan Timespan { get; set; }
    }

    public class EntityInsertArgs : EventArgs
    {
        internal EntityInsertArgs(object item, bool insertReferences)
        {
            Item = item;
            InsertReferences = insertReferences;
            Data = null;
            Timespan = new TimeSpan(0);
        }

        public object Item { get; set; }
        public bool InsertReferences { get; set; }
        public object Data { get; set; }
        public TimeSpan Timespan { get; set; }
    }

    public class EntityDeleteArgs : EventArgs
    {
        internal EntityDeleteArgs(object item)
        {
            Item = item;
            Filters = null;
            EntityType = null;
            if (Item != null) Item.GetType();
            Data = null;
            Timespan = new TimeSpan(0);
        }

        internal EntityDeleteArgs(Type entityType, IEnumerable<FilterCondition> filters)
        {
            Item = null;
            Filters = filters;
            EntityType = entityType;
            Data = null;
            Timespan = new TimeSpan(0);
        }

        public object Item { get; set; }
        public IEnumerable<FilterCondition> Filters { get; set; }
        public Type EntityType { get; set; }
        public object Data { get; set; }
        public TimeSpan Timespan { get; set; }
    }
}
