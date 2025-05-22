namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Fat Ring</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="row"><inheritdoc cref="Row" path="/summary"/></param>
/// <param name="column"><inheritdoc cref="Column" path="/summary"/></param>
/// <param name="blocks"><inheritdoc cref="Blocks" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="digitsCanAppearTwiceOrMore"><inheritdoc cref="DigitsCanAppearTwiceOrMore" path="/summary"/></param>
public sealed class FatRingStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	RowIndex row,
	ColumnIndex column,
	HouseMask blocks,
	Mask digitsMask,
	Mask digitsCanAppearTwiceOrMore
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

	/// <summary>
	/// Indicates the row used.
	/// </summary>
	public RowIndex Row { get; } = row;

	/// <summary>
	/// Indicates the column used.
	/// </summary>
	public ColumnIndex Column { get; } = column;

	/// <summary>
	/// Indicates the blocks used.
	/// </summary>
	public BlockIndex Blocks { get; } = blocks;

	/// <summary>
	/// Indicates the digits mask.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the digits that can appear in the target row and column twice or more.
	/// </summary>
	public Mask DigitsCanAppearTwiceOrMore { get; } = digitsCanAppearTwiceOrMore;

	private string DigitsString => Options.Converter.DigitConverter(DigitsMask);

	private string RowString => Options.Converter.HouseConverter(1 << Row);

	private string ColumnString => Options.Converter.HouseConverter(1 << Column);
}
