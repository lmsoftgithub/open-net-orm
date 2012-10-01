using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM.Model
{
    /// <summary>
    /// This table is a children item of hierarchy1.
    /// It also contains a Boolean field (boolfield) which allows filtering of returned rows based on its value.
    /// In turn, it contains yet another level of children elements.
    /// </summary>
    [Entity(KeyScheme = KeyScheme.Identity)]
    public class hierarchy2a : modelbase
    {
        [Field(IsPrimaryKey = true, DataType = System.Data.DbType.Int64)]
        public Int64 pid { get; set; }

        [Field(DataType = System.Data.DbType.Int64)]
        public Int64 fid { get; set; }

        [Field(DataType = System.Data.DbType.Boolean)]
        public Boolean boolfield { get; set; }

        [Reference(typeof(hierarchy3a), "fid")]
        public hierarchy3a[] level3a { get; set; }

        public override object CreateRandomObject(object primarykey)
        {
            var item = CreateRandomObject() as hierarchy2a;
            item.fid = (Int64)primarykey;
            return item;
        }
        public override object CreateRandomObject()
        {
            hierarchy2a item = new hierarchy2a();
            Random rand = new Random(DateTime.Now.Millisecond);
            item.boolfield = (rand.Next(100) >= 50);
            return item;
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", this.GetType().ToString(), pid);
        }
    }
}
