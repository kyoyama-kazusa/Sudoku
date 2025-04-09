namespace Sudoku.Inferring;

/// <summary>
/// Represents an inferrer that can checks for symmetrical placements.
/// </summary>
public sealed unsafe class SymmetryInferrer : IInferrable<SymmetryInferredResult>
{
	/// <summary>
	/// The internal methods.
	/// </summary>
	private static readonly delegate*<in Grid, out SymmetricType, out ReadOnlySpan<Digit?>, out Mask, bool>[] Checkers = [
		&Diagonal,
		&AntiDiagonal,
		&Central
	];


	/// <inheritdoc/>
	public static bool TryInfer(in Grid grid, out SymmetryInferredResult result)
	{
		if (grid.PuzzleType != SudokuType.Standard || grid.GetUniqueness() != Uniqueness.Unique)
		{
			goto FastFail;
		}

		foreach (var functionPointer in Checkers)
		{
			if (functionPointer(grid, out var symmetricType, out var mappingDigits, out var selfPairedDigitsMask))
			{
				result = new(symmetricType, mappingDigits, selfPairedDigitsMask);
				return true;
			}
		}

	FastFail:
		result = default;
		return false;
	}

	/// <summary>
	/// Try to get the its mapping rule for the specified grid via the specified symmetric type.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="symmetricType">The symmetric type to be checked.</param>
	/// <param name="mappingDigits">The mapping digits returned.</param>
	/// <param name="selfPairedDigitsMask">A mask that contains a list of digits self-paired.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the grid is a symmetrical-placement pattern.</returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Throws when the argument <paramref name="symmetricType"/> is not <see cref="SymmetricType.Central"/>,
	/// <see cref="SymmetricType.Diagonal"/> or	<see cref="SymmetricType.AntiDiagonal"/>.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSymmetricalPlacement(
		in Grid grid,
		SymmetricType symmetricType,
		out ReadOnlySpan<Digit?> mappingDigits,
		out Mask selfPairedDigitsMask
	)
	{
		if (symmetricType is not (SymmetricType.Central or SymmetricType.Diagonal or SymmetricType.AntiDiagonal))
		{
			throw new ArgumentOutOfRangeException(nameof(symmetricType));
		}

		var index = symmetricType switch { SymmetricType.Diagonal => 0, SymmetricType.AntiDiagonal => 1, _ => 2 };
		return Checkers[index](grid, out _, out mappingDigits, out selfPairedDigitsMask);
	}

	/// <summary>
	/// Check for the symmetry behavior on axes or center point.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <param name="symmetricType">The symmetric type.</param>
	/// <param name="nonselfPairedDigitsMask">The mask that holds a list of digits that is non-self-paired.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	internal static bool CheckAxesOrCenterPointForSymmetry(in Grid grid, SymmetricType symmetricType, Mask nonselfPairedDigitsMask)
	{
		foreach (var cell in symmetricType.GetCellsInSymmetryAxis())
		{
			switch (grid.GetState(cell))
			{
				case CellState.Given or CellState.Modifiable when (nonselfPairedDigitsMask >> grid.GetDigit(cell) & 1) != 0:
				{
					return false;
				}
				case CellState.Empty when grid.GetCandidates(cell) is var digitsMask:
				{
					var allDigitsAreNonselfPaired = true;
					foreach (var digit in digitsMask)
					{
						if ((nonselfPairedDigitsMask >> digit & 1) == 0)
						{
							allDigitsAreNonselfPaired = false;
							break;
						}
					}
					if (allDigitsAreNonselfPaired)
					{
						return false;
					}
					break;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Find GSP step if worth.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <param name="options">The options to set.</param>
	/// <returns>The found step.</returns>
	internal static GurthSymmetricalPlacementStep? GetStep(in Grid grid, StepGathererOptions options)
	{
		if (CheckDiagonal(grid, options) is { } diagonalTypeStep)
		{
			return diagonalTypeStep;
		}
		if (CheckAntiDiagonal(grid, options) is { } antiDiagonalTypeStep)
		{
			return antiDiagonalTypeStep;
		}
		if (CheckCentral(grid, options) is { } centralTypeStep)
		{
			return centralTypeStep;
		}
		return null;
	}

	/// <summary>
	/// Records all possible highlight cells.
	/// </summary>
	/// <param name="grid">The grid as reference.</param>
	/// <param name="cellOffsets">The target collection.</param>
	/// <param name="mapping">The mapping relation.</param>
	private static void GetHighlightCells(in Grid grid, List<CellViewNode> cellOffsets, ReadOnlySpan<Digit?> mapping)
	{
		var colorIndices = (stackalloc Digit[9]);
		for (var (digit, colorIndexCurrent, digitsMaskBucket) = (0, 0, (Mask)0); digit < 9; digit++)
		{
			if ((digitsMaskBucket >> digit & 1) != 0)
			{
				continue;
			}

			var currentMappingRelationDigit = mapping[digit];

			colorIndices[digit] = colorIndexCurrent;
			digitsMaskBucket |= (Mask)(1 << digit);
			if (currentMappingRelationDigit is { } relatedDigit && relatedDigit != digit)
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
	/// The internal method to check for diagonal symmetrical placement kind.
	/// </summary>
	/// <param name="grid">The grid to be check.</param>
	/// <param name="symmetricType">
	/// The symmetrical placement type if so. The value can only be <see cref="SymmetricType.Diagonal"/> or <see cref="SymmetricType.None"/>.
	/// </param>
	/// <param name="mappingDigits">The mapping digits.</param>
	/// <param name="selfPairedDigitsMask">A mask holding a list of digits being self-paired.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the grid is diagonal symmetrical placement.</returns>
	private static bool Diagonal(
		in Grid grid,
		out SymmetricType symmetricType,
		out ReadOnlySpan<Digit?> mappingDigits,
		out Mask selfPairedDigitsMask
	)
	{
		var mapping = new Digit?[9];
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < i; j++)
			{
				var c1 = i * 9 + j;
				var c2 = j * 9 + i;
				var condition = grid.GetState(c1) == CellState.Empty;
				if (condition ^ grid.GetState(c2) == CellState.Empty)
				{
					// One of two cells is empty. Not this symmetry.
					goto ReturnFalse;
				}

				if (condition)
				{
					continue;
				}

				var d1 = grid.GetDigit(c1);
				var d2 = grid.GetDigit(c2);
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
						goto ReturnFalse;
					}
				}
				else
				{
					var o1 = mapping[d1];
					var o2 = mapping[d2];
					if (o1.HasValue ^ o2.HasValue)
					{
						goto ReturnFalse;
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
						goto ReturnFalse;
					}
				}
			}
		}

		symmetricType = SymmetricType.Diagonal;
		mappingDigits = mapping;
		selfPairedDigitsMask = 0;
		for (var digit = 0; digit < 9; digit++)
		{
			if (mapping[digit] switch { { } currentDigit => currentDigit == digit, _ => true })
			{
				selfPairedDigitsMask |= (Mask)(1 << digit);
			}
		}

		// Check behavior on axes.
		if (!CheckAxesOrCenterPointForSymmetry(grid, SymmetricType.Diagonal, (Mask)(Grid.MaxCandidatesMask & ~selfPairedDigitsMask)))
		{
			goto ReturnFalse;
		}

		return true;

	ReturnFalse:
		symmetricType = SymmetricType.None;
		mappingDigits = null;
		selfPairedDigitsMask = 0;
		return false;
	}

	/// <summary>
	/// The internal method to check for anti-diagonal symmetrical placement kind.
	/// </summary>
	/// <param name="grid">The grid to be check.</param>
	/// <param name="symmetricType">
	/// The symmetrical placement type if so. The value can only be <see cref="SymmetricType.AntiDiagonal"/> or <see cref="SymmetricType.None"/>.
	/// </param>
	/// <param name="mappingDigits">The mapping digits.</param>
	/// <param name="selfPairedDigitsMask">A mask holding a list of digits being self-paired.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the grid is anti-diagonal symmetrical placement.</returns>
	private static bool AntiDiagonal(
		in Grid grid,
		out SymmetricType symmetricType,
		out ReadOnlySpan<Digit?> mappingDigits,
		out Mask selfPairedDigitsMask
	)
	{
		var mapping = new Digit?[9];
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 8 - i; j++)
			{
				var c1 = i * 9 + j;
				var c2 = (8 - j) * 9 + (8 - i);
				var condition = grid.GetState(c1) == CellState.Empty;
				if (condition ^ grid.GetState(c2) == CellState.Empty)
				{
					// One of two cells is empty. Not this symmetry.
					goto ReturnFalse;
				}

				if (condition)
				{
					continue;
				}

				var d1 = grid.GetDigit(c1);
				var d2 = grid.GetDigit(c2);
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
						goto ReturnFalse;
					}
				}
				else
				{
					var o1 = mapping[d1];
					var o2 = mapping[d2];
					if (o1.HasValue ^ o2.HasValue)
					{
						goto ReturnFalse;
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
						goto ReturnFalse;
					}
				}
			}
		}

		symmetricType = SymmetricType.AntiDiagonal;
		mappingDigits = mapping;
		selfPairedDigitsMask = 0;
		for (var digit = 0; digit < 9; digit++)
		{
			if (mapping[digit] switch { { } currentDigit => currentDigit == digit, _ => true })
			{
				selfPairedDigitsMask |= (Mask)(1 << digit);
			}
		}

		// Check behavior on axes.
		if (!CheckAxesOrCenterPointForSymmetry(grid, SymmetricType.AntiDiagonal, (Mask)(Grid.MaxCandidatesMask & ~selfPairedDigitsMask)))
		{
			goto ReturnFalse;
		}

		return true;

	ReturnFalse:
		symmetricType = SymmetricType.None;
		mappingDigits = null;
		selfPairedDigitsMask = 0;
		return false;
	}

	/// <summary>
	/// The internal method to check for central symmetrical placement kind.
	/// </summary>
	/// <param name="grid">The grid to be check.</param>
	/// <param name="symmetricType">
	/// The symmetrical placement type if so. The value can only be <see cref="SymmetricType.Central"/> or <see cref="SymmetricType.None"/>.
	/// </param>
	/// <param name="mappingDigits">The mapping digits.</param>
	/// <param name="selfPairedDigitsMask">A mask holding a list of digits being self-paired.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the grid is central symmetrical placement.</returns>
	private static bool Central(
		in Grid grid,
		out SymmetricType symmetricType,
		out ReadOnlySpan<Digit?> mappingDigits,
		out Mask selfPairedDigitsMask
	)
	{
		var mapping = new Digit?[9];
		for (var cell = 0; cell < 40; cell++)
		{
			var anotherCell = 80 - cell;
			var condition = grid.GetState(cell) == CellState.Empty;
			if (condition ^ grid.GetState(anotherCell) == CellState.Empty)
			{
				// One of two cell is empty, not central symmetry type.
				goto ReturnFalse;
			}

			if (condition)
			{
				continue;
			}

			var d1 = grid.GetDigit(cell);
			var d2 = grid.GetDigit(anotherCell);
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
					goto ReturnFalse;
				}
			}
			else
			{
				var o1 = mapping[d1];
				var o2 = mapping[d2];
				if (o1 is not null ^ o2 is not null)
				{
					goto ReturnFalse;
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
					goto ReturnFalse;
				}
			}
		}

		symmetricType = SymmetricType.Central;
		mappingDigits = mapping;
		selfPairedDigitsMask = 0;
		for (var digit = 0; digit < 9; digit++)
		{
			if (mapping[digit] switch { { } currentDigit => currentDigit == digit, _ => true })
			{
				selfPairedDigitsMask |= (Mask)(1 << digit);
			}
		}

		// Check behavior on center point (r5c5).
		if (!CheckAxesOrCenterPointForSymmetry(grid, SymmetricType.Central, (Mask)(Grid.MaxCandidatesMask & ~selfPairedDigitsMask)))
		{
			goto ReturnFalse;
		}

		return true;

	ReturnFalse:
		symmetricType = SymmetricType.None;
		mappingDigits = null;
		selfPairedDigitsMask = 0;
		return false;
	}

	/// <summary>
	/// Checks for diagonal symmetry steps.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="options">The options to set.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static GurthSymmetricalPlacementStep? CheckDiagonal(in Grid grid, StepGathererOptions options)
	{
		var diagonalHasEmptyCell = false;
		for (var i = 0; i < 9; i++)
		{
			if (grid.GetState(i * 9 + i) == CellState.Empty)
			{
				diagonalHasEmptyCell = true;
				break;
			}
		}
		if (!diagonalHasEmptyCell)
		{
			// No conclusion.
			return null;
		}

		if (!IsSymmetricalPlacement(grid, SymmetricType.Diagonal, out var mapping, out var selfPairedDigitsMask))
		{
			return null;
		}

		var nonselfPairedDigitsMask = (Mask)(Grid.MaxCandidatesMask & ~selfPairedDigitsMask);
		var selfPairedDigits = new List<Digit>();
		for (var digit = 0; digit < 9; digit++)
		{
			var mappingDigit = mapping[digit];
			if (!mappingDigit.HasValue || mappingDigit == digit)
			{
				selfPairedDigits.Add(digit);
			}
		}

		// Check whether the diagonal line contains non-self-paired digit.
		if (!CheckAxesOrCenterPointForSymmetry(grid, SymmetricType.Diagonal, nonselfPairedDigitsMask))
		{
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		var candidateOffsets = new List<CandidateViewNode>();
		var conclusions = new List<Conclusion>();
		for (var i = 0; i < 9; i++)
		{
			var cell = i * 9 + i;
			if (grid.GetState(cell) != CellState.Empty)
			{
				continue;
			}

			foreach (var digit in grid.GetCandidates(cell))
			{
				if (selfPairedDigits.Contains(digit))
				{
					candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
					continue;
				}

				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		GetHighlightCells(grid, cellOffsets, mapping);

		return conclusions.Count == 0
			? null
			: new(
				conclusions.AsMemory(),
				[[.. cellOffsets, .. candidateOffsets]],
				options,
				SymmetricType.Diagonal,
				[.. mapping]
			);
	}

	/// <summary>
	/// Checks for anti-diagonal symmetry steps.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="options">The options to set.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static GurthSymmetricalPlacementStep? CheckAntiDiagonal(in Grid grid, StepGathererOptions options)
	{
		var antiDiagonalHasEmptyCell = false;
		for (var i = 0; i < 9; i++)
		{
			if (grid.GetState(i * 9 + (8 - i)) == CellState.Empty)
			{
				antiDiagonalHasEmptyCell = true;
				break;
			}
		}
		if (!antiDiagonalHasEmptyCell)
		{
			// No conclusion.
			return null;
		}

		if (!IsSymmetricalPlacement(grid, SymmetricType.AntiDiagonal, out var mapping, out var selfPairedDigitsMask))
		{
			return null;
		}

		var nonselfPairedDigitsMask = (Mask)(Grid.MaxCandidatesMask & ~selfPairedDigitsMask);
		var selfPairedDigits = new List<Digit>();
		for (var digit = 0; digit < 9; digit++)
		{
			var mappingDigit = mapping[digit];
			if (!mappingDigit.HasValue || mappingDigit == digit)
			{
				selfPairedDigits.Add(digit);
			}
		}

		// Check whether the diagonal line contains non-self-paired digit.
		if (!CheckAxesOrCenterPointForSymmetry(grid, SymmetricType.AntiDiagonal, nonselfPairedDigitsMask))
		{
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		var candidateOffsets = new List<CandidateViewNode>();
		var conclusions = new List<Conclusion>();
		for (var i = 0; i < 9; i++)
		{
			var cell = i * 9 + (8 - i);
			if (grid.GetState(cell) != CellState.Empty)
			{
				continue;
			}

			foreach (var digit in grid.GetCandidates(cell))
			{
				if (selfPairedDigits.Contains(digit))
				{
					candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
					continue;
				}

				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		GetHighlightCells(grid, cellOffsets, mapping);

		return conclusions.Count == 0
			? null
			: new(conclusions.AsMemory(), [[.. cellOffsets, .. candidateOffsets]], options, SymmetricType.AntiDiagonal, [.. mapping]);
	}

	/// <summary>
	/// Checks for central symmetry steps.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="options">The options to set.</param>
	/// <returns>A correct step if found; otherwise, <see langword="null"/>.</returns>
	private static GurthSymmetricalPlacementStep? CheckCentral(in Grid grid, StepGathererOptions options)
	{
		if (!IsSymmetricalPlacement(grid, SymmetricType.Central, out var mapping, out var selfPairedDigitsMask))
		{
			return null;
		}

		var nonselfPairedDigitsMask = (Mask)(Grid.MaxCandidatesMask & ~selfPairedDigitsMask);
		var selfPairedDigits = new List<Digit>();
		for (var digit = 0; digit < 9; digit++)
		{
			var mappingDigit = mapping[digit];
			if (!mappingDigit.HasValue || mappingDigit == digit)
			{
				selfPairedDigits.Add(digit);
			}
		}

		// Check whether the diagonal line contains non-self-paired digit.
		if (!CheckAxesOrCenterPointForSymmetry(grid, SymmetricType.Central, nonselfPairedDigitsMask))
		{
			return null;
		}

		if (grid.GetDigit(40) != -1)
		{
			// No eliminations will be found.
			return null;
		}

		var cellOffsets = new List<CellViewNode>();
		GetHighlightCells(grid, cellOffsets, mapping);

		return new(
			(from digit in nonselfPairedDigitsMask select new Conclusion(Elimination, 40, digit)).ToArray(),
			[[.. cellOffsets]],
			options,
			SymmetricType.Central,
			[.. mapping]
		);
	}
}
