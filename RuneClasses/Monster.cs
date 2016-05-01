using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RuneOptim
{
    // The monster stores its base stats in its base class
    public class Monster : Stats
    {
        [JsonProperty("name")]
        public string Name = "Missingno";

        [JsonProperty("id")]
        public int ID = -1;
        
        [JsonProperty("level")]
        public int level = 1;

        public int priority = 0;

        public int SwapCost(Loadout l)
        {
            int cost = 0;
            for (int i = 0; i < 6; i++)
            {
                if (l.runes[i].AssignedName != Name)
                {
                    if (l.runes[i].AssignedName != "Unknown name")
                        cost += l.runes[i].UnequipCost;
                    cost += Current.runes[i].UnequipCost;
                }
            }
            return cost;
        }

        // what is currently equiped for this instance of a monster
        public Loadout Current = new Loadout();

        public Monster()
        {
        }

        // copy down!
        public Monster(Monster rhs) : base(rhs)
        {
            Name = rhs.Name;
            ID = rhs.ID;
            level = rhs.level;
        }

        // put this rune on the current build
        public void ApplyRune(Rune rune)
        {
            Current.AddRune(rune);
        }

        // get the stats of the current build.
        // NOTE: the monster will contain it's base stats
        public Stats GetStats()
        {
            return Current.GetStats(this);
        }

        // NYI comparison
        public EquipCompare CompareTo(Monster rhs)
        {
            if (Loadout.CompareSets(Current.sets, rhs.Current.sets) == 0)
                return EquipCompare.Unknown;

            Stats a = GetStats();
            Stats b = rhs.GetStats();

            if (a.Health <= b.Health)
                return EquipCompare.Worse;
            if (a.Attack <= b.Attack)
                return EquipCompare.Worse;
            if (a.Defense <= b.Defense)
                return EquipCompare.Worse;
            if (a.Speed <= b.Speed)
                return EquipCompare.Worse;
            if (a.CritRate <= b.CritRate)
                return EquipCompare.Worse;
            if (a.CritDamage <= b.CritDamage)
                return EquipCompare.Worse;
            if (a.Accuracy <= b.Accuracy)
                return EquipCompare.Worse;
            if (a.Resistance <= b.Resistance)
                return EquipCompare.Worse;

            return EquipCompare.Better;
        }
    }
}
