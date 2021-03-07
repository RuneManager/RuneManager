using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RuneApp {
    class ControlHolder<T> where T : Control {
        Control parent;
        public ControlHolder(Control p) {
            parent = p;
        }

        public Dictionary<string, T> ctrls = new Dictionary<string, T>();
        public T this[string s] {
            get {
                var ctrl = ctrls.ContainsKey(s) ? ctrls[s] : null;
                if (ctrl == null) {
                    ctrl = parent.Controls.Find(s, true).FirstOrDefault() as T;
                }
                return ctrl;
            }
        }
    }
}
