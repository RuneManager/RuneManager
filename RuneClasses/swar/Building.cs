using Newtonsoft.Json;

namespace RuneOptim.swar {
    public class Building {
        [JsonProperty("building_id")]
        public ulong Id;

        [JsonProperty("wizard_id")]
        public ulong WizardId;

        [JsonProperty("island_id")]
        public int IslandId;

        [JsonProperty("building_master_id")]
        public BuildingType BuildingType;

        [JsonProperty("pos_x")]
        public int X;

        [JsonProperty("pos_y")]
        public int Y;

        /// <summary>
        /// Presumable XP
        /// </summary>
        [JsonProperty("gain_per_hour")]
        public double GainPerHour;

        [JsonProperty("harvest_max")]
        public int HarvestCapacity;

        [JsonProperty("harvest_available")]
        public int HarvestAvailable;

        [JsonProperty("next_harvest")]
        public int NextHarvest;
    }

    public enum BuildingType {
        PondOfMana = 3,
        CrystalMine,
        AncientStone = 12,
        CrystalTitan = 15,

        CrystalLake = 21,
        TranquilForest,
        GustyCliffs,
        DeepForestEnt,
        MonsterStorage,

        CrystalDragon = 41,
        PracticeBattleField,
    }
}
