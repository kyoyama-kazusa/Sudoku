namespace Sudoku.Concepts;

/// <summary>
/// Provides with extension methods on <see cref="CellMap"/> or <see cref="CandidateMap"/>.
/// </summary>
public static class CellOrCandidateMapExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Cell"/>.
	/// </summary>
	extension(Cell @this)
	{
		/// <summary>
		/// Try to get the band index (mega-row) of the specified cell.
		/// </summary>
		public int Band
		{
			get
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
		}

		/// <summary>
		/// Try to get the tower index (mega-column) of the specified cell.
		/// </summary>
		/// <returns>The chute index.</returns>
		public int Tower
		{
			get
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

		/// <summary>
		/// Get the houses for the specified cell, representing as a <see cref="HouseMask"/> instance.
		/// </summary>
		public HouseMask Houses
		{
			get
			{
				var result = 0;
				result |= 1 << @this.ToHouse(HouseType.Block);
				result |= 1 << @this.ToHouse(HouseType.Row);
				result |= 1 << @this.ToHouse(HouseType.Column);
				return result;
			}
		}


		/// <summary>
		/// Gets the row, column and block value and copies to the specified array that represents by a pointer
		/// of 3 elements, where the first element stores the block index, second element stores the row index
		/// and the third element stores the column index.
		/// </summary>
		/// <param name="reference">
		/// The specified reference to the first element in a sequence. The sequence type can be an array or a <see cref="Span{T}"/>,
		/// only if the sequence can store at least 3 values.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Throws when the argument <paramref name="reference"/> references to <see langword="null"/>.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyHouseInfo(ref House reference)
		{
			reference = BlockTable[@this];
			Unsafe.Add(ref reference, 1) = RowTable[@this];
			Unsafe.Add(ref reference, 2) = ColumnTable[@this];
		}

		/// <summary>
		/// Converts the specified <see cref="Cell"/> into a singleton <see cref="CellMap"/> instance.
		/// </summary>
		/// <returns>A <see cref="CellMap"/> instance, containing only one element of the current cell.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly CellMap AsCellMap() => ref CellMaps[@this];

		/// <summary>
		/// Get the house index (0..27 for block 1-9, row 1-9 and column 1-9) for the specified cell and the house type.
		/// </summary>
		/// <param name="houseType">The house type.</param>
		/// <returns>The house index. The return value must be between 0 and 26.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Throws when the argument <paramref name="houseType"/> is not defined.
		/// </exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public House ToHouse(HouseType houseType)
			=> houseType switch
			{
				HouseType.Block => BlockTable[@this],
				HouseType.Row => RowTable[@this],
				HouseType.Column => ColumnTable[@this],
				_ => throw new ArgumentOutOfRangeException(nameof(houseType))
			};
	}

	/// <summary>
	/// Provides extension members on <see cref="Cell"/>[].
	/// </summary>
	extension(Cell[] @this)
	{
		/// <inheritdoc cref="AsCellMap(ReadOnlySpan{Cell})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CellMap AsCellMap() => [.. @this];
	}

	/// <summary>
	/// Provides extension members on <see cref="Candidate"/>[].
	/// </summary>
	extension(Candidate[] @this)
	{
		/// <inheritdoc cref="AsCandidateMap(ReadOnlySpan{Candidate})"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CandidateMap AsCandidateMap() => [.. @this];
	}

	/// <summary>
	/// Provides extension members on <see cref="Candidate"/>.
	/// </summary>
	extension(Candidate @this)
	{
		/// <summary>
		/// Converts the specified <see cref="Candidate"/> into a singleton <see cref="CandidateMap"/> instance.
		/// </summary>
		/// <returns>A <see cref="CandidateMap"/> instance, containing only one element of the current candidate.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#if CACHE_CANDIDATE_MAPS
		public ref readonly CandidateMap AsCandidateMap() => ref CandidateMaps[@this];
#else
		public CandidateMap AsCandidateMap() => [@this];
#endif
	}

	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <see cref="Cell"/>.
	/// </summary>
	extension(ReadOnlySpan<Cell> @this)
	{
		/// <summary>
		/// Converts the specified list of <see cref="Cell"/> instances into a <see cref="CellMap"/> instance.
		/// </summary>
		/// <returns>A <see cref="CellMap"/> instance, containing all elements come from the current sequence.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CellMap AsCellMap() => [.. @this];
	}

	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <see cref="Candidate"/>.
	/// </summary>
	extension(ReadOnlySpan<Candidate> @this)
	{
		/// <summary>
		/// Converts the specified list of <see cref="Candidate"/> instances into a <see cref="CandidateMap"/> instance.
		/// </summary>
		/// <returns>A <see cref="CandidateMap"/> instance, containing all elements come from the current sequence.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CandidateMap AsCandidateMap() => [.. @this];
	}
}
