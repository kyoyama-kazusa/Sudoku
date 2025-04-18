namespace Sudoku.Analytics.StepSearchers.Uniqueness;

public partial class UniqueRectangleStepSearcher
{
	/// <summary>
	/// Check burred subset.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>
	/// The pattern:
	/// <code><![CDATA[
	///  ↓ corner1, corner2
	/// (abx) | (abcd) acd acd
	///  ab   |  ab
	/// ]]></code>
	/// will remove the digit <c>a</c> in the corner cell <c>abx</c>.
	/// </para>
	/// <para>
	/// The letter <c>x</c> may present multiple digits and not be used. However, if all digits <c>x</c> are false,
	/// the UR pattern will be degenerated into type 1, and the normal subset with digits <c>c</c> and <c>d</c> will be formed.
	/// </para>
	/// </remarks>
	private partial void CheckBurredSubset(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		// Deconstruct the other cells, determining whether they holds a same two digits.
		if (otherCellsMap is not [var c1, var c2]
			|| grid.GetCandidates(c1) is var digitsMask1 && grid.GetCandidates(c2) is var digitsMask2
			&& (digitsMask1 != comparer || digitsMask2 != comparer))
		{
			return;
		}

		// Check whether corner cells hold both digits.
		if (grid.GetCandidates(corner1) is var digitsMaskCorner1 && grid.GetCandidates(corner2) is var digitsMaskCorner2
			&& (Mask)(digitsMaskCorner1 & digitsMaskCorner2) != comparer)
		{
			return;
		}

		// Determine whether a corner holds both and only two digits.
		if (digitsMaskCorner1 == comparer || digitsMaskCorner2 == comparer)
		{
			return;
		}

		// Iterate on each combination of corner cells, determining the final cell as conclusion producer.
		foreach (var house in (corner1.AsCellMap() + corner2).SharedHouses)
		{
			var extraCells = (HousesMap[house] & EmptyCells) - corner1 - corner2;

			// Here 'size' can be started at 1.
			// Example:
			// .....+6..9.369+4..5..5.27..6..48..3.....5..7......89+4+5.6+5....+2.47.67+4.+982+52.+4+7.56..:112 812 114 115 815 217 317 318 131 252 155 359 162
			for (var size = 1; size <= extraCells.Count - 1; size++)
			{
				foreach (var (thisCorner, elimCorner) in ((corner1, corner2), (corner2, corner1)))
				{
					var otherDigits = (Mask)(grid.GetCandidates(thisCorner) & ~comparer);
					if (otherDigits == 0)
					{
						// There is not necessary to determine the pattern because 3 of 4 cells are only two digits.
						return;
					}

					foreach (ref readonly var subsetCells in extraCells & size)
					{
						// Determine whether the 'size' cells contain 'size + 1' digits.
						var subsetDigitsMask = grid[subsetCells];
						if (BitOperations.PopCount(subsetDigitsMask) != size + 1)
						{
							continue;
						}

						// The burred subset must contain 1 digit that is UR digit.
						var onlyDigitMask = (Mask)(subsetDigitsMask & comparer);
						if (!Mask.IsPow2(onlyDigitMask))
						{
							continue;
						}

						var elimDigit = Mask.Log2(onlyDigitMask);

						// Check whether the extra cells holds all possible digits appeared in 'thisCorner',
						// with UR digits having been removed.
						if ((subsetDigitsMask & otherDigits) != otherDigits)
						{
							continue;
						}

						// Check whether the other side 'elimCorner' contains elimination digit.
						var elimDigitsMask = (Mask)(grid.GetCandidates(elimCorner) & subsetDigitsMask);
						if (elimDigitsMask == 0)
						{
							// No elimination exists.
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>();
						foreach (var cell in otherCellsMap)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
							}
						}
						foreach (var digit in grid.GetCandidates(thisCorner))
						{
							candidateOffsets.Add(
								new(
									(comparer >> digit & 1) != 0 ? ColorIdentifier.Normal : ColorIdentifier.Auxiliary1,
									thisCorner * 9 + digit
								)
							);
						}
						foreach (var digit in (Mask)(grid.GetCandidates(elimCorner) & comparer))
						{
							candidateOffsets.Add(new(ColorIdentifier.Normal, elimCorner * 9 + digit));
						}
						foreach (var cell in subsetCells)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
							}
						}

						accumulator.Add(
							new UniqueRectangleBurredSubsetStep(
								new SingletonArray<Conclusion>(new(Elimination, elimCorner, elimDigit)),
								[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, house)]],
								context.Options,
								d1,
								d2,
								urCells.AsCellMap(),
								index,
								subsetCells,
								thisCorner,
								subsetDigitsMask
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR-XY-Wing, UR-XYZ-Wing, UR-WXYZ-Wing and AR-XY-Wing, AR-XYZ-Wing and AR-WXYZ-Wing.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <param name="areCornerCellsAligned">Indicates whether the corner cells cannot see each other.</param>
	/// <remarks>
	/// <para>The structures:</para>
	/// <para>
	/// Subtype 1:
	/// <code><![CDATA[
	///   ↓ corner1
	/// (ab )  abxy  yz  xz
	/// (ab )  abxy  *
	///   ↑ corner2
	/// ]]></code>
	/// Note that the pair of cells <c>abxy</c> should be in the same house.
	/// </para>
	/// <para>
	/// Subtype 2:
	/// <code><![CDATA[
	///   ↓ corner1
	/// (ab )  abx   xz
	///  aby  (ab )  *   yz
	///         ↑ corner2
	/// ]]></code>
	/// </para>
	/// </remarks>
	private partial void CheckRegularWing(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index, bool areCornerCellsAligned)
	{
		// Test examples:
		// UR-XY-Wing
		// 00+60507+4050+34000060+7+40090+5+3+74190+5002+9+520+400706+3+8+72+159+4+18+7004005+30+5108400+409+530010:212 819 222 357 685
		// +103004+58004010030000+9+3200+1+470+40+1380000+140+29300+3500600100620+105+3+3100090084+5003+7100:712 719 821 223 525 725 825 831 632 642 855 659 861 767 268 787
		// +1+86009200+2306000104+7002+10060+4+816+70+250+57200+64+1+61+2000870+501000700+70+4030+10280+3+7+10060:927 537 538 364 972 874 476 876 479 979 496 997
		// 040700+8907+3+90080+2600200+9500+40+32+80009+908060200+250+9130803+2+10907+40090000+310004301+9+62:515 516 132 742 459 559 574 881 684 686 786
		// 4+1+350+890000703+40010800010+30+85+17+26+34+93001+59006+906+48+3100+260+3+1+7008+10580+270070+804+50+10:624 227 228 628 934 635 237 537 239 982
		// 100+506090070+91+300+20034000+1520+1+30+400904010+9200009260040002+6+4+18+5+7010035+9+2660009+2030:835 747 767 883 493
		//
		// UR-XYZ-Wing
		// +70005008+96000+790030904+8070027+5+9+64+8+3100651000000+93+200500076+903+2+8020+7300049+6+3+8+42000:412 116 422 136 171 187 587 188
		// ...9+2+56+8....+7+14.2+9+29.+683.+71..2+379.4.+9..8+62+7...7.45+19.+21.+923..5..2.5.......3+1.8+2..:311 413 841 842 547 487
		//
		// UR-WXYZ-Wing
		// +4.65+7.29+3..+3+4....6..+9.3684.1+92+64+357+8.+68+7..4...47+82.6.9.2196.+3..9.+5+3...+6.+634..79..:521 721 122 825 531 732 156 876 186 886 189 789
		// 100460700+70800300+6+3+6005+700201+60+75+3000003+106005+7+3+6492+1800+170+406060000190003008+6001:518 441 941 458 958 459 959 588 293

		// Firstly, we should check whether the 2 corner cells should contain both a and b, and only contain a and b.
		// This expression only uses candidates to check digits appearing, so it doesn't determine whether the pattern is a UR or not.
		// ARs can also be passed of course.
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		var otherCell1Mask = grid.GetCandidates(otherCellsMap[0]);
		var otherCell2Mask = grid.GetCandidates(otherCellsMap[1]);
		if ((otherCell1Mask & otherCell2Mask & comparer) == 0 || ((otherCell1Mask | otherCell2Mask) & comparer) != comparer)
		{
			return;
		}

		// Now we check for other 2 cells, collecting digits not being UR/AR digits.
		var otherDigitsMask = (Mask)((otherCell1Mask | otherCell2Mask) & ~comparer);

		// Merge incomplete and complete wing logic into one loop.
		// Here we don't know what digit will be selected as a pivot, so we should iterate all digits.
		// The last case -1 is for complete wing.
		var cells = urCells.AsCellMap();
		var pivotDigits = (Digit[])[.. otherDigitsMask.AllSets, -1];
		foreach (var pivotDigit in pivotDigits)
		{
			Cell[][] cellsGroups;
			if (pivotDigit == -1)
			{
				// No pivot digit should be checked. Due to no way to check intersection, we should delay the checking.
				(cellsGroups, var tempIndex) = (new Cell[BitOperations.PopCount(otherDigitsMask)][], 0);
				foreach (var lastDigit in otherDigitsMask)
				{
					cellsGroups[tempIndex++] = [.. cells % CandidatesMap[lastDigit] & BivalueCells];
				}
			}
			else
			{
				// Pivot digit is specified. We should check for cells that contain that digit.
				var lastDigitsMask = (Mask)(otherDigitsMask & ~(1 << pivotDigit));
				if (BitOperations.PopCount(lastDigitsMask) is 0 or 1)
				{
					continue;
				}

				(cellsGroups, var (tempIndex, atLeastOneGroupIsEmpty)) = (new Cell[BitOperations.PopCount(lastDigitsMask)][], (0, false));
				foreach (var lastDigit in lastDigitsMask)
				{
					ref var currentCellGroup = ref cellsGroups[tempIndex++];
					currentCellGroup = [.. cells % CandidatesMap[lastDigit] & CandidatesMap[pivotDigit] & BivalueCells];
					if (currentCellGroup.Length == 0)
					{
						atLeastOneGroupIsEmpty = true;
						break;
					}
				}
				if (atLeastOneGroupIsEmpty)
				{
					// If a cells group does not contain such cells, the current pivot digit will be considered invalid.
					// Now just skip for this case.
					continue;
				}
			}

			// Extract one element for each cells group.
			var finalPivotDigit = pivotDigit;
			foreach (var patternCombination in cellsGroups.GetExtractedCombinations())
			{
				var combination = patternCombination.AsCellMap();
				if (cellsGroups.Length != combination.Count)
				{
					// The selected items cannot duplicate with others.
					continue;
				}

				// Here we should check for pivot digit for case 'pivotDigit == -1'.
				if (pivotDigit == -1)
				{
					var mergedMask = grid[combination, false, MaskAggregator.And];
					if (!Mask.IsPow2(mergedMask))
					{
						// No pivot digit can be found, meaning no eliminations can be found.
						continue;
					}

					finalPivotDigit = Mask.TrailingZeroCount(mergedMask);
				}

				var maskToCompare = pivotDigit == -1 ? grid[combination] & ~(1 << finalPivotDigit) : grid[combination];
				if (((otherCell1Mask | otherCell2Mask) & ~comparer) != maskToCompare)
				{
					// Digits are not matched.
					continue;
				}

				var elimMapBase = pivotDigit == -1 ? combination : combination | cells & CandidatesMap[finalPivotDigit];
				var elimMap = elimMapBase.PeerIntersection & CandidatesMap[finalPivotDigit];
				if (!elimMap)
				{
					// No eliminations.
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in cells & EmptyCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								(comparer >> digit & 1) != 0
									? ColorIdentifier.Normal
									: digit == finalPivotDigit ? ColorIdentifier.Auxiliary2 : ColorIdentifier.Auxiliary1,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in combination)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								digit != finalPivotDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Auxiliary2,
								cell * 9 + digit
							)
						);
					}
				}

				var cellOffsets = new List<CellViewNode>();
				foreach (var cell in urCells)
				{
					if (!EmptyCells.Contains(cell))
					{
						cellOffsets.Add(new(ColorIdentifier.Normal, cell));
					}
				}

				if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out _))
				{
					return;
				}

				accumulator.Add(
					new UniqueRectangleRegularWingStep(
						(from cell in elimMap select new Conclusion(Elimination, cell, finalPivotDigit)).ToArray(),
						[[.. candidateOffsets, .. cellOffsets]],
						context.Options,
						(arMode, pivotDigit, combination.Count) switch
						{
							(false, -1, 2) => Technique.UniqueRectangleXyWing,
							(false, -1, 3) => Technique.UniqueRectangleXyzWing,
							(false, -1, 4) => Technique.UniqueRectangleWxyzWing,
							(false, _, 2) => Technique.UniqueRectangleXyzWing,
							(false, _, 3) => Technique.UniqueRectangleWxyzWing,
							(_, -1, 2) => Technique.AvoidableRectangleXyWing,
							(_, -1, 3) => Technique.AvoidableRectangleXyzWing,
							(_, -1, 4) => Technique.AvoidableRectangleWxyzWing,
							(_, _, 2) => Technique.AvoidableRectangleXyzWing,
							(_, _, 3) => Technique.AvoidableRectangleWxyzWing
						},
						d1,
						d2,
						cells,
						arMode,
						combination,
						otherCellsMap,
						otherDigitsMask,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR-W-Wing and AR-W-Wing.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>The structures:</para>
	/// <para>
	/// Subtype 1:
	/// <code><![CDATA[
	///         ↓ corner1
	/// xz   abx  (ab )
	///     (ab )  aby   yz
	///       ↑ corner2
	/// ]]></code>
	/// </para>
	/// <para>
	/// Subtype 2:
	/// <code><![CDATA[
	///       ↓ corner1
	/// xz  (ab )  abx
	///      abx  (ab )  xz
	///             ↑ corner2
	/// ]]></code>
	/// </para>
	/// <para>Please note that corner cells may be aligned as a same row or column.</para>
	/// <para>
	/// <i>Also, this method is useless because it may be replaced with another techniques such as UR-XY-Wing and UR External Type 2.</i>
	/// </para>
	/// </remarks>
	partial void CheckWWing(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		// Firstly, we should check whether the 2 corner cells should contain both a and b, and only contain a and b.
		// This expression only uses candidates to check digits appearing, so it doesn't determine whether the pattern is a UR or not.
		// ARs can also be passed of course.
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		var otherCell1Mask = grid.GetCandidates(otherCellsMap[0]);
		var otherCell2Mask = grid.GetCandidates(otherCellsMap[1]);
		if ((otherCell1Mask & otherCell2Mask & comparer) == 0 || ((otherCell1Mask | otherCell2Mask) & comparer) != comparer)
		{
			return;
		}

		var otherDigits1 = (Mask)(otherCell1Mask & ~comparer);
		if (!Mask.IsPow2(otherDigits1))
		{
			return;
		}
		var otherDigits2 = (Mask)(otherCell2Mask & ~comparer);
		if (!Mask.IsPow2(otherDigits2))
		{
			return;
		}

		var otherDigit1 = Mask.TrailingZeroCount(otherDigits1);
		var otherDigit2 = Mask.TrailingZeroCount(otherDigits2);

		// Now we check for other 2 cells, collecting digits not being UR/AR digits.
		var cells = urCells.AsCellMap();
		foreach (var endCell1 in PeersMap[otherCellsMap[0]] & BivalueCells & CandidatesMap[otherDigit1])
		{
			foreach (var endCell2 in (PeersMap[otherCellsMap[1]] & BivalueCells & CandidatesMap[otherDigit2]) - endCell1)
			{
				// Check whether two cells are same, or in a same house. If so, the pattern will be degenerated to a normal type 3.
				if ((endCell1.AsCellMap() + endCell2).FirstSharedHouse != 32)
				{
					continue;
				}

				var mergedMask = (Mask)(grid.GetCandidates(endCell1) & grid.GetCandidates(endCell2));
				if (!Mask.IsPow2(mergedMask))
				{
					continue;
				}

				var wDigit = Mask.TrailingZeroCount(mergedMask);
				if (otherDigit1 == wDigit || otherDigit2 == wDigit)
				{
					continue;
				}

				var elimMap = (endCell1.AsCellMap() + endCell2).PeerIntersection & CandidatesMap[wDigit];
				if (!elimMap)
				{
					// No eliminations.
					continue;
				}

				// A W-Wing found.
				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in urCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								digit == d1 || digit == d2 ? ColorIdentifier.Normal : ColorIdentifier.Auxiliary1,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var digit in grid.GetCandidates(endCell1))
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, endCell1 * 9 + digit));
				}
				foreach (var digit in grid.GetCandidates(endCell2))
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, endCell2 * 9 + digit));
				}

				if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out _))
				{
					return;
				}

				var isAvoidable = arMode && (EmptyCells & cells).Count != 4;
				accumulator.Add(
					new UniqueRectangleWWingStep(
						(from cell in elimMap select new Conclusion(Elimination, cell, wDigit)).ToArray(),
						[[.. candidateOffsets]],
						context.Options,
						isAvoidable ? Technique.AvoidableRectangleWWing : Technique.UniqueRectangleWWing,
						d1,
						d2,
						cells,
						isAvoidable,
						wDigit,
						otherCellsMap,
						endCell1.AsCellMap() + endCell2,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + SdC.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The pattern:
	/// <code><![CDATA[
	///           |   xyz
	///  ab+ ab+  | abxyz abxyz
	///           |   xyz
	/// ----------+------------
	/// (ab)(ab)  |
	///  ↑ corner1, corner2
	/// ]]></code>
	/// </remarks>
	private partial void CheckSueDeCoq(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		var notSatisfiedType3 = false;
		var mergedMaskInOtherCells = (Mask)0;
		foreach (var cell in otherCellsMap)
		{
			var currentMask = grid.GetCandidates(cell);
			mergedMaskInOtherCells |= currentMask;
			if ((currentMask & comparer) == 0 || currentMask == comparer || arMode && grid.GetState(cell) != CellState.Empty)
			{
				notSatisfiedType3 = true;
				break;
			}
		}

		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer || notSatisfiedType3
			|| (mergedMaskInOtherCells & comparer) != comparer)
		{
			return;
		}

		// Check whether the corners spanned two blocks. If so, UR + SdC can't be found.
		var blockMaskInOtherCells = otherCellsMap.BlockMask;
		if (!Mask.IsPow2(blockMaskInOtherCells))
		{
			return;
		}

		var otherDigitsMask = (Mask)(mergedMaskInOtherCells & ~comparer);
		var line = (byte)otherCellsMap.SharedLine;
		var block = (byte)HouseMask.TrailingZeroCount(otherCellsMap.SharedHouses & ~(1 << line));
		var d = Miniline.Map[new(line, block)].OtherBlocks;
		var list = new List<CellMap>(4);
		foreach (var cannibalMode in (false, true))
		{
			foreach (var otherBlock in d)
			{
				var emptyCellsInInterMap = HousesMap[otherBlock] & HousesMap[line] & EmptyCells;
				if (emptyCellsInInterMap.Count < 2)
				{
					// The intersection needs at least two empty cells.
					continue;
				}

				var a = Miniline.Map[new(line, otherBlock)].LineMap;
				var b = HousesMap[otherBlock] & ~HousesMap[line];
				var c = a & b;

				list.Clear();
				switch (emptyCellsInInterMap)
				{
					case { Count: 2 }:
					{
						list.AddRef(emptyCellsInInterMap);
						break;
					}
					case [var i, var j, var k]:
					{
						list.AddRef([i, j]);
						list.AddRef([j, k]);
						list.AddRef([i, k]);
						list.AddRef(emptyCellsInInterMap);
						break;
					}
				}

				// Iterate on each intersection combination.
				foreach (var currentInterMap in list)
				{
					var selectedInterMask = grid[currentInterMap];
					if (BitOperations.PopCount(selectedInterMask) <= currentInterMap.Count + 1)
					{
						// The intersection combination is an ALS or a normal subset,
						// which is invalid in SdCs.
						continue;
					}

					var blockMap = (b | c & ~currentInterMap) & EmptyCells;
					var lineMap = a & EmptyCells & ~otherCellsMap;

					// Iterate on the number of the cells that should be selected in block.
					for (var i = 1; i <= blockMap.Count - 1; i++)
					{
						// Iterate on each combination in block.
						foreach (ref readonly var selectedCellsInBlock in blockMap & i)
						{
							var flag = false;
							foreach (var digit in otherDigitsMask)
							{
								foreach (var cell in selectedCellsInBlock)
								{
									if (CandidatesMap[digit].Contains(cell))
									{
										flag = true;
										break;
									}
								}
							}
							if (flag)
							{
								continue;
							}

							var (currentBlockMap, elimMapBlock, elimMapLine) = (selectedCellsInBlock, CellMap.Empty, CellMap.Empty);

							// Get the links of the block.
							var blockMask = grid[selectedCellsInBlock];

							// Get the elimination map in the block.
							foreach (var digit in blockMask)
							{
								elimMapBlock |= CandidatesMap[digit];
							}
							elimMapBlock &= blockMap & ~currentBlockMap;

							foreach (var digit in otherDigitsMask)
							{
								elimMapLine |= CandidatesMap[digit];
							}
							elimMapLine &= lineMap & ~currentInterMap;

							checkGeneralizedSdc(
								accumulator, grid, ref context, arMode, cannibalMode, d1, d2, urCells,
								line, otherBlock, otherDigitsMask, blockMask, selectedInterMask,
								otherDigitsMask, elimMapLine, elimMapBlock, otherCellsMap, currentBlockMap,
								currentInterMap, i, 0, index
							);
						}
					}
				}
			}
		}


		static void checkGeneralizedSdc(
			SortedSet<UniqueRectangleStep> accumulator,
			in Grid grid,
			ref StepAnalysisContext context,
			bool arMode,
			bool cannibalMode,
			Digit digit1,
			Digit digit2,
			Cell[] urCells,
			House line,
			House block,
			Mask lineMask,
			Mask blockMask,
			Mask selectedInterMask,
			Mask otherDigitsMask,
			in CellMap elimMapLine,
			in CellMap elimMapBlock,
			in CellMap currentLineMap,
			in CellMap currentBlockMap,
			in CellMap currentInterMap,
			int i,
			int j,
			int index
		)
		{
			var maskOnlyInInter = (Mask)(selectedInterMask & ~(blockMask | lineMask));
			var maskIsolated = (Mask)(cannibalMode ? lineMask & blockMask & selectedInterMask : maskOnlyInInter);
			if (!cannibalMode && ((blockMask & lineMask) != 0 || maskIsolated != 0 && !Mask.IsPow2(maskIsolated))
				|| cannibalMode && !Mask.IsPow2(maskIsolated))
			{
				return;
			}

			var elimMapIsolated = CellMap.Empty;
			var digitIsolated = Mask.TrailingZeroCount(maskIsolated);
			if (digitIsolated != 16)
			{
				elimMapIsolated = (cannibalMode ? currentBlockMap | currentLineMap : currentInterMap)
					% CandidatesMap[digitIsolated]
					& EmptyCells;
			}

			if (currentInterMap.Count + i + j + 1 == BitOperations.PopCount(blockMask) + BitOperations.PopCount(lineMask) + BitOperations.PopCount(maskOnlyInInter)
				&& !!(elimMapBlock | elimMapLine | elimMapIsolated))
			{
				// Check eliminations.
				var conclusions = new List<Conclusion>(10);
				foreach (var cell in elimMapBlock)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if ((blockMask >> digit & 1) != 0)
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
				}
				foreach (var cell in elimMapLine)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if ((lineMask >> digit & 1) != 0)
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
				}
				foreach (var cell in elimMapIsolated)
				{
					conclusions.Add(new(Elimination, cell, digitIsolated));
				}
				if (conclusions.Count == 0)
				{
					return;
				}

				// Record highlight candidates and cells.
				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in urCells)
				{
					if (arMode && grid.GetState(cell) != CellState.Empty)
					{
						// Skip for non-empty cells.
						continue;
					}

					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								(otherDigitsMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in currentBlockMap)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								!cannibalMode && digit == digitIsolated ? ColorIdentifier.Auxiliary3 : ColorIdentifier.Auxiliary2,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in currentInterMap)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								digitIsolated == digit
									? ColorIdentifier.Auxiliary3
									: (otherDigitsMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Auxiliary2,
								cell * 9 + digit
							)
						);
					}
				}

				accumulator.Add(
					new UniqueRectangleSueDeCoqStep(
						conclusions.AsMemory(),
						[
							[
								.. arMode ? GetHighlightCells(urCells) : [],
								.. candidateOffsets,
								new HouseViewNode(ColorIdentifier.Normal, block),
								new HouseViewNode(ColorIdentifier.Auxiliary2, line)
							]
						],
						context.Options,
						digit1,
						digit2,
						[.. urCells],
						arMode,
						block,
						line,
						blockMask,
						lineMask,
						selectedInterMask,
						cannibalMode,
						maskIsolated,
						currentBlockMap,
						currentLineMap,
						currentInterMap,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + ALS and AR + ALS.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the method searches for avoidable rectangles.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1.</param>
	/// <param name="d2">The digit 2.</param>
	/// <param name="alses">
	/// <para>The ALS patterns found.</para>
	/// <para>
	/// <include file="../../global-doc-comments.xml" path="g/csharp11/feature[@name='scoped-keyword']/para"/>
	/// </para>
	/// <para>
	/// This parameter expects a <see langword="scoped"/> keyword
	/// because <paramref name="alses"/> may assign to <paramref name="context"/>
	/// because <paramref name="context"/> expects a <see cref="ReadOnlySpan{T}"/> instance,
	/// which is a very "implicit" compiler error that we cannot aware of this at once.
	/// </para>
	/// <para>
	/// By appending modifier <see langword="scoped"/>, compiler will know that this parameter should only be used
	/// inside this method or other places allowing such <see langword="scoped"/> usages (recursive usages),
	/// which won't extend its lifecycle.
	/// </para>
	/// <para>
	/// Such usage is same as method <see cref="CheckExternalAlmostLockedSetsXz"/>.
	/// </para>
	/// <para>
	/// Please check this page to learn more information on
	/// <see href="https://blog.walterlv.com/post/cs8350-ref-arguments-combination-is-disallowed">CS8350</see>.
	/// </para>
	/// </param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>
	/// The pattern:
	/// <code><![CDATA[
	///       ↓ cell1, cells2
	///  ab | abc    *
	///  ab | abcd   *
	///     |       cdef
	/// ----+------------
	///     |       def
	///     |       def
	/// ]]></code>
	/// where a cell <c>cdef</c> and 2 cells <c>def</c> will be formed an ALS pattern. Therefore, a chain is formed:
	/// <code><![CDATA[abcd(d) == {abc, abcd}(c) -- cdef(c) == {def, def}(d) => * != d]]></code>
	/// </para>
	/// <para>
	/// In addition, this pattern will cover all possible cases of UR + 2D and UR + 3X.
	/// </para>
	/// </remarks>
	private partial void CheckAlmostLockedSetsXz(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, scoped ReadOnlySpan<AlmostLockedSetPattern> alses, int index)
	{
		var cells = urCells.AsCellMap();
		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Check whether there're only 2 extra digit.
		var extraDigitsMask = (Mask)(grid[cells] & ~comparer);
		if (BitOperations.PopCount(extraDigitsMask) != 2)
		{
			return;
		}

		// We know that cells holding two digits will form a strong link.
		var extraDigit1 = (int)Mask.TrailingZeroCount(extraDigitsMask);
		var extraDigit2 = extraDigitsMask.GetNextSet(extraDigit1);
		var extraCells1 = CandidatesMap[extraDigit1] & cells;
		var extraCells2 = CandidatesMap[extraDigit2] & cells;
		foreach (ref readonly var quadruple in (
			(ExtraCells1: extraCells1, ExtraCells2: extraCells2, ExtraDigit1: extraDigit1, ExtraDigit2: extraDigit2),
			(ExtraCells1: extraCells2, ExtraCells2: extraCells1, ExtraDigit1: extraDigit2, ExtraDigit2: extraDigit1)
		))
		{
			ref readonly var cells1 = ref quadruple.ExtraCells1;
			ref readonly var cells2 = ref quadruple.ExtraCells2;
			var digit1 = quadruple.ExtraDigit1;
			var digit2 = quadruple.ExtraDigit2;

			// Check elimination on digit2 and find weak links on digit1.
			foreach (var als in alses)
			{
				// Check whether the ALS digits mask holds all possible digits as extra digits in UR.
				var alsDigitsMask = als.DigitsMask;
				if ((alsDigitsMask & extraDigitsMask) != extraDigitsMask)
				{
					continue;
				}

				// Check whether the ALS pattern intersects with the current UR pattern.
				// If so, we should skip it because it'll disturb out work.
				// In fact, technique allows ALS pattern intersects with UR. There're two cases:
				//
				//   1) ALS node (strong link node) overlaps with cells that UR uses.
				//      In this case, the doubly-linked ALS-XZ will be replaced
				//      with another technique called 'Self Constraint' in this program. This will be handled in chaining searcher.
				//   2) Though 1) doesn't cover, ALS pattern still overlaps with UR pattern cells.
				//      In this case, the doubly-linked ALS-XZ still forms,
				//      but extra eliminations produced by UR pattern will become unavailable.
				//
				// Considered its complexity of implementation, I will skip overlapped cases and may not handle such cases.
				var alsCells = als.Cells;
				if (alsCells & cells)
				{
					continue;
				}

				var alsCells1 = alsCells & CandidatesMap[digit1];
				var alsCells2 = alsCells & CandidatesMap[digit2];

				// Check whether digit1 forms a weak link from both ALS and UR.
				if ((cells1.PeerIntersection & alsCells1) != alsCells1)
				{
					continue;
				}

				// Check for elimination cells.
				var digit2UnionMap = cells2 | alsCells2;
				var elimMap = digit2UnionMap.PeerIntersection & CandidatesMap[digit2];
				if (!elimMap)
				{
					continue;
				}

				// Okay. Now elimination is found. Collect view nodes and record the step.
				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in urCells)
				{
					if (grid.GetState(cell) != CellState.Empty)
					{
						continue;
					}

					foreach (var digit in comparer)
					{
						if ((grid.GetCandidates(cell) >> digit & 1) != 0)
						{
							candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
						}
					}
				}
				foreach (var cell in cells2)
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit2));
				}
				foreach (var cell in cells1)
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit1));
				}
				foreach (var cell in alsCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								digit == digit1
									? ColorIdentifier.Auxiliary2
									: digit == digit2 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.AlmostLockedSet1,
								cell * 9 + digit
							)
						);
					}
				}
				if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
				{
					continue;
				}

				var eliminations = from cell in elimMap select new Conclusion(Elimination, cell, digit2);
				var cellOffsets = (List<CellViewNode>)[
					.. arMode ? from cell in urCells select new CellViewNode(ColorIdentifier.Normal, cell) : [],
					.. from cell in alsCells select new CellViewNode(ColorIdentifier.AlmostLockedSet1, cell)
				];
				var multivalueCellsCount = 0;
				foreach (var cell in urCells)
				{
					if ((Mask)(grid.GetCandidates(cell) & ~comparer) != 0)
					{
						multivalueCellsCount++;
					}
				}
				if (digit2UnionMap.FirstSharedHouse == 32)
				{
					accumulator.Add(
						new UniqueRectangleAlmostLockedSetsXzStep(
							eliminations.ToArray(),
							[[.. cellOffsets, .. candidateOffsets]],
							context.Options,
							d1,
							d2,
							cells,
							isIncomplete,
							arMode,
							false,
							als,
							multivalueCellsCount,
							index
						)
					);
					continue;
				}

				// Skip the case on ARs.
				if (arMode)
				{
					continue;
				}

				// A Doubly-linked ALS-XZ pattern is formed. We should check for extra eliminations.
				// Extra eliminations can be found in two places:
				//
				//   1) ALS pattern -> subset
				//   2) UR pattern
				//
				// For 2), it will be tough to be checked.
				var doublyLinkedEliminations = (List<Conclusion>)[
					.. eliminations,
					..
					from elimCell in (cells1 | alsCells1).PeerIntersection & CandidatesMap[digit1]
					select new Conclusion(Elimination, elimCell, digit1),
					.. EliminationCalculator.UniqueRectangle.GetConclusions(cells, comparer, grid)
				];
				foreach (var cell in HousesMap[als.House] & ~alsCells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & alsDigitsMask & ~extraDigitsMask))
					{
						doublyLinkedEliminations.Add(new(Elimination, cell, digit));
					}
				}
				if (doublyLinkedEliminations.Count == 0)
				{
					continue;
				}

				// Change view nodes.
				foreach (var cell in cells1)
				{
					candidateOffsets.Remove(new(ColorIdentifier.Auxiliary2, cell * 9 + digit1));
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit1));
				}
				foreach (var cell in alsCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if (digit != digit1)
						{
							continue;
						}

						candidateOffsets.Remove(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}
				}

				accumulator.Add(
					new UniqueRectangleAlmostLockedSetsXzStep(
						doublyLinkedEliminations.ToArray(),
						[[.. cellOffsets, .. candidateOffsets]],
						context.Options,
						d1,
						d2,
						cells,
						isIncomplete,
						arMode,
						true,
						als,
						multivalueCellsCount,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check AR + Hidden single.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">
	/// The map of other cells during the current UR searching.
	/// </param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>The pattern:</para>
	/// <para>
	/// <code><![CDATA[
	/// ↓corner1
	/// a   | aby.  .
	/// abx | a  .  b
	///     | .  .  .
	///       ↑corner2(cell 'a')
	/// ]]></code>
	/// There's only one cell can be filled with the digit 'b' besides the cell 'aby'.
	/// </para>
	/// </remarks>
	private partial void CheckHiddenSingleAvoidable(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		if (grid.GetState(corner1) != CellState.Modifiable || grid.GetState(corner2) != CellState.Modifiable
			|| grid.GetDigit(corner1) != grid.GetDigit(corner2) || grid.GetDigit(corner1) != d1 && grid.GetDigit(corner1) != d2)
		{
			return;
		}

		// Get the base digit ('a') and the other digit ('b').
		// Here 'b' is the digit that we should check the possible hidden single.
		var baseDigit = grid.GetDigit(corner1);
		var otherDigit = baseDigit == d1 ? d2 : d1;
		var cellsThatTwoOtherCellsBothCanSee = otherCellsMap.PeerIntersection & CandidatesMap[otherDigit];

		// Iterate on two cases (because we holds two other cells,
		// and both those two cells may contain possible elimination).
		for (var i = 0; i < 2; i++)
		{
			var (baseCell, anotherCell) = i == 0 ? (otherCellsMap[0], otherCellsMap[1]) : (otherCellsMap[1], otherCellsMap[0]);

			// Iterate on each house type.
			foreach (var houseType in HouseTypes)
			{
				var houseIndex = baseCell.ToHouse(houseType);

				// If the house doesn't overlap with the specified house, just skip it.
				if (!(cellsThatTwoOtherCellsBothCanSee & HousesMap[houseIndex]))
				{
					continue;
				}

				var otherCells = HousesMap[houseIndex] & CandidatesMap[otherDigit] & PeersMap[anotherCell];
				var sameHouses = (otherCells + anotherCell).SharedHouses;
				foreach (var sameHouse in sameHouses)
				{
					// Check whether all possible positions of the digit 'b' in this house only
					// lies in the given cells above ('cellsThatTwoOtherCellsBothCanSee').
					if ((HousesMap[sameHouse] - anotherCell & CandidatesMap[otherDigit]) != otherCells)
					{
						continue;
					}

					// Possible hidden single found.
					// If the elimination doesn't exist, just skip it.
					if (!CandidatesMap[otherDigit].Contains(baseCell))
					{
						continue;
					}

					accumulator.Add(
						new AvoidableRectangleHiddenSingleStep(
							new SingletonArray<Conclusion>(new(Elimination, baseCell, otherDigit)),
							[
								[
									.. from cell in urCells select new CellViewNode(ColorIdentifier.Normal, cell),
									new CandidateViewNode(ColorIdentifier.Normal, anotherCell * 9 + otherDigit),
									..
									from cell in otherCells
									select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + otherDigit),
									new HouseViewNode(ColorIdentifier.Normal, sameHouse)
								]
							],
							context.Options,
							d1,
							d2,
							[.. urCells],
							baseCell,
							anotherCell,
							sameHouse,
							index
						)
					);
				}
			}
		}
	}
}
