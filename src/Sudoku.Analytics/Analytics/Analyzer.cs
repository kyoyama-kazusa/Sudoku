namespace Sudoku.Analytics;

/// <summary>
/// Provides an analyzer that solves a sudoku puzzle using the human-friendly logics,
/// and creates an <see cref="AnalysisResult"/> instance indicating the analytics data.
/// </summary>
/// <seealso cref="AnalysisResult"/>
/// <seealso cref="Analyzer"/>
public sealed class Analyzer : StepGatherer, IAnalyzer<Analyzer, AnalysisResult, Grid, Step>
{
	/// <summary>
	/// Indicates the default steps capacity.
	/// </summary>
	private const int DefaultStepsCapacity = 54;


	/// <summary>
	/// The random number generator.
	/// </summary>
	private readonly Random _random = new();


	/// <summary>
	/// Indicates whether the solver will apply all found steps in a step searcher, in order to solve a puzzle faster.
	/// </summary>
	public bool IsFullApplying { get; set; }

	/// <summary>
	/// Indicates whether the solver will choose a step to be applied after having searched all possible steps, in random.
	/// </summary>
	public bool RandomizedChoosing { get; set; }

	/// <summary>
	/// Indicates whether the solver will ignore slow step searchers being configured <see cref="StepSearcherRuntimeFlags.TimeComplexity"/>.
	/// </summary>
	/// <remarks>
	/// The default value is <see langword="false"/>.
	/// </remarks>
	/// <seealso cref="StepSearcherRuntimeFlags.TimeComplexity"/>
	public bool IgnoreSlowAlgorithms { get; set; }

	/// <summary>
	/// Indicates whether the solver will ignore slow step searchers being configured <see cref="StepSearcherRuntimeFlags.SpaceComplexity"/>.
	/// </summary>
	/// <remarks>
	/// The default value is <see langword="false"/>.
	/// </remarks>
	/// <seealso cref="StepSearcherRuntimeFlags.SpaceComplexity"/>
	public bool IgnoreHighAllocationAlgorithms { get; set; }

	/// <inheritdoc/>
	public override ReadOnlyMemory<StepSearcher> ResultStepSearchers { get; internal set; } =
		from searcher in StepSearcherFactory.StepSearchers
		where searcher.RunningArea.HasFlag(StepSearcherRunningArea.Searching)
		select searcher;

	/// <inheritdoc/>
	protected override StepSearcherRunningArea RunningArea => StepSearcherRunningArea.Searching;


	/// <summary>
	/// Indicates the default <see cref="Analyzer"/> instance that has no extra configuration.
	/// </summary>
	public static Analyzer Default => new();

	/// <summary>
	/// Indicates an <see cref="Analyzer"/> instance that all possible <see cref="StepSearcher"/> instances are included.
	/// </summary>
	public static Analyzer AllIn
		=> Balanced
			.WithIgnoreHighTimeComplexityStepSearchers(false)
			.WithIgnoreHighSpaceComplexityStepSearchers(false)
			.ApplySetter<NormalFishStepSearcher>(static s => { s.DisableFinnedOrSashimiXWing = false; s.AllowSiamese = true; })
			.ApplySetter<RegularWingStepSearcher>(static s => s.MaxSearchingPivotsCount = 9)
			.ApplySetter<ReverseBivalueUniversalGraveStepSearcher>(static s => { s.MaxSearchingEmptyCellsCount = 4; s.AllowPartiallyUsedTypes = true; })
			.ApplySetter<ComplexFishStepSearcher>(static s => { s.MaxSize = 7; s.AllowSiamese = true; })
			.ApplySetter<XyzRingStepSearcher>(static s => s.AllowSiamese = false)
			.ApplySetter<BowmanBingoStepSearcher>(static s => s.MaxLength = 64)
			.ApplySetter<AlignedExclusionStepSearcher>(static s => s.MaxSearchingSize = 5);

