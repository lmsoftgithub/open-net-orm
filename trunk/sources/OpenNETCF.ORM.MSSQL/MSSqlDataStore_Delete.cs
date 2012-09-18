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
        //protected override void Delete(Type t, object primaryKey)
        //{
        //    string entityName = m_entities.GetNameForType(t);

        //    if (entityName == null)
        //    {
        //        throw new EntityNotFoundException(t);
        //    }

        //    if (Entities[entityName].Fields.KeyField == null)
        //    {
        //        throw new PrimaryKeyRequiredException("A primary key is required on an Entity in order to perform a Delete");
        //    }
        //    IEnumerable<FilterCondition> filters;
        //    bool tableDirect;

        //    // handle cascade deletes
        //    foreach (var reference in Entities[entityName].References)
        //    {
        //        if (!reference.CascadeDelete) continue;

        //        Delete(reference.ReferenceEntityType, reference.ReferenceField, primaryKey);
        //    }

        //    var connection = GetConnection(false);
        //    try
        //    {
        //        CheckOrdinals(entityName);
        //        CheckPrimaryKeyIndex(entityName);

        //        //command = GetSelectCommand<SqlCommand, SqlParameter>(entityName, filters, out tableDirect);
        //        //command.Connection = connection as SqlConnection;
        //        using (var command = new SqlCommand())
        //        {
        //            command.Connection = connection as SqlConnection;
        //            String sSQL = String.Format("DELETE FROM {0} WHERE {1} = {2}", entityName, Entities[entityName].Fields.KeyField.FieldName, primaryKey.ToString());
        //            command.CommandText = entityName;
        //            command.CommandType = CommandType.Text;

        //            using (var results = command.ExecuteReader(CommandBehavior.SingleResult))
        //            {

        //                // seek on the PK
        //                var found = true; // results.Seek(DbSeekOptions.BeforeEqual, new object[] { primaryKey });

        //                if (!found)
        //                {
        //                    throw new RecordNotFoundException("Cannot locate a record with the provided primary key.  Unable to delete the item");
        //                }
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        DoneWithConnection(connection, false);
        //    }
        //}
    }
}
