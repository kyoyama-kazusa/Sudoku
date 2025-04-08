namespace Sudoku.Analytics.Construction.Chaining.Rules;

/// <summary>
/// Represents a chaining rule on locked candidates (i.e. <see cref="LinkType.LockedCandidates"/>).
/// </summary>
/// <seealso cref="LinkType.LockedCandidates"/>
public sealed class LockedCandidatesChainingRule : ChainingRule
{
	/// <inheritdoc/>
	public override void GetLinks(in Grid grid, LinkDictionary strongLinks, LinkDictionary weakLinks, StepGathererOptions options)
	{
		if (options.GetLinkOption(LinkType.LockedCandidates) == LinkOption.None)
		{
			return;
		}

		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { CandidatesMap: var __CandidatesMap };
		// VARIABLE_DECLARATION_END

		// Strong.
		for (var house = 0; house < 27; house++)
		{
			for (var digit = 0; digit < 9; digit++)
			{
				var cells = HousesMap[house] & __CandidatesMap[digit];
				if (!Grouped.IsGroupedStrongLink(cells, house, out var pairHouse))
				{
					continue;
				}

				var firstPair = pairHouse[0];
				var h1 = HouseMask.TrailingZeroCount(firstPair);
				var h2 = firstPair.GetNextSet(h1);
				var cells1 = cells & HousesMap[h1];
				var cells2 = cells & HousesMap[h2];
				var node1 = new Node(cells1 * digit, false);
				var node2 = new Node(cells2 * digit, true);
				strongLinks.AddEntry(node1, node2);
			}
		}

		// Weak.
		for (var house = 0; house < 27; house++)
		{
			for (var digit = 0; digit < 9; digit++)
			{
				if ((HousesMap[house] & __CandidatesMap[digit]) is not { Count: > 2 } cells)
				{
					continue;
				}

				foreach (ref readonly var cells1 in cells | 3)
				{
					if (!cells1.IsInIntersection)
					{
						continue;
					}

					foreach (ref readonly var cells2 in cells & ~cells1 | 3)
					{
						if (!cells2.IsInIntersection || cells1.Count * cells2.Count == 1)
						{
							continue;
						}

						var node1 = new Node(cells1 * digit, true);
						var node2 = new Node(cells2 * digit, false);
						weakLinks.AddEntry(node1, node2);
					}
				}
			}
		}
	}

	/// <inheritdoc/>
	public override void GetLoopConclusions(in Grid grid, ReadOnlySpan<Link> links, ref ConclusionSet conclusions)
	{
		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { CandidatesMap: var __CandidatesMap };
		// VARIABLE_DECLARATION_END

		var result = ConclusionSet.Empty;
		foreach (var element in links)
		{
			if (element is ({ Map: { Digits: var digitsMask1, Cells: var cells1 } }, { Map: { Digits: var digitsMask2, Cells: var cells2 } })
				{
					GroupedLinkPattern: null
				}
				&& digitsMask1 == digitsMask2 && Mask.IsPow2(digitsMask1) && Mask.IsPow2(digitsMask2)
				&& Mask.Log2(digitsMask1) is var digit
				&& (cells1 & cells2 & __CandidatesMap[digit]) is { Count: not 0 } intersection)
			{
				result.AddRange(from cell in intersection select new Conclusion(Elimination, cell, digit));
			}
		}
		conclusions |= result;
	}
}
