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
    public class hierarchy1 : modelbase
    {
        [Field(IsPrimaryKey = true, DataType = System.Data.DbType.Int64)]
        public Int64 pid { get; set; }

        [Field(DataType = System.Data.DbType.Int32, SearchOrder=FieldSearchOrder.Ascending)]
        public Int32 numfield { get; set; }

        // The ConditionField will filter all child elements which have their "boolfield" column value set to true.
        [Reference(typeof(hierarchy2a), "fid", ConditionField = "boolfield", ConditionValue = true)]
        public hierarchy2a[] level2a { get; set; }

        [Reference(typeof(hierarchy2b), "fid")]
        public hierarchy2b[] level2b { get; set; }

        public override object CreateRandomObject(object primarykey)
        {
            return CreateRandomObject();
        }
        public override object CreateRandomObject()
        {
            hierarchy1 item = new hierarchy1();
            Random rand = new Random(DateTime.Now.Millisecond);
            item.numfield = rand.Next(1000000);
            return item;
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}", this.GetType().ToString(), pid);
        }
    }
}
