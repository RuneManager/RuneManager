using RuneOptim.swar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
    /// <summary>
    /// Use fast to pick a non-Extra winner, then pick the winner based on extra
    /// </summary>
    public class BuildTieBreakerFast : IBuildStrategyDefinition {
        public int Order { get => 9_000; }
        public string Name { get; }

        public IBuildRunner GetRunner() {
            return new BuildTieBreakerFastRunner();
        }

        public bool IsValid(Build build) {
            return RuneOptim.BuildProcessing.Build.StatEnums.Any(a => build.Sort[a] != 0) && RuneOptim.BuildProcessing.Build.ExtraEnums.Any(a => build.Sort[a] != 0);
        }

        // todo: actually implement it properly
        public class BuildTieBreakerFastRunner : BuildSetDropper.BuildSetDropperRunner {
            public int takeNum = 5;

            protected override Rune[][] messupRunes(IEnumerable<Rune> inRunes) {

                var reqs = inRunes.Where(r => build.RequiredSets.Contains(r.Set)).OrderByDescending(r => build.CalcScore(r, this.build.Mon)).ToArray();
                var opts = inRunes.Except(reqs).OrderByDescending(r => build.CalcScore(r, this.build.Mon)).ToArray();

                reqs = reqs.GroupBy(r => r.Slot + "_" + r.Set).SelectMany(g => g.Take(takeNum)).ToArray();
                opts = opts.GroupBy(r => r.Slot + "_" + r.Set).SelectMany(g => g.Take(takeNum)).ToArray();

                return reqs.Concat(opts).GroupBy(r => r.Slot).OrderBy(r => r.Key).Select(r => r.ToArray()).ToArray();
            }
            
        }

    }


    public class BuildBitTieBreakerFast : IBuildStrategyDefinition {
        public int Order { get => 8_000; }
        public string Name { get; }

        public IBuildRunner GetRunner() {
            return new BuildBitTieBreakerFastRunner();
        }

        public bool IsValid(Build build) {
            return RuneOptim.BuildProcessing.Build.StatEnums.Any(a => build.Sort[a] != 0) && 
                RuneOptim.BuildProcessing.Build.ExtraEnums.Any(a => build.Sort[a] != 0) &&
                !build.AllowBroken;
        }
    }

    public class BuildBitTieBreakerFastRunner : BuildBitMatcherBaseRunner {
        public int takeNum = 10;

        protected override Rune[][] messupRunes(IEnumerable<Rune> inRunes) {

            int?[] fakes = new int?[6];
            bool[] pred = new bool[6];

            build.getPrediction(fakes, pred);

            var hps = inRunes.StdDev(out double hpa, r => r[Attr.HealthPercent, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);
            var hfs = inRunes.StdDev(out double hfa, r => r[Attr.HealthFlat, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);

            var aps = inRunes.StdDev(out double apa, r => r[Attr.AttackPercent, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);
            var afs = inRunes.StdDev(out double afa, r => r[Attr.AttackFlat, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);

            var dps = inRunes.StdDev(out double dpa, r => r[Attr.DefensePercent, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);
            var dfs = inRunes.StdDev(out double dfa, r => r[Attr.DefenseFlat, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);

            var crs = inRunes.StdDev(out double cra, r => r[Attr.CritRate, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);
            var cds = inRunes.StdDev(out double cda, r => r[Attr.CritDamage, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);

            var acs = inRunes.StdDev(out double aca, r => r[Attr.Accuracy, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);
            var res = inRunes.StdDev(out double rea, r => r[Attr.Resistance, fakes[r.Slot - 1] ?? 0, pred[r.Slot - 1]]);

            var aStats = new Stats();
            aStats.Health = build.Mon.Health * ((hpa + hps * 1.5) * 0.01) + hfa + hfs * 1.5;
            aStats.Attack = build.Mon.Attack * ((apa + aps * 1.5) * 0.01) + afa + afs * 1.5;
            aStats.Defense = build.Mon.Defense * ((dpa + dps * 1.5) * 0.01) + dfa + dfs * 1.5;
            aStats.CritRate = cra + crs * 1.5;
            aStats.CritDamage = cda + cds * 1.5;
            aStats.Accuracy = aca + acs * 1.5;
            aStats.Resistance = rea + res * 1.5;


            var reqs = inRunes.Where(r => build.RequiredSets.Contains(r.Set))
                .OrderByDescending(r => build.CalcScore(r, this.build.Mon, aStats)).ToArray();
            var opts = inRunes.Except(reqs)
                .OrderByDescending(r => build.CalcScore(r, this.build.Mon, aStats)).ToArray();

            reqs = reqs.GroupBy(r => r.Slot + "_" + r.Set).SelectMany(g => g.Take(takeNum)).ToArray();
            opts = opts.GroupBy(r => r.Slot + "_" + r.Set).SelectMany(g => g.Take(takeNum)).ToArray();

            return reqs.Concat(opts).GroupBy(r => r.Slot).OrderBy(r => r.Key).Select(r => r.ToArray()).ToArray();
        }
    }

}
