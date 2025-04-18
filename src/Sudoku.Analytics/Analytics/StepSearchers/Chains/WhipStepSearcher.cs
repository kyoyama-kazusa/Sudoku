#define CELL_CONTRADICTION_CHECK
#define HOUSE_CONTRADICTION_CHECK
#if !CELL_CONTRADICTION_CHECK && !HOUSE_CONTRADICTION_CHECK
#error You should configure at least one symbol of 'CELL_CONTRADICTION_CHECK' and 'HOUSE_CONTRADICTION_CHECK'.
#endif

namespace Sudoku.Analytics.StepSearchers.Chains;

/// <summary>
/// Provides with a <b>Whip</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Whip</item>
/// <item>Grouped Whip</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_WhipStepSearcher",
	Technique.Whip, Technique.GroupedWhip,
	RuntimeFlags = StepSearcherRuntimeFlags.SpaceComplexity | StepSearcherRuntimeFlags.SkipVerification)]
public sealed partial class WhipStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Test examples:
		// Whip
		// .3..6.8..6..2...3.4.5..1+6.9..+64....3..9.5..6..4+31.+65...+62.....1.1.9.7..+63..6+1..8.:111 516 722 727 748 948 251 751 768 571 871 885 487 497
		// 9+1..685...+83+1.5....2.3.+9..+8..168.9..8...+92..6.9..1.+8.71..+8.+6..5.....1+68..6+89..3.+1:414 228 431 438 561 463 563 475 775 483 583 783 584 485 495 795
		// ..3.8..7.1.....2...8...5..6..18....7.9.7..4..6..+54..92.1...6.3...94.........5...4:116 625 431 773
		// ..3..+895..1..4...88..3...2.3..78..4..6......2.+856..1..1....6.7....8....9.7..3.2..:215 546 557 966 377 199
		// .2.7..+1.94...+2+15....1.9..2..5...2..4..6.7.+2.82..1...3..8...46..6..2....1.....7.9.:315 815 624 634 737 748 963 769 573 773 482 787 588
		// .1..6+7.....72...+656..45..7..9...3..8......9..74.1..+63+2.+7......6..3.7.5..1....2.8.:311 543 851 873 975 177 986
		// .1..7..5+25..2..3....2...+1.4..6....2.+27.49...11....+28.......3+2.63..12..9..2..5.7..:914 656 368 668 582 583
		//
		// g-Whip
		// 5..3..1...8.....2....6....76.45.3.......7.......4.63..4....16...2......8..7....9.:436 858
		// 3...7.5...9...8.4...72....64......8..1..2...4..38..1...+3...9.1...53....2....8+23..:213 242 261

		ref readonly var grid = ref context.Grid;

		// Iterate on each candidate that can be asserted as false in solution.
		foreach (var cell in EmptyCells)
		{
			// Iterate on the case that means whether the searcher supports for grouped whip (g-whip).
			foreach (var groupedWhip in (false, true))
			{
				var trueDigit = Solution.IsUndefined ? -1 : Solution.GetDigit(cell);
				foreach (var digit in grid.GetCandidates(cell))
				{
					if (trueDigit == digit)
					{
						// Skip for the true digits.
						continue;
					}

					// If the digit is not correct in solution, we can try to suppose it can be eliminated, and find any contradictions.
					// The basic algorithm is simply find try & error - just suppose it is true,
					// and then find the next true digit, and continue.
					var startCandidate = cell * 9 + digit;

					// Create a pending queue to record all interim cases, and a collection recording visited nodes.
					var pendingNodes = new LinkedList<WhipNode>();
					var startNode = new WhipNode(new NormalWhipAssignment(startCandidate, Technique.None));
					startNode = new(startNode.Assignment, GetNextAssignments(grid, startNode, groupedWhip));
					pendingNodes.AddLast(startNode);

					// Iterate the pending queue and never stops, until all nodes are tried.
					while (pendingNodes.Count != 0)
					{
						// Deconstruct the object and apply digit into playground.
						var currentNode = pendingNodes.RemoveFirstNode();

						// Here we should check for 2 kinds of contradictions:
						//   1) No candidates in one empty cell
						//   2) No possible positions of a digit in one house
						// If we can find out such contradiction, we can conclude that the start assertion is failed.
						if (ExistsContradiction(grid, currentNode, out var failedSpace)
							&& IsGroupedWhip(currentNode) is var isGroupedWhip && !(groupedWhip ^ isGroupedWhip))
						{
							// Contradiction is found. Now we can construct a step instance and return.
							var step = CreateStep(context, currentNode, startCandidate, grid, failedSpace, isGroupedWhip);
							if (context.OnlyFindOne)
							{
								return step;
							}

							context.Accumulator.Add(step);
							goto NextCandidate;
						}

						// Add all found conclusion into the pending queue.
						foreach (var assignment in currentNode.AvailableAssignments)
						{
							#region Description to optimization, and a problem that I want you to consider and solve it
							// Check whether the current found assignment indeed exists in ancestor nodes.
							// If so, such conclusion should not be used as children nodes of the current node.
							//
							// But... why we should check for this?
							// For example, if node A can make 3 new conclusions B, C and D,
							// we'll know that the parent of nodes B, C and D is A.
							// However, due to branching rules, if C or D cannot be appeared in the branch A -> B
							// because C or D is a children of A, not B. We should ignore C and D if checking for branch A -> B.
							//
							//           A
							//           |
							//      /----|----\
							//     B     C     D
							//    / \
							//   E   F
							//
							// In the diagram, the grid state at conclusion E can also produce new conclusions C, D and F,
							// and F grid state can also produce steps C, D and E.
							// We should ignore all the other conclusions that don't exist in the branch A -> B -> E
							// (conclusions C, D and F) and A -> B -> F (conclusions C, D and E).
							//
							// In order to find minimal path, we may not iterate on all possible paths.
							// There's a one way to reduce path length.
							//
							//           A
							//           |
							//      /----|----\
							//     B     C     D
							//    / \
							//   E   F
							//
							// Also, in the diagram, we can know there're 4 possible paths:
							//
							//   * A-B-E
							//   * A-B-F
							//   * A-C
							//   * A-D
							//
							// But, if we can find that both paths A-C and B-E in path A-B-E produces same interim candidates,
							// we'll know that in fact A-C is a better choice than A-B-E,
							// because A-C has a shorter path to reach same whip node.
							// However, due to design of this algorithm, we'll return if any one possible path is found.
							// This means, if A-B-E is encountered before A-C, this algorithm will ignore better paths like A-C.
							// To fix this issue, we should check for all conclusions produced here instead,
							// in order to choose a shorter one without ignoring the other paths A-C and A-D
							// while searching A-B-E and A-B-F.
							#endregion

							var isParentNodeContainsSuchAssignment = false;
							for (var parentNode = currentNode.Parent; parentNode is not null; parentNode = parentNode.Parent)
							{
								if (parentNode.AvailableAssignments.Span.Contains(assignment))
								{
									isParentNodeContainsSuchAssignment = true;
									break;
								}
							}
							if (isParentNodeContainsSuchAssignment)
							{
								continue;
							}

							var nextNode = new WhipNode(assignment) >> currentNode;
							nextNode = new WhipNode(assignment, GetNextAssignments(grid, nextNode, groupedWhip), nextNode.Parent);
							pendingNodes.AddLast(nextNode);
						}
					}
				}
			}

		NextCandidate:;
		}

		// No conclusions found, or just find for all possible steps. Return null.
		return null;
	}


	/// <summary>
	/// Try to assign the grid with all conclusions produced by the specified whip chain.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="currentNode">The node.</param>
	private static void UpdateGrid(ref Grid grid, WhipNode currentNode)
	{
		for (var node = currentNode; node is not null; node = node.Parent)
		{
			switch (node.Assignment)
			{
				case NormalWhipAssignment { Candidate: var candidate }:
				{
					grid.Apply(new(Assignment, candidate));
					break;
				}
				case GroupedWhipAssignment { Digit: var digit, Cells: var cells }:
				{
					foreach (var cell in grid.CandidatesMap[digit] & cells.PeerIntersection)
					{
						grid.Apply(new(Elimination, cell, digit));
					}
					break;
				}
			}
		}
	}

	/// <summary>
	/// Determine whether the whip chain is grouped whip.
	/// </summary>
	/// <param name="lastNode">The last node.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	private static bool IsGroupedWhip(WhipNode lastNode)
	{
		for (var node = lastNode; node is not null; node = node.Parent)
		{
			if (node.Assignment is GroupedWhipAssignment)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether the specified grid state contains any contradiction.
	/// </summary>
	/// <param name="playground">The grid.</param>
	/// <param name="currentNode">Indicates the current node.</param>
	/// <param name="failedSpace">The failed space.</param>
	/// <returns>A <see cref="bool"/> result whether any contradiction is found.</returns>
	private static bool ExistsContradiction(Grid playground, WhipNode currentNode, out Space failedSpace)
	{
		// Temporarily update the grid in order not to cache the full grid.
		UpdateGrid(ref playground, currentNode);

#if CELL_CONTRADICTION_CHECK
		// Check for cell.
		// The basic contradiction is to find a "null" cell - a cell has no possibilities available.
		foreach (var cell in playground.EmptyCells)
		{
			if (playground.GetCandidates(cell) == 0)
			{
				failedSpace = Space.RowColumn(cell / 9, cell % 9);
				return true;
			}
		}
#endif

#if HOUSE_CONTRADICTION_CHECK
		// Check for house.
		// Different with cell contradiction, a house contradiction is to find a house contains at least 2 cells
		// containing only one same digit.
		var candidatesMap = playground.CandidatesMap;
		var emptyCells = playground.EmptyCells;
		for (var house = 0; house < 27; house++)
		{
			var digitsMask = playground[HousesMap[house], false];
			foreach (var digit in digitsMask)
			{
				var otherDigitsMask = (Mask)(digitsMask & ~(1 << digit));
				var map = CellMap.Empty;
				foreach (var d in otherDigitsMask)
				{
					map |= candidatesMap[d];
				}

				if ((HousesMap[house] & emptyCells & ~map).Count >= 2)
				{
					failedSpace = house switch
					{
						< 9 => Space.BlockNumber(house, digit),
						< 18 => Space.RowNumber(house - 9, digit),
						_ => Space.ColumnNumber(house - 18, digit)
					};
					return true;
				}
			}
		}
#endif

		// No conclusions found.
		failedSpace = default;
		return false;
	}

	/// <summary>
	/// Try to find all possible conclusions inside the grid.
	/// </summary>
	/// <param name="playground">The grid.</param>
	/// <param name="currentNode">Indicates the current node.</param>
	/// <param name="groupedWhip">Indicates whether the searcher allows for searching grouped whip nodes.</param>
	/// <returns>All conclusions and reason why the assignment raised.</returns>
	private static ReadOnlyMemory<WhipAssignment> GetNextAssignments(Grid playground, WhipNode currentNode, bool groupedWhip)
	{
		// Temporarily update the grid in order not to cache the full grid.
		UpdateGrid(ref playground, currentNode);

		var emptyCells = playground.EmptyCells;
		var candidatesMap = playground.CandidatesMap;

		var result = new List<WhipAssignment>();
		var concludedCells = CellMap.Empty;
		var concludedLockedCandidates = new HashSet<CellMap>();
		for (var house = 0; house < 27; house++)
		{
			// Check for full houses.
			if ((HousesMap[house] & emptyCells) is [var fullHouseCell])
			{
				// Check for the missing digit.
				var appearedDigitsMask = (Mask)0;
				foreach (var cell in HousesMap[house])
				{
					if (!emptyCells.Contains(cell))
					{
						appearedDigitsMask |= (Mask)(1 << playground.GetDigit(cell));
					}
				}

				var targetCandidate = fullHouseCell * 9 + Mask.Log2((Mask)(Grid.MaxCandidatesMask & ~appearedDigitsMask));
				if (concludedCells.Add(fullHouseCell))
				{
					result.Add(new NormalWhipAssignment(targetCandidate, Technique.FullHouse));
				}
			}

			// Check hidden singles.
			for (var digit = 0; digit < 9; digit++)
			{
				var hiddenSingleCells = candidatesMap[digit] & HousesMap[house];
				var houseType = house switch
				{
					< 9 => Technique.CrosshatchingBlock,
					< 18 => Technique.CrosshatchingRow,
					_ => Technique.CrosshatchingColumn
				};
				switch (hiddenSingleCells)
				{
					case [var hiddenSingleCell]
					when hiddenSingleCell * 9 + digit is var targetCandidate && concludedCells.Add(hiddenSingleCell):
					{
						result.Add(new NormalWhipAssignment(targetCandidate, houseType));
						break;
					}
					case { Count: 2 or 3 }
					when hiddenSingleCells.IsInIntersection && groupedWhip && concludedLockedCandidates.Add(hiddenSingleCells):
					{
						result.Add(new GroupedWhipAssignment(digit, hiddenSingleCells, houseType));
						break;
					}
				}
			}
		}

		// Check naked singles.
		foreach (var nakedSingleCell in emptyCells)
		{
			var digitsMask = playground.GetCandidates(nakedSingleCell);
			if (BitOperations.IsPow2(digitsMask))
			{
				var digit = Mask.Log2(digitsMask);
				var targetCandidate = nakedSingleCell * 9 + digit;
				if (concludedCells.Add(nakedSingleCell))
				{
					result.Add(new NormalWhipAssignment(targetCandidate, Technique.NakedSingle));
				}
			}
		}

		return result.AsMemory();
	}

	/// <summary>
	/// Create a <see cref="WhipStep"/> instance via the current conflict rule.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="contradictionNode">The node that makes such contradiction.</param>
	/// <param name="startCandidate">Indicates the start candidate.</param>
	/// <param name="initialGrid">Indicates the initial grid.</param>
	/// <param name="failedSpace">Indicates the space indicating where such contradiction is raised.</param>
	/// <param name="isGrouped">Indicates whether the pattern is grouped.</param>
	/// <returns>The final <see cref="WhipStep"/> instance.</returns>
	private static WhipStep CreateStep(
		in StepAnalysisContext context,
		WhipNode contradictionNode,
		Candidate startCandidate,
		in Grid initialGrid,
		Space failedSpace,
		bool isGrouped
	)
	{
		return new(
			new SingletonArray<Conclusion>(new(Elimination, startCandidate)),
			getViews(initialGrid, out var burredCandidates, out var truths, out var links),
			context.Options,
			truths,
			links,
			isGrouped
		);


		View[] getViews(
			in Grid initialGrid,
			out CandidateMap burredCandidates,
			out ReadOnlyMemory<Space> truths,
			out ReadOnlyMemory<Space> links
		)
		{
			var candidateOffsets = new HashSet<CandidateViewNode>();
			var linkOffsets = new List<ChainLinkViewNode>();
			for (var node = contradictionNode; node is not null; node = node.Parent)
			{
				var currentCandidates = node.Assignment.Map;
				var reason = node.Assignment.Reason;
				currentCandidates.ForEach(c => candidateOffsets.Add(new(ColorIdentifier.Normal, c)));

				// Due to design of this algorithm, we should append extra strong and weak links between two assignments.
				// Please note that the links are reversed, we should make a reversion
				// in order to keep the chain and node states correct.
				if (node.Parent is { Assignment.Map: [var f, ..] parentCandidates } && reason != Technique.None)
				{
					switch (reason)
					{
						case Technique.FullHouse or Technique.NakedSingle:
						{
							var interimCandidate = currentCandidates[0] / 9 * 9 + f % 9;
							parentCandidates.ForEach(p => candidateOffsets.Add(new(ColorIdentifier.Normal, p)));
							currentCandidates.ForEach(c => candidateOffsets.Add(new(ColorIdentifier.Normal, c)));
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, interimCandidate));

							linkOffsets.Add(new(ColorIdentifier.Normal, parentCandidates, interimCandidate.AsCandidateMap(), false));
							linkOffsets.Add(new(ColorIdentifier.Normal, interimCandidate.AsCandidateMap(), currentCandidates, true));
							break;
						}
						case var type and >= Technique.CrosshatchingBlock and <= Technique.CrosshatchingColumn:
						{
							parentCandidates.ForEach(p => candidateOffsets.Add(new(ColorIdentifier.Normal, p)));
							currentCandidates.ForEach(c => candidateOffsets.Add(new(ColorIdentifier.Normal, c)));

							var currentCells = currentCandidates.Cells;
							var currentDigit = Mask.Log2(currentCandidates.Digits);
							var houseType = type switch
							{
								Technique.CrosshatchingBlock => HouseType.Block,
								Technique.CrosshatchingRow => HouseType.Row,
								_ => HouseType.Column
							};

							if ((parentCandidates.Cells.PeerIntersection & currentCells) != currentCells)
							{
								var groupedCells = (parentCandidates.Cells | currentCells).PeerIntersection
									& HousesMap[currentCells[0].ToHouse(houseType)]
									& CandidatesMap[currentDigit];
								if (!groupedCells)
								{
									// Invalid case.
									continue;
								}

								foreach (var cell in groupedCells)
								{
									candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + currentDigit));
								}

								var interimMap = (from cell in groupedCells select cell * 9 + currentDigit).AsCandidateMap();
								linkOffsets.Add(new(ColorIdentifier.Normal, parentCandidates, interimMap, false));
								linkOffsets.Add(new(ColorIdentifier.Normal, interimMap, currentCandidates, true));
							}
							else
							{
								var interimCandidate = f / 9 * 9 + currentDigit;
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, interimCandidate));
								linkOffsets.Add(new(ColorIdentifier.Normal, parentCandidates, interimCandidate.AsCandidateMap(), false));
								linkOffsets.Add(new(ColorIdentifier.Normal, interimCandidate.AsCandidateMap(), currentCandidates, true));
							}
							break;
						}
					}
				}
			}

			// Remove the node that is start.
			candidateOffsets.RemoveWhere(node => node.Candidate == startCandidate);

			var (tryAndErrorCandidateOffsets, tryAndErrorLinkOffsets) = (new List<CandidateViewNode>(), new List<ChainLinkViewNode>());
			for (var node = contradictionNode; node is not null; node = node.Parent)
			{
				var currentCandidates = node.Assignment.Map;
				if (node.Assignment.Map is [var s, ..] && s != startCandidate)
				{
					// Skip for the start candidate if the start node is a single candidate that is the real start candidate.
					foreach (var candidate in currentCandidates)
					{
						tryAndErrorCandidateOffsets.Add(new(ColorIdentifier.Normal, candidate));
					}
				}

				if (node.Parent is { Assignment.Map: var parentCandidates })
				{
					tryAndErrorLinkOffsets.Add(new(ColorIdentifier.Normal, parentCandidates, currentCandidates, false));
				}
			}

			// Contradiction-related view nodes.
			ReadOnlySpan<ViewNode> contradictionViewNodes = [
				.. failedSpace.Type == SpaceType.RowColumn
					? [
						new CellViewNode(ColorIdentifier.Auxiliary1, failedSpace.Cell),
						..
						from digit in initialGrid.GetCandidates(failedSpace.Cell)
						select new CandidateViewNode(ColorIdentifier.Auxiliary1, failedSpace.Cell * 9 + digit)
					]
					: ReadOnlySpan<ViewNode>.Empty,
				.. failedSpace.Type != SpaceType.RowColumn
					? [
						new HouseViewNode(ColorIdentifier.Auxiliary1, failedSpace.House),
						..
						from cell in HousesMap[failedSpace.House] & CandidatesMap[failedSpace.Digit]
						select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + failedSpace.Digit)
					]
					: ReadOnlySpan<ViewNode>.Empty
			];

			burredCandidates = analyzeBurredCandidates(linkOffsets, initialGrid, out truths, out links);
			var missingCandidateOffsets = new List<CandidateViewNode>();
			foreach (var candidate in burredCandidates)
			{
				missingCandidateOffsets.Add(new(ColorIdentifier.Auxiliary2, candidate));
			}

			return [
				[.. candidateOffsets, .. linkOffsets, .. contradictionViewNodes, .. missingCandidateOffsets],
				[.. tryAndErrorCandidateOffsets, .. tryAndErrorLinkOffsets, .. contradictionViewNodes]
			];
		}

		static CandidateMap analyzeBurredCandidates(
			List<ChainLinkViewNode> linkOffsets,
			in Grid initialGrid,
			out ReadOnlyMemory<Space> truths,
			out ReadOnlyMemory<Space> links
		)
		{
			var truthsList = new List<Space>();
			var linksList = new List<Space>();
			var result = CandidateMap.Empty;
			foreach (var link in linkOffsets)
			{
				// Analyzes the link type.
				// Because of design of algorithm, there're only 4 kinds of links used,
				// and can exactly corresponds to 4 kinds of spaces.
				var space = (link.Start, link.End) switch
				{
					({ Digits: var d1, Cells: [var c1] }, { Digits: var d2, Cells: [var c2] }) when d1 != d2 && c1 == c2
						=> Space.RowColumn(c1 / 9, c1 % 9),
					({ Digits: var d1, Cells: var c1 }, { Digits: var d2, Cells: var c2 })
					when d1 == d2 && BitOperations.IsPow2(d1) && Mask.Log2(d1) is var digit && (c1 | c2).FirstSharedHouse is var house
						=> house switch
						{
							< 9 => Space.BlockNumber(house, digit),
							< 18 => Space.RowNumber(house - 9, digit),
							_ => Space.ColumnNumber(house - 18, digit)
						},
					_ => throw new NotImplementedException()
				};
				(link.IsStrongLink ? truthsList : linksList).Add(space);

				if (link.IsStrongLink)
				{
					// If the link is strong, we should check which candidates are not hold for the link in the target space.
					if (space.Type == SpaceType.RowColumn)
					{
						var missingDigits = initialGrid.GetCandidates(space.Cell) & ~link.Start.Digits & ~link.End.Digits;
						foreach (var digit in missingDigits)
						{
							result.Add(space.Cell * 9 + digit);
						}
					}
					else
					{
						var missingCells = HousesMap[space.House] & CandidatesMap[space.Digit] & ~link.Start.Cells & ~link.End.Cells;
						foreach (var cell in missingCells)
						{
							result.Add(cell * 9 + space.Digit);
						}
					}
				}
			}

			(truths, links) = (truthsList.AsMemory(), linksList.AsMemory());
			return result;
		}
	}
}
