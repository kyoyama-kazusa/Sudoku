namespace Sudoku.Analytics;

public partial class Hub
{
	public partial class EliminationCalculator
	{
		/// <summary>
		/// Provides with chain elimination rule.
		/// </summary>
		public static class Chain
		{
			/// <summary>
			/// Try to get all possible conclusions via the specified grid and two <see cref="Node"/> instances.
			/// </summary>
			/// <param name="grid">The grid.</param>
			/// <param name="node1">The first node.</param>
			/// <param name="node2">The second node.</param>
			/// <returns>A sequence of <see cref="Conclusion"/> instances.</returns>
			/// <seealso cref="Conclusion"/>
			public static ReadOnlySpan<Conclusion> GetConclusions(in Grid grid, Node node1, Node node2)
			{
				var candidatesMap = grid.CandidatesMap;
				if (node1 == ~node2)
				{
					// Two nodes are same, meaning the node must be true. Check whether it is grouped one.
					var digit = node1.Map[0] % 9;
					var map = node1.Map / digit;
					return node1.IsGroupedNode
						? from cell in map.PeerIntersection & candidatesMap[digit] select new Conclusion(Elimination, cell, digit)
						: [];
				}

				// Two nodes aren't same. Check for values.
				if ((node1, node2) is not ({ Map: { Digits: var p, Cells: var c1 } m1 }, { Map: { Digits: var q, Cells: var c2 } m2 }))
				{
					return [];
				}

				switch (m1, m2)
				{
					case ([var candidate1], [var candidate2]):
					{
						var (cell1, digit1) = (candidate1 / 9, candidate1 % 9);
						var (cell2, digit2) = (candidate2 / 9, candidate2 % 9);
						if (cell1 == cell2)
						{
							// Same cell.
							return
								from digit in (Mask)(grid.GetCandidates(cell1) & ~(1 << digit1 | 1 << digit2))
								select new Conclusion(Elimination, cell1, digit);
						}
						else if (digit1 == digit2)
						{
							// Same digit.
							return
								from cell in (cell1.AsCellMap() + cell2).PeerIntersection & candidatesMap[digit1]
								select new Conclusion(Elimination, cell, digit1);
						}
						else
						{
							// Otherwise (Different cell and digit).
							var result = new List<Conclusion>(2);
							if ((grid.GetCandidates(cell1) >> digit2 & 1) != 0)
							{
								result.Add(new(Elimination, cell1, digit2));
							}
							if ((grid.GetCandidates(cell2) >> digit1 & 1) != 0)
							{
								result.Add(new(Elimination, cell2, digit1));
							}
							return result.AsSpan();
						}
					}
					case var _ when BitOperations.IsPow2(p) && BitOperations.IsPow2(q) && p == q:
					{
						var digit = BitOperations.Log2(p);
						return from cell in (c1 | c2).PeerIntersection & candidatesMap[digit] select new Conclusion(Elimination, cell, digit);
					}
					default:
					{
						return [];
					}
				}
			}
		}
	}
}
