namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Rectangle Forcing Chains</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="PatternBasedChainStep" path="/summary"/></param>
public sealed class RectangleForcingChainsStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	RectangleForcingChains pattern
) : PatternBasedChainStep(conclusions, views, options, pattern)
{
	/// <inheritdoc/>
	public override bool IsMultiple => true;

	/// <inheritdoc/>
	public override bool IsDynamic => false;

	/// <summary>
	/// Indicates whether the pattern uses grouped nodes.
	/// </summary>
	public bool IsGrouped => Casted.Exists(static chain => chain.IsGrouped);

	/// <inheritdoc/>
	public override int BaseDifficulty => 70;

	/// <inheritdoc/>
	public override int Complexity => Casted.Complexity;

	/// <inheritdoc/>
	public override Technique Code => Technique.RectangleForcingChains;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Casted.DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations => [new(SR.EnglishLanguage, [ChainsStr]), new(SR.ChineseLanguage, [ChainsStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_MultipleForcingChainsGroupedFactor",
				[nameof(IsGrouped)],
				GetType(),
				static args => (bool)args![0]! ? 2 : 0
			),
			Factor.Create(
				"Factor_MultipleForcingChainsGroupedNodeFactor",
				[nameof(Pattern)],
				GetType(),
				static args =>
				{
					var result = 0;
					foreach (var branch in ((RectangleForcingChains)args![0]!).Values)
					{
						foreach (var link in branch.Links)
						{
							result += link.GroupedLinkPattern switch
							{
								AlmostLockedSetPattern => 2,
								AlmostHiddenSetPattern => 3,
								UniqueRectanglePattern => 4,
								FishPattern => 6,
								XyzWingPattern => 8,
								null when link.FirstNode.IsGroupedNode || link.SecondNode.IsGroupedNode => 1,
								_ => 0
							};
						}
					}
					return result;
				}
			),
			Factor.Create(
				"Factor_MultipleForcingChainsLengthFactor",
				[nameof(Complexity)],
				GetType(),
				static args => ChainingLength.GetLengthDifficulty((int)args![0]!)
			)
		];

	private string ChainsStr => Casted.ToString(new ChainFormatInfo(Options.Converter));

	private RectangleForcingChains Casted => (RectangleForcingChains)Pattern;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is RectangleForcingChainsStep comparer && Casted.Equals(comparer.Casted);

	/// <inheritdoc/>
	public override int CompareTo(Step? other)
		=> other is RectangleForcingChainsStep comparer
			? Conclusions.Length.CompareTo(comparer.Conclusions.Length) is var r and not 0
				? r
				: Casted.CompareTo(comparer.Casted)
			: -1;
}
