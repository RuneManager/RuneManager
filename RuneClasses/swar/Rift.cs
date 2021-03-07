using System;
using Newtonsoft.Json;

namespace RuneOptim.swar {
    public enum RiftRating {
        F,
        E,
        D,
        C,
        Bminus,
        B,
        Bplus,
        Aminus,
        A,
        Aplus,
        S,
        SS,
        SSS,
    }

    public enum RiftDungeon {
        Ice = 1001,
        Fire = 2001,
        Wind = 3001,
        Light = 4001,
        Dark = 5001
    }

    public class RiftPosition : ListProp<long?> {
        [ListProperty(0)]
        [JsonProperty("position")]
        public long? Position = null;

        [ListProperty(1)]
        [JsonProperty("unit_id")]
        public long? MonsterId = null;

        [ListProperty(2)]
        [JsonProperty("unit_master_id")]
        public long? MonsterTypeId = null;

        [ListProperty(3)]
        [JsonProperty("grade")]
        public long? Grade = null;

        [ListProperty(4)]
        [JsonProperty("level")]
        public long? Level = null;

        public override bool IsReadOnly { get { return false; } }
        protected override int maxInd { get { return 4; } }

        public override void Add(long? item) {
            if (Position == null)
                Position = item;
            else if (MonsterId == null)
                MonsterId = item;
            else if (MonsterTypeId == null)
                MonsterTypeId = item;
            else if (Grade == null)
                Grade = item;
            else if (Level == null)
                Level = item;
            else
                throw new Exception();
        }
    }

    public class RiftRun {
        [JsonProperty("wizard_id")]
        public ulong WizardId;
        [JsonProperty("rift_dungeon_id")]
        public RiftDungeon RiftDungeonId;
    }

    public class RiftDeck : RiftRun {
        [JsonProperty("clear_rating")]
        public RiftRating ClearRating;
        [JsonProperty("clear_damage")]
        public int ClearDamage;
        [JsonProperty("leader_index")]
        public int LeaderIndex;
        [JsonProperty("my_unit_list")]
        public RiftPosition[] Monsters;
    }

    public class BestRiftDeck : RiftRun {
        [JsonProperty("best_rating")]
        public RiftRating BestRating;
        [JsonProperty("best_damage")]
        public int BestDamage;
        [JsonProperty("RID")]
        public ulong RID;
    }
}
