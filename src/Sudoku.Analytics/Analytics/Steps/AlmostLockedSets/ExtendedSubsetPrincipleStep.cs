namespace Sudoku.Analytics.Steps.AlmostLockedSets;

/// <summary>
/// Provides with a step that is an <b>Extended Subset Principle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells">Indicates the cells used.</param>
/// <param name="digitsMask">Indicates the digits used.</param>
/// <param name="extraDigit">Indicates the extra digit used.</param>
public sealed partial class ExtendedSubsetPrincipleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CellMap cells,
	[Property] Mask digitsMask,
	[Property] Digit extraDigit
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

	/// <inheritdoc/>
	int ICellListTrait.CellSize => Cells.Count;

	private string CellsStr => Options.Converter.CellConverter(Cells);

	private string EspDigitStr => Options.Converter.DigitConverter((Mask)(1 << ExtraDigit));
}
