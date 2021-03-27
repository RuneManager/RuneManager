using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RuneApp;
using RuneOptim.swar;

namespace RuneAppTests
{
    [TestClass]
    public class GithubIssues
    {
        Main main;

        [TestInitialize]
        public void Setup()
        {
            Program.Main(new[] { "-H" } );
            main = new Main();
        }

        [TestCleanup]
        public void Teardown()
        {
            main.Close();
            main.Dispose();
        }


        [TestMethod]
        public async Task _99_RefreshBreaksLockedLoadouts()
        {

            // load the save?
            if (File.Exists(Program.Settings.SaveLocation))
            {
                Program.LoadSave(Program.Settings.SaveLocation);
                main.RebuildLists();
                main.refreshLoadouts();
            }
            else
                Assert.Fail("No save");

            Assert.IsTrue(Program.builds.Count > 0, "No builds");

            var b = Program.builds.FirstOrDefault();
            Program.RunBuild(b);
            await Program.RunTask;

            Assert.IsTrue(Program.loads.Any(), "No loadout was created.");

            // refresh button implementation:
            if (File.Exists(Program.Settings.SaveLocation))
            {
                Program.LoadSave(Program.Settings.SaveLocation);
                main.RebuildLists();
                main.refreshLoadouts();
            }

            


        }
    }
}
