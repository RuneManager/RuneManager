using System;
using System.Runtime.CompilerServices;

namespace RuneOptim.BuildProcessing {
    public class PrintToEventArgs : EventArgs {
        public Build build;
        public string Message;
        public long Order;

        public string File;
        public string Caller;
        public int Line;

        protected PrintToEventArgs(Build b, string m) { build = b; Message = m; }
        public static PrintToEventArgs GetEvent(Build b, string m,
            [CallerFilePath] string file = null,
            [CallerMemberName] string caller = null,
            [CallerLineNumber] int line = 0) {
            return new PrintToEventArgs(b, m) {
                File = file,
                Caller = caller,
                Line = line,
            };
        }
    }

}