using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RuneOptim;

namespace RuneApp.InternalServer
{
	public partial class Master : PageRenderer
	{
		[PageAddressRender("loads")]
		public class LoadsRenderer : PageRenderer
		{
			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				var resp = this.Recurse(req, uri);
				if (resp != null)
					return resp;

				/* TODO:
				Put completed mons in storage
				Put runes on mon
					Remove rune from Source Mon
						Take mon out of storage
					Remove rune from Target Mon
						Take mon out of storage

				Group by thingo
				*/

				List<RuneAction> runeIns = new List<RuneAction>();
				List<RuneAction> runeOuts = new List<RuneAction>();
				List<RuneAction> monIns = new List<RuneAction>();
				List<RuneAction> monOuts = new List<RuneAction>();
				var storageId = Program.data.Buildings.FirstOrDefault(b => b.BuildingType == BuildingType.MonsterStorage).Id;

				// For each loadout
				foreach (var l in Program.loads)
				{
					var b = Program.builds.FirstOrDefault(bb => bb.ID == l.BuildID);

					// prep a pull
					RuneAction ma = null;
					if (b.mon.BuildingId == storageId)
						ma = new RuneAction(ActionType.MonOut) { mon = b.mon };

					bool shuffleRune = false;

					foreach (var r in l.Runes)
					{
						// if the rune is not currently on the monster
						if (r.AssignedId != b.MonId)
						{
							// prepare to put it on it
							var ra = new RuneAction(ActionType.RuneIn) { rune = r, mon = b?.mon };
							if (ma != null)
								ra.prereq.Add(ma);
							runeIns.Add(ra);
							shuffleRune = true;
						}
					}

					// enqueue a pull
					if (shuffleRune)
						monOuts.Add(ma);
				}

				// for each rune going onto a monster
				foreach (var r in runeIns)
				{
					if (r.mon != null)
					{
						var or = r.mon.Current.Runes.FirstOrDefault(rr => rr.Slot == r.rune.Slot);
						// if there is another rune in the same slot
						if (or != null)
						{
							var ra = runeOuts.FirstOrDefault(a => a.rune == or) ?? new RuneAction(ActionType.RuneOut) { mon = r.mon, rune = or };
							r.prereq.Add(ra);
							if (!runeOuts.Contains(ra))
								runeOuts.Add(ra);
						}
					}
					// if that rune is on another monster
					if (r.rune.Assigned != null)
					{
						var ra = runeOuts.FirstOrDefault(a => a.rune == r.rune) ?? new RuneAction(ActionType.RuneOut) { mon = r.rune.Assigned, rune = r.rune };
						r.prereq.Add(ra);
						if (!runeOuts.Contains(ra))
							runeOuts.Add(ra);
					}

					if (r.mon.BuildingId == storageId)
					{
						var ma = monOuts.FirstOrDefault(a => a.mon == r.mon) ?? new RuneAction(ActionType.MonOut) { mon = r.mon };
						r.prereq.Add(ma);
						if (!monOuts.Contains(ma))
							monOuts.Add(ma);
					}
				}

				// possible runeOut dupes
				var rcop = runeOuts;
				runeOuts = new List<RuneApp.InternalServer.Master.LoadsRenderer.RuneAction>();
				foreach (var r in rcop)
				{
					var fra = runeOuts.FirstOrDefault(ra => ra.mon == r.mon && ra.rune == r.rune);
					if (fra == null)
						runeOuts.Add(r);
					else
						fra.Merge(r);
				}

				foreach (var r in runeOuts)
				{
					if (r.mon.BuildingId == storageId)
					{
						var ma = monOuts.FirstOrDefault(ra => ra.mon == r.mon) ?? new RuneAction(ActionType.MonOut) { mon = r.mon };
						r.prereq.Add(ma);
						if (!monOuts.Contains(ma))
							monOuts.Add(ma);
					}
				}

				// stash wandering monsters with no build/loadout or a unchanged
				foreach (var m in Program.data.Monsters.Where(mo => mo.BuildingId == 0))
				{
					var ma = monIns.FirstOrDefault(a => a.mon == m) ?? new RuneAction(ActionType.MonIn) { mon = m };
					var b = Program.builds.FirstOrDefault(bu => bu.mon == m);
					if (b == null)
					{
						monIns.Add(ma);
					}
					else
					{
						var l = Program.loads.FirstOrDefault(lo => lo.BuildID == b.ID);
						if (l == null)
						{
							monIns.Add(ma);
						}
						else
						{
							if (l.Runes.All(r => r.Assigned == m))
								monIns.Add(ma);
						}
					}
				}

				int boxSize = Program.data.WizardInfo.UnitSlots.Number;
				int storeSize = Program.data.WizardInfo.UnitDepositorySlots.Number;
				
				var boxMons = Program.data.Monsters.Where(m => m.BuildingId != storageId).ToList();
				var storeMons = Program.data.Monsters.Where(m => m.BuildingId == storageId).ToList();

				List<RuneAction> actList = new List<RuneAction>();

				// store things
				while (storeMons.Count < storeSize && monIns.Count > 0)
				{
					var mm = monIns.ElementAt(0);
					actList.Add(mm);
					monIns.Remove(mm);
					var m = boxMons.FirstOrDefault(mo => mo.Id == mm.mon.Id);
					storeMons.Add(m);
					boxMons.Remove(m);
				}
				/*
				while (boxMons.Count < boxSize && monOuts.Count > 0)
				{
					var mm = monOuts.ElementAt(0);
					actList.Add(mm);
					monOuts.Remove(mm);
					var m = storeMons.FirstOrDefault(mo => mo.Id == mm.mon.Id);
					boxMons.Add(m);
					storeMons.Remove(m);
				}
				*/

				runeIns = runeIns.OrderBy(a => a.mon == null ? int.MaxValue : Program.builds.FirstOrDefault(b => b.MonId == a.mon.Id)?.priority ?? int.MaxValue).ToList();

				// TODO: go up the dependency chain, do the things until can't be done

				foreach (var ra in runeIns)
				{
					// TODO: stash 'temp actList' so that we can transact and ensure that we finish the mon before switching action type
					foreach (var rd in ra.prereq.Where(p => p.action == ActionType.RuneOut))
					{
						foreach (var pr in rd.prereq.Where(p => p.action == ActionType.MonOut))
						{

						}
					}
				}

				return returnHtml();
			}

			class RuneAction
			{
				public ActionType action;
				public Rune rune;
				public Monster mon;
				public List<RuneAction> prereq = new List<RuneAction>();

				public RuneAction(ActionType t)
				{
					action = t;
				}

				public void Merge(RuneAction rhs)
				{
					// add missing prereqs to this
					foreach (var pr in rhs.prereq)
					{
						var fp = prereq.FirstOrDefault(p => p.action == pr.action && p.mon == pr.mon && p.rune == pr.rune);
						if (fp == null)
							prereq.Add(fp);
						else
							fp.Merge(pr);
					}
				}
			}

			enum ActionType
			{
				MonIn,
				MonOut,
				RuneIn,
				RuneOut,
			}
		}
	}
}
