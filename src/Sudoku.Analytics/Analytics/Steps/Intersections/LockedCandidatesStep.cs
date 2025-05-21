namespace Sudoku.Analytics.Steps.Intersections;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Locked Candidates</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit">Indicates the digit used.</param>
/// <param name="baseSet">Indicates the house that the current locked candidates forms.</param>
/// <param name="coverSet">Indicates the house that the current locked candidates effects.</param>
public sealed partial class LockedCandidatesStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] Digit digit,
	[Property] House baseSet,
	[Property] House coverSet
) : IntersectionStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => BaseSet < 9 ? 26 : 28;

	/// <inheritdoc/>
	public override Technique Code => BaseSet < 9 ? Technique.Pointing : Technique.Claiming;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit);

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [DigitStr, BaseSetStr, CoverSetStr]), new(SR.ChineseLanguage, [DigitStr, BaseSetStr, CoverSetStr])];

	private string DigitStr => Options.Converter.DigitConverter((Mask)(1 << Digit));

	private string BaseSetStr => Options.Converter.HouseConverter(1 << BaseSet);

	private string CoverSetStr => Options.Converter.HouseConverter(1 << CoverSet);
}
