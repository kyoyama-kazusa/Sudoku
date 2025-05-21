namespace Sudoku.Analytics.Steps.Singles;

/// <summary>
/// Represents a data structure that describes for a technique of <b>Full House</b>.
/// </summary>
/// <param name="conclusions"><inheritdoc cref="Step.Conclusions" path="/summary"/></param>
/// <param name="views"><inheritdoc cref="Step.Views" path="/summary"/></param>
/// <param name="options"><inheritdoc cref="Step.Options" path="/summary"/></param>
/// <param name="house">The house to be displayed.</param>
/// <param name="cell"><inheritdoc cref="SingleStep.Cell" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="SingleStep.Digit" path="/summary"/></param>
/// <param name="lasting"><inheritdoc cref="ILastingTrait.Lasting" path="/summary" /></param>
public sealed partial class FullHouseStep(
	ReadOnlyMemory<Conclusion> conclusions,
	View[]? views,
	StepGathererOptions options,
	[Property] House house,
	Cell cell,
	Digit digit,
	[Property] int lasting
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

	/// <inheritdoc/>
	public override Technique Code => Technique.FullHouse;
}
