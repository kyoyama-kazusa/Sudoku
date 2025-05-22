namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Anonymous Deadly Pattern Type 1</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="patternCandidates"><inheritdoc cref="PatternCandidates" path="/summary"/></param>
/// <param name="targetCell"><inheritdoc cref="TargetCell" path="/summary"/></param>
/// <param name="targetDigitsMask"><inheritdoc cref="TargetDigitsMask" path="/summary"/></param>
/// <param name="technique"><inheritdoc cref="Step.Code" path="/summary"/></param>
public sealed class AnonymousDeadlyPatternType1Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CandidateMap patternCandidates,
	Cell targetCell,
	Mask targetDigitsMask,
	Technique technique
) : AnonymousDeadlyPatternStep(conclusions, views, options, patternCandidates.Digits, patternCandidates.Cells, technique)
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <summary>
	/// Indicates the candidates used.
	/// </summary>
	public CandidateMap PatternCandidates { get; } = patternCandidates;

	/// <summary>
	/// Indicates the target cell used.
	/// </summary>
	public Cell TargetCell { get; } = targetCell;

	/// <summary>
	/// Indicates the target digits that the pattern will be formed if the target cell only holds such digits.
	/// </summary>
	public Mask TargetDigitsMask { get; } = targetDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitsStr, CellsStr]), new(SR.ChineseLanguage, [DigitsStr, CellsStr])];
}
