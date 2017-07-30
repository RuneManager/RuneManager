using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace RuneOptim
{
	// The monster stores its base stats in its base class
	public class Monster : Stats, IComparable<Monster>
	{
		[JsonProperty("name")]
		private string name = "Missingno";

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		[JsonIgnore]
		public string FullName
		{
			get
			{
				if (IsHomunculus)
					return HomunculusName;
				return (awakened == 1 ? "" : Element.ToString() + " ") + name;
			}
			set
			{
				name = value;
			}
		}

		[JsonProperty("unit_id")]
		public ulong Id = 0;

		[JsonProperty("wizard_id")]
		public ulong WizardId = 0;

		[JsonProperty("class")]
		public int Grade;

		[JsonProperty("unit_level")]
		public int level = 1;

		[JsonProperty("unit_master_id")]
		public int monsterTypeId;

		public int GetFamily { get { return monsterTypeId / 100; } }

		[JsonProperty("building_id")]
		public ulong BuildingId;
		
		[JsonProperty("create_time")]
		public DateTime? createdOn = null;

		private static Dictionary<int, MonsterDefinitions.Monster> monDefs = null;

		private static Dictionary<int, MonsterDefinitions.Monster> MonDefs
		{
			get
			{
				if (monDefs == null)
				{
					monDefs = new Dictionary<int, MonsterDefinitions.Monster>();
					foreach (var item in SkillList)
					{
						if (!monDefs.ContainsKey(item.Com2usId))
							monDefs.Add(item.Com2usId, item);
					}
				}
				return monDefs;
			}
		}

		private static Dictionary<int, MonsterDefinitions.Skill> skillDefs = null;
		private static Dictionary<int, MonsterDefinitions.Skill> SkillDefs {
			get {
				if (skillDefs == null) {
					skillDefs = new Dictionary<int, MonsterDefinitions.Skill>();
					foreach (var item in SkillList) {
						foreach (var skill in item.Skills) {
							if (!skillDefs.ContainsKey(skill.Com2usId))
								skillDefs.Add(skill.Com2usId, skill);
						}
						foreach (var skill in item.HomunculusSkills) {
							if (!skillDefs.ContainsKey(skill.Skill.Com2usId))
								skillDefs.Add(skill.Skill.Com2usId, skill.Skill);
						}
					}
				}
				return skillDefs;
			}
		}

		[JsonProperty("attribute")]
		public Element Element;

		[JsonProperty("skills")]
		public IList<Skill> _skilllist = new List<Skill>();

		[JsonIgnore]
		public int SkillupsLevel { get { checkSkillups(); return SkillupLevel.Sum() - SkillupLevel.Count(i => i > 0); } }

		[JsonIgnore]
		public int SkillupsTotal { get { checkSkillups(); return SkillupMax.Sum() - SkillupMax.Count(i => i > 0); } }

		[JsonConverter(typeof(RuneLoadConverter))]
		[JsonProperty("runes")]
		public Rune[] Runes;

		public int priority = 0;

		public bool Locked = false;

		[JsonIgnore]
		public bool downloaded = false;

		[JsonIgnore]
		public double score = 0;

		[JsonIgnore]
		private Stats curStats = null;

		[JsonIgnore]
		private bool changeStats = true;

		[JsonIgnore]
		public bool inStorage = false;

		[JsonProperty("homunculus")]
		public bool IsHomunculus = false;

		[JsonProperty("homunculus_name")]
		public string HomunculusName;

		[JsonIgnore]
		public int loadOrder = int.MaxValue;

		[JsonIgnore]
		public bool IsRep = false;

		[JsonIgnore]
		public bool OnDefense = false;

		[JsonIgnore]
		public override int ExtraCritRate {
			get {
				return base.ExtraCritRate;
			}
			set {
				changeStats = true;
				base.ExtraCritRate = value;
			}
		}

		public int SwapCost(Loadout l)
		{
			int cost = 0;
			for (int i = 0; i < 6; i++)
			{
				if (l.Runes[i] != null && l.Runes[i].AssignedName != FullName)
				{
					// unequip current rune
					if (Current.Runes[i] != null)
						cost += Current.Runes[i].UnequipCost;
					// unequip new rune from host
					if ((l.Runes[i].IsUnassigned) && l.Runes[i].Swapped != true)
					{
						cost += l.Runes[i].UnequipCost;
					}
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
		public Monster(Monster rhs, bool loadout = false) : base(rhs)
		{
			FullName = rhs.FullName;
			Id = rhs.Id;
			level = rhs.level;
			monsterTypeId = rhs.monsterTypeId;
			Grade = rhs.Grade;
			Element = rhs.Element;
			_skilllist = _skilllist.Concat(rhs._skilllist).ToList();
			priority = rhs.priority;
			downloaded = rhs.downloaded;
			inStorage = rhs.inStorage;
		
			if (loadout)
			{
				Current = new Loadout(rhs.Current); 
			}
		}

		// put this rune on the current build
		public void ApplyRune(Rune rune, int checkOn = 2)
		{
			Current.AddRune(rune, checkOn);
			changeStats = true;
		}

		private static MonsterDefinitions.Monster[] skillList = null;

		private static MonsterDefinitions.Monster[] SkillList
		{
			get
			{
				if (skillList == null)
					Monster.skillList = JsonConvert.DeserializeObject<MonsterDefinitions.Monster[]>(File.ReadAllText(global::RuneOptim.Properties.Resources.SkillsJSON));

				return skillList;
			}
		}

		public int awakened
		{
			get
			{
				return (this.monsterTypeId / 10) - (this.monsterTypeId / 100) * 10;
			}
		}

		// get the stats of the current build.
		// NOTE: the monster will contain it's base stats
		public Stats GetStats()
		{
			if (changeStats || Current.Changed)
			{
				checkSkillups();

				curStats = Current.GetStats(this);
				changeStats = false;
			}

			return curStats;
		}

		private void checkSkillups()
		{
			if (this.damageFormula == null && MonDefs.ContainsKey(monsterTypeId))
			{
				MonsterDefinitions.MultiplierGroup average = new MonsterDefinitions.MultiplierGroup();

				int skdmg = 0;
				int i = 0;

				foreach (var si in _skilllist)
				{
					if (!SkillDefs.ContainsKey(si.SkillId ?? 0))
						continue;
					var ss = SkillDefs[si.SkillId ?? 0];
					var df = JsonConvert.DeserializeObject<MonsterDefinitions.MultiplierGroup>(ss.MultiplierFormulaRaw, new MonsterDefinitions.MultiplierGroupConverter());
					if (df.props.Count > 0)
					{
						var levels = ss.LevelProgressDescription.Split('\n').Take(_skilllist[i].Level ?? 0);
						this.SkillupMax[i] = ss.LevelProgressDescription.Split('\n').Length;
						this.SkillupLevel[i] = _skilllist[i].Level ?? 0;
						var ct = levels.Count(s => s == "Cooltime Turn -1");
						int cooltime = (ss.Cooltime ?? 1) - ct;
						this.SkillTimes[i] = cooltime;
						var dmg = levels.Where(s => s.StartsWith("Damage")).Select(s => int.Parse(s.Replace("%", "").Replace("Damage +", "")));

						this.DamageSkillups[i] = (dmg.Any() ? dmg.Sum() : 0);
						skdmg += (dmg.Any() ? dmg.Sum() : 0) / cooltime;

						this._skillsFormula[i] = Expression.Lambda<Func<Stats, double>>(df.AsExpression(Stats.statType), Stats.statType).Compile();

						df.props.Last().op = MonsterDefinitions.MultiplierOperator.Div;
						df.props.Add(new MonsterDefinitions.MultiplierValue(cooltime));
						average.props.Add(new MonsterDefinitions.MultiplierValue(df));
						if (i != 0)
						{
							average.props[i - 1].op = MonsterDefinitions.MultiplierOperator.Add;
						}
						i++;
					}
				}
				this.damageFormula = average;
				this.SkillupDamage = skdmg;
			}
		}

		// NYI comparison
		public EquipCompare CompareTo(Monster rhs)
		{
			if (Loadout.CompareSets(Current.Sets, rhs.Current.Sets) == 0)
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

		public override string ToString()
		{
			return Id + " " + FullName + " lvl. " + level;
		}

		int IComparable<Monster>.CompareTo(Monster rhs)
		{
			var comp = rhs.Grade - Grade;
			if (comp != 0) return comp;
			comp = rhs.level - level;
			if (comp != 0) return comp;
			comp = (int)Element - (int)rhs.Element;
			if (comp != 0) return comp;
			comp = rhs.awakened - awakened;
			if (comp != 0) return comp;
			comp = loadOrder - rhs.loadOrder;
			return comp;
		}
	}

	public class Skill : ListProp<int?>
	{
		// TODO: name
		[ListProperty(0)]
		public int? SkillId = null;
		[ListProperty(1)]
		public int? Level = null;

		protected override int maxInd
		{
			get
			{
				return 2;
			}
		}
	}



	public class RuneLoadConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(IList<RuneOptim.Rune>).IsAssignableFrom(objectType);
		}

		public override bool CanRead
		{
			get
			{
				return base.CanRead;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return base.CanWrite;
			}
		}

		public override object ReadJson(JsonReader reader,
			Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken tok = JToken.Load(reader);
			if (tok is JArray)
			{
				return tok.ToObject<RuneOptim.Rune[]>();
			}
			else if (tok is JObject)
			{
				var jo = tok as JObject;
				return jo.Children().Select(o => o.First.ToObject<RuneOptim.Rune>()).ToArray();
			}
			throw new InvalidCastException("A monsters runes are in an invalid format.");
		}

		public override void WriteJson(JsonWriter writer,
			object value, JsonSerializer serializer)
		{
			if (value is Array)
			{
				// TODO: not having 6 runes is a mess
				var a = value as Array;
				//if (a.Length == 6)
				{
					var ja = JArray.FromObject(value);
					ja.WriteTo(writer);
					return;
				}
			}
			throw new NotImplementedException();
		}
	}
}

namespace MonsterDefinitions
{
	public class Entry
	{
		[JsonProperty("url")]
		public string URL;
		[JsonProperty("pk")]
		public int Pk;
		[JsonProperty("com2us_id")]
		public int Com2usId;
		[JsonProperty("name")]
		public string Name;
		[JsonProperty("element")]
		public RuneOptim.Element Element;
		[JsonProperty("archetype")]
		public RuneOptim.Archetype Archetype;
		[JsonProperty("base_stars")]
		public int BaseStars;
		public override string ToString() {
			return ((Com2usId / 10) % 10 == 0 ? Element + " " : "") + Name;
		}
	}

	public class SkillEff
	{
		[JsonProperty("name")]
		public string Name;
		[JsonProperty("is_buff")]
		public bool IsBuff;
	}

	public class Skill
	{
		[JsonProperty("pk")]
		public int Pk;
		[JsonProperty("com2us_id")]
		public int Com2usId;
		[JsonProperty("name")]
		public string Name;
		[JsonProperty("cooltime")]
		public int? Cooltime;
		[JsonProperty("hits")]
		public int? Hits;
		[JsonProperty("passive")]
		public bool Passive;
		[JsonProperty("level_progress_description")]
		public string LevelProgressDescription;
		[JsonProperty("multiplier_formula_raw")]
		public string MultiplierFormulaRaw;
		[JsonProperty("skill_effect")]
		public SkillEff[] SkillEffect;
	}

	public class Material
	{
		[JsonProperty("name")]
		public string Name;
		[JsonProperty("quantity")]
		public int Quantity;
	}

	public class HomuSkill
	{
		[JsonProperty("skill")]
		public Skill Skill;
		[JsonProperty("craft_materials")]
		public Material[] CraftMaterials;
		[JsonProperty("mana_cost")]
		public int ManaCost;
		[JsonProperty("prerequisites")]
		public int[] Prerequisites;
	}

	public class Monster : Entry
	{
		[JsonProperty("skills")]
		public Skill[] Skills;
		[JsonProperty("homunculus_skills")]
		public HomuSkill[] HomunculusSkills;
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum MultiplierOperator
	{
		[EnumMember(Value = "+")]
		Add,
		[EnumMember(Value = "-")]
		Sub,
		[EnumMember(Value = "*")]
		Mult,
		[EnumMember(Value = "/")]
		Div,
		[EnumMember(Value = "=")]
		End,
		[EnumMember(Value = "FIXED")]
		Fixed
	}

	abstract public class MultiplierBase
	{
		abstract public double GetValue(RuneOptim.Stats vals);
		abstract public Expression AsExpression(ParameterExpression statType);
	}

	public class MultiplierValue : MultiplierBase
	{
		public double? value = null;
		public MultiplierBase inner = null;
		public RuneOptim.Attr key = RuneOptim.Attr.Null;

		public MultiplierOperator op = MultiplierOperator.End;

		public MultiplierValue()
		{
		}

		public MultiplierValue(double v, MultiplierOperator o = MultiplierOperator.End)
		{
			value = v;
			op = o;
		}

		public MultiplierValue(MultiplierBase i, MultiplierOperator o = MultiplierOperator.End)
		{
			inner = i;
			op = o;
		}

		public MultiplierValue(RuneOptim.Attr a, MultiplierOperator o = MultiplierOperator.End)
		{
			key = a;
			op = o;
		}

		public override Expression AsExpression(ParameterExpression statType)
		{
			if (inner != null)
			{
				return inner.AsExpression(statType);
			}
			else if (key != RuneOptim.Attr.Null)
			{//Expression.Parameter(typeof(RuneOptim.Stats), "stats")
				if (key == RuneOptim.Attr.Neg)
					return Expression.Constant(1.0);
				return Expression.Property(statType, "Item", Expression.Constant(key));
			}
			else
			{
				return Expression.Constant(value);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (inner != null)
				sb.Append(inner.ToString());
			else if (key != RuneOptim.Attr.Null)
				sb.Append(GetEnumMemberAttrValue(key));
			else
				sb.Append(value);
			sb.Append(" ");
			sb.Append(GetEnumMemberAttrValue(op));
			return sb.ToString();
		}

		public string GetEnumMemberAttrValue<T>(T enumVal)
		{
			var enumType = typeof(T);
			var memInfo = enumType.GetMember(enumVal.ToString());
			var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
			return attr?.Value;
		}

		public override double GetValue(RuneOptim.Stats vals)
		{
			if (inner != null)
				return inner.GetValue(vals);
			else if (key != RuneOptim.Attr.Null)
				return vals[key];
			else if (value != null)
				return value ?? 0;
			return 0;
		}
	}

	public class MultiplierGroup : MultiplierBase
	{
		public List<MultiplierValue> props = new List<MultiplierValue>();

		public MultiplierGroup()
		{
		}

		public MultiplierGroup(params MultiplierValue[] vals)
		{
			foreach (var v in vals)
			{
				props.Add(v);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("[");

			foreach (var prop in props)
			{
				sb.Append(prop.ToString());
				sb.Append(" ");
			}
			sb.Append("]");

			return sb.ToString();
		}

		public override double GetValue(RuneOptim.Stats vals)
		{
			if (props.Count == 0)
				return 0;

			double ret = props.First().GetValue(vals);

			var operate = props.First().op;

			foreach (var prop in props.Skip(1))
			{
				switch (operate)
				{
					case MultiplierOperator.Add:
						ret += prop.GetValue(vals);
						break;
					case MultiplierOperator.Sub:
						ret -= prop.GetValue(vals);
						break;
					case MultiplierOperator.Mult:
						ret *= prop.GetValue(vals);
						break;
					case MultiplierOperator.Div:
						ret /= prop.GetValue(vals);
						break;
					case MultiplierOperator.End:
						return ret;
					default:
						break;
				}
				operate = prop.op;
			}

			return ret;
		}

		public override Expression AsExpression(ParameterExpression statType)
		{
			if (props.Count == 0)
				return Expression.Constant(0.0);

			var express = props.First().AsExpression(statType);
			var operate = props.First().op;

			foreach (var prop in props.Skip(1))
			{
				switch (operate)
				{
					case MultiplierOperator.Add:
						express = Expression.Add(express, prop.AsExpression(statType));
						break;
					case MultiplierOperator.Sub:
						express = Expression.Subtract(express, prop.AsExpression(statType));
						break;
					case MultiplierOperator.Mult:
						express = Expression.Multiply(express, prop.AsExpression(statType));
						break;
					case MultiplierOperator.Div:
						express = Expression.Divide(express, prop.AsExpression(statType));
						break;
					case MultiplierOperator.End:
						return express;
					default:
						break;
				}
				operate = prop.op;
			}
			return express;

		}
	}

	public class MultiplierGroupConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(MultiplierGroup).IsAssignableFrom(objectType);
		}

		private MultiplierGroup GetProp(JArray jarray)
		{
			MultiplierGroup multiGroup = new MultiplierGroup();
			for (int i = 0; i < jarray.Count; i += 2)
			{
				JToken jvalue = jarray[i];
				JToken joperator = (i + 1 < jarray.Count) ? jarray[i + 1] : "=";
				MultiplierValue value = new MultiplierValue();
				if (joperator is JArray)
				{
					joperator = (joperator as JArray)[0];
				}
				value.op = joperator.ToObject<MultiplierOperator>();
				if (jvalue is JArray)
				{
					value.inner = GetProp(jvalue as JArray);
				}
				else
				{
					double tempval;
					if (double.TryParse(jvalue.ToString(), out tempval))
					{
						value.value = tempval;
					}
					else
					{
						var tstr = jvalue.ToObject<string>();
						value.key = GetStatAttrValue(tstr);
					}
				}
				multiGroup.props.Add(value);
			}
			return multiGroup;
		}

		public RuneOptim.Attr GetStatAttrValue(string str)
		{
			var enumType = typeof(RuneOptim.Attr);
			foreach (var enumVal in Enum.GetValues(enumType))
			{
				var memInfo = enumType.GetMember(enumVal.ToString());
				var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<RuneOptim.SkillAttrAttribute>().Any(m => m.Attr == str);
				if (attr ?? false)
					return (RuneOptim.Attr)enumVal;
			}
			throw new ArgumentException("str:" + str);
		}

		public override object ReadJson(JsonReader reader,
			Type objectType, object existingValue, JsonSerializer serializer)
		{
			return GetProp(JArray.Load(reader));
		}

		public override void WriteJson(JsonWriter writer,
			object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}

}
