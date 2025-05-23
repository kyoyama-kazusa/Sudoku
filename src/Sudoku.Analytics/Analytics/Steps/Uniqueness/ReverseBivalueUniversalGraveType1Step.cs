namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Reverse Bi-value Universal Grave Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit2" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="ReverseBivalueUniversalGraveStep.CompletePattern" path="/summary"/></param>
/// <param name="emptyCells"><inheritdoc cref="ReverseBivalueUniversalGraveStep.EmptyCells" path="/summary"/></param>
public sealed class ReverseBivalueUniversalGraveType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap pattern,
	in CellMap emptyCells
) : ReverseBivalueUniversalGraveStep(conclusions, views, options, digit1, digit2, pattern, emptyCells)
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [Cell1Str, Cell2Str, PatternStr]), new(SR.ChineseLanguage, [PatternStr, Cell1Str, Cell2Str])];
}
