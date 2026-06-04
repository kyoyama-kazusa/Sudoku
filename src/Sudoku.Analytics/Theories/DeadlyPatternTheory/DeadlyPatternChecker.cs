namespace Sudoku.Theories.DeadlyPatternTheory;

/// <summary>
/// Represents a type that checks and infers for a pattern (specified as a <see cref="Grid"/>, but invalid - multiple solutions),
/// determining whether the pattern is a deadly pattern.
/// </summary>
/// <seealso cref="Grid"/>
public static class DeadlyPatternChecker
{
	/// <summary>
	/// Determine whether a grid will form a deadly pattern if the specified truths are constrained.
	/// </summary>
	/// <param name="grid">Indicates the grid. It's required that only specified cells are assigned with candidates.</param>
	/// <param name="truths">Indicates the truths to be checked.</param>
	/// <param name="options">Indicates the extra options.</param>
	/// <returns>The result after the analysis operation is finished.</returns>
	/// <exception cref="DeadlyPatternInferrerLimitReachedException">
	/// Throws when the pattern contains more than the number of solutions
	/// specified in parameter <paramref name="options"/>,
	/// and its property <see cref="DeadlyPatternCheckingOptions.ThrowExceptionIfMaximumSolutionsCountReached"/>
	/// is set <see langword="true"/>.
	/// </exception>
	/// <seealso cref="DeadlyPatternCheckingOptions.ThrowExceptionIfMaximumSolutionsCountReached"/>
	public static DeadlyPatternCheckingResult CheckWhetherFormsDeadlyPattern(
		in Grid grid,
		in SpaceSet truths,
		DeadlyPatternCheckingOptions options = default
	)
	{
		var defaultResult = new DeadlyPatternCheckingResult(in grid) { PermutationsCount = 0, IsDeadlyPattern = false, FailedCases = [] };

		var patternMap = CandidateMap.Empty;
		if (grid is not { IsValid: false, EmptyCellsCount: 81, IsStandard: true })
		{
			// Invalid values to be checked.
			goto FastFail;
		}

		// Verify whether at least one cell in pattern hold nothing.
		foreach (var truth in truths)
		{
			if (truth.GetAvailableRange(grid) is not (var candidates and not []))
			{
				goto FastFail;
			}
			patternMap |= candidates;
		}
		var patternCells = patternMap.Cells;

		// Step 0: Determine whether at least one house the pattern spanned only hold one cell used.
		// A valid deadly pattern must hold at least 2 cells for all spanned houses.
		if (!options.IgnorePotentialSinglePositionInHouse)
		{
			foreach (var kvp in patternMap.DigitDistribution)
			{
				var digit = kvp.Key;
				ref readonly var cells = ref kvp.ValueRef;
				foreach (var house in cells.Houses)
				{
					if ((HousesMap[house] & cells).Count == 1)
					{
						goto FastFail;
					}
				}
			}
		}

		// Step 1: Get all solutions for that pattern.
		var permutations = getPermutations(grid, truths, out var isFailed);
		if (isFailed)
		{
			return defaultResult with
			{
				PatternCandidates = patternMap,
				FailedReason = DeadlyPatternResultFailedReason.MaxSolutionsReached
			};
		}

		if (permutations.Length == 0)
		{
			goto FastFail;
		}

		var failedCases = new List<Grid>();
		foreach (ref readonly var permutation in permutations)
		{
			var permutationHouses = permutation.Houses;
			ref readonly var caseToCheck = ref permutation.Grid;

			// Step 2: Iterate on all the other solutions,
			// and find whether each solution contains at least one possible corresponding solution
			// whose digits used in *all* houses are completely same.
			var tempSolutions = new List<Grid>();
			foreach (ref readonly var anotherPermutation in permutations[..])
			{
				var anotherPermutationHouses = anotherPermutation.Houses;
				ref readonly var anotherCaseToCheck = ref anotherPermutation.Grid;

				// Skip for invalid states to check.
				if (anotherPermutation == permutation || anotherPermutationHouses != permutationHouses)
				{
					continue;
				}

				// Check for all possible houses.
				var containsPermutationWithInequivalentFillings = true;
				foreach (var house in anotherPermutationHouses)
				{
					var m1 = caseToCheck[HousesMap[house] & patternCells, true];
					var m2 = anotherCaseToCheck[HousesMap[house] & patternCells, true];
					if (m1 != m2)
					{
						containsPermutationWithInequivalentFillings = false;
						break;
					}
				}
				if (containsPermutationWithInequivalentFillings)
				{
					tempSolutions.AddRef(anotherCaseToCheck);
				}
			}

			// Step 3: Check for the validity on this case.
			// If failed to check, we should collect the case into the result, as an item in failed cases set.
			if (tempSolutions.Count == 0)
			{
				failedCases.AddRef(caseToCheck);
			}
		}

		// If all possible solutions has exchangable patterns, the pattern will be a real deadly pattern;
		// otherwise, not a deadly pattern.
		return defaultResult with
		{
			PermutationsCount = permutations.Length,
			IsDeadlyPattern = failedCases.Count == 0,
			FailedCases = failedCases.AsSpan(),
			PatternCandidates = patternMap,
			FailedReason = DeadlyPatternResultFailedReason.None
		};

	FastFail:
		return defaultResult with
		{
			PatternCandidates = patternMap,
			FailedReason = DeadlyPatternResultFailedReason.NotDeadlyPattern
		};


		ReadOnlySpan<(Grid Grid, HouseMask Houses)> getPermutations(in Grid grid, in SpaceSet truths, out bool isFailed)
		{
			var result = new List<(Grid, HouseMask)>();
			var links = SpaceSet.Empty;
			foreach (var truth in truths)
			{
				foreach (var candidate in truth.GetAvailableRange(grid))
				{
					links.AddRange(candidate.Spaces);
				}
			}

			var pattern = new Logic(truths, links, grid);
			var permutations = LogicReasoner.GetPermutations(in pattern);
			if (options.LimitSolutionsCount != 0 && permutations.Length > options.LimitSolutionsCount)
			{
				if (options.ThrowExceptionIfMaximumSolutionsCountReached)
				{
					throw new DeadlyPatternInferrerLimitReachedException();
				}

				isFailed = true;
				return result.AsSpan();
			}

			foreach (var permutation in permutations)
			{
				var emptyGrid = Grid.Empty;
				var houses = 0;
				foreach (var candidate in permutation)
				{
					var cell = candidate / 9;
					var digit = candidate % 9;
					emptyGrid[cell] = (Mask)(Grid.ModifiableMask | 1 << digit);
					houses |= 1 << cell.GetHouse(HouseType.Block);
					houses |= 1 << cell.GetHouse(HouseType.Row);
					houses |= 1 << cell.GetHouse(HouseType.Column);
				}
				result.AddRef((emptyGrid, houses));
			}

			isFailed = false;
			return result.AsSpan();
		}
	}

