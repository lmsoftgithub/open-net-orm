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
        public override int Delete(Type t, object primaryKey, bool cascade)
        {
            int result = 0;
            string entityName = m_entities.GetNameForType(t);

            if (entityName == null)
            {
                throw new EntityNotFoundException(t);
            }

            if (Entities[entityName].Fields.KeyField == null)
            {
                throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform a Delete");
            }
            var connection = GetConnection(false);
            IDbTransaction transaction = null;
            try
            {
                if (cascade)
                {
                    transaction = connection.BeginTransaction();
                    // handle cascade deletes
                    foreach (var reference in Entities[entityName].References)
                    {
                        if (!reference.CascadeDelete) continue;
                        var filters = new List<FilterCondition>();
                        filters.Add(new FilterCondition(reference.ReferenceField, primaryKey, FilterCondition.FilterOperator.Equals));
                        result += Delete(reference.ReferenceEntityType, filters, connection, transaction, cascade);
                    }
                }

                CheckPrimaryKeyIndex(entityName);

                using (var command = new SqlCeCommand())
                {
                    command.Connection = connection as SqlCeConnection;
                    command.CommandText = entityName;
                    command.CommandType = CommandType.TableDirect;
                    command.IndexName = Entities[entityName].PrimaryKeyIndexName;

                    using (var results = command.ExecuteResultSet(ResultSetOptions.Scrollable | ResultSetOptions.Updatable))
                    {

                        // seek on the PK
                        var found = results.Seek(DbSeekOptions.BeforeEqual, new object[] { primaryKey });

                        if (!found)
                        {
                            throw new RecordNotFoundException("Cannot locate a record with the provided primary key.  Unable to delete the item");
                        }

                        results.Read();
                        results.Delete();
                        result++;
                    }
                }
                if (transaction != null) transaction.Commit();
            }
            catch (Exception ex)
            {
                if (transaction != null) transaction.Rollback();
                throw ex;
            }
            finally
            {
                DoneWithTransaction(transaction, false);
                DoneWithConnection(connection, false);
            }
            return result;
        }
    }
}
