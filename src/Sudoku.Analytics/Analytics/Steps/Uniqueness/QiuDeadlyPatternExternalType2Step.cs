namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern External Type 2</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc cref="QiuDeadlyPatternStep.Is2LinesWith2Cells" path="/summary"/></param>
/// <param name="houses"><inheritdoc cref="QiuDeadlyPatternStep.Houses" path="/summary"/></param>
/// <param name="corner1"><inheritdoc cref="QiuDeadlyPatternStep.Corner1" path="/summary"/></param>
/// <param name="corner2"><inheritdoc cref="QiuDeadlyPatternStep.Corner2" path="/summary"/></param>
/// <param name="mirrorCells"><inheritdoc cref="MirrorCells" path="/summary"/></param>
/// <param name="targetDigit"><inheritdoc cref="TargetDigit" path="/summary"/></param>
public sealed class QiuDeadlyPatternExternalType2Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool is2LinesWith2Cells,
	HouseMask houses,
	Cell? corner1,
	Cell? corner2,
	in CellMap mirrorCells,
	Digit targetDigit
) : QiuDeadlyPatternExternalTypeStep(conclusions, views, options, is2LinesWith2Cells, houses, corner1, corner2)
{
	/// <inheritdoc/>
	public override int Type => 7;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << TargetDigit);

	/// <summary>
	/// Indicates the mirror cells.
	/// </summary>
	public CellMap MirrorCells { get; } = mirrorCells;

	/// <summary>
	/// Indicates the target digit.
	/// </summary>
	public Digit TargetDigit { get; } = targetDigit;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [PatternStr, DigitStr, CellsStr]), new(SR.ChineseLanguage, [PatternStr, CellsStr, DigitStr])];

	private string CellsStr => Options.Converter.CellConverter(MirrorCells);

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << TargetDigit));
}
