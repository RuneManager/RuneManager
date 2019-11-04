using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using RuneOptim;
using RuneOptim.BuildProcessing;
using RuneOptim.swar;

namespace RuneApp {
	partial class Create {
		// TODO: generate these only when showing the tab :P
		private void genTabRuneGrid(ref int x, ref int y, ref int colWidth, ref int rowHeight) {
			Label label;
			Control textBox;
			// put the grid on all the tabs
			for (int t = 0; t < tabNames.Length; t++) {
				var tab = tabNames[t];
				TabPage page = tabControl1.TabPages["tab" + tab];
				page.Tag = tab;

				page.Controls.MakeControl<Label>(tab, "divprompt", 6, 6, 140, 14, "Divide stats into points");
				page.Controls.MakeControl<Label>(tab, "inhprompt", 60, 14, 134, 6, "Inherited");
				page.Controls.MakeControl<Label>(tab, "curprompt", 60, 14, 214, 6, "Current");

				ComboBox filterJoin = new ComboBox();
				filterJoin.DropDownStyle = ComboBoxStyle.DropDownList;
				filterJoin.FormattingEnabled = true;
				filterJoin.Items.AddRange(Enum.GetValues(typeof(FilterType)).OfType<FilterType>().OrderBy(f => f).OfType<object>().ToArray());
				filterJoin.Location = new Point(298, 6);
				filterJoin.Name = tab + "join";
				filterJoin.Size = new Size(72, 21);
				FilterType filter = FilterType.None;
				if (build.RuneScoring.ContainsKey(tabIndexes[t]))
					filter = build.RuneScoring[tabIndexes[t]].Key;
				filterJoin.SelectedItem = filter;

				filterJoin.SelectionChangeCommitted += filterJoin_SelectedIndexChanged;
				filterJoin.Tag = tab;
				page.Controls.Add(filterJoin);


				rowHeight = 25;
				colWidth = 42;

				bool first = true;
				int predX = 0;
				y = 45;
				foreach (var stat in Build.StatNames) {
					page.Controls.MakeControl<Label>(tab, stat, 5, y, 30, 20, stat);

					x = 35;
					foreach (var pref in new string[] { "", "i", "c" }) {
						foreach (var type in new string[] { "flat", "perc" }) {
							if (first) {
								label = page.Controls.MakeControl<Label>(tab + pref, type, x, 25, 45, 16, pref + type);
								if (type == "flat")
									label.Text = "Flat";
								if (type == "perc")
									label.Text = "Percent";
							}

							if (type == "perc" && stat == "SPD") {
								x += colWidth;
								continue;
							}
							if (type == "flat" && (stat == "ACC" || stat == "RES" || stat == "CD" || stat == "CR")) {
								x += colWidth;
								continue;
							}

							if (pref == "") {
								textBox = page.Controls.MakeControl<TextBox>(tab + pref + stat, type, x, y - 2);
								textBox.TextChanged += global_TextChanged;
							}
							else
								page.Controls.MakeControl<Label>(tab + pref + stat, type, x, y);

							x += colWidth;
						}
					}

					predX = x;

					page.Controls.MakeControl<Label>(tab + stat, "gt", x, y, 30, 20, ">=");
					x += colWidth;

					textBox = page.Controls.MakeControl<TextBox>(tab + stat, "test", x, y - 2);
					textBox.TextChanged += global_TextChanged;
					textBox.Enabled = RuneAttributeFilterEnabled(filter);
					x += colWidth;

					page.Controls.MakeControl<Label>(tab + "r", stat + "test", x, y, 30, 20, ">=");
					x += colWidth;

					y += rowHeight;
					first = false;
				}

				y += 8;
				x = predX;

				page.Controls.MakeControl<Label>(tab, "testGt", x - 3, y, 45, 20, "Sum >=");
				x += colWidth;

				textBox = page.Controls.MakeControl<TextBox>(tab, "test", x, y - 2);
				textBox.TextChanged += global_TextChanged;
				// default scoring is OR which doesn't need this box
				textBox.Enabled = RuneSumFilterEnabled(filter);
				x += colWidth;

				page.Controls.MakeControl<Label>(tab, "Check", x, y, 60, 14);

				x = predX;
				y += rowHeight + 8;

				page.Controls.MakeControl<Label>(tab, "raiseLabel", x, y, text: "Make+");
				x += colWidth;

				textBox = page.Controls.MakeControl<TextBox>(tab, "raise", x, y - 2);
				textBox.TextChanged += global_TextChanged;
				x += colWidth;

				page.Controls.MakeControl<Label>(tab, "raiseInherit", x, y, text: "0");
				x += colWidth;

				x = predX;
				y += rowHeight;

				CheckBox check = page.Controls.MakeControl<CheckBox>(tab, "bonus", x, y, 17, 17);
				check.Checked = false;
				check.CheckedChanged += global_CheckChanged;
				x += 17;

				label = page.Controls.MakeControl<Label>(tab, "bonusLabel", x, y, 67, 20, "Predict Subs");
				label.Click += (s, e) => { check.Checked = !check.Checked; };
				x += colWidth;

				x += colWidth - 17;

				page.Controls.MakeControl<Label>(tab, "bonusInherit", x, y, 80, 20, "FT");
				x += colWidth;
			}

		}

