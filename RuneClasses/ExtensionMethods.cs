using System.Collections.Generic;

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
}
