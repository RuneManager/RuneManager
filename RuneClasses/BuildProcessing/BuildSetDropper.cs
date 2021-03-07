using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuneOptim.swar;

namespace RuneOptim.BuildProcessing {
    /// <summary>
    /// Figures out if you can't make a set, then drops subsequent tries.
    /// Basically the old version
    /// </summary>
    public class BuildSetDropper : IBuildStrategyDefinition {
        public int Order { get => 8_000; }
        public string Name { get; }

        public IBuildRunner GetRunner() {
            return new BuildSetDropperRunner();
        }

        public bool IsValid(Build build) {
            return !build.AllowBroken;
        }

        public class sdData {
            public int pop2 = 0;
            public int pop4 = 0;
            public RuneSet set2 = RuneSet.Null;
            public RuneSet set4 = RuneSet.Null;

        }

        public class BuildSetDropperRunner : BuildRunner<sdData> {

            protected override sdData preLoop() {
                return new sdData();
            }

            protected override bool prePick(int s, Rune r, ParallelLoopState state, loopData data, sdData tdat) {

                if (r.SetIs4) {
                    // reset, if the last rune here was a 2-set
                    if (tdat.pop2 == s + 1)
                        tdat.pop2 = 7;
                    // if we haven't claimed a 4-set, or the 4-set is claimed in a higher rune (which we will travel over anyway).
                    if (tdat.set4 == RuneSet.Null || tdat.pop4 >= s + 1) {
                        tdat.set4 = r.Set;
                        tdat.pop4 = s + 1;
                    }
                    // looks like there is a *different* 4-set behind us :S
                    else if (tdat.set4 != r.Set) {
                        var kill = 1;
                        if (s < 5)
                            kill *= runes[5].Length;
                        if (s < 4)
                            kill *= runes[4].Length;
                        if (s < 3)
                            kill *= runes[3].Length;
                        if (s < 2)
                            kill *= runes[2].Length;
                        data.kill += kill;
                        data.skip += kill;
                        return true;
                    }
                }
                else {
                    // reset, if the last rune here was a 4-set
                    if (tdat.pop4 == s + 1)
                        tdat.pop4 = 7;
                    if (tdat.set2 == RuneSet.Null || tdat.pop2 >= s + 1) {
                        tdat.set2 = r.Set;
                        tdat.pop2 = s + 1;
                    }
                    // todo: can/do skip the 2s?

                    else if (tdat.set4 != RuneSet.Null && tdat.set2 != r.Set) {
                        var kill = 1;
                        if (s < 5)
                            kill *= runes[5].Length;
                        if (s < 4)
                            kill *= runes[4].Length;
                        if (s < 3)
                            kill *= runes[3].Length;
                        if (s < 2)
                            kill *= runes[2].Length;
                        data.kill += kill;
                        data.skip += kill;
                        return true;
                    }
                }
                return false;
            }
            
        }
    }
}
