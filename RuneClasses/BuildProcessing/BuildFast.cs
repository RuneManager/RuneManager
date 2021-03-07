using System;

namespace RuneOptim.BuildProcessing {
    public abstract class BuildFast : IBuildStrategyDefinition {
        public abstract int Order { get; }
        public abstract string Name { get; }

        public abstract IBuildRunner GetRunner();

        public virtual bool IsValid(Build b) {
            foreach (var a in RuneOptim.BuildProcessing.Build.ExtraEnums) {
                if (b.Sort[a] != 0)
                    return false;
            }
            return true;
        }
    }

}
