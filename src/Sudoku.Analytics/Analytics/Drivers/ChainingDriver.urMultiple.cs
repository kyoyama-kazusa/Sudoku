namespace Sudoku.Analytics.Drivers;

internal partial class ChainingDriver
{
	/// <summary>
	/// Collect all multiple forcing chains on applying to a unique rectangle, appeared in a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one valid chain.</param>
	/// <returns>All possible multiple forcing chain instances.</returns>
	public static ReadOnlySpan<RectangleForcingChains> CollectRectangleMultipleForcingChains(in Grid grid, bool onlyFindOne)
	{
		var result = new SortedSet<RectangleForcingChains>(ChainingComparers.MultipleForcingChainsComparer);
		foreach (var urCells in UniqueRectanglePattern.AllPatterns)
		{
			var cells = urCells.AsCellMap();
			if ((EmptyCells & cells) != cells)
			{
				// Not all 4 cells are empty cells.
				continue;
			}

			// Collect all digits of the rectangle, to determine which digits can be used as a rectangle.
			var allDigitsMask = grid[cells].AllSets;
			if (allDigitsMask.Length is 0 or 1)
			{
				// No enough digits available.
				continue;
			}

			// Iterate on each combination of pair of digits, as ones used by rectangle.
			foreach (var digitPair in allDigitsMask.GetSubsets(2))
			{
				var (d1, d2) = (digitPair[0], digitPair[1]);

				// Determine whether such digits can be filled in diagonal cells.
				// If a rectangle is correct, we should guarantee both digits can be filled twice in rectangle pattern,
				// in order to make a valid deadly pattern (i.e. guarantee having at least one possible unavoidable set).
				if (!UniqueRectanglePattern.CanMakeDeadlyPattern(grid, d1, d2, urCells))
				{
					continue;
				}

				// If so, the UR pattern is valid. Now check for forcing chains.
				var branchStartCandidates = CandidateMap.Empty;
				foreach (var cell in urCells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~(1 << d1 | 1 << d2)))
					{
						branchStartCandidates.Add(cell * 9 + digit);
					}
				}

				// Collect branches for all possible candidates to be iterated,
				// and determine whether at least one candidate can be considered as a conclusion from all different start candidates.
				var nodesSupposedOnGrouped = new Dictionary<Candidate, HashSet<Node>>();
				var nodesSupposedOffGrouped = new Dictionary<Candidate, HashSet<Node>>();
				var resultNodesSupposedOn = default(HashSet<Node>);
				var resultNodesSupposedOff = default(HashSet<Node>);
				foreach (var candidate in branchStartCandidates)
				{
					var currentNode = new Node(candidate.AsCandidateMap(), true);
					var (nodesSupposedOn, nodesSupposedOff) = FindForcingChains(currentNode);

					nodesSupposedOnGrouped.Add(candidate, nodesSupposedOn);
					nodesSupposedOffGrouped.Add(candidate, nodesSupposedOff);
					if (resultNodesSupposedOn is null)
					{
						resultNodesSupposedOn = new(ChainingComparers.NodeMapComparer);
						resultNodesSupposedOff = new(ChainingComparers.NodeMapComparer);
						resultNodesSupposedOn.UnionWith(nodesSupposedOn);
						resultNodesSupposedOff.UnionWith(nodesSupposedOff);
					}
					else
					{
						Debug.Assert(resultNodesSupposedOff is not null);
						resultNodesSupposedOn.IntersectWith(nodesSupposedOn);
						resultNodesSupposedOff.IntersectWith(nodesSupposedOff);
					}
				}

				var step1 = rfcOn(urCells, grid, d1, d2, branchStartCandidates, nodesSupposedOnGrouped, resultNodesSupposedOn);
				if (!step1.IsEmpty)
				{
					return step1;
				}

				var step2 = rfcOff(urCells, grid, d1, d2, branchStartCandidates, nodesSupposedOffGrouped, resultNodesSupposedOff);
				if (!step2.IsEmpty)
				{
					return step2;
				}
			}
		}
		return result.ToArray();


		ReadOnlySpan<RectangleForcingChains> rfcOn(
			Cell[] urCells,
			in Grid grid,
			Digit d1,
			Digit d2,
			scoped in CandidateMap branchStartCandidates,
			Dictionary<Candidate, HashSet<Node>> onNodes,
			HashSet<Node>? resultOnNodes
		)
		{
			foreach (var node in resultOnNodes ?? [])
			{
				if (node.IsGroupedNode)
				{
					// Grouped nodes are not supported as target node.
					continue;
				}

				var conclusion = new Conclusion(Assignment, node.Map[0]);
				if (grid.Exists(conclusion.Candidate) is not true)
				{
					continue;
				}

				var rfc = new RectangleForcingChains(urCells, (Mask)(1 << d1 | 1 << d2), conclusion);
				foreach (var candidate in branchStartCandidates)
				{
					var branchNode = onNodes[candidate].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(candidate, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (onlyFindOne)
				{
					return (RectangleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}

		ReadOnlySpan<RectangleForcingChains> rfcOff(
			Cell[] urCells,
			in Grid grid,
			Digit d1,
			Digit d2,
			scoped in CandidateMap branchStartCandidates,
			Dictionary<Candidate, HashSet<Node>> offNodes,
			HashSet<Node>? resultOffNodes
		)
		{
			foreach (var node in resultOffNodes ?? [])
			{
				if (node.IsGroupedNode)
				{
					// Grouped nodes are not supported as target node.
					continue;
				}

				var conclusion = new Conclusion(Elimination, node.Map[0]);
				if (grid.Exists(conclusion.Candidate) is not true)
				{
					continue;
				}

				var rfc = new RectangleForcingChains(urCells, (Mask)(1 << d1 | 1 << d2));
				foreach (var candidate in branchStartCandidates)
				{
					var branchNode = offNodes[candidate].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(candidate, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (rfc.GetThoroughConclusions(grid) is not { Length: not 0 } conclusions)
				{
					continue;
				}

				rfc.Conclusions = conclusions;
				if (onlyFindOne)
				{
					return (RectangleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}
	}
}
