namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Extended Rectangle Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="ExtendedRectangleStep.Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExtendedRectangleStep.DigitsMask" path="/summary"/></param>
/// <param name="subsetCells"><inheritdoc cref="SubsetCells" path="/summary"/></param>
/// <param name="subsetDigitsMask"><inheritdoc cref="SubsetDigitsMask" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="isCannibalism"><inheritdoc cref="IsCannibalism" path="/summary"/></param>
public sealed class ExtendedRectangleType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	in CellMap subsetCells,
	Mask subsetDigitsMask,
	House house,
	bool isCannibalism
) :
	ExtendedRectangleStep(conclusions, views, options, cells, digitsMask),
	IPatternType3StepTrait<ExtendedRectangleType3Step>
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Technique Code => IsCannibalism ? Technique.ExtendedRectangleType3Cannibalism : Technique.ExtendedRectangleType3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | SubsetDigitsMask);

	/// <summary>
	/// Indicates the extra cells used that can form the subset.
	/// </summary>
	public CellMap SubsetCells { get; } = subsetCells;

	/// <summary>
	/// Indicates the subset digits used.
	/// </summary>
	public Mask SubsetDigitsMask { get; } = subsetDigitsMask;

	/// <summary>
	/// Indicates the house that subset formed.
	/// </summary>
	public int House { get; } = house;

	/// <summary>
	/// Indicates whether the pattern is cannibalism.
	/// </summary>
	public bool IsCannibalism { get; } = isCannibalism;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsStr, CellsStr, ExtraDigitsStr, ExtraCellsStr, HouseStr]),
			new(SR.ChineseLanguage, [DigitsStr, CellsStr, HouseStr, ExtraCellsStr, ExtraDigitsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			.. base.Factors,
			Factor.Create(
				"Factor_ExtendedRectangleSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			),
			Factor.Create(
				"Factor_ExtendedRectangleCannibalismFactor",
				[nameof(IsCannibalism)],
				GetType(),
				static args => (bool)args![0]! ? 2 : 0
			)
		];

	/// <inheritdoc/>
	bool IPatternType3StepTrait<ExtendedRectangleType3Step>.IsHidden => false;

	/// <inheritdoc/>
	int IPatternType3StepTrait<ExtendedRectangleType3Step>.SubsetSize => BitOperations.PopCount(SubsetDigitsMask);

	private string ExtraDigitsStr => Options.Converter.DigitConverter(SubsetDigitsMask);

	private string ExtraCellsStr => Options.Converter.CellConverter(SubsetCells);

	private string HouseStr => Options.Converter.HouseConverter(1 << House);
}
