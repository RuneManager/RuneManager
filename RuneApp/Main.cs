using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using RuneOptim;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;

namespace RuneApp {
	public partial class Main : Form
	{
		string filelink = "";
		string whatsNewText = "";
		Build currentBuild = null;

		private Dictionary<string, List<ToolStripMenuItem>> shrineMap = new Dictionary<string, List<ToolStripMenuItem>>();

		private Task runTask = null;
		private CancellationToken runToken;
		private CancellationTokenSource runSource = null;
		bool plsDie = false;
		bool isRunning = false;
		public static Help help = null;
		public static Irene irene = null;

		public static bool goodRunes { get { return Program.goodRunes; } set { Program.goodRunes = value; } }

		public static Main currentMain = null;
		public static RuneDisplay runeDisplay = null;
		Monster displayMon = null;

		bool teamChecking = false;
		Build teamBuild = null;
		Dictionary<string, List<string>> toolmap = null;
		List<string> knownTeams = new List<string>();
		List<string> extraTeams = new List<string>();

		public static Main Instance = null;


		//public static Configuration config {  get { return Program.config; } }
		public static log4net.ILog Log { get { return Program.log; } }

		public Main()	
		{
			InitializeComponent();
			Log.Info("Initialized Main");
			Instance = this;

			currentMain = this;

			useRunesCheck.Checked = Program.Settings.UseEquipped;

			findGoodRunes.Enabled = Program.Settings.MakeStats;
			if (!Program.Settings.MakeStats)
				findGoodRunes.Checked = false;

			#region Sorter
			var sorter = new ListViewSort();
			dataMonsterList.ListViewItemSorter = sorter;
			sorter.OnColumnClick(colMonGrade.Index);
			sorter.OnColumnClick(colMonPriority.Index);

			dataRuneList.ListViewItemSorter = new ListViewSort();

			dataCraftList.ListViewItemSorter = new ListViewSort();

			sorter = new ListViewSort();
			sorter.OnColumnClick(0);
			buildList.ListViewItemSorter = sorter;
			#endregion

			#region Update

			if (Program.Settings.CheckUpdates)
			{
				Task.Factory.StartNew(() =>
				{
					using (WebClient client = new WebClient())
					{
						Log.Info("Checking for updates");
						client.DownloadStringCompleted += client_DownloadStringCompleted;
						client.DownloadStringAsync(new Uri("https://raw.github.com/Skibisky/RuneManager/master/version.txt"));
					}
				});
			}
			else
			{
				updateBox.Show();
				Log.Info("Updates Disabled");
				updateComplain.Text = "Updates Disabled";
				var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
				string oldvernum = ver.ProductVersion;
				updateCurrent.Text = "Current: " + oldvernum;
				updateNew.Text = "";
			}

			#endregion

			#region Labels

			int yStart = 410;

			Label l = new Label();
			l.Location = new Point(4 + 50, yStart - 18);
			l.Name = "compBefore";
			l.Text = "Before";
			l.Size = new Size(50, 14);
			groupBox1.Controls.Add(l);

			l = new Label();
			l.Location = new Point(4 + 100, yStart - 18);
			l.Name = "compAfter";
			l.Text = "After";
			l.Size = new Size(50, 14);
			groupBox1.Controls.Add(l);

			l = new Label();
			l.Location = new Point(4 + 150, yStart - 18);
			l.Name = "compDiff";
			l.Text = "Difference";
			l.Size = new Size(60, 14);
			groupBox1.Controls.Add(l);

			int xx = 0;
			int yy = 0;
			var labelPrefixes = new string[] { "Pts" }.Concat(Build.statNames).Concat(Build.extraNames);
			foreach (var s in labelPrefixes)
			{
				groupBox1.Controls.MakeControl<Label>(s, "compStat", 4 + xx, yStart + yy, 50, 14, s);
				xx += 50;

				groupBox1.Controls.MakeControl<Label>(s, "compBefore", 4 + xx, yStart + yy, 50, 14, "");
				xx += 50;

				groupBox1.Controls.MakeControl<Label>(s, "compAfter", 4 + xx, yStart + yy, 50, 14, "");
				xx += 50;
				
				groupBox1.Controls.MakeControl<Label>(s, "compDiff", 4 + xx, yStart + yy, 150, 14, "");

				if (s == "SPD")
					yy += 4;
				if (s == "ACC")
					yy += 8;
				if (s == "MxD")
					yy += 8;

				yy += 16;
				xx = 0;
			}

			for (int i = 0; i < 4; i++) {
				groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compStat", 4 + xx, yStart + yy, 50, 14, "Skill" + (i + 1));
				xx += 50;

				groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compBefore", 4 + xx, yStart + yy, 50, 14, "");
				xx += 50;

				groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compAfter", 4 + xx, yStart + yy, 50, 14, "");
				xx += 50;

				groupBox1.Controls.MakeControl<Label>("Skill" + (i + 1), "compDiff", 4 + xx, yStart + yy, 150, 14, "");

				yy += 16;
				xx = 0;
			}

			#endregion
		}

		private void AddShrine(string stat, int num, int value, ToolStripMenuItem owner)
		{
			ToolStripMenuItem it = new ToolStripMenuItem(num.ToString() + (num > 0 ? " (" + value + "%)" : ""));
			it.Tag = new KeyValuePair<string, int>(stat, value);
			it.Click += ShrineClick;
			owner.DropDownItems.Add(it);
			if (!shrineMap.ContainsKey(stat))
				shrineMap.Add(stat, new List<ToolStripMenuItem>());
			shrineMap[stat].Add(it);
			if (Program.data.shrines[stat].EqualTo(value))
				it.Checked = true;
		}

		private void ShrineClick(object sender, EventArgs e)
		{
			var it = (ToolStripMenuItem)sender;
			if (it != null)
			{
				var tag = (KeyValuePair<string, int>)it.Tag;
				var stat = tag.Key;

				foreach (ToolStripMenuItem i in shrineMap[stat])
				{
					i.Checked = false;
				}
				it.Checked = true;
				if (Program.data == null)
					return;

				if (string.IsNullOrWhiteSpace(stat))
					return;

				Program.data.shrines[stat] = tag.Value;
				File.WriteAllText("shrine_overwrite.json", JsonConvert.SerializeObject(Program.data.shrines));
			}
		}

