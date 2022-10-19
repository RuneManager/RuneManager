﻿using System;
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
    partial class Main {
        // TODO: shuffle these under custom controls?

        public IEnumerable<ListViewItem> BuildListViewItems => buildList.Items.OfType<ListViewItem>();

        private DateTime resumeTime = DateTime.MinValue;

        private System.Windows.Forms.Timer resumeTimer;

        private void RunBuild(ListViewItem pli, bool saveStats = false) {
            if (pli?.Tag is Build)
                Program.RunBuild((Build)pli.Tag, saveStats);
        }

        private void ListViewItemBuild(ListViewItem lvi, Build b) {
            lvi.Text = b.ID.ToString();

            while (lvi.SubItems.Count < 6)
                lvi.SubItems.Add("");

            int i = 0;
            lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, b.Mon?.FullName ?? b.MonName);
            lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, b.Priority.ToString());
            lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, b.ID.ToString());
            var bs = "";
            if (b.BuildStrategy == null || b.BuildStrategy.Contains("Bad"))
                bs = "!";
            lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, bs);
            lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, (b.Mon?.Id ?? b.MonId).ToString());
            if (b.Type == BuildType.Lock) {
                lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, "Locked");
                lvi.ForeColor = Color.Gray;
            }
            else if (b.Type == BuildType.Link) {
                lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, "Linked");
                lvi.ForeColor = Color.Teal;
            }
            else
                lvi.SubItems[i++] = new ListViewItem.ListViewSubItem(lvi, getTeamStr(b));

            lvi.Tag = b;

            

            if (b.RunePrediction.Any(p => p.Value.Value))
                lvi.ForeColor = Color.Purple;
        }

        ListViewItem lastLvi;

        public void ProgressToList(Build b, string str) {
            if (isClosing) return;
            //Program.log.Info("_" + str);
            this.Invoke((MethodInvoker)delegate {
                if (!IsDisposed) {
                    if (lastLvi == null || (lastLvi.Tag as Build)?.ID != b.ID)
                        lastLvi = buildList.Items.OfType<ListViewItem>().FirstOrDefault(bli => (bli.Tag as Build)?.ID == b.ID);
                    if (lastLvi == null)
                        return;
                    while (lastLvi.SubItems.Count < 4)
                        lastLvi.SubItems.Add("");
                    lastLvi.SubItems[3].Text = str;
                }
            });
        }

        private void RefreshBuildPriority() {
            foreach (var bli in buildList.Items.OfType<ListViewItem>()) {
                if (bli.Tag is Build b) {
                    bli.SubItems[1].Text = b.Priority.ToString();
                }
            }
        }

        public void RebuildBuildList() {
            List<ListViewItem> tempMons = null;
            this.Invoke((MethodInvoker)delegate {
                tempMons = viewMonsterList.Items.OfType<ListViewItem>().ToList();
                buildList.Items.Clear();
            });

            var blis = new List<ListViewItem>();

            foreach (var build in Program.Builds) {
                ListViewItem bli = new ListViewItem();
                this.Invoke((MethodInvoker)delegate {
                    ListViewItemBuild(bli, build);
                });
                blis.Add(bli);
                var mli = tempMons.FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == (build.Mon?.Id ?? build.MonId).ToString()));
                if (mli != null) {
                    mli.ForeColor = Color.Green;
                }
            }

            this.Invoke((MethodInvoker)delegate {
                buildList.Items.AddRange(blis.ToArray());
                buildList.Sort();
            });
        }

        /// <summary>
        /// Refresh the build list UI from the attached build
        /// </summary>
        public void RefreshBuildList()
        {
            // TODO: Combine with RebuildBuildList?
            foreach (ListViewItem bli in buildList.Items)
            {
                if (bli.Tag is Build b)
                    ListViewItemBuild(bli, b);
            }

        }

    }
}
