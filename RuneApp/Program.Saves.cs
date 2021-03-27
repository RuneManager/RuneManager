using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RuneOptim.BuildProcessing;
using RuneOptim.Management;
using RuneOptim.swar;

namespace RuneApp
{
    public static partial class Program
    {

        public static LoadSaveResult LoadBuilds(string filename = "builds.json")
        {
            if (!File.Exists(filename))
            {
                LineLog.Error($"{filename} wasn't found.");
                return LoadSaveResult.FileNotFound;
            }
            LineLog.Debug($"Loading {filename} as builds.");

#if !DEBUG
            try {
#endif
            var bstr = File.ReadAllText(filename);

            // upgrade:
            bstr = bstr.Replace("\"b_hp\"", "\"hp\"");
            bstr = bstr.Replace("\"b_atk\"", "\"atk\"");
            bstr = bstr.Replace("\"b_def\"", "\"def\"");
            bstr = bstr.Replace("\"b_spd\"", "\"spd\"");
            bstr = bstr.Replace("\"b_crate\"", "\"critical_rate\"");
            bstr = bstr.Replace("\"b_cdmg\"", "\"critical_damage\"");
            bstr = bstr.Replace("\"b_acc\"", "\"accuracy\"");
            bstr = bstr.Replace("\"b_res\"", "\"res\"");

            var bs = JsonConvert.DeserializeObject<List<Build>>(bstr);
            foreach (var b in bs.OrderBy(b => b.Priority))
            {
                builds.Add(b);
            }
            foreach (var b in builds.Where(b => b.Type == BuildType.Link))
            {
                b.LinkBuild = Program.builds.FirstOrDefault(bu => bu.ID == b.LinkId);
            }
#if !DEBUG
            }
            catch (Exception e) {
                File.WriteAllText("error_build.txt", e.ToString());
                throw new InvalidOperationException("Error occurred loading Build JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_build.txt");
            }
#endif

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
            LineLog.Debug("processing builds");
            int current_pri = 1;
            foreach (Build b in Program.builds.OrderBy(bu => bu.Priority))
            {
                int id = b.ID;
                if (b.ID == 0 || Program.builds.Where(bu => bu != b).Select(bu => bu.ID).Any(bid => bid == b.ID))
                {
                    //id = buildList.Items.Count + 1;
                    id = 1;
                    while (Program.builds.Any(bu => bu.ID == id))
                        id++;

                    foreach (var lb in Program.builds.Where(bu => bu.LinkId == b.ID))
                    {
                        lb.LinkId = id;
                    }

                    b.ID = id;
                }
                if (b.Type == BuildType.Lock)
                    b.Priority = 0;
                else
                    b.Priority = current_pri++;

                // make sure bad things are removed
                foreach (var ftab in b.RuneFilters)
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
                        case 1:
                            foreach (var tabN in b.RuneScoring.Keys.ToArray())
                            {
                                var tab = b.RuneScoring[tabN];
                                if (tab.Type == FilterType.SumN)
                                {
                                    tab.Count = (int?)(tab.Value);
                                    tab.Value = null;
                                    b.RuneScoring[tabN] = tab;
                                }
                            }
                            break;
                        case 2:
                            b.AutoRuneAmount = Build.AutoRuneAmountDefault;
                            break;
                    }
                    b.VERSIONNUM++;
                }
            }
        }

        public static LoadSaveResult SaveBuilds(string filename = "builds.json")
        {
            LineLog.Debug($"Saving builds to {filename}");
            // TODO: fix this mess
            foreach (Build bb in builds)
            {
                if (bb.Mon != null && bb.Mon.FullName != "Missingno")
                {
                    if (!bb.DownloadAwake || (Program.data.GetMonster(bb.Mon.FullName) != null && Program.data.GetMonster(bb.Mon.FullName).FullName != "Missingno"))
                    {
                        bb.MonName = bb.Mon.FullName;
                        bb.MonId = bb.Mon.Id;
                    }
                    else
                    {
                        if (Program.data.GetMonster(bb.Mon.Id).FullName != "Missingno")
                        {
                            bb.MonId = bb.Mon.Id;
                            bb.MonName = Program.data.GetMonster(bb.Mon.Id).FullName;
                        }
                    }
                }
            }

            // only write if there are builds, may save some files
            if (Program.builds.Count > 0)
            {
                try
                {
                    // keep a single recent backup
                    if (File.Exists(filename))
                        File.Copy(filename, filename + ".backup", true);
                    var str = JsonConvert.SerializeObject(Program.builds, Formatting.Indented);
                    File.WriteAllText(filename, str);
                    return LoadSaveResult.Success;
                }
                catch (Exception e)
                {
                    LineLog.Error($"Error while saving builds {e.GetType()}", e);
                    throw;
                    //MessageBox.Show(e.ToString());
                }
            }
            return LoadSaveResult.Failure;
        }

        public static LoadSaveResult SaveLoadouts(string filename = "loads.json")
        {
            LineLog.Debug($"Saving loads to {filename}");

            if (loads.Count > 0)
            {
                try
                {
                    // keep a single recent backup
                    if (File.Exists(filename))
                        File.Copy(filename, filename + ".backup", true);
                    var str = JsonConvert.SerializeObject(loads, Formatting.Indented);
                    File.WriteAllText(filename, str);
                    return LoadSaveResult.Success;
                }
                catch (Exception e)
                {
                    LineLog.Error($"Error while saving loads {e.GetType()}", e);
                    throw;
                }
            }
            return LoadSaveResult.EmptyFile;
        }

        public static LoadSaveResult LoadLoadouts(string filename = "loads.json")
        {
            try
            {
                string text = File.ReadAllText(filename);
                var lloads = JsonConvert.DeserializeObject<Loadout[]>(text);
                loads.Clear();

                foreach (var load in lloads)
                {
                    if (load.RuneIDs != null)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            var ids = load.RuneIDs[i];
                            load.Runes[i] = Program.data.Runes.FirstOrDefault(r => r.Id == ids);
                            if (load.Runes[i] != null)
                            {
                                load.Runes[i].Locked = true;
                                if (load.manageStats != null)
                                    foreach (var ms in load.manageStats[i])
                                        load.Runes[i].manageStats.AddOrUpdate(ms.Key, ms.Value, (s, d) => ms.Value);
                            }
                        }
                    }
                    load.Shrines = data.shrines;
                    loads.Add(load);
                }
                return LoadSaveResult.Success;
            }
            catch (Exception e)
            {
                LineLog.Error($"Error while loading loads {e.GetType()}", e);
                //MessageBox.Show("Error occurred loading Save JSON.\r\n" + e.GetType() + "\r\nInformation is saved to error_save.txt");
                File.WriteAllText("error_loads.txt", e.ToString());
                throw;
            }
        }

        internal static void ClearLoadouts()
        {
            foreach (Loadout l in loads)
            {
                Build build = Program.builds.FirstOrDefault(b => b.ID == l.BuildID);
                BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, "!"));
                l.Unlock();
            }
            loads.Clear();
        }

        public static void RemoveLoad(Loadout l)
        {
            Build build = Program.builds.FirstOrDefault(b => b.ID == l.BuildID);
            BuildsPrintTo?.Invoke(null, PrintToEventArgs.GetEvent(build, "!"));
            l.Unlock();
            loads.Remove(l);
        }

        public static LoadSaveResult SaveGoals(string filename = "goals.json")
        {
            LineLog.Debug($"Saving loads to {filename}");

            try
            {
                // keep a single recent backup
                var str = JsonConvert.SerializeObject(goals, Formatting.Indented);
                File.WriteAllText(filename, str);
                return LoadSaveResult.Success;
            }
            catch (Exception e)
            {
                LineLog.Error($"Error while saving loads {e.GetType()}", e);
                throw;
            }
        }

        public static LoadSaveResult LoadGoals(string filename = "goals.json")
        {
            try
            {
                LineLog.Debug("Loading goals");
                if (File.Exists(filename))
                {
                    string text = File.ReadAllText(filename);
                    goals = JsonConvert.DeserializeObject<Goals>(text);
                }
                else
                {
                    goals = new Goals();
                }
                return LoadSaveResult.Success;
            }
            catch (Exception e)
            {
                LineLog.Error($"Error while loading goals {e.GetType()}", e);
                File.WriteAllText("error_goals.txt", e.ToString());
                throw;
            }
        }

        public static void SaveData()
        {
            // tag the save as a modified save
            Program.data.isModified = true;
            var l = Program.data.Runes.Where(r => r.Locked);
            foreach (var r in l)
                r.Locked = false;

            if (File.Exists(Program.Settings.SaveLocation))
            {
                // backup, just in case
                string fname = Path.ChangeExtension(Program.Settings.SaveLocation, ".backup.json");
                int i = 2;
                while (File.Exists(fname))
                    fname = Path.ChangeExtension(Program.Settings.SaveLocation, ".backup" + (i++) + ".json");
                File.Copy(Program.Settings.SaveLocation, fname);
            }

            File.WriteAllText(Program.Settings.SaveLocation, JsonConvert.SerializeObject(Program.data, Formatting.Indented));

            foreach (var r in l)
                r.Locked = true;
        }

    }
}