		private void addAttrsToEvens() {
			// there are lists on the rune filter tabs 2,4, and 6
			foreach (var lv in new ListView[] { priStat2, priStat4, priStat6 }) {
				// mess 'em up
				ListViewItem li;

				addRuneMainToList(lv, "HP", "flat");
				li = addRuneMainToList(lv, "HP", "perc");
				li.Text = "HP%";
				addRuneMainToList(lv, "ATK", "flat");
				li = addRuneMainToList(lv, "ATK", "perc");
				li.Text = "ATK%";
				addRuneMainToList(lv, "DEF", "flat");
				li = addRuneMainToList(lv, "DEF", "perc");
				li.Text = "DEF%";

				if (lv == priStat2)
					addRuneMainToList(lv, "SPD", "flat");
				if (lv == priStat4) {
					addRuneMainToList(lv, "CR", "perc");
					addRuneMainToList(lv, "CD", "perc");
				}
				if (lv == priStat6) {
					addRuneMainToList(lv, "RES", "perc");
					addRuneMainToList(lv, "ACC", "perc");
				}
			}
		}

		private static ListViewItem addRuneMainToList(ListView lv, string stat, string suff) {
			ListViewItem li = new ListViewItem(stat);
			// put the right type on it
			li.Name = stat + suff;
			li.Text = stat;
			li.Tag = stat + suff;
			li.Group = lv.Groups[1];

			lv.Items.Add(li);
			return li;
		}

		private TabPage GetTab(string tabName) {
			// figure out which tab it's on
			int tabId;
			if (!int.TryParse(tabName, out tabId)) {
				switch (tabName) {
					case "g":
					case "Global":
						tabId = 0;
						break;
					case "e":
					case "Even":
						tabId = -2;
						break;
					case "o":
					case "Odd":
						tabId = -1;
						break;
					case "One":
						tabId = 1;
						break;
					case "Two":
						tabId = 2;
						break;
					case "Three":
						tabId = 3;
						break;
					case "Four":
						tabId = 4;
						break;
					case "Five":
						tabId = 5;
						break;
					case "Six":
						tabId = 6;
						break;
					default:
						throw new ArgumentException($"{tabName} is invalid as a tabName");
				}
			}

			TabPage tab = null;
			if (tabId <= 0)
				tab = tabControl1.TabPages[-tabId];
			if (tabId > 0) {
				if (tabId % 2 == 0)
					tab = tabControl1.TabPages[2 + tabId / 2];
				else
					tab = tabControl1.TabPages[6 + tabId / 2];
			}
			return tab;
		}

		private bool RuneAttributeFilterEnabled(FilterType and) {
			switch (and) {
				case FilterType.None:
				case FilterType.Sum:
				case FilterType.SumN:
					return false;
				case FilterType.Or:
				case FilterType.And:
				default:
					return true;
			}
		}

		private bool RuneSumFilterEnabled(FilterType and) {
			switch (and) {
				case FilterType.None:
				case FilterType.Or:
				case FilterType.And:
					return false;
				case FilterType.Sum:
				case FilterType.SumN:
				default:
					return true;
			}
		}

