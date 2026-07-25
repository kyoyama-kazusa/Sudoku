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
				if (BitOperations.PopCount((uint)extraDigitsMask) != 2)
				{
					return [];
				}

				var digit1 = BitOperations.PopTwo((uint)extraDigitsMask, out var digit2);
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
				var urDigit1 = BitOperations.PopTwo((uint)comparer, out var urDigit2);
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

			/// <summary>
			/// Try to get extra eliminations produced by strong links inside a Unique Rectangle pattern, in a loop.
			/// </summary>
			/// <param name="cells">Indicates the cells to be checked.</param>
			/// <param name="map1">Indicates the map 1 in the node.</param>
			/// <param name="map2">Indicates the map 2 in the node.</param>
			/// <param name="comparer">Digits used in pattern.</param>
			/// <param name="grid">The grid to be checked.</param>
			/// <returns>A list of <see cref="Conclusion"/> instances found.</returns>
			/// <remarks>
			/// <para>Checking this would be tough. The basic rule is to assume both sides, and find intersection.</para>
			/// <para>
			/// Suppose the pattern:
			/// <code><![CDATA[
			/// ab  | abx
			/// abc | aby   a
			/// ]]></code>
			/// (suppose digit <c>a</c> can only be filled in 3 cells in the second block,
			/// digit <c>b</c> can only be filled in 2 cells in the second block,
			/// and <c>x</c> and <c>y</c> are extra digits, no matter what digits are here, but not <c>c</c>)
			/// The two cases are:
			/// <code><![CDATA[
			/// .------------------------------.     .-------------------------------.
			/// |         Missing  (c)         |     |         Missing  (a)          |
			/// |----------.---------.---------|     |----------.----------.---------|
			/// | a  .  .  | b  .  . | .  .  . |     | ab .  .  | ab  .  . | .  .  . |
			/// | b  .  .  | y  .  a | .  .  . |     | c  .  .  | ab  .  . | .  .  . |
			/// | .  .  .  | .  .  . | .  .  . |     | .  .  .  | .   .  . | .  .  . |
			/// |----------+---------+---------|     |----------+----------+---------|
			/// | .  .  .  | .  .  . | .  .  . |     | .  .  .  | .   .  . | .  .  . | 
			/// | .  .  .  | .  .  . | .  .  . |  &  | .  .  .  | .   .  . | .  .  . |
			/// | .  .  .  | .  .  . | .  .  . |     | .  .  .  | .   .  . | .  .  . |
			/// |----------+---------+---------|     |----------+----------+---------|
			/// | .  .  .  | .  .  . | .  .  . |     | .  .  .  | .   .  . | .  .  . |
			/// | .  .  .  | .  .  . | .  .  . |     | .  .  .  | .   .  . | .  .  . |
			/// | .  .  .  | .  .  . | .  .  . |     | .  .  .  | .   .  . | .  .  . |
			/// '----------'---------'---------'     '----------'----------'---------'
			/// ]]></code>
			/// For the first case, <c>b</c> must be filled into cell <c>abx</c>, and the pattern will be generalized;
			/// while for the second case, a hidden pair will be found in cells <c>abx</c> and <c>aby</c>.
			/// </para>
			/// <para>
			/// Therefore, both cases can eliminate <c>x</c> digits appeared in cell <c>abx</c>.
			/// </para>
			/// </remarks>
			public static ReadOnlySpan<Conclusion> GetConclusions2(
				in CellMap cells,
				in CandidateMap map1,
				in CandidateMap map2,
				Mask comparer,
				in Grid grid
			)
			{
				// Check whether the extra node is inside the block of one pair of UR cells.
				// If in line, no eliminations can be found because there's not an intersection can be created.
				var pairs = from cell in cells group cell by cell.GetHouse(HouseType.Block);

				// Iterate two states.
				var result = new HashSet<Conclusion>();
			checking_for_map_pair_loop:
				foreach (ref readonly var mapPair in ((map1, map2), (map2, map1)))
				{
					ref readonly var internalSide = ref mapPair.Item1;
					ref readonly var externalSide = ref mapPair.Item2;

					// Check whether two maps only hold one digit for each.
					var internalSideDigit = BitOperations.TrailingZeroCount(internalSide.Digits);
					var externalSideDigit = BitOperations.TrailingZeroCount(externalSide.Digits);
					if (internalSideDigit == FallbackConstants.@int || externalSideDigit == FallbackConstants.@int)
					{
						continue;
					}

					// Check whether the first map is inside one pair of UR cells, and the other map is not.
					var internalSideCells = internalSide.Cells;
					var externalSideCells = externalSide.Cells;
					var internalSideCellPair = CellMap.Empty;
					var externalSideCellPair = CellMap.Empty;
					foreach (ref readonly var pair in pairs)
					{
						var pairCells = pair.Values;
						if ((pair.Values & internalSideCells) == internalSideCells)
						{
							internalSideCellPair = pairCells;
						}
						if ((pair.Values & externalSideCells) == externalSideCells)
						{
							externalSideCellPair = pairCells;
						}
					}
					if (!internalSideCellPair || !!externalSideCellPair)
					{
						// Invalid pair.
						continue;
					}

					// Now 'internalSideCellPair' is the pair of cells in UR cells, which are in a same block,
					// including all cells from 'internalSide.Values'.
					// Now we should assume two sides, to determine the eliminations can be raised.

					// Case 1: Assign internal node with true, and determine which cells form hidden pairs.
					var internalNodeElims = CandidateMap.Empty;
					foreach (var cell in cells & ~internalSideCells)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~comparer))
						{
							internalNodeElims += cell * 9 + digit;
						}
					}

					// Case 2: Assign external node with true, and determine which cells are naked singles.
					var externalNodeElims = CandidateMap.Empty;
					foreach (var candidate in externalSide)
					{
						var cell = candidate / 9;
						var digit = candidate % 9;

						// Check intersection of peers of 'cell' and 'internalSideCells', to know which cell is filled with 'a'.
						switch (internalSideCells & Peer.PeersMap[cell])
						{
							// No intersection found. Sketch:
							//
							//  abc | abX
							//  abc | abY
							// -----+-----
							//      | a
							//
							// The pattern doesn't contain eliminations.
							case []:
							// There's more intersection cells. Sketch:
							//
							//  abc | ab
							//  abc | ab
							// -----+----
							//  a   |
							//
							// The pattern doesn't consider for eliminations now due to complexity.
							default:
							{
								continue checking_for_map_pair_loop;
							}

							// There's one intersection cell. Sketch:
							//
							//  abc | abX
							//  abc | abY  a
							// -----'--------
							//
							// The pattern can form elimination.
							case [var abcCellCanSeeExternalCell]:
							{
								// Check whether the intersection cell 'abcCellCanSeeExternalCell' only contains one extra digit.
								var digitsMaskOnAbcCell = (Mask)(grid.GetCandidates(abcCellCanSeeExternalCell) & ~comparer);
								if (BitOperations.TrailingZeroCount(digitsMaskOnAbcCell) == digit)
								{
									// Invalid.
									continue checking_for_map_pair_loop;
								}

								// We can know that the cell can be filled only 'b' now.
								// Therefore, the other cell in 'internalSideCells' can only be filled with 'a'.
								// Now check for the other two cells 'abX' and 'abY'.
								var theOtherTwoCells = cells & ~internalSideCellPair;
								var abyCells = theOtherTwoCells & Peer.PeersMap[abcCellCanSeeExternalCell];
								var abxCells = theOtherTwoCells & ~abyCells;

								// Because of assigning 'b' into 'abcCellCanSeeExternalCell',
								// we can know that two cells 'abcCellCanSeeExternalCell' and external cell form a hidden pair now.
								// Therefore, the house of those two cells cannot fill with digits 'a' and 'b',
								// and the two cells cannot be filled with the other digits of 'a' or 'b'.
								if (abxCells is not [var abxCell])
								{
									continue checking_for_map_pair_loop;
								}

								foreach (var otherDigit in (Mask)(grid.GetCandidates(abxCell) & ~comparer))
								{
									externalNodeElims += abxCell * 9 + otherDigit;
								}
								break;
							}
						}
					}

					// Determine intersection of eliminations, to know which candidates can be eliminated from both cases.
					var elimMap = internalNodeElims & externalNodeElims;
					if (!elimMap)
					{
						continue;
					}

					// Whoa! What a good message!
					foreach (var candidate in elimMap)
					{
						result.Add(new(Elimination, candidate));
					}
				}

				return result.AsSpan();
			}

			/// <summary>
			/// Try to get extra eliminations produced by strong links inside a Unique Rectangle pattern, in a loop.
			/// </summary>
			/// <param name="cells">Indicates the cells to be checked.</param>
			/// <param name="map1">Indicates the map 1 in the node.</param>
			/// <param name="map2">Indicates the map 2 in the node.</param>
			/// <param name="comparer">Digits used in pattern.</param>
			/// <param name="grid">The grid to be checked.</param>
			/// <returns>A list of <see cref="Conclusion"/> instances found.</returns>
			/// <remarks>
			/// <para>Checking this would be tough. The basic rule is to assume both sides, and find intersection.</para>
			/// <para>
			/// Suppose the pattern:
			/// <code><![CDATA[
			/// a  abw | aby
			///    abx | abz  b
			/// ]]></code>
			/// (strong links of <c>b</c> in block 1, and <c>a</c> in block 2)
			/// The two cases are:
			/// <code><![CDATA[
			/// .-----------------------------.     .-----------------------------.
			/// |         Missing (a)         |     |         Missing (b)         |
			/// |---------.---------.---------|     |---------.---------.---------|
			/// | .  .  b | a  .  . | .  .  . |     | a  .  w | b  .  . | .  .  . |
			/// | .  .  a | z  .  b | .  .  . |     | .  .  b | a  .  . | .  .  . |
			/// | .  .  . | .  .  . | .  .  . |     | .  .  . | .  .  . | .  .  . |
			/// |---------+---------+---------|     |---------+---------+---------|
			/// | .  .  . | .  .  . | .  .  . |  &  | .  .  . | .  .  . | .  .  . | 
			/// | .  .  . | .  .  . | .  .  . |     | .  .  . | .  .  . | .  .  . |
			/// | .  .  . | .  .  . | .  .  . |     | .  .  . | .  .  . | .  .  . |
			/// |---------+---------+---------|     |---------+---------+---------|
			/// | .  .  . | .  .  . | .  .  . |     | .  .  . | .  .  . | .  .  . |
			/// | .  .  . | .  .  . | .  .  . |     | .  .  . | .  .  . | .  .  . |
			/// | .  .  . | .  .  . | .  .  . |     | .  .  . | .  .  . | .  .  . |
			/// '---------'---------'---------'     '---------'---------'---------'
			/// ]]></code>
			/// Because both cases don't cover other digits (not <c>a</c> or <c>b</c>) from <c>abx</c> and <c>aby</c>,
			/// so they can be eliminated.
			/// </para>
			/// <para>
			/// 
			/// </para>
			/// </remarks>
			public static ReadOnlySpan<Conclusion> GetConclusions3(
				in CellMap cells,
				in CandidateMap map1,
				in CandidateMap map2,
				Mask comparer,
				in Grid grid
			)
			{
				throw new NotImplementedException();
			}
		}
	}
}
