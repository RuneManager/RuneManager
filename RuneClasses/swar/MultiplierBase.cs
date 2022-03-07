using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RuneOptim.swar;

namespace MonsterDefinitions {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MultiplierOperator {
        [EnumMember(Value = "+")]
        Add,
        [EnumMember(Value = "-")]
        Sub,
        [EnumMember(Value = "*")]
        Mult,
        [EnumMember(Value = "/")]
        Div,
        [EnumMember(Value = "=")]
        End,
        [EnumMember(Value = "FIXED")]
        Fixed
    }

    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class SkillAttrAttribute : Attribute {
        readonly string attrName;

        public SkillAttrAttribute(string name) {
            this.attrName = name;
        }

        public string MultiAttr {
            get { return attrName; }
        }

    }

    // Allows me to steal the JSON values into Enum
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MultiAttr {
        [EnumMember(Value = "-")]
        [SkillAttr("LIFE_SHARE_TARGET")]
        [SkillAttr("FIXED")]
        Neg = -1,

        [EnumMember(Value = "")]
        // TODO: FIXME:
        Null = 0,

        [EnumMember(Value = "HP flat")]
        [SkillAttr("HP")]
        [SkillAttr("ATTACK_TOT_HP")]
        [SkillAttr("LIFE_SHARE_ALL")]
        HealthFlat = 1,

        [EnumMember(Value = "HP%")]
        HealthPercent = 2,

        [EnumMember(Value = "ATK flat")]
        [SkillAttr("ATK")]
        AttackFlat = 3,

        [EnumMember(Value = "ATK%")]
        AttackPercent = 4,

        [EnumMember(Value = "DEF flat")]
        [SkillAttr("DEF")]
        DefenseFlat = 5,

        [EnumMember(Value = "DEF%")]
        DefensePercent = 6,

        // Thanks Swift -_-
        SpeedPercent = 7,

        [EnumMember(Value = "SPD")]
        [SkillAttr("ATTACK_SPEED")]
        [SkillAttr("RIDER_SPEED")]
        Speed = 8,

        [EnumMember(Value = "CRate")]
        CritRate = 9,

        [EnumMember(Value = "CDmg")]
        CritDamage = 10,

        [EnumMember(Value = "RES")]
        Resistance = 11,

        [EnumMember(Value = "ACC")]
        Accuracy = 12,

        [SkillAttr("DIE_RATE")]
        [SkillAttr("ATTACK_WIZARD_LIFE_RATE")]
        PercentOfAlliesAlive,

        [SkillAttr("TARGET_SPEED")]
        TargetSpeed,

        [SkillAttr("TARGET_TOT_HP")]
        TargetHealth,

        [SkillAttr("ATTACK_LOSS_HP")]
        MissingHealth,

        [SkillAttr("ATTACK_CUR_HP")]
        CurrentHealth,

        [SkillAttr("ATTACK_CUR_HP_RATE")]
        CurrentHealthPercent,

        [SkillAttr("TARGET_CUR_HP_RATE")]
        TargetHealthPercent,

        [SkillAttr("ATTACK_LV")]
        MonsterLevel,

        [SkillAttr("DICE")]
        DiceAverage,

        [SkillAttr("DICE_MIN")]
        DiceAverageTwoMin,

        [SkillAttr("TARGET_ALIVE_CNT")]
        TargetAliveCount,

        [SkillAttr("ALIVE_RATE")]
        PercentOfEnemiesAlive,
    }

    abstract public class MultiplierBase {
        abstract public double GetValue(Stats vals);
        abstract public Expression AsExpression(ParameterExpression statType);
    }

    public class MultiplierValue : MultiplierBase {
        public double? Value = null;
        public MultiplierBase Inner = null;
        public MultiAttr Key = MultiAttr.Null;

        public MultiplierOperator Op = MultiplierOperator.End;

        public MultiplierValue() {
        }

        public MultiplierValue(double v, MultiplierOperator o = MultiplierOperator.End) {
            Value = v;
            Op = o;
        }

        public MultiplierValue(MultiplierBase i, MultiplierOperator o = MultiplierOperator.End) {
            Inner = i;
            Op = o;
        }

        public MultiplierValue(MultiAttr a, MultiplierOperator o = MultiplierOperator.End) {
            Key = a;
            Op = o;
        }

