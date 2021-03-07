using RuneOptim.swar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
    /// <summary>
    /// Perfect for SWIFT
    /// </summary>
    public class BuildFourSetBrokenFast : BuildFast {
        public override int Order { get => 2; }
        public override string Name { get; }

        public override IBuildRunner GetRunner() {
            return new BuildFourSetBrokenFastRunner();
        }

        public override bool IsValid(Build b) {

            if (!base.IsValid(b))
                return false;

            // todo: maybe consider a BuildFourAndTwoSetFast
            if (b.RequiredSets.Count < 1)
                return false;
            if (!b.RequiredSets.Any(rs => Rune.SetRequired(rs) == 4))
                return false;

            return b.AllowBroken;
        }

        public class BuildFourSetBrokenFastRunner : BuildSetDropper.BuildSetDropperRunner {

            public int takeNum = 10;

            protected override Rune[][] messupRunes(IEnumerable<Rune> inRunes) {

                var set4 = build.RequiredSets.FirstOrDefault(rs => Rune.SetRequired(rs) == 4);

                var reqs = inRunes.Where(r => r.Set == set4).OrderByDescending(r => build.CalcScore(r, this.build.Mon)).ToArray();
                var opts = inRunes.Except(reqs).OrderByDescending(r => build.CalcScore(r, this.build.Mon)).ToArray();

                reqs = reqs.GroupBy(r => r.Slot).SelectMany(g => g.Take(takeNum)).ToArray();
                opts = opts.GroupBy(r => r.Slot).SelectMany(g => g.Take(takeNum)).ToArray();

                return reqs.Concat(opts).GroupBy(r => r.Slot).OrderBy(r => r.Key).Select(r => r.ToArray()).ToArray();
            }
            

            
        }

    }

}
