using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RuneOptim.swar {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CraftType {
        Null = 0,
        Enchant = 1,
        Grind = 2,
    }

    public class Craft {
        [JsonProperty("wizard_id")]
        public ulong wizardId;

        [JsonProperty("craft_item_id")]
        public ulong ItemId;

        [JsonProperty("craft_type")]
        public CraftType Type;

        [JsonProperty("craft_type_id")]
        public ulong TypeId;

        [JsonProperty("sell_value")]
        public int SellValue;

        public Attr Stat {
            get {
                return (Attr)(TypeId / 100 % 100);
            }
        }

        public RuneSet Set {
            get {
                return (RuneSet)(TypeId / 10000);
            }
        }

        public int Rarity {
            get {
                return (int)TypeId % 10;
            }
        }

        public override string ToString() {
            return Rarity + " " + Set + " " + Stat + " " + Type;
        }

        public ValueRange Value {
            get {
                if (Type == CraftType.Grind) {
                    return GrindValues[Stat][Rarity];
                }
                else {
                    return EnchantValues[Stat][Rarity];
                }
            }
        }

        #region Grind/Enchant Values
        // 1 = normal, 2 = magic, 3 = rare, 4 = hero, 5 = legend
        private static readonly ImmutableDictionary<int, ValueRange> FlatAtkDefGrindValues = new Dictionary<int, ValueRange>() {
            {1, new ValueRange(4, 8) },
            {2, new ValueRange(6, 12) },
            {3, new ValueRange(10, 18) },
            {4, new ValueRange(12, 22) },
            {5, new ValueRange(18, 30) },
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<int, ValueRange> PercentGrindValues = new Dictionary<int, ValueRange>() {
            //{1, new ValueRange(1, 4) },
            {2, new ValueRange(2, 5) },
            {3, new ValueRange(3, 6) },
            {4, new ValueRange(4, 7) },
            {5, new ValueRange(5, 10) },
        }.ToImmutableDictionary();

        public static readonly ImmutableDictionary<Attr, ImmutableDictionary<int, ValueRange>> GrindValues = new Dictionary<Attr, ImmutableDictionary<int, ValueRange>>()
        {
            { Attr.AttackFlat, FlatAtkDefGrindValues },
            { Attr.DefenseFlat, FlatAtkDefGrindValues },
            { Attr.HealthFlat, new Dictionary<int, ValueRange>() {
                //{1, new ValueRange(80, 160) },
                {2, new ValueRange(100, 200) },
                {3, new ValueRange(180, 250) },
                {4, new ValueRange(230, 450) },
                {5, new ValueRange(430, 550) },
            }.ToImmutableDictionary() },
            { Attr.AttackPercent, PercentGrindValues },
            { Attr.DefensePercent, PercentGrindValues },
            { Attr.HealthPercent, PercentGrindValues },
            { Attr.Speed, new Dictionary<int, ValueRange>() {
                //{1, new ValueRange(1, 1) },
                {2, new ValueRange(1, 2) },
                {3, new ValueRange(2, 3) },
                {4, new ValueRange(3, 4) },
                {5, new ValueRange(4, 5) },
            }.ToImmutableDictionary() },

        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<int, ValueRange> FlatAtkDefEnchantValues = new Dictionary<int, ValueRange>() {
            //{1, new ValueRange(6, 10) },
            {2, new ValueRange(10, 16) },
            {3, new ValueRange(15, 23) },
            {4, new ValueRange(20, 30) },
            {5, new ValueRange(28, 40) },
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<int, ValueRange> PercentEnchantValues = new Dictionary<int, ValueRange>() {
            //{1, new ValueRange(1, 5) },
            {2, new ValueRange(3, 7) },
            {3, new ValueRange(5, 9) },
            {4, new ValueRange(7, 11) },
            {5, new ValueRange(9, 13) },
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<int, ValueRange> AccResEnchantValues = new Dictionary<int, ValueRange>() {
            //{1, new ValueRange(1, 5) },
            {2, new ValueRange(3, 6) },
            {3, new ValueRange(5, 8) },
            {4, new ValueRange(6, 9) },
            {5, new ValueRange(8, 11) },
        }.ToImmutableDictionary();

        public static readonly ImmutableDictionary<Attr, ImmutableDictionary<int, ValueRange>> EnchantValues = new Dictionary<Attr, ImmutableDictionary<int, ValueRange>>()
        {
            { Attr.AttackFlat, FlatAtkDefEnchantValues },
            { Attr.DefenseFlat, FlatAtkDefEnchantValues },
            { Attr.HealthFlat, new Dictionary<int, ValueRange>() {
                //{1, new ValueRange(100, 170) },
                {2, new ValueRange(130, 220) },
                {3, new ValueRange(200, 310) },
                {4, new ValueRange(290, 420) },
                {5, new ValueRange(400, 580) },
            }.ToImmutableDictionary() },
            { Attr.AttackPercent, PercentEnchantValues },
            { Attr.DefensePercent, PercentEnchantValues },
            { Attr.HealthPercent, PercentEnchantValues },
            { Attr.Speed, new Dictionary<int, ValueRange>() {
                //{1, new ValueRange(1, 2) },
                {2, new ValueRange(2, 4) },
                {3, new ValueRange(3, 6) },
                {4, new ValueRange(5, 8) },
                {5, new ValueRange(7, 10) },
            }.ToImmutableDictionary() },
            { Attr.CritRate, new Dictionary<int, ValueRange>() {
                //{1, new ValueRange(1, 2) },
                {2, new ValueRange(2, 4) },
                {3, new ValueRange(3, 5) },
                {4, new ValueRange(4, 7) },
                {5, new ValueRange(6, 9) },
            }.ToImmutableDictionary() },
            { Attr.CritDamage, new Dictionary<int, ValueRange>() {
                //{1, new ValueRange(2, 4) },
                {2, new ValueRange(3, 5) },
                {3, new ValueRange(4, 6) },
                {4, new ValueRange(5, 8) },
                {5, new ValueRange(7, 10) },
            }.ToImmutableDictionary() },
            { Attr.Accuracy, AccResEnchantValues },
            { Attr.Resistance, AccResEnchantValues },
        }.ToImmutableDictionary();
        #endregion
    }

    public class ValueRange {
        public int Min;
        public int Max;

        public ValueRange(int i, int a) {
            Min = i;
            Max = a;
        }

        public double Average {
            get {
                return (Max - Min) / (double)2 + Min;
            }
        }
        [JsonIgnore]
        public bool Locked;

    }
}
