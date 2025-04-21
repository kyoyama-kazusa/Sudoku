namespace Sudoku.Analytics.Steps.Uniqueness;

/// <summary>
/// Provides with a step that is a <b>Unique Matrix</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="cells">Indicates the cells used in this pattern.</param>
/// <param name="digitsMask">Indicates the mask that describes all digits used in this pattern.</param>
public abstract partial class UniqueMatrixStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CellMap cells,
	[Property] Mask digitsMask
) : UnconditionalDeadlyPatternStep(conclusions, views, options), IDeadlyPatternTypeTrait
{
	/// <inheritdoc/>
	public sealed override bool OnlyUseBivalueCells => true;

	/// <inheritdoc/>
	public override int BaseDifficulty => 53;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public sealed override Technique Code => Technique.Parse($"UniqueMatrixType{Type}");

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	private protected string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private protected string CellsStr => Options.Converter.CellConverter(Cells);
}
