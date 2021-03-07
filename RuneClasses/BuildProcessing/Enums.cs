namespace RuneOptim.BuildProcessing {
    public enum BuildResult {
        Success = 0,
        Failure = 1,
        NoPermutations = 2,
        NoSorting = 3,
        BadRune = 4,
    }

    public enum FilterType {
        None = -1,
        Or = 0,
        And = 1,
        Sum = 2,
        SumN = 3
    }

    public enum BuildType {
        Build = 0,
        Lock,
        Link,
    }

}