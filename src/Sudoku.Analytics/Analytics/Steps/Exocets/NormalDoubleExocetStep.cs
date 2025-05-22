namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Double Exocet (Base)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
/// <param name="baseCellsTheOther"><inheritdoc cref="BaseCellsTheOther" path="/summary"/></param>
/// <param name="targetCellsTheOther"><inheritdoc cref="TargetCellsTheOther" path="/summary"/></param>
public sealed class NormalDoubleExocetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells,
	in CellMap baseCellsTheOther,
	in CellMap targetCellsTheOther
) :
	ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, CellMap.Empty, crosslineCells),
	IDoubleExocet
{
	/// <inheritdoc/>
	public override Technique Code => Technique.DoubleExocet;

	/// <summary>
	/// Indicates the other side of the base cells.
	/// </summary>
	public CellMap BaseCellsTheOther { get; } = baseCellsTheOther;

	/// <summary>
	/// Indicates the other side of the target cells.
	/// </summary>
	public CellMap TargetCellsTheOther { get; } = targetCellsTheOther;
}
