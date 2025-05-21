namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc/></param>
/// <param name="houses"><inheritdoc/></param>
/// <param name="corner1"><inheritdoc/></param>
/// <param name="corner2"><inheritdoc/></param>
/// <param name="targetCell">Indicates the target cell.</param>
/// <param name="targetDigits">Indicates the target digits.</param>
public sealed partial class QiuDeadlyPatternType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool is2LinesWith2Cells,
	HouseMask houses,
	Cell? corner1,
	Cell? corner2,
	[Property] Cell targetCell,
	[Property] Mask targetDigits
) : QiuDeadlyPatternStep(conclusions, views, options, is2LinesWith2Cells, houses, corner1, corner2)
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <inheritdoc/>
	public override Mask DigitsUsed => TargetDigits;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [PatternStr, TargetDigitsStr, CandidatesStr]), new(SR.ChineseLanguage, [PatternStr, CandidatesStr, TargetDigitsStr])];

	private string CandidatesStr
		=> Options.Converter.CandidateConverter([.. from digit in TargetDigits select TargetCell * 9 + digit]);

	private string TargetDigitsStr => Options.Converter.DigitConverter(TargetDigits);
}
