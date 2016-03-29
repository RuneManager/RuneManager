using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneOptim
{
    public static class ExtensionMethods
    {
        
        // Enable casting Attr enums to a string
        public static string ToForms(this Attr attr)
        {
            switch (attr)
            {
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
            }

            return "null";
        }
    }
}
