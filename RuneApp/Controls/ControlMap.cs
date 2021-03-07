using System.Windows.Forms;

namespace RuneApp {
    class ControlMap {
        Control parent;
        public ControlMap(Control p) {
            parent = p;
        }
        public ControlHolder<Control> Box;
    }
}
