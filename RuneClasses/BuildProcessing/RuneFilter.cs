using Newtonsoft.Json;

namespace RuneOptim.BuildProcessing {
    // Per-stat filter
    public class RuneFilter {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? Flat = null; // div flat by
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? Percent = null; // div percent by
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? Test = null; // sum both >= test

        public RuneFilter() {

        }

        public RuneFilter(double? f = null, double? p = null, double? t = null) {
            Flat = f;
            Percent = p;
            Test = t;
        }

        public RuneFilter(RuneFilter rhs) {
            Flat = rhs.Flat;
            Percent = rhs.Percent;
            Test = rhs.Test;
        }

        // for debugging niceness
        public override string ToString() {
            return "/" + Flat + " + /" + Percent + "% >= " + Test;
        }

        public static RuneFilter Dominant(RuneFilter child, RuneFilter parent) {
            RuneFilter m = new RuneFilter();

            m.Flat = child.Flat ?? parent.Flat;
            m.Percent = child.Percent ?? parent.Percent;
            m.Test = child.Test ?? parent.Test;

            return m;
        }

        // Gets the minimum divisor from A and B per type
        public static RuneFilter Min(RuneFilter a, RuneFilter b) {
            RuneFilter m = new RuneFilter();

            m.Flat = MinNZero(a.Flat, b.Flat);
            m.Percent = MinNZero(a.Percent, b.Percent);
            m.Test = MinNZero(a.Test, b.Test);

            return m;
        }

        // Returns the smaller int that's not zero
        public static double? MinNZero(double? a, double? b) {
            if (a != null) {
                if (b == null)
                    return a;
                return a < b ? a : b;
            }
            else if (b != null)
                return b;

            return null;
        }

        // speedy iterating
        public double? this[string stat] {
            get {
                switch (stat) {
                    case "flat":
                        return Flat;
                    case "perc":
                        return Percent;
                    default:
                        return Test;
                }
            }
            set {
                switch (stat) {
                    case "flat":
                        Flat = value;
                        break;
                    case "perc":
                        Percent = value;
                        break;
                    case "test":
                        Test = value;
                        break;
                    default:
                        return;
                }
            }
        }

        // returns if this instance is non-zero
        [JsonIgnore]
        public bool NonZero {
            get {
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
