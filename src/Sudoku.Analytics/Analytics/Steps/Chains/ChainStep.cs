namespace Sudoku.Analytics.Steps.Chains;

/// <summary>
/// Provides with a step that is a <b>Chain</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public abstract class ChainStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	FullPencilmarkingStep(conclusions, views, options)
{
	/// <summary>
	/// Indicates whether the chain pattern consists of multiple sub-chains.
	/// </summary>
	public abstract bool IsMultiple { get; }

	/// <summary>
	/// Indicates whether the chain pattern is dynamic, which means it should be checked dynamically inside searching algorithm;
	/// also, dynamic chains always contain internal branches.
	/// </summary>
	public abstract bool IsDynamic { get; }

	/// <summary>
	/// Indicates whether the chain is specialized.
	/// In other words, the pattern may not be checked as a chain or forcing chains pattern.
	/// </summary>
	public abstract bool IsSpecialized { get; }

	/// <summary>
	/// Indicates the length of the whole chain pattern, i.e. the number of links used in a pattern.
	/// </summary>
	public abstract int Complexity { get; }
}
