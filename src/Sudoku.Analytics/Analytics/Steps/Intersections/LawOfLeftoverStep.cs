namespace Sudoku.Analytics.Steps;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Law of Leftover (LoL)</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="set1House"><inheritdoc cref="Set1House" path="/summary"/></param>
/// <param name="set2House"><inheritdoc cref="Set2House" path="/summary"/></param>
/// <param name="set1"><inheritdoc cref="Set1" path="/summary"/></param>
/// <param name="set2"><inheritdoc cref="Set2" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
public sealed class LawOfLeftoverStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House set1House,
	House set2House,
	in CellMap set1,
	in CellMap set2,
	Mask digitsMask
) : IntersectionStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 20;

	/// <inheritdoc/>
	public override Technique Code => Technique.LawOfLeftover;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <summary>
	/// Indicates the house of <see cref="Set1"/>.
	/// </summary>
	public House Set1House { get; } = set1House;

	/// <summary>
	/// Indicates the house of <see cref="Set2"/>.
	/// </summary>
	public House Set2House { get; } = set2House;

	/// <summary>
	/// Indicates the first set to be used.
	/// </summary>
	public CellMap Set1 { get; } = set1;

	/// <summary>
	/// Indicates the second set to be used.
	/// </summary>
	public CellMap Set2 { get; } = set2;

	/// <summary>
	/// Indicates all 6 digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [Set1Str, Set2Str]), new(SR.ChineseLanguage, [Set1Str, Set2Str])];

	private string Set1Str => Options.Converter.CellConverter(Set1);

	private string Set2Str => Options.Converter.CellConverter(Set2);
}