		private void refreshStats(Monster mon, Stats cur) {
			statName.Text = mon.FullName;
			statID.Text = mon.Id.ToString();
			statLevel.Text = mon.level.ToString();

			// read a bunch of numbers
			foreach (var stat in Build.StatNames) {
				var ctrlBase = (Label)groupBox1.Controls.Find(stat + "Base", true).FirstOrDefault();
				ctrlBase.Text = mon[stat].ToString();

				var ctrlBonus = (Label)groupBox1.Controls.Find(stat + "Bonus", true).FirstOrDefault();
				var ctrlTotal = (TextBox)groupBox1.Controls.Find(stat + "Total", true).FirstOrDefault();

				ctrlTotal.Tag = new KeyValuePair<Label, Label>(ctrlBase, ctrlBonus);

				//var ctrlCurrent = groupBox1.Controls.Find(stat + "Current", true).FirstOrDefault();
				Ctrls.Box[stat + "Current"].Text = cur[stat].ToString();

				var ctrlGoal = groupBox1.Controls.Find(stat + "Goal", true).FirstOrDefault();

				var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();

				var ctrlThresh = groupBox1.Controls.Find(stat + "Thresh", true).FirstOrDefault();

				//var ctrlMax = groupBox1.Controls.Find(stat + "Max", true).FirstOrDefault();

				if (build.Minimum[stat] > 0)
					ctrlTotal.Text = build.Minimum[stat].ToString();
				if (!build.Goal[stat].EqualTo(0))
					ctrlGoal.Text = build.Goal[stat].ToString();
				if (!build.Sort[stat].EqualTo(0))
					ctrlWorth.Text = build.Sort[stat].ToString();
				if (!build.Maximum[stat].EqualTo(0))
					Ctrls.Box[stat + "Max"].Text = build.Maximum[stat].ToString();
				if (!build.Threshold[stat].EqualTo(0))
					ctrlThresh.Text = build.Threshold[stat].ToString();

			}

			foreach (var extra in Build.ExtraNames) {
				var ctrlBase = (Label)groupBox1.Controls.Find(extra + "Base", true).FirstOrDefault();
				ctrlBase.Text = mon.ExtraValue(extra).ToString();

				var ctrlBonus = (Label)groupBox1.Controls.Find(extra + "Bonus", true).FirstOrDefault();
				var ctrlTotal = (TextBox)groupBox1.Controls.Find(extra + "Total", true).FirstOrDefault();

				ctrlTotal.Tag = new KeyValuePair<Label, Label>(ctrlBase, ctrlBonus);

				var ctrlCurrent = groupBox1.Controls.Find(extra + "Current", true).FirstOrDefault();
				ctrlCurrent.Text = cur.ExtraValue(extra).ToString();

				var ctrlGoal = groupBox1.Controls.Find(extra + "Goal", true).FirstOrDefault();

				var ctrlWorth = groupBox1.Controls.Find(extra + "Worth", true).FirstOrDefault();

				var ctrlThresh = groupBox1.Controls.Find(extra + "Thresh", true).FirstOrDefault();

				var ctrlMax = groupBox1.Controls.Find(extra + "Max", true).FirstOrDefault();

				if (build.Minimum.ExtraGet(extra) > 0)
					ctrlTotal.Text = build.Minimum.ExtraGet(extra).ToString();
				if (!build.Goal.ExtraGet(extra).EqualTo(0))
					ctrlGoal.Text = build.Goal.ExtraGet(extra).ToString();
				if (!build.Sort.ExtraGet(extra).EqualTo(0))
					ctrlWorth.Text = build.Sort.ExtraGet(extra).ToString();
				if (!build.Maximum.ExtraGet(extra).EqualTo(0))
					ctrlMax.Text = build.Maximum.ExtraGet(extra).ToString();
				if (!build.Threshold.ExtraGet(extra).EqualTo(0))
					ctrlThresh.Text = build.Threshold.ExtraGet(extra).ToString();
			}
			for (int i = 0; i < 4; i++) {
				if (build?.Mon?.SkillFunc?[i] != null) {
					//var ff = build.mon.SkillFunc[i];
					string stat = "Skill" + i;
					Attr aaa = Attr.Skill1 + i;

					double aa = build.Mon.GetSkillDamage(Attr.AverageDamage, i); //ff(build.mon);
					double cc = build.Mon.GetStats().GetSkillDamage(Attr.AverageDamage, i); //ff(build.mon.GetStats());

					var ctrlBase = (Label)groupBox1.Controls.Find(stat + "Base", true).FirstOrDefault();
					ctrlBase.Text = aa.ToString();
					var ctrlBonus = (Label)groupBox1.Controls.Find(stat + "Bonus", true).FirstOrDefault();
					var ctrlTotal = (TextBox)groupBox1.Controls.Find(stat + "Total", true).FirstOrDefault();

					ctrlTotal.Tag = new KeyValuePair<Label, Label>(ctrlBase, ctrlBonus);

					var ctrlCurrent = groupBox1.Controls.Find(stat + "Current", true).FirstOrDefault();
					ctrlCurrent.Text = cc.ToString();

					var ctrlGoal = groupBox1.Controls.Find(stat + "Goal", true).FirstOrDefault();

					var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();

					var ctrlThresh = groupBox1.Controls.Find(stat + "Thresh", true).FirstOrDefault();

					var ctrlMax = groupBox1.Controls.Find(stat + "Max", true).FirstOrDefault();

					if (build.Minimum.DamageSkillups[i] > 0)
						ctrlTotal.Text = build.Minimum.DamageSkillups[i].ToString();
					if (!build.Goal.DamageSkillups[i].EqualTo(0))
						ctrlGoal.Text = build.Goal.DamageSkillups[i].ToString();
					if (!build.Sort.DamageSkillups[i].EqualTo(0))
						ctrlWorth.Text = build.Sort.DamageSkillups[i].ToString();
					if (!build.Maximum.DamageSkillups[i].EqualTo(0))
						ctrlMax.Text = build.Maximum.DamageSkillups[i].ToString();
					if (!build.Threshold.DamageSkillups[i].EqualTo(0))
						ctrlThresh.Text = build.Threshold.DamageSkillups[i].ToString();

				}
			}
		}

