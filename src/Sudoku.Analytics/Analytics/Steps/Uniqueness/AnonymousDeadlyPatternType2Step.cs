namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Anonymous Deadly Pattern Type 2</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="patternCandidates"><inheritdoc cref="PatternCandidates" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="TargetCells" path="/summary"/></param>
/// <param name="extraDigit"><inheritdoc cref="ExtraDigit" path="/summary"/></param>
/// <param name="technique"><inheritdoc cref="Step.Code" path="/summary"/></param>
public sealed class AnonymousDeadlyPatternType2Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CandidateMap patternCandidates,
	in CellMap targetCells,
	Digit extraDigit,
	Technique technique
) : AnonymousDeadlyPatternStep(conclusions, views, options, patternCandidates.Digits, patternCandidates.Cells, technique)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override int Type => 2;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << ExtraDigit));

	/// <inheritdoc cref="AnonymousDeadlyPatternType1Step.PatternCandidates"/>
	public CandidateMap PatternCandidates { get; } = patternCandidates;

	/// <summary>
	/// Indicates the target cells.
	/// </summary>
	public CellMap TargetCells { get; } = targetCells;

	/// <summary>
	/// Indicates the extra digit.
	/// </summary>
	public Digit ExtraDigit { get; } = extraDigit;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, ExtraDigitStr]),
			new(SR.ChineseLanguage, [DigitsStr, CellsStr, ExtraDigitStr])
		];

	private string ExtraDigitStr => Options.Converter.DigitConverter((Mask)(1 << ExtraDigit));
}
