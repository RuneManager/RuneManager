using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace RuneOptim
{
    // WON'T COMPILE
    class AttrComparer : IEqualityComparer<Attr>
    {
        public bool Equals(Attr x, Attr y)
        {
            return (x == y);
        }
        public int GetHashCode(Attr obj)
        {
            return (int)obj;
        }
    }

    public static class ExtensionMethods
    {
        public static bool EqualTo(this double a, double b, double within = 0.00000001)
        {
            return (Math.Abs(a - b) < within);
        }

        public static bool IsA(this Type type, Type typeToBe)
        {
            if (!typeToBe.IsGenericTypeDefinition)
                return typeToBe.IsAssignableFrom(type);

            var toCheckTypes = new List<Type> { type };
            if (typeToBe.IsInterface)
                toCheckTypes.AddRange(type.GetInterfaces());

            var basedOn = type;
            while (basedOn.BaseType != null)
            {
                toCheckTypes.Add(basedOn.BaseType);
                basedOn = basedOn.BaseType;
            }

            return toCheckTypes.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeToBe);
        }

        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static SlotIndex GetIndex(string str)
        {
            int v;
            if (int.TryParse(str, out v))
                return (SlotIndex)v;

            if (str == "g" || str == "Global")
                return SlotIndex.Global;
            if (str == "e" || str == "Even")
                return SlotIndex.Even;
            if (str == "o" || str == "Odd")
                return SlotIndex.Odd;

            throw new Exception();
        }

        // Enable casting Attr enums to a string
        public static string ToForms(this Attr attr)
        {
            switch (attr)
            {
                case Attr.Null:
                    return "null";
                case Attr.Accuracy:
                    return "ACCperc";
                case Attr.AttackFlat:
                    return "ATKflat";
                case Attr.AttackPercent:
                    return "ATKperc";
                case Attr.CritDamage:
                    return "CDperc";
                case Attr.CritRate:
                    return "CRperc";
                case Attr.DefenseFlat:
                    return "DEFflat";
                case Attr.DefensePercent:
                    return "DEFperc";
                case Attr.HealthFlat:
                    return "HPflat";
                case Attr.HealthPercent:
                    return "HPperc";
                case Attr.Resistance:
                    return "RESperc";
                case Attr.Speed:
                    return "SPDflat";
                case Attr.SpeedPercent:
                    return "SPDperc";
                case Attr.ExtraStat:
                    return "Ext";
                case Attr.EffectiveHP:
                    return "EHP";
                case Attr.EffectiveHPDefenseBreak:
                    return "EHPDB";
                case Attr.DamagePerSpeed:
                    return "DPS";
                case Attr.AverageDamage:
                    return "AvD";
                case Attr.MaxDamage:
                    return "MxD";
                default:
                    return "unhandled";
            }

        }
        // Enable casting Attr enums to a string
        public static string ToShortForm(this Attr attr)
        {
            switch (attr)
            {
                case Attr.Null:
                    return "-";
                case Attr.Accuracy:
                    return "ACC";
                case Attr.AttackFlat:
                case Attr.AttackPercent:
                    return "ATK";
                case Attr.CritDamage:
                    return "CD";
                case Attr.CritRate:
                    return "CR";
                case Attr.DefenseFlat:
                case Attr.DefensePercent:
                    return "DEF";
                case Attr.HealthFlat:
                case Attr.HealthPercent:
                    return "HP";
                case Attr.Resistance:
                    return "RES";
                case Attr.Speed:
                case Attr.SpeedPercent:
                    return "SPD";
                case Attr.ExtraStat:
                    return "Ext";
                case Attr.EffectiveHP:
                    return "EHP";
                case Attr.EffectiveHPDefenseBreak:
                    return "EHPDB";
                case Attr.DamagePerSpeed:
                    return "DPS";
                case Attr.AverageDamage:
                    return "AvD";
                case Attr.MaxDamage:
                    return "MxD";
                default:
                    return "_";
            }
        }

        // Enable casting Attr enums to a string
        public static string ToGameString(this Attr attr)
        {
            switch (attr)
            {
                case Attr.Null:
                case Attr.ExtraStat:
                case Attr.EffectiveHP:
                case Attr.EffectiveHPDefenseBreak:
                case Attr.DamagePerSpeed:
                case Attr.AverageDamage:
                case Attr.MaxDamage:
                    return "";
                case Attr.Accuracy:
                    return "ACC%";
                case Attr.AttackFlat:
                    return "ATK";
                case Attr.AttackPercent:
                    return "ATK%";
                case Attr.CritDamage:
                    return "CD%";
                case Attr.CritRate:
                    return "CR%";
                case Attr.DefenseFlat:
                    return "DEF";
                case Attr.DefensePercent:
                    return "DEF%";
                case Attr.HealthFlat:
                    return "HP";
                case Attr.HealthPercent:
                    return "HP%";
                case Attr.Resistance:
                    return "RES%";
                case Attr.Speed:
                    return "SPD";
                case Attr.SpeedPercent:
                    return "SPD%";
                default:
                    return "unhandled";
            }
        }
    }

    
    public class DictionaryWithSpecialEnumKeyConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var valueType = objectType.GetGenericArguments()[1];
            var intermediateDictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
            var intermediateDictionary = (IDictionary)Activator.CreateInstance(intermediateDictionaryType);
            serializer.Populate(reader, intermediateDictionary);

            var finalDictionary = (IDictionary)Activator.CreateInstance(objectType);
            foreach (DictionaryEntry pair in intermediateDictionary)
            {
                SlotIndex ind;
                if (!Enum.TryParse(pair.Key.ToString(), true, out ind))
                {
                    foreach (var q in Enum.GetValues(typeof(SlotIndex)))
                    {
                        var qw = (SlotIndex)q;
                        if (qw.GetAttributeOfType<EnumMemberAttribute>().Value == pair.Key.ToString())
                        {
                            ind = qw;
                            break;
                        }
                    }
                }

                finalDictionary.Add(ind, pair.Value);
            }

            return finalDictionary;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsA(typeof(IDictionary<,>)) &&
                   objectType.GetGenericArguments()[0].IsA(typeof(SlotIndex));
        }
    }

}
