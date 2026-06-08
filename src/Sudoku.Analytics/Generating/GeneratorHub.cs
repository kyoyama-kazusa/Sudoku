namespace Sudoku.Generating;

/// <summary>
/// Represents an entry point for generating puzzles with mixed configuration.
/// </summary>
public static partial class GeneratorHub
{
	/// <summary>
	/// Provides a way to generate a puzzle in asynchronous environment.
	/// </summary>
	/// <typeparam name="TProgressDataProvider">The type of progress data provider.</typeparam>
	/// <param name="onlyGenerateOne">Indicates whether the method only generate one puzzle and return.</param>
	/// <param name="generatorDriver">The generator driver.</param>
	/// <returns>A task that encapsulates asynchronous operation.</returns>
	/// <exception cref="OperationCanceledException">Throws when operation canceled.</exception>
	public static async Task GenerateAsync<TProgressDataProvider>(bool onlyGenerateOne, IGeneratorDriver<TProgressDataProvider> generatorDriver)
		where TProgressDataProvider : struct, IEquatable<TProgressDataProvider>, IProgressDataProvider<TProgressDataProvider>
	{
		// State initializer.
		generatorDriver.StateInitializer();

		// Cancellation token source assigner.
		using var cts = new CancellationTokenSource();
		generatorDriver.CancellationTokenSourceAssigner(cts);

		// Core operations.
		var filters = generatorDriver.BottleneckFiltersCreator();
		var constraints = generatorDriver.ConstraintsCreator();
		var difficultyLevel = generatorDriver.DifficultyLevelCreator(constraints);
		var analyzer = generatorDriver.AnalyzerCreator(difficultyLevel);
		var ittoryuFinder = generatorDriver.IttoryuFinderCreator();
		var (generatingCount, generatingFilteredCount) = (0, 0);
		if (onlyGenerateOne)
		{
			switch (await Task.Run(taskEntry))
			{
				case ({ IsUndefined: false } grid, false):
				{
					generatorDriver.GridStateChanger?.Invoke(ref grid, analyzer);
					generatorDriver.GridTextConsumer?.Invoke(grid.ToString("#"));
					break;
				}
				case (_, true):
				{
					break;
				}
			}
		}
		else
		{
			while (true)
			{
				switch (await Task.Run(taskEntry))
				{
					case ({ IsUndefined: false } grid, false):
					{
						generatorDriver.GridStateChanger?.Invoke(ref grid, analyzer);
						generatorDriver.GridTextConsumer?.Invoke(grid.ToString("#"));

						generatingFilteredCount++;
						continue;
					}
					case (_, true):
					{
						goto BreakWhileTrueLoop;
					}
				}
				continue;

			BreakWhileTrueLoop:
				break;
			}
		}

		// State recoverer.
		generatorDriver.StateFinalizer();


		unsafe (Grid TargetGrid, bool IsCanceled) taskEntry()
		{
			var specializedConditions = (
				HasFullHouseConstraint:
					constraints.OfType<PrimarySingleConstraint>() is [{ Primary: SingleTechniqueFlag.FullHouse }],
				HasNakedSingleConstraint:
					constraints.OfType<PrimarySingleConstraint>() is [{ Primary: SingleTechniqueFlag.NakedSingle }],
				HasFullHouseConstraintInTechniqueSet:
					constraints.OfType<TechniqueSetConstraint>() is [{ Techniques: [Technique.FullHouse] }],
				HasNakedSingleConstraintInTechniqueSet:
					constraints.OfType<TechniqueSetConstraint>() is [{ Techniques: [Technique.NakedSingle] }],
				HasIttoryuConstraint:
					constraints.OfType<IttoryuConstraint>() is [{ Operator: ComparisonOperator.Equality, Rounds: 1 }],
				HasMissingDigitConstraint:
					constraints.OfType<MissingDigitConstraint>() is [{ Digit: not -1 }],
				HasMissingHouseConstraint:
					constraints.Has<EmptyHousesCountConstraint>()
			);
			var result = HandlerCore(
				ref generatingCount,
				ref generatingFilteredCount,
				constraints,
				specializedConditions switch
				{
					{ HasFullHouseConstraint: true } or { HasFullHouseConstraintInTechniqueSet: true } => &Optimizer_FullHouseOnly,
					{ HasNakedSingleConstraint: true } or { HasNakedSingleConstraintInTechniqueSet: true } => &Optimizer_NakedSingleOnly,
					{ HasIttoryuConstraint: true } => &Optimizer_IttoryuMode,
					{ HasMissingDigitConstraint: true, HasIttoryuConstraint: false } => &Optimizer_MissingDigit,
					{ HasMissingHouseConstraint: true } => &Optimizer_EmptyHouses,
					_ => &DefaultGenerator
				},
				specializedConditions is { HasMissingDigitConstraint: true, HasIttoryuConstraint: false }
					? &TransformChecker_MissingDigit
					: null,
				specializedConditions is { HasMissingDigitConstraint: true, HasIttoryuConstraint: false }
					? &Transformer_MissingDigit
					: null,
				generatorDriver.ReportAction,
				specializedConditions is { HasNakedSingleConstraint: true } or { HasNakedSingleConstraintInTechniqueSet: true }
					? analyzer.WithOptions(analyzer.Options with { PrimarySingle = SingleTechniqueFlag.NakedSingle })
					: analyzer,
				ittoryuFinder,
				filters,
				cts.Token
			);
			return (result, cts.Token.IsCancellationRequested);
		}
	}

