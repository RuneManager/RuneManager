using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim.swar;

namespace RunePlugin.Response {
    [SWCommand(SWCommand.GetBestClearRiftDungeon)]
    public class GetBestClearRiftDungeonResponse : SWResponse {
        [JsonProperty("best_clear_rift_dungeon_list")]
        public BestRiftDeck[] BestClearRiftDungeons;

        [JsonProperty("bestdeck_rift_dungeon_list")]
        public RiftDeck[] BestDeckRiftDungeons;
    }
}
