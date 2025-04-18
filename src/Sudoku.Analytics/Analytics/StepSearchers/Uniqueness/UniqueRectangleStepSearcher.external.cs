namespace Sudoku.Analytics.StepSearchers.Uniqueness;

public partial class UniqueRectangleStepSearcher
{
	/// <summary>
	/// <para>Check UR/AR + Guardian (i.e. UR External Type 2) and UR External Type 1.</para>
	/// <para>
	/// A UR external type 1 is a special case for type 2, which means only one guardian cell will be used.
	/// </para>
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The index.</param>
	/// <param name="arMode"></param>
	private partial void CheckExternalType1Or2(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Digit d1, Digit d2, int index, bool arMode)
	{
		var cells = urCells.AsCellMap();
		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.AllSets.GetSubsets(2))
		{
			var houseCells = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((houseCells & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardian1 = houseCells & ~cells & CandidatesMap[d1];
			var guardian2 = houseCells & ~cells & CandidatesMap[d2];
			if (!guardian1 ^ !guardian2)
			{
				var guardianDigit = -1;
				var targetElimMap = default(CellMap?);
				var targetGuardianMap = default(CellMap?);
				if (!!guardian1 && (guardian1.PeerIntersection & CandidatesMap[d1]) is var a and not [])
				{
					targetElimMap = a;
					guardianDigit = d1;
					targetGuardianMap = guardian1;
				}
				else if (!!guardian2 && (guardian2.PeerIntersection & CandidatesMap[d2]) is var b and not [])
				{
					targetElimMap = b;
					guardianDigit = d2;
					targetGuardianMap = guardian2;
				}

				if (targetElimMap is not { } elimMap || targetGuardianMap is not { } guardianMap || guardianDigit == -1)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(16);
				var cellOffsets = new List<CellViewNode>();
				foreach (var cell in urCells)
				{
					if (CandidatesMap[d1].Contains(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d1));
					}
					if (CandidatesMap[d2].Contains(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d2));
					}

					if (grid.GetState(cell) == CellState.Modifiable)
					{
						cellOffsets.Add(new(ColorIdentifier.Normal, cell));
					}
				}
				foreach (var cell in guardianMap)
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + guardianDigit));
				}

				if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
				{
					return;
				}

				accumulator.Add(
					new UniqueRectangleExternalType1Or2Step(
						(from cell in elimMap select new Conclusion(Elimination, cell, guardianDigit)).ToArray(),
						[
							[
								.. cellOffsets,
								.. candidateOffsets,
								new HouseViewNode(ColorIdentifier.Normal, houseCombination[0]),
								new HouseViewNode(ColorIdentifier.Normal, houseCombination[1])
							]
						],
						context.Options,
						d1,
						d2,
						[.. urCells],
						guardianMap,
						guardianDigit,
						isIncomplete,
						arMode,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external subset (i.e. UR External Type 3).
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The index.</param>
	/// <param name="arMode"></param>
	private partial void CheckExternalType3(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Mask comparer, Digit d1, Digit d2, int index, bool arMode)
	{
		var cells = urCells.AsCellMap();
		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.AllSets.GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap & ~cells & EmptyCells;
			foreach (ref readonly var guardianCellPair in guardianCells & 2)
			{
				var c1 = guardianCellPair[0];
				var c2 = guardianCellPair[1];
				if (!IsSameHouseCell(c1, c2, out var houses))
				{
					// Those two cells must lie in a same house.
					continue;
				}

				var mask = (Mask)(grid.GetCandidates(c1) | grid.GetCandidates(c2));
				if ((mask & comparer) != comparer)
				{
					// The two cells must contain both two digits.
					continue;
				}

				if ((grid.GetCandidates(c1) & comparer) == 0 || (grid.GetCandidates(c2) & comparer) == 0)
				{
					// Both two cells chosen must contain at least one of two UR digits.
					continue;
				}

				if ((guardianCells & (CandidatesMap[d1] | CandidatesMap[d2])) != guardianCellPair)
				{
					// The current map must be equal to the whole guardian full map.
					continue;
				}

				foreach (var house in houses)
				{
					var houseCells = HousesMap[house] & ~cells & ~guardianCellPair & EmptyCells;
					for (var size = 2; size <= houseCells.Count; size++)
					{
						foreach (ref readonly var otherCells in houseCells & size - 1)
						{
							var subsetDigitsMask = (Mask)(grid[otherCells] | comparer);
							if (BitOperations.PopCount(subsetDigitsMask) != size)
							{
								// The subset cannot formed.
								continue;
							}

							// UR Guardian External Subsets found. Now check eliminations.
							var elimMap = houseCells & ~otherCells | guardianCellPair;
							var conclusions = new List<Conclusion>();
							foreach (var cell in elimMap)
							{
								var elimDigitsMask = guardianCellPair.Contains(cell) ? (Mask)(subsetDigitsMask & ~comparer) : subsetDigitsMask;
								foreach (var digit in elimDigitsMask)
								{
									if (CandidatesMap[digit].Contains(cell))
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
							foreach (var cell in urCells)
							{
								if (CandidatesMap[d1].Contains(cell))
								{
									candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d1));
								}
								if (CandidatesMap[d2].Contains(cell))
								{
									candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d2));
								}

								if (grid.GetState(cell) == CellState.Modifiable)
								{
									cellOffsets.Add(new(ColorIdentifier.Normal, cell));
								}
							}
							foreach (var cell in guardianCellPair)
							{
								if (CandidatesMap[d1].Contains(cell))
								{
									candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d1));
								}
								if (CandidatesMap[d2].Contains(cell))
								{
									candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d2));
								}
							}
							foreach (var cell in otherCells)
							{
								foreach (var digit in grid.GetCandidates(cell))
								{
									candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
								}
							}

							if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
							{
								return;
							}

							accumulator.Add(
								new UniqueRectangleExternalType3Step(
									conclusions.AsMemory(),
									[
										[
											.. cellOffsets,
											.. candidateOffsets,
											new HouseViewNode(ColorIdentifier.Normal, house),
											new HouseViewNode(ColorIdentifier.Auxiliary2, houseCombination[0]),
											new HouseViewNode(ColorIdentifier.Auxiliary2, houseCombination[1])
										]
									],
									context.Options,
									d1,
									d2,
									cells,
									guardianCellPair,
									otherCells,
									subsetDigitsMask,
									isIncomplete,
									arMode,
									index
								)
							);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external conjugate pair (i.e. UR External Type 4).
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The index.</param>
	/// <param name="arMode"></param>
	private partial void CheckExternalType4(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Mask comparer, Digit d1, Digit d2, int index, bool arMode)
	{
		var cells = urCells.AsCellMap();
		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.AllSets.GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap & ~cells & EmptyCells;
			foreach (ref readonly var guardianCellPair in guardianCells & 2)
			{
				var c1 = guardianCellPair[0];
				var c2 = guardianCellPair[1];
				if (!IsSameHouseCell(c1, c2, out var houses))
				{
					// Those two cells must lie in a same house.
					continue;
				}

				var mask = (Mask)(grid.GetCandidates(c1) | grid.GetCandidates(c2));
				if ((mask & comparer) != comparer)
				{
					// The two cells must contain both two digits.
					continue;
				}

				if ((grid.GetCandidates(c1) & comparer) == 0 || (grid.GetCandidates(c2) & comparer) == 0)
				{
					// Both two cells chosen must contain at least one of two UR digits.
					continue;
				}

				if ((guardianCells & (CandidatesMap[d1] | CandidatesMap[d2])) != guardianCellPair)
				{
					// The current map must be equal to the whole guardian full map.
					continue;
				}

				var possibleConjugatePairDigitsMask = (Mask)(grid[guardianCellPair] & ~comparer);
				foreach (var house in houses)
				{
					foreach (var conjugatePairDigit in possibleConjugatePairDigitsMask)
					{
						if (!CandidatesMap[conjugatePairDigit].Contains(c1) || !CandidatesMap[conjugatePairDigit].Contains(c2))
						{
							// The conjugate pair can be formed if and only if both guardian cells
							// must contain that digit.
							continue;
						}

						if ((CandidatesMap[conjugatePairDigit] & HousesMap[house]) != guardianCellPair)
						{
							// The house cannot contain any other cells containing that digit.
							continue;
						}

						var conclusions = new List<Conclusion>();
						var elimDigitsMask = (Mask)(possibleConjugatePairDigitsMask & ~(1 << conjugatePairDigit));
						foreach (var elimDigit in elimDigitsMask)
						{
							foreach (var cell in CandidatesMap[elimDigit] & guardianCellPair)
							{
								conclusions.Add(new(Elimination, cell, elimDigit));
							}
						}
						if (conclusions.Count == 0)
						{
							// No eliminations found.
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>();
						var cellOffsets = new List<CellViewNode>();
						foreach (var cell in urCells)
						{
							if (CandidatesMap[d1].Contains(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d1));
							}
							if (CandidatesMap[d2].Contains(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d2));
							}

							if (grid.GetState(cell) == CellState.Modifiable)
							{
								cellOffsets.Add(new(ColorIdentifier.Normal, cell));
							}
						}
						foreach (var cell in guardianCellPair)
						{
							if (CandidatesMap[d1].Contains(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d1));
							}
							if (CandidatesMap[d2].Contains(cell))
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d2));
							}

							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + conjugatePairDigit));
						}

						if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
						{
							return;
						}

						accumulator.Add(
							new UniqueRectangleExternalType4Step(
								conclusions.AsMemory(),
								[
									[
										.. cellOffsets,
										.. candidateOffsets,
										new ConjugateLinkViewNode(
											ColorIdentifier.Normal,
											guardianCellPair[0],
											guardianCellPair[1],
											conjugatePairDigit
										),
										new HouseViewNode(ColorIdentifier.Auxiliary2, houseCombination[0]),
										new HouseViewNode(ColorIdentifier.Auxiliary2, houseCombination[1])
									]
								],
								context.Options,
								d1,
								d2,
								cells,
								guardianCellPair,
								new(guardianCellPair, conjugatePairDigit),
								isIncomplete,
								arMode,
								index
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR + Guardian, with external turbot fish.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The mask index.</param>
	private partial void CheckExternalTurbotFish(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Mask comparer, Digit d1, Digit d2, int index)
	{
		var cells = urCells.AsCellMap();

		// Iterates on each digit, checking whether the current digit forms a guardian pattern but the other digit not.
		foreach (var (guardianDigit, nonGuardianDigit) in ((d1, d2), (d2, d1)))
		{
			// Iterates on each pair of houses where the UR pattern located.
			foreach (var houses in cells.Houses.AllSets.GetSubsets(2))
			{
				if (HousesMap[houses[0]] & HousesMap[houses[1]])
				{
					// Two houses iterated must contain no intersection.
					continue;
				}

				var housesFullMap = HousesMap[houses[0]] | HousesMap[houses[1]];

				// Gets the guardian cells in both houses.
				// Here guardian cells may contain multiple cells. We don't check for it because it can be used as grouped turbot fish.
				var guardianCells = housesFullMap & CandidatesMap[guardianDigit] & ~cells;

				// Then check whether the other digit is locked in the UR pattern.
				var flag = true;
				foreach (var house in houses)
				{
					var tempMap = HousesMap[house] & CandidatesMap[nonGuardianDigit];
					if ((cells & tempMap) != tempMap || tempMap.Count != 2)
					{
						// The current house may not form a valid conjugate pair
						// because the current house may contain at least 3 cells can appear that digit.
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}

				if (guardianCells.SharedHouses != 0)
				{
					// It is a UR type 4 because all possible guardian cells lie in a same house.
					continue;
				}

				// Gets the last cell that the houses iterated don't contain.
				// For example, if the houses iterated are a row and a column, then the houses may cover 3 cells in the UR pattern.
				// Here we should get the cell uncovered, to check whether it is a bi-value cell and only contains the digits used by UR.
				//
				// I give a diagram to show what I want to tell you.
				// If the last cell not covered contains not only the first and the second digit in UR:
				//
				//         a
				//         ↓
				//   ab → abx | aby
				//    b → abz | abw
				//
				// Where the arrow means the current row or column contains a strong link of that digit.
				// Here the cell 'abw' isn't covered for strong links of digit a.
				// If we suppose that both cells 'aby' and 'abz' is filled with digit b, the pattern will be reduced:
				//
				//         a
				//         ↓
				//   ab →  a | b
				//    b →  b | ?
				//
				// But what digit will be filled with 'abw'? We don't know! We cannot decide which digit will be filled with the cell,
				// so the final digit a (causing a deadly pattern) may not be formed.
				// We should exclude this case to make the pattern be strict, which is what I want to tell you.
				if ((cells & ~housesFullMap)[0] is var lastCell and not -1
					&& (!BivalueCells.Contains(lastCell) || comparer != grid.GetCandidates(lastCell)))
				{
					continue;
				}

				// Check whether guardian cells cannot create links to form a turbot fish.
				var (a, b) = (getAvailableHouses(houses[0], guardianCells), getAvailableHouses(houses[1], guardianCells));
				if (a == 0 && b == 0)
				{
					continue;
				}

				foreach (var weakLinkHouse in a | b)
				{
					var otherCellsInWeakLinkHouse = HousesMap[weakLinkHouse] & CandidatesMap[guardianDigit] & ~guardianCells;
					if (!otherCellsInWeakLinkHouse)
					{
						// Cannot continue the turbot fish.
						continue;
					}

					foreach (var otherCellInWeakLinkHouse in otherCellsInWeakLinkHouse)
					{
						foreach (var strongLinkHouse in otherCellInWeakLinkHouse.AsCellMap().Houses)
						{
							if ((HousesMap[strongLinkHouse] & CandidatesMap[guardianDigit]) - otherCellInWeakLinkHouse is not [var finalCell])
							{
								// No eliminations will exist in this case.
								continue;
							}

							// A turbot fish found. Now check eliminations.
							var elimMap = (guardianCells & ~(HousesMap[weakLinkHouse] & guardianCells)).PeerIntersection & PeersMap[finalCell] & CandidatesMap[guardianDigit];
							if (!elimMap)
							{
								// No eliminations.
								continue;
							}

							var candidateOffsets = new List<CandidateViewNode>();
							foreach (var cell in cells)
							{
								foreach (var digit in (Mask)(grid.GetCandidates(cell) & comparer))
								{
									candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
								}
							}
							foreach (var cell in guardianCells)
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + guardianDigit));
							}
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, otherCellInWeakLinkHouse * 9 + guardianDigit));
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, finalCell * 9 + guardianDigit));

							if (!IsIncompleteValid(false, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
							{
								return;
							}

							accumulator.Add(
								new UniqueRectangleExternalTurbotFishStep(
									(from cell in elimMap select new Conclusion(Elimination, cell, guardianDigit)).ToArray(),
									[
										[
											.. candidateOffsets,
											new HouseViewNode(ColorIdentifier.Normal, houses[0]),
											new HouseViewNode(ColorIdentifier.Normal, houses[1])
										]
									],
									context.Options,
									d1,
									d2,
									cells,
									guardianCells,
									isIncomplete,
									index
								)
							);
						}
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static HouseMask getAvailableHouses(House house, in CellMap guardianCells)
		{
			var intersection = guardianCells & HousesMap[house];
			return house switch
			{
				< 9 => (BitOperations.TrailingZeroCount(intersection.RowMask), BitOperations.TrailingZeroCount(intersection.ColumnMask)) switch
				{
					(var a and not 16, var b and not 16) => 1 << a + 9 | 1 << b + 18,
					(var a and not 16, _) => 1 << a + 9,
					(_, var b and not 16) => 1 << b + 18,
					_ => 0
				},
				_ => BitOperations.TrailingZeroCount(intersection.BlockMask) switch
				{
					var result and not 16 => 1 << result,
					_ => 0
				}
			};
		}
	}

	/// <summary>
	/// Check UR + Guardian, with external W-Wing.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The mask index.</param>
	private partial void CheckExternalWWing(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Mask comparer, Digit d1, Digit d2, int index)
	{
		// Collect all digits that all bi-value cells in the current grid used.
		// W-Wing should contain a pair of cells which contain same 2 digits.
		// This step will collect all digits used, in order to check which cells will be skipped.
		var (bivalueCellsDigitsMask, bivalueCellsFiltered) = ((Mask)0, CellMap.Empty);
		foreach (var cell in BivalueCells)
		{
			// Check whether the current cell contain either the first or the second digit appeared in UR.
			// This step is important, because the external W-Wing must hold at least one digit appeared in UR.
			// According to the following W-Wing pattern:
			//
			//   (w=x)-(x=x)-(x=w)
			//
			// this method will start the chaining from the middle of the pattern. From this pattern, we can learn that
			// digit x must be a guardian digit, so it must be the first or the second digit appeared in UR.
			// Therefore, no matter what digit w will be, the digit x must be a UR digit.
			var digitsMask = grid.GetCandidates(cell);
			if ((digitsMask & comparer) == 0)
			{
				continue;
			}

			bivalueCellsFiltered.Add(cell);
			bivalueCellsDigitsMask |= digitsMask;
		}

		// Check whether at least one bi-value cell has at least one UR digit.
		if ((bivalueCellsDigitsMask & comparer) == 0)
		{
			return;
		}

		var cells = urCells.AsCellMap();

		// Iterates on each digit, checking whether the current digit forms a guardian pattern but the other digit not.
		// Here the variable 'xDigit' has a same concept as guardian digit.
		foreach (var (xDigit, nonGuardianDigit) in ((d1, d2), (d2, d1)))
		{
			// Iterates on each pair of houses where the UR pattern located.
			foreach (var houses in cells.Houses.AllSets.GetSubsets(2))
			{
				if (HousesMap[houses[0]] & HousesMap[houses[1]])
				{
					// Two houses iterated must contain no intersection.
					continue;
				}

				var housesFullMap = HousesMap[houses[0]] | HousesMap[houses[1]];

				// Gets the guardian cells in both houses.
				// Here guardian cells may contain multiple cells. We don't check for it because it can be used as grouped turbot fish.
				var guardianCells = housesFullMap & CandidatesMap[xDigit] & ~cells;

				// Then check whether the other digit is locked in the UR pattern.
				var flag = true;
				foreach (var house in houses)
				{
					var tempMap = HousesMap[house] & CandidatesMap[nonGuardianDigit];
					if ((cells & tempMap) != tempMap || tempMap.Count != 2)
					{
						// The current house may not form a valid conjugate pair
						// because the current house may contain at least 3 cells can appear that digit.
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}

				if (guardianCells.SharedHouses != 0)
				{
					// It is a UR type 4 because all possible guardian cells lie in a same house.
					continue;
				}

				// Gets the last cell that the houses iterated don't contain.
				// Why we should do this? Please see the comments above this method (same statement in the method "UR external turbot fish").
				if ((cells & ~housesFullMap)[0] is var lastCell and not -1
					&& (!BivalueCells.Contains(lastCell) || comparer != grid.GetCandidates(lastCell)))
				{
					continue;
				}

				// Check whether guardian cells cannot create links to form a W-Wing.
				var (a, b) = (getAvailableHouses(houses[0], guardianCells), getAvailableHouses(houses[1], guardianCells));
				if (a == 0 || b == 0)
				{
					continue;
				}

				foreach (var weakLinkHouses in (a | b).AllSets.GetSubsets(2))
				{
					if (((HousesMap[weakLinkHouses[0]] | HousesMap[weakLinkHouses[1]]) & guardianCells) != guardianCells)
					{
						continue;
					}

					var bivalueCellsFoundStart = HousesMap[weakLinkHouses[0]] & CandidatesMap[xDigit] & ~guardianCells & BivalueCells;
					var bivalueCellsFoundEnd = HousesMap[weakLinkHouses[1]] & CandidatesMap[xDigit] & ~guardianCells & BivalueCells;
					foreach (var startCell in bivalueCellsFoundStart)
					{
						var startCellDigitsMask = grid.GetCandidates(startCell);
						foreach (var endCell in bivalueCellsFoundEnd - startCell)
						{
							var endCellDigitsMask = grid.GetCandidates(endCell);
							if (startCellDigitsMask != endCellDigitsMask)
							{
								continue;
							}

							// A valid pattern is found. Now check eliminations.
							var wDigit = BitOperations.TrailingZeroCount((Mask)(startCellDigitsMask & ~(1 << xDigit)));
							var elimMap = (startCell.AsCellMap() + endCell).PeerIntersection & CandidatesMap[wDigit];
							if (!elimMap)
							{
								// No eliminations found.
								continue;
							}

							var candidateOffsets = new List<CandidateViewNode>();
							foreach (var cell in urCells)
							{
								foreach (var digit in comparer & grid.GetCandidates(cell))
								{
									candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
								}
							}
							foreach (var cell in guardianCells)
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + xDigit));
							}
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, startCell * 9 + xDigit));
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, startCell * 9 + wDigit));
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, endCell * 9 + xDigit));
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, endCell * 9 + wDigit));

							if (!IsIncompleteValid(false, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
							{
								return;
							}

							accumulator.Add(
								new UniqueRectangleExternalWWingStep(
									(from cell in elimMap select new Conclusion(Elimination, cell, wDigit)).ToArray(),
									[
										[
											.. candidateOffsets,
											new HouseViewNode(ColorIdentifier.Normal, houses[0]),
											new HouseViewNode(ColorIdentifier.Normal, houses[1]),
											new HouseViewNode(ColorIdentifier.Auxiliary1, weakLinkHouses[0]),
											new HouseViewNode(ColorIdentifier.Auxiliary1, weakLinkHouses[1])
										]
									],
									context.Options,
									d1,
									d2,
									cells,
									guardianCells,
									startCell.AsCellMap() + endCell,
									isIncomplete,
									false,
									index
								)
							);
						}
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static HouseMask getAvailableHouses(House house, in CellMap guardianCells)
		{
			var intersection = guardianCells & HousesMap[house];
			return house switch
			{
				< 9 => (BitOperations.TrailingZeroCount(intersection.RowMask), BitOperations.TrailingZeroCount(intersection.ColumnMask)) switch
				{
					(var a and not 16, var b and not 16) => 1 << a + 9 | 1 << b + 18,
					(var a and not 16, _) => 1 << a + 9,
					(_, var b and not 16) => 1 << b + 18,
					_ => 0
				},
				_ => BitOperations.TrailingZeroCount(intersection.BlockMask) switch
				{
					var result and not 16 => 1 << result,
					_ => 0
				}
			};
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external XY-Wing.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The mask index.</param>
	/// <param name="arMode"></param>
	private partial void CheckExternalXyWing(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Mask comparer, Digit d1, Digit d2, int index, bool arMode)
	{
		var cells = urCells.AsCellMap();
		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.AllSets.GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap & ~cells & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (!(guardianCells & CandidatesMap[d1]) || !(guardianCells & CandidatesMap[d2]))
			{
				// Guardian cells must contain both two digits; otherwise, skip the current case.
				continue;
			}

			var cellsToEnumerate = guardianCells.ExpandedPeers & ~guardianCells & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (cellsToEnumerate.Count < 2)
			{
				// No valid combinations.
				continue;
			}

			//forOneEndoLeaf(grid, ref context, cellsToEnumerate, guardianCells, houseCombination);
			forBothExoLeaves(grid, ref context, cellsToEnumerate, guardianCells, houseCombination);
		}


#pragma warning disable CS8321
		void forOneEndoLeaf(
			in Grid grid,
			ref StepAnalysisContext context,
			in CellMap cellsToEnumerate,
			in CellMap guardianCells,
			House[] houseCombination
		)
		{
			foreach (var cell1 in guardianCells)
			{
				foreach (var cell2 in cellsToEnumerate)
				{
					if (!PeersMap[cell1].Contains(cell2))
					{
						// Two cells cannot see each other.
						continue;
					}

					var (mask1, mask2) = (grid.GetCandidates(cell1), grid.GetCandidates(cell2));
					var intersectionMask = (Mask)(mask1 & mask2);
					if (!BitOperations.IsPow2(intersectionMask))
					{
						// No eliminations can be found in this pattern.
						continue;
					}

					var unionMask = (Mask)(mask1 | mask2);
					if ((unionMask & comparer) != comparer)
					{
						// The two cells must contain both two digits.
						continue;
					}

					if ((unionMask & ~(comparer | intersectionMask)) != 0)
					{
						// These two cells may contain extra cells, which is disallowed in this pattern.
						continue;
					}

					var elimDigit = BitOperations.TrailingZeroCount(intersectionMask);
					if ((mask1 >> elimDigit & 1) == 0)
					{
						// No eliminations found.
						continue;
					}

					var (candidateOffsets, cellOffsets) = (new List<CandidateViewNode>(), new List<CellViewNode>());
					foreach (var cell in urCells)
					{
						switch (grid.GetState(cell))
						{
							case CellState.Empty:
							{
								foreach (var digit in (Mask)(grid.GetCandidates(cell) & comparer))
								{
									candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
								}
								break;
							}
							case CellState.Modifiable:
							{
								cellOffsets.Add(new(ColorIdentifier.Normal, cell));
								break;
							}
						}
					}
					foreach (var cell in guardianCells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							if (digit == d1 || digit == d2)
							{
								candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
							}
						}
					}

					var cellPair = cell1.AsCellMap() + cell2;
					foreach (var cell in cellPair)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							// Elimination cannot be colorized.
							if (cell != cell1 || digit != elimDigit)
							{
								candidateOffsets.Add(
									new(
										digit != d1 && digit != d2 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Auxiliary2,
										cell * 9 + digit
									)
								);
							}
						}
					}

					if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
					{
						return;
					}

					accumulator.Add(
						new UniqueRectangleExternalXyWingStep(
							new SingletonArray<Conclusion>(new(Elimination, cell1, elimDigit)),
							[[.. cellOffsets, .. candidateOffsets]],
							context.Options,
							d1,
							d2,
							cells,
							guardianCells,
							cellPair,
							isIncomplete,
							arMode,
							index
						)
					);
				}
			}
		}