		// switch the cool icon on the button (and the bool in the build)
		private void ChangeBroken(bool state) {
			tBtnBreak.Tag = state;
			tBtnBreak.Image = state ? App.broken : App.whole;
			if (build != null)
				build.AllowBroken = state;
		}

		void RegenSetList() {
			if (build == null)
				return;

			foreach (var sl in setList.Items.OfType<ListViewItem>()) {
				var ss = (RuneSet)sl.Tag;
				if (build.RequiredSets.Contains(ss)) {
					sl.Group = setList.Groups[0];
					int num = build.RequiredSets.Count(s => s == ss);
					sl.Text = ss.ToString() + (num > 1 ? " x" + num : "");
					sl.ForeColor = Color.Black;
					if (Rune.SetRequired(ss) == 4 && build.RequiredSets.Any(s => s != ss && Rune.SetRequired(s) == 4))
						sl.ForeColor = Color.Red;
					// TODO: too many twos
				}
				else if (build.BuildSets.Contains(ss)) {
					sl.Group = setList.Groups[1];
					sl.Text = ss.ToString();
					sl.ForeColor = Color.Black;
					if (Rune.SetRequired(ss) == 4 && build.RequiredSets.Any(s => s != ss && Rune.SetRequired(s) == 4))
						sl.ForeColor = Color.Red;
				}
				else if ((Rune.SetRequired(ss) == 2 && build.BuildSets.All(s => Rune.SetRequired(s) == 4) && build.RequiredSets.All(s => Rune.SetRequired(s) == 4))
					|| (Rune.SetRequired(ss) == 4 && build.BuildSets.Count == 0 && build.RequiredSets.Count == 0)
					) {
					sl.Group = setList.Groups[1];
					sl.Text = ss.ToString();
					sl.ForeColor = Color.Gray;
				}
				else {
					sl.Group = setList.Groups[2];
					sl.Text = ss.ToString();
					sl.ForeColor = Color.Black;
				}
			}
			//CalcPerms();
		}

