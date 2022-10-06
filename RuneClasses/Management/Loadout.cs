using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using RuneOptim.swar;

namespace RuneOptim.Management {
    public enum EquipCompare {
        Unknown,
        Worse,
        Better
    }

    public struct Buffs {
        public bool Attack;
        public bool Defense;
        public bool Speed;
        public bool Crit;
        public float AttackMod;
        public float DefenseMod;
        public float SpeedMod;
        public float CritMod;
    }

    public class Loadout : ILoadout {
        private readonly Rune[] runes = new Rune[6];

        private int runeCount = 0;

        [JsonProperty("Sets")]
        private readonly RuneSet[] sets = new RuneSet[3];

        private bool setsFull = false;

        private int[] fakeLevel = new int[6];

        private bool[] predictSubs = new bool[6];
        public int BuildID { get; set; }

        [JsonIgnore]
        public Rune[] Runes => runes;

        public bool HasRunesUnder12()
        {
            foreach (var rune in Runes)
            {
                if (rune.Level < 12) {
                    return true;
                }
            }
            return false;
        }

        [JsonIgnore]
        public int RuneCount => runeCount;

        [JsonIgnore]
        public RuneSet[] Sets => sets;

        [JsonIgnore]
        public bool SetsFull => setsFull;

        public Element Element = Element.Pure;

        private Buffs buffs;

        [JsonIgnore]
        public Buffs Buffs {
            get => buffs;
            set {
                buffs = value;
                buffs.SpeedMod = buffs.Speed ? 0.3f : 0;
                buffs.AttackMod = buffs.Attack ? 0.5f : 0;
                buffs.DefenseMod = buffs.Defense ? 0.7f : 0;
                buffs.CritMod = buffs.Crit ? 0.3f : 0;
            }
        }

        public double Time;

        public double DeltaPoints;

        public long ActualTests = 0;

        [JsonIgnore]
        private bool tempLoad;

        [JsonIgnore]
        public bool TempLoad {
            get {
                return tempLoad;
            }
            set {
                tempLoad = value;
                if (!tempLoad) {
                    foreach (var r in Runes.Where(ru => ru != null)) {
                        r.OnUpdate += Rune_OnUpdate;
                    }
                }
                else {
                    foreach (var r in Runes.Where(ru => ru != null)) {
                        r.OnUpdate -= Rune_OnUpdate;
                    }
                }
            }
        }

        [JsonIgnore]
        private ConcurrentDictionary<string, double>[] manageStats;

        [JsonIgnore]
        public bool HasManageStats => manageStats != null;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ConcurrentDictionary<string, double>[] ManageStats {
            get {
                if (runes.All(r => r != null))
                    return runes.Select(r => r.ManageStats).ToArray();
                return null;
            }
            set {
                manageStats = value;
            }
        }

        private ulong[] runeIDs;

        //[JsonIgnore]
        public ulong[] RuneIDs {
            get {
                if (runeIDs == null)
                    runeIDs = new ulong[6];

                for (int i = 0; i < 6; i++) {
                    var ru = runes.FirstOrDefault(r => r != null && r.Slot == i + 1);
                    if (ru != null && ru.Id != runeIDs[i])
                        runeIDs[i] = ru.Id;
                }

                return runeIDs;
            }
            set {
                runeIDs = value;
            }
        }

