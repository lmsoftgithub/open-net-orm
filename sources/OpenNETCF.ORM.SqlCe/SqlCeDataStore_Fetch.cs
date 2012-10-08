using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlServerCe;
using System.Data;

namespace OpenNETCF.ORM
{
    partial class SqlCeDataStore
    {
        public override object[] Fetch(Type entityType, int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences)
        {
            throw new NotImplementedException();
        }

        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences)
        {
            var type = typeof(T);
            string entityName = m_entities.GetNameForType(type);
            List<T> list = new List<T>();
            string indexName;

            if (!string.IsNullOrEmpty(sortField))
            {
                var field = Entities[entityName].Fields[sortField];

                if (field == null)
                {
                    throw new FieldNotFoundException(
                        string.Format("Sort Field '{0}' not found in Entity '{1}'", sortField, entityName));
                }

                if (sortOrder == FieldOrder.None)
                {
                    throw new System.ArgumentException("You must select a valid sort order if providing a sort field.");
                }

                // ensure that an index exists on the sort field - yes this may slow things down on the first query
                indexName = VerifyIndex(entityName, sortField, sortOrder);
            }
            else
            {
                indexName = Entities[entityName].PrimaryKeyIndexName;
            }

            var connection = GetConnection(false);
            try
            {
                using (var command = new SqlCeCommand())
                {
                    command.CommandText = Entities[entityName].EntityAttribute.NameInStore;
                    command.CommandType = CommandType.TableDirect;
                    command.IndexName = indexName;
                    command.Connection = connection as SqlCeConnection;

                    int count = 0;
                    int currentOffset = 0;
                    int filterOrdinal = -1;

                    using (var results = command.ExecuteResultSet(ResultSetOptions.Scrollable))
                    {
                        var ordinals = GetOrdinals(entityName, results);
                        // get the ordinal for the filter field (if one is used)
                        if (filter != null)
                        {
                            filterOrdinal =ordinals[filter.FieldName];
                        }

                        while (results.Read())
                        {                            
                            // skip until we hit the desired offset - respecting any filter
                            if (currentOffset < firstRowOffset)
                            {
                                if (filterOrdinal >= 0)
                                {
                                    var checkValue = results[filterOrdinal];

                                    if (MatchesFilter(checkValue, filter))
                                    {
                                        currentOffset++;
                                    }
                                }
                                else
                                {
                                    currentOffset++;
                                }
                            }
                            else // pull values (respecting filters) until we have the count fulfilled
                            {
                                if (filterOrdinal >= 0)
                                {
                                    var checkValue = results[filterOrdinal];

                                    if (MatchesFilter(checkValue, filter))
                                    {
                                        // hydrate the object
                                        var item = HydrateEntity<T>(entityName, results, ordinals, fillReferences, connection);
                                        list.Add(item);

                                        currentOffset++;
                                        count++;
                                    }
                                }
                                else
                                {
                                    // hydrate the object
                                    var item = HydrateEntity<T>(entityName, results, ordinals, fillReferences, connection);
                                    list.Add(item);

                                    currentOffset++;
                                    count++;
                                }

                                if (count >= fetchCount) break;
                            }
                        }
                    }
                }
            }
            finally
            {
                DoneWithConnection(connection, false);
            }

            return list.ToArray();
        }

        private T HydrateEntity<T>(string entityName, SqlCeResultSet results, Dictionary<string, int> ordinals, bool fillReferences, IDbConnection connection)
            where T : new()
        {
            var objectType = typeof(T);
//            object item = Activator.CreateInstance(objectType);
            T item = new T();
            object rowPK = null;
            var fields = Entities[entityName].Fields;

            foreach (var field in fields)
            {
                var value = results[ordinals[field.FieldName]];
                if (value != DBNull.Value)
                {
                    if (field.DataType == DbType.Object)
                    {
                        if (fillReferences)
                        {
                            var deserializer = Entities[entityName].Deserializer;

                            if (deserializer == null)
                            {
                                throw new MissingMethodException(
                                    string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                    field.FieldName, entityName));
                            }

                            var @object = deserializer.Invoke(item, field.FieldName, value);
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
                if (field.IsPrimaryKey)
                {
                    rowPK = value;
                }
            }

            // TODO: cache this maybe for perf?
            ReferenceAttribute[] referenceFields = null;

            // autofill references if desired
            if (referenceFields == null)
            {
                referenceFields = Entities[entityName].References.ToArray();
            }

            if ((fillReferences) && (referenceFields.Length > 0))
            {
                //FillReferences(item, rowPK, referenceFields, true);
                FillReferences(item, rowPK, referenceFields, false, fillReferences, connection);
            }

            return item;
        }


        private bool MatchesFilter(object value, FilterCondition filter)
        {
            switch (filter.Operator)
            {
                case FilterCondition.FilterOperator.Equals:
                    return value.Equals(filter.Value);
                default:
                    throw new NotSupportedException("Currently only an 'Equals' filter is supported");
            }
        }
    }
}
