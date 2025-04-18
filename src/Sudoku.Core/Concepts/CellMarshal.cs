namespace Sudoku.Concepts;

/// <summary>
/// Represents a list of extension methods that operates with cell instances, as <see cref="Cell"/> representation.
/// </summary>
/// <seealso cref="Cell"/>
public static class CellMarshal
{
	/// <summary>
	/// Gets the row, column and block value and copies to the specified array that represents by a pointer
	/// of 3 elements, where the first element stores the block index, second element stores the row index
	/// and the third element stores the column index.
	/// </summary>
	/// <param name="this">The cell. The available values must be between 0 and 80.</param>
	/// <param name="reference">
	/// The specified reference to the first element in a sequence. The sequence type can be an array or a <see cref="Span{T}"/>,
	/// only if the sequence can store at least 3 values.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// Throws when the argument <paramref name="reference"/> references to <see langword="null"/>.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyHouseInfo(this Cell @this, ref House reference)
	{
		reference = BlockTable[@this];
		Unsafe.Add(ref reference, 1) = RowTable[@this];
		Unsafe.Add(ref reference, 2) = ColumnTable[@this];
	}

	/// <summary>
	/// Get the house index (0..27 for block 1-9, row 1-9 and column 1-9)
	/// for the specified cell and the house type.
	/// </summary>
	/// <param name="this">The cell. The available values must be between 0 and 80.</param>
	/// <param name="houseType">The house type.</param>
	/// <returns>The house index. The return value must be between 0 and 26.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="houseType"/> is not defined.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static House ToHouse(this byte @this, HouseType houseType)
		=> houseType switch
		{
			HouseType.Block => BlockTable[@this],
			HouseType.Row => RowTable[@this],
			HouseType.Column => ColumnTable[@this],
			_ => throw new ArgumentOutOfRangeException(nameof(houseType))
		};

	/// <inheritdoc cref="ToHouse(byte, HouseType)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static House ToHouse(this Cell cell, HouseType houseType)
		=> houseType switch
		{
			HouseType.Block => BlockTable[cell],
			HouseType.Row => RowTable[cell],
			HouseType.Column => ColumnTable[cell],
			_ => throw new ArgumentOutOfRangeException(nameof(houseType))
		};

	/// <summary>
	/// Get the house indices for the specified cell.
	/// </summary>
	/// <param name="this">The cell.</param>
	/// <returns>A <see cref="HouseMask"/> result.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static HouseMask ToHouseIndices(this Cell @this)
	{
		var result = 0;
		result |= 1 << @this.ToHouse(HouseType.Block);
		result |= 1 << @this.ToHouse(HouseType.Row);
		result |= 1 << @this.ToHouse(HouseType.Column);
		return result;
	}

	/// <summary>
	/// Try to get the band index (mega-row) of the specified cell.
	/// </summary>
	/// <param name="this">The cell.</param>
	/// <returns>The chute index.</returns>
	public static int ToBand(this Cell @this)
	{
		for (var i = 0; i < 3; i++)
		{
			if (Chutes[i].Cells.Contains(@this))
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// Try to get the tower index (mega-column) of the specified cell.
	/// </summary>
	/// <param name="this">The cell.</param>
	/// <returns>The chute index.</returns>
	public static int ToTower(this Cell @this)
	{
		for (var i = 3; i < 6; i++)
		{
			if (Chutes[i].Cells.Contains(@this))
			{
				return i;
			}
		}
		return -1;
	}
}
