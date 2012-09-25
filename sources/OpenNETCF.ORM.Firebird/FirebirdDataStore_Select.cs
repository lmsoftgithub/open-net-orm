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
using System.Threading;

namespace OpenNETCF.ORM
{
    public partial class FirebirdDataStore
    {

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
                        FieldName = (Entities[entityName] as FbEntityInfo).Fields.KeyField.FieldName,
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

        protected object[] Select(Type objectType, IEnumerable<FilterCondition> filters, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences)
        {
            string entityName = m_entities.GetNameForType(objectType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(objectType);
            }

            UpdateIndexCacheForType(entityName);

            var items = new List<object>();
            bool tableDirect;

            var connection = GetConnection(false);
            FbCommand command = null;

            if (UseCommandCache)
            {
                Monitor.Enter(CommandCache);
            }

            try
            {
                CheckOrdinals(entityName);
                command = GetSelectCommand<FbCommand, FbParameter>(entityName, filters, out tableDirect);
                command.Connection = connection as FbConnection;

                int searchOrdinal = -1;
                //ResultSetOptions options = ResultSetOptions.Scrollable;

                object matchValue = null;
                string matchField = null;

                using (var results = command.ExecuteReader(CommandBehavior.Default))
                {
                    if (results.HasRows)
                    {
                        ReferenceAttribute[] referenceFields = null;

                        int currentOffset = 0;

                        if (matchValue != null)
                        {
                            // convert enums to an int, else the .Equals later check will fail
                            // this feels a bit kludgey, but for now it's all I can think of
                            if (matchValue.GetType().IsEnum)
                            {
                                matchValue = (int)matchValue;
                            }

                            if (searchOrdinal < 0)
                            {
                                searchOrdinal = results.GetOrdinal(matchField);
                            }
                        }

                        while (results.Read())
                        {
                            if (currentOffset < firstRowOffset)
                            {
                                currentOffset++;
                                continue;
                            }

                            object item = Activator.CreateInstance(objectType);
                            object rowPK = null;

                            // autofill references if desired
                            if (referenceFields == null)
                            {
                                referenceFields = Entities[entityName].References.ToArray();
                            }

                            foreach (var field in Entities[entityName].Fields)
                            {
                                var value = results[field.FieldName];
                                if (value != DBNull.Value)
                                {
                                    if (field.DataType == DbType.Object)
                                    {
                                        if (fillReferences)
                                        {
                                            // get serializer
                                            var itemType = item.GetType();
                                            var deserializer = GetDeserializer(itemType);

                                            if (deserializer == null)
                                            {
                                                throw new MissingMethodException(
                                                    string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                                    field.FieldName, entityName));
                                            }

                                            var @object = deserializer.Invoke(item, new object[] { field.FieldName, value });
                                            field.PropertyInfo.SetValue(item, @object, null);
                                        }
                                    }
                                    else if (field.IsRowVersion)
                                    {
                                        // sql stores this an 8-byte array
                                        field.PropertyInfo.SetValue(item, BitConverter.ToInt64((byte[])value, 0), null);
                                    }
                                    else if (field.PropertyInfo.PropertyType.UnderlyingTypeIs<TimeSpan>())
                                    {
                                        // SQL Compact doesn't support Time, so we're convert to ticks in both directions
                                        var valueAsTimeSpan = new TimeSpan((long)value);
                                        field.PropertyInfo.SetValue(item, valueAsTimeSpan, null);
                                    }
                                    else
                                    {
                                        field.PropertyInfo.SetValue(item, value, null);
                                    }
                                }
                                //Check if it is reference key to set, not primary.
                                ReferenceAttribute attr = referenceFields.Where(
                                    x => x.ReferenceField == field.FieldName).FirstOrDefault();

                                if (attr != null)
                                {
                                    rowPK = value;
                                }
                                if (field.IsPrimaryKey)
                                {
                                    rowPK = value;
                                }
                            }

                            if ((fillReferences) && (referenceFields.Length > 0))
                            {
                                //FillReferences(item, rowPK, referenceFields, true);
                                FillReferences(item, rowPK, referenceFields, false, filterReferences);
                            }

                            items.Add(item);

                            if ((fetchCount > 0) && (items.Count >= fetchCount))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                if ((!UseCommandCache) && (command != null))
                {
                    command.Dispose();
                }

                if (UseCommandCache)
                {
                    Monitor.Exit(CommandCache);
                }

                FlushReferenceTableCache();
                DoneWithConnection(connection, false);
            }

            return items.ToArray();
        }

    }
}
