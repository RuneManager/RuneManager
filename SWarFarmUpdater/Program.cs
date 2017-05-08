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
using Newtonsoft.Json.Linq;
using MonsterDefinitions;

namespace MonsterDefinitions
{
	class Program
	{
		static void Main(string[] args)
		{
#if false
			GetData();
#else
			var list = JsonConvert.DeserializeObject<Monster[]>(File.ReadAllText("skills.json"));
			var mm = list.FirstOrDefault(l => l.Name == "Theomars");
			var msk = mm.Skills;
			foreach (var s in msk)
			{
				var levels = s.LevelProgressDescription.Split('\n');
				var qq = JsonConvert.DeserializeObject<MultiplierBase>(s.MultiplierFormulaRaw, new MultiplierGroupConverter());
				RuneOptim.Stats ss = new RuneOptim.Stats();
				ss.Attack = 1500;
				ss.Speed = 170;
				var dam = qq.GetValue(ss);
			}
#endif
		}

		static void GetData()
		{
			List<Monster> monsters = new List<Monster>();
			var list = AskFor<Entry[]>("https://swarfarm.com/api/bestiary");
			int i = 0;
			foreach (var it in list)
			{
				Console.Write($"{i * 100.0 / list.Length:0.##}% "); i++;
				var mm = AskFor<Monster>(it.URL);
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
