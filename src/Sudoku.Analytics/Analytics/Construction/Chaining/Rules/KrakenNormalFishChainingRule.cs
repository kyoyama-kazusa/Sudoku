namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on normal fish rule (i.e. <see cref="LinkType.KrakenNormalFish"/>).
/// </summary>
/// <seealso cref="LinkType.KrakenNormalFish"/>
public sealed class KrakenNormalFishChainingRule : ChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.KrakenNormalFish) == LinkOption.None)
		{
			return;
		}

		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { ValuesMap: var __ValuesMap, CandidatesMap: var __CandidatesMap };
		// VARIABLE_DECLARATION_END

		// Collect for available rows and columns.
		var sets = (stackalloc HouseMask[9]);
		sets.Clear();
		for (var digit = 0; digit < 9; digit++)
		{
			if (__ValuesMap[digit].Count == 9)
			{
				continue;
			}

			for (var house = 9; house < 27; house++)
			{
				if ((__CandidatesMap[digit] & HousesMap[house]).Count >= 2)
				{
					sets[digit] |= 1 << house;
				}
			}
		}

		// Iterate on each combination of base and cover sets.
		for (var size = 2; size <= 4; size++)
		{
			for (var digit = 0; digit < 9; digit++)
			{
				collect(true, size, digit, sets, __CandidatesMap);
				collect(false, size, digit, sets, __CandidatesMap);
			}
		}


		void collect(bool isRow, Digit size, Digit digit, Span<HouseMask> sets, ReadOnlySpan<CellMap> candidatesMap)
		{
			var linkOption = options.GetLinkOption(LinkType.KrakenNormalFish);
			var baseSetsToIterate = (sets[digit] & ~(isRow ? HouseMaskOperations.AllColumnsMask : HouseMaskOperations.AllRowsMask)).GetAllSets();
			var coverSetsToIterate = (sets[digit] & ~(isRow ? HouseMaskOperations.AllRowsMask : HouseMaskOperations.AllColumnsMask)).GetAllSets();
			if (baseSetsToIterate.Length < size || coverSetsToIterate.Length < size)
			{
				return;
			}

			foreach (var bs in baseSetsToIterate.GetSubsets(size))
			{
				var (baseSetsMap, baseSetIsValid) = (CellMap.Empty, true);
				foreach (var p in bs)
				{
					var cells = candidatesMap[digit] & HousesMap[p];
					if (cells.Count < 2)
					{
						// Here we keep the pattern to be "readable", we don't allow Sashimi Kraken Fishes here.
						// Sometimes the Sashimi Kraken Fishes may look very ugly (e.g. X-Wing with only 3 positions).
						baseSetIsValid = false;
						break;
					}
					baseSetsMap |= cells;
				}
				if (!baseSetIsValid)
				{
					continue;
				}

				var baseSetsMask = HouseMaskOperations.Create(bs);
				foreach (var cs in coverSetsToIterate.GetSubsets(size))
				{
					var coverSetsMap = CellMap.Empty;
					foreach (var p in cs)
					{
						var cells = candidatesMap[digit] & HousesMap[p];
						coverSetsMap |= cells;
					}

					var fins = baseSetsMap & ~coverSetsMap;
					if (!fins)
					{
						continue;
					}

					var coverSetsMask = HouseMaskOperations.Create(cs);
					var fish = new FishPattern(digit, baseSetsMask, coverSetsMask, fins, CellMap.Empty);
					if (linkOption == LinkOption.Intersection && !fins.IsInIntersection
						|| linkOption == LinkOption.House && fins.FirstSharedHouse == 32)
					{
						continue;
					}

					// Strong.
					var cells1 = baseSetsMap & ~fins;
					if (cells1.Count < size << 1 || Mask.IsPow2(cells1.BlockMask))
					{
						// A valid fish node cannot be degenerated, or all cells are inside a block.
						continue;
					}

					// Verification: avoid X-Wing nodes that can also be used as UR nodes.
					// This will fix issue #672: https://github.com/KyouyamaKazusa0805/Sudoku/issues/672
					// Counter-example:
					//   .+1..6...5..9....1.3....12..2..4+98...+9.1.5..8..68..39...9..3......4..5..91..2+4+97..:714 814 724 824 327 734 834 657 659 169 571 674 184 885
					if (Mask.PopCount(cells1.BlockMask) == 2)
					{
						continue;
					}

					var node1 = new Node(cells1 * digit, false);
					var node2 = new Node(fins * digit, true);
					strongLinks.AddEntry(node1, node2, true, fish);

					// Weak.
					// Please note that weak links may not contain pattern objects,
					// because it will be rendered into view nodes; but they are plain ones,
					// behaved as normal locked candidate nodes.
					var elimMap = coverSetsMap & ~baseSetsMap;
					var node3 = ~node1;
					var limit = linkOption switch
					{
						LinkOption.Intersection => 3,
						LinkOption.House => Math.Min(elimMap.Count, 9),
						LinkOption.All => elimMap.Count,
						_ => 3
					};
					foreach (ref readonly var cells4 in elimMap | limit)
					{
						if (linkOption == LinkOption.Intersection && !cells4.IsInIntersection
							|| linkOption == LinkOption.House && cells4.FirstSharedHouse == 32)
						{
							continue;
						}

						var node4 = new Node(cells4 * digit, false);
						weakLinks.AddEntry(node3, node4);
					}
				}
			}
		}
	}

	/// <inheritdoc/>
	public override void GetViewNodes(
		in Grid grid,
		Chain pattern,
		View view,
		ref int currentAlsIndex,
		ref int currentUrIndex,
		out ReadOnlySpan<ViewNode> producedViewNodes
	)
	{
		var candidatesMap = grid.CandidatesMap;
		var result = new List<ViewNode>();
		foreach (var link in pattern.Links)
		{
			if (link.GroupedLinkPattern is not FishPattern { Digit: var digit, BaseSets: var baseSets, Exofins: var fins })
			{
				continue;
			}

			foreach (var baseSet in baseSets)
			{
				foreach (var cell in candidatesMap[digit] & HousesMap[baseSet] & ~fins)
				{
					var candidate = cell * 9 + digit;
					if (view.FindCandidate(candidate) is { } candidateViewNode)
					{
						view.Remove(candidateViewNode);
					}

					var node = new CandidateViewNode(ColorIdentifier.Auxiliary2, candidate);
					view.Add(node);
					result.Add(node);
				}
			}
		}
		producedViewNodes = result.AsSpan();
	}

	/// <inheritdoc/>
	public override void GetLoopConclusions(in Grid grid, ReadOnlySpan<Link> links, ref ConclusionSet conclusions)
	{
		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { CandidatesMap: var __CandidatesMap };
		// VARIABLE_DECLARATION_END

		var result = ConclusionSet.Empty;
		foreach (var link in links)
		{
			if (link is not ({ Map.Cells: var firstCells }, { Map.Cells: var secondCells })
				{
					GroupedLinkPattern: FishPattern { Digit: var digit, BaseSets: var baseSets, CoverSets: var coverSets }
				})
			{
				continue;
			}

			var elimMap = CellMap.Empty;
			foreach (var coverSet in coverSets)
			{
				elimMap |= HousesMap[coverSet] & __CandidatesMap[digit];
			}
			foreach (var baseSet in baseSets)
			{
				elimMap &= ~HousesMap[baseSet];
			}

			// Filter eliminations on strong links.
			foreach (var otherLink in links)
			{
				if (otherLink is ({ Map.Cells: var map1 }, { Map.Cells: var map2 }, true) && otherLink != link)
				{
					elimMap &= ~map1;
					elimMap &= ~map2;
				}
			}
			result.AddRange(from cell in elimMap select new Conclusion(Elimination, cell, digit));
		}
		conclusions |= result;
	}
}
