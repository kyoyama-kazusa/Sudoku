namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Burred Subset</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
/// <param name="extraCells"><inheritdoc cref="ExtraCells" path="/summary"/></param>
/// <param name="subsetIncludedCorner"><inheritdoc cref="SubsetIncludedCorner" path="/summary"/></param>
/// <param name="extraDigitsMask"><inheritdoc cref="ExtraDigitsMask" path="/summary"/></param>
public sealed class UniqueRectangleBurredSubsetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	int absoluteOffset,
	in CellMap extraCells,
	Cell subsetIncludedCorner,
	Mask extraDigitsMask
) :
	UniqueRectangleBurredStep(
		conclusions,
		views,
		options,
		Technique.UniqueRectangleBurredSubset,
		digit1,
		digit2,
		cells,
		false,
		absoluteOffset
	),
	IPatternType3StepTrait<UniqueRectangleBurredSubsetStep>
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty - 1;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | ExtraDigitsMask);

	/// <summary>
	/// Indicates the extra cells used.
	/// </summary>
	public CellMap ExtraCells { get; } = extraCells;

	/// <summary>
	/// Indicates the subset-included corner cell.
	/// </summary>
	public Cell SubsetIncludedCorner { get; } = subsetIncludedCorner;

	/// <summary>
	/// Indicates the extra digits used.
	/// </summary>
	public Mask ExtraDigitsMask { get; } = extraDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [CellsStr, DigitsStr, ExtraCellsStr, ExtraDigitsStr]),
			new(SR.ChineseLanguage, [CellsStr, DigitsStr, ExtraCellsStr, ExtraDigitsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_UniqueRectangleBurredSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			)
		];

	/// <inheritdoc/>
	bool IPatternType3StepTrait<UniqueRectangleBurredSubsetStep>.IsHidden => false;

	/// <inheritdoc/>
	int IPatternType3StepTrait<UniqueRectangleBurredSubsetStep>.SubsetSize => BitOperations.PopCount(ExtraDigitsMask);

	/// <inheritdoc/>
	Mask IPatternType3StepTrait<UniqueRectangleBurredSubsetStep>.SubsetDigitsMask => ExtraDigitsMask;

	/// <inheritdoc/>
	CellMap IPatternType3StepTrait<UniqueRectangleBurredSubsetStep>.SubsetCells => ExtraCells;

	private string ExtraCellsStr => Options.Converter.CellConverter(ExtraCells + SubsetIncludedCorner);

	private string ExtraDigitsStr => Options.Converter.DigitConverter(ExtraDigitsMask);
}
