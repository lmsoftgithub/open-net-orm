using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM.Model
{
    /// <summary>
    /// This table is the topmost level of imbricated classes
    /// </summary>
    [Entity(KeyScheme = KeyScheme.Identity)]
    public class hierarchy2b : modelbase
    {
        [Field(IsPrimaryKey = true, DataType = System.Data.DbType.Int64)]
        public Int64 pid { get; set; }

        [Field(DataType = System.Data.DbType.Int64)]
        public Int64 fid { get; set; }

        [Field(Length = 50, DataType = System.Data.DbType.AnsiString)]
        public String textfield { get; set; }

        public override object CreateRandomObject(object primarykey)
        {
            var item = CreateRandomObject() as hierarchy2b;
            item.fid = (Int64)primarykey;
            return item;
        }
        public override object CreateRandomObject()
        {
            hierarchy2b item = new hierarchy2b();
            Random rand = new Random(DateTime.Now.Millisecond);
            item.textfield = "";
            for (int i = 0; i < 10; i++)
            {
                item.textfield += "abcdefghijklmnopqrstuvwxyz1234567890-_/."[rand.Next(40)];
            }
            return item;
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", this.GetType().ToString(), pid);
        }

        private static hierarchy2b ORM_CreateProxy(System.Data.IDataReader result, System.Collections.Generic.IDictionary<string, int> ordinals)
        {
            return new hierarchy2b()
            {
                pid = result.IsDBNull(ordinals["pid"]) ? 0 : result.GetInt64(ordinals["pid"]),
                fid = result.IsDBNull(ordinals["fid"]) ? 0 : result.GetInt64(ordinals["fid"]),
                textfield = result.IsDBNull(ordinals["textfield"]) ? null : result.GetString(ordinals["textfield"])
            };
        }
    }
}