	/// <summary>
	/// Try to assign a candidate, and find a complete pattern where all cells are assigned,
	/// and then check whether the assigned pattern can form a deadly pattern.
	/// </summary>
	/// <param name="assigned">The assigned candidate.</param>
	/// <param name="assigningMap">
	/// The assigning map to be checked. This argument can accelerate checking if specified.
	/// This argument limits the checking to the specified digits for the specified cells,
	/// in order to prevent the backing searching module doing unnecessary digits' checking.
	/// Assign <see langword="null"/> if you don't want to specify, meaning all candidates in the grid will be checked.
	/// </param>
	/// <param name="grid">The grid.</param>
	/// <param name="cells">The cells in the grid to be checked.</param>
	/// <param name="assignedCandidates">All assigned candidates that will cause the pattern invalid.</param>
	/// <returns>
	/// A <see cref="bool"/> result indicating whether the candidate <paramref name="assigned"/>
	/// will make the pattern to be a deadly pattern.
	/// </returns>
	/// <remarks>
	/// <para>
	/// This method will help you find a contradiction of deadly pattern if you assign a value into a cell.
	/// For example, the following grid shows two extended rectangles:
	/// <code><![CDATA[
	/// .---------------,--------------------,--------------.
	/// | 7    2    8   | 3       59    59   | 4    1   6   |
	/// | 4    5   *19  |*127     127   6    | 3   *29  8   |
	/// | 16   3   *169 |*12      4     8    | 7   *29  5   |
	/// :---------------+--------------------+--------------:
	/// | 8    46   126 | 12469   3     1249 | 19   5   7   |
	/// | 126  9    5   | 1267    127   127  | 8    3   4   |
	/// | 3    47   17  | 1459    8     1459 | 2    6   19  |
	/// :---------------+--------------------+--------------:
	/// | 9   #678  3   |#12458   125   1245 | 156 #47  12  |
	/// | 25  #78   27  |#124589  6     3    | 159 #47  129 |
	/// | 256  1    4   | 2579    2579  2579 | 569  8   3   |
	/// '---------------'--------------------'--------------'
	/// ]]></code>
	/// This method returns <see langword="true"/> if assigning <c>r2c4</c> with value 2, or <c>r7c2</c> with 7.
	/// For the former one, we will make cells <c>r23c348</c> forms a invalid state of digits 1, 2 and 9;
	/// and for the latter one, we will lead to a contradiction of cells <c>r78c248</c> with digits 4, 7 and 8.
	/// </para>
	/// <para>
	/// This method will perform breadth-first searching to find any contradictions if we assign with <c>r2c4 = 2</c>
	/// and <c>r7c2 = 7</c>, and return a <see langword="bool"/> value indicating whether the first assigned value
	/// can form such contradiction or not.
	/// If can, argument <paramref name="assignedCandidates"/> will include the full path of trial of cells;
	/// otherwise, <see cref="CandidateMap.Empty"/>.
	/// </para>
	/// </remarks>
	public static bool CanLeadToDeadlyPatternContradiction(
		Candidate assigned,
		DeadlyPatternAssigningMap? assigningMap,
		in Grid grid,
		in CellMap cells,
		out CandidateMap assignedCandidates
	)
	{
		// Check whether the first assigned cell is empty. If not, we cannot start.
		if (grid.GetState(assigned / 9) != CellState.Empty)
		{
			// Invalid state.
			goto ReturnFalse;
		}

		// Check whether all cells are empty or modifiable cells.
		var allSpecifiedCellsAreNonGiven = true;
		foreach (var cell in cells)
		{
			if (grid.GetState(cell) == CellState.Given)
			{
				allSpecifiedCellsAreNonGiven = false;
				break;
			}
		}
		if (!allSpecifiedCellsAreNonGiven)
		{
			// Invalid state.
			goto ReturnFalse;
		}

		var root = new Node(assigned, null);
		var queue = new Queue<Node>();
		queue.Enqueue(root);

		// Do a BFS to find a path.
		while (queue.TryDequeue(out var currentNode))
		{
			// Apply grid.
			var appliedGrid = grid;
			currentNode.ApplyTo(ref appliedGrid);

			// Check whether all cells are assigned or not.
			var unassignedCells = cells & ~appliedGrid.ModifiableCells;
			if (!unassignedCells)
			{
				// All cells are assigned.
				// Check whether the digits can form a deadly pattern or not.

				// Set all digits to a temporary grid, making the local vairable only contains such assignments.
				var tempGrid = Grid.Empty;
				foreach (var cell in cells)
				{
					tempGrid.SetDigit(cell, appliedGrid.GetDigit(cell));
				}

				// Check whether all cells are filled with a valid digit specified.
				if (assigningMap is not null)
				{
					var doesCombinationContainAnyInvalidAssignments = false;
					foreach (var cell in cells)
					{
						if (!assigningMap[cell, tempGrid.GetDigit(cell)])
						{
							// The cell cannot assign with the digit unassigned.
							doesCombinationContainAnyInvalidAssignments = true;
						}
					}
					if (doesCombinationContainAnyInvalidAssignments)
					{
						// This combination contains at least one invalid assignment.
						continue;
					}
				}

				// Check whether the combination can correspond with another grid,
				// whose houses occupied use same digits as the original combination.
				if (!checkDeadlyPatternOnAssignedCells(tempGrid, cells))
				{
					continue;
				}

				// Valid.
				assignedCandidates = currentNode.AssignedCandidates;
				return true;
			}

			// If not, we should continue to search for cells unassigned.
			foreach (var cell in unassignedCells)
			{
				if (appliedGrid.GetState(cell) != CellState.Empty)
				{
					continue;
				}

				// Check any hidden singles and naked singles.
				if (isNakedSingle(appliedGrid, cell, out var digit))
				{
					// Assign the value.
					var nextNode = new Node(cell * 9 + digit, currentNode);
					queue.Enqueue(nextNode);
				}
				else if (isHiddenSingle(appliedGrid, cell, out var targetCell, out digit) && cells.Contains(targetCell))
				{
					var nextNode = new Node(targetCell * 9 + digit, currentNode);
					queue.Enqueue(nextNode);
				}
			}
		}

	ReturnFalse:
		assignedCandidates = CandidateMap.Empty;
		return false;


		static bool isNakedSingle(in Grid grid, Cell cell, out Digit digit)
		{
			var mask = (uint)(grid.GetCandidates(cell) & Grid.MaxCandidatesMask);
			if (BitOperations.IsPow2(mask))
			{
				digit = BitOperations.Log2(mask);
				return true;
			}
			digit = -1;
			return false;
		}

		static bool isHiddenSingle(in Grid grid, Cell cell, out Cell targetCell, out Digit digit)
		{
			for (var eachDigit = 0; eachDigit < 9; eachDigit++)
			{
				foreach (var houseType in HouseTypes)
				{
					var counter = 0;
					targetCell = -1;
					foreach (var eachCell in HousesMap[cell.GetHouse(houseType)])
					{
						if (grid.Exists(eachCell, eachDigit) is true && (targetCell = eachCell) is var _ && ++counter >= 2)
						{
							break;
						}
					}
					if (counter == 1)
					{
						digit = eachDigit;
						return true;
					}
				}
			}

			digit = -1;
			targetCell = -1;
			return false;
		}

		static bool checkDeadlyPatternOnAssignedCells(in Grid grid, in CellMap cells)
		{
			// Find for all digits appeared in all houses.
			var houseDigitsDictionary = new Dictionary<House, Mask>(27);
			foreach (var house in cells.Houses)
			{
				houseDigitsDictionary.Add(house, grid[HousesMap[house] & cells]);
			}

			// Then enumerate assignments to find a new assignments, where all digits are same as the current one.
			var queue = new Queue<Node>();
			var firstCell = cells[0];

			var a = houseDigitsDictionary[firstCell.GetHouse(HouseType.Block)];
			var b = houseDigitsDictionary[firstCell.GetHouse(HouseType.Row)];
			var c = houseDigitsDictionary[firstCell.GetHouse(HouseType.Column)];
			foreach (var digit in (Mask)(a | (Mask)(b | c)))
			{
				queue.Enqueue(new(firstCell * 9 + digit, null));
			}

			// Try to assign the value.
			while (queue.TryDequeue(out var currentNode))
			{
				// Apply grid.
				var appliedGrid = Grid.Empty;
				currentNode.ApplyTo(ref appliedGrid);

				// Check for existence of unassigned cells.
				// If all cells are assigned, we should check whether all cells satisfy deadly pattern basic rule.
				var unassignedCells = cells & ~appliedGrid.ModifiableCells;
				if (!unassignedCells)
				{
					if (grid == appliedGrid)
					{
						continue;
					}

					// Check for each house.
					var tempHouseDigitsDictionary = new Dictionary<House, Mask>(27);
					var houses = cells.Houses;
					foreach (var house in houses)
					{
						tempHouseDigitsDictionary.Add(house, appliedGrid[HousesMap[house] & cells]);
					}
					var flag = true;
					foreach (var house in houses)
					{
						if (houseDigitsDictionary[house] != tempHouseDigitsDictionary[house])
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}

					return true;
				}

				// Otherwise, we should continue to iterate other cells.
				foreach (var cell in unassignedCells)
				{
					var d = houseDigitsDictionary[cell.GetHouse(HouseType.Block)];
					var e = houseDigitsDictionary[cell.GetHouse(HouseType.Row)];
					var f = houseDigitsDictionary[cell.GetHouse(HouseType.Column)];
					var g = (Mask)0;
					foreach (var houseType in HouseTypes)
					{
						foreach (var tempCell in cells & HousesMap[cell.GetHouse(houseType)] & ~unassignedCells)
						{
							g |= (Mask)(1 << appliedGrid.GetDigit(tempCell));
						}
					}
					foreach (var digit in (Mask)((Mask)(d | (Mask)(e | f)) & ~g))
					{
						queue.Enqueue(new(cell * 9 + digit, currentNode));
					}
				}
			}

			return false;
		}
	}
}

