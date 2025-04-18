namespace Sudoku.Analytics.StepSearchers.Uniqueness;

public partial class UniqueLoopStepSearcher
{
	/// <summary>
	/// Check type 1.
	/// </summary>
	private static partial UniqueLoopStep? CheckType1(
		SortedSet<UniqueLoopStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer,
		Cell[] path
	)
	{
		if (extraCellsMap is not [var extraCell])
		{
			return null;
		}

		var conclusions = new List<Conclusion>(2);
		if (CandidatesMap[d1].Contains(extraCell))
		{
			conclusions.Add(new(Elimination, extraCell, d1));
		}
		if (CandidatesMap[d2].Contains(extraCell))
		{
			conclusions.Add(new(Elimination, extraCell, d2));
		}
		if (conclusions.Count == 0)
		{
			goto ReturnNull;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in loop - extraCell)
		{
			candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d1));
			candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d2));
		}

		var step = new UniqueLoopType1Step(
			conclusions.AsMemory(),
			[[.. candidateOffsets, .. GetLoopLinks(path)]],
			context.Options,
			d1,
			d2,
			loop,
			path
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check type 2.
	/// </summary>
	private static partial UniqueLoopStep? CheckType2(
		SortedSet<UniqueLoopStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer,
		Cell[] path
	)
	{
		var mask = (Mask)(grid[extraCellsMap] & ~comparer);
		if (!Mask.IsPow2(mask))
		{
			return null;
		}

		var extraDigit = Mask.TrailingZeroCount(mask);
		var elimMap = extraCellsMap % CandidatesMap[extraDigit];
		if (!elimMap)
		{
			goto ReturnNull;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in loop)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(digit == extraDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell * 9 + digit));
			}
		}

		var step = new UniqueLoopType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
			[[.. candidateOffsets, .. GetLoopLinks(path)]],
			context.Options,
			d1,
			d2,
			loop,
			extraDigit,
			path
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check type 3.
	/// </summary>
	private static partial UniqueLoopStep? CheckType3(
		SortedSet<UniqueLoopStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer,
		Cell[] path
	)
	{
		// Check whether the extra cells contain at least one digit of digit 1 and 2,
		// and extra digits not appeared in two digits mentioned above.
		var notSatisfiedType3 = false;
		foreach (var cell in extraCellsMap)
		{
			var mask = grid.GetCandidates(cell);
			if ((mask & comparer) == 0 || mask == comparer)
			{
				notSatisfiedType3 = true;
				break;
			}
		}
		if (notSatisfiedType3)
		{
			return null;
		}

		// Gather the union result of digits appeared, and check whether the result mask
		// contains both digit 1 and 2.
		var m = grid[extraCellsMap];
		if ((m & comparer) != comparer)
		{
			return null;
		}

		CellMap otherCells;
		var otherDigitsMask = (Mask)(m & ~comparer);
		if (extraCellsMap.FirstSharedHouse != 32)
		{
			if (extraCellsMap.Count != 2)
			{
				return null;
			}

			// All extra cells lie in a same house. This is the basic subtype of type 3.
			foreach (var houseIndex in extraCellsMap.SharedHouses)
			{
				if ((ValuesMap[d1] || ValuesMap[d2]) && HousesMap[houseIndex])
				{
					continue;
				}

				otherCells = HousesMap[houseIndex] & EmptyCells & ~loop;
				for (var size = BitOperations.PopCount(otherDigitsMask) - 1; size < otherCells.Count; size++)
				{
					foreach (ref readonly var cells in otherCells & size)
					{
						var mask = grid[cells];
						if (BitOperations.PopCount(mask) != size + 1 || (mask & otherDigitsMask) != otherDigitsMask)
						{
							continue;
						}

						if ((HousesMap[houseIndex] & EmptyCells & ~cells & ~loop) is not (var elimMap and not []))
						{
							continue;
						}

						var conclusions = new List<Conclusion>();
						foreach (var digit in mask)
						{
							foreach (var cell in elimMap & CandidatesMap[digit])
							{
								conclusions.Add(new(Elimination, cell, digit));
							}
						}
						if (conclusions.Count == 0)
						{
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>();
						foreach (var cell in loop)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(
									new(
										(otherDigitsMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
										cell * 9 + digit
									)
								);
							}
						}
						foreach (var cell in cells)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
							}
						}

						var step = new UniqueLoopType3Step(
							conclusions.AsMemory(),
							[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, houseIndex), .. GetLoopLinks(path)]],
							context.Options,
							d1,
							d2,
							loop,
							cells,
							mask,
							path
						);
						if (context.OnlyFindOne)
						{
							return step;
						}

						accumulator.Add(step);
					}
				}
			}

			return null;
		}

		// Extra cells may not lie in a same house. However the type 3 can form in this case.
		otherCells = extraCellsMap.PeerIntersection & ~loop & EmptyCells;
		if (!otherCells)
		{
			return null;
		}

		for (var size = BitOperations.PopCount(otherDigitsMask) - 1; size < otherCells.Count; size++)
		{
			foreach (ref readonly var cells in otherCells & size)
			{
				var mask = grid[cells];
				if (BitOperations.PopCount(mask) != size + 1 || (mask & otherDigitsMask) != otherDigitsMask)
				{
					continue;
				}

				if (((extraCellsMap | cells).PeerIntersection & ~loop) is not (var elimMap and not []))
				{
					continue;
				}

				var conclusions = new List<Conclusion>();
				foreach (var cell in elimMap)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & otherDigitsMask))
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in loop)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								(otherDigitsMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in cells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}
				}

				var step = new UniqueLoopType3Step(
					conclusions.AsMemory(),
					[[.. candidateOffsets, .. GetLoopLinks(path)]],
					context.Options,
					d1,
					d2,
					loop,
					cells,
					mask,
					path
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				accumulator.Add(step);
			}
		}

		return null;
	}

	/// <summary>
	/// Check type 4.
	/// </summary>
	private static partial UniqueLoopStep? CheckType4(
		SortedSet<UniqueLoopStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer,
		Cell[] path
	)
	{
		if (extraCellsMap is not ([var first, var second] and { SharedHouses: var houses }) || extraCellsMap.FirstSharedHouse == 32)
		{
			return null;
		}

		foreach (var house in houses)
		{
			foreach (var (digit, otherDigit) in ((d1, d2), (d2, d1)))
			{
				var map = HousesMap[house] & CandidatesMap[digit];
				if (map != (HousesMap[house] & loop))
				{
					continue;
				}

				var conclusions = new List<Conclusion>(2);
				if (CandidatesMap[otherDigit].Contains(first))
				{
					conclusions.Add(new(Elimination, first, otherDigit));
				}
				if (CandidatesMap[otherDigit].Contains(second))
				{
					conclusions.Add(new(Elimination, second, otherDigit));
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in loop & ~extraCellsMap)
				{
					foreach (var d in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d));
					}
				}
				foreach (var cell in extraCellsMap)
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
				}

				var step = new UniqueLoopType4Step(
					conclusions.AsMemory(),
					[
						[
							.. candidateOffsets,
							new ConjugateLinkViewNode(ColorIdentifier.Normal, first, second, digit),
							.. GetLoopLinks(path)
						]
					],
					context.Options,
					d1,
					d2,
					loop,
					new(first, second, digit),
					path
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				accumulator.Add(step);
			}
		}

		return null;
	}

	/// <summary>
	/// Check strong link type.
	/// </summary>
	private static partial UniqueLoopStep? CheckStrongLinkType(
		SortedSet<UniqueLoopStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer,
		Cell[] path
	)
	{
		var pathLength = path.Length;

		// Iterate on each digit the current loop used.
		foreach (var (digit, theOtherDigit) in ((d1, d2), (d2, d1)))
		{
			// Iterate on each cell of the current loop.
			foreach (var cell in loop)
			{
				var startIndex = Array.IndexOf(path, cell);

				// Iterate cells.
				// There are two ways to check: clockwise and counter-clockwise.
				foreach (var isClockwise in (true, false))
				{
					// Construct an array to be iterated.
					var loopCells = new List<Cell>(pathLength);
					if (isClockwise)
					{
						for (var (i, j) = (startIndex, 0); j < pathLength; i = (i + 1) % pathLength, j++)
						{
							loopCells.Add(path[i]);
						}
					}
					else
					{
						for (var (i, j) = (startIndex, 0); j < pathLength; i = makeDecrement(i) % pathLength, j++)
						{
							loopCells.Add(path[i]);
						}


						int makeDecrement(int i) => (i <= 1 ? i + pathLength : i) - 1;
					}

					// Suppose it is true, and fill digits through the path, to determine whether the loop can be filled completely.
					var isFailed = false;
					var conjugatePairsUsed = new List<Conjugate>();
					for (
						var (i, thisDigit, nextDigit) = (0, digit, theOtherDigit);
						i < pathLength - 1;
						i++, @ref.Swap(ref thisDigit, ref nextDigit)
					)
					{
						var (thisCell, nextCell) = (loopCells[i], loopCells[i + 1]);
						// There are two ways to continue filling:
						//
						//   1) Two cells 'thisCell' and 'nextCell' forms a conjugate pair of digit 'nextDigit':
						//      If supposing 'abx' ('thisCell') is 'a', then:
						//      .-------------------------------.
						//      |       b        makes          |
						//      | abx ===== aby ------>  a => b |
						//      '-------------------------------'
						//   2) The cell 'nextCell' is a bi-value cell.
						//      If supposing 'abx' ('thisCell') is 'a', then:
						//      .--------------------------.
						//      |           makes          |
						//      | abx  ab  ------>  a => b |
						//      '--------------------------'
						//
						// If neither cases are not successful, the loop cannot continue. We should break the supposing.

						// Case 2.
						if (BivalueCells.Contains(nextCell) && (grid.GetCandidates(nextCell) & comparer) == comparer)
						{
							continue;
						}

						// Case 1.
						var (twoCellsMap, case1) = (thisCell.AsCellMap() + nextCell, false);
						var house = twoCellsMap.FirstSharedHouse;
						if ((HousesMap[house] & CandidatesMap[nextDigit]) == twoCellsMap)
						{
							conjugatePairsUsed.Add(new(twoCellsMap, nextDigit));
							case1 = true;
						}
						if (case1)
						{
							continue;
						}

						// Neither cases are not successful.
						isFailed = true;
						break;
					}
					if (isFailed)
					{
						continue;
					}

					if (conjugatePairsUsed.Count == 0)
					{
						// If a UL doesn't use any conjugate pairs, it'll be a normal UL, we should skip it.
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var c in loop)
					{
						foreach (var d in (Mask)(grid.GetCandidates(c) & comparer))
						{
							if (cell == c && digit == d)
							{
								continue;
							}

							candidateOffsets.Add(
								new(
									conjugatePairsUsed.Exists(cp => cp.Map.Contains(c) && cp.Digit == d)
										? ColorIdentifier.Auxiliary1
										: ColorIdentifier.Normal,
									c * 9 + d
								)
							);
						}
					}

					var step = new UniqueLoopConjugatePairsTypeStep(
						new SingletonArray<Conclusion>(new(Elimination, cell, digit)),
						[
							[
								.. candidateOffsets,
								..
								from cp in conjugatePairsUsed
								let id = cp.Digit == digit ? ColorIdentifier.Normal : ColorIdentifier.Auxiliary1
								select new ConjugateLinkViewNode(id, cp.Map[0], cp.Map[1], cp.Digit),
								.. GetLoopLinks(path)
							]
						],
						context.Options,
						d1,
						d2,
						loop,
						path,
						extraCellsMap.Count,
						[.. conjugatePairsUsed]
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
