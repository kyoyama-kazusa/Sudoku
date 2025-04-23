namespace Sudoku.Analytics;

/// <summary>
/// Represents a pattern searcher type.
/// </summary>
/// <typeparam name="TPattern">The type of pattern.</typeparam>
public abstract class PatternSearcher<TPattern> where TPattern : Pattern
{
	/// <summary>
	/// Try to search patterns and return them.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <returns>The found patterns.</returns>
	public abstract ReadOnlySpan<TPattern> Search(in Grid grid);
}
