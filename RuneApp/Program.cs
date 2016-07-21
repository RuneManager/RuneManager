using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using RuneOptim;
using System.Diagnostics;

namespace RuneApp
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        public static double StandardDeviation<T>(this IEnumerable<T> src, Func<T, double> selector)
        {
            double av = src.Where(p => selector(p) != 0).Average(selector);
            List<double> nls = new List<double>();
            foreach (var o in src.Where(p => selector(p) != 0))
            {
                nls.Add((selector(o) - av)*(selector(o) - av));
            }
            double avs = nls.Average();
            return Math.Sqrt(avs);
        }

    }
}
