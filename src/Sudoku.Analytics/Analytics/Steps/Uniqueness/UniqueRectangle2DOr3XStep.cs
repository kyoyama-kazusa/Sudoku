namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle 2D (or 3X)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="code"><inheritdoc/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="xDigit">Indicates the digit X defined in this pattern.</param>
/// <param name="yDigit">Indicates the digit Y defined in this pattern.</param>
/// <param name="xyCell">Indicates a bi-value cell that only contains digit X and Y.</param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public sealed partial class UniqueRectangle2DOr3XStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Technique code,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	[Property] Digit xDigit,
	[Property] Digit yDigit,
	[Property] Cell xyCell,
	int absoluteOffset
) : UniqueRectangleStep(conclusions, views, options, code, digit1, digit2,  cells, isAvoidable, absoluteOffset)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << XDigit | 1 << YDigit));

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, XDigitStr, YDigitStr, XYCellsStr]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, XDigitStr, YDigitStr, XYCellsStr])
		];

	private string XDigitStr => Options.Converter.DigitConverter((Mask)(1 << XDigit));

	private string YDigitStr => Options.Converter.DigitConverter((Mask)(1 << YDigit));

	private string XYCellsStr => Options.Converter.CellConverter(in XyCell.AsCellMap());
}
