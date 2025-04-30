namespace Sudoku.Analytics.StepSearchers.Fishes;

/// <summary>
/// Provides with a <b>Normal Fish</b> step searcher. The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Normal fishes:
/// <list type="bullet">
/// <item>X-Wing</item>
/// <item>Swordfish</item>
/// <item>Jellyfish</item>
/// </list>
/// </item>
/// <item>
/// Finned fishes:
/// <list type="bullet">
/// <item>
/// Finned normal fishes:
/// <list type="bullet">
/// <item>Finned X-Wing</item>
/// <item>Finned Swordfish</item>
/// <item>Finned Jellyfish</item>
/// </list>
/// </item>
/// <item>
/// Finned sashimi fishes:
/// <list type="bullet">
/// <item>Sashimi X-Wing</item>
/// <item>Sashimi Swordfish</item>
/// <item>Sashimi Jellyfish</item>
/// </list>
/// </item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_NormalFishStepSearcher",

	// Normal Fishes
	Technique.XWing, Technique.Swordfish, Technique.Jellyfish,

	// Finned Fishes
	Technique.FinnedXWing, Technique.FinnedSwordfish, Technique.FinnedJellyfish,

	// Sashimi Fishes
	Technique.SashimiXWing, Technique.SashimiSwordfish, Technique.SashimiJellyfish,

	// Siamese Fishes
	Technique.SiameseFinnedXWing, Technique.SiameseFinnedSwordfish, Technique.SiameseFinnedJellyfish,
	Technique.SiameseSashimiXWing, Technique.SiameseSashimiSwordfish, Technique.SiameseSashimiJellyfish)]
