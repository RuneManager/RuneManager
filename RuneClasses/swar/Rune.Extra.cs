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
        public ConcurrentDictionary<string, double> ManageStats = new ConcurrentDictionary<string, double>();

        [JsonIgnore]
        private bool? setIs4;

        [JsonIgnore]
        public bool SetIs4 {
            get {
                return setIs4 ?? (setIs4 = (Rune.SetSize(this.Set) == 4)).Value;
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
            ManageStats.Clear();
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
            ManageStats.AddOrUpdate("testScore", val, (k, v) => val);
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
            r.ManageStats = new ConcurrentDictionary<string, double>(this.ManageStats);
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

        /// <summary>
        /// Checks how many of the stat scores in s are met or exceeded by r. 
        /// Assumes Stat A/D/H are percent
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public int HasStats(Stats s)
        {
            int ret = 0;

            if (HealthPercent[0] >= s.Health)
                ret++;
            if (AttackPercent[0] >= s.Attack)
                ret++;
            if (DefensePercent[0] >= s.Defense)
                ret++;
            if (Speed[0] >= s.Speed)
                ret++;

            if (CritDamage[0] >= s.CritDamage)
                ret++;
            if (CritRate[0] >= s.CritRate)
                ret++;
            if (Accuracy[0] >= s.Accuracy)
                ret++;
            if (Resistance[0] >= s.Resistance)
                ret++;

            return ret;
        }

        /// <summary>
        /// Returns the sum of how much the rune fufills the Stat requirement.
        /// Eg. if the stat has 10% ATK & 20% HP, and the rune only has 5% ATK, it will return 0.5.
        /// Assumes Stat A/D/H are percent
        /// </summary>
        /// <param name="r"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public double VsStats(Stats s)
        {
            double ret = 0;

            if (s.Health > 0)
                ret += HealthPercent[0] / s.Health;
            if (s.Attack > 0)
                ret += AttackPercent[0] / s.Attack;
            if (s.Defense > 0)
                ret += DefensePercent[0] / s.Defense;
            if (s.Speed > 0)
                ret += Speed[0] / s.Speed;

            if (s.CritDamage > 0)
                ret += CritDamage[0] / s.CritDamage;
            if (s.CritRate > 0)
                ret += CritRate[0] / s.CritRate;
            if (s.Accuracy > 0)
                ret += Accuracy[0] / s.Accuracy;
            if (s.Resistance > 0)
                ret += Resistance[0] / s.Resistance;

            //Health / ((1000 / (1000 + Defense * 3)))
            if (s.EffectiveHP > 0)
            {
                var sh = 6000 * (100 + HealthPercent[0] + 20) / 100.0 + HealthFlat[0] + 1200;
                var sd = 300 * (100 + DefensePercent[0] + 10) / 100.0 + DefenseFlat[0] + 70;

                double delt = 0;
                delt += sh / (1000 / (1000 + sd * 3));
                delt -= 6000 / (1000 / (1000 + 300.0 * 3));
                ret += delt / s.EffectiveHP;
            }

            //Health / ((1000 / (1000 + Defense * 3 * 0.3)))
            if (s.EffectiveHPDefenseBreak > 0)
            {
                var sh = 6000 * (100 + HealthPercent[0] + 20) / 100.0 + HealthFlat[0] + 1200;
                var sd = 300 * (100 + DefensePercent[0] + 10) / 100.0 + DefenseFlat[0] + 70;

                double delt = 0;
                delt += sh / (1000 / (1000 + sd * 0.9));
                delt -= 6000 / (1000 / (1000 + 300 * 0.9));
                ret += delt / s.EffectiveHPDefenseBreak;
            }

            // (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage + CritDamage / 100)
            if (s.MaxDamage > 0)
            {
                var sa = 300 * (100 + AttackPercent[0] + 20) / 100.0 + AttackFlat[0] + 100;
                var cd = 50 + CritDamage[0] + 20;

                double delt = 0;
                delt += sa * (1 + cd / 100);
                delt -= 460 * (1 + 0.7);
                ret += delt / s.MaxDamage;
            }

            // (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage + CritDamage / 100 * Math.Min(CritRate, 100) / 100)
            if (s.AverageDamage > 0)
            {
                var sa = 300 * (100 + AttackPercent[0] + 20) / 100.0 + AttackFlat[0] + 100;
                var cd = 50 + CritDamage[0] + 20;
                var cr = 15 + CritRate[0] + 15;

                double delt = 0;
                delt += sa * (1 + cd / 100 * Math.Min(cr, 100) / 100);
                delt -= 460 * (1 + 0.7 * 0.3);
                ret += delt / s.AverageDamage;
            }

            // ExtraValue(Attr.AverageDamage) * Speed / 100
            if (s.DamagePerSpeed > 0)
            {
                var sa = 300 * (100 + AttackPercent[0] + 20) / 100.0 + AttackFlat[0] + 100;
                var cd = 50 + CritDamage[0] + 20;
                var cr = 15 + CritRate[0] + 15;
                var sp = 100 + Speed[0] + 15;

                double delt = 0;
                delt += sa * (1 + cd / 100 * Math.Min(cr, 100) / 100) * sp / 100;
                delt -= 460 * (1 + 0.70 * 0.30) * 1.15;
                ret += delt / s.DamagePerSpeed;
            }

            return ret;
        }

    }
}
