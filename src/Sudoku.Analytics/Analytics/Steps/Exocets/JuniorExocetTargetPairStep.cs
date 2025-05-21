namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is a <b>Junior Exocet (Incompatible Pair)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="targetPairMask"><inheritdoc cref="TargetPairMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
public sealed class JuniorExocetTargetPairStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	Mask targetPairMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells
) : ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, CellMap.Empty, crosslineCells)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override Technique Code => Technique.JuniorExocetTargetPair;

	/// <summary>
	/// Indicates the mask that holds the pair of digits that target cells forming a naked pair of such digits.
	/// </summary>
	public Mask TargetPairMask { get; } = targetPairMask;
}
