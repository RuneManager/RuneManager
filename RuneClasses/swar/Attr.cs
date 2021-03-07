using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RuneOptim.swar {
    // Allows me to steal the JSON values into Enum
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Attr {
        [EnumMember(Value = "-")]
        Neg = -1,

        [EnumMember(Value = "")]
        // TODO: FIXME:
        Null = 0,

        [EnumMember(Value = "HP flat")]
        HealthFlat = 1,

        [EnumMember(Value = "HP%")]
        HealthPercent = 2,

        [EnumMember(Value = "ATK flat")]
        AttackFlat = 3,

        [EnumMember(Value = "ATK%")]
        AttackPercent = 4,

        [EnumMember(Value = "DEF flat")]
        DefenseFlat = 5,

        [EnumMember(Value = "DEF%")]
        DefensePercent = 6,

        // Thanks Swift -_-
        SpeedPercent = 7,

        [EnumMember(Value = "SPD")]
        Speed = 8,

        [EnumMember(Value = "CRate")]
        CritRate = 9,

        [EnumMember(Value = "CDmg")]
        CritDamage = 10,

        [EnumMember(Value = "RES")]
        Resistance = 11,

        [EnumMember(Value = "ACC")]
        Accuracy = 12,

        // Flag for below
        ExtraStat = 16,

        [EnumMember(Value = "EHP")]
        EffectiveHP = 1 | ExtraStat,

        [EnumMember(Value = "EHPDB")]
        EffectiveHPDefenseBreak = 2 | ExtraStat,

        [EnumMember(Value = "DPS")]
        DamagePerSpeed = 3 | ExtraStat,

        [EnumMember(Value = "AvD")]
        AverageDamage = 4 | ExtraStat,

        [EnumMember(Value = "MxD")]
        MaxDamage = 5 | ExtraStat,

        // Flag for below
        SkillStat = 32,

        [EnumMember(Value = "Skill1")]
        Skill1 = 1 | SkillStat,

        [EnumMember(Value = "Skill2")]
        Skill2 = 2 | SkillStat,

        [EnumMember(Value = "Skill3")]
        Skill3 = 3 | SkillStat,

        [EnumMember(Value = "Skill4")]
        Skill4 = 4 | SkillStat,
    }
}
