using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RuneOptim
{
    // Allows me to steal the JSON values into Enum
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Attr
    {
        [EnumMember(Value = "-")]
        Neg = -1,

        [EnumMember(Value = "")]
        Null = 0,

        [EnumMember(Value = "HP flat")]
        HealthFlat = 1,

        [EnumMember(Value = "HP%")]
        HealthPercent = 2,

        [EnumMember(Value = "ATK flat")]
        AttackFlat = 3,

        [EnumMember(Value = "ATK%")]
        AttackPercent = 4,

        [EnumMember(Value = "DEF flat")]
        DefenseFlat = 5,

        [EnumMember(Value = "DEF%")]
        DefensePercent = 6,

        // Thanks Swift -_-
        SpeedPercent = 7,

        [EnumMember(Value = "SPD")]
        Speed = 8,

        [EnumMember(Value = "CRate")]
        CritRate = 9,

        [EnumMember(Value = "CDmg")]
        CritDamage = 10,

        [EnumMember(Value = "RES")]
        Resistance = 11,

		[EnumMember(Value = "ACC")]
		Accuracy = 12,

		// Flag for below
		ExtraStat = 16,

        [EnumMember(Value = "EHP")]
        EffectiveHP = 1 | ExtraStat,

        [EnumMember(Value = "EHPDB")]
        EffectiveHPDefenseBreak = 2 | ExtraStat,

        [EnumMember(Value = "DPS")]
        DamagePerSpeed = 3 | ExtraStat,

        [EnumMember(Value = "AvD")]
        AverageDamage = 4 | ExtraStat,

        [EnumMember(Value = "MxD")]
        MaxDamage = 5 | ExtraStat
    }

    public enum AttributeCategory
    {
        Neutral,
        Offensive,
        Defensive,
        Support
    }

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
        public Stats(Stats rhs, bool copyExtra = false)
        {
            Health = rhs.Health;
            Attack = rhs.Attack;
            Defense = rhs.Defense;
            Speed = rhs.Speed;
            CritRate = rhs.CritRate;
            CritDamage = rhs.CritDamage;
            Resistance = rhs.Resistance;
            Accuracy = rhs.Accuracy;
            if (copyExtra)
            {
                EffectiveHP = rhs.EffectiveHP;
                EffectiveHPDefenseBreak = rhs.EffectiveHPDefenseBreak;
                DamagePerSpeed = rhs.DamagePerSpeed;
                AverageDamage = rhs.AverageDamage;
                MaxDamage = rhs.MaxDamage;
            }
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
                default:
                    return 0;
            }
        }

        public double ExtraGet(Attr extra)
        {
            switch (extra)
            {
                case Attr.EffectiveHP:
                    return EffectiveHP;
                case Attr.EffectiveHPDefenseBreak:
                    return EffectiveHPDefenseBreak;
                case Attr.DamagePerSpeed:
                    return DamagePerSpeed;
                case Attr.AverageDamage:
                    return AverageDamage;
                case Attr.MaxDamage:
                    return MaxDamage;
                default:
                    return 0;
            }
        }

        // Computes and returns the Extra stat
        public double ExtraValue(string extra)
        {
            switch (extra)
            {
                case "EHP":
                    return (Health / ((1000 / (1000 + Defense * 3))));
                case "EHPDB":
                    return (Health / ((1000 / (1000 + Defense * 3 * 0.3))));
                case "DPS":
                    return (ExtraValue("AvD") * (Speed / 100));
                case "AvD":
                    return Attack + (Attack * CritDamage / 100 * (Math.Min(CritRate, 100) / 100));
                case "MxD":
                    return (Attack * (1 + CritDamage / 100));
                default:
                    return 0;
            }
        }

        public double ExtraValue(Attr extra)
        {
            switch (extra)
            {
                case Attr.EffectiveHP:
                    return (Health / ((1000 / (1000 + Defense * 3))));
                case Attr.EffectiveHPDefenseBreak:
                    return (Health / ((1000 / (1000 + Defense * 3 * 0.3))));
                case Attr.DamagePerSpeed:
                    return (ExtraValue("AvD") * (Speed / 100));
                case Attr.AverageDamage:
                    return Attack + (Attack * CritDamage / 100 * (Math.Min(CritRate, 100) / 100));
                case Attr.MaxDamage:
                    return (Attack * (1 + CritDamage / 100));
                default:
                return 0;
            }
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
                default:
                    return;
            }
        }

        // manually sets the Extra stat (used for scoring)
        public void ExtraSet(Attr extra, double value)
        {
            switch (extra)
            {
                case Attr.EffectiveHP:
                    EffectiveHP = value;
                    break;
                case Attr.EffectiveHPDefenseBreak:
                    EffectiveHPDefenseBreak = value;
                    break;
                case Attr.DamagePerSpeed:
                    DamagePerSpeed = value;
                    break;
                case Attr.AverageDamage:
                    AverageDamage = value;
                    break;
                case Attr.MaxDamage:
                    MaxDamage = value;
                    break;
                default:
                    return;
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
                // TODO: switch from using [string] to [Attr]
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
                    default:
                        return 0;
                }
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
                    default:
                        return;
                }
            }

        }

        public double this[Attr stat]
        {
            get
            {
                switch (stat)
                {
                    case Attr.HealthFlat:
                    case Attr.HealthPercent:
                        return Health;
                    case Attr.AttackFlat:
                    case Attr.AttackPercent:
                        return Attack;
                    case Attr.DefenseFlat:
                    case Attr.DefensePercent:
                        return Defense;
                    case Attr.Speed:
                    case Attr.SpeedPercent:
                        return Speed;
                    case Attr.CritDamage:
                        return CritDamage;
                    case Attr.CritRate:
                        return CritRate;
                    case Attr.Accuracy:
                        return Accuracy;
                    case Attr.Resistance:
                        return Resistance;
                    case Attr.EffectiveHP:
                        return EffectiveHP;
                    case Attr.EffectiveHPDefenseBreak:
                        return EffectiveHPDefenseBreak;
                    case Attr.DamagePerSpeed:
                        return DamagePerSpeed;
                    case Attr.AverageDamage:
                        return AverageDamage;
                    case Attr.MaxDamage:
                        return MaxDamage;

                }
                return 0;
            }

            set
            {
                switch (stat)
                {
                    case Attr.HealthFlat:
                    case Attr.HealthPercent:
                        Health = value;
                        break;
                    case Attr.AttackFlat:
                    case Attr.AttackPercent:
                        Attack = value;
                        break;
                    case Attr.DefenseFlat:
                    case Attr.DefensePercent:
                        Defense = value;
                        break;
                    case Attr.Speed:
                    case Attr.SpeedPercent:
                        Speed = value;
                        break;
                    case Attr.CritDamage:
                        CritDamage = value;
                        break;
                    case Attr.CritRate:
                        CritRate = value;
                        break;
                    case Attr.Accuracy:
                        Accuracy = value;
                        break;
                    case Attr.Resistance:
                        Resistance = value;
                        break;
                    case Attr.EffectiveHP:
                        EffectiveHP = value;
                        break;
                    case Attr.EffectiveHPDefenseBreak:
                        EffectiveHPDefenseBreak = value;
                        break;
                    case Attr.DamagePerSpeed:
                        DamagePerSpeed = value;
                        break;
                    case Attr.AverageDamage:
                        AverageDamage = value;
                        break;
                    case Attr.MaxDamage:
                        MaxDamage = value;
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

        /// <summary>
        /// Compares this to rhs returning if any non-zero attribute on RHS is exceeded by this.
        /// </summary>
        /// <param name="rhs">Stats to compare to</param>
        /// <returns>If any values in this Stats are greater than rhs</returns>
        public bool CheckMax(Stats rhs)
        {
            return Build.statEnums.Any(s => rhs[s] > 0 && this[s] > rhs[s]);

            /*
            foreach (var stat in Build.statEnums)
            {
                if (rhs[stat] > 0 && this[stat] > rhs[stat])
                    return true;
            }

            if (rhs.Health > 0 && Health > rhs.Health)
                return true;
            if (rhs.Attack > 0 && Attack > rhs.Attack)
                return true;
            if (rhs.Defense > 0 && Defense > rhs.Defense)
                return true;
            if (rhs.Speed > 0 && Speed > rhs.Speed)
                return true;
            if (rhs.CritRate > 0 && CritRate > rhs.CritRate)
                return true;
            if (rhs.CritDamage > 0 && CritDamage > rhs.CritDamage)
                return true;
            if (rhs.Resistance > 0 && Resistance > rhs.Resistance)
                return true;
            if (rhs.Accuracy > 0 && Accuracy > rhs.Accuracy)
                return true;

            return false;
            */
        }

        public bool GreaterEqual(Stats rhs, bool extraGet = false)
        {
            if (Accuracy < rhs.Accuracy)
                return false;
            if (Attack < rhs.Attack)
                return false;
            if (CritDamage < rhs.CritDamage)
                return false;
            if (CritRate < rhs.CritRate)
                return false;
            if (Defense < rhs.Defense)
                return false;
            if (Health < rhs.Health)
                return false;
            if (Resistance < rhs.Resistance)
                return false;
            if (Speed < rhs.Speed)
                return false;

            if (!extraGet) return true;

            if (ExtraValue(Attr.EffectiveHP) < rhs.EffectiveHP)
                return false;
            if (ExtraValue(Attr.EffectiveHPDefenseBreak) < rhs.EffectiveHPDefenseBreak)
                return false;
            if (ExtraValue(Attr.DamagePerSpeed) < rhs.DamagePerSpeed)
                return false;
            if (ExtraValue(Attr.AverageDamage) < rhs.AverageDamage)
                return false;
            if (ExtraValue(Attr.MaxDamage) < rhs.MaxDamage)
                return false;

            return true;
        }

        public static Stats operator +(Stats lhs, Stats rhs)
        {
            Stats ret = new Stats(lhs, true);
            ret.Health += rhs.Health;
            ret.Attack += rhs.Attack;
            ret.Defense += rhs.Defense;
            ret.Speed += rhs.Speed;
            ret.CritRate += rhs.CritRate;
            ret.CritDamage += rhs.CritDamage;
            ret.Resistance += rhs.Resistance;
            ret.Accuracy += rhs.Accuracy;
            ret.EffectiveHP += rhs.EffectiveHP;
            ret.EffectiveHPDefenseBreak += rhs.EffectiveHPDefenseBreak;
            ret.DamagePerSpeed += rhs.DamagePerSpeed;
            ret.AverageDamage += rhs.AverageDamage;
            ret.MaxDamage += rhs.MaxDamage;
            return ret;
        }

        public static Stats operator -(Stats lhs, Stats rhs)
        {
            Stats ret = new Stats(lhs, true);
            ret.Health -= rhs.Health;
            ret.Attack -= rhs.Attack;
            ret.Defense -= rhs.Defense;
            ret.Speed -= rhs.Speed;
            ret.CritRate -= rhs.CritRate;
            ret.CritDamage -= rhs.CritDamage;
            ret.Resistance -= rhs.Resistance;
            ret.Accuracy -= rhs.Accuracy;
            ret.EffectiveHP -= rhs.EffectiveHP;
            ret.EffectiveHPDefenseBreak -= rhs.EffectiveHPDefenseBreak;
            ret.DamagePerSpeed -= rhs.DamagePerSpeed;
            ret.AverageDamage -= rhs.AverageDamage;
            ret.MaxDamage -= rhs.MaxDamage;
            return ret;
        }

        public static Stats operator /(Stats lhs, double rhs)
        {
            Stats ret = new Stats(lhs, true);
            ret.Health /= rhs;
            ret.Attack /= rhs;
            ret.Defense /= rhs;
            ret.Speed /= rhs;
            ret.CritRate /= rhs;
            ret.CritDamage /= rhs;
            ret.Resistance /= rhs;
            ret.Accuracy /= rhs;
            ret.EffectiveHP /= rhs;
            ret.EffectiveHPDefenseBreak /= rhs;
            ret.DamagePerSpeed /= rhs;
            ret.AverageDamage /= rhs;
            ret.MaxDamage /= rhs;
            return ret;
        }

        public static Stats operator /(Stats lhs, Stats rhs)
        {
            Stats ret = new Stats(lhs, true);
            
            foreach (var a in Build.statEnums)
            {
                if (rhs[a].EqualTo(0))
                    ret[a] = 0;
                else
                    ret[a] /= rhs[a];
            }

            foreach (var a in Build.extraEnums)
            {
                if (rhs[a].EqualTo(0))
                    ret[a] = 0;
                else
                    ret[a] /= rhs[a];
            }

            return ret;
        }

        // how much % of the 3 pris RHS needs to get to this
        public Stats Of(Stats rhs)
        {
            Stats ret = new Stats(this);

            if (rhs.Health > 0)
                ret.Health /= rhs.Health;
            if (rhs.Attack > 0)
                ret.Attack /= rhs.Attack;
            if (rhs.Defense > 0)
                ret.Defense /= rhs.Defense;


            return ret;
        }

        // boots this by RHS
        // assumes A/D/H/S are 100.00 instead of 1.00 (leader/shrine)
        public Stats Boost(Stats rhs)
        {
            Stats ret = new Stats(this);

            ret.Attack *= 1 + rhs.Attack / 100;
            ret.Defense *= 1 + rhs.Defense / 100;
            ret.Health *= 1 + rhs.Health / 100;
            ret.Speed *= 1 + rhs.Speed / 100;

            ret.CritRate += rhs.CritRate;
            ret.CritDamage += rhs.CritDamage;
            ret.Accuracy += rhs.Accuracy;
            ret.Resistance += rhs.Resistance;

            return ret;
        }

        public bool NonZero()
        {
            if (!Accuracy.EqualTo(0))
                return true;
            if (!Attack.EqualTo(0))
                return true;
            if (!CritDamage.EqualTo(0))
                return true;
            if (!CritRate.EqualTo(0))
                return true;
            if (!Defense.EqualTo(0))
                return true;
            if (!Health.EqualTo(0))
                return true;
            if (!Resistance.EqualTo(0))
                return true;
            if (!Speed.EqualTo(0))
                return true;

            if (!EffectiveHP.EqualTo(0))
                return true;
            if (!EffectiveHPDefenseBreak.EqualTo(0))
                return true;
            if (!DamagePerSpeed.EqualTo(0))
                return true;
            if (!AverageDamage.EqualTo(0))
                return true;
            if (!MaxDamage.EqualTo(0))
                return true;

            return false;
        }

        public Attr FirstNonZero()
        {
            if (!Accuracy.EqualTo(0))
                return Attr.Accuracy;
            if (!Attack.EqualTo(0))
                return Attr.AttackPercent;
            if (!CritDamage.EqualTo(0))
                return Attr.CritDamage;
            if (!CritRate.EqualTo(0))
                return Attr.CritRate;
            if (!Defense.EqualTo(0))
                return Attr.DefensePercent;
            if (!Health.EqualTo(0))
                return Attr.HealthPercent;
            if (!Resistance.EqualTo(0))
                return Attr.Resistance;
            if (!Speed.EqualTo(0))
                return Attr.SpeedPercent;

            return Attr.Null;
        }
    }
}
