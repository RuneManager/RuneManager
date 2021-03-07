using System;

namespace RuneOptim.BuildProcessing {
    public class BuildFlatScore : BuildFast {
        public override int Order { get => 1_000; }
        public override string Name { get; }

        public override IBuildRunner GetRunner() {
            throw new NotImplementedException();
        }

    }
}
