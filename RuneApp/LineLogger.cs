using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RuneApp {
    public class LineLogger {
        private log4net.ILog logger;
        public LineLogger(log4net.ILog logger) {
            this.logger = logger;
        }
        
        [DebuggerStepThrough]
        protected string Bake(string str,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string filepath = null) {
            return System.IO.Path.GetFileNameWithoutExtension(filepath) + "." + caller + "@" + lineNumber + ": " + str;
        }

        [DebuggerStepThrough]
        public void Info(string str,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string filepath = null) {
            logger?.Info(Bake(str, lineNumber, caller, filepath));
        }

        [DebuggerStepThrough]
        public void Debug(string str,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string filepath = null) {
            logger?.Debug(Bake(str, lineNumber, caller, filepath));
        }

        [DebuggerStepThrough]
        public void Error(string str,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string filepath = null) {
            logger?.Error(Bake(str, lineNumber, caller, filepath));
        }

        [DebuggerStepThrough]
        public void Error(string str, Exception e,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string filepath = null) {
            logger?.Error(Bake(str, lineNumber, caller, filepath), e);
        }

        [DebuggerStepThrough]
        public void Fatal(string str,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string filepath = null) {
            logger?.Fatal(Bake(str, lineNumber, caller, filepath));
        }

        [DebuggerStepThrough]
        public void Fatal(string str, Exception e,
            [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = null,
            [CallerFilePath] string filepath = null) {
            logger?.Fatal(Bake(str, lineNumber, caller, filepath), e);
        }

    }
}
