﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;
using System.Threading;
using System.Data.Common;

namespace OpenNETCF.ORM
{
    partial class SqlCeDataStore
    {
        protected override TCommand GetSelectCommand<TCommand, TParameter>(string entityName, IEnumerable<FilterCondition> filters, int rowOffset, int rowCount, out bool tableDirect)
        {
            tableDirect = true;
            var buildFilter = false;
            string indexName = null;

            if (filters != null)
            {
                if (filters.Count() == 1)
                {
                    var filter = filters.First();

                    if (!(filter is SqlFilterCondition))
                    {
                        var field = Entities[entityName].Fields[filter.FieldName];

                        if (!field.IsPrimaryKey)
                        {
                            if (field.SearchOrder == FieldOrder.None)
                            {
                                buildFilter = true;
                            }
                            else if (filter.Operator != FilterCondition.FilterOperator.Equals)
                            {
                                buildFilter = true;
                            }
                            else
                            {
                                indexName = string.Format("ORM_IDX_{0}_{1}_{2}", entityName, filter.FieldName,
                                    field.SearchOrder == FieldOrder.Descending ? "DESC" : "ASC");

                                // build the index if it's not there
                                field.IndexName = VerifyIndex(entityName, filter.FieldName, field.SearchOrder, null);
                            }
                        }
                    }
                }
                else if (filters.Count() >= 1)
                {
                    var filter = filters.First() as SqlFilterCondition;

                    if (filter == null || !filter.PrimaryKey)
                    {
                        buildFilter = true;
                    }
                }
            }

            if (buildFilter)
            {
                tableDirect = false;
                return BuildFilterCommand<TCommand, TParameter>(entityName, filters, false, rowOffset, rowCount);
            }

            return new SqlCeCommand()
            {
                CommandText = entityName,
                CommandType = CommandType.TableDirect,
                IndexName = indexName ?? Entities[entityName].PrimaryKeyIndexName
            } as TCommand;
        }

