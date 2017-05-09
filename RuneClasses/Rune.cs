using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Concurrent;

namespace RuneOptim
{
	// Enums up Runesets
	[JsonConverter(typeof(StringEnumConverter))]
	public enum RuneSet
	{
		[EnumMember(Value = "???")]
		Unknown = -1, // SW Proxy say what?

		[EnumMember(Value = "")]
		Null = 0, // No set

		Energy, // Health
		Guard, // Def
		Swift, // Speed
		Blade, // CRate
		Rage, // CDmg
		Focus, // Acc
		Endure, // Res
		Fatal, // Attack

		__unknown9,

		// Here be magic
		Despair,
		Vampire,

		__unknown12,

		Violent,
		Nemesis,
		Will,
		Shield,
		Revenge,
		Destroy,

		// Ally sets
		Fight,
		Determination,
		Enhance,
		Accuracy,
		Tolerance,

		Broken
	}

	
	[JsonConverter(typeof(StringEnumConverter))]
	public enum SlotIndex
	{
		[EnumMember(Value = "e")]
		Even = -2,

		//o = -1,

		[EnumMember(Value = "o")]
		Odd = -1,

		[EnumMember(Value = "g")]
		Global = 0,

		[EnumMember(Value = "1")]
		One = 1,
		[EnumMember(Value = "2")]
		Two = 2,
		[EnumMember(Value = "3")]
		Three = 3,
		[EnumMember(Value = "4")]
		Four = 4,
		[EnumMember(Value = "5")]
		Five = 5,
		[EnumMember(Value = "6")]
		Six = 6
	};

	#region Fixing {prop:[1,3]}

	public class OnSetEventArgs
	{
		public int i = -1;
		public int val = -1;
	}

