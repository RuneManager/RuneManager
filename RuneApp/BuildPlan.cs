using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RuneOptim.BuildProcessing;
using RuneOptim.Management;
using RuneOptim.swar;

namespace RuneApp
{
    class BuildPlanConfig
    {
        public int minImprovement = 0;
        public bool fillOnly = false;
    }

    class BuildPlan
    {
        public enum BuildStrategies
        {
            Skip = 0,  // monster should end up unruned
            Lock = -1,  // monster retains current runes
            Build = 1,  // iterate through enabled build until one succeeds
        } 

        private BuildStrategies _buildStrategy = BuildStrategies.Build;
        public BuildStrategies buildStrategy
        {
            get
            {
                return _buildStrategy;
            }
            set
            {
                _buildStrategy = value;
                if (_buildStrategy == BuildStrategies.Lock)
                {
                    if (monster != null)
                        best = monster.Current;
                }
                else if (_buildStrategy == BuildStrategies.Skip)
                {
                    best = new Loadout();
                }
            }
        }

        private Monster _monster;
        public Monster monster
        {
            get
            {
                return _monster;
            }
            set
            {
                _monster = value;
                if (buildStrategy == BuildStrategies.Lock)
                    best = monster.Current;
            }
        }

        public BuildPlanConfig config;
        // each build is configured with a leader (appropriate to their situation)
        public Build[] builds;
        public Loadout best;
    }
}
