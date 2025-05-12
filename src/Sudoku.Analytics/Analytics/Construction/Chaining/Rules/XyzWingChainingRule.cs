namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on XYZ-Wing rule (i.e. <see cref="LinkType.XyzWing"/>).
/// </summary>
/// <seealso cref="LinkType.XyzWing"/>
public sealed class XyzWingChainingRule : ChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.XyzWing) is not (var linkOption and not LinkOption.None))
		{
			return;
		}

		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { CandidatesMap: var __CandidatesMap, EmptyCells: var __EmptyCells };
		// VARIABLE_DECLARATION_END

		// Iterate on each XYZ-Wing pattern, to get strong links.
		foreach (var pattern in new XyzWingPatternSearcher().Search(grid))
		{
			var (pivot, leafCell1, leafCell2, _, _, _, zDigit) = pattern;
			var patternCells = pivot.AsCellMap() + leafCell1 + leafCell2;
			foreach (ref readonly var pair in patternCells & 2)
			{
				if (!pair.CanSeeEachOther)
				{
					continue;
				}

				var cells1 = pair;
				var cells2 = patternCells & ~pair;
				if (linkOption == LinkOption.Intersection && !(cells1.IsInIntersection && cells2.IsInIntersection)
					|| linkOption == LinkOption.House
					&& !(cells1.FirstSharedHouse != FallbackConstants.@int && cells2.FirstSharedHouse != FallbackConstants.@int))
				{
					goto CollectWeak;
				}

				// Strong.
				var node1 = new Node(cells1 * zDigit, false);
				var node2 = new Node(cells2 * zDigit, true);
				strongLinks.AddEntry(node1, node2, true, pattern);

			CollectWeak:
				// Weak.
				// Please note that weak links may not contain pattern objects,
				// because it will be rendered into view nodes; but they are plain ones,
				// behaved as normal locked candidate nodes.
				var possibleCells1 = cells1.PeerIntersection & __CandidatesMap[zDigit];
				var possibleCells2 = cells2.PeerIntersection & __CandidatesMap[zDigit];
				var (limit1, limit2) = linkOption switch
				{
					LinkOption.House => (Math.Min((__EmptyCells & possibleCells1).Count, 9), Math.Min((__EmptyCells & possibleCells2).Count, 9)),
					LinkOption.All => (possibleCells1.Count, possibleCells2.Count),
					_ => (3, 3)
				};
				foreach (ref readonly var cells in possibleCells1 | limit1)
				{
					if (linkOption == LinkOption.Intersection && !cells.IsInIntersection
						|| linkOption == LinkOption.House && cells.FirstSharedHouse == FallbackConstants.@int)
					{
						continue;
					}

					var node3 = new Node(cells1 * zDigit, true);
					var node4 = new Node(cells * zDigit, false);
					weakLinks.AddEntry(node3, node4);
				}
				foreach (ref readonly var cells in possibleCells2 | limit2)
				{
					if (linkOption == LinkOption.Intersection && !cells.IsInIntersection
						|| linkOption == LinkOption.House && cells.FirstSharedHouse == FallbackConstants.@int)
					{
						continue;
					}

					var node3 = new Node(cells2 * zDigit, true);
					var node4 = new Node(cells * zDigit, false);
					weakLinks.AddEntry(node3, node4);
				}
			}
		}
	}

	/// <inheritdoc/>
	public override void GetViewNodes(
		in Grid grid,
		Chain pattern,
		View view,
		ProcessedViewNodeMap processedViewNodesMap,
		out ReadOnlySpan<ViewNode> producedViewNodes
	)
	{
		var result = new List<ViewNode>();
		foreach (var link in pattern.Links)
		{
			if (link.GroupedLinkPattern is not XyzWingPattern { Cells: var cells })
			{
				continue;
			}

			foreach (var cell in cells)
			{
				var node = new CellViewNode(ColorIdentifier.Normal, cell);
				view.Add(node);
				result.Add(node);
			}
		}
		producedViewNodes = result.AsSpan();
	}
}
