using System.Collections.Concurrent;
using RuneOptim.swar;

namespace RuneOptim.Management {
    public interface ILoadout {
        int Accuracy { get; }
        int AttackFlat { get; }
        int AttackPercent { get; }
        Buffs Buffs { get; set; }
        int BuildID { get; set; }
        bool Changed { get; }
        int CritDamage { get; }
        int CritRate { get; }
        int DefenseFlat { get; }
        int DefensePercent { get; }
        int[] FakeLevel { get; set; }
        int HealthFlat { get; }
        int HealthPercent { get; }
        Stats Leader { get; set; }
        ConcurrentDictionary<string, double>[] ManageStats { get; set; }
        bool[] PredictSubs { get; set; }
        int Resistance { get; }
        int RuneCount { get; }
        ulong[] RuneIDs { get; set; }
        Rune[] Runes { get; }
        RuneSet[] Sets { get; }
        bool SetsFull { get; }
        Stats Shrines { get; set; }
        int Speed { get; }
        int SpeedPercent { get; }
        bool TempLoad { get; set; }

        Rune AddRune(Rune rune, int checkOn = 2);
        void CheckSets();
        Stats GetStats(Stats baseStats);
        Stats GetStats(Stats baseStats, ref Stats value);
        void Lock();
        void RecountDiff(ulong monId);
        Rune RemoveRune(int slot, int checkOn = 2);
        int SetStat(Attr attr);
        string ToString();
    }
}