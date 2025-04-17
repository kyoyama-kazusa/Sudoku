namespace Sudoku.Measuring;

/// <summary>
/// Provides a way to calculate empty area of a <see cref="Grid"/> or a <see cref="CellMap"/>.
/// </summary>
public static class EmptyArea
{
	/// <summary>
	/// Try to get the maximum empty area exists in the specified grid.
	/// </summary>
	/// <param name="this">The grid to be checked.</param>
	/// <param name="topLeftCell">
	/// <inheritdoc cref="GetMaxEmptySquareArea(in CellMap, out Cell)" path="/param[@name='topLeftCell']"/>
	/// </param>
	/// <returns>A <see cref="Cell"/> value indicating the result.</returns>
	/// <remarks>
	/// <inheritdoc cref="GetMaxEmptyArea(in CellMap, out Cell)" path="/remarks"/>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Cell GetMaxEmptyArea(this in Grid @this, out Cell topLeftCell) => @this.EmptyCells.GetMaxEmptyArea(out topLeftCell);

	/// <summary>
	/// Try to get the maximum empty square area exists in the specified grid.
	/// </summary>
	/// <param name="this">The grid to be checked.</param>
	/// <param name="topLeftCell">
	/// <inheritdoc cref="GetMaxEmptySquareArea(in CellMap, out Cell)" path="/param[@name='topLeftCell']"/>
	/// </param>
	/// <returns>A <see cref="Cell"/> value indicating the result.</returns>
	/// <remarks>
	/// <inheritdoc cref="GetMaxEmptySquareArea(in CellMap, out Cell)" path="/remarks"/>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Cell GetMaxEmptySquareArea(this in Grid @this, out Cell topLeftCell)
		=> @this.EmptyCells.GetMaxEmptySquareArea(out topLeftCell);

	/// <summary>
	/// Try to get the maximum empty area exists in the specified cells.
	/// </summary>
	/// <param name="this">The cells to be checked.</param>
	/// <param name="topLeftCell">
	/// <inheritdoc cref="GetMaxEmptySquareArea(in CellMap, out Cell)" path="/param[@name='topLeftCell']"/>
	/// </param>
	/// <returns>A <see cref="Cell"/> value indicating the result.</returns>
	/// <remarks>
	/// This algorithm is from the puzzle called
	/// <see href="https://leetcode.com/problems/maximal-rectangle/"><i>Maximal Rectangle</i></see>.
	/// </remarks>
	public static Cell GetMaxEmptyArea(this in CellMap @this, out Cell topLeftCell)
	{
		var dp = (stackalloc Cell[9]);
		dp.Clear();

		(topLeftCell, var max) = (-1, 0);
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				dp[j] = @this.Contains(i * 9 + j) ? dp[j] + 1 : 0;
			}

			var (currentMax, currentTopLeft) = getMaxRow(dp, i);
			if (currentMax > max)
			{
				max = currentMax;
				topLeftCell = currentTopLeft;
			}
		}
		return max;


		static (Cell, Cell) getMaxRow(ReadOnlySpan<Cell> height, RowIndex row)
		{
			var (stack, max, topLeft) = (new Stack<Cell>(), 0, -1);
			for (var i = 0; i <= 9; i++)
			{
				var h = i == 9 ? 0 : height[i];
				while (stack.Count != 0 && height[stack.Peek()] >= h)
				{
					var maxHeight = height[stack.Pop()];
					var width = stack.Count == 0 ? i : i - 1 - stack.Peek();
					var area = maxHeight * width;
					if (area > max)
					{
						max = area;
						topLeft = (row - maxHeight + 1) * 9 + (stack.Count == 0 ? 0 : stack.Peek() + 1);
					}
				}
				stack.Push(i);
			}
			return (max, topLeft);
		}
	}

	/// <summary>
	/// Try to get the maximum empty square area exists in the specified cells.
	/// </summary>
	/// <param name="this">The cells to be checked.</param>
	/// <param name="topLeftCell">Indicates the cell at the top left index of the square.</param>
	/// <returns>A <see cref="Cell"/> value indicating the result.</returns>
	/// <remarks>
	/// This algorithm is from the puzzle called
	/// <see href="https://leetcode.com/problems/maximal-square/"><i>Maximal Square</i></see>.
	/// </remarks>
	public static Cell GetMaxEmptySquareArea(this in CellMap @this, out Cell topLeftCell)
	{
		(topLeftCell, var maxSide) = (-1, 0);
		var dp = (stackalloc Cell[81]);

		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				var index = i * 9 + j;
				if (@this.Contains(index))
				{
					if (i == 0 || j == 0)
					{
						dp[index] = 1;
					}
					else
					{
						var up = dp[(i - 1) * 9 + j];
						var left = dp[i * 9 + (j - 1)];
						var upLeft = dp[(i - 1) * 9 + (j - 1)];
						dp[index] = Math.Min(up, left, upLeft) + 1;
					}
				}
				else
				{
					dp[index] = 0;
				}

				var currentSide = dp[index];
				if (currentSide > maxSide)
				{
					maxSide = currentSide;
					var topRow = i - maxSide + 1;
					var leftCol = j - maxSide + 1;
					topLeftCell = topRow * 9 + leftCol;
				}
				else if (currentSide == maxSide && currentSide > 0)
				{
					var currentTopRow = i - currentSide + 1;
					var currentLeftCol = j - currentSide + 1;
					var currentTopIndex = currentTopRow * 9 + currentLeftCol;

					// To choose the minimal index of the top-left cell.
					if (currentTopRow < topLeftCell / 9 || currentTopRow == topLeftCell / 9 && currentLeftCol < topLeftCell % 9)
					{
						topLeftCell = currentTopIndex;
					}
				}
			}
		}

		return maxSide * maxSide;
	}
}
