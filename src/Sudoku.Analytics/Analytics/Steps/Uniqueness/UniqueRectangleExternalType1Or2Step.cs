namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle External Type 1/2</b> or <b>Avoidable Rectangle External Type 1/2</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="guardianCells"><inheritdoc cref="GuardianCells" path="/summary"/></param>
/// <param name="guardianDigit"><inheritdoc cref="GuardianDigit" path="/summary"/></param>
/// <param name="isIncomplete"><inheritdoc cref="IsIncomplete" path="/summary"/></param>
/// <param name="isAvoidable"><inheritdoc cref="UniqueRectangleStep.IsAvoidable" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
public sealed class UniqueRectangleExternalType1Or2Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	in CellMap guardianCells,
	Digit guardianDigit,
	bool isIncomplete,
	bool isAvoidable,
	int absoluteOffset
) :
	UniqueRectangleStep(
		conclusions,
		views,
		options,
		(isAvoidable, guardianCells.Count == 1) switch
		{
			(true, true) => Technique.AvoidableRectangleExternalType1,
			(true, false) => Technique.AvoidableRectangleExternalType2,
			(false, true) => Technique.UniqueRectangleExternalType1,
			_ => Technique.UniqueRectangleExternalType2
		},
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
	public bool IsIncomplete { get; } = isIncomplete;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << GuardianDigit));

	/// <inheritdoc/>
	public CellMap GuardianCells { get; } = guardianCells;

	/// <summary>
	/// Indicates the digit that the guardians are used.
	/// </summary>
	public Digit GuardianDigit { get; } = guardianDigit;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, GuardianDigitStr, GuardianCellsStr]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, GuardianDigitStr, GuardianCellsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_UniqueRectangleExternalType1Or2GuardianFactor",
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
				"Factor_UniqueRectangleExternalType1Or2IsIncompleteFactor",
				[nameof(IsIncomplete)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	/// <inheritdoc/>
	int IGuardianTrait.GuardianCellsCount => GuardianCells.Count;

	private string GuardianDigitStr => Options.Converter.DigitConverter((Mask)(1 << GuardianDigit));

	private string GuardianCellsStr => Options.Converter.CellConverter(GuardianCells);
}
