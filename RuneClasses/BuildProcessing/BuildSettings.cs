using RuneOptim.swar;

namespace RuneOptim.BuildProcessing {
    public struct BuildSettings {
        public bool AllowBroken;
        public bool RunesUseEquipped;
        public bool RunesUseLocked;
        public int BuildGenerate;
        public int BuildTake;
        public int BuildTimeout;
        public Stats Shrines;
        public bool BuildDumpBads;
        public bool BuildSaveStats;
        public bool BuildGoodRunes;
        public bool RunesOnlyFillEmpty;
        public bool RunesDropHalfSetStat;
        public bool IgnoreLess5;

        public BuildSettings Default() {
            return new BuildSettings() {
                AllowBroken = false,
                RunesUseEquipped = false,
                RunesUseLocked = false,
                BuildGenerate = 0,
                BuildTake = 0,
                BuildTimeout = 0,
                Shrines = new Stats(),
                BuildDumpBads = false,
                BuildSaveStats = false,
                BuildGoodRunes = false,
                RunesOnlyFillEmpty = false,
                RunesDropHalfSetStat = false,
                IgnoreLess5 = false,
            };

        }


        /*
        
                build.RunesUseEquipped = Program.Settings.UseEquipped;
                build.RunesUseLocked = false;
                build.BuildGenerate = 0;
                build.BuildTake = 0;
                build.BuildTimeout = 0;
                build.Shrines = Program.data.shrines;
                build.BuildDumpBads = true;
                build.BuildSaveStats = saveStats;
                build.BuildGoodRunes = false;
                build.RunesOnlyFillEmpty = Program.fillRunes;
                build.RunesDropHalfSetStat = Program.goFast;
                build.IgnoreLess5 = Program.Settings.IgnoreLess5;
        */
    }

}
