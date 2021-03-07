using System.Collections.Concurrent;
using RuneOptim.swar;

namespace RuneOptim.BuildProcessing {
    public class RuneUsage {
        // the runes legit used 
        public ConcurrentDictionary<Rune, byte> runesUsed = new ConcurrentDictionary<Rune, byte>();
        // runes which where in a passing build
        public ConcurrentDictionary<Rune, byte> runesGood = new ConcurrentDictionary<Rune, byte>();

        // the runes that got in the winning builds when runesUsed got banned
        public ConcurrentDictionary<Rune, byte> runesSecond = new ConcurrentDictionary<Rune, byte>();
        // runes which got generated while banning runesUsed --> goodRunes
        public ConcurrentDictionary<Rune, byte> runesOkay = new ConcurrentDictionary<Rune, byte>();
        public ConcurrentDictionary<Rune, byte> runesBetter = new ConcurrentDictionary<Rune, byte>();
    }

}