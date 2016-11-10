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
        private int? val = null;
        private bool? isMain = null;

        public RuneStat(Rune p, Attr s)
        {
            parent = p;
            stat = s;
        }

        public int this[int fake, bool pred]
        {
            get
            {
                if ((val == null) || (isMain ?? true) || pred) 
                    return parent.GetValue(stat, fake, pred);
                return val ?? 0;
            }
        }

        public int Value
        {
            get
            {
                if (val == null)
                {
                    val = parent.GetValue(stat, -1, false);
                    if (parent.MainType == stat)
                        isMain = true;
                }
                
                return val.Value;
            }
        }

        public int ValueGet()
        {
            return this.Value;
        }

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
