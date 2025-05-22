namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern External Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc cref="QiuDeadlyPatternStep.Is2LinesWith2Cells" path="/summary"/></param>
/// <param name="houses"><inheritdoc cref="QiuDeadlyPatternStep.Houses" path="/summary"/></param>
/// <param name="corner1"><inheritdoc cref="QiuDeadlyPatternStep.Corner1" path="/summary"/></param>
/// <param name="corner2"><inheritdoc cref="QiuDeadlyPatternStep.Corner2" path="/summary"/></param>
/// <param name="targetCell"><inheritdoc cref="TargetCell" path="/summary"/></param>
/// <param name="targetDigits"><inheritdoc cref="TargetDigits" path="/summary"/></param>
public sealed class QiuDeadlyPatternExternalType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool is2LinesWith2Cells,
	HouseMask houses,
	Cell? corner1,
	Cell? corner2,
	Cell targetCell,
	Mask targetDigits
) : QiuDeadlyPatternExternalTypeStep(conclusions, views, options, is2LinesWith2Cells, houses, corner1, corner2)
{
	/// <inheritdoc/>
	public override int Type => 6;

	/// <inheritdoc/>
	public override Mask DigitsUsed => TargetDigits;

	/// <summary>
	/// Indicates the target cell.
	/// </summary>
	public Cell TargetCell { get; } = targetCell;

	/// <summary>
	/// Indicates the target digits.
	/// </summary>
	public Mask TargetDigits { get; } = targetDigits;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [PatternStr, DigitsStr, CellStr]), new(SR.ChineseLanguage, [PatternStr, CellStr, DigitsStr])];

	private string CellStr => Options.Converter.CellConverter(in TargetCell.AsCellMap());

	private string DigitsStr => Options.Converter.DigitConverter(TargetDigits);
}
