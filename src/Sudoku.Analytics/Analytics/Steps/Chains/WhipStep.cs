namespace Sudoku.Analytics.Steps.Chains;

/// <summary>
/// Provides with a step that is a <b>Whip</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="truths"><inheritdoc cref="Truths" path="/summary"/></param>
/// <param name="links"><inheritdoc cref="Links" path="/summary"/></param>
/// <param name="isGrouped"><inheritdoc cref="IsGrouped" path="/summary"/></param>
public sealed class WhipStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	ReadOnlyMemory<Space> truths,
	ReadOnlyMemory<Space> links,
	bool isGrouped
) : SpecializedChainStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override bool IsMultiple => false;

	/// <inheritdoc/>
	public override bool IsDynamic => true;

	/// <summary>
	/// Indicates whether the whip pattern is grouped.
	/// </summary>
	public bool IsGrouped { get; } = isGrouped;

	/// <inheritdoc/>
	public override int Complexity => Views![0].OfType<CandidateViewNode>().Length; // A tricky way to check number of nodes used.

	/// <inheritdoc/>
	public override int BaseDifficulty => IsGrouped ? 82 : 80;

	/// <summary>
	/// Indicates the truths.
	/// </summary>
	public ReadOnlyMemory<Space> Truths { get; } = truths;

	/// <summary>
	/// Indicates the links.
	/// </summary>
	public ReadOnlyMemory<Space> Links { get; } = links;

	/// <inheritdoc/>
	public override Technique Code => IsGrouped ? Technique.GroupedWhip : Technique.Whip;

	/// <inheritdoc/>
	public override Mask DigitsUsed
		=> (Mask)(
			Mask.Create((from t in Truths select t.Digit).Span)
				| Mask.Create((from l in Links select l.Digit).Span)
		);

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_WhipComplexityFactor",
				[nameof(Complexity)],
				GetType(),
				static args => ChainingLength.GetLengthDifficulty((int)args[0]!)
			)
		];

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [TruthsStr, LinksStr]), new(SR.ChineseLanguage, [TruthsStr, LinksStr])];

	private string TruthsStr => string.Join(' ', from t in Truths.Span select t.ToString());

	private string LinksStr => string.Join(' ', from l in Links.Span select l.ToString());


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Step? other)
		=> other is WhipStep comparer && Conclusions.Span[0] == comparer.Conclusions.Span[0];

	/// <inheritdoc/>
	public override int CompareTo(Step? other)
		=> other is WhipStep comparer
			? Conclusions.Span[0].CompareTo(comparer.Conclusions.Span[0]) is var conclusionComparisonResult and not 0
				? conclusionComparisonResult
				: 0
			: -1;
}
