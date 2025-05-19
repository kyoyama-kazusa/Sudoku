namespace Sudoku.Analytics.StepSearchers.Invalidity;

/// <summary>
/// Provides with a <b>Chromatic Pattern</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Basic types:
/// <list type="bullet">
/// <item>Chromatic Pattern type 1</item>
/// <!--
/// <item>Chromatic Pattern type 2</item>
/// <item>Chromatic Pattern type 3</item>
/// <item>Chromatic Pattern type 4</item>
/// -->
/// </list>
/// </item>
/// <item>
/// Extended types:
/// <list type="bullet">
/// <item>Chromatic Pattern XZ</item>
/// </list>
/// </item>
/// </list>
/// </summary>
/// <remarks>
/// For more information about a "chromatic pattern",
/// please visit <see href="http://forum.enjoysudoku.com/chromatic-patterns-t39885.html">this link</see>.
/// </remarks>
[StepSearcher(
	"StepSearcherName_ChromaticPatternStepSearcher",
	Technique.ChromaticPatternType1, Technique.ChromaticPatternType2, Technique.ChromaticPatternType3, Technique.ChromaticPatternType4,
	Technique.ChromaticPatternXzRule)]
public sealed partial class ChromaticPatternStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (EmptyCells.Count < 12)
		{
			// This technique requires at least 12 cells to be used.
			return null;
		}

		var satisfiedBlocksMask = (Mask)0;
		for (var block = 0; block < 9; block++)
		{
			if ((EmptyCells & HousesMap[block]).Count >= 3)
			{
				satisfiedBlocksMask |= (Mask)(1 << block);
			}
		}

		if (BitOperations.PopCount(satisfiedBlocksMask) < 4)
		{
			// At least four blocks should contain at least 3 cells.
			return null;
		}

		ref readonly var grid = ref context.Grid;
		foreach (var blocks in satisfiedBlocksMask.AllSets.GetSubsets(4))
		{
			var blocksMask = Mask.Create(blocks);
			var flag = false;
			foreach (var tempBlocksMask in ChromaticPatternPattern.ChromaticPatternBlocksCombinations)
			{
				if ((tempBlocksMask & blocksMask) == blocksMask)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}

			var c1 = HousesCells[blocks[0]][0];
			var c2 = HousesCells[blocks[1]][0];
			var c3 = HousesCells[blocks[2]][0];
			var c4 = HousesCells[blocks[3]][0];
			foreach (var (a, b, c, d) in ChromaticPatternPattern.Patterns)
			{
				var pattern = f(a, c1) | f(b, c2) | f(c, c3) | f(d, c4);
				if ((EmptyCells & pattern) != pattern)
				{
					// All cells in this pattern should be empty.
					continue;
				}

				var containsNakedSingle = false;
				foreach (var cell in pattern)
				{
					var candidatesMask = grid.GetCandidates(cell);
					if (BitOperations.IsPow2(candidatesMask))
					{
						containsNakedSingle = true;
						break;
					}
				}
				if (containsNakedSingle)
				{
					continue;
				}

				// Gather steps.
				if (CheckType1(ref context, pattern, blocks) is { } type1Step)
				{
					return type1Step;
				}
				if (CheckXz(ref context, pattern, blocks) is { } typeXzStep)
				{
					return typeXzStep;
				}
			}
		}
		return null;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static CellMap f(Cell[] cells, Cell currentCellOffset)
			=> [cells[0] + currentCellOffset, cells[1] + currentCellOffset, cells[2] + currentCellOffset];
	}

	/// <summary>
	/// Checks for the type 1.
	/// </summary>
	private ChromaticPatternType1Step? CheckType1(ref StepAnalysisContext context, in CellMap pattern, House[] blocks)
	{
		// Test examples:
		// ........1.....2.......3..45+8.6.......71.8....32..67..8+16..23....837.+61..7.281.6..:144 444 544 944 454 554 954
		// 57....9..........8.1.........168..42...1.28.9..2.9416.....26....6.9.82.4...41.6..:178 378 578 778 398 598 798

		ref readonly var grid = ref context.Grid;
		foreach (var extraCell in pattern)
		{
			var otherCells = pattern - extraCell;
			var digitsMask = grid[otherCells];
			if (BitOperations.PopCount(digitsMask) != 3)
			{
				continue;
			}

			var elimDigitsMask = (Mask)(grid.GetCandidates(extraCell) & digitsMask);
			if (elimDigitsMask == 0)
			{
				// No eliminations.
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>((12 - 1) * 3);
			foreach (var otherCell in otherCells)
			{
				foreach (var otherDigit in grid.GetCandidates(otherCell))
				{
					candidateOffsets.Add(new(ColorIdentifier.Normal, otherCell * 9 + otherDigit));
				}
			}

			var step = new ChromaticPatternType1Step(
				(from digit in elimDigitsMask select new Conclusion(Elimination, extraCell, digit)).ToArray(),
				[[.. candidateOffsets, .. from house in blocks select new HouseViewNode(ColorIdentifier.Normal, house)]],
				context.Options,
				blocks,
				pattern,
				extraCell,
				digitsMask
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
	/// Checks for XZ rule.
	/// </summary>
	private ChromaticPatternXzStep? CheckXz(ref StepAnalysisContext context, in CellMap pattern, House[] blocks)
	{
		// Test examples:
		// 0000000010000023400+45013602000+470036000089400004600000012500060403100520560000100:611 711 811 911 712 812 912 915 516 716 816 721 821 921 925 565 568 779 879 979 794 894 994 799 899 999

		ref readonly var grid = ref context.Grid;
		var allDigitsMask = grid[pattern];
		if (BitOperations.PopCount(allDigitsMask) != 5)
		{
			// The pattern cannot find any possible eliminations because the number of extra digits
			// are not 2 will cause the extra digits not forming a valid strong link
			// as same behavior as ALS-XZ or BUG-XZ rule.
			return null;
		}

		foreach (var digits in allDigitsMask.AllSets.GetSubsets(3))
		{
			var patternDigitsMask = (Mask)(1 << digits[0] | 1 << digits[1] | 1 << digits[2]);
			var otherDigitsMask = (Mask)(allDigitsMask & ~patternDigitsMask);
			var d1 = BitOperations.TrailingZeroCount(otherDigitsMask);
			var d2 = otherDigitsMask.GetNextSet(d1);
			var otherDigitsCells = pattern & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (otherDigitsCells is not [var c1, var c2])
			{
				continue;
			}

			foreach (var extraCell in (PeersMap[c1] ^ PeersMap[c2]) & BivalueCells)
			{
				if (grid.GetCandidates(extraCell) != otherDigitsMask)
				{
					continue;
				}

				// XZ rule found.
				var conclusions = new List<Conclusion>();
				var condition = (c1.AsCellMap() + extraCell).FirstSharedHouse != FallbackConstants.@int;
				var anotherCell = condition ? c2 : c1;
				var anotherDigit = condition ? d1 : d2;
				foreach (var peer in (extraCell.AsCellMap() + anotherCell).PeerIntersection)
				{
					if (CandidatesMap[anotherDigit].Contains(peer))
					{
						conclusions.Add(new(Elimination, peer, anotherDigit));
					}
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(33);
				foreach (var patternCell in pattern)
				{
					foreach (var digit in grid.GetCandidates(patternCell))
					{
						candidateOffsets.Add(
							new(
								otherDigitsCells.Contains(patternCell) && (d1 == digit || d2 == digit)
									? ColorIdentifier.Auxiliary1
									: ColorIdentifier.Normal,
								patternCell * 9 + digit
							)
						);
					}
				}

				var step = new ChromaticPatternXzStep(
					conclusions.AsMemory(),
					[
						[
							.. candidateOffsets,
							new CellViewNode(ColorIdentifier.Normal, extraCell),
							.. from block in blocks select new HouseViewNode(ColorIdentifier.Normal, block)
						]
					],
					context.Options,
					blocks,
					pattern,
					otherDigitsCells,
					extraCell,
					patternDigitsMask,
					otherDigitsMask
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
}
