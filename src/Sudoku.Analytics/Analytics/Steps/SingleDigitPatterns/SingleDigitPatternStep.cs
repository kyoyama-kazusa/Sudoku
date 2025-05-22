namespace Sudoku.Analytics.Steps.SingleDigitPatterns;

/// <summary>
/// Provides with a step that is a <b>Single Digit Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
public abstract partial class SingleDigitPatternStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit
) : FullPencilmarkingStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public sealed override Mask DigitsUsed => (Mask)(1 << Digit);

	/// <summary>
	/// Indicates the digit used in this pattern.
	/// </summary>
	public Digit Digit { get; } = digit;
}
