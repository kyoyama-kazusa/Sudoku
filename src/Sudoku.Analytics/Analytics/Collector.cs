namespace Sudoku.Analytics;

/// <summary>
/// Represents an instance that can collect all possible <see cref="Step"/>s in a grid for one state.
/// </summary>
public sealed class Collector : StepGatherer, ICollector<Grid, Step>
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
		from searcher in StepSearcherFactory.StepSearchers
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
		catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
		{
			return null;
		}
		catch
		{
			throw;
		}


		ReadOnlySpan<Step> s(IProgress<StepGathererProgressPresenter>? progress, scoped in Grid puzzle, CancellationToken ct)
		{
			const int defaultLevel = int.MaxValue;

			var possibleStepSearchers = ResultStepSearchers;
			var totalSearchersCount = possibleStepSearchers.Length;

			var playground = puzzle;
			Initialize(playground, playground.SolutionGrid);

			var accumulator = new List<Step>();
			var context = new StepAnalysisContext(playground, in puzzle)
			{
				Accumulator = accumulator,
				OnlyFindOne = false,
				Options = Options,
				CancellationToken = cancellationToken
			};
			var (l, bag, currentSearcherIndex) = (defaultLevel, new List<Step>(), 0);
			foreach (var searcher in possibleStepSearchers)
			{
				switch (searcher)
				{
					case { RunningArea: var runningArea } when !runningArea.HasFlag(StepSearcherRunningArea.Collecting):
					case { Metadata.SupportsSukaku: false } when puzzle.PuzzleType == SudokuType.Sukaku:
					{
						goto ReportProgress;
					}
					case { Level: var currentLevel }:
					{
						// If a searcher contains the upper level, it will be skipped.
						switch (DifficultyLevelMode)
						{
							case CollectorDifficultyLevelMode.OnlySame when l != defaultLevel && currentLevel <= l || l == defaultLevel:
							case CollectorDifficultyLevelMode.OneLevelHarder when l != defaultLevel && currentLevel <= l + 1 || l == defaultLevel:
							case CollectorDifficultyLevelMode.All:
							{
								break;
							}
							default:
							{
								goto ReportProgress;
							}
						}

						ct.ThrowIfCancellationRequested();

						// Searching.
						accumulator.Clear();

						searcher.Collect(ref context);

						if (accumulator.Count is not (var count and not 0))
						{
							goto ReportProgress;
						}

						l = currentLevel;
						bag.AddRange(count > MaxStepsCollected ? accumulator[..MaxStepsCollected] : accumulator);
						break;
					}
				}

			// Report the progress if worth.
			ReportProgress:
				progress?.Report(new(searcher.ToString(Options.CurrentCulture), ++currentSearcherIndex / (double)totalSearchersCount));
			}

			// Return the result.
			return bag.AsSpan();
		}
	}

	/// <inheritdoc/>
	ReadOnlySpan<Step> ICollector<Grid, Step>.Collect(Grid board, CancellationToken cancellationToken)
		=> Collect(board, cancellationToken: cancellationToken);
}
