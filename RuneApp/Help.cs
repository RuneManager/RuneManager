using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuneApp
{
    public partial class Help : Form
    {
        public string url = null;

        public Help()
        {
            InitializeComponent();
            webBrowser1.Navigated += WebBrowser1_Navigated;

            this.Shown += Help_Shown;
        }

        private void Help_Shown(object sender, EventArgs e)
        {
            if (url == null)
                url = Environment.CurrentDirectory + "\\User Manual\\index.html";

            webBrowser1.Navigate(url);
        }

        private void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //toolStripButton1.Image = App.left;
            //if (!webBrowser1.CanGoBack)
            //    toolStripButton1.Image = null;
            ////toolStripButton2.Image = App.right;
            //if (!webBrowser1.CanGoForward)
            //    toolStripButton2.Image = null;


            toolStripButton1.Enabled = webBrowser1.CanGoBack;
            toolStripButton2.Enabled = webBrowser1.CanGoForward;

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
