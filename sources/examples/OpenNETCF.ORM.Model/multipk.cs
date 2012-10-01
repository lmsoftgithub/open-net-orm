using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM.Model
{
    /// <summary>
    /// This table has 2 primary keys. There could be more.
    /// </summary>
    [Entity]
    public class multipk : modelbase
    {
        [Field(IsPrimaryKey = true, DataType = System.Data.DbType.Int64)]
        public Int64 pk1 { get; set; }

        [Field(Length = 10, DataType = System.Data.DbType.AnsiString)]
        public String pk2 { get; set; }

        [Field(DataType = System.Data.DbType.Int32)]
        public Int32 numfield { get; set; }

        // Very bad way to make a unique PID, but I was lazy... the principle is to showcase an identity-less class.
        private static int _increment = 0;
        public multipk()
        {
            Random rand = new Random(_increment);
            pk1 = (DateTime.Now.Ticks % 100000000) * 100000 + _increment++;
            pk2 = "";
            for (int i = 0; i < 10; i++)
            {
                pk2 += "abcdefghijklmnopqrstuvwxyz1234567890-_/."[rand.Next(40)];
            }
        }

        public override object CreateRandomObject(object primarykey)
        {
            return CreateRandomObject();
        }
        public override object CreateRandomObject()
        {
            multipk item = new multipk();
            Random rand = new Random();
            item.numfield = rand.Next(1000000);
            return item;
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}/{2}", this.GetType().ToString(), pk1, pk2);
        }

        private static multipk ORM_CreateProxy(System.Data.IDataReader result, System.Collections.Generic.IDictionary<string, int> ordinals)
        {
            var obj = new multipk();
            obj.pk1 = (long)result[ordinals["pk1"]];
            obj.pk2 = (string)result[ordinals["pk2"]];
            obj.numfield = (int)result[ordinals["numfield"]];
            return obj;
        }
    }
}
