namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Anonymous Deadly Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="technique"><inheritdoc cref="Code" path="/summary"/></param>
public abstract class AnonymousDeadlyPatternStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Mask digitsMask,
	in CellMap cells,
	Technique technique
) :
	ConditionalDeadlyPatternStep(conclusions, views, options),
	IDeadlyPatternTypeTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty
		=> Code is >= Technique.RotatingDeadlyPatternType1 and <= Technique.RotatingDeadlyPatternType4 ? 58 : 50;

	/// <inheritdoc/>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public sealed override bool OnlyUseBivalueCells => false;

	/// <inheritdoc/>
	public sealed override Technique Code { get; } = technique;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	private protected string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private protected string CellsStr => Options.Converter.CellConverter(Cells);
}
