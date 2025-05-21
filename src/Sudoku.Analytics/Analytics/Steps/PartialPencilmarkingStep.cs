namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a solving step that can be found without any candidates, except few of candidates marked.
/// Logically, the technique can be treated as a single step with locked candidates or subset patterns.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="SingleStep.Subtype" path="/summary"/></param>
public abstract class PartialPencilmarkingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	SingleSubtype subtype
) : SingleStep(conclusions, views, options, cell, digit, subtype)
{
	/// <inheritdoc/>
	public sealed override PencilmarkVisibility PencilmarkType => PencilmarkVisibility.PartialMarking;
}
