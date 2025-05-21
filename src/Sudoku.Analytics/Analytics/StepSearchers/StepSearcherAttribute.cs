namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Indicates a type marked this attribute is a runnable <see cref="StepSearcher"/>.
/// </summary>
/// <param name="nameKey"><inheritdoc cref="NameKey" path="/summary"/></param>
/// <param name="techniques">All supported techniques.</param>
/// <seealso cref="StepSearcher"/>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class StepSearcherAttribute(string nameKey, params Technique[] techniques) : Attribute
{
	/// <summary>
	/// <para>
	/// Indicates the technique searcher doesn't use any cached fields in implementation, i.e. caching-free or caching-safe.
	/// </para>
	/// <para>
	/// This property can be used by checking free-caching rules, in order to inline-declaring data members,
	/// storing inside another step searcher type, or creating a temporary logic to call this searcher.
	/// </para>
	/// </summary>
	public bool IsCachingSafe { get; init; }

	/// <summary>
	/// <para>
	/// Indicates the technique searcher always uses cached fields in implementation, i.e. caching-unsafe.
	/// </para>
	/// <para>
	/// As a property reversed from <see cref="IsCachingSafe"/>, this property tags for a step searcher not using
	/// <see cref="StepAnalysisContext.Grid"/>. If a step searcher type always use caching fields without any doubt,
	/// this property should be set with <see langword="true"/>.
	/// </para>
	/// <para><i>
	/// This property won't be used in API, but it may be used in future versions.
	/// </i></para>
	/// </summary>
	/// <seealso cref="IsCachingSafe"/>
	/// <seealso cref="StepAnalysisContext.Grid"/>
	public bool IsCachingUnsafe { get; init; }

	/// <summary>
	/// Indicates whether the option is read-only that cannot be modified in UI,
	/// meaning a user cannot modify the ordering of this step searcher.
	/// </summary>
	public bool IsOrderingFixed { get; init; }

	/// <summary>
	/// Indicates whether the option is read-only that cannot be modified in UI,
	/// meaning a user cannot disable the current step searcher.
	/// </summary>
	public bool IsAvailabilityReadOnly { get; init; }

	/// <summary>
	/// Indicates whether the step searcher can be invoked by puzzles containing multiple solutions.
	/// By default the value is <see langword="true"/>.
	/// </summary>
	public bool SupportAnalyzingMultipleSolutionsPuzzle { get; init; } = true;

	/// <summary>
	/// Indicates the key in resource dictionary.
	/// </summary>
	public string NameKey { get; } = nameKey;

	/// <summary>
	/// Indicates the runtime options that controls extra behaviors.
	/// </summary>
	public StepSearcherRuntimeFlags RuntimeFlags { get; init; }

	/// <summary>
	/// Indicates what difficulty levels the current step searcher can produce.
	/// </summary>
	public DifficultyLevel DifficultyLevels
		=> (from t in SupportedTechniques select t.DifficultyLevel).Aggregate(@delegate.EnumFlagMerger);

	/// <summary>
	/// Indicates the supported sudoku types.
	/// </summary>
	public SudokuType SupportedSudokuTypes { get; init; } = SudokuType.Standard | SudokuType.Sukaku | SudokuType.JustOneCell;

	/// <summary>
	/// <inheritdoc cref="StepSearcherAttribute" path="/param[@name='techniques']"/>
	/// </summary>
	public TechniqueSet SupportedTechniques { get; } = [.. techniques];
}
