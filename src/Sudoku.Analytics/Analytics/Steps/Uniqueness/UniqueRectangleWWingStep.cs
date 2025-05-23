namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle W-Wing</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="code"><inheritdoc cref="Step.Code" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="UniqueRectangleStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="UniqueRectangleStep.Digit2" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="UniqueRectangleStep.Cells" path="/summary"/></param>
/// <param name="isAvoidable"><inheritdoc cref="UniqueRectangleStep.IsAvoidable" path="/summary"/></param>
/// <param name="wDigit"><inheritdoc cref="WDigit" path="/summary"/></param>
/// <param name="connectors"><inheritdoc cref="Connectors" path="/summary"/></param>
/// <param name="endCells"><inheritdoc cref="EndCells" path="/summary"/></param>
/// <param name="absoluteOffset"><inheritdoc cref="UniqueRectangleStep.AbsoluteOffset" path="/summary"/></param>
public sealed class UniqueRectangleWWingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Technique code,
	Digit digit1,
	Digit digit2,
	in CellMap cells,
	bool isAvoidable,
	Digit wDigit,
	in CellMap connectors,
	in CellMap endCells,
	int absoluteOffset
) : UniqueRectangleStep(
	conclusions,
	views,
	options,
	code,
	digit1,
	digit2,
	cells,
	isAvoidable,
	absoluteOffset
)
{
	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | (Mask)(1 << WDigit));

	/// <summary>
	/// Indicates the digit W.
	/// </summary>
	public Digit WDigit { get; } = wDigit;

	/// <summary>
	/// Indicates the connectors.
	/// </summary>
	public CellMap Connectors { get; } = connectors;

	/// <summary>
	/// Indicates the end cells.
	/// </summary>
	public CellMap EndCells { get; } = endCells;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [D1Str, D2Str, CellsStr, ConnectorsString, EndCellsString, WDigitsString]),
			new(SR.ChineseLanguage, [D1Str, D2Str, CellsStr, ConnectorsString, EndCellsString, WDigitsString])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_RectangleIsAvoidableFactor",
				[nameof(IsAvoidable)],
				GetType(),
				static args => (bool)args![0]! ? 1 : 0
			)
		];

	private string ConnectorsString => Options.Converter.CellConverter(Connectors);

	private string EndCellsString => Options.Converter.CellConverter(EndCells);

	private string WDigitsString => Options.Converter.DigitConverter((Mask)(1 << WDigit));
}
