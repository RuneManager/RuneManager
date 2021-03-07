using RuneOptim.Management;
using System.Text;

namespace RuneOptim.BuildProcessing {
    public interface IBuildStrategyDefinition {

        /// <summary>
        /// How "fast" the strategy is, 0 being instant.
        /// </summary>
        int Order { get; }

        string Name { get; }

        /// <summary>
        /// If the strategy can be used for the build
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        bool IsValid(Build build);

        /// <summary>
        /// Use the strategy
        /// </summary>
        /// <param name="build"></param>
        /// <returns></returns>
        IBuildRunner GetRunner();

    }

}
