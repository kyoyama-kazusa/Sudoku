namespace Sudoku.Analytics.Steps.Chains;

/// <summary>
/// Provides with a step that is a <b>Pattern-Based Chain</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pattern">Indicates the pattern to be used.</param>
public abstract partial class PatternBasedChainStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] IChainOrForcingChains pattern
) : ChainStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public sealed override bool IsSpecialized => false;
}
