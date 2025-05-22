namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Single</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
/// <param name="subtype"><inheritdoc cref="Subtype" path="/summary"/></param>
public abstract class SingleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell cell,
	Digit digit,
	SingleSubtype subtype
) : DirectStep(conclusions, views, options)
{
	/// <summary>
	/// Indicates the subtype of the technique.
	/// </summary>
	public SingleSubtype Subtype { get; } = subtype;

	/// <inheritdoc/>
	public sealed override Mask DigitsUsed => (Mask)(1 << Digit);

	/// <summary>
	/// Indicates the cell used.
	/// </summary>
	public Cell Cell { get; } = cell;

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	public Digit Digit { get; } = digit;
}
