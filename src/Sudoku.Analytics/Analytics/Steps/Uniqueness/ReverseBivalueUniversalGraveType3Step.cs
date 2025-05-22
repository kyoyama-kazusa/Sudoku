namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Reverse Bi-value Universal Grave Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="digit1"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit1" path="/summary"/></param>
/// <param name="digit2"><inheritdoc cref="ReverseBivalueUniversalGraveStep.Digit2" path="/summary"/></param>
/// <param name="subsetHouse"><inheritdoc cref="SubsetHouse" path="/summary"/></param>
/// <param name="subsetMask"><inheritdoc cref="SubsetMask" path="/summary"/></param>
/// <param name="pattern"><inheritdoc cref="ReverseBivalueUniversalGraveStep.CompletePattern" path="/summary"/></param>
/// <param name="emptyCells"><inheritdoc cref="ReverseBivalueUniversalGraveStep.EmptyCells" path="/summary"/></param>
public sealed class ReverseBivalueUniversalGraveType3Step(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	Digit digit1,
	Digit digit2,
	House subsetHouse,
	Mask subsetMask,
	in CellMap pattern,
	in CellMap emptyCells
) : ReverseBivalueUniversalGraveStep(conclusions, views, options, digit1, digit2, pattern, emptyCells)
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override Mask DigitsUsed => (Mask)(base.DigitsUsed | SubsetMask);

	/// <summary>
	/// Indicates the subset house used.
	/// </summary>
	public House SubsetHouse { get; } = subsetHouse;

	/// <summary>
	/// Indicates the subset digits mask.
	/// </summary>
	public Mask SubsetMask { get; } = subsetMask;

	/// <inheritdoc/>
	public override InterpolationArray Interpolations
		=> [new(SR.EnglishLanguage, [ExtraHouseStr, ExtraDigitsStr]), new(SR.ChineseLanguage, [ExtraHouseStr, ExtraDigitsStr])];

	private string ExtraHouseStr => Options.Converter.HouseConverter(1 << SubsetHouse);

	private string ExtraDigitsStr => Options.Converter.DigitConverter(SubsetMask);
}