	/// <summary>
	/// The backing handler.
	/// </summary>
	private static unsafe Grid HandlerCore<TProgressDataProvider>(
		ref int generatingCount,
		ref int generatingFilteredCount,
		ConstraintCollection constraints,
		delegate*<int, SymmetricType, ConstraintCollection, CancellationToken, Grid> gridCreator,
		[AllowNull, MaybeNull] delegate*<in Grid, out object?, bool> gridTransformingChecker,
		[AllowNull, MaybeNull] delegate*<ref Grid, ConstraintCollection, object?, void> gridTransformer,
		Action<TProgressDataProvider> reporter,
		Analyzer analyzer,
		DisorderedIttoryuFinder finder,
		BottleneckFilter[] filters,
		CancellationToken cancellationToken
	)
		where TProgressDataProvider : struct, IEquatable<TProgressDataProvider>, IProgressDataProvider<TProgressDataProvider>
	{
		// Update generating configurations.
		if (constraints.OfType<BottleneckTechniqueConstraint>() is { Length: not 0 } list)
		{
			foreach (var element in list)
			{
				element.Filters = filters;
			}
		}

		var rng = Random.Shared;
		var symmetries = GetSymmetry(constraints);
		var chosenGivensCountSeed = GetChosenGivensCountRange(constraints);
		var givensCount = GetGivensCount(rng, chosenGivensCountSeed);
		var difficultyLevel = GetDifficultyLevel(constraints, rng);
		var progress = new SelfReportingProgress<TProgressDataProvider>(reporter);
		while (true)
		{
			var chosenSymmetricType = symmetries.Length == 0 ? SymmetricType.None : rng.Choose(symmetries);
			var grid = gridCreator(givensCount, chosenSymmetricType, constraints, cancellationToken);
			if (grid.IsEmpty || analyzer.Analyze(grid, cancellationToken: cancellationToken) is not { IsSolved: true } analysisResult)
			{
				goto ReportState;
			}

			// Transform if worth. This transform rules may conflict with other rules so be careful to use this.
			if (gridTransformingChecker != null && gridTransformingChecker(grid, out var outVariable) && gridTransformer != null)
			{
				gridTransformer(ref grid, constraints, outVariable);
			}

			if (constraints.IsValidFor(new(grid, analysisResult)))
			{
				return grid;
			}

		ReportState:
			progress.Report(TProgressDataProvider.Create(++generatingCount, generatingFilteredCount));
			if (cancellationToken.IsCancellationRequested)
			{
				return Grid.Undefined;
			}
		}
	}

	private static partial DifficultyLevel GetDifficultyLevel(ConstraintCollection constraints, Random rng);
	private static partial Cell GetGivensCount(Random rng, (Cell, Cell) chosenGivensCountSeed);
	private static partial (Cell, Cell) GetChosenGivensCountRange(ConstraintCollection constraints);
	private static partial (Cell, Cell) DetermineEmptyCellsCount(BetweenRule betweenRule, Cell start, Cell end);
	private static partial ReadOnlySpan<SymmetricType> GetSymmetry(ConstraintCollection constraints);

	private static partial bool TransformChecker_MissingDigit(in Grid grid, out object? result);

	private static partial void Transformer_MissingDigit(ref Grid grid, ConstraintCollection constraints, object? variable);

	private static partial Grid Optimizer_FullHouseOnly(Cell givens, SymmetricType type, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_NakedSingleOnly(Cell givens, SymmetricType type, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_IttoryuMode(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_MissingDigit(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid Optimizer_EmptyHouses(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
	private static partial Grid DefaultGenerator(Cell givens, SymmetricType symmetry, ConstraintCollection constraints, CancellationToken ct);
}
