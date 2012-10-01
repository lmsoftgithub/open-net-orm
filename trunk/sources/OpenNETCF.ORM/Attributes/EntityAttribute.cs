using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace OpenNETCF.ORM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityAttribute : Attribute
    {
        public EntityAttribute()
            : this(KeyScheme.None)
        {
        }

        public EntityAttribute(KeyScheme keyScheme)
        {
            KeyScheme = keyScheme;
        }

        private string _nameInStore;
        public string NameInStore {
            get { return _nameInStore; }
            set
            {
                if (!Extensions.MatchTableNamingConstraints(value)) throw new InvalidElementNameException(value);
                _nameInStore = value;
            }
        }
        public KeyScheme KeyScheme { get; set; }
    }
}
