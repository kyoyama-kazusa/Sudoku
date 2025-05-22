namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Junior Exocet (Mirror Almost Hidden Set)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
/// <param name="extraCells"><inheritdoc cref="ExtraCells" path="/summary"/></param>
/// <param name="extraDigitsMask"><inheritdoc cref="ExtraDigitsMask" path="/summary"/></param>
public sealed class JuniorExocetMirrorAlmostHiddenSetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells,
	in CellMap extraCells,
	Mask extraDigitsMask
) :
	ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, CellMap.Empty, crosslineCells),
	IPatternType3StepTrait<JuniorExocetMirrorAlmostHiddenSetStep>
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <summary>
	/// Indicates the subset size.
	/// </summary>
	public int SubsetSize => BitOperations.PopCount(ExtraDigitsMask);

	/// <inheritdoc/>
	public override Technique Code => Technique.JuniorExocetMirrorAlmostHiddenSet;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | ExtraDigitsMask);

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ExocetAlmostHiddenSetSizeFactor",
				[nameof(SubsetSize)],
				GetType(),
				static args => OeisSequences.A002024((int)args![0]!)
			)
		];

	/// <summary>
	/// Indicates the cells that provides with the AHS rule.
	/// </summary>
	public CellMap ExtraCells { get; } = extraCells;

	/// <summary>
	/// Indicates the mask that holds the digits used by the AHS.
	/// </summary>
	public Mask ExtraDigitsMask { get; } = extraDigitsMask;

	/// <inheritdoc/>
	bool IPatternType3StepTrait<JuniorExocetMirrorAlmostHiddenSetStep>.IsHidden => true;

	/// <inheritdoc/>
	Mask IPatternType3StepTrait<JuniorExocetMirrorAlmostHiddenSetStep>.SubsetDigitsMask => ExtraDigitsMask;

	/// <inheritdoc/>
	CellMap IPatternType3StepTrait<JuniorExocetMirrorAlmostHiddenSetStep>.SubsetCells => ExtraCells;
}
