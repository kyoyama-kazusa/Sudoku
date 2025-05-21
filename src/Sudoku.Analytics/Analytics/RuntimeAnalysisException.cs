namespace Sudoku.Analytics;

/// <summary>
/// Represents an exception type that will be thrown by an <see cref="Analyzer"/> instance.
/// </summary>
/// <param name="grid"><inheritdoc cref="InvalidGrid" path="/summary"/></param>
/// <seealso cref="Analyzer"/>
public abstract class RuntimeAnalysisException(in Grid grid) : Exception
{
	/// <inheritdoc/>
	public abstract override string Message { get; }

	/// <summary>
	/// Indicates the grid to be analyzed.
	/// </summary>
	public Grid InvalidGrid { get; } = grid;
}