		private void Main_Load(object sender, EventArgs e)
		{
			#region Watch collections and try loading
			Program.saveFileTouched += Program_saveFileTouched;
			Program.builds.CollectionChanged += Builds_CollectionChanged;
			Program.OnRuneUpdate += Program_OnRuneUpdate;
			Program.OnMonsterUpdate += Program_OnMonsterUpdate;
			Program.loads.CollectionChanged += Loads_CollectionChanged;
			Program.BuildsProgressTo += Program_BuildsProgressTo;

#if !DEBUG
			try {
#endif
				LoadSaveResult loadResult = 0;
				do
				{
					loadResult = Program.FindSave();
					switch (loadResult)
					{
						case LoadSaveResult.Success:
							break;
						default:
							if (MessageBox.Show("Couldn't automatically load save.\r\nManually locate a save file?", "Load Save", MessageBoxButtons.YesNo) == DialogResult.Yes)
							{
								loadResult = loadSaveDialogue();
							}
							else
							{
								Application.Exit();
								return;
							}
							break;
					}
				} while (loadResult != LoadSaveResult.Success) ;

				loadResult = 0;
				do
				{
					loadResult = Program.LoadBuilds();
					switch (loadResult)
					{
						case LoadSaveResult.Failure:
							if (MessageBox.Show("Save was invalid while loading builds.\r\nManually locate a save file?", "Load Builds", MessageBoxButtons.YesNo) == DialogResult.Yes)
							{
								loadResult = loadSaveDialogue();
							}
							break;
						case LoadSaveResult.EmptyFile:
						case LoadSaveResult.FileNotFound:
							loadResult = LoadSaveResult.Success;
							break;
						default:
							break;
					}
				} while (loadResult != LoadSaveResult.Success) ;
#if !DEBUG
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Critical Error {ex.GetType()}\r\nDetails in log file.", "Error");
				Log.Fatal($"Fatal during load {ex.GetType()}", ex);
			}
#endif
			#endregion

			RegenLists();

			#region Shrines

			ToolStripMenuItem[] shrineMenu = new ToolStripMenuItem[] { speedToolStripMenuItem, defenseToolStripMenuItem , attackToolStripMenuItem, healthToolStripMenuItem,
			waterAttackToolStripMenuItem, fireAttackToolStripMenuItem, windAttackToolStripMenuItem, lightAttackToolStripMenuItem, darkAttackToolStripMenuItem, criticalDamageToolStripMenuItem};
			for (int i = 0; i < 11; i++)
			{
				for (int j = 0; j < Deco.ShrineStats.Length; j++)
				{
					if (j < 4)
						AddShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(i * Deco.ShrineLevel[j]), shrineMenu[j]);
					else if (j < 9)
						AddShrine(Deco.ShrineStats[j], i, (int)Math.Ceiling(1 + i * Deco.ShrineLevel[j]), shrineMenu[j]);
					else
						AddShrine(Deco.ShrineStats[j], i, (int)Math.Floor(i * Deco.ShrineLevel[j]), shrineMenu[j]);
				}
			}

			#endregion

			buildList.SelectedIndexChanged += buildList_SelectedIndexChanged;

			#region DoubleBuffered
			this.SetDoubleBuffered();
			buildList.SetDoubleBuffered();
			#endregion

			foreach (ToolStripItem ii in menu_buildlist.Items)
			{
				if (ii.Text == "Team")
				{
					toolmap = new Dictionary<string, List<string>>()
					{
						{ "PvE", new List<string> { "Farmer", "World Boss", "ToA" } },
						{ "Dungeon", new List<string> { "Giant", "Dragon", "Necro", "Secret", "HoH", "Elemental" } },
						{ "Raid", new List<string> {"Group", "Light R", "Dark R", "Fire R", "Water R", "Wind R" } },
						{ "PvP", new List<string> { "AO", "AD", "GWO", "GWD", "RTA" } },

						{ "Elemental", new List<string> {"Magic", "Light D", "Dark D", "Fire D", "Water D", "Wind D" } },
						{ "ToA", new List<string> { "ToAN", "ToAH" } }
					};

					ToolStripMenuItem tsmi = ii as ToolStripMenuItem;

					tsTeamAdd(tsmi, "PvE");
					tsTeamAdd(tsmi, "Dungeon");
					tsTeamAdd(tsmi, "Raid");
					tsTeamAdd(tsmi, "PvP");

					var tsnone = new ToolStripMenuItem("(Clear)");
					tsnone.Font = new Font(tsnone.Font, FontStyle.Italic);
					tsnone.Click += tsTeamHandler;

					tsmi.DropDownItems.Add(tsnone);
				}
			}

			if (Program.Settings.StartUpHelp)
				OpenHelp();

			if (irene == null)
				irene = new Irene(this);
			if (Program.Settings.ShowIreneOnStart)
				irene.Show(this);
		}

		private void Program_saveFileTouched(object sender, EventArgs e)
		{
			this.fileBox.Visible = true;
		}

		private void ColorMonsWithBuilds()
		{
			foreach (ListViewItem lvim in dataMonsterList.Items)
			{
				if (Program.builds.Any(b => b.mon == lvim.Tag as Monster))
				{
					lvim.ForeColor = Color.Green;
				}
			}
		}

		private void Program_BuildsProgressTo(object sender, PrintToEventArgs e)
		{
			ProgressToList(e.build, e.Message);
		}

