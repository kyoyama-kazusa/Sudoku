namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Full House</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="lasting"><inheritdoc cref="Lasting" path="/summary"/></param>
public sealed class FullHouseStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	House house,
	Cell cell,
	Digit digit,
	int lasting
) :
	SingleStep(
		conclusions,
		views,
		options,
		cell,
		digit,
		house switch
		{
			< 9 => SingleSubtype.FullHouseBlock,
			>= 9 and < 18 => SingleSubtype.FullHouseRow,
			_ => SingleSubtype.FullHouseColumn
		}
	),
	ILastingTrait
{
	/// <inheritdoc/>
	public override int BaseDifficulty => 10;

	/// <inheritdoc cref="ILastingTrait.Lasting"/>
	public int Lasting { get; } = lasting;

	/// <inheritdoc/>
	public override Technique Code => Technique.FullHouse;

	/// <summary>
	/// The house to be displayed.
	/// </summary>
	public House House { get; } = house;
}
