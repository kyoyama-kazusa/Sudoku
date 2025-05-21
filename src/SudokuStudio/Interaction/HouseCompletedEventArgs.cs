namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="HouseCompletedEventHandler"/>.
/// </summary>
/// <param name="lastCell"><inheritdoc cref="LastCell" path="/summary"/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
/// <param name="method"><inheritdoc cref="Method" path="/summary"/></param>
/// <seealso cref="HouseCompletedEventHandler"/>
public sealed class HouseCompletedEventArgs(Cell lastCell, House house, PuzzleUpdatingMethod method) : EventArgs
{
	/// <summary>
	/// Indicates a method kind that makes a house be completed.
	/// </summary>
	public PuzzleUpdatingMethod Method { get; } = method;

	/// <summary>
	/// Indicates the last cell finished.
	/// </summary>
	public Cell LastCell { get; } = lastCell;

	/// <summary>
	/// Indicates the house finished.
	/// </summary>
	public House House { get; } = house;
}
