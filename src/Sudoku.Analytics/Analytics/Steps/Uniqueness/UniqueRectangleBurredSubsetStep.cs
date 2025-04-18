namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Burred Subset</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="absoluteOffset"><inheritdoc/></param>
/// <param name="extraCells">Indicates the extra cells used.</param>
/// <param name="subsetIncludedCorner">Indicates the subset-included corner cell.</param>
/// <param name="extraDigitsMask">Indicates the extra digits used.</param>
public sealed partial class UniqueRectangleBurredSubsetStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	int absoluteOffset,
	[Property] in CellMap extraCells,
	[Property] Cell subsetIncludedCorner,
	[Property] Mask extraDigitsMask
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
