namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with an <b>Anonymous Deadly Pattern</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Anonymous Deadly Pattern:
/// <list type="bullet">
/// <item>Anonymous Deadly Pattern Type 1</item>
/// <item>Anonymous Deadly Pattern Type 2</item>
/// <item>Anonymous Deadly Pattern Type 3</item>
/// <item>Anonymous Deadly Pattern Type 4</item>
/// </list>
/// </item>
/// <item>
/// Rotating Deadly Pattern:
/// <list type="bullet">
/// <item>Rotating Deadly Pattern Type 1</item>
/// <item>Rotating Deadly Pattern Type 2</item>
/// <item>Rotating Deadly Pattern Type 3</item>
/// <item>Rotating Deadly Pattern Type 4</item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_AnonymousDeadlyPatternStepSearcher",
	Technique.AnonymousDeadlyPatternType1, Technique.AnonymousDeadlyPatternType2,
	Technique.AnonymousDeadlyPatternType3, Technique.AnonymousDeadlyPatternType4,
	Technique.RotatingDeadlyPatternType1, Technique.RotatingDeadlyPatternType2,
	Technique.RotatingDeadlyPatternType3, Technique.RotatingDeadlyPatternType4,
	SupportsSukaku = false,
	SupportsAnalyzingPuzzleHavingMultipleSolutions = false)]
public sealed partial class AnonymousDeadlyPatternStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates all 7-cell patterns.
	/// </summary>
	private static readonly ReadOnlyMemory<CellMap> SevenCellPatterns = Construct7Cells();

	/// <summary>
	/// Indicates all 8-cell patterns.
	/// </summary>
	private static readonly ReadOnlyMemory<CellMap> EightCellPatterns = Construct8Cells();


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (Collect_7Cells(ref context) is { } sevenCellsStep)
		{
			return sevenCellsStep;
		}
		if (Collect_8Cells(ref context) is { } eightCellsStep)
		{
			return eightCellsStep;
		}
		if (Collect_8Cells_Rotating(ref context) is { } eightCells2Step)
		{
			return eightCells2Step;
		}
		if (Collect_9Cells(ref context) is { } nineCellsStep)
		{
			return nineCellsStep;
		}
		return null;
	}

	/// <remarks>
	/// There's only 1 pattern:
	/// <code><![CDATA[
	/// 123 .  .  |  123 .  .  |  123 .  .
	/// 123 .  .  |  123 .  .  |  .   .  .
	/// 123 .  .  |  .   .  .  |  123 .  .
	/// ]]></code>
	/// </remarks>
	private AnonymousDeadlyPatternStep? Collect_7Cells(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;

		// Iterate on each pattern.
		foreach (ref readonly var pattern in SevenCellPatterns)
		{
			if ((EmptyCells & pattern) != pattern)
			{
				// There's any non-empty cell.
				continue;
			}

			// Check the number of digits appeared in the pattern, and determine which type it would be.
			var digitsMask = grid[pattern];

			// Adds an filter nearly same as anonymous deadly pattern, but with 3 different digits.
			var possiblePatternDigitsMask = (Mask)0;
			foreach (var digit in digitsMask)
			{
				if ((pattern & CandidatesMap[digit]).Count >= 3)
				{
					possiblePatternDigitsMask |= (Mask)(1 << digit);
				}
			}
			if (BitOperations.PopCount((uint)possiblePatternDigitsMask) < 3)
			{
				continue;
			}

			// Iterate on each combination.
			foreach (var combination in possiblePatternDigitsMask.AllSets & 3)
			{
				// Try to get all cells that holds extra digits.
				var currentCombinationDigitsMask = Mask.Create(combination);
				var extraDigitsMask = (Mask)(digitsMask & ~currentCombinationDigitsMask);
				var extraCells = CellMap.Empty;
				foreach (var cell in pattern)
				{
					if ((grid.GetCandidates(cell) & extraDigitsMask) != 0)
					{
						extraCells += cell;
					}
				}
				if (extraCells.Count >= 2 && extraCells.FirstSharedHouse == FallbackConstants.@int)
				{
					// All extra cells must share with a same house.
					continue;
				}

				if (!VerifyPattern(grid, pattern, extraDigitsMask, out var p))
				{
					// The pattern cannot be passed to be verified.
					continue;
				}

				switch (BitOperations.PopCount((uint)extraDigitsMask))
				{
					case 0:
					{
						throw new PuzzleInvalidException(grid, typeof(AnonymousDeadlyPatternStep));
					}
					case 1:
					{
						var extraDigit = BitOperations.Log2((uint)extraDigitsMask);
						if (CheckType1Or2(
							ref context, grid, pattern, currentCombinationDigitsMask, extraDigit, p,
							(pattern & CandidatesMap[extraDigit]).Count == 1
								? Technique.AnonymousDeadlyPatternType1
								: Technique.AnonymousDeadlyPatternType2
						) is { } type1Or2Step)
						{
							return type1Or2Step;
						}
						continue;
					}
					case 2:
					{
						if (CheckType3(
							ref context, grid, pattern, currentCombinationDigitsMask,
							extraDigitsMask, extraCells, p, Technique.AnonymousDeadlyPatternType3) is { } type3Step)
						{
							return type3Step;
						}
						goto default;
					}
					default:
					{
						if (CheckType4(
							ref context, grid, pattern, currentCombinationDigitsMask,
							extraDigitsMask, extraCells, p, Technique.AnonymousDeadlyPatternType4) is { } type4Step)
						{
							return type4Step;
						}
						break;
					}
				}
			}
		}
		return null;
	}

	/// <remarks>
	/// There're 2 patterns:
	/// <code><![CDATA[
	/// (1) UL + XR, 3 digits
	/// 12 .  .  |  23 .  .  |  13 .  .
	/// .  12 .  |  23 .  .  |  13 .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 12 12 .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	///
	/// (2) XR + UR, 3 digits
	/// 12 23 .  |  13 .  .  |  .  .  .
	/// .  .  13 |  13 .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 12 23 13 |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ]]></code>
	/// </remarks>
	private AnonymousDeadlyPatternStep? Collect_8Cells(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		foreach (ref readonly var pattern in EightCellPatterns)
		{
			if ((EmptyCells & pattern) != pattern)
			{
				// The pattern may contain non-empty cells. Skip it.
				continue;
			}

			// Check the number of digits appeared in the pattern, and determine which type it would be.
			var digitsMask = grid[pattern];

			// The pattern may be very complex to be checked, so we should append an extra check here:
			// determine which digits can be used as a deadly pattern, and which are not.
			// Obviously, both patterns of size 8 should use 3 digits, and two of them must use >= 6 times,
			// and the last one must use >= 4 times.
			// We should check which digits can be used in pattern. If the number of found digits are not enough 3,
			// the pattern will also be ignored to be checked.
			var (greaterThan6Digits, greaterThan4Digits) = ((Mask)0, (Mask)0);
			foreach (var digit in digitsMask)
			{
				var m = (pattern & CandidatesMap[digit]).Count;
				if (m >= 6)
				{
					greaterThan6Digits |= (Mask)(1 << digit);
				}
				else if (m >= 4)
				{
					greaterThan4Digits |= (Mask)(1 << digit);
				}
			}
			if (BitOperations.PopCount((uint)greaterThan6Digits) < 2
				|| (Mask)(greaterThan4Digits | greaterThan6Digits) is var possiblePatternDigitsMask
				&& BitOperations.PopCount((uint)possiblePatternDigitsMask) < 3)
			{
				continue;
			}

			// Iterate on each combination.
			foreach (var combination in possiblePatternDigitsMask.AllSets & 3)
			{
				// Try to get all cells that holds extra digits.
				var currentCombinationDigitsMask = Mask.Create(combination);
				var extraDigitsMask = (Mask)(digitsMask & ~currentCombinationDigitsMask);
				var extraCells = CellMap.Empty;
				foreach (var cell in pattern)
				{
					if ((grid.GetCandidates(cell) & extraDigitsMask) != 0)
					{
						extraCells += cell;
					}
				}
				if (extraCells.Count >= 2 && extraCells.FirstSharedHouse == FallbackConstants.@int)
				{
					// All extra cells must share with a same house.
					continue;
				}

				if (!VerifyPattern(grid, pattern, extraDigitsMask, out var p))
				{
					// The pattern cannot be passed to be verified.
					continue;
				}

				switch (BitOperations.PopCount((uint)extraDigitsMask))
				{
					case 0:
					{
						throw new PuzzleInvalidException(grid, typeof(AnonymousDeadlyPatternStep));
					}
					case 1:
					{
						var extraDigit = BitOperations.Log2((uint)extraDigitsMask);
						if (CheckType1Or2(
							ref context, grid, pattern, currentCombinationDigitsMask, extraDigit, p,
							(pattern & CandidatesMap[extraDigit]).Count == 1
								? Technique.AnonymousDeadlyPatternType1
								: Technique.AnonymousDeadlyPatternType2
						) is { } type1Or2Step)
						{
							return type1Or2Step;
						}
						continue;
					}
					case 2:
					{
						if (CheckType3(
							ref context, grid, pattern, currentCombinationDigitsMask,
							extraDigitsMask, extraCells, p, Technique.AnonymousDeadlyPatternType3) is { } type3Step)
						{
							return type3Step;
						}
						goto default;
					}
					default:
					{
						if (CheckType4(
							ref context, grid, pattern, currentCombinationDigitsMask,
							extraDigitsMask, extraCells, p, Technique.AnonymousDeadlyPatternType4) is { } type4Step)
						{
							return type4Step;
						}
						break;
					}
				}
			}
		}
		return null;
	}

	/// <remarks>
	/// There's only 1 pattern:
	/// <code><![CDATA[
	/// 12 .  .  |  24 .  .  |  14 .  .
	/// 23 .  .  |  23 .  .  |  .  .  .
	/// 13 .  .  |  34 .  .  |  14 .  .
	/// ]]></code>
	/// </remarks>
	private AnonymousDeadlyPatternStep? Collect_8Cells_Rotating(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;

		// Iterate on each pattern. Here the pattern cells is same as patterns used in Unique Matrix pattern.
		// We should remove one cell from the pattern, to make the pattern become a possible Rotating Deadly Pattern.
		foreach (ref readonly var cells in UniqueMatrixStepSearcher.Patterns)
		{
			// Iterate on each cell as missing cell.
			foreach (var missingCell in cells)
			{
				// Check whether all last 8 cells are empty cells.
				var pattern = cells - missingCell;
				if ((EmptyCells & pattern) != pattern)
				{
					// The pattern may contain non-empty cells. Skip it.
					continue;
				}

				// Check the number of digits appeared in the pattern, and determine which type it would be.
				var digitsMask = grid[pattern];

				// Adds an filter nearly same as anonymous deadly pattern, but with 4 different digits.
				var possiblePatternDigitsMask = (Mask)0;
				foreach (var digit in digitsMask)
				{
					if ((pattern & CandidatesMap[digit]).Count >= 4)
					{
						possiblePatternDigitsMask |= (Mask)(1 << digit);
					}
				}
				if (BitOperations.PopCount((uint)possiblePatternDigitsMask) < 4)
				{
					continue;
				}

				// Iterate on each combination.
				foreach (var combination in possiblePatternDigitsMask.AllSets & 4)
				{
					// Try to get all cells that holds extra digits.
					var currentCombinationDigitsMask = Mask.Create(combination);
					var extraDigitsMask = (Mask)(digitsMask & ~currentCombinationDigitsMask);
					var extraCells = CellMap.Empty;
					foreach (var cell in pattern)
					{
						if ((grid.GetCandidates(cell) & extraDigitsMask) != 0)
						{
							extraCells += cell;
						}
					}
					if (extraCells.Count >= 2 && extraCells.FirstSharedHouse == FallbackConstants.@int)
					{
						// All extra cells must share with a same house.
						continue;
					}

					if (!VerifyPattern(grid, pattern, extraDigitsMask, out var p))
					{
						// The pattern cannot be passed to be verified.
						continue;
					}

					switch (BitOperations.PopCount((uint)extraDigitsMask))
					{
						case 0:
						{
							throw new PuzzleInvalidException(grid, typeof(AnonymousDeadlyPatternStep));
						}
						case 1:
						{
							var extraDigit = BitOperations.Log2((uint)extraDigitsMask);
							if (CheckType1Or2(
								ref context, grid, pattern, currentCombinationDigitsMask, extraDigit, p,
								(pattern & CandidatesMap[extraDigit]).Count == 1
									? Technique.RotatingDeadlyPatternType1
									: Technique.RotatingDeadlyPatternType2
							) is { } type1Or2Step)
							{
								return type1Or2Step;
							}
							continue;
						}
						case 2:
						{
							if (CheckType3(
								ref context, grid, pattern, currentCombinationDigitsMask,
								extraDigitsMask, extraCells, p, Technique.RotatingDeadlyPatternType3) is { } type3Step)
							{
								return type3Step;
							}
							goto default;
						}
						default:
						{
							if (CheckType4(
								ref context, grid, pattern, currentCombinationDigitsMask,
								extraDigitsMask, extraCells, p, Technique.RotatingDeadlyPatternType4) is { } type4Step)
							{
								return type4Step;
							}
							break;
						}
					}
				}
			}
		}
		return null;
	}

	/// <remarks>
	/// There're 3 patterns:
	/// <code><![CDATA[
	/// (1) 2URs + XR, 3 digits
	/// 12 .  .  |  12 .  .  |  .  .  .
	/// .  23 .  |  23 .  .  |  .  .  .
	/// .  .  13 |  13 .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 12 23 13 |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	///
	/// (2) 2XRs, 4 digits
	/// 12 24 .  |  14 .  .  |  .  .  .
	/// 13 .  34 |  14 .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 23 24 34 |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	///
	/// (3) BDP + UR, 3 digits
	/// 12 .  .  |  12 .  .  |  .  .  .
	/// 23 .  .  |  .  .  .  |  23 .  .
	/// .  13 .  |  12 .  .  |  23 .  .
	/// ---------+-----------+---------
	/// 13 13 .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ]]></code>
	/// </remarks>
	private AnonymousDeadlyPatternStep? Collect_9Cells(ref StepAnalysisContext context)
	{
		return null;
	}

	/// <summary>
	/// Check for type 1 and 2.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digitsMask">The digits used.</param>
	/// <param name="targetDigit">The target digit.</param>
	/// <param name="p">All candidates used.</param>
	/// <param name="technique">The technique.</param>
	/// <returns>The found step.</returns>
	private AnonymousDeadlyPatternStep? CheckType1Or2(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap pattern,
		Mask digitsMask,
		Digit targetDigit,
		in CandidateMap p,
		Technique technique
	)
	{
		// You have found a deadly pattern with 8 cells without any names! Now check eliminations.
		var extraCells = CandidatesMap[targetDigit] & pattern;
		var eliminations = extraCells is [var extraCell]
			?
			from digit in (Mask)(grid.GetCandidates(extraCell) & ~(1 << targetDigit))
			select new Conclusion(Elimination, extraCell, digit)
			: from cell in extraCells % CandidatesMap[targetDigit] select new Conclusion(Elimination, cell, targetDigit);
		if (eliminations.Length == 0)
		{
			return null;
		}

		if (extraCells.Count == 1)
		{
			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in pattern - extraCells[0])
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					candidateOffsets.Add(new(ColorDescriptorAlias.Normal, cell * 9 + digit));
				}
			}

			var step = new AnonymousDeadlyPatternType1Step(
				eliminations.ToArray(),
				[[.. candidateOffsets]],
				context.Options,
				p,
				extraCells[0],
				digitsMask,
				technique
			);
			if (context.OnlyFindOne)
			{
				return step;
			}
			context.Accumulator.Add(step);
		}
		else
		{
			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in pattern)
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					candidateOffsets.Add(
						new(
							digit == targetDigit ? ColorDescriptorAlias.Auxiliary1 : ColorDescriptorAlias.Normal,
							cell * 9 + digit
						)
					);
				}
			}

			var step = new AnonymousDeadlyPatternType2Step(
				eliminations.ToArray(),
				[[.. candidateOffsets]],
				context.Options,
				p,
				extraCells,
				targetDigit,
				technique
			);
			if (context.OnlyFindOne)
			{
				return step;
			}
			context.Accumulator.Add(step);
		}
		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digitsMask">The digits used.</param>
	/// <param name="extraDigitsMask">The extra digits.</param>
	/// <param name="extraCells">Indicates the extra cells used.</param>
	/// <param name="p">All candidates used.</param>
	/// <param name="technique">The technique.</param>
	/// <returns>The found step.</returns>
	private AnonymousDeadlyPatternType3Step? CheckType3(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap pattern,
		Mask digitsMask,
		Mask extraDigitsMask,
		in CellMap extraCells,
		in CandidateMap p,
		Technique technique
	)
	{
		// Iterate on each shared house, to find a subset.
		foreach (var house in extraCells.SharedHouses)
		{
			var availableCells = HousesMap[house] & EmptyCells & ~pattern;
			if (availableCells.Count <= 2)
			{
				continue;
			}

			// Iterate each combination.
			foreach (ref readonly var subsetCells in availableCells | availableCells.Count - 1)
			{
				var subsetDigitsMask = (Mask)(grid[subsetCells] | extraDigitsMask);
				if (BitOperations.PopCount((uint)subsetDigitsMask) != subsetCells.Count + 1)
				{
					// The (n) digits should be inside (n - 1) cells.
					continue;
				}

				var eliminations = new List<Conclusion>();
				foreach (var cell in availableCells & ~subsetCells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & subsetDigitsMask))
					{
						eliminations.Add(new(Elimination, cell, digit));
					}
				}
				if (eliminations.Count == 0)
				{
					// No eliminations found.
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in pattern)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								HousesMap[house].Contains(cell) && (subsetDigitsMask >> digit & 1) != 0
									? ColorDescriptorAlias.Auxiliary1
									: ColorDescriptorAlias.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in subsetCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary1, cell * 9 + digit));
					}
				}

				var step = new AnonymousDeadlyPatternType3Step(
					eliminations.AsMemory(),
					[[.. candidateOffsets, new HouseViewNode(ColorDescriptorAlias.Normal, house)]],
					context.Options,
					p,
					pattern,
					subsetCells,
					subsetDigitsMask,
					technique
				);
				if (context.OnlyFindOne)
				{
					return step;
				}
				context.Accumulator.Add(step);
			}
		}
		return null;
	}

	/// <summary>
	/// Check for type 4.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digitsMask">The digits used.</param>
	/// <param name="extraDigitsMask">The extra digits.</param>
	/// <param name="extraCells">Indicates the extra cells used.</param>
	/// <param name="p">All candidates used.</param>
	/// <param name="technique">The technique.</param>
	/// <returns>The found step.</returns>
	private AnonymousDeadlyPatternType4Step? CheckType4(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap pattern,
		Mask digitsMask,
		Mask extraDigitsMask,
		in CellMap extraCells,
		in CandidateMap p,
		Technique technique
	)
	{
		if (extraCells.Count is 0 or 1)
		{
			return null;
		}

		var house = extraCells.FirstSharedHouse;

		// Check whether at least one digit hold a conjugate pair inside the extra cells.
		var (conclusions, conjugatePairDigitsMask) = (new List<Conclusion>(), (Mask)0);
		foreach (var digit in (Mask)(grid[extraCells] & digitsMask))
		{
			var conjugatePairCells = extraCells & CandidatesMap[digit];
			if (conjugatePairCells == (CandidatesMap[digit] & HousesMap[house]))
			{
				conjugatePairDigitsMask |= (Mask)(1 << digit);
			}
		}
		if (BitOperations.PopCount((uint)conjugatePairDigitsMask) != extraCells.Count - 1)
		{
			// The number of conjugate pairs must be less than the number of extra cells of 1.
			return null;
		}

		foreach (var digit in (Mask)(digitsMask & ~conjugatePairDigitsMask))
		{
			foreach (var cell in extraCells & CandidatesMap[digit])
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		if (conclusions.Count == 0)
		{
			// No conclusions found.
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in pattern & ~extraCells)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(ColorDescriptorAlias.Normal, cell * 9 + digit));
			}
		}
		foreach (var digit in conjugatePairDigitsMask)
		{
			var map = extraCells & CandidatesMap[digit];
			foreach (var cell in map)
			{
				candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary1, cell * 9 + digit));
			}
		}

		var step = new AnonymousDeadlyPatternType4Step(
			conclusions.AsMemory(),
			[[.. candidateOffsets, new HouseViewNode(ColorDescriptorAlias.Normal, house)]],
			context.Options,
			p,
			house,
			conjugatePairDigitsMask,
			technique
		);
		if (context.OnlyFindOne)
		{
			return step;
		}
		context.Accumulator.Add(step);
		return null;
	}


	/// <summary>
	/// Try to verify whether a pattern is a deadly pattern without the specified digits from a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digits">The digits.</param>
	/// <param name="c">Indicates all candidates used.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private static bool VerifyPattern(in Grid grid, in CellMap pattern, Mask digits, out CandidateMap c)
	{
		var emptyGrid = Grid.Empty;
		var truths = SpaceSet.Empty;
		foreach (var cell in pattern)
		{
			emptyGrid.SetCandidates(cell, (Mask)(grid.GetCandidates(cell) & ~digits));
			truths += Space.RowColumn(cell / 9, cell % 9);
		}

		var result = DeadlyPatternChecker.CheckWhetherFormsDeadlyPattern(
			emptyGrid,
			truths,
			new() { IgnorePotentialSinglePositionInHouse = true }
		);
		c = result.PatternCandidates;
		return result.IsDeadlyPattern;
	}

	/// <summary>
	/// Construct patterns for 7 cells.
	/// </summary>
	private static ReadOnlyMemory<CellMap> Construct7Cells()
	{
		var result = new List<CellMap>();
		foreach (var rowsCombination in Digits & 3)
		{
			foreach (var columnsCombination in Digits & 3)
			{
				// Find for each 3 * 3 cells.
				var originalPatternCells = (rowsCombination[0] * 9 + columnsCombination[0]).AsCellMap()
					+ (rowsCombination[0] * 9 + columnsCombination[1])
					+ (rowsCombination[0] * 9 + columnsCombination[2])
					+ (rowsCombination[1] * 9 + columnsCombination[0])
					+ (rowsCombination[1] * 9 + columnsCombination[1])
					+ (rowsCombination[1] * 9 + columnsCombination[2])
					+ (rowsCombination[2] * 9 + columnsCombination[0])
					+ (rowsCombination[2] * 9 + columnsCombination[1])
					+ (rowsCombination[2] * 9 + columnsCombination[2]);
				if (BitOperations.PopCount((uint)originalPatternCells.BlockMask) != 3)
				{
					// Skip for same-block cells.
					continue;
				}

				// Now we have a standard 3 * 3 cells.

				// We should remove out 2 cells from the pattern. Iterate on all pair combinations of 9 cells.
			loop_pair:
				foreach (ref readonly var pair in originalPatternCells & 2)
				{
					// Remove out those two.
					var patternCells = originalPatternCells & ~pair;
					foreach (var house in patternCells.Houses)
					{
						if ((HousesMap[house] & patternCells).Count == 1)
						{
							continue loop_pair;
						}
					}

					// Add result into collection.
					result.AddRef(patternCells);
				}
			}
		}
		return result.AsMemory();
	}

	/// <summary>
	/// Construct patterns for 8 cells.
	/// </summary>
	private static ReadOnlyMemory<CellMap> Construct8Cells()
	{
		// We can unify two patterns of 8 cells into one, by using extended rectangles.
		// Enumerate all possible XR patterns with 6 cells, and enumerate all cells.
		// For each cell, we will remove it and insert a unique rectangle pattern that covers the current cell.
		var result = new HashSet<CellMap>();
		foreach (var pattern in ExtendedRectanglePattern.AllPatterns[..1620]) // [0..1620] -> XR size 6
		{
			var patternCells = pattern.PatternCells;
			foreach (var missingCell in patternCells)
			{
				// Determine which side has more cells (row or column).
				var lastPatternCells = patternCells - missingCell;
				var row = missingCell.GetHouse(HouseType.Row);
				var column = missingCell.GetHouse(HouseType.Column);
				var rowCells = patternCells & HousesMap[row];
				var columnCells = patternCells & HousesMap[column];
				var isRow = rowCells.Count > columnCells.Count;
				var targetHouse = pattern.IsFat ? isRow ? column : row : isRow ? row : column;
				var chute = default(Chute);
				foreach (ref readonly var c in Chute.Chutes)
				{
					if ((c.HousesMask >> targetHouse & 1) != 0)
					{
						chute = c;
						break;
					}
				}

				// Iterate the other two houses that exclude the current cell, and find all possible cells.
				// For each cell, we should treat it as the diagonal cell of UR pattern with the missing cell.
				// If two cells are RxCy and RzCw, all 4 UR cells are RxCy, RxCw, RzCy and RzCw.
				var (x, y) = (missingCell / 9, missingCell % 9);
				foreach (var house in chute.HousesMask & ~(1 << targetHouse))
				{
					foreach (var cell in HousesMap[house])
					{
						if ((missingCell.AsCellMap() + cell).FirstSharedHouse != FallbackConstants.@int)
						{
							continue;
						}

						// Add it into the whole pattern.
						var (z, w) = (cell / 9, cell % 9);
						var extraCells = cell.AsCellMap() + (x * 9 + w) + (z * 9 + y);
						if (lastPatternCells & extraCells)
						{
							continue;
						}

						result.AddRef(lastPatternCells | extraCells);
					}
				}
			}
		}
		return result.ToArray();
	}
}