        public int[] FakeLevel {
            get { return fakeLevel; }
            set {
                foreach (var rune in Runes) {
                    if (rune == null)
                        continue;
                    var slot = rune.Slot;

                    int ind = predictSubs[slot - 1] ? fakeLevel[slot - 1] + 16 : fakeLevel[slot - 1];
                    healthFlatCache -= rune.HealthFlat[ind];
                    healthPercentCache -= rune.HealthPercent[ind];
                    attackFlatCache -= rune.AttackFlat[ind];
                    attackPercentCache -= rune.AttackPercent[ind];
                    defenseFlatCache-= rune.DefenseFlat[ind];
                    defensePercentCache -= rune.DefensePercent[ind];
                    speedFlatCache -= rune.Speed[ind];
                    speedPercentCache -= rune.SpeedPercent[ind];


                    critRateCache -= rune.CritRate[ind];
                    critDamageCache -= rune.CritDamage[ind];
                    accuracyCache -= rune.Accuracy[ind];
                    resistanceCache -= rune.Resistance[ind];
                }
                fakeLevel = value;

                foreach (var rune in Runes) {
                    if (rune == null)
                        continue;
                    var slot = rune.Slot;

                    int ind = predictSubs[slot - 1] ? fakeLevel[slot - 1] + 16 : fakeLevel[slot - 1];
                    healthFlatCache += rune.HealthFlat[ind];
                    healthPercentCache += rune.HealthPercent[ind];
                    attackFlatCache += rune.AttackFlat[ind];
                    attackPercentCache += rune.AttackPercent[ind];
                    defenseFlatCache += rune.DefenseFlat[ind];
                    defensePercentCache += rune.DefensePercent[ind];
                    speedFlatCache += rune.Speed[ind];
                    speedPercentCache += rune.SpeedPercent[ind];


                    critRateCache += rune.CritRate[ind];
                    critDamageCache += rune.CritDamage[ind];
                    accuracyCache += rune.Accuracy[ind];
                    resistanceCache += rune.Resistance[ind];
                }
                changed = true;
            }
        }

        public bool[] PredictSubs {
            get { return predictSubs; }
            set {
                foreach (var rune in Runes) {
                    if (rune == null)
                        continue;
                    var slot = rune.Slot;

                    int ind = predictSubs[slot - 1] ? fakeLevel[slot - 1] + 16 : fakeLevel[slot - 1];
                    healthFlatCache -= rune.HealthFlat[ind];
                    healthPercentCache -= rune.HealthPercent[ind];
                    attackFlatCache -= rune.AttackFlat[ind];
                    attackPercentCache -= rune.AttackPercent[ind];
                    defenseFlatCache -= rune.DefenseFlat[ind];
                    defensePercentCache -= rune.DefensePercent[ind];
                    speedFlatCache -= rune.Speed[ind];
                    speedPercentCache -= rune.SpeedPercent[ind];


                    critRateCache -= rune.CritRate[ind];
                    critDamageCache -= rune.CritDamage[ind];
                    accuracyCache -= rune.Accuracy[ind];
                    resistanceCache -= rune.Resistance[ind];
                }
                predictSubs = value;

                foreach (var rune in Runes) {
                    if (rune == null)
                        continue;
                    var slot = rune.Slot;

                    int ind = predictSubs[slot - 1] ? fakeLevel[slot - 1] + 16 : fakeLevel[slot - 1];
                    healthFlatCache += rune.HealthFlat[ind];
                    healthPercentCache += rune.HealthPercent[ind];
                    attackFlatCache += rune.AttackFlat[ind];
                    attackPercentCache += rune.AttackPercent[ind];
                    defenseFlatCache += rune.DefenseFlat[ind];
                    defensePercentCache += rune.DefensePercent[ind];
                    speedFlatCache += rune.Speed[ind];
                    speedPercentCache += rune.SpeedPercent[ind];


                    critRateCache += rune.CritRate[ind];
                    critDamageCache += rune.CritDamage[ind];
                    accuracyCache += rune.Accuracy[ind];
                    resistanceCache += rune.Resistance[ind];
                }
                changed = true;
            }
        }

        private Stats shrines = new Stats();
        [JsonIgnore]
        private Guild guild = new Guild();
        private Stats leader = new Stats();

        private bool changed = false;
        public int RunesNew;
        public int RunesChanged;
        public int Upgrades;
        public int Powerup;

        [JsonIgnore]
        public bool Changed { get { return changed; } }

        [JsonIgnore]
        public Stats Shrines {
            get {
                return shrines;
            }
            set {
                shrines = value;

                if (shrines == null)
                    shrines = new Stats();

                changed = true;
            }
        }
        [JsonIgnore]
        public Guild Guild
        {
            get
            {
                return guild;
            }
            set
            {
                guild = value;

                if (guild == null)
                    guild = new Guild();

                changed = true;
            }
        }
        public Stats Leader {
            get {
                return leader;
            }
            set {
                leader = value;

                if (leader == null)
                    leader = new Stats();

                changed = true;
            }
        }
        public Loadout(Loadout rhs = null, int[] fake = null, bool[] predict = null) {
            if (fake != null)
                fake.CopyTo(fakeLevel, 0);
            if (predict != null)
                predict.CopyTo(predictSubs, 0);

            if (rhs != null) {
                Shrines = rhs.shrines;
                Guild = rhs.guild;
                Leader = rhs.leader;
                fakeLevel = rhs.fakeLevel;
                predictSubs = rhs.predictSubs;
                BuildID = rhs.BuildID;
                Buffs = rhs.Buffs;
                tempLoad = rhs.tempLoad;
                Element = rhs.Element;
                foreach (var r in rhs.Runes) {
                    AddRune(r, 7);
                }
                UpdateSetsAndCache();
                // TODO: do we even need to?
                manageStats = rhs.manageStats;
            }
        }

