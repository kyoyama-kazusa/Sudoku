namespace Sudoku.Solving.Dlx;

/// <summary>
/// Defines a solver that uses the dancing links algorithm.
/// </summary>
public sealed class DancingLinksSolver : ISolver, ISolutionEnumerableSolver
{
	/// <summary>
	/// Indicates the stack that stores the raw data for the solutions.
	/// </summary>
	private readonly Stack<DancingLinkNode> _answerNodesStack = [];


	/// <summary>
	/// indicates the number of all found solutions.
	/// </summary>
	private int _solutionCount;

	/// <summary>
	/// Indicates the found solution.
	/// </summary>
	private Grid _solution;

	/// <summary>
	/// Indicates the root node of the full link map.
	/// </summary>
	private ColumnNode? _root;


	/// <inheritdoc/>
	public string UriLink => "https://en.wikipedia.org/wiki/Dancing_Links";


	/// <inheritdoc/>
	public event EventHandler<SolverSolutionFoundEventArgs>? SolutionFound;


	/// <inheritdoc/>
	public unsafe bool? Solve(in Grid grid, out Grid result)
	{
		try
		{
			_root = DancingLink.Entry.Create(grid);
			Search(&g, &r);

			(result, var @return) = _solutionCount == 0 ? (Grid.Undefined, (bool?)null) : (_solution, true);
			return @return;
		}
		catch (MultipleSolutionException)
		{
			result = Grid.Undefined;
			return false;
		}


		[DoesNotReturn]
		static void g() => throw new MultipleSolutionException();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool r(DancingLinksSolver @this, Stack<DancingLinkNode> resultNodes)
		{
			@this._solution = ToGrid(resultNodes);
			return true;
		}
	}

	/// <inheritdoc cref="Solve(in Grid, out Grid)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool? Solve(Digit[] grid, out Grid result) => Solve(Grid.Create(grid), out result);

	/// <inheritdoc/>
	unsafe void ISolutionEnumerableSolver.EnumerateSolutionsCore(Grid grid, CancellationToken cancellationToken)
	{
		_root = DancingLink.Entry.Create(grid);
		Search(&@delegate.DoNothing, &r);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool r(DancingLinksSolver @this, Stack<DancingLinkNode> resultNodes)
		{
			var newFoundGrid = ToGrid(resultNodes);
			@this.SolutionFound?.Invoke(@this, new(newFoundGrid));
			return true;
		}
	}

	/// <summary>
	/// Try to search the full dancing link map and get the possible solution.
	/// </summary>
	/// <param name="multipleSolutionGuard">A method that guards the case that multiple solutions (at least 2) are found.</param>
	/// <param name="resultTargeting">A method that assigns or consume the result raw value.</param>
	/// <exception cref="InvalidOperationException">Throws when the puzzle has multiple solutions.</exception>
	private unsafe void Search(
		delegate*<void> multipleSolutionGuard,
		delegate*<DancingLinksSolver, Stack<DancingLinkNode>, bool> resultTargeting
	)
	{
		if (_solutionCount > 1)
		{
			multipleSolutionGuard();
		}

		Debug.Assert(_root is not null);
		if (ReferenceEquals(_root.Right, _root))
		{
			// All columns were removed!
			if (resultTargeting(this, _answerNodesStack))
			{
				_solutionCount++;
			}
			return;
		}

		var c = ChooseNextColumn();
		Cover(c);

		for (var r = c.Down; !ReferenceEquals(r, c); r = r.Down)
		{
			_answerNodesStack.Push(r);
			for (var j = r.Right; !ReferenceEquals(j, r); j = j.Right)
			{
				Cover(j.Column!);
			}

			Search(multipleSolutionGuard, resultTargeting);

			r = _answerNodesStack.Pop();
			c = r.Column;
			for (var j = r.Left; !ReferenceEquals(j, r); j = j.Left)
			{
				Uncover(j.Column!);
			}
		}

		Uncover(c);
	}

	/// <summary>
	/// Cover the nodes for the specified column.
	/// </summary>
	/// <param name="column">The column.</param>
	private void Cover(DancingLinkNode column)
	{
		column.Right.Left = column.Left;
		column.Left.Right = column.Right;
		for (var i = column.Down; !ReferenceEquals(i, column); i = i.Down)
		{
			for (var j = i.Right; !ReferenceEquals(j, i); j = j.Right)
			{
				j.Down.Up = j.Up;
				j.Up.Down = j.Down;
				j.Column!.Size--;
			}
		}
	}

	/// <summary>
	/// Uncover the nodes for the specified column.
	/// </summary>
	/// <param name="column">The column.</param>
	private void Uncover(DancingLinkNode column)
	{
		for (var i = column.Up; !ReferenceEquals(i, column); i = i.Up)
		{
			for (var j = i.Left; !ReferenceEquals(j, i); j = j.Left)
			{
				j.Column!.Size++;
				j.Down.Up = j;
				j.Up.Down = j;
			}
		}
		column.Right.Left = column;
		column.Left.Right = column;
	}

	/// <summary>
	/// Try to choose the next column node.
	/// </summary>
	/// <returns>The chosen next column node.</returns>
	private ColumnNode ChooseNextColumn()
	{
		Debug.Assert(_root is not null);
		var (size, nextColumn, j) = (int.MaxValue, new ColumnNode(-1), _root.Right.Column);
		while (!ReferenceEquals(j, _root))
		{
			if (j!.Size < size)
			{
				nextColumn = j;
				size = j.Size;
			}
			j = j.Right.Column;
		}
		return nextColumn;
	}


	/// <summary>
	/// Converts a list of <see cref="DancingLinkNode"/> as solution.
	/// </summary>
	/// <param name="answer">The solution nodes.</param>
	/// <returns>A grid converted.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Grid ToGrid(Stack<DancingLinkNode> answer)
		=> Grid.Create(from id in (from k in answer orderby k.Candidate select k.Candidate).ToArray() select id % 9);
}
