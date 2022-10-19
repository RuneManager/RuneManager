using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using RuneOptim;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;
using RuneOptim.Management;

namespace RuneApp {
    public partial class Main {
        string filelink = "";
        string whatsNewLink = "";

        private Dictionary<string, List<ToolStripMenuItem>> shrineMap = new Dictionary<string, List<ToolStripMenuItem>>();

        bool loading = true;
        bool isClosing = false;

        public static Help Help;
        public static Irene Irene;

        public static bool GoodRunes { get { return Program.GoodRunes; } set { Program.GoodRunes = value; } }
        public static bool GoFast { get { return Program.GoFast; } set { Program.GoFast = value; } }
        public static bool FillRunes { get { return Program.FillRunes; } set { Program.FillRunes = value; } }

        public static Main CurrentMain;
        public static RuneDisplay RuneDisplay;
        Monster displayMon;

        
        public static Main Instance;

        public static LineLogger LineLog { [DebuggerStepThrough] get { return Program.LineLog; } }

        private ListViewItem lastFocused = null;

        private static readonly Control[] statCtrls = new Control[37];


        public Main() {
            InitializeComponent();
            LineLog.Info("Initialized Main");
            Instance = this;

            CurrentMain = this;

            useRunesCheck.Checked = Program.Settings.UseEquipped;

            findGoodRunes.Enabled = Program.Settings.MakeStats;
            if (!Program.Settings.MakeStats)
                findGoodRunes.Checked = false;

            #region Update

            if (Program.Settings.CheckUpdates) {
                checkForUpdates();
            }
            else {
                updateBox.Show();
                LineLog.Info("Updates Disabled");
                updateComplain.Text = "Updates Disabled";
                var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
                string oldvernum = ver.ProductVersion;
                updateCurrent.Text = "Current: " + oldvernum;
                updateNew.Text = "";
            }

            #endregion

            #region Labels

            int yStart = 410;

            groupBox1.Controls.Add(new Label {
                Location = new Point(4 + 50, yStart - 18),
                Name = "compBefore",
                Text = "Before",
                Size = new Size(50, 14)
            });

            groupBox1.Controls.Add(new Label {
                Location = new Point(4 + 100, yStart - 18),
                Name = "compAfter",
                Text = "After",
                Size = new Size(50, 14)
            });

            groupBox1.Controls.Add(new Label {
                Location = new Point(4 + 150, yStart - 18),
                Name = "compDiff",
                Text = "Difference",
                Size = new Size(60, 14)
            });

            int xx = 0;
            int yy = 0;
            var labelPrefixes = new string[] { "Pts" }.Concat(Build.StatNames).Concat(Build.ExtraNames);
            foreach (var s in labelPrefixes) {
                groupBox1.Controls.MakeControl<Label>(s, "compStat", 4 + xx, yStart + yy, 50, 14, s);
                xx += 50;

                groupBox1.Controls.MakeControl<Label>(s, "compBefore", 4 + xx, yStart + yy, 50, 14, "");
                xx += 50;

                groupBox1.Controls.MakeControl<Label>(s, "compAfter", 4 + xx, yStart + yy, 50, 14, "");
                xx += 50;

                groupBox1.Controls.MakeControl<Label>(s, "compDiff", 4 + xx, yStart + yy, 150, 14, "");

                if (s == "SPD")
                    yy += 4;
                if (s == "ACC")
                    yy += 8;
                if (s == "MxD")
                    yy += 8;

                yy += 16;
                xx = 0;
            }

            for (int i = 0; i < 4; i++) {
                groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compStat", 4 + xx, yStart + yy, 50, 14, "Skill" + (i + 1));
                xx += 50;

                groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compBefore", 4 + xx, yStart + yy, 50, 14, "");
                xx += 50;

                groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compAfter", 4 + xx, yStart + yy, 50, 14, "");
                xx += 50;

                groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compDiff", 4 + xx, yStart + yy, 150, 14, "");

                yy += 16;
                xx = 0;
            }

            #endregion



            #region DoubleBuffered and Sort
            this.SetDoubleBuffered();
            buildList.SetDoubleBuffered();
            viewMonsterList.SetDoubleBuffered();
            viewRuneList.SetDoubleBuffered();
            viewCraftList.SetDoubleBuffered();
            loadoutList.SetDoubleBuffered();

            buildList.ListViewItemSorter = null;
            viewMonsterList.ListViewItemSorter = null;
            viewRuneList.ListViewItemSorter = null;
            viewCraftList.ListViewItemSorter = null;
            loadoutList.ListViewItemSorter = null;
            #endregion

        }

    }
}
