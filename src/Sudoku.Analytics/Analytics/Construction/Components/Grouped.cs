namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a list of methods that operates with concept "Grouped Node".
/// </summary>
public static class Grouped
{
	/// <summary>
	/// Provides extension members on <see cref="CellMap"/>.
	/// </summary>
	extension(CellMap)
	{
		/// <summary>
		/// Determine whether the specified cells can be split into two parts, and they form a grouped strong link with each other.
		/// </summary>
		/// <param name="cells">The cells to be checked.</param>
		/// <param name="house">The house.</param>
		/// <param name="spannedHousesList">The spanned houses.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FormsGroupedStrongLink(in CellMap cells, House house, out ReadOnlySpan<HouseMask> spannedHousesList)
		{
			// The link must be one of the cases in:
			//   1) If non-grouped, two cell maps must contain 2 cells.
			//   2) If grouped, two cell maps must be the case either:
			//     a. The house type is block - the number of spanned rows or columns must be 2.
			//     b. The house type is row or column - the number of spanned blocks must be 2.
			// Otherwise, invalid.
			switch (house.ToHouseType())
			{
				case HouseType.Block:
				{
					// Check row.
					var list = new List<HouseMask>(2);
					if (cells.RowMask << 9 is var rowsSpanned && BitOperations.PopCount(rowsSpanned) == 2)
					{
						list.Add(rowsSpanned);
					}

					// Check column.
					if (cells.ColumnMask << 18 is var columnSpanned && BitOperations.PopCount(columnSpanned) == 2)
					{
						list.Add(columnSpanned);
					}

					if (list.Count != 0)
					{
						spannedHousesList = list.AsSpan();
						return true;
					}

					goto default;
				}
				case HouseType.Row or HouseType.Column
				when cells.BlockMask is var blocksSpanned && BitOperations.PopCount(blocksSpanned) == 2:
				{
					// Check block.
					spannedHousesList = (HouseMask[])[blocksSpanned];
					return true;
				}
				default:
				{
					spannedHousesList = default;
					return false;
				}
			}
		}
	}
}
