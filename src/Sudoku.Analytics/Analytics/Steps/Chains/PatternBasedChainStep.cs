namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Pattern-Based Chain</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="Pattern" path="/summary"/></param>
public abstract class PatternBasedChainStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	IChainOrForcingChains pattern
) : ChainStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public sealed override bool IsSpecialized => false;

	/// <summary>
	/// Indicates the pattern to be used.
	/// </summary>
	public IChainOrForcingChains Pattern { get; } = pattern;
}
