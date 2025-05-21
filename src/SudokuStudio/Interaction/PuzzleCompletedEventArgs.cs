namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="PuzzleCompletedEventHandler"/>.
/// </summary>
/// <param name="finishedPuzzle"><inheritdoc cref="FinishedPuzzle" path="/summary"/></param>
/// <seealso cref="PuzzleCompletedEventHandler"/>
public sealed class PuzzleCompletedEventArgs(in Grid finishedPuzzle) : EventArgs
{
	/// <summary>
	/// Indicates whether the puzzle is fully fixed.
	/// </summary>
	public bool IsFullyFixed => FinishedPuzzle.ModifiableCellsCount == 0;

	/// <summary>
	/// Indicates the finished puzzle.
	/// </summary>
	public Grid FinishedPuzzle { get; } = finishedPuzzle;
}
