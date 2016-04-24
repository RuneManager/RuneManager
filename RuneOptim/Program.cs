using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RuneOptim
{
    class Program
    {
        /// <summary>
        /// Original test program.
        /// Do not use
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string save = System.IO.File.ReadAllText("..\\..\\save.json");
            Save data = JsonConvert.DeserializeObject<Save>(save);
            foreach (Monster mon in data.Monsters)
            {
                var equipedRunes = data.Runes.Where(r => r.AssignedId == mon.ID);

                foreach (Rune rune in equipedRunes)
                {
                    mon.ApplyRune(rune);
                }

                var stats = mon.GetStats();

            }

            var rsets = new Rune[(int)RuneSet.Destroy + 1][][];
            foreach (RuneSet r in Enum.GetValues(typeof(RuneSet)))
            {
                rsets[(int)r] = new Rune[6][];
                for (int i = 0; i < 6; ++i)
                {
                    rsets[(int)r][i] = data.Runes.Where(t => t.Set.Equals(r) && t.Slot == i).ToArray();
                }
            }

            var veromos = data.GetMonster("Veromos");
            var belladeon = data.GetMonster("Belladeon");
            var sath = data.GetMonster("Sath");
            var verdehile = data.GetMonster("Verdehile");
            var acasis = data.GetMonster("Acasis (In Storage)");
            var shannon = data.GetMonster("Shannon");
            var bernard = data.GetMonster("Bernard");
            var chasun = data.GetMonster("Chasun");
            var arnold = data.GetMonster("Arnold");
            var fuco = data.GetMonster("Fuco");
            var ahman = data.GetMonster("Ahman");
            var orochi = data.GetMonster("Orochi");
            var theomars = data.GetMonster("Theomars");
            var basalt = data.GetMonster("Basalt");
            var baretta = data.GetMonster("Baretta");
            var briand = data.GetMonster("Briand");
            var lapis = data.GetMonster("Lapis");
            var megan = data.GetMonster("Megan");
            var lyn = data.GetMonster("Lyn");
            var garoche = data.GetMonster("Garoche (In Storage)");
            var lushen1 = data.GetMonster(20);
            var lushen2 = data.GetMonster(21);
            var mav = data.GetMonster("Mav");
            var darion = data.GetMonster("Darion");
            var gina = data.GetMonster("Gina");
            var xing = data.GetMonster("Xing Zhe");
            
            foreach (Rune r in data.Runes)
            {
                r.Locked = false;
            }

            Loadout best;
            
            
            best = MakeBuild(veromos, MakeSets(data.Runes, 
                r => r.MainType == Attr.HealthPercent || r.MainType == Attr.Speed || r.MainType == Attr.Accuracy, 
                r => r.HealthPercent > 0 && r.Speed > 0, 
                new RuneSet[]{RuneSet.Swift, RuneSet.Energy}),
                s => s.Health, 
                s => s.Contains(RuneSet.Swift) && s.Contains(RuneSet.Energy), 
                s => s.Accuracy >= 60 && s.Speed > 200 && s.Health > 22000);
            
            Console.WriteLine("vero: " + best);
            best.Lock();
            
            best = MakeBuild(belladeon, MakeSets(data.Runes, 
                Attr.Speed, Attr.HealthPercent, Attr.Accuracy, 
                r => r.HealthPercent > 10 || r.Accuracy > 5, 
                new RuneSet[]{RuneSet.Swift, RuneSet.Focus}),
                s => s.Speed / 3 + s.Defense / 50 + s.Health / 300 - s.CritRate + s.Accuracy / 2, 
                s => s.Contains(RuneSet.Swift) && s.Contains(RuneSet.Focus), 
                s => s.Accuracy >= 70 && s.Speed >= 170 && s.Health >= 15000 && s.Defense >= 950);

            Console.WriteLine("bella: " + best);
            best.Lock();

            best = MakeBuild(sath, MakeSets(data.Runes,
                Attr.AttackPercent, Attr.CritRate, Attr.AttackPercent,
                r => r.Speed > 0 && (r.AttackPercent > 0 || r.CritRate > 0),
                new RuneSet[] { RuneSet.Swift, RuneSet.Blade }),
                s => s.Speed + s.CritDamage * 2 + s.Attack / 10 + s.CritRate * 3 - Math.Max(0, s.CritRate - 100) * 3,
                s => s.Contains(RuneSet.Swift) && s.Contains(RuneSet.Blade),
                s => s.Attack >= 1900 && s.Speed >= 143 && s.CritRate >= 95);

            Console.WriteLine("sath: " + best);
            best.Lock(); 
            
            best = MakeBuild(verdehile, MakeSets(data.Runes,
                Attr.Speed, Attr.CritRate, Attr.AttackPercent,
                r => r.Speed > 0 && (r.HealthPercent > 0 || r.CritRate > 0),
                new RuneSet[] { RuneSet.Violent, RuneSet.Blade }),
                s => s.Speed + s.Health / 500 + s.CritRate + s.Attack / 50 + s.Defense / 50,
                s => s.Contains(RuneSet.Violent) && s.Contains(RuneSet.Blade),
                s => s.Health >= 10000 && s.Speed >= 120 && s.CritRate >= 95);

            Console.WriteLine("verd: " + best);
            best.Lock();

            best = MakeBuild(acasis, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.HealthPercent, Attr.Accuracy,
                r => r.Accuracy > 0,
                new RuneSet[] { RuneSet.Despair, RuneSet.Focus }),
                s => s.Speed + s.Health / 500 + s.Accuracy,
                s => s.Contains(RuneSet.Despair) && s.Contains(RuneSet.Focus),
                s => s.Health >= 13000 && s.Speed >= 110 && s.Accuracy >= 80);/**/

            Console.WriteLine("acas: " + best);
            best.Lock();

            best = MakeBuild(shannon, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.HealthPercent, Attr.Accuracy,
                r => r.Accuracy > 0,
                new RuneSet[] { RuneSet.Energy, RuneSet.Focus, RuneSet.Focus }),
                s => s.Defense / 40 + s.Health / 100 + s.Accuracy,
                s => s.Contains(RuneSet.Energy) && s.Count(r => r == RuneSet.Focus) == 2,
                s => s.Health >= 12000 && s.Accuracy >= 60);/**/

            Console.WriteLine("shann: " + best);
            best.Lock();

            best = MakeBuild(bernard, MakeSets(data.Runes,
                Attr.Speed, Attr.HealthPercent, Attr.Accuracy,
                r => r.Accuracy + r.Speed / 2 > 6,
                new RuneSet[] { RuneSet.Energy, RuneSet.Swift }),
                s => s.Defense / 40 + s.Health / 100 + s.Accuracy + s.Speed,
                s => s.Contains(RuneSet.Energy) && s.Contains(RuneSet.Swift),
                s => s.Health >= 13000 && s.Speed >= 170 && s.Accuracy >= 60);/**/

            Console.WriteLine("bern: " + best);
            best.Lock();

            best = MakeBuild(chasun, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.HealthPercent, Attr.HealthPercent,
                r => r.HealthPercent >= 10,
                new RuneSet[] { RuneSet.Energy, RuneSet.Energy, RuneSet.Guard }),
                s => s.Health / 300 + s.Resistance + s.Speed / 2 - s.CritRate / 2 + s.Accuracy / 2,
                s => s.Contains(RuneSet.Guard) && s.Count(r => r == RuneSet.Energy) == 2,
                s => s.Health >= 30000 && s.Resistance >= 80);/**/

            Console.WriteLine("chas: " + best);
            best.Lock();

            best = MakeBuild(arnold, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.HealthPercent, Attr.HealthPercent,
                r => r.HealthPercent * 2 + r.DefensePercent + r.Speed / 2 >= 8,
                new RuneSet[] { RuneSet.Guard, RuneSet.Shield, RuneSet.Energy}),
                s => s.Health / 300 + s.Resistance + s.Speed / 2 - s.CritRate / 2 + s.Accuracy / 2,
                s => true,//s.Contains(RuneSet.Guard) && s.Contains(RuneSet.Shield),
                s => s.Health >= 20000);/**/

            Console.WriteLine("arn: " + best);
            best.Lock();

            best = MakeBuild(fuco, MakeSets(data.Runes,
                Attr.Speed, Attr.AttackPercent, Attr.AttackPercent,
                r => r.Speed > 6 | r.AttackPercent > 6,
                new RuneSet[] { RuneSet.Swift, RuneSet.Guard }),
                s => s.Health / 300 + s.Defense + s.Speed + s.Defense / 20 + s.Accuracy / 2,
                s => s.Contains(RuneSet.Guard) && s.Contains(RuneSet.Swift),
                s => s.Speed >= 170);/**/

            Console.WriteLine("fuco: " + best);
            best.Lock();

            best = MakeBuild(ahman, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.CritRate, Attr.HealthPercent,
                r => r.HealthPercent > 0 | r.CritRate > 0,
                new RuneSet[] { RuneSet.Energy, RuneSet.Blade, RuneSet.Blade }),
                s => s.CritRate + s.Health / 250 + s.Speed / 3,
                s => s.Contains(RuneSet.Energy) && s.Count(r => r == RuneSet.Blade) == 2,
                s => s.Health >= 15000 && s.CritRate >= 90);/**/

            Console.WriteLine("ahm: " + best);
            best.Lock();

            best = MakeBuild(orochi, MakeSets(data.Runes,
                Attr.Speed, Attr.CritRate, Attr.Accuracy,
                r => r.Accuracy + r.CritRate + r.AttackPercent / 2 + r.HealthPercent / 2 >= 10,
                new RuneSet[] { RuneSet.Focus, RuneSet.Blade }),
                s => s.CritRate + s.Health / 500 + s.Speed / 2 + s.Accuracy,
                s => s.Contains(RuneSet.Focus) && s.Contains(RuneSet.Blade),
                s => s.Health >= 8000 && s.CritRate >= 90 && s.Accuracy >= 60);/**/

            Console.WriteLine("oro: " + best);
            best.Lock();

            best = MakeBuild(theomars, MakeSets(data.Runes,
                Attr.Speed, Attr.CritDamage, Attr.AttackPercent,
                r => r.Speed + r.CritRate + r.CritDamage + r.AttackPercent >= 10,
                new RuneSet[] { RuneSet.Violent, RuneSet.Blade }),
                s => s.CritRate + s.Health / 500 + s.Speed + s.CritDamage - Math.Max(0, s.CritRate - 61),
                s => s.Contains(RuneSet.Violent) && s.Contains(RuneSet.Blade),
                s => s.Health >= 8000 && s.CritRate >= 61 && s.CritDamage >= 100);/**/

            Console.WriteLine("theo: " + best);
            best.Lock();

            best = MakeBuild(basalt, MakeSets(data.Runes,
                Attr.DefensePercent, Attr.DefensePercent, Attr.DefensePercent,
                r => r.Speed + r.CritRate + r.CritDamage + r.Accuracy + r.DefensePercent >= 10,
                new RuneSet[] { RuneSet.Violent, RuneSet.Energy }),
                s => s.Defense / 20 + s.Health / 500 + s.Speed + s.CritDamage,
                s => s.Contains(RuneSet.Violent) && s.Contains(RuneSet.Energy),
                s => s.Health >= 9000 && s.Defense >= 1000);/**/

            Console.WriteLine("basa: " + best);
            best.Lock();

            best = MakeBuild(baretta, MakeSets(data.Runes,
                Attr.AttackPercent, Attr.HealthPercent, Attr.Accuracy,
                r => r.Speed + r.Accuracy + r.HealthPercent >= 10,
                new RuneSet[] { RuneSet.Swift, RuneSet.Energy }),
                s => s.Health / 500 + s.Speed + s.Accuracy,
                s => s.Contains(RuneSet.Swift) && s.Contains(RuneSet.Energy),
                s => s.Health >= 14000 && s.Speed > 140);/**/

            Console.WriteLine("baretta: " + best);
            best.Lock();

            best = MakeBuild(briand, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.Null, Attr.Null,
                r => r.Speed + r.Resistance + r.HealthPercent + r.Accuracy >= 5,
                new RuneSet[] { RuneSet.Despair, RuneSet.Energy }),
                s => s.Health / 400 + s.Speed / 3 + s.Accuracy / 2,
                s => s.Contains(RuneSet.Despair) && s.Contains(RuneSet.Energy),
                s => s.Health >= 20000);/**/

            Console.WriteLine("briand: " + best);
            best.Lock();

            best = MakeBuild(lapis, MakeSets(data.Runes,
                Attr.AttackPercent, Attr.AttackPercent, Attr.AttackPercent,
                r => r.Speed + r.AttackPercent * 2 + r.CritRate / 2 + r.CritDamage / 2 >= 5,
                new RuneSet[] { RuneSet.Vampire, RuneSet.Revenge }),
                s => s.Attack / 40 + s.Speed / 2,
                s => s.Contains(RuneSet.Vampire) && s.Contains(RuneSet.Revenge),
                s => s.Attack >= 1900);/**/

            Console.WriteLine("lapis: " + best);
            best.Lock();


            best = MakeBuild(megan, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.HealthPercent, Attr.Null,
                r => r.Speed + r.HealthPercent + r.Accuracy >= 5,
                new RuneSet[] { RuneSet.Swift, RuneSet.Energy }),
                s => s.Health / 400 + s.Speed / 2 + s.Accuracy,
                s => s.Contains(RuneSet.Swift) && s.Contains(RuneSet.Energy));/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/**/

            Console.WriteLine("megan: " + best);
            best.Lock();

            best = MakeBuild(lyn, MakeSets(data.Runes,
               Attr.Null, Attr.CritDamage, Attr.Null,
               r => r.CritRate + r.CritDamage + r.HealthPercent + r.Speed >= 10,
               new RuneSet[] { RuneSet.Violent, RuneSet.Blade }),
               s => s.Health / 200 + s.Speed / 2 + s.CritRate * 3 + s.CritDamage * 2  + s.Attack / 80 + s.Defense / 30,
               s => s.Contains(RuneSet.Violent) && s.Count(r => r == RuneSet.Null) == 1);/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/**/

            Console.WriteLine("lyn: " + best);
            best.Lock();
            
            best = MakeBuild(garoche, MakeSets(data.Runes,
                Attr.HealthPercent, Attr.HealthPercent, Attr.HealthPercent,
                r => r.HealthFlat + r.HealthPercent + r.Accuracy + r.Speed >= 12,
                new RuneSet[] { RuneSet.Focus, RuneSet.Blade, RuneSet.Shield }),
                s => s.Health / 400 + s.Speed / 2 + s.Accuracy,
                s => s.Count(q => q == RuneSet.Null) == 0,
                s => s.Accuracy >= 40 && s.Health >= 13000);/**/

            Console.WriteLine("garoche: " + best);
            best.Lock();

            var qq = data.Runes.Where(r => r.Locked == false && r.MainType == Attr.CritDamage && (r.Set == RuneSet.Fatal || r.Set == RuneSet.Blade)).OrderByDescending(r => r.CritDamage).ToArray();
            if (qq.Length > 0)
                Console.WriteLine(qq[0].ID);
            if (qq.Length > 1)
                Console.WriteLine(qq[1].ID);


            best = MakeBuild(lushen1, MakeSets(data.Runes,
                Attr.Null, Attr.Null, Attr.Null,
                r => r.Speed + r.AttackPercent + r.CritRate + r.CritDamage >= 5,
                new RuneSet[] { RuneSet.Fatal, RuneSet.Blade }),
                s => s.Health / 300 + s.Attack / 20 + s.Speed + s.CritRate * 3 + s.CritDamage * 2,
                s => s.Contains(RuneSet.Fatal) && s.Contains(RuneSet.Blade));/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/**/

            Console.WriteLine("lushen1: " + best);
            best.Lock();

            best = MakeBuild(lushen2, MakeSets(data.Runes,
                Attr.Null, Attr.Null, Attr.Null,
                r => r.Speed + r.AttackPercent + r.CritRate + r.CritDamage >= 5,
                new RuneSet[] { RuneSet.Fatal, RuneSet.Blade }),
                s => s.Health / 300 + s.Attack / 20 + s.Speed + s.CritRate * 3 + s.CritDamage * 2,
                s => s.Contains(RuneSet.Fatal) && s.Contains(RuneSet.Blade));/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/**/

            Console.WriteLine("lushen2: " + best);
            best.Lock();
            
            best = MakeBuild(xing, MakeSets(data.Runes,
                Attr.Null, Attr.Null, Attr.Null,
                r => r.Speed + r.AttackPercent + r.CritRate + r.CritDamage + r.HealthPercent >= 10,
                new RuneSet[] { RuneSet.Energy}),
                s => s.Health / 100 + s.Attack / 8 + s.Speed + s.CritRate * 4 + s.CritDamage * 3 + s.Defense / 10,
                s => !s.Contains(RuneSet.Null));/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/**/

            Console.WriteLine("xing: " + best);
            best.Lock();

            best = MakeBuild(darion, MakeSets(data.Runes,
                Attr.Null, Attr.Null, Attr.Null,
                r => r.Speed / 2 + r.DefensePercent + r.AttackPercent + r.HealthFlat + r.HealthPercent * 2 >= 10,
                new RuneSet[] { RuneSet.Energy }),
                s => s.Health / 200 + s.Attack / 40 + s.Speed / 2 + s.Defense / 30,
                s => true,
                s => s.Health >= 15000);/**/

            Console.WriteLine("darion: " + best);
            /*
            best = MakeBuild(darion, MakeSets(data.Runes,
                r => r.AttackPercent + r.DefensePercent + r.HealthPercent >= 18,
                r => r.AttackPercent + r.DefensePercent + r.HealthPercent + r.HealthFlat >= 12),
                s => s.Health / 200 + s.Attack / 50 + s.Speed + s.Defense / 25,
                s => !s.Contains(RuneSet.Null));/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/**

            Console.WriteLine("darion: " + best);
            best.Lock();
            */
            /*
            best = MakeBuild(gina, MakeSets(data.Runes,
                r => r.MainType != Attr.CritRate && r.MainType != Attr.CritDamage && r.MainType != Attr.Resistance && (r.Speed + r.HealthPercent > 0),
                r => r.Accuracy + r.Speed + r.HealthPercent + r.HealthFlat >= 10),
                s => s.Health / 200 + s.Accuracy + s.Speed / 2 + s.Defense / 30,
                s => !s.Contains(RuneSet.Null));/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/**

            Console.WriteLine("gina: " + best);
            best.Lock();
            */
            /*
            best = MakeBuild(mav, MakeSets(data.Runes,
                r => true,
                r => true),
                s => s.Health,
                s => !s.Contains(RuneSet.Null), null);/*,
                s => s.Accuracy >= 50 && s.Health >= 15000 && s.Speed > 130);/

            Console.WriteLine("mav: " + best);
            best.Lock();
            */
            Console.ReadKey();
        }

        #region Builders

        public static IEnumerable<Rune>[] MakeSets(IList<Rune> runes, Predicate<Rune> evens, Predicate<Rune> odds, RuneSet[] reqsets = null)
        {
            IEnumerable<Rune>[] runesSlot = new IEnumerable<Rune>[6];

            Predicate<Rune> set = r => true;
            if (reqsets != null)
            {
                int reqCount = 0;
                foreach (RuneSet s in reqsets)
                    reqCount += Rune.SetRequired(s);

                if (reqCount == 6)
                    set = r => reqsets.Any(s => s == r.Set);
            }

            runesSlot[0] = runes.Where(r => r.Slot == 1 && !r.Locked && odds.Invoke(r) && set.Invoke(r));
            runesSlot[1] = runes.Where(r => r.Slot == 2 && !r.Locked && evens.Invoke(r) && set.Invoke(r));
            runesSlot[2] = runes.Where(r => r.Slot == 3 && !r.Locked && odds.Invoke(r) && set.Invoke(r));
            runesSlot[3] = runes.Where(r => r.Slot == 4 && !r.Locked && evens.Invoke(r) && set.Invoke(r));
            runesSlot[4] = runes.Where(r => r.Slot == 5 && !r.Locked && odds.Invoke(r) && set.Invoke(r));
            runesSlot[5] = runes.Where(r => r.Slot == 6 && !r.Locked && evens.Invoke(r) && set.Invoke(r));

            return runesSlot;
        }
        public static IEnumerable<Rune>[] MakeSets(IList<Rune> runes, Attr slot2, Attr slot4, Attr slot6, Predicate<Rune> odds, RuneSet[] reqsets = null)
        {
            Rune[][] runesSlot = new Rune[6][];

            Predicate<Rune> set = r => true;
            if (reqsets != null)
            {
                int reqCount = 0;
                foreach (RuneSet s in reqsets)
                    reqCount += Rune.SetRequired(s);

                if (reqCount == 6)
                    set = r => reqsets.Any(s => s == r.Set);
            }

            var unlocked = runes.Where(r => !r.Locked).ToArray();
            var sets = unlocked.Where(r => set.Invoke(r)).ToArray();
            var odd = sets.Where(r => odds.Invoke(r)).ToArray();

            runesSlot[0] = odd.Where(r => r.Slot == 1).ToArray();
            runesSlot[1] = sets.Where(r => r.Slot == 2 && (r.MainType == slot2 || slot2 == Attr.Null)).ToArray();
            runesSlot[2] = odd.Where(r => r.Slot == 3).ToArray();
            runesSlot[3] = sets.Where(r => r.Slot == 4 && (r.MainType == slot4 || slot4 == Attr.Null)).ToArray();
            runesSlot[4] = odd.Where(r => r.Slot == 5).ToArray();
            runesSlot[5] = sets.Where(r => r.Slot == 6 && (r.MainType == slot6 || slot6 == Attr.Null)).ToArray();

            foreach (Rune[] rs in runesSlot.Where(r => r.Length == 0))
                Console.WriteLine("No runes for slot " + (runesSlot.ToList().IndexOf(rs) + 1) + ":(");

            return runesSlot;
        }

        public static Loadout MakeBuild(Monster mon, IEnumerable<Rune>[] runesSlot, Func<Stats, double> sort, Predicate<RuneSet[]> reqsets = null, Predicate<Stats> minimum = null)
        {

            System.Collections.Generic.SynchronizedCollection<Monster> tests = new SynchronizedCollection<Monster>();//new List<Monster>();
            long count = 0;
            long total = runesSlot[0].Count();
            total *= runesSlot[1].Count();
            total *= runesSlot[2].Count();
            total *= runesSlot[3].Count();
            total *= runesSlot[4].Count();
            total *= runesSlot[5].Count();
            long complete = total;

            DateTime timer = DateTime.Now;

            Console.WriteLine(count + "/" + total + "  " + String.Format("{0:P2}", (double)(count + complete - total) / (double)complete));

            Parallel.ForEach<Rune>(runesSlot[0], r0 =>
            {
                int kill = 0;
                int plus = 0;

                foreach (Rune r1 in runesSlot[1])
                {
                    foreach (Rune r2 in runesSlot[2])
                    {
                        foreach (Rune r3 in runesSlot[3])
                        {
                            foreach (Rune r4 in runesSlot[4])
                            {
                                foreach (Rune r5 in runesSlot[5])
                                {
                                    Monster test = new Monster(mon);
                                    test.ApplyRune(r0);
                                    test.ApplyRune(r1);
                                    test.ApplyRune(r2);
                                    test.ApplyRune(r3);
                                    test.ApplyRune(r4);
                                    test.ApplyRune(r5);

                                    if (minimum != null && !minimum.Invoke(test.GetStats()))
                                    {
                                        //total--;
                                        kill++;
                                        //Interlocked.Decrement(ref total);
                                    }
                                    else if (reqsets != null && !reqsets.Invoke(test.Current.sets))
                                    {
                                        kill++;
                                        //Interlocked.Decrement(ref total);
                                    }
                                    else
                                    {
                                        plus++;
                                        tests.Add(test);
                                        //Interlocked.Increment(ref count);
                                        //count++;
                                    }


                                    if (DateTime.Now > timer.AddSeconds(1))
                                    {
                                        timer = DateTime.Now;
                                        Console.WriteLine(count + "/" + total + "  " + String.Format("{0:P2}", (double)(count + complete - total) / (double)complete));
                                    }
                                }
                                Interlocked.Add(ref total, -kill);
                                kill = 0;
                                Interlocked.Add(ref count, plus);
                                plus = 0;
                            }
                        }
                    }
                }
            });
            Console.WriteLine(count + "/" + total + "  " + String.Format("{0:P2}", (double)(count + complete - total) / (double)complete));

            var desc = tests.Where(t => t != null).OrderByDescending(r => sort(r.GetStats())).Take(10).ToArray();
            foreach (var l in desc)
            {
                Console.WriteLine(l.GetStats().Health + "  " + l.GetStats().Attack + "  " + l.GetStats().Defense + "  " + l.GetStats().Speed
                    + "  " + l.GetStats().CritRate + "%" + "  " + l.GetStats().CritDamage + "%" + "  " + l.GetStats().Resistance + "%" + "  " + l.GetStats().Accuracy + "%");
            }

            if (desc.Count() == 0)
            {
                Console.WriteLine("No builds :(");
                return null;
            }

            var best = desc.First().Current;

            foreach (Rune r in best.runes)
            {
                r.Locked = true;
            }
            
            return best;

        }
        #endregion
    }
}
