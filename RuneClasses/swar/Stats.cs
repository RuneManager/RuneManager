//#define FOR_THREADS

using System;
using System.Linq;
using Newtonsoft.Json;
using System.Linq.Expressions;
using RuneOptim.BuildProcessing;
using System.Collections.Generic;

namespace RuneOptim.swar {

    public class Stats {
        // allows mapping save.json into the program via Monster
        [JsonProperty("con")]
        public double _con = 0;

        [JsonProperty("hp", NullValueHandling = NullValueHandling.Ignore)]
        public double? _health = null;

        // TODO: should I set con?
        [JsonIgnore]
        public double Health { get { return _health ?? _con * 15; } set { _con = value / 15.0; _health = value; } }

        [JsonProperty("atk")]
        private double attack = 0;

        [JsonIgnore]
        public double Attack {
            get { return attack; }
            set { attack = value; OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.AttackFlat, value)); OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.AttackPercent, value)); }
        }

        [JsonProperty("def")]
        public double Defense = 0;

        [JsonProperty("spd")]
        public double Speed = 0;

        [JsonProperty("critical_rate")]
        public double CritRate = 0;

        [JsonProperty("critical_damage")]
        public double CritDamage = 0;

        [JsonProperty("resist")]
        public double Resistance = 0;

        [JsonProperty("accuracy")]
        public double Accuracy = 0;

        [JsonIgnore]
        public double SkillupDamage = 0;

        [JsonProperty("skillup_damage")]
        private double[] damageSkillups = null;

        [JsonIgnore]
        public double[] DamageSkillups {
            get {
                if (damageSkillups == null)
                    damageSkillups = new double[8];
                return damageSkillups;
            }
            set {
                damageSkillups = value;
            }
        }


        public void DamageSkillupsSet(int ind, double val) {
            DamageSkillups[ind] = val;
            OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.Skill1 + ind, val));
        }

        public bool ShouldSerializedamageSkillups() {
            return damageSkillups != null && damageSkillups.Any(d => d != 0);
        }

        [JsonProperty("skill_cooltime")]
        private int[] skillTimes = null;

        public int[] SkillTimes {
            get {
                if (skillTimes == null)
                    skillTimes = new int[8];
                return skillTimes;
            }
            set {
                skillTimes = value;
            }
        }

        public bool ShouldSerializeskillTimes() {
            return skillTimes != null && skillTimes.Any(i => i != 0);
        }

        [JsonIgnore]
        private int[] skillupLevel = null;

        [JsonIgnore]
        public int[] SkillupLevel {
            get {
                if (skillupLevel == null)
                    skillupLevel = new int[8];
                return skillupLevel;
            }
            set {
                skillupLevel = value;
            }
        }

        [JsonIgnore]
        private int[] skillupMax = null;

        [JsonIgnore]
        public int[] SkillupMax {
            get {
                if (skillupMax == null)
                    skillupMax = new int[8];
                return skillupMax;
            }
            set {
                skillupMax = value;
            }
        }

        [JsonIgnore]
        private int extraCritRate = 0;
        public virtual int ExtraCritRate {
            get {
                return extraCritRate;
            }
            set {
                extraCritRate = value;
            }
        }

        public event EventHandler<StatModEventArgs> OnStatChanged;

        public Stats() { }

        public Stats(double i) { SetTo(i); }

        // copy constructor, amrite?
        public Stats(Stats rhs, bool copyExtra = false) {
            CopyFrom(rhs, copyExtra);
        }

        public void CopyFrom(Stats rhs, bool copyExtra = false) {
            Health = rhs.Health;
            attack = rhs.Attack;
            Defense = rhs.Defense;
            Speed = rhs.Speed;
            CritRate = rhs.CritRate;
            CritDamage = rhs.CritDamage;
            Resistance = rhs.Resistance;
            Accuracy = rhs.Accuracy;

            damageFormula = rhs.damageFormula;
            _damageFormula = rhs._damageFormula;

            //rhs._skillsFormula.CopyTo(_skillsFormula, 0);
            //rhs.DamageSkillups.CopyTo(DamageSkillups, 0);
            _skillsFormula = rhs._skillsFormula;
            //DamageSkillups = rhs.DamageSkillups.ToArray();
            ExtraCritRate = rhs.ExtraCritRate;

            SkillupDamage = rhs.SkillupDamage;

            if (copyExtra) {
                EffectiveHP = rhs.EffectiveHP;
                EffectiveHPDefenseBreak = rhs.EffectiveHPDefenseBreak;
                DamagePerSpeed = rhs.DamagePerSpeed;
                AverageDamage = rhs.AverageDamage;
                MaxDamage = rhs.MaxDamage;
                // danger zone
                DamageSkillups = rhs.DamageSkillups;
                nonZero = rhs.NonZeroStats.ToArray();

                //OnStatChanged += rhs.OnStatChanged;
                /*foreach (var target in rhs.OnStatChanged.GetInvocationList())
                {
                    var mi = target.Method;
                    var del = Delegate.CreateDelegate(
                                  typeof(EventHandler<StatModEventArgs>), this, mi.Name);
                    OnStatChanged += (EventHandler<StatModEventArgs>)del;
                }*/
            }
            else {
#if FOR_THREADS
                DamageSkillups = rhs.DamageSkillups.ToArray();
                nonZero = rhs.nonZero.ToArray();
#else
                DamageSkillups = rhs.DamageSkillups;
                nonZero = rhs.nonZero;
#endif
            }
        }

        // fake "stats", need to be stored for scoring
        [JsonProperty("fake_ehp")]
        public double EffectiveHP = 0;

        public bool ShouldSerializeEffectiveHP() {
            return EffectiveHP != 0;
        }

        [JsonProperty("fake_ehpdb")]
        public double EffectiveHPDefenseBreak = 0;

        public bool ShouldSerializeEffectiveHPDefenseBreak() {
            return EffectiveHPDefenseBreak != 0;
        }

        [JsonProperty("fake_dps")]
        public double DamagePerSpeed = 0;

        public bool ShouldSerializeDamagePerSpeed() {
            return DamagePerSpeed != 0;
        }

        [JsonProperty("fake_avd")]
        public double AverageDamage = 0;

        public bool ShouldSerializeAverageDamage() {
            return AverageDamage != 0;
        }

        [JsonProperty("fake_mxd")]
        public double MaxDamage = 0;

        public bool ShouldSerializeMaxDamage() {
            return MaxDamage != 0;
        }

        [JsonIgnore]
        public MonsterDefinitions.MultiplierBase damageFormula = null;

        [JsonIgnore]
        protected readonly static ParameterExpression statType = Expression.Parameter(typeof(Stats), "stats");

        [JsonIgnore]
        protected Func<Stats, double> _damageFormula = null;

        [JsonIgnore]
        protected Func<Stats, double>[] _skillsFormula = null;

        [JsonIgnore]
        protected Func<Stats, double>[] _SkillsFormula {
            get {
                if (_skillsFormula == null)
                    _skillsFormula = new Func<Stats, double>[8];
                return _skillsFormula;
            }
        }

        [JsonIgnore]
        protected Expression __form = null;

        [JsonIgnore]
        public Func<Stats, double> DamageFormula {
            get {
                if (_damageFormula == null) {
                    BakeDamageFormula();
                }
                return _damageFormula;
            }
        }

        public void BakeDamageFormula() {
            if (_damageFormula == null && damageFormula != null) {
                __form = damageFormula.AsExpression(statType);
                _damageFormula = Expression.Lambda<Func<Stats, double>>(__form, statType).Compile();
            }
        }

        [JsonIgnore]
        public Func<Stats, double>[] SkillFunc {
            get {
                return _SkillsFormula;
            }
        }

        public double GetSkillMultiplier(int skillNum, Stats applyTo = null) {
            if (_SkillsFormula.Length < skillNum || _SkillsFormula[skillNum] == null)
                return 0;
            if (applyTo == null)
                applyTo = this;
            return _SkillsFormula[skillNum](applyTo);
        }

        public double GetSkillDamage(Attr type, int skillNum, Stats applyTo = null) {
            var mult = GetSkillMultiplier(skillNum, applyTo);

            if (type == Attr.MaxDamage) {
                return mult * (1 + CritDamage * 0.01 + DamageSkillups[skillNum] * 0.01);
            }
            else if (type == Attr.AverageDamage) {
                return mult * (1 + CritDamage * 0.01 * Math.Min(100, CritRate + ExtraCritRate) * 0.01 + DamageSkillups[skillNum] * 0.01);
            }
            else if (type == Attr.DamagePerSpeed) {
                return mult * (1 + CritDamage * 0.01 * Math.Min(100, CritRate + ExtraCritRate) * 0.01 + DamageSkillups[skillNum] * 0.01) * Speed / SkillTimes[skillNum];
            }

            return mult;
        }

        // Gets the Extra stat manually stored (for scoring)
        public double ExtraGet(string extra) {
            switch (extra) {
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
                    throw new NotImplementedException();
            }
        }

        public double ExtraGet(Attr extra) {
            switch (extra) {
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
                case Attr.Skill1:
                case Attr.Skill2:
                case Attr.Skill3:
                case Attr.Skill4:
                    return DamageSkillups[extra - Attr.Skill1];
                default:
                    throw new NotImplementedException();
            }
        }

        // Computes and returns the Extra stat
        public double ExtraValue(string extra) {
            switch (extra) {
                case "EHP":
                    return ExtraValue(Attr.EffectiveHP);
                case "EHPDB":
                    return ExtraValue(Attr.EffectiveHPDefenseBreak);
                case "DPS":
                    return ExtraValue(Attr.DamagePerSpeed);
                case "AvD":
                    return ExtraValue(Attr.AverageDamage);
                case "MxD":
                    return ExtraValue(Attr.MaxDamage);
                default:
                    throw new NotImplementedException();
            }
        }

        public double ExtraValue(Attr extra) {
            switch (extra) {
                case Attr.EffectiveHP:
                    return Health * (1140 + Defense * 3.5) * 0.001;
                case Attr.EffectiveHPDefenseBreak:
                    return Health * (1140 + Defense * 3.5 * 0.3) * 0.001;
                case Attr.DamagePerSpeed:
                    return ExtraValue(Attr.AverageDamage) * Speed * 0.01;
                case Attr.AverageDamage:
                    return (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage * 0.01 + CritDamage * 0.01 * Math.Min(CritRate + ExtraCritRate, 100) * 0.01);
                case Attr.MaxDamage:
                    return (DamageFormula?.Invoke(this) ?? Attack) * (1 + SkillupDamage * 0.01 + CritDamage * 0.01);
                default:
                    throw new NotImplementedException();
            }
        }

        // manually sets the Extra stat (used for scoring)
        public void ExtraSet(string extra, double value) {
            switch (extra) {
                case "EHP":
                    EffectiveHP = value;
                    OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.EffectiveHP, value));
                    break;
                case "EHPDB":
                    EffectiveHPDefenseBreak = value;
                    OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.EffectiveHPDefenseBreak, value));
                    break;
                case "DPS":
                    DamagePerSpeed = value;
                    OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.DamagePerSpeed, value));
                    break;
                case "AvD":
                    AverageDamage = value;
                    OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.AverageDamage, value));
                    break;
                case "MxD":
                    MaxDamage = value;
                    OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.MaxDamage, value));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        // manually sets the Extra stat (used for scoring)
        public void ExtraSet(Attr extra, double value) {
            switch (extra) {
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
                case Attr.Skill1:
                case Attr.Skill2:
                case Attr.Skill3:
                case Attr.Skill4:
                    DamageSkillups[extra - Attr.Skill1] = value;
                    break;
                default:
                    throw new NotImplementedException();
            }
            OnStatChanged?.Invoke(this, new StatModEventArgs(extra, value));
        }

        public double Sum() {
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
                + MaxDamage
                + DamageSkillups.Sum();
        }

        // Allows speedy iteration through the entity
        public double this[string stat] {
            get {
                // TODO: switch from using [string] to [Attr]
                switch (stat) {
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
                    case "WaterATK":
                        return DamageSkillups[(int)Element.Water - 1];
                    case "FireATK":
                        return DamageSkillups[(int)Element.Fire - 1];
                    case "WindATK":
                        return DamageSkillups[(int)Element.Wind - 1];
                    case "LightATK":
                        return DamageSkillups[(int)Element.Light - 1];
                    case "DarkATK":
                        return DamageSkillups[(int)Element.Dark - 1];
                    default:
                        return 0;
                        //throw new NotImplementedException();
                }
            }

            set {
                switch (stat) {
                    case "HP":
                        Health = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.HealthFlat, value));
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.HealthPercent, value));
                        break;
                    case "ATK":
                        Attack = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.AttackFlat, value));
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.AttackPercent, value));
                        break;
                    case "DEF":
                        Defense = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.DefenseFlat, value));
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.DefensePercent, value));
                        break;
                    case "SPD":
                        Speed = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.Speed, value));
                        break;
                    case "CD":
                        CritDamage = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.CritDamage, value));
                        break;
                    case "CR":
                        CritRate = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.CritRate, value));
                        break;
                    case "ACC":
                        Accuracy = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.Accuracy, value));
                        break;
                    case "RES":
                        Resistance = value;
                        OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.Resistance, value));
                        break;
                    case "WaterATK":
                        DamageSkillups[0] = value;
                        break;
                    case "FireATK":
                        DamageSkillups[1] = value;
                        break;
                    case "WindATK":
                        DamageSkillups[2] = value;
                        break;
                    case "LightATK":
                        DamageSkillups[3] = value;
                        break;
                    case "DarkATK":
                        DamageSkillups[4] = value;
                        break;
                    default:
