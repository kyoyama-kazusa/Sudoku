namespace Sudoku.Analytics.Steps.LockedSets;

/// <summary>
/// Provides with a step that is a <b>3-dimensional Sue de Coq</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="rowDigitsMask"><inheritdoc cref="RowDigitsMask" path="/summary"/></param>
/// <param name="columnDigitsMask"><inheritdoc cref="ColumnDigitsMask" path="/summary"/></param>
/// <param name="blockDigitsMask"><inheritdoc cref="BlockDigitsMask" path="/summary"/></param>
/// <param name="rowCells"><inheritdoc cref="RowCells" path="/summary"/></param>
/// <param name="columnCells"><inheritdoc cref="ColumnCells" path="/summary"/></param>
/// <param name="blockCells"><inheritdoc cref="BlockCells" path="/summary"/></param>
public sealed class SueDeCoq3DimensionStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask rowDigitsMask,
	Mask columnDigitsMask,
	Mask blockDigitsMask,
	in CellMap rowCells,
	in CellMap columnCells,
	in CellMap blockCells
) : LockedSetStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 55;

	/// <inheritdoc/>
	public override Technique Code => Technique.SueDeCoq3Dimension;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)((Mask)(RowDigitsMask | ColumnDigitsMask) | BlockDigitsMask);

	/// <summary>
	/// Indicates the digits mask that describes which digits are used in this pattern in a row.
	/// </summary>
	public Mask RowDigitsMask { get; } = rowDigitsMask;

	/// <summary>
	/// Indicates the digits mask that describes which digits are used in this pattern in a column.
	/// </summary>
	public Mask ColumnDigitsMask { get; } = columnDigitsMask;

	/// <summary>
	/// Indicates the digits mask that describes which digits are used in this pattern in a block.
	/// </summary>
	public Mask BlockDigitsMask { get; } = blockDigitsMask;

	/// <summary>
	/// Indicates the cells used in this pattern in a row.
	/// </summary>
	public CellMap RowCells { get; } = rowCells;

	/// <summary>
	/// Indicates the cells used in this pattern in a column.
	/// </summary>
	public CellMap ColumnCells { get; } = columnCells;

	/// <summary>
	/// Indicates the cells used in this pattern in a block.
	/// </summary>
	public CellMap BlockCells { get; } = blockCells;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [Cells1Str, Digits1Str, Cells2Str, Digits2Str, Cells3Str, Digits3Str]),
			new(SR.ChineseLanguage, [Cells1Str, Digits1Str, Cells2Str, Digits2Str, Cells3Str, Digits3Str])
		];

	private string Cells1Str => Options.Converter.CellConverter(RowCells);

	private string Digits1Str => Options.Converter.DigitConverter(RowDigitsMask);

	private string Cells2Str => Options.Converter.CellConverter(ColumnCells);

	private string Digits2Str => Options.Converter.DigitConverter(ColumnDigitsMask);

	private string Cells3Str => Options.Converter.CellConverter(BlockCells);

	private string Digits3Str => Options.Converter.DigitConverter(BlockDigitsMask);
}