	public abstract class ListProp
		: IList<int>
	{
		int maxind = -1;

		virtual protected int MaxInd
		{
			get
			{
				if (maxind == -1)
				{
					var type = this.GetType();
					maxind = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(ListPropertyAttribute)))
						.Max(p => ((ListPropertyAttribute)p.GetCustomAttributes(typeof(ListPropertyAttribute), false).First()).Index) + 1;

				}
				return maxind;
			}
		}

		virtual protected void OnSet(int i, int val) { }

		private void _onSet(int i, int v)
		{
			OnSet(i, v);
			onSet?.Invoke(this, new OnSetEventArgs() { i = i, val = v });
		}

		public event EventHandler<OnSetEventArgs> onSet;

		public int Count
		{
			get
			{
				for (int i = 0; i < MaxInd; i++)
				{
					if (this[i] == -1)
						return i;
				}
				return MaxInd;
			}
		}

		virtual public bool IsReadOnly
		{
			get
			{
				foreach (var p in Props)
				{
					if (this[p.Key] == -1)
						return false;
				}
				return true;
			}
		}

		Dictionary<int, System.Reflection.FieldInfo> props = null;

		Dictionary<int, System.Reflection.FieldInfo> Props
		{
			get
			{
				if (props == null)
				{
					var type = this.GetType();
					var pros = type.GetFields().Where(p => Attribute.IsDefined(p, typeof(ListPropertyAttribute)));
					props = pros.ToDictionary(p => ((ListPropertyAttribute)p.GetCustomAttributes(typeof(ListPropertyAttribute), false).First()).Index);
				}
				return props;
			}
		}

		virtual public int this[int index]
		{
			get
			{
				if (Props[index] == null)
					throw new IndexOutOfRangeException("No class member assigned to that index!");
				return (int)props[index].GetValue(this);
			}
			set
			{
				if (Props[index] == null)
					throw new IndexOutOfRangeException("No class member assigned to that index!");

				props[index].SetValue(this, (int)value);
				_onSet(index, value);
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

		virtual public void Add(int item)
		{
			this[Count] = item;
		}
	}

	public class ListPropertyAttribute : Attribute
	{
		public int Index;

		public ListPropertyAttribute(int ind)
		{
			Index = ind;
		}
	}
	#endregion

	public class RuneAttr : ListProp
	{
		[ListProperty(0)]
		public Attr Type = Attr.Neg;

		[ListProperty(1)]
		public int BaseValue = -1;

		[ListProperty(2)]
		public int __int2 = -1;

		[ListProperty(3)]
		public int GrindBonus = -1;

		[JsonIgnore]
		private int _calcVal = -1;

		protected override void OnSet(int i, int val)
		{
			if (Type != Attr.Neg)
			{
				_calcVal = BaseValue + (GrindBonus > 0 ? GrindBonus : 0);
			}
			else
				_calcVal = 0;
		}

		[JsonIgnore]
		public int Value
		{
			get
			{
				if (_calcVal == -1)
					OnSet(0, 0);
				return _calcVal;
			}
			set
			{
				GrindBonus = 0;
				BaseValue = value;
				OnSet(1, value);
			}
		}

		public override string ToString()
		{
			return Type + " +" + Value;
		}

		protected override int MaxInd
		{
			get
			{
				return 4;
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public override int this[int index]
		{
			get
			{
				if (index == 0)
					return (int)Type;
				else if (index == 1)
					return BaseValue;
				else if (index == 2)
					return __int2;
				else if (index == 3)
					return GrindBonus;
				return -1;
			}

			set
			{
				if (index == 0)
					Type = (Attr)value;
				else if (index == 1)
					BaseValue = value;
				else if (index == 2)
					__int2 = value;
				else if (index == 3)
					GrindBonus = value;
			}
		}

		public override void Add(int item)
		{
			if (Type == Attr.Neg)
				Type = (Attr)item;
			else if (BaseValue == -1)
				BaseValue = item;
			else if (__int2 == -1)
				__int2 = item;
			else if (GrindBonus == -1)
				GrindBonus = item;
			else
				throw new IndexOutOfRangeException();
		}
	}

	public class RuneLink
	{
		[JsonProperty("rune_id")]
		public ulong Id { get; set; }

		[JsonProperty("occupied_id")]
		public ulong AssignedId { get; set; }
	}

	public class Rune : RuneLink
	{
		#region JSON Props

		[JsonProperty("set_id")]
		public RuneSet Set;

		[JsonProperty("class")]
		public int Grade;

		[JsonProperty("slot_no")]
		public int Slot;

		[JsonProperty("upgrade_curr")]
		public int Level;

		[JsonProperty("rank")]
		public int _rank;

		[JsonProperty("locked")]
		public bool Locked;

		[JsonProperty("occupied_type")]
		public int _occupiedType;

		[JsonProperty("sell_value")]
		public int SellValue;

		[JsonProperty("monster_n")]
		public string AssignedName;

		[JsonProperty("pri_eff")]
		public RuneAttr Main;

		[JsonProperty("prefix_eff")]
		public RuneAttr Innate;

		[JsonProperty("sec_eff")]
		public List<RuneAttr> Subs;

		#endregion

		#region Nicer getters for stats by type

		[JsonIgnore]
		public int[] HealthFlat = new int[32];

		[JsonIgnore]
		public int[] HealthPercent = new int[32];

		[JsonIgnore]
		public int[] AttackFlat = new int[32];

		[JsonIgnore]
		public int[] AttackPercent = new int[32];

		[JsonIgnore]
		public int[] DefenseFlat = new int[32];

		[JsonIgnore]
		public int[] DefensePercent = new int[32];

		[JsonIgnore]
		public int[] Speed = new int[32];

		[JsonIgnore]
		public int[] SpeedPercent = new int[32];

		[JsonIgnore]
		public int[] CritRate = new int[32];

		[JsonIgnore]
		public int[] CritDamage = new int[32];

		[JsonIgnore]
		public int[] Accuracy = new int[32];
		
		[JsonIgnore]
		public int[] Resistance = new int[32];
		
		#endregion
		
		[JsonIgnore]
		public Monster Assigned;
		
		[JsonIgnore]
		public bool Swapped = false;

		[JsonIgnore]
		public static readonly int[] UnequipCosts = { 1000, 2500, 5000, 10000, 25000, 50000 };

		[JsonIgnore]
		private bool? setIs4;

		public bool SetIs4
		{
			get
			{
				return setIs4 ?? (setIs4 = (Rune.SetRequired(this.Set) == 4)).Value;
			}
		}

		[JsonIgnore]
		public int UnequipCost
		{
			get
			{
				return UnequipCosts[Grade - 1];
			}
		}

		[JsonIgnore]
		public int Rarity
		{
			get
			{
				// TODO: base-rarity is a thing now, consider this
				return Subs.Count;
			}
		}

		public Rune()
		{
			Main = new RuneAttr();
			Innate = new RuneAttr();
			Main.onSet += (a, b) => { FixShit(); };
			Innate.onSet += (a, b) => { FixShit(); };
			Subs = new List<RuneAttr>();
		}

		public Rune(Rune rhs)
		{
			Main = new RuneAttr();
			Main.onSet += (a, b) => { FixShit(); };
			Innate = new RuneAttr();
			Innate.onSet += (a, b) => { FixShit(); };
			Subs = new List<RuneAttr>();

			Id = rhs.Id;
			Set = rhs.Set;
			Grade = rhs.Grade;
			Slot = rhs.Slot;
			Level = rhs.Level;
			Locked = rhs.Locked;
			AssignedId = rhs.AssignedId;
			AssignedName = rhs.AssignedName;
			Assigned = rhs.Assigned;
			Swapped = rhs.Swapped;
		}

		// fast iterate over rune stat types
		public int this[string stat, int fake, bool pred]
		{
			get
			{
				int ind = pred ? fake + 16 : fake;
				switch (stat)
				{
					case "HPflat":
						return HealthFlat[ind];
					case "HPperc":
						return HealthPercent[ind];
					case "ATKflat":
						return AttackFlat[ind];
					case "ATKperc":
						return AttackPercent[ind];
					case "DEFflat":
						return DefenseFlat[ind];
					case "DEFperc":
						return DefensePercent[ind];
					case "SPDflat":
						return Speed[ind];
					case "CDperc":
						return CritDamage[ind];
					case "CRperc":
						return CritRate[ind];
					case "ACCperc":
						return Accuracy[ind];
					case "RESperc":
						return Resistance[ind];
					default:
						return 0;
				}
			}
		}

		// fast iterate over rune stat types
		public int this[Attr stat, int fake, bool pred]
		{
			get
			{
				int ind = pred ? fake + 16 : fake;
				switch (stat)
				{
					case Attr.HealthFlat:
						return HealthFlat[ind];
					case Attr.HealthPercent:
						return HealthPercent[ind];
					case Attr.AttackFlat:
						return AttackFlat[ind];
					case Attr.AttackPercent:
						return AttackPercent[ind];
					case Attr.DefenseFlat:
						return DefenseFlat[ind];
					case Attr.DefensePercent:
						return DefensePercent[ind];
					case Attr.Speed:
					case Attr.SpeedPercent:
						return Speed[ind];
					case Attr.CritDamage:
						return CritDamage[ind];
					case Attr.CritRate:
						return CritRate[ind];
					case Attr.Accuracy:
						return Accuracy[ind];
					case Attr.Resistance:
						return Resistance[ind];
				}
				return 0;
			}
		}

		[JsonIgnore]
		public bool IsUnassigned
		{
			get
			{
				return new string[] { "Unknown name", "Inventory", "Unknown name (???[0])" }.Any(s => s.Equals(this.AssignedName));
			}
		}
		
		[JsonIgnore]
		public double Efficiency
		{
			get
			{
				double num = 0;
				num += GetEfficiency(Innate.Type, Innate.Value);
				if (Subs.Count > 0)
					num += GetEfficiency(Subs[0].Type, Subs[0].Value);
				if (Subs.Count > 1)
					num += GetEfficiency(Subs[1].Type, Subs[1].Value);
				if (Subs.Count > 2)
					num += GetEfficiency(Subs[2].Type, Subs[2].Value);
				if (Subs.Count > 3)
					num += GetEfficiency(Subs[3].Type, Subs[3].Value);

				num /= 1.8;
				return num;
			}
		}

		public int FlatCount()
		{
			int count = 0;
			if (Subs.Count == 0 || Subs[0].Type == Attr.Null) return count;
			count += (Subs[0].Type == Attr.HealthFlat || Subs[0].Type == Attr.DefenseFlat || Subs[0].Type == Attr.AttackFlat) ? 1 : 0;
			if (Subs.Count == 1 || Subs[1].Type == Attr.Null) return count;
			count += (Subs[1].Type == Attr.HealthFlat || Subs[1].Type == Attr.DefenseFlat || Subs[1].Type == Attr.AttackFlat) ? 1 : 0;
			if (Subs.Count == 2 || Subs[2].Type == Attr.Null) return count;
			count += (Subs[2].Type == Attr.HealthFlat || Subs[2].Type == Attr.DefenseFlat || Subs[2].Type == Attr.AttackFlat) ? 1 : 0;
			if (Subs.Count == 3 || Subs[3].Type == Attr.Null) return count;
			count += (Subs[3].Type == Attr.HealthFlat || Subs[3].Type == Attr.DefenseFlat || Subs[3].Type == Attr.AttackFlat) ? 1 : 0;

			return count;
		}

		public AttributeCategory getSetType()
		{
			var neutral = new RuneSet[] { RuneSet.Violent, RuneSet.Swift, RuneSet.Focus, RuneSet.Nemesis };
			if (new RuneSet[] { RuneSet.Fatal, RuneSet.Blade, RuneSet.Rage, RuneSet.Vampire, RuneSet.Revenge }.Contains(Set)) // o
				return AttributeCategory.Offensive;
			else if (new RuneSet[] { RuneSet.Energy, RuneSet.Endure, RuneSet.Guard, RuneSet.Shield, RuneSet.Will }.Contains(Set)) // d
				return AttributeCategory.Defensive;
			else if (new RuneSet[] { RuneSet.Despair, RuneSet.Determination, RuneSet.Enhance, RuneSet.Fight, RuneSet.Accuracy, RuneSet.Tolerance }.Contains(Set)) // s
				return AttributeCategory.Support;

			return AttributeCategory.Neutral;
		}

		[JsonIgnore]
		private static readonly Attr[] attackSubs = new Attr[] { Attr.AttackPercent, Attr.CritDamage, Attr.CritRate };
		[JsonIgnore]
		private static readonly Attr[] defenseSubs = new Attr[] { Attr.HealthPercent, Attr.DefensePercent };
		[JsonIgnore]
		private static readonly Attr[] supportSubs = new Attr[] { Attr.Accuracy, Attr.Resistance };

		public double ComputeRating()
		{
			double r = 0;
			var type = getSetType();

			// for each sub (if flat = 0, null = 0.3, else 1)

			// set types
			// stat types
			// offense/defense/support/neutral

			var subs = new Attr[4];
			if (Subs.Count > 0)
				subs[0] = Subs[0].Type;
			if (Subs.Count > 1)
				subs[1] = Subs[1].Type;
			if (Subs.Count > 2)
				subs[2] = Subs[2].Type;
			if (Subs.Count > 3)
				subs[3] = Subs[3].Type;

			foreach (var sub in subs)
			{
				// if null
				if (sub == Attr.Null)
				{
					r += 1 / (double)3;
					continue;
				}

				// if not flat
				if (!new Attr[] { Attr.AttackFlat, Attr.DefenseFlat, Attr.HealthFlat }.Contains(sub))
					r += 1;

				if (new Attr[] { Attr.HealthPercent, Attr.Speed }.Contains(sub))
					r += 0.5;

				AttributeCategory subt;
				r += computeSubTypeRating(type, sub, out subt);
				r += computeSubRating(sub, subs, subt);
			}

			if (Grade == 5)
				r += 4;
			else if (Grade == 6)
				r += 7;

			return r;
		}


		private double computeSubTypeRating(AttributeCategory type, Attr sub, out AttributeCategory subt)
		{
			subt = AttributeCategory.Neutral;
			if (attackSubs.Contains(sub))
				subt = AttributeCategory.Offensive;
			else if (defenseSubs.Contains(sub))
				subt = AttributeCategory.Defensive;
			else if (supportSubs.Contains(sub))
				subt = AttributeCategory.Support;

			double r = 0;

			switch (type)
			{
				case AttributeCategory.Offensive:
					switch (subt)
					{
						case AttributeCategory.Offensive:
							return 1;
						default:
							return 0.25;
					}
				case AttributeCategory.Defensive:
					switch (subt)
					{
						case AttributeCategory.Offensive:
							return 0.25;
						case AttributeCategory.Defensive:
							return 1;
						default:
							return 0.5;
					}
				case AttributeCategory.Support:
					switch (subt)
					{
						case AttributeCategory.Defensive:
							return 0.5;
						case AttributeCategory.Support:
							return 1;
						default:
							return 0.25;
					}
				case AttributeCategory.Neutral:
					switch (subt)
					{
						case AttributeCategory.Offensive:
						case AttributeCategory.Defensive:
							return 0.5;
						default:
							return 0.25;
					}
				default:
					break;
			}

			return r;
		}

		private double computeSubRating(Attr sub, Attr[] subs, AttributeCategory subt)
		{
			double r = 0;
			foreach (var sub2 in subs)
			{
				if (sub == sub2 || sub2 == Attr.Null)
					continue;

				if ((subt == AttributeCategory.Offensive && attackSubs.Contains(sub2))
					|| (subt == AttributeCategory.Defensive && defenseSubs.Contains(sub2))
					|| (subt == AttributeCategory.Support && supportSubs.Contains(sub2)))
					r += 1;
			}
			return r;
		}

		[JsonIgnore]
		public ConcurrentDictionary<string, double> manageStats = new ConcurrentDictionary<string, double>();
		
		[JsonIgnore]
		private Attr[] hpStats = new Attr[] { Attr.HealthPercent, Attr.DefensePercent, Attr.Resistance, Attr.HealthFlat, Attr.DefenseFlat };

		public double ScoreHP()
		{
			double v = 0;

			v += HealthPercent[0] / (double)subMaxes[Attr.HealthPercent];
			v += DefensePercent[0] / (double)subMaxes[Attr.DefensePercent];
			v += Resistance[0] / (double)subMaxes[Attr.Resistance];
			v += 0.5 * HealthFlat[0] / subMaxes[Attr.HealthFlat];
			v += 0.5 * DefenseFlat[0] / subMaxes[Attr.DefenseFlat];

			if (Main.Type == Attr.HealthPercent || Main.Type == Attr.DefensePercent || Main.Type == Attr.Resistance)
			{
				v -= Main.Value / (double)subMaxes[Main.Type];
				if (Slot % 2 == 0)
					v += Grade /(double) 6;
			}
			else if (Main.Type == Attr.DefenseFlat || Main.Type == Attr.HealthFlat)
			{
				v -= 0.5 * Main.Value / subMaxes[Main.Type];
				if (Slot % 2 == 0)
					v += Grade / (double) 6;
			}
			
			double d = 0.5;
			if (Slot == 3 || Slot == 5)
				d += 0.2;

			if (Slot % 2 == 0)
				d += 1;

			var stt = 0;
			if (Subs.Count > 0 && hpStats.Contains(Subs[0].Type))
				stt++;
			if (Subs.Count > 1 && hpStats.Contains(Subs[1].Type))
				stt++;
			if (Subs.Count > 2 && hpStats.Contains(Subs[2].Type))
				stt++;
			if (Subs.Count > 3 && hpStats.Contains(Subs[3].Type))
				stt++;
			
			d += 0.2 * (
				Rarity
				- (
					Rarity
					- Math.Floor(Level / (double)3)
				) * stt
			);

			return v/d;
		}

		[JsonIgnore]
		private readonly Attr[] atkStats = new Attr[] { Attr.AttackFlat, Attr.AttackPercent, Attr.CritRate, Attr.CritDamage };

		public double ScoreATK()
		{
			double v = 0;

			v += AttackPercent[0] / (double)subMaxes[Attr.AttackPercent];
			v += CritRate[0] / (double)subMaxes[Attr.CritRate];
			v += CritDamage[0] / (double)subMaxes[Attr.CritDamage];
			v += 0.5 * AttackFlat[0] / subMaxes[Attr.AttackFlat];

			if (Main.Type == Attr.AttackPercent || Main.Type == Attr.CritRate || Main.Type == Attr.CritDamage)
			{
				v -= Main.Value / (double)subMaxes[Main.Type];
				if (Slot % 2 == 0)
					v += Grade / (double)6;
			}
			else if (Main.Type == Attr.AttackFlat)
			{
				v -= 0.5 * Main.Value / subMaxes[Main.Type];
				if (Slot % 2 == 0)
					v += Grade / (double)6;
			}

			double d = 0.4;

			if (Slot == 1)
				d += 0.2;

			if (Slot % 2 == 0)
				d += 1.1;

			var stt = 0;
			if (Subs.Count > 0 && atkStats.Contains(Subs[0].Type))
				stt++;
			if (Subs.Count > 1 && atkStats.Contains(Subs[1].Type))
				stt++;
			if (Subs.Count > 2 && atkStats.Contains(Subs[2].Type))
				stt++;
			if (Subs.Count > 3 && atkStats.Contains(Subs[3].Type))
				stt++;
			
			d += 0.2 * (
				Rarity
				- (
					Rarity
					- Math.Floor(Level / (double)3)
				) * stt
			);

			return v/d;
		}

		public double ScoreRune()
		{
			double v = 0;
				
			if (Innate != null && Innate.Value > 0)
			{
				if (Innate.Type == Attr.AttackFlat || Innate.Type == Attr.HealthFlat || Innate.Type == Attr.DefenseFlat)
					v += 0.5 * Innate.Value / subMaxes[Innate.Type];
				else
					v += Innate.Value / (double)subMaxes[Innate.Type];
			}

			if (Subs.Count > 0 && Subs[0].Value > 0)
			{
				if (Subs[0].Type == Attr.AttackFlat || Subs[0].Type == Attr.HealthFlat || Subs[0].Type == Attr.DefenseFlat)
					v += 0.5 * Subs[0].Value / subMaxes[Subs[0].Type];
				else
					v += Subs[0].Value / (double)subMaxes[Subs[0].Type];
			}

			if (Subs.Count > 1 && Subs[1].Value > 0)
			{
				if (Subs[1].Type == Attr.AttackFlat || Subs[1].Type == Attr.HealthFlat || Subs[1].Type == Attr.DefenseFlat)
					v += 0.5 * Subs[1].Value / subMaxes[Subs[1].Type];
				else
					v += Subs[1].Value / (double)subMaxes[Subs[1].Type];
			}

			if (Subs.Count > 2 && Subs[2].Value > 0)
			{
				if (Subs[2].Type == Attr.AttackFlat || Subs[2].Type == Attr.HealthFlat || Subs[2].Type == Attr.DefenseFlat)
					v += 0.5 * Subs[2].Value / subMaxes[Subs[2].Type];
				else
					v += Subs[2].Value / (double)subMaxes[Subs[2].Type];
			}

			if (Subs.Count > 3 && Subs[3].Value > 0)
			{
				if (Subs[3].Type == Attr.AttackFlat || Subs[3].Type == Attr.HealthFlat || Subs[3].Type == Attr.DefenseFlat)
					v += 0.5 * Subs[3].Value / subMaxes[Subs[3].Type];
				else
					v += Subs[3].Value / (double)subMaxes[Subs[3].Type];
			}

			v += Grade / (double)6;

			double d = 2;
			d += 0.2 * Math.Min(4, Math.Floor(Level / (double)3));

			return v/d;
		}

		public double GetEfficiency(Attr a, int val)
		{
			if (a == Attr.Null)
				return 0;
			if (a == Attr.HealthFlat || a == Attr.AttackFlat || a == Attr.DefenseFlat)
				return val / (double)2 / (5 * subUpgrades[a][Grade - 1]);

			return val / (double) (5 * subUpgrades[a][Grade - 1]);
		}

		public void ResetStats()
		{
			manageStats.Clear();
		}

		// Number of sets
		public static readonly int SetCount = Enum.GetNames(typeof(RuneSet)).Length;

		// Number of runes required for set to be complete
		public static int SetRequired(RuneSet set)
		{
			if (set == RuneSet.Swift || set == RuneSet.Fatal || set == RuneSet.Violent || set == RuneSet.Vampire || set == RuneSet.Despair || set == RuneSet.Rage)
				return 4;
			// Not a 4 set => is a 2 set
			return 2;
		}

		// Format rune values okayish
		public string StringIt()
		{
			return StringIt(Main.Type, Main.Value);
		}

		public static string StringIt(Attr type, int? val)
		{
			if (type == Attr.Null || !val.HasValue)
				return "";
			return StringIt(type, val.Value);
		}

		public static string StringIt(Attr type, int val)
		{
			string ret = StringIt(type);

			ret += " +" + val;

			if (type.ToString().Contains("Percent") || type == Attr.CritRate || type == Attr.CritDamage || type == Attr.Accuracy || type == Attr.Resistance)
			{
				ret += "%";
			}

			return ret;
		}
		
		public static string StringIt(List<RuneAttr> subs, int v)
		{
			if (subs.Count > v)
				return StringIt(subs[v].Type, subs[v].Value);
			return "";
		}
		
		// Ask the rune for the value of the Attribute type as a string
		public static string StringIt(Attr type, bool suffix = false)
		{
			string ret = "";

			switch (type)
			{
				case Attr.HealthFlat:
				case Attr.HealthPercent:
					ret += "HP";
					break;
				case Attr.AttackPercent:
				case Attr.AttackFlat:
					ret += "ATK";
					break;
				case Attr.DefenseFlat:
				case Attr.DefensePercent:
					ret += "DEF";
					break;
				case Attr.Speed:
					ret += "SPD";
					break;
				case Attr.CritRate:
					ret += "CRI Rate";
					break;
				case Attr.CritDamage:
					ret += "CRI Dmg";
					break;
				case Attr.Accuracy:
					ret += "Accuracy";
					break;
				case Attr.Resistance:
					ret += "Resistance";
					break;
			}
			if (type.ToString().Contains("Percent") || type == Attr.CritRate || type == Attr.CritDamage || type == Attr.Accuracy || type == Attr.Resistance)
				ret += "%";

			return ret;
		}

		// these sets can't be superceded by really good stats
		// Eg. Blade can be replaced by 12% crit.
		public static readonly RuneSet[] MagicalSets = 
		{
			RuneSet.Violent, RuneSet.Will, RuneSet.Nemesis, RuneSet.Shield, RuneSet.Revenge, RuneSet.Despair, RuneSet.Vampire, RuneSet.Destroy,
			RuneSet.Tolerance, RuneSet.Accuracy, RuneSet.Determination, RuneSet.Enhance, RuneSet.Fight,
		};

		// Debugger niceness
		public override string ToString()
		{
			return Grade + "* " + Set + " " + StringIt();
		}

		// Gets the value of that Attribute on this rune
		public int GetValue(Attr stat, int FakeLevel = -1, bool PredictSubs = false)
		{
			if (Main == null) return -1;
			// the stat can only be present once per rune, early exit
			if (Main.Type == stat && Grade >= 3 && FakeLevel <= 15 && FakeLevel > Level)
				return MainValues[Main.Type][Grade - 3][FakeLevel];
			else if (Main.Type == stat)
				return Main.Value;

			// Need to be able to load in null values (requiring int?) but xType shouldn't be a Type if xValue is null
			if (Innate.Type == stat) return Innate.Value;
			// Here, if a subs type is null, there is not further subs (it's how runes work), so we quit early.
			if (!PredictSubs)
			{
				if (Subs.Count > 0 && (Subs[0].Type == stat || Subs[0].Type == Attr.Null)) return Subs[0].Value;
				else if (Subs.Count > 1 && (Subs[1].Type == stat || Subs[1].Type == Attr.Null)) return Subs[1].Value;
				else if (Subs.Count > 2 && (Subs[2].Type == stat || Subs[2].Type == Attr.Null)) return Subs[2].Value;
				else if (Subs.Count > 3 && (Subs[3].Type == stat || Subs[3].Type == Attr.Null)) return Subs[3].Value;
			}
			else
			{
				// count how many upgrades have gone into the rune
				int maxUpgrades = Math.Min(Rarity, Math.Max(Level, FakeLevel) / 3);
				int upgradesGone = Math.Min(4, Level / 3);
				// how many new sub are to appear (0 legend will be 4 - 4 = 0, 6 rare will be 4 - 3 = 1, 6 magic will be 4 - 2 = 2)
				int subNew = 4 - Rarity;
				// how many subs will go into existing stats (0 legend will be 4 - 0 - 0 = 4, 6 rare will be 4 - 1 - 2 = 1, 6 magic will be 4 - 2 - 2 = 0)
				int subEx = maxUpgrades - upgradesGone;// - subNew;
				int subVal = (subNew > 0 ? 1 : 0);
				
				// TODO: sub prediction
				if (Subs.Count > 0 && (Subs[0].Type == stat || Subs[0].Type == Attr.Null)) return (Subs[0].Value + subEx);//?? subVal;
				else if (Subs.Count > 1 && (Subs[1].Type == stat || Subs[1].Type == Attr.Null)) return (Subs[1].Value + subEx);// ?? subVal;
				else if (Subs.Count > 2 && (Subs[2].Type == stat || Subs[2].Type == Attr.Null)) return (Subs[2].Value + subEx);// ?? subVal;
				else if (Subs.Count > 3 && (Subs[3].Type == stat || Subs[3].Type == Attr.Null)) return (Subs[3].Value + subEx);// ?? subVal;
			}
		
			return 0;
		}

		// Does it have this stat at all?
		// TODO: should I listen to fake/pred?
		public bool HasStat(Attr stat, int fake = -1, bool pred = false)
		{
			if (GetValue(stat, fake, pred) > 0)
				return true;
			return false;
		}

		// For each non-zero stat in flat and percent, divide the runes value and see if any >= test
		public bool Or(Stats rFlat, Stats rPerc, Stats rTest, int fake = -1, bool pred = false)
		{
			foreach (Attr attr in Build.statEnums)
			{
				if (attr != Attr.Speed && !rPerc[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rPerc[attr] >= rTest[attr])
						return true;
			}
			foreach (Attr attr in new Attr[] { Attr.Speed, Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat })
			{
				if (!rFlat[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rFlat[attr] >= rTest[attr])
						return true;
			}
			return false;
		}

		// For each non-zero stat in flat and percent, divide the runes value and see if *ALL* >= test
		public bool And(Stats rFlat, Stats rPerc, Stats rTest, int fake = -1, bool pred = false)
		{
			foreach (Attr attr in Build.statEnums)
			{
				if (attr != Attr.Speed && !rPerc[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rPerc[attr] < rTest[attr])
						return false;
			}
			foreach (Attr attr in new Attr[] { Attr.Speed, Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat })
			{
				if (!rFlat[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rFlat[attr] < rTest[attr])
						return false;
			}
			return true;
		}

		// sum the result of dividing the runes value by flat/percent per stat
		public double Test(Stats rFlat, Stats rPerc, int fake = -1, bool pred = false)
		{
			double val = 0;
			foreach (Attr attr in Build.statEnums)
			{
				if (attr != Attr.Speed && !rPerc[attr].EqualTo(0))
				{
					Console.Out.WriteLine(attr + ": " + this[attr, fake, pred] + "/" + rPerc[attr] + (this[attr, fake, pred] / rPerc[attr]));
					val += this[attr, fake, pred] / rPerc[attr];
				}
			}
			foreach (Attr attr in new Attr[] { Attr.Speed, Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat })
			{
				if (!rFlat[attr].EqualTo(0))
				{
					Console.Out.WriteLine(attr + ": " + this[attr, fake, pred] + "/" + rFlat[attr] + (this[attr, fake, pred] / rFlat[attr]));
					val += this[attr, fake, pred] / rFlat[attr];
				}
			}
			manageStats.AddOrUpdate("testScore", val, (k, v) => val);
			return val;
		}

		// NYI rune comparison
		public EquipCompare CompareTo(Rune rhs)
		{
			if (Set != rhs.Set)
				return EquipCompare.Unknown;

			if (HealthPercent[0] < rhs.HealthPercent[0])
				return EquipCompare.Worse;
			if (AttackPercent[0] < rhs.AttackPercent[0])
				return EquipCompare.Worse;
			if (DefensePercent[0] < rhs.DefensePercent[0])
				return EquipCompare.Worse;
			if (Speed[0] < rhs.Speed[0])
				return EquipCompare.Worse;
			if (CritRate[0] < rhs.CritRate[0])
				return EquipCompare.Worse;
			if (CritDamage[0] < rhs.CritDamage[0])
				return EquipCompare.Worse;
			if (Accuracy[0] < rhs.Accuracy[0])
				return EquipCompare.Worse;
			if (Resistance[0] < rhs.Resistance[0])
				return EquipCompare.Worse;

			return EquipCompare.Better;
		}

#region stats

		private static Dictionary<Attr, int> subMaxes = new Dictionary<Attr, int>()
		{
			{Attr.Null, 1 },
			{Attr.HealthFlat, 1875 },
			{Attr.AttackFlat, 100 },
			{Attr.DefenseFlat, 100 },
			{Attr.Speed, 30 },

			{Attr.HealthPercent, 40 },
			{Attr.AttackPercent, 40 },
			{Attr.DefensePercent, 40 },

			{ Attr.CritRate, 30 },
			{ Attr.CritDamage, 35 },

			{Attr.Resistance, 40 },
			{Attr.Accuracy, 40 },
		};


		private static Dictionary<Attr, int[]> subUpgrades = new Dictionary<Attr, int[]>()
		{
			{Attr.HealthFlat, new int[] { 20, 90, 160, 222, 279, 365 } },
			{Attr.AttackFlat, new int[] { 3, 4, 6, 9, 15, 23 } },
			{Attr.DefenseFlat, new int[] { 3, 4, 6, 9, 15, 23 } },
			{Attr.Speed, new int[] { 1, 2, 3, 4, 5, 6} },

			{Attr.HealthPercent, new int[] { 3, 4, 5, 6, 7, 8} },
			{Attr.AttackPercent, new int[] { 3, 4, 5, 6, 7, 8} },
			{Attr.DefensePercent, new int[] { 3, 4, 5, 6, 7, 8 } },

			{ Attr.CritRate, new int[] { 1, 2, 3, 4, 5, 6 } },
			{ Attr.CritDamage, new int[] { 2, 3, 4, 5, 6, 7 } },

			{Attr.Resistance, new int[] { 3, 4, 5, 6, 7, 8 } },
			{Attr.Accuracy, new int[] { 3, 4, 5, 6, 7, 8 } },
		};

		public static readonly int[][] MainValues_Speed = new int[][] {
			new int[]{3,4,5,6,8,9,10,12,13,14,16,17,18,19,21,25},
			new int[]{4,5,7,8,10,11,13,14,16,17,19,20,22,23,25,30},
			new int[]{5,7,9,11,13,15,17,19,21,23,25,27,29,31,33,39},
			new int[]{7,9,11,13,15,17,19,21,23,25,27,29,31,33,35,42}
		};

		public static readonly int[][] MainValues_Flat = new int[][] {
			new int[]{7,12,17,22,27,32,37,42,47,52,57,62,67,72,77,92},
			new int[]{10,16,22,28,34,40,46,52,58,64,70,76,82,88,94,112},
			new int[]{15,22,29,36,43,50,57,64,71,78,85,92,99,106,113,135},
			new int[]{22,30,38,46,54,62,70,78,86,94,102,110,118,126,134,160}
		};

		public static readonly int[][] MainValues_HPflat = new int[][] {
			new int[]{100,175,250,325,400,475,550,625,700,775,850,925,1000,1075,1150,1380},
			new int[]{160,250,340,430,520,610,700,790,880,970,1060,1150,1240,1330,1420,1704},
			new int[]{270,375,480,585,690,795,900,1005,1110,1215,1320,1425,1530,1635,1740,2088},
			new int[]{360,480,600,720,840,960,1080,1200,1320,1440,1560,1680,1800,1920,2040,2448}
		};

		public static readonly int[][] MainValues_Percent = new int[][] {
			new int[]{4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,38},
			new int[]{5,7,9,11,13,16,18,20,22,23,27,29,31,33,36,43},
			new int[]{8,10,12,15,17,20,22,24,27,29,32,34,37,40,43,51},
			new int[]{11,14,17,20,23,26,29,32,35,38,41,44,47,50,53,63}
		};
		public static readonly int[][] MainValues_CRate = new int[][] {
			new int[]{3,5,7,9,11,13,15,17,19,21,23,25,27,29,31,37},
			new int[]{4,6,8,11,13,15,17,19,22,24,26,28,30,33,35,41},
			new int[]{5,7,10,12,15,17,19,22,24,27,29,31,34,36,39,47},
			new int[]{7,10,13,16,19,22,25,28,31,34,37,40,43,46,49,58}
		};

		public static readonly int[][] MainValues_CDmg = new int[][] {
			new int[]{4,6,9,11,13,16,18,20,22,25,27,29,32,34,36,43},
			new int[]{6,9,12,15,18,21,24,27,30,33,36,39,42,45,48,57},
			new int[]{8,11,15,18,21,25,28,31,34,38,41,44,48,51,54,65},
			new int[]{11,15,19,23,27,31,35,39,43,47,51,55,59,63,67,80}
		};

		public static readonly int[][] MainValues_ResAcc = new int[][] {
			new int[] {4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,38},
			new int[] {6,8,10,13,15,17,19,21,24,26,28,30,32,35,37,44},
			new int[] {9,11,14,16,19,21,23,26,28,31,33,35,38,40,43,51},
			new int[] {12,15,18,21,24,27,30,33,36,39,42,45,48,51,54,64}
		};

		public static readonly Dictionary<Attr, int[][]> MainValues = new Dictionary<Attr, int[][]>
		{
			{Attr.HealthFlat, MainValues_HPflat },
			{Attr.AttackFlat, MainValues_Flat },
			{Attr.DefenseFlat, MainValues_Flat },
			{Attr.Speed,MainValues_Speed },

			{Attr.HealthPercent, MainValues_Percent },
			{Attr.AttackPercent, MainValues_Percent },
			{Attr.DefensePercent, MainValues_Percent },

			{Attr.CritRate, MainValues_CRate },
			{Attr.CritDamage, MainValues_CDmg },

			{Attr.Accuracy, MainValues_ResAcc },
			{Attr.Resistance, MainValues_ResAcc },
		};
		
		public void FixShit()
		{
			Accuracy = FixOneShit(Attr.Accuracy);
			AttackFlat = FixOneShit(Attr.AttackFlat);
			AttackPercent = FixOneShit(Attr.AttackPercent);
			CritDamage = FixOneShit(Attr.CritDamage);
			CritRate = FixOneShit(Attr.CritRate);
			DefenseFlat = FixOneShit(Attr.DefenseFlat);
			DefensePercent = FixOneShit(Attr.DefensePercent);
			HealthFlat = FixOneShit(Attr.HealthFlat);
			HealthPercent = FixOneShit(Attr.HealthPercent);
			Resistance = FixOneShit(Attr.Resistance);
			Speed = FixOneShit(Attr.Speed);
		}

		private int[] FixOneShit(Attr a)
		{
			int[] vs = new int[32];
			for (int i = 0; i < 16; i++)
			{
				vs[i] = GetValue(a, i, false);
				vs[i + 16] = GetValue(a, i, true);
			}
			return vs;
		}

		public void SetValue(int p, Attr a, int v)
		{
			switch (p)
			{
				case -1:
					Innate.Type = a;
					Innate.Value = v;
					break;
				case 0:
					Main.Type = a;
					Main.Value = v;
					break;
				case 1:
					Subs[0].Type = a;
					Subs[0].Value = v;
					break;
				case 2:
					Subs[1].Type = a;
					Subs[1].Value = v;
					break;
				case 3:
					Subs[2].Type = a;
					Subs[2].Value = v;
					break;
				case 4:
					Subs[3].Type = a;
					Subs[3].Value = v;
					break;
				default:
					break;
			}
			FixShit();
		}

		#endregion
	}
}
