namespace Sudoku.Analytics.Steps.Intersections;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Law of Leftover (LoL)</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="set1House">Indicates the house of <see cref="Set1"/>.</param>
/// <param name="set2House">Indicates the house of <see cref="Set2"/>.</param>
/// <param name="set1">Indicates the first set to be used.</param>
/// <param name="set2">Indicates the second set to be used.</param>
/// <param name="digitsMask">Indicates all 6 digits used.</param>
public sealed partial class LawOfLeftoverStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] House set1House,
	[Property] House set2House,
	[Property] in CellMap set1,
	[Property] in CellMap set2,
	[Property] Mask digitsMask
) : IntersectionStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 20;

	/// <inheritdoc/>
	public override Technique Code => Technique.LawOfLeftover;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [Set1Str, Set2Str]), new(SR.ChineseLanguage, [Set1Str, Set2Str])];

	private string Set1Str => Options.Converter.CellConverter(Set1);

	private string Set2Str => Options.Converter.CellConverter(Set2);
}
