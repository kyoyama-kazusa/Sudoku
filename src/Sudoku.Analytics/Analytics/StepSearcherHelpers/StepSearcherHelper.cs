namespace Sudoku.Analytics.StepSearcherHelpers;

/// <summary>
/// Represents a type that creates some members helping for target <see cref="StepSearcher"/> instances.
/// </summary>
/// <seealso cref="StepSearcher"/>
internal closed class StepSearcherHelper
{
	/// <summary>
	/// Indicates supported step searcher types.
	/// </summary>
	public abstract ReadOnlyMemory<Type> SupportedStepSearcherTypes { get; }
}
