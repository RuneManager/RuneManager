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
using RuneOptim.swar;

namespace SWFarmLoader {
    class MonDude : MonsterStat {
        private MonDudeAwake awake;
        public MonDudeAwake Awake {
            get {
                throw new Exception();
                if (awake == null)
                    awake = MonsterStat.AskSWApi<MonDudeAwake>(AwakenTo.URL);
                return awake;
            }
        }
    }
    public class MonDudeAwake : MonsterStat {

    }
    class MonFam {
        public int monGroupPrefix;
        private MonDude fire = null;
        public MonDude Fire {
            get {
                throw new Exception();
                if (fire == null) 
                    fire = MonsterStat.AskSWApi<MonDude>(MonsterStat.FindMon(monGroupPrefix * 100 + 1).URL);
                return fire;
            }
        }
        public MonDude Water;
        public MonDude Wind;
        public MonDude Light;
        public MonDude Dark;
        public MonFam(int prefix) {
            monGroupPrefix = prefix;
        }
    }

    class Program {
        static void Main(string[] args) {
#if true
            Console.Write("Press Y for new data: ");
            GetData(Console.ReadKey().Key == ConsoleKey.Y);
#elif false
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
#else
            var list = StatReference.AskSWApi<StatLoader[]>("https://swarfarm.com/api/bestiary");
            var newmons = list.Where(r => r.monsterTypeId > 21800 && r.monsterTypeId < 40000).GroupBy(r => r.name).Select(rg => rg.FirstOrDefault()).OrderBy(r => r.monsterTypeId).Select(r => new KeyValuePair<int, string>(r.monsterTypeId, r.name));
#endif
        }

        static void GetData(bool refetch = false) {
            using (var prog = new ProgressBar()) {
                try {
                    Console.WriteLine("\nGetting monsters.");
                    List<MonsterStat> monsters = new List<MonsterStat>();
                    var list = StatReference.AskSWApi<StatLoader[]>("https://swarfarm.com/api/bestiary", refetch);
                    int i = 0;
                    //foreach (var it in list) {
                    Parallel.ForEach(list, new ParallelOptions() { 
                        MaxDegreeOfParallelism = Environment.ProcessorCount,
                    }, it => {
                        //Console.CursorLeft = 0;
                        //Console.Write($"{i * 100.0 / list.Length:0.##}% "); i++;
                        var mm = StatReference.AskSWApi<MonsterStat>(it.URL, refetch);
                        i++;
                        prog.Report(i / (double)list.Length);
                        monsters.Add(mm);
                    }
                    );

                    Console.WriteLine("\nGetting names.");
                    i = 0;
                    SortedDictionary<int, string> monstersJSON = new SortedDictionary<int, string>();
                    foreach (var mon in monsters.OrderBy(m => m.monsterTypeId)) {
                        prog.Report(i / (double)monsters.Count);
                        i++;
                        if (mon.monsterTypeId % 100 == 1) {
                            monstersJSON.Add(mon.monsterTypeId / 100, mon.name);
                        }
                        else if (mon.monsterTypeId % 100 / 10 == 0) {
                            // duplicate so skip
                        }
                        else {
                            monstersJSON.Add(mon.monsterTypeId, mon.name);
                        }
                    };

                    /*
                    File.WriteAllText("mons2.cs", $@"using RuneOptim;

namespace RuneOptim.Monsters {{
    public static class Library {{
{string.Join(@"
", monsters.Where(m => m.obtainable).Select(m => writeSMonCS(m)))}
    }}
}}
");*/
                    /*
                    var qwerqe = monsters.GroupBy(m => m.monsterTypeId / 100);

                    File.WriteAllText("mons3.cs", $@"using RuneOptim;

namespace RuneOptim.Monsters {{
    public enum MonsterTypeMap {{
        {string.Join(@",
        ", monsters.Select(m => m.name.Replace(" ", "").Replace("(", "_").Replace(")", "_").Replace("-", "").Replace("'", "") + (m.Awakened ? "_" + m.element : "") + " = " + m.monsterTypeId))}
    }}
    public static class Library {{
{string.Join(@"
", qwerqe.Select(g => writeGroup(g, @"      ")))}
    }}
}}");
                    */

                    File.WriteAllText("skills.json", JsonConvert.SerializeObject(monsters.OrderBy(m => m.monsterTypeId), Formatting.Indented), Encoding.UTF8);
                    File.WriteAllText("monsters.json", JsonConvert.SerializeObject(monstersJSON, Formatting.Indented), Encoding.UTF8);
                }
                catch (Exception e) {
                    Console.WriteLine(e.GetType() + ": " + e.Message);
                }
            }
        }

