namespace Sudoku.Analytics.StepSearchers.SingleDigitPatterns;

/// <summary>
/// Provides with a <b>Two-strong Links</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Skyscraper</item>
/// <item>Two-string Kite</item>
/// <item>Turbot Fish</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_TwoStrongLinksStepSearcher",
	Technique.Skyscraper, Technique.TwoStringKite, Technique.TurbotFish,
	IsCachingUnsafe = true)]
public sealed partial class TwoStrongLinksStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether the searcher will ignore patterns when it can formed a dual two-strong-link pattern (i.e. a remote pair).
	/// </summary>
	[SettingItemName(SettingItemNames.DisableRemotePair)]
	public bool DisableRemotePair { get; set; } = false;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		for (var digit = 0; digit < 9; digit++)
		{
			for (var h1 = 0; h1 < 26; h1++)
			{
				for (var h2 = h1 + 1; h2 < 27; h2++)
				{
					// Get masks.
					var mask1 = (HousesMap[h1] & CandidatesMap[digit]) / h1;
					var mask2 = (HousesMap[h2] & CandidatesMap[digit]) / h2;
					if (Mask.PopCount(mask1) != 2 || Mask.PopCount(mask2) != 2)
					{
						continue;
					}

					// Get all cells.
					var (cells1, cells2) = (CellMap.Empty, CellMap.Empty);
					var (cellsList1, cellsList2) = (new List<Cell>(2), new List<Cell>(2));
					foreach (var pos1 in mask1)
					{
						var cell1 = HousesCells[h1][pos1];
						cellsList1.Add(cell1);
						cells1.Add(cell1);
					}
					foreach (var pos2 in mask2)
					{
						var cell2 = HousesCells[h2][pos2];
						cellsList2.Add(cell2);
						cells2.Add(cell2);
					}

					if (cells1 && cells2)
					{
						continue;
					}

					// Check two cells share a same house.
					House sameHouse;
					int headIndex, tailIndex, c1Index, c2Index;
					for (var i = 0; i < 2; i++)
					{
						var cell1 = cellsList1[i];
						for (var j = 0; j < 2; j++)
						{
							var cell2 = cellsList2[j];
							if ((sameHouse = (cell1.AsCellMap() + cell2).FirstSharedHouse) != 32)
							{
								(c1Index, c2Index) = (i, j);
								(headIndex, tailIndex) = (i == 0 ? 1 : 0, j == 0 ? 1 : 0);
								goto Checking;
							}
						}
					}

					// Not same house.
					continue;

				Checking:
					// Two strong link found.
					// Now we should collect for eliminations.
					var (head, tail) = (cellsList1[headIndex], cellsList2[tailIndex]);
					if ((head.AsCellMap() + tail).FirstSharedHouse != 32)
					{
						// A standard X-Wing pattern.
						continue;
					}

					if ((PeersMap[head] & PeersMap[tail] & CandidatesMap[digit]) is not (var elimMap and not []))
					{
						// Eliminations may not found.
						continue;
					}

					// We should insert an extra check - whether all 4 chosen cells are bi-value cells
					// and contain same two digits. If so, this will be a technique called "Remote Pair".
					// Remote Pair patterns may contain 4 or more bi-value cells, with same 2 digits found.
					// However, it will be generalized and replaced by two-strong-link techniques.
					// Here is an optimization: We should ignore the pattern found here
					// if all four used cells only use same two digits, and the other digit also contains elimination.
					//
					// Maybe you might say, "We can change the rule (this step searcher logic) to collect both two-strong-links
					// and remote pairs!". Okay, I admit this is a good question but the answer is no.
					// Because the remote pair pattern is designed as a harder technique to be used. If we put here to find them,
					// we may break the balance on the difficulty on searching for such patterns.
					if (DisableRemotePair
						&& grid[cellsList1[c1Index].AsCellMap() + cellsList2[c2Index] + head + tail] is var mergedDigitsMask
						&& Mask.PopCount(mergedDigitsMask) == 2
						&& mergedDigitsMask.AllSets is var pairDigits
						&& (pairDigits[0] == digit ? pairDigits[1] : pairDigits[0]) is var theOtherDigit
						&& !!(CandidatesMap[theOtherDigit] & elimMap))
					{
						// Skip the pattern when it is a remote pair.
						continue;
					}

					var step = new TwoStrongLinksStep(
						(from cell in elimMap select new Conclusion(Elimination, cell, digit)).ToArray(),
						[
							[
								new CandidateViewNode(ColorIdentifier.Normal, cellsList1[c1Index] * 9 + digit),
								new CandidateViewNode(ColorIdentifier.Normal, cellsList2[c2Index] * 9 + digit),
								new CandidateViewNode(ColorIdentifier.Normal, head * 9 + digit),
								new CandidateViewNode(ColorIdentifier.Normal, tail * 9 + digit),
								new HouseViewNode(ColorIdentifier.Normal, h1),
								new HouseViewNode(ColorIdentifier.Normal, h2),
								new HouseViewNode(ColorIdentifier.Auxiliary1, sameHouse)
							]
						],
						context.Options,
						digit,
						h1,
						h2,
						false
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}

		return null;
	}
}
