using RuneOptim.swar;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
    public interface IBuildRunner {


        void Setup(Build build, BuildSettings settings);
        void TearDown();
        void Cancel();

        Task<Monster> Run(IEnumerable<Rune> inRunes);

        long Good { get; }
        long Completed { get; }
        long Expected { get; }
        long Skipped { get; }

    }

}