        private void UpdateIndexCacheForType(string entityName)
        {
            // have we already cached this?
            if (Entities[entityName].IndexNames != null) return;

            // get all iindex names for the type
            var connection = GetConnection(true);
            try
            {
                string sql = string.Format("SELECT INDEX_NAME FROM information_schema.indexes WHERE (TABLE_NAME = '{0}')", entityName);

                using (SqlCeCommand command = new SqlCeCommand(sql, connection as SqlCeConnection))
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
            finally
            {
                DoneWithConnection(connection, true);
            }
        }
        
        protected override object[] Select(string entityName, IEnumerable<FilterCondition> filters, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences, IDbConnection connection)
        {
            if (!m_entities.HasEntity(entityName)) throw new EntityNotFoundException(entityName);

            UpdateIndexCacheForType(entityName);

            //var genType = typeof(List<>).MakeGenericType(objectType);
            //var items = (System.Collections.IList)Activator.CreateInstance(genType);

            var items = new List<object>();
            bool tableDirect;

            SqlEntityInfo entity = m_entities[entityName];
            var isDynamicEntity = entity is DynamicEntityInfo;

            if (connection == null) 
                connection = GetConnection(false);
            SqlCeCommand command = null;

            if (UseCommandCache)
            {
                Monitor.Enter(CommandCache);
            }

            try
            {

                CheckPrimaryKeyIndex(entityName);

                OnBeforeSelect(entity, filters, fillReferences);
                var start = DateTime.Now;

                command = GetSelectCommand<SqlCeCommand, SqlCeParameter>(entityName, filters, firstRowOffset, fetchCount, out tableDirect);
                command.Connection = connection as SqlCeConnection;

                int searchOrdinal = -1;
                ResultSetOptions options = ResultSetOptions.Scrollable;

                object matchValue = null;
                string matchField = null;
                bool primarykeyfilter = false;

                if (tableDirect) // use index
                {
                    if ((filters != null) && (filters.Count() > 0))
                    {
                        var filter = filters.First();

                        matchValue = filter.Value ?? DBNull.Value;
                        matchField = filter.FieldName;

                        var sqlfilter = filter as SqlFilterCondition;
                        if ((sqlfilter != null) && (sqlfilter.PrimaryKey))
                        {
                            primarykeyfilter = true;
                        }
                    }

                    // we need to ensure that the search value does not exceed the length of the indexed
                    // field, else we'll get an exception on the Seek call below
                    var indexInfo = GetIndexInfo(command.IndexName);
                    if (indexInfo != null)
                    {
                        if (indexInfo.MaxCharLength > 0)
                        {
                            var value = (string)matchValue;
                            if (value.Length > indexInfo.MaxCharLength)
                            {
                                matchValue = value.Substring(0, indexInfo.MaxCharLength);
                            }
                        }
                    }
                }
                using (var results = command.ExecuteResultSet(options))
                {
                    if (results.HasRows)
                    {
                        var ordinals = GetOrdinals(entityName, results);

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

                            if (primarykeyfilter)
                                searchOrdinal = ordinals[m_entities[entityName].Fields.KeyField.FieldName];
                            else if (searchOrdinal < 0)
                                searchOrdinal = ordinals[matchField];

                            if (tableDirect)
                            {
                                results.Seek(DbSeekOptions.FirstEqual, new object[] { matchValue });
                            }
                        }

                        // autofill references if desired
                        if (referenceFields == null)
                        {
                            referenceFields = entity.References.ToArray();
                        }

                        while (results.Read())
                        {
                            if (currentOffset < firstRowOffset)
                            {
                                currentOffset++;
                                continue;
                            }

                            if (tableDirect && (matchValue != null))
                            {
                                // if we have a match value, we'll have seeked to the first match above
                                // then at this point the first non-match means we have no more matches, so
                                // we can exit out once we hit the first non-match.

                                // For string we want a case-insensitive search, so it's special-cased here
                                if (matchValue is string)
                                {
                                    if (string.Compare((string)results[searchOrdinal], (string)matchValue, true) != 0)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    if (!results[searchOrdinal].Equals(matchValue))
                                    {
                                        break;
                                    }
                                }
                            }

                            object item = null;
                            object rowPK = null;

                            if (isDynamicEntity)
                            {
                                var dynamic = new DynamicEntity(entity as DynamicEntityInfo);
                                foreach (var pair in ordinals)
                                {
                                    if (entity.Fields[pair.Key].DataType == DbType.Object)
                                    {
                                        if (entity.Deserializer == null)
                                        {
                                            throw new MissingMethodException(
                                                string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                                pair.Key, entityName));
                                        }
                                        dynamic[pair.Key] = entity.Deserializer(dynamic, pair.Key, results[pair.Value]);
                                    }
                                    else
                                    {
                                        dynamic[pair.Key] = results[pair.Value];
                                    }
                                }
                                item = dynamic;
                            }
                            else if (entity.CreateProxy == null)
                            {
                                if (entity.DefaultConstructor == null)
                                    item = Activator.CreateInstance(entity.EntityType);
                                else
                                    item = entity.DefaultConstructor.Invoke(null);

                                foreach (var field in entity.Fields)
                                {
                                    var value = results[ordinals[field.FieldName]];
                                    if (value != DBNull.Value)
                                    {
                                        if (field.DataType == DbType.Object)
                                        {
                                            if (entity.Deserializer == null)
                                            {
                                                throw new MissingMethodException(
                                                    string.Format("The field '{0}' requires a custom serializer/deserializer method pair in the '{1}' Entity",
                                                    field.FieldName, entityName));
                                            }

                                            var @object = entity.Deserializer.Invoke(item, field.FieldName, value);
                                            field.PropertyInfo.SetValue(item, @object, null);
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
                            }
                            else
                            {
                                item = entity.CreateProxy.Invoke(results, ordinals);
                            }

                            if ((fillReferences) && (referenceFields.Length > 0))
                            {
                                if (entity.Fields.KeyFields.Count == 1)
                                {
                                    rowPK = entity.Fields.KeyField.PropertyInfo.GetValue(item, null);
                                    //FillReferences(item, rowPK, referenceFields, true);
                                    FillReferences(item, rowPK, referenceFields, false, fillReferences, connection);
                                }
                            }

                            items.Add(item);
                            if ((fetchCount > 0) && (items.Count >= fetchCount))
                            {
                                break;
                            }
                        }
                    }
                }
                OnAfterSelect(entity, filters, fillReferences, DateTime.Now.Subtract(start), command.CommandText);
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
