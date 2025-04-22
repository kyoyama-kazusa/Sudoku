namespace Sudoku.Solving;

/// <summary>
/// Provides with extension methods on <see cref="SolverType"/>.
/// </summary>
/// <seealso cref="SolverType"/>
public static class SolverTypeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="SolverType"/>.
	/// </summary>
	extension(SolverType)
	{
		/// <summary>
		/// Creates a <see cref="ISolver"/> instance via the specified type.
		/// </summary>
		/// <param name="solverType">The type.</param>
		/// <returns>The instance.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Throws when the argument is not defined.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ISolver CreateSolver(SolverType solverType)
			=> solverType switch
			{
				SolverType.Backtracking => new BacktrackingSolver(),
				SolverType.BfsBacktracking => new BfsBacktrackingSolver(),
				SolverType.DfsBacktracking => new DfsBacktrackingSolver(),
				SolverType.Bitwise => new BitwiseSolver(),
				SolverType.DancingLinks => new DancingLinksSolver(),
				SolverType.DictionaryQuery => new DictionaryQuerySolver(),
				SolverType.EnumerableQuery => new EnumerableQuerySolver(),
				_ => throw new ArgumentOutOfRangeException(nameof(solverType))
			};
	}
}
