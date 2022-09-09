using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RuneApp;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneAppTests
{
    [TestClass]
    public class GithubIssues
    {
        Main main;

        [TestInitialize]
        public async Task Setup()
        {
            var t = new Thread(() =>
            {
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                {
                    throw new ThreadStateException("The current threads apartment state is not STA");
                }

                Program.Main(new string[] { "-W", "-S" });
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            await Program.Ready;
            main = Application.OpenForms.OfType<Main>().FirstOrDefault();
        }

        [TestCleanup]
        public async Task Teardown()
        {
            await Program.Ready;
            main.Invoke((MethodInvoker)delegate
            {
                //Program.Close();
            });
        }


        [TestMethod]
        public async Task Issue_99_RefreshBreaksLockedLoadouts()
        {

            // load the save?
            if (File.Exists(Program.Settings.SaveLocation))
            {
                Program.LoadExportedRunesJSON(Program.Settings.SaveLocation);
                main.RebuildLists();
                main.RefreshLoadouts();
            }
            else
                Assert.Fail("No save");

            Assert.IsTrue(Program.Builds.Count > 0, "No builds");

            var b = Program.Builds.FirstOrDefault();
            Program.RunBuild(b);
            await Program.RunTask;

            Assert.IsTrue(Program.Loads.Any(), "No loadout was created.");

            // refresh button implementation:
            if (File.Exists(Program.Settings.SaveLocation))
            {
                Program.LoadExportedRunesJSON(Program.Settings.SaveLocation);
                main.RebuildLists();
                main.RefreshLoadouts();
            }

            


        }

        [TestMethod]
        public void Issue_45_BuildNamesDontRefresh()
        {
            // get a non-homu build, and check that the LVI has the correct name
            var build = Program.Builds.FirstOrDefault(b => !b.Mon.IsHomunculus);
            ListViewItem lvi = main.Invoke(() => main.BuildListViewItems.FirstOrDefault(l => l.Tag == build));
            Assert.AreEqual(build.Mon.FullName, lvi.Text);

            // "update" the save with new info (awakened name)
            var ddat = new Save(Program.Data);
            var tempMonster = ddat.GetMonster(build.MonId);
            ddat.Monsters.Clear();
            // this property drives the listView text
            tempMonster.FullName = "test_monster";
            ddat.Monsters.Add(tempMonster);

            // pretend to click the "refresh" button
            Program.Data = Program.LoadSaveData(JsonConvert.SerializeObject(ddat), Program.Loads);
            main.RebuildLists();
            main.RefreshLoadouts();

            // the monster has a new ref, but the lvi is same.
            var updatedMonster = Program.Data.GetMonster(build.MonId);
            Assert.AreEqual(updatedMonster.FullName, lvi.Text);
        }
    }
}
