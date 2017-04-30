using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim;

namespace RuneOptimSWF
{
    public class SWarFarmSave
    {
        [JsonProperty("unit_list")]
        public readonly ObservableCollection<SWFMonster> Monsters = new ObservableCollection<SWFMonster>();

        [JsonProperty("deco_list")]
        public IList<Deco> Decorations;

        [JsonProperty("runes")]
        public readonly ObservableCollection<SWFRune> Runes = new ObservableCollection<SWFRune>();

        [JsonIgnore]
        Dictionary<int, string> monIdNames;
        
        public SWarFarmSave()
        {
            monIdNames = JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText("monsters.json"));
            Monsters.CollectionChanged += Monsters_CollectionChanged;
            Runes.CollectionChanged += Runes_CollectionChanged;
        }

        private void Runes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var r in e.NewItems.Cast<SWFRune>())
            {
                foreach (var s in r.Subs)
                {
                    if (s.__int2 != 0)
                        throw new Exception("What is this?");
                }
            }
        }

        private void Monsters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var mon in e.NewItems.Cast<SWFMonster>())
            {
                mon.Name = monIdNames.FirstOrDefault(m => m.Key == mon._monsterTypeId).Value;
                if (mon.Name == null)
                {
                    mon.Name = mon._attribute + " " + monIdNames.FirstOrDefault(m => m.Key == mon._monsterTypeId / 100).Value;
                }
                foreach (var r in mon.Runes)
                {
                    r.AssignedTo = mon;
                    Runes.Add(r);
                }
            }
        }
    }
    
    public class SWFStats
    {
        [JsonProperty("atk")]
        public int Attack;

        [JsonProperty("def")]
        public int Defense;

        [JsonProperty("con")]
        public int _con;

        [JsonIgnore]
        public int? health;

        [JsonIgnore]
        public int Health { get { return health ?? _con * 15; } set { health = value; } }

        [JsonProperty("spd")]
        public int Speed;

        [JsonProperty("critical_rate")]
        public int CritRate;

        [JsonProperty("critical_damage")]
        public int CritDamage;

        [JsonProperty("accuracy")]
        public int Accuracy;

        [JsonProperty("res")]
        public int Resistance;

    }

    public class SWFMonster : SWFStats
    {
        [JsonProperty("attribute")]
        public Element _attribute;

        [JsonProperty("skills")]
        public IList<SWFSkill> _skilllist;

        [JsonProperty("class")]
        public int _class;

        [JsonProperty("unit_id")]
        public long Id;

        [JsonProperty("runes")]
        public SWFRune[] Runes;

        [JsonProperty("unit_master_id")]
        public int _monsterTypeId;

        [JsonIgnore]
        public string Name;

        [JsonProperty("unit_level")]
        public int Level;

        public override string ToString()
        {
            return Id + " " + Name + " lvl. " + Level;
        }
    }

    #region Fixing {prop:[1,3]}
    public abstract class SWFListProp
        : IList<int>
    {
        public int Count
        {
            get
            {
                var type = this.GetType();
                var maxind = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(SWFListPropertyAttribute)))
                    .Max(p => ((SWFListPropertyAttribute)p.GetCustomAttributes(typeof(SWFListPropertyAttribute), false).First()).Index) + 1;
                for (int i = 0; i < maxind; i++)
                {
                    if (this[i] == -1)
                        return i;
                }
                return maxind;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                var type = this.GetType();
                var props = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(SWFListPropertyAttribute)));
                foreach (var p in props)
                {
                    if (this[((SWFListPropertyAttribute)p.GetCustomAttributes(typeof(SWFListPropertyAttribute), false).First()).Index] == -1)
                        return false;
                }
                return true;
            }
        }

        public int this[int index]
        {
            get
            {
                var type = this.GetType();
                var props = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(SWFListPropertyAttribute)));
                var prop = props.Where(p => ((SWFListPropertyAttribute)p.GetCustomAttributes(typeof(SWFListPropertyAttribute), false).First()).Index == index).FirstOrDefault();
                if (prop == null)
                    throw new IndexOutOfRangeException("No class member assigned to that index!");
                return (int) prop.GetValue(this);
            }
            set
            {
                var type = this.GetType();
                var props = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(SWFListPropertyAttribute)));
                var prop = props.Where(p => ((SWFListPropertyAttribute)p.GetCustomAttributes(typeof(SWFListPropertyAttribute), false).First()).Index == index).FirstOrDefault();
                if (prop == null)
                    throw new IndexOutOfRangeException("No class member assigned to that index!");

                prop.SetValue(this, (int)value);
            }
        }

        public int IndexOf(int item) { throw new NotImplementedException(); }

        public void Insert(int index, int item) { throw new NotImplementedException(); }

        public void RemoveAt(int index) { throw new NotImplementedException(); }

        public void Clear() { throw new NotImplementedException(); }

        public bool Contains(int item) { throw new NotImplementedException(); }

        public void CopyTo(int[] array, int arrayIndex) { throw new NotImplementedException(); }

        public bool Remove(int item) { throw new NotImplementedException(); }

        public IEnumerator<int> GetEnumerator() { throw new NotImplementedException(); }

        IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }

        public void Add(int item)
        {
            this[Count] = item;
        }
    }

    public class SWFListPropertyAttribute : Attribute
    {
        public int Index;

        public SWFListPropertyAttribute(int ind)
        {
            Index = ind;
        }
    }
    #endregion

    public class SWFSkill : SWFListProp
    {
        // TODO: name
        [SWFListProperty(0)]
        public int SkillId = -1;
        [SWFListProperty(1)]
        public int Level = -1;
    }

    public class SWFRuneAttr : SWFListProp
    {
        [SWFListProperty(0)]
        public Attr Type = Attr.Neg;

        [SWFListProperty(1)]
        public int BaseValue = -1;

        [SWFListProperty(2)]
        public int __int2 = -1;

        [SWFListProperty(3)]
        public int GrindBonus = -1;

        [JsonIgnore]
        public int Value { get { return BaseValue + (GrindBonus > 0 ? GrindBonus : 0); } }

        public override string ToString()
        {
            return Type + " +" + Value;
        }
    }

    // cut down rune, SWarFarm.json has entire runes inside monsters
    public class SWFRuneLink
    {
        [JsonProperty("occupied_id")]
        public long EquippedMonId;

        [JsonProperty("rune_id")]
        public long RuneId;
    }


    public class SWFRune : SWFRuneLink
    {
        [JsonProperty("class")]
        public int Grade;

        [JsonProperty("set_id")]
        public int _set;

        [JsonProperty("upgrade_curr")]
        public int Level;

        [JsonProperty("rune_id")]
        public long Id;

        [JsonProperty("slot_no")]
        public int Slot;

        [JsonProperty("rank")]
        public int _rank;

        [JsonProperty("pri_eff")]
        public SWFRuneAttr Main;

        [JsonProperty("prefix_eff")]
        public SWFRuneAttr Innate;

        [JsonProperty("sec_eff")]
        public List<SWFRuneAttr> Subs;

        [JsonProperty("occupied_type")]
        public int _occupiedType;

        [JsonIgnore]
        public SWFMonster AssignedTo;

        [JsonProperty("sell_value")]
        public int SellValue;

    }

}
