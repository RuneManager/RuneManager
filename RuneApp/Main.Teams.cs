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
    partial class Main {

        // Teams are an experimental way of grouping together monsters, so that you can keep them together

        bool teamChecking;
        Build teamBuild;
        Dictionary<string, List<string>> toolmap = new Dictionary<string, List<string>>() {
            { "PvE", new List<string> { "Farmer", "World" } },
            { "Dungeon", new List<string> { "GB", "DB", "NB", "PC", "SF", "Secret", "HoH" } },
            { "Halls", new List<string> {"H-Magic", "H-Light", "H-Dark", "H-Fire", "H-Water", "H-Wind" } },
            { "Rift", new List<string> {"Raid", "R-Light", "R-Dark", "R-Fire", "R-Water", "R-Wind" } },
            { "PvP", new List<string> { "AO", "AD", "GWO", "GWD", "RTA", "SD", "SO"} },
            { "Lab", new List<string> { "Tartarus", "Leos", "Guilles", "Kotos", "L-Norm", "L-Resc", "L-Expl", "L-Cool", "L-SL", "L-TL" } },
            { "ToA", new List<string> { "ToAN", "ToAH", "ToAHell" } },
            { "2A", new List<string> { "Griffon", "Inugami", "Warbear", "High Elem", "Fairy", "Pixie", "Werewolf", "Martial Cat", "Howl", "Grim R" } },
            { "DimH", new List<string> { "Karzan", "Ellunia", "Lumel", "Khalderun" } }
        };

        List<string> knownTeams = new List<string>();


        private string getTeamStr(Build b) {
            if (b.Teams == null || b.Teams.Count == 0)
                return "";

            var sz = buildCHTeams.Width;
            var str = "";
            for (int i = 0; i < b.Teams.Count; i++) {
                var sb = new StringBuilder(string.Join(", ", b.Teams.Take(i)));
                if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    sb.Append(", ");
                sb.Append(b.Teams.Count - i);
                var tstr = string.Join(", ", b.Teams.Take(i + 1));
                if (this.CreateGraphics().MeasureString(tstr + "...", buildList.Font).Width > sz - 10)
                    return sb.ToString();
                str = tstr;
            }

            return str;
        }

        private void tsTeamAdd(ToolStripMenuItem parent, string item) {
            knownTeams.Add(item);
            ToolStripMenuItem n = new ToolStripMenuItem(item);
            parent.DropDownItems.Add(n);
            n.CheckedChanged += tsTeamHandler;
            n.CheckOnClick = true;

            if (toolmap[item] != null) {
                foreach (var smi in toolmap[item]) {
                    if (toolmap.ContainsKey(smi)) {
                        tsTeamAdd(n, smi);
                    }
                    else {
                        ToolStripMenuItem s = new ToolStripMenuItem(smi);
                        s.CheckedChanged += tsTeamHandler;
                        s.CheckOnClick = true;
                        n.DropDownItems.Add(s);
                    }
                }
            }
        }

        private bool tsTeamCheck(ToolStripMenuItem t) {
            bool ret = false;
            t.Checked = false;
            t.Image = null;
            if (teamBuild.Teams.Contains(t.Text)) {
                t.Checked = true;
                ret = true;
            }
            foreach (ToolStripMenuItem smi in t.DropDownItems) {
                if (tsTeamCheck(smi)) {
                    t.Image = App.add;
                    ret = true;
                }
            }
            return ret;
        }

        int GetRel(string first, string second) {
            if (first == second)
                return 0;

            string p1 = null;
            string p2 = null;

            if (toolmap.Keys.Contains(first) && toolmap.Keys.Contains(second))
                return 1;

            foreach (var k in toolmap) {
                if (k.Value.Contains(first) && k.Value.Contains(second))
                    return 1;
                if (k.Value.Contains(first))
                    p1 = k.Key;
                if (k.Value.Contains(second))
                    p2 = k.Key;
            }

            if (toolmap.Keys.Contains(first) && toolmap[first].Contains(second))
                return 1;
            if (toolmap.Keys.Contains(second) && toolmap[second].Contains(first))
                return 1;

            if (p2 != null && toolmap[p2].Contains(p1))
                return 2;
            if (p1 != null && toolmap[p1].Contains(p2))
                return 2;

            if (p2 != null && toolmap[p2].Contains(first))
                return 3;
            if (p1 != null && toolmap[p1].Contains(second))
                return 3;

            return -1;
        }

    }
}
