namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is a <b>Junior Exocet (Adjacent Target)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="endoTargetCells"><inheritdoc cref="ExocetStep.EndoTargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
/// <param name="singleMirrors"><inheritdoc cref="SingleMirrors" path="/summary"/></param>
public sealed class JuniorExocetAdjacentTargetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap endoTargetCells,
	in CellMap crosslineCells,
	in CellMap singleMirrors
) : ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, endoTargetCells, crosslineCells)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override Technique Code => Technique.JuniorExocetAdjacentTarget;

	/// <summary>
	/// Indicates the single mirror cells. The value should be used one-by-one.
	/// </summary>
	public CellMap SingleMirrors { get; } = singleMirrors;
}
