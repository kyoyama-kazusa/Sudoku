namespace Sudoku.Generating;

/// <summary>
/// Provides a driver object that registers a complex generator.
/// </summary>
/// <typeparam name="TProgressDataProvider">The type of progress data provider.</typeparam>
public interface IGeneratorDriver<TProgressDataProvider>
	where TProgressDataProvider :
		struct,
		IEquatable<TProgressDataProvider>,
		IProgressDataProvider<TProgressDataProvider>,
		allows ref struct
{
	/// <summary>
	/// The method that create constraints.
	/// </summary>
	Func<ConstraintCollection> ConstraintsCreator { get; init; }

	/// <summary>
	/// The method that create a difficulty level.
	/// </summary>
	Func<ConstraintCollection, DifficultyLevel> DifficultyLevelCreator { get; init; }

	/// <summary>
	/// The method that create a analyzer.
	/// </summary>
	Func<DifficultyLevel, Analyzer> AnalyzerCreator { get; init; }

	/// <summary>
	/// The method that create a ittoryu finder.
	/// </summary>
	Func<DisorderedIttoryuFinder> IttoryuFinderCreator { get; init; }

	/// <summary>
	/// The assigner operation for <see cref="CancellationTokenSource"/> object.
	/// </summary>
	Action<CancellationTokenSource> CancellationTokenSourceAssigner { get; init; }

	/// <summary>
	/// The state-initialization operation.
	/// </summary>
	Action StateInitializer { get; init; }

	/// <summary>
	/// The state finalizer operation.
	/// </summary>
	Action StateFinalizer { get; init; }

	/// <summary>
	/// The bottleneck filters creator.
	/// </summary>
	Func<BottleneckFilter[]> BottleneckFiltersCreator { get; init; }

	/// <summary>
	/// The progress-report action.
	/// </summary>
	Action<TProgressDataProvider> ReportAction { get; init; }

	/// <summary>
	/// The grid state changer.
	/// </summary>
	GridStateChanger<Analyzer>? GridStateChanger { get; init; }

	/// <summary>
	/// The grid text consumer, triggered after each puzzle generated.
	/// </summary>
	Action<string>? GridTextConsumer { get; init; }
}
