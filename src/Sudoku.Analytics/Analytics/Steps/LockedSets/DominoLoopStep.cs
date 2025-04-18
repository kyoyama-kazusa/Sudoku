namespace Sudoku.Analytics.Steps.LockedSets;

/// <summary>
/// Provides with a step that is a <b>Domino Loop</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="cells">Indicates the cells used.</param>
/// <param name="digitsMask">Indicates the digits used.</param>
public sealed partial class DominoLoopStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CellMap cells,
	[Property] Mask digitsMask
) : LockedSetStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 96;

	/// <inheritdoc/>
	public override Technique Code => Technique.DominoLoop;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [CellsCountStr, CellsStr]), new(SR.ChineseLanguage, [CellsCountStr, CellsStr])];

	private string CellsCountStr => Cells.Count.ToString();

	private string CellsStr => Options.Converter.CellConverter(Cells);
}
