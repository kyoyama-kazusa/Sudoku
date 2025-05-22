namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Complex Junior Exocet (Adjacent Target)</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="ExocetStep.DigitsMask" path="/summary"/></param>
/// <param name="baseCells"><inheritdoc cref="ExocetStep.BaseCells" path="/summary"/></param>
/// <param name="targetCells"><inheritdoc cref="ExocetStep.TargetCells" path="/summary"/></param>
/// <param name="crosslineCells"><inheritdoc cref="ExocetStep.CrosslineCells" path="/summary"/></param>
/// <param name="crosslineHousesMask"><inheritdoc cref="CrosslineHousesMask" path="/summary"/></param>
/// <param name="extraHousesMask"><inheritdoc cref="ExtraHousesMask" path="/summary"/></param>
/// <param name="singleMirrors"><inheritdoc cref="SingleMirrors" path="/summary"/></param>
public sealed class ComplexJuniorExocetAdjacentTargetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap baseCells,
	in CellMap targetCells,
	in CellMap crosslineCells,
	HouseMask crosslineHousesMask,
	HouseMask extraHousesMask,
	in CellMap singleMirrors
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

	/// <summary>
	/// Indicates the mask holding a list of houses spanned for cross-line cells.
	/// </summary>
	public HouseMask CrosslineHousesMask { get; } = crosslineHousesMask;

	/// <summary>
	/// Indicates the mask holding a list of extra houses.
	/// </summary>
	public HouseMask ExtraHousesMask { get; } = extraHousesMask;

	/// <summary>
	/// Indicates the single mirror cells. The value should be used one-by-one.
	/// </summary>
	public CellMap SingleMirrors { get; } = singleMirrors;
}
