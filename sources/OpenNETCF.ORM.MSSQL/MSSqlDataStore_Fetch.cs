using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace OpenNETCF.ORM
{
    partial class MSSqlDataStore
    {
        public override T[] Fetch<T>(int fetchCount, int firstRowOffset, string sortField, FieldSearchOrder sortOrder, FilterCondition filter, bool fillReferences, bool filterReferences)
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

                if (sortOrder == FieldSearchOrder.NotSearchable)
                {
                    throw new System.ArgumentException("You must select a valid sort order if providing a sort field.");
                }

                // ensure that an index exists on the sort field - yes this may slow things down on the first query
                indexName = VerifyIndex(entityName, sortField, sortOrder);
            }
            else
            {
                CheckPrimaryKeyIndex(entityName);
                indexName = Entities[entityName].Fields.KeyField.FieldName;
            }

            var connection = GetConnection(false);
            try
            {
                using (var command = new SqlCommand())
                {
                    //command.CommandText = Entities[entityName].EntityAttribute.NameInStore;
                    //command.CommandType = CommandType.TableDirect;
                    //command.IndexName = indexName;
                    //command.Connection = connection as SqlConnection;

                    //command = GetSelectCommand<SqlCommand, SqlParameter>(entityName, filters, out tableDirect);
                    //command.Connection = connection as SqlConnection;

                    //int count = 0;
                    //int currentOffset = 0;
                    //int filterOrdinal = -1;

                    //using (var results = command.ExecuteReader(CommandBehavior.SingleResult))
                    //{
                    //    // get the ordinal for the filter field (if one is used)
                    //    if (filter != null)
                    //    {
                    //        filterOrdinal = results.GetOrdinal(filter.FieldName);
                    //    }

                    //    while (results.Read())
                    //    {
                    //        // skip until we hit the desired offset - respecting any filter
                    //        if (currentOffset < firstRowOffset)
                    //        {
                    //            if (filterOrdinal >= 0)
                    //            {
                    //                var checkValue = results[filterOrdinal];

                    //                //if (MatchesFilter(checkValue, filter))
                    //                //{
                    //                //    currentOffset++;
                    //                //}
                    //            }
                    //            else
                    //            {
                    //                currentOffset++;
                    //            }
                    //        }
                    //        else // pull values (respecting filters) until we have the count fulfilled
                    //        {
                    //            //if (filterOrdinal >= 0)
                    //            //{
                    //            //    var checkValue = results[filterOrdinal];

                    //            //    if (MatchesFilter(checkValue, filter))
                    //            //    {
                    //            //        // hydrate the object
                    //            //        var item = HydrateEntity<T>(entityName, results, fillReferences);
                    //            //        list.Add(item);

                    //            //        currentOffset++;
                    //            //        count++;
                    //            //    }
                    //            //}
                    //            //else
                    //            //{
                    //            //    // hydrate the object
                    //            //    var item = HydrateEntity<T>(entityName, results, fillReferences);
                    //            //    list.Add(item);

                    //            //    currentOffset++;
                    //            //    count++;
                    //            //}

                    //            if (count >= fetchCount) break;
                    //        }
                    //    }
                    //}
                }
            }
            finally
            {
                DoneWithConnection(connection, false);
            }

            return list.ToArray();
        }
    }
}
