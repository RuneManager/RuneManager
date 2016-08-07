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
        public double? Flat = null; // div flat by
        public double? Percent = null; // div percent by
        public double? Test = null; // sum both >= test

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
        private static double? MinNZero(double? a, double? b)
        {
            if (a != null)
            {
                if (b == null)
                    return a;
                return (a < b ? a : b);
            }
            else if (b != null)
                return b;

            return null;
        }

        // speedy iterating
        public double? this[string stat]
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
                if (Flat != null)
                    return true;
                if (Percent != null)
                    return true;
                if (Test != null)
                    return true;
                return false;
            }
        }
    }
}
