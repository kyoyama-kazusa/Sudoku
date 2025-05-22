namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle External W-Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="guardianCells">Indicates the cells that the guardians lie in.</param>
/// <param name="cellPair">Indicates the cell pair.</param>
/// <param name="isIncomplete">Indicates whether the rectangle is incomplete.</param>
/// <param name="isAvoidable"><inheritdoc/></param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public sealed partial class UniqueRectangleExternalWWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	[Property] in CellMap guardianCells,
	[Property] in CellMap cellPair,
	[Property] bool isIncomplete,
	bool isAvoidable,
	int absoluteOffset
) :
	UniqueRectangleStep(
		conclusions,
		views,
		options,
		isAvoidable ? Technique.AvoidableRectangleExternalWWing : Technique.UniqueRectangleExternalWWing,
		digit1,
		digit2,
		cells,
		isAvoidable,
		absoluteOffset
	),
	IGuardianTrait,
	IIncompleteTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 3;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, GuardianCellsStr, CellPairStr]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, GuardianCellsStr, CellPairStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_UniqueRectangleExternalWWingGuardianFactor",
				[nameof(IGuardianTrait.GuardianCellsCount)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!)
			),
			Factor.Create(
				"Factor_RectangleIsAvoidableFactor",
				[nameof(IsAvoidable)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			),
			Factor.Create(
				"Factor_UniqueRectangleExternalWWingIsCompleteFactor",
				[nameof(IsIncomplete)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	/// <inheritdoc/>
	int IGuardianTrait.GuardianCellsCount => GuardianCells.Count;

	private string GuardianCellsStr => Options.Converter.CellConverter(GuardianCells);

	private string CellPairStr => Options.Converter.CellConverter(CellPair);
}
