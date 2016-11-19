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
		public static List<MonsterStat> monStats;

		public static StatReference FindMon(Monster mon)
		{
			return FindMon(mon.Name);
		}

		public static StatReference FindMon(string name)
		{
			if (monStats == null)
				return null;

			int bracketInd = name.IndexOf('(');
			string element = "";

			if (bracketInd > 0)
			{
				element = name.Substring(bracketInd + 1, name.Length - bracketInd - 2).Trim();
				name = name.Substring(0, bracketInd).Trim();
			}

			Console.WriteLine("searching for \"" + name + "\"");
			if (element == "")
				return monStats.FirstOrDefault(m => m.name == name);
			else
				return monStats.FirstOrDefault(m => m.name == name && m.element.ToString() == element);
		}

		public static MonsterStat Download(StatReference refer)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(refer.URL);
			req.Accept = "application/json";

			string resp;

		    var wresp = req.GetResponse();
		    var respStr = wresp.GetResponseStream();

		    if (respStr == null)
		    {
		        return null;
		    }

		    using (var stream = new StreamReader(respStr))
		    {
		        resp = stream.ReadToEnd();
		    }

		    var ret = JsonConvert.DeserializeObject<MonsterStat>(resp);

			return ret;
		}

		public Monster GetMon(StatReference mref, Monster mon)
		{
			//var mstat = mref.Download();
			return GetMon(mon);
		}

		public Monster GetMon(Monster mon)
		{
			return new Monster()
			{
				ID = mon.ID,
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
				Name = (Awakened ? name : name + " (" + element.ToString() + ")"),
				downloaded = true
			};
		}
	}

	// Allows me to steal the JSON values into Enum
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Element
	{
		[EnumMember(Value = "")]
		Null = 0,

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
		[EnumMember(Value = "")]
		Null = 0,

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
		
		public MonsterStat Download()
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
			req.Accept = "application/json";

			string resp;

            var wresp = req.GetResponse();
            var respStr = wresp.GetResponseStream();

            if (respStr == null)
            {
                return null;
            }

            using (var stream = new StreamReader(respStr))
			{
				resp = stream.ReadToEnd();
			}

			var ret = JsonConvert.DeserializeObject<MonsterStat>(resp);

			return ret;
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

		[JsonProperty("fusion_food")]
		public bool isFusion;
	}
}
