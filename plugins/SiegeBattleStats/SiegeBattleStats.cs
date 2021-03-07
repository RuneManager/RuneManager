using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RunePlugin;

namespace SiegeBattleStats {
    public class SiegeBattleStats : SWPlugin {
        public override void ProcessRequest(object sender, SWEventArgs args) {

            if (args.Request.Command == SWCommand.GetGuildSiegeBattleLogByWizardId) {

            }

        }
    }
}
