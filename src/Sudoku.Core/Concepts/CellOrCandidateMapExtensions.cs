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
		/// Converts the specified <see cref="Cell"/> into a singleton <see cref="CellMap"/> instance.
		/// </summary>
		/// <returns>A <see cref="CellMap"/> instance, containing only one element of the current cell.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly CellMap AsCellMap() => ref CellMaps[@this];
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
