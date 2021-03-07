namespace RuneOptim.BuildProcessing {
    public class BuildOriginalBad : IBuildStrategyDefinition {
        public int Order { get => -1; }
        public string Name { get => "(Deprecated)"; }

        public IBuildRunner GetRunner() {
            return null;
        }

        public bool IsValid(Build build) {
            return true;
        }
    }

}
