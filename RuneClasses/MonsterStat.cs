using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Net;
using System.IO;

namespace RuneOptim
{
	public class MonsterStat : StatLoader
	{
		[JsonProperty("max_lvl_hp")]
		public int Health;

		[JsonProperty("max_lvl_attack")]
		public int Attack;

		[JsonProperty("max_lvl_defense")]
		public int Defense;

		[JsonProperty("speed")]
		public int Speed;

		[JsonProperty("crit_rate")]
		public int CritRate;

		[JsonProperty("crit_damage")]
		public int CritDamage;

		[JsonProperty("resistance")]
		public int Resistance;

		[JsonProperty("accuracy")]
		public int Accuracy;

		[JsonProperty("is_awakened")]
		public bool Awakened;

		[JsonProperty("awakens_to")]
		public StatReference AwakenRef;

		[JsonIgnore]
		private static List<MonsterStat> monStats = null;

		[JsonIgnore]
		public static List<MonsterStat> MonStats
		{
			get
			{
				if (monStats == null)
					monStats = StatReference.AskSWApi<List<MonsterStat>>("https://swarfarm.com/api/bestiary");
				return monStats;
			}
		}

		public static int BaseStars(string familyName)
		{
			var m = MonStats.FirstOrDefault(ms => ms.name == familyName);
			if (m != null)
				return m.grade;
			// TODO: lookup?
			return 4; // close enough
		}

		public static StatReference FindMon(Monster mon)
		{
			return FindMon(mon.Name, mon.Element.ToString());
		}

		public static StatReference FindMon(string name, string element = null)
		{
			if (MonStats == null)
				return null;
			
			RuneLog.Info($"searching for \"{name} ({element})\"");
			if (element == null)
				return MonStats.FirstOrDefault(m => m.name == name);
			else
				return MonStats.FirstOrDefault(m => m.name == name && m.element.ToString() == element);
		}
		
		public Monster GetMon(Monster mon)
		{
			return new Monster()
			{
				Id = mon.Id,
				priority = mon.priority,
				Current = mon.Current,
				Accuracy = Accuracy,
				Attack = Attack,
				CritDamage = CritDamage,
				CritRate = CritRate,
				Defense = Defense,
				Health = Health,
				level = 40,
				Resistance = Resistance,
				Speed = Speed,
				Element = element,
				Name = name,
				downloaded = true,
				monsterTypeId = monsterTypeId,
				_skilllist = mon._skilllist.ToList()
			};
		}
	}

	// Allows me to steal the JSON values into Enum
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Element
	{
		[EnumMember(Value = "Pure")]
		Pure = 0,

		[EnumMember(Value = "Water")]
		Water = 1,

		[EnumMember(Value = "Fire")]
		Fire = 2,

		[EnumMember(Value = "Wind")]
		Wind = 3,

		[EnumMember(Value = "Light")]
		Light = 4,

		[EnumMember(Value = "Dark")]
		Dark = 5,

	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum Archetype
	{
		[EnumMember(Value = "None")]
		None = 0,

		[EnumMember(Value = "Attack")]
		Attack = 1,

		[EnumMember(Value = "HP")]
		HP = 2,

		[EnumMember(Value = "Defense")]
		Defense = 3,

		[EnumMember(Value = "Support")]
		Support = 4,

		[EnumMember(Value = "Material")]
		Material = 5,

	}

	public class StatReference
	{
		[JsonProperty("url")]
		public string URL;

		[JsonProperty("pk")]
		public int pk;

		[JsonProperty("name")]
		public string name;

		[JsonProperty("element")]
		public Element element;

		static Dictionary<string, object> apiObjs = new Dictionary<string, object>();

		public static T AskSWApi<T>(string location)
		{
			var fpath = location.Replace("https://swarfarm.com/api", "swf_api_cache") + ".json";
			var data = "";
			if (apiObjs.ContainsKey(location))
			{
				return (T)apiObjs[location];
			}
			if (File.Exists(fpath) && new FileInfo(fpath).CreationTime < DateTime.Now.AddDays(-7))
			{
				File.Delete(fpath);
			}
			if (!File.Exists(fpath))
			{
				Directory.CreateDirectory(new FileInfo(fpath).Directory.FullName);
				using (WebClient client = new WebClient())
				{
					client.Headers["accept"] = "application/json";
					data = client.DownloadString(location);
					File.WriteAllText(fpath, data);
				}
			}
			else
			{
				data = File.ReadAllText(fpath);
			}
			if (string.IsNullOrWhiteSpace(data))
				return default(T);
			apiObjs.Add(location, JsonConvert.DeserializeObject<T>(data));
			return (T)apiObjs[location];
		}

		public MonsterStat Download()
		{
			return AskSWApi<MonsterStat>(URL);
		}
	}

	public class StatLoader : StatReference
	{
		[JsonProperty("image_filename")]
		public string imageFileName;

		[JsonProperty("archetype")]
		public Archetype archetype;

		[JsonProperty("base_stars")]
		public int grade;

		[JsonProperty("com2us_id")]
		public int monsterTypeId;

		[JsonProperty("fusion_food")]
		public bool isFusion;
	}
}
