namespace Sudoku.Analytics.Steps.Chains;

/// <summary>
/// Provides with a step that is a <b>(Grouped) Finned Chain</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="pattern"><inheritdoc/></param>
/// <param name="fins"><inheritdoc cref="Fins" path="/summary"/></param>
/// <param name="basedComponent"><inheritdoc cref="BasedComponent" path="/summary"/></param>
public sealed class FinnedChainStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	NamedChain pattern,
	in CandidateMap fins,
	MultipleChainBasedComponent basedComponent
) : NormalChainStep(conclusions, views, options, pattern)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <summary>
	/// Indicates the base technique used.
	/// </summary>
	public Technique BasedOn => TechniqueNaming.Chain.GetTechnique(Casted, Conclusions.AsSet());

	/// <inheritdoc/>
	public override Technique Code
		=> Casted.IsStrictlyGrouped || BasedComponent is not (MultipleChainBasedComponent.Cell or MultipleChainBasedComponent.House)
			? Technique.FinnedGroupedChain
			: Technique.FinnedChain;

	/// <inheritdoc/>
	public override Mask DigitsUsed => Casted.DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [ChainString, FinsStr]), new(SR.ChineseLanguage, [ChainString, FinsStr])];

	/// <summary>
	/// Indicates the extra fins.
	/// </summary>
	public CandidateMap Fins { get; } = fins;

	/// <summary>
	/// Indicates the base component.
	/// </summary>
	public MultipleChainBasedComponent BasedComponent { get; } = basedComponent;

	private string FinsStr => Fins.ToString(CoordinateConverter.GetInstance(Options.Converter));
}
