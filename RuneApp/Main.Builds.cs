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
using System.Windows.Forms;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;
using RuneOptim.Management;

namespace RuneApp {
    partial class Main {
        // TODO: shuffle these under custom controls?



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
                        lastLvi = buildList.Items.OfType<ListViewItem>().FirstOrDefault(ll => (ll.Tag as Build)?.ID == b.ID);
                    if (lastLvi == null)
                        return;
                    while (lastLvi.SubItems.Count < 4)
                        lastLvi.SubItems.Add("");
                    lastLvi.SubItems[3].Text = str;
                }
            });
        }

        private void RegenBuildList() {
            foreach (var lvi in buildList.Items.OfType<ListViewItem>()) {
                if (lvi.Tag is Build b) {
                    lvi.SubItems[1].Text = b.Priority.ToString();
                }
            }
        }

        public void RebuildBuildList() {
            List<ListViewItem> tempMons = null;
            this.Invoke((MethodInvoker)delegate {
                tempMons = dataMonsterList.Items.OfType<ListViewItem>().ToList();
                buildList.Items.Clear();
            });

            var lviList = new List<ListViewItem>();

            foreach (var b in Program.builds) {
                ListViewItem li = new ListViewItem();
                this.Invoke((MethodInvoker)delegate {
                    ListViewItemBuild(li, b);
                });
                lviList.Add(li);
                var lv1li = tempMons.FirstOrDefault(i => i.SubItems.OfType<ListViewItem.ListViewSubItem>().Any(s => s.Text == (b.Mon?.Id ?? b.MonId).ToString()));
                if (lv1li != null) {
                    lv1li.ForeColor = Color.Green;
                }
            }

            this.Invoke((MethodInvoker)delegate {
                buildList.Items.AddRange(lviList.ToArray());
                buildList.Sort();
            });
        }

    }
}
