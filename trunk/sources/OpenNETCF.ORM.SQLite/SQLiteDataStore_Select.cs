using System;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

#if ANDROID
// note the case difference between the System.Data.SQLite and Mono's implementation
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using SQLiteConnection = Mono.Data.Sqlite.SqliteConnection;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
#elif WINDOWS_PHONE
// ah the joys of an open-source project changing cases on us
using SQLiteConnection = Community.CsharpSqlite.SQLiteClient.SqliteConnection;
using SQLiteCommand = Community.CsharpSqlite.SQLiteClient.SqliteCommand;
using SQLiteParameter = Community.CsharpSqlite.SQLiteClient.SqliteParameter;
using SQLiteDataReader = Community.CsharpSqlite.SQLiteClient.SqliteDataReader;
#else
using System.Data.SQLite;
#endif

namespace OpenNETCF.ORM
{
    public partial class SQLiteDataStore
    {

        protected override object[] Select(Type objectType, IEnumerable<FilterCondition> filters, int fetchCount, int firstRowOffset, bool fillReferences, bool filterReferences, IDbConnection connection)
        {
            string entityName = m_entities.GetNameForType(objectType);

            if (entityName == null)
            {
                throw new EntityNotFoundException(objectType);
            }

            UpdateIndexCacheForType(entityName);

            var items = new List<object>();
            var entity = m_entities[entityName];

            if (connection == null) connection = GetConnection(false);
            SQLiteCommand command = null;

            try
            {
                //Deprecated
                //CheckOrdinals(entityName);

                OnBeforeSelect(m_entities[entityName], filters, fillReferences);
                var start = DateTime.Now;

                bool tableDirect;
                command = GetSelectCommand<SQLiteCommand, SQLiteParameter>(entityName, filters, firstRowOffset, fetchCount, out tableDirect);
                command.Connection = connection as SQLiteConnection;

                int searchOrdinal = -1;
                //    ResultSetOptions options = ResultSetOptions.Scrollable;

                object matchValue = null;
                string matchField = null;

                // TODO: we need to ensure that the search value does not exceed the length of the indexed
                // field, else we'll get an exception on the Seek call below (see the SQL CE implementation)

                using (var results = command.ExecuteReader(CommandBehavior.SingleResult))
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

                            if (searchOrdinal < 0)
                            {
                                searchOrdinal = ordinals[matchField];
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

                            if (entity.CreateProxy == null)
                            {
                                if (entity.DefaultConstructor == null)
                                    item = Activator.CreateInstance(objectType);
                                else
                                    item = entity.DefaultConstructor.Invoke(null);

                                foreach (var field in Entities[entityName].Fields)
                                {
                                    var value = results[ordinals[field.FieldName]];
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
                                        else if ((field.IsPrimaryKey) && (value is Int64) && (field.PropertyInfo.PropertyType.Equals(typeof(Int32))))
                                        {
                                            // SQLite automatically makes auto-increment fields 64-bit, so this works around that behavior
                                            field.PropertyInfo.SetValue(item, Convert.ToInt32(value), null);
                                        }
                                        else if ((value is Int64) || (value is double))
                                        {
                                            // Work Around SQLite underlying storage type
                                            if (field.PropertyInfo.PropertyType.Equals(typeof(UInt32)))
                                            {
                                                field.PropertyInfo.SetValue(item, Convert.ToUInt32(value), null);
                                            }
                                            else if (field.PropertyInfo.PropertyType.Equals(typeof(Int32)))
                                            {
                                                field.PropertyInfo.SetValue(item, Convert.ToInt32(value), null);
                                            }
                                            else if (field.PropertyInfo.PropertyType.Equals(typeof(decimal)))
                                            {
                                                field.PropertyInfo.SetValue(item, Convert.ToDecimal(value), null);
                                            }
                                            else if (field.PropertyInfo.PropertyType.Equals(typeof(float)))
                                            {
                                                field.PropertyInfo.SetValue(item, Convert.ToSingle(value), null);
                                            }
                                            else
                                            {
                                                field.PropertyInfo.SetValue(item, value, null);
                                            }
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
                                item = entity.CreateProxy(results, ordinals);
                            }


                            if ((fillReferences) && (referenceFields.Length > 0))
                            {
                                //FillReferences(item, rowPK, referenceFields, true);
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
