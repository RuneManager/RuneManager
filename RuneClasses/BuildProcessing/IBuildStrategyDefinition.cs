using RuneOptim.Management;
using RuneOptim.swar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
	interface IBuildStrategyDefinition {

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

	public interface IBuildRunner {


		void Setup(Build build, BuildSettings settings);
		void TearDown();

		Task<Monster> Run(IEnumerable<Rune> inRunes);

	}

	public abstract class BuildFast : IBuildStrategyDefinition {
		public abstract int Order { get; }
		public abstract string Name { get; }

		public abstract IBuildRunner GetRunner(BuildSettings settings);

		public IBuildRunner GetRunner() {
			throw new NotImplementedException();
		}

		public virtual bool IsValid(Build b) {
			foreach (var a in RuneOptim.BuildProcessing.Build.ExtraEnums) {
				if (b.Sort[a] != 0)
					return false;
			}
			return true;
		}
	}

	public class BuildBrokenFast : BuildFast {
		public override int Order { get => 1; }
		public override string Name { get; }

		public override IBuildRunner GetRunner(BuildSettings settings) {
			throw new NotImplementedException();
		}

		public override bool IsValid(Build b) {

			if (!base.IsValid(b))
				return false;

			if (b.RequiredSets.Any())
				return false;

			return b.AllowBroken;
		}


	}

	public class BuildFourSetBrokenFast : BuildFast {
		public override int Order { get => 2; }
		public override string Name { get; }

		public override IBuildRunner GetRunner(BuildSettings settings) {
			throw new NotImplementedException();
		}

		public override bool IsValid(Build b) {

			if (!base.IsValid(b))
				return false;


			if (b.RequiredSets.Count > 1)
				return false;
			if (Rune.SetRequired(b.RequiredSets.FirstOrDefault()) != 4)
				return false;

			return b.AllowBroken;
		}


	}

	/// <summary>
	/// Pick good looking runes by set, make a full house.
	/// Match 2s and 4s by comparing bits
	/// </summary>
	public class BuildBitMatcher : IBuildStrategyDefinition {
		public int Order { get => 100; }
		public string Name { get; }

		public IBuildRunner GetRunner(BuildSettings settings) {
			throw new NotImplementedException();
		}

		public IBuildRunner GetRunner() {
			throw new NotImplementedException();
		}

		public bool IsValid(Build b) {
			return !b.AllowBroken;
		}
	}

	/// <summary>
	/// Use fast to pick a non-Extra winner, then pick the winner based on extra
	/// </summary>
	public class BuildTieBreakerFast : BuildFast {
		public override int Order { get => 10; }
		public override string Name { get; }

		public override IBuildRunner GetRunner(BuildSettings settings) {
			throw new NotImplementedException();
		}
	}

}
