using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.ORM
{
    public class SqlEntityInfo : EntityInfo
    {
        public SqlEntityInfo()
        {
            PrimaryKeyIndexName = null;
        }

        public string PrimaryKeyIndexName { get; set; }
        public List<string> IndexNames { get; set; }
    }
}
