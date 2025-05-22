namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle External Turbot Fish</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="guardianCells">Indicates the cells that the guardians lie in.</param>
/// <param name="isIncomplete">Indicates whether the pattern is incomplete.</param>
/// <param name="absoluteOffset"><inheritdoc/></param>
public sealed partial class UniqueRectangleExternalTurbotFishStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	[Property] in CellMap guardianCells,
	[Property] bool isIncomplete,
	int absoluteOffset
) :
	UniqueRectangleStep(
		conclusions,
		views,
		options,
		Technique.UniqueRectangleExternalTurbotFish,
		digit1,
		digit2,
		cells,
		false,
		absoluteOffset
	),
	IIncompleteTrait,
	IGuardianTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, GuardianCellsStr]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, GuardianCellsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_UniqueRectangleExternalTurbotFishGuardianFactor",
				[nameof(IGuardianTrait.GuardianCellsCount)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!)
			),
			Factor.Create(
				"Factor_UniqueRectangleExternalTurbotFishIsIncompleteFactor",
				[nameof(IsIncomplete)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	/// <inheritdoc/>
	int IGuardianTrait.GuardianCellsCount => GuardianCells.Count;

	private string GuardianCellsStr => Options.Converter.CellConverter(GuardianCells);
}
