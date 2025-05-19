namespace Sudoku.Analytics.StepSearchers.Invalidity;

/// <summary>
/// <para>
/// Provides with a <b>Bi-value Oddagon</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Bi-value Oddagon Type 2</item>
/// <item>Bi-value Oddagon Type 3</item>
/// </list>
/// </para>
/// <para>
/// <i>
/// In practicing, type 1 and 4 do not exist. A bi-value oddagon type 1 is a remote pair
/// and a type 4 cannot be formed as a stable pattern.
/// </i>
/// </para>
/// <para><i>A Remote Pair is a XY-Chain that only uses two digits.</i></para>
/// </summary>
[StepSearcher(
	"StepSearcherName_BivalueOddagonStepSearcher",
	Technique.BivalueOddagonType2, Technique.BivalueOddagonType3)]
public sealed partial class BivalueOddagonStepSearcher : StepSearcher
{
	/// <summary>
	/// The maximum number of loops can be searched for in code.
	/// </summary>
	private const int MaximumCount = 100;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (BivalueCells.Count < 4)
		{
			return null;
		}

		var resultAccumulator = new SortedSet<BivalueOddagonStep>();

		// Now iterate on each bi-value cells as the start cell to get all possible unique loops,
		// making it the start point to execute the recursion.
		ref readonly var grid = ref context.Grid;
		var onlyFindOne = context.OnlyFindOne;
		if (collect(grid) is not { Count: not 0 } oddagonInfoList)
		{
			return null;
		}

		foreach (var (currentLoop, extraCells, comparer) in oddagonInfoList)
		{
			var d1 = BitOperations.TrailingZeroCount(comparer);
			var d2 = comparer.GetNextSet(d1);
			switch (extraCells.Count)
			{
				case 0:
				{
					// This puzzle has no puzzle solutions.
					throw new PuzzleInvalidException(grid, typeof(BivalueOddagonStepSearcher));
				}
				case not 1:
				{
					// Type 2, 3.
					// Here use default label to ensure the order of the handling will be 1->2->3.
					if (CheckType2(resultAccumulator, grid, ref context, d1, d2, currentLoop, extraCells, comparer, onlyFindOne) is { } step2)
					{
						return step2;
					}

					if (extraCells.Count == 2
						&& CheckType3(resultAccumulator, grid, ref context, d1, d2, currentLoop, extraCells, comparer, onlyFindOne) is { } step3)
					{
						return step3;
					}
					break;
				}
			}
		}

		if (context.OnlyFindOne && resultAccumulator.Count != 0)
		{
			return resultAccumulator.Min;
		}
		if (!context.OnlyFindOne && resultAccumulator.Count != 0)
		{
			context.Accumulator.AddRange(resultAccumulator);
		}
		return null;


		static HashSet<BivalueOddagonPattern> collect(in Grid grid)
		{
			var (foundLoopsCount, result) = (-1, new HashSet<BivalueOddagonPattern>(MaximumCount));
			for (var d1 = 0; d1 < 8; d1++)
			{
				for (var d2 = d1 + 1; d2 < 9; d2++)
				{
					var comparer = (Mask)(1 << d1 | 1 << d2);
					var cellsContainingBothTwoDigits = CandidatesMap[d1] & CandidatesMap[d2];
					foreach (var cell in cellsContainingBothTwoDigits)
					{
						dfs(
							grid,
							cell,
							cell,
							-1,
							cellsContainingBothTwoDigits,
							cell.AsCellMap(),
							BitOperations.PopCount(grid.GetCandidates(cell)) > 2 ? [cell] : [],
							result,
							ref foundLoopsCount,
							comparer,
							0
						);
					}
				}
			}
			return result;
		}