		void UpdateGlobal() {
			// if the window is loading, try not to save the window
			if (loading)
				return;

			foreach (string tab in tabNames) {
				SlotIndex tabdex = ExtensionMethods.GetIndex(tab);
				foreach (string stat in Build.StatNames) {
					UpdateStat(tab, stat);
				}
				if (!build.RuneScoring.ContainsKey(tabdex) && build.RuneFilters.ContainsKey(tabdex)) {
					// if there is a non-zero
					if (build.RuneFilters[tabdex].Any(r => r.Value.NonZero)) {
						build.RuneScoring.Add(tabdex, new KeyValuePair<FilterType, double?>(FilterType.None, null));
					}
				}
				if (build.RuneScoring.ContainsKey(tabdex)) {
					var kv = build.RuneScoring[tabdex];
					var ctrlTest = Controls.Find(tab + "test", true).FirstOrDefault();

					//if (!string.IsNullOrWhiteSpace(ctrlTest?.Text))
					double? testVal = null;
					double tempval;
					if (double.TryParse(ctrlTest?.Text, out tempval))
						testVal = tempval;

					build.RuneScoring[tabdex] = new KeyValuePair<FilterType, double?>(kv.Key, testVal);

				}
				TextBox tb = (TextBox)Controls.Find(tab + "raise", true).FirstOrDefault();
				int raiseLevel;
				if (!int.TryParse(tb.Text, out raiseLevel))
					raiseLevel = -1;
				CheckBox cb = (CheckBox)Controls.Find(tab + "bonus", true).FirstOrDefault();

				if (raiseLevel == -1 && !cb.Checked) {
					build.RunePrediction.Remove(tabdex);
				}
				else
					build.RunePrediction[tabdex] = new KeyValuePair<int?, bool>(raiseLevel, cb.Checked);

				if (build.RunePrediction.ContainsKey(tabdex)) {
					var kv = build.RunePrediction[tabdex];
					var ctrlRaise = Controls.Find(tab + "raiseInherit", true).FirstOrDefault();
					ctrlRaise.Text = kv.Key.ToString();
					var ctrlPred = Controls.Find(tab + "bonusInherit", true).FirstOrDefault();
					ctrlPred.Text = (kv.Value ? "T" : "F");

				}

			}
			foreach (string stat in Build.StatNames) {
				double val;
				double total;
				var ctrlTotal = groupBox1.Controls.Find(stat + "Total", true).FirstOrDefault();
				if (double.TryParse(ctrlTotal.Text, out val))
					total = val;
				build.Minimum[stat] = val;

				var ctrlGoal = groupBox1.Controls.Find(stat + "Goal", true).FirstOrDefault();
				double goal = 0;
				if (double.TryParse(ctrlGoal.Text, out val))
					goal = val;
				build.Goal[stat] = val;

				var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();
				double worth = 0;
				if (double.TryParse(ctrlWorth.Text, out val))
					worth = val;
				build.Sort[stat] = val;

				var ctrlMax = groupBox1.Controls.Find(stat + "Max", true).FirstOrDefault();
				double max = 0;
				if (double.TryParse(ctrlMax.Text, out val))
					max = val;
				build.Maximum[stat] = val;

				//var ctrlThresh = groupBox1.Controls.Find(stat + "Thresh", true).FirstOrDefault();
				double thr = 0;
				if (double.TryParse(Ctrls.Box[stat + "Thresh"].Text, out val))
					thr = val;
				build.Threshold[stat] = val;

				var ctrlCurrent = groupBox1.Controls.Find(stat + "Current", true).FirstOrDefault();
				double current = 0;
				if (double.TryParse(ctrlCurrent.Text, out val))
					current = val;
				var ctrlWorthPts = groupBox1.Controls.Find(stat + "CurrentPts", true).FirstOrDefault();
				if (worth != 0 && current != 0) {
					double pts = current;
					if (goal > 0 && current > goal)
						pts = (current - goal) / 2 + goal;
					if (max != 0)
						pts = Math.Min(max, current);
					ctrlWorthPts.Text = (pts / worth).ToString("0.##");
				}
			}

			foreach (string extra in Build.ExtraNames) {
				var ctrlTotal = groupBox1.Controls.Find(extra + "Total", true).FirstOrDefault();
				double val;
				double total = 0;
				if (double.TryParse(ctrlTotal.Text, out val))
					total = val;
				build.Minimum.ExtraSet(extra, val);

				var ctrlGoal = groupBox1.Controls.Find(extra + "Goal", true).FirstOrDefault();
				double goal = 0;
				if (double.TryParse(ctrlGoal.Text, out val))
					goal = val;
				build.Goal.ExtraSet(extra, val);

				var ctrlWorth = groupBox1.Controls.Find(extra + "Worth", true).FirstOrDefault();
				double worth = 0;
				if (double.TryParse(ctrlWorth.Text, out val))
					worth = val;
				build.Sort.ExtraSet(extra, val);

				var ctrlMax = groupBox1.Controls.Find(extra + "Max", true).FirstOrDefault();
				double max = 0;
				if (double.TryParse(ctrlMax.Text, out val))
					max = val;
				build.Maximum.ExtraSet(extra, val);

				var ctrlThresh = groupBox1.Controls.Find(extra + "Thresh", true).FirstOrDefault();
				double thr = 0;
				if (double.TryParse(ctrlThresh.Text, out val))
					thr = val;
				build.Threshold.ExtraSet(extra, val);

				var ctrlCurrent = groupBox1.Controls.Find(extra + "Current", true).FirstOrDefault();
				double current = 0;
				if (double.TryParse(ctrlCurrent.Text, out val))
					current = val;
				var ctrlWorthPts = groupBox1.Controls.Find(extra + "CurrentPts", true).FirstOrDefault();
				if (worth != 0 && current != 0) {
					double pts = current;
					if (goal > 0 && current > goal)
						pts = (current - goal) / 2 + goal;
					if (max != 0)
						pts = Math.Min(max, current);
					ctrlWorthPts.Text = (pts / worth).ToString("0.##");
				}
				else {
					ctrlWorthPts.Text = "";
				}
			}

			for (int i = 0; i < 4; i++) {
				if (build?.Mon?.SkillFunc?[i] != null) {
					var ff = build.Mon.SkillFunc[i];
					string stat = "Skill" + i;
					Attr aaa = Attr.Skill1 + i;

					var ctrlTotal = groupBox1.Controls.Find(stat + "Total", true).FirstOrDefault();
					double val;
					double total = 0;
					if (double.TryParse(ctrlTotal.Text, out val))
						total = val;
					build.Minimum.ExtraSet(aaa, val);

					var ctrlGoal = groupBox1.Controls.Find(stat + "Goal", true).FirstOrDefault();
					double goal = 0;
					if (double.TryParse(ctrlGoal.Text, out val))
						goal = val;
					build.Goal.ExtraSet(aaa, val);

					var ctrlWorth = groupBox1.Controls.Find(stat + "Worth", true).FirstOrDefault();
					double worth = 0;
					if (double.TryParse(ctrlWorth.Text, out val))
						worth = val;
					build.Sort.ExtraSet(aaa, val);

					var ctrlMax = groupBox1.Controls.Find(stat + "Max", true).FirstOrDefault();
					double max = 0;
					if (double.TryParse(ctrlMax.Text, out val))
						max = val;
					build.Maximum.ExtraSet(aaa, val);

					var ctrlThresh = groupBox1.Controls.Find(stat + "Thresh", true).FirstOrDefault();
					double thr = 0;
					if (double.TryParse(ctrlThresh.Text, out val))
						thr = val;
					build.Threshold.ExtraSet(aaa, val);

					var ctrlCurrent = groupBox1.Controls.Find(stat + "Current", true).FirstOrDefault();
					double current = 0;
					if (double.TryParse(ctrlCurrent.Text, out val))
						current = val;
					var ctrlWorthPts = groupBox1.Controls.Find(stat + "CurrentPts", true).FirstOrDefault();
					if (worth != 0 && current != 0) {
						double pts = current;
						if (goal > 0 && current > goal)
							pts = (current - goal) / 2 + goal;
						if (max != 0)
							pts = Math.Min(max, current);
						ctrlWorthPts.Text = (pts / worth).ToString("0.##");
					}
					else {
						ctrlWorthPts.Text = "";
					}
				}
			}

			var lists = new ListView[] { priStat2, priStat4, priStat6 };
			for (int j = 0; j < lists.Length; j++) {
				var lv = lists[j];
				var bl = build.SlotStats[(j + 1) * 2 - 1];
				bl.Clear();

				for (int i = 0; i < Build.StatNames.Length; i++) {
					string stat = Build.StatNames[i];
					if (i < 3) {
						if (lv.Items.Find(stat + "flat", true).FirstOrDefault().Group == lv.Groups[0])
							bl.Add(stat + "flat");
						if (lv.Items.Find(stat + "perc", true).FirstOrDefault().Group == lv.Groups[0])
							bl.Add(stat + "perc");

					}
					else {
						if (j == 0 && stat != "SPD")
							continue;
						if (j == 1 && (stat != "CR" && stat != "CD"))
							continue;
						if (j == 2 && (stat != "ACC" && stat != "RES"))
							continue;

						if (lv.Items.Find(stat + (stat == "SPD" ? "flat" : "perc"), true).FirstOrDefault().Group == lv.Groups[0])
							bl.Add(stat + (stat == "SPD" ? "flat" : "perc"));
					}
				}
			}

			TestRune(runeTest);

			updatePerms();

			if (testWindow != null && !testWindow.IsDisposed)
				testWindow.textBox_TextChanged(null, null);
		}

