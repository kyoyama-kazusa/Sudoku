namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Unique Loop</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Unique Loop Type 1</item>
/// <item>Unique Loop Type 2</item>
/// <item>Unique Loop Type 3</item>
/// <item>Unique Loop Type 4</item>
/// <item>Unique Loop Strong Link Type</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_UniqueLoopStepSearcher",
	Technique.UniqueLoopType1, Technique.UniqueLoopType2, Technique.UniqueLoopType3, Technique.UniqueLoopType4,
	Technique.UniqueLoopStrongLinkType,
	SupportsSukaku = false,
	SupportsAnalyzingPuzzleHavingMultipleSolutions = false)]
public sealed partial class UniqueLoopStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the type checkers.
	/// </summary>
	private static readonly safe delegate*<SortedSet<UniqueLoopStep>, in Grid, ref StepAnalysisContext, Digit, Digit, in CellMap, in CellMap, Mask, Cell[], UniqueLoopStep?>[] TypeCheckers = [
		&CheckType1,
		&CheckType2,
		&CheckType3,
		&CheckType4,
		&CheckStrongLinkType
	];


	/// <summary>
	/// Indicates whether the searcher collects for extended types of Unique Loop patterns.
	/// </summary>
	[SettingItemName(SettingItemNames.SearchForExtendedUniqueLoops)]
	public bool SearchExtendedTypes { get; set; } = true;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Now iterate on each bi-value cells as the start cell to get all possible unique loops,
		// making it the start point to execute the recursion.
		ref readonly var grid = ref context.Grid;
		var tempAccumulator = new SortedSet<UniqueLoopStep>();
		foreach (var (loop, path, comparer) in FindLoops(grid))
		{
			if ((loop & ~BivalueCells) is not (var extraCellsMap and not []))
			{
				// The current puzzle has multiple solutions.
				throw new PuzzleInvalidException(grid, typeof(UniqueLoopStepSearcher));
			}

			var d1 = BitOperations.PopTwo((uint)comparer, out var d2);
			for (var i = 0; i < unsafe (TypeCheckers).Length; i++)
			{
				if (!SearchExtendedTypes && i == unsafe (TypeCheckers).Length - 1)
				{
					continue;
				}

				if (unsafe (TypeCheckers[i](tempAccumulator, grid, ref context, d1, d2, loop, extraCellsMap, comparer, path)) is { } step)
				{
					return step;
				}
			}
		}
		if (tempAccumulator.Count == 0)
		{
			return null;
		}

		if (context.OnlyFindOne && tempAccumulator.Count != 0)
		{
			return tempAccumulator.Min;
		}
		else if (!context.OnlyFindOne)
		{
			context.Accumulator.AddRange(tempAccumulator);
		}
		return null;
	}

	/// <summary>
	/// Try to find all possible loops appeared in a grid.
	/// </summary>
	/// <param name="grid">The grid to be used.</param>
	/// <returns>A list of <see cref="UniqueLoopPattern"/> instances.</returns>
	private ReadOnlySpan<UniqueLoopPattern> FindLoops(in Grid grid)
	{
		var result = new HashSet<UniqueLoopPattern>();
		foreach (var cell in BivalueCells)
		{
			var queue = new Queue<LinkedList<Cell>>();
			queue.Enqueue(LinkedList.Singleton(cell));

			var comparer = grid.GetCandidates(cell);
			var d1 = BitOperations.PopTwo((uint)comparer, out var d2);
			var pairMap = CandidatesMap[d1] & CandidatesMap[d2];
			while (queue.Count != 0)
			{
				var currentBranch = queue.Dequeue();

				// The node should be appended after the last node, and its start node is the first node of the linked list.
				var previousCell = currentBranch.LastValue;
				foreach (var currentCell in Peer.PeersMap[previousCell] & pairMap)
				{
					// Determine whether the current cell iterated is the first node.
					// If so, check whether the loop is of length greater than 6, and validity of the loop.
					if (currentCell == cell && currentBranch.Count is 6 or 8 or 10 or 12 or 14 && UniqueLoopPattern.IsValid(currentBranch))
					{
						result.Add(new([.. currentBranch], [.. currentBranch], comparer));
						break;
					}

					if (!currentBranch.Contains(currentCell) && currentBranch.Count < 14
						&& (hasBivalueCell(previousCell, currentCell) || hasConjugatePair(previousCell, currentCell, d1, d2)))
					{
						// Create a new link with original value, and a new value at the last position.
						queue.Enqueue(LinkedList.Create(currentBranch, currentCell));
					}
				}
			}
		}
		return (UniqueLoopPattern[])[.. result];


		// This may raise a bug that type 3 cannot be found. Example:
		// 0000291000006+1+8+9001+960+4580+2401+500+290500080+40706000+45030084006+29+600007+300+904860+7+5+1:312 318 718 222 322 428 342 253 254 354 356 764 372 282
		static bool hasBivalueCell(Cell previous, Cell current) => BivalueCells.Contains(previous) || BivalueCells.Contains(current);

		static bool hasConjugatePair(Cell previous, Cell current, Digit d1, Digit d2)
		{
			var twoCellsMap = previous.AsCellMap() + current;
			var house = twoCellsMap.FirstSharedHouse;
			return (HousesMap[house] & CandidatesMap[d1]) == twoCellsMap || (HousesMap[house] & CandidatesMap[d2]) == twoCellsMap;
		}
	}


	/// <summary>
	/// Gets all possible links of the loop.
	/// </summary>
	/// <param name="path">The loop, specified as its path.</param>
	/// <returns>A list of <see cref="CellLinkViewNode"/> instances.</returns>
	private static ReadOnlySpan<CellLinkViewNode> GetLoopLinks(Cell[] path)
	{
		var result = new List<CellLinkViewNode>();
		for (var i = 0; i < path.Length; i++)
		{
			result.Add(new(ColorDescriptorAlias.Normal, path[i], path[i + 1 == path.Length ? 0 : i + 1]));
		}
		return result.AsSpan();
	}


	private static partial UniqueLoopStep? CheckType1(SortedSet<UniqueLoopStep> accumulator, in Grid grid, ref StepAnalysisContext context, Digit d1, Digit d2, in CellMap loop, in CellMap extraCellsMap, Mask comparer, Cell[] path);
	private static partial UniqueLoopStep? CheckType2(SortedSet<UniqueLoopStep> accumulator, in Grid grid, ref StepAnalysisContext context, Digit d1, Digit d2, in CellMap loop, in CellMap extraCellsMap, Mask comparer, Cell[] path);
	private static partial UniqueLoopStep? CheckType3(SortedSet<UniqueLoopStep> accumulator, in Grid grid, ref StepAnalysisContext context, Digit d1, Digit d2, in CellMap loop, in CellMap extraCellsMap, Mask comparer, Cell[] path);
	private static partial UniqueLoopStep? CheckType4(SortedSet<UniqueLoopStep> accumulator, in Grid grid, ref StepAnalysisContext context, Digit d1, Digit d2, in CellMap loop, in CellMap extraCellsMap, Mask comparer, Cell[] path);
	private static partial UniqueLoopStep? CheckStrongLinkType(SortedSet<UniqueLoopStep> accumulator, in Grid grid, ref StepAnalysisContext context, Digit d1, Digit d2, in CellMap loop, in CellMap extraCellsMap, Mask comparer, Cell[] path);
}

/// <summary>
/// Represents a type that creates a <see cref="LinkedList{T}"/> instance.
/// </summary>
/// <seealso cref="LinkedList{T}"/>
file static class LinkedList
{
	/// <summary>
	/// Creates a <see cref="LinkedList{T}"/> with only one element.
	/// </summary>
	/// <typeparam name="T">The type of each element of the return value.</typeparam>
	/// <param name="value">The value to be created.</param>
	/// <returns>A <see cref="LinkedList{T}"/> instance, with only one element inside it.</returns>
	public static LinkedList<T> Singleton<T>(T value)
	{
		var result = new LinkedList<T>();
		result.AddLast(value);
		return result;
	}

	/// <summary>
	/// Creates a <see cref="LinkedList{T}"/> with the original values and a new value.
	/// </summary>
	/// <typeparam name="T">The type of each element.</typeparam>
	/// <param name="original">The original collection.</param>
	/// <param name="newValue">The new value to be added.</param>
	/// <returns>A <see cref="LinkedList{T}"/> instance.</returns>
	public static LinkedList<T> Create<T>(LinkedList<T> original, T newValue)
	{
		var result = new LinkedList<T>(original);
		result.AddLast(newValue);
		return result;
	}
}
