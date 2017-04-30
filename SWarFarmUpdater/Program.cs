using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SWarFarmUpdater
{
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

	class SWFREntry
	{
		public string url;
		public int pk;
		public int com2us_id;
		public string name;
		public Element element;
		public Archetype archetype;
		public int base_stars = 6;
	}

	class SWFREffect
	{
		public string name;
		public bool is_buff;
	}

	class SWFRSkill
	{
		public int pk;
		public int com2us_id;
		public string name;
		public int? cooltime;
		public int? hits;
		public bool passive;
		public string level_progress_description;
		public string multiplier_formula_raw;
		public SWFREffect[] skill_effect;
	}

	class SWFRMaterial
	{
		public string name;
		public int quantity;
	}

	class SWFRHSkill
	{
		public SWFRSkill skill;
		public SWFRMaterial[] craft_materials;
		public int mana_cost;
		public int[] prerequisites;
	}

	class SWFRMonster : SWFREntry
	{
		public SWFRSkill[] skills;
		public SWFRHSkill[] homunculus_skills;
	}

	class SWFRSkillProp : List<SWFRSkillProp>
	{
		public string val;
	}

	class Program
	{
		static void Main(string[] args)
		{
#if false
			GetData();
#else
			var list = JsonConvert.DeserializeObject<SWFRMonster[]>(File.ReadAllText("skills.json"));
			var mm = list.FirstOrDefault(l => l.name == "Theomars");
			var msk = mm.skills;
			foreach (var s in msk)
			{
				var levels = s.level_progress_description.Split('\n');
				var qq = JsonConvert.DeserializeObject<SWFRSkillProp>(s.multiplier_formula_raw);
			}
#endif
		}

		static void GetData()
		{
			List<SWFRMonster> monsters = new List<SWFRMonster>();
			var list = AskFor<SWFREntry[]>("https://swarfarm.com/api/bestiary");
			int i = 0;
			foreach (var it in list)
			{
				Console.Write($"{i * 100.0 / list.Length:0.##}% "); i++;
				var mm = AskFor<SWFRMonster>(it.url);
				monsters.Add(mm);
			}
			File.WriteAllText("skills.json", JsonConvert.SerializeObject(monsters));
		}

		static DateTime lastRequest = DateTime.Now;

		public static T AskFor<T>(string url) where T : class
		{
			while ((DateTime.Now - lastRequest).Seconds < 1)
			{
				System.Threading.Thread.Sleep(750);
			}
			lastRequest = DateTime.Now;

			Console.Write($"Getting {url}...");
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Accept = "application/json";

			string resp;

			var wresp = req.GetResponse();
			var respStr = wresp.GetResponseStream();

			Console.WriteLine("Done!");

			if (respStr == null)
				return null;

			using (var stream = new StreamReader(respStr))
			{
				resp = stream.ReadToEnd();
			}

			return JsonConvert.DeserializeObject<T>(resp);
		}
	}
}
