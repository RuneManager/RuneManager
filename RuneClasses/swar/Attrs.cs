using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneOptim.swar
{
    public class Attrs
    {
        public double Health = 0;
        public double HealthFlat = 0;
        public double Attack = 0;
        public double AttackFlat = 0;
        public double Defense = 0;
        public double DefenseFlat = 0;
        public double SpeedPercent = 0;
        public double Speed = 0;
        public double CritRate = 0;
        public double CritDamage = 0;
        public double Resistance = 0;
        public double Accuracy = 0;

        public Attrs() { }

        // copy constructor, amrite?
        public Attrs(Attrs rhs)
        {
            CopyFrom(rhs);
        }
        public Attrs(Stats rhs)
        {
            Health = rhs.Health;
            Attack = rhs.Attack;
            Defense = rhs.Defense;
            SpeedPercent = rhs.Speed;
            CritRate = rhs.CritRate;
            CritDamage = rhs.CritDamage;
            Accuracy = rhs.Accuracy;
            Resistance = rhs.Resistance;
        }

        public void CopyFrom(Attrs rhs, bool copyExtra = false)
        {
            Health = rhs.Health;
            HealthFlat = rhs.HealthFlat;
            Attack = rhs.Attack;
            AttackFlat = rhs.AttackFlat;
            Defense = rhs.Defense;
            DefenseFlat = rhs.DefenseFlat;
            SpeedPercent = rhs.SpeedPercent;
            Speed = rhs.Speed;
            CritRate = rhs.CritRate;
            CritDamage = rhs.CritDamage;
            Resistance = rhs.Resistance;
            Accuracy = rhs.Accuracy;
        }

        public double this[Attr stat]
        {
            get
            {
                switch (stat)
                {
                    case Attr.HealthFlat:
                        return HealthFlat;
                    case Attr.HealthPercent:
                        return Health;
                    case Attr.AttackFlat:
                        return AttackFlat;
                    case Attr.AttackPercent:
                        return Attack;
                    case Attr.DefenseFlat:
                        return DefenseFlat;
                    case Attr.DefensePercent:
                        return Defense;
                    case Attr.SpeedPercent:
                        return SpeedPercent;
                    case Attr.Speed:
                        return Speed;
                    case Attr.CritDamage:
                        return CritDamage;
                    case Attr.CritRate:
                        return CritRate;
                    case Attr.Accuracy:
                        return Accuracy;
                    case Attr.Resistance:
                        return Resistance;

                }
                throw new NotImplementedException();
            }

            set
            {
                switch (stat)
                {
                    case Attr.HealthFlat:
                        HealthFlat = value;
                        break;
                    case Attr.HealthPercent:
                        Health = value;
                        break;
                    case Attr.AttackFlat:
                        AttackFlat = value;
                        break;
                    case Attr.AttackPercent:
                        Attack = value;
                        break;
                    case Attr.DefenseFlat:
                        DefenseFlat = value;
                        break;
                    case Attr.DefensePercent:
                        Defense = value;
                        break;
                    case Attr.SpeedPercent:
                        SpeedPercent = value;
                        break;
                    case Attr.Speed:
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
                    default:
                        throw new NotImplementedException();
                }
            }

        }


        public static Attrs operator +(Attrs lhs, Attrs rhs)
        {
            Attrs ret = new Attrs(lhs);
            ret.Health += rhs.Health;
            ret.HealthFlat += rhs.HealthFlat;
            ret.Attack += rhs.Attack;
            ret.AttackFlat += rhs.AttackFlat;
            ret.Defense += rhs.Defense;
            ret.DefenseFlat += rhs.DefenseFlat;
            ret.SpeedPercent += rhs.SpeedPercent;
            ret.Speed += rhs.Speed;
            ret.CritRate += rhs.CritRate;
            ret.CritDamage += rhs.CritDamage;
            ret.Resistance += rhs.Resistance;
            ret.Accuracy += rhs.Accuracy;
            return ret;
        }

        public static Attrs operator -(Attrs lhs, Attrs rhs)
        {
            Attrs ret = new Attrs(lhs);
            ret.Health -= rhs.Health;
            ret.HealthFlat -= rhs.HealthFlat;
            ret.Attack -= rhs.Attack;
            ret.AttackFlat -= rhs.AttackFlat;
            ret.Defense -= rhs.Defense;
            ret.DefenseFlat -= rhs.DefenseFlat;
            ret.SpeedPercent -= rhs.SpeedPercent;
            ret.Speed -= rhs.Speed;
            ret.CritRate -= rhs.CritRate;
            ret.CritDamage -= rhs.CritDamage;
            ret.Resistance -= rhs.Resistance;
            ret.Accuracy -= rhs.Accuracy;
            return ret;
        }


    }
}
