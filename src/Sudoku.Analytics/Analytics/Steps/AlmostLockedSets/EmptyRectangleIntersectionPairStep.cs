namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is an <b>Empty Rectangle Intersection Pair</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="startCell"><inheritdoc cref="StartCell" path="/summary"/></param>
/// <param name="endCell"><inheritdoc cref="EndCell" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="Digit2" path="/summary"/></param>
public sealed class EmptyRectangleIntersectionPairStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Cell startCell,
	Cell endCell,
	House house,
	Digit digit1,
	Digit digit2
) : AlmostLockedSetsStep(conclusions, views, options)
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 60;

	/// <inheritdoc/>
	public override Technique Code => Technique.EmptyRectangleIntersectionPair;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(1 << Digit1 | 1 << Digit2);

	/// <summary>
	/// Indicates the start cell to be calculated.
	/// </summary>
	public Cell StartCell { get; } = startCell;

	/// <summary>
	/// Indicates the end cell to be calculated.
	/// </summary>
	public Cell EndCell { get; } = endCell;

	/// <summary>
	/// Indicates the house index that the empty rectangle forms.
	/// </summary>
	public House House { get; } = house;

	/// <summary>
	/// Indicates the first digit used.
	/// </summary>
	public Digit Digit1 { get; } = digit1;

	/// <summary>
	/// Indicates the second digit used.
	/// </summary>
	public Digit Digit2 { get; } = digit2;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [
			new(SR.EnglishLanguage, [Digit1Str, Digit2Str, StartCellStr, EndCellStr, HouseStr]),
			new(SR.ChineseLanguage, [Digit1Str, Digit2Str, StartCellStr, EndCellStr, HouseStr])
		];

	private string Digit1Str => Options.Converter.DigitConverter((Mask)(1 << Digit1));

	private string Digit2Str => Options.Converter.DigitConverter((Mask)(1 << Digit2));

	private string StartCellStr => Options.Converter.CellConverter(in StartCell.AsCellMap());

	private string EndCellStr => Options.Converter.CellConverter(in EndCell.AsCellMap());

	private string HouseStr => Options.Converter.HouseConverter(1 << House);
}
