using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNETCF.ORM.Model
{
    public abstract class modelbase
    {
        public abstract object CreateRandomObject();
        public abstract object CreateRandomObject(object primarykey);
        public abstract override string ToString();
    }
}