		void UpdateStat(string tab, string stat) {
			SlotIndex tabdex = ExtensionMethods.GetIndex(tab);
			TabPage ctab = GetTab(tab);
			var ctest = ctab.Controls.Find(tab + stat + "test", true).First();
			double tt;
			double? test = 0;
			double.TryParse(ctest.Text, out tt);
			test = tt;
			if (ctest.Text.Length == 0)
				test = null;

			if (!build.RuneFilters.ContainsKey(tabdex)) {
				build.RuneFilters.Add(tabdex, new Dictionary<string, RuneFilter>());
			}
			var fd = build.RuneFilters[tabdex];
			if (!fd.ContainsKey(stat)) {
				fd.Add(stat, new RuneFilter());
			}
			var fi = fd[stat];

			foreach (string type in new string[] { "flat", "perc" }) {
				if (type == "perc" && stat == "SPD") {
					continue;
				}
				if (type == "flat" && (stat == "ACC" || stat == "RES" || stat == "CD" || stat == "CR")) {
					continue;
				}

				if (tab == "g")
					ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = "";
				else if (tab == "e" || tab == "o") {
					ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = tabg.Controls.Find("gc" + stat + type, true).First().Text;
				}
				else {
					int s = int.Parse(tab);
					if (s % 2 == 0)
						ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = tabe.Controls.Find("ec" + stat + type, true).First().Text;
					else
						ctab.Controls.Find(tab + "i" + stat + type, true).First().Text = tabo.Controls.Find("oc" + stat + type, true).First().Text;
				}

				var c = ctab.Controls.Find(tab + "c" + stat + type, true).First();

				double i = 0;
				double t = 0;
				var ip = double.TryParse(ctab.Controls.Find(tab + "i" + stat + type, true).First().Text, out i);
				var tp = double.TryParse(ctab.Controls.Find(tab + stat + type, true).First().Text, out t);

				if (ip) {
					c.Text = tp ? t.ToString() : i.ToString();
				}
				else {
					c.Text = tp ? t.ToString() : "";
				}

				fi[type] = t;

				if (ctab.Controls.Find(tab + stat + type, true).First().Text.Length == 0) {
					fi[type] = null;
				}

			}

			fi.Test = test;
		}

