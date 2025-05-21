namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="DigitInputEventHandler"/>.
/// </summary>
/// <param name="cell"><inheritdoc cref="Cell" path="/summary"/></param>
/// <param name="newDigitInput"><inheritdoc cref="DigitInput" path="/summary"/></param>
/// <seealso cref="DigitInputEventHandler"/>
public sealed class DigitInputEventArgs(Cell cell, Digit newDigitInput) : EventArgs
{
	/// <summary>
	/// A <see cref="bool"/> value indicating whether the event will cancel the inputting operation.
	/// </summary>
	public bool Cancel { get; set; }

	/// <summary>
	/// Indicates the cell that raises the event triggered.
	/// </summary>
	public Cell Cell { get; } = cell;

	/// <summary>
	/// Indicates the digit input that raises the event triggered. -1 is for clear the cell.
	/// </summary>
	public Digit DigitInput { get; } = newDigitInput;

	/// <summary>
	/// Indicates the candidate constructed. -1 is for the case that <see cref="DigitInput"/> is -1.
	/// </summary>
	public Candidate Candidate => DigitInput != -1 ? Cell * 9 + DigitInput : -1;
}
