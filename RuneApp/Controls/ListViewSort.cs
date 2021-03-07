using System;
using System.Collections;
using System.Windows.Forms;


namespace RuneApp {
    // I remember taking this from somewhere on stackoverflow maybe
    class ListViewSort : IComparer {
        // which column/subitem to sort on
        int sortPrimary = -1;
        int sortSecondary = -1;
        // which direction to sort it
        public bool orderPrimary = true;
        public bool orderSecondary = true;

        public bool ShouldSort = true;

        // Compare two ListViewItems
        public int Compare(object a, object b) {
            if (!ShouldSort)
                return 0;

            ListViewItem lhs = (ListViewItem)a;
            ListViewItem rhs = (ListViewItem)b;

            if (sortPrimary == -1)
                return 0;

            // Okay so this will attempt to turn strings into numbers eg. "435" -> 435, "32 (124)" -> 32
            // this is to help sort the Generate scoring columns, otherwise it will sort by string

            string val1 = lhs.SubItems[sortPrimary].Text;
            if (val1.EndsWith("%"))
                val1 = val1.Replace("%", "");
            string val2 = rhs.SubItems[sortPrimary].Text;
            if (val2.EndsWith("%"))
                val2 = val2.Replace("%", "");

            int val1sp = val1.IndexOf(' ');
            int val2sp = val2.IndexOf(' ');

            string val1i = val1;
            string val2i = val2;
            if (val1sp != -1)
                val1i = val1.Substring(0, val1sp);
            if (val2sp != -1)
                val2i = val2.Substring(0, val2sp);

            if (val1 == "" && val2 != "")
                return 1;
            else if (val1 != "" && val2 == "")
                return -1;

            // Upgraded to doubles because that's what's scoring.
            // Problem: still have to return an int (that is slightly representative of the level of difference)
            // Single value: Return -1 or lower
            // Double values: Return the sign of the comparison unless the abs is > 1

            double comp;
            double val, valc;
            if (double.TryParse(val1i, out val)) {
                if (val2 == "" || !double.TryParse(val2i, out valc))
                    return -(int)Math.Max(1, val);
                comp = val - valc;
            }
            else {
                comp = String.Compare(val1, val2, StringComparison.Ordinal);
            }
            if (Math.Abs(comp) > 0.00000001)
                return Math.Sign(comp) * (int)Math.Max(1, Math.Abs(comp)) * (orderPrimary ? 1 : -1);

            if (sortSecondary == -1)
                return 0;

            string val3 = lhs.SubItems[sortSecondary].Text;
            string val4 = rhs.SubItems[sortSecondary].Text;

            int val3sp = val1.IndexOf(' ');
            int val4sp = val2.IndexOf(' ');

            string val3i = val3;
            string val4i = val4;
            if (val3sp != -1)
                val3i = val1.Substring(0, val3sp);
            if (val4sp != -1)
                val4i = val2.Substring(0, val4sp);

            if (double.TryParse(val3i, out val)) {
                if (val4 == "" || !double.TryParse(val4i, out valc))
                    return -(int)Math.Max(1, val);
                comp = val - valc;
            }
            else {
                comp = String.Compare(val3, val4, StringComparison.Ordinal);
            }

            return Math.Sign(comp) * (int)Math.Max(1, Math.Abs(comp)) * (orderSecondary ? 1 : -1);
        }

        public void OrderBy(int column, bool ascend = true) {
            sortPrimary = column;
            orderPrimary = ascend;
        }

        public void ThenBy(int column, bool ascend = true) {
            sortSecondary = column;
            orderSecondary = ascend;
        }

        // When the lists column is clicked
        public void OnColumnClick(int column, bool ascend = true, bool force = false) {
            // if we don't care about fancy stuff
            if (force) {
                sortPrimary = column;
                orderPrimary = ascend;
                return;
            }

            if (column == sortPrimary)
                orderPrimary = !orderPrimary;
            else {
                // seems okay, right?
                orderSecondary = orderPrimary;
                sortSecondary = sortPrimary;
                sortPrimary = column;
                orderPrimary = ascend;
            }
        }
    }
}
