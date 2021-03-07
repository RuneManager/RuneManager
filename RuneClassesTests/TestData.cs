using RuneOptim.swar;

namespace RuneOptim.Tests {
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
                Main = new RuneAttr() { (int)Attr.AttackFlat, 78 },
                Innate = new RuneAttr() { (int)Attr.Null, 0 },
                Subs = { new RuneAttr() { (int)Attr.AttackPercent, 18 },
                        new RuneAttr() { (int)Attr.Speed, 4 },
                        new RuneAttr() { (int)Attr.CritRate, 4 },
                },
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
                Main = new RuneAttr() { (int)Attr.AttackPercent, 22 },
                Innate = new RuneAttr() { (int)Attr.Resistance, 3 },
                Subs = { new RuneAttr() { (int)Attr.CritRate, 9 },
                        new RuneAttr() { (int)Attr.Speed, 5 },
                },
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
                Main = new RuneAttr() { (int)Attr.DefenseFlat, 57 },
                Innate = new RuneAttr() { (int)Attr.DefensePercent, 5 },
                Subs = { new RuneAttr() { (int)Attr.Speed, 5 },
                        new RuneAttr() { (int)Attr.CritDamage, 12 },
                },
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
                Main = new RuneAttr() { (int)Attr.CritRate, 27 },
                Innate = new RuneAttr() { (int)Attr.Null, 0 },
                Subs = { new RuneAttr() { (int)Attr.Speed, 10 },
                        new RuneAttr() { (int)Attr.Accuracy, 11 },
                        new RuneAttr() { (int)Attr.HealthPercent, 13 },
                        new RuneAttr() { (int)Attr.DefenseFlat, 8 },
                },
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
                Main = new RuneAttr() { (int)Attr.HealthFlat, 1530 },
                Innate = new RuneAttr() { (int)Attr.DefenseFlat, 8 },
                Subs = { new RuneAttr() { (int)Attr.Accuracy, 6 },
                        new RuneAttr() { (int)Attr.Speed, 10 },
                        new RuneAttr() { (int)Attr.CritRate, 5 },
                        new RuneAttr() { (int)Attr.AttackPercent, 4 },
                },
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
                Main = new RuneAttr() { (int)Attr.DefensePercent, 37 },
                Innate = new RuneAttr() { (int)Attr.Speed, 5 },
                Subs = { new RuneAttr() { (int)Attr.CritRate, 12 },
                        new RuneAttr() { (int)Attr.DefenseFlat, 19 },
                        new RuneAttr() { (int)Attr.CritDamage, 4 },
                        new RuneAttr() { (int)Attr.HealthFlat, 210 },
                },
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
