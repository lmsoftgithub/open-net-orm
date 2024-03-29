﻿using System;
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

    public class EntityTypeChangedArgs : EventArgs
    {
        internal EntityTypeChangedArgs(EntityInfo info, ReferenceAttribute reference)
        {
            EntityInfo = info;
            ReferenceAttribute = reference;
            FieldAttribute = null;
            TableCreated = false;
        }
        internal EntityTypeChangedArgs(EntityInfo info, FieldAttribute field)
        {
            EntityInfo = info;
            ReferenceAttribute = null;
            FieldAttribute = field;
            TableCreated = false;
        }
        public EntityInfo EntityInfo { get; internal set; }
        public FieldAttribute FieldAttribute { get; internal set; }
        public ReferenceAttribute ReferenceAttribute { get; internal set; }
        public bool TableCreated { get; internal set; }
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
            EntityInfo = null;
            if (Item != null) Item.GetType();
            Data = null;
            Timespan = new TimeSpan(0);
        }

        internal EntityDeleteArgs(EntityInfo entity, IEnumerable<FilterCondition> filters)
        {
            Item = null;
            Filters = filters;
            EntityInfo = entity;
            Data = null;
            Timespan = new TimeSpan(0);
        }

        public object Item { get; set; }
        public IEnumerable<FilterCondition> Filters { get; set; }
        public EntityInfo EntityInfo { get; set; }
        public object Data { get; set; }
        public TimeSpan Timespan { get; set; }
    }

    public class SqlStatementArgs : EventArgs
    {
        internal SqlStatementArgs(string query, List<FilterCondition> filters, List<System.Data.IDataParameter> parameters)
        {
            Query = query;
            Filters = filters;
            Parameters = parameters;
            TimeStamp = DateTime.Now;
        }
        internal SqlStatementArgs(System.Data.IDbCommand command, List<FilterCondition> filters)
        {
            Query = command.CommandText;
            Filters = filters;
            if (command.Parameters != null)
            {
                var list = new List<System.Data.IDataParameter>();
                foreach (var param in command.Parameters)
                    list.Add((System.Data.IDataParameter)param);
                Parameters = list;
            }
            TimeStamp = DateTime.Now;
        }

        public String Query { get; set; }
        public IEnumerable<FilterCondition> Filters { get; set; }
        public IEnumerable<System.Data.IDataParameter> Parameters { get; set; }
        public DateTime TimeStamp { get; set; }
    }

}
