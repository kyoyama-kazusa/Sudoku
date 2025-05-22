namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="loop"><inheritdoc/></param>
/// <param name="subsetCells">Indicates the cells that are subset cells.</param>
/// <param name="subsetDigitsMask">Indicates the mask that contains the subset digits used in this instance.</param>
/// <param name="loopPath"><inheritdoc/></param>
public sealed partial class UniqueLoopType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap loop,
	[Property] in CellMap subsetCells,
	[Property] Mask subsetDigitsMask,
	Cell[] loopPath
) : UniqueLoopStep(conclusions, views, options, digit1, digit2, loop, loopPath), IPatternType3StepTrait<UniqueLoopType3Step>
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | SubsetDigitsMask);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [Digit1Str, Digit2Str, LoopStr, SubsetName, DigitsStr, SubsetCellsStr]),
			new(SR.ChineseLanguage, [Digit1Str, Digit2Str, LoopStr, SubsetName, DigitsStr, SubsetCellsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			.. base.Factors,
			Factor.Create(
				"Factor_UniqueLoopSubsetSizeFactor",
				[nameof(IPatternType3StepTrait<>.SubsetSize)],
				GetType(),
				static args => (int)args![0]!
			)
		];

	/// <inheritdoc/>
	bool IPatternType3StepTrait<UniqueLoopType3Step>.IsHidden => false;

	/// <inheritdoc/>
	int IPatternType3StepTrait<UniqueLoopType3Step>.SubsetSize => BitOperations.PopCount(SubsetDigitsMask);

	private string SubsetCellsStr => Options.Converter.CellConverter(SubsetCells);

	private string DigitsStr => Options.Converter.DigitConverter(SubsetDigitsMask);

	private string SubsetName => TechniqueNaming.Subset.GetSubsetName(BitOperations.PopCount(SubsetDigitsMask));
}
