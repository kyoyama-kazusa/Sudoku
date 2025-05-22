namespace Sudoku.Analytics.Steps.LastResorts;

/// <summary>
/// Provides with a step that is a <b>Template</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="isTemplateDeletion"><inheritdoc cref="IsTemplateDeletion" path="/summary"/></param>
public sealed class TemplateStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool isTemplateDeletion
) : LastResortStep(conclusions, views, options)
{
	/// <summary>
	/// Indicates the current template step is a template deletion.
	/// </summary>
	public bool IsTemplateDeletion { get; } = isTemplateDeletion;

	/// <inheritdoc/>
	public override int BaseDifficulty => 90;

	/// <inheritdoc/>
	public override Technique Code => IsTemplateDeletion ? Technique.TemplateDelete : Technique.TemplateSet;

	/// <summary>
	/// Indicates the digit.
	/// </summary>
	public Digit Digit => Conclusions.Span[0].Digit;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations => [new(SR.EnglishLanguage, [DigitStr]), new(SR.ChineseLanguage, [DigitStr])];

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << Digit));
}
