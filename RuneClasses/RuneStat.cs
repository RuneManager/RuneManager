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
                    val = parent.GetValue(stat);
                    if (parent.MainType == stat)
                        isMain = true;
                }
                
                return val.Value;
            }
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
