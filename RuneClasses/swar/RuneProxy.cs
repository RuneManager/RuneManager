using System.Diagnostics;

namespace RuneOptim.swar {
    public partial class Rune {
        internal class RuneProxy {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private Rune _r;

            public RuneProxy(Rune r) {
                _r = r;
            }

            public Monster Assigned => _r.Assigned;

            public RuneSet Set => _r.Set;

            public int HealthFlat => _r.HealthFlat[0];
            public int HealthPercent => _r.HealthPercent[0];

            public int AttackFlat => _r.AttackFlat[0];
            public int AttackPercent => _r.AttackPercent[0];

            public int DefenseFlat => _r.DefenseFlat[0];
            public int DefensePercent => _r.DefensePercent[0];

            public int Speed => _r.Speed[0];

            public int CritRate => _r.CritRate[0];
            public int CritDamage => _r.CritDamage[0];
            public int Accuracy => _r.Accuracy[0];
            public int Resistance => _r.Resistance[0];

        }

    }
}
