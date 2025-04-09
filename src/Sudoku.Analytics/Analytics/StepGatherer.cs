namespace Sudoku.Analytics;

/// <summary>
/// Represents a type that support collecting <see cref="Step"/> instances on a certain checking logic for a grid.
/// </summary>
public abstract class StepGatherer
{
	/// <summary>
	/// <para>
	/// Indicates the <see cref="StepSearcher"/> instances to run.
	/// By default, the solver will use <see cref="StepSearcherFactory.StepSearchers"/> to solve a puzzle.
	/// If you assign a new array of <see cref="StepSearcher"/>s into this property
	/// the step searchers will use this property instead of <see cref="StepSearcherFactory.StepSearchers"/> to solve a puzzle.
	/// </para>
	/// <para>
	/// Please note that the property will keep the <see langword="null"/> value if you don't assign any values into it;
	/// however, if you want to use the customized collection to solve a puzzle, assign a non-<see langword="null"/> array into it.
	/// </para>
	/// </summary>
	/// <seealso cref="StepSearcherFactory.StepSearchers"/>
	public ReadOnlyMemory<StepSearcher> StepSearchers
	{
		get;

		set
		{
			field = value;
			ResultStepSearchers = from searcher in field where searcher.RunningArea.HasFlag(RunningArea) select searcher;
		}
	}

	/// <summary>
	/// Indicates the result step searchers used in the current analyzer or collector.
	/// </summary>
	public abstract ReadOnlyMemory<StepSearcher> ResultStepSearchers { get; internal set; }

	/// <summary>
	/// Indicates the extra options to be set. The options will be passed into <see cref="Step"/> instances collected
	/// in internal method called <c>Collect</c>, and create <see cref="Step"/> instances and pass into constructor.
	/// </summary>
	/// <seealso cref="Step"/>
	public StepGathererOptions Options { get; set; } = StepGathererOptions.Default;

	/// <summary>
	/// Represents a list of <see cref="Action{T}"/> of <see cref="StepSearcher"/> instances
	/// to assign extra configuration to step searcher instances.
	/// </summary>
	/// <seealso cref="Action{T}"/>
	/// <seealso cref="StepSearcher"/>
	public ICollection<Action<StepSearcher>> Setters { get; } = [];

	/// <summary>
	/// Indicates the running area.
	/// </summary>
	protected abstract StepSearcherRunningArea RunningArea { get; }


	/// <summary>
	/// Try to apply setters.
	/// </summary>
	/// <param name="instance">The instance itself.</param>
	public static void ApplySetters(StepGatherer instance)
	{
		foreach (var setter in instance.Setters)
		{
			foreach (var stepSearcher in instance.ResultStepSearchers)
			{
				setter(stepSearcher);
			}
		}
	}
}
