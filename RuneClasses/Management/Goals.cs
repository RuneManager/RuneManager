using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuneOptim.swar;

namespace RuneOptim.Management {
    public class Goals {
        [JsonIgnore]
        public static List<GoalDef> AvailableGoals { get; set; } = new List<GoalDef>() {
            new FuseGoalDef(MonsterTypeMap.KungFuGirl_Wind) {
                requiredTypes = {
                    MonsterTypeMap.ChargerShark_Wind_Zephicus,
                    MonsterTypeMap.GrimReaper_Wind_Hiva,
                    MonsterTypeMap.Imp_Water_Fynn,
                    MonsterTypeMap.Inugami_Fire_Raoq
                }
            }
        };

        public List<ulong> ReservedIds = new List<ulong>();

        public List<ulong> NoSkillIds = new List<ulong>();

        public List<Goal> goals { get; set; }
        public List<Goal> Calculate(Save data) {
            GoalState gs = new GoalState(data);
            foreach (var g in goals) {
                g.status = FufilmentStatus.Pending;
            }
            List<Goal> output = new List<Goal>();

            while (goals.Any(g => g.status == FufilmentStatus.Pending)) {
                goals.FirstOrDefault().Fufill(output, gs);
            }
            return output;
        }
    }

    public class GoalState {
        public Dictionary<EssenceType, InventoryItem> essence;
        public List<MonsterG> monsters;
        public Dictionary<MonsterTypeMap, InventoryItem> pieces;

        public GoalState(Save data) {
            pieces = data.InventoryItems.Where(i => i.Type == ItemType.SummoningPieces).Select(p => new InventoryItem() { Id = p.Id, Quantity = p.Quantity, Type = p.Type, WizardId = p.WizardId }).ToDictionary(p => (MonsterTypeMap)p.Id);
            essence = data.InventoryItems.Where(i => i.Type == ItemType.Essence).Select(p => new InventoryItem() { Id = p.Id, Quantity = p.Quantity, Type = p.Type, WizardId = p.WizardId }).ToDictionary(p => (EssenceType)p.Id);
            monsters = data.Monsters.Select(m => new MonsterG((MonsterTypeMap)m.monsterTypeId) { level = m.level, grade = m.Grade }).ToList();
        }
    }

    public class Goal {
        public Goal(GoalDef baseDef) {
            def = baseDef;

        }
        public void Fufill(List<Goal> output, GoalState state) {
            output.Add(this);

            status = def.Fufil(this, state, output);
        }


        public FufilmentStatus status = FufilmentStatus.Pending;
        public GoalDef def;
        public List<MonsterG> reservedMonsters = new List<MonsterG>();
        public List<Goal> dependantOn = new List<Goal>();
    }

    public class MonsterG {
        public MonsterG(MonsterTypeMap monsterType) {
            type = monsterType;
        }
        public MonsterTypeMap type;
        public Goal requiredFor = null;
        public int level;
        public int grade;
    }

    public enum FufilmentStatus {
        Pending,
        Dependent,
        Fufilled,
        Failed
    }

    public enum GoalType {
        Fusion,
        Own,
        Awaken,
    }

    // TODO: consider hard typing for reals eg. from SWarFarm/localvalues.dat?
    public enum MonsterTypeMap {
        KungFuGirl_Wind = 17303,
        Imp_Water = 10201,
        Imp_Water_Fynn = 10211,
        GrimReaper_Wind = 16003,
        GrimReaper_Wind_Hiva = 16013,
        Inugami_Fire = 11002,
        Inugami_Fire_Raoq = 11012,
        ChargerShark_Wind = 19503,
        ChargerShark_Wind_Zephicus = 19513,
    }

    public class AwakenGoalDef : GoalDef {
        public AwakenGoalDef(MonsterTypeMap monsterType) : base(monsterType, GoalType.Awaken) {
            type = monsterType;
            stat = MonsterStat.FindMon((int)type).Download();
        }
        public MonsterTypeMap type;
        public MonsterStat stat;

        public override FufilmentStatus Fufil(Goal goal, GoalState state, List<Goal> goals) {
            if (!(goal.def is AwakenGoalDef))
                return FufilmentStatus.Failed;
            var tmon = state.monsters.FirstOrDefault(m => m.type == type);
            bool hasMon = false;
            bool hasEss = false;
            if (tmon != null) {
                tmon.requiredFor = goal;
                goal.reservedMonsters.Add(tmon);
                hasMon = true;
            }

            if (hasMon && hasEss)
                return FufilmentStatus.Fufilled;
            return FufilmentStatus.Dependent;
        }
    }

    public class FuseGoalDef : GoalDef {
        public FuseGoalDef(MonsterTypeMap monsterType) : base(monsterType, GoalType.Fusion) {
            type = monsterType;
        }
        public MonsterTypeMap type;
        public List<MonsterTypeMap> requiredTypes = new List<MonsterTypeMap>();
        public override FufilmentStatus Fufil(Goal goal, GoalState state, List<Goal> goals) {
            if (!(goal.def is FuseGoalDef))
                return FufilmentStatus.Failed;

            foreach (var r in requiredTypes) {
                var mmm = state.monsters.FirstOrDefault(c => c.requiredFor == null && c.type == r);
                mmm.requiredFor = goal;
                goal.reservedMonsters.Add(mmm);
            }

            foreach (var r in requiredTypes.Where(t => !goal.reservedMonsters.Any(m => m.type == t))) {
                //var ngs = Goals.AvailableGoals.Where(gd => gd.CanProduce(r));
                var qqqew = Goals.AvailableGoals.FirstOrDefault(gd => gd is AwakenGoalDef && (gd as AwakenGoalDef).type == r - 10);
                if (qqqew == null) {
                    qqqew = new AwakenGoalDef(r - 10);
                    Goals.AvailableGoals.Add(qqqew);
                }
                var gg = new Goal(qqqew);
                goal.dependantOn.Add(gg);
                goals.Add(gg);
            }

            return requiredTypes.Any(t => !goal.reservedMonsters.Any(m => m.type == t)) ? FufilmentStatus.Pending : FufilmentStatus.Fufilled;
        }
    }

    public class AcquireMonsterGoalDef : GoalDef {
        public AcquireMonsterGoalDef(MonsterTypeMap monsterType) : base(monsterType, GoalType.Own) {
        }
        public Monster FindMonster(Goals goals, GoalState state) {
            /*var mm = data.Monsters.FirstOrDefault(m => !goals.goals.Where(g => g.def is AcquireMonsterGoalDef).Any(g => g.reservedMonsters.Contains(m)));
            if (mm == null) {
                Save.getPiecesRequired(1);
            }*/
            return null;
        }

        public override FufilmentStatus Fufil(Goal goal, GoalState state, List<Goal> goals) {
            throw new NotImplementedException();
        }
    }

    public abstract class GoalDef {
        protected GoalDef(string prettyName, GoalType type) {
            Name = prettyName;
            Type = type;
        }
        protected GoalDef(MonsterTypeMap monsterType, GoalType type) {
            if (!Save.MonIdNames.ContainsKey((int)monsterType))
                Name = Save.MonIdNames[(int)monsterType / 100];
            else
                Name = Save.MonIdNames[(int)monsterType];
            Type = type;
        }

        public abstract FufilmentStatus Fufil(Goal goal, GoalState state, List<Goal> goals);
        public string Name { get; set; }
        public GoalType Type { get; set; }
    }
}
