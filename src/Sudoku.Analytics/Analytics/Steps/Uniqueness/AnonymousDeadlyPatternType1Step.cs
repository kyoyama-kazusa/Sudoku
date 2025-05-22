namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Anonymous Deadly Pattern Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="patternCandidates"><inheritdoc/></param>
/// <param name="targetCell">Indicates the target cell used.</param>
/// <param name="targetDigitsMask">
/// Indicates the target digits that the pattern will be formed if the target cell only holds such digits.
/// </param>
/// <param name="technique"><inheritdoc/></param>
public sealed partial class AnonymousDeadlyPatternType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CandidateMap patternCandidates,
	[Property] Cell targetCell,
	[Property] Mask targetDigitsMask,
	Technique technique
) : AnonymousDeadlyPatternStep(conclusions, views, options, patternCandidates.Digits, patternCandidates.Cells, technique)
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitsStr, CellsStr]), new(SR.ChineseLanguage, [DigitsStr, CellsStr])];
}
