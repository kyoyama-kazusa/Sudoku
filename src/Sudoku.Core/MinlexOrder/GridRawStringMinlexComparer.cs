namespace Sudoku.MinlexOrder;

/// <summary>
/// Represents a comparer that checks for min-lex rank of two <see cref="string"/> values
/// representing two different grids, and compare them.
/// </summary>
public sealed class GridRawStringMinlexComparer : IComparer<string>
{
	/// <summary>
	/// Indicates the backing min-lex finder object.
	/// </summary>
	private readonly MinlexFinder _finder = new();


	/// <inheritdoc/>
	public int Compare(string? x, string? y)
		=> (x, y) switch
		{
			(null, null) => 0,
			(null, not null) => -1,
			(not null, null) => 1,
			_ => unsafe ((x | _finder.Find | &MinlexRanker.GetRank, y | _finder.Find | &MinlexRanker.GetRank) | &ulong.CompareTo)
		};
}
