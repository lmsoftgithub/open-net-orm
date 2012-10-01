using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNETCF.ORM;

namespace OpenNETCF.ORM.Model
{
    /// <summary>
    /// This class will be bound with a table having a different name than the class.
    /// It also implements an Identity key scheme type. Which means it has AutoIncrement keys.
    /// </summary>
    [Entity(KeyScheme = KeyScheme.Identity, NameInStore = "identity_table")]
    public class identitytable : modelbase
    {
        [Field(IsPrimaryKey = true, DataType = System.Data.DbType.Int64)]
        public Int64 pid { get; set; }

        [Field(DataType = System.Data.DbType.Int32, SearchOrder=FieldSearchOrder.Ascending)]
        public Int32 numfield { get; set; }

        [Field(Length = 50, DataType = System.Data.DbType.AnsiString)]
        public String textfield { get; set; }

        [Field(DataType = System.Data.DbType.Boolean)]
        public Boolean boolfield { get; set; }

        [Field(DataType = System.Data.DbType.Boolean)]
        private Boolean privatefield { get; set; }

        public identitytable()
        {
        }

        public override object CreateRandomObject(object primarykey)
        {
            return CreateRandomObject();
        }
        public override object CreateRandomObject()
        {
            identitytable item = new identitytable();
            Random rand = new Random();
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

        private static identitytable ORM_CreateProxy(System.Data.IDataReader result, System.Collections.Generic.IDictionary<string, int> ordinals)
        {
            var obj = new identitytable();
            obj.pid = (long)result[ordinals["pid"]];
            obj.numfield = (int)result[ordinals["numfield"]];
            obj.boolfield = (bool)result[ordinals["boolfield"]];
            obj.privatefield = (bool)result[ordinals["privatefield"]];
            obj.textfield = (string)result[ordinals["textfield"]];
            return obj;
        }
    }
}