		static void dfs(
			in Grid grid,
			Cell startCell,
			Cell previousCell,
			House previousHouse,
			in CellMap cellsContainingBothDigits,
			in CellMap loop,
			in CellMap extraCells,
			HashSet<BivalueOddagonPattern> result,
			ref int loopsCount,
			Mask comparer,
			Mask extraDigitsMask
		)
		{
			if (loopsCount > 100)
			{
				// There is no need to iterate more loops because they are same.
				return;
			}

			var h = (stackalloc House[3]);
			foreach (var houseType in HouseTypes)
			{
				var nextHouse = previousCell.ToHouse(houseType);
				if (nextHouse == previousHouse)
				{
					continue;
				}

				var loopCellsInThisHouse = loop & HousesMap[nextHouse];
				if (loopCellsInThisHouse.Count >= 2 && !loopCellsInThisHouse.Contains(startCell))
				{
					continue;
				}

				var otherCellsCanBeIterated = (cellsContainingBothDigits & ~loop) + startCell & HousesMap[nextHouse];
				if (!otherCellsCanBeIterated)
				{
					continue;
				}

				foreach (var cell in otherCellsCanBeIterated)
				{
					cell.CopyHouseInfo(ref h[0]);

					if (((loop & HousesMap[h[0]]).Count >= 2 || (loop & HousesMap[h[1]]).Count >= 2 || (loop & HousesMap[h[2]]).Count >= 2)
						&& startCell != cell)
					{
						// All valid loops can only at most 2 cells from all houses that the current loop uses.
						continue;
					}

					if (startCell == cell)
					{
						if ((loop.Count & 1) != 0 && loop.Count > 4)
						{
							// The pattern is found.
							if (++loopsCount < MaximumCount)
							{
								result.Add(new(loop, extraCells, comparer));
							}

							return;
						}
					}
					else
					{
						var newExtraDigitsMask = (Mask)(extraDigitsMask | (Mask)(grid.GetCandidates(cell) & ~comparer));
						var newExtraCells = BitOperations.PopCount(grid.GetCandidates(cell)) > 2 ? extraCells + cell : extraCells;
						if (newExtraCells.FirstSharedHouse != FallbackConstants.@int
							|| BitOperations.IsPow2(newExtraDigitsMask)
							&& !!(newExtraCells.PeerIntersection & CandidatesMap[BitOperations.Log2(newExtraDigitsMask)])
							|| newExtraCells.Count < 3)
						{
							dfs(
								grid,
								startCell,
								cell,
								nextHouse,
								cellsContainingBothDigits,
								loop + cell,
								newExtraCells,
								result,
								ref loopsCount,
								comparer,
								newExtraDigitsMask
							);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Check for type 2.
	/// </summary>
	private BivalueOddagonType2Step? CheckType2(
		SortedSet<BivalueOddagonStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer,
		bool onlyFindOne
	)
	{
		// Test examples:
		// .5.+1+2+6+39+81+6+9+3+5+8+2+74+3+824796+1+5..+8735..+6.3.6+41.8.+6..+9+8+2.+5+38...6...9..6.1.8...23+89.56+1:441 751 773 476 481 781

		var mask = (Mask)(grid[extraCellsMap] & ~comparer);
		if (!BitOperations.IsPow2(mask))
		{
			goto ReturnNull;
		}

		var extraDigit = BitOperations.TrailingZeroCount(mask);
		if (extraCellsMap % CandidatesMap[extraDigit] is not (var elimMap and not []))
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

		var hamiltonianLoop = new CellGraph(loop).GetHamiltonianCycles() is [var l, ..] ? l : default;
		var links = new List<CellLinkViewNode>();
		foreach (var (first, second) in hamiltonianLoop.EnumerateAdjacentCells())
		{
			links.Add(new(ColorIdentifier.Normal, first, second));
		}

		var step = new BivalueOddagonType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
			[[.. candidateOffsets, .. links]],
			context.Options,
			loop,
			d1,
			d2,
			extraDigit
		);

		if (onlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);

	ReturnNull:
		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	private BivalueOddagonType3Step? CheckType3(
		SortedSet<BivalueOddagonStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		in CellMap loop,
		in CellMap extraCellsMap,
		Mask comparer,
		bool onlyFindOne
	)
	{
		// Test examples:
		// .1..69+2...8.153.+4.....+7.1......4+1.25+17..28+493...3+9..6+152+7+9+1+6+3+8+4.31.8..724.8.+3.+5+1.:727 231 632 932 233 333 533

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

		if (extraCellsMap.FirstSharedHouse == FallbackConstants.@int || notSatisfiedType3)
		{
			goto ReturnNull;
		}

		var m = grid[extraCellsMap];
		if ((m & comparer) != comparer)
		{
			goto ReturnNull;
		}

		var otherDigitsMask = (Mask)(m & ~comparer);
		foreach (var house in extraCellsMap.SharedHouses)
		{
			if ((ValuesMap[d1] | ValuesMap[d2]) & HousesMap[house])
			{
				goto ReturnNull;
			}

			var otherCells = HousesMap[house] & EmptyCells & ~loop;
			for (var size = BitOperations.PopCount(otherDigitsMask) - 1; size < otherCells.Count; size++)
			{
				foreach (ref readonly var cells in otherCells & size)
				{
					var mask = grid[cells];
					if (BitOperations.PopCount(mask) != size + 1 || (mask & otherDigitsMask) != otherDigitsMask)
					{
						continue;
					}

					if ((HousesMap[house] & EmptyCells & ~cells & ~loop) is not (var elimMap and not []))
					{
						continue;
					}

					var conclusions = new List<Conclusion>(16);
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

					var hamiltonianLoop = new CellGraph(loop).GetHamiltonianCycles() is [var l, ..] ? l : default;
					var links = new List<CellLinkViewNode>();
					foreach (var (first, second) in hamiltonianLoop.EnumerateAdjacentCells())
					{
						links.Add(new(ColorIdentifier.Normal, first, second));
					}
					var step = new BivalueOddagonType3Step(
						conclusions.AsMemory(),
						[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, house), .. links]],
						context.Options,
						loop,
						d1,
						d2,
						cells,
						mask
					);

					if (onlyFindOne)
					{
						return step;
					}

					accumulator.Add(step);
				}
			}
		}

	ReturnNull:
		return null;
	}
}
