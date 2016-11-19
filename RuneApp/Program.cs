using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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
            double av = src.Where(p => Math.Abs(selector(p)) > 0.00000001).Average(selector);
            List<double> nls = new List<double>();
            foreach (var o in src.Where(p => Math.Abs(selector(p)) > 0.00000001))
            {
                nls.Add((selector(o) - av)*(selector(o) - av));
            }
            double avs = nls.Average();
            return Math.Sqrt(avs);
        }

        public static T MakeControl<T>(this Control.ControlCollection ctrlC, string name, string suff, int x, int y, int w = 40, int h = 20, string text = null)
            where T : Control, new()
        {
            T ctrl = new T();
            ctrl.Name = name + suff;
            ctrl.Size = new Size(w, h);
            ctrl.Location = new Point(x, y);
            ctrl.Text = text;
            ctrlC.Add(ctrl);

            return ctrl;
        }

    }
}
