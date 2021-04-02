using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp
{
    public static partial class Program
    {

        public static void BuildPriority(Build build, int deltaPriority)
        {
            build.Priority += deltaPriority;
            var bpri = builds.OrderBy(b => b.Priority).ThenBy(b => b == build ? deltaPriority : 0).ToList();
            int i = 1;
            foreach (var b in bpri)
            {
                if (b.Type == BuildType.Lock)
                {
                    b.Priority = 0;
                }
                else
                    b.Priority = i++;
            }
        }

        public static void RunTest(Build build, Action<Build, BuildResult> onFinish = null)
        {
            if (build.IsRunning)
                throw new InvalidOperationException("This build is already running");

            Task.Factory.StartNew(() => {
                // Allow the window to draw before destroying the CPU
                Thread.Sleep(100);

                // Disregard locked, but honor equippedness checking
                build.RunesUseEquipped = Program.Settings.UseEquipped;
                build.RunesUseLocked = Program.Settings.LockTest;
                build.BuildGenerate = Program.Settings.TestGen;
                build.BuildTake = Program.Settings.TestShow;
                build.BuildTimeout = Program.Settings.TestTime;
                build.Shrines = Program.Data.shrines;
                build.BuildDumpBads = false;
                build.BuildSaveStats = false;
                build.BuildGoodRunes = false;
                build.RunesOnlyFillEmpty = Program.fillRunes;
                build.RunesDropHalfSetStat = Program.goFast;
                build.IgnoreLess5 = Program.Settings.IgnoreLess5;

                build.GenRunes(Program.Data);
                var result = build.GenBuilds();

                onFinish?.Invoke(build, result);
            });
        }

        public static void StopBuild()
        {
            // no build selected, no build running
            if (currentBuild == null)
                return;
            runSource?.Cancel();
            if (currentBuild.runner != null)
            {
                currentBuild.runner.Cancel();
            }
        }

        public static void RunBuild(Build build, bool saveStats = false)
        {
            if (Program.Data == null)
                return;


            if (currentBuild != null)
            {
                if (runTask != null && runTask.Status != TaskStatus.Running)
                    throw new Exception("Already running builds!");
                else
                {
                    runSource.Cancel();
                    if (currentBuild.runner != null)
                    {
                        currentBuild.runner.Cancel();
                    }
                    return;
                }
            }

            runSource = new CancellationTokenSource();
            runToken = runSource.Token;
            runTask = Task.Factory.StartNew(() => {
                runBuild(build, saveStats);
            }, runToken);
        }

        private static void runBuild(Build build, bool saveStats = false)
        {
            runBuild(build, Program.Data, new BuildSettings()
            {
                BuildSaveStats = saveStats,
                RunesUseEquipped = Program.Settings.UseEquipped,
                Shrines = Program.Data.shrines,
                RunesOnlyFillEmpty = Program.fillRunes,
                RunesDropHalfSetStat = Program.goFast,
                IgnoreLess5 = Program.Settings.IgnoreLess5,
                BuildDumpBads = true,
            });
        }

        // TODO: pull more of the App-dependant stuff out
        private static void runBuild(Build build, Save data, BuildSettings bSettings)
        {
            try
            {
                if (build == null)
                {
                    LineLog.Info("Build is null");
                    return;
                }
                // TODO: show this somewhere
                if (currentBuild != null)
                    throw new InvalidOperationException("Already running a build");
                if (build.IsRunning)
                    throw new InvalidOperationException("This build is already running");
                currentBuild = build;

                LineLog.Info("Starting watch " + build.ID + " " + build.MonName);

                Stopwatch buildTime = Stopwatch.StartNew();

                // TODO: get builds to use the settings directly
                build.RunesUseEquipped = bSettings.RunesUseEquipped;
                build.RunesUseLocked = bSettings.RunesUseLocked;
                build.BuildGenerate = bSettings.BuildGenerate;
                build.BuildTake = bSettings.BuildTake;
                build.BuildTimeout = bSettings.BuildTimeout;
                build.Shrines = bSettings.Shrines;
                build.BuildDumpBads = bSettings.BuildDumpBads;
                build.BuildSaveStats = bSettings.BuildSaveStats;
                build.BuildGoodRunes = bSettings.BuildGoodRunes;
                build.RunesOnlyFillEmpty = bSettings.RunesOnlyFillEmpty;
                build.RunesDropHalfSetStat = bSettings.RunesDropHalfSetStat;
                build.IgnoreLess5 = bSettings.IgnoreLess5;


                BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, "Runes..."));
                if (build.Type == BuildType.Link)
                {
                    build.CopyFrom(build.LinkBuild);
                }

                // unlock runes on current loadout (if present)
                var load = loads.FirstOrDefault(l => l.BuildID == build.ID);
                if (load != null)
                    load.Unlock();

                build.GenRunes(data);

                #region Check enough runes
                string nR = "";
                for (int i = 0; i < build.runes.Length; i++)
                {
                    if (build.runes[i] != null && build.runes[i].Length == 0)
                        nR += (i + 1) + " ";
                }

                if (nR != "")
                {
                    BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, ":( " + nR + "Runes"));
                    return;
                }
                #endregion

                build.BuildPrintTo += BuildsPrintTo;
                build.BuildProgTo += BuildsProgressTo;

                EventHandler<ProgToEventArgs> qw = (bq, s) => {
                    if (runToken.IsCancellationRequested)
                        build.Cancel();
                };
                build.BuildProgTo += qw;

                var result = build.GenBuilds();

                buildTime.Stop();
                build.Time = buildTime.ElapsedMilliseconds;
                LineLog.Info("Stopping watch " + build.ID + " " + build.MonName + " @ " + buildTime.ElapsedMilliseconds);

                if (build.Best != null)
                {
                    BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, "Best"));

                    build.Best.Current.BuildID = build.ID;

                    #region Get the rune diff
                    build.Best.Current.Lock();
                    build.Best.Current.RecountDiff(build.Mon.Id);
                    #endregion

                    //currentBuild = null;
                    build.Best.Current.Time = build.Time;

                    var dmon = Program.Data.GetMonster(build.Best.Id);

                    var dmonld = dmon.Current.Leader;
                    var dmonsh = dmon.Current.Shrines;
                    dmon.Current.Leader = build.Best.Current.Leader;
                    dmon.Current.Shrines = build.Best.Current.Shrines;
                    var dmonfl = dmon.Current.FakeLevel;
                    var dmonps = dmon.Current.PredictSubs;
                    dmon.Current.FakeLevel = build.Best.Current.FakeLevel;
                    dmon.Current.PredictSubs = build.Best.Current.PredictSubs;
                    var dmonbf = dmon.Current.Buffs;
                    dmon.Current.Buffs = build.Best.Current.Buffs;

                    var ds = build.CalcScore(dmon.GetStats());
                    var cs = build.CalcScore(build.Best.Current.GetStats(build.Best));
                    build.Best.Current.DeltaPoints = cs - ds;

                    dmon.Current.Leader = dmonld;
                    dmon.Current.Shrines = dmonsh;
                    dmon.Current.FakeLevel = dmonfl;
                    dmon.Current.PredictSubs = dmonps;
                    dmon.Current.Buffs = dmonbf;

                    loads.Add(build.Best.Current);

                    // if we are on the hunt of good runes.
                    if (goodRunes && bSettings.BuildSaveStats && build.Type != BuildType.Lock)
                    {
                        var theBest = build.Best;
                        int count = 0;
                        // we must progressively ban more runes from the build to find second-place runes.
                        //GenDeep(b, 0, printTo, ref count);
                        RunBanned(build, ++count, theBest.Current.Runes.Where(r => r.Slot % 2 != 0).Select(r => r.Id).ToArray());
                        RunBanned(build, ++count, theBest.Current.Runes.Where(r => r.Slot % 2 == 0).Select(r => r.Id).ToArray());
                        RunBanned(build, ++count, theBest.Current.Runes.Select(r => r.Id).ToArray());

                        // after messing all that shit up
                        build.Best = theBest;
                    }

                    #region Save Build stats

                    /* TODO: put Excel on Program */
                    if (bSettings.BuildSaveStats && build.Type != BuildType.Lock)
                    {
                        BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, "Excel"));
                        runeSheet.StatsExcelBuild(build, build.Mon, build.Best.Current, true);
                    }

                    BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, "Clean"));
                    // clean up for GC
                    if (build.BuildUsage != null)
                        build.BuildUsage.loads.Clear();
                    if (build.RuneUsage != null)
                    {
                        build.RuneUsage.runesGood.Clear();
                        build.RuneUsage.runesUsed.Clear();
                    }
                    build.RuneUsage = null;
                    build.BuildUsage = null;
                    /**/
                    #endregion
                }

                build.BuildPrintTo -= BuildsPrintTo;
                build.BuildProgTo -= qw;
                build.BuildProgTo -= BuildsProgressTo;

                //if (plsDie)
                //    printTo?.Invoke("Canned");
                //else 
                if (build.Best != null)
                    BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, "Done"));
                else
                    BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, result + " :("));

                LineLog.Info("Cleaning up");
                //b.isRun = false;
                //currentBuild = null;
            }
            catch (Exception e)
            {
                LineLog.Error("Error during build " + build.ID + " " + e.Message + Environment.NewLine + e.StackTrace);
            }
            finally
            {
                currentBuild = null;
                LineLog.Info("Cleaned");
            }
        }

        private static void RunBanned(Build b, int c, params ulong[] doneIds)
        {
            LineLog.Info("Running ban");
            try
            {
                b.BanEmTemp(doneIds);

                b.RunesUseLocked = false;
                b.RunesUseEquipped = Program.Settings.UseEquipped;
                b.BuildSaveStats = true;
                b.RunesOnlyFillEmpty = Program.fillRunes;
                b.BuildGoodRunes = goodRunes;
                b.RunesDropHalfSetStat = Program.goFast;
                b.IgnoreLess5 = Program.Settings.IgnoreLess5;
                b.GenRunes(Program.Data);

                b.BuildTimeout = 0;
                b.BuildTake = 0;
                b.BuildGenerate = 0;
                b.BuildDumpBads = true;
                var result = b.GenBuilds($"{c} ");
                b.BuildGoodRunes = false;
                LineLog.Info("ran ban with result: " + result);
            }
            catch (Exception ex)
            {
                LineLog.Error("Running ban failed ", ex);
            }
            finally
            {
                b.BanEmTemp(new ulong[] { });
                b.BuildSaveStats = false;
                b.GenRunes(Program.Data);
                LineLog.Info("Ban finished");
            }
        }

        public static void RunBuilds(bool skipLoaded, int runTo = -1)
        {
            if (Program.Data == null)
                return;

            if (isRunning)
            {
                if (runTask != null && runTask.Status != TaskStatus.Running)
                    throw new Exception("Already running builds!");
                else
                {
                    runSource.Cancel();
                    return;
                }
            }
            isRunning = true;

            try
            {
                if (runTask != null && runTask.Status == TaskStatus.Running)
                {
                    runSource.Cancel();
                    //if (currentBuild != null)
                    //   currentBuild.isRun = false;
                    //plsDie = true;
                    isRunning = false;
                    return;
                }
                //plsDie = false;

                List<int> loady = new List<int>();

                if (!skipLoaded)
                {
                    ClearLoadouts();
                    foreach (var r in Program.Data.Runes)
                    {
                        r.manageStats.AddOrUpdate("buildScoreIn", 0, (k, v) => 0);
                        r.manageStats.AddOrUpdate("buildScoreTotal", 0, (k, v) => 0);
                    }
                }

                List<Build> toRun = new List<Build>();
                foreach (var build in builds.OrderBy(b => b.Priority))
                {
                    if ((!skipLoaded || !loads.Any(l => l.BuildID == build.ID)) && (runTo == -1 || build.Priority <= runTo))
                        toRun.Add(build);
                }

                /*
                bool collect = true;
                int newPri = 1;
                // collect the builds
                List<ListViewItem> list5 = new List<ListViewItem>();
                foreach (ListViewItem li in buildList.Items)
                {
                    li.SubItems[0].Text = newPri.ToString();
                    (li.Tag as Build).priority = newPri++;

                    if (loady.Contains((li.Tag as Build).ID))
                        continue;

                    if ((li.Tag as Build).ID == runTo)
                        collect = false;

                    if (collect)
                        list5.Add(li);

                    li.SubItems[3].Text = "";
                }
                */

                runSource = new CancellationTokenSource();
                runToken = runSource.Token;
                runTask = Task.Factory.StartNew(() => {
                    if (Program.Data.Runes != null && !skipLoaded)
                    {
                        foreach (Rune r in Program.Data.Runes)
                        {
                            r.Swapped = false;
                            r.ResetStats();
                        }
                    }

                    foreach (Build bbb in toRun)
                    {
                        runBuild(bbb, Program.Settings.MakeStats);
                        if (runToken.IsCancellationRequested || bbb.Best == null)
                            break;
                    }

                    if (!runToken.IsCancellationRequested && Program.Settings.MakeStats)
                    {
                        if (!skipLoaded)
                            Program.runeSheet.StatsExcelRunes(true);
                        try
                        {
                            Program.runeSheet.StatsExcelSave(true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                    isRunning = false;
                }, runSource.Token);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + Environment.NewLine + e.StackTrace, e.GetType().ToString());
            }
        }

    }
}
