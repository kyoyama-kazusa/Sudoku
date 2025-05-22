namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Bi-value Universal Grave + n</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="trueCandidates">Indicates the true candidates.</param>
public sealed partial class BivalueUniversalGraveMultipleStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] in CandidateMap trueCandidates
) : BivalueUniversalGraveStep(conclusions, views, options), ITrueCandidatesTrait, ICandidateListTrait
{
	/// <inheritdoc/>
	public override int Type => 5;

	/// <inheritdoc/>
	public override string EnglishName => $"{base.EnglishName[..^4]} + {TrueCandidates.Count}";

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override Technique Code => Technique.BivalueUniversalGravePlusN;

	/// <inheritdoc/>
	public override Mask DigitsUsed => TrueCandidates.Digits;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [CandidatesStr]), new(SR.ChineseLanguage, [CandidatesStr])];

	/// <inheritdoc/>
	public override FactorArray Factors
		=> [
			Factor.Create(
				"Factor_BivalueUniversalGraveMultipleTrueCandidateFactor",
				[nameof(ICandidateListTrait.CandidateSize)],
				GetType(),
				static args => OeisSequences.A002024((int)args![0]!)
			)
		];

	/// <inheritdoc/>
	int ICandidateListTrait.CandidateSize => TrueCandidates.Count;

	private string CandidatesStr => Options.Converter.CandidateConverter(TrueCandidates);


	/// <inheritdoce/>
	public override string GetName(IFormatProvider? formatProvider)
		=> $"{base.GetName(GetCulture(formatProvider))[..^4]} + {TrueCandidates.Count}";
}
