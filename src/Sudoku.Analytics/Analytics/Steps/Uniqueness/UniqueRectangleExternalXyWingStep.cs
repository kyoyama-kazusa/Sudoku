namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle External XY-Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="guardianCells"><inheritdoc cref="GuardianCells" path="/summary"/></param>
/// <param name="cellPair"><inheritdoc cref="CellPair" path="/summary"/></param>
/// <param name="isIncomplete"><inheritdoc cref="IsIncomplete" path="/summary"/></param>
/// <param name="isAvoidable"><inheritdoc cref="UniqueRectangleStep.IsAvoidable" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
public sealed class UniqueRectangleExternalXyWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	in CellMap guardianCells,
	in CellMap cellPair,
	bool isIncomplete,
	bool isAvoidable,
	int absoluteOffset
) :
	UniqueRectangleStep(
		conclusions,
		views,
		options,
		isAvoidable ? Technique.AvoidableRectangleExternalXyWing : Technique.UniqueRectangleExternalXyWing,
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
	public bool IsIncomplete { get; } = isIncomplete;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public CellMap GuardianCells { get; } = guardianCells;

	/// <summary>
	/// Indicates the cell pair.
	/// </summary>
	public CellMap CellPair { get; } = cellPair;

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
				"Factor_UniqueRectangleExternalXyWingGuardianFactor",
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
				"Factor_UniqueRectangleExternalXyWingIsCompleteFactor",
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
