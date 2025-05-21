namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is a <b>Weak Exocet (BZ Rectangle)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="stabilityBalancer"><inheritdoc cref="StabilityBalancer" path="/summary"/></param>
/// <param name="missingValueCell"><inheritdoc cref="MissingValueCell" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
public sealed class WeakExocetBzRectangleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	Cell stabilityBalancer,
	Cell missingValueCell,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells
) : ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, CellMap.Empty, crosslineCells)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 7;

	/// <inheritdoc/>
	public override Technique Code => Technique.WeakExocetBzRectangle;

	/// <summary>
	/// Indicates the value cell that makes the exocet pattern to be stable.
	/// </summary>
	public Cell StabilityBalancer { get; } = stabilityBalancer;

	/// <summary>
	/// Indicates the missing value cell in cross-line.
	/// </summary>
	public Cell MissingValueCell { get; } = missingValueCell;
}