        public override Expression AsExpression(ParameterExpression statType) {
            if (Inner != null) {
                return Inner.AsExpression(statType);
            }
            else if (Key != MultiAttr.Null) {//Expression.Parameter(typeof(RuneOptim.Stats), "stats")
                if (Key == MultiAttr.Neg)
                    return Expression.Constant(1.0);
                var attr = GetAttr(Key);
                if (attr <= RuneOptim.swar.Attr.Null)
                    return Expression.Constant(GetAttrValue(Key));
                else if (attr < RuneOptim.swar.Attr.Null)
                    return Expression.Multiply(Expression.Property(statType, "Item", Expression.Constant((Attr)(-(int)attr))), Expression.Constant(GetAttrValue(Key)));
                return Expression.Property(statType, "Item", Expression.Constant(attr));
            }
            else {
                return Expression.Constant(Value);
            }
        }

        public Attr GetAttr(MultiAttr mattr) {
            switch (mattr) {
                case MultiAttr.HealthFlat:
                case MultiAttr.HealthPercent:
                case MultiAttr.AttackFlat:
                case MultiAttr.AttackPercent:
                case MultiAttr.DefenseFlat:
                case MultiAttr.DefensePercent:
                case MultiAttr.SpeedPercent:
                case MultiAttr.Speed:
                case MultiAttr.CritRate:
                case MultiAttr.CritDamage:
                case MultiAttr.Resistance:
                case MultiAttr.Accuracy:
                    return (Attr)Key;
                case MultiAttr.Neg:
                case MultiAttr.Null:
                case MultiAttr.PercentOfAlliesAlive:
                case MultiAttr.TargetSpeed:
                case MultiAttr.TargetHealth:
                case MultiAttr.CurrentHealthPercent:
                case MultiAttr.TargetHealthPercent:
                case MultiAttr.MonsterLevel:
                case MultiAttr.DiceAverage:
                case MultiAttr.DiceAverageTwoMin:
                case MultiAttr.TargetAliveCount:
                case MultiAttr.PercentOfEnemiesAlive:
                default:
                    return RuneOptim.swar.Attr.Null;
                case MultiAttr.MissingHealth:
                case MultiAttr.CurrentHealth:
                    return (Attr)(-1 * (int)RuneOptim.swar.Attr.HealthFlat);
            }
        }

        /// <summary>
        /// Provide reasonable default values for dynamic skills
        /// </summary>
        /// <param name="mattr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public double GetAttrValue(MultiAttr mattr) {
            switch (mattr) {
                case MultiAttr.Neg:
                case MultiAttr.Null:
                    return 1;
                case MultiAttr.HealthFlat:
                case MultiAttr.HealthPercent:
                case MultiAttr.AttackFlat:
                case MultiAttr.AttackPercent:
                case MultiAttr.DefenseFlat:
                case MultiAttr.DefensePercent:
                case MultiAttr.SpeedPercent:
                case MultiAttr.Speed:
                case MultiAttr.CritRate:
                case MultiAttr.CritDamage:
                case MultiAttr.Resistance:
                case MultiAttr.Accuracy:
                    return 0;
                case MultiAttr.PercentOfAlliesAlive:
                case MultiAttr.PercentOfEnemiesAlive:
                    return 0.75;
                case MultiAttr.TargetSpeed:
                    return 200;
                case MultiAttr.TargetHealth:
                    return 200000; // If you're max-healthing, probably Giants/Dragon/Water dungeon
                case MultiAttr.MissingHealth: // TODO: Should TargetHealthPercent and MissingHealth be symetric?
                case MultiAttr.CurrentHealth:
                case MultiAttr.CurrentHealthPercent:
                    return 0.66;
                case MultiAttr.TargetHealthPercent:
                    return 0.75;
                case MultiAttr.MonsterLevel:
                    return 40;
                case MultiAttr.DiceAverage:
                    return 3.5;
                case MultiAttr.DiceAverageTwoMin:
                case MultiAttr.TargetAliveCount:
                    return 2.5;
                default:
                    break;
            }
            throw new Exception();

        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            if (Inner != null)
                sb.Append(Inner.ToString());
            else if (Key != MultiAttr.Null)
                sb.Append(GetEnumMemberAttrValue(Key));
            else
                sb.Append(Value);
            sb.Append(" ");
            sb.Append(GetEnumMemberAttrValue(Op));
            return sb.ToString();
        }

        public string GetEnumMemberAttrValue<T>(T enumVal) {
            var enumType = typeof(T);
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
            return attr?.Value;
        }

