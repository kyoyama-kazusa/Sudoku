namespace Sudoku.Analytics.StepSearchers.SymmetricalPlacements;

/// <summary>
/// Provides with a <b>Gurth's Symmetrical Placement</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Anti- Gurth's Symmetrical Placement</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_AntiGurthSymmetricalPlacementStepSearcher",
	Technique.AntiGurthSymmetricalPlacement,
	IsCachingSafe = true)]
public sealed partial class AntiGurthSymmetricalPlacementStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the functions that are searchers for subtypes.
	/// </summary>
	private static readonly unsafe delegate*<in Grid, ref StepAnalysisContext, AntiGurthSymmetricalPlacementStep?>[] AntiTypeCheckers = [
		&Diagonal,
		&Antidiagonal,
		&XAxis,
		&YAxis,
		&Central
	];


	/// <inheritdoc/>
	protected internal override unsafe Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		if (grid.PuzzleType == SudokuType.Sukaku)
		{
			// Skip if the puzzle is a Sukaku.
			return null;
		}

		// Anti types.
		foreach (var checker in AntiTypeCheckers)
		{
			if (checker(grid, ref context) is not { } antiTypeStep)
			{
				continue;
			}
			if (context.OnlyFindOne)
			{
				return antiTypeStep;
			}
			context.Accumulator.Add(antiTypeStep);
		}
		return null;
	}


	/// <summary>
	/// Records all possible highlight cells.
	/// </summary>
	/// <param name="grid">The grid as reference.</param>
	/// <param name="cellOffsets">The target collection.</param>
	/// <param name="mapping">The mapping relation.</param>
	private static void GetHighlightCells(in Grid grid, List<CellViewNode> cellOffsets, Span<Digit?> mapping)
	{
		var colorIndices = (stackalloc Digit[9]);
		for (var (digit, colorIndexCurrent, digitsMaskBucket) = (0, 0, (Mask)0); digit < 9; digit++)
		{
			if ((digitsMaskBucket >> digit & 1) != 0)
			{
				continue;
			}

			colorIndices[digit] = colorIndexCurrent;
			digitsMaskBucket |= (Mask)(1 << digit);
			if (mapping[digit] is { } relatedDigit && relatedDigit != digit)
			{
				colorIndices[relatedDigit] = colorIndexCurrent;
				digitsMaskBucket |= (Mask)(1 << relatedDigit);
			}

			colorIndexCurrent++;
		}

		foreach (var cell in ~grid.EmptyCells)
		{
			cellOffsets.Add(new(colorIndices[grid.GetDigit(cell)], cell));
		}
	}

	/// <summary>
	/// Checks for diagonal symmetry steps, anti-GSP type.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static AntiGurthSymmetricalPlacementStep? Diagonal(in Grid grid, ref StepAnalysisContext context)
	{
		var mapping = (stackalloc Digit?[9]);
		mapping.Clear();
		var cellsNotSymmetrical = CellMap.Empty;
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < i; j++)
			{
				var (c1, c2) = (i * 9 + j, j * 9 + i);
				var condition = grid.GetState(c1) == CellState.Empty;
				if (condition ^ grid.GetState(c2) == CellState.Empty)
				{
					// One of two cells is empty. Not this symmetry.
					if (cellsNotSymmetrical)
					{
						return null;
					}

					cellsNotSymmetrical.Add(c1);
					cellsNotSymmetrical.Add(c2);
					continue;
				}

				if (condition)
				{
					continue;
				}

				var (d1, d2) = (grid.GetDigit(c1), grid.GetDigit(c2));
				if (d1 == d2)
				{
					var o1 = mapping[d1];
					if (o1 is null)
					{
						mapping[d1] = d1;
						continue;
					}

					if (o1 != d1)
					{
						return null;
					}
				}
				else
				{
					var (o1, o2) = (mapping[d1], mapping[d2]);
					if (o1.HasValue ^ o2.HasValue)
					{
						return null;
					}

					if (o1 is null && o2 is null)
					{
						(mapping[d1], mapping[d2]) = (d2, d1);
						continue;
					}

					// 'o1' and 'o2' are both not null.
					if (o1 != d2 || o2 != d1)
					{
						return null;
					}
				}
			}
		}

		if (!cellsNotSymmetrical)
		{
			// All cells are symmetrical placements. This will have been checked in normal types.
			return null;
		}

		var singleDigitList = new List<Digit>();
		for (var digit = 0; digit < 9; digit++)
		{
			var mappingDigit = mapping[digit];
			if (!mappingDigit.HasValue || mappingDigit == digit)
			{
				singleDigitList.Add(digit);
			}
		}

		var singleDigitsMask = Mask.Create(singleDigitList.AsSpan());

		// Now check for diagonal line cells, determining whether the solution grid may not be a symmetrical placement.
		var isSolutionAsymmetry = singleDigitList.Count > 3;
		foreach (var cell in SymmetricType.Diagonal.CellsInSymmetryAxis)
		{
			if ((Mask)(grid.GetCandidates(cell) & singleDigitsMask) == 0)
			{
				// We can conclude that the solution should be an asymmetrical placement if one of two following conditions is true:
				// 1) If the cell is empty, and no possible candidates in the single-digit set can be filled into the cell.
				// 2) If the cell is not empty, and it is filled with other digits not appearing in the single-digit set.
				// Two conditions can be expressed with one boolean expression.
				isSolutionAsymmetry = true;
				break;
			}
		}
		if (!isSolutionAsymmetry)
		{
			// We cannot determine whether the solution is an asymmetrical placement.
			return null;
		}

		if (!SatisfyForAxis(grid, mapping, out _))
		{
			return null;
		}

		var elimCell = cellsNotSymmetrical.First(grid, static (cell, in grid) => grid.GetState(cell) == CellState.Empty);
		var elimDigit = mapping[grid.GetDigit((cellsNotSymmetrical - elimCell)[0])]!.Value;
		if ((grid.GetCandidates(elimCell) >> elimDigit & 1) == 0)
		{
			// No elimination.
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		var candidateOffsets = new List<CandidateViewNode>();
		GetHighlightCells(grid, cellOffsets, mapping);

		return new(
			new SingletonArray<Conclusion>(new(Elimination, elimCell, elimDigit)),
			[[.. cellOffsets, .. candidateOffsets]],
			context.Options,
			SymmetricType.Diagonal,
			[.. mapping]
		);
	}

	/// <summary>
	/// Checks for diagonal symmetry steps, anti-GSP type.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static AntiGurthSymmetricalPlacementStep? Antidiagonal(in Grid grid, ref StepAnalysisContext context)
	{
		var mapping = (stackalloc Digit?[9]);
		mapping.Clear();
		var cellsNotSymmetrical = CellMap.Empty;
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 8 - i; j++)
			{
				var (c1, c2) = (i * 9 + j, (8 - j) * 9 + (8 - i));
				var condition = grid.GetState(c1) == CellState.Empty;
				if (condition ^ grid.GetState(c2) == CellState.Empty)
				{
					// One of two cells is empty. Not this symmetry.
					if (cellsNotSymmetrical)
					{
						return null;
					}

					cellsNotSymmetrical.Add(c1);
					cellsNotSymmetrical.Add(c2);
					continue;
				}

				if (condition)
				{
					continue;
				}

				var (d1, d2) = (grid.GetDigit(c1), grid.GetDigit(c2));
				if (d1 == d2)
				{
					var o1 = mapping[d1];
					if (o1 is null)
					{
						mapping[d1] = d1;
						continue;
					}

					if (o1 != d1)
					{
						return null;
					}
				}
				else
				{
					var (o1, o2) = (mapping[d1], mapping[d2]);
					if (o1.HasValue ^ o2.HasValue)
					{
						return null;
					}

					if (o1 is null || o2 is null)
					{
						(mapping[d1], mapping[d2]) = (d2, d1);
						continue;
					}

					// 'o1' and 'o2' are both not null.
					if (o1 != d2 || o2 != d1)
					{
						return null;
					}
				}
			}
		}

		if (!cellsNotSymmetrical)
		{
			// All cells are symmetrical placements. This will have been checked in normal types.
			return null;
		}

		var singleDigitList = new List<Digit>();
		for (var digit = 0; digit < 9; digit++)
		{
			var mappingDigit = mapping[digit];
			if (!mappingDigit.HasValue || mappingDigit == digit)
			{
				singleDigitList.Add(digit);
			}
		}

		var singleDigitsMask = Mask.Create(singleDigitList.AsSpan());

		// Now check for diagonal line cells, determining whether the solution grid may not be a symmetrical placement.
		var isSolutionAsymmetry = singleDigitList.Count > 3;
		foreach (var cell in SymmetricType.AntiDiagonal.CellsInSymmetryAxis)
		{
			if ((Mask)(grid.GetCandidates(cell) & singleDigitsMask) == 0)
			{
				// We can conclude that the solution should be an asymmetrical placement if one of two following conditions is true:
				//   1) If the cell is empty, and no possible candidates in the single-digit set can be filled into the cell.
				//   2) If the cell is not empty, and it is filled with other digits not appearing in the single-digit set.
				// Two conditions can be expressed with one boolean expression.
				isSolutionAsymmetry = true;
				break;
			}
		}
		if (!isSolutionAsymmetry)
		{
			// We cannot determine whether the solution is an asymmetrical placement.
			return null;
		}

		if (!SatisfyForAxis(grid, mapping, out _))
		{
			return null;
		}

		var elimCell = cellsNotSymmetrical.First(grid, static (cell, in grid) => grid.GetState(cell) == CellState.Empty);
		var elimDigit = mapping[grid.GetDigit((cellsNotSymmetrical - elimCell)[0])]!.Value;
		if ((grid.GetCandidates(elimCell) >> elimDigit & 1) == 0)
		{
			// No elimination.
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		var candidateOffsets = new List<CandidateViewNode>();
		GetHighlightCells(grid, cellOffsets, mapping);

		return new(
			new SingletonArray<Conclusion>(new(Elimination, elimCell, elimDigit)),
			[[.. cellOffsets, .. candidateOffsets]],
			context.Options,
			SymmetricType.AntiDiagonal,
			[.. mapping]
		);
	}

	/// <summary>
	/// Checks for X-axis symmetry steps, anti-GSP type.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static AntiGurthSymmetricalPlacementStep? XAxis(in Grid grid, ref StepAnalysisContext context)
	{
		var mapping = (stackalloc Digit?[9]);
		mapping.Clear();
		var cellsNotSymmetrical = CellMap.Empty;
		for (var i = 0; i < 4; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				var c1 = i * 9 + j;
				var c2 = (SymmetricType.XAxis.GetCells(c1) - c1)[0];
				var condition = grid.GetState(c1) == CellState.Empty;
				if (condition ^ grid.GetState(c2) == CellState.Empty)
				{
					// One of two cells is empty. Not this symmetry.
					if (cellsNotSymmetrical)
					{
						return null;
					}

					cellsNotSymmetrical.Add(c1);
					cellsNotSymmetrical.Add(c2);
					continue;
				}

				if (condition)
				{
					continue;
				}

				var (d1, d2) = (grid.GetDigit(c1), grid.GetDigit(c2));
				if (d1 == d2)
				{
					var o1 = mapping[d1];
					if (o1 is null)
					{
						mapping[d1] = d1;
						continue;
					}

					if (o1 != d1)
					{
						return null;
					}
				}
				else
				{
					var (o1, o2) = (mapping[d1], mapping[d2]);
					if (o1.HasValue ^ o2.HasValue)
					{
						return null;
					}

					if (o1 is null || o2 is null)
					{
						mapping[d1] = d2;
						mapping[d2] = d1;
						continue;
					}

					// 'o1' and 'o2' are both not null.
					if (o1 != d2 || o2 != d1)
					{
						return null;
					}
				}
			}
		}

		if (!cellsNotSymmetrical)
		{
			// All cells are symmetrical placements. This will have been checked in normal types.
			return null;
		}

		if (!SatisfyForAxis(grid, mapping, out _))
		{
			return null;
		}

		var elimCell = cellsNotSymmetrical.First(grid, static (cell, in grid) => grid.GetState(cell) == CellState.Empty);
		var elimDigit = mapping[grid.GetDigit((cellsNotSymmetrical - elimCell)[0])]!.Value;
		if ((grid.GetCandidates(elimCell) >> elimDigit & 1) == 0)
		{
			// No elimination.
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		var candidateOffsets = new List<CandidateViewNode>();
		GetHighlightCells(grid, cellOffsets, mapping);

		return new(
			new SingletonArray<Conclusion>(new(Elimination, elimCell, elimDigit)),
			[[.. cellOffsets, .. candidateOffsets]],
			context.Options,
			SymmetricType.XAxis,
			[.. mapping]
		);
	}

	/// <summary>
	/// Checks for Y-axis symmetry steps, anti-GSP type.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static AntiGurthSymmetricalPlacementStep? YAxis(in Grid grid, ref StepAnalysisContext context)
	{
		var mapping = (stackalloc Digit?[9]);
		mapping.Clear();
		var cellsNotSymmetrical = CellMap.Empty;
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 4; j++)
			{
				var c1 = i * 9 + j;
				var c2 = (SymmetricType.YAxis.GetCells(c1) - c1)[0];
				var condition = grid.GetState(c1) == CellState.Empty;
				if (condition ^ grid.GetState(c2) == CellState.Empty)
				{
					// One of two cells is empty. Not this symmetry.
					if (cellsNotSymmetrical)
					{
						return null;
					}

					cellsNotSymmetrical.Add(c1);
					cellsNotSymmetrical.Add(c2);
					continue;
				}

				if (condition)
				{
					continue;
				}

				var (d1, d2) = (grid.GetDigit(c1), grid.GetDigit(c2));
				if (d1 == d2)
				{
					var o1 = mapping[d1];
					if (o1 is null)
					{
						mapping[d1] = d1;
						continue;
					}

					if (o1 != d1)
					{
						return null;
					}
				}
				else
				{
					var (o1, o2) = (mapping[d1], mapping[d2]);
					if (o1.HasValue ^ o2.HasValue)
					{
						return null;
					}

					if (o1 is null || o2 is null)
					{
						mapping[d1] = d2;
						mapping[d2] = d1;
						continue;
					}

					// 'o1' and 'o2' are both not null.
					if (o1 != d2 || o2 != d1)
					{
						return null;
					}
				}
			}
		}

		if (!cellsNotSymmetrical)
		{
			// All cells are symmetrical placements. This will have been checked in normal types.
			return null;
		}

		if (!SatisfyForAxis(grid, mapping, out _))
		{
			return null;
		}

		var elimCell = cellsNotSymmetrical.First(grid, static (cell, in grid) => grid.GetState(cell) == CellState.Empty);
		var elimDigit = mapping[grid.GetDigit((cellsNotSymmetrical - elimCell)[0])]!.Value;
		if ((grid.GetCandidates(elimCell) >> elimDigit & 1) == 0)
		{
			// No elimination.
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		var candidateOffsets = new List<CandidateViewNode>();
		GetHighlightCells(grid, cellOffsets, mapping);

		return new(
			new SingletonArray<Conclusion>(new(Elimination, elimCell, elimDigit)),
			[[.. cellOffsets, .. candidateOffsets]],
			context.Options,
			SymmetricType.YAxis,
			[.. mapping]
		);
	}

	/// <summary>
	/// Checks for central symmetry steps, anti-GSP type.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static AntiGurthSymmetricalPlacementStep? Central(in Grid grid, ref StepAnalysisContext context)
	{
		var mapping = (stackalloc Digit?[9]);
		mapping.Clear();
		var cellsNotSymmetrical = CellMap.Empty;
		for (var cell = 0; cell < 40; cell++)
		{
			var anotherCell = 80 - cell;
			var condition = grid.GetState(cell) == CellState.Empty;
			if (condition ^ grid.GetState(anotherCell) == CellState.Empty)
			{
				// One of two cell is empty, not central symmetry type.
				if (cellsNotSymmetrical)
				{
					return null;
				}

				cellsNotSymmetrical.Add(cell);
				cellsNotSymmetrical.Add(anotherCell);
				continue;
			}

			if (condition)
			{
				continue;
			}

			var (d1, d2) = (grid.GetDigit(cell), grid.GetDigit(anotherCell));
			if (d1 == d2)
			{
				var o1 = mapping[d1];
				if (o1 is null)
				{
					mapping[d1] = d1;
					continue;
				}

				if (o1 != d1)
				{
					return null;
				}
			}
			else
			{
				var (o1, o2) = (mapping[d1], mapping[d2]);
				if (o1 is not null ^ o2 is not null)
				{
					return null;
				}

				if (o1 is null || o2 is null)
				{
					mapping[d1] = d2;
					mapping[d2] = d1;
					continue;
				}

				// 'o1' and 'o2' are both not null.
				if (o1 != d2 || o2 != d1)
				{
					return null;
				}
			}
		}

		if (!cellsNotSymmetrical)
		{
			// All cells are symmetrical placements. This will have been checked in normal types.
			return null;
		}

		var singleDigitList = new List<Digit>();
		for (var digit = 0; digit < 9; digit++)
		{
			var mappingDigit = mapping[digit];
			if (!mappingDigit.HasValue || mappingDigit == digit)
			{
				singleDigitList.Add(digit);
			}
		}

		var singleDigitsMask = Mask.Create(singleDigitList.AsSpan());

		// Now check for diagonal line cells, determining whether the solution grid may not be a symmetrical placement.
		var isSolutionAsymmetry = singleDigitList.Count > 1 || (Mask)(grid.GetCandidates(40) & singleDigitsMask) == 0;
		if (!isSolutionAsymmetry)
		{
			// We cannot determine whether the solution is an asymmetrical placement.
			return null;
		}

		if (!SatisfyForAxis(grid, mapping, out _))
		{
			return null;
		}

		var elimCell = cellsNotSymmetrical.First(grid, static (cell, in grid) => grid.GetState(cell) == CellState.Empty);
		var elimDigit = mapping[grid.GetDigit((cellsNotSymmetrical - elimCell)[0])]!.Value;
		if ((grid.GetCandidates(elimCell) >> elimDigit & 1) == 0)
		{
			// No elimination.
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		var candidateOffsets = new List<CandidateViewNode>();
		GetHighlightCells(grid, cellOffsets, mapping);

		return new(
			new SingletonArray<Conclusion>(new(Elimination, elimCell, elimDigit)),
			[[.. cellOffsets, .. candidateOffsets]],
			context.Options,
			SymmetricType.Central,
			[.. mapping]
		);
	}

	/// <summary>
	/// Checks for axes.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="mapping">Mapping digits table.</param>
	/// <param name="selfPairedDigitsMask">Self-paired digits.</param>
	/// <returns>A <see cref="bool"/> mask.</returns>
	private static bool SatisfyForAxis(in Grid grid, Span<int?> mapping, out Mask selfPairedDigitsMask)
	{
		selfPairedDigitsMask = 0;
		for (var digit = 0; digit < 9; digit++)
		{
			if (mapping[digit] switch { { } currentDigit => currentDigit == digit, _ => true })
			{
				selfPairedDigitsMask |= (Mask)(1 << digit);
			}
		}
		return SymmetryInferrer.CheckAxesOrCenterPointForSymmetry(
			grid,
			SymmetricType.Diagonal,
			(Mask)(Grid.MaxCandidatesMask & ~selfPairedDigitsMask)
		);
	}
}
