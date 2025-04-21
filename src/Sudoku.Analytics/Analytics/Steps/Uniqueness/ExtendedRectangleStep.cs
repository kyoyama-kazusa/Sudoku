namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is an <b>Extended Rectangle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="cells">Indicates the cells used in this pattern.</param>
/// <param name="digitsMask">Indicates the mask of digits used.</param>
public abstract partial class ExtendedRectangleStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CellMap cells,
	[Property] Mask digitsMask
) : UnconditionalDeadlyPatternStep(conclusions, views, options), IDeadlyPatternTypeTrait, ICellListTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => false;

	/// <inheritdoc/>
	public override int BaseDifficulty => 45;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public override Technique Code => Technique.Parse($"ExtendedRectangleType{Type}");

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ExtendedRectangleSizeFactor",
				[nameof(ICellListTrait.CellSize)],
				GetType(),
				static args => OeisSequences.A004526((int)args![0]!) - 2
			)
		];

	/// <inheritdoc/>
	int ICellListTrait.CellSize => Cells.Count;

	private protected string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private protected string CellsStr => Options.Converter.CellConverter(Cells);
}
