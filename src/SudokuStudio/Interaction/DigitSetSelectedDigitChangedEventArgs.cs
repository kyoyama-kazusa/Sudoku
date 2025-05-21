namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="DigitSetSelectedDigitChangedEventHandler"/>.
/// </summary>
/// <param name="newDigit"><inheritdoc cref="NewDigit" path="/summary"/></param>
/// <seealso cref="DigitSetSelectedDigitChangedEventHandler"/>
public sealed class DigitSetSelectedDigitChangedEventArgs(Digit newDigit) : EventArgs
{
	/// <summary>
	/// Indicates the new digit selected.
	/// </summary>
	public Digit NewDigit { get; } = newDigit;
}