	/// <summary>
	/// Indicates an <see cref="Analyzer"/> instance that has some extra configuration, suitable for a whole analysis lifecycle.
	/// </summary>
	public static Analyzer Balanced
		=> Default
			.WithIgnoreHighTimeComplexityStepSearchers(false)
			.WithIgnoreHighSpaceComplexityStepSearchers(true)
			.ApplySetter<SingleStepSearcher>(static s => { s.EnableFullHouse = true; s.EnableLastDigit = true; s.HiddenSinglesInBlockFirst = true; s.EnableOrderingStepsByLastingValue = false; })
			.ApplySetter<NormalFishStepSearcher>(static s => { s.DisableFinnedOrSashimiXWing = false; s.AllowSiamese = false; })
			.ApplySetter<UniqueRectangleStepSearcher>(static s => { s.AllowIncompleteUniqueRectangles = true; s.SearchForExtendedUniqueRectangles = true; })
			.ApplySetter<BivalueUniversalGraveStepSearcher>(static s => s.SearchExtendedTypes = true)
			.ApplySetter<ReverseBivalueUniversalGraveStepSearcher>(static s => { s.MaxSearchingEmptyCellsCount = 2; s.AllowPartiallyUsedTypes = true; })
			.ApplySetter<RegularWingStepSearcher>(static s => s.MaxSearchingPivotsCount = 5)
			.ApplySetter<TemplateStepSearcher>(static s => s.TemplateDeleteOnly = false)
			.ApplySetter<ComplexFishStepSearcher>(static s => { s.MaxSize = 5; s.AllowSiamese = false; })
			.ApplySetter<XyzRingStepSearcher>(static s => s.AllowSiamese = false)
			.ApplySetter<BowmanBingoStepSearcher>(static s => s.MaxLength = 64)
			.ApplySetter<AlmostLockedCandidatesStepSearcher>(static s => s.CheckAlmostLockedQuadruple = false)
			.ApplySetter<AlignedExclusionStepSearcher>(static s => s.MaxSearchingSize = 3);

	/// <summary>
	/// Indicates an <see cref="Analyzer"/> instance that has some extra configuration, suitable for a whole analysis lifecycle.
	/// </summary>
	public static Analyzer Standard
		=> Default
			.WithIgnoreHighTimeComplexityStepSearchers(false)
			.WithIgnoreHighSpaceComplexityStepSearchers(true)
			.ApplySetter<SingleStepSearcher>(static s => { s.EnableFullHouse = true; s.EnableLastDigit = true; s.HiddenSinglesInBlockFirst = true; s.EnableOrderingStepsByLastingValue = false; })
			.ApplySetter<NormalFishStepSearcher>(static s => { s.DisableFinnedOrSashimiXWing = false; s.AllowSiamese = false; })
			.ApplySetter<UniqueRectangleStepSearcher>(static s => { s.AllowIncompleteUniqueRectangles = true; s.SearchForExtendedUniqueRectangles = true; })
			.ApplySetter<BivalueUniversalGraveStepSearcher>(static s => s.SearchExtendedTypes = true)
			.ApplySetter<ReverseBivalueUniversalGraveStepSearcher>(static s => { s.MaxSearchingEmptyCellsCount = 2; s.AllowPartiallyUsedTypes = true; })
			.ApplySetter<RegularWingStepSearcher>(static s => s.MaxSearchingPivotsCount = 5)
			.ApplySetter<TemplateStepSearcher>(static s => s.TemplateDeleteOnly = false)
			.ApplySetter<ComplexFishStepSearcher>(static s => { s.MaxSize = 5; s.AllowSiamese = false; })
			.ApplySetter<XyzRingStepSearcher>(static s => s.AllowSiamese = false)
			.ApplySetter<BowmanBingoStepSearcher>(static s => s.MaxLength = 64)
			.ApplySetter<AlmostLockedCandidatesStepSearcher>(static s => s.CheckAlmostLockedQuadruple = false)
			.ApplySetter<AlignedExclusionStepSearcher>(static s => s.MaxSearchingSize = 3);

