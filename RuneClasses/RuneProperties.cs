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
	public partial class Rune
	{
		public static readonly RuneSet[] RuneSets = new RuneSet[] { RuneSet.Energy, // Health
			RuneSet.Guard, // Def
			RuneSet.Swift, // Speed
			RuneSet.Blade, // CRate
			RuneSet.Rage, // CDmg
			RuneSet.Focus, // Acc
			RuneSet.Endure, // Res
			RuneSet.Fatal, // Attack

			// Here be magic
			RuneSet.Despair,
			RuneSet.Vampire,

			RuneSet.Violent,
			RuneSet.Nemesis,
			RuneSet.Will,
			RuneSet.Shield,
			RuneSet.Revenge,
			RuneSet.Destroy,

			// Ally sets
			RuneSet.Fight,
			RuneSet.Determination,
			RuneSet.Enhance,
			RuneSet.Accuracy,
			RuneSet.Tolerance,
		};

		[JsonIgnore]
		public double Efficiency
		{
			get
			{
				double num = 0;
				num += this.GetEfficiency(Innate.Type, Innate.Value);
				if (Subs.Count > 0)
					num += this.GetEfficiency(Subs[0].Type, Subs[0].Value);
				if (Subs.Count > 1)
					num += this.GetEfficiency(Subs[1].Type, Subs[1].Value);
				if (Subs.Count > 2)
					num += this.GetEfficiency(Subs[2].Type, Subs[2].Value);
				if (Subs.Count > 3)
					num += this.GetEfficiency(Subs[3].Type, Subs[3].Value);

				num /= 1.8;
				return num;
			}
		}
	}

	public static class RuneProperties
	{
		// these sets can't be superceded by really good stats
		// Eg. Blade can be replaced by 12% crit.
		public static readonly RuneSet[] MagicalSets =
		{
			RuneSet.Violent, RuneSet.Will, RuneSet.Nemesis, RuneSet.Shield, RuneSet.Revenge, RuneSet.Despair, RuneSet.Vampire, RuneSet.Destroy,
			RuneSet.Tolerance, RuneSet.Accuracy, RuneSet.Determination, RuneSet.Enhance, RuneSet.Fight,
		};

		public static int FlatCount(this Rune rune)
		{
			int count = 0;
			if (rune.Subs.Count == 0 || rune.Subs[0].Type == Attr.Null) return count;
			count += (rune.Subs[0].Type == Attr.HealthFlat || rune.Subs[0].Type == Attr.DefenseFlat || rune.Subs[0].Type == Attr.AttackFlat) ? 1 : 0;
			if (rune.Subs.Count == 1 || rune.Subs[1].Type == Attr.Null) return count;
			count += (rune.Subs[1].Type == Attr.HealthFlat || rune.Subs[1].Type == Attr.DefenseFlat || rune.Subs[1].Type == Attr.AttackFlat) ? 1 : 0;
			if (rune.Subs.Count == 2 || rune.Subs[2].Type == Attr.Null) return count;
			count += (rune.Subs[2].Type == Attr.HealthFlat || rune.Subs[2].Type == Attr.DefenseFlat || rune.Subs[2].Type == Attr.AttackFlat) ? 1 : 0;
			if (rune.Subs.Count == 3 || rune.Subs[3].Type == Attr.Null) return count;
			count += (rune.Subs[3].Type == Attr.HealthFlat || rune.Subs[3].Type == Attr.DefenseFlat || rune.Subs[3].Type == Attr.AttackFlat) ? 1 : 0;

			return count;
		}
		
		public static AttributeCategory getSetType(this Rune rune)
		{
			var neutral = new RuneSet[] { RuneSet.Violent, RuneSet.Swift, RuneSet.Focus, RuneSet.Nemesis };
			if (new RuneSet[] { RuneSet.Fatal, RuneSet.Blade, RuneSet.Rage, RuneSet.Vampire, RuneSet.Revenge }.Contains(rune.Set)) // o
				return AttributeCategory.Offensive;
			else if (new RuneSet[] { RuneSet.Energy, RuneSet.Endure, RuneSet.Guard, RuneSet.Shield, RuneSet.Will }.Contains(rune.Set)) // d
				return AttributeCategory.Defensive;
			else if (new RuneSet[] { RuneSet.Despair, RuneSet.Determination, RuneSet.Enhance, RuneSet.Fight, RuneSet.Accuracy, RuneSet.Tolerance }.Contains(rune.Set)) // s
				return AttributeCategory.Support;

			return AttributeCategory.Neutral;
		}

		[JsonIgnore]
		private static readonly Attr[] attackSubs = new Attr[] { Attr.AttackPercent, Attr.CritDamage, Attr.CritRate };
		[JsonIgnore]
		private static readonly Attr[] defenseSubs = new Attr[] { Attr.HealthPercent, Attr.DefensePercent };
		[JsonIgnore]
		private static readonly Attr[] supportSubs = new Attr[] { Attr.Accuracy, Attr.Resistance };

		public static double ComputeRating(this Rune rune)
		{
			double r = 0;
			var type = rune.getSetType();

			// for each sub (if flat = 0, null = 0.3, else 1)

			// set types
			// stat types
			// offense/defense/support/neutral

			var subs = new Attr[4];
			if (rune.Subs.Count > 0)
				subs[0] = rune.Subs[0].Type;
			if (rune.Subs.Count > 1)
				subs[1] = rune.Subs[1].Type;
			if (rune.Subs.Count > 2)
				subs[2] = rune.Subs[2].Type;
			if (rune.Subs.Count > 3)
				subs[3] = rune.Subs[3].Type;

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

			if (rune.Grade == 5)
				r += 4;
			else if (rune.Grade == 6)
				r += 7;

			return r;
		}

		private static double computeSubTypeRating(AttributeCategory type, Attr sub, out AttributeCategory subt)
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

		private static double computeSubRating(Attr sub, Attr[] subs, AttributeCategory subt)
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
		private static readonly Attr[] hpStats = new Attr[] { Attr.HealthPercent, Attr.DefensePercent, Attr.Resistance, Attr.HealthFlat, Attr.DefenseFlat };

		public static double ScoreHP(this Rune rune)
		{
			double v = 0;

			v += rune.HealthPercent[0] / (double)subMaxes[Attr.HealthPercent];
			v += rune.DefensePercent[0] / (double)subMaxes[Attr.DefensePercent];
			v += rune.Resistance[0] / (double)subMaxes[Attr.Resistance];
			v += 0.5 * rune.HealthFlat[0] / subMaxes[Attr.HealthFlat];
			v += 0.5 * rune.DefenseFlat[0] / subMaxes[Attr.DefenseFlat];

			if (rune.Main.Type == Attr.HealthPercent || rune.Main.Type == Attr.DefensePercent || rune.Main.Type == Attr.Resistance)
			{
				v -= rune.Main.Value / (double)subMaxes[rune.Main.Type];
				if (rune.Slot % 2 == 0)
					v += rune.Grade / (double)6;
			}
			else if (rune.Main.Type == Attr.DefenseFlat || rune.Main.Type == Attr.HealthFlat)
			{
				v -= 0.5 * rune.Main.Value / subMaxes[rune.Main.Type];
				if (rune.Slot % 2 == 0)
					v += rune.Grade / (double)6;
			}

			double d = 0.5;
			if (rune.Slot == 3 || rune.Slot == 5)
				d += 0.2;

			if (rune.Slot % 2 == 0)
				d += 1;

			var stt = 0;
			if (rune.Subs.Count > 0 && hpStats.Contains(rune.Subs[0].Type))
				stt++;
			if (rune.Subs.Count > 1 && hpStats.Contains(rune.Subs[1].Type))
				stt++;
			if (rune.Subs.Count > 2 && hpStats.Contains(rune.Subs[2].Type))
				stt++;
			if (rune.Subs.Count > 3 && hpStats.Contains(rune.Subs[3].Type))
				stt++;

			d += 0.2 * (
				rune.Rarity
				- (
					rune.Rarity
					- Math.Floor(rune.Level / (double)3)
				) * stt
			);

			return v / d;
		}

		[JsonIgnore]
		private static readonly Attr[] atkStats = new Attr[] { Attr.AttackFlat, Attr.AttackPercent, Attr.CritRate, Attr.CritDamage };

		public static double ScoreATK(this Rune rune)
		{
			double v = 0;

			v += rune.AttackPercent[0] / (double)subMaxes[Attr.AttackPercent];
			v += rune.CritRate[0] / (double)subMaxes[Attr.CritRate];
			v += rune.CritDamage[0] / (double)subMaxes[Attr.CritDamage];
			v += 0.5 * rune.AttackFlat[0] / subMaxes[Attr.AttackFlat];

			if (rune.Main.Type == Attr.AttackPercent || rune.Main.Type == Attr.CritRate || rune.Main.Type == Attr.CritDamage)
			{
				v -= rune.Main.Value / (double)subMaxes[rune.Main.Type];
				if (rune.Slot % 2 == 0)
					v += rune.Grade / (double)6;
			}
			else if (rune.Main.Type == Attr.AttackFlat)
			{
				v -= 0.5 * rune.Main.Value / subMaxes[rune.Main.Type];
				if (rune.Slot % 2 == 0)
					v += rune.Grade / (double)6;
			}

			double d = 0.4;

			if (rune.Slot == 1)
				d += 0.2;

			if (rune.Slot % 2 == 0)
				d += 1.1;

			var stt = 0;
			if (rune.Subs.Count > 0 && atkStats.Contains(rune.Subs[0].Type))
				stt++;
			if (rune.Subs.Count > 1 && atkStats.Contains(rune.Subs[1].Type))
				stt++;
			if (rune.Subs.Count > 2 && atkStats.Contains(rune.Subs[2].Type))
				stt++;
			if (rune.Subs.Count > 3 && atkStats.Contains(rune.Subs[3].Type))
				stt++;

			d += 0.2 * (
				rune.Rarity
				- (
					rune.Rarity
					- Math.Floor(rune.Level / (double)3)
				) * stt
			);

			return v / d;
		}

		public static double ScoreRune(this Rune rune)
		{
			double v = 0;

			if (rune.Innate != null && rune.Innate.Value > 0)
			{
				if (rune.Innate.Type == Attr.AttackFlat || rune.Innate.Type == Attr.HealthFlat || rune.Innate.Type == Attr.DefenseFlat)
					v += 0.5 * rune.Innate.Value / subMaxes[rune.Innate.Type];
				else
					v += rune.Innate.Value / (double)subMaxes[rune.Innate.Type];
			}

			if (rune.Subs.Count > 0 && rune.Subs[0].Value > 0)
			{
				if (rune.Subs[0].Type == Attr.AttackFlat || rune.Subs[0].Type == Attr.HealthFlat || rune.Subs[0].Type == Attr.DefenseFlat)
					v += 0.5 * rune.Subs[0].Value / subMaxes[rune.Subs[0].Type];
				else
					v += rune.Subs[0].Value / (double)subMaxes[rune.Subs[0].Type];
			}

			if (rune.Subs.Count > 1 && rune.Subs[1].Value > 0)
			{
				if (rune.Subs[1].Type == Attr.AttackFlat || rune.Subs[1].Type == Attr.HealthFlat || rune.Subs[1].Type == Attr.DefenseFlat)
					v += 0.5 * rune.Subs[1].Value / subMaxes[rune.Subs[1].Type];
				else
					v += rune.Subs[1].Value / (double)subMaxes[rune.Subs[1].Type];
			}

			if (rune.Subs.Count > 2 && rune.Subs[2].Value > 0)
			{
				if (rune.Subs[2].Type == Attr.AttackFlat || rune.Subs[2].Type == Attr.HealthFlat || rune.Subs[2].Type == Attr.DefenseFlat)
					v += 0.5 * rune.Subs[2].Value / subMaxes[rune.Subs[2].Type];
				else
					v += rune.Subs[2].Value / (double)subMaxes[rune.Subs[2].Type];
			}

			if (rune.Subs.Count > 3 && rune.Subs[3].Value > 0)
			{
				if (rune.Subs[3].Type == Attr.AttackFlat || rune.Subs[3].Type == Attr.HealthFlat || rune.Subs[3].Type == Attr.DefenseFlat)
					v += 0.5 * rune.Subs[3].Value / subMaxes[rune.Subs[3].Type];
				else
					v += rune.Subs[3].Value / (double)subMaxes[rune.Subs[3].Type];
			}

			v += rune.Grade / (double)6;

			double d = 2;
			d += 0.2 * Math.Min(4, Math.Floor(rune.Level / (double)3));

			return v / d;
		}

		public static double GetEfficiency(this Rune rune, Attr a, int val)
		{
			if (a == Attr.Null)
				return 0;
			if (a == Attr.HealthFlat || a == Attr.AttackFlat || a == Attr.DefenseFlat)
				return val / (double)2 / (5 * subUpgrades[a][rune.Grade - 1]);

			return val / (double)(5 * subUpgrades[a][rune.Grade - 1]);
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

		#endregion
	}

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
	
	public class RuneAttr : ListProp<int?>
	{
		[ListProperty(0)]
		public Attr Type = default(Attr);

		[ListProperty(1)]
		public int BaseValue = -1;

		[ListProperty(2)]
		public int __int2 = -1;

		[ListProperty(3)]
		public int GrindBonus = -1;

		[JsonIgnore]
		private int _calcVal = -1;

		protected override void OnSet(int i, int? val)
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

		#region Remove the slow attribute checking
		protected override int maxInd { get { return 4; } }

		public override bool IsReadOnly { get { return false; } }

		public override int? this[int index]
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
					BaseValue = value ?? -1;
				else if (index == 2)
					__int2 = value ?? -1;
				else if (index == 3)
					GrindBonus = value ?? -1;
			}
		}

		public override void Add(int? item)
		{
			if (Type == Attr.Null)
				Type = (Attr)item;
			else if (BaseValue == -1)
				BaseValue = item ?? -1;
			else if (__int2 == -1)
				__int2 = item ?? -1;
			else if (GrindBonus == -1)
				GrindBonus = item ?? -1;
			else
				throw new IndexOutOfRangeException();
		}
		#endregion
	}

	public class RuneLink
	{
		[JsonProperty("rune_id")]
		public ulong Id { get; set; }

		[JsonProperty("occupied_id")]
		public ulong AssignedId { get; set; }
	}

}
