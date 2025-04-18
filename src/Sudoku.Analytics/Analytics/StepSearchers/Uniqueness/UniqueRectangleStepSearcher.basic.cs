namespace Sudoku.Analytics.StepSearchers.Uniqueness;

public partial class UniqueRectangleStepSearcher
{
	/// <summary>
	/// Check type 1.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The pattern:
	/// <code><![CDATA[
	///   ↓ cornerCell
	/// (abc) ab
	///  ab   ab
	/// ]]></code>
	/// </remarks>
	private partial void CheckType1(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		// Get the summary mask.
		var mask = grid[otherCellsMap];
		if (mask != comparer)
		{
			return;
		}

		// Type 1 found. Now check elimination.
		var d1Exists = CandidatesMap[d1].Contains(cornerCell);
		var d2Exists = CandidatesMap[d2].Contains(cornerCell);
		if (!d1Exists && !d2Exists)
		{
			return;
		}

		var conclusions = new List<Conclusion>(2);
		if (d1Exists)
		{
			conclusions.Add(new(Elimination, cornerCell, d1));
		}
		if (d2Exists)
		{
			conclusions.Add(new(Elimination, cornerCell, d2));
		}
		if (conclusions.Count == 0)
		{
			return;
		}

		var candidateOffsets = new List<CandidateViewNode>(6);
		foreach (var cell in otherCellsMap)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
			}
		}

		if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Count) != (6, 2))
		{
			return;
		}

		accumulator.Add(
			new UniqueRectangleType1Step(
				conclusions.AsMemory(),
				[[.. arMode ? GetHighlightCells(urCells) : [], .. arMode ? [] : candidateOffsets]],
				context.Options,
				d1,
				d2,
				[.. urCells],
				arMode,
				index
			)
		);
	}

	/// <summary>
	/// Check type 2.
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
	///   ↓ corner1, corner2
	/// (abc) (abc)
	///  ab    ab
	/// ]]></code>
	/// </remarks>
	private partial void CheckType2(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		// Get the summary mask.
		var mask = grid[otherCellsMap];
		if (mask != comparer)
		{
			return;
		}

		// Gets the extra mask.
		// If the mask is the power of 2, the type 2 will be formed.
		var extraMask = (Mask)((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) ^ comparer);
		if (extraMask == 0 || (extraMask & extraMask - 1) != 0)
		{
			return;
		}

		// Type 2 or 5 found. Now check elimination.
		var extraDigit = Mask.TrailingZeroCount(extraMask);
		var elimMap = (corner1.AsCellMap() + corner2).PeerIntersection & CandidatesMap[extraDigit];
		if (!elimMap)
		{
			return;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		var extraCells = CellMap.Empty;
		foreach (var cell in urCells)
		{
			if (grid.GetState(cell) == CellState.Empty)
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					candidateOffsets.Add(
						new(
							digit == extraDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
							cell * 9 + digit
						)
					);
				}

				if (CandidatesMap[extraDigit].Contains(cell))
				{
					extraCells.Add(cell);
				}
			}
		}

		if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out _))
		{
			return;
		}

		var isType5 = (corner1.AsCellMap() + corner2).FirstSharedHouse == 32;
		accumulator.Add(
			new UniqueRectangleType2Or5Step(
				(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
				[[.. arMode ? GetHighlightCells(urCells) : [], .. candidateOffsets]],
				context.Options,
				d1,
				d2,
				(arMode, isType5) switch
				{
					(true, true) => Technique.AvoidableRectangleType5,
					(true, false) => Technique.AvoidableRectangleType2,
					(false, true) => Technique.UniqueRectangleType5,
					_ => Technique.UniqueRectangleType2
				},
				[.. urCells],
				arMode,
				extraDigit,
				index
			)
		);
	}

	/// <summary>
	/// Check type 3.
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
	/// <param name="otherCellsMap">
	/// The map of other cells during the current UR searching.
	/// </param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The pattern:
	/// <code><![CDATA[
	///  ↓ corner1, corner2
	/// (ab ) (ab )
	///  abx   aby
	/// ]]></code>
	/// </remarks>
	private partial void CheckType3(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		var notSatisfiedType3 = false;
		foreach (var cell in otherCellsMap)
		{
			var currentMask = grid.GetCandidates(cell);
			if ((currentMask & comparer) == 0 // The current cell does not contain a valid digit appeared in UR.
				|| currentMask == comparer // The current cell contains both digits appeared in UR.
				|| !arMode && grid.GetState(cell) != CellState.Empty) // The current cell is not empty.
			{
				notSatisfiedType3 = true;
				break;
			}
		}
		if (notSatisfiedType3)
		{
			return;
		}

		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		var mask = grid[otherCellsMap];
		if ((mask & comparer) != comparer)
		{
			return;
		}

		var otherDigitsMask = (Mask)(mask ^ comparer);
		foreach (var houseIndex in otherCellsMap.SharedHouses)
		{
			if ((ValuesMap[d1] || ValuesMap[d2]) && HousesMap[houseIndex])
			{
				return;
			}

			var iterationMap = HousesMap[houseIndex] & EmptyCells & ~otherCellsMap;
			for (var size = BitOperations.PopCount(otherDigitsMask) - 1; size < iterationMap.Count; size++)
			{
				foreach (ref readonly var iteratedCells in iterationMap & size)
				{
					var tempMask = grid[iteratedCells];
					if ((tempMask & comparer) != 0 || BitOperations.PopCount(tempMask) - 1 != size || (tempMask & otherDigitsMask) != otherDigitsMask)
					{
						continue;
					}

					var conclusions = new List<Conclusion>(16);
					foreach (var digit in tempMask)
					{
						foreach (var cell in iterationMap & ~iteratedCells & CandidatesMap[digit])
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var cellOffsets = new List<CellViewNode>();
					foreach (var cell in urCells)
					{
						if (grid.GetState(cell) != CellState.Empty)
						{
							cellOffsets.Add(new(ColorIdentifier.Normal, cell));
						}
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var cell in urCells)
					{
						if (grid.GetState(cell) == CellState.Empty)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(
									new(
										(tempMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
										cell * 9 + digit
									)
								);
							}
						}
					}
					foreach (var cell in iteratedCells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
						}
					}

					accumulator.Add(
						new UniqueRectangleType3Step(
							conclusions.AsMemory(),
							[[.. arMode ? cellOffsets : [], .. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, houseIndex)]],
							context.Options,
							d1,
							d2,
							[.. urCells],
							iteratedCells,
							otherDigitsMask,
							houseIndex,
							arMode,
							index
						)
					);
				}
			}
		}
	}

	/// <summary>
	/// Check type 4.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">
	/// The map of other cells during the current UR searching.
	/// </param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The pattern:
	/// <code><![CDATA[
	///  ↓ corner1, corner2
	/// (ab ) ab
	///  abx  aby
	/// ]]></code>
	/// </remarks>
	private partial void CheckType4(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		var house = otherCellsMap.FirstSharedHouse;
		foreach (var digit in (d1, d2))
		{
			if (!IsConjugatePair(digit, otherCellsMap, house))
			{
				continue;
			}

			// Yes, Type 4 found.
			// Now check elimination.
			var elimDigit = Mask.TrailingZeroCount((Mask)(comparer ^ (1 << digit)));
			if ((otherCellsMap & CandidatesMap[elimDigit]) is not (var elimMap and not []))
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>(6);
			foreach (var cell in urCells)
			{
				if (grid.GetState(cell) != CellState.Empty)
				{
					continue;
				}

				if (otherCellsMap.Contains(cell))
				{
					if (d1 != elimDigit && CandidatesMap[d1].Contains(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + d1));
					}
					if (d2 != elimDigit && CandidatesMap[d2].Contains(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + d2));
					}
				}
				else
				{
					// Corner1 and corner2.
					foreach (var d in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d));
					}
				}
			}

			var conclusions = from cell in elimMap select new Conclusion(Elimination, cell, elimDigit);
			if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Length) != (6, 2))
			{
				continue;
			}

			accumulator.Add(
				new UniqueRectangleConjugatePairStep(
					conclusions.ToArray(),
					[
						[
							.. candidateOffsets,
							new ConjugateLinkViewNode(ColorIdentifier.Normal, otherCellsMap[0], otherCellsMap[1], digit)
						]
					],
					context.Options,
					Technique.UniqueRectangleType4,
					d1,
					d2,
					[.. urCells],
					false,
					[new(otherCellsMap[0], otherCellsMap[1], digit)],
					index
				)
			);
		}
	}

	/// <summary>
	/// Check type 5.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The pattern:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab ) abc
	///  abc  abc
	/// ]]></code>
	/// </remarks>
	private partial void CheckType5(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		// Get the summary mask.
		var otherCellsMask = grid[otherCellsMap];

		// Degenerate to type 1.
		var extraMask = (Mask)(otherCellsMask ^ comparer);
		if ((extraMask & extraMask - 1) != 0 || extraMask == 0)
		{
			return;
		}

		// Type 5 found. Now check elimination.
		var extraDigit = Mask.TrailingZeroCount(extraMask);
		var cellsThatContainsExtraDigit = otherCellsMap & CandidatesMap[extraDigit];

		// Degenerate to type 1.
		if (cellsThatContainsExtraDigit.Count == 1)
		{
			return;
		}

		if ((cellsThatContainsExtraDigit.PeerIntersection & CandidatesMap[extraDigit]) is not (var elimMap and not []))
		{
			return;
		}

		var candidateOffsets = new List<CandidateViewNode>(16);
		var extraCells = CellMap.Empty;
		foreach (var cell in urCells)
		{
			if (grid.GetState(cell) != CellState.Empty)
			{
				continue;
			}

			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(digit == extraDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell * 9 + digit));
			}

			if (CandidatesMap[extraDigit].Contains(cell))
			{
				extraCells.Add(cell);
			}
		}

		if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out _))
		{
			return;
		}

		accumulator.Add(
			new UniqueRectangleType2Or5Step(
				(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
				[[.. arMode ? GetHighlightCells(urCells) : [], .. candidateOffsets]],
				context.Options,
				d1,
				d2,
				arMode ? Technique.AvoidableRectangleType5 : Technique.UniqueRectangleType5,
				[.. urCells],
				arMode,
				extraDigit,
				index
			)
		);
	}

	/// <summary>
	/// Check type 6.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
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
	///  ↓ corner1
	/// (ab )  aby
	///  abx  (ab)
	///        ↑corner2
	/// ]]></code>
	/// </remarks>
	private partial void CheckType6(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		var o1 = otherCellsMap[0];
		var o2 = otherCellsMap[1];
		var r1 = corner1.ToHouse(HouseType.Row);
		var c1 = corner1.ToHouse(HouseType.Column);
		var r2 = corner2.ToHouse(HouseType.Row);
		var c2 = corner2.ToHouse(HouseType.Column);
		foreach (var digit in (d1, d2))
		{
			foreach (var (h1, h2) in ((r1, r2), (c1, c2)))
			{
				collectCore(grid, ref context, otherCellsMap, h1 is >= 9 and < 18, digit, h1, h2);
			}
		}


		void collectCore(
			in Grid grid,
			ref StepAnalysisContext context,
			in CellMap otherCellsMap,
			bool isRow,
			Digit digit,
			House house1,
			House house2
		)
		{
			var precheck = isRow && IsConjugatePair(digit, [corner1, o1], house1) && IsConjugatePair(digit, [corner2, o2], house2)
				|| !isRow && IsConjugatePair(digit, [corner1, o2], house1) && IsConjugatePair(digit, [corner2, o1], house2);
			if (!precheck)
			{
				return;
			}

			// Check eliminations.
			if ((otherCellsMap & CandidatesMap[digit]) is not (var elimMap and not []))
			{
				return;
			}

			var candidateOffsets = new List<CandidateViewNode>(6);
			foreach (var cell in urCells)
			{
				if (otherCellsMap.Contains(cell))
				{
					if (d1 != digit && CandidatesMap[d1].Contains(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d1));
					}
					if (d2 != digit && CandidatesMap[d2].Contains(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d2));
					}
				}
				else
				{
					foreach (var d in grid.GetCandidates(cell))
					{
						var colorIdentifier = d == digit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal;
						candidateOffsets.Add(new(colorIdentifier, cell * 9 + d));
					}
				}
			}

			var conclusions = from cell in elimMap select new Conclusion(Elimination, cell, digit);
			if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Length) != (6, 2))
			{
				return;
			}

			accumulator.Add(
				new UniqueRectangleConjugatePairStep(
					conclusions.ToArray(),
					[
						[
							.. candidateOffsets,
							new ConjugateLinkViewNode(ColorIdentifier.Normal, corner1, isRow ? o1 : o2, digit),
							new ConjugateLinkViewNode(ColorIdentifier.Normal, corner2, isRow ? o2 : o1, digit),
						]
					],
					context.Options,
					Technique.UniqueRectangleType6,
					d1,
					d2,
					[.. urCells],
					false,
					[new(corner1, isRow ? o1 : o2, digit), new(corner2, isRow ? o2 : o1, digit)],
					index
				)
			);
		}
	}

	/// <summary>
	/// Check hidden UR.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The pattern:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab ) abx
	///  aby  abz
	/// ]]></code>
	/// </remarks>
	private partial void CheckHidden(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		var cells = urCells.AsCellMap();
		if (!arMode && grid.GetCandidates(cornerCell) != comparer || arMode && (EmptyCells & cells) != otherCellsMap)
		{
			return;
		}

		var abzCell = GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		foreach (var digit in (d1, d2))
		{
			var map1 = abzCell.AsCellMap() + abxCell;
			var map2 = abzCell.AsCellMap() + abyCell;
			if (map1.SharedLine is not (var m1cl and not 32) || map2.SharedLine is not (var m2cl and not 32))
			{
				// There's no common covered line to display.
				continue;
			}

			if (!IsConjugatePair(digit, map1, m1cl) || !IsConjugatePair(digit, map2, m2cl))
			{
				continue;
			}

			// Determine whether the Hidden ARs don't use unrelated digits.
			if (arMode && ((1 << grid.GetDigit(cornerCell)) | 1 << digit) != comparer)
			{
				continue;
			}

			// Hidden UR/AR found. Now check eliminations.
			var elimDigit = Mask.TrailingZeroCount((Mask)(comparer ^ (1 << digit)));
			if (!CandidatesMap[elimDigit].Contains(abzCell))
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in urCells)
			{
				if (grid.GetState(cell) != CellState.Empty)
				{
					continue;
				}

				if (otherCellsMap.Contains(cell))
				{
					if ((cell != abzCell || d1 != elimDigit) && CandidatesMap[d1].Contains(cell))
					{
						candidateOffsets.Add(new(d1 != elimDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell * 9 + d1));
					}
					if ((cell != abzCell || d2 != elimDigit) && CandidatesMap[d2].Contains(cell))
					{
						candidateOffsets.Add(new(d2 != elimDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell * 9 + d2));
					}
				}
				else
				{
					foreach (var d in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + d));
					}
				}
			}

			if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
			{
				continue;
			}

			accumulator.Add(
				new HiddenUniqueRectangleStep(
					new SingletonArray<Conclusion>(new(Elimination, abzCell, elimDigit)),
					[
						[
							.. arMode ? GetHighlightCells(urCells) : [],
							.. candidateOffsets,
							new ConjugateLinkViewNode(ColorIdentifier.Normal, abzCell, abxCell, digit),
							new ConjugateLinkViewNode(ColorIdentifier.Normal, abzCell, abyCell, digit)
						]
					],
					context.Options,
					d1,
					d2,
					[.. urCells],
					arMode,
					[new(abzCell, abxCell, digit), new(abzCell, abyCell, digit)],
					index
				)
			);
		}
	}
}
