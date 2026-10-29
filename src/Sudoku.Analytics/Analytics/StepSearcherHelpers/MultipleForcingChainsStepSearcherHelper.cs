namespace Sudoku.Analytics.StepSearcherHelpers;

/// <summary>
/// Represents a type that can search for multiple forcing chains. This type can also search for finned chains.
/// </summary>
internal sealed class MultipleForcingChainsStepSearcherHelper : ForcingChainsStepSearcherHelper
{
	/// <inheritdoc/>
	public override ReadOnlyMemory<Type> SupportedStepSearcherTypes
		=> (Type[])[
			typeof(MultipleForcingChainsStepSearcher),
			typeof(FinnedChainStepSearcher),
			typeof(GroupedFinnedChainStepSearcher)
		];


	/// <summary>
	/// The collect method called by multiple forcing chains step searcher.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="accumulator">The instance that temporarily records for chain steps.</param>
	/// <param name="allowsAdvancedLinks">Indicates whether the method allows advanced links.</param>
	/// <param name="onlyFindFinnedChain">Indicates whether the method only finds for (grouped) finned chains.</param>
	/// <returns>The first found step.</returns>
	public static Step? CollectCore(
		ref StepAnalysisContext context,
		SortedSet<ChainStep> accumulator,
		bool allowsAdvancedLinks,
		bool onlyFindFinnedChain
	)
	{
		return unsafe (
			CollectGeneralizedMultipleCore(
				ref context,
				accumulator,
				allowsAdvancedLinks,
				onlyFindFinnedChain,
				&component,
				&CollectMultipleForcingChains,
				&stepCreator
			)
		);


		static MultipleChainBasedComponent component(MultipleForcingChains mfc)
			=> mfc.IsCellMultiple ? MultipleChainBasedComponent.Cell : MultipleChainBasedComponent.House;

		static MultipleForcingChainsStep stepCreator(
			MultipleForcingChains chain,
			in Grid grid,
			ref readonly StepAnalysisContext context,
			ChainingRuleCollection supportedRules
		) => new(chain.Conclusions, ((IForcingChains)chain).GetViews(grid, chain.Conclusions, supportedRules), context.Options, chain);
	}

