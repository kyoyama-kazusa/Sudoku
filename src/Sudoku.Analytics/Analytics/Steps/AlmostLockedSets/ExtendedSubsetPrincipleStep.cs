namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Extended Subset Principle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="extraDigit"><inheritdoc cref="ExtraDigit" path="/summary"/></param>
public sealed class ExtendedSubsetPrincipleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask,
	Digit extraDigit
) : AlmostLockedSetsStep(conclusions, views, options), ICellListTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 55;

	/// <inheritdoc/>
	public override Technique Code => Technique.ExtendedSubsetPrinciple;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [EspDigitStr, CellsStr]), new(SR.ChineseLanguage, [EspDigitStr, CellsStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ExtendedSubsetPrincipleSizeFactor",
				[nameof(ICellListTrait.CellSize)],
				GetType(),
				static args => (int)args![0]! switch { 3 or 4 => 0, 5 or 6 or 7 => 2, 8 or 9 => 4 }
			)
		];

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the extra digit used.
	/// </summary>
	public Digit ExtraDigit { get; } = extraDigit;

	/// <inheritdoc/>
	int ICellListTrait.CellSize => Cells.Count;

	private string CellsStr => Options.Converter.CellConverter(Cells);

	private string EspDigitStr => Options.Converter.DigitConverter((Mask)(1 << ExtraDigit));
}