	/// <summary>
	/// Indicates an <see cref="Analyzer"/> instance that only contains SSTS techniques:
	/// <list type="bullet">
	/// <item><see cref="SingleStepSearcher"/></item>
	/// <item><see cref="LockedCandidatesStepSearcher"/></item>
	/// <item><see cref="LockedSubsetStepSearcher"/></item>
	/// <item><see cref="NormalSubsetStepSearcher"/></item>
	/// </list>
	/// </summary>
	/// <seealso cref="SingleStepSearcher"/>
	/// <seealso cref="LockedCandidatesStepSearcher"/>
	/// <seealso cref="LockedSubsetStepSearcher"/>
	/// <seealso cref="NormalSubsetStepSearcher"/>
	public static Analyzer SstsOnly
		=> Default
			.WithStepSearchers(
				new SingleStepSearcher(),
				new LockedSubsetStepSearcher(),
				new LockedCandidatesStepSearcher(),
				new NormalSubsetStepSearcher()
			)
			.WithUserDefinedOptions(new() { IsDirectMode = true })
			.ApplySetter<SingleStepSearcher>(
				static s =>
				{
					s.EnableFullHouse = true;
					s.EnableLastDigit = true;
					s.HiddenSinglesInBlockFirst = true;
					s.EnableOrderingStepsByLastingValue = false;
				}
			);

	/// <summary>
	/// Indicates an <see cref="Analyzer"/> instance that only supports for techniques used in Sudoku Explainer.
	/// </summary>
	public static Analyzer SudokuExplainer
		=> Default
			.WithIgnoreHighTimeComplexityStepSearchers(false)
			.WithIgnoreHighSpaceComplexityStepSearchers(false)
			.WithStepSearchers(
				new SingleStepSearcher(),
				new LockedSubsetStepSearcher(),
				new LockedCandidatesStepSearcher(),
				new NormalSubsetStepSearcher(),
				new NormalFishStepSearcher(),
				new RegularWingStepSearcher(),
				new UniqueRectangleStepSearcher(),
				new UniqueLoopStepSearcher(),
				new BivalueUniversalGraveStepSearcher(),
				new AlignedExclusionStepSearcher(),
				new ChainStepSearcher(),
				new MultipleForcingChainsStepSearcher()
			)
			.WithUserDefinedOptions(new() { IsDirectMode = true })
			.ApplySetter<SingleStepSearcher>(
				static s =>
				{
					s.EnableFullHouse = true;
					s.EnableLastDigit = true;
					s.HiddenSinglesInBlockFirst = true;
					s.EnableOrderingStepsByLastingValue = false;
				}
			)
			.ApplySetter<NormalFishStepSearcher>(static s => s.AllowSiamese = false)
			.ApplySetter<RegularWingStepSearcher>(static s => s.MaxSearchingPivotsCount = 3)
			.ApplySetter<UniqueRectangleStepSearcher>(
				static s =>
				{
					s.AllowIncompleteUniqueRectangles = false;
					s.SearchForExtendedUniqueRectangles = false;
				}
			)
			.ApplySetter<BivalueUniversalGraveStepSearcher>(static s => s.SearchExtendedTypes = false)
			.ApplySetter<AlignedExclusionStepSearcher>(static s => s.MaxSearchingSize = 3);


	/// <summary>
	/// Indicates the event to be triggered when a new step is found.
	/// </summary>
	public event EventHandler<AnalyzerStepFoundEventArgs>? StepFound;

	/// <summary>
	/// Indicates the event to be triggered when the whole analysis operation is finished.
	/// </summary>
	public event EventHandler<AnalyzerFinishedEventArgs>? Finished;

	/// <summary>
	/// Indicates the event to be triggered when an exception is thrown.
	/// </summary>
	public event EventHandler<AnalyzerExceptionThrownEventArgs>? ExceptionThrown;


