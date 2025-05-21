namespace Sudoku.Analytics.Steps.LastResorts;

/// <summary>
/// Provides with a step that is a <b>Pattern Overlay</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
public sealed class PatternOverlayStep(ReadOnlyMemory<Conclusion> conclusions, StepGathererOptions options) :
	LastResortStep(conclusions, null, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 85;

	/// <inheritdoc/>
	public override Technique Code => Technique.PatternOverlay;

	/// <summary>
	/// Indicates the digit.
	/// </summary>
	public Digit Digit => Conclusions.Span[0].Digit;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitStr]), new(SR.ChineseLanguage, [DigitStr])];

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << Digit));
}
