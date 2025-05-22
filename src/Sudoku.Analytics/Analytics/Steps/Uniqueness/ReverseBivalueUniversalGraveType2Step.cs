namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Reverse Bi-value Universal Grave Type 2</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit2" path="/summary"/></param>
/// <param name="extraDigit"><inheritdoc cref="ExtraDigit" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="ReverseBivalueUniversalGraveStep.CompletePattern" path="/summary"/></param>
/// <param name="emptyCells"><inheritdoc cref="ReverseBivalueUniversalGraveStep.EmptyCells" path="/summary"/></param>
public sealed class ReverseBivalueUniversalGraveType2Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	Digit extraDigit,
	in CellMap pattern,
	in CellMap emptyCells
) : ReverseBivalueUniversalGraveStep(conclusions, views, options, digit1, digit2, pattern, emptyCells)
{
	/// <inheritdoc/>
	public override int Type => 2;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << ExtraDigit));

	/// <summary>
	/// Indicates the extra digit used.
	/// </summary>
	public Digit ExtraDigit { get; } = extraDigit;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [ExtraDigitStr]), new(SR.ChineseLanguage, [ExtraDigitStr])];

	/// <inheritdoc/>
	private string ExtraDigitStr => Options.Converter.DigitConverter((Mask)(1 << ExtraDigit));
}
