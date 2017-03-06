using System;
using System.Linq;
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

            Shown += Help_Shown;
        }

        private void Help_Shown(object sender, EventArgs e)
        {
            if (url == null)
                url = Environment.CurrentDirectory + "\\User Manual\\index.html";

            webBrowser1.Navigate(url);

            if (Main.config.AppSettings.Settings.AllKeys.Contains("startuphelp"))
            {
                bool startuphelp;
                if (bool.TryParse(Main.config.AppSettings.Settings["startuphelp"].Value, out startuphelp))
                {
                    showOnStartupToolStripMenuItem.Checked = startuphelp;
                }
            }
        }

        private void WebBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
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
            Close();
        }

        private void showOnStartupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool startuphelp = !showOnStartupToolStripMenuItem.Checked;
            Main.config.AppSettings.Settings.Remove("startuphelp");
            Main.config.AppSettings.Settings.Add("startuphelp", startuphelp.ToString());
            Main.config.Save(System.Configuration.ConfigurationSaveMode.Modified);
            showOnStartupToolStripMenuItem.Checked = startuphelp;
        }

        public void SetStartupCheck(bool state)
        {
            showOnStartupToolStripMenuItem.Checked = state;
        }
    }
}
