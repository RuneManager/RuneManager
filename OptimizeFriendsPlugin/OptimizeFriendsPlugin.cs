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
				foreach (var friend in args.ResponseJson["friend_list"]) {
					var login = TimeSpan.FromSeconds(int.Parse(friend["last_login_time"].ToString()));
					if (login > TimeSpan.FromDays(10)) {
						Console.WriteLine("{0,-14} hasn't logged in for {1,-3} days, rep is lvl {2,-2} {3}",
							friend["wizard_name"],
							login.Days,
							long.Parse(friend["rep_unit_level"].ToString()),
							MonsterName(long.Parse(friend["rep_unit_master_id"].ToString()))
						);
					}
				}
			}
		}
	}
}