        // Debugging niceness
        public override string ToString() {
            System.Text.StringBuilder str = new System.Text.StringBuilder("");
            foreach (Rune r in runes) {
                str.Append(r.Id).Append("  ");
            }
            str.Append("|  ");
            foreach (RuneSet s in sets) {
                str.Append(s).Append(" ");
            }
            return str.ToString();
        }

        /// <summary>
        /// Lock all the runes on the loadout
        /// </summary>
        public void Lock() {
            foreach (Rune r in runes) {
                if (r != null)
                    r.UsedInBuild = true;
            }
        }
        /// <summary>
        /// Lock all the runes on the loadout
        /// </summary>
        public void Unlock()
        {
            foreach (Rune r in runes)
            {
                if (r != null)
                    r.UsedInBuild = false;
            }
        }

        /// <summary>
        /// Fills Runes based on RuneIDs and an external Rune list
        /// </summary>
        /// <param name="runes"></param>
        public void LinkRunes(ObservableCollection<Rune> runes)
        {
            if (RuneIDs == null)
                return;
            for (int i = 0; i < 6; i++)
            {
                var ids = RuneIDs[i];
                Runes[i] = runes.FirstOrDefault(r => r.Id == ids);
                if (Runes[i] != null)
                {
                    Runes[i].UsedInBuild = true;
                    if (HasManageStats)
                        foreach (var ms in ManageStats[i])
                            Runes[i].ManageStats.AddOrUpdate(ms.Key, ms.Value, (s, d) => ms.Value);
                }
            }
        }

        /// <summary>
        /// Replaces runes with the copies from 
        /// </summary>
        public void RelinkRunes(ObservableCollection<Rune> newRunes)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Runes[i] == null)
                    continue;
                var rr = newRunes.FirstOrDefault(r => r.Id == Runes[i].Id);
                if (rr != null)
                {
                    Runes[i] = rr;
                }
                else
                {
                    // represent rune no longer available in set
                    Runes[i].AssignedId = 0;
                    Runes[i].AssignedName = "RUNE MISSING";
                }
            }
            Lock();
        }

        #region AttrGetters

        // When runes are equipped, these are updated with the stat effects of those runes
        private int healthFlatCache = 0;
        private int healthPercentCache = 0;

        private int attackFlatCache = 0;
        private int attackPercentCache = 0;

        private int defenseFlatCache = 0;
        private int defensePercentCache = 0;

        private int speedFlatCache = 0;
        private int speedPercentCache = 0;

        private int critRateCache = 0;
        private int critDamageCache = 0;

        private int accuracyCache = 0;
        private int resistanceCache = 0;

        // This section calculates the final effects of a loadout, including shrines, leaders, and guild bonuses
        [JsonIgnore]
        public int HealthFlat {
            get {
                var c = healthFlatCache;
                return c;
            }
        }

        [JsonIgnore]
        public int HealthPercent {
            get {

                var c = healthPercentCache + (int)shrines.Health + 
                    (int)guild.Health +
                    (int)leader.Health;
                return c;
            }
        }

        [JsonIgnore]
        public int AttackFlat {
            get {
                var c = attackFlatCache;
                return c;
            }
        }

        [JsonIgnore]
        public int AttackPercent {
            get {
                var c = attackPercentCache + (int)shrines.Attack + (int)shrines.DamageSkillups[(int)Element - 1] +
                    (int)guild.Attack +
                    (int)leader.Attack;
                return c;
            }
        }

        [JsonIgnore]
        public int DefenseFlat {
            get {
                var c = defenseFlatCache;
                return c;
            }
        }

        [JsonIgnore]
        public int DefensePercent {
            get {
                var c = defensePercentCache + (int)shrines.Defense +
                    (int)guild.Defense +
                    (int)leader.Defense;
                return c;
            }
        }

        [JsonIgnore]
        public int Speed {
            get {
                var c = speedFlatCache;
                return c;
            }
        }

        // Runes don't get SPD%
        [JsonIgnore]
        public int SpeedPercent {
            get {
                var c = speedPercentCache + (int)shrines.Speed +
                    (int)guild.Speed +
                    (int)leader.Speed;
                return c;
            }
        }

        [JsonIgnore]
        public int CritRate {
            get {
                var c = critRateCache + (int)shrines.CritRate +
                    (int)guild.CritRate +
                    (int)leader.CritRate;
                return c;

            }
        }

        [JsonIgnore]
        public int CritDamage {
            get {
                var c = critDamageCache + (int)shrines.CritDamage +
                    (int)guild.CritDamage +
                    (int)leader.CritDamage;
                return c;
            }
        }

        [JsonIgnore]
        public int Accuracy {
            get {
                var c = accuracyCache + (int)shrines.Accuracy +
                    (int)guild.Accuracy +
                    (int)leader.Accuracy;
                return c;
            }
        }

        [JsonIgnore]
        public int Resistance {
            get {
                var c = resistanceCache + (int)shrines.Resistance +
                    (int)guild.Resistance +
                    (int)leader.Resistance;
                return c;
            }
        }

