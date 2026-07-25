#undef DRAW_TRUTH_SPACES
#define DRAW_LINK_SPACES
#define USE_DIFFERENT_COLOR_FOR_CELL_VIEW_NODES

namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Multifish</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Multifish</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_MultifishStepSearcher", Technique.Multifish)]
public sealed partial class MultifishStepSearcher : StepSearcher
{
	/// <summary>
	/// Represents block table, grouped by chute.
	/// </summary>
	private static readonly House[][] ChuteBlocks = [[0, 1, 2], [3, 4, 5], [6, 7, 8], [0, 3, 6], [1, 4, 7], [2, 5, 8]];


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		var tempAccumulator = new List<MultifishStep>();
		if (CollectCore(in context, tempAccumulator) is { } step)
		{
			return step;
		}

		if (!context.OnlyFindOne && tempAccumulator.Count != 0)
		{
			context.Accumulator.AddRange(
				from s in tempAccumulator.AsSpan()
				orderby s.Truths.Count
				select s into s
				select s
			);
		}
		return null;
	}

	/// <summary>
	/// The core method to find multifishes.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="accumulator">The accumulator.</param>
	/// <returns>The step found.</returns>
	private MultifishStep? CollectCore(ref readonly StepAnalysisContext context, List<MultifishStep> accumulator)
	{
		ref readonly var grid = ref context.Grid;

		var notSolved = new Mask[27];
		foreach (var cell in EmptyCells)
		{
			var mask = grid.GetCandidates(cell);
			notSolved[cell.GetHouse(HouseType.Block)] |= mask;
			notSolved[cell.GetHouse(HouseType.Row)] |= mask;
			notSolved[cell.GetHouse(HouseType.Column)] |= mask;
		}

		// Create some buffered arrays.
		var patternCellsGroupedByDigit = (stackalloc CellMap[9]);
		var rct = (stackalloc Mask[27]);
		var tcl = (stackalloc Mask[27]);
		var tbl = (stackalloc Mask[27]);
		var rcl = (stackalloc Mask[27]);
		var removedHouseTable = (stackalloc House[27]);
		var isWorthTable = (stackalloc bool[3]);
		var boxLk = (stackalloc Mask[3]);
		var bbl = (stackalloc Mask[3]);
		var minInLine = (stackalloc int[3]);

		// Iterate digit combinations via bit enumerator.
		for (var digitSize = 2; digitSize <= 4; digitSize++)
		{
			// Iterate each digit combination of length 'digitSize'.
			foreach (var d in Digits & digitSize)
			{
				var digitsMask = Mask.Create(d);

				// Iterate on houses offsets, also via bit enumerator.
				for (var houseOffsetsSize = digitSize == 2 ? 3 : 2; houseOffsetsSize <= 5; houseOffsetsSize++)
				{
					// Here variable is a 9-bit integer, with some bits set 1 of length 'houseOffsetsSize' exactly.
					// The variable will be used for house checking (via operations << 9 and << 18).
					foreach (var h in Digits & houseOffsetsSize)
					{
						// Iterate main line type. The multifish "bones" should be either in rows or in columns.
					loop_isRow:
						foreach (var isRow in (true, false))
						{
							var chosenHouseOffsets = Mask.Create(h);

							// Skip cases when the chosen houses only uses 3 lines in a same chute.
							if ((chosenHouseOffsets & ~7) == 0 || (chosenHouseOffsets & ~56) == 0 || (chosenHouseOffsets & ~448) == 0)
							{
								continue;
							}

							var housesMask = chosenHouseOffsets << (isRow ? 9 : 18);

							// Iterate on each house.
							var patternCells = CellMap.Empty;
							patternCellsGroupedByDigit.Clear();
							rct.Clear();
							var availableDigitsMask = (Mask)0;
							foreach (var house in housesMask)
							{
								ref var rcTruthsCurrentHouse = ref rct[house];
								foreach (var digit in digitsMask)
								{
									var cells = HousesMap[house] & CandidatesMap[digit];
									if (cells.Count < 2)
									{
										// Potential hidden single found. But in this technique we don't handle with that.
										continue;
									}

									rcTruthsCurrentHouse |= (Mask)(1 << digit);
									patternCellsGroupedByDigit[digit] |= cells;
									patternCells |= cells;
								}

								if (BitOperations.PopCount((uint)rcTruthsCurrentHouse) < 2)
								{
									// We cannot choose such houses with specified line type.
									continue loop_isRow;
								}

								availableDigitsMask |= rcTruthsCurrentHouse;
							}
							if (availableDigitsMask != digitsMask)
							{
								// Not all available digits can be used in chosen houses.
								continue;
							}

							// Here we know that the houses can be treated as valid truths. Now we should find for links.

							// Iterate on each pattern cell, to collect all possible links that can be iterated.
							tcl.Clear();
							tbl.Clear();
							foreach (var cell in patternCells)
							{
								var block = cell.GetHouse(HouseType.Block);
								var line = cell.GetHouse(isRow ? HouseType.Column : HouseType.Row);
								var candidates = (Mask)(grid.GetCandidates(cell) & digitsMask);
								tbl[line] = tcl[line] |= candidates;
								tbl[block] = tcl[block] |= candidates;
							}

							removedHouseTable.Clear();
							isWorthTable.Clear();
							// Iterate on each house.
							for (var (house, i) = (isRow ? 18 : 9, 0); i < 3; house += 3, i++)
							{
								// Let's suppose we should choose line links. Block links can be adjusted later.
								var (llk, blk) = (0, 0);

								// Define a local counter to sum up usage cases on 3 bands or towers having any house-digit truths.
								// This value can be used for rank-balancing operations.
								var localCounter = (tcl[house] != 0 ? 1 : 0)
									+ (tcl[house + 1] != 0 ? 1 : 0)
									+ (tcl[house + 2] != 0 ? 1 : 0);

								// House type index: 9..18 => 0..3, 18..27 => 3..6.
								var targetChuteIndex = (house - 9) / 3;
								switch (localCounter)
								{
									// There'll be only one line link. We don't consider any conversions to block links.
									case 1:
									{
										// Set cached truths in blocks to 0 (ignore block links to be calculated).
										tcl[ChuteBlocks[targetChuteIndex][0]] = 0;
										tcl[ChuteBlocks[targetChuteIndex][1]] = 0;
										tcl[ChuteBlocks[targetChuteIndex][2]] = 0;
										break;
									}

									// There'll be 2 line links. We should measure block links here.
									case 2:
									{
										for (var line = house; line < house + 3; line++)
										{
											if (tcl[line] == 0)
											{
												continue;
											}

											var cells = patternCells & HousesMap[line];
											llk += Math.Min(BitOperations.PopCount((uint)tcl[line]), cells.Count);
										}
										foreach (var block in ChuteBlocks[targetChuteIndex])
										{
											if (tcl[block] == 0)
											{
												continue;
											}

											var cells = patternCells & HousesMap[block];
											blk += Math.Min(BitOperations.PopCount((uint)tcl[block]), cells.Count);
										}
										if (blk < llk)
										{
											tcl[house] = 0;
											tcl[house + 1] = 0;
											tcl[house + 2] = 0;
										}
										else
										{
											// Worth to adjust.
											if (blk == llk && blk != 0)
											{
												isWorthTable[house % 9 / 3] = true;
											}

											tcl[ChuteBlocks[targetChuteIndex][0]] = 0;
											tcl[ChuteBlocks[targetChuteIndex][1]] = 0;
											tcl[ChuteBlocks[targetChuteIndex][2]] = 0;
										}

										removedHouseTable[ChuteBlocks[targetChuteIndex][0]] = -1;
										removedHouseTable[ChuteBlocks[targetChuteIndex][1]] = -1;
										removedHouseTable[ChuteBlocks[targetChuteIndex][2]] = -1;
										break;
									}

									// There'll be 3 line links.
									case 3:
									{
										minInLine.Clear();
										for (var offset = 0; offset < 3; offset++)
										{
											var currentHouse = house + offset;
											var cells = patternCells & HousesMap[currentHouse];
											llk += minInLine[offset] = Math.Min(BitOperations.PopCount((uint)tcl[currentHouse]), cells.Count);
										}

										var flag = false;
										foreach (var block in ChuteBlocks[targetChuteIndex])
										{
											if (tcl[block] == 0)
											{
												continue;
											}

											var cells = patternCells & HousesMap[block];
											if (cells.Count >= digitSize)
											{
												flag = true;
											}

											blk += Math.Min(BitOperations.PopCount((uint)tcl[block]), cells.Count);
										}

										var (state, count) = blk < llk || blk == llk && flag ? (4, blk) : (0, llk);
										var temp = 0;
										boxLk.Clear();
										bbl.Clear();
										for (var offset1 = 0; offset1 < 2; offset1++)
										{
											for (var offset2 = offset1 + 1; offset2 < 3; offset2++)
											{
												temp++;
												var deletedCount = 0;
												var cells = patternCells & (HousesMap[house + offset1] | HousesMap[house + offset2]);
												for (var offset3 = 0; offset3 < 3; offset3++)
												{
													var t = cells & HousesMap[ChuteBlocks[targetChuteIndex][offset3]];
													boxLk[offset3] = 0;

													if (!t)
													{
														continue;
													}

													foreach (var cell in t)
													{
														boxLk[offset3] |= (Mask)(grid.GetCandidates(cell) & digitsMask);
													}
													deletedCount += Math.Min(BitOperations.PopCount((uint)boxLk[offset3]), t.Count);
												}
												if (deletedCount + minInLine[3 - offset1 - offset2] is var p && p < count)
												{
													state = temp;
													count = deletedCount + p;
													boxLk.CopyTo(bbl);
												}
											}
										}

										switch (state)
										{
											case 0:
											{
												tcl[ChuteBlocks[targetChuteIndex][0]] = 0;
												tcl[ChuteBlocks[targetChuteIndex][1]] = 0;
												tcl[ChuteBlocks[targetChuteIndex][2]] = 0;
												if (blk == llk && blk != 0)
												{
													isWorthTable[house % 9 / 3] = true;
												}
												break;
											}
											case 4:
											{
												tcl[house] = 0;
												tcl[house + 1] = 0;
												tcl[house + 2] = 0;
												removedHouseTable[ChuteBlocks[targetChuteIndex][0]] = -1;
												removedHouseTable[ChuteBlocks[targetChuteIndex][1]] = -1;
												removedHouseTable[ChuteBlocks[targetChuteIndex][2]] = -1;
												break;
											}
											default:
											{
												// Performs an adjustment to block link.
												var chuteLinesArray = (int[][])[[], [0, 1], [0, 2], [1, 2]];
												tcl[house + chuteLinesArray[state][0]] = 0;
												tcl[house + chuteLinesArray[state][1]] = 0;
												tcl[ChuteBlocks[targetChuteIndex][0]] = bbl[0];
												tcl[ChuteBlocks[targetChuteIndex][1]] = bbl[1];
												tcl[ChuteBlocks[targetChuteIndex][2]] = bbl[2];
												removedHouseTable[ChuteBlocks[targetChuteIndex][0]] = 3 - state + house;
												removedHouseTable[ChuteBlocks[targetChuteIndex][1]] = 3 - state + house;
												removedHouseTable[ChuteBlocks[targetChuteIndex][2]] = 3 - state + house;
												break;
											}
										}
										break;
									}
								}
							}

							var other = CellMap.Empty;

							// According to previous judgement of line links, we should here find a best rank of link combinations,
							// which is an optimization of a single house
							// (choosing four kinds of links in order to make rank to be a minimum value).
							rcl.Clear();
							var cellLinks = CellMap.Empty;
							for (var house = 0; house < 27; house++)
							{
								if (tcl[house] == 0)
								{
									continue;
								}

								ChooseLink(
									grid,
									patternCells,
									ref other,
									ref cellLinks,
									rct,
									rcl,
									tcl,
									house,
									notSolved,
									removedHouseTable
								);
							}

							var truthsCount = other.Count;
							var linksCount = cellLinks.Count;
							foreach (var mask in rct)
							{
								truthsCount += BitOperations.PopCount((uint)mask);
							}
							foreach (var mask in rcl)
							{
								linksCount += BitOperations.PopCount((uint)mask);
							}

							// Try to merge cell links into block links in order to make pattern valid.
							// Every cell links here only holds one digit of multifish pattern,
							// and all cells covered by line links are inside one block, so we can try this.
							if (linksCount > truthsCount && linksCount < truthsCount + 3)
							{
								// Adjust links phase 1.
								foreach (var cell in cellLinks)
								{
									var linkHouse = cell.GetHouse(isRow ? HouseType.Column : HouseType.Row);
									var candidates = (Mask)((Mask)(grid.GetCandidates(cell) & digitsMask) | rct[linkHouse]);
									if (BitOperations.PopCount((uint)candidates) != 1)
									{
										continue;
									}

									var (first, second) = (linkHouse % 3) switch
									{
										0 => (linkHouse + 1, linkHouse + 2),
										1 => (linkHouse - 1, linkHouse + 1),
										_ => (linkHouse - 2, linkHouse - 1)
									};
									for (var offset = 0; offset < 2; offset++)
									{
										var house = offset == 0 ? first : second;
										if ((rcl[house] & candidates) == 0)
										{
											continue;
										}

										var tempCells = CandidatesMap[BitOperations.Log2((uint)candidates)]
											& HousesMap[house]
											& (patternCells | other);
										if (tempCells.PeerIntersection.Contains(cell))
										{
											// This cell can be cleared, to form a block link.
											rcl[cell.GetHouse(HouseType.Block)] |= candidates;
											rcl[house] &= (Mask)~candidates;
											cellLinks -= cell;
											linksCount--;
											break;
										}
									}
								}

								// Adjust links phase 2.
								if (linksCount > truthsCount)
								{
									// Add block truths.
									var blockMask = cellLinks.BlockMask;
								loop_block:
									foreach (var block in blockMask)
									{
										var chuteRow = block / 3 * 3 + 9;
										var chuteColumn = block % 3 * 3 + 18;
#pragma warning disable CS0675
										// Calculate truths in chute row (band) and chute column (tower) and its containing block,
										// in order to avoid truth triplets.
										var candidatesMask = (Mask)(
											rct[chuteRow]
												| rct[chuteRow + 1]
												| rct[chuteRow + 2]
												| rct[chuteColumn]
												| rct[chuteColumn + 1]
												| rct[chuteColumn + 2]
												| rct[block]
										);
#pragma warning restore CS0675
										candidatesMask = (Mask)(notSolved[block] & ~candidatesMask);
										if (candidatesMask == 0)
										{
											continue;
										}

										var candidates = candidatesMask.AllSets;
										foreach (var digitCombination in candidates | candidates.Length)
										{
											var cells = CellMap.Empty;
											foreach (var digit in digitCombination)
											{
												cells |= CandidatesMap[digit];
											}
											cells &= HousesMap[block] & ~cellLinks;
											if (cells.Count >= digitCombination.Length)
											{
												continue;
											}

											// The new block link is found here.
											truthsCount += digitCombination.Length;
											linksCount += cells.Count;
											cellLinks |= cells;
											rct[block] |= Mask.Create(digitCombination);
											if (linksCount == truthsCount)
											{
												break loop_block;
											}
											else
											{
												continue loop_block;
											}
										}
									}
								}

								// Adjust links phase 3.
								if (linksCount > truthsCount)
								{
									// Rebalance rank by using adjustment to block links.
									var rebalanced = false;
									for (var (house, i) = (isRow ? 18 : 9, 0); i < 3; house += 3, i++)
									{
										var targetChuteIndex = (house - 9) / 3;
										if (!isWorthTable[house % 9 / 3]
											|| rct[HousesCells[house][0].GetHouse(HouseType.Block)] != 0
											|| rct[HousesCells[house][3].GetHouse(HouseType.Block)] != 0
											|| rct[HousesCells[house][6].GetHouse(HouseType.Block)] != 0)
										{
											continue;
										}

										var cells = (HousesMap[house] | HousesMap[house + 1] | HousesMap[house + 2]) & other;
										rebalanced = true;

										tcl[house] = 0;
										tcl[house + 1] = 0;
										tcl[house + 2] = 0;
										rcl[house] = 0;
										rcl[house + 1] = 0;
										rcl[house + 2] = 0;
										rct[house] = 0;
										rct[house + 1] = 0;
										rct[house + 2] = 0;
										tcl[ChuteBlocks[targetChuteIndex][0]] = tbl[ChuteBlocks[targetChuteIndex][0]];
										tcl[ChuteBlocks[targetChuteIndex][1]] = tbl[ChuteBlocks[targetChuteIndex][1]];
										tcl[ChuteBlocks[targetChuteIndex][2]] = tbl[ChuteBlocks[targetChuteIndex][2]];

										cellLinks &= ~(HousesMap[house] | HousesMap[house + 1] | HousesMap[house + 2]);
										other &= ~cells;
										removedHouseTable[ChuteBlocks[targetChuteIndex][0]] = -1;
										removedHouseTable[ChuteBlocks[targetChuteIndex][1]] = -1;
										removedHouseTable[ChuteBlocks[targetChuteIndex][2]] = -1;
										for (var j = 0; j < 3; j++)
										{
											ChooseLink(
												grid,
												patternCells,
												ref other,
												ref cellLinks,
												rct,
												rcl,
												tcl,
												ChuteBlocks[targetChuteIndex][j],
												notSolved,
												removedHouseTable
											);
										}
									}
									if (rebalanced)
									{
										truthsCount = other.Count;
										linksCount = cellLinks.Count;
										foreach (var mask in rct)
										{
											truthsCount += BitOperations.PopCount((uint)mask);
										}
										foreach (var mask in rcl)
										{
											linksCount += BitOperations.PopCount((uint)mask);
										}
									}
								}
							}

							// I have run out of thoughts to rebalance rank. Now check rank.
							if (linksCount != truthsCount)
							{
								continue;
							}

							var conclusions = new List<Conclusion>();

							// Elimination phase - cell links.
							foreach (var cell in cellLinks)
							{
								var line = cell.GetHouse(isRow ? HouseType.Column : HouseType.Row);
								var block = cell.GetHouse(HouseType.Block);
								foreach (var digit in
#pragma warning disable CS0675
									(Mask)(grid.GetCandidates(cell) & ~(rct[block] | digitsMask | rct[line])))
#pragma warning restore CS0675
								{
									conclusions.Add(new(Elimination, cell, digit));
								}
							}

							// Elimination phase - cannibalism on cell links.
							foreach (var cell in cellLinks)
							{
								foreach (var digit in (Mask)(grid.GetCandidates(cell) & rcl[cell.GetHouse(HouseType.Block)]))
								{
									conclusions.Add(new(Elimination, cell, digit));
								}
							}

							// Elimination phase - house-digit links.
							for (var house = 0; house < 27; house++)
							{
								if (rcl[house] == 0)
								{
									continue;
								}

								foreach (var cell in HousesMap[house] & EmptyCells & ~(other | patternCells))
								{
									foreach (var digit in (Mask)(grid.GetCandidates(cell) & rcl[house]))
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}

								foreach (var cell in HousesMap[house] & EmptyCells & ~(other | cellLinks))
								{
									foreach (var digit in (Mask)(grid.GetCandidates(cell) & tcl[house] & ~digitsMask))
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}

								if ((HousesMap[house] & (patternCells | other) & ~cellLinks) is var rebalancedLinkCells
									&& rebalancedLinkCells.Count == BitOperations.PopCount((uint)rcl[house]))
								{
									foreach (var cell in rebalancedLinkCells)
									{
										foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~tcl[house]))
										{
											conclusions.Add(new(Elimination, cell, digit));
										}
									}
								}

								foreach (var digit in rcl[house])
								{
									foreach (var cell in (
										(patternCellsGroupedByDigit[digit] | other)
											& HousesMap[house]
											& CandidatesMap[digit]
									).PeerIntersection & CandidatesMap[digit] & ~HousesMap[house])
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}
							}

							foreach (var cell in patternCells)
							{
								foreach (var digit in (Mask)(
									grid.GetCandidates(cell)
										& rcl[cell.GetHouse(isRow ? HouseType.Column : HouseType.Row)]
										& rcl[cell.GetHouse(HouseType.Block)]
								))
								{
									conclusions.Add(new(Elimination, cell, digit));
								}
							}

							for (var house = 0; house < 27; house++)
							{
								if (rcl[house] == 0)
								{
									continue;
								}

								foreach (var cell in cellLinks & HousesMap[house])
								{
									foreach (var digit in (Mask)(grid.GetCandidates(cell) & rcl[house]))
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}
							}

							if (conclusions.Count == 0)
							{
								continue;
							}

							var candidateOffsets = new List<CandidateViewNode>();
							var cellOffsets = new List<CellViewNode>();
#if DRAW_TRUTH_SPACES || DRAW_LINK_SPACES
							var houseOffsets = new List<HouseViewNode>();
#endif
							var (truths, links) = (SpaceSet.Empty, SpaceSet.Empty);

							// Collect for cell truths & candidate view nodes.
							foreach (var cell in other)
							{
								truths += Space.RowColumn(cell / 9, cell % 9);
								foreach (var digit in grid.GetCandidates(cell))
								{
									candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary3, cell * 9 + digit));
								}
							}

							// Collect for house-digit truths & candidate view nodes.
							for (var house = 0; house < 27; house++)
							{
								if (rct[house] == 0)
								{
									continue;
								}

								var houseColorIdentifier = house switch
								{
									< 9 => ColorDescriptorAlias.Auxiliary2,
									< 18 => ColorDescriptorAlias.Normal,
									_ => ColorDescriptorAlias.Auxiliary1
								};
								foreach (var digit in rct[house])
								{
									truths += house switch
									{
										< 9 => Space.BlockDigit(house, digit),
										< 18 => Space.RowDigit(house - 9, digit),
										_ => Space.ColumnDigit(house - 18, digit)
									};
								}
								var cells = CellMap.Empty;
								foreach (var digit in rct[house])
								{
									cells |= CandidatesMap[digit];
								}
								foreach (var cell in cells & HousesMap[house])
								{
									foreach (var digit in (Mask)(grid.GetCandidates(cell) & rct[house]))
									{
										candidateOffsets.Add(new(houseColorIdentifier, cell * 9 + digit));
									}
								}
							}

							// Collect for cell links.
							foreach (var cell in cellLinks)
							{
								links += Space.RowColumn(cell / 9, cell % 9);
							}
							for (var house = 0; house < 27; house++)
							{
								foreach (var digit in rcl[house])
								{
									links += house switch
									{
										< 9 => Space.BlockDigit(house, digit),
										< 18 => Space.RowDigit(house - 9, digit),
										_ => Space.ColumnDigit(house - 18, digit)
									};
								}
							}

							// Check whether all candidates from truths are covered by links.
							// This fixes GitHub #799:
							//   https://github.com/kyoyama-kazusa/Sudoku/issues/799
							// Counterexample:
							//   .....735...2.5..49.....9..81..63....+28..+7..3.....24..73..5...+7.74..9.6.+5.257....+3:611 811 413 114 814 415 322 126 826 631 132 632 133 433 633 134 334 435 662 962 175 284 186 895
							var candidatesToCheck = CandidateMap.Empty;
							foreach (var truth in truths)
							{
								candidatesToCheck |= truth.GetAvailableRange(grid);
							}
							foreach (var link in links)
							{
								candidatesToCheck &= ~link.GetAvailableRange(grid);
							}
							if (candidatesToCheck)
							{
								// Not all candidates are covered.
								continue;
							}

#if DRAW_TRUTH_SPACES
							foreach (var truth in truths)
							{
								switch (truth)
								{
									case { House: var house and not -1 }:
									{
										houseOffsets.Add(new(ColorIdentifierAlias.Normal, house));
										break;
									}
									case { Cell: var cell }:
									{
										cellOffsets.Add(
											new(
#if USE_DIFFERENT_COLOR_FOR_CELL_VIEW_NODES
												ColorIdentifierAlias.Auxiliary3,
#else
												ColorIdentifierAlias.Auxiliary2,
#endif
												cell
											)
										);
										break;
									}
								}
							}
#endif
#if DRAW_LINK_SPACES
							foreach (var link in links)
							{
								switch (link)
								{
									case { House: var house and not -1 }:
									{
										houseOffsets.Add(new(ColorDescriptorAlias.Auxiliary2, house));
										break;
									}
									case { Cell: var cell }:
									{
										cellOffsets.Add(
											new(
#if USE_DIFFERENT_COLOR_FOR_CELL_VIEW_NODES
												ColorDescriptorAlias.Auxiliary3,
#else
												ColorIdentifierAlias.Auxiliary2,
#endif
												cell
											)
										);
										break;
									}
								}
							}
#endif

							// Add step to the target collection or just return if only-one mode is enabled.
							var step = new MultifishStep(
								conclusions.AsMemory(),
								[
									[
										.. candidateOffsets,
										.. cellOffsets,
#if DRAW_TRUTH_SPACES || DRAW_LINK_SPACES
										.. houseOffsets
#endif
									]
								],
								context.Options,
								truths,
								links
							);
							if (context.OnlyFindOne)
							{
								return step;
							}

							accumulator.Add(step);
						}
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// To convert links in order to balance rank value, in order to make rank equal to 0.
	/// </summary>
	/// <param name="grid">The target grid.</param>
	/// <param name="patternCells">The pattern cells.</param>
	/// <param name="other">The cell truths.</param>
	/// <param name="cellLinks">The cell links.</param>
	/// <param name="rct">The house-digit truths.</param>
	/// <param name="rcl">The house-digit links.</param>
	/// <param name="tcl">The house-digit truths.</param>
	/// <param name="house">The house.</param>
	/// <param name="notSolved">Unsolved candidates table.</param>
	/// <param name="removedHouses">Removed houses.</param>
	private void ChooseLink(
		in Grid grid,
		in CellMap patternCells,
		ref CellMap other,
		ref CellMap cellLinks,
		Span<Mask> rct,
		Span<Mask> rcl,
		ReadOnlySpan<Mask> tcl,
		House house,
		ReadOnlySpan<Mask> notSolved,
		ReadOnlySpan<House> removedHouses
	)
	{
		var cells = patternCells & HousesMap[house];
		if (house < 9 && removedHouses[house] is var removedHouse and not -1)
		{
			// Previous cases on adjustment from line links to block links in case 2 and 3.
			cells &= ~HousesMap[removedHouse];
		}

		var possibleTruthTripletDigitsMask = (Mask)0;
		if (house < 9)
		{
			foreach (var cell in HousesMap[house] & EmptyCells)
			{
				possibleTruthTripletDigitsMask |= (Mask)(
					grid.GetCandidates(cell) & (rct[cell.GetHouse(HouseType.Row)] | rct[cell.GetHouse(HouseType.Column)])
				);
			}
		}

		var reduction = 0;
		var addRct = (Mask)0;
		var addedCellLink = CellMap.Empty;
		if (cells.Count <= BitOperations.PopCount((uint)tcl[house]))
		{
			// Add line truths.
			var candidatesMask = (Mask)(notSolved[house] & ~(tcl[house] | possibleTruthTripletDigitsMask));
			var candidates = candidatesMask.AllSets;
			foreach (var candidateCombination in candidates | candidates.Length)
			{
				var candidateCombinationMask = Mask.Create(candidateCombination);
				var tempCells = CellMap.Empty;
				foreach (var candidate in candidateCombination)
				{
					tempCells |= CandidatesMap[candidate];
				}
				tempCells &= HousesMap[house] & ~cells;

				if (candidateCombination.Length - tempCells.Count is var p && p > reduction)
				{
					reduction = p;
					addRct = candidateCombinationMask;
					addedCellLink = tempCells;
				}
			}
			if (reduction > 0)
			{
				rct[house] |= addRct;
				cellLinks |= addedCellLink | cells;
				return;
			}

			if (cells.Count < BitOperations.PopCount((uint)tcl[house]))
			{
				cellLinks |= cells;
			}
		}

		var isAnyCellsAdded = false;
		if (cells.Count >= BitOperations.PopCount((uint)tcl[house]))
		{
			// Add cell truths.
			var tempCells = HousesMap[house] & EmptyCells & ~cells;
			if (tempCells.Count < 2)
			{
				rcl[house] |= tcl[house];
				return;
			}

			foreach (var cell in tempCells)
			{
				if ((grid.GetCandidates(cell) & ~tcl[house]) == 0)
				{
					other += cell;
					isAnyCellsAdded = true;
				}
			}
			if (isAnyCellsAdded)
			{
				rcl[house] |= tcl[house];
				return;
			}

			for (var cellSize = 2; cellSize <= tempCells.Count - 1; cellSize++)
			{
				foreach (ref readonly var cellCombination in tempCells & cellSize)
				{
					var candidates = (Mask)0;
					foreach (var cell in cellCombination)
					{
						candidates |= grid.GetCandidates(cell);
					}
					candidates &= (Mask)~tcl[house];

					if (BitOperations.PopCount((uint)candidates) < cellSize)
					{
						other |= cellCombination;
						rcl[house] |= (Mask)(candidates | tcl[house]);
						return;
					}
				}
			}

			if (cells.Count > BitOperations.PopCount((uint)tcl[house]))
			{
				rcl[house] |= tcl[house];
			}
		}

		if (cells.Count == BitOperations.PopCount((uint)tcl[house]))
		{
			rcl[house] |= tcl[house];
		}
	}
}
