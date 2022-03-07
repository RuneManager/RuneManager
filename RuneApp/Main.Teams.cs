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
            { "PvP", new List<string> { "AO", "AD", "GWO", "GWD", "RTA", "SO", "SD"} },
            { "Lab", new List<string> { "Tartarus", "Leos", "Guilles", "Kotos", "L-Norm", "L-Resc", "L-Expl", "L-Cool", "L-SL", "L-TL" } },
            { "ToA", new List<string> { "ToAN", "ToAH", "ToAHell" } },
            { "2A", new List<string> { "Griffon", "Inugami", "Warbear", "High Elem", "Fairy", "Pixie", "Werewolf", "Martial Cat", "Howl", "Grim R" } },
            { "DimH", new List<string> { "Karzan", "Ellunia", "Lumel", "Khalderun" } }
        };

        /// <summary>
        /// Compacts all teams into a string, truncating long entries
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add teams from toolmap to the UI element <c>parent</c>
        /// </summary>
        /// <param name="parent"></param>
        private void tsTeamsAdd(ToolStripMenuItem parent)
        {
            foreach (var item in toolmap)
            {
                ToolStripMenuItem n = tsTeamAdd(parent, item.Key);
                foreach (var smi in item.Value)
                {
                    tsTeamAdd(n, smi);
                }
            }
        }

        /// <summary>
        /// Add a toolstrip item to <c>parent</c> with value <c>value</c>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private ToolStripMenuItem tsTeamAdd(ToolStripMenuItem parent, string value) {
            ToolStripMenuItem n = new ToolStripMenuItem(value);
            parent.DropDownItems.Add(n);
            n.CheckedChanged += tsTeamHandler;
            n.CheckOnClick = true;
            return n;
        }

        /// <summary>
        /// Marks teams on Teams menu if it's included in TeamBuilds.Teams
        /// </summary>
        /// <param name="t"></param>
        /// <returns>bool indicating whether the item was found</returns>
        private bool tsTeamCheck(ToolStripMenuItem t) {
            bool ret = false;
            t.Checked = false;
            t.Image = null;
            foreach (ToolStripMenuItem smi in t.DropDownItems) {
                if (tsTeamCheck(smi)) {
                    // indicate that a nested team is in use
                    t.Image = App.add;
                    ret = true;
                }
            }
            if (teamBuild.Teams.Contains(t.Text))
            {
                // indicate that the team is in use
                t.Checked = true;
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Determines the amount of separation between two strings in the teams list.
        /// </summary>
        /// <param name="first">team name</param>
        /// <param name="second">team name</param>
        /// <returns>Degrees of separation between keys</returns>
        int GetRel(string first, string second) {
            // same
            if (first == second)
                return 0;

            // parent or child
            if (toolmap.Keys.Contains(first) && toolmap[first].Contains(second))
                return 1;
            if (toolmap.Keys.Contains(second) && toolmap[second].Contains(first))
                return 1;

            // siblings (top level)
            if (toolmap.Keys.Contains(first) && toolmap.Keys.Contains(second))
                return 1;

            // are either of the values nested?
            string parent_first = null;
            string parent_second = null;

            foreach (var k in toolmap) {
                if (k.Value.Contains(first))
                    parent_first = k.Key;
                if (k.Value.Contains(second))
                    parent_second = k.Key;
                // siblings (shortcutting loop)
                if (parent_first != null && parent_first == parent_second)
                    return 1;
            }

            // aunt/uncle
            if (parent_second != null && toolmap.Keys.Contains(first))
                return 2;
            if (parent_first != null && toolmap.Keys.Contains(second))
                return 2;

            // counsins
            if (parent_first != null && parent_second != null)
                return 3;

            // one or both not found
            return -1;
        }

    }
}
