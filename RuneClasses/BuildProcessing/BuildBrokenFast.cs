using RuneOptim.swar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
    /// <summary>
    /// Perfect for SPEED
    /// </summary>
    public class BuildBrokenFast : BuildFast {
        public override int Order { get => 1; }
        public override string Name { get; }

        public override IBuildRunner GetRunner() {
            return new BuildBrokenFastRunner();
        }

        public override bool IsValid(Build b) {

            if (!base.IsValid(b))
                return false;

            if (b.RequiredSets.Any())
                return false;

            return b.AllowBroken;
        }

        public class BuildBrokenFastRunner : BuildRunner<bool> {

            public int takeNum = 5;

            protected override Rune[][] messupRunes(IEnumerable<Rune> inRunes) {



                return inRunes.GroupBy(r => r.Slot).OrderBy(r => r.Key).Select(g => g.OrderByDescending(r => build.CalcScore(r, this.build.Mon)).Take(takeNum).ToArray()).ToArray();
            }

        }
    }

}
