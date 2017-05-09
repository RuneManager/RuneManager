using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq.Expressions;

namespace RuneOptim
{
	[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	sealed class SkillAttrAttribute : Attribute
	{
		readonly string attrName;

		public SkillAttrAttribute(string name)
		{
			this.attrName = name;
		}

		public string Attr
		{
			get { return attrName; }
		}

	}

	[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	sealed class AttrFieldAttribute : Attribute
	{
		readonly Attr attrName;

		public AttrFieldAttribute(Attr attr)
		{
			this.attrName = attr;
		}

		public Attr attr
		{
			get { return attrName; }
		}

	}

	// Allows me to steal the JSON values into Enum
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Attr
	{
		[EnumMember(Value = "-")]
		Neg = -1,

		[EnumMember(Value = "")]
		// TODO: FIXME:
		[SkillAttr("DIE_RATE")]
		[SkillAttr("ATTACK_CUR_HP_RATE")]
		[SkillAttr("TARGET_CUR_HP_RATE")]
		[SkillAttr("ATTACK_LV")]
		Null = 0,

		[EnumMember(Value = "HP flat")]
		[SkillAttr("HP")]
		[SkillAttr("ATTACK_CUR_HP")]
		[SkillAttr("ATTACK_TOT_HP")]
		[SkillAttr("ATTACK_LOSS_HP")]
		[SkillAttr("TARGET_TOT_HP")]
		[SkillAttr("LIFE_SHARE_ALL")]
		HealthFlat = 1,

		[EnumMember(Value = "HP%")]
		HealthPercent = 2,

		[EnumMember(Value = "ATK flat")]
		[SkillAttr("ATK")]
		AttackFlat = 3,

		[EnumMember(Value = "ATK%")]
		AttackPercent = 4,

		[EnumMember(Value = "DEF flat")]
		[SkillAttr("DEF")]
		DefenseFlat = 5,

		[EnumMember(Value = "DEF%")]
		DefensePercent = 6,

		// Thanks Swift -_-
		SpeedPercent = 7,

		[EnumMember(Value = "SPD")]
		[SkillAttr("ATTACK_SPEED")]
		Speed = 8,

		[EnumMember(Value = "CRate")]
		CritRate = 9,

		[EnumMember(Value = "CDmg")]
		CritDamage = 10,

		[EnumMember(Value = "RES")]
		Resistance = 11,

		[EnumMember(Value = "ACC")]
		Accuracy = 12,

		// Flag for below
		ExtraStat = 16,

		[EnumMember(Value = "EHP")]
		EffectiveHP = 1 | ExtraStat,

		[EnumMember(Value = "EHPDB")]
		EffectiveHPDefenseBreak = 2 | ExtraStat,

		[EnumMember(Value = "DPS")]
		DamagePerSpeed = 3 | ExtraStat,

		[EnumMember(Value = "AvD")]
		AverageDamage = 4 | ExtraStat,

		[EnumMember(Value = "MxD")]
		MaxDamage = 5 | ExtraStat,

		[EnumMember(Value = "Skill1")]
		Skill1 = 6 | ExtraStat,

		[EnumMember(Value = "Skill2")]
		Skill2 = 7 | ExtraStat,

		[EnumMember(Value = "Skill3")]
		Skill3 = 8 | ExtraStat,

		[EnumMember(Value = "Skill4")]
		Skill4 = 9 | ExtraStat,
	}

	public enum AttributeCategory
	{
		Neutral,
		Offensive,
		Defensive,
		Support
	}

	public class Stats
	{
		// allows mapping save.json into the program via Monster
		[JsonProperty("con")]
		public double _con = 0;

		[JsonProperty("hp")]
		public double? _health = null;

		// TODO: should I set con?
		[JsonIgnore]
		public double Health { get { return _health ??  _con * 15; } set { _con = value / 15.0; _health = value; } }

		[JsonProperty("atk")]
		public double Attack = 0;

		[JsonProperty("def")]
		public double Defense = 0;

		[JsonProperty("spd")]
		public double Speed = 0;

		[JsonProperty("critical_rate")]
		public double CritRate = 0;

		[JsonProperty("critical_damage")]
		public double CritDamage = 0;

		[JsonProperty("resist")]
		public double Resistance = 0;

		[JsonProperty("accuracy")]
		public double Accuracy = 0;

		[JsonIgnore]
		public double SkillupDamage = 0;

		[JsonProperty("skillup_damage")]
		public double[] DamageSkillups = new double[8];

		[JsonProperty("skill_cooltime")]
		public int[] SkillTimes = new int[8];

		public Stats() { }
		// copy constructor, amrite?
		public Stats(Stats rhs, bool copyExtra = false)
		{
			Health = rhs.Health;
			Attack = rhs.Attack;
			Defense = rhs.Defense;
			Speed = rhs.Speed;
			CritRate = rhs.CritRate;
			CritDamage = rhs.CritDamage;
			Resistance = rhs.Resistance;
			Accuracy = rhs.Accuracy;

			damageFormula = rhs.damageFormula;
			_damageFormula = rhs._damageFormula;

			//rhs._skillsFormula.CopyTo(_skillsFormula, 0);
			//rhs.DamageSkillups.CopyTo(DamageSkillups, 0);
			_skillsFormula = rhs._skillsFormula;
			DamageSkillups = rhs.DamageSkillups;

			if (copyExtra)
			{
				EffectiveHP = rhs.EffectiveHP;
				EffectiveHPDefenseBreak = rhs.EffectiveHPDefenseBreak;
				DamagePerSpeed = rhs.DamagePerSpeed;
				AverageDamage = rhs.AverageDamage;
				MaxDamage = rhs.MaxDamage;
			}
		}

		// fake "stats", need to be stored for scoring
		[JsonProperty("fake_ehp")]
		public double EffectiveHP = 0;

		[JsonProperty("fake_ehpdb")]
		public double EffectiveHPDefenseBreak = 0;

		[JsonProperty("fake_dps")]
		public double DamagePerSpeed = 0;

		[JsonProperty("fake_avd")]
		public double AverageDamage = 0;

		[JsonProperty("fake_mxd")]
		public double MaxDamage = 0;


		[JsonIgnore]
		public MonsterDefinitions.MultiplierBase damageFormula = null;// new MonsterDefinitions.MultiplierValue(Attr.AttackFlat);

		[JsonIgnore]
		protected static ParameterExpression statType = Expression.Parameter(typeof(RuneOptim.Stats), "stats");

		[JsonIgnore]
		private Func<Stats, double> _damageFormula = null;

		[JsonIgnore]
		protected Func<Stats, double>[] _skillsFormula = new Func<Stats, double>[8];

		[JsonIgnore]
		private Expression __form = null;

		[JsonIgnore]
		public Func<Stats, double> DamageFormula
		{
			get
			{
				if (_damageFormula == null)
				{
					__form = damageFormula.AsExpression(statType);
					_damageFormula = Expression.Lambda<Func<Stats, double>>(__form, statType).Compile();
				}
				return _damageFormula;
			}
		}

		public Func<Stats, double>[] SkillFunc
		{
			get
			{
				return _skillsFormula;
			}
		}

		public double GetSkillMultiplier(int skillNum, Stats applyTo = null)
		{
			if (_skillsFormula.Length < skillNum || _skillsFormula[skillNum] == null)
				return 0;
			if (applyTo == null)
				applyTo = this;
			return this._skillsFormula[skillNum](applyTo);
		}

		public double GetSkillDamage(Attr type, int skillNum, Stats applyTo = null)
		{
			var mult = GetSkillMultiplier(skillNum, applyTo);

			if (type == Attr.MaxDamage)
			{
				return mult * (1 + CritDamage + DamageSkillups[skillNum] * 0.01);
			}
			else if (type == Attr.AverageDamage)
			{
				return mult * (1 + CritDamage * CritRate + DamageSkillups[skillNum] * 0.01);
			}
			else if (type == Attr.DamagePerSpeed)
			{
				return mult * (1 + CritDamage * CritRate + DamageSkillups[skillNum] * 0.01) * Speed / SkillTimes[skillNum];
			}

			return mult;
		}

		// Gets the Extra stat manually stored (for scoring)
		public double ExtraGet(string extra)
		{
			switch (extra)
			{
				case "EHP":
					return EffectiveHP;
				case "EHPDB":
					return EffectiveHPDefenseBreak;
				case "DPS":
					return DamagePerSpeed;
				case "AvD":
					return AverageDamage;
				case "MxD":
					return MaxDamage;
				default:
					throw new NotImplementedException();
			}
		}

		public double ExtraGet(Attr extra)
		{
			switch (extra)
			{
				case Attr.EffectiveHP:
					return EffectiveHP;
				case Attr.EffectiveHPDefenseBreak:
					return EffectiveHPDefenseBreak;
				case Attr.DamagePerSpeed:
					return DamagePerSpeed;
				case Attr.AverageDamage:
					return AverageDamage;
				case Attr.MaxDamage:
					return MaxDamage;
				case Attr.Skill1:
				case Attr.Skill2:
				case Attr.Skill3:
				case Attr.Skill4:
					return DamageSkillups[(int)(extra - Attr.Skill1)];
				default:
					throw new NotImplementedException();
			}
		}

		// Computes and returns the Extra stat
		public double ExtraValue(string extra)
		{
			switch (extra)
			{
				case "EHP":
					return ExtraValue(Attr.EffectiveHP);
				case "EHPDB":
					return ExtraValue(Attr.EffectiveHPDefenseBreak);
				case "DPS":
					return ExtraValue(Attr.DamagePerSpeed);
				case "AvD":
					return ExtraValue(Attr.AverageDamage);
				case "MxD":
					return ExtraValue(Attr.MaxDamage);
				default:
					throw new NotImplementedException();
			}
		}

		public double ExtraValue(Attr extra)
		{
			switch (extra)
			{
				case Attr.EffectiveHP:
					return Health / ((1000 / (1000 + Defense * 3)));
				case Attr.EffectiveHPDefenseBreak:
					return Health / ((1000 / (1000 + Defense * 3 * 0.3)));
				case Attr.DamagePerSpeed:
					return ExtraValue(Attr.AverageDamage) * Speed / 100;
				case Attr.AverageDamage:
					return DamageFormula(this) * (1 + SkillupDamage + CritDamage / 100 * Math.Min(CritRate, 100) / 100);
				case Attr.MaxDamage:
					return DamageFormula(this) * (1 + SkillupDamage + CritDamage / 100);
				default:
					throw new NotImplementedException();
			}
		}

		// manually sets the Extra stat (used for scoring)
		public void ExtraSet(string extra, double value)
		{
			switch (extra)
			{
				case "EHP":
					EffectiveHP = value;
					break;
				case "EHPDB":
					EffectiveHPDefenseBreak = value;
					break;
				case "DPS":
					DamagePerSpeed = value;
					break;
				case "AvD":
					AverageDamage = value;
					break;
				case "MxD":
					MaxDamage = value;
					break;
				default:
					throw new NotImplementedException();
			}
		}

		// manually sets the Extra stat (used for scoring)
		public void ExtraSet(Attr extra, double value)
		{
			switch (extra)
			{
				case Attr.EffectiveHP:
					EffectiveHP = value;
					break;
				case Attr.EffectiveHPDefenseBreak:
					EffectiveHPDefenseBreak = value;
					break;
				case Attr.DamagePerSpeed:
					DamagePerSpeed = value;
					break;
				case Attr.AverageDamage:
					AverageDamage = value;
					break;
				case Attr.MaxDamage:
					MaxDamage = value;
					break;
				case Attr.Skill1:
				case Attr.Skill2:
				case Attr.Skill3:
				case Attr.Skill4:
					DamageSkillups[(int)(extra - Attr.Skill1)] = value;
					break;
				default:
					throw new NotImplementedException();
			}
		}
		
		public void SetZero()
		{
			Accuracy = 0;
			Attack = 0;
			CritDamage = 0;
			CritRate = 0;
			Defense = 0;
			Health = 0;
			Resistance = 0;
			Speed = 0;

			EffectiveHP = 0;
			EffectiveHPDefenseBreak = 0;
			DamagePerSpeed = 0;
			AverageDamage = 0;
			MaxDamage = 0;
			DamageSkillups = new double[8];
		}

		public double Sum()
		{
			return Accuracy
				+ Attack
				+ CritDamage
				+ CritRate
				+ Defense
				+ Health
				+ Resistance
				+ Speed
				+ EffectiveHP
				+ EffectiveHPDefenseBreak
				+ DamagePerSpeed
				+ AverageDamage
				+ MaxDamage
				+ DamageSkillups.Sum();
		}

		// Allows speedy iteration through the entity
		public double this[string stat]
		{
			get
			{
				// TODO: switch from using [string] to [Attr]
				switch (stat)
				{
					case "HP":
						return Health;
					case "ATK":
						return Attack;
					case "DEF":
						return Defense;
					case "SPD":
						return Speed;
					case "CD":
						return CritDamage;
					case "CR":
						return CritRate;
					case "ACC":
						return Accuracy;
					case "RES":
						return Resistance;
					default:
						return 0;
						//throw new NotImplementedException();
				}
			}

			set
			{
				switch (stat)
				{
					case "HP":
						Health = value;
						break;
					case "ATK":
						Attack = value;
						break;
					case "DEF":
						Defense = value;
						break;
					case "SPD":
						Speed = value;
						break;
					case "CD":
						CritDamage = value;
						break;
					case "CR":
						CritRate = value;
						break;
					case "ACC":
						Accuracy = value;
						break;
					case "RES":
						Resistance = value;
						break;
					default:
						break;
						//throw new NotImplementedException();
				}
			}

		}

		public double this[Attr stat]
		{
			get
			{
				switch (stat)
				{
					case Attr.HealthFlat:
					case Attr.HealthPercent:
						return Health;
					case Attr.AttackFlat:
					case Attr.AttackPercent:
						return Attack;
					case Attr.DefenseFlat:
					case Attr.DefensePercent:
						return Defense;
					case Attr.Speed:
					case Attr.SpeedPercent:
						return Speed;
					case Attr.CritDamage:
						return CritDamage;
					case Attr.CritRate:
						return CritRate;
					case Attr.Accuracy:
						return Accuracy;
					case Attr.Resistance:
						return Resistance;
					case Attr.EffectiveHP:
						return EffectiveHP;
					case Attr.EffectiveHPDefenseBreak:
						return EffectiveHPDefenseBreak;
					case Attr.DamagePerSpeed:
						return DamagePerSpeed;
					case Attr.AverageDamage:
						return AverageDamage;
					case Attr.MaxDamage:
						return MaxDamage;

				}
				throw new NotImplementedException();
			}

			set
			{
				switch (stat)
				{
					case Attr.HealthFlat:
					case Attr.HealthPercent:
						Health = value;
						break;
					case Attr.AttackFlat:
					case Attr.AttackPercent:
						Attack = value;
						break;
					case Attr.DefenseFlat:
					case Attr.DefensePercent:
						Defense = value;
						break;
					case Attr.Speed:
					case Attr.SpeedPercent:
						Speed = value;
						break;
					case Attr.CritDamage:
						CritDamage = value;
						break;
					case Attr.CritRate:
						CritRate = value;
						break;
					case Attr.Accuracy:
						Accuracy = value;
						break;
					case Attr.Resistance:
						Resistance = value;
						break;
					case Attr.EffectiveHP:
						EffectiveHP = value;
						break;
					case Attr.EffectiveHPDefenseBreak:
						EffectiveHPDefenseBreak = value;
						break;
					case Attr.DamagePerSpeed:
						DamagePerSpeed = value;
						break;
					case Attr.AverageDamage:
						AverageDamage = value;
						break;
					case Attr.MaxDamage:
						MaxDamage = value;
						break;
					default:
						throw new NotImplementedException();
				}
			}

		}

		// Perfectly legit operator overloading to compare builds/minimum
		public static bool operator <(Stats lhs, Stats rhs)
		{
			return rhs.GreaterEqual(lhs);
		}

		public static bool operator >(Stats lhs, Stats rhs)
		{
			return lhs.GreaterEqual(rhs);
		}

		/// <summary>
		/// Compares this to rhs returning if any non-zero attribute on RHS is exceeded by this.
		/// </summary>
		/// <param name="rhs">Stats to compare to</param>
		/// <returns>If any values in this Stats are greater than rhs</returns>
		public bool CheckMax(Stats rhs)
		{
			return Build.statEnums.Any(s => rhs[s] != 0 && this[s] > rhs[s]);
		}

		public bool GreaterEqual(Stats rhs, bool extraGet = false)
		{
			if (Accuracy < rhs.Accuracy)
				return false;
			if (Attack < rhs.Attack)
				return false;
			if (CritDamage < rhs.CritDamage)
				return false;
			if (CritRate < rhs.CritRate)
				return false;
			if (Defense < rhs.Defense)
				return false;
			if (Health < rhs.Health)
				return false;
			if (Resistance < rhs.Resistance)
				return false;
			if (Speed < rhs.Speed)
				return false;

			if (!extraGet) return true;

			if (ExtraValue(Attr.EffectiveHP) < rhs.EffectiveHP)
				return false;
			if (ExtraValue(Attr.EffectiveHPDefenseBreak) < rhs.EffectiveHPDefenseBreak)
				return false;
			if (ExtraValue(Attr.DamagePerSpeed) < rhs.DamagePerSpeed)
				return false;
			if (ExtraValue(Attr.AverageDamage) < rhs.AverageDamage)
				return false;
			if (ExtraValue(Attr.MaxDamage) < rhs.MaxDamage)
				return false;

			if (_skillsFormula[0] != null && _skillsFormula[0](this) < rhs.DamageSkillups[0])
				return false;
			if (_skillsFormula[1] != null && _skillsFormula[1](this) < rhs.DamageSkillups[1])
				return false;
			if (_skillsFormula[2] != null && _skillsFormula[2](this) < rhs.DamageSkillups[2])
				return false;
			if (_skillsFormula[3] != null && _skillsFormula[3](this) < rhs.DamageSkillups[3])
				return false;

			return true;
		}

		public static Stats operator +(Stats lhs, Stats rhs)
		{
			Stats ret = new Stats(lhs, true);
			ret.Health += rhs.Health;
			ret.Attack += rhs.Attack;
			ret.Defense += rhs.Defense;
			ret.Speed += rhs.Speed;
			ret.CritRate += rhs.CritRate;
			ret.CritDamage += rhs.CritDamage;
			ret.Resistance += rhs.Resistance;
			ret.Accuracy += rhs.Accuracy;
			ret.EffectiveHP += rhs.EffectiveHP;
			ret.EffectiveHPDefenseBreak += rhs.EffectiveHPDefenseBreak;
			ret.DamagePerSpeed += rhs.DamagePerSpeed;
			ret.AverageDamage += rhs.AverageDamage;
			ret.MaxDamage += rhs.MaxDamage;
			return ret;
		}

		public static Stats operator -(Stats lhs, Stats rhs)
		{
			Stats ret = new Stats(lhs, true);
			ret.Health -= rhs.Health;
			ret.Attack -= rhs.Attack;
			ret.Defense -= rhs.Defense;
			ret.Speed -= rhs.Speed;
			ret.CritRate -= rhs.CritRate;
			ret.CritDamage -= rhs.CritDamage;
			ret.Resistance -= rhs.Resistance;
			ret.Accuracy -= rhs.Accuracy;
			ret.EffectiveHP -= rhs.EffectiveHP;
			ret.EffectiveHPDefenseBreak -= rhs.EffectiveHPDefenseBreak;
			ret.DamagePerSpeed -= rhs.DamagePerSpeed;
			ret.AverageDamage -= rhs.AverageDamage;
			ret.MaxDamage -= rhs.MaxDamage;
			return ret;
		}

		public static Stats operator /(Stats lhs, double rhs)
		{
			Stats ret = new Stats(lhs, true);
			ret.Health /= rhs;
			ret.Attack /= rhs;
			ret.Defense /= rhs;
			ret.Speed /= rhs;
			ret.CritRate /= rhs;
			ret.CritDamage /= rhs;
			ret.Resistance /= rhs;
			ret.Accuracy /= rhs;
			ret.EffectiveHP /= rhs;
			ret.EffectiveHPDefenseBreak /= rhs;
			ret.DamagePerSpeed /= rhs;
			ret.AverageDamage /= rhs;
			ret.MaxDamage /= rhs;
			return ret;
		}

		public static Stats operator /(Stats lhs, Stats rhs)
		{
			Stats ret = new Stats(lhs, true);
			
			foreach (var a in Build.statEnums)
			{
				if (rhs[a].EqualTo(0))
					ret[a] = 0;
				else
					ret[a] /= rhs[a];
			}

			foreach (var a in Build.extraEnums)
			{
				if (rhs[a].EqualTo(0))
					ret[a] = 0;
				else
					ret[a] /= rhs[a];
			}

			return ret;
		}

		// how much % of the 3 pris RHS needs to get to this
		public Stats Of(Stats rhs)
		{
			Stats ret = new Stats(this);

			if (rhs.Health > 0)
				ret.Health /= rhs.Health;
			if (rhs.Attack > 0)
				ret.Attack /= rhs.Attack;
			if (rhs.Defense > 0)
				ret.Defense /= rhs.Defense;


			return ret;
		}

		// boots this by RHS
		// assumes A/D/H/S are 100.00 instead of 1.00 (leader/shrine)
		public Stats Boost(Stats rhs)
		{
			Stats ret = new Stats(this);

			ret.Attack *= 1 + rhs.Attack / 100;
			ret.Defense *= 1 + rhs.Defense / 100;
			ret.Health *= 1 + rhs.Health / 100;
			ret.Speed *= 1 + rhs.Speed / 100;

			ret.CritRate += rhs.CritRate;
			ret.CritDamage += rhs.CritDamage;
			ret.Accuracy += rhs.Accuracy;
			ret.Resistance += rhs.Resistance;

			return ret;
		}

		public bool NonZero()
		{
			if (!Accuracy.EqualTo(0))
				return true;
			if (!Attack.EqualTo(0))
				return true;
			if (!CritDamage.EqualTo(0))
				return true;
			if (!CritRate.EqualTo(0))
				return true;
			if (!Defense.EqualTo(0))
				return true;
			if (!Health.EqualTo(0))
				return true;
			if (!Resistance.EqualTo(0))
				return true;
			if (!Speed.EqualTo(0))
				return true;

			if (!EffectiveHP.EqualTo(0))
				return true;
			if (!EffectiveHPDefenseBreak.EqualTo(0))
				return true;
			if (!DamagePerSpeed.EqualTo(0))
				return true;
			if (!AverageDamage.EqualTo(0))
				return true;
			if (!MaxDamage.EqualTo(0))
				return true;

			if (!DamageSkillups[0].EqualTo(0))
				return true;
			if (!DamageSkillups[1].EqualTo(0))
				return true;
			if (!DamageSkillups[2].EqualTo(0))
				return true;
			if (!DamageSkillups[3].EqualTo(0))
				return true;

			return false;
		}

		public Attr FirstNonZero()
		{
			if (!Accuracy.EqualTo(0))
				return Attr.Accuracy;
			if (!Attack.EqualTo(0))
				return Attr.AttackPercent;
			if (!CritDamage.EqualTo(0))
				return Attr.CritDamage;
			if (!CritRate.EqualTo(0))
				return Attr.CritRate;
			if (!Defense.EqualTo(0))
				return Attr.DefensePercent;
			if (!Health.EqualTo(0))
				return Attr.HealthPercent;
			if (!Resistance.EqualTo(0))
				return Attr.Resistance;
			if (!Speed.EqualTo(0))
				return Attr.SpeedPercent;

			return Attr.Null;
		}
	}
}
