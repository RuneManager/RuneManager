﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using RuneOptim;

namespace RuneApp
{
    public enum LoadSaveResult
    {
        Failure = -2,
        FileNotFound = -1,
        EmptyFile = 0,
        Success = 1,
    }

    public static class Program
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static Configuration config = null;

        public static Save data;
        public static ObservableCollection<Build> builds = new ObservableCollection<Build>();
        public static ObservableCollection<Loadout> loads = new ObservableCollection<Loadout>();

        public static bool useEquipped = false;
        private static bool isRunning;

        public static RuneSheet runeSheet = new RuneSheet();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            builds.CollectionChanged += Builds_CollectionChanged;
            loads.CollectionChanged += Loads_CollectionChanged;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        private static void Loads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private static void Builds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var b in e.NewItems.Cast<Build>())
                    {
                        if (Program.data != null)
                        {
                            // for each build, find the build in the buildlist with the same mon name?
                            //var bnum = buildList.Items.Cast<ListViewItem>().Select(it => it.Tag as Build).Where(d => d.MonName == b.MonName).Count();
                            // if there is a build with this monname, maybe I have 2 mons with that name?!
                            var bnum = builds.Where(bu => bu.MonName == b.MonName).Count();
                            b.mon = Program.data.GetMonster(b.MonName, bnum + 1);
                        }
                        else
                        {
                            b.mon = new Monster();
                            b.mon.Name = b.MonName;
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        public static LoadSaveResult LoadMonStats()
        {
            if (File.Exists("basestats.json"))
                MonsterStat.monStats = JsonConvert.DeserializeObject<List<MonsterStat>>(File.ReadAllText("basestats.json"));
            else
                return LoadSaveResult.FileNotFound;
            return LoadSaveResult.Success;
        }

        /// <summary>
        /// Checks the Working directory for a supported save file
        /// </summary>
        public static LoadSaveResult FindSave()
        {
            var files = Directory.GetFiles(Environment.CurrentDirectory, "*-swarfarm.json");

            if (files.Count() > 0)
            {
                var firstFile = files.First();
                return LoadSave(firstFile, "swarfarm");
            }
            else if (File.Exists("save.json"))
            {
                return LoadSave("save.json");
            }
            return LoadSaveResult.FileNotFound;
        }

        public static LoadSaveResult LoadSave(string filename, string format = "optimizer")
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                log.Error("Filename for save is null");
                return LoadSaveResult.FileNotFound;
            }
            if (!File.Exists(filename))
            {
                log.Error($"File {filename} doesn't exist");
                return LoadSaveResult.FileNotFound;
            }
            log.Info("Loading " + filename + " as save.");
            string text = File.ReadAllText(filename);

            try
            {
                // TODO: pick a format maybe
                switch (format)
                {
                    case "optimizer":
                        Program.data = JsonConvert.DeserializeObject<Save>(text);
                        break;
                    case "swarfarm":
                        Program.data = new Save();
                        var qq = JsonConvert.DeserializeObject<SWarFarmSave>(text);

                        foreach (var r in qq.Runes)
                        {
                            var rr = new Rune()
                            {
                                ID = (int)r.Id,
                                MainType = r.Main.Type,
                                MainValue = r.Main.Value,
                                InnateType = r.Innate.Type,
                                InnateValue = r.Innate.Value,
                                Slot = r.Slot,
                                Set = (RuneSet)r._set,
                                Level = r.Level,
                                Grade = r.Grade,
                            };

                            if (r.AssignedTo != null)
                                rr.AssignedId = (int)r.AssignedTo.Id;
                            else
                                rr.AssignedName = "Inventory";

                            if (r.Subs.Count > 0)
                            {
                                rr.Sub1Type = r.Subs[0].Type;
                                rr.Sub1Value = r.Subs[0].Value;
                            }
                            if (r.Subs.Count > 1)
                            {
                                rr.Sub2Type = r.Subs[1].Type;
                                rr.Sub2Value = r.Subs[1].Value;
                            }
                            if (r.Subs.Count > 2)
                            {
                                rr.Sub3Type = r.Subs[2].Type;
                                rr.Sub3Value = r.Subs[2].Value;
                            }
                            if (r.Subs.Count > 3)
                            {
                                rr.Sub4Type = r.Subs[3].Type;
                                rr.Sub4Value = r.Subs[3].Value;
                            }

                            Program.data.Runes.Add(rr);
                        }

                        foreach (var m in qq.Monsters.Select(m => new Monster()
                        {
                            ID = (int)m.Id,
                            level = m.Level,
                            Health = m.Health,
                            Attack = m.Attack,
                            Defense = m.Defense,
                            Speed = m.Speed,
                            CritRate = m.CritRate,
                            CritDamage = m.CritDamage,
                            Resistance = m.Resistance,
                            Accuracy = m.Accuracy,
                            Name = m.Name,
                        }))
                        {
                            Program.data.Monsters.Add(m);
                        }
                        foreach (var dec in qq.Decorations)
                            Program.data.Decorations.Add(dec);
                        break;
                }
                
                // TODO: trash
                for (int i = 0; i < Deco.ShrineStats.Length; i++)
                {
                    var stat = Deco.ShrineStats[i];

                    if (config != null && config.AppSettings.Settings.AllKeys.Contains("shrine" + stat))
                    {
                        int val = 0;
                        int.TryParse(config.AppSettings.Settings["shrine" + stat].Value, out val);
                        Program.data.shrines[stat] = val;
                        int level = (int)Math.Floor(val / Deco.ShrineLevel[i]);
                    }
                }
            }
            catch (Exception e)
            {
                File.WriteAllText("error_save.txt", e.ToString());
                throw new Exception("Error occurred loading Save JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_save.txt");
            }
            return LoadSaveResult.Success;//text.Length;
        }

        public static LoadSaveResult LoadBuilds(string filename = "builds.json")
        {
            if (!File.Exists(filename))
            {
                log.Error($"{filename} wasn't found.");
                return LoadSaveResult.FileNotFound;
            }
            log.Info($"Loading {filename} as builds.");

            try
            {
                var bs = JsonConvert.DeserializeObject<List<Build>>(File.ReadAllText(filename));
                foreach (var b in bs)
                {
                    builds.Add(b);
                }
            }
            catch (Exception e)
            {
                File.WriteAllText("error_build.txt", e.ToString());
                throw new Exception("Error occurred loading Build JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_build.txt");
            }
        
            if (Program.builds.Count > 0 && (Program.data?.Monsters == null))
            {
                // backup, just in case
                string destFile = Path.Combine("", string.Format("{0}.backup{1}", "builds", ".json"));
                int num = 2;
                while (File.Exists(destFile))
                {
                    destFile = Path.Combine("", string.Format("{0}.backup{1}{2}", "builds", num, ".json"));
                    num++;
                }

                File.Copy("builds.json", destFile);
                return LoadSaveResult.Failure;
            }

            SanitizeBuilds();

            return LoadSaveResult.Success;
        }
        
        public static void SanitizeBuilds()
        {
            int current_pri = 1;
            foreach (Build b in Program.builds.OrderBy(bu => bu.priority))
            {
                int id = b.ID;
                if (b.ID == 0 || Program.builds.Where(bu => bu != b).Select(bu => bu.ID).Any(bid => bid == b.ID))
                {
                    //id = buildList.Items.Count + 1;
                    id = 1;
                    while (Program.builds.Any(bu => bu.ID == id))
                        id++;
                    b.ID = id;
                }
                b.priority = current_pri++;

                // make sure bad things are removed
                foreach (var ftab in b.runeFilters)
                {
                    foreach (var filter in ftab.Value)
                    {
                        if (filter.Key == "SPD")
                            filter.Value.Percent = null;
                        if (filter.Key == "ACC" || filter.Key == "RES" || filter.Key == "CR" || filter.Key == "CD")
                            filter.Value.Flat = null;
                    }
                }

                // upgrade builds, hopefully
                while (b.VERSIONNUM < Create.VERSIONNUM)
                {
                    switch (b.VERSIONNUM)
                    {
                        case 0: // unversioned to 1
                            b.Threshold = b.Maximum;
                            b.Maximum = new Stats();
                            break;

                    }
                    b.VERSIONNUM++;
                }
            }
        }

        public static LoadSaveResult SaveBuilds(string filename = "builds.json")
        {
            log.Info($"Saving builds to {filename}");
            // TODO: fix this mess
            //Program.builds.Clear();

            //var lbs = buildList.Items;

            foreach (Build bb in builds)
            {
                if (bb.mon.Name != "Missingno")
                {
                    if (!bb.DownloadAwake || Program.data.GetMonster(bb.mon.Name).Name != "Missingno")
                        bb.MonName = bb.mon.Name;
                    else
                    {
                        if (Program.data.GetMonster(bb.mon.ID).Name != "Missingno")
                            bb.MonName = Program.data.GetMonster(bb.mon.ID).Name;
                    }
                }
                //Program.builds.Add(bb);
            }

            // only write if there are builds, may save some files
            if (Program.builds.Count > 0)
            {
                try
                {
                    // keep a single recent backup
                    if (File.Exists(filename))
                        File.Copy(filename, filename + ".backup", true);
                    var str = JsonConvert.SerializeObject(Program.builds);
                    File.WriteAllText(filename, str);
                    return LoadSaveResult.Success;
                }
                catch (Exception e)
                {
                    log.Error($"Error while saving builds {e.GetType()}", e);
                    throw;
                    //MessageBox.Show(e.ToString());
                }
            }
            return LoadSaveResult.Failure;
        }

        public static void RunBuild(Build b, bool saveStats = false, Action<string> printTo = null)
        {
            try
            {
                if (b == null)
                {
                    log.Info("Build is null");
                    return;
                }
                if (isRunning)
                    throw new Exception("Already running a build");
                isRunning = true;
                /*
                if (plsDie)
                {
                    log.Info("Cancelling build " + b.ID + " " + b.MonName);
                    //plsDie = false;
                    return;
                }*/
                /*
                if (currentBuild != null)
                {
                    log.Info("Force stopping " + currentBuild.ID + " " + currentBuild.MonName);
                    currentBuild.isRun = false;
                }

                if (isRunning)
                    log.Info("Looping...");

                while (isRunning)
                {
                    plsDie = true;
                    b.isRun = false;
                    Thread.Sleep(100);
                }
                */

                log.Info("Starting watch " + b.ID + " " + b.MonName);

                Stopwatch buildTime = Stopwatch.StartNew();
                //currentBuild = b;

                // TODO: what is this? Maybe it unlocks runes used if this build was already run?
                //ListViewItem[] olvs = null;
                //Invoke((MethodInvoker)delegate { olvs = loadoutList.Items.Find(b.ID.ToString(), false); });
                /*
                if (olvs.Length > 0)
                {
                    var olv = olvs.First();
                    Loadout ob = (Loadout)olv.Tag;
                    foreach (Rune r in ob.Runes)
                    {
                        r.Locked = false;
                    }
                }*/

                b.GenRunes(Program.data, false, Program.useEquipped, saveStats);
                b.shrines = Program.data.shrines;

                #region Check enough runes
                string nR = "";
                for (int i = 0; i < b.runes.Length; i++)
                {
                    if (b.runes[i] != null && b.runes[i].Length == 0)
                        nR += (i + 1) + " ";
                }

                if (nR != "")
                {
                    printTo?.Invoke(":( " + nR + "Runes");
                    return;
                }
                #endregion

                //isRunning = true;

                b.GenBuilds(0, 0, printTo, null, true, saveStats);
                
                b.Best.Current.BuildID = b.ID;

                #region Get the rune diff
                b.Best.Current.powerup =
                b.Best.Current.upgrades =
                b.Best.Current.runesNew =
                b.Best.Current.runesChanged = 0;
                
                foreach (Rune r in b.Best.Current.Runes)
                {
                    r.Locked = true;
                    if (r.AssignedName != b.Best.Name)
                    {
                        if (r.IsUnassigned)
                            b.Best.Current.runesNew++;
                        else
                            b.Best.Current.runesChanged++;
                    }
                    b.Best.Current.powerup += Math.Max(0, (b.Best.Current.FakeLevel[r.Slot - 1]) - r.Level);
                    if (b.Best.Current.FakeLevel[r.Slot - 1] != 0)
                    {
                        int tup = (int)Math.Floor(Math.Min(12, (b.Best.Current.FakeLevel[r.Slot - 1])) / (double)3);
                        int cup = (int)Math.Floor(Math.Min(12, r.Level) / (double)3);
                        b.Best.Current.upgrades += Math.Max(0, tup - cup);
                    }
                }
                #endregion

                //currentBuild = null;
                buildTime.Stop();
                b.Time = buildTime.ElapsedMilliseconds;
                log.Info("Stopping watch " + b.ID + " " + b.MonName + " @ " + buildTime.ElapsedMilliseconds);
                b.Best.Current.Time = b.Time;
                /*
                // if we are on the hunt of good runes.
                if (goodRunes && saveStats)
                {
                    var theBest = b.Best;
                    int count = 0;
                    // we must progressively ban more runes from the build to find second-place runes.
                    //GenDeep(b, 0, printTo, ref count);
                    RunBanned(b, printTo, ++count, theBest.Current.Runes.Where(r => r.Slot % 2 != 0).Select(r => r.ID).ToArray());
                    RunBanned(b, printTo, ++count, theBest.Current.Runes.Where(r => r.Slot % 2 == 0).Select(r => r.ID).ToArray());
                    RunBanned(b, printTo, ++count, theBest.Current.Runes.Select(r => r.ID).ToArray());

                    // after messing all that shit up
                    b.Best = theBest;
                }
                */

                #region Save Build stats
                /* TODO: put Excel on Program
                if (saveStats)
                {
                    if (StatsExcelBind(true))
                    {
                        StatsExcelBuild(b, b.mon, b.Best.Current);
                        StatsExcelSave();
                    }
                    StatsExcelBind(true);
                }

                // clean up for GC
                if (b.buildUsage != null)
                    b.buildUsage.loads.Clear();
                if (b.runeUsage != null)
                {
                    b.runeUsage.runesGood.Clear();
                    b.runeUsage.runesUsed.Clear();
                }
                b.runeUsage = null;
                b.buildUsage = null;
                */
                #endregion

                finishBuild:
                //if (plsDie)
                //    printTo?.Invoke("Canned");
                //else 
                if (b.Best != null)
                    printTo?.Invoke("Done");
                else
                    printTo?.Invoke("Zero :(");

                log.Info("Cleaning up");
                isRunning = false;
                b.isRun = false;
                //currentBuild = null;
                log.Info("Cleaned");
            }
            catch (Exception e)
            {
                log.Error("Error during build " + b.ID + " " + e.Message + Environment.NewLine + e.StackTrace);
            }
        }

        public static LoadSaveResult SaveLoadouts(string filename = "loads.json")
        {
            log.Info($"Saving loads to {filename}");
            
            if (loads.Count > 0)
            {
                try
                {
                    // keep a single recent backup
                    if (File.Exists(filename))
                        File.Copy(filename, filename + ".backup", true);
                    var str = JsonConvert.SerializeObject(loads);
                    File.WriteAllText(filename, str);
                    return LoadSaveResult.Success;
                }
                catch (Exception e)
                {
                    log.Error($"Error while saving loads {e.GetType()}", e);
                    throw;
                    //MessageBox.Show(e.ToString());
                }
                return LoadSaveResult.Failure;
            }
            return LoadSaveResult.EmptyFile;
        }

        public static LoadSaveResult LoadLoadouts(string filename = "loads.json")
        {
            try
            {
                string text = File.ReadAllText(filename);
                var lloads = JsonConvert.DeserializeObject<Loadout[]>(text);

                foreach (var load in lloads)
                {
                    loads.Add(load);
                }
                return LoadSaveResult.Success;
            }
            catch (Exception e)
            {
                log.Error($"Error while loading loads {e.GetType()}", e);
                //MessageBox.Show("Error occurred loading Save JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_save.txt");
                File.WriteAllText("error_loads.txt", e.ToString());
                throw;
            }
            return LoadSaveResult.Failure;
        }

        #region Extension Methods
        public static double StandardDeviation<T>(this IEnumerable<T> src, Func<T, double> selector)
        {
            double av = src.Where(p => Math.Abs(selector(p)) > 0.00000001).Average(selector);
            List<double> nls = new List<double>();
            foreach (var o in src.Where(p => Math.Abs(selector(p)) > 0.00000001))
            {
                nls.Add((selector(o) - av) * (selector(o) - av));
            }
            double avs = nls.Average();
            return Math.Sqrt(avs);
        }

        public static T MakeControl<T>(this Control.ControlCollection ctrlC, string name, string suff, int x, int y, int w = 40, int h = 20, string text = null)
            where T : Control, new()
        {
            T ctrl = new T()
            {
                Name = name + suff,
                Size = new Size(w, h),
                Location = new Point(x, y),
                Text = text
            };
            ctrlC.Add(ctrl);

            return ctrl;
        }
        #endregion

    }
}
