using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RuneOptim
{
    // Deserializes the .json into this
    public class Save
    {
        [JsonProperty("mons")]
        public IList<Monster> Monsters;

        [JsonProperty("deco_list")]
        public IList<Deco> Decorations;
        
        [JsonProperty("crafts")]
        public IList<Craft> Crafts;

        // Ask for monsters nicely
        public Monster GetMonster(string name)
        {
            return getMonster(name, 1);
        }

        public Monster GetMonster(string name, int num)
        {
            return getMonster(name, num);
        }

        private Monster getMonster(string name, int num)
        {
            Monster mon = Monsters.Where(m => m.Name == name).Skip(num - 1).FirstOrDefault();
            if (mon == null)
                mon = Monsters.Where(m => m.Name == name).FirstOrDefault();
            if (mon != null)
                return mon;
            return new Monster();
        }

        public Monster GetMonster(int id)
        {
            return Monsters.Where(m => m.ID == id).FirstOrDefault();
        }

        [JsonProperty("runes")]
        public IList<Rune> Runes;

        // builds from rune optimizer don't match mine.
        // Don't care right now, perhaps a fuzzy-import later?
        [JsonProperty("savedBuilds")]
        public IList<object> Builds;

        [JsonIgnore]
        public Stats shrines;

        public bool isModified = false;
    }
}