/// <summary>
/// Represents a node that describes assigned cases.
/// </summary>
/// <param name="Assigned">Indicates the assigned candidate.</param>
/// <param name="Parent">Indicates the parent node.</param>
file sealed record Node(Candidate Assigned, Node? Parent) : IEqualityOperators<Node, Node, bool>
{
	/// <summary>
	/// Indicates all assigned candidates.
	/// </summary>
	public CandidateMap AssignedCandidates
	{
		get
		{
			var result = CandidateMap.Empty;
			for (var node = this; node is not null; node = node.Parent)
			{
				result += node.Assigned;
			}
			return result;
		}
	}


	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] Node? other)
		=> other is not null && AssignedCandidates == other.AssignedCandidates;

	/// <inheritdoc/>
	public override int GetHashCode() => AssignedCandidates.GetHashCode();

	/// <summary>
	/// Apply all assignments to the target grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	public void ApplyTo(ref Grid grid)
	{
		foreach (var assigned in AssignedCandidates)
		{
			grid.SetDigit(assigned / 9, assigned % 9);
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		var cell = Assigned / 9;
		var digit = Assigned % 9;

		builder.Append($"{nameof(Assigned)} = ");
		builder.Append($"r{cell / 9 + 1}c{cell % 9 + 1}({digit + 1})");
		builder.Append($", {nameof(Parent)} = ");
		if (Parent is not null)
		{
			var parentAssigned = Parent.Assigned;
			var parentCell = parentAssigned / 9;
			var parentDigit = parentAssigned % 9;
			builder.Append($"r{parentCell / 9 + 1}c{parentCell % 9 + 1}({parentDigit + 1})");
		}
		else
		{
			builder.Append("<null>");
		}
		return true;
	}
}
