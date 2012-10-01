using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM.Model
{
    /// <summary>
    /// This is the most basic way to define a mapping.
    /// Note that since we don't define a keyscheme, the PID won't be an identity field.
    /// </summary>
    [Entity]
    public class basictable : modelbase
    {
        [Field(IsPrimaryKey = true, DataType = System.Data.DbType.Int64)]
        public Int64 pid { get; set; }

        [Field(DataType = System.Data.DbType.Int32)]
        public Int32 numfield { get; set; }

        [Field(Length = 50, DataType = System.Data.DbType.AnsiString)]
        public String textfield { get; set; }

        [Field(DataType = System.Data.DbType.Boolean)]
        public Boolean boolfield { get; set; }

        [Field(DataType = System.Data.DbType.Boolean)]
        private Boolean privatefield { get; set; }

        // Very bad way to make a unique PID, but I was lazy... the principle is to showcase an identity-less class.
        private static int _increment = 0;
        public basictable()
        {
            Random rand = new Random();
            pid = (DateTime.Now.Ticks % 100000000) * 100000 + _increment++;
        }

        public override object CreateRandomObject(object primarykey)
        {
            return CreateRandomObject();
        }
        public override object CreateRandomObject()
        {
            basictable item = new basictable();
            Random rand = new Random(DateTime.Now.Millisecond);
            item.numfield = rand.Next(1000000);
            item.boolfield = (rand.Next(100) >= 50);
            item.privatefield = (rand.Next(100) >= 50);
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

        private static basictable ORM_CreateProxy(System.Data.IDataReader result, System.Collections.Generic.IDictionary<string, int> ordinals)
        {
            var obj = new basictable();
            obj.pid = (long)result[ordinals["pid"]];
            obj.numfield = (int)result[ordinals["numfield"]];
            obj.boolfield = (bool)result[ordinals["boolfield"]];
            obj.privatefield = (bool)result[ordinals["privatefield"]];
            obj.textfield = (string)result[ordinals["textfield"]];
            return obj;
        }
    }
}
