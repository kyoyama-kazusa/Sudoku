namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Subset</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public abstract class SubsetStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House house,
	in CellMap cells,
	Mask digitsMask
) :
	FullPencilmarkingStep(conclusions, views, options),
	ISizeTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 30;

	/// <inheritdoc/>
	public int Size => BitOperations.PopCount(DigitsMask);

	/// <inheritdoc/>
	public sealed override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indiscates the house that pattern cells lying.
	/// </summary>
	public House House { get; } = house;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the mask that contains all digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;
}
