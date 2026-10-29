namespace Sudoku.MinlexOrder;

/// <summary>
/// Represents a pattern for a sudoku grid.
/// </summary>
internal struct GridPattern
{
	/// <summary>
	/// A precomputed min-lexicographical-ordered recompositing of the bit triplets for a 9-bits input.
	/// </summary>
	internal static readonly Mask[] MinCanNineBits = [
		0, 1, 1, 3, 1, 3, 3, 7, 1, 9, 9, 11, 9, 11, 11, 15, 1, 9, 9, 11, 9, 11, 11, 15, 3, 11, 11, 27, 11,
		27, 27, 31, 1, 9, 9, 11, 9, 11, 11, 15, 3, 11, 11, 27, 11, 27, 27, 31, 3, 11, 11, 27, 11, 27, 27,
		31, 7, 15, 15, 31, 15, 31, 31, 63, 1, 9, 9, 11, 9, 11, 11, 15, 9, 73, 73, 75, 73, 75, 75, 79, 9,
		73, 73, 75, 73, 75, 75, 79, 11, 75, 75, 91, 75, 91, 91, 95, 9, 73, 73, 75, 73, 75, 75, 79, 11, 75,
		75, 91, 75, 91, 91, 95, 11, 75, 75, 91, 75, 91, 91, 95, 15, 79, 79, 95, 79, 95, 95, 127, 1, 9, 9,
		11, 9, 11, 11, 15, 9, 73, 73, 75, 73, 75, 75, 79, 9, 73, 73, 75, 73, 75, 75, 79, 11, 75, 75, 91,
		75, 91, 91, 95, 9, 73, 73, 75, 73, 75, 75, 79, 11, 75, 75, 91, 75, 91, 91, 95, 11, 75, 75, 91, 75,
		91, 91, 95, 15, 79, 79, 95, 79, 95, 95, 127, 3, 11, 11, 27, 11, 27, 27, 31, 11, 75, 75, 91, 75, 91,
		91, 95, 11, 75, 75, 91, 75, 91, 91, 95, 27, 91, 91, 219, 91, 219, 219, 223, 11, 75, 75, 91, 75, 91,
		91, 95, 27, 91, 91, 219, 91, 219, 219, 223, 27, 91, 91, 219, 91, 219, 219, 223, 31, 95, 95, 223, 95,
		223, 223, 255, 1, 9, 9, 11, 9, 11, 11, 15, 9, 73, 73, 75, 73, 75, 75, 79, 9, 73, 73, 75, 73, 75, 75,
		79, 11, 75, 75, 91, 75, 91, 91, 95, 9, 73, 73, 75, 73, 75, 75, 79, 11, 75, 75, 91, 75, 91, 91, 95,
		11, 75, 75, 91, 75, 91, 91, 95, 15, 79, 79, 95, 79, 95, 95, 127, 3, 11, 11, 27, 11, 27, 27, 31, 11,
		75, 75, 91, 75, 91, 91, 95, 11, 75, 75, 91, 75, 91, 91, 95, 27, 91, 91, 219, 91, 219, 219, 223, 11,
		75, 75, 91, 75, 91, 91, 95, 27, 91, 91, 219, 91, 219, 219, 223, 27, 91, 91, 219, 91, 219, 219, 223,
		31, 95, 95, 223, 95, 223, 223, 255, 3, 11, 11, 27, 11, 27, 27, 31, 11, 75, 75, 91, 75, 91, 91, 95,
		11, 75, 75, 91, 75, 91, 91, 95, 27, 91, 91, 219, 91, 219, 219, 223, 11, 75, 75, 91, 75, 91, 91, 95,
		27, 91, 91, 219, 91, 219, 219, 223, 27, 91, 91, 219, 91, 219, 219, 223, 31, 95, 95, 223, 95, 223,
		223, 255, 7, 15, 15, 31, 15, 31, 31, 63, 15, 79, 79, 95, 79, 95, 95, 127, 15, 79, 79, 95, 79, 95,
		95, 127, 31, 95, 95, 223, 95, 223, 223, 255, 15, 79, 79, 95, 79, 95, 95, 127, 31, 95, 95, 223, 95,
		223, 223, 255, 31, 95, 95, 223, 95, 223, 223, 255, 63, 127, 127, 255, 127, 255, 255, 511
	];


	/// <summary>
	/// Indicates the rows.
	/// </summary>
	public safe fixed int Rows[9];

	/// <summary>
	/// Indicates the digit.
	/// </summary>
	public safe fixed int Digits[81];


	/// <inheritdoc cref="object.ToString"/>
	public readonly override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append('[');
		for (var i = 0; i < 9; i++)
		{
			sb.Append(unsafe (Rows[i]));
			sb.Append(',');
		}
		sb.Append("]...");
		return sb.ToString();
	}

	/// <summary>
	/// Try to calculate the best top row score from the specified <see cref="GridPattern"/> instance.
	/// </summary>
	/// <param name="p">The grid pattern instance.</param>
	/// <returns>The score value calculated.</returns>
	public static int BestTopRowScore(in GridPattern p)
	{
		// Returns the smallest row after canonicalization of each row independently.
		Mask x;
		var amin = MinCanNineBits[unsafe (p.Rows[0])];
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[1])])) { amin = x; }
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[2])])) { amin = x; }
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[3])])) { amin = x; }
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[4])])) { amin = x; }
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[5])])) { amin = x; }
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[6])])) { amin = x; }
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[7])])) { amin = x; }
		if (amin > (x = MinCanNineBits[unsafe (p.Rows[8])])) { amin = x; }
		return amin;
	}

	/// <summary>
	/// Initializes for normal and transposed <see cref="GridPattern"/> instances from a string grid code.
	/// </summary>
	/// <param name="grid">Indicates the grid code to be used.</param>
	/// <param name="resultPair">Indicates the 2 <see cref="GridPattern"/> instances for normal and transposed cases.</param>
	/// <returns>An <see cref="int"/> value indicating the number of given cells.</returns>
	public static int FromString(string grid, out ReadOnlySpan<GridPattern> resultPair)
	{
		var pair = new GridPattern[2];
		ref var normal = ref pair[0];
		ref var transposed = ref pair[1];

		var src = 0; // Pointer to a character in the given text.
		var result = 0;
		unsafe
		{
			transposed.Rows[0] = 0;
			transposed.Rows[1] = 0;
			transposed.Rows[2] = 0;
			transposed.Rows[3] = 0;
			transposed.Rows[4] = 0;
			transposed.Rows[5] = 0;
			transposed.Rows[6] = 0;
			transposed.Rows[7] = 0;
			transposed.Rows[8] = 0;
		}

		for (var row = 0; row < 9; row++)
		{
			var r = 0;
			for (var col = 0; col < 9; col++)
			{
				var c = grid[src]; // Read the character src points to.
				if (c is >= '1' and <= '9')
				{
					result++;
					r |= 1 << (8 - col); // Most significant bit is for c0, less significant for c8.
					unsafe (transposed.Rows[col]) |= 1 << (8 - row);
					unsafe (normal.Digits[row * 9 + col]) = unsafe (transposed.Digits[col * 9 + row]) = c - '0';
				}
				else
				{
					unsafe (normal.Digits[row * 9 + col]) = unsafe (transposed.Digits[col * 9 + row]) = 0;
				}

				src++; // Move to next char.
			}

			unsafe (normal.Rows[row]) = r;
		}

		resultPair = pair;
		return result;
	}
}