        public override double GetValue(Stats vals) {
            if (Inner != null)
                return Inner.GetValue(vals);
            else if (Key != MultiAttr.Null) {

            }
            else if (Value != null)
                return Value ?? 0;
            return 0;
        }
    }

    public class MultiplierGroup : MultiplierBase {
        public List<MultiplierValue> Props = new List<MultiplierValue>();

        public MultiplierGroup() {
        }

        public MultiplierGroup(params MultiplierValue[] vals) {
            foreach (var v in vals) {
                Props.Add(v);
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder("[");

            foreach (var prop in Props) {
                sb.Append(prop.ToString());
                sb.Append(" ");
            }
            sb.Append("]");

            return sb.ToString();
        }

        public override double GetValue(Stats vals) {
            if (Props.Count == 0)
                return 0;

            double ret = Props.First().GetValue(vals);

            var operate = Props.First().Op;

            foreach (var prop in Props.Skip(1)) {
                switch (operate) {
                    case MultiplierOperator.Add:
                        ret += prop.GetValue(vals);
                        break;
                    case MultiplierOperator.Sub:
                        ret -= prop.GetValue(vals);
                        break;
                    case MultiplierOperator.Mult:
                        ret *= prop.GetValue(vals);
                        break;
                    case MultiplierOperator.Div:
                        ret /= prop.GetValue(vals);
                        break;
                    case MultiplierOperator.End:
                        return ret;
                    default:
                        break;
                }
                operate = prop.Op;
            }

            return ret;
        }

        public override Expression AsExpression(ParameterExpression statType) {
            if (Props.Count == 0)
                return Expression.Constant(0.0);

            var express = Props.First().AsExpression(statType);
            var operate = Props.First().Op;

            foreach (var prop in Props.Skip(1)) {
                switch (operate) {
                    case MultiplierOperator.Add:
                        express = Expression.Add(express, prop.AsExpression(statType));
                        break;
                    case MultiplierOperator.Sub:
                        express = Expression.Subtract(express, prop.AsExpression(statType));
                        break;
                    case MultiplierOperator.Mult:
                        express = Expression.Multiply(express, prop.AsExpression(statType));
                        break;
                    case MultiplierOperator.Div:
                        express = Expression.Divide(express, prop.AsExpression(statType));
                        break;
                    case MultiplierOperator.End:
                        return express;
                    default:
                        break;
                }
                operate = prop.Op;
            }
            return express;

        }
    }

    public class MultiplierGroupConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(MultiplierGroup).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// This method converts skill strings into mathematical operations.
        /// </summary>
        /// <param name="jarray"></param>
        /// <returns></returns>
        private MultiplierGroup GetProp(JArray jarray) {
            MultiplierGroup multiGroup = new MultiplierGroup();
            for (int i = 0; i < jarray.Count; i += 2) {
                JToken jvalue = jarray[i];
                // Christina (wind cannon girl) has a formula without an operator 
                // "[[\"ATK\", \"*\", 1.0], [\"*\"], [5.4, \"ALIVE_RATE\"]]"
                if (jvalue.Count() == 2)
                    jvalue.First.AddAfterSelf("*");
                JToken joperator = (i + 1 < jarray.Count) ? jarray[i + 1] : "=";
                MultiplierValue value = new MultiplierValue();
                if (joperator is JArray) {
                    joperator = (joperator as JArray)[0];
                }
                value.Op = joperator.ToObject<MultiplierOperator>();
                if (jvalue is JArray) {
                    value.Inner = GetProp(jvalue as JArray);
                }
                else {
                    double tempval;
                    if (double.TryParse(jvalue.ToString(), out tempval)) {
                        value.Value = tempval;
                    }
                    else {
                        var tstr = jvalue.ToObject<string>();
                        if (tstr != "CEIL")
                            value.Key = GetStatAttrValue(tstr);
                    }
                }
                multiGroup.Props.Add(value);
            }
            return multiGroup;
        }

        public MultiAttr GetStatAttrValue(string str) {
            var enumType = typeof(MultiAttr);
            foreach (var enumVal in Enum.GetValues(enumType)) {
                var memInfo = enumType.GetMember(enumVal.ToString());
                var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<SkillAttrAttribute>().Any(m => m.MultiAttr == str);
                if (attr ?? false)
                    return (MultiAttr)enumVal;
            }
            throw new ArgumentException("str:" + str);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer) {
            return GetProp(JArray.Load(reader));
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