	/// <summary>
	/// Collect all multiple forcing chains appeared in a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="onlyFindOne">Indicates whether the method only find one valid chain.</param>
	/// <returns>All possible multiple forcing chains instances.</returns>
	public static ReadOnlySpan<MultipleForcingChains> CollectMultipleForcingChains(in Grid grid, bool onlyFindOne)
	{
		var result = new SortedSet<MultipleForcingChains>(ChainingComparers.MultipleForcingChainsComparer);
		foreach (var cell in EmptyCells & ~BivalueCells)
		{
			var nodesSupposedOn_GroupedByDigit = new Dictionary<Candidate, HashSet<Node>>();
			var nodesSupposedOff_GroupedByDigit = new Dictionary<Candidate, HashSet<Node>>();
			var nodesSupposedOn_InCell = default(HashSet<Node>);
			var nodesSupposedOff_InCell = default(HashSet<Node>);
			var digitsMask = grid.GetCandidates(cell);
			foreach (var digit in digitsMask)
			{
				var currentNode = new Node((cell * 9 + digit).AsCandidateMap(), true);
				var (nodesSupposedOn, nodesSupposedOff) = FindForcingChains(currentNode);

				// Iterate on three house types, to collect with region forcing chains.
				if (chaining_Region(cell, digit, grid, nodesSupposedOn, nodesSupposedOff) is var regionForcingChainsFound and not [])
				{
					return regionForcingChainsFound;
				}

				nodesSupposedOn_GroupedByDigit.Add(cell * 9 + digit, nodesSupposedOn);
				nodesSupposedOff_GroupedByDigit.Add(cell * 9 + digit, nodesSupposedOff);
				if (nodesSupposedOn_InCell is null)
				{
					nodesSupposedOn_InCell = [with(ChainingComparers.NodeMapComparer)];
					nodesSupposedOff_InCell = [with(ChainingComparers.NodeMapComparer)];
					nodesSupposedOn_InCell.UnionWith(nodesSupposedOn);
					nodesSupposedOff_InCell.UnionWith(nodesSupposedOff);
				}
				else
				{
					Debug.Assert(nodesSupposedOff_InCell is not null);
					nodesSupposedOn_InCell.IntersectWith(nodesSupposedOn);
					nodesSupposedOff_InCell.IntersectWith(nodesSupposedOff);
				}
			}

			if (chaining_Cell(
				cell, digitsMask, grid, nodesSupposedOn_GroupedByDigit, nodesSupposedOff_GroupedByDigit,
				nodesSupposedOn_InCell, nodesSupposedOff_InCell)
				is var cellForcingChainsFound and not [])
			{
				return cellForcingChainsFound;
			}
		}
		return result.ToArray();


		ReadOnlySpan<MultipleForcingChains> chaining_Cell(
			Cell cell,
			Mask digitsMask,
			in Grid grid,
			Dictionary<Cell, HashSet<Node>> nodesSupposedOn_GroupedByDigit,
			Dictionary<Cell, HashSet<Node>> nodesSupposedOff_GroupedByDigit,
			HashSet<Node>? nodesSupposedOn_InCell,
			HashSet<Node>? nodesSupposedOff_InCell
		)
		{
			//////////////////////////////////////
			// Collect with cell forcing chains //
			//////////////////////////////////////
			var cellOn = cfcOn(grid, cell, nodesSupposedOn_GroupedByDigit, nodesSupposedOn_InCell, digitsMask);
			if (!cellOn.IsEmpty)
			{
				return cellOn;
			}
			var cellOff = cfcOff(grid, cell, nodesSupposedOff_GroupedByDigit, nodesSupposedOff_InCell, digitsMask);
			if (!cellOff.IsEmpty)
			{
				return cellOff;
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> chaining_Region(
			Cell cell,
			Digit digit,
			in Grid grid,
			HashSet<Node> nodesSupposedOn,
			HashSet<Node> nodesSupposedOff
		)
		{
			foreach (var houseType in HouseTypes)
			{
				var house = cell.GetHouse(houseType);
				var cellsInHouse = HousesMap[house] & CandidatesMap[digit];
				if (cellsInHouse.Count <= 2)
				{
					// There's no need iterating on such house because the chain only contains 2 branches,
					// which means it can be combined into one normal chain.
					continue;
				}

				var firstCellInHouse = cellsInHouse[0];
				if (firstCellInHouse != cell)
				{
					// We should skip the other cells in the house, in order to avoid duplicate forcing chains.
					continue;
				}

				var nodesSupposedOn_GroupedByHouse = new Dictionary<Candidate, HashSet<Node>>();
				var nodesSupposedOff_GroupedByHouse = new Dictionary<Candidate, HashSet<Node>>();
				var nodesSupposedOn_InHouse = new HashSet<Node>(ChainingComparers.NodeMapComparer);
				var nodesSupposedOff_InHouse = new HashSet<Node>(ChainingComparers.NodeMapComparer);
				foreach (var otherCell in cellsInHouse)
				{
					var otherCandidate = otherCell * 9 + digit;
					if (otherCell == cell)
					{
						nodesSupposedOn_GroupedByHouse.Add(otherCandidate, nodesSupposedOn);
						nodesSupposedOff_GroupedByHouse.Add(otherCandidate, nodesSupposedOff);
						nodesSupposedOn_InHouse.UnionWith(nodesSupposedOn);
						nodesSupposedOff_InHouse.UnionWith(nodesSupposedOff);
					}
					else
					{
						var other = new Node(otherCandidate.AsCandidateMap(), true);
						var (otherNodesSupposedOn_InHouse, otherNodesSupposedOff_InHouse) = FindForcingChains(other);
						nodesSupposedOn_GroupedByHouse.Add(otherCandidate, otherNodesSupposedOn_InHouse);
						nodesSupposedOff_GroupedByHouse.Add(otherCandidate, otherNodesSupposedOff_InHouse);
						nodesSupposedOn_InHouse.IntersectWith(otherNodesSupposedOn_InHouse);
						nodesSupposedOff_InHouse.IntersectWith(otherNodesSupposedOff_InHouse);
					}
				}

				////////////////////////////////////////
				// Collect with region forcing chains //
				////////////////////////////////////////
				var regionOn = rfcOn(grid, digit, cellsInHouse, nodesSupposedOn_GroupedByHouse, nodesSupposedOn_InHouse);
				if (!regionOn.IsEmpty)
				{
					return regionOn;
				}
				var regionOff = rfcOff(grid, digit, cellsInHouse, nodesSupposedOff_GroupedByHouse, nodesSupposedOff_InHouse);
				if (!regionOff.IsEmpty)
				{
					return regionOff;
				}
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> cfcOn(
			in Grid grid,
			Cell cell,
			Dictionary<Candidate, HashSet<Node>> onNodes,
			HashSet<Node>? resultOnNodes,
			Mask digitsMask
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

				var cfc = new MultipleForcingChains(conclusion);
				foreach (var d in digitsMask)
				{
					var branchNode = onNodes[cell * 9 + d].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					cfc.Add(cell * 9 + d, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (onlyFindOne)
				{
					return (MultipleForcingChains[])[cfc];
				}
				result.Add(cfc);
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> cfcOff(
			in Grid grid,
			Cell cell,
			Dictionary<Candidate, HashSet<Node>> offNodes,
			HashSet<Node>? resultOffNodes,
			Mask digitsMask
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

				var cfc = new MultipleForcingChains();
				foreach (var d in digitsMask)
				{
					var branchNode = offNodes[cell * 9 + d].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					cfc.Add(cell * 9 + d, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (cfc.GetThoroughConclusions(grid) is not { Length: not 0 } conclusions)
				{
					continue;
				}

				cfc.Conclusions = conclusions;
				if (onlyFindOne)
				{
					return (MultipleForcingChains[])[cfc];
				}
				result.Add(cfc);
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> rfcOn(
			in Grid grid,
			Digit digit,
			scoped in CellMap cellsInHouse,
			Dictionary<Candidate, HashSet<Node>> onNodes,
			HashSet<Node> houseOnNodes
		)
		{
			foreach (var node in houseOnNodes)
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

				var rfc = new MultipleForcingChains(conclusion);
				foreach (var c in cellsInHouse)
				{
					var branchNode = onNodes[c * 9 + digit].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(c * 9 + digit, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (onlyFindOne)
				{
					return (MultipleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}

		ReadOnlySpan<MultipleForcingChains> rfcOff(
			in Grid grid,
			Digit digit,
			scoped in CellMap cellsInHouse,
			Dictionary<Candidate, HashSet<Node>> offNodes,
			HashSet<Node> houseOffNodes
		)
		{
			foreach (var node in houseOffNodes)
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

				var rfc = new MultipleForcingChains();
				foreach (var c in cellsInHouse)
				{
					var branchNode = offNodes[c * 9 + digit].First(n => n.Equals(node, NodeComparison.IncludeIsOn));
					rfc.Add(c * 9 + digit, node.IsOn ? new StrongForcingChain(branchNode) : new WeakForcingChain(branchNode));
				}
				if (rfc.GetThoroughConclusions(grid) is not { Length: not 0 } conclusions)
				{
					continue;
				}

				rfc.Conclusions = conclusions;
				if (onlyFindOne)
				{
					return (MultipleForcingChains[])[rfc];
				}
				result.Add(rfc);
			}
			return [];
		}
	}
}
