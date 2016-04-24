using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim;

namespace RuneOptim
{
    // Per-stat filter
    public class RuneFilter
    {
        public int Flat = 0; // div flat by
        public int Percent = 0; // div percent by
        public int Test = 0; // sum both >= test

        // for debugging niceness
        public override string ToString()
        {
            return "/" + Flat + " + /" + Percent + "% >= " + Test;
        }
        
        // Gets the minimum divisor from A and B per type
        public static RuneFilter Min(RuneFilter a, RuneFilter b)
        {
            RuneFilter m = new RuneFilter();

            m.Flat = MinNZero(a.Flat, b.Flat);
            m.Percent = MinNZero(a.Percent, b.Percent);
            m.Test = MinNZero(a.Test, b.Test);

            return m;
        }

        // Returns the smaller int that's not zero
        private static int MinNZero(int a, int b)
        {
            if (a != 0)
            {
                if (b == 0)
                    return a;
                return (a < b ? a : b);
            }
            else if (b != 0)
                return b;

            return 0;
        }

        // speedy iterating
        public int this[string stat]
        {
            get
            {
                switch (stat)
                {
                    case "flat":
                        return Flat;
                    case "perc":
                        return Percent;
                }
                return Test;
            }
            set
            {
                switch (stat)
                {
                    case "flat":
                        Flat = value;
                        break;
                    case "perc":
                        Percent = value;
                        break;
                    case "test":
                        Test = value;
                        break;
                }
            }
        }

        // returns if this instance is non-zero
        [JsonIgnore]
        public bool NonZero
        {
            get
            {
                if (Flat != 0)
                    return true;
                if (Percent != 0)
                    return true;
                if (Test != 0)
                    return true;
                return false;
            }
        }
    }
}
