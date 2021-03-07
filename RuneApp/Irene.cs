using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuneApp {
    public partial class Irene : Form {
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        private const int WH_MOUSE = 7;
        private const int WH_KEYBOARD = 2;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_H = 72;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static Irene instance = null;
        private Form main = null;
        public Form Main { get { return main; } }

        IreneAppearence currentAppearence = IreneAppearence.Talk;

        string lastHover = "";
        Timer hoverTimer = new Timer();
        int hoverSpecificIndex = -1;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point pnt);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public Irene(Form m) {
            main = m;
            InitializeComponent();
            this.BackColor = Color.Wheat;
            this.TransparencyKey = Color.Wheat;
            this.SetDoubleBuffered();
            //this.imageIrene.SetDoubleBuffered();
            ChangeAppearence(currentAppearence);

            cboxResponse.SelectedIndex = 0;
            hoverSpecificIndex = cboxResponse.Items.Add("Show help for Nothing");

            instance = this;
            _hookID = SetHook(_proc);
            // 15 fps?
            hoverTimer.Interval = 150;
            hoverTimer.Tick += HoverTimer_Tick;
        }

        private void imageIrene_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Irene_Shown(object sender, EventArgs e) {
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Right - this.Width, Screen.PrimaryScreen.WorkingArea.Bottom - this.Height);
            hoverTimer.Start();
        }

        private void Irene_Load(object sender, EventArgs e) {
        }

        private void btnOk_Click(object sender, EventArgs e) {
            if (cboxResponse.Visible) {
                switch (cboxResponse.SelectedItem.ToString()) {
                    case "Go away for now":
                        this.Hide();
                        break;
                    case "Leave until called":
                        this.Hide();
                        Program.Settings.ShowIreneOnStart = false;
                        Program.Settings.Save();
                        break;
                }
            }
        }

        private void richTextSpeech_LinkClicked(object sender, LinkClickedEventArgs e) {
            if (e.LinkText.Contains("[CBox]")) {
                var ss = e.LinkText.Substring(e.LinkText.IndexOf("[CBox]") + 6);
                int i = 0;
                if (int.TryParse(ss, out i)) {
                    cboxResponse.SelectedIndex = i;
                }
            }
            else if (e.LinkText.Contains("[Help]")) {
                var ss = e.LinkText.Substring(e.LinkText.IndexOf("[Help]") + 6);
                RuneApp.Main.help.Url = ss;
            }
            else if (e.LinkText.Contains("[Window]")) {
                var ss = e.LinkText.Substring(e.LinkText.IndexOf("[Window]") + 8);
                if (ss == "Options") {
                    var opt = new Options();
                    opt.StartPosition = FormStartPosition.Manual;
                    opt.Location = new Point(main.Location.X + main.Width / 2 - opt.Width / 2, main.Location.Y + main.Height / 2 - opt.Height / 2);
                    opt.ShowDialog();
                }
            }
            else {
                MessageBox.Show(e.LinkText);
            }
        }

        private void lnkResponse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {

        }

        private void HoverTimer_Tick(object sender, EventArgs e) {
            IntPtr hWnd = WindowFromPoint(Control.MousePosition);
            if (hWnd != IntPtr.Zero) {
                Control ctl = Control.FromHandle(hWnd);
                if (ctl != null && cboxResponse.SelectedItem.ToString() == "Explain what I point at") {
                    List<string> sb = new List<string>();
                    Control c = ctl;
                    for (; !(c is Form); c = c.Parent)
                        sb.Insert(0, c.Name);
                    sb.Insert(0, c.Name + ":");
                    if (lastHover != string.Join(":", sb)) {
                        lastHover = string.Join(":", sb);
                        richTextSpeech.Clear();
                        richTextSpeech.AppendText(lastHover);
                        richTextSpeech.AppendText(Environment.NewLine + Environment.NewLine + "Press H to Hold this Help.");
                    }
                }
            }
        }

        private void cboxResponse_SelectedIndexChanged(object sender, EventArgs e) {
            ChangeAppearence(IreneAppearence.Talk);
            /*
            Who are you?
            Explain what I point at
            Go away for now
            Leave until called
            */
            richTextSpeech.Clear();
            switch (cboxResponse.SelectedItem.ToString()) {
                case "Explain what I point at":
                    ChangeAppearence(IreneAppearence.Option);
                    break;
                case "Who are you?":
                    richTextSpeech.AppendText("My name is ");
                    richTextSpeech.AppendLink("Irene", "[Help]help/Irene.html");
                    richTextSpeech.AppendText(", you may have met my twin sister, Ellia." + Environment.NewLine
                        + "I am here to help you make the most of this RuneManager." + Environment.NewLine + Environment.NewLine
                        + "Try my ");
                    richTextSpeech.AppendLink("'point and help'", "[CBox]1");
                    richTextSpeech.AppendText(" mode!" + Environment.NewLine + "It's an good place to start.");
                    break;
                case "Go away for now":
                    richTextSpeech.AppendText("Well, see you next time I guess...");
                    ChangeAppearence(IreneAppearence.Ponder);
                    break;
                case "Leave until called":
                    richTextSpeech.AppendText("If you want to see me on start-up again, go to ");
                    richTextSpeech.AppendLink("File > Options", "[Window]Options");
                    richTextSpeech.AppendText("." + Environment.NewLine + Environment.NewLine);
                    richTextSpeech.AppendText("I cry everytime :(", Color.Gray);
                    ChangeAppearence(IreneAppearence.Mortified);
                    break;
                default:
                    if (cboxResponse.SelectedIndex == hoverSpecificIndex) {
                        richTextSpeech.Clear();
                        richTextSpeech.AppendText(lastHover);
                        richTextSpeech.AppendText(Environment.NewLine + Environment.NewLine + "Press H to Hold this Help.");
                    }
                    break;
            }
        }
        public void SendKey(int vkCode) {
            if (vkCode == VK_H) {
                cboxResponse.Items[hoverSpecificIndex] = "Show help for " + lastHover;
                cboxResponse.SelectedIndex = hoverSpecificIndex;
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc) {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) {
                int vkCode = Marshal.ReadInt32(lParam);
                instance.SendKey(vkCode);
            }
            //(IntPtr)(-1);// 
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void ChangeAppearence(IreneAppearence pic) {
            switch (pic) {
                case IreneAppearence.None:
                    break;
                case IreneAppearence.Talk: // Next FRR will be a breeze
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_01;
                    break;
                case IreneAppearence.Smile: // Today is a beautiful day to optimize runes!
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_02;
                    break;
                case IreneAppearence.Fight: // I won't just minimize quietly!
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_03;
                    break;
                case IreneAppearence.Ponder: // Maybe if you fed your Artamiel, that could work...
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_04;
                    break;
                case IreneAppearence.Option: // That's actually a pretty good Wind Bounty Hunter build
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_05;
                    break;
                case IreneAppearence.Shock: // I would never have asked, but I guess now I know.
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_06;
                    break;
                case IreneAppearence.Reject: // You know, I've reconsidered going out for coffee with you...
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_07;
                    break;
                case IreneAppearence.Mortified: // Who just eats monkey brains for dinner?
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_08;
                    break;
                case IreneAppearence.Angry: // Who are you calling Blondie!
                    imageIrene.Image = global::RuneApp.Properties.Resources.npc_irene_09;
                    break;
                default:
                    break;
            }
            currentAppearence = pic;
        }

    }

    public enum IreneAppearence {
        None,
        Talk,
        Smile,
        Fight,
        Ponder,
        Option,
        Shock,
        Reject,
        Mortified,
        Angry
    }
}
