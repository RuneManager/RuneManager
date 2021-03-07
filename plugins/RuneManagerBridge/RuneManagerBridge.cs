using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RunePlugin;
using RunePlugin.Response;
using RunePlugin.Request;

namespace RuneManagerBridge
{
    public class RuneManagerBridge : SWPlugin
    {
        RuneManagerApi api;
        bool isConnected;

        public override void OnLoad() {
            Dictionary<string, string> settings = new Dictionary<string, string>() { { "baseUri", "http://localhost:7676" } };
            if (File.Exists(PluginDataDirectory + "\\settings.json")) {
                settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PluginDataDirectory + "\\settings.json"));
            }
            else
                File.WriteAllText(PluginDataDirectory + "\\settings.json", JsonConvert.SerializeObject(settings));

            Console.WriteLine("RuneManager bridge connecting to " + settings["baseUri"] + "...");
            api = new RuneManagerApi(settings["baseUri"]);
            try {
                isConnected = api.TestConnection();
                if (isConnected)
                    Console.WriteLine("RuneManager bridge connected!");
                else
                    Console.WriteLine("RuneManager bridge failed.");
            }
            catch (Exception e) {
                Console.WriteLine("RuneManager bridge failed with " + e.GetType() + ": " + e.Message);
            }
        }

        public override void ProcessRequest(object sender, SWEventArgs args)
        {
            if (!isConnected)
                return;

            try {
                // TODO: onload check if RM is running and ask for defs
                switch (args.Request.Command) {

                    #region Monster Loadouting
                    case SWCommand.EquipRune:
                        Console.WriteLine(api.MonsterPost(args.ResponseAs<EquipRuneResponse>().Monster));
                        break;
                    case SWCommand.EquipRuneList:
                        Console.WriteLine(api.MonsterPost(args.ResponseAs<EquipRuneListResponse>().TargetMonster));
                        foreach (var m in args.ResponseAs<EquipRuneListResponse>().SourceMonsters)
                            Console.WriteLine(api.MonsterPost(m.Value));
                        break;
                    case SWCommand.UnequipRune:
                        Console.WriteLine(api.MonsterPost(args.ResponseAs<UnequipRuneResponse>().Monster));
                        break;
                    case SWCommand.LockUnit:
                        Console.WriteLine(api.MonsterAction(args.ResponseAs<GenericUnitResponse>().UnitId, "lock"));
                        break;
                    case SWCommand.UnlockUnit:
                        Console.WriteLine(api.MonsterAction(args.ResponseAs<GenericUnitResponse>().UnitId, "unlock"));
                        break;
                    #endregion

                    #region Monster Summon/XP
                    case SWCommand.BattleDungeonResult: {
                            var resp = args.ResponseAs<BattleDungeonResultResponse>();

                            var rune = resp.Reward?.Crate?.Rune;
                            if (rune != null) {
                                Console.WriteLine(api.RunePost(rune));
                            }
                            var mon = resp.Reward?.Crate?.Monster;
                            if (mon != null) {
                                Console.WriteLine(api.MonsterPost(mon));
                            }
                            /*var craft = resp.Reward?.Crate?.Craft;
                            if (craft != null) {
                                Console.WriteLine(api.CraftPost(craft));
                            }*/

                            foreach (var m in resp.Monsters)
                                Console.WriteLine(api.MonsterPost(m));
                        }
                        break;
                    case SWCommand.BattleScenarioResult: {
                            var resp = args.ResponseAs<GenericBattleResponse>();

                            var rune = resp.Reward?.Crate?.Rune;
                            if (rune != null) {
                                Console.WriteLine(api.RunePost(rune));
                            }
                            var mon = resp.Reward?.Crate?.Monster;
                            if (mon != null) {
                                Console.WriteLine(api.MonsterPost(mon));
                            }
                            /*var craft = resp.Reward?.Crate?.Craft;
                            if (craft != null) {
                                Console.WriteLine(api.CraftPost(craft));
                            }*/

                            foreach (var m in resp.Monsters)
                                Console.WriteLine(api.MonsterPost(m));
                        }
                        break;
                    case SWCommand.SummonUnit:
                        foreach (var m in args.ResponseAs<SummonUnitResponse>().Monsters)
                            Console.WriteLine(api.MonsterPost(m));
                        break;
                    case SWCommand.SacrificeUnit:
                        Console.WriteLine(api.MonsterPost(args.ResponseAs<SacrificeUnitResponse>().Target));
                        foreach (var m in args.RequestAs<SourceUnitRequest>().Sources)
                            Console.WriteLine(api.MonsterDelete(m.Id));
                        break;
                    case SWCommand.UpdateUnitExpGained:
                        foreach (var m in args.ResponseAs<GenericUnitListResponse>().Monsters)
                            Console.WriteLine(api.MonsterPost(m));
                        break;
                    case SWCommand.UpgradeUnit:
                        Console.WriteLine(api.MonsterPost(args.ResponseAs<UpgradeUnitResponse>().Target));
                        foreach (var m in args.RequestAs<SourceUnitRequest>().Sources)
                            Console.WriteLine(api.MonsterDelete(m.Id));
                        break;
                    #endregion

                    case SWCommand.ConfirmRune: {
                            var req = args.RequestAs<ConfirmRuneRequest>();
                            var resp = args.ResponseAs<GenericRuneResponse>();
                            if (!req.Rollback) {
                                Console.WriteLine(api.RunePost(resp.Rune));
                            }
                        }
                        break;

                    case SWCommand.UpgradeRune: {
                            var req = args.RequestAs<UpgradeRuneRequest>();
                            var resp = args.ResponseAs<GenericRuneResponse>();
                            if (req.CurrentLevel != resp.Rune.Level) {
                                Console.WriteLine(api.RunePost(resp.Rune));
                            }
                        }
                        break;
                    /*case SWCommand.SellRuneCraftItem:
                        foreach (var r in args.ResponseAs<SellRuneCraftItemResponse>().SoldCrafts)
                            Console.WriteLine(api.CraftDelete(r.ItemId));
                        break;*/
                    case SWCommand.SellRune:
                        foreach (var r in args.ResponseAs<SellRuneResponse>().SoldRunes)
                            Console.WriteLine(api.RuneDelete(r.Id));
                        break;
                }
            }
            catch (WebException we) when (we.Status == WebExceptionStatus.ConnectFailure) {
                Console.WriteLine("RuneManager bridge connection failure.");
            }
            catch (WebException we) when (we.Status == WebExceptionStatus.ProtocolError) {
                Console.WriteLine("RuneManager bridge protocol error. " + we.Message);
            }
            catch (WebException we) {

                Console.WriteLine("RuneManager bridge WebException " + we.Message + Environment.NewLine + we.Status);
                Console.WriteLine(we.TargetSite + ": " + we.Source + Environment.NewLine + we.StackTrace);
                this.isConnected = false;
            }
            catch (Exception e) {
                Console.WriteLine("RuneManager bridge failed with " + e.GetType() + ": " + e.Message);
            }
        }
    }
}
