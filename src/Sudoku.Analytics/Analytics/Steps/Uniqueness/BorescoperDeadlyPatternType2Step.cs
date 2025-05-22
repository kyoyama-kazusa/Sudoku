namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Borescoper's Deadly Pattern Type 2</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="BorescoperDeadlyPatternStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="BorescoperDeadlyPatternStep.DigitsMask" path="/summary"/></param>
/// <param name="extraDigit"><inheritdoc cref="ExtraDigit" path="/summary"/></param>
public sealed class BorescoperDeadlyPatternType2Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	Digit extraDigit
) : BorescoperDeadlyPatternStep(conclusions, views, options, cells, digitsMask)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override int Type => 2;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << ExtraDigit));

	/// <summary>
	/// Indicates the extra digit used.
	/// </summary>
	public Digit ExtraDigit { get; } = extraDigit;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, ExtraDigitStr]),
			new(SR.ChineseLanguage, [DigitsStr, CellsStr, ExtraDigitStr])
		];

	private string ExtraDigitStr => Options.Converter.DigitConverter((Mask)(1 << ExtraDigit));
}
