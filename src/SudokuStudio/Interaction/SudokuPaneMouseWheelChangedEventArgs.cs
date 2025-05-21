namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="SudokuPaneMouseWheelChangedEventHandler"/>.
/// </summary>
/// <param name="isClockwise"><inheritdoc cref="IsClockwise" path="/summary"/></param>
/// <remarks>
/// Initializes a <see cref="SudokuPaneMouseWheelChangedEventArgs"/> instance via a <see cref="bool"/> value
/// indicating whether the mouse wheel is clockwise.
/// </remarks>
/// <seealso cref="SudokuPaneMouseWheelChangedEventHandler"/>
public sealed class SudokuPaneMouseWheelChangedEventArgs(bool isClockwise) : EventArgs
{
	/// <summary>
	/// A <see cref="bool"/> value indicating whether the mouse wheel is clockwise.
	/// </summary>
	public bool IsClockwise { get; } = isClockwise;
}
