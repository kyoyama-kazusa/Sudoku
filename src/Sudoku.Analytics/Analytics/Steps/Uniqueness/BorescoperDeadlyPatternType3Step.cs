namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Borescoper's Deadly Pattern Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="BorescoperDeadlyPatternStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="BorescoperDeadlyPatternStep.DigitsMask" path="/summary"/></param>
/// <param name="subsetCells"><inheritdoc cref="SubsetCells" path="/summary"/></param>
/// <param name="subsetDigitsMask"><inheritdoc cref="SubsetDigitsMask" path="/summary"/></param>
public sealed class BorescoperDeadlyPatternType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	in CellMap subsetCells,
	Mask subsetDigitsMask
) :
	BorescoperDeadlyPatternStep(conclusions, views, options, cells, digitsMask),
	IPatternType3StepTrait<BorescoperDeadlyPatternType3Step>
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | SubsetDigitsMask);

	/// <summary>
	/// Indicates the cells that the subset used.
	/// </summary>
	public CellMap SubsetCells { get; } = subsetCells;

	/// <summary>
	/// Indicates the mask of subset digits used.
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
				"Factor_BorescoperDeadlyPatternSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			)
		];

	/// <inheritdoc/>
	bool IPatternType3StepTrait<BorescoperDeadlyPatternType3Step>.IsHidden => false;

	/// <inheritdoc/>
	int IPatternType3StepTrait<BorescoperDeadlyPatternType3Step>.SubsetSize => BitOperations.PopCount(SubsetDigitsMask);

	private string ExtraDigitsStr => Options.Converter.DigitConverter(SubsetDigitsMask);

	private string ExtraCellsStr => Options.Converter.CellConverter(SubsetCells);
}
