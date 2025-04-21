namespace Sudoku.Analytics;

public partial class Hub
{
	/// <summary>
	/// Represents empty rectangle rule.
	/// </summary>
	public static class EmptyRectangle
	{
		/// <summary>
		/// Determine whether the specified cells in the specified block form an empty rectangle.
		/// </summary>
		/// <param name="cells">The cells to be checked.</param>
		/// <param name="block">The block where the cells may form an empty rectangle pattern.</param>
		/// <param name="row">The row that the empty rectangle used.</param>
		/// <param name="column">The column that the empty rectangle used.</param>
		/// <returns>
		/// A <see cref="bool"/> value indicating that. If <see langword="true"/>,
		/// both arguments <paramref name="row"/> and <paramref name="column"/> can be used;
		/// otherwise, both arguments should be discards.
		/// </returns>
		public static bool IsEmptyRectangle(in CellMap cells, House block, out House row, out House column)
		{
			var (r, c) = (block / 3 * 3 + 9, block % 3 * 3 + 18);
			for (var (i, count) = (r, 0); i < r + 3; i++)
			{
				if (!!(cells & HousesMap[i]) || ++count <= 1)
				{
					continue;
				}

				row = column = -1;
				return false;
			}

			for (var (i, count) = (c, 0); i < c + 3; i++)
			{
				if (!!(cells & HousesMap[i]) || ++count <= 1)
				{
					continue;
				}

				row = column = -1;
				return false;
			}

			for (var i = r; i < r + 3; i++)
			{
				for (var j = c; j < c + 3; j++)
				{
					if (cells & ~(HousesMap[i] | HousesMap[j]))
					{
						continue;
					}

					row = i;
					column = j;
					return true;
				}
			}

			row = column = -1;
			return false;
		}
	}
}
