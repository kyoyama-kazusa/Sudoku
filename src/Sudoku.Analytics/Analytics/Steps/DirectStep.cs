namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a solving step that doesn't use any candidates.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public abstract class DirectStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	Step(conclusions, views, options)
{
	/// <inheritdoc/>
	public override PencilmarkVisibility PencilmarkType => PencilmarkVisibility.Direct;
}
