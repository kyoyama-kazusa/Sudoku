namespace Sudoku.Analytics.Dependency.Contradictions;

/// <summary>
/// Represents a type that can find for contradiction that will be used in complex patterns.
/// </summary>
public static class ContradictionDetector
{
	/// <summary>
	/// Do try and error logic (T&amp;E) and find for all candidates that causes contradiction.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="includesGroupedNodes">Indicates whether the searching method will includes grouped nodes.</param>
	/// <returns>All candidates making contradictions.</returns>
	public static ReadOnlySpan<Candidate> TryAndError(in Grid grid, bool includesGroupedNodes)
	{
		var result = new List<Candidate>();
		for (var cell = 0; cell < 81; cell++)
		{
			if (grid.GetState(cell) != CellState.Empty)
			{
				continue;
			}

			foreach (var digit in grid.GetCandidates(cell))
			{
				if (tryAndError(grid, cell, digit, includesGroupedNodes))
				{
					result.Add(cell * 9 + digit);
				}
			}
		}
		return result.AsSpan();


		static bool tryAndError(in Grid grid, Cell cell, Digit digit, bool includesGroupedNodes)
		{
			// Fast check. Just assign it and find for conclusions.
			var firstAssignment = new DependencyAssignment(cell * 9 + digit);
			var tempGrid = grid;
			Update(ref tempGrid, firstAssignment);

			bool isChanged;
			do
			{
				isChanged = false;

				if (tryFindConflict(tempGrid))
				{
					return true;
				}

				// Find for the next conclusion.
				var collector = new HashSet<DependencyAssignment>();

				// Collect for valid next steps.
				FindHiddenSingle(tempGrid, collector);
				FindNakedSingle(tempGrid, collector);
				if (includesGroupedNodes)
				{
					FindLockedCandidates(tempGrid, collector);
				}

				if (collector.Count != 0)
				{
					isChanged = true;
					foreach (var assignment in collector)
					{
						Update(ref tempGrid, assignment);
					}
				}
			} while (isChanged);

			// No contradictions found.
			return false;
		}

		static bool tryFindConflict(in Grid grid)
		{
			// Check cell.
			foreach (var cell in grid.EmptyCells)
			{
				if (grid.GetCandidates(cell) == 0)
				{
					return true;
				}
			}

			// Check house.
			for (var digit = 0; digit < 9; digit++)
			{
			loop_house:
				for (var house = 0; house < 27; house++)
				{
					var count = 0;
					foreach (var cell in HousesMap[house])
					{
						var cellState = grid.GetState(cell);
						if (grid.GetDigit(cell) == digit)
						{
							continue loop_house;
						}
						if (grid.Exists(cell, digit) is true)
						{
							count++;
						}
					}
					if (count == 0)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Create a new grid that applies the specified assignment.
	/// </summary>
	/// <param name="grid">The grid to be updated.</param>
	/// <param name="assignment">The assignment.</param>
	private static void Update(scoped ref Grid grid, DependencyAssignment assignment)
	{
		_ = assignment is (var digit, { PeerIntersection: var peerCells } cells);
		foreach (var peerCell in peerCells)
		{
			ref var mask = ref grid[peerCell];
			if (MaskToCellState(mask) == CellState.Empty && (mask >> digit & 1) != 0)
			{
				mask &= (Mask)~(1 << digit);
			}
		}

		// Set digit for the current cell.
		if (cells is [var cell])
		{
			// Updates mask of the target cell.
			grid[cell] = (Mask)(GetHeaderBits(in grid, cell) | Grid.ModifiableMask | 1 << digit);
		}
	}

	/// <summary>
	/// Find for hidden singles, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="result">The result.</param>
	private static void FindHiddenSingle(in Grid grid, HashSet<DependencyAssignment> result)
	{
		// Iterate on each digit.
		for (var digit = 0; digit < 9; digit++)
		{
			// Iterate on each house.
		loop_house:
			for (var house = 0; house < 27; house++)
			{
				var count = 0;
				var firstFoundCell = -1;
				foreach (var cell in HousesMap[house])
				{
					var cellState = grid.GetState(cell);
					if (grid.GetDigit(cell) == digit)
					{
						continue loop_house;
					}
					if (grid.Exists(cell, digit) is true)
					{
						firstFoundCell = cell;
						if (++count >= 2)
						{
							break;
						}
					}
				}

				if (count == 1)
				{
					result.Add(new(firstFoundCell * 9 + digit));
				}
			}
		}
	}

	/// <summary>
	/// Find for naked singles, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="result">The result.</param>
	private static void FindNakedSingle(in Grid grid, HashSet<DependencyAssignment> result)
	{
		foreach (var cell in grid.EmptyCells)
		{
			var mask = grid.GetCandidates(cell);
			if (BitOperations.IsPow2((uint)mask))
			{
				result.Add(new(cell * 9 + BitOperations.Log2((uint)mask)));
			}
		}
	}

	/// <summary>
	/// Find for locked candidates, and collect them into <paramref name="result"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="result">The result.</param>
	private static void FindLockedCandidates(in Grid grid, HashSet<DependencyAssignment> result)
	{
		var emptyCells = grid.EmptyCells;
		var candidatesMap = grid.CandidatesMap;
		foreach (var ((baseSet, coverSet), (a, b, c, _)) in Segments.Map)
		{
			// Check whether the locked candidates pattern can be formed or not.
			if (!LockedCandidates.IsLockedCandidates(grid, a, b, c, emptyCells, out var digitsMaskFormingLockedCandidates))
			{
				continue;
			}

			// Now iterate on the mask to get all digits.
			foreach (var digit in digitsMaskFormingLockedCandidates)
			{
				ref readonly var map = ref candidatesMap[digit];

				// Check whether the digit contains any eliminations.
				var intersection = c & map;
				if (intersection.Count >= 2)
				{
					// a & map => Cells in lines are not empty => pointing eliminations.
					result.Add(new(digit, intersection));
				}
			}
		}
	}

	/// <summary>
	/// An unsafe entry to method <c>GetHeaderBits</c> defined in type <see cref="Grid"/>.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="cell">The cell.</param>
	/// <returns>The mask of mask bits, unshifted.</returns>
	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetHeaderBits")]
	private static extern Mask GetHeaderBits(ref readonly Grid grid, Cell cell);
}
