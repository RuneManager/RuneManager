namespace RuneOptim
{
    public class RuneStat
    {
        private Rune parent = null;
        private Attr stat;
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
                return parent.GetValue(stat, fake, pred);
                if (val == null)
                {
                    val = parent.GetValue(stat);
                    isMain = parent.MainType == stat;
                }

                if ((isMain ?? true) || pred) 
                    return parent.GetValue(stat, fake, pred);
                return val ?? 0;
            }
        }

        public int Value
        {
            get
            {
                return parent.GetValue(stat);
                if (val == null)
                {
                    val = parent.GetValue(stat);
                    isMain = parent.MainType == stat;
                }
                
                return val.Value;
            }
        }

        public void SetVals(int v, bool m)
        {
            val = v;
            isMain = m;
        }

        public int ValueGet()
        {
            return Value;
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
