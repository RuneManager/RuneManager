using Newtonsoft.Json;

namespace RuneOptim.swar {
    public enum ItemType {
        WizardInfo = 0,
        Scrolls = 9,
        Essence = 11,
        SummoningPieces = 12,
        _Fifteen = 15,
        _Nineteen = 19,
        Material = 29,
        _ThirtySeven = 37,
    }

    // hax for teh lulz
    public enum WizardType {
        Mana,
        Crystal,
        EnergyMax,
        UnitSlotMax,
        DepositorySlotMax,
        CostumePoint
    }

    public enum ScrollType {
        Unknown = 1,
        Mystical,
        LightDark,
        Water,
        Fire,
        Wind,
        Legendary,
        SummoningStones,
        LegendaryPieces,
        LightDarkPieces,
    }

    public enum EssenceType {
        WaterLow = 11001,
        FireLow,
        WindLow,
        LightLow,
        DarkLow,
        MagicLow,
        WaterMid = 12001,
        FireMid,
        WindMid,
        LightMid,
        DarkMid,
        MagicMid,
        WaterHigh = 13001,
        FireHigh,
        WindHigh,
        LightHigh,
        DarkHigh,
        MagicHigh
    }

    public enum MaterialType {
        HardWood = 1001,
        ToughLeather,
        SolidRock,
        SolidIronOre,
        ShiningMithril,
        ThickCloth,
        RunePiece = 2001,
        MagicDust = 3001,
        SymbolOfHarmony = 4001,
        SymbolOfTranscendence,
        SymbolOfChaos,
        FrozenWaterCrystal = 5001,
        FlamingFireCrystal,
        WhirlingWindCrystal,
        ShinyLightCrystal,
        PitchBlackDarkCrystal,
        CondensedMagicCrystal = 6001,
        PureMagicCrystal = 7001,
    }

    public class InventoryItem {
        [JsonProperty("wizard_id")]
        public ulong WizardId;

        [JsonProperty("item_master_type")]
        public ItemType Type;

        [JsonProperty("item_master_id")]
        public int Id;

        [JsonProperty("item_quantity")]
        public long Quantity;

        public static int PiecesRequired(int stars) {
            switch (stars) {
                case 1:
                    return 10;
                case 2:
                    return 20;
                case 3:
                    return 40;
                case 4:
                    return 50;
                case 5:
                    return 100;
            }
            return 50;
        }

        public string Name {
            get {
                switch (Type) {
                    case ItemType.WizardInfo:
                        return ((WizardType)Id).ToString();
                    case ItemType.Scrolls:
                        return ((ScrollType)Id).ToString();
                    case ItemType.Essence:
                        return ((EssenceType)Id).ToString();
                    case ItemType.SummoningPieces:
                        if (Id > 10000) {
                            if (Save.MonIdNames.ContainsKey(Id / 100))
                                return Save.MonIdNames[Id / 100] + " ";
                            return "Missingno " + Id;
                        }
                        break;
                    case ItemType.Material:
                        return ((MaterialType)Id).ToString();
                }
                return "N/A" + Id;
            }
        }

        public Element Element {
            get {
                if (Type == ItemType.SummoningPieces && Id > 10000 && Save.MonIdNames.ContainsKey(Id / 100))
                    return (Element)(Id % 10);
                return Element.Pure;
            }
        }

        public override string ToString() {
            return Name;
        }
    }
}