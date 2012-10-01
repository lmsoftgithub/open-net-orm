using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM.Model
{
    /// <summary>
    /// This table is the bottommost in the hierarchy. It is a children of hierarchy2a.
    /// It contains a Guid primary key.
    /// </summary>
    [Entity(KeyScheme = KeyScheme.GUID)]
    public class hierarchy3a : modelbase
    {
        [Field(IsPrimaryKey = true, DataType = System.Data.DbType.Guid)]
        public Guid pid { get; set; }

        [Field(DataType = System.Data.DbType.Int64)]
        public Int64 fid { get; set; }

        [Field(Length = 50, DataType = System.Data.DbType.AnsiString)]
        public String textfield1 { get; set; }

        [Field(Length = 50, DataType = System.Data.DbType.AnsiString)]
        public String textfield2 { get; set; }

        [Field(Length = 50, DataType = System.Data.DbType.AnsiString)]
        public String textfield3 { get; set; }

        [Field(DataType = System.Data.DbType.Guid)]
        public Guid uid { get; set; }

        [Field(DataType = System.Data.DbType.Int32, SortOrder=FieldSearchOrder.Descending)]
        public Int32 numfield1 { get; set; }

        [Field(DataType = System.Data.DbType.Int32, SortOrder = FieldSearchOrder.Descending)]
        public Int32 numfield2 { get; set; }

        [Field(DataType = System.Data.DbType.Int32, SortOrder = FieldSearchOrder.Descending)]
        public Int32 numfield3 { get; set; }

        public hierarchy3a()
        {
            // This step is mandatory, the ORM won't create GUIDs for you.
            pid = Guid.NewGuid();
        }

        public override object CreateRandomObject(object primarykey)
        {
            var item = CreateRandomObject() as hierarchy3a;
            item.fid = (Int64)primarykey;
            return item;
        }
        public override object CreateRandomObject()
        {
            hierarchy3a item = new hierarchy3a();
            Random rand = new Random(DateTime.Now.Millisecond);
            item.numfield1 = 0 - rand.Next(1000000);
            item.textfield1 = "text1" + item.numfield1.ToString();
            item.textfield2 = "text2" + item.numfield1.ToString();
            item.textfield3 = "text3" + item.numfield1.ToString();
            item.numfield2 = 2;
            item.numfield3 = 3;
            item.uid = Guid.NewGuid();
            return item;
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", this.GetType().ToString(), pid);
        }

        private static hierarchy3a ORM_CreateProxy(System.Data.IDataReader result, System.Collections.Generic.IDictionary<string, int> ordinals)
        {
            return new hierarchy3a()
            {
                pid = result.IsDBNull(ordinals["pid"]) ? new Guid() : result.GetGuid(ordinals["pid"]),
                fid = result.IsDBNull(ordinals["fid"]) ? 0 : result.GetInt64(ordinals["fid"]),
                uid = result.IsDBNull(ordinals["uid"]) ? new Guid() : result.GetGuid(ordinals["uid"]),
                numfield1 = result.IsDBNull(ordinals["numfield1"]) ? 0 : result.GetInt32(ordinals["numfield1"]),
                numfield2 = result.IsDBNull(ordinals["numfield2"]) ? 0 : result.GetInt32(ordinals["numfield2"]),
                numfield3 = result.IsDBNull(ordinals["numfield3"]) ? 0 : result.GetInt32(ordinals["numfield3"]),
                textfield1 = result.IsDBNull(ordinals["textfield1"]) ? null : result.GetString(ordinals["textfield1"]),
                textfield2 = result.IsDBNull(ordinals["textfield2"]) ? null : result.GetString(ordinals["textfield2"]),
                textfield3 = result.IsDBNull(ordinals["textfield3"]) ? null : result.GetString(ordinals["textfield3"])
            };
            //var obj = new hierarchy3a();
            //obj.pid = result.IsDBNull(ordinals["pid"]) ? new Guid() : result.GetGuid(ordinals["pid"]),
            //obj.fid = result.IsDBNull(ordinals["fid"]) ? 0 : result.GetInt64(ordinals["fid"]),
            //obj.uid = result.IsDBNull(ordinals["uid"]) ? new Guid() : result.GetGuid(ordinals["uid"]),
            //obj.numfield1 = result.IsDBNull(ordinals["numfield1"]) ? 0 : result.GetInt32(ordinals["numfield1"]),
            //obj.numfield2 = result.IsDBNull(ordinals["numfield2"]) ? 0 : result.GetInt32(ordinals["numfield2"]),
            //obj.numfield3 = result.IsDBNull(ordinals["numfield3"]) ? 0 : result.GetInt32(ordinals["numfield3"]),
            //obj.textfield1 = result.IsDBNull(ordinals["textfield1"]) ? null : result.GetString(ordinals["textfield1"]),
            //obj.textfield2 = result.IsDBNull(ordinals["textfield2"]) ? null : result.GetString(ordinals["textfield2"]),
            //obj.textfield3 = result.IsDBNull(ordinals["textfield3"]) ? null : result.GetString(ordinals["textfield3"])
            //return obj;
        }
    }
}
