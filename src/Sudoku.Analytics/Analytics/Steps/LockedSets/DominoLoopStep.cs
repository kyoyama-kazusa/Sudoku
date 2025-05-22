namespace Sudoku.Analytics.Steps.LockedSets;

/// <summary>
/// Provides with a step that is a <b>Domino Loop</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public sealed class DominoLoopStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask
) : LockedSetStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 96;

	/// <inheritdoc/>
	public override Technique Code => Technique.DominoLoop;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [CellsCountStr, CellsStr]), new(SR.ChineseLanguage, [CellsCountStr, CellsStr])];

	private string CellsCountStr => Cells.Count.ToString();

	private string CellsStr => Options.Converter.CellConverter(Cells);
}
