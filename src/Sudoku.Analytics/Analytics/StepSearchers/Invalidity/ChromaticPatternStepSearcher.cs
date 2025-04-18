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
	/// <summary>
	/// The possible pattern offsets.
	/// </summary>
	private static readonly ChromaticPatternPattern[] Patterns;

	/// <summary>
	/// All possible blocks combinations being reserved for chromatic pattern searcher's usages.
	/// </summary>
	private static readonly Mask[] ChromaticPatternBlocksCombinations = [
		0b000_011_011, 0b000_101_101, 0b000_110_110,
		0b011_000_011, 0b101_000_101, 0b110_000_110,
		0b011_011_000, 0b101_101_000, 0b110_110_000
	];

	/// <summary>
	/// Indicates the possible offset values for diagonal cases.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly Cell[][] DiagonalCases = [[0, 10, 20], [1, 11, 18], [2, 9, 19]];

	/// <summary>
	/// Indicates the possible offset values for anti-diagonal cases.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly Cell[][] AntidiagonalCases = [[0, 11, 19], [1, 9, 20], [2, 10, 18]];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static ChromaticPatternStepSearcher()
	{
		var patternOffsetsList = new List<ChromaticPatternPattern>();
		foreach (var (aCase, bCase, cCase, dCase) in (
			(true, false, false, false),
			(false, true, false, false),
			(false, false, true, false),
			(false, false, false, true)
		))
		{
			// Phase 1.
			foreach (var a in aCase ? DiagonalCases : AntidiagonalCases)
			{
				foreach (var b in bCase ? DiagonalCases : AntidiagonalCases)
				{
					foreach (var c in cCase ? DiagonalCases : AntidiagonalCases)
					{
						foreach (var d in dCase ? DiagonalCases : AntidiagonalCases)
						{
							patternOffsetsList.Add(new(a, b, c, d));
						}
					}
				}
			}

			// Phase 2.
			foreach (var a in aCase ? AntidiagonalCases : DiagonalCases)
			{
				foreach (var b in bCase ? AntidiagonalCases : DiagonalCases)
				{
					foreach (var c in cCase ? AntidiagonalCases : DiagonalCases)
					{
						foreach (var d in dCase ? AntidiagonalCases : DiagonalCases)
						{
							patternOffsetsList.Add(new(a, b, c, d));
						}
					}
				}
			}
		}
		Patterns = [.. patternOffsetsList];
	}


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
			var blocksMask = MaskOperations.Create(blocks);
			var flag = false;
			foreach (var tempBlocksMask in ChromaticPatternBlocksCombinations)
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
			foreach (var (a, b, c, d) in Patterns)
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
					if (Mask.IsPow2(candidatesMask))
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
	/// Here I give you 2 examples to test this method:
	/// <list type="number">
	/// <item>
	/// <see href="http://forum.enjoysudoku.com/the-tridagon-rule-t39859.html#p318380">the first one</see>
	/// </item>
	/// <item>
	/// <see href="http://forum.enjoysudoku.com/the-tridagon-rule-t39859.html#p318378">the second one</see>
	/// </item>
	/// </list>
	/// </summary>
	private ChromaticPatternType1Step? CheckType1(ref StepAnalysisContext context, in CellMap pattern, House[] blocks)
	{
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
	/// Here I give you 1 example to test this method:
	/// <list type="number">
	/// <item>
	/// <code>
	/// 0000000010000023400+45013602000+470036000089400004600000012500060403100520560000100:611 711 811 911 712 812 912 915 516 716 816 721 821 921 925 565 568 779 879 979 794 894 994 799 899 999
	/// </code>
	/// </item>
	/// </list>
	/// </summary>
	private ChromaticPatternXzStep? CheckXz(ref StepAnalysisContext context, in CellMap pattern, House[] blocks)
	{
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
				var condition = (c1.AsCellMap() + extraCell).FirstSharedHouse != 32;
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
