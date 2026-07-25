namespace Sudoku.Analytics;

/// <summary>
/// Represents an instance that can collect all possible <see cref="Step"/>s in a grid for one state.
/// </summary>
public sealed class Collector : StepGatherer
{
	/// <summary>
	/// Indicates the maximum steps can be collected.
	/// </summary>
	public int MaxStepsCollected { get; set; } = 1000;

	/// <summary>
	/// Indicates the difficulty level mode that the step searcher will be called and checked.
	/// </summary>
	public CollectorDifficultyLevelMode DifficultyLevelMode { get; set; } = CollectorDifficultyLevelMode.OnlySame;

	/// <inheritdoc/>
	public override ReadOnlyMemory<StepSearcher> ResultStepSearchers { get; internal set; } =
		from searcher in StepSearcher.StepSearchers
		where searcher.RunningArea.HasFlag(StepSearcherRunningArea.Collecting)
		select searcher;

	/// <inheritdoc/>
	protected override StepSearcherRunningArea RunningArea => StepSearcherRunningArea.Collecting;


	/// <summary>
	/// Search for all possible <see cref="Step"/> instances appeared at the specified grid state.
	/// </summary>
	/// <param name="grid">Indicates the grid to be checked.</param>
	/// <param name="progress">Indicates the progress reporter object.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>The result value.</returns>
	/// <exception cref="InvalidOperationException">
	/// Throws when property <see cref="DifficultyLevelMode"/> is not defined.
	/// </exception>
	public ReadOnlySpan<Step> Collect(
		scoped in Grid grid,
		IProgress<StepGathererProgressPresenter>? progress = null,
		CancellationToken cancellationToken = default
	)
	{
		if (!Enum.IsDefined(DifficultyLevelMode))
		{
			throw new InvalidOperationException(SR.ExceptionMessage("ModeIsUndefined"));
		}

		ref readonly var puzzle = ref grid;
		if (puzzle.IsSolved)
		{
			return [];
		}

		ApplySetters(this);

		try
		{
			return s(progress, puzzle, cancellationToken);
		}
		catch
		{
			throw;
		}


		ReadOnlySpan<Step> s(IProgress<StepGathererProgressPresenter>? progress, scoped in Grid puzzle, CancellationToken cancellationToken)
		{
			const int defaultLevel = int.MaxValue;

			var possibleStepSearchers = ResultStepSearchers;
			var totalSearchersCount = possibleStepSearchers.Length;

			var playground = puzzle;
			Initialize(playground, playground.SolutionGrid);

			var accumulator = new List<Step>();
			var context = new StepAnalysisContext(in playground, in puzzle)
			{
				Accumulator = accumulator,
				OnlyFindOne = false,
				Options = Options,
				CancellationToken = cancellationToken
			};
			var (l, bag, currentSearcherIndex) = (defaultLevel, new List<Step>(), 0);
			foreach (var searcher in possibleStepSearchers)
			{
			searcher_branches:
				switch (searcher)
				{
					case { RunningArea: var runningArea } when !runningArea.HasFlag(StepSearcherRunningArea.Collecting):
					case { Metadata.SupportsSukaku: false } when puzzle.IsSukaku:
					{
						break searcher_branches;
					}
					case { Level: var currentLevel }:
					{
						// If a searcher contains the upper level, it will be skipped.
						switch (DifficultyLevelMode)
						{
							case CollectorDifficultyLevelMode.OnlySame
								when l != defaultLevel && currentLevel <= l || l == defaultLevel:
							case CollectorDifficultyLevelMode.OneLevelHarder
								when l != defaultLevel && currentLevel <= l + 1 || l == defaultLevel:
							case CollectorDifficultyLevelMode.All:
							{
								break;
							}
							default:
							{
								break searcher_branches;
							}
						}

						if (cancellationToken.IsCancellationRequested)
						{
							return null;
						}

						accumulator.Clear();
						searcher.Collect(ref context);
						if (accumulator.Count == 0)
						{
							break searcher_branches;
						}

						l = currentLevel;
						bag.AddRange(accumulator.AsSpan()[..Math.Min(MaxStepsCollected - bag.Count, accumulator.Count)]);
						break;
					}
				}

				// Report the progress if worth.
				progress?.Report(new(searcher.ToString(Options.CurrentCulture), ++currentSearcherIndex / (double)totalSearchersCount));
			}

			// Return the result.
			return bag.AsSpan();
		}
	}
}
