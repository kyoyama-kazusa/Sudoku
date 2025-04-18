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
	/// <summary>
	/// Indicates all possible patterns to iterate.
	/// </summary>
	/// <remarks>
	/// Please note that all possible heptagons and octagons are in here.
	/// </remarks>
	private static readonly BorescoperDeadlyPatternPattern[] Patterns;

	/// <summary>
	/// Indicates the quadruple list that describes the chosen cells in the target block.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly BlockIndex[][] OffsetQuadruples = [
		[0, 1, 3, 4], [1, 2, 4, 5], [3, 4, 6, 7],
		[4, 5, 7, 8], [0, 2, 3, 5], [3, 5, 6, 8],
		[0, 1, 6, 7], [1, 2, 7, 8], [0, 2, 6, 8]
	];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static BorescoperDeadlyPatternStepSearcher()
	{
		var count = 0;
		Patterns = new BorescoperDeadlyPatternPattern[14580];
		for (var block = 0; block < 9; block++)
		{
			for (var i = 0; i < 9; i++) // 9 cases.
			{
				var quadruple = OffsetQuadruples[i];
				var tempQuadruple = new Cell[4];
				for (var j = 0; j < 4; j++)
				{
					// Set all indices to cell offsets.
					tempQuadruple[j] = (block / 3 * 3 + quadruple[j] / 3) * 9 + block % 3 * 3 + quadruple[j] % 3;
				}

				collectHeptagonPatterns(block, i, tempQuadruple, ref count);
				collectOctagonPatterns(block, i, tempQuadruple, ref count);
			}
		}


		static void collectHeptagonPatterns(House block, int i, Cell[] quadruple, ref int count)
		{
			if (quadruple is not [var q1, var q2, var q3, var q4])
			{
				return;
			}

			var blockTriplets = (ReadOnlySpan<(Cell, Cell, Cell)>)[(q1, q2, q3), (q2, q1, q4), (q3, q1, q4), (q4, q2, q3)];
			for (var j = 0; j < 4; j++)
			{
				_ = blockTriplets[j] is (var t1, var t2, var t3) triplet;
				var house1 = (t1.AsCellMap() + t2).SharedLine;
				var house2 = (t1.AsCellMap() + t3).SharedLine;
				var pair1 = new Cell[6, 2];
				var pair2 = new Cell[6, 2];
				var (o1, o2) = i switch { >= 0 and <= 3 => (9, 1), 4 or 5 => (9, 2), 6 or 7 => (18, 1), 8 => (18, 2) };
				if (house1 is >= 9 and < 18)
				{
					// 'house1' is a row and 'house2' is a column.
					r(block, house1, pair1, o1, j);
					r(block, house2, pair2, o2, j);
				}
				else
				{
					// 'house1' is a column and 'house2' is a row.
					r(block, house1, pair1, o2, j);
					r(block, house2, pair2, o1, j);
				}

				for (var i1 = 0; i1 < 6; i1++)
				{
					for (var i2 = 0; i2 < 6; i2++)
					{
						// Now check extra digits.
						var allCells = (ReadOnlySpan<Cell>)[.. triplet, pair1[i1, 0], pair1[i1, 1], pair2[i2, 0], pair2[i2, 1]];
						var v = 0L;
						for (var z = 0; z < allCells.Length; z++)
						{
							v |= (long)allCells[z];

							if (z != allCells.Length - 1)
							{
								v <<= 7;
							}
							if (z == 2)
							{
								v |= 127;
								v <<= 7;
							}
						}

						Patterns[count++] = new(v);
					}
				}
			}
		}

		static void collectOctagonPatterns(House block, int i, Cell[] quad, ref int count)
		{
			if (quad is not [var t1, var t2, var t3, _])
			{
				return;
			}

			var house1 = (t1.AsCellMap() + t2).SharedLine;
			var house2 = (t1.AsCellMap() + t3).SharedLine;
			var pair1 = new Cell[6, 2];
			var pair2 = new Cell[6, 2];
			var (o1, o2) = i switch { >= 0 and <= 3 => (9, 1), 4 or 5 => (9, 2), 6 or 7 => (18, 1), 8 => (18, 2) };
			if (house1 is >= 9 and < 18)
			{
				// 'house1' is a row and 'house2' is a column.
				r(block, house1, pair1, o1, 0);
				r(block, house2, pair2, o2, 0);
			}
			else
			{
				// 'house1' is a column and 'house2' is a row.
				r(block, house1, pair1, o2, 0);
				r(block, house2, pair2, o1, 0);
			}

			for (var i1 = 0; i1 < 6; i1++)
			{
				for (var i2 = 0; i2 < 6; i2++)
				{
					// Now check extra digits.
					var allCells = (ReadOnlySpan<Cell>)[.. quad, pair1[i1, 0], pair1[i1, 1], pair2[i2, 0], pair2[i2, 1]];
					var v = 0L;
					for (var z = 0; z < allCells.Length; z++)
					{
						var cell = allCells[z];
						v |= (long)cell;
						if (z != allCells.Length - 1)
						{
							v <<= 7;
						}
					}

					Patterns[count++] = new(v);
				}
			}
		}

		static void r(House block, House houseIndex, Cell[,] pair, int increment, int index)
		{
			for (var (i, cur) = (0, 0); i < 9; i++)
			{
				var cell = HousesCells[houseIndex][i];
				if (block == cell.ToHouse(HouseType.Block))
				{
					continue;
				}

				(pair[cur, 0], pair[cur, 1]) = index switch
				{
					0 => (cell, cell + increment),
					1 => houseIndex is >= 18 and < 27 ? (cell - increment, cell) : (cell, cell + increment),
					2 => houseIndex is >= 9 and < 18 ? (cell - increment, cell) : (cell, cell + increment),
					3 => (cell - increment, cell)
				};
				cur++;
			}
		}
	}


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (EmptyCells.Count < 7)
		{
			return null;
		}

		ref readonly var grid = ref context.Grid;
		var accumulator = context.Accumulator!;
		var onlyFindOne = context.OnlyFindOne;
		for (var i = 0; i < (EmptyCells.Count == 7 ? 14580 : 11664); i++)
		{
			var pattern = Patterns[i];
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

			if (CheckType1(accumulator, grid, ref context, pattern, onlyFindOne, cornerMask1, cornerMask2, centerMask, map) is { } type1Step)
			{
				return type1Step;
			}
			if (CheckType2(accumulator, grid, ref context, pattern, onlyFindOne, cornerMask1, cornerMask2, centerMask, map) is { } type2Step)
			{
				return type2Step;
			}
			if (CheckType3(accumulator, grid, ref context, pattern, onlyFindOne, cornerMask1, cornerMask2, centerMask, map) is { } type3Step)
			{
				return type3Step;
			}
			if (CheckType4(accumulator, grid, ref context, pattern, onlyFindOne, cornerMask1, cornerMask2, centerMask, map) is { } type4Step)
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
		List<Step> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		bool findOnlyOne,
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
			if (findOnlyOne)
			{
				return step;
			}

			accumulator.Add(step);
		}

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check type 2.
	/// </summary>
	private BorescoperDeadlyPatternType2Step? CheckType2(
		List<Step> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		bool findOnlyOne,
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
			if (findOnlyOne)
			{
				return step;
			}

			accumulator.Add(step);
		}

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	private BorescoperDeadlyPatternType3Step? CheckType3(
		List<Step> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		bool findOnlyOne,
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
						if (findOnlyOne)
						{
							return step;
						}

						accumulator.Add(step);
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
		List<Step> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		BorescoperDeadlyPatternPattern pattern,
		bool findOnlyOne,
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
					if (findOnlyOne)
					{
						return step;
					}

					accumulator.Add(step);
				}
			}
		}

		return null;
	}
}
