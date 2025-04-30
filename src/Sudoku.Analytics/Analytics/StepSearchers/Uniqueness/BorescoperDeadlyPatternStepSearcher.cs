namespace Sudoku.Analytics.StepSearchers.Uniqueness;

/// <summary>
/// Provides with a <b>Borescoper's Deadly Pattern</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Borescoper's Deadly Pattern Type 1</item>
/// <item>Borescoper's Deadly Pattern Type 2</item>
/// <item>Borescoper's Deadly Pattern Type 3</item>
/// <item>Borescoper's Deadly Pattern Type 4</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_BorescoperDeadlyPatternStepSearcher",
	Technique.BorescoperDeadlyPatternType1, Technique.BorescoperDeadlyPatternType2,
	Technique.BorescoperDeadlyPatternType3, Technique.BorescoperDeadlyPatternType4,
	SupportedSudokuTypes = SudokuType.Standard,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class BorescoperDeadlyPatternStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (EmptyCells.Count < 7)
		{
			return null;
		}

		ref readonly var grid = ref context.Grid;
		for (var i = 0; i < (EmptyCells.Count == 7 ? 14580 : 11664); i++)
		{
			var pattern = BorescoperDeadlyPatternPattern.Patterns[i];
			if ((EmptyCells & pattern.Map) != pattern.Map)
			{
				// The pattern contains non-empty cells.
				continue;
			}

			var map = pattern.Map;
			var ((p11, p12), (p21, p22), (c1, c2, c3, c4)) = pattern;
			var cornerMask1 = (Mask)(grid.GetCandidates(p11) | grid.GetCandidates(p12));
			var cornerMask2 = (Mask)(grid.GetCandidates(p21) | grid.GetCandidates(p22));
			var centerMask = (Mask)((Mask)(grid.GetCandidates(c1) | grid.GetCandidates(c2)) | grid.GetCandidates(c3));
			if (map.Count == 8)
			{
				centerMask |= grid.GetCandidates(c4);
			}

			if (CheckType1(grid, ref context, pattern, cornerMask1, cornerMask2, centerMask, map) is { } type1Step)
			{
				return type1Step;
			}
			if (CheckType2(grid, ref context, pattern, cornerMask1, cornerMask2, centerMask, map) is { } type2Step)
			{
				return type2Step;
			}
			if (CheckType3(grid, ref context, pattern, cornerMask1, cornerMask2, centerMask, map) is { } type3Step)
			{
				return type3Step;
			}
			if (CheckType4(grid, ref context, pattern, cornerMask1, cornerMask2, centerMask, map) is { } type4Step)
			{
				return type4Step;
			}
		}

		return null;
	}

	/// <summary>
	/// Check for type 1.
	/// </summary>
	private BorescoperDeadlyPatternType1Step? CheckType1(
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		Mask cornerMask1,
		Mask cornerMask2,
		Mask centerMask,
		in CellMap map
	)
	{
		var orMask = (Mask)((Mask)(cornerMask1 | cornerMask2) | centerMask);
		if (BitOperations.PopCount(orMask) != (pattern.IsHeptagon ? 4 : 5))
		{
			goto ReturnNull;
		}

		// Iterate on each combination.
		foreach (var digits in orMask.AllSets.GetSubsets(pattern.IsHeptagon ? 3 : 4))
		{
			var tempMask = Mask.Create(digits);
			var otherDigit = BitOperations.TrailingZeroCount((Mask)(orMask & ~tempMask));
			var mapContainingThatDigit = map & CandidatesMap[otherDigit];
			if (mapContainingThatDigit is not [var elimCell])
			{
				continue;
			}

			var elimMask = (Mask)(grid.GetCandidates(elimCell) & tempMask);
			if (elimMask == 0)
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in map)
			{
				if (!mapContainingThatDigit.Contains(cell))
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
					}
				}
			}

			var step = new BorescoperDeadlyPatternType1Step(
				(from digit in elimMask select new Conclusion(Elimination, elimCell, digit)).ToArray(),
				[[.. candidateOffsets]],
				context.Options,
				map,
				tempMask
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check type 2.
	/// </summary>
	private BorescoperDeadlyPatternType2Step? CheckType2(
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		Mask cornerMask1,
		Mask cornerMask2,
		Mask centerMask,
		in CellMap map
	)
	{
		var orMask = (Mask)((Mask)(cornerMask1 | cornerMask2) | centerMask);
		if (BitOperations.PopCount(orMask) != (pattern.IsHeptagon ? 4 : 5))
		{
			goto ReturnNull;
		}

		// Iterate on each combination.
		foreach (var digits in orMask.AllSets.GetSubsets(pattern.IsHeptagon ? 3 : 4))
		{
			var tempMask = Mask.Create(digits);
			var otherDigit = BitOperations.TrailingZeroCount((Mask)(orMask & ~tempMask));
			var mapContainingThatDigit = map & CandidatesMap[otherDigit];
			var elimMap = mapContainingThatDigit.PeerIntersection & ~map & CandidatesMap[otherDigit];
			if (!elimMap)
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in map)
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					candidateOffsets.Add(new(digit == otherDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell * 9 + digit));
				}
			}

			var step = new BorescoperDeadlyPatternType2Step(
				(from cell in elimMap select new Conclusion(Elimination, cell, otherDigit)).ToArray(),
				[[.. candidateOffsets]],
				context.Options,
				map,
				tempMask,
				otherDigit
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	private BorescoperDeadlyPatternType3Step? CheckType3(
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		Mask cornerMask1,
		Mask cornerMask2,
		Mask centerMask,
		in CellMap map
	)
	{
		var orMask = (Mask)((Mask)(cornerMask1 | cornerMask2) | centerMask);
		foreach (var houseIndex in map.Houses)
		{
			var currentMap = HousesMap[houseIndex] & map;
			var otherCellsMap = map & ~currentMap;
			var otherMask = grid[otherCellsMap];
			foreach (var digits in orMask.AllSets.GetSubsets(pattern.IsHeptagon ? 3 : 4))
			{
				var tempMask = Mask.Create(digits);
				if (otherMask != tempMask)
				{
					continue;
				}

				// Iterate on the cells by the specified size.
				var iterationCellsMap = HousesMap[houseIndex] & ~currentMap & EmptyCells;
				var otherDigitsMask = (Mask)(orMask & ~tempMask);
				for (var size = BitOperations.PopCount(otherDigitsMask) - 1; size < iterationCellsMap.Count; size++)
				{
					foreach (ref readonly var combination in iterationCellsMap & size)
					{
						var comparer = grid[combination];
						if ((tempMask & comparer) != 0 || BitOperations.PopCount(tempMask) - 1 != size
							|| (tempMask & otherDigitsMask) != otherDigitsMask)
						{
							continue;
						}

						// Type 3 found.
						// Now check eliminations.
						var conclusions = new List<Conclusion>();
						foreach (var digit in comparer)
						{
							if ((iterationCellsMap & CandidatesMap[digit]) is var cells and not [])
							{
								foreach (var cell in cells)
								{
									conclusions.Add(new(Elimination, cell, digit));
								}
							}
						}
						if (conclusions.Count == 0)
						{
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>();
						foreach (var cell in currentMap)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(
									new(
										(tempMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
										cell * 9 + digit
									)
								);
							}
						}
						foreach (var cell in otherCellsMap)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
							}
						}
						foreach (var cell in combination)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
							}
						}

						var step = new BorescoperDeadlyPatternType3Step(
							conclusions.AsMemory(),
							[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, houseIndex)]],
							context.Options,
							map,
							tempMask,
							combination,
							otherDigitsMask
						);
						if (context.OnlyFindOne)
						{
							return step;
						}

						context.Accumulator.Add(step);
					}
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Check for type 4.
	/// </summary>
	private BorescoperDeadlyPatternType4Step? CheckType4(
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		Mask cornerMask1,
		Mask cornerMask2,
		Mask centerMask,
		in CellMap map
	)
	{
		// The type 4 may be complex and terrible to process.
		// All houses that the pattern lies in should be checked.
		var orMask = (Mask)((Mask)(cornerMask1 | cornerMask2) | centerMask);
		foreach (var houseIndex in map.Houses)
		{
			var currentMap = HousesMap[houseIndex] & map;
			var otherCellsMap = map & ~currentMap;
			var otherMask = grid[otherCellsMap];

			// Iterate on each possible digit combination.
			// For example, if values are { 1, 2, 3 }, then all combinations taken 2 values
			// are { 1, 2 }, { 2, 3 } and { 1, 3 }.
			foreach (var digits in orMask.AllSets.GetSubsets(pattern.IsHeptagon ? 3 : 4))
			{
				var tempMask = Mask.Create(digits);
				if (otherMask != tempMask)
				{
					continue;
				}

				// Iterate on each combination.
				// Only one digit should be eliminated, and other digits should form a "conjugate house".
				// In a so-called conjugate house, the digits can only appear in these cells in this house.
				foreach (var combination in (tempMask & orMask).AllSets.GetSubsets(currentMap.Count - 1))
				{
					var combinationMask = (Mask)0;
					var combinationMap = CellMap.Empty;
					var flag = false;
					foreach (var digit in combination)
					{
						if (ValuesMap[digit] && HousesMap[houseIndex])
						{
							flag = true;
							break;
						}

						combinationMask |= (Mask)(1 << digit);
						combinationMap |= CandidatesMap[digit] & HousesMap[houseIndex];
					}
					if (flag)
					{
						// The house contains digit value, which is not a normal pattern.
						continue;
					}

					if (combinationMap != currentMap)
					{
						// If not equal, the map may contains other digits in this house.
						// Therefore the conjugate house can't form.
						continue;
					}

					// Type 4 forms. Now check eliminations.
					var finalDigits = (Mask)(tempMask & ~combinationMask);
					var possibleCandMaps = CellMap.Empty;
					foreach (var finalDigit in finalDigits)
					{
						possibleCandMaps |= CandidatesMap[finalDigit];
					}
					if ((combinationMap & possibleCandMaps) is not (var elimMap and not []))
					{
						continue;
					}

					var conclusions = new List<Conclusion>();
					foreach (var cell in elimMap)
					{
						foreach (var digit in finalDigits)
						{
							if (CandidatesMap[digit].Contains(cell))
							{
								conclusions.Add(new(Elimination, cell, digit));
							}
						}
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var cell in currentMap)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & combinationMask))
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
						}
					}
					foreach (var cell in otherCellsMap)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
						}
					}

					var step = new BorescoperDeadlyPatternType4Step(
						conclusions.AsMemory(),
						[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, houseIndex)]],
						context.Options,
						map,
						otherMask,
						currentMap,
						combinationMask
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}

		return null;
	}
}
