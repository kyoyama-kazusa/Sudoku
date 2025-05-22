namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Deadly Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public abstract class DeadlyPatternStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	FullPencilmarkingStep(conclusions, views, options)
{
	/// <summary>
	/// Indicates whether the deadly pattern only uses bi-value cells.
	/// </summary>
	public abstract bool OnlyUseBivalueCells { get; }

	/// <summary>
	/// Indicates whether the pattern is unconditional.
	/// </summary>
	public abstract bool? IsUnconditional { get; }
}
