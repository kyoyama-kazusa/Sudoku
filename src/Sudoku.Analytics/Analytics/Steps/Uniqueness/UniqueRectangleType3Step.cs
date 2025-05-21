namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="extraCells">Indicates the extra cells used, forming the subset.</param>
/// <param name="extraDigitsMask">Indicates the mask that contains all extra digits used.</param>
/// <param name="house">Indicates the house used.</param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="absoluteOffset"><inheritdoc/></param>
/// <param name="isNaked">
/// Indicates whether the subset is naked subset. If <see langword="true"/>, a naked subset; otherwise, a hidden subset.
/// </param>
public sealed partial class UniqueRectangleType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	[Property] in CellMap extraCells,
	[Property] Mask extraDigitsMask,
	[Property] House house,
	bool isAvoidable,
	int absoluteOffset,
	[Property] bool isNaked = true
) :
	UniqueRectangleStep(
		conclusions,
		views,
		options,
		isAvoidable ? Technique.AvoidableRectangleType3 : Technique.UniqueRectangleType3,
		digit1,
		digit2,
		cells,
		isAvoidable,
		absoluteOffset
	),
	IPatternType3StepTrait<UniqueRectangleType3Step>
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | ExtraDigitsMask);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(
				SR.EnglishLanguage,
				[D1Str, D2Str, CellsStr, SubsetDigitsMask, OnlyKeywordEnUs, CellsStr, HouseStr]
			),
			new(
				SR.ChineseLanguage,
				[D1Str, D2Str, CellsStr, SubsetDigitsMask, OnlyKeywordZhCn, HouseStr, CellsStr, AppearLimitKeywordZhCn]
			)
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_UniqueRectangleSubsetIsHiddenFactor",
				[nameof(IPatternType3StepTrait<>.IsHidden)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			),
			Factor.Create(
				"Factor_UniqueRectangleSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			)
		];

	/// <inheritdoc/>
	bool IPatternType3StepTrait<UniqueRectangleType3Step>.IsHidden => !IsNaked;

	/// <inheritdoc/>
	int IPatternType3StepTrait<UniqueRectangleType3Step>.SubsetSize => BitOperations.PopCount(ExtraDigitsMask);

	/// <inheritdoc/>
	Mask IPatternType3StepTrait<UniqueRectangleType3Step>.SubsetDigitsMask => ExtraDigitsMask;

	/// <inheritdoc/>
	CellMap IPatternType3StepTrait<UniqueRectangleType3Step>.SubsetCells => ExtraCells;

	private string SubsetDigitsMask => Options.Converter.DigitConverter(ExtraDigitsMask);

	private string OnlyKeywordEnUs => IsNaked ? string.Empty : "only ";

	private string OnlyKeywordZhCn => IsNaked ? string.Empty : SR.Get("Only", new(SR.ChineseLanguage));

	private string HouseStr => Options.Converter.HouseConverter(1 << House);

	private string AppearLimitKeywordZhCn => SR.Get("Appear", new(SR.ChineseLanguage));
}
