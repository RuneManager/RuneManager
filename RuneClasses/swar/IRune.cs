namespace RuneOptim.swar {
    public interface IRune {

        bool Locked { get; set; }

    }


    public class RuneOg : RuneLink, IRune {
        public bool Locked { get; set; }
    }

    public class RuneBaked : RuneLink, IRune {

        public bool Locked { get; set; }
    }

    public class RuneBranch : RuneLink, IRune {
        public bool Locked { get; set; }

    }

}
