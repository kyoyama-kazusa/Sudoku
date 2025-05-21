namespace Sudoku.Analytics.Steps.Chains;

/// <summary>
/// Provides with a step that is a <b>(Grouped) Chain</b> or <b>(Grouped) Loop</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="PatternBasedChainStep.Pattern" path="/summary"/></param>
public class NormalChainStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	NamedChain pattern
) : PatternBasedChainStep(conclusions, views, options, pattern)
{
	/// <summary>
	/// Indicates the sort key that can be used as chaining comparison.
	/// </summary>
	public int SortKey
		=> (IsLoop ? 2000 : 0)
			+ (IsCannibalistic ? 1000 : 0)
			+ (IsGrouped ? 500 : 0)
			+ TechniqueNaming.Chain.GetSortKey(Casted, Conclusions.AsSet());

	/// <inheritdoc/>
	public sealed override bool IsMultiple => false;

	/// <inheritdoc/>
	public sealed override bool IsDynamic => false;

	/// <summary>
	/// Indicates whether the chain is cannibalistic,
	/// meaning at least one of a candidate inside a node used in the pattern is also a conclusion.
	/// </summary>
	public bool IsCannibalistic => Casted.OverlapsWithConclusions(Conclusions.AsSet());

	/// <summary>
	/// Indicates whether the chain uses at least one grouped node.
	/// </summary>
	public bool IsGrouped => Pattern.IsGrouped;

	/// <summary>
	/// Indicates whether the chain pattern forms a Continuous Nice Loop or Grouped Continuous Nice Loop.
	/// </summary>
	public bool IsLoop => Pattern is ContinuousNiceLoop;

	/// <inheritdoc/>
	public override int BaseDifficulty => 60;

	/// <inheritdoc/>
	public sealed override int Complexity => Casted.Length;

	/// <inheritdoc/>
	public override Technique Code => TechniqueNaming.Chain.GetTechnique(Casted, Conclusions.AsSet());

	/// <inheritdoc/>
	public override Mask DigitsUsed => Casted.DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [ChainString]), new(SR.ChineseLanguage, [ChainString])];

	/// <inheritdoc/>
	public sealed override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_ChainLengthFactor",
				[nameof(Complexity)],
				GetType(),
				static args => ChainingLength.GetLengthDifficulty((int)args![0]!)
			),
			Factor.Create(
				"Factor_ChainGroupedFactor",
				[nameof(IsGrouped)],
				GetType(),
				static args => (bool)args![0]! ? 2 : 0
			),
			Factor.Create(
				"Factor_ChainGroupedNodeFactor",
				[nameof(Pattern)],
				GetType(),
				static args =>
				{
					var result = 0;
					var p = (Chain)args![0]!;
					foreach (var link in p.Links)
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
					return result;
				}
			)
		];

	private protected string ChainString => Casted.ToString(new ChainFormatInfo(Options.Converter));

	private protected NamedChain Casted => (NamedChain)Pattern;


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] Step? other)
		=> other is NormalChainStep comparer && Casted.Equals(comparer.Casted);

	/// <inheritdoc/>
	public sealed override int CompareTo(Step? other)
		=> other is NormalChainStep comparer
			? SortKey.CompareTo(comparer.SortKey) is var result and not 0
				? result
				: Casted.CompareTo(comparer.Casted)
			: -1;
}
