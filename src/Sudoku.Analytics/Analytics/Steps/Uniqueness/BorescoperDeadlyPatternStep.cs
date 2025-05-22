namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Borescoper's Deadly Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public abstract class BorescoperDeadlyPatternStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	in CellMap cells,
	Mask digitsMask
) :
	UnconditionalDeadlyPatternStep(conclusions, views, options),
	IDeadlyPatternTypeTrait
{
	/// <inheritdoc/>
	public override bool OnlyUseBivalueCells => false;

	/// <inheritdoc/>
	public override int BaseDifficulty => 53;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public sealed override Technique Code => Technique.Parse($"BorescoperDeadlyPatternType{Type}");

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// The map that contains the cells used for this technique.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the mask of used digits.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	private protected string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private protected string CellsStr => Options.Converter.CellConverter(Cells);
}
