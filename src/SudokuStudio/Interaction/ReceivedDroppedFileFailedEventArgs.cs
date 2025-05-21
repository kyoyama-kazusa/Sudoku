namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="ReceivedDroppedFileFailedEventHandler"/>.
/// </summary>
/// <param name="reason"><inheritdoc cref="Reason" path="/summary"/></param>
/// <remarks>
/// Initializes a <see cref="ReceivedDroppedFileFailedEventArgs"/> instance via the specified reason.
/// </remarks>
/// <seealso cref="ReceivedDroppedFileFailedEventHandler"/>
public sealed class ReceivedDroppedFileFailedEventArgs(ReceivedDroppedFileFailedReason reason) : EventArgs
{
	/// <summary>
	/// The failed reason.
	/// </summary>
	public ReceivedDroppedFileFailedReason Reason { get; } = reason;
}
