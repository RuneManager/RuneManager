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
		[PageAddressRender("monsters")]
		public class MonstersRenderer : PageRenderer
		{
			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				if (uri.Length == 0)
				{
					return returnHtml(new ServedResult[]{
					new ServedResult("link") { contentDic = { { "rel", "\"stylesheet\"" }, { "type", "\"text/css\"" }, { "href", "\"/css/runes.css\"" } } },
					new ServedResult("script")
					{
						contentDic = { { "type", "\"application/javascript\"" } },
						contentList = { @"function showhide(id) {
	var ee = document.getElementById(id);
	if (ee.style.display == 'none')
		ee.style.display = 'block';
	else
		ee.style.display = 'none';
}" }
					}
				}, renderMagicList());
				}

				ulong mid = 0;
				if (ulong.TryParse(uri[0], out mid))
				{
					// got Monster Id
					var m = Program.data.GetMonster(mid);
					return returnHtml(null, (m.Locked ? "L " : "") + m.Name + " " + m.Id);
				}
				else
				{
					var m = Program.data.GetMonster(uri[0]);
					if (m != null)
						return new HttpResponseMessage(HttpStatusCode.SeeOther) { Headers = { { "Location", "/monsters/" + m.Id } } };
				}
				return return404();
			}
		}

		static int getPiecesRequired(InventoryItem p)
		{
			var a = p.Id / 100;
			var b = Save.MonIdNames[a];
			var c = MonsterStat.BaseStars(b);
			var d = InventoryItem.PiecesRequired(c);
			return d;
		}

		static ServedResult renderMagicList()
		{
			// return all completed loads on top, in progress build, unrun builds, mons with no builds
			ServedResult list = new ServedResult("ul");
			list.contentList = new List<ServedResult>();

			var pieces = Program.data.InventoryItems.Where(i => i.Type == ItemType.SummoningPieces)
				.Select(p => new InventoryItem() { Id = p.Id, Quantity = p.Quantity, Type = p.Type, WizardId = p.WizardId }).ToDictionary(p => p.Id);
			foreach (var p in pieces)
			{
				pieces[p.Key].Quantity -= getPiecesRequired(p.Value);
			}
			pieces = pieces.Where(p => p.Value.Quantity > getPiecesRequired(p.Value)).ToDictionary(p => p.Key, p => p.Value);

			var ll = Program.loads;
			var ldic = ll.ToDictionary(l => l.BuildID);

			var bb = Program.builds.Where(b => !ldic.ContainsKey(b.ID));
			var bdic = bb.ToDictionary(b => b.MonId);

			var mm = Program.data.Monsters.Where(m => !bdic.ContainsKey(m.Id));

			var locked = Program.data.Monsters.Where(m => m.Locked).ToList();
			var unlocked = Program.data.Monsters.Except(locked).ToList();
			var pairs = new Dictionary<Monster, List<Monster>>();
			foreach (var m in locked.OrderByDescending(m => bb.FirstOrDefault(b => b.mon == m)?.priority ?? m.priority))
			{
				pairs.Add(m, new List<Monster>());
				int i = m.SkillupsTotal - m.SkillupsLevel;
				for (; i > 0; i--)
				{
					var um = unlocked.FirstOrDefault(ul => ul._monsterTypeId.ToString().Substring(0, 3) == m._monsterTypeId.ToString().Substring(0, 3));
					if (um == null)
						break;
					pairs[m].Add(um);
					unlocked.Remove(um);
				}
				for (; i > 0; i--)
				{
					int monbase = (m._monsterTypeId / 100) * 100;
					for (int j = 1; j < 4; j++)
					{
						if (pieces.ContainsKey(monbase + j) && pieces[monbase + j].Quantity >= getPiecesRequired(pieces[monbase + j]))
						{
							pieces[monbase + j].Quantity -= getPiecesRequired(pieces[monbase + j]);
							pairs[m].Add(new Monster() { Name = pieces[monbase + j].Name + " Pieces (" + pieces[monbase + j].Quantity + " remain)" });
						}
					}
				}
			}

			mm = mm.OrderByDescending(m => !unlocked.Contains(m))
				.ThenByDescending(m => m.Locked)
				.ThenByDescending(m => m._class)
				.ThenByDescending(m => m.level)
				.ThenBy(m => m._attribute)
				.ThenByDescending(m => m.awakened)
				.ThenBy(m => m.loadOrder)
				;

			list.contentList.AddRange(ll.Select(l => renderLoad(l, pairs)));
			list.contentList.AddRange(bb.Select(b =>
			{
				var m = b.mon;
				var nl = new ServedResult("ul");
				var li = new ServedResult("li")
				{
					contentList = {
						new ServedResult("span") { contentList = { "build " + m.Name + " " + +m._class + "* " + m.level + " " + m.SkillupsLevel + "/" + m.SkillupsTotal } }, nl }
				};
				nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { "- " + mo.Name + " " + mo._class + "* " + mo.level } }));
				if (nl.contentList.Count == 0)
					nl.name = "br";
				return li;
			}
			));

			Console.WriteLine(mm.Count());
			mm = mm.Except(bb.Select(b => b.mon));
			Console.WriteLine(mm.Count());
			mm = mm.Except(Program.builds.Select(b => b.mon));
			Console.WriteLine(mm.Count());
			mm = mm.Except(pairs.SelectMany(p => p.Value));
			Console.WriteLine(mm.Count());
			list.contentList.AddRange(mm.Select(m =>
			{
				var nl = new ServedResult("ul");
				var stars = new StringBuilder();
				for (int s = 0; s < m._class; s++)
					stars.Append("<img class='star' src='/runes/star_unawakened.png' >"); //style='left: -" + (0.3*s) + "em' 
				var li = new ServedResult("li")
				{
					contentList = { ((unlocked.Contains(m)) ?
					("TRASH: " + m.Name + " " + m._class + "* " + m.level ) :
					("mon " + m.Name + " " + m._class + " " + m.level + " " + (m.Locked ? "<span class=\"locked\">L</span>" : "") + " " + m.SkillupsLevel + "/" + m.SkillupsTotal)), nl }
				};
				if (!unlocked.Contains(m))
					nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { "- " + mo.Name + " " + mo._class + "* " + mo.level } }));
				if (nl.contentList.Count == 0)
					nl.name = "br";
				return li;
			}));

			return list;
		}

		protected static ServedResult renderLoad(Loadout l, Dictionary<Monster, List<Monster>> pairs)
		{
			var b = Program.builds.FirstOrDefault(bu => bu.ID == l.BuildID);
			var m = b.mon;

			var li = new ServedResult("li");

			// render name
			var span = new ServedResult("span")
			{
				contentList = {
					new ServedResult("a") { contentDic = { { "href", "\"javascript:showhide(" +m.Id.ToString() + ")\"" } }, contentList = { "+ load" } },
					" ",
					new ServedResult("a") { contentDic = { { "href", "\"monsters/" + m.Id + "\"" } }, contentList = { m.Name + " " + +m._class + "* " + m.level + " " + m.SkillupsLevel + "/" + m.SkillupsTotal } }
			}
			};

			// render rune swaps
			var div = new ServedResult("div")
			{
				contentDic = { { "id", '"' + m.Id.ToString() + '"' },
					//{ "class", "\"rune-container\"" }
			}
			};
			foreach (var r in l.Runes)
			{
				var rd = RuneRenderer.renderRune(r);
				var hide = rd.contentList.FirstOrDefault(ele => ele.contentDic.Any(pr => pr.Value.ToString() == '"' + r.Id.ToString() + '"'));
				hide.contentDic["style"] = (r.AssignedId == m.Id) ? "\"display:none;\"" : "";
				div.contentList.Add(rd);
			}
			if (l.Runes.All(r => r.AssignedId == m.Id))
				div.contentDic.Add("style", "\"display:none;\"");

			// list skillups
			var nl = new ServedResult("ul");
			nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { "- " + mo.Name + " " + mo._class + "* " + mo.level } }));
			if (nl.contentList.Count == 0)
				nl.name = "br";

			li.contentList.Add(span);
			li.contentList.Add(div);
			li.contentList.Add(nl);

			return li;
		}

		protected static ServedResult renderMonName(Monster m)
		{
			return new ServedResult("span") { contentList = { m.Name } };
		}

	}
}