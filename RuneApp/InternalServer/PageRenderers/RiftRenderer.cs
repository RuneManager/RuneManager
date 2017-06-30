using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RunePlugin;

namespace RuneApp.InternalServer
{
	public partial class Master : PageRenderer
	{

		[PageAddressRender("rift")]
		public class RiftRenderer : PageRenderer
		{
			public override HttpResponseMessage Render(HttpListenerRequest req, string[] uri)
			{
				var resp = this.Recurse(req, uri);
				if (resp != null)
					return resp;

				var best = Directory.GetFiles(Environment.CurrentDirectory, "GetBestClearRiftDungeon*.resp.json").OrderByDescending(s => s);
				if (best.Any())
				{
					var bestRift = JsonConvert.DeserializeObject<RunePlugin.Response.GetBestClearRiftDungeonResponse>(File.ReadAllText(best.First()), new SWResponseConverter());
					var sr = new List<ServedResult>();
					foreach (var br in bestRift.BestDeckRiftDungeons)
					{
						sr.Add("<h1>" + br.RiftDungeonId + "</h1>");
						sr.Add("<h3>" + br.ClearRating + " " + br.ClearDamage + "</h3>");
						var table = "<table>";

						table += "<tr>";
						for (int i = 1; i < 5; i++)
						{
							if (i == br.LeaderIndex)
								table += "<td style='background-color: yellow' >";
							else
								table += "<td>";
							var mp = br.Monsters.FirstOrDefault(p => p.Position == i);
							if (mp != null)
							{
								var mon = Program.data.GetMonster((ulong)mp.MonsterId);
								if (mon == null)
								{
									table += RunePlugin.SWPlugin.MonsterName((long)mp.MonsterId) + "";
								}
								else
								{
									table += mon.Name + " " + mon._class + "*";
								}
							}
							table += "</td>";
						}

						table += "</tr>";
						table += "<tr>";
						for (int i = 5; i < 9; i++)
						{
							if (i == br.LeaderIndex)
								table += "<td style='background-color: yellow' >";
							else
								table += "<td>";
							var mp = br.Monsters.FirstOrDefault(p => p.Position == i);
							if (mp != null)
							{
								var mon = Program.data.GetMonster((ulong)mp.MonsterId);
								if (mon == null)
								{
									table += RunePlugin.SWPlugin.MonsterName((long)mp.MonsterId) + "";
								}
								else
								{
									table += mon.Name + " " + mon._class + "*";
								}
							}
							table += "</td>";
						}
						table += "</tr>";

						table += "</table>";

						sr.Add(table);
					}
					return returnHtml(new ServedResult[] { new ServedResult("style") { contentList = { "td {border: 1px solid black;}" } } }, sr.ToArray());
				}
				else
				{
					return returnHtml(null, "Please put GetBestClearRiftDungeon_*.resp.json");
				}
			}
		}

	}
}
