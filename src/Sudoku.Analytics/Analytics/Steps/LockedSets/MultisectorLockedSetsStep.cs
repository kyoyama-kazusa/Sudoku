namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Multi-sector Locked Sets</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="rowsCount"><inheritdoc cref="RowsCount" path="/summary"/></param>
/// <param name="columnsCount"><inheritdoc cref="ColumnsCount" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public sealed class MultisectorLockedSetsStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	int rowsCount,
	int columnsCount,
	Mask digitsMask
) :
	LockedSetStep(conclusions, views, options),
	ICellListTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 94;

	/// <summary>
	/// Indicates the number of rows used.
	/// </summary>
	public int RowsCount { get; } = rowsCount;

	/// <summary>
	/// Indicates the number of columns used.
	/// </summary>
	public int ColumnsCount { get; } = columnsCount;

	/// <inheritdoc/>
	public override Technique Code => Technique.MultisectorLockedSets;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the cells used in this pattern.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [CellsCountStr, CellsStr]), new(SR.ChineseLanguage, [CellsCountStr, CellsStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_MultisectorLockedSetsSizeFactor",
				[nameof(ICellListTrait.CellSize)],
				GetType(),
				static args => OeisSequences.A002024((int)args![0]!)
			)
		];

	/// <inheritdoc/>
	int ICellListTrait.CellSize => Cells.Count;

	private string CellsCountStr => Cells.Count.ToString();

	private string CellsStr => Options.Converter.CellConverter(Cells);
}
