namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Anonymous Deadly Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digitsMask">Indicates the digits used.</param>
/// <param name="cells">Indicates cells used.</param>
/// <param name="technique"><inheritdoc cref="Code" path="/summary"/></param>
public abstract partial class AnonymousDeadlyPatternStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Mask digitsMask,
	[Property] in CellMap cells,
	Technique technique
) : ConditionalDeadlyPatternStep(conclusions, views, options), IDeadlyPatternTypeTrait
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

	private protected string DigitsStr => Options.Converter.DigitConverter(DigitsMask);

	private protected string CellsStr => Options.Converter.CellConverter(Cells);
}
