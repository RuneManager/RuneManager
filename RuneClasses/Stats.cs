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
        public int Health = 0;

        [JsonProperty("b_atk")]
        public int Attack = 0;

        [JsonProperty("b_def")]
        public int Defense = 0;

        [JsonProperty("b_spd")]
        public int Speed = 0;

        [JsonProperty("b_crate")]
        public int CritRate = 0;

        [JsonProperty("b_cdmg")]
        public int CritDamage = 0;

        [JsonProperty("b_res")]
        public int Resistance = 0;

        [JsonProperty("b_acc")]
        public int Accuracy = 0;

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
        public int EffectiveHP = 0;

        [JsonProperty("fake_ehpdb")]
        public int EffectiveHPDefenseBreak = 0;
        
        [JsonProperty("fake_dps")]
        public int DamagePerSpeed = 0;

        [JsonProperty("fake_avd")]
        public int AverageDamage = 0;

        [JsonProperty("fake_mxd")]
        public int MaxDamage = 0;

        // Gets the Extra stat manually stored (for scoring)
        public int ExtraGet(string extra)
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
        public int ExtraValue(string extra)
        {
            switch (extra)
            {
                case "EHP":
                    return (int)(Health / ((1000 / (double)(1000 + Defense * 3))));
                case "EHPDB":
                    return (int)(Health / ((1000 / (double)(1000 + Defense * 1.5))));
                case "DPS":
                    return (int)(ExtraValue("AvD") * (Speed / (double) 100));
                case "AvD":
                    return (int)(ExtraValue("MxD") * (Math.Max(CritRate, 100) / (double) 100));
                case "MxD":
                    return (int)(Attack * CritDamage / (double)100);
            }
            return 0;
        }

        // manually sets the Extra stat (used for scoring)
        public void ExtraSet(string extra, int value)
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

        // Allows speedy iteration through the entity
        public int this[string stat]
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
            return rhs.Greater(lhs);
        }

        public static bool operator >(Stats lhs, Stats rhs)
        {
            return lhs.Greater(rhs);
        }

        internal bool Greater(Stats rhs)
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
    }
}
