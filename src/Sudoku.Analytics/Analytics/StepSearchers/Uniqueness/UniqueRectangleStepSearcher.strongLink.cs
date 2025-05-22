namespace Sudoku.Analytics.StepSearchers;

public partial class UniqueRectangleStepSearcher
{
	/// <summary>
	/// Check UR + 2B/1SL.
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
	/// (ab )  (ab )
	///  |
	///  | a
	///  |
	///  abx    aby
	/// ]]></code>
	/// </remarks>
	private partial void Check2B1SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		ReadOnlySpan<Digit> digits = [d1, d2];
		foreach (var cell in (corner1, corner2))
		{
			foreach (var otherCell in otherCellsMap)
			{
				if (!IsSameHouseCell(cell, otherCell, out var houses))
				{
					continue;
				}

				foreach (var house in houses)
				{
					if (house < 9)
					{
						continue;
					}

					for (var digitIndex = 0; digitIndex < 2; digitIndex++)
					{
						var digit = digits[digitIndex];
						if (!IsConjugatePair(digit, cell.AsCellMap() + otherCell, house))
						{
							continue;
						}

						var elimCell = (otherCellsMap - otherCell)[0];
						if (!CandidatesMap[digit].Contains(otherCell))
						{
							continue;
						}

						var elimDigit = BitOperations.TrailingZeroCount((Mask)(comparer ^ (1 << digit)));
						var conclusions = new List<Conclusion>(4);
						if (CandidatesMap[elimDigit].Contains(elimCell))
						{
							conclusions.Add(new(Elimination, elimCell, elimDigit));
						}
						if (conclusions.Count == 0)
						{
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>(10);
						foreach (var urCell in urCells)
						{
							if (urCell == corner1 || urCell == corner2)
							{
								var coveredHouses = (urCell.AsCellMap() + otherCell).SharedHouses;
								if ((coveredHouses >> house & 1) != 0)
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(
											new(
												d == digit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
												urCell * 9 + d
											)
										);
									}
								}
								else
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(new(ColorIdentifier.Normal, urCell * 9 + d));
									}
								}
							}
							else if (urCell == otherCell || urCell == elimCell)
							{
								if (CandidatesMap[d1].Contains(urCell))
								{
									if (urCell != elimCell || d1 != elimDigit)
									{
										candidateOffsets.Add(
											new(
												urCell == elimCell
													? ColorIdentifier.Normal
													: d1 == digit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
												urCell * 9 + d1
											)
										);
									}
								}
								if (CandidatesMap[d2].Contains(urCell))
								{
									if (urCell != elimCell || d2 != elimDigit)
									{
										candidateOffsets.Add(
											new(
												urCell == elimCell
													? ColorIdentifier.Normal
													: d2 == digit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
												urCell * 9 + d2
											)
										);
									}
								}
							}
						}

						if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
						{
							continue;
						}

						accumulator.Add(
							new UniqueRectangleConjugatePairStep(
								conclusions.AsMemory(),
								[
									[
										.. arMode ? GetHighlightCells(urCells) : [],
										.. candidateOffsets,
										new ConjugateLinkViewNode(ColorIdentifier.Normal, cell, otherCell, digit)
									]
								],
								context.Options,
								Technique.UniqueRectangle2B1,
								d1,
								d2,
								[.. urCells],
								arMode,
								[new(cell, otherCell, digit)],
								index
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR + 2D/1SL.
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
	///   ↓ corner1
	/// (ab )   aby
	///  |
	///  | a
	///  |
	///  abx   (ab )
	///          ↑ corner2
	/// ]]></code>
	/// </remarks>
	private partial void Check2D1SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		foreach (var cell in (corner1, corner2))
		{
			foreach (var otherCell in otherCellsMap)
			{
				if (!IsSameHouseCell(cell, otherCell, out var houses))
				{
					continue;
				}

				foreach (var house in houses)
				{
					if (house < 9)
					{
						continue;
					}

					foreach (var digit in (d1, d2))
					{
						if (!IsConjugatePair(digit, [cell, otherCell], house))
						{
							continue;
						}

						var elimCell = (otherCellsMap - otherCell)[0];
						if (!CandidatesMap[digit].Contains(otherCell))
						{
							continue;
						}

						var conclusions = new List<Conclusion>(4);
						if (CandidatesMap[digit].Contains(elimCell))
						{
							conclusions.Add(new(Elimination, elimCell, digit));
						}
						if (conclusions.Count == 0)
						{
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>(10);
						foreach (var urCell in urCells)
						{
							if (urCell == corner1 || urCell == corner2)
							{
								var flag = false;
								foreach (var r in (urCell.AsCellMap() + otherCell).SharedHouses)
								{
									if (r == house)
									{
										flag = true;
										break;
									}
								}

								if (flag)
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(
											new(
												d == digit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
												urCell * 9 + d
											)
										);
									}
								}
								else
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(new(ColorIdentifier.Normal, urCell * 9 + d));
									}
								}
							}
							else if (urCell == otherCell || urCell == elimCell)
							{
								if (CandidatesMap[d1].Contains(urCell) && (urCell != elimCell || d1 != digit))
								{
									candidateOffsets.Add(
										new(
											urCell == elimCell
												? ColorIdentifier.Normal
												: d1 == digit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
											urCell * 9 + d1
										)
									);
								}
								if (CandidatesMap[d2].Contains(urCell) && (urCell != elimCell || d2 != digit))
								{
									candidateOffsets.Add(
										new(
											urCell == elimCell
												? ColorIdentifier.Normal
												: d2 == digit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
											urCell * 9 + d2
										)
									);
								}
							}
						}

						if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
						{
							continue;
						}

						accumulator.Add(
							new UniqueRectangleConjugatePairStep(
								conclusions.AsMemory(),
								[
									[
										.. arMode ? GetHighlightCells(urCells) : [],
										.. candidateOffsets,
										new ConjugateLinkViewNode(ColorIdentifier.Normal, cell, otherCell, digit)
									]
								],
								context.Options,
								Technique.UniqueRectangle2D1,
								d1,
								d2,
								[.. urCells],
								arMode,
								[new(cell, otherCell, digit)],
								index
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR + 3X/2SL.
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
	/// (ab )    abx
	///           |
	///           | b
	///       a   |
	///  aby-----abz
	/// ]]></code>
	/// </remarks>
	private partial void Check3X2SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var abzCell = GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		foreach (var (a, b) in ((d1, d2), (d2, d1)))
		{
			var abxCell = adjacentCellsMap[0];
			var abyCell = adjacentCellsMap[1];
			var map1 = abzCell.AsCellMap() + abxCell;
			var map2 = abzCell.AsCellMap() + abyCell;
			if (!IsConjugatePair(b, map1, map1.SharedLine) || !IsConjugatePair(a, map2, map2.SharedLine))
			{
				continue;
			}

			var conclusions = new List<Conclusion>(2);
			if (CandidatesMap[a].Contains(abxCell))
			{
				conclusions.Add(new(Elimination, abxCell, a));
			}
			if (CandidatesMap[b].Contains(abyCell))
			{
				conclusions.Add(new(Elimination, abyCell, b));
			}
			if (conclusions.Count == 0)
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>(6);
			foreach (var digit in grid.GetCandidates(abxCell))
			{
				if ((digit == d1 || digit == d2) && digit != a)
				{
					candidateOffsets.Add(new(digit == b ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, abxCell * 9 + digit));
				}
			}
			foreach (var digit in grid.GetCandidates(abyCell))
			{
				if ((digit == d1 || digit == d2) && digit != b)
				{
					candidateOffsets.Add(new(digit == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, abyCell * 9 + digit));
				}
			}
			foreach (var digit in grid.GetCandidates(abzCell))
			{
				if (digit == a || digit == b)
				{
					candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, abzCell * 9 + digit));
				}
			}
			foreach (var digit in comparer)
			{
				candidateOffsets.Add(new(ColorIdentifier.Normal, cornerCell * 9 + digit));
			}
			if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 6)
			{
				continue;
			}

			accumulator.Add(
				new UniqueRectangleConjugatePairStep(
					conclusions.AsMemory(),
					[
						[
							.. arMode ? GetHighlightCells(urCells) : [],
							.. candidateOffsets,
							new ConjugateLinkViewNode(ColorIdentifier.Normal, abxCell, abzCell, b),
							new ConjugateLinkViewNode(ColorIdentifier.Normal, abyCell, abzCell, a)
						]
					],
					context.Options,
					Technique.UniqueRectangle3X2,
					d1,
					d2,
					[.. urCells],
					arMode,
					[new(abxCell, abzCell, b), new(abyCell, abzCell, a)],
					index
				)
			);
		}
	}

	/// <summary>
	/// Check UR + 3N/2SL.
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
	/// (ab )-----abx
	///        a   |
	///            | b
	///            |
	///  aby      abz
	/// ]]></code>
	/// </remarks>
	private partial void Check3N2SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		// Step 1: Get the diagonal cell of 'cornerCell' and determine the existence of strong link.
		var abzCell = GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		ReadOnlySpan<(Digit, Digit)> digitPairs = [(d1, d2), (d2, d1)];
		ReadOnlySpan<Digit> digits = [d1, d2];
		foreach (var (begin, end) in ((abxCell, abyCell), (abyCell, abxCell)))
		{
			var linkMap = begin.AsCellMap() + abzCell;
			foreach (var (a, b) in digitPairs)
			{
				if (!IsConjugatePair(b, linkMap, linkMap.SharedLine))
				{
					continue;
				}

				// Step 2: Get the link cell that is adjacent to 'cornerCell' and check the strong link.
				var secondLinkMap = cornerCell.AsCellMap() + begin;
				if (!IsConjugatePair(a, secondLinkMap, secondLinkMap.SharedLine))
				{
					continue;
				}

				// Step 3: Check eliminations.
				if (!CandidatesMap[a].Contains(end))
				{
					continue;
				}

				// Step 4: Check highlight candidates.
				var candidateOffsets = new List<CandidateViewNode>(7);
				foreach (var d in comparer)
				{
					candidateOffsets.Add(new(d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cornerCell * 9 + d));
				}
				foreach (var d in digits)
				{
					if (CandidatesMap[d].Contains(abzCell))
					{
						candidateOffsets.Add(new(d == b ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, abzCell * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if ((d == d1 || d == d2) && d != a)
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, end * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
				{
					continue;
				}

				var conjugatePairs = (Conjugate[])[new(cornerCell, begin, a), new(begin, abzCell, b)];
				accumulator.Add(
					new UniqueRectangleConjugatePairStep(
						new SingletonArray<Conclusion>(new(Elimination, end, a)),
						[
							[
								.. arMode ? GetHighlightCells(urCells) : [],
								.. candidateOffsets,
								new ConjugateLinkViewNode(ColorIdentifier.Normal, cornerCell, begin, a),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, begin, abzCell, b)
							]
						],
						context.Options,
						Technique.UniqueRectangle3N2,
						d1,
						d2,
						[.. urCells],
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 3U/2SL.
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
	/// (ab )-----abx
	///        a
	///
	///        b
	///  aby -----abz
	/// ]]></code>
	/// </remarks>
	private partial void Check3U2SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var abzCell = GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		foreach (var (begin, end) in ((abxCell, abyCell), (abyCell, abxCell)))
		{
			var linkMap = begin.AsCellMap() + abzCell;
			foreach (var (a, b) in ((d1, d2), (d2, d1)))
			{
				if (!IsConjugatePair(b, linkMap, linkMap.SharedLine))
				{
					continue;
				}

				var secondLinkMap = cornerCell.AsCellMap() + end;
				if (!IsConjugatePair(a, secondLinkMap, secondLinkMap.SharedLine))
				{
					continue;
				}

				if (!CandidatesMap[a].Contains(begin))
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(7);
				foreach (var d in comparer)
				{
					candidateOffsets.Add(new(d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cornerCell * 9 + d));
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if ((d == d1 || d == d2) && d != a)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, end * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(abzCell))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == b ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, abzCell * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
				{
					continue;
				}

				var conjugatePairs = (Conjugate[])[new(cornerCell, end, a), new(begin, abzCell, b)];
				accumulator.Add(
					new UniqueRectangleConjugatePairStep(
						new SingletonArray<Conclusion>(new(Elimination, begin, a)),
						[
							[
								.. arMode ? GetHighlightCells(urCells) : [],
								.. candidateOffsets,
								new ConjugateLinkViewNode(ColorIdentifier.Normal, cornerCell, end, a),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, begin, abzCell, b)
							]
						],
						context.Options,
						Technique.UniqueRectangle3U2,
						d1,
						d2,
						[.. urCells],
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 3E/2SL.
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
	/// (ab )-----abx
	///        a
	///
	///        a
	///  aby -----abz
	/// ]]></code>
	/// </remarks>
	private partial void Check3E2SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var abzCell = GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		foreach (var (begin, end) in ((abxCell, abyCell), (abyCell, abxCell)))
		{
			var linkMap = begin.AsCellMap() + abzCell;
			foreach (var (a, b) in ((d1, d2), (d2, d1)))
			{
				if (!IsConjugatePair(a, linkMap, linkMap.SharedLine))
				{
					continue;
				}

				var secondLinkMap = cornerCell.AsCellMap() + end;
				if (!IsConjugatePair(a, secondLinkMap, secondLinkMap.SharedLine))
				{
					continue;
				}

				if (!CandidatesMap[b].Contains(abzCell))
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(7);
				foreach (var d in comparer)
				{
					candidateOffsets.Add(new(d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cornerCell * 9 + d));
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, end * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(abzCell))
				{
					if ((d == d1 || d == d2) && d != b)
					{
						candidateOffsets.Add(new(d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, abzCell * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
				{
					continue;
				}

				var conjugatePairs = (Conjugate[])[new(cornerCell, end, a), new(begin, abzCell, a)];
				accumulator.Add(
					new UniqueRectangleConjugatePairStep(
						new SingletonArray<Conclusion>(new(Elimination, abzCell, b)),
						[
							[
								.. arMode ? GetHighlightCells(urCells) : [],
								.. candidateOffsets,
								new ConjugateLinkViewNode(ColorIdentifier.Normal, cornerCell, end, a),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, begin, abzCell, a)
							]
						],
						context.Options,
						Technique.UniqueRectangle3E2,
						d1,
						d2,
						[.. urCells],
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 4X/3SL.
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
	/// (abx)-----(aby)
	///        a    |
	///             | b
	///        a    |
	///  abz ----- abw
	/// ]]></code>
	/// </remarks>
	private partial void Check4X3SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		var link1Map = corner1.AsCellMap() + corner2;
		foreach (var (a, b) in ((d1, d2), (d2, d1)))
		{
			if (!IsConjugatePair(a, link1Map, link1Map.SharedLine))
			{
				continue;
			}

			var abwCell = GetDiagonalCell(urCells, corner1);
			var abzCell = (otherCellsMap - abwCell)[0];
			foreach (var (head, begin, end, extra) in ((corner2, corner1, abzCell, abwCell), (corner1, corner2, abwCell, abzCell)))
			{
				var link2Map = begin.AsCellMap() + end;
				if (!IsConjugatePair(b, link2Map, link2Map.SharedLine))
				{
					continue;
				}

				var link3Map = end.AsCellMap() + extra;
				if (!IsConjugatePair(a, link3Map, link3Map.SharedLine))
				{
					continue;
				}

				var conclusions = new List<Conclusion>(2);
				if (CandidatesMap[b].Contains(head))
				{
					conclusions.Add(new(Elimination, head, b));
				}
				if (CandidatesMap[b].Contains(extra))
				{
					conclusions.Add(new(Elimination, extra, b));
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(6);
				foreach (var d in grid.GetCandidates(head))
				{
					if ((d == d1 || d == d2) && d != b)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, head * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(extra))
				{
					if ((d == d1 || d == d2) && d != b)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, extra * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, end * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Count) != (6, 2))
				{
					continue;
				}

				var conjugatePairs = (Conjugate[])[new(head, begin, a), new(begin, end, b), new(end, extra, a)];
				accumulator.Add(
					new UniqueRectangleConjugatePairStep(
						conclusions.AsMemory(),
						[
							[
								.. arMode ? GetHighlightCells(urCells) : [],
								.. candidateOffsets,
								new ConjugateLinkViewNode(ColorIdentifier.Normal, head, begin, a),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, begin, end, b),
								new ConjugateLinkViewNode(ColorIdentifier.Normal, end, extra, a)
							]
						],
						context.Options,
						Technique.UniqueRectangle4X3,
						d1,
						d2,
						[.. urCells],
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 4C/3SL.
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
	///   ↓ corner1, corner2
	/// (abx)-----(aby)
	///        a    |
	///             | a
	///        b    |
	///  abz ----- abw
	/// ]]></code>
	/// </para>
	/// <para>
	/// Subtype 2:
	/// <code><![CDATA[
	///   ↓ corner1, corner2
	/// (abx)-----(aby)
	///   |    a    |
	///   | b       | a
	///   |         |
	///  abz       abw
	/// ]]></code>
	/// </para>
	/// </remarks>
	private partial void Check4C3SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		var link1Map = corner1.AsCellMap() + corner2;
		var innerMaps = (stackalloc CellMap[2]);
		foreach (var (a, b) in ((d1, d2), (d2, d1)))
		{
			if (!IsConjugatePair(a, link1Map, link1Map.SharedLine))
			{
				continue;
			}

			var end = GetDiagonalCell(urCells, corner1);
			var extra = (otherCellsMap - end)[0];
			foreach (var (abx, aby, abw, abz) in ((corner2, corner1, extra, end), (corner1, corner2, end, extra)))
			{
				var link2Map = aby.AsCellMap() + abw;
				if (!IsConjugatePair(a, link2Map, link2Map.SharedLine))
				{
					continue;
				}

				var link3Map1 = abw.AsCellMap() + abz;
				var link3Map2 = abx.AsCellMap() + abz;
				innerMaps[0] = link3Map1;
				innerMaps[1] = link3Map2;
				for (var i = 0; i < 2; i++)
				{
					var linkMap = innerMaps[i];
					if (!IsConjugatePair(b, link3Map1, link3Map1.SharedLine))
					{
						continue;
					}

					if (!CandidatesMap[b].Contains(aby))
					{
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>(7);
					foreach (var d in grid.GetCandidates(abx))
					{
						if (d == d1 || d == d2)
						{
							candidateOffsets.Add(
								new(
									i == 0 ? d == a ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal : ColorIdentifier.Auxiliary1,
									abx * 9 + d
								)
							);
						}
					}
					foreach (var d in grid.GetCandidates(abz))
					{
						if (d == d1 || d == d2)
						{
							candidateOffsets.Add(new(d == b ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, abz * 9 + d));
						}
					}
					foreach (var d in grid.GetCandidates(aby))
					{
						if ((d == d1 || d == d2) && d != b)
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, aby * 9 + d));
						}
					}
					foreach (var d in grid.GetCandidates(abw))
					{
						if (d == d1 || d == d2)
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, abw * 9 + d));
						}
					}
					if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
					{
						continue;
					}

					var conjugatePairs = (Conjugate[])[new(abx, aby, a), new(aby, abw, a), new(linkMap[0], linkMap[1], b)];
					accumulator.Add(
						new UniqueRectangleConjugatePairStep(
							new SingletonArray<Conclusion>(new(Elimination, aby, b)),
							[
								[
									.. arMode ? GetHighlightCells(urCells) : [],
									.. candidateOffsets,
									new ConjugateLinkViewNode(ColorIdentifier.Normal, abx, aby, a),
									new ConjugateLinkViewNode(ColorIdentifier.Normal, aby, abw, a),
									new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, linkMap[0], linkMap[1], b)
								]
							],
							context.Options,
							Technique.UniqueRectangle4C3,
							d1,
							d2,
							[.. urCells],
							arMode,
							conjugatePairs,
							index
						)
					);
				}
			}
		}
	}
}