#if DEBUG
                        throw new NotImplementedException();
#else
                        break;
#endif
                }
            }

        }

        public double this[Attr stat] {
            get {
                switch (stat) {
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
                throw new NotImplementedException();
            }

            set {
                switch (stat) {
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
                    default:
                        throw new NotImplementedException();
                }
                OnStatChanged?.Invoke(this, new StatModEventArgs(stat, value));
            }

        }

        // Perfectly legit operator overloading to compare builds/minimum
        public static bool operator <(Stats lhs, Stats rhs) {
            return !lhs.GreaterEqual(rhs);
        }

        public static bool operator >=(Stats lhs, Stats rhs) {
            return lhs.GreaterEqual(rhs);
        }

        public static bool operator >(Stats lhs, Stats rhs) {
            return !rhs.GreaterEqual(lhs);
        }

        public static bool operator <=(Stats lhs, Stats rhs) {
            return rhs.GreaterEqual(lhs);
        }
        /// <summary>
        /// Compares this to rhs returning if any non-zero attribute on RHS is exceeded by this.
        /// </summary>
        /// <param name="rhs">Stats to compare to</param>
        /// <returns>If any values in this Stats are greater than rhs</returns>
        public bool AnyExceed(Stats rhs) {
            foreach (var a in rhs.NonZeroStats) {
                if (this[a] > rhs[a])
                    return true;
            }

            return false;
        }

        public bool AnyExceedCached(Stats rhs) {
            foreach (var a in rhs.NonZeroCached) {
                if (this[a] > rhs[a])
                    return true;
            }

            return false;
        }

        public static bool CheckMax(double lhs, double rhs) {
            return rhs != 0 && lhs > rhs;
        }

        public bool GreaterEqual(Stats rhs, bool extraGet = false) {
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

            if (_skillsFormula != null) {
                if (_skillsFormula[0] != null && GetSkillDamage(Attr.AverageDamage, 0) < rhs.DamageSkillups[0])
                    return false;
                if (_skillsFormula[1] != null && GetSkillDamage(Attr.AverageDamage, 1) < rhs.DamageSkillups[1])
                    return false;
                if (_skillsFormula[2] != null && GetSkillDamage(Attr.AverageDamage, 2) < rhs.DamageSkillups[2])
                    return false;
                if (_skillsFormula[3] != null && GetSkillDamage(Attr.AverageDamage, 3) < rhs.DamageSkillups[3])
                    return false;
            }

            return true;
        }

        public void SetTo(double v) {
            foreach (var a in Build.StatAll) {
                this[a] = v;
            }

            DamageSkillups = new double[8];
        }

        public Stats SetTo(Stats rhs) {
            foreach (var a in Build.StatAll) {
                this[a] = rhs[a];
            }
            for (int i = 0; i < 4; i++) {
                OnStatChanged?.Invoke(this, new StatModEventArgs(Attr.Skill1 + i, GetSkillDamage(Attr.AverageDamage, i, this)));
            }
            for (int i = 0; i < 8; i++) {
                DamageSkillups[i] = rhs.DamageSkillups[i];
            }
            return this;
        }

        public static Stats operator +(Stats lhs, Stats rhs) {
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

        public static Stats operator -(Stats lhs, Stats rhs) {
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

        public static Stats operator /(Stats lhs, double rhs) {
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

        public static Stats operator /(Stats lhs, Stats rhs) {
            Stats ret = new Stats(lhs, true);

            foreach (var a in Build.StatEnums) {
                if (rhs[a].EqualTo(0))
                    ret[a] = 0;
                else
                    ret[a] /= rhs[a];
            }

            foreach (var a in Build.ExtraEnums) {
                if (rhs[a].EqualTo(0))
                    ret[a] = 0;
                else
                    ret[a] /= rhs[a];
            }

            return ret;
        }

        // how much % of the 3 pris RHS needs to get to this
        public Stats Of(Stats rhs) {
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
        public Stats Boost(Stats rhs) {
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

        [JsonIgnore]
        Attr[] nonZero = null;

        public IEnumerable<Attr> NonZeroCached {
            get {
                if (nonZero == null)
                    nonZero = NonZeroStats.ToArray();
                return nonZero;
            }
        }

        public IEnumerable<Attr> NonZeroStats {
            get {
                foreach (Attr a in Build.StatAll) {
                    if (!this[a].EqualTo(0))
                        yield return a;
                }
                for (int i = 0; i < 4; i++) {
                    if (!DamageSkillups[i].EqualTo(0))
                        yield return Attr.Skill1 + i;
                }
            }
        }

        public bool IsNonZero {
            get {
                return NonZeroStats.Any();
            }
        }
    }
}
