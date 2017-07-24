using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

				if (uri.Length > 0 && uri[0].Contains(".png"))
				{
					var res = uri[0].Replace(".png", "").ToLower();
					try
					{
						using (var stream = new MemoryStream())
						{
							var img = Program.GetMonPortrait(int.Parse(res));
							img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
							//return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };

							return new HttpResponseMessage(HttpStatusCode.OK) { Content = new FileContent(res, stream.ToArray(), "image/png") };
						}
					}
					catch (Exception e)
					{
						Program.log.Error(e.GetType() + " " + e.Message);
					}
				}

				ulong mid = 0;
				if (ulong.TryParse(uri[0], out mid))
				{
					if (Program.data == null)
						return returnHtml(null, "missingdata");
					// got Monster Id
					var m = Program.data.GetMonster(mid);
					if (m == null)
						return returnHtml(null, "missingno");
					return returnHtml(null, (m.Locked ? "L " : "") + m.Name + " " + m.Id, new ServedResult("img") { contentDic = { { "src", $"\"/monsters/{m.monsterTypeId}.png\"" } } });
				}
				else
				{
					if (Program.data == null)
						return returnHtml(null, "missingdata");

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
			if (Program.data == null)
				return "no";

			var pieces = Program.data.InventoryItems.Where(i => i.Type == ItemType.SummoningPieces)
				.Select(p => new InventoryItem() { Id = p.Id, Quantity = p.Quantity, Type = p.Type, WizardId = p.WizardId }).ToDictionary(p => p.Id);
			foreach (var p in pieces)
			{
				pieces[p.Key].Quantity -= getPiecesRequired(p.Value);
			}
			pieces = pieces.Where(p => p.Value.Quantity > getPiecesRequired(p.Value)).ToDictionary(p => p.Key, p => p.Value);

			var ll = Program.loads;
			var ldic = ll.ToDictionary(l => l.BuildID);

			var bq = Program.builds.Where(b => !ldic.ContainsKey(b.ID));
			var bb = new List<Build>();
			foreach (var b in bq)
			{
				if (!bb.Any(u => u.MonId == b.MonId))
					bb.Add(b);
			}

			var bdic = bb.ToDictionary(b => b.MonId);

			var mm = Program.data.Monsters.Where(m => !bdic.ContainsKey(m.Id));

			var locked = Program.data.Monsters.Where(m => m.Locked).ToList();
			var unlocked = Program.data.Monsters.Except(locked).ToList();

			var trashOnes = unlocked.Where(m => m.Grade == 1 && m.Name != "Devilmon" && !m.Name.Contains("Angelmon")).ToList();
			unlocked = unlocked.Except(trashOnes).ToList();

			var pairs = new Dictionary<Monster, List<Monster>>();
			var rem = new List<Monster>();
			foreach (var m in locked.OrderByDescending(m => 1/(bb.FirstOrDefault(b => b.mon == m)?.priority ?? m.priority - 0.1)).ThenByDescending(m => m.Grade)
				.ThenByDescending(m => m.level)
				.ThenBy(m => m._attribute)
				.ThenByDescending(m => m.awakened)
				.ThenBy(m => m.loadOrder))
			{
				pairs.Add(m, new List<Monster>());
				int i = m.SkillupsTotal - m.SkillupsLevel;
				for (; i > 0; i--)
				{
					var um = unlocked.FirstOrDefault(ul => ul.monsterTypeId.ToString().Substring(0, 3) == m.monsterTypeId.ToString().Substring(0, 3));
					if (um == null)
						break;
					pairs[m].Add(um);
					rem.Add(um);
					unlocked.Remove(um);
				}
				for (; i > 0; i--)
				{
					int monbase = (m.monsterTypeId / 100) * 100;
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

			mm = mm.Except(bb.Select(b => b.mon));
			mm = mm.Except(Program.builds.Select(b => b.mon));
			mm = mm.Except(pairs.SelectMany(p => p.Value));
			mm = mm.Except(rem);
			
			trashOnes = trashOnes.Concat(mm.Where(m => !pairs.ContainsKey(m) && !m.Locked && m.Name != "Devilmon" && !m.Name.Contains("Angelmon"))).ToList();
			mm = mm.Except(trashOnes);

			mm = mm.OrderByDescending(m => !unlocked.Contains(m))
				.ThenByDescending(m => m.Locked)
				.ThenByDescending(m => m.Grade)
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
						new ServedResult("span") { contentList = { renderMonLink(m, "build") } }, nl }
				};
				nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { "- " + mo.Name + " " + mo.Grade + "* " + mo.level } }));
				if (nl.contentList.Count == 0)
					nl.name = "br";
				return li;
			}
			));

			list.contentList.AddRange(mm.Select(m =>
			{
				var nl = new ServedResult("ul");
				var stars = new StringBuilder();
				for (int s = 0; s < m.Grade; s++)
					stars.Append("<img class='star' src='/runes/star_unawakened.png' >"); //style='left: -" + (0.3*s) + "em' 
				var li = new ServedResult("li")
				{
					contentList = { ((unlocked.Contains(m)) ?
					("TRASH: " + m.Name + " " + m.Grade + "* " + m.level ) :
					renderMonLink(m, "mon")), nl }
				};
				if (!unlocked.Contains(m) && pairs.ContainsKey(m))
					nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { "- " + mo.Name + " " + mo.Grade + "* " + mo.level } }));
				if (nl.contentList.Count == 0)
					nl.name = "br";
				return li;
			}));

			var food = trashOnes.Select(m => new Food() { mon = m, fakeLevel = m.Grade }).ToList();
			food = makeFood(2, food);
			food = makeFood(3, food);
			food = makeFood(4, food);

			list.contentList.AddRange(food.Select(f => recurseFood(f)).ToList());

			return list;
		}

		static ServedResult recurseFood(Food f)
		{
			ServedResult sr = new ServedResult("li");
			
			sr.contentList.Add(f.mon.Name + " " + f.mon.Grade + "*" + "L" + f.mon.level + (f.mon.Grade != f.fakeLevel ? " > " + f.fakeLevel + "*" : ""));
			if (f.food.Any())
			{
				var rr = new ServedResult("ul");
				foreach (var o in f.food)
				{
					rr.contentList.Add(recurseFood(o));
				}
				sr.contentList.Add(rr);
			}
			return sr;
		}

		static List<Food> makeFood(int lev, List<Food> food)
		{
			var outFood = new List<Food>();
			Food current = null;
			while (food.Any(f => f.fakeLevel == lev - 1))
			{
				if (current == null)
				{
					if (food.Count(f => f.fakeLevel == lev - 1) <= lev - 1)
						break;
					current = food.OrderByDescending(f => f.mon.level).FirstOrDefault(f => f.fakeLevel == lev - 1);
					if (current == null)
						break;
					food.Remove(current);
					outFood.Add(current);
					current.fakeLevel = lev;
				}
				else
				{
					// only eat things which are 1 or upgraded
					var tfood = food.Where(f => f.mon.level == 1 || f.fakeLevel != f.mon.Grade).FirstOrDefault(f => f.fakeLevel == lev - 1);
					if (tfood == null)
						break;
					if (current.food.Count(f => f.fakeLevel == lev - 1) >= lev - 1)
					{
						current = null;
					}
					else
					{
						current.food.Add(tfood);
						food.Remove(tfood);
					}
				}
			}

			return outFood.Concat(food).ToList();
		}

		class Food
		{
			public Monster mon;
			public List<Food> food = new List<Food>();
			public int fakeLevel;
		}

		public static ServedResult renderMonLink(Monster m, string prefix = null, bool renderPortrait = true)
		{
			var res = new ServedResult("span");
			if (prefix != null)
				res.contentList.Add(prefix);
			if (renderPortrait)
				res.contentList.Add(new ServedResult("img") { contentDic = { { "class", "\"monster-profile\"" }, { "style", "\"height: 2em;\"" }, { "src", $"\"/monsters/{m.monsterTypeId}.png\"" } } });
			res.contentList.Add(
				new ServedResult("a") {
				contentDic = { { "href", "\"monsters/" + m.Id + "\"" } },
				contentList = { m.Name + " " + +m.Grade + "* " + m.level} });
			res.contentList.Add((m.Locked ? " <span class=\"locked\">L</span>" : "") + " " + m.SkillupsLevel + "/" + m.SkillupsTotal);
			return res;
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
					renderMonLink(m)
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
				var hide = rd.contentList.FirstOrDefault(ele => ele.contentDic.Any(pr => pr.Value.ToString().Contains(r.Id.ToString() + "_")));
				hide.contentDic["style"] = (r.AssignedId == m.Id) ? "\"display:none;\"" : "";
				div.contentList.Add(rd);
			}
			if (l.Runes.All(r => r.AssignedId == m.Id))
				div.contentDic.Add("style", "\"display:none;\"");

			// list skillups
			var nl = new ServedResult("ul");
			nl.contentList.AddRange(pairs?[m]?.Select(mo => new ServedResult("li") { contentList = { "- " + mo.Name + " " + mo.Grade + "* " + mo.level } }));
			if (nl.contentList.Count == 0)
				nl.name = "br";

			li.contentList.Add(span);
			li.contentList.Add(div);
			li.contentList.Add(nl);

			return li;
		}

	}
}