#endregion

        // Put the rune on the build, updating the Cache values
        public Rune AddRune(Rune rune, int checkOn = 2) {
            // don't bother if not a rune
            if (rune == null)
                return null;

            changed = true;

            var slot = rune.Slot;
            var old = RemoveRune(slot, checkOn);

            int ind = predictSubs[slot - 1] ? fakeLevel[slot - 1] + 16 : fakeLevel[slot - 1];
            healthFlatCache += rune.HealthFlat[ind];
            healthPercentCache += rune.HealthPercent[ind];
            attackFlatCache += rune.AttackFlat[ind];
            attackPercentCache += rune.AttackPercent[ind];
            defenseFlatCache += rune.DefenseFlat[ind];
            defensePercentCache += rune.DefensePercent[ind];
            speedFlatCache += rune.Speed[ind];
            speedPercentCache += rune.SpeedPercent[ind];


            critRateCache += rune.CritRate[ind];
            critDamageCache += rune.CritDamage[ind];
            accuracyCache += rune.Accuracy[ind];
            resistanceCache += rune.Resistance[ind];

            runes[slot - 1] = rune;
            runeCount++;

            if (!tempLoad)
                rune.OnUpdate += Rune_OnUpdate;

            // see #628 with different formula
            if (runeCount % checkOn == 0)
                UpdateSetsAndCache();
            return old;
        }

        // Removes the rune from slot, updating Cache values
        public Rune RemoveRune(int slot, int checkOn = 2) {
            // don't bother if there was none
            if (runes[slot - 1] == null)
                return null;

            changed = true;

            var rune = runes[slot - 1];

            int ind = predictSubs[slot - 1] ? fakeLevel[slot - 1] + 16 : fakeLevel[slot - 1];
            healthFlatCache -= rune.HealthFlat[ind];
            healthPercentCache -= rune.HealthPercent[ind];
            attackFlatCache -= rune.AttackFlat[ind];
            attackPercentCache -= rune.AttackPercent[ind];
            defenseFlatCache -= rune.DefenseFlat[ind];
            defensePercentCache -= rune.DefensePercent[ind];
            speedFlatCache -= rune.Speed[ind];
            speedPercentCache -= rune.SpeedPercent[ind];


            critRateCache -= rune.CritRate[ind];
            critDamageCache -= rune.CritDamage[ind];
            accuracyCache -= rune.Accuracy[ind];
            resistanceCache -= rune.Resistance[ind];

            if (!tempLoad)
                rune.OnUpdate -= Rune_OnUpdate;

            runes[slot - 1] = null;
            runeCount--;
            
            // todo: spooky?
            // see #588 with different formula
            if ((runeCount + 1) % checkOn == 0)
                UpdateSetsAndCache();
            return rune;
        }

        private void Rune_OnUpdate(object sender, EventArgs e) {
            changed = true;
        }


        public void UpdateSetsAndCache() {
            // remove old set bonus(es)
            healthFlatCache -= SetStat(Attr.HealthFlat);
            healthPercentCache -= SetStat(Attr.HealthPercent);
            attackFlatCache -= SetStat(Attr.AttackFlat);
            attackPercentCache -= SetStat(Attr.AttackPercent);
            defenseFlatCache -= SetStat(Attr.DefenseFlat);
            defensePercentCache -= SetStat(Attr.DefensePercent);
            speedFlatCache -= SetStat(Attr.Speed);
            speedPercentCache -= SetStat(Attr.SpeedPercent);

            critRateCache -= SetStat(Attr.CritRate);
            critDamageCache -= SetStat(Attr.CritDamage);
            accuracyCache -= SetStat(Attr.Accuracy);
            resistanceCache -= SetStat(Attr.Resistance);

            // refersh sets[]
            sets[0] = 0;
            sets[1] = 0;
            sets[2] = 0;
            // firist blank set in sets[]
            int ind = 0;
            // count of runes in completed sets
            int setNum = 0;
            // tracks pairs of runes (e.g. complete 2-sets)
            RuneSet flag2 = 0;
            // tracks pairs of pairs of runes (i.e. complete 4-sets) 
            RuneSet flag4 = 0;

            for (int i = 0; i < 6; i++) {
                var r = Runes[i];
                if (r == null)
                    continue;

                // Very hard to read, but this probably uses bitwise operators for performance reasons
                // The outer cycle (flag2) finds pairs of runes from the same set
                // The inner cycle (flag4) finds pairs of pairs for complete 4-sets
                if ((flag2 & r.Set) != r.Set) {
                    // found an un-paired rune (first or third or fifth) so track it 
                    flag2 |= r.Set;
                }
                else
                {
                    // found a pair (the second or forth or sixth)
                    if (r.Set.Size() == 2) {
                        // a pair from a 2-set is a complete set
                        sets[ind] = r.Set;
                        setNum += 2;
                        ind++;
                    }
                    else {
                        // a 4-set requires two pairs
                        if ((flag4 & r.Set) != r.Set) {
                            // first pair from a 4-set so track it
                            flag4 |= r.Set;
                        }
                        else
                        {
                            // second pair from a 4-set is a complete set
                            sets[ind] = r.Set;
                            setNum += 4;
                            ind++;
                        }
                    }
                    // reset "pair" tracker
                    flag2 ^= r.Set;
                }
            }

            setsFull = setNum == 6;

            // apply new set bonus(es)
            healthFlatCache += SetStat(Attr.HealthFlat);
            healthPercentCache += SetStat(Attr.HealthPercent);
            attackFlatCache += SetStat(Attr.AttackFlat);
            attackPercentCache += SetStat(Attr.AttackPercent);
            defenseFlatCache += SetStat(Attr.DefenseFlat);
            defensePercentCache += SetStat(Attr.DefensePercent);
            speedFlatCache += SetStat(Attr.Speed);
            speedPercentCache += SetStat(Attr.SpeedPercent);

            critRateCache += SetStat(Attr.CritRate);
            critDamageCache += SetStat(Attr.CritDamage);
            accuracyCache += SetStat(Attr.Accuracy);
            resistanceCache += SetStat(Attr.Resistance);

        }

        // Check what sets are completed in this build
        public void CheckSets2() {
            setsFull = false;

            // If there are an odd number of runes, don't bother (maybe even check < 6?)
            if (runeCount % 2 == 1)
                return;

            // can only have 3 sets max (eg. energy / energy / blade)
            sets[0] = 0;
            sets[1] = 0;
            sets[2] = 0;

            // which set we are looking for
            int setInd = 0;
            // what slot we are looking at
            int slotInd = 0;
            // if we have used this slot in a set yet
            //bool[] used = new bool[6];
            int use = 0;

            // how many runes are in sets
            int setNums = 0;

            Rune rune;
            RuneSet set;
            int getNum;
            int gotNum;

            // todo: flatten this
            // Check slots 1 - 5, minimum set size is 2.
            // Because it will search forward for sets, can't find more from 6
            // Eg. starting from 6, working around, find 2 runes?
            for (; slotInd < 5; slotInd++) {
                //if there is a uncounted rune in this slot
                rune = runes[slotInd];
                // && !used[slotInd]
                if (rune != null && (use & 1 << slotInd) != 1 << slotInd) {
                    // look for more in the set
                    set = rune.Set;
                    // how many runes we need to get
                    getNum = set.Size();
                    // how many we got
                    gotNum = 1;
                    // we have now used this slot
                    //used[slotInd] = true;
                    use |= 1 << slotInd;

                    // for the runes after this rune
                    for (int ind = slotInd + 1; ind < 6; ind++) {
                        // if there is a rune in this slot that is the type I want
                        // && !used[ind] 
                        if (runes[ind] != null && runes[ind].Set == set && (use & 1 << ind) != 1 << ind) {
                            //used[ind] = true;
                            use |= 1 << ind;
                            gotNum++;
                        }

                        // if we have more than 1 rune and we have enough runes for a set
                        if (gotNum == getNum) {
                            // log this set
                            sets[setInd] = set;
                            // increase the number of runes in sets
                            setNums += getNum;
                            // look for the next set
                            setInd++;
                            // stop looking forward
                            break;
                        }
                    }
                }
            }

            // if all runes are in sets
            if (setNums == 6)
                setsFull = true;
            // notify hackers their attempt has failed
            else if (setNums > 6)
                throw new InvalidOperationException("Wut");

        }

        // pull how much Attr is in the equipped sets
        public int SetStat(Attr attr) {
#pragma warning disable S3358 // Ternary operators should not be nested
            switch (attr) {
                case Attr.Null:
                case Attr.HealthFlat:
                case Attr.AttackFlat:
                case Attr.DefenseFlat:
                case Attr.Speed:
                case Attr.ExtraStat:
                case Attr.EffectiveHP:
                case Attr.EffectiveHPDefenseBreak:
                case Attr.DamagePerSpeed:
                case Attr.AverageDamage:
                case Attr.MaxDamage:
                    return 0;

                // todo: (set[0] & SET) * magic => 15

                // I could use sets.Where(s => s.Equals(RuneSet.SET)).Count() * BONUS, but it was too slow
                case Attr.HealthPercent:
                    return (sets[0] == RuneSet.Energy ? 15 : sets[0] == RuneSet.Enhance ? 8 : 0) +
                        (sets[1] == RuneSet.Energy ? 15 : sets[1] == RuneSet.Enhance ? 8 : 0) +
                        (sets[2] == RuneSet.Energy ? 15 : sets[2] == RuneSet.Enhance ? 8 : 0);
                // 4 slot sets aren't going to be in [2]
                case Attr.AttackPercent:
                    return (sets[0] == RuneSet.Fatal ? 35 : sets[0] == RuneSet.Fight ? 8 : 0) +
                        (sets[1] == RuneSet.Fatal ? 35 : sets[1] == RuneSet.Fight ? 8 : 0) +
                        (sets[2] == RuneSet.Fight ? 8 : 0);
                case Attr.CritRate:
                    return (sets[0] == RuneSet.Blade ? 12 : 0) + (sets[1] == RuneSet.Blade ? 12 : 0) + (sets[2] == RuneSet.Blade ? 12 : 0);
                case Attr.CritDamage:
                    return sets[0] == RuneSet.Rage ? 40 : sets[1] == RuneSet.Rage ? 40 : 0;
                case Attr.SpeedPercent:
                    return sets[0] == RuneSet.Swift ? 25 : sets[1] == RuneSet.Swift ? 25 : 0;
                case Attr.Accuracy:
                    return (sets[0] == RuneSet.Focus ? 20 : sets[0] == RuneSet.Accuracy ? 10 : 0) +
                        (sets[1] == RuneSet.Focus ? 20 : sets[1] == RuneSet.Accuracy ? 10 : 0) +
                        (sets[2] == RuneSet.Focus ? 20 : sets[2] == RuneSet.Accuracy ? 10 : 0);
                case Attr.DefensePercent:
                    return (sets[0] == RuneSet.Guard ? 15 : sets[0] == RuneSet.Determination ? 8 : 0) +
                        (sets[1] == RuneSet.Guard ? 15 : sets[1] == RuneSet.Determination ? 8 : 0) +
                        (sets[2] == RuneSet.Guard ? 15 : sets[2] == RuneSet.Determination ? 8 : 0);
                case Attr.Resistance:
                    return (sets[0] == RuneSet.Endure ? 20 : sets[0] == RuneSet.Tolerance ? 10 : 0) +
                        (sets[1] == RuneSet.Endure ? 20 : sets[1] == RuneSet.Tolerance ? 10 : 0) +
                        (sets[2] == RuneSet.Endure ? 20 : sets[2] == RuneSet.Tolerance ? 10 : 0);
                default:
                    return 0;
#pragma warning restore S3358 // Ternary operators should not be nested
            }
        }

        //private static ParameterExpression statType = Expression.Parameter(typeof(Stats), "stats");
        //private static Func<Stats, Stats> _getStats;

        // Using the given stats as a base, apply the modifiers
        public Stats GetStats(Stats baseStats) {
            var v = new Stats();
            return GetStats(baseStats, ref v);
        }
        public Stats GetStats(Stats baseStats, ref Stats value) {
            if (value == null)
                value = new Stats();

            value.CopyFrom(baseStats);

            // Apply percent before flat
            value.Health += Math.Ceiling(baseStats.Health * HealthPercent * 0.01) + HealthFlat;

            value.Attack += Math.Ceiling(baseStats.Attack * AttackPercent * 0.01) + AttackFlat;
            value.Attack *= 1.0 + buffs.AttackMod;

            value.Defense += Math.Ceiling(baseStats.Defense * DefensePercent * 0.01) + DefenseFlat;
            value.Defense *= 1.0 + buffs.DefenseMod;

            value.Speed += Math.Ceiling(baseStats.Speed * SpeedPercent * 0.01) + Speed;
            value.Speed *= 1.0 + buffs.SpeedMod;

            value.CritDamage += CritDamage;
            value.CritRate += CritRate + buffs.CritMod;

            value.Accuracy += Accuracy;
            value.Resistance += Resistance;

            changed = false;

            return value;
        }

        /*// NYI comparison
        public EquipCompare CompareTo(Loadout rhs)
        {
            // check if the sets are comparable
            if (CompareSets(sets, rhs.sets) == 0)
                return EquipCompare.Unknown;

            int side = 0;
            if (HealthPercent > rhs.HealthPercent)
                side++;
            else
                side--;

            if (AttackPercent > rhs.AttackPercent)
                side++;
            else
                side--;

            if (DefensePercent > rhs.DefensePercent)
                side++;
            else
                side--;

            if (Speed > rhs.Speed)

            return EquipCompare.Unknown;
        }
        */

        // TODO: return an enum?
        // NYI comparison
        // 0 = differing magical sets
        // 1 = exact match
        // 2 = none or no differing magic sets
        public static int CompareSets(RuneSet[] a, RuneSet[] b) {
            if (a == b)
                return 1;

            if (a.Length == b.Length) {
                int same = 0;
                for (int i = 0; i < a.Length; i++) {
                    if (a[i] == b[i])
                        same++;
                }
                if (same == a.Length)
                    return 1;
            }
            // different lengths, or not the same sets

            // count the number of magical sets, and make sure both loadout have the same number of sets
            foreach (RuneSet s in RuneProperties.MagicalSets) {
                if (a.Count(x => x == s) != b.Count(x => x == s))
                    return 0;
            }

            return 1;
        }

        public void RecountDiff(ulong monId) {
            Powerup = 0;
            Upgrades = 0;
            RunesNew = 0;
            RunesChanged = 0;

            foreach (Rune r in Runes) {
                if (r == null) continue;
                if (r.AssignedId != monId) {
                    if (r.IsUnassigned)
                        RunesNew++;
                    else
                        RunesChanged++;
                }
                Powerup += Math.Max(0, FakeLevel[r.Slot - 1] - r.Level);
                if (FakeLevel[r.Slot - 1] != 0) {
                    int tup = (int)Math.Floor(Math.Min(12, FakeLevel[r.Slot - 1]) / (double)3);
                    int cup = (int)Math.Floor(Math.Min(12, r.Level) / (double)3);
                    Upgrades += Math.Max(0, tup - cup);
                }
            }
        }
    }
}
