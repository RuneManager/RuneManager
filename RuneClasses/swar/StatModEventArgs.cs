using System;

namespace RuneOptim.swar {
    public class StatModEventArgs : EventArgs {
        public Attr Attr { get; private set; }
        public double Value { get; private set; }

        public StatModEventArgs(Attr a, double v) {
            Attr = a;
            Value = v;
        }
    }
}
