using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace RuneOptim.swar {
    public partial class Rune
    {
        
        public static readonly ImmutableArray<RuneSet> RuneSets = new RuneSet[] { RuneSet.Energy, // Health
            RuneSet.Guard, // Def
            RuneSet.Swift, // Speed
            RuneSet.Blade, // CRate
            RuneSet.Rage, // CDmg
            RuneSet.Focus, // Acc
            RuneSet.Endure, // Res
            RuneSet.Fatal, // Attack

            // Here be magic
            RuneSet.Despair,
            RuneSet.Vampire,

            RuneSet.Violent,
            RuneSet.Nemesis,
            RuneSet.Will,
            RuneSet.Shield,
            RuneSet.Revenge,
            RuneSet.Destroy,

            // Ally sets
            RuneSet.Fight,
            RuneSet.Determination,
            RuneSet.Enhance,
            RuneSet.Accuracy,
            RuneSet.Tolerance,
        }.ToImmutableArray();

        [JsonIgnore]
        public double BarionEfficiency
        {
            get
            {
                double num = 0;
                num += this.GetEfficiency(Innate.Type, Innate.Value);
                if (Subs.Count > 0)
                    num += this.GetEfficiency(Subs[0].Type, Subs[0].Value);
                if (Subs.Count > 1)
                    num += this.GetEfficiency(Subs[1].Type, Subs[1].Value);
                if (Subs.Count > 2)
                    num += this.GetEfficiency(Subs[2].Type, Subs[2].Value);
                if (Subs.Count > 3)
                    num += this.GetEfficiency(Subs[3].Type, Subs[3].Value);

                num /= 1.8;
                return num;
            }
        }

        [JsonIgnore]
        public double MaxEfficiency {
            get {
                return (BarionEfficiency + ((Math.Max(Math.Ceiling((12 - Level) / 3.0), 0) * 0.2) / 2.8));
            }
        }

        public static readonly ImmutableDictionary<Attr, double> VivoMod = new Dictionary<Attr, double>()
        {
            { Attr.HealthFlat, 110 },
            { Attr.HealthPercent, 1 },
            { Attr.AttackFlat, 7 },
            { Attr.AttackPercent, 1 },
            { Attr.DefenseFlat, 6 },
            { Attr.DefensePercent, 1 },
            { Attr.Speed, 0.85 },
            { Attr.SpeedPercent, 0.85 },
            { Attr.CritRate, 1 },
            { Attr.CritDamage, 1 },
            { Attr.Resistance, 1 },
            { Attr.Accuracy, 1 }
        }.ToImmutableDictionary();

        [JsonIgnore]
        public double VivoPrestoModel
        {
            get
            {
                double vv = 0;
                // penalty for Grade
                double scale = 1 / VivoMod[Main.Type] / RuneProperties.MainValues[Main.Type][5][15];
                double tv = RuneProperties.MainValues[Main.Type][Grade - 1][15] - RuneProperties.MainValues[Main.Type][5][15];
                vv += tv * scale;

                // transform each sub into a percentage of a 6*15 rune.
                if (Innate != null && Innate.Type > Attr.Null)
                {
                    scale = 1 / VivoMod[Innate.Type] / RuneProperties.MainValues[Innate.Type][5][15];
                    vv += Innate.Value * scale;
                }
                foreach (var s in Subs)
                {
                    if (s != null && s.Type > Attr.Null)
                    {
                        scale = 1 / VivoMod[s.Type] / RuneProperties.MainValues[s.Type][5][15];
                        vv += s.Value * scale;
                    }
                }

                // Bonus for under-levelled
                vv += (4 - Math.Min(4, Level/3)) * 0.07;
                return vv;
            }
        }
    }
    
    public class RuneChangeEventArgs : EventArgs
    {
        public Rune OldRune { get; set; }
        public Rune NewRune { get; set; }
    }

    public static class RuneProperties
    {
        // these sets can't be superceded by really good stats
        // Eg. Blade can be replaced by 12% crit.
        public static readonly RuneSet[] MagicalSets =
        {
            RuneSet.Violent, RuneSet.Will, RuneSet.Nemesis, RuneSet.Shield, RuneSet.Revenge, RuneSet.Despair, RuneSet.Vampire, RuneSet.Destroy,
            RuneSet.Tolerance, RuneSet.Accuracy, RuneSet.Determination, RuneSet.Enhance, RuneSet.Fight,
        };

        public static int FlatCount(this Rune rune)
        {
            int count = 0;
            if (rune.Subs.Count == 0 || rune.Subs[0].Type <= Attr.Null) return count;
            count += (rune.Subs[0].Type == Attr.HealthFlat || rune.Subs[0].Type == Attr.DefenseFlat || rune.Subs[0].Type == Attr.AttackFlat) ? 1 : 0;
            if (rune.Subs.Count == 1 || rune.Subs[1].Type <= Attr.Null) return count;
            count += (rune.Subs[1].Type == Attr.HealthFlat || rune.Subs[1].Type == Attr.DefenseFlat || rune.Subs[1].Type == Attr.AttackFlat) ? 1 : 0;
            if (rune.Subs.Count == 2 || rune.Subs[2].Type <= Attr.Null) return count;
            count += (rune.Subs[2].Type == Attr.HealthFlat || rune.Subs[2].Type == Attr.DefenseFlat || rune.Subs[2].Type == Attr.AttackFlat) ? 1 : 0;
            if (rune.Subs.Count == 3 || rune.Subs[3].Type <= Attr.Null) return count;
            count += (rune.Subs[3].Type == Attr.HealthFlat || rune.Subs[3].Type == Attr.DefenseFlat || rune.Subs[3].Type == Attr.AttackFlat) ? 1 : 0;

            return count;
        }

        public static AttributeCategory getSetType(this Rune rune)
        {
            var neutral = new RuneSet[] { RuneSet.Violent, RuneSet.Swift, RuneSet.Focus, RuneSet.Nemesis };
            if (new RuneSet[] { RuneSet.Fatal, RuneSet.Blade, RuneSet.Rage, RuneSet.Vampire, RuneSet.Revenge }.Contains(rune.Set)) // o
                return AttributeCategory.Offensive;
            else if (new RuneSet[] { RuneSet.Energy, RuneSet.Endure, RuneSet.Guard, RuneSet.Shield, RuneSet.Will }.Contains(rune.Set)) // d
                return AttributeCategory.Defensive;
            else if (new RuneSet[] { RuneSet.Despair, RuneSet.Determination, RuneSet.Enhance, RuneSet.Fight, RuneSet.Accuracy, RuneSet.Tolerance }.Contains(rune.Set)) // s
                return AttributeCategory.Support;

            return AttributeCategory.Neutral;
        }

        [JsonIgnore]
        private static readonly Attr[] attackSubs = new Attr[] { Attr.AttackPercent, Attr.CritDamage, Attr.CritRate };
        [JsonIgnore]
        private static readonly Attr[] defenseSubs = new Attr[] { Attr.HealthPercent, Attr.DefensePercent };
        [JsonIgnore]
        private static readonly Attr[] supportSubs = new Attr[] { Attr.Accuracy, Attr.Resistance };

        public static double ComputeRating(this Rune rune)
        {
            double r = 0;
            var type = rune.getSetType();

            // for each sub (if flat = 0, null = 0.3, else 1)

            // set types
            // stat types
            // offense/defense/support/neutral

            var subs = new Attr[4];
            if (rune.Subs.Count > 0)
                subs[0] = rune.Subs[0].Type;
            if (rune.Subs.Count > 1)
                subs[1] = rune.Subs[1].Type;
            if (rune.Subs.Count > 2)
                subs[2] = rune.Subs[2].Type;
            if (rune.Subs.Count > 3)
                subs[3] = rune.Subs[3].Type;

            foreach (var sub in subs)
            {
                // if null
                if (sub <= Attr.Null)
                {
                    r += 1 / (double)3;
                    continue;
                }

                // if not flat
                if (!new Attr[] { Attr.AttackFlat, Attr.DefenseFlat, Attr.HealthFlat }.Contains(sub))
                    r += 1;

                if (new Attr[] { Attr.HealthPercent, Attr.Speed }.Contains(sub))
                    r += 0.5;

                AttributeCategory subt;
                r += computeSubTypeRating(type, sub, out subt);
                r += computeSubRating(sub, subs, subt);
            }

            if (rune.Grade == 5)
                r += 4;
            else if (rune.Grade == 6)
                r += 7;

            return r;
        }

        private static double computeSubTypeRating(AttributeCategory type, Attr sub, out AttributeCategory subt)
        {
            subt = AttributeCategory.Neutral;
            if (attackSubs.Contains(sub))
                subt = AttributeCategory.Offensive;
            else if (defenseSubs.Contains(sub))
                subt = AttributeCategory.Defensive;
            else if (supportSubs.Contains(sub))
                subt = AttributeCategory.Support;

            double r = 0;

            switch (type)
            {
                case AttributeCategory.Offensive:
                    switch (subt)
                    {
                        case AttributeCategory.Offensive:
                            return 1;
                        default:
                            return 0.25;
                    }
                case AttributeCategory.Defensive:
                    switch (subt)
                    {
                        case AttributeCategory.Offensive:
                            return 0.25;
                        case AttributeCategory.Defensive:
                            return 1;
                        default:
                            return 0.5;
                    }
                case AttributeCategory.Support:
                    switch (subt)
                    {
                        case AttributeCategory.Defensive:
                            return 0.5;
                        case AttributeCategory.Support:
                            return 1;
                        default:
                            return 0.25;
                    }
                case AttributeCategory.Neutral:
                    switch (subt)
                    {
                        case AttributeCategory.Offensive:
                        case AttributeCategory.Defensive:
                            return 0.5;
                        default:
                            return 0.25;
                    }
                default:
                    break;
            }

            return r;
        }

        private static double computeSubRating(Attr sub, Attr[] subs, AttributeCategory subt)
        {
            double r = 0;
            foreach (var sub2 in subs)
            {
                if (sub == sub2 || sub2 <= Attr.Null)
                    continue;

                if ((subt == AttributeCategory.Offensive && attackSubs.Contains(sub2))
                    || (subt == AttributeCategory.Defensive && defenseSubs.Contains(sub2))
                    || (subt == AttributeCategory.Support && supportSubs.Contains(sub2)))
                    r += 1;
            }
            return r;
        }

        [JsonIgnore]
        private static readonly Attr[] hpStats = new Attr[] { Attr.HealthPercent, Attr.DefensePercent, Attr.Resistance, Attr.HealthFlat, Attr.DefenseFlat };

        public static double ScoreHP(this Rune rune)
        {
            double v = 0;

            v += rune.HealthPercent[0] / (double)subMaxes[Attr.HealthPercent];
            v += rune.DefensePercent[0] / (double)subMaxes[Attr.DefensePercent];
            v += rune.Resistance[0] / (double)subMaxes[Attr.Resistance];
            v += 0.5 * rune.HealthFlat[0] / subMaxes[Attr.HealthFlat];
            v += 0.5 * rune.DefenseFlat[0] / subMaxes[Attr.DefenseFlat];

            if (rune.Main.Type == Attr.HealthPercent || rune.Main.Type == Attr.DefensePercent || rune.Main.Type == Attr.Resistance)
            {
                v -= rune.Main.Value / (double)subMaxes[rune.Main.Type];
                if (rune.Slot % 2 == 0)
                    v += rune.Grade / (double)6;
            }
            else if (rune.Main.Type == Attr.DefenseFlat || rune.Main.Type == Attr.HealthFlat)
            {
                v -= 0.5 * rune.Main.Value / subMaxes[rune.Main.Type];
                if (rune.Slot % 2 == 0)
                    v += rune.Grade / (double)6;
            }

            double d = 0.5;
            if (rune.Slot == 3 || rune.Slot == 5)
                d += 0.2;

            if (rune.Slot % 2 == 0)
                d += 1;

            var stt = 0;
            if (rune.Subs.Count > 0 && hpStats.Contains(rune.Subs[0].Type))
                stt++;
            if (rune.Subs.Count > 1 && hpStats.Contains(rune.Subs[1].Type))
                stt++;
            if (rune.Subs.Count > 2 && hpStats.Contains(rune.Subs[2].Type))
                stt++;
            if (rune.Subs.Count > 3 && hpStats.Contains(rune.Subs[3].Type))
                stt++;

            d += 0.2 * (
                rune.Rarity
                - (
                    rune.Rarity
                    - Math.Floor(rune.Level / (double)3)
                ) * stt
            );

            return v / d;
        }

        [JsonIgnore]
        private static readonly Attr[] atkStats = new Attr[] { Attr.AttackFlat, Attr.AttackPercent, Attr.CritRate, Attr.CritDamage };

        public static double ScoreATK(this Rune rune)
        {
            double v = 0;

            v += rune.AttackPercent[0] / (double)subMaxes[Attr.AttackPercent];
            v += rune.CritRate[0] / (double)subMaxes[Attr.CritRate];
            v += rune.CritDamage[0] / (double)subMaxes[Attr.CritDamage];
            v += 0.5 * rune.AttackFlat[0] / subMaxes[Attr.AttackFlat];

            if (rune.Main.Type == Attr.AttackPercent || rune.Main.Type == Attr.CritRate || rune.Main.Type == Attr.CritDamage)
            {
                v -= rune.Main.Value / (double)subMaxes[rune.Main.Type];
                if (rune.Slot % 2 == 0)
                    v += rune.Grade / (double)6;
            }
            else if (rune.Main.Type == Attr.AttackFlat)
            {
                v -= 0.5 * rune.Main.Value / subMaxes[rune.Main.Type];
                if (rune.Slot % 2 == 0)
                    v += rune.Grade / (double)6;
            }

            double d = 0.4;

            if (rune.Slot == 1)
                d += 0.2;

            if (rune.Slot % 2 == 0)
                d += 1.1;

            var stt = 0;
            if (rune.Subs.Count > 0 && atkStats.Contains(rune.Subs[0].Type))
                stt++;
            if (rune.Subs.Count > 1 && atkStats.Contains(rune.Subs[1].Type))
                stt++;
            if (rune.Subs.Count > 2 && atkStats.Contains(rune.Subs[2].Type))
                stt++;
            if (rune.Subs.Count > 3 && atkStats.Contains(rune.Subs[3].Type))
                stt++;

            d += 0.2 * (
                rune.Rarity
                - (
                    rune.Rarity
                    - Math.Floor(rune.Level / (double)3)
                ) * stt
            );

            return v / d;
        }

        public static double ScoreRune(this Rune rune)
        {
            double v = 0;

            if (rune.Innate != null && rune.Innate.Value > 0)
            {
                if (rune.Innate.Type == Attr.AttackFlat || rune.Innate.Type == Attr.HealthFlat || rune.Innate.Type == Attr.DefenseFlat)
                    v += 0.5 * rune.Innate.Value / subMaxes[rune.Innate.Type];
                else
                    v += rune.Innate.Value / (double)subMaxes[rune.Innate.Type];
            }

            if (rune.Subs.Count > 0 && rune.Subs[0].Value > 0)
            {
                if (rune.Subs[0].Type == Attr.AttackFlat || rune.Subs[0].Type == Attr.HealthFlat || rune.Subs[0].Type == Attr.DefenseFlat)
                    v += 0.5 * rune.Subs[0].Value / subMaxes[rune.Subs[0].Type];
                else
                    v += rune.Subs[0].Value / (double)subMaxes[rune.Subs[0].Type];
            }

            if (rune.Subs.Count > 1 && rune.Subs[1].Value > 0)
            {
                if (rune.Subs[1].Type == Attr.AttackFlat || rune.Subs[1].Type == Attr.HealthFlat || rune.Subs[1].Type == Attr.DefenseFlat)
                    v += 0.5 * rune.Subs[1].Value / subMaxes[rune.Subs[1].Type];
                else
                    v += rune.Subs[1].Value / (double)subMaxes[rune.Subs[1].Type];
            }

            if (rune.Subs.Count > 2 && rune.Subs[2].Value > 0)
            {
                if (rune.Subs[2].Type == Attr.AttackFlat || rune.Subs[2].Type == Attr.HealthFlat || rune.Subs[2].Type == Attr.DefenseFlat)
                    v += 0.5 * rune.Subs[2].Value / subMaxes[rune.Subs[2].Type];
                else
                    v += rune.Subs[2].Value / (double)subMaxes[rune.Subs[2].Type];
            }

            if (rune.Subs.Count > 3 && rune.Subs[3].Value > 0)
            {
                if (rune.Subs[3].Type == Attr.AttackFlat || rune.Subs[3].Type == Attr.HealthFlat || rune.Subs[3].Type == Attr.DefenseFlat)
                    v += 0.5 * rune.Subs[3].Value / subMaxes[rune.Subs[3].Type];
                else
                    v += rune.Subs[3].Value / (double)subMaxes[rune.Subs[3].Type];
            }

            v += rune.Grade / (double)6;

            double d = 2;
            d += 0.2 * Math.Min(4, Math.Floor(rune.Level / (double)3));

            return v / d;
        }

        public static double GetEfficiency(this Rune rune, Attr a, int val)
        {
            if (a <= Attr.Null)
                return 0;
            var g = rune.Grade;
            if (g > 10)
                g -= 10;
            if (a == Attr.HealthFlat || a == Attr.AttackFlat || a == Attr.DefenseFlat)
                return val / (double)2 / (5 * subUpgrades[a][g - 1].Max);

            return val / (double)(5 * subUpgrades[a][g - 1].Max);
        }

        public static double EffectiveBaseRank(this Rune r)
        {
            double ret = 0;
            var g = r.Grade;
            if (g > 10)
                g -= 10;
            foreach (RuneAttr s in r.Subs)
            {
                ret += (s.BaseValue - subUpgrades[s.Type][g - 1].Average) / (double)subUpgrades[s.Type][g - 1].Average;
            }
            return ret;
        }

        #region stats

        public readonly static Dictionary<RuneSet, string> setUnicode = new Dictionary<RuneSet, string>() {
            { RuneSet.Blade, "ᚬ"},
            { RuneSet.Despair, "ᛃ" },
            { RuneSet.Violent, "ᛒ" },
            { RuneSet.Will, "ᛠ" },
            { RuneSet.Determination, "ᛗ" },
            { RuneSet.Revenge, "ᛝ" },
            { RuneSet.Rage, "ᛟ" },
            { RuneSet.Energy, "ᛊ" },
            { RuneSet.Fatal, "A" },
            { RuneSet.Guard, "ᚤ" },
            { RuneSet.Endure, "ᛡ" },
            { RuneSet.Nemesis, "ᚹ" },
            { RuneSet.Swift, "≈̇ " },

        };


        private readonly static Dictionary<Attr, int> subMaxes = new Dictionary<Attr, int>()
        {
            {Attr.Neg, 1 },
            {Attr.Null, 1 },
            {Attr.HealthFlat, 1875 },
            {Attr.AttackFlat, 100 },
            {Attr.DefenseFlat, 100 },
            {Attr.Speed, 30 },

            {Attr.HealthPercent, 40 },
            {Attr.AttackPercent, 40 },
            {Attr.DefensePercent, 40 },

            { Attr.CritRate, 30 },
            { Attr.CritDamage, 35 },

            {Attr.Resistance, 40 },
            {Attr.Accuracy, 40 },
        };

        private static readonly ValueRange[] flatSubUpgrades = { new ValueRange(1, 4), new ValueRange(2, 5), new ValueRange(3, 8), new ValueRange(4, 10), new ValueRange(8, 15), new ValueRange(10, 20) };
        private static readonly ValueRange[] percentSubUpgrades = { new ValueRange(1, 2), new ValueRange(1, 3), new ValueRange(2, 5), new ValueRange(3, 6), new ValueRange(4, 7), new ValueRange(5, 8) };
        private static readonly ValueRange[] accResSubUpgrades = { new ValueRange(1, 2), new ValueRange(1, 3), new ValueRange(2, 4), new ValueRange(2, 5), new ValueRange(3, 7), new ValueRange(4, 8) };

        private readonly static Dictionary<Attr, ValueRange[]> subUpgrades = new Dictionary<Attr, ValueRange[]>()
        {
            {Attr.HealthFlat, new ValueRange[] { new ValueRange(15, 60), new ValueRange(30, 105), new ValueRange(45, 165), new ValueRange(60, 225), new ValueRange(90, 300), new ValueRange(135, 375) } },
            {Attr.AttackFlat, flatSubUpgrades },
            {Attr.DefenseFlat, flatSubUpgrades },
            {Attr.Speed, new ValueRange[] { new ValueRange(1, 1), new ValueRange(1, 2), new ValueRange(1, 3), new ValueRange(2, 4), new ValueRange(3, 5), new ValueRange(4, 6) } },

            {Attr.HealthPercent, percentSubUpgrades },
            {Attr.AttackPercent, percentSubUpgrades },
            {Attr.DefensePercent, percentSubUpgrades },

            { Attr.CritRate, new ValueRange[] { new ValueRange(1, 2), new ValueRange(1, 3), new ValueRange(1, 3), new ValueRange(2, 4), new ValueRange(3, 5), new ValueRange(4, 6) } },
            { Attr.CritDamage, new ValueRange[] { new ValueRange(1, 2), new ValueRange(1, 3), new ValueRange(2, 4), new ValueRange(2, 5), new ValueRange(3, 5), new ValueRange(4, 7) } },

            {Attr.Resistance, accResSubUpgrades },
            {Attr.Accuracy, accResSubUpgrades },
        };

        public static readonly ImmutableArray<RuneMainStatValue> MainValues_Speed = new RuneMainStatValue[] {
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(3,4/(double)3),
            new RuneMainStatValue(4,1.5),
            new RuneMainStatValue(5,2),
            new RuneMainStatValue(7,2),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(3,4/(double)3),
            new RuneMainStatValue(4,1.5),
            new RuneMainStatValue(5,2),
            new RuneMainStatValue(7,2),
        }.ToImmutableArray();

        public static readonly ImmutableArray<RuneMainStatValue> MainValues_Flat = new RuneMainStatValue[] {
            new RuneMainStatValue(3,3),
            new RuneMainStatValue(5,4),
            new RuneMainStatValue(7,5),
            new RuneMainStatValue(10,6),
            new RuneMainStatValue(15,7),
            new RuneMainStatValue(22,8),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(3,3),
            new RuneMainStatValue(5,4),
            new RuneMainStatValue(7,5),
            new RuneMainStatValue(10,6),
            new RuneMainStatValue(15,7),
            new RuneMainStatValue(22,8),
        }.ToImmutableArray();

        public static readonly ImmutableArray<RuneMainStatValue> MainValues_HPflat = new RuneMainStatValue[] {
            new RuneMainStatValue(40,45),
            new RuneMainStatValue(70,60),
            new RuneMainStatValue(100,75),
            new RuneMainStatValue(160,90),
            new RuneMainStatValue(270,105),
            new RuneMainStatValue(360,120),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(40,45),
            new RuneMainStatValue(70,60),
            new RuneMainStatValue(100,75),
            new RuneMainStatValue(160,90),
            new RuneMainStatValue(270,105),
            new RuneMainStatValue(360,120),
        }.ToImmutableArray();

        public static readonly ImmutableArray<RuneMainStatValue> MainValues_Percent = new RuneMainStatValue[] {
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(4,2),
            new RuneMainStatValue(5,2.15),
            new RuneMainStatValue(8,2.45),
            new RuneMainStatValue(11,3),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(4,2),
            new RuneMainStatValue(5,2.15),
            new RuneMainStatValue(8,2.45),
            new RuneMainStatValue(11,3),
        }.ToImmutableArray();

        public static readonly ImmutableArray<RuneMainStatValue> MainValues_CRate = new RuneMainStatValue[] {
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(3,2),
            new RuneMainStatValue(4,2.15),
            new RuneMainStatValue(5,2.45),
            new RuneMainStatValue(7,3),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(3,2),
            new RuneMainStatValue(4,2.15),
            new RuneMainStatValue(5,2.45),
            new RuneMainStatValue(7,3),
        }.ToImmutableArray();

        public static readonly ImmutableArray<RuneMainStatValue> MainValues_CDmg = new RuneMainStatValue[] {
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(3,2),
            new RuneMainStatValue(4,2.25),
            new RuneMainStatValue(6,3),
            new RuneMainStatValue(8,10/(double)3),
            new RuneMainStatValue(11,4),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(3,2),
            new RuneMainStatValue(4,2.25),
            new RuneMainStatValue(6,3),
            new RuneMainStatValue(8,10/(double)3),
            new RuneMainStatValue(11,4),
        }.ToImmutableArray();

        public static readonly ImmutableArray<RuneMainStatValue> MainValues_ResAcc = new RuneMainStatValue[] {
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(4,2),
            new RuneMainStatValue(6,2.15),
            new RuneMainStatValue(9,2.45),
            new RuneMainStatValue(12,3),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(0,0),
            new RuneMainStatValue(1,1),
            new RuneMainStatValue(2,1),
            new RuneMainStatValue(4,2),
            new RuneMainStatValue(6,2.15),
            new RuneMainStatValue(9,2.45),
            new RuneMainStatValue(12,3),
        }.ToImmutableArray();

        public static readonly ImmutableDictionary<Attr, ImmutableArray<RuneMainStatValue>> MainValues = new Dictionary<Attr, ImmutableArray<RuneMainStatValue>>
        {
            {Attr.HealthFlat, MainValues_HPflat },
            {Attr.AttackFlat, MainValues_Flat },
            {Attr.DefenseFlat, MainValues_Flat },
            {Attr.Speed, MainValues_Speed },

            {Attr.HealthPercent, MainValues_Percent },
            {Attr.AttackPercent, MainValues_Percent },
            {Attr.DefensePercent, MainValues_Percent },

            {Attr.CritRate, MainValues_CRate },
            {Attr.CritDamage, MainValues_CDmg },

            {Attr.Accuracy, MainValues_ResAcc },
            {Attr.Resistance, MainValues_ResAcc },
        }.ToImmutableDictionary();

        #endregion
    }

    public class RuneMainStatValue
    {
        readonly int start;
        readonly double growth;

        public RuneMainStatValue(int s, double g)
        {
            start = s;
            growth = g;
        }

        public int this[int grade]
        {
            get
            {
                if (grade == 15)
                    return (int)Math.Round((start + (14 * growth)) * 1.2);
                else if (grade >= 0 && grade < 15)
                    return (int)Math.Round(start + (grade * growth));
                else
                    throw new IndexOutOfRangeException("Cannot check grade " + grade + " rune");
            }
        }
    }

    // Enums up Runesets
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RuneSet
    {
        [EnumMember(Value = "???")]
        Unknown = -1, // SW Proxy say what?

        [EnumMember(Value = "")]
        Null = 0, // No set

        Energy          = 1 << 0, // Health
        Guard           = 1 << 1, // Def
        Swift           = 1 << 2, // Speed
        Blade           = 1 << 3, // CRate
        Rage            = 1 << 4, // CDmg
        Focus           = 1 << 5, // Acc
        Endure          = 1 << 6, // Res
        Fatal           = 1 << 7, // Attack

        __unknown9      = 1 << 8,

        // Here be magic
        Despair         = 1 << 9,
        Vampire         = 1 << 10,

        __unknown12     = 1 << 11,

        Violent         = 1 << 12,
        Nemesis         = 1 << 13,
        Will            = 1 << 14,
        Shield          = 1 << 15,
        Revenge         = 1 << 16,
        Destroy         = 1 << 17,

        // Ally sets
        Fight           = 1 << 18,
        Determination   = 1 << 19,
        Enhance         = 1 << 20,
        Accuracy        = 1 << 21,
        Tolerance       = 1 << 22,

        Broken          = 1 << 23,

        Set4 = Swift | Rage | Fatal | Despair | Vampire | Violent,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SlotIndex
    {
        [EnumMember(Value = "e")]
        Even = -2,

        //o = -1,

        [EnumMember(Value = "o")]
        Odd = -1,

        [EnumMember(Value = "g")]
        Global = 0,

        [EnumMember(Value = "1")]
        One = 1,
        [EnumMember(Value = "2")]
        Two = 2,
        [EnumMember(Value = "3")]
        Three = 3,
        [EnumMember(Value = "4")]
        Four = 4,
        [EnumMember(Value = "5")]
        Five = 5,
        [EnumMember(Value = "6")]
        Six = 6
    };
    
    public class RuneAttr : ListProp<int?>
    {
        [ListProperty(0)]
        public Attr Type = default(Attr);

        [ListProperty(1)]
        public int BaseValue = -1;

        [ListProperty(2)]
        public int Enchanted = -1;

        [ListProperty(3)]
        public int GrindBonus = -1;

        [JsonIgnore]
        public bool IsEnchanted { get { return Enchanted == 1; } }

        [JsonIgnore]
        private int _calcVal = -1;

        protected override void OnChange(int i, int? val)
        {
            if (Type != Attr.Neg)
            {
                _calcVal = BaseValue + (GrindBonus > 0 ? GrindBonus : 0);
            }
            else
                _calcVal = 0;
        }

        public void Refresh() {
            OnChange(0,0);
        }

        [JsonIgnore]
        public int Value
        {
            get
            {
                if (_calcVal == -1)
                    OnChange(0, 0);
                return _calcVal;
            }
            set
            {
                GrindBonus = 0;
                BaseValue = value;
                OnChange(1, value);
            }
        }

        public override string ToString()
        {
            return Type + " +" + Value;
        }

        #region Remove the slow attribute checking
        protected override int maxInd { get { return 4; } }

        public override bool IsReadOnly { get { return false; } }

        public override int? this[int index]
        {
            get
            {
                if (index == 0)
                    return (int)Type;
                else if (index == 1)
                    return BaseValue;
                else if (index == 2)
                    return Enchanted;
                else if (index == 3)
                    return GrindBonus;
                return -1;
            }

            set
            {
                if (index == 0)
                    Type = (Attr)value;
                else if (index == 1)
                    BaseValue = value ?? -1;
                else if (index == 2)
                    Enchanted = value ?? -1;
                else if (index == 3)
                    GrindBonus = value ?? -1;
                OnChange(1, value);
            }
        }

        public override void Add(int? item)
        {
            if (Type == Attr.Null)
                Type = (Attr)item;
            else if (BaseValue == -1)
                BaseValue = item ?? -1;
            else if (Enchanted == -1)
                Enchanted = item ?? -1;
            else if (GrindBonus == -1)
                GrindBonus = item ?? -1;
            else
                throw new IndexOutOfRangeException();
        }

        internal void CopyTo(ref RuneAttr rhs)
        {
            rhs.Type = Type;
            rhs.BaseValue = BaseValue;
            rhs.Enchanted = Enchanted;
            rhs.GrindBonus = GrindBonus;
            rhs.OnChange(0, 0);
        }

        internal void CopyFrom(RuneAttr lhs) {
            Type = lhs.Type;
            BaseValue = lhs.BaseValue;
            Enchanted = lhs.Enchanted;
            GrindBonus = lhs.GrindBonus;
            OnChange(0, 0);
        }

        #endregion
    }

    public class RuneLink
    {
        [JsonProperty("rune_id")]
        public ulong Id { get; set; }

        [JsonProperty("occupied_id")]
        public ulong AssignedId { get; set; }
    }

}
