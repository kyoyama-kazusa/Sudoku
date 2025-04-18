namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is a <b>Complex Junior Exocet (Adjacent Target)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="digitsMask"><inheritdoc/></param>
/// <param name="baseCells"><inheritdoc/></param>
/// <param name="targetCells"><inheritdoc/></param>
/// <param name="crosslineCells"><inheritdoc/></param>
/// <param name="crosslineHousesMask">Indicates the mask holding a list of houses spanned for cross-line cells.</param>
/// <param name="extraHousesMask">Indicates the mask holding a list of extra houses.</param>
/// <param name="singleMirrors">Indicates the single mirror cells. The value should be used one-by-one.</param>
public sealed partial class ComplexJuniorExocetAdjacentTargetStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells,
	[Property] HouseMask crosslineHousesMask,
	[Property] HouseMask extraHousesMask,
	[Property] in CellMap singleMirrors
) :
	ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, CellMap.Empty, crosslineCells),
	IComplexSeniorExocet
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> base.BaseDifficulty + 1 + this.ShapeKind switch { ExocetShapeKind.Franken => 4, ExocetShapeKind.Mutant => 6 };

	/// <inheritdoc/>
	public override Technique Code
		=> this.ShapeKind switch
		{
			ExocetShapeKind.Franken => Technique.FrankenJuniorExocetAdjacentTarget,
			ExocetShapeKind.Mutant => Technique.MutantJuniorExocetAdjacentTarget
		};
}
