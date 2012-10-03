using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Data.Common;

namespace OpenNETCF.ORM
{
    partial class MSSqlDataStore
    {
        private void UpdateIndexCacheForType(string entityName)
        {
            // have we already cached this?
            if (Entities[entityName].IndexNames != null) return;

            // get all iindex names for the type
            var connection = GetConnection(true);
            try
            {
                string sql = string.Format("" +
                    "SELECT DISTINCT " +
                    " ind.name   " +
                    "FROM sys.indexes ind   " +
                    "INNER JOIN sys.index_columns ic   " +
                    "    ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id   " +
                    "INNER JOIN sys.tables t   " +
                    "    ON ind.object_id = t.object_id   " +
                    "WHERE t.name = '{0}'   " +
                    "    AND ind.is_primary_key = 0   " +
                    "    AND t.is_ms_shipped = 0   ", entityName);

                using (SqlCommand command = new SqlCommand(sql, connection as SqlConnection))
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

        protected override object[] Select(Type objectType, IEnumerable<FilterCondition> filters, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences, IDbConnection connection)
        {
            string entityName = m_entities.GetNameForType(objectType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(objectType);
            }

            UpdateIndexCacheForType(entityName);

            var items = new List<object>();
            bool tableDirect;

            var entity = m_entities[entityName];

            if (connection == null) connection = GetConnection(false);
            SqlCommand command = null;

            if (UseCommandCache)
            {
                Monitor.Enter(CommandCache);
            }

            try
            {
                OnBeforeSelect(m_entities[entityName], filters, fillReferences);
                var start = DateTime.Now;

                command = GetSelectCommand<SqlCommand, SqlParameter>(entityName, filters, firstRowOffset, fetchCount, out tableDirect);
                command.Connection = connection as SqlConnection;

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

                        Dictionary<string, int> dicOrdinals = null;
                        if (entity.CreateProxy != null)
                        {
                            dicOrdinals = new Dictionary<string, int>();
                            foreach (var field in entity.Fields)
                            {
                                try
                                {
                                    if (!dicOrdinals.ContainsKey(field.FieldName))
                                        dicOrdinals.Add(field.FieldName, results.GetOrdinal(field.FieldName));
                                }
                                catch
                                {
                                    if (!dicOrdinals.ContainsKey(field.FieldName))
                                        dicOrdinals.Add(field.FieldName, -1);
                                }
                            }
                        }

                        // autofill references if desired
                        if (referenceFields == null)
                        {
                            referenceFields = Entities[entityName].References.ToArray();
                        }

                        while (results.Read())
                        {
                            if (currentOffset < firstRowOffset)
                            {
                                currentOffset++;
                                continue;
                            }

                            object item = null;
                            object rowPK = null;

                            if (entity.CreateProxy != null)
                            {
                                if (entity.DefaultConstructor == null)
                                    item = Activator.CreateInstance(objectType);
                                else
                                    item = entity.DefaultConstructor.Invoke(null);

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
                            }
                            else
                            {
                                item = entity.CreateProxy(results, dicOrdinals);
                            }


                            if ((fillReferences) && (referenceFields.Length > 0))
                            {
                                FillReferences(item, rowPK, referenceFields, false, filterReferences, connection);
                            }

                            items.Add(item);

                            if ((fetchCount > 0) && (items.Count >= fetchCount))
                            {
                                break;
                            }
                        }
                    }
                }
                OnAfterSelect(m_entities[entityName], filters, fillReferences, DateTime.Now.Subtract(start), command.CommandText);
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
