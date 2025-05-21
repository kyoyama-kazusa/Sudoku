namespace SudokuStudio.Interaction;

/// <summary>
/// Provides event data used by delegate type <see cref="ReceivedDroppedFileSuccessfullyEventHandler"/>.
/// </summary>
/// <param name="filePath"><inheritdoc cref="FilePath" path="/summary"/></param>
/// <param name="gridInfo"><inheritdoc cref="GridInfo" path="/summary"/></param>
/// <seealso cref="ReceivedDroppedFileSuccessfullyEventHandler"/>
public sealed class ReceivedDroppedFileSuccessfullyEventArgs(string filePath, GridInfo gridInfo) : EventArgs
{
	/// <summary>
	/// The path of the dropped file.
	/// </summary>
	public string FilePath { get; } = filePath;

	/// <summary>
	/// The loaded grid info.
	/// </summary>
	public GridInfo GridInfo { get; } = gridInfo;
}
