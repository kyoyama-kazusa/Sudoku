namespace Sudoku.Transformations;

/// <summary>
/// Provides methods for <see cref="Grid"/> instances on transformations.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridTransformationExtensions
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


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ref Grid @this)
	{
		/// <summary>
		/// Transforms the grid by the specified type.
		/// </summary>
		/// <param name="transformTypes">
		/// The transform types can be applied.
		/// </param>
		public ref Grid Transform(TransformType transformTypes)
		{
			if (transformTypes == TransformType.None)
			{
				return ref @this;
			}

			var rng = Random.Shared;
			foreach (var kind in transformTypes)
			{
				unsafe
				{
					delegate*<Random, ref Grid, void> method = kind switch
					{
						TransformType.DigitSwap => &swapDigits,
						TransformType.RowSwap => &swapRow,
						TransformType.ColumnSwap => &swapColumn,
						TransformType.BandSwap => &swapBand,
						TransformType.TowerSwap => &swapTower,
						TransformType.MirrorLeftRight => &mirrorLeftRight,
						TransformType.MirrorTopBottom => &mirrorTopBottom,
						TransformType.MirrorDiagonal => &mirrorDiagonal,
						TransformType.MirrorAntidiagonal => &mirrorAntidiagonal,
						TransformType.RotateClockwise => &rotateClockwise,
						TransformType.RotateCounterclockwise => &rotateCounterclockwise
					};
					method(rng, ref @this);
				}
			}
			return ref @this;


			static void swapDigits(Random random, ref Grid grid)
			{
				for (var i = 0; i < 10; i++)
				{
					var d1 = random.NextDigit();
					int d2;
					do
					{
						d2 = random.NextDigit();
					} while (d1 == d2);
					grid.SwapDigit(d1, d2);
				}
			}

			static void swapRow(Random random, ref Grid grid)
			{
				for (var i = 0; i < 10; i++)
				{
					var l1 = random.Next(9, 18);
					var l1p = l1 / 3;
					int l2;
					do
					{
						l2 = l1p * 3 + random.Next(0, 3);
					} while (l1 == l2);
					grid.SwapHouse(l1, l2);
				}
			}

			static void swapColumn(Random random, ref Grid grid)
			{
				for (var i = 0; i < 10; i++)
				{
					var l1 = random.Next(18, 27);
					var l1p = l1 / 3;
					int l2;
					do
					{
						l2 = l1p * 3 + random.Next(0, 3);
					} while (l1 == l2);
					grid.SwapHouse(l1, l2);
				}
			}

			static void swapBand(Random random, ref Grid grid)
			{
				for (var i = 0; i < 2; i++)
				{
					var c1 = random.Next(0, 3);
					int c2;
					do
					{
						c2 = random.Next(0, 3);
					} while (c1 == c2);
					grid.SwapChute(c1, c2);
				}
			}

			static void swapTower(Random random, ref Grid grid)
			{
				for (var i = 0; i < 2; i++)
				{
					var c1 = random.Next(3, 6);
					int c2;
					do
					{
						c2 = random.Next(3, 6);
					} while (c1 == c2);
					grid.SwapChute(c1, c2);
				}
			}

			static void mirrorLeftRight(Random _, ref Grid grid) => grid.MirrorLeftRight();

			static void mirrorTopBottom(Random _, ref Grid grid) => grid.MirrorTopBottom();

			static void mirrorDiagonal(Random _, ref Grid grid) => grid.MirrorDiagonal();

			static void mirrorAntidiagonal(Random _, ref Grid grid) => grid.MirrorAntidiagonal();

			static void rotateClockwise(Random random, ref Grid grid)
			{
				var times = random.Next(0, 4);
				for (var i = 0; i < times; i++)
				{
					grid.RotateClockwise();
				}
			}

			static void rotateCounterclockwise(Random random, ref Grid grid)
			{
				var times = random.Next(0, 4);
				for (var i = 0; i < times; i++)
				{
					grid.RotateCounterclockwise();
				}
			}
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	/// <typeparam name="TGrid">The type of grid.</typeparam>
	extension<TGrid>(ref TGrid @this) where TGrid : unmanaged, IInlineArrayGrid<TGrid>
	{
		/// <summary>
		/// Mirror left-right the grid.
		/// </summary>
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		public ref TGrid MirrorLeftRight()
		{
			for (var i = 0; i < 9; i++)
			{
				for (var j = 0; j < 9; j++)
				{
					Unsafe.Swap(ref @this[i * 9 + j], ref @this[i * 9 + (8 - j)]);
				}
			}
			return ref @this;
		}

		/// <summary>
		/// Mirror top-bottom the grid.
		/// </summary>
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		public ref TGrid MirrorTopBottom()
		{
			for (var i = 0; i < 9; i++)
			{
				for (var j = 0; j < 9; j++)
				{
					Unsafe.Swap(ref @this[i * 9 + j], ref @this[(8 - i) * 9 + j]);
				}
			}
			return ref @this;
		}

		/// <summary>
		/// Mirror diagonal the grid.
		/// </summary>
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		public ref TGrid MirrorDiagonal()
		{
			for (var i = 0; i < 9; i++)
			{
				for (var j = i + 1; j < 9; j++)
				{
					Unsafe.Swap(ref @this[i * 9 + j], ref @this[j * 9 + i]);
				}
			}
			return ref @this;
		}

		/// <summary>
		/// Transpose the grid.
		/// </summary>
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		public ref TGrid Transpose() => ref @this.MirrorDiagonal<TGrid>();

		/// <summary>
		/// Mirror anti-diagonal the grid.
		/// </summary>
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		public ref TGrid MirrorAntidiagonal()
		{
			for (var i = 0; i < 9; i++)
			{
				for (var j = 0; j < 9; j++)
				{
					Unsafe.Swap(ref @this[i * 9 + j], ref @this[(8 - j) * 9 + (8 - i)]);
				}
			}
			return ref @this;
		}

		/// <summary>
		/// Rotate the grid clockwise.
		/// </summary>
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		public ref TGrid RotateClockwise()
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
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		public ref TGrid RotateCounterclockwise()
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
		/// <returns>The reference to the current grid.</returns>
		/// <remarks>
		/// This method will return the reference that is same as the argument,
		/// in order to inline multiple transformation operations.
		/// </remarks>
		/// <seealso cref="Math.PI"/>
		public ref TGrid RotatePi()
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
		/// <param name="digit1">The digit 1 to be swapped.</param>
		/// <param name="digit2">The digit 2 to be swapped.</param>
		/// <returns>The reference to the current grid.</returns>
		/// <exception cref="ArgumentException">Throws when the puzzle is not solved.</exception>
		public ref TGrid SwapDigit(Digit digit1, Digit digit2)
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
		/// <param name="houseIndex1">The house 1 to be swapped.</param>
		/// <param name="houseIndex2">The house 2 to be swapped.</param>
		/// <returns>The reference to the current grid.</returns>
		/// <exception cref="ArgumentException">
		/// Throws when two specified house argument is not in valid range (0..27),
		/// two houses are not in same house type, or are not swappable.
		/// </exception>
		public ref TGrid SwapHouse(House houseIndex1, House houseIndex2)
		{
			if (houseIndex1 == houseIndex2)
			{
				return ref @this;
			}

			ArgumentOutOfRangeException.Assert(houseIndex1 is >= 9 and < 27);
			ArgumentOutOfRangeException.Assert(houseIndex2 is >= 9 and < 27);
			ArgumentException.Assert(houseIndex1.HouseType == houseIndex2.HouseType);

			for (var i = 0; i < 9; i++)
			{
				Unsafe.Swap(ref @this[HousesCells[houseIndex1][i]], ref @this[HousesCells[houseIndex2][i]]);
			}
			return ref @this;
		}

		/// <summary>
		/// Swap chutes (i.e. mega-rows or mega-columns).
		/// </summary>
		/// <param name="chuteIndex1">The first chute to be swapped.</param>
		/// <param name="chuteIndex2">The second chute to be swapped.</param>
		/// <returns>The reference to the current grid.</returns>
		/// <exception cref="ArgumentException">Throws when two specified chute index is not in valid range (0..6).</exception>
		public ref TGrid SwapChute(int chuteIndex1, int chuteIndex2)
		{
			ArgumentOutOfRangeException.Assert(chuteIndex1 is >= Chute.MinChuteIndex and < Chute.MaxChuteIndex);
			ArgumentOutOfRangeException.Assert(chuteIndex2 is >= Chute.MinChuteIndex and < Chute.MaxChuteIndex);
			ArgumentException.Assert(
				chuteIndex1 is >= Chute.MinChuteIndex and < (Chute.MaxChuteIndex >> 1)
					== chuteIndex2 is >= Chute.MinChuteIndex and < (Chute.MaxChuteIndex >> 1)
			);

			if (chuteIndex1 == chuteIndex2)
			{
				return ref @this;
			}

			ref readonly var chuteCells1 = ref Chute.Chutes[chuteIndex1].Cells;
			ref readonly var chuteCells2 = ref Chute.Chutes[chuteIndex2].Cells;
			for (var i = 0; i < 27; i++)
			{
				Unsafe.Swap(ref @this[chuteCells1[i]], ref @this[chuteCells2[i]]);
			}
			return ref @this;
		}
	}
}
