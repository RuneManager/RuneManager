using RuneOptim.swar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
    /// <summary>
    /// Pick good looking runes by set, make a full house.
    /// Match 2s and 4s by comparing bits
    /// </summary>
    public class BuildBitMatcher : IBuildStrategyDefinition {
        public int Order { get => 100; }
        public string Name { get; }

        public IBuildRunner GetRunner() {
            return new BuildBitMatcherBaseRunner();
        }

        public bool IsValid(Build b) {
            return !b.AllowBroken;
        }

    }


}
