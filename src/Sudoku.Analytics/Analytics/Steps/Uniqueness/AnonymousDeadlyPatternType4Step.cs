namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Anonymous Deadly Pattern Type 4</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="patternCandidates"><inheritdoc cref="PatternCandidates" path="/summary"/></param>
/// <param name="conjugateHouse"><inheritdoc cref="ConjugateHouse" path="/summary"/></param>
/// <param name="extraDigitsMask"><inheritdoc cref="ExtraDigitsMask" path="/summary"/></param>
/// <param name="technique"><inheritdoc cref="Step.Code" path="/summary"/></param>
public sealed class AnonymousDeadlyPatternType4Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CandidateMap patternCandidates,
	House conjugateHouse,
	Mask extraDigitsMask,
	Technique technique
) : AnonymousDeadlyPatternStep(conclusions, views, options, patternCandidates.Digits, patternCandidates.Cells, technique)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override int Type => 4;

	/// <inheritdoc cref="AnonymousDeadlyPatternType1Step.PatternCandidates"/>
	public CandidateMap PatternCandidates { get; } = patternCandidates;

	/// <summary>
	/// Indicates the target house.
	/// </summary>
	public House ConjugateHouse { get; } = conjugateHouse;

	/// <summary>
	/// Indicates the extra digits used.
	/// </summary>
	public Mask ExtraDigitsMask { get; } = extraDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, ConjHouseStr, ExtraCombStr]),
			new(SR.ChineseLanguage, [DigitsStr, CellsStr, ExtraCombStr, ConjHouseStr])
		];

	private string ExtraCombStr => Options.Converter.DigitConverter(ExtraDigitsMask);

	private string ConjHouseStr => Options.Converter.HouseConverter(1 << ConjugateHouse);
}
