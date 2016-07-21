using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneOptim
{
    public class RuneStat
    {
        private Rune parent = null;
        private Attr stat = Attr.Null;

        public RuneStat(Rune p, Attr s)
        {
            parent = p;
            stat = s;
        }

        public int this[int fake, bool pred]
        {
            get
            {
                return parent.GetValue(stat, fake, pred);
            }
        }

        public int Value { get { return parent.GetValue(stat, -1, false); } }

        public static implicit operator int (RuneStat rhs)
        {
            return rhs.Value;
        }

        public override string ToString()
        {
            return stat + " " + Value;
        }
    }
}