#pragma warning restore CS8321

		void forBothExoLeaves(
			in Grid grid,
			ref StepAnalysisContext context,
			in CellMap cellsToEnumerate,
			in CellMap guardianCells,
			House[] houseCombination
		)
		{
			foreach (ref readonly var cellPair in cellsToEnumerate & 2)
			{
				var (cell1, cell2) = (cellPair[0], cellPair[1]);
				var (mask1, mask2) = (grid.GetCandidates(cell1), grid.GetCandidates(cell2));
				var intersectionMask = (Mask)(mask1 & mask2);
				if (!BitOperations.IsPow2(intersectionMask))
				{
					// No eliminations can be found in this pattern.
					continue;
				}

				var unionMask = (Mask)(mask1 | mask2);
				if ((unionMask & comparer) != comparer)
				{
					// The two cells must contain both two digits.
					continue;
				}

				if ((unionMask & ~(comparer | intersectionMask)) != 0)
				{
					// These two cells may contain extra cells, which is disallowed in this pattern.
					continue;
				}

				var cell1UrDigit = BitOperations.TrailingZeroCount((Mask)(mask1 & ~intersectionMask));
				var cell2UrDigit = BitOperations.TrailingZeroCount((Mask)(mask2 & ~intersectionMask));
				var guardianCellsThatContainsDigit1 = guardianCells & CandidatesMap[cell1UrDigit];
				var guardianCellsThatContainsDigit2 = guardianCells & CandidatesMap[cell2UrDigit];
				if ((PeersMap[cell1] & guardianCellsThatContainsDigit1) != guardianCellsThatContainsDigit1
					|| (PeersMap[cell2] & guardianCellsThatContainsDigit2) != guardianCellsThatContainsDigit2)
				{
					// Two cells must see all guardian cells.
					continue;
				}

				// UR External XY-Wing found. Now check for eliminations.
				var elimDigit = BitOperations.TrailingZeroCount(intersectionMask);
				var elimMap = cellPair.PeerIntersection & CandidatesMap[elimDigit];
				if (!elimMap)
				{
					// No elimination cell.
					continue;
				}

				var (candidateOffsets, cellOffsets) = (new List<CandidateViewNode>(), new List<CellViewNode>());
				foreach (var cell in urCells)
				{
					switch (grid.GetState(cell))
					{
						case CellState.Empty:
						{
							foreach (var digit in (Mask)(grid.GetCandidates(cell) & comparer))
							{
								candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
							}
							break;
						}
						case CellState.Modifiable:
						{
							cellOffsets.Add(new(ColorIdentifier.Normal, cell));
							break;
						}
					}
				}
				foreach (var cell in guardianCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if (digit == d1 || digit == d2)
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
						}
					}
				}
				foreach (var cell in cellPair)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								digit != d1 && digit != d2 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Auxiliary2,
								cell * 9 + digit
							)
						);
					}
				}

				if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
				{
					return;
				}

				accumulator.Add(
					new UniqueRectangleExternalXyWingStep(
						(from cell in elimMap select new Conclusion(Elimination, cell, elimDigit)).ToArray(),
						[
							[
								.. cellOffsets,
								.. candidateOffsets,
								new HouseViewNode(ColorIdentifier.Normal, houseCombination[0]),
								new HouseViewNode(ColorIdentifier.Normal, houseCombination[1])
							]
						],
						context.Options,
						d1,
						d2,
						cells,
						guardianCells,
						cellPair,
						isIncomplete,
						arMode,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external ALS-XZ rule.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="alses">The ALS patterns found.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The mask index.</param>
	/// <param name="arMode"></param>
	private partial void CheckExternalAlmostLockedSetsXz(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, scoped ReadOnlySpan<AlmostLockedSetPattern> alses, Mask comparer, Digit d1, Digit d2, int index, bool arMode)
	{
		var cells = urCells.AsCellMap();
		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.AllSets.GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap & ~cells & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (guardianCells is { Count: not 1, SharedHouses: 0 })
			{
				// All guardian cells must lie in one house.
				continue;
			}

			if (!(guardianCells & CandidatesMap[d1]) || !(guardianCells & CandidatesMap[d2]))
			{
				// Guardian cells must contain both two digits; otherwise, skip the current case.
				continue;
			}

			foreach (var (zDigit, xDigit) in ((d1, d2), (d2, d1)))
			{
				var xDigitGuardianCells = CandidatesMap[xDigit] & guardianCells;
				var zDigitGuardianCells = CandidatesMap[zDigit] & guardianCells;
				foreach (var als in alses)
				{
					var alsHouse = als.House;
					var alsMask = als.DigitsMask;
					var alsMap = als.Cells;
					if ((cells.Houses >> alsHouse & 1) != 0)
					{
						// The current ALS cannot lie in the house that UR cells covered.
						continue;
					}

					if ((alsMask >> d1 & 1) == 0 || (alsMask >> d2 & 1) == 0)
					{
						// The ALS must uses both two digits.
						continue;
					}

					if (!!(alsMap & xDigitGuardianCells) || zDigitGuardianCells == (alsMap & CandidatesMap[zDigit]))
					{
						// The ALS cannot only use X or Z digits that all appears in guardian cells.
						continue;
					}

					var elimMap = (alsMap | zDigitGuardianCells) % CandidatesMap[zDigit];
					if (!elimMap)
					{
						// No eliminations found.
						continue;
					}

					var xDigitMap = (alsMap | xDigitGuardianCells) & CandidatesMap[xDigit];
					if (xDigitMap.FirstSharedHouse == 32)
					{
						// The X digit must be connected.
						continue;
					}

					// ALS-XZ formed.
					var candidateOffsets = new List<CandidateViewNode>();
					var cellOffsets = new List<CellViewNode>();
					foreach (var cell in urCells)
					{
						switch (grid.GetState(cell))
						{
							case CellState.Empty:
							{
								foreach (var digit in comparer)
								{
									if (CandidatesMap[digit].Contains(cell))
									{
										candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
									}
								}
								break;
							}
							case CellState.Modifiable:
							{
								cellOffsets.Add(new(ColorIdentifier.Normal, cell));
								break;
							}
						}
					}
					foreach (var cell in xDigitGuardianCells)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + xDigit));
					}
					foreach (var cell in zDigitGuardianCells)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + zDigit));
					}
					foreach (var cell in alsMap)
					{
						cellOffsets.Add(new(ColorIdentifier.AlmostLockedSet1, cell));

						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(
								new(
									digit == d1 || digit == d2 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.AlmostLockedSet1,
									cell * 9 + digit
								)
							);
						}
					}

					if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out var isIncomplete))
					{
						return;
					}

					accumulator.Add(
						new UniqueRectangleExternalAlmostLockedSetsXzStep(
							(from cell in elimMap select new Conclusion(Elimination, cell, zDigit)).ToArray(),
							[
								[
									.. candidateOffsets,
									.. cellOffsets,
									new HouseViewNode(ColorIdentifier.Normal, houseCombination[0]),
									new HouseViewNode(ColorIdentifier.Normal, houseCombination[1])
								]
							],
							context.Options,
							d1,
							d2,
							cells,
							guardianCells,
							als,
							isIncomplete,
							arMode,
							index
						)
					);
				}
			}
		}
	}
}
