using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RuneOptim
{
    // Deserializes the .json into this
    public class Save
    {
        [JsonProperty("unit_list")]
        public readonly ObservableCollection<Monster> Monsters = new ObservableCollection<Monster>();

        [JsonProperty("deco_list")]
        public readonly ObservableCollection<Deco> Decorations = new ObservableCollection<Deco>();
        
        [JsonProperty("crafts")]
        public readonly ObservableCollection<Craft> Crafts = new ObservableCollection<Craft>();

        [JsonProperty("runes")]
        public readonly ObservableCollection<Rune> Runes = new ObservableCollection<Rune>();

        // builds from rune optimizer don't match mine.
        // Don't care right now, perhaps a fuzzy-import later?
        [JsonProperty("savedBuilds")]
        public IList<object> Builds;

        [JsonIgnore]
        public readonly Stats shrines = new Stats();

        public bool isModified = false;

		[JsonIgnore]
		Dictionary<int, string> monIdNames;

		[JsonIgnore]
        int priority = 1;

        public Save()
        {
            monIdNames = JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText("monsters.json"));
            Runes.CollectionChanged += Runes_CollectionChanged;
            Monsters.CollectionChanged += Monsters_CollectionChanged;
            Decorations.CollectionChanged += Decorations_CollectionChanged;
        }

        private void Decorations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var shr in e.NewItems.Cast<Deco>().Where(d => d.Shrine != Shrine.Unknown))
                    {
                        var i = Deco.ShrineStats.ToList().IndexOf(shr.Shrine.ToString());
                        shrines[shr.Shrine.ToString()] = Math.Ceiling(shr.Level * Deco.ShrineLevel[i]);
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                default:
                    throw new NotImplementedException();
            }
        }

        private void Monsters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (Monster mon in e.NewItems.Cast<Monster>())
                    {
						mon.Name = monIdNames.FirstOrDefault(m => m.Key == mon._monsterTypeId).Value;
						if (mon.Name == null)
						{
							mon.Name = mon._attribute + " " + monIdNames.FirstOrDefault(m => m.Key == mon._monsterTypeId / 100).Value;
						}
						foreach (var r in mon.Runes)
						{
							r.Assigned = mon;
							Runes.Add(r);
						}
						var equipedRunes = Runes.Where(r => r.AssignedId == mon.Id);

						/*

                        mon.inStorage = (mon.Name.IndexOf("In Storage") >= 0);
                        mon.Name = mon.Name.Replace(" (In Storage)", "");
						*/
						mon.Current.Shrines = shrines;

                        foreach (Rune rune in equipedRunes)
                        {
                            mon.ApplyRune(rune);
                            rune.AssignedName = mon.Name;
                        }

                        if (mon.priority == 0 && mon.Current.RuneCount > 0)
                        {
                            //mon.priority = priority++;
                        }

                        var stats = mon.GetStats();
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                default:
                    throw new NotImplementedException();
            }
            
        }

        private void Runes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var r in e.NewItems.Cast<Rune>())
                    {
						foreach (var s in r.Subs)
						{
							if (s.__int2 != 0)
								throw new Exception("What is this?");
						}
						r.FixShit();
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                default:
                    throw new NotImplementedException();
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
            Monster mon = Monsters.Skip(num - 1).FirstOrDefault(m => m.Name == name);
            if (mon == null)
                mon = Monsters.FirstOrDefault(m => m.Name == name);
            if (mon != null)
                return mon;
            return new Monster();
        }

        public Monster GetMonster(ulong id)
        {
            return Monsters.FirstOrDefault(m => m.Id == id);
        }
    }
}
