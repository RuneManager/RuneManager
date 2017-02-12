using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace RuneOptim
{
    // Deserializes the .json into this
    public class Save
    {
        [JsonProperty("mons")]
        public readonly ObservableCollection<Monster> Monsters = new ObservableCollection<Monster>();

        [JsonProperty("deco_list")]
        public IList<Deco> Decorations;
        
        [JsonProperty("crafts")]
        public IList<Craft> Crafts;

        [JsonProperty("runes")]
        public readonly ObservableCollection<Rune> Runes = new ObservableCollection<Rune>();

        // builds from rune optimizer don't match mine.
        // Don't care right now, perhaps a fuzzy-import later?
        [JsonProperty("savedBuilds")]
        public IList<object> Builds;

        [JsonIgnore]
        public Stats shrines;

        public bool isModified = false;

        [JsonIgnore]
        int priority = 1;

        public Save()
        {
            Runes.CollectionChanged += Runes_CollectionChanged;
            Monsters.CollectionChanged += Monsters_CollectionChanged;
        }

        private void Monsters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (Monster mon in e.NewItems.Cast<Monster>())
            {
                var equipedRunes = Runes.Where(r => r.AssignedId == mon.ID);

                mon.inStorage = (mon.Name.IndexOf("In Storage") >= 0);
                mon.Name = mon.Name.Replace(" (In Storage)", "");

                mon.Current.Shrines = shrines;

                foreach (Rune rune in equipedRunes)
                {
                    mon.ApplyRune(rune);
                    rune.AssignedName = mon.Name;
                }

                if (mon.priority == 0 && mon.Current.RuneCount > 0)
                {
                    mon.priority = priority++;
                }

                var stats = mon.GetStats();
            }
        }

        private void Runes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var r in e.NewItems.Cast<Rune>())
            {
                r.FixShit();
            }
        }

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
    }
}
