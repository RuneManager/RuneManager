using RuneOptim.Management;
using RuneOptim.swar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuneOptim.BuildProcessing {
	public class BuildFallback : IBuildStrategyDefinition {
		public int Order { get => 10_000; }
		public string Name { get; }


		public IBuildRunner GetRunner() {
			return new BuildFallbackRunner();
		}

		public bool IsValid(Build b) {
			return true;
		}

		public class BuildFallbackRunner : IBuildRunner {
			private Build build;
			private BuildSettings settings;

			public BuildFallbackRunner() {
			}

			public void Setup(Build build, BuildSettings settings) {
				this.build = build;
				this.settings = settings;
			}

			public void TearDown() {

			}

			volatile Monster best;
			volatile bool isRunning;

			class loopData {
				internal Stats Minimum;
				internal int kill;
				internal int plus;
				internal List<Monster> list;
				internal Stats Maximum;
				internal BuildSettings settings;
			}

			Rune[][] runes;
			double currentScore;

			public Task<Monster> Run(IEnumerable<Rune> inRunes) {

				runes = inRunes.GroupBy(r => r.Slot).OrderBy(r => r.Key).Select(r => r.ToArray()).ToArray();

				var cts = new CancellationTokenSource();

				var options = new ParallelOptions() {
					CancellationToken = cts.Token,
				};

				ConcurrentBag<Monster> bag = new ConcurrentBag<Monster>();

				currentScore = build.CalcScore(build.Mon);

				// TODO: async
				return Task.Run(() => {
					var result = Parallel.ForEach(runes[0], options,
						() => new loopData {
							Minimum = (Stats)null,
							kill = 0,
							plus = 0,
							list = new List<Monster>(),
							settings = this.settings,
							Maximum = (Stats)null,
						},
						runLoop,
						data => {
							foreach (var m in data.list) {
								bag.Add(m);
							}
						});
					best = bag.OrderByDescending(a => build.CalcScore(a)).FirstOrDefault();
					return best;
				});
			}

			private loopData runLoop(Rune rune, ParallelLoopState state, long ind, loopData data) {
				if (!isRunning || state.IsStopped || state.ShouldExitCurrentIteration || state.IsExceptional) {
					state.Stop();
					return data;
				}

				Monster m = new Monster();

				m.ApplyRune(rune);

				void bake() {
					var cstats = m.GetStats();
					bool isBad = false;

					// check if build meets minimum
					isBad |= !data.settings.RunesOnlyFillEmpty && !data.settings.AllowBroken && !m.Current.SetsFull;

					isBad |= data.Maximum != null && cstats.AnyExceed(data.Maximum);

					isBad |= !data.settings.RunesOnlyFillEmpty && data.Minimum != null && !cstats.GreaterEqual(data.Minimum, true);

					double curScore;

					if (isBad) {
						data.kill++;
						curScore = 0;
					}
					else {
						// try to reduce CalcScore hits
						curScore = build.CalcScore(cstats);
						isBad |= data.settings.IgnoreLess5 && curScore < currentScore * 1.05;
						if (isBad)
							data.kill++;
					}

					if (!isBad) {
						// we found an okay build!
						data.plus++;
						m.score = curScore;
						data.list.Add(new Monster(m, true));
					}
				}

				void pick(int s) {
					if (s < 6) {
						foreach (var r in runes[s]) {
							m.ApplyRune(r);
							pick(s + 1);
						}
					}
					else {
						bake();
					}
				}

				pick(1);

				return data;
			}
		}

	}
}
