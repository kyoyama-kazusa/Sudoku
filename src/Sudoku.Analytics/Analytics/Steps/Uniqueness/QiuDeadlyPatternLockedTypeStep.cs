namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern Locked Type</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="is2LinesWith2Cells"><inheritdoc/></param>
/// <param name="houses"><inheritdoc/></param>
/// <param name="corner1"><inheritdoc/></param>
/// <param name="corner2"><inheritdoc/></param>
/// <param name="candidates">Indicates the candidates used as locked one.</param>
public sealed partial class QiuDeadlyPatternLockedTypeStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	bool is2LinesWith2Cells,
	HouseMask houses,
	Cell? corner1,
	Cell? corner2,
	[Property(NamingRule = ">@Locked")] in CandidateMap candidates
) : QiuDeadlyPatternStep(conclusions, views, options, is2LinesWith2Cells, houses, corner1, corner2)
{
	/// <inheritdoc/>
	public override int Type => 5;

	/// <inheritdoc/>
	public override int BaseDifficulty => base.BaseDifficulty + 2;

	/// <inheritdoc/>
	public override Technique Code => Technique.LockedQiuDeadlyPattern;

	/// <inheritdoc/>
	public override Mask DigitsUsed => CandidatesLocked.Digits;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [PatternStr, Quantifier, Number, SingularOrPlural, CandidateStr, BeVerb]),
			new(SR.ChineseLanguage, [Number, PatternStr])
		];

	private string CandidateStr => Options.Converter.CandidateConverter(CandidatesLocked);

	private string Quantifier => CandidatesLocked.Count switch { 1 => string.Empty, 2 => " both", _ => " all" };

	private string Number => CandidatesLocked.Count == 1 ? " the" : $" {CandidatesLocked.Count}";

	private string SingularOrPlural => CandidatesLocked.Count == 1 ? "candidate" : "candidates";

	private string BeVerb => CandidatesLocked.Count == 1 ? "is" : "are";
}
