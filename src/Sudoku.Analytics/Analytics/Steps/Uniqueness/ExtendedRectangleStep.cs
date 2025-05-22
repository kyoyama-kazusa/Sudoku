namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Extended Rectangle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public abstract class ExtendedRectangleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask
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

	/// <summary>
	/// Indicates the cells used in this pattern.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the mask of digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

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
