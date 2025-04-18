#undef SYNC_ROOT_VIA_METHODIMPL
#define SYNC_ROOT_VIA_OBJECT
#undef SYNC_ROOT_VIA_THREAD_LOCAL
#if SYNC_ROOT_VIA_METHODIMPL && SYNC_ROOT_VIA_OBJECT && SYNC_ROOT_VIA_THREAD_LOCAL
#line 1 "Grid.cs"
#error You cannot set all three symbols 'SYNC_ROOT_VIA_METHODIMPL', 'SYNC_ROOT_VIA_OBJECT' and 'SYNC_ROOT_VIA_THREAD_LOCAL' - they are designed by the same purpose. You should only define at most one of three symbols.
#line default
#elif SYNC_ROOT_VIA_METHODIMPL && SYNC_ROOT_VIA_OBJECT
#line 1 "Grid.cs"
#error Don't set both symbols 'SYNC_ROOT_VIA_METHODIMPL' and 'SYNC_ROOT_VIA_OBJECT'.
#line default
#elif SYNC_ROOT_VIA_METHODIMPL && SYNC_ROOT_VIA_THREAD_LOCAL
#line 1 "Grid.cs"
#error Don't set both symbols 'SYNC_ROOT_VIA_METHODIMPL' and 'SYNC_ROOT_VIA_THREAD_LOCAL'.
#line default
#elif SYNC_ROOT_VIA_OBJECT && SYNC_ROOT_VIA_THREAD_LOCAL
#line 1 "Grid.cs"
#error Don't set both symbols 'SYNC_ROOT_VIA_OBJECT' and 'SYNC_ROOT_VIA_THREAD_LOCAL'.
#line default
#elif !SYNC_ROOT_VIA_METHODIMPL && !SYNC_ROOT_VIA_OBJECT && !SYNC_ROOT_VIA_THREAD_LOCAL
#line 1 "Grid.cs"
#warning No sync-root mode is selected, meaning we cannot use this type in multi-threading (i.e. this type becomes thread-unsafe) because some members will rely on pointers and shared memory, which is unsafe. You can ONLY use property 'IsValid', 'SolutionGrid' and method 'ExactlyValidate' in this type inside a lock statement.
#line default
#endif

namespace Sudoku.Solving;

/// <summary>
/// Provides with extension methods on <see cref="Grid"/> for solving.
/// </summary>
/// <seealso cref="Grid"/>
public static class GridSolvingExtensions
{
#if SYNC_ROOT_VIA_OBJECT && !SYNC_ROOT_VIA_METHODIMPL
	/// <summary>
	/// The internal field that can be used for making threads run in order while using <see cref="Solver"/>,
	/// keeping the type being thread-safe.
	/// </summary>
	/// <seealso cref="Solver"/>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private static readonly Lock PuzzleSolvingSynchronizer = new();
#endif

	/// <summary>
	/// Indicates the backing solver.
	/// </summary>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[EditorBrowsable(EditorBrowsableState.Never)]
#if SYNC_ROOT_VIA_THREAD_LOCAL
	private static readonly ThreadLocal<BitwiseSolver> Solver = new(static () => new());
#else
	private static readonly BitwiseSolver Solver = new();
#endif


	/// <summary>
	/// Provides extension members on <see langword="in"/> <see cref="Grid"/>.
	/// </summary>
	extension(in Grid @this)
	{
		/// <summary>
		/// Indicates whether the puzzle is valid (solved or a normal puzzle with a unique solution).
		/// </summary>
		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => @this.IsSolved || @this.Uniqueness == Uniqueness.Unique;
		}

		/// <summary>
		/// Checks the uniqueness of the current sudoku puzzle.
		/// </summary>
		public Uniqueness Uniqueness
		{
#if SYNC_ROOT_VIA_METHODIMPL && !SYNC_ROOT_VIA_OBJECT
			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
#endif
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

				var r = @this.ResetGrid;
				long count;
#if SYNC_ROOT_VIA_OBJECT && !SYNC_ROOT_VIA_METHODIMPL
				lock (PuzzleSolvingSynchronizer)
#endif
				{
					count = Solver
#if SYNC_ROOT_VIA_THREAD_LOCAL
						.Value!
#endif
						.SolveString(r.ToString(), out _, 2);
				}

				return count switch { 0 => Uniqueness.Bad, 1 => Uniqueness.Unique, _ => Uniqueness.Multiple };
			}
		}

		/// <summary>
		/// Indicates the solution of the current grid. If the puzzle has no solution or multiple solutions,
		/// this property will return <see cref="Grid.Undefined"/>.
		/// </summary>
		/// <seealso cref="Grid.Undefined"/>
		public Grid SolutionGrid
		{
#if SYNC_ROOT_VIA_METHODIMPL && !SYNC_ROOT_VIA_OBJECT
			[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
#endif
			get
			{
#if SYNC_ROOT_VIA_OBJECT && !SYNC_ROOT_VIA_METHODIMPL
				lock (PuzzleSolvingSynchronizer)
#endif
				{
					return Solver
#if SYNC_ROOT_VIA_THREAD_LOCAL
						.Value!
#endif
						.Solve(in @this) is { IsUndefined: false } solution ? SolutionGrid_Unfix(solution, @this.GivenCells) : Grid.Undefined;
				}
			}
		}


		private static Grid SolutionGrid_Unfix(in Grid solution, in CellMap pattern)
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
