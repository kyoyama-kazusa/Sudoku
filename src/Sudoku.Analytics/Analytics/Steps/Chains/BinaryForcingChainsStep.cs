namespace Sudoku.Analytics.Steps.Chains;

/// <summary>
/// Provides with a step that is a <b>Dynamic Binary Forcing Chains</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pattern"><inheritdoc/></param>
public sealed partial class BinaryForcingChainsStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	BinaryForcingChains pattern
) : PatternBasedChainStep(conclusions, views, options, pattern)
{
	/// <inheritdoc/>
	public override bool IsMultiple => false;

	/// <inheritdoc/>
	public override bool IsDynamic => true;

	/// <summary>
	/// Indicates whether the binary forcing chains is a contradiction forcing chains.
	/// </summary>
	public bool IsContradiction => Casted.IsContradiction;

	/// <inheritdoc/>
	public override int Complexity => Casted.Complexity;

	/// <inheritdoc/>
	public override int BaseDifficulty => 88;

	/// <inheritdoc/>
	public override Technique Code
		=> IsContradiction ? Technique.DynamicContradictionForcingChains : Technique.DynamicDoubleForcingChains;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Casted.DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [ChainsStr]), new(SR.ChineseLanguage, [ChainsStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_DynamicForcingChainsLengthFactor",
				[nameof(Complexity)],
				GetType(),
				static args => ChainingLength.GetLengthDifficulty((int)args![0]!)
			)
		];

	private string ChainsStr => Casted.ToString(new ChainFormatInfo(Options.Converter));

	private BinaryForcingChains Casted => (BinaryForcingChains)Pattern;
}
