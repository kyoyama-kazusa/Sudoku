namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class EliminationCalculator
	{
		/// <summary>
		/// Provides with unique rectangle rule.
		/// </summary>
		public static class UniqueRectangle
		{
			/// <summary>
			/// Try to get extra eliminations produced by strong links inside a Unique Rectangle pattern, in a loop.
			/// </summary>
			/// <param name="cells">Indicates the cells to be checked.</param>
			/// <param name="comparer">Digits used in pattern.</param>
			/// <param name="grid">The grid to be checked.</param>
			/// <returns>A list of <see cref="Conclusion"/> instances found.</returns>
			/// <remarks>
			/// <para>Checking this would be tough. The basic rule is to assume both sides, and find intersection.</para>
			/// <para>
			/// Suppose the pattern:
			/// <code><![CDATA[
			/// ab  | abc
			/// abd | ab
			/// ]]></code>
			/// The two cases are:
			/// <code><![CDATA[
			/// .--------------------------------.     .--------------------------------.
			/// |          Missing  (c)          |     |          Missing  (d)          |
			/// |-----------.----------.---------|     |-----------.----------.---------|
			/// | ab  /  /  | ab  /  / | /  /  / |     | ab  /  /  | abc .  . | .  .  . |
			/// | abd .  .  | ab  /  / | .  .  . |     | ab  /  /  | ab  /  / | /  /  / |
			/// | .   .  .  | /   /  / | .  .  . |     | /   /  /  | .   .  . | .  .  . |
			/// |-----------+----------+---------|     |-----------+----------+---------|
			/// | .   .  .  | /   .  . | .  .  . |     | /   .  .  | .   .  . | .  .  . |
			/// | .   .  .  | /   .  . | .  .  . |  &  | /   .  .  | .   .  . | .  .  . |
			/// | .   .  .  | /   .  . | .  .  . |     | /   .  .  | .   .  . | .  .  . |
			/// |-----------+----------+---------|     |-----------+----------+---------|
			/// | .   .  .  | /   .  . | .  .  . |     | /   .  .  | .   .  . | .  .  . |
			/// | .   .  .  | /   .  . | .  .  . |     | /   .  .  | .   .  . | .  .  . |
			/// | .   .  .  | /   .  . | .  .  . |     | /   .  .  | .   .  . | .  .  . |
			/// '-----------'----------'---------'     '-----------'----------'---------'
			/// ]]></code>
			/// where slashes <c>/</c> means they are in the elimination range of subsets <c>ab</c>.
			/// Therefore, the elimination intersection will be:
			/// <code><![CDATA[
			/// .-----------.----------.---------.
			/// | ab  /  /  | abc .  . | .  .  . |
			/// | abd .  .  | ab  /  / | .  .  . |
			/// | .   .  .  | .   .  . | .  .  . |
			/// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			/// ]]></code>
			/// Which is intersection cells of cells <c>(abc)</c> and <c>(abd)</c>.
			/// Such cells can remove both digits <c>a</c> and <c>b</c>.
			/// </para>
			/// <para>
			/// All the other cases can be handled by supposing positions of digits <c>a</c> and <c>b</c>.
			/// </para>
			/// </remarks>
			public static ReadOnlySpan<Conclusion> GetConclusions(in CellMap cells, Mask comparer, in Grid grid)
			{
				var candidatesMap = grid.CandidatesMap;
				var extraDigitsMask = (Mask)(grid[cells] & ~comparer);
				if (BitOperations.PopCount(extraDigitsMask) != 2)
				{
					return [];
				}

				var digit1 = BitOperations.TrailingZeroCount(extraDigitsMask);
				var digit2 = extraDigitsMask.GetNextSet(digit1);
				var cells1 = candidatesMap[digit1] & cells;
				var cells2 = candidatesMap[digit2] & cells;

				// For two maps, we should determine which cells can be represented as naked pair eliminations.
				// Checking this, we should enumerate all possible pair of cells appeared in cells in both two cases:
				//
				//   1) Case 1: cells & ~cells1
				//   2) Case 2: cells & ~cells2
				//
				// Then we should find all peer intersection cells and make union.
				var (nakedPairElims1, nakedPairElims2) = (CellMap.Empty, CellMap.Empty);
				var urDigit1 = BitOperations.TrailingZeroCount(comparer);
				var urDigit2 = comparer.GetNextSet(urDigit1);
				var template = candidatesMap[urDigit1] | candidatesMap[urDigit2];
				foreach (ref readonly var pair in cells & ~cells1 & 2)
				{
					if (pair.FirstSharedHouse != FallbackConstants.@int)
					{
						nakedPairElims1 |= pair.PeerIntersection & template;
					}
				}
				foreach (ref readonly var pair in cells & ~cells2 & 2)
				{
					if (pair.FirstSharedHouse != FallbackConstants.@int)
					{
						nakedPairElims2 |= pair.PeerIntersection & template;
					}
				}

				var elimCells = nakedPairElims1 & nakedPairElims2;
				if (!elimCells)
				{
					return [];
				}

				var result = new List<Conclusion>();
				foreach (var elimCell in elimCells)
				{
					foreach (var digit in grid.GetCandidates(elimCell) & comparer)
					{
						result.Add(new(Elimination, elimCell, digit));
					}
				}
				return result.AsSpan();
			}
		}
	}
}
