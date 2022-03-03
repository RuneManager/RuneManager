using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using RuneOptim;
using RuneOptim.swar;

namespace RuneApp
{
    public static class AppExtensionMethods
    {

        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        public static double StandardDeviation<T>(this IEnumerable<T> src, Func<T, double> selector)
        {
            double av = src.Where(p => Math.Abs(selector(p)) > 0.00000001).Average(selector);
            List<double> nls = new List<double>();
            foreach (var o in src.Where(p => Math.Abs(selector(p)) > 0.00000001))
            {
                nls.Add((selector(o) - av) * (selector(o) - av));
            }
            double avs = nls.Average();
            return Math.Sqrt(avs);
        }

        public static T MakeControl<T>(this Control.ControlCollection ctrlC, Attr attr, string suff, int x, int y, int w = 40, int h = 20, string text = null)
            where T : Control, new()
        {
            return MakeControl<T>(ctrlC, attr.ToShortForm(), suff, x, y, w, h, text);
        }

        public static T MakeControl<T>(this Control.ControlCollection ctrlC, string name, string suff, int x, int y, int w = 40, int h = 20, string text = null)
            where T : Control, new()
        {
            T ctrl = new T()
            {
                Name = name + suff,
                Size = new Size(w, h),
                Location = new Point(x, y),
                Text = text
            };
            ctrlC.Add(ctrl);

            return ctrl;
        }

        //http://stackoverflow.com/a/77233
        public static void SetDoubleBuffered(this System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }
    }
}
