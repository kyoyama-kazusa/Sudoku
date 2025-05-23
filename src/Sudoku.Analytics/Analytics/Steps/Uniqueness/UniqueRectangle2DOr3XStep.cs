namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle 2D (or 3X)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="code"><inheritdoc cref="Step.Code" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="isAvoidable"><inheritdoc cref="UniqueRectangleStep.IsAvoidable" path="/summary"/></param>
/// <param name="xDigit"><inheritdoc cref="XDigit" path="/summary"/></param>
/// <param name="yDigit"><inheritdoc cref="YDigit" path="/summary"/></param>
/// <param name="xyCell"><inheritdoc cref="XyCell" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
public sealed class UniqueRectangle2DOr3XStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Technique code,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	Digit xDigit,
	Digit yDigit,
	Cell xyCell,
	int absoluteOffset
) : UniqueRectangleStep(conclusions, views, options, code, digit1, digit2, cells, isAvoidable, absoluteOffset)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << XDigit | 1 << YDigit));

	/// <summary>
	/// Indicates the digit X defined in this pattern.
	/// </summary>
	public Digit XDigit { get; } = xDigit;

	/// <summary>
	/// Indicates the digit Y defined in this pattern.
	/// </summary>
	public Digit YDigit { get; } = yDigit;

	/// <summary>
	/// Indicates a bi-value cell that only contains digit X and Y.
	/// </summary>
	public Cell XyCell { get; } = xyCell;

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
