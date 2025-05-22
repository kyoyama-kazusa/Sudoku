namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Chromatic Pattern XZ</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="blocks"><inheritdoc cref="ChromaticPatternStep.Blocks" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="ChromaticPatternStep.Pattern" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="extraCell"><inheritdoc cref="ExtraCell" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ChromaticPatternStep.DigitsMask" path="/summary"/></param>
/// <param name="extraDigitsMask"><inheritdoc cref="ExtraDigitsMask" path="/summary"/></param>
public sealed class ChromaticPatternXzStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House[] blocks,
	in CellMap pattern,
	in CellMap cells,
	Cell extraCell,
	Mask digitsMask,
	Mask extraDigitsMask
) : ChromaticPatternStep(conclusions, views, options, blocks, pattern, digitsMask)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override Technique Code => Technique.ChromaticPatternXzRule;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(DigitsMask | ExtraDigitsMask);

	/// <summary>
	/// Indicates the cells that contains extra digit.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the extra cell used.
	/// </summary>
	public Cell ExtraCell { get; } = extraCell;

	/// <summary>
	/// Indicates the mask of extra digits.
	/// </summary>
	public Mask ExtraDigitsMask { get; } = extraDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, BlocksStr, ExtraCellStr]),
			new(SR.ChineseLanguage, [BlocksStr, CellsStr, DigitsStr, ExtraCellStr])
		];

	private string ExtraCellStr => Options.Converter.CellConverter(in ExtraCell.AsCellMap());
}
