namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Bi-value Universal Grave XZ</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="xzCell"><inheritdoc cref="XzCell" path="/summary"/></param>
public sealed class BivalueUniversalGraveXzStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap cells,
	Cell xzCell
) : BivalueUniversalGraveStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override int Type => 5;

	/// <inheritdoc/>
	public override Technique Code => Technique.BivalueUniversalGraveXzRule;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the extra cell used. This cell is a bivalue cell that only contains digit X and Z.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// The generated property declaration for parameter <c>xzCell</c>.
	/// </summary>
	public Cell XzCell { get; } = xzCell;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitStr, CellsStr, ExtraCellStr]), new(SR.ChineseLanguage, [DigitStr, CellsStr, ExtraCellStr])];

	private string DigitStr => Options.Converter.DigitConverter(DigitsMask);

	private string CellsStr => Options.Converter.CellConverter(Cells);

	private string ExtraCellStr => Options.Converter.CellConverter(in XzCell.AsCellMap());
}
