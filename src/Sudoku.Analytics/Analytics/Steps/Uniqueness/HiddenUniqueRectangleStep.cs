namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Hidden Unique Rectangle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="conjugatePairs"><inheritdoc/></param>
/// <param name="absoluteOffset"><inheritdoc/></param>
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
