namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Hidden Unique Rectangle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="isAvoidable"><inheritdoc cref="UniqueRectangleStep.IsAvoidable" path="/summary"/></param>
/// <param name="conjugatePairs"><inheritdoc cref="UniqueRectangleConjugatePairStep.ConjugatePairs" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
public sealed class HiddenUniqueRectangleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	Conjugate[] conjugatePairs,
	int absoluteOffset
) : UniqueRectangleConjugatePairStep(
	conclusions,
	views,
	options,
	isAvoidable ? Technique.HiddenAvoidableRectangle : Technique.HiddenUniqueRectangle,
	digit1,
	digit2,
	cells,
	isAvoidable,
	conjugatePairs,
	absoluteOffset
)
{
	/// <inheritdoc/>
	protected override bool TechniqueResourceKeyInheritsFromBase => true;
}
