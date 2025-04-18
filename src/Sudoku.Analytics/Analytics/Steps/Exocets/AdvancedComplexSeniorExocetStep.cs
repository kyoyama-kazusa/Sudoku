namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is an <b>Advanced Complex Senior Exocet</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="digitsMask"><inheritdoc/></param>
/// <param name="baseCells"><inheritdoc/></param>
/// <param name="targetCells"><inheritdoc/></param>
/// <param name="endoTargetCells"><inheritdoc/></param>
/// <param name="crosslineCells"><inheritdoc/></param>
/// <param name="crosslineHousesMask">Indicates the mask holding a list of houses spanned for cross-line cells.</param>
/// <param name="extraHousesMask">Indicates the mask holding a list of extra houses.</param>
/// <param name="almostHiddenSetMask">The mask that holds a list of digits forming an external AHS.</param>
public sealed partial class AdvancedComplexSeniorExocetStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap endoTargetCells,
	in CellMap crosslineCells,
	[Property] HouseMask crosslineHousesMask,
	[Property] HouseMask extraHousesMask,
	[Property] Mask almostHiddenSetMask
) :
	ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, endoTargetCells, crosslineCells),
	IComplexSeniorExocet
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> base.BaseDifficulty + this.ShapeKind switch
		{
			ExocetShapeKind.Franken => 4,
			ExocetShapeKind.Mutant => 7,
			ExocetShapeKind.Basic => 0
		};

	/// <inheritdoc/>
	public override Technique Code
		=> this.ShapeKind switch
		{
			ExocetShapeKind.Franken => Technique.AdvancedFrankenSeniorExocet,
			ExocetShapeKind.Mutant => Technique.AdvancedMutantSeniorExocet
		};

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | AlmostHiddenSetMask);
}
