using RuneOptim.Management;
using RuneOptim.swar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {

    /// <summary>
    /// No guessing, no skipping.
    /// </summary>
    public class BuildFallback : IBuildStrategyDefinition {
        public int Order { get => 10_000; }
        public string Name { get; }


        public IBuildRunner GetRunner() {
            return new BuildFallbackRunner();
        }

        public bool IsValid(Build b) {
            return true;
        }

        public class BuildFallbackRunner : BuildRunner<bool> {
            
        }

    }
}
