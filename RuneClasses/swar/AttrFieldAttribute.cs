using System;

namespace RuneOptim.swar {
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class AttrFieldAttribute : Attribute {
        readonly Attr attrName;

        public AttrFieldAttribute(Attr attr) {
            attrName = attr;
        }

        public Attr attr {
            get { return attrName; }
        }

    }
}