		private void Loads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					foreach (var l in e.NewItems.Cast<Loadout>())
					{
						var mm = Program.builds.FirstOrDefault(b => b.ID == l.BuildID)?.mon;
						if (mm != null) {
							mm.OnRunesChanged += Mm_OnRunesChanged;
						}

						checkLocked();
						Invoke((MethodInvoker)delegate
						{
							ListViewItem nli = loadoutList.Items.Cast<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout).BuildID == l.BuildID) ?? new ListViewItem();
							
							ListViewItemLoad(nli, l);
							if (!loadoutList.Items.Contains(nli))
								loadoutList.Items.Add(nli);
						});
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					loadoutList.Items.Clear();
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					foreach (var l in e.OldItems.Cast<Loadout>())
					{
						var mm = Program.builds.FirstOrDefault(b => b.ID == l.BuildID)?.mon;
						if (mm != null) {
							mm.OnRunesChanged -= Mm_OnRunesChanged;
						}

						Invoke((MethodInvoker)delegate
						{
							loadoutList.Items.Cast<ListViewItem>().FirstOrDefault(li => li.Tag == l).Remove();
						});
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
				default:
					throw new NotImplementedException();
			}
		}

		private void Mm_OnRunesChanged(object sender, EventArgs e) {
			var bb = Program.builds.FirstOrDefault(b => b.mon != null && b.mon == (sender as Monster));
			if (bb != null) {
				var l = Program.loads.FirstOrDefault(lo => lo.BuildID == bb.ID);
				Invoke((MethodInvoker)delegate {
					ListViewItem nli = loadoutList.Items.Cast<ListViewItem>().FirstOrDefault(li => (li.Tag as Loadout).BuildID == l.BuildID) ?? new ListViewItem();
					ListViewItemLoad(nli, l);
				});
			}
		}

		private ListViewItem ListViewItemRune(Rune rune, ListViewItem nli = null) {
			if (nli == null)
				nli = new ListViewItem();
			nli.Tag = rune;
			nli.BackColor = rune.Locked ? Color.Red : Color.Transparent;

			while (nli.SubItems.Count < 6)
				nli.SubItems.Add("");

			nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, rune.Set.ToString());
			nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, rune.Id.ToString());
			nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, rune.Grade.ToString());
			nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, Rune.StringIt(rune.Main.Type, true));
			nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, rune.Main.Value.ToString());
			nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, rune.Level.ToString());
			return nli;
		}

		private void ListViewItemLoad(ListViewItem nli, Loadout l)
		{
			Build b = Program.builds.FirstOrDefault(bu => bu.ID == l.BuildID);
			nli.Tag = l;
			nli.Text = b.ID.ToString();
			nli.Name = b.ID.ToString();
			nli.UseItemStyleForSubItems = false;
			while (nli.SubItems.Count < 6)
				nli.SubItems.Add("");
			nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, b.ID.ToString());
			nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, b.MonName);
			nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, b.mon.Id.ToString());

			l.RecountDiff(b.mon.Id);

			nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, (l.runesNew + l.runesChanged).ToString());
			if (l.runesNew > 0 && b.mon.Current.RuneCount < 6)
				nli.SubItems[3].ForeColor = Color.Green;
			if (Program.Settings.SplitAssign)
				nli.SubItems[3].Text = l.runesNew.ToString() + "/" + l.runesChanged.ToString();
			nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, l.powerup.ToString());
			nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, (l.Time / (double)1000).ToString("0.##"));
			if (l.Time / (double)1000 > 60)
				nli.SubItems[5].BackColor = Color.Red;
			else if (l.Time / (double)1000 > 30)
				nli.SubItems[5].BackColor = Color.Orange;
		}
		
		private ListViewItem ListViewItemMonster(Monster mon, ListViewItem nli = null) {
			if (nli == null)
				nli = new ListViewItem();
			nli.Tag = mon;
			nli.ForeColor = mon.inStorage ? Color.Gray : Color.Black;
			nli.Text = mon.FullName;

			while (nli.SubItems.Count < 6)
				nli.SubItems.Add("");

			nli.SubItems[0] = new ListViewItem.ListViewSubItem(nli, mon.FullName);
			nli.SubItems[1] = new ListViewItem.ListViewSubItem(nli, mon.Grade.ToString());
			nli.SubItems[2] = new ListViewItem.ListViewSubItem(nli, mon.priority.ToString("#"));
			nli.SubItems[3] = new ListViewItem.ListViewSubItem(nli, mon.Id.ToString());
			nli.SubItems[4] = new ListViewItem.ListViewSubItem(nli, mon.monsterTypeId.ToString());
			nli.SubItems[5] = new ListViewItem.ListViewSubItem(nli, mon.level.ToString());
			return nli;
		}

		private void Program_OnMonsterUpdate(object sender, bool deleted) {
			var mon = sender as Monster;
			if (mon != null) {
				Invoke((MethodInvoker)delegate {
					var nli = dataMonsterList.Items.Cast<ListViewItem>().FirstOrDefault(li => (li.Tag as Monster).Id == mon.Id);
					if (deleted) {
						if (nli != null)
							dataMonsterList.Items.Remove(nli);
						return;
					}

					bool add = nli == null;
					nli = ListViewItemMonster(mon, nli);
					if (add)
						dataMonsterList.Items.Add(nli);

					if (displayMon?.Id == mon.Id)
						ShowMon(displayMon);
				});
			}
		}

		private void Program_OnRuneUpdate(object sender, bool deleted) {
			var rune = sender as Rune;
			if (rune != null) {
				Invoke((MethodInvoker)delegate {
					var nli = dataRuneList.Items.Cast<ListViewItem>().FirstOrDefault(li => (li.Tag as Rune).Id == rune.Id);
					if (deleted) {
						if (nli != null)
							dataRuneList.Items.Remove(nli);
						return;
					}

					bool add = nli == null;
					nli = ListViewItemRune(rune, nli);
					if (add)
						dataRuneList.Items.Add(nli);

					if (this.runeInventory.RuneId == rune.Id)
						runeInventory.SetRune(rune);
					if (this.runeEquipped.RuneId == rune.Id)
						runeEquipped.SetRune(rune);
					if (displayMon?.Id == rune.AssignedId)
						ShowMon(displayMon);
				});
			}
		}

		private void Builds_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					List<ListViewItem> tempMons = null;
					this.Invoke((MethodInvoker)delegate { tempMons = dataMonsterList.Items.Cast<ListViewItem>().ToList(); });

					foreach (var b in e.NewItems.Cast<Build>())
					{
						ListViewItem li = new ListViewItem();
						ListViewItemBuild(li, b);
						this.Invoke((MethodInvoker)delegate { buildList.Items.Add(li); });
						var lv1li = tempMons.FirstOrDefault(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Any(s => s.Text == (b.mon?.Id ?? b.MonId).ToString()));
						if (lv1li != null)
						{
							lv1li.ForeColor = Color.Green;
						}
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					foreach (var b in e.OldItems.Cast<Build>()) {
						var bli = buildList.Items.Cast<ListViewItem>().FirstOrDefault(lvi => b.Equals(lvi.Tag));
						buildList.Items.Remove(bli);

						var lv1li = dataMonsterList.Items.Cast<ListViewItem>().FirstOrDefault(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Any(s => s.Text == b.mon.FullName));
						if (lv1li != null) {
							lv1li.ForeColor = Color.Black;
							if ((lv1li.Tag as Monster)?.inStorage ?? false)
								lv1li.ForeColor = Color.Gray;

						}
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
				default:
					throw new NotImplementedException();
			}
		}

		private void ListViewItemBuild(ListViewItem lvi, Build b)
		{
			lvi.Text = b.ID.ToString();

			while (lvi.SubItems.Count < 6)
				lvi.SubItems.Add("");

			lvi.SubItems[0] = new ListViewItem.ListViewSubItem(lvi, b.priority.ToString());
			lvi.SubItems[1] = new ListViewItem.ListViewSubItem(lvi, b.ID.ToString());
			lvi.SubItems[2] = new ListViewItem.ListViewSubItem(lvi, b.mon?.FullName ?? b.MonName);
			lvi.SubItems[3] = new ListViewItem.ListViewSubItem(lvi, "");
			lvi.SubItems[4] = new ListViewItem.ListViewSubItem(lvi, (b.mon?.Id ?? b.MonId).ToString());
			lvi.SubItems[5] = new ListViewItem.ListViewSubItem(lvi, getTeamStr(b));

			lvi.Tag = b;

			if (b.runePrediction.Any(p => p.Value.Value))
				lvi.ForeColor = Color.Purple;
		}

		private string getTeamStr(Build b)
		{
			if (b.Teams == null || b.Teams.Count == 0)
				return "";

			var sz = buildCHTeams.Width;
			var str = "";
			for (int i = 0; i < b.Teams.Count; i++)
			{
				var sb = new StringBuilder(string.Join(", ", b.Teams.Take(i)));
				if (!string.IsNullOrWhiteSpace(sb.ToString()))
					sb.Append(", ");
				sb.Append(b.Teams.Count - i);
				var tstr = string.Join(", ", b.Teams.Take(i+1));
				if (this.CreateGraphics().MeasureString(tstr + "...", buildList.Font).Width > sz - 10)
					return sb.ToString();
				str = tstr;
			}

			return str;
		}

		private void tsTeamAdd(ToolStripMenuItem parent, string item)
		{
			knownTeams.Add(item);
			ToolStripMenuItem n = new ToolStripMenuItem(item);
			parent.DropDownItems.Add(n);
			n.CheckedChanged += tsTeamHandler;
			n.CheckOnClick = true;

			if (toolmap[item] != null)
			{
				foreach (var smi in toolmap[item])
				{
					if (toolmap.ContainsKey(smi))
					{
						tsTeamAdd(n, smi);
					}
					else
					{
						ToolStripMenuItem s = new ToolStripMenuItem(smi);
						s.CheckedChanged += tsTeamHandler;
						s.CheckOnClick = true;
						n.DropDownItems.Add(s);
					}
				}
			}
		}

		private void tsTeamHandler(object sender, EventArgs e)
		{
			if (teamChecking || teamBuild == null)
				return;

			ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
			if (tsmi.Text == "(Clear)")
			{
				teamBuild.Teams.Clear();
			}
			else
			{
				if (tsmi.Checked)
				{
					if (!teamBuild.Teams.Contains(tsmi.Text))
						teamBuild.Teams.Add(tsmi.Text);
				}
				else
				{
					teamBuild.Teams.Remove(tsmi.Text);
				}
			}
			var bli = buildList.Items.Cast<ListViewItem>().FirstOrDefault(it => it.Tag == teamBuild);
			if (bli != null)
			{
				while (bli.SubItems.Count < 6) bli.SubItems.Add("");
				bli.SubItems[5].Text = getTeamStr(teamBuild);
			}
			menu_buildlist.Close();
		}

		private void monstertab_list_select(object sender, EventArgs e)
		{
			Monster mon = dataMonsterList?.FocusedItem?.Tag as Monster;
			if (mon != null)
				ShowMon(mon);
		}

		private void loadSaveDialogue(object sender, EventArgs e)
		{
			loadSaveDialogue();
		}

		private LoadSaveResult loadSaveDialogue()
		{
			LoadSaveResult loadres = LoadSaveResult.Failure;
			using (var lsd = new LoadSaveDialogue())
			{
				if (lsd.ShowDialog() == DialogResult.OK) // Test result.
				{
					try
					{
						loadres = Program.LoadSave(lsd.Filename);
					}
					catch (IOException ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
			}
			return loadres;
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// confirm maybe?
			Close();
		}

		private void runeDial_RuneClick(object sender, RuneClickEventArgs e)
		{
			if (e.Rune != null)
			{
				runeEquipped.Show();
				runeEquipped.SetRune(e.Rune);
			}
			else
			{
				runeEquipped.Hide();
			}
		}

		private void lbCloseEquipped_Click(object sender, EventArgs e)
		{
			runeEquipped.Hide();
			runeDial.ResetRuneClicked();
		}

		private void lbCloseInventory_Click(object sender, EventArgs e)
		{
			runeInventory.Hide();
		}

		private void runetab_list_select(object sender, EventArgs e)
		{
			var rune = dataRuneList.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.Tag as Rune;
			if (rune != null)
			{
				runeInventory.Show();
				runeInventory.SetRune(rune);
			}
		}

		private void crafttab_list_select(object sender, EventArgs e)
		{
			var craft = dataCraftList.SelectedItems.Cast<ListViewItem>().FirstOrDefault()?.Tag as Craft;
			if (craft != null)
			{
				runeInventory.Show();
				runeInventory.SetCraft(craft);
			}
		}

		private void listView2_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			var sorter = (ListViewSort)((ListView)sender).ListViewItemSorter;
			sorter.OnColumnClick(e.Column);
			((ListView)sender).Sort();
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			Monster mon = dataMonsterList?.FocusedItem?.Tag as Monster;
			if (mon != null)
			{
				int maxPri = Program.data.Monsters.Max(x => x.priority);
				if (mon.priority == 0)
				{
					mon.priority = maxPri + 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
				}
				else if (mon.priority != 1)
				{
					int pri = mon.priority;
					Monster mon2 = Program.data.Monsters.FirstOrDefault(x => x.priority == pri - 1);
					if (mon2 != null)
					{
						ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.FullName);
						mon2.priority += 1;
						listMon.SubItems[colMonPriority.Index].Text = mon2.priority.ToString();
					}
					mon.priority -= 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (mon.priority).ToString();
				}
				dataMonsterList.Sort();
			}
		}

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			Monster mon = dataMonsterList?.FocusedItem?.Tag as Monster;
			if (mon != null)
			{
				int maxPri = Program.data.Monsters.Max(x => x.priority);
				if (mon.priority == 0)
				{
					mon.priority = maxPri + 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (maxPri + 1).ToString();
				}
				else if (mon.priority != maxPri)
				{
					int pri = mon.priority;
					Monster mon2 = Program.data.Monsters.Where(x => x.priority == pri + 1).FirstOrDefault();
					if (mon2 != null)
					{
						var items = dataMonsterList.Items;
						ListViewItem listMon = dataMonsterList.FindItemWithText(mon2.FullName);
						mon2.priority -= 1;
						listMon.SubItems[colMonPriority.Index].Text = mon2.priority.ToString();
					}
					mon.priority += 1;
					dataMonsterList.FocusedItem.SubItems[colMonPriority.Index].Text = (mon.priority).ToString();
				}
				dataMonsterList.Sort();
			}
			
		}

		private void button2_Click(object sender, EventArgs e)
		{
			tabControl1.SelectTab(tabRunes.Name);
			filterRunesList(x => ((Rune)x).Slot == ((Rune)runeEquipped.Tag).Slot);
		}

		private void filterRunesList(Predicate<object> p)
		{
			dataRuneList.Items.Clear();

			if (Program.data?.Runes == null) return;

			foreach (Rune rune in Program.data.Runes.Where(p.Invoke)) {
				dataRuneList.Items.Add(ListViewItemRune(rune));
			}
		}

		private void runetab_clearfilter(object sender, EventArgs e)
		{
			filterRunesList(x => true);
		}
		
		private ListViewItem lastFocused = null;

		private void loadoutlist_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (loadoutList.SelectedItems.Count == 0)
				lastFocused = null;
			if (loadoutList.FocusedItem != null && lastFocused != loadoutList.FocusedItem)
			{
				var item = loadoutList.FocusedItem;
				if (item.Tag != null)
				{
					Loadout load = (Loadout)item.Tag;

					var monid = ulong.Parse(item.SubItems[2].Text);
					var bid = int.Parse(item.SubItems[0].Text);

					var build = Program.builds.FirstOrDefault(b => b.ID == bid);

					Monster mon = null;
					if (build == null)
						mon = Program.data.GetMonster(monid);
					else
						mon = build.mon;
					
					ShowMon(mon);

					ShowStats(load.GetStats(mon), mon);

					ShowLoadout(load);

					var dmon = Program.data.GetMonster(monid);
					if (dmon != null)
					{
						var dmonld = dmon.Current.Leader;
						var dmonsh = dmon.Current.Shrines;
						dmon.Current.Leader = load.Leader;
						dmon.Current.Shrines = load.Shrines;
						var dmonfl = dmon.Current.FakeLevel;
						var dmonps = dmon.Current.PredictSubs;
						dmon.Current.FakeLevel = load.FakeLevel;
						dmon.Current.PredictSubs = load.PredictSubs;

						if (build != null)
						{
							var beforeScore = build.CalcScore(dmon.GetStats());
							var afterScore = build.CalcScore(load.GetStats(mon));
							groupBox1.Controls.Find("PtscompBefore", false).FirstOrDefault().Text = beforeScore.ToString("0.##");
							groupBox1.Controls.Find("PtscompAfter", false).FirstOrDefault().Text = afterScore.ToString("0.##");
							groupBox1.Controls.Find("PtscompDiff", false).FirstOrDefault().Text = (afterScore - beforeScore).ToString("0.##");
						}
						ShowDiff(dmon.GetStats(), load.GetStats(mon), build);

						dmon.Current.Leader = dmonld;
						dmon.Current.Shrines = dmonsh;
						dmon.Current.FakeLevel = dmonfl;
						dmon.Current.PredictSubs = dmonps;

					}
				}
			}
			lastFocused = loadoutList.FocusedItem;
			
			int cost = 0;
			foreach (ListViewItem li in loadoutList.SelectedItems)
			{
				Loadout load = li.Tag as Loadout;
				if (load != null)
				{
					var mon = Program.data.GetMonster(ulong.Parse(li.SubItems[2].Text));
					if (mon != null)
						cost += mon.SwapCost(load);
				}
			}
			toolStripStatusLabel2.Text = "Unequip: " + cost.ToString();
		}

		private void tsBtnLoadsClear_Click(object sender, EventArgs e)
		{
			ClearLoadouts();
		}

		private void toolStripButton7_Click(object sender, EventArgs e)
		{
			if (dataMonsterList.SelectedItems.Count <= 0) return;
			var mon = dataMonsterList.SelectedItems[0].Tag as Monster;
			if (mon == null)
				return;

			var nextId = 1;
			if (Program.builds.Any())
				nextId = Program.builds.Max(q => q.ID) + 1;

			Build bb = new Build((Monster)dataMonsterList.SelectedItems[0].Tag)
			{
				New = true,
				ID = nextId,
				MonId = mon.Id,
				MonName = mon.FullName
			};

			if (Program.Settings.ShowBuildWizard)
			{
				using (var bwiz = new BuildWizard(bb))
				{
					var res = bwiz.ShowDialog();
					if (res != DialogResult.OK) return;
				}
			}

			using (var ff = new Create(bb))
			{
				while (Program.builds.Any(b => b.ID == bb.ID))
				{
					bb.ID++;
				}
				
				var res = ff.ShowDialog();
				if (res != DialogResult.OK) return;

				if (Program.builds.Count > 0)
					bb.priority = Program.builds.Max(b => b.priority) + 1;
				else
					bb.priority = 1;
				
				ListViewItem li = new ListViewItem(new string[] { bb.priority.ToString(), bb.ID.ToString(), bb.mon.FullName, "", bb.mon.Id.ToString(), "" });
				li.Tag = bb;
				Program.builds.Add(bb);

				var lv1li = dataMonsterList.Items.Cast<ListViewItem>().FirstOrDefault(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Any(s => s.Text == bb.mon.FullName));
				if (lv1li != null)
					lv1li.ForeColor = Color.Green;
			}
		}

		private void listView5_DoubleClick(object sender, EventArgs e)
		{
			var items = buildList.SelectedItems;
			if (items.Count > 0)
			{
				var item = items[0];
				if (item.Tag != null)
				{
					Build bb = (Build)item.Tag;
					Monster before = bb.mon;
					using (var ff = new Create(bb))
					{
						var res = ff.ShowDialog();
						if (res == DialogResult.OK)
						{
							item.SubItems[2].Text = bb.mon.FullName;
							item.SubItems[4].Text = bb.mon.Id.ToString();
							item.ForeColor = bb.runePrediction.Any(p => p.Value.Value) ? Color.Purple : Color.Black;
							if (bb.mon != before)
							{
								// TODO: check tag?
								var lv1li = dataMonsterList.Items.Cast<ListViewItem>().FirstOrDefault(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Any(s => s.Text == before.FullName));
								if (lv1li != null)
									lv1li.ForeColor = before.inStorage ? Color.Gray : Color.Black;

								lv1li = dataMonsterList.Items.Cast<ListViewItem>().FirstOrDefault(i => i.SubItems.Cast<ListViewItem.ListViewSubItem>().Any(s => s.Text == ff.build.mon.FullName));
								if (lv1li != null)
									lv1li.ForeColor = Color.Green;

							}
						}
					}
				}
			}
		}

		private void tsBtnBuildsRunOne_Click(object sender, EventArgs e)
		{
			plsDie = false;
			var lis = buildList.SelectedItems;
			if (lis.Count > 0)
			{
				var li = lis[0];
				RunBuild(li, Program.Settings.MakeStats);
			}
		}

		private void tsBtnBuildsSave_Click(object sender, EventArgs e)
		{
			Program.SaveBuilds();
		}

		private void tsBtnBuildsRemove_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("This will delete a build (not a loadout)." + Environment.NewLine + "How many minutes of work will be undone?", "Delete Build?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				return;

			var lis = buildList.SelectedItems;
			if (lis.Count > 0)
			{
				foreach (ListViewItem li in lis)
				{
					Build b = (Build)li.Tag;
					if (b != null)
					{
						Program.builds.Remove(b);
					}
				}
			}
		}

		private void tsBtnBuildsUnlock_Click(object sender, EventArgs e)
		{
			if (Program.data == null || Program.data.Runes == null)
				return;
			foreach (Rune r in Program.data.Runes)
			{
				r.Locked = false;
			}
			checkLocked();
		}

		private void tsBtnLoadsRemove_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem li in loadoutList.SelectedItems)
			{
				Loadout l = (Loadout)li.Tag;

				Program.loads.Remove(l);
				//loadoutList.Items.Remove(li);
			}
			checkLocked();
		}

		public void ProgressToList(Build b, string str)
		{
			Program.log.Info("_" + str);
			this.BeginInvoke((MethodInvoker)delegate
			{
				if (!IsDisposed)
				{
					var lvi = buildList.Items.Cast<ListViewItem>().FirstOrDefault(ll => (ll.Tag as Build)?.ID == b.ID);
					if (lvi == null)
						return;
					while (lvi.SubItems.Count < 4)
						lvi.SubItems.Add("");
					lvi.SubItems[3].Text = str;
				}
			});
		}

		private void tsBtnBuildsRunAll_Click(object sender, EventArgs e)
		{
			Program.RunBuilds(false, -1);
		}

		private DialogResult CheckSaveChanges()
		{
			if (!Program.data.isModified)
				return DialogResult.Yes;
					 
			var res = MessageBox.Show("Would you like to save changes to your imported data?", "Save Data", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (res == DialogResult.Yes)
				Program.SaveData();
			return res;
		}

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (CheckSaveChanges() == DialogResult.Cancel) {
				e.Cancel = true;
				return;
			}
			Program.BuildsProgressTo -= Program_BuildsProgressTo;
			Program.SaveBuilds();
		}

		private void toolStripButton15_Click(object sender, EventArgs e)
		{
			if (Program.data == null)
				return;

			if (Program.data.Monsters != null)
				foreach (Monster mon in Program.data.Monsters)
				{
					for (int i = 1; i < 7; i++) {
						var r = mon.Current.RemoveRune(i);
						if (r != null) {
							r.Assigned = null;
							r.AssignedId = 0;
							r.AssignedName = "";
						}
					}
				}

			if (Program.data.Runes != null)
				foreach (Rune r in Program.data.Runes)
				{
					r.AssignedId = 0;
					r.AssignedName = "Inventory";
				}
		}

		private void toolStripButton14_Click(object sender, EventArgs e)
		{
			if (File.Exists(Program.Settings.SaveLocation))
			{
				Program.LoadSave(Program.Settings.SaveLocation);
				RegenLists();
			}
		}

		private void tsBtnBuildsMoveUp_Click(object sender, EventArgs e)
		{
			if (buildList.SelectedItems.Count > 0)
			{
				foreach (ListViewItem sli in buildList.SelectedItems.Cast<ListViewItem>().Where(l => l.Tag != null).OrderBy(l => (l.Tag as Build).priority))
				{
					Build build = sli.Tag as Build;
					if (build != null)
						Program.BuildPriority(build, -1);
				}

				RegenBuildList();

				buildList.Sort();
			}
		}

		private void RegenBuildList()
		{
			foreach (var lvi in buildList.Items.Cast<ListViewItem>())
			{
				var b = lvi.Tag as Build;
				if (b != null)
				{
					lvi.SubItems[0].Text = b.priority.ToString();
				}
			}
		}

		private void tsBtnBuildsMoveDown_Click(object sender, EventArgs e)
		{
			if (buildList.SelectedItems.Count > 0)
			{
				foreach (ListViewItem sli in buildList.SelectedItems.Cast<ListViewItem>().Where(l => l.Tag != null).OrderByDescending(l => (l.Tag as Build).priority))
				{
					Build build = sli.Tag as Build;
					if (build != null)
						Program.BuildPriority(build, 1);
				}

				RegenBuildList();

				buildList.Sort();
			}
		}

		private void useRunesCheck_CheckedChanged(object sender, EventArgs e)
		{
			Program.Settings.UseEquipped = ((CheckBox)sender).Checked;
			Program.Settings.Save();
		}

		void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
		{
			Invoke((MethodInvoker)delegate
			{
				updateBox.Visible = true;
				try
				{
					string result = e.Result.Replace("\r\n", "\n");
					int firstline = result.IndexOf('\n');

					string newvernum = result;
					if (firstline != -1)
						newvernum = newvernum.Substring(0, firstline);

					Console.WriteLine(newvernum);
					int ind1 = result.IndexOf('\n');

					if (result.IndexOf('\n') != -1)
					{
						int ind2 = result.IndexOf('\n', ind1 + 1);
						if (ind2 == -1)
							filelink = e.Result.Substring(ind1 + 1);
						else
						{
							filelink = e.Result.Substring(ind1 + 1, ind2 - ind1 - 1);
							whatsNewText = e.Result.Substring(ind2 + 1);
						}

					}

					var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
					string oldvernum = ver.ProductVersion;
					updateCurrent.Text = "Current: " + oldvernum;
					updateNew.Text = "New: " + newvernum;
					int newver = VersionCompare(oldvernum, newvernum);
					if (newver > 0)
					{
						updateComplain.Text = "You hacker";
					}
					else if (newver < 0)
					{
						updateComplain.Text = "Update available!";
						if (filelink != "")
						{
							updateDownload.Enabled = true;
							if (whatsNewText != "")
								updateWhat.Visible = true;
						}
					}
					else
					{
						updateComplain.Visible = false;
					}
				}
				catch (Exception ex)
				{
					updateComplain.Text = e.Error.ToString();
					updateComplain.Visible = false;
					Console.WriteLine(ex);
				}
			});
		}

		private void updateDownload_Click(object sender, EventArgs e)
		{
			if (filelink != "")
			{
				Process.Start(new Uri(filelink).ToString());
			}
		}

		private void updateWhat_Click(object sender, EventArgs e)
		{
			if (whatsNewText != "")
			{
				MessageBox.Show(whatsNewText, "What's New");
			}
		}

		private void tsBtnRuneStats_Click(object sender, EventArgs e)
		{
			Program.runeSheet.StatsExcelRunes(false);
		}

		public void ShowOptions() {
			using (var f = new Options()) {
				f.ShowDialog();
				findGoodRunes.Enabled = Program.Settings.MakeStats;
				if (!Program.Settings.MakeStats)
					findGoodRunes.Checked = false;
			}
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowOptions();
		}

		private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateBox.Show();
			updateComplain.Text = "Checking...";
			Task.Factory.StartNew(() =>
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadStringCompleted += client_DownloadStringCompleted;
					client.DownloadStringAsync(new Uri("https://raw.github.com/Skibisky/RuneManager/master/version.txt"));
				}
			});
		}

		private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			var ver = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
			string oldvernum = ver.ProductVersion;

			MessageBox.Show("Rune Manager\r\nBy Skibisky\r\nVersion " + oldvernum, "About", MessageBoxButtons.OK);
		}

		private void userManualHelpToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenHelp();
		}

		private void runelistSwapLocked(object sender, EventArgs e)
		{
			// swap the selected runes locked state
			foreach (ListViewItem li in dataRuneList.SelectedItems)
			{
				var rune = li.Tag as Rune;
				if (rune != null)
				{
					if (rune.Locked)
					{
						rune.Locked = false;
						li.BackColor = Color.Transparent;
					}
					else
					{
						rune.Locked = true;
						li.BackColor = Color.Red;
					}
				}
			}
		}

		private void runetab_savebutton_click(object sender, EventArgs e)
		{
			Program.SaveData();
		}

		private void unequipMonsterButton_Click(object sender, EventArgs e)
		{
			if (Program.data?.Monsters == null)
				return;

			foreach (ListViewItem li in dataMonsterList.SelectedItems)
			{
				Monster mon = li.Tag as Monster;
				if (mon == null)
					continue;

				for (int i = 1; i < 7; i++)
				{
					var r = mon.Current.RemoveRune(i);
					if (r == null)
						continue;

					r.AssignedId = 0;
					r.Assigned = null;
					r.AssignedName = "Inventory";
				}
			}
		}

		private void tsBtnLoadsLock_Click(object sender, EventArgs e)
		{
			foreach (ListViewItem li in loadoutList.SelectedItems)
			{
				Loadout l = (Loadout)li.Tag;

				foreach (Rune r in l.Runes)
				{
					r.Locked = true;
				}
				checkLocked();
			}
		}

		private void tsBtnBuildsResume_Click(object sender, EventArgs e)
		{
			Program.RunBuilds(true, -1);
		}

		private void tsBtnBuildsRunUpTo_Click(object sender, EventArgs e)
		{
			int selected = -1;
			if (buildList.SelectedItems.Count > 0)
				selected = (buildList.SelectedItems[0].Tag as Build).priority;
			Program.RunBuilds(true, selected);
		}

		static int VersionCompare(string v1, string v2)
		{
			Version ver1;
			Version ver2;
			if (!Version.TryParse(v1, out ver1))
				return 1;
			if (!Version.TryParse(v2, out ver2))
				return -1;

			return ver1.CompareTo(ver2);
		}

		public void OpenHelp(string url = null, Form owner = null)
		{
			if (help == null || help.IsDisposed)
				help = new Help();
			help.url = url;

			if (!help.Visible)
				help.Show(owner ?? this);

			var xx = Location.X + 1105 + 8 - 271;//271, 213
			var yy = Location.Y + 49 + 208 - 213;// 8, 208 1105, 49
			help.Height = this.Height;
			help.Location = new Point(xx, yy);
			help.Location = new Point(Location.X + Width, Location.Y);
		}

		private void ShowMon(Monster mon)
		{
			displayMon = mon;
			var cur = mon.GetStats();

			statName.Text = mon.FullName;
			statID.Text = mon.Id.ToString();
			statLevel.Text = mon.level.ToString();

			ShowStats(cur, mon);
			ShowLoadout(mon.Current);

			var fname = Environment.CurrentDirectory.Replace("\\", "/") + "/data/unit/" + Program.GetMonIconName(mon.monsterTypeId) + ".png";
			if (File.Exists(fname))
				monImage.ImageLocation = fname;
			else
				monImage.Image = RuneApp.InternalServer.InternalServer.mon_spot;
		}

		private void ShowLoadout(Loadout l)
		{
			runeDial.Loadout = l;

			if (runeDisplay != null && !runeDisplay.IsDisposed)
				runeDisplay.UpdateLoad(l);
		}

		private static string[] statCtrlNames = {"", "HP", "", "ATK", "", "DEF", "", "", "SPD", "CR", "CD", "RES", "ACC" };
		private static Control[] statCtrls = new Control[37];

		private void ShowStats(Stats cur, Stats mon)
		{
			foreach (Attr a in new Attr[] { Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat, Attr.Speed, Attr.CritRate, Attr.CritDamage, Attr.Resistance, Attr.Accuracy })
			{
				if (statCtrls[(int)a] == null)
					statCtrls[(int)a] = groupBox1.Controls.Find(statCtrlNames[(int)a] + "Base", false).FirstOrDefault();
				statCtrls[(int)a].Text = mon[a] + (((int)a) > 8 ? "%" : "");

				if (statCtrls[12 + (int)a] == null)
					statCtrls[12 + (int)a] = groupBox1.Controls.Find(statCtrlNames[(int)a] + "Total", false).FirstOrDefault();
				statCtrls[12 + (int)a].Text = cur[a] + (((int)a) > 8 ? "%" : "");

				if (statCtrls[24 + (int)a] == null)
					statCtrls[24 + (int)a] = groupBox1.Controls.Find(statCtrlNames[(int)a] + "Bonus", false).FirstOrDefault();
				statCtrls[24 + (int)a].Text = "+" + (cur[a] - mon[a]);
			}
		}

		private void ShowDiff(Stats old, Stats load, Build build = null)
		{
			foreach (string stat in new string[] { "HP", "ATK", "DEF", "SPD" })
			{
				groupBox1.Controls.Find(stat + "compBefore", false).FirstOrDefault().Text = old[stat].ToString();
				groupBox1.Controls.Find(stat + "compAfter", false).FirstOrDefault().Text = load[stat].ToString();
				string pts = (load[stat] - old[stat]).ToString();
				if (build != null && !build.Sort[stat].EqualTo(0))
				{
					pts += " (" + ((load[stat] - old[stat]) / build.Sort[stat]).ToString("0.##") + ")";
				}
				groupBox1.Controls.Find(stat + "compDiff", false).FirstOrDefault().Text =  pts;
			}

			foreach (string stat in new string[] { "CR", "CD", "RES", "ACC" })
			{
				groupBox1.Controls.Find(stat + "compBefore", false).FirstOrDefault().Text = old[stat].ToString() + "%";
				groupBox1.Controls.Find(stat + "compAfter", false).FirstOrDefault().Text = load[stat].ToString() + "%";
				string pts = (load[stat] - old[stat]).ToString();
				if (build != null && !build.Sort[stat].EqualTo(0))
				{
					pts += " (" + ((load[stat] - old[stat]) / build.Sort[stat]).ToString("0.##") + ")";
				}
				groupBox1.Controls.Find(stat + "compDiff", false).FirstOrDefault().Text = pts;
			}

			foreach (string extra in Build.extraNames)
			{
				groupBox1.Controls.Find(extra + "compBefore", false).FirstOrDefault().Text = old.ExtraValue(extra).ToString("0");
				groupBox1.Controls.Find(extra + "compAfter", false).FirstOrDefault().Text = load.ExtraValue(extra).ToString("0");
				string pts = (load.ExtraValue(extra) - old.ExtraValue(extra)).ToString("0");
				if (build != null && !build.Sort.ExtraGet(extra).EqualTo(0))
				{
					var aa = load.ExtraValue(extra);
					var cc = old.ExtraValue(extra);
					var ss = build.Sort.ExtraGet(extra);

					pts += " (" + ((aa - cc) / ss).ToString("0.##") + ")";
				}
				
				groupBox1.Controls.Find(extra + "compDiff", false).FirstOrDefault().Text = pts;

			}

			for (int i = 0; i < 4;i++) {
				var stat = "Skill" + (i+1);
				groupBox1.Controls.Find(stat + "compBefore", false).FirstOrDefault().Text = old.GetSkillDamage(Attr.AverageDamage, i).ToString("0.##");
				groupBox1.Controls.Find(stat + "compAfter", false).FirstOrDefault().Text = load.GetSkillDamage(Attr.AverageDamage, i).ToString("0.##");
				string pts = (load.GetSkillDamage(Attr.AverageDamage, i) - old.GetSkillDamage(Attr.AverageDamage, i)).ToString("0.##");
				if (build != null && !build.Sort.DamageSkillups[i].EqualTo(0)) {
					pts += " (" + ((load.GetSkillDamage(Attr.AverageDamage, i) - old.GetSkillDamage(Attr.AverageDamage, i)) / build.Sort.DamageSkillups[i]).ToString("0.##") + ")";
				}
				groupBox1.Controls.Find(stat + "compDiff", false).FirstOrDefault().Text = pts;
			}
		}
		
		public void RegenLists()
		{
			// TODO: comment it up a little?
			((ListViewSort)dataMonsterList.ListViewItemSorter).ShouldSort = false;
			((ListViewSort)dataRuneList.ListViewItemSorter).ShouldSort = false;
			
			dataMonsterList.Items.Clear();
			dataRuneList.Items.Clear();
			listView4.Items.Clear();
			if (Program.data == null)
				return;
			int maxPri = 0;
			if (Program.builds.Count > 0)
				maxPri = Program.builds.Max(b => b.priority) + 1;
			foreach (var mon in Program.data.Monsters) {
				mon.priority = (Program.builds.FirstOrDefault(b => b.mon == mon)?.priority) ?? (mon.Current.RuneCount > 0 ? (maxPri++) : 0);
			}
			dataMonsterList.Items.AddRange(Program.data.Monsters.Select(mon => ListViewItemMonster(mon)).ToArray());

			dataCraftList.Items.AddRange(Program.data.Crafts.Select(craft => new ListViewItem()
			{
				Text = craft.ItemId.ToString(),
				SubItems =
				{
					craft.Set.ToString(),
					craft.Stat.ToString(),
					craft.Rarity.ToString(),
					craft.Type.ToString(),
				}
			}).ToArray());

			foreach (Rune rune in Program.data.Runes) {
				dataRuneList.Items.Add(ListViewItemRune(rune));
			}
			checkLocked();
			ColorMonsWithBuilds();

			var mlvs = (ListViewSort)dataMonsterList.ListViewItemSorter;
			mlvs.ShouldSort = true;
			mlvs.OrderBy(colMonGrade.Index, false);
			mlvs.ThenBy(colMonPriority.Index, true);
			((ListViewSort)dataRuneList.ListViewItemSorter).ShouldSort = true;

			dataMonsterList.Sort();
			dataRuneList.Sort();
		}

		public void checkLocked()
		{
			if (Program.data?.Runes == null)
				return;

			Invoke((MethodInvoker)delegate
			{
				toolStripStatusLabel1.Text = "Locked: " + Program.data.Runes.Count(r => r.Locked);
				foreach (ListViewItem li in dataRuneList.Items)
				{
					var rune = li.Tag as Rune;
					if (rune != null)
						li.BackColor = rune.Locked ? Color.Red : Color.Transparent;
				}
			});
		}

		private void RunBuild(ListViewItem pli, bool saveStats = false)
		{
			if (pli?.Tag is Build)
				Program.RunBuild((Build)pli.Tag, saveStats);
		}
		
		private void ClearLoadouts()
		{
			Program.ClearLoadouts();
			checkLocked();
		}
		
		private void buildList_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				teamBuild = null;
				if (buildList.FocusedItem.Bounds.Contains(e.Location))
				{
					if (buildList.FocusedItem.Tag == null)
						return;

					teamBuild = buildList.FocusedItem.Tag as Build;

					teamChecking = true;

					foreach (ToolStripMenuItem tsmi in teamToolStripMenuItem.DropDownItems)
					{
						tsmi.Image = null;
						if (tsTeamCheck(tsmi))
							tsmi.Image = App.add;
					}
					teamChecking = false;

					menu_buildlist.Show(Cursor.Position);
				}
			}
		}

		private bool tsTeamCheck(ToolStripMenuItem t)
		{
			bool ret = false;
			t.Checked = false;
			t.Image = null;
			if (teamBuild.Teams.Contains(t.Text))
			{
				t.Checked = true;
				ret = true;
			}
			foreach (ToolStripMenuItem smi in t.DropDownItems)
			{
				if (tsTeamCheck(smi))
				{
					t.Image = App.add;
					ret = true;
				}
			}
			return ret;
		}

		int GetRel(string first, string second)
		{
			if (first == second)
				return 0;

			string p1 = null;
			string p2 = null;

			if (toolmap.Keys.Contains(first) && toolmap.Keys.Contains(second))
				return 1;

			foreach (var k in toolmap)
			{
				if (k.Value.Contains(first) && k.Value.Contains(second))
					return 1;
				if (k.Value.Contains(first))
					p1 = k.Key;
				if (k.Value.Contains(second))
					p2 = k.Key;
			}

			if (toolmap.Keys.Contains(first) && toolmap[first].Contains(second))
				return 1;
			if (toolmap.Keys.Contains(second) && toolmap[second].Contains(first))
				return 1;

			if (p2 != null && toolmap[p2].Contains(p1))
				return 2;
			if (p1 != null && toolmap[p1].Contains(p2))
				return 2;

			if (p2 != null && toolmap[p2].Contains(first))
				return 3;
			if (p1 != null && toolmap[p1].Contains(second))
				return 3;

			return -1;
		}

		private void buildList_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool doColor = buildList.SelectedItems.Count == 1;
			var b1 = doColor ? buildList.SelectedItems[0].Tag as Build : null;

			foreach (ListViewItem li in buildList.Items)
			{
				if (Program.Settings.ColorTeams && b1 != null && b1.Teams.Count > 0)
				{
					li.BackColor = Color.White;
					var b2 = li.Tag as Build;
					if (b2 != null && b2.Teams.Count > 0)
					{
						int close = -1;
						foreach (var t1 in b1.Teams)
						{
							foreach (var t2 in b2.Teams)
							{
								var c = GetRel(t1, t2);
								if (c != -1)
									close = close == -1 ? c : (c < close ? c : close);
								if (close == 0)
									break;
							}
							if (close == 0)
								break;
						}
						if (close == 0)
							li.BackColor = Color.Lime;
						else if (close == 1)
							li.BackColor = Color.LightGreen;
						else if (close == 2)
							li.BackColor = Color.DimGray;
						else if (close == 3)
							li.BackColor = Color.LightGray;
					}
				}
			}
		}

		private void tsBtnLoadsSave_Click(object sender, EventArgs e)
		{
			Program.SaveLoadouts();
		}

		private void tsBtnLoadsLoad_Click(object sender, EventArgs e)
		{
			Program.LoadLoadouts();
			checkLocked();
		}

		private void pictureBox1_DoubleClick(object sender, EventArgs e)
		{
			if (runeDisplay == null || runeDisplay.IsDisposed)
				runeDisplay = new RuneDisplay();
			if (!runeDisplay.Visible)
			{
				runeDisplay.Show(this);
				var xx = Location.X + 1105 + 8 - 271;//271, 213
				var yy = Location.Y + 49 + 208 - 213;// 8, 208 1105, 49

				runeDisplay.Location = new Point(xx, yy);
				runeDisplay.Location = new Point(Location.X + Width, Location.Y);
			}
			if (displayMon != null)
				runeDisplay.UpdateLoad(displayMon.Current);
		}

		private void findGoodRunes_CheckedChanged(object sender, EventArgs e)
		{
			if (findGoodRunes.Checked && MessageBox.Show("This runs each test multiple times.\r\nThat means leaving it overnight or something.", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
			{
				goodRunes = true;
			}
			else
				findGoodRunes.Checked = false;
			goodRunes = findGoodRunes.Checked;
		}

		private void tsBtnFindSpeed_Click(object sender, EventArgs e)
		{
			foreach (var li in buildList.Items.Cast<ListViewItem>())
			{
				var b = li.Tag as Build;
				if (b != null)
				{
					b.RunesUseLocked = false;
					b.RunesUseEquipped = Program.Settings.UseEquipped;
					b.BuildSaveStats = false;
					b.GenRunes(Program.data);
					long c = b.runes[0].Length;
					c *= b.runes[1].Length;
					c *= b.runes[2].Length;
					c *= b.runes[3].Length;
					c *= b.runes[4].Length;
					c *= b.runes[5].Length;

					// I get about 4mil/sec, and can be bothered to wait 10 - 25 seconds
					li.SubItems[0].ForeColor = Color.Black;
					if (c > 1000 * 1000 * (long)1000 * 100) {
						li.SubItems[0].ForeColor = Color.DarkBlue;
						li.SubItems[0].BackColor = Color.Red;
					}
					else if (c > 1000 * 1000 * (long)1000 * 10) {
						li.SubItems[0].ForeColor = Color.DarkBlue;
						li.SubItems[0].BackColor = Color.OrangeRed;
					}
					else if (c > 1000 * 1000 * (long)1000 * 1) {
						li.SubItems[0].ForeColor = Color.DarkBlue;
						li.SubItems[0].BackColor = Color.Orange;
					}
					else if (c > 4000000 * 25)
						li.SubItems[0].ForeColor = Color.Red;
					else if (c > 4000000 * 10)
						li.SubItems[0].ForeColor = Color.Orange;
				}
			}
		}

		private void btnRefreshSave_Click(object sender, EventArgs e)
		{
			Program.LoadSave(Program.Settings.SaveLocation);
		}

		private void buildList_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
		{
			if (e.ColumnIndex == buildCHTeams.Index)
			{
				foreach (var lvi in buildList.Items.Cast<ListViewItem>())
				{
					if (lvi.SubItems.Count > buildCHTeams.Index)
						lvi.SubItems[buildCHTeams.Index].Text = getTeamStr(lvi.Tag as Build);
				}
			}
		}
	}

}