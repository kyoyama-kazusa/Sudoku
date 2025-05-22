namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop Type 2</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueLoopStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueLoopStep.Digit2" path="/summary"/></param>
/// <param name="loop"><inheritdoc cref="UniqueLoopStep.Loop" path="/summary"/></param>
/// <param name="extraDigit"><inheritdoc cref="ExtraDigit" path="/summary"/></param>
/// <param name="loopPath"><inheritdoc cref="UniqueLoopStep.LoopPath" path="/summary"/></param>
public sealed class UniqueLoopType2Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap loop,
	Digit extraDigit,
	Cell[] loopPath
) : UniqueLoopStep(conclusions, views, options, digit1, digit2, loop, loopPath)
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
		=> [
			new(SR.EnglishLanguage, [Digit1Str, Digit2Str, LoopStr, ExtraDigitStr]),
			new(SR.ChineseLanguage, [Digit1Str, Digit2Str, LoopStr, ExtraDigitStr])
		];

	private string ExtraDigitStr => Options.Converter.DigitConverter((Mask)(1 << ExtraDigit));
}
