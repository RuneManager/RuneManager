using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace RuneOptim
{
	public enum EquipCompare
	{
		Unknown,
		Worse,
		Better
	}

	public class Loadout
	{
		private readonly Rune[] runes = new Rune[6];

		private int runeCount = 0;

		[JsonProperty("Sets")]
		private readonly RuneSet[] sets = new RuneSet[3];

		private bool setsFull = false;

		private int[] fakeLevel = new int[6];

		private bool[] predictSubs = new bool[6];

		private int buildID;
		
		public int BuildID { get { return buildID; } set { buildID = value; } }

		[JsonIgnore]
		public Rune[] Runes { get { return runes; } }

		[JsonIgnore]
		public int RuneCount { get { return runeCount; } }

		[JsonIgnore]
		public RuneSet[] Sets { get { return sets; } }
		
		[JsonIgnore]
		public bool SetsFull { get { return setsFull; } }
		
		public double Time;

		public long ActualTests = 0;
		
		[JsonIgnore]
		public ConcurrentDictionary<string, double>[] manageStats;
		
		public ConcurrentDictionary<string, double>[] ManageStats
		{
			get
			{
				if (runes.All(r => r != null))
					return runes.Select(r => r.manageStats).ToArray();
				return null;
			}
			set
			{
				manageStats = value;
			}
		}

		private ulong[] runeIDs;

		public ulong[] RuneIDs
		{
			get
			{
				if (runes.All(r => r != null))
					runeIDs = runes.Select(r => r.Id).ToArray();
				return runeIDs;
			}
			set
			{
				runeIDs = value;
			}
		}

		public int[] FakeLevel { get { return fakeLevel; } set { fakeLevel = value; changed = true; } }
		public bool[] PredictSubs { get { return predictSubs; } set { predictSubs = value; changed = true; } }

		private Stats shrines = new Stats();
		private Stats leader = new Stats();

		private bool changed = false;
		public int runesNew;
		public int runesChanged;
		public int upgrades;
		public int powerup;

		[JsonIgnore]
		public bool Changed { get { return changed; } }

		[JsonIgnore]
		public Stats Shrines
		{
			get
			{
				return shrines;
			}
			set
			{
				shrines = value;
				if (shrines == null)
					shrines = new Stats();
				changed = true;
			}
		}
		public Stats Leader
		{
			get
			{
				return leader;
			}
			set
			{
				leader = value;
				changed = true;
			}
		}
		
		public Loadout(Loadout rhs = null)
		{
			if (rhs != null)
			{
				shrines = rhs.shrines;
				leader = rhs.leader;
				fakeLevel = rhs.fakeLevel;
				predictSubs = rhs.predictSubs;
				buildID = rhs.buildID;
				foreach (var r in rhs.Runes)
				{
					AddRune(r);
				}
				// TODO: do we even need to?
				manageStats = rhs.manageStats;
			}
		}

		// Debugging niceness
		public override string ToString()
		{
			System.Text.StringBuilder str = new System.Text.StringBuilder("");
			foreach (Rune r in runes)
			{
				str.Append(r.Id).Append("  ");
			}
			str.Append("|  ");
			foreach (RuneSet s in sets)
			{
				str.Append(s).Append(" ");
			}
			return str.ToString();
		}

		// Lock all the runes on the build
		public void Lock()
		{
			foreach (Rune r in runes)
			{
				r.Locked = true;
			}
		}

		#region AttrGetters

		[JsonIgnore]
		public int HealthFlat
		{
			get
			{
				return 
					(runes[0]?.HealthFlat[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.HealthFlat[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.HealthFlat[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.HealthFlat[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.HealthFlat[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.HealthFlat[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					SetStat(Attr.HealthFlat);
			}
		}
		[JsonIgnore]
		public int HealthPercent
		{
			get
			{
				return
					(runes[0]?.HealthPercent[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.HealthPercent[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.HealthPercent[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.HealthPercent[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.HealthPercent[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.HealthPercent[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					(int)shrines.Health + 
					(int)leader.Health + 
					SetStat(Attr.HealthPercent);
			}
		}

		[JsonIgnore]
		public int AttackFlat
		{
			get
			{
				return
					(runes[0]?.AttackFlat[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.AttackFlat[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.AttackFlat[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.AttackFlat[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.AttackFlat[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.AttackFlat[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					SetStat(Attr.AttackFlat);
			}
		}
		[JsonIgnore]
		public int AttackPercent
		{
			get
			{
				return
					(runes[0]?.AttackPercent[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.AttackPercent[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.AttackPercent[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.AttackPercent[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.AttackPercent[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.AttackPercent[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					(int)shrines.Attack + 
					(int)leader.Attack + 
					SetStat(Attr.AttackPercent);
			}
		}

		[JsonIgnore]
		public int DefenseFlat
		{
			get
			{
				return
					(runes[0]?.DefenseFlat[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.DefenseFlat[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.DefenseFlat[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.DefenseFlat[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.DefenseFlat[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.DefenseFlat[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					SetStat(Attr.DefenseFlat);
			}
		}
		[JsonIgnore]
		public int DefensePercent
		{
			get
			{
				return
					(runes[0]?.DefensePercent[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.DefensePercent[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.DefensePercent[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.DefensePercent[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.DefensePercent[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.DefensePercent[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					(int)shrines.Defense + 
					(int)leader.Defense + 
					SetStat(Attr.DefensePercent);
			}
		}

		[JsonIgnore]
		public int Speed
		{
			get
			{
				return
					(runes[0]?.Speed[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.Speed[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.Speed[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.Speed[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.Speed[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.Speed[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					SetStat(Attr.Speed);
			}
		}

		// Runes don't get SPD%
		[JsonIgnore]
		public int SpeedPercent
		{
			get
			{
				return SetStat(Attr.SpeedPercent) + (int)shrines.Speed + (int)leader.Speed;
			}
		}

		[JsonIgnore]
		public int CritRate
		{
			get
			{
				return
					(runes[0]?.CritRate[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.CritRate[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.CritRate[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.CritRate[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.CritRate[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.CritRate[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					(int)shrines.CritRate + 
					(int)leader.CritRate + 
					SetStat(Attr.CritRate);
			}
		}

		[JsonIgnore]
		public int CritDamage
		{
			get
			{
				return
					(runes[0]?.CritDamage[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.CritDamage[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.CritDamage[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.CritDamage[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.CritDamage[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.CritDamage[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					(int)shrines.CritDamage + 
					(int)leader.CritDamage + 
					SetStat(Attr.CritDamage);
			}
		}

		[JsonIgnore]
		public int Accuracy
		{
			get
			{
				return
					(runes[0]?.Accuracy[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.Accuracy[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.Accuracy[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.Accuracy[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.Accuracy[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.Accuracy[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					(int)shrines.Accuracy + 
					(int)leader.Accuracy + 
					SetStat(Attr.Accuracy);
			}
		}

		[JsonIgnore]
		public int Resistance
		{
			get
			{
				return
					(runes[0]?.Resistance[predictSubs[0] ? fakeLevel[0] + 16 : fakeLevel[0]] ?? 0) +
					(runes[1]?.Resistance[predictSubs[1] ? fakeLevel[1] + 16 : fakeLevel[1]] ?? 0) +
					(runes[2]?.Resistance[predictSubs[2] ? fakeLevel[2] + 16 : fakeLevel[2]] ?? 0) +
					(runes[3]?.Resistance[predictSubs[3] ? fakeLevel[3] + 16 : fakeLevel[3]] ?? 0) +
					(runes[4]?.Resistance[predictSubs[4] ? fakeLevel[4] + 16 : fakeLevel[4]] ?? 0) +
					(runes[5]?.Resistance[predictSubs[5] ? fakeLevel[5] + 16 : fakeLevel[5]] ?? 0) +
					(int)shrines.Resistance + 
					(int)leader.Resistance + 
					SetStat(Attr.Resistance);
			}
		}

		#endregion

		// Put the rune on the build
		public void AddRune(Rune rune, int checkOn = 2)
		{
			// don't bother if not a rune
			if (rune == null)
				return;

			changed = true;

			RemoveRune(rune.Slot);
			runeCount++;
			
			runes[rune.Slot - 1] = rune;
			if (runeCount % checkOn == 0)
				CheckSets();
		}

		// Removes the rune from slot
		public Rune RemoveRune(int slot)
		{
			// don't bother if there was none
			if (runes[slot - 1] == null)
				return null;

			changed = true;

			var r = runes[slot - 1];
			
			runes[slot - 1] = null;
			runeCount--;
			CheckSets();
			return r;
		}
		
		// Check what sets are completed in this build
		public void CheckSets()
		{
			setsFull = false;
			
			// If there are an odd number of runes, don't bother (maybe even check < 6?)
			if (runeCount % 2 == 1)
				return;

			// can only have 3 sets max (eg. energy / energy / blade)
			sets[0] = 0;
			sets[1] = 0;
			sets[2] = 0;

			// which set we are looking for
			int setInd = 0;
			// what slot we are looking at
			int slotInd = 0;
			// if we have used this slot in a set yet
			bool[] used = new bool[6];

			// how many runes are in sets
			int setNums = 0;

			Rune rune;
			RuneSet set;
			int getNum;
			int gotNum;

			// Check slots 1 - 5, minimum set size is 2.
			// Because it will search forward for sets, can't find more from 6
			// Eg. starting from 6, working around, find 2 runes?
			for (; slotInd < 5; slotInd++)
			{
				//if there is a uncounted rune in this slot
				rune = runes[slotInd];
				if (rune != null && !used[slotInd])
				{
					// look for more in the set
					set = rune.Set;
					// how many runes we need to get
					getNum = Rune.SetRequired(set);
					// how many we got
					gotNum = 1;
					// we have now used this slot
					used[slotInd] = true;

					// for the runes after this rune
					for (int ind = slotInd + 1; ind < 6; ind++)
					{
						// if there is a rune in this slot that is the type I want
						if (runes[ind] != null && !used[ind] && runes[ind].Set == set)
						{
							used[ind] = true;
							gotNum++;
						}

						// if we have more than 1 rune and we have enough runes for a set
						if (gotNum == getNum)
						{
							// log this set
							sets[setInd] = set;
							// increase the number of runes in sets
							setNums += getNum;
							// look for the next set
							setInd++;
							// stop looking forward
							break;
						}
					}
				}
			}

			// if all runes are in sets
			if (setNums == 6)
				setsFull = true;
			// notify hackers their attempt has failed
			else if (setNums > 6)
				throw new Exception("Wut");

		}

		// pull how much Attr is in the equipped sets
		public int SetStat(Attr attr)
		{
			switch (attr)
			{
				case Attr.Null:
				case Attr.HealthFlat:
				case Attr.AttackFlat:
				case Attr.DefenseFlat:
				case Attr.Speed:
				case Attr.ExtraStat:
				case Attr.EffectiveHP:
				case Attr.EffectiveHPDefenseBreak:
				case Attr.DamagePerSpeed:
				case Attr.AverageDamage:
				case Attr.MaxDamage:
					return 0;

				// I could use sets.Where(s => s.Equals(RuneSet.SET)).Count() * BONUS, but it was too slow
				case Attr.HealthPercent:
					return (sets[0] == RuneSet.Energy ? 15 : sets[0] == RuneSet.Enhance ? 8 : 0) +
						(sets[1] == RuneSet.Energy ? 15 : sets[1] == RuneSet.Enhance ? 8 : 0) +
						(sets[2] == RuneSet.Energy ? 15 : sets[2] == RuneSet.Enhance ? 8 : 0);
				// 4 slot sets aren't going to be in [2]
				case Attr.AttackPercent:
					return (sets[0] == RuneSet.Fatal ? 35 : sets[0] == RuneSet.Fight ? 8 : 0) + 
						(sets[1] == RuneSet.Fatal ? 35 : sets[1] == RuneSet.Fight ? 8 : 0) + 
						(sets[2] == RuneSet.Fight ? 8 : 0);
				case Attr.CritRate:
					return (sets[0] == RuneSet.Blade ? 12 : 0) + (sets[1] == RuneSet.Blade ? 12 : 0) + (sets[2] == RuneSet.Blade ? 12 : 0);
				case Attr.CritDamage:
					return sets[0] == RuneSet.Rage ? 40 : sets[1] == RuneSet.Rage ? 40 : 0;
				case Attr.SpeedPercent:
					return sets[0] == RuneSet.Swift ? 25 : sets[1] == RuneSet.Swift ? 25 : 0;
				case Attr.Accuracy:
					return (sets[0] == RuneSet.Focus ? 20 : sets[0] == RuneSet.Accuracy ? 10 : 0) +
						(sets[1] == RuneSet.Focus ? 20 : sets[1] == RuneSet.Accuracy ? 10 : 0) +
						(sets[2] == RuneSet.Focus ? 20 : sets[2] == RuneSet.Accuracy ? 10 : 0);
				case Attr.DefensePercent:
					return (sets[0] == RuneSet.Guard ? 15 : sets[0] == RuneSet.Determination ? 8 : 0) +
						(sets[1] == RuneSet.Guard ? 15 : sets[1] == RuneSet.Determination ? 8 : 0) +
						(sets[2] == RuneSet.Guard ? 15 : sets[2] == RuneSet.Determination ? 8 : 0);
				case Attr.Resistance:
					return (sets[0] == RuneSet.Endure ? 20 : sets[0] == RuneSet.Tolerance ? 10 : 0) +
						(sets[1] == RuneSet.Endure ? 20 : sets[1] == RuneSet.Tolerance ? 10 : 0) +
						(sets[2] == RuneSet.Endure ? 20 : sets[2] == RuneSet.Tolerance ? 10 : 0);
				default:
					return 0;
			}
		}

		private static ParameterExpression statType = Expression.Parameter(typeof(RuneOptim.Stats), "stats");
		private static Func<Stats, Stats> _getStats = null;

		// Using the given stats as a base, apply the modifiers
		public Stats GetStats(Stats baseStats)
		{
			/*if (_getStats == null)
			{
				Expression.
				var expr = ;
				_getStats = Expression.Lambda<Func<Stats, Stats>>(expr, statType).Compile();
			}*/
			
			Stats value = new Stats(baseStats);

			// Apply percent before flat
			value.Health += (int)Math.Ceiling(baseStats.Health * HealthPercent * 0.01) + HealthFlat;
			value.Attack += (int)Math.Ceiling(baseStats.Attack * AttackPercent * 0.01) + AttackFlat;
			value.Defense += (int)Math.Ceiling(baseStats.Defense * DefensePercent * 0.01) + DefenseFlat;
			value.Speed += (int)Math.Ceiling(baseStats.Speed * SpeedPercent * 0.01) + Speed;

			value.CritDamage += CritDamage;
			value.CritRate += CritRate;

			value.Accuracy += Accuracy;
			value.Resistance += Resistance;

			changed = false;

			return value;
		}

		/*// NYI comparison
		public EquipCompare CompareTo(Loadout rhs)
		{
			// check if the sets are comparable
			if (CompareSets(sets, rhs.sets) == 0)
				return EquipCompare.Unknown;

			int side = 0;
			if (HealthPercent > rhs.HealthPercent)
				side++;
			else
				side--;

			if (AttackPercent > rhs.AttackPercent)
				side++;
			else
				side--;

			if (DefensePercent > rhs.DefensePercent)
				side++;
			else
				side--;

			if (Speed > rhs.Speed)

			return EquipCompare.Unknown;
		}
		*/

		// TODO: return an enum?
		// NYI comparison
		// 0 = differing magical sets
		// 1 = exact match
		// 2 = none or no differing magic sets
		public static int CompareSets(RuneSet[] a, RuneSet[] b)
		{
			if (a == b)
				return 1;

			if (a.Length == b.Length)
			{
				int same = 0;
				for (int i = 0; i < a.Length; i++)
				{
					if (a[i] == b[i])
						same++;
				}
				if (same == a.Length)
					return 1;
			}
			// different lengths, or not the same sets

			// count the number of magical sets, and make sure both loadout have the same number of sets
			foreach (RuneSet s in RuneProperties.MagicalSets)
			{
				if (a.Count(x => x == s) != b.Count(x => x == s))
					return 0;
			}

			return 1;
		}

		public void RecountDiff(ulong monId)
		{
			powerup = 0;
			upgrades = 0;
			runesNew = 0;
			runesChanged = 0;

			foreach (Rune r in Runes)
			{
				r.Locked = true;
				if (r.AssignedId != monId)
				{
					if (r.IsUnassigned)
						runesNew++;
					else
						runesChanged++;
				}
				powerup += Math.Max(0, (FakeLevel[r.Slot - 1]) - r.Level);
				if (FakeLevel[r.Slot - 1] != 0)
				{
					int tup = (int)Math.Floor(Math.Min(12, (FakeLevel[r.Slot - 1])) / (double)3);
					int cup = (int)Math.Floor(Math.Min(12, r.Level) / (double)3);
					upgrades += Math.Max(0, tup - cup);
				}
			}
		}
	}
}
