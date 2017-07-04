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

		[JsonProperty("inventory_info")]
		public readonly ObservableCollection<InventoryItem> InventoryItems = new ObservableCollection<InventoryItem>();

		[JsonProperty("unit_lock_list")]
		public readonly ObservableCollection<ulong> LockedUnits = new ObservableCollection<ulong>();

		// builds from rune optimizer don't match mine.
		// Don't care right now, perhaps a fuzzy-import later?
		[JsonProperty("savedBuilds")]
		public IList<object> Builds;

		[JsonIgnore]
		public readonly Stats shrines = new Stats();

		public bool isModified = false;

		[JsonIgnore]
		static Dictionary<int, string> monIdNames = null;

		public static Dictionary<int, string> MonIdNames
		{
			get
			{
				if (monIdNames == null)
					monIdNames = JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText(global::RuneOptim.Properties.Resources.MonstersJSON));
				return monIdNames;
			}
		}

		[JsonIgnore]
		int priority = 1;

		[JsonIgnore]
		private int monLoaded = 0;

		public Save()
		{
			Runes.CollectionChanged += Runes_CollectionChanged;
			Monsters.CollectionChanged += Monsters_CollectionChanged;
			Decorations.CollectionChanged += Decorations_CollectionChanged;
			LockedUnits.CollectionChanged += LockedUnits_CollectionChanged;
		}
		
		private void LockedUnits_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					foreach (var i in e.NewItems.Cast<ulong>())
					{
						var mon = GetMonster(i);
						if (mon != null)
							mon.Locked = true;
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
						mon.Name = MonIdNames.FirstOrDefault(m => m.Key == mon._monsterTypeId).Value;
						mon.loadOrder = monLoaded++;
						if (mon.Name == null)
						{
							mon.Name = mon._attribute + " " + MonIdNames.FirstOrDefault(m => m.Key == mon._monsterTypeId / 100).Value;
						}
						foreach (var r in mon.Runes)
						{
							r.Assigned = mon;
							Runes.Add(r);
						}
						var equipedRunes = Runes.Where(r => r.AssignedId == mon.Id);
						
						mon.Current.Shrines = shrines;

						foreach (Rune rune in equipedRunes)
						{
							mon.ApplyRune(rune);
							rune.AssignedName = mon.Name;
						}

						if (LockedUnits.Contains(mon.Id))
							mon.Locked = true;
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
								throw new Exception("What is this?\r\nPlease give me your savefile :)");
						}
						r.PrebuildAttributes();
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
			if (!System.Diagnostics.Debugger.IsAttached)
				RuneLog.Debug("GetMonster " + num + "th " + name + " from " + Monsters.Count);
			Monster mon = Monsters.Skip(num - 1).FirstOrDefault(m => m.Name == name);
			if (mon == null)
				mon = Monsters.FirstOrDefault(m => m.Name == name);
			if (mon != null)
				return mon;
			return null;
		}

		public Monster GetMonster(ulong id)
		{
			if (!System.Diagnostics.Debugger.IsAttached)
				RuneLog.Debug("GetMonster " + id + " from " + Monsters.Count);
			return Monsters.FirstOrDefault(m => m.Id == id);
		}
	}
}
