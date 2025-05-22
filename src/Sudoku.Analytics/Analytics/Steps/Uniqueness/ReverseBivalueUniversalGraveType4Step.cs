namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Reverse Bi-value Universal Grave Type 4</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit2" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="ReverseBivalueUniversalGraveStep.CompletePattern" path="/summary"/></param>
/// <param name="emptyCells"><inheritdoc cref="ReverseBivalueUniversalGraveStep.EmptyCells" path="/summary"/></param>
/// <param name="conjugatePair"><inheritdoc cref="ConjugatePair" path="/summary"/></param>
public sealed class ReverseBivalueUniversalGraveType4Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap pattern,
	in CellMap emptyCells,
	in Conjugate conjugatePair
) : ReverseBivalueUniversalGraveStep(conclusions, views, options, digit1, digit2, pattern, emptyCells)
{
	/// <inheritdoc/>
	public override int Type => 4;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << ConjugatePair.Digit));

	/// <summary>
	/// Indicates the conjugate pair used.
	/// </summary>
	public Conjugate ConjugatePair { get; } = conjugatePair;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [ConjugatePairStr]), new(SR.ChineseLanguage, [ConjugatePairStr])];

	/// <inheritdoc/>
	private string ConjugatePairStr => Options.Converter.ConjugateConverter([ConjugatePair]);
}
