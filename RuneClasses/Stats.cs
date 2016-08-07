using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RuneOptim
{
    public class Stats
    {
        // allows mapping save.json into the program via Monster
        [JsonProperty("b_hp")]
        public double Health = 0;

        [JsonProperty("b_atk")]
        public double Attack = 0;

        [JsonProperty("b_def")]
        public double Defense = 0;

        [JsonProperty("b_spd")]
        public double Speed = 0;

        [JsonProperty("b_crate")]
        public double CritRate = 0;

        [JsonProperty("b_cdmg")]
        public double CritDamage = 0;

        [JsonProperty("b_res")]
        public double Resistance = 0;

        [JsonProperty("b_acc")]
        public double Accuracy = 0;

        public Stats() { }
        // copy constructor, amrite?
        public Stats(Stats rhs)
        {
            Health = rhs.Health;
            Attack = rhs.Attack;
            Defense = rhs.Defense;
            Speed = rhs.Speed;
            CritRate = rhs.CritRate;
            CritDamage = rhs.CritDamage;
            Resistance = rhs.Resistance;
            Accuracy = rhs.Accuracy;
        }

        // fake "stats", need to be stored for scoring
        [JsonProperty("fake_ehp")]
        public double EffectiveHP = 0;

        [JsonProperty("fake_ehpdb")]
        public double EffectiveHPDefenseBreak = 0;
        
        [JsonProperty("fake_dps")]
        public double DamagePerSpeed = 0;

        [JsonProperty("fake_avd")]
        public double AverageDamage = 0;

        [JsonProperty("fake_mxd")]
        public double MaxDamage = 0;
        
        // Gets the Extra stat manually stored (for scoring)
        public double ExtraGet(string extra)
        {
            switch (extra)
            {
                case "EHP":
                    return EffectiveHP;
                case "EHPDB":
                    return EffectiveHPDefenseBreak;
                case "DPS":
                    return DamagePerSpeed;
                case "AvD":
                    return AverageDamage;
                case "MxD":
                    return MaxDamage;
            }
            return 0;
        }
        
        // Computes and returns the Extra stat
        public double ExtraValue(string extra)
        {
            switch (extra)
            {
                case "EHP":
                    return (Health / ((1000 / (double)(1000 + Defense * 3))));
                case "EHPDB":
                    return (Health / ((1000 / (double)(1000 + Defense * 3 * 0.3))));
                case "DPS":
                    return (ExtraValue("AvD") * (Speed / (double) 100));
                case "AvD":
                    return (ExtraValue("MxD") * (Math.Min(CritRate, 100) / (double) 100));
                case "MxD":
                    return (Attack * CritDamage / (double)100);
            }
            return 0;
        }

        // manually sets the Extra stat (used for scoring)
        public void ExtraSet(string extra, double value)
        {
            switch (extra)
            {
                case "EHP":
                    EffectiveHP = value;
                    break;
                case "EHPDB":
                    EffectiveHPDefenseBreak = value;
                    break;
                case "DPS":
                    DamagePerSpeed = value;
                    break;
                case "AvD":
                    AverageDamage = value;
                    break;
                case "MxD":
                    MaxDamage = value;
                    break;
            }
        }

        public void SetZero()
        {
            Accuracy = 0;
            Attack = 0;
            CritDamage = 0;
            CritRate = 0;
            Defense = 0;
            Health = 0;
            Resistance = 0;
            Speed = 0;

            EffectiveHP = 0;
            EffectiveHPDefenseBreak = 0;
            DamagePerSpeed = 0;
            AverageDamage = 0;
            MaxDamage = 0;
        }

        public double Sum()
        {
            return Accuracy
                + Attack
                + CritDamage
                + CritRate
                + Defense
                + Health
                + Resistance
                + Speed
                + EffectiveHP
                + EffectiveHPDefenseBreak
                + DamagePerSpeed
                + AverageDamage
                + MaxDamage;
        }

        // Allows speedy iteration through the entity
        public double this[string stat]
        {
            get
            {
                switch (stat)
                {
                    case "HP":
                        return Health;
                    case "ATK":
                        return Attack;
                    case "DEF":
                        return Defense;
                    case "SPD":
                        return Speed;
                    case "CD":
                        return CritDamage;
                    case "CR":
                        return CritRate;
                    case "ACC":
                        return Accuracy;
                    case "RES":
                        return Resistance;
                }
                return 0;
            }

            set
            {
                switch (stat)
                {
                    case "HP":
                        Health = value;
                        break;
                    case "ATK":
                        Attack = value;
                        break;
                    case "DEF":
                        Defense = value;
                        break;
                    case "SPD":
                        Speed = value;
                        break;
                    case "CD":
                        CritDamage = value;
                        break;
                    case "CR":
                        CritRate = value;
                        break;
                    case "ACC":
                        Accuracy = value;
                        break;
                    case "RES":
                        Resistance = value;
                        break;
                }
            }

        }

        // Perfectly legit operator overloading to compare builds/minimum
        public static bool operator <(Stats lhs, Stats rhs)
        {
            return rhs.GreaterEqual(lhs);
        }

        public static bool operator >(Stats lhs, Stats rhs)
        {
            return lhs.GreaterEqual(rhs);
        }

        internal bool GreaterEqual(Stats rhs)
        {
            if (this.Accuracy < rhs.Accuracy)
                return false;
            if (this.Attack < rhs.Attack)
                return false;
            if (this.CritDamage < rhs.CritDamage)
                return false;
            if (this.CritRate < rhs.CritRate)
                return false;
            if (this.Defense < rhs.Defense)
                return false;
            if (this.Health < rhs.Health)
                return false;
            if (this.Resistance < rhs.Resistance)
                return false;
            if (this.Speed < rhs.Speed)
                return false;


            return true;
        }

        public static Stats operator+(Stats lhs, Stats rhs)
        {
            lhs.Health += rhs.Health;
            lhs.Attack += rhs.Attack;
            lhs.Defense += rhs.Defense;
            lhs.Speed += rhs.Speed;
            lhs.CritRate += rhs.CritRate;
            lhs.CritDamage += rhs.CritDamage;
            lhs.Resistance += rhs.Resistance;
            lhs.Accuracy += rhs.Accuracy;
            lhs.EffectiveHP += rhs.EffectiveHP;
            lhs.EffectiveHPDefenseBreak += rhs.EffectiveHPDefenseBreak;
            lhs.DamagePerSpeed += rhs.DamagePerSpeed;
            lhs.AverageDamage += rhs.AverageDamage;
            lhs.MaxDamage += rhs.MaxDamage;
            return lhs;
        }

        public static Stats operator/(Stats lhs, int rhs)
        {
            lhs.Health /= rhs;
            lhs.Attack /= rhs;
            lhs.Defense /= rhs;
            lhs.Speed /= rhs;
            lhs.CritRate /= rhs;
            lhs.CritDamage /= rhs;
            lhs.Resistance /= rhs;
            lhs.Accuracy /= rhs;
            lhs.EffectiveHP /= rhs;
            lhs.EffectiveHPDefenseBreak /= rhs;
            lhs.DamagePerSpeed /= rhs;
            lhs.AverageDamage /= rhs;
            lhs.MaxDamage /= rhs;
            return lhs;
        }

        public bool NonZero()
        {
            if (Accuracy != 0)
                return true;
            if (Attack != 0)
                return true;
            if (CritDamage != 0)
                return true;
            if (CritRate != 0)
                return true;
            if (Defense != 0)
                return true;
            if (Health != 0)
                return true;
            if (Resistance != 0)
                return true;
            if (Speed != 0)
                return true;

            if (EffectiveHP != 0)
                return true;
            if (EffectiveHPDefenseBreak != 0)
                return true;
            if (DamagePerSpeed != 0)
                return true;
            if (AverageDamage != 0)
                return true;
            if (MaxDamage != 0)
                return true;

            return false;
        }

        public Attr FirstNonZero()
        {
            if (Accuracy != 0)
                return Attr.Accuracy;
            if (Attack != 0)
                return Attr.AttackPercent;
            if (CritDamage != 0)
                return Attr.CritDamage;
            if (CritRate != 0)
                return Attr.CritRate;
            if (Defense != 0)
                return Attr.DefensePercent;
            if (Health != 0)
                return Attr.HealthPercent;
            if (Resistance != 0)
                return Attr.Resistance;
            if (Speed != 0)
                return Attr.SpeedPercent;
            
            return Attr.Null;
        }
    }
}