		void TestRune(Rune rune) {
			if (rune == null)
				return;

			// consider moving to the slot tab for the rune
			foreach (var tab in tabNames) {
				TestRuneTab(rune, tab);
			}
		}

		double DivCtrl(double val, string tab, string stat, string type) {
			var ctrls = Controls.Find(tab + "c" + stat + type, true);
			if (ctrls.Length == 0)
				return 0;

			var ctrl = ctrls[0];
			double num;
			if (double.TryParse(ctrl.Text, out num)) {
				if (num == 0)
					return 0;

				return val / num;
			}
			return 0;
		}

		bool GetPts(Rune rune, string tab, string stat, ref double points, int fake, bool pred) {
			double pts = 0;

			PropertyInfo[] props = typeof(Rune).GetProperties();
			foreach (var prop in props) {

			}

			pts += DivCtrl(rune[stat + "flat", fake, pred], tab, stat, "flat");
			pts += DivCtrl(rune[stat + "perc", fake, pred], tab, stat, "perc");
			points += pts;

			var lCtrls = Controls.Find(tab + "r" + stat + "test", true);
			var tbCtrls = Controls.Find(tab + stat + "test", true);
			if (lCtrls.Length != 0 && tbCtrls.Length != 0) {
				var tLab = (Label)lCtrls[0];
				var tBox = (TextBox)tbCtrls[0];
				tLab.Text = pts.ToString();
				tLab.ForeColor = Color.Black;
				double vs = 1;
				if (double.TryParse(tBox.Text, out vs)) {
					if (pts >= vs) {
						tLab.ForeColor = Color.Green;
						return true;
					}
					tLab.ForeColor = Color.Red;
					return false;
				}
				return true;
			}

			return false;
		}

		void TestRuneTab(Rune rune, string tab) {
			bool res = false;
			SlotIndex tabdex = ExtensionMethods.GetIndex(tab);
			if (!build.RuneScoring.ContainsKey(tabdex))
				return;

			int? fake = 0;
			bool pred = false;
			if (build.RunePrediction.ContainsKey(tabdex)) {
				fake = build.RunePrediction[tabdex].Key;
				pred = build.RunePrediction[tabdex].Value;
			}

			var kv = build.RuneScoring[tabdex];
			FilterType scoring = kv.Key;
			if (scoring == FilterType.And)
				res = true;

			double points = 0;
			foreach (var stat in Build.StatNames) {
				bool s = GetPts(rune, tab, stat, ref points, fake ?? 0, pred);
				if (scoring == FilterType.And)
					res &= s;
				else if (scoring == 0)
					res |= s;
			}

			var ctrl = Controls.Find(tab + "Check", true).FirstOrDefault();

			if (ctrl != null)
				ctrl.Text = res.ToString();

			if (scoring == FilterType.Sum || scoring == FilterType.SumN)
				ctrl.Text = points.ToString("#.##");
			if (scoring == FilterType.Sum) {
				ctrl.ForeColor = Color.Red;
				if (points >= build.RuneScoring[tabdex].Value)
					ctrl.ForeColor = Color.Green;
			}
		}


