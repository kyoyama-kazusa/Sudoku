namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Bi-value Universal Grave False Candidate Type</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="falseCandidate"><inheritdoc cref="FalseCandidate" path="/summary"/></param>
public sealed class BivalueUniversalGraveFalseCandidateTypeStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Candidate falseCandidate
) : BivalueUniversalGraveStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 1;

	/// <inheritdoc/>
	public override int Type => 5;

	/// <inheritdoc/>
	public override Technique Code => Technique.BivalueUniversalGraveFalseCandidateType;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << FalseCandidate % 9);

	/// <summary>
	/// Indicates the false candidate that will cause a BUG deadly pattern if it is true.
	/// </summary>
	public Candidate FalseCandidate { get; } = falseCandidate;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [FalseCandidateStr]), new(SR.ChineseLanguage, [FalseCandidateStr])];

	private string FalseCandidateStr => Options.Converter.CandidateConverter([FalseCandidate]);
}
