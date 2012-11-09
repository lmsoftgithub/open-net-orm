using System;

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

        public string NameInStore { get; set; }
        public KeyScheme KeyScheme { get; set; }
    }
}