		// Returns if to abort that operation
		bool AnnoyUser() {
			if (build == null)
				return true;

			foreach (var tbf in build.RuneFilters) {
				if (build.RuneScoring.ContainsKey(tbf.Key)) {
					FilterType and = build.RuneScoring[tbf.Key].Key;
					double sum = 0;

					foreach (var rbf in tbf.Value) {
						if (rbf.Value.Flat.HasValue)
							sum += rbf.Value.Flat.Value;
						if (rbf.Value.Percent.HasValue)
							sum += rbf.Value.Percent.Value;
						switch (and) {
							case FilterType.Or:
							case FilterType.And:
								if (rbf.Value.Test == 0) {
									if (rbf.Value.Flat > 0 || rbf.Value.Percent > 0) {
										if (tabControl1.TabPages.ContainsKey("tab" + tbf.Key)) {
											var tab = tabControl1.TabPages["tab" + tbf.Key];
											tabControl1.SelectTab(tab);
											var ctrl = tab.Controls.Find(tbf.Key + rbf.Key + "test", false).FirstOrDefault();
											tooltipBadRuneFilter.IsBalloon = true;
											tooltipBadRuneFilter.Show(string.Empty, ctrl);
											tooltipBadRuneFilter.Show("GEQ how much?", ctrl, 0);
											return true;
										}
									}
								}
								else {
									if (rbf.Value.Flat + rbf.Value.Percent == 0) {
										var tab = tabControl1.TabPages["tab" + tbf.Key];
										tabControl1.SelectTab(tab);
										var ctrl = tab.Controls.Find(tbf.Key + rbf.Key, false).FirstOrDefault();
										tooltipBadRuneFilter.IsBalloon = true;
										tooltipBadRuneFilter.Show(string.Empty, ctrl);
										tooltipBadRuneFilter.Show("Counts for what?", ctrl, 0);
										return true;
									}
								}
								break;
						}
					}
					switch (and) {
						case FilterType.Sum:
						case FilterType.SumN:
							if (sum > 0 && build.RuneScoring[tbf.Key].Value == 0) {
								if (tabControl1.TabPages.ContainsKey("tab" + tbf.Key)) {
									var tab = tabControl1.TabPages["tab" + tbf.Key];
									tabControl1.SelectTab(tab);
									var ctrl = tab.Controls.Find(tbf.Key + "test", false).FirstOrDefault();
									tooltipBadRuneFilter.IsBalloon = true;
									tooltipBadRuneFilter.Show(string.Empty, ctrl);
									tooltipBadRuneFilter.Show("GEQ how much?", ctrl, 0);
									return true;
								}
							}
							break;
					}
				}
			}

			if (!build.Sort.IsNonZero) {
				var ctrl = groupBox1.Controls.Find("SPDWorth", false).FirstOrDefault();
				if (ctrl != null) {
					tooltipNoSorting.IsBalloon = true;
					tooltipNoSorting.Show(string.Empty, ctrl);
					tooltipNoSorting.Show("Enter a value somewhere, please.\nLike 1 or 3.14", ctrl, 0);
					return true;
				}
			}

			return false;
		}

		async Task updatePerms() {
			if (btnPerms.Visible)
				btnPerms.Enabled = true;
			else
				await CalcPerms();
		}

		async Task<long> CalcPerms() {

			await Task.Run(() => {
				// good idea, generate right now whenever the user clicks a... whatever
				build.RunesUseLocked = false;
				build.RunesUseEquipped = Program.Settings.UseEquipped;
				build.RunesDropHalfSetStat = Program.goFast;
				build.RunesOnlyFillEmpty = Program.fillRunes;
				build.BuildSaveStats = false;
				build.GenRunes(Program.data);
			});

			// figure stuff out
			long perms = 0;
			Label ctrl;
			for (int i = 0; i < 6; i++) {
				if (build.runes[i] == null)
					continue;

				int num = build.runes[i].Length;

				if (i == 0)
					perms = num;
				else
					perms *= num;
				if (num == 0)
					perms = 0;

				ctrl = (Label)Controls.Find("runeNum" + (i + 1).ToString(), true).FirstOrDefault();
				if (ctrl == null) continue;
				ctrl.Text = num.ToString();
				ctrl.ForeColor = Color.Black;

				// arbitrary colours for goodness/badness
				if (num < 12)
					ctrl.ForeColor = Color.Green;
				if (num > 24)
					ctrl.ForeColor = Color.Orange;
				if (num > 32)
					ctrl.ForeColor = Color.Red;
			}
			ctrl = (Label)Controls.Find("runeNums", true).FirstOrDefault();
			if (ctrl != null) {
				ctrl.Text = String.Format("{0:#,##0}", perms);
				ctrl.ForeColor = Color.Black;

				// arbitrary colours for goodness/badness
				if (perms < 2 * 1000 * 1000) // 2m (0.5s)
					ctrl.ForeColor = Color.Green;
				if (perms > 10 * 1000 * 1000) // 10m (~2s)
					ctrl.ForeColor = Color.Orange;
				if (perms > 50 * 1000 * 1000 || perms == 0) // 50m (>10s)
					ctrl.ForeColor = Color.Red;
			}

			btnPerms.Enabled = false;

			return perms;
		}

	}
}
