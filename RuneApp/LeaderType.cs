using System.Collections.Generic;
using RuneOptim.swar;

namespace RuneApp {
    public partial class Create {

        private readonly LeaderType[] leadTypes = {
            new LeaderType(Attr.Null),
            new LeaderType(Attr.Speed).AddRange(new int[] { 0, 10, 15, 16, 17, 19, 23, 24, 28, 30, 33 }),
            new LeaderType(Attr.HealthPercent).AddRange(new int[] { 0, 15, 17, 18, 21, 22, 25, 30, 33, 38, 40, 44, 50 }),
            new LeaderType(Attr.DefensePercent).AddRange(new int[] { 0, 20, 21, 22, 25, 27, 30, 33, 40, 44, 50 }),
            new LeaderType(Attr.AttackPercent).AddRange(new int[] { 0, 15, 18, 20, 21, 22, 25, 28, 30, 33, 35, 38, 40, 44, 50 }),
            new LeaderType(Attr.Resistance).AddRange(new int[] { 0, 15, 20, 25, 26, 28, 30, 33, 38, 40, 41, 48, 50, 55 }),
            new LeaderType(Attr.CritRate).AddRange(new int[] { 0, 10, 15, 16, 17, 19, 23, 24, 28, 33, 38 }),
            new LeaderType(Attr.CritDamage).AddRange(new int[] { 0, 25 }),
        };

        public class LeaderType {
            public LeaderType(Attr t) {
                type = t;
            }

            public Attr type;
            public class LeaderValue {
                public LeaderValue(Attr t, int v) {
                    type = t;
                    value = v;
                }
                public int value;
                public Attr type;
                public override string ToString() {
                    return value.ToString() + ((type == Attr.HealthFlat || type == Attr.DefenseFlat || type == Attr.AttackFlat || type == Attr.Speed) ? "" : "%");
                }
            }

            public void Add(int i) {
                values.Add(new LeaderValue(type, i));
            }

            public LeaderType AddRange(int[] ii) {
                foreach (int i in ii)
                    Add(i);

                return this;
            }

            public List<LeaderValue> values = new List<LeaderValue>();

            public override string ToString() {
                if (type <= Attr.Null) return "(None)";
                return type.ToString();
            }
        }
    }
}

