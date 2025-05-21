namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="OpenFileFailedEventHandler"/>.
/// </summary>
/// <param name="reason"><inheritdoc cref="Reason" path="/summary"/></param>
/// <seealso cref="OpenFileFailedEventHandler"/>
/// <remarks>
/// Initializes an <see cref="OpenFileFailedEventArgs"/> instance via the specified reason.
/// </remarks>
public sealed class OpenFileFailedEventArgs(OpenFileFailedReason reason) : EventArgs
{
	/// <summary>
	/// The failed reason.
	/// </summary>
	public OpenFileFailedReason Reason { get; } = reason;
}
