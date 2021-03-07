using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunePlugin
{
    public static class SWReference
    {
        private static Dictionary<string, string> monsterNameMap = null;
        
        public static Dictionary<string, string> MonsterNameMap
        {
            get
            {
                if (monsterNameMap == null && System.IO.File.Exists("data/monsterNames.json")) {
                    monsterNameMap = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText("data/monsterNames.json"));
                }
                return monsterNameMap;
            }
            set
            {
                if (monsterNameMap == null)
                    monsterNameMap = value;
            }
        }
    }
}
