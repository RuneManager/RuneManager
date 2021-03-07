using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RunePlugin;

namespace OptimizeFriendsPlugin {
    public class OptimizeFriendsPlugin : SWPlugin {
        public override void ProcessRequest(object sender, SWEventArgs args) {
            if (((string)args.ResponseJson["command"]).Equals("GetFriendList")) {
                foreach (var friend in args.ResponseJson["friend_list"].OrderBy(f => f["last_login_time"])) {
                    var lastlogin = TimeSpan.FromSeconds(int.Parse(friend["last_login_time"].ToString()));
                    var replevel = long.Parse(friend["rep_unit_level"].ToString());
                    if ((lastlogin > TimeSpan.FromDays(10)) || (replevel < 30)) {
                        Console.WriteLine("{0,-14} hasn't logged in for {1,3} days, rep is lvl {2,2} {3}",
                            friend["wizard_name"],
                            lastlogin.Days,
                            replevel,
                            MonsterName(long.Parse(friend["rep_unit_master_id"].ToString()))
                        );
                    }
                }
            }
        }
    }
}
