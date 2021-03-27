using System.IO;
using System.Text;

namespace RuneApp {
    class progWriter : TextWriter {
        public override Encoding Encoding {
            get {
                return Encoding.Default;
            }
        }

        public override void WriteLine(string value) {
            Program.LineLog.Debug("progWriter: " + value);
        }
    }
}
