using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneOptim.Tests
{
    static class TestData
    {
        #region Runes
        public static Rune Rune1()
        {
            return new Rune()
            {
                Level = 9,
                Grade = 5,
                Slot = 1,
                Set = RuneSet.Energy,
                MainType = Attr.AttackFlat,
                MainValue = 78,
                InnateType = Attr.Null,
                InnateValue = 0,
                Sub1Type = Attr.AttackPercent,
                Sub1Value = 18,
                Sub2Type = Attr.Speed,
                Sub2Value = 4,
                Sub3Type = Attr.CritRate,
                Sub3Value = 4,
                Sub4Type = Attr.Null,
                Sub4Value = 0,
            };
        }

        public static Rune Rune2()
        {
            return new Rune()
            {
                Level = 6,
                Grade = 5,
                Slot = 2,
                Set = RuneSet.Swift,
                MainType = Attr.AttackPercent,
                MainValue = 22,
                InnateType = Attr.Resistance,
                InnateValue = 3,
                Sub1Type = Attr.CritRate,
                Sub1Value = 9,
                Sub2Type = Attr.Speed,
                Sub2Value = 5,
                Sub3Type = Attr.Null,
                Sub3Value = 0,
                Sub4Type = Attr.Null,
                Sub4Value = 0,
            };
        }

        public static Rune Rune3()
        {
            return new Rune()
            {
                Level = 6,
                Grade = 5,
                Slot = 3,
                Set = RuneSet.Energy,
                MainType = Attr.DefenseFlat,
                MainValue = 57,
                InnateType = Attr.DefensePercent,
                InnateValue = 5,
                Sub1Type = Attr.Speed,
                Sub1Value = 5,
                Sub2Type = Attr.CritDamage,
                Sub2Value = 12,
                Sub3Type = Attr.Null,
                Sub3Value = 0,
                Sub4Type = Attr.Null,
                Sub4Value = 0,
            };
        }

        public static Rune Rune4()
        {
            return new Rune()
            {
                Level = 9,
                Grade = 5,
                Slot = 4,
                Set = RuneSet.Swift,
                MainType = Attr.CritRate,
                MainValue = 27,
                InnateType = Attr.Null,
                InnateValue = 0,
                Sub1Type = Attr.Speed,
                Sub1Value = 10,
                Sub2Type = Attr.Accuracy,
                Sub2Value = 11,
                Sub3Type = Attr.HealthPercent,
                Sub3Value = 13,
                Sub4Type = Attr.DefenseFlat,
                Sub4Value = 8,
            };
        }

        public static Rune Rune5()
        {
            return new Rune()
            {
                Level = 12,
                Grade = 5,
                Slot = 5,
                Set = RuneSet.Swift,
                MainType = Attr.HealthFlat,
                MainValue = 1530,
                InnateType = Attr.DefenseFlat,
                InnateValue = 8,
                Sub1Type = Attr.Accuracy,
                Sub1Value = 6,
                Sub2Type = Attr.Speed,
                Sub2Value = 10,
                Sub3Type = Attr.CritRate,
                Sub3Value = 5,
                Sub4Type = Attr.AttackPercent,
                Sub4Value = 4,
            };
        }

        public static Rune Rune6()
        {
            return new Rune()
            {
                Level = 12,
                Grade = 5,
                Slot = 6,
                Set = RuneSet.Swift,
                MainType = Attr.DefensePercent,
                MainValue = 37,
                InnateType = Attr.Speed,
                InnateValue = 5,
                Sub1Type = Attr.CritRate,
                Sub1Value = 12,
                Sub2Type = Attr.DefenseFlat,
                Sub2Value = 19,
                Sub3Type = Attr.CritDamage,
                Sub3Value = 4,
                Sub4Type = Attr.HealthFlat,
                Sub4Value = 210,
            };
        }
        #endregion

        #region Stats
        public static Stats statsBase()
        {
            return new Stats()
            {
                Accuracy = 0,
                Attack = 100,
                CritDamage = 50,
                CritRate = 15,
                Defense = 100,
                Health = 1000,
                Resistance = 0,
                Speed = 100,
            };
        }

        public static Stats statsFull()
        {
            return new Stats()
            {
                Accuracy = 0 + 11 + 6,
                Attack = 100 * (100 + 18 + 22 + 4) / 100
                                     + 78,
                CritDamage = 50 + 12 + 4,
                CritRate = 15 + 4 + 9 + 27 + 5 + 12,
                Defense = 100 * (100 + 5 + 8 + 8 + 37) / 100
                                             + 57 + 19,
                Health = 1000 * (100 + 13 + 15) / 100
                                    + 1530 + 210,
                Resistance = 0 + 3,
                Speed = 100 * (100 + 25) / 100
                                     + 4 + 5 + 5 + 10 + 10 + 5,
            };
        }
        #endregion
    }
}
