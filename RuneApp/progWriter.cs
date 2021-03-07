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
#pragma warning disable CS0618 // Type or member is obsolete
            Program.log.Debug("progWriter: " + value);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
