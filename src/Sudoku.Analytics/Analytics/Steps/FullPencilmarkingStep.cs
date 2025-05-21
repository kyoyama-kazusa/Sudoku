namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a solving step that cannot be found by using given and modifiable digits only.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public abstract class FullPencilmarkingStep(ReadOnlyMemory<Conclusion> conclusions, View[]? views, StepGathererOptions options) :
	Step(conclusions, views, options)
{
	/// <inheritdoc/>
	public sealed override PencilmarkVisibility PencilmarkType => PencilmarkVisibility.FullMarking;
}
