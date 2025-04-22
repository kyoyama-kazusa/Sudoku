namespace Sudoku.Shuffling.Transforming;

/// <summary>
/// Provides with extension methods for <see cref="Random"/>.
/// </summary>
/// <seealso cref="Random"/>
public static class RandomizationExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Random"/>.
	/// </summary>
	extension(Random random)
	{
		/// <summary>
		/// Returns a random integer that is within valid digit range (0..9).
		/// </summary>
		/// <returns>
		/// An integer that represents a valid <see cref="Digit"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Digit NextDigit() => random.Next(0, 9);

		/// <summary>
		/// Returns a random integer that is within valid cell range (0..81).
		/// </summary>
		/// <returns>
		/// An integer that represents a valid <see cref="Cell"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Cell NextCell() => random.Next(0, 81);

		/// <summary>
		/// Returns a random integer that is within valid house range (0..27).
		/// </summary>
		/// <returns>
		/// An integer that represents a valid <see cref="House"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public House NextHouse() => random.Next(0, 27);

		/// <summary>
		/// Randomly select the specified number of elements from the current collection.
		/// </summary>
		/// <param name="cells">The cells to be chosen.</param>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>The specified number of elements returned, represented as a <see cref="CellMap"/> instance.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CellMap RandomlySelect(in CellMap cells, int count)
		{
			var result = cells.Offsets[..];
			random.Shuffle(result);
			return [.. result[..count]];
		}

		/// <summary>
		/// Randomly select the specified number of elements from the current collection.
		/// </summary>
		/// <param name="cells">The cells to be chosen.</param>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>The specified number of elements returned, represented as a <see cref="CandidateMap"/> instance.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CandidateMap RandomlySelect(in CandidateMap cells, int count)
		{
			var result = cells.Offsets[..];
			random.Shuffle(result);
			return [.. result[..count]];
		}

		/// <summary>
		/// Creates a <see cref="CellMap"/> instance, with the specified number of <see cref="Cell"/>s stored in the collection.
		/// </summary>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>A <see cref="CellMap"/> instance.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CellMap CreateCellMap(int count) => random.RandomlySelect(CellMap.Full, count);

		/// <summary>
		/// Creates a <see cref="CandidateMap"/> instance, with the specified number of <see cref="Candidate"/>s stored in the collection.
		/// </summary>
		/// <param name="count">The desired number of elements.</param>
		/// <returns>A <see cref="CandidateMap"/> instance.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CandidateMap CreateCandidateMap(int count) => random.RandomlySelect(CandidateMap.Full, count);
	}
}
