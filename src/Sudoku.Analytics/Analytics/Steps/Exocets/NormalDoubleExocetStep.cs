namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is a <b>Double Exocet (Base)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="digitsMask"><inheritdoc/></param>
/// <param name="baseCells"><inheritdoc/></param>
/// <param name="targetCells"><inheritdoc/></param>
/// <param name="crosslineCells"><inheritdoc/></param>
/// <param name="baseCellsTheOther">Indicates the other side of the base cells.</param>
/// <param name="targetCellsTheOther">Indicates the other side of the target cells.</param>
public sealed partial class NormalDoubleExocetStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells,
	[Property] in CellMap baseCellsTheOther,
	[Property] in CellMap targetCellsTheOther
) :
	ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, CellMap.Empty, crosslineCells),
	IDoubleExocet
{
	/// <inheritdoc/>
	public override Technique Code => Technique.DoubleExocet;
}
