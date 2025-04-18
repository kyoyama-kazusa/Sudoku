namespace Sudoku.Shuffling.Transforming;

/// <summary>
/// Provides methods for <see cref="Grid"/> instances on transformations.
/// </summary>
/// <seealso cref="Grid"/>
public static class Transformation
{
	/// <summary>
	/// The table of clockwise rotation.
	/// </summary>
	private static readonly Cell[] ClockwiseTable = [
		72, 63, 54, 45, 36, 27, 18, 9, 0,
		73, 64, 55, 46, 37, 28, 19, 10, 1,
		74, 65, 56, 47, 38, 29, 20, 11, 2,
		75, 66, 57, 48, 39, 30, 21, 12, 3,
		76, 67, 58, 49, 40, 31, 22, 13, 4,
		77, 68, 59, 50, 41, 32, 23, 14, 5,
		78, 69, 60, 51, 42, 33, 24, 15, 6,
		79, 70, 61, 52, 43, 34, 25, 16, 7,
		80, 71, 62, 53, 44, 35, 26, 17, 8
	];

	/// <summary>
	/// The table of counter-clockwise rotation.
	/// </summary>
	private static readonly Cell[] CounterclockwiseTable = [
		8, 17, 26, 35, 44, 53, 62, 71, 80,
		7, 16, 25, 34, 43, 52, 61, 70, 79,
		6, 15, 24, 33, 42, 51, 60, 69, 78,
		5, 14, 23, 32, 41, 50, 59, 68, 77,
		4, 13, 22, 31, 40, 49, 58, 67, 76,
		3, 12, 21, 30, 39, 48, 57, 66, 75,
		2, 11, 20, 29, 38, 47, 56, 65, 74,
		1, 10, 19, 28, 37, 46, 55, 64, 73,
		0, 9, 18, 27, 36, 45, 54, 63, 72
	];

	/// <summary>
	/// The table of pi-rotation.
	/// </summary>
	private static readonly Cell[] PiRotateTable = [
		80, 79, 78, 77, 76, 75, 74, 73, 72,
		71, 70, 69, 68, 67, 66, 65, 64, 63,
		62, 61, 60, 59, 58, 57, 56, 55, 54,
		53, 52, 51, 50, 49, 48, 47, 46, 45,
		44, 43, 42, 41, 40, 39, 38, 37, 36,
		35, 34, 33, 32, 31, 30, 29, 28, 27,
		26, 25, 24, 23, 22, 21, 20, 19, 18,
		17, 16, 15, 14, 13, 12, 11, 10, 9,
		8, 7, 6, 5, 4, 3, 2, 1, 0
	];

	/// <summary>
	/// Indicates the swappable pairs, which means the swappable houses.
	/// </summary>
	private static readonly (House, House)[] SwappableHouses = [
		(9, 10), (9, 11), (10, 11), (12, 13), (12, 14), (13, 14), (15, 16), (15, 17), (16, 17),
		(18, 19), (18, 20), (19, 20), (21, 22), (21, 23), (22, 23), (24, 25), (24, 26), (25, 26)
	];


