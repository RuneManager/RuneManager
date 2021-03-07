using System;
using System.Runtime.CompilerServices;

namespace RuneOptim.BuildProcessing {
    public class ProgToEventArgs : EventArgs {
        public Build build;
        public long Progress;
        public double Percent;

        public string File;
        public string Caller;
        public int Line;

        protected ProgToEventArgs(Build b, double d, long p) { build = b; Percent = d; Progress = p; }
        public static ProgToEventArgs GetEvent(Build b, double d, long p,
            [CallerFilePath] string file = null,
            [CallerMemberName] string caller = null,
            [CallerLineNumber] int line = 0) {
            return new ProgToEventArgs(b, d, p) {
                File = file,
                Caller = caller,
                Line = line,
            };
        }
    }

}