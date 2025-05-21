namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="SaveFileFailedEventHandler"/>.
/// </summary>
/// <param name="reason"><inheritdoc cref="Reason" path="/summary"/></param>
/// <remarks>
/// Initializes an <see cref="SaveFileFailedEventArgs"/> instance via the specified reason.
/// </remarks>
/// <seealso cref="SaveFileFailedEventHandler"/>
public sealed class SaveFileFailedEventArgs(SaveFileFailedReason reason) : EventArgs
{
	/// <summary>
	/// The failed reason.
	/// </summary>
	public SaveFileFailedReason Reason { get; } = reason;
}