	/// <summary>
	/// Mirror left-right the grid.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result grid.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	public static ref TGrid MirrorLeftRight<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
	{
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				@ref.Swap(ref @this[i * 9 + j], ref @this[i * 9 + (8 - j)]);
			}
		}
		return ref @this;
	}

	/// <summary>
	/// Mirror top-bottom the grid.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result grid.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	public static ref TGrid MirrorTopBottom<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
	{
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				@ref.Swap(ref @this[i * 9 + j], ref @this[(8 - i) * 9 + j]);
			}
		}
		return ref @this;
	}

	/// <summary>
	/// Mirror diagonal the grid.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result grid.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	public static ref TGrid MirrorDiagonal<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
	{
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				@ref.Swap(ref @this[i * 9 + j], ref @this[j * 9 + i]);
			}
		}
		return ref @this;
	}

	/// <summary>
	/// Transpose the grid.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result grid.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TGrid Transpose<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
		=> ref @this.MirrorDiagonal<TGrid>();

	/// <summary>
	/// Mirror anti-diagonal the grid.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result grid.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	public static ref TGrid MirrorAntidiagonal<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
	{
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				@ref.Swap(ref @this[i * 9 + j], ref @this[(8 - j) * 9 + (8 - i)]);
			}
		}
		return ref @this;
	}

	/// <summary>
	/// Rotate the grid clockwise.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	public static ref TGrid RotateClockwise<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
	{
		var result = TGrid.Undefined;
		for (var cell = 0; cell < 81; cell++)
		{
			result[cell] = @this[ClockwiseTable[cell]];
		}

		@this = result;
		return ref @this;
	}

	/// <summary>
	/// Rotate the grid counterclockwise.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	public static ref TGrid RotateCounterclockwise<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
	{
		var result = TGrid.Undefined;
		for (var cell = 0; cell < 81; cell++)
		{
			result[cell] = @this[CounterclockwiseTable[cell]];
		}

		@this = result;
		return ref @this;
	}

	/// <summary>
	/// Rotate the grid <c><see cref="Math.PI"/></c> degrees.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <returns>The result.</returns>
	/// <remarks>
	/// This method will return the reference that is same as the argument,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	/// <seealso cref="Math.PI"/>
	public static ref TGrid RotatePi<TGrid>(this ref TGrid @this) where TGrid : unmanaged, IGrid<TGrid>
	{
		var result = TGrid.Undefined;
		for (var cell = 0; cell < 81; cell++)
		{
			result[cell] = @this[PiRotateTable[cell]];
		}

		@this = result;
		return ref @this;
	}

	/// <summary>
	/// Swap two digits.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <param name="digit1">The digit 1 to be swapped.</param>
	/// <param name="digit2">The digit 2 to be swapped.</param>
	/// <returns>The result.</returns>
	/// <exception cref="ArgumentException">Throws when the puzzle is not solved.</exception>
	public static ref TGrid SwapDigit<TGrid>(this ref TGrid @this, Digit digit1, Digit digit2) where TGrid : unmanaged, IGrid<TGrid>
	{
		if (digit1 == digit2)
		{
			return ref @this;
		}

		@this.Unfix();
		var digits1Map = @this.ValuesMap[digit1];
		var digits2Map = @this.ValuesMap[digit2];
		foreach (var cell in digits1Map) { @this.SetDigit(cell, -1); }
		foreach (var cell in digits2Map) { @this.SetDigit(cell, -1); }
		foreach (var cell in digits1Map) { @this.SetDigit(cell, digit2); }
		foreach (var cell in digits2Map) { @this.SetDigit(cell, digit1); }
		@this.Fix();
		return ref @this;
	}

	/// <summary>
	/// Swap to houses.
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <param name="houseIndex1">The house 1 to be swapped.</param>
	/// <param name="houseIndex2">The house 2 to be swapped.</param>
	/// <returns>The result.</returns>
	/// <exception cref="ArgumentException">
	/// Throws when two specified house argument is not in valid range (0..27),
	/// two houses are not in same house type, or are not swappable.
	/// </exception>
	/// <remarks>
	/// This method will return the reference that is same as the argument <paramref name="this"/>,
	/// in order to inline multiple transformation operations.
	/// </remarks>
	public static ref TGrid SwapHouse<TGrid>(this ref TGrid @this, House houseIndex1, House houseIndex2)
		where TGrid : unmanaged, IGrid<TGrid>
	{
		ArgumentOutOfRangeException.ThrowIfOutOfRange(houseIndex1, 9, 27);
		ArgumentOutOfRangeException.ThrowIfOutOfRange(houseIndex2, 9, 27);
		ArgumentException.ThrowIfAssertionFailed(houseIndex1.ToHouseType() == houseIndex2.ToHouseType());
		ArgumentException.ThrowIfAssertionFailed(Array.Exists(SwappableHouses, houseIndexChecker));

		for (var i = 0; i < 9; i++)
		{
			@ref.Swap(ref @this[HousesCells[houseIndex1][i]], ref @this[HousesCells[houseIndex2][i]]);
		}
		return ref @this;


		bool houseIndexChecker((House, House) pair) => pair == (houseIndex1, houseIndex2) || pair == (houseIndex2, houseIndex1);
	}

	/// <summary>
	/// Swap chutes (i.e. mega-rows or mega-columns).
	/// </summary>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	/// <param name="this">The grid.</param>
	/// <param name="chuteIndex1">The first chute to be swapped.</param>
	/// <param name="chuteIndex2">The second chute to be swapped.</param>
	/// <returns>The result.</returns>
	/// <exception cref="ArgumentException">Throws when two specified chute index is not in valid range (0..6).</exception>
	public static ref TGrid SwapChute<TGrid>(this ref TGrid @this, int chuteIndex1, int chuteIndex2)
		where TGrid : unmanaged, IGrid<TGrid>
	{
		ArgumentOutOfRangeException.ThrowIfOutOfRange(chuteIndex1, 0, 6);
		ArgumentOutOfRangeException.ThrowIfOutOfRange(chuteIndex2, 0, 6);
		ArgumentException.ThrowIfAssertionFailed(chuteIndex1 is >= 0 and < 3 == chuteIndex2 is >= 0 and < 3);

		if (chuteIndex1 == chuteIndex2)
		{
			return ref @this;
		}

		ref readonly var chuteCells1 = ref Chutes[chuteIndex1].Cells;
		ref readonly var chuteCells2 = ref Chutes[chuteIndex2].Cells;
		for (var i = 0; i < 27; i++)
		{
			@ref.Swap(ref @this[chuteCells1[i]], ref @this[chuteCells2[i]]);
		}
		return ref @this;
	}
}