public sealed partial class NormalFishStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether Finned X-Wing and Sashimi X-Wing should be disabled.
	/// </summary>
	/// <remarks>
	/// This option may be used when you are used to spotting skyscrapers (grouped or non-grouped).
	/// All Finned X-Wings can be replaced with Grouped Skyscrapers, and all Sashimi X-Wings can be replaced with Non-grouped Skyscrapers.
	/// </remarks>
	[SettingItemName(SettingItemNames.DisableFinnedOrSashimiXWing)]
	public bool DisableFinnedOrSashimiXWing { get; set; }

	/// <summary>
	/// Indicates whether the step searcher allows searching for Siamese fishes.
	/// </summary>
	[SettingItemName(SettingItemNames.AllowSiameseNormalFish)]
	public bool AllowSiamese { get; set; }


	/// <inheritdoc/>
	protected internal override unsafe Step? Collect(ref StepAnalysisContext context)
	{
		var r = stackalloc House*[9];
		var c = stackalloc House*[9];
		Unsafe.InitBlock(r, 0, (uint)sizeof(House*) * 9);
		Unsafe.InitBlock(c, 0, (uint)sizeof(House*) * 9);

		ref readonly var grid = ref context.Grid;
		var accumulator = new List<FishStep>();
		for (var digit = 0; digit < 9; digit++)
		{
			if (ValuesMap[digit].Count > 5)
			{
				continue;
			}

			// Collect.
			for (var house = 9; house < 27; house++)
			{
				if (HousesMap[house] & CandidatesMap[digit])
				{
					if (house < 18)
					{
						if (r[digit] == null)
						{
							var ptr = stackalloc House[10];
							Unsafe.InitBlock(ptr, 0, 10 * sizeof(House));

							r[digit] = ptr;
						}

						r[digit][++r[digit][0]] = house;
					}
					else
					{
						if (c[digit] == null)
						{
							var ptr = stackalloc House[10];
							Unsafe.InitBlock(ptr, 0, 10 * sizeof(House));

							c[digit] = ptr;
						}

						c[digit][++c[digit][0]] = house;
					}
				}
			}
		}

		// Core invocation.
		for (var size = 2; size <= 4; size++)
		{
			Collect(accumulator, grid, ref context, size, r, c, false, true);
			Collect(accumulator, grid, ref context, size, r, c, false, false);
			Collect(accumulator, grid, ref context, size, r, c, true, true);
			Collect(accumulator, grid, ref context, size, r, c, true, false);
		}

		// For Siamese fish, we should manually deal with them.
		var siameses = AllowSiamese ? Siamese.Fish.GetSiamese(accumulator, grid) : [];
		if (context.OnlyFindOne)
		{
			return siameses is [var siamese, ..] ? siamese : accumulator is [var normal, ..] ? normal : null;
		}

		if (siameses.Length != 0)
		{
			foreach (var step in siameses)
			{
				context.Accumulator.Add(step);
			}
		}
		if (accumulator.Count != 0)
		{
			context.Accumulator.AddRange(accumulator);
		}
		return null;
	}

	/// <summary>
	/// Get all possible normal fishes.
	/// </summary>
	/// <param name="accumulator">The accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="size">The size.</param>
	/// <param name="r">The possible row table to iterate.</param>
	/// <param name="c">The possible column table to iterate.</param>
	/// <param name="withFin">Indicates whether the searcher will check for the existence of fins.</param>
	/// <param name="searchRow">
	/// Indicates whether the searcher searches for fishes in the direction of rows.
	/// </param>
	/// <returns>The first found step.</returns>
	[InterceptorMethodCaller]
	private unsafe void Collect(
		List<FishStep> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		int size,
		House** r,
		House** c,
		bool withFin,
		bool searchRow
	)
	{
		// Iterate on each digit.
		for (var digit = 0; digit < 9; digit++)
		{
			// Check the validity of the distribution for the current digit.
			var pBase = searchRow ? r[digit] : c[digit];
			var pCover = searchRow ? c[digit] : r[digit];
			if (pBase == null || pBase[0] <= size)
			{
				continue;
			}

			// Iterate on the base set combination.
			var baseSetsSpan = new ReadOnlySpan<House>(pBase, 10);
			var coverSetsSpan = new ReadOnlySpan<House>(pCover, 10);
			foreach (var bs in baseSetsSpan[1..(baseSetsSpan.IndexOf(0) is var i and not -1 ? i : ^0)].GetSubsets(size))
			{
				// 'baseLine' is the map that contains all base set cells.
				var baseLine = size switch
				{
					2 => CandidatesMap[digit] & (HousesMap[bs[0]] | HousesMap[bs[1]]),
					3 => CandidatesMap[digit] & (HousesMap[bs[0]] | HousesMap[bs[1]] | HousesMap[bs[2]]),
					4 => CandidatesMap[digit] & (HousesMap[bs[0]] | HousesMap[bs[1]] | HousesMap[bs[2]] | HousesMap[bs[3]])
				};

				// Iterate on the cover set combination.
				foreach (var cs in coverSetsSpan[1..(coverSetsSpan.IndexOf(0) is var j and not -1 ? j : ^0)].GetSubsets(size))
				{
					// 'coverLine' is the map that contains all cover set cells.
					var coverLine = size switch
					{
						2 => CandidatesMap[digit] & (HousesMap[cs[0]] | HousesMap[cs[1]]),
						3 => CandidatesMap[digit] & (HousesMap[cs[0]] | HousesMap[cs[1]] | HousesMap[cs[2]]),
						4 => CandidatesMap[digit] & (HousesMap[cs[0]] | HousesMap[cs[1]] | HousesMap[cs[2]] | HousesMap[cs[3]])
					};

					// Now check the fins and the elimination cells.
					CellMap elimMap, fins = [];
					if (!withFin)
					{
						// If the current searcher doesn't check fins, we'll just get the pure check:
						//   1) Base set contain more cells than cover sets.
						//   2) Elimination cells set isn't empty.
						if (baseLine & ~coverLine)
						{
							continue;
						}

						elimMap = coverLine & ~baseLine;
						if (!elimMap)
						{
							continue;
						}
					}
					else // Should check fins.
					{
						// All fins should be in the same block.
						var blockMask = (fins = baseLine & ~coverLine).BlockMask;
						if (!fins || !BitOperations.IsPow2(blockMask))
						{
							continue;
						}

						// Cover set shouldn't overlap with the block of all fins lying in.
						var finBlock = BitOperations.TrailingZeroCount(blockMask);
						if (!(coverLine & HousesMap[finBlock]))
						{
							continue;
						}

						// Don't intersect.
						if (!(HousesMap[finBlock] & coverLine & ~baseLine))
						{
							continue;
						}

						// Finally, get the elimination cells.
						elimMap = coverLine & ~baseLine & HousesMap[finBlock];
					}

					if (DisableFinnedOrSashimiXWing && size == 2 && !!fins)
					{
						// We should disallow collecting Finned X-Wing and Sashimi X-Wings
						// when option 'DisableFinnedOrSashimiXWing' is configured.
						continue;
					}

					accumulator.Add(
						new NormalFishStep(
							(from cell in elimMap select new Conclusion(Elimination, cell, digit)).ToArray(),
							[
								[
									..
									from cell in withFin ? baseLine & ~fins : baseLine
									select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + digit),
									.. withFin
										? from cell in fins select new CandidateViewNode(ColorIdentifier.Exofin, cell * 9 + digit)
										: [],
									.. from baseSet in bs select new HouseViewNode(ColorIdentifier.Normal, baseSet),
									.. from coverSet in cs select new HouseViewNode(ColorIdentifier.Auxiliary2, coverSet),
								],
								GetDirectView(digit, bs, cs, fins, searchRow)
							],
							context.Options,
							digit,
							HouseMask.Create(bs),
							HouseMask.Create(cs),
							fins,
							Sashimi.IsSashimi(grid, bs, fins, digit)
						)
					);
				}
			}
		}
	}

	/// <summary>
	/// Get the direct fish view with the specified grid and the base sets.
	/// </summary>
	/// <param name="digit">The digit.</param>
	/// <param name="baseSets">The base sets.</param>
	/// <param name="coverSets">The cover sets.</param>
	/// <param name="fins">The cells of the fin in the current fish.</param>
	/// <param name="searchRow">Indicates whether the current searcher searches row.</param>
	/// <returns>The view.</returns>
	private static View GetDirectView(Digit digit, House[] baseSets, House[] coverSets, in CellMap fins, bool searchRow)
	{
		var cellOffsets = new List<CellViewNode>();
		foreach (var baseSet in baseSets)
		{
			foreach (var cell in HousesMap[baseSet])
			{
				switch (CandidatesMap[digit].Contains(cell))
				{
					case true when fins.Contains(cell):
					{
						cellOffsets.Add(new(ColorIdentifier.Auxiliary1, cell));
						break;
					}
					default:
					{
						var flag = false;
						foreach (var c in ValuesMap[digit])
						{
							if (HousesMap[c.ToHouse(searchRow ? HouseType.Column : HouseType.Row)].Contains(cell))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							continue;
						}

						var (baseMap, coverMap) = (CellMap.Empty, CellMap.Empty);
						foreach (var b in baseSets)
						{
							baseMap |= HousesMap[b];
						}
						foreach (var c in coverSets)
						{
							coverMap |= HousesMap[c];
						}
						baseMap &= coverMap;
						if (baseMap.Contains(cell))
						{
							continue;
						}

						cellOffsets.Add(new(ColorIdentifier.Normal, cell));
						break;
					}
				}
			}
		}

		return [
			.. cellOffsets,
			.. from cell in ValuesMap[digit] select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
			.. fins ? from cell in fins select new CandidateViewNode(ColorIdentifier.Exofin, cell * 9 + digit) : []
		];
	}
}
