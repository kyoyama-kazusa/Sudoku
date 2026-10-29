namespace Sudoku.Solving;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/> for solving.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridSolvingExtensions
{
	/// <summary>
	/// The internal field that can be used for making threads run in order while using <see cref="Solver"/>,
	/// keeping the type being thread-safe.
	/// </summary>
	/// <seealso cref="Solver"/>
	private static readonly Lock PuzzleSolvingSyncRoot = new();

	/// <summary>
	/// Indicates the backing solver.
	/// </summary>
	private static readonly BitwiseSolver Solver = new();


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(in Grid @this)
	{
		/// <summary>
		/// Indicates whether the puzzle is valid (solved or a normal puzzle with a unique solution).
		/// </summary>
		public bool IsValid => @this.IsSolved || @this.Uniqueness == Uniqueness.Unique;

		/// <summary>
		/// Checks the uniqueness of the current sudoku puzzle.
		/// </summary>
		public Uniqueness Uniqueness
		{
			get
			{
				if (@this.IsSolved)
				{
					// Special case: If a puzzle has already been solved, return 'Uniqueness.Unique' directly
					// because it had been checked by 'Grid.IsSolved' property.
					return Uniqueness.Unique;
				}

				if (@this.HasCellHavingNoCandidates)
				{
					// Special case: If a puzzle has at least one cell having no candidates, the grid will always invalid.
					return Uniqueness.Bad;
				}

				lock (PuzzleSolvingSyncRoot)
				{
					return unsafe (Solver.SolveString(@this.ResetGrid.ToString(), null, 2)) switch
					{
						0 => Uniqueness.Bad,
						1 => Uniqueness.Unique,
						_ => Uniqueness.Multiple
					};
				}
			}
		}

		/// <summary>
		/// Indicates the solution of the current grid. If the puzzle has no solution or multiple solutions,
		/// this property will return <see cref="Grid.Undefined"/>.
		/// </summary>
		/// <seealso cref="Grid.Undefined"/>
		public Grid SolutionGrid
		{
			get
			{
				lock (PuzzleSolvingSyncRoot)
				{
					return Solver.Solve(@this) is { IsUndefined: false } solution
						? unfix(solution, @this.GivenCells)
						: Grid.Undefined;
				}


				static Grid unfix(in Grid solution, in CellMap pattern)
				{
					var result = solution;
					foreach (var cell in ~pattern)
					{
						if (result.GetState(cell) == CellState.Given)
						{
							result.SetState(cell, CellState.Modifiable);
						}
					}
					return result;
				}
			}
		}
	}
}
