namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Advanced Complex Senior Exocet</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="endoTargetCells"><inheritdoc cref="ExocetStep.EndoTargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
/// <param name="crosslineHousesMask"><inheritdoc cref="CrosslineHousesMask" path="/summary"/></param>
/// <param name="extraHousesMask"><inheritdoc cref="ExtraHousesMask" path="/summary"/></param>
/// <param name="almostHiddenSetMask"><inheritdoc cref="AlmostHiddenSetMask" path="/summary"/></param>
public sealed class AdvancedComplexSeniorExocetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap endoTargetCells,
	in CellMap crosslineCells,
	HouseMask crosslineHousesMask,
	HouseMask extraHousesMask,
	Mask almostHiddenSetMask
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

	/// <summary>
	/// Indicates the mask holding a list of houses spanned for cross-line cells.
	/// </summary>
	public HouseMask CrosslineHousesMask { get; } = crosslineHousesMask;

	/// <summary>
	/// Indicates the mask holding a list of extra houses.
	/// </summary>
	public HouseMask ExtraHousesMask { get; } = extraHousesMask;

	/// <summary>
	/// The mask that holds a list of digits forming an external AHS.
	/// </summary>
	public Mask AlmostHiddenSetMask { get; } = almostHiddenSetMask;
}
