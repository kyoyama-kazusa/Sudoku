namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Bivalue Oddagon Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="loopCells"><inheritdoc cref="BivalueOddagonStep.LoopCells" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="BivalueOddagonStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="BivalueOddagonStep.Digit2" path="/summary"/></param>
/// <param name="extraCells"><inheritdoc cref="ExtraCells" path="/summary"/></param>
/// <param name="extraDigitsMask"><inheritdoc cref="ExtraDigitsMask" path="/summary"/></param>
public sealed class BivalueOddagonType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap loopCells,
	Digit digit1,
	Digit digit2,
	in CellMap extraCells,
	Mask extraDigitsMask
) : BivalueOddagonStep(conclusions, views, options, loopCells, digit1, digit2), IExtraCellListTrait
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | ExtraDigitsMask);

	/// <summary>
	/// Indicates the extra cells used.
	/// </summary>
	public CellMap ExtraCells { get; } = extraCells;

	/// <summary>
	/// Indicates the mask that contains all extra digits used.
	/// </summary>
	public Mask ExtraDigitsMask { get; } = extraDigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [LoopStr, Digit1Str, Digit2Str, DigitsStr, ExtraCellsStr]),
			new(SR.ChineseLanguage, [Digit1Str, Digit2Str, LoopStr, ExtraCellsStr, DigitsStr])
		];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			.. base.Factors,
			Factor.Create(
				"Factor_BivalueOddagonSubsetSizeFactor",
				[nameof(IExtraCellListTrait.ExtraCellSize)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!)
			)
		];

	/// <inheritdoc/>
	int IExtraCellListTrait.ExtraCellSize => ExtraCells.Count;

	private string Digit1Str => Options.Converter.DigitConverter((Mask)(1 << Digit1));

	private string Digit2Str => Options.Converter.DigitConverter((Mask)(1 << Digit2));

	private string DigitsStr => Options.Converter.DigitConverter(ExtraDigitsMask);

	private string ExtraCellsStr => Options.Converter.CellConverter(ExtraCells);
}
