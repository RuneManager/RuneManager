using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using RuneOptim.BuildProcessing;

namespace RuneOptim.swar {
    partial class Rune {

        [JsonIgnore]
        public Monster Assigned;

        [JsonIgnore]
        public bool Swapped = false;

        [JsonIgnore]
        public static readonly int[] UnequipCosts = {
            1000, 2500, 5000, 10000, 25000, 50000, 0 , 0, 0 , 0 ,
            // ancient runes are 0 from 10 (to 15)
            1000, 2500, 5000, 10000, 25000, 50000
        };

        [JsonIgnore]
        public ConcurrentDictionary<string, double> manageStats = new ConcurrentDictionary<string, double>();

        [JsonIgnore]
        private bool? setIs4;

        [JsonIgnore]
        public bool SetIs4 {
            get {
                return setIs4 ?? (setIs4 = (Rune.SetRequired(this.Set) == 4)).Value;
            }
        }

        [JsonIgnore]
        public int UnequipCost { get { return UnequipCosts[Grade - 1]; } }

        [JsonIgnore]
        public int Rarity {
            get {
                // TODO: base-rarity is a thing now, consider this
                return Subs.Count;
            }
        }

        public event EventHandler<EventArgs> OnUpdate;

        public void ResetStats() {
            manageStats.Clear();
        }

        protected void Freeze() {
            Main.PreventOnChange = true;
            Innate.PreventOnChange = true;
            foreach (var s in Subs) {
                s.PreventOnChange = true;
            }
        }

        protected void Unfreeze() {
            Main.PreventOnChange = false;
            Innate.PreventOnChange = false;
            foreach (var s in Subs) {
                s.PreventOnChange = false;
            }
        }


        // For each non-zero stat in flat and percent, divide the runes value and see if any >= test
        public bool Or(Stats rFlat, Stats rPerc, Stats rTest, int fake = 0, bool pred = false) {
            foreach (Attr attr in Build.StatEnums) {
                if (attr != Attr.Speed && !rPerc[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rPerc[attr] >= rTest[attr])
                    return true;
            }
            foreach (Attr attr in new Attr[] { Attr.Speed, Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat }) {
                if (!rFlat[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rFlat[attr] >= rTest[attr])
                    return true;
            }
            return false;
        }

        // For each non-zero stat in flat and percent, divide the runes value and see if *ALL* >= test
        public bool And(Stats rFlat, Stats rPerc, Stats rTest, int fake = 0, bool pred = false) {
            foreach (Attr attr in Build.StatEnums) {
                if (attr != Attr.Speed && !rPerc[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rPerc[attr] < rTest[attr])
                    return false;
            }
            foreach (Attr attr in new Attr[] { Attr.Speed, Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat }) {
                if (!rFlat[attr].EqualTo(0) && !rTest[attr].EqualTo(0) && this[attr, fake, pred] / rFlat[attr] < rTest[attr])
                    return false;
            }
            return true;
        }

        // sum the result of dividing the runes value by flat/percent per stat
        public double Test(Stats rFlat, Stats rPerc, int fake = 0, bool pred = false) {
            double val = 0;
            foreach (Attr attr in Build.StatEnums) {
                if (attr != Attr.Speed && !rPerc[attr].EqualTo(0)) {
                    if (false && !System.Diagnostics.Debugger.IsAttached)
                        RuneLog.Debug(attr + ": " + this[attr, fake, pred] + "/" + rPerc[attr] + (this[attr, fake, pred] / rPerc[attr]));
                    val += this[attr, fake, pred] / rPerc[attr];
                }
            }
            foreach (Attr attr in new Attr[] { Attr.Speed, Attr.HealthFlat, Attr.AttackFlat, Attr.DefenseFlat }) {
                if (!rFlat[attr].EqualTo(0)) {
                    if (false && !System.Diagnostics.Debugger.IsAttached)
                        RuneLog.Debug(attr + ": " + this[attr, fake, pred] + "/" + rFlat[attr] + (this[attr, fake, pred] / rFlat[attr]));
                    val += this[attr, fake, pred] / rFlat[attr];
                }
            }
            manageStats.AddOrUpdate("testScore", val, (k, v) => val);
            return val;
        }

        public void SetValue(int p, Attr a, int v) {
            switch (p) {
                case -1:
                    Innate.Type = a;
                    Innate.Value = v;
                    break;
                case 0:
                    Main.Type = a;
                    Main.Value = v;
                    break;
                case 1:
                    Subs[0].Type = a;
                    Subs[0].Value = v;
                    break;
                case 2:
                    Subs[1].Type = a;
                    Subs[1].Value = v;
                    break;
                case 3:
                    Subs[2].Type = a;
                    Subs[2].Value = v;
                    break;
                case 4:
                    Subs[3].Type = a;
                    Subs[3].Value = v;
                    break;
                default:
                    break;
            }
            PrebuildAttributes();
        }

        public IEnumerable<Craft> FilterGrinds(IEnumerable<Craft> data) {
            var grinds = data.Where(c => c.Type == CraftType.Grind);
            grinds = grinds.Where(c => c.Set == this.Set);
            grinds = grinds.Where(c => c.Stat != this.Main.Type);
            grinds = grinds.Where(c => c.Stat != this.Innate?.Type);
            grinds = grinds.Where(c => this.HasStat(c.Stat));
            switch (this.Slot) {
                case 1:
                    grinds = grinds.Where(c => c.Stat != Attr.DefenseFlat && c.Stat != Attr.DefensePercent);
                    break;
                case 3:
                    grinds = grinds.Where(c => c.Stat != Attr.AttackFlat && c.Stat != Attr.AttackPercent);
                    break;
            }
            return grinds;
        }

        public IEnumerable<Craft> FilterEnchants(IEnumerable<Craft> data) {
            var enchants = data.Where(c => c.Type == CraftType.Enchant);
            enchants = enchants.Where(c => c.Set == this.Set);
            // except you can enchant 4% resist to 5-7% resist >.>
            enchants = enchants.Where(c => !this.HasStat(c.Stat));
            switch (this.Slot) {
                case 1:
                    enchants = enchants.Where(c => c.Stat != Attr.DefenseFlat && c.Stat != Attr.DefensePercent);
                    break;
                case 3:
                    enchants = enchants.Where(c => c.Stat != Attr.AttackFlat && c.Stat != Attr.AttackPercent);
                    break;
            }
            return enchants;
        }

        public Rune Grind(Craft craft, int index = 0) {
            // assume craft is valid
            var r = new Rune();
            this.CopyTo(r, true, null);
            r.manageStats = new ConcurrentDictionary<string, double>(this.manageStats);
            if (craft.Type == CraftType.Grind) {
                var sub = r.Subs.FirstOrDefault(s => s.Type == craft.Stat);
                if (sub != null) {
                    if (craft.Value.Average > sub.GrindBonus) {
                        var d = Math.Max(craft.Value.Min, sub.GrindBonus);
                        sub.GrindBonus = (craft.Value.Max - d) / 2 + d;
                    }
                    sub.Refresh();
                }
            }
            else {
                r.Subs[index].Type = craft.Stat;
                r.Subs[index].Enchanted = 1;
                r.Subs[index].BaseValue = (int)craft.Value.Average;
                r.Subs[index].GrindBonus = 0;

                r.Subs[index].Refresh();
            }

            return r;
        }

    }
}
