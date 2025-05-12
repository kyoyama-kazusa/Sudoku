namespace Sudoku.Analytics.Steps.AlmostLockedSets;

/// <summary>
/// Provides with a step that is a <b>Fat Ring</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="options"><inheritdoc/></param>
/// <param name="row">Indicates the row.</param>
/// <param name="column">Indicates the column.</param>
/// <param name="blocks">Indicates the blocks used.</param>
/// <param name="digitsMask">Indicates the digits mask.</param>
/// <param name="digitsCanAppearTwiceOrMore">Indicates the digits that can appear in the target row and column twice or more.</param>
public sealed partial class FatRingStep(
	StepConclusions conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] RowIndex row,
	[Property] ColumnIndex column,
	[Property] HouseMask blocks,
	[Property] Mask digitsMask,
	[Property] Mask digitsCanAppearTwiceOrMore
) : AlmostLockedSetsStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 65;

	/// <inheritdoc/>
	public override Technique Code => Technique.FatRing;

	/// <inheritdoc/>
	public override Mask DigitsUsed => DigitsMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [DigitsString, RowString, ColumnString]),
			new(SR.ChineseLanguage, [DigitsString, RowString, ColumnString])
		];

	private string DigitsString => Options.Converter.DigitConverter(DigitsMask);

	private string RowString => Options.Converter.HouseConverter(1 << Row);

	private string ColumnString => Options.Converter.HouseConverter(1 << Column);
}
