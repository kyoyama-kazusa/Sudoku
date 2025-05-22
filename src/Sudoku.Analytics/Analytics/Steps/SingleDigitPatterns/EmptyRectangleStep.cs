namespace Sudoku.Analytics.Steps.SingleDigitPatterns;

/// <summary>
/// Provides with a step that is an <b>Empty Rectangle</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleDigitPatternStep.Digit" path="/summary"/></param>
/// <param name="block"><inheritdoc cref="Block" path="/summary"/></param>
/// <param name="conjugatePair"><inheritdoc cref="ConjugatePair" path="/summary"/></param>
public sealed class EmptyRectangleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit,
	BlockIndex block,
	in Conjugate conjugatePair
) : SingleDigitPatternStep(conclusions, views, options, digit)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 46;

	/// <inheritdoc/>
	public override Technique Code => Technique.EmptyRectangle;

	/// <summary>
	/// Indicates the block that the real empty rectangle pattern lis in.
	/// </summary>
	public BlockIndex Block { get; } = block;

	/// <summary>
	/// Indicates the conjugate pair used.
	/// </summary>
	public Conjugate ConjugatePair { get; } = conjugatePair;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitStr, HouseStr, ConjStr]), new(SR.ChineseLanguage, [DigitStr, HouseStr, ConjStr])];

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << Digit));

	private string HouseStr => Options.Converter.HouseConverter(1 << Block);

	private string ConjStr => Options.Converter.ConjugateConverter([ConjugatePair]);
}
