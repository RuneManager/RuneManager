using Newtonsoft.Json;

namespace RuneOptim.swar {
    public class RuneLock {
        [JsonProperty("wizard_id")]
        public ulong WizardId;

        [JsonProperty("rune_id")]
        public ulong Id;

        [JsonProperty("lock_type")]
        public int Type;
    }
}
