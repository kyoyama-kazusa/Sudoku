namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Anonymous Deadly Pattern Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="patternCandidates"><inheritdoc cref="PatternCandidates" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="TargetCells" path="/summary"/></param>
/// <param name="subsetCells"><inheritdoc cref="SubsetCells" path="/summary"/></param>
/// <param name="subsetDigitsMask"><inheritdoc cref="SubsetDigitsMask" path="/summary"/></param>
/// <param name="technique"><inheritdoc cref="Step.Code" path="/summary"/></param>
public sealed class AnonymousDeadlyPatternType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CandidateMap patternCandidates,
	in CellMap targetCells,
	in CellMap subsetCells,
	Mask subsetDigitsMask,
	Technique technique
) :
	AnonymousDeadlyPatternStep(conclusions, views, options, patternCandidates.Digits, patternCandidates.Cells, technique),
	IPatternType3StepTrait<AnonymousDeadlyPatternType3Step>
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc cref="AnonymousDeadlyPatternType1Step.PatternCandidates"/>
	public CandidateMap PatternCandidates { get; } = patternCandidates;

	/// <summary>
	/// Indicates the target cells.
	/// </summary>
	public CellMap TargetCells { get; } = targetCells;

	/// <summary>
	/// Indicates the subset cells.
	/// </summary>
	public CellMap SubsetCells { get; } = subsetCells;

	/// <summary>
	/// Indicates the extra digits used.
	/// </summary>
	public Mask SubsetDigitsMask { get; } = subsetDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, ExtraDigitsStr, ExtraCellsStr]),
			new(SR.ChineseLanguage, [DigitsStr, CellsStr, ExtraCellsStr, ExtraDigitsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_AnonymousDeadlyPatternSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			)
		];

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | SubsetDigitsMask);

	/// <inheritdoc/>
	bool IPatternType3StepTrait<AnonymousDeadlyPatternType3Step>.IsHidden => false;

	/// <inheritdoc/>
	int IPatternType3StepTrait<AnonymousDeadlyPatternType3Step>.SubsetSize => BitOperations.PopCount(SubsetDigitsMask);

	private string ExtraDigitsStr => Options.Converter.DigitConverter(SubsetDigitsMask);

	private string ExtraCellsStr => Options.Converter.CellConverter(SubsetCells);
}
