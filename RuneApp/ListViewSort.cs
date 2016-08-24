using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RuneApp
{
    // I remember taking this from somewhere on stackoverflow maybe
    class ListViewSort : IComparer
    {
        // which column/subitem to sort on
        int sortPrimary = -1;
        int sortSecondary = -1;
        // which direction to sort it
        public bool orderPrimary = true;
        public bool orderSecondary = true;

        // Compare two ListViewItems
        public int Compare(object a, object b)
        {
            ListViewItem lhs = (ListViewItem)a;
            ListViewItem rhs = (ListViewItem)b;

            if (sortPrimary == -1)
                return 0;
            
            // Okay so this will attempt to turn strings into numbers eg. "435" -> 435, "32 (124)" -> 32
            // this is to help sort the Generate scoring columns, otherwise it will sort by string

            string val1 = lhs.SubItems[sortPrimary].Text;
            string val2 = rhs.SubItems[sortPrimary].Text;

			int val1sp = val1.IndexOf(' ');
			int val2sp = val2.IndexOf(' ');

			string val1i = val1;
			string val2i = val2;
			if (val1sp != -1)
				val1i = val1.Substring(0, val1sp);
			if (val2sp != -1)
				val2i = val2.Substring(0, val2sp);

			// Upgraded to doubles because that's what's scoring.
			// Problem: still have to return an int (that is slightly representative of the level of difference)
			// Single value: Return -1 or lower
			// Double values: Return the sign of the comparison unless the abs is > 1

			double comp;
			double val, valc;
            if (double.TryParse(val1i, out val))
            {
                if (val2 == "" || !double.TryParse(val2i, out valc))
                    return -(int)Math.Max(1, val);
                comp = val - valc;
            }
            else
            {
                comp = val1.CompareTo(val2);
            }
            if (comp != 0)
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

            if (double.TryParse(val3, out val))
            {
                if (val4 == "" || !double.TryParse(val4i, out valc))
                    return -(int)Math.Max(1, val);
                comp = val - valc;
            }
            else
            {
                comp = val3.CompareTo(val4);
            }

            return Math.Sign(comp) * (int)Math.Max(1, Math.Abs( comp)) * (orderSecondary ? 1 : -1);
        }

        // When the lists column is clicked
        public void OnColumnClick(int column, bool ascend = true, bool force = false)
        {
            // if we don't care about fancy stuff
			if (force)
			{
				sortPrimary = column;
				orderPrimary = ascend;
				return;
			}

            if (column == sortPrimary)
                orderPrimary = !orderPrimary;
            else
            {
                // seems okay, right?
                orderSecondary = orderPrimary;
                sortSecondary = sortPrimary;
                sortPrimary = column;
                orderPrimary = ascend;
            }
        }
    }
}
