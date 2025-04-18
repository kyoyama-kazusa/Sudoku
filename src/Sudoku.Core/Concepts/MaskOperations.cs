namespace Sudoku.Concepts;

using GridBase = IGrid<Grid>;

/// <summary>
/// Provides with a set of methods that operates with mask defined in basic sudoku concepts, as data structures.
/// </summary>
public static class MaskOperations
{
	/// <summary>
	/// Try to convert the current mask value into a valid <see cref="string"/> representation of binary format.
	/// </summary>
	/// <param name="mask">The mask to be formatted.</param>
	/// <param name="upperCasedPrefix">
	/// Indicates whether the prefix <c>"0b"</c> will become upper-cased (i.e. <c>"0B"</c>).
	/// The default value is <see langword="false"/>.
	/// </param>
	/// <returns>A <see cref="string"/> result representing the current mask value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToBinaryString(Mask mask, bool upperCasedPrefix = false)
		=> $"0{(upperCasedPrefix ? 'B' : 'b')}{Convert.ToString(mask, 2).PadLeft(9, '0')}";

	/// <summary>
	/// Try to convert the current mask value into a valid <see cref="string"/> representation of octal format.
	/// </summary>
	/// <param name="mask">The mask to be formatted.</param>
	/// <param name="upperCasedPrefix">
	/// Indicates whether the prefix <c>"0o"</c> will become upper-cased (i.e. <c>"0O"</c>).
	/// The default value is <see langword="false"/>.
	/// </param>
	/// <returns>A <see cref="string"/> result representing the current mask value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToOctalString(Mask mask, bool upperCasedPrefix = false)
		=> $"0{(upperCasedPrefix ? 'O' : 'o')}{Convert.ToString(mask, 8).PadLeft(3, '0')}";

	/// <summary>
	/// Try to convert the current mask value into a valid <see cref="string"/> representation of hexadecimal format.
	/// </summary>
	/// <param name="mask">The mask to be formatted.</param>
	/// <param name="upperCasedPrefix">
	/// Indicates whether the prefix <c>"0x"</c> will become upper-cased (i.e. <c>"0X"</c>).
	/// The default value is <see langword="false"/>.
	/// </param>
	/// <returns>A <see cref="string"/> result representing the current mask value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToHexadecimalString(Mask mask, bool upperCasedPrefix = false)
		=> $"0{(upperCasedPrefix ? 'X' : 'x')}{Convert.ToString(mask, 16).PadLeft(3, '0')}";

	/// <summary>
	/// Creates for a <see cref="Mask"/> instance via the specified digits.
	/// </summary>
	/// <param name="digits">
	/// <para>Indicates the digits to assign.</para>
	/// <include file="../../global-doc-comments.xml" path="//g/csharp12/feature[@name='params-collections']/target[@name='parameter']"/>
	/// </param>
	/// <returns>A <see cref="Mask"/> instance.</returns>
	public static Mask Create(params ReadOnlySpan<Digit> digits)
	{
		var result = (Mask)0;
		foreach (var digit in digits)
		{
			result |= (Mask)(1 << digit);
		}
		return result;
	}

	/// <inheritdoc cref="Create(ReadOnlySpan{Digit})"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Mask Create(Digit[] digits) => Create(digits.AsReadOnlySpan());

	/// <typeparam name="TDigits">The type of the enumerable sequence.</typeparam>
	/// <inheritdoc cref="Create(ReadOnlySpan{Digit})"/>
	public static Mask Create<TDigits>(TDigits digits) where TDigits : IEnumerable<Digit>, allows ref struct
	{
		var result = (Mask)0;
		foreach (var digit in digits)
		{
			result |= (Mask)(1 << digit);
		}
		return result;
	}

	/// <summary>
	/// To get the digits that the current mask represents for. The mask must be between 0 and 512, and exclude 512.
	/// </summary>
	/// <param name="digitMask">The digit mask.</param>
	/// <returns>The digits returned.</returns>
	public static ReadOnlySpan<Digit> MaskToDigits(Mask digitMask)
	{
		var result = new Digit[BitOperations.PopCount(digitMask)];
		for (var (i, p) = (0, 0); i < 9; i++)
		{
			if ((digitMask >> i & 1) != 0)
			{
				result[p++] = i;
			}
		}
		return result;
	}

	/// <summary>
	/// To get the sudoku type for the specified cell mask inside a <see cref="Grid"/>.
	/// </summary>
	/// <param name="mask">The cell mask.</param>
	/// <returns>The sudoku type configured.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SudokuType MaskToSudokuType(Mask mask)
		=> (mask >> GridBase.HeaderShift << GridBase.HeaderShift) switch
		{
			var resultMask and not 0 => (SudokuType)resultMask,
			_ => SudokuType.Standard
		};

	/// <summary>
	/// To get the cell state for a mask value. The mask is an inner representation to describe a cell's state.
	/// For more information please visit the details of the design for type <see cref="Grid"/>.
	/// </summary>
	/// <param name="mask">The mask.</param>
	/// <returns>The cell state.</returns>
	/// <seealso cref="Grid"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellState MaskToCellState(Mask mask) => (CellState)(mask >> 9 & 7);

	/// <summary>
	/// Try to split a mask into 3 parts, 3-bit as a unit.
	/// </summary>
	/// <param name="this">The mask instance to be split.</param>
	/// <returns>A triplet of values.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (int High, int Mid, int Low) SplitMask(this Mask @this) => (@this >> 6 & 7, @this >> 3 & 7, @this & 7);
}
