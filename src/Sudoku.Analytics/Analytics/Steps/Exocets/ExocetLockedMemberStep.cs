namespace Sudoku.Analytics.Steps.Exocets;

/// <summary>
/// Provides with a step that is an <b>Exocet (Locked Member)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="lockedMemberDigitsMask"><inheritdoc cref="LockedMemberDigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="endoTargetCells"><inheritdoc cref="ExocetStep.EndoTargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
public sealed class ExocetLockedMemberStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	Mask lockedMemberDigitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap endoTargetCells,
	in CellMap crosslineCells
) : ExocetStep(conclusions, views, options, digitsMask, baseCells, targetCells, endoTargetCells, crosslineCells)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override Technique Code => Delta < 0 ? Technique.SeniorExocetLockedMember : Technique.JuniorExocetLockedMember;

	/// <summary>
	/// Indicates the mask that holds a list of locked member digits.
	/// </summary>
	public Mask LockedMemberDigitsMask { get; } = lockedMemberDigitsMask;
}
