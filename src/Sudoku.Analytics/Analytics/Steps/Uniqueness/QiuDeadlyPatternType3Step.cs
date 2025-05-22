namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc cref="QiuDeadlyPatternStep.Is2LinesWith2Cells" path="/summary"/></param>
/// <param name="houses"><inheritdoc cref="QiuDeadlyPatternStep.Houses" path="/summary"/></param>
/// <param name="corner1"><inheritdoc cref="QiuDeadlyPatternStep.Corner1" path="/summary"/></param>
/// <param name="corner2"><inheritdoc cref="QiuDeadlyPatternStep.Corner2" path="/summary"/></param>
/// <param name="subsetDigitsMask"><inheritdoc cref="SubsetDigitsMask" path="/summary"/></param>
/// <param name="subsetCells"><inheritdoc cref="SubsetCells" path="/summary"/></param>
/// <param name="isNaked"><inheritdoc cref="IsNaked" path="/summary"/></param>
public sealed class QiuDeadlyPatternType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool is2LinesWith2Cells,
	HouseMask houses,
	Cell? corner1,
	Cell? corner2,
	in CellMap subsetCells,
	Mask subsetDigitsMask,
	bool isNaked
) :
	QiuDeadlyPatternStep(conclusions, views, options, is2LinesWith2Cells, houses, corner1, corner2),
	IPatternType3StepTrait<QiuDeadlyPatternType3Step>
{
	/// <summary>
	/// Indicates whether the subset is naked one.
	/// </summary>
	public bool IsNaked { get; } = isNaked;

	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => SubsetDigitsMask;

	/// <summary>
	/// Indicates the mask of subset digits used.
	/// </summary>
	public CellMap SubsetCells { get; } = subsetCells;

	/// <summary>
	/// Indicates the subset cells used.
	/// </summary>
	public Mask SubsetDigitsMask { get; } = subsetDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [PatternStr, DigitsStr, CellsStr, SubsetName]),
			new(SR.ChineseLanguage, [PatternStr, DigitsStr, CellsStr, SubsetName])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_QiuDeadlyPatternSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			)
		];

	/// <inheritdoc/>
	bool IPatternType3StepTrait<QiuDeadlyPatternType3Step>.IsHidden => false;

	/// <inheritdoc/>
	int IPatternType3StepTrait<QiuDeadlyPatternType3Step>.SubsetSize => BitOperations.PopCount(SubsetDigitsMask);

	private string DigitsStr => Options.Converter.DigitConverter(SubsetDigitsMask);

	private string CellsStr => Options.Converter.CellConverter(SubsetCells);

	private string SubsetName => TechniqueNaming.Subset.GetSubsetName(BitOperations.PopCount(SubsetDigitsMask));
}