        private static string writeGroup(IGrouping<int, MonsterStat> g, string offset) {
            string s = offset + "public static class ";
            s += g.FirstOrDefault(m => !m.Awakened).name.Replace(" ", "").Replace("(", "_").Replace(")", "_").Replace("-", "").Replace("'", "")
                + "{" + Environment.NewLine;

            foreach (var eg in g.GroupBy(m => m.element)) {

            }

            return s;
        }

        public static string writeSMonCS(MonsterStat mon) {
            var vname = mon.name.Replace(" ", "").Replace("(", "_").Replace(")", "_").Replace("-", "").Replace("'", "") + "_" + mon.element;
            string s = $@"      private static {nameof(MonsterStat)} _{vname} = null;
        public static {nameof(MonsterStat)} {vname} {{
            get {{
                if (_{vname} == null)
                    _{vname} = {nameof(StatReference)}.AskSWApi<{nameof(MonsterStat)}>(""{mon.URL}"");
                return _{vname};
            }}
        }}";
            return s;
        }

        public static string writeMonCS(MonsterStat mon) {
            string s = $@"      public static {nameof(MonsterStat)} {mon.name.Replace(" ", "").Replace("(", "_").Replace(")", "_").Replace("-", "").Replace("'", "")}_{mon.element} = new {nameof(MonsterStat)}() {{
            {nameof(mon.URL)} = ""{mon.URL}"",
            {nameof(mon.pk)} = {mon.pk},
            {nameof(mon.name)} = ""{mon.name}"",
            {nameof(mon.monsterTypeId)} = {mon.monsterTypeId},
            {nameof(mon.imageFileName)} = ""{mon.imageFileName}"",
            {nameof(mon.element)} = {nameof(Element)}.{mon.element},
            {nameof(mon.Health)} = {mon.Health},
            {nameof(mon.Attack)} = {mon.Attack},
            {nameof(mon.Defense)} = {mon.Defense},
            {nameof(mon.CritRate)} = {mon.CritRate},
            {nameof(mon.CritDamage)} = {mon.CritDamage},
            {nameof(mon.Resistance)} = {mon.Resistance},
            {nameof(mon.Accuracy)} = {mon.Accuracy},
            {nameof(mon.archetype)} = {nameof(Archetype)}.{mon.archetype},
            {nameof(mon.grade)} = {mon.grade},
            {nameof(mon.isFusion)} = {mon.isFusion.ToString().ToLower()},
            {nameof(mon.obtainable)} = {mon.obtainable.ToString().ToLower()},
            {nameof(mon.Awakened)} = {mon.Awakened.ToString().ToLower()},
            {nameof(mon.base_hp)} = {mon.base_hp},
            {nameof(mon.base_attack)} = {mon.base_attack},
            {nameof(mon.base_defense)} = {mon.base_defense},
            {nameof(mon.Skills)} = new {nameof(SkillDef)}[] {{
                {string.Join(@",
                ", mon.Skills.Select(sk => $@"new {nameof(SkillDef)}(){{
                    {nameof(sk.Pk)} = {sk.Pk},
                    {nameof(sk.Com2usId)} = {sk.Com2usId},
                    {nameof(sk.Name)} = ""{sk.Name}"",
                    {nameof(sk.Cooltime)} = {(sk.Cooltime.HasValue ? sk.Cooltime.ToString() : "null")},
                    {nameof(sk.Hits)} = {(sk.Hits.HasValue ? sk.Hits.ToString() : "null")},
                    {nameof(sk.Passive)} = {sk.Passive.ToString().ToLower()},
                    {nameof(sk.LevelProgressDescription)} = ""{sk.LevelProgressDescription.Replace("\r","").Replace("\n","\\n")}"",
                    {nameof(sk.MultiplierFormulaRaw)} = ""{sk.MultiplierFormulaRaw.Replace("\"", "\\\"")}"",
                    {nameof(sk.SkillEffect)} = new {nameof(SkillEff)}[] {{
                        {string.Join(@",
                        ", sk.SkillEffect.Select(skef => $@"new {nameof(SkillEff)}(){{
                            {nameof(skef.Name)} = ""{skef.Name}"",
                            {nameof(skef.IsBuff)} = {skef.IsBuff.ToString().ToLower()},
                        }}"))}
                    }}
                }}"))}
            }}
        }};
";
            return s;
        }

        static DateTime lastRequest = DateTime.Now;

        public static T AskFor<T>(string url) where T : class {
            while ((DateTime.Now - lastRequest).Seconds < 1) {
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

            using (var stream = new StreamReader(respStr)) {
                resp = stream.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<T>(resp);
        }
    }
}