	/// <summary>
	/// Analyze the specified puzzle, and return an instance of <see cref="AnalysisResult"/> indicating the analyzed result.
	/// </summary>
	/// <param name="grid">Indicates the grid to be checked.</param>
	/// <param name="progress">Indicates the progress reporter object.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	/// <returns>The result value.</returns>
	public AnalysisResult Analyze(
		in Grid grid,
		IProgress<StepGathererProgressPresenter>? progress = null,
		CancellationToken cancellationToken = default
	)
	{
		ref readonly var puzzle = ref grid;
		if (puzzle.IsSolved)
		{
			throw new InvalidOperationException(SR.ExceptionMessage("GridAlreadySolved"));
		}

		ApplySetters(this);

		var result = new AnalysisResult(puzzle) { IsSolved = false };
		var solution = puzzle.SolutionGrid;

		// #1 Memory usage snapshot
		var gcSnapshot1 = GC.GetTotalMemory(false);
		try
		{
			if (puzzle.Uniqueness != Uniqueness.Bad)
			{
				// We should check whether the puzzle is a GSP firstly.
				// This method doesn't check for Sukaku puzzles, or ones containing multiple solutions.
				SymmetryInferrer.TryInfer(puzzle, out var triplet);
				var (symmetricType, mappingDigits, selfPairedDigitsMask) = triplet;

				try
				{
					// Here 'puzzle' may contains multiple solutions, so 'solution' may equal to 'Grid.Undefined'.
					// We will defer the checking inside this method stackframe.
					return result = analyzeInternal(
						puzzle,
						solution,
						result,
						symmetricType,
						mappingDigits,
						selfPairedDigitsMask,
						progress,
						gcSnapshot1,
						cancellationToken
					);
				}
				catch (Exception ex)
				{
					// Trigger the event.
					ExceptionThrown?.Invoke(this, new(ex));

					return result = ex switch
					{
						RuntimeAnalysisException e => e switch
						{
							WrongStepException
								=> result with { IsSolved = false, FailedReason = FailedReason.WrongStep, UnhandledException = e },
							PuzzleInvalidException
								=> result with { IsSolved = false, FailedReason = FailedReason.PuzzleIsInvalid }
						},
						OperationCanceledException { CancellationToken: var c } when c == cancellationToken
							=> result with { IsSolved = false, FailedReason = FailedReason.UserCancelled },
						NotImplementedException or NotSupportedException
							=> result with { IsSolved = false, FailedReason = FailedReason.NotImplemented },
						_
							=> result with { IsSolved = false, FailedReason = FailedReason.ExceptionThrown, UnhandledException = ex }
					};
				}
			}
			return result = result with { IsSolved = false, FailedReason = FailedReason.PuzzleHasNoSolution };
		}
		finally
		{
			// Trigger the event.
			Finished?.Invoke(this, new(result));
		}


		AnalysisResult analyzeInternal(
			in Grid puzzle,
			in Grid solution,
			AnalysisResult resultBase,
			SymmetricType symmetricType,
			ReadOnlySpan<Digit?> mappingDigits,
			Mask selfPairedDigitsMask,
			IProgress<StepGathererProgressPresenter>? progress,
			long gcSnapshot1,
			CancellationToken cancellationToken
		)
		{
			var playground = puzzle;
			var (totalCandidatesCount, stepSearchers) = (playground.CandidatesCount, ResultStepSearchers);
			var (collectedSteps, stepGrids) = (new List<Step>(DefaultStepsCapacity), new List<Grid>(DefaultStepsCapacity));
			var timestampOriginal = Stopwatch.GetTimestamp();
			var accumulator = IsFullApplying || RandomizedChoosing || Options.PrimarySingle != SingleTechniqueFlag.None
				? []
				: default(List<Step>);
			var context = new StepAnalysisContext(playground, in puzzle)
			{
				Accumulator = accumulator,
				Options = Options,
				OnlyFindOne = !IsFullApplying && !RandomizedChoosing && Options.PrimarySingle == SingleTechniqueFlag.None,
				CancellationToken = cancellationToken
			};

			// Determine whether the grid is a GSP pattern. If so, check for eliminations.
			if ((symmetricType, selfPairedDigitsMask) is (not SymmetricType.None, not 0) && !mappingDigits.IsEmpty)
			{
				context.GspPatternInferred = symmetricType;
				context.MappingRelations = mappingDigits;

				if (SymmetryInferrer.GetStep(playground, Options) is { } step)
				{
					if (verifyConclusionValidity(null, solution, step))
					{
						if (onCollectingSteps(
							collectedSteps, step, context, ref playground, timestampOriginal,
							stepGrids, resultBase, gcSnapshot1, cancellationToken, out var result))
						{
							return result;
						}
					}
					else
					{
						throw new WrongStepException(playground, step);
					}
				}
			}

		FindNextStep:
			Initialize(this, playground, solution);

			string progressedStepSearcherName;
			foreach (var searcher in stepSearchers)
			{
				switch (playground, solution, searcher, this)
				{
					case ({ PuzzleType: SudokuType.Sukaku }, _, { Metadata.SupportsSukaku: false }, _):
					case (_, _, { RunningArea: StepSearcherRunningArea.None }, _):
					case (_, _, { Metadata.IsConfiguredSlow: true }, { IgnoreSlowAlgorithms: true }):
					case (_, _, { Metadata.IsConfiguredHighAllocation: true }, { IgnoreHighAllocationAlgorithms: true }):
					case (_, _, { Metadata.IsOnlyRunForDirectViews: true }, { Options.IsDirectMode: false }):
					case (_, _, { Metadata.IsOnlyRunForIndirectViews: true }, { Options.IsDirectMode: true }):
					case (_, { IsUndefined: true }, { Metadata.SupportAnalyzingMultipleSolutionsPuzzle: false }, _):
					{
						// Skips on those two cases:
						//   1) Sukaku puzzles can't use techniques that is marked as "not supported for sukaku".
						//   2) If the searcher is currently disabled.
						//   3) If the searcher is configured as slow.
						//   4) If the searcher is configured as high-allocation.
						//   5) If the searcher is only run for direct view, and the current is indirect view.
						//   6) If the searcher is only run for indirect view, and the current is direct view.
						//   7) If the searcher doesn't support for analyzing puzzles with multiple solutions, but we enable it.
						continue;
					}
#pragma warning disable format
					case (
						_,
						_,
						SingleStepSearcher,
						{
							Options:
							{
								PrimaryHiddenSingleAllowsLines: var allowLine,
								PrimarySingle: var limited and not 0
							}
						}
					):
#pragma warning restore format
					{
						accumulator!.Clear();

						searcher.Collect(ref context);
						if (accumulator.Count == 0)
						{
							continue;
						}

						// Special case: consider the step is a full house, hidden single or naked single,
						// ignoring steps not belonging to the technique set.
						var chosenSteps = new List<SingleStep>();
						foreach (var step in accumulator)
						{
							if (step is SingleStep { Code: var code } s)
							{
								switch (limited, code, allowLine)
								{
									case (SingleTechniqueFlag.FullHouse, not Technique.FullHouse, _):
									{
										break;
									}
									case (_, Technique.FullHouse, _):
									case (
										SingleTechniqueFlag.HiddenSingle,
										Technique.LastDigit or Technique.CrosshatchingBlock or Technique.HiddenSingleBlock,
										false
									):
									case (
										SingleTechniqueFlag.HiddenSingle,
										Technique.LastDigit
											or >= Technique.HiddenSingleBlock and <= Technique.HiddenSingleColumn
											or >= Technique.CrosshatchingBlock and <= Technique.CrosshatchingColumn,
										true
									):
									case (SingleTechniqueFlag.NakedSingle, Technique.NakedSingle, _):
									{
										chosenSteps.Add(s);
										break;
									}
								}
							}
						}
						if (chosenSteps.Count == 0)
						{
							continue;
						}

						if (IsFullApplying)
						{
							foreach (var step in chosenSteps)
							{
								if (!verifyConclusionValidity(searcher, solution, step))
								{
									throw new WrongStepException(playground, step);
								}

								if (onCollectingSteps(
									collectedSteps, step, context, ref playground,
									timestampOriginal, stepGrids, resultBase, gcSnapshot1, cancellationToken, out var result))
								{
									return result;
								}
							}
						}
						else
						{
							var chosenStep = RandomizedChoosing ? chosenSteps[_random.Next(0, chosenSteps.Count)] : chosenSteps[0];
							if (!verifyConclusionValidity(searcher, solution, chosenStep))
							{
								throw new WrongStepException(playground, chosenStep);
							}

							if (onCollectingSteps(
								collectedSteps, chosenStep, context, ref playground,
								timestampOriginal, stepGrids, resultBase, gcSnapshot1, cancellationToken, out var result))
							{
								return result;
							}
						}

						goto MakeProgress;
					}
					case (_, _, BruteForceStepSearcher, { RandomizedChoosing: true }):
					{
						accumulator!.Clear();

						searcher.Collect(ref context);
						if (accumulator.Count == 0)
						{
							continue;
						}

						// Here will fetch a correct step to be applied.
						var chosenStep = accumulator[_random.Next(0, accumulator.Count)];
						if (!verifyConclusionValidity(searcher, solution, chosenStep))
						{
							throw new WrongStepException(playground, chosenStep);
						}

						if (onCollectingSteps(
							collectedSteps, chosenStep, context, ref playground,
							timestampOriginal, stepGrids, resultBase, gcSnapshot1, cancellationToken, out var result))
						{
							return result;
						}

						goto MakeProgress;
					}
					case (_, _, not BruteForceStepSearcher, { IsFullApplying: true } or { RandomizedChoosing: true }):
					{
						accumulator!.Clear();

						searcher.Collect(ref context);
						if (accumulator.Count == 0)
						{
							continue;
						}

						if (RandomizedChoosing)
						{
							// Here will fetch a correct step to be applied.
							var chosenStep = accumulator[_random.Next(0, accumulator.Count)];
							if (!verifyConclusionValidity(searcher, solution, chosenStep))
							{
								throw new WrongStepException(playground, chosenStep);
							}

							if (onCollectingSteps(
								collectedSteps, chosenStep, context, ref playground,
								timestampOriginal, stepGrids, resultBase, gcSnapshot1, cancellationToken, out var result))
							{
								return result;
							}
						}
						else
						{
							foreach (var foundStep in accumulator)
							{
								if (!verifyConclusionValidity(searcher, solution, foundStep))
								{
									throw new WrongStepException(playground, foundStep);
								}

								if (onCollectingSteps(
									collectedSteps, foundStep, context, ref playground, timestampOriginal, stepGrids,
									resultBase, gcSnapshot1, cancellationToken, out var result))
								{
									return result;
								}
							}
						}

						// The puzzle has not been finished, we should turn to the first step finder
						// to continue solving puzzle.
						goto MakeProgress;
					}
					default:
					{
						switch (searcher.Collect(ref context))
						{
							case null:
							{
								continue;
							}
							case var foundStep:
							{
								if (verifyConclusionValidity(searcher, solution, foundStep))
								{
									if (onCollectingSteps(
										collectedSteps, foundStep, context, ref playground, timestampOriginal, stepGrids,
										resultBase, gcSnapshot1, cancellationToken, out var result))
									{
										return result;
									}
								}
								else
								{
									throw new WrongStepException(playground, foundStep);
								}

								// The puzzle has not been finished, we should turn to the first step finder
								// to continue solving puzzle.
								goto MakeProgress;
							}
						}
					}
				}

			MakeProgress:
				progressedStepSearcherName = searcher.ToString(Options.CurrentCulture);
				goto ReportStateAndTryNextStep;
			}

			// All solver can't finish the puzzle... :(
			return resultBase with
			{
				FailedReason = FailedReason.AnalyzerGiveUp,
				ElapsedTime = Stopwatch.GetElapsedTime(timestampOriginal),
				InterimSteps = [.. collectedSteps],
				InterimGrids = [.. stepGrids]
			};

		ReportStateAndTryNextStep:
			progress?.Report(new(progressedStepSearcherName, (double)(totalCandidatesCount - playground.CandidatesCount) / totalCandidatesCount));
			goto FindNextStep;


			static bool verifyConclusionValidity(StepSearcher? searcher, in Grid solution, Step step)
			{
				if (searcher?.Metadata.SkipVerificationOnConclusions ?? false)
				{
					// Skip steps that is always correct.
					return true;
				}

				if (solution.IsUndefined)
				{
					// This will be triggered when the puzzle has multiple solutions.
					return true;
				}

				foreach (var (t, c, d) in step.Conclusions)
				{
					var digit = solution.GetDigit(c);
					if (t == Assignment && digit != d || t == Elimination && digit == d)
					{
						return false;
					}
				}
				return true;
			}

			bool onCollectingSteps(
				List<Step> steps,
				Step step,
				in StepAnalysisContext context,
				ref Grid playground,
				long timestampOriginal,
				List<Grid> steppingGrids,
				AnalysisResult resultBase,
				long gcSnapshot1,
				CancellationToken cancellationToken,
				[NotNullWhen(true)] out AnalysisResult? result
			)
			{
				// Optimization: If the grid is inferred as a GSP pattern, we can directly add extra eliminations at symmetric positions.
				if (context is { GspPatternInferred: { } symmetricType } && step is not GurthSymmetricalPlacementStep)
				{
					var mappingRelations = context.MappingRelations;
					var originalConclusions = step.Conclusions;
					var newConclusions = new List<Conclusion>();
					foreach (var conclusion in originalConclusions)
					{
						var newConclusion = conclusion.GetSymmetricConclusion(symmetricType, mappingRelations[conclusion.Digit] ?? -1);
						if (newConclusion != conclusion && playground.Exists(newConclusion.Cell, newConclusion.Digit) is true)
						{
							newConclusions.Add(newConclusion);
						}
					}

					ConclusionBackingField(step) = (Conclusion[])[.. originalConclusions, .. newConclusions];
				}

				var atLeastOneConclusionIsWorth = false;
				foreach (var (t, c, d) in step.Conclusions)
				{
					switch (t)
					{
						case Assignment when playground.GetState(c) == CellState.Empty:
						case Elimination when playground.Exists(c, d) is true:
						{
							atLeastOneConclusionIsWorth = true;

							goto FinalCheck;
						}
					}
				}

			FinalCheck:
				if (atLeastOneConclusionIsWorth)
				{
					steppingGrids.AddRef(playground);
					playground.Apply(step);
					steps.Add(step);

					// Trigger the event.
					StepFound?.Invoke(this, new(step));

					if (playground.IsSolved)
					{
						var gcSnapshot2 = GC.GetTotalMemory(false);

						result = resultBase with
						{
							IsSolved = true,
							Solution = playground,
							ElapsedTime = Stopwatch.GetElapsedTime(timestampOriginal),
							InterimSteps = [.. steps],
							InterimGrids = [.. steppingGrids],
							MemoryUsed = gcSnapshot2 - gcSnapshot1
						};
						return true;
					}
				}
				else
				{
					// No steps are available.
					goto ReturnFalse;
				}

				cancellationToken.ThrowIfCancellationRequested();

			ReturnFalse:
				result = null;
				return false;
			}
		}
	}

	/// <inheritdoc/>
	AnalysisResult IAnalyzer<Analyzer, AnalysisResult, Grid, Step>.Analyze(Grid board, CancellationToken cancellationToken)
		=> Analyze(board, cancellationToken: cancellationToken);


	/// <summary>
	/// Gets field <c><![CDATA[<Conclusions>k__BackingField]]></c> inside type <see cref="Step"/>.
	/// </summary>
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Conclusions>k__BackingField")]
	private static extern ref StepConclusions ConclusionBackingField(Step step);
}
