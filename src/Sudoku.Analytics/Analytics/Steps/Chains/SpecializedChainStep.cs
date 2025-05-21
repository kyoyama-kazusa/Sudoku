namespace Sudoku.Analytics.Steps.Chains;

/// <summary>
/// Provides with a step that is a <b>Specialized Chain</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public abstract class SpecializedChainStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	ChainStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public sealed override bool IsSpecialized => true;
}
