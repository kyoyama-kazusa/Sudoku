namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is a <b>Junior Exocet (Mirror Almost Hidden Set)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="digitsMask"><inheritdoc/></param>
/// <param name="baseCells"><inheritdoc/></param>
/// <param name="targetCells"><inheritdoc/></param>
/// <param name="crosslineCells"><inheritdoc/></param>
/// <param name="extraCells">Indicates the cells that provides with the AHS rule.</param>
/// <param name="extraDigitsMask">Indicates the mask that holds the digits used by the AHS.</param>
public sealed partial class JuniorExocetMirrorAlmostHiddenSetStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells,
	[Property] in CellMap extraCells,
	[Property] Mask extraDigitsMask
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

	/// <inheritdoc/>
	bool IPatternType3StepTrait<JuniorExocetMirrorAlmostHiddenSetStep>.IsHidden => true;

	/// <inheritdoc/>
	Mask IPatternType3StepTrait<JuniorExocetMirrorAlmostHiddenSetStep>.SubsetDigitsMask => ExtraDigitsMask;

	/// <inheritdoc/>
	CellMap IPatternType3StepTrait<JuniorExocetMirrorAlmostHiddenSetStep>.SubsetCells => ExtraCells;
}
