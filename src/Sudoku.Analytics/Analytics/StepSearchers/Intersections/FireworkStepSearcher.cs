namespace Sudoku.Analytics.StepSearchers.Intersections;

/// <summary>
/// Provides with a <b>Firework</b> step searcher. The step searcher will include the following techniques:
/// <list type="bullet">
/// <!--
/// <item>
/// Firework Pair:
/// <list type="bullet">
/// <item>Firework Pair Type 1 (Single Firework + 2 Bi-value cells)</item>
/// <item>Firework Pair Type 2 (Double Fireworks)</item>
/// <item>Firework Pair Type 3 (Single Fireworks + Empty Rectangle)</item>
/// </list>
/// </item>
/// -->
/// <item>Firework Triple</item>
/// <item>Firework Quadruple</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_FireworkStepSearcher",
	Technique.FireworkTriple, Technique.FireworkQuadruple,
	RuntimeFlags = StepSearcherRuntimeFlags.TimeComplexity)]
public sealed partial class FireworkStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the patterns used.
	/// </summary>
	private static readonly FireworkPattern[] Patterns;

	/// <summary>
	/// Indicates the house combinations.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly BlockIndex[][] HouseCombinations = [
		[0, 1, 3, 4], [0, 2, 3, 5], [1, 2, 4, 5],
		[0, 1, 6, 7], [0, 2, 6, 8], [1, 2, 7, 8],
		[3, 4, 6, 7], [3, 5, 6, 8], [4, 5, 7, 8]
	];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static FireworkStepSearcher()
	{
		Patterns = new FireworkPattern[3645];

		var i = 0;
		foreach (var houseQuad in HouseCombinations)
		{
			// Collection for pattern triples.
			foreach (var triple in houseQuad.AsReadOnlySpan().GetSubsets(3))
			{
				foreach (var a in HousesMap[triple[0]])
				{
					foreach (var b in HousesMap[triple[1]])
					{
						foreach (var c in HousesMap[triple[2]])
						{
							if ((a.AsCellMap() + b).FirstSharedHouse != 32 && (a.AsCellMap() + c).FirstSharedHouse != 32)
							{
								Patterns[i++] = new([a, b, c], a);
								continue;
							}

							if ((a.AsCellMap() + b).FirstSharedHouse != 32 && (b.AsCellMap() + c).FirstSharedHouse != 32)
							{
								Patterns[i++] = new([a, b, c], b);
								continue;
							}

							if ((a.AsCellMap() + c).FirstSharedHouse != 32 && (b.AsCellMap() + c).FirstSharedHouse != 32)
							{
								Patterns[i++] = new([a, b, c], c);
							}
						}
					}
				}
			}

			// Collection for pattern quadruples.
			foreach (var a in HousesMap[houseQuad[0]])
			{
				foreach (var b in HousesMap[houseQuad[1]])
				{
					foreach (var c in HousesMap[houseQuad[2]])
					{
						foreach (var d in HousesMap[houseQuad[3]])
						{
							if ((a.AsCellMap() + b).FirstSharedHouse == 32 || (a.AsCellMap() + c).FirstSharedHouse == 32
								|| (b.AsCellMap() + d).FirstSharedHouse == 32 || (c.AsCellMap() + d).FirstSharedHouse == 32)
							{
								continue;
							}

							Patterns[i++] = new([a, b, c, d], null);
						}
					}
				}
			}
		}
	}

	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var accumulator = context.Accumulator!;
		var onlyFindOne = context.OnlyFindOne;
		foreach (var pattern in Patterns)
		{
			if ((EmptyCells & pattern.Map) != pattern.Map)
			{
				// At least one cell is not empty cell. Just skip this pattern.
				continue;
			}

			// Checks for the number of kinds of digits in three cells.
			var digitsMask = grid[pattern.Map];
			if (Mask.PopCount(digitsMask) < 3)
			{
				continue;
			}

			switch (pattern.Pivot)
			{
				case { } pivot when Mask.PopCount(digitsMask) >= 3:
				{
					if (CheckTriple(accumulator, grid, ref context, onlyFindOne, pattern, digitsMask, pivot) is { } stepTriple)
					{
						return stepTriple;
					}
					break;
				}
				case null when Mask.PopCount(digitsMask) >= 4:
				{
					if (CheckQuadruple(accumulator, grid, ref context, onlyFindOne, pattern) is { } step)
					{
						return step;
					}
					break;
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Checks for firework triple steps.
	/// </summary>
	private FireworkTripleStep? CheckTriple(
		List<Step> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		bool onlyFindOne,
		FireworkPattern pattern,
		Mask digitsMask,
		Cell pivot
	)
	{
		var characters = context.Options.BabaGroupInitialLetter.GetSequence(context.Options.BabaGroupLetterCasing).ToArray();

		var nonPivotCells = pattern.Map - pivot;
		var cell1 = nonPivotCells[0];
		var cell2 = nonPivotCells[1];
		var satisfiedDigitsMask = GetFireworkDigits(cell1, cell2, pivot, grid, out var house1CellsExcluded, out var house2CellsExcluded);
		if (satisfiedDigitsMask == 0)
		{
			// No possible digits found as a firework digit.
			return null;
		}

		foreach (var digits in digitsMask.AllSets.GetSubsets(3))
		{
			var currentDigitsMask = (Mask)(1 << digits[0] | 1 << digits[1] | 1 << digits[2]);
			if ((satisfiedDigitsMask & currentDigitsMask) != currentDigitsMask)
			{
				continue;
			}

			// Firework Triple is found.
			var pivotCellBlock = pivot.ToHouse(HouseType.Block);
			var pivotRowCells = HousesMap[pivot.ToHouse(HouseType.Row)];
			var pivotColumnCells = HousesMap[pivot.ToHouse(HouseType.Column)];

			// Now check eliminations.
			var conclusions = new List<Conclusion>(18);
			foreach (var digit in (Mask)(grid.GetCandidates(pivot) & ~currentDigitsMask))
			{
				conclusions.Add(new(Elimination, pivot, digit));
			}
			foreach (var digit in (Mask)(grid.GetCandidates(cell1) & ~currentDigitsMask))
			{
				conclusions.Add(new(Elimination, cell1, digit));
			}
			foreach (var digit in (Mask)(grid.GetCandidates(cell2) & ~currentDigitsMask))
			{
				conclusions.Add(new(Elimination, cell2, digit));
			}
			foreach (var digit in currentDigitsMask)
			{
				var possibleBlockCells = HousesMap[pivotCellBlock] & EmptyCells & CandidatesMap[digit];
				foreach (var cell in possibleBlockCells & ~pivotRowCells & ~pivotColumnCells)
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
			if (conclusions.Count == 0)
			{
				// No eliminations found.
				continue;
			}

			var step = new FireworkTripleStep(
				conclusions.AsMemory(),
				[
					[
						..
						from digit in (Mask)(grid.GetCandidates(pivot) & currentDigitsMask)
						select new CandidateViewNode(ColorIdentifier.Normal, pivot * 9 + digit),
						..
						from digit in (Mask)(grid.GetCandidates(cell1) & currentDigitsMask)
						select new CandidateViewNode(ColorIdentifier.Normal, cell1 * 9 + digit),
						..
						from digit in (Mask)(grid.GetCandidates(cell2) & currentDigitsMask)
						select new CandidateViewNode(ColorIdentifier.Normal, cell2 * 9 + digit)
					],
					[
						..
						from house1CellExcluded in house1CellsExcluded
						select new CellViewNode(ColorIdentifier.Elimination, house1CellExcluded),
						..
						from house2CellExcluded in house2CellsExcluded
						select new CellViewNode(ColorIdentifier.Elimination, house2CellExcluded),
						..
						from cell in (HousesMap[(cell1.AsCellMap() + pivot).SharedLine] & HousesMap[pivotCellBlock] & EmptyCells) - pivot
						select new BabaGroupViewNode(cell, characters[1], currentDigitsMask),
						..
						from cell in (HousesMap[(cell2.AsCellMap() + pivot).SharedLine] & HousesMap[pivotCellBlock] & EmptyCells) - pivot
						select new BabaGroupViewNode(cell, characters[0], currentDigitsMask),
						new BabaGroupViewNode(pivot, characters[2], (Mask)(grid.GetCandidates(pivot) & currentDigitsMask)),
						new BabaGroupViewNode(cell1, characters[0], (Mask)(grid.GetCandidates(cell1) & currentDigitsMask)),
						new BabaGroupViewNode(cell2, characters[1], (Mask)(grid.GetCandidates(cell2) & currentDigitsMask))
					]
				],
				context.Options,
				pattern.Map,
				currentDigitsMask
			);
			if (onlyFindOne)
			{
				return step;
			}

			accumulator.Add(step);
		}

		return null;
	}

	/// <summary>
	/// Checks for firework quadruple steps.
	/// </summary>
	private FireworkQuadrupleStep? CheckQuadruple(
		List<Step> accumulator,
		in Grid grid,
		ref StepAnalysisContext context,
		bool onlyFindOne,
		FireworkPattern pattern
	)
	{
		if (pattern is not { Map: [var c1, var c2, var c3, var c4] map })
		{
			return null;
		}

		var digitsMask = grid[map];
		if (Mask.PopCount(digitsMask) < 4)
		{
			return null;
		}

		foreach (var digits in digitsMask.AllSets.GetSubsets(4))
		{
			ReadOnlySpan<((Digit, Digit), (Digit, Digit))> cases = [
				((digits[0], digits[1]), (digits[2], digits[3])),
				((digits[0], digits[2]), (digits[1], digits[3])),
				((digits[0], digits[3]), (digits[1], digits[2])),
				((digits[1], digits[2]), (digits[0], digits[3])),
				((digits[1], digits[3]), (digits[0], digits[2])),
				((digits[2], digits[3]), (digits[0], digits[1]))
			];

			foreach (var (pivot1, pivot2) in ((c1, c4), (c2, c3)))
			{
				var nonPivot1Cells = (map - pivot1) & (HousesMap[pivot1.ToHouse(HouseType.Row)] | HousesMap[pivot1.ToHouse(HouseType.Column)]);
				var nonPivot2Cells = (map - pivot2) & (HousesMap[pivot2.ToHouse(HouseType.Row)] | HousesMap[pivot2.ToHouse(HouseType.Column)]);
				var cell1Pivot1 = nonPivot1Cells[0];
				var cell2Pivot1 = nonPivot1Cells[1];
				var cell1Pivot2 = nonPivot2Cells[0];
				var cell2Pivot2 = nonPivot2Cells[1];
				foreach (var ((d1, d2), (d3, d4)) in cases)
				{
					var pair1DigitsMask = (Mask)(1 << d1 | 1 << d2);
					var pair2DigitsMask = (Mask)(1 << d3 | 1 << d4);
					var satisfiedDigitsMaskPivot1 = GetFireworkDigits(
						cell1Pivot1, cell2Pivot1, pivot1, grid, out var house1CellsExcludedPivot1,
						out var house2CellsExcludedPivot1
					);
					var satisfiedDigitsMaskPivot2 = GetFireworkDigits(
						cell1Pivot2, cell2Pivot2, pivot2, grid, out var house1CellsExcludedPivot2,
						out var house2CellsExcludedPivot2
					);
					if ((satisfiedDigitsMaskPivot1 & pair1DigitsMask) != pair1DigitsMask
						|| (satisfiedDigitsMaskPivot2 & pair2DigitsMask) != pair2DigitsMask)
					{
						continue;
					}

					// Firework quadruple found.
					var fourDigitsMask = (Mask)(pair1DigitsMask | pair2DigitsMask);
					var conclusions = new List<Conclusion>(20);
					foreach (var digit in (Mask)(grid.GetCandidates(pivot1) & ~pair1DigitsMask))
					{
						conclusions.Add(new(Elimination, pivot1, digit));
					}
					foreach (var digit in (Mask)(grid.GetCandidates(pivot2) & ~pair2DigitsMask))
					{
						conclusions.Add(new(Elimination, pivot2, digit));
					}
					foreach (var cell in map - pivot1 - pivot2)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~fourDigitsMask))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					foreach (var cell in
						HousesMap[pivot1.ToHouse(HouseType.Block)]
							& ~HousesMap[pivot1.ToHouse(HouseType.Row)]
							& ~HousesMap[pivot1.ToHouse(HouseType.Column)]
							& EmptyCells)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & pair1DigitsMask))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					foreach (var cell in
						HousesMap[pivot2.ToHouse(HouseType.Block)]
							& ~HousesMap[pivot2.ToHouse(HouseType.Row)]
							& ~HousesMap[pivot2.ToHouse(HouseType.Column)]
							& EmptyCells)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & pair2DigitsMask))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						// No eliminations found.
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var digit in (Mask)(grid.GetCandidates(pivot1) & fourDigitsMask))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, pivot1 * 9 + digit));
					}
					foreach (var digit in (Mask)(grid.GetCandidates(pivot2) & fourDigitsMask))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, pivot2 * 9 + digit));
					}
					foreach (var cell in map - pivot1 - pivot2)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & fourDigitsMask))
						{
							candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
						}
					}

					var candidateOffsetsView2 = new List<CandidateViewNode>();
					foreach (var cell in map - pivot2)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & pair1DigitsMask))
						{
							candidateOffsetsView2.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
						}
					}
					var candidateOffsetsView3 = new List<CandidateViewNode>();
					foreach (var cell in map - pivot1)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & pair2DigitsMask))
						{
							candidateOffsetsView3.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
						}
					}

					var cellOffsets1 = new List<CellViewNode>();
					foreach (var cell in house1CellsExcludedPivot1 | house2CellsExcludedPivot1)
					{
						cellOffsets1.Add(new(ColorIdentifier.Elimination, cell));
					}
					var cellOffsets2 = new List<CellViewNode>();
					foreach (var cell in house1CellsExcludedPivot2 | house2CellsExcludedPivot2)
					{
						cellOffsets2.Add(new(ColorIdentifier.Elimination, cell));
					}

					var step = new FireworkQuadrupleStep(
						conclusions.AsMemory(),
						[
							[.. candidateOffsets],
							[.. cellOffsets1, .. candidateOffsetsView2],
							[.. cellOffsets2, .. candidateOffsetsView3]
						],
						context.Options,
						map,
						fourDigitsMask
					);
					if (onlyFindOne)
					{
						return step;
					}

					accumulator.Add(step);
				}
			}
		}

		return null;
	}


	/// <summary>
	/// <para>Checks for all digits which the cells containing form a firework pattern.</para>
	/// <para>
	/// This method returns the digits that satisfied the condition. If none found,
	/// this method will return 0.
	/// </para>
	/// </summary>
	/// <param name="c1">The cell 1 used in this pattern.</param>
	/// <param name="c2">The cell 2 used in this pattern.</param>
	/// <param name="pivot">The pivot cell.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="house1CellsExcluded">
	/// The excluded cells that is out of the firework pattern in the <paramref name="c1"/>'s house.
	/// </param>
	/// <param name="house2CellsExcluded">
	/// The excluded cells that is out of the firework pattern in the <paramref name="c2"/>'s house.
	/// </param>
	/// <returns>All digits that satisfied the firework rule. If none found, 0.</returns>
	private static Mask GetFireworkDigits(
		Cell c1,
		Cell c2,
		Cell pivot,
		in Grid grid,
		out CellMap house1CellsExcluded,
		out CellMap house2CellsExcluded
	)
	{
		var pivotCellBlock = pivot.ToHouse(HouseType.Block);
		var excluded1 = HousesMap[(c1.AsCellMap() + pivot).SharedLine] & ~HousesMap[pivotCellBlock] - c1;
		var excluded2 = HousesMap[(c2.AsCellMap() + pivot).SharedLine] & ~HousesMap[pivotCellBlock] - c2;
		var finalMask = (Mask)0;
		foreach (var digit in grid[[c1, c2, pivot]])
		{
			if (isFireworkFor(digit, excluded1, grid) && isFireworkFor(digit, excluded2, grid))
			{
				finalMask |= (Mask)(1 << digit);
			}
		}

		(house1CellsExcluded, house2CellsExcluded) = (excluded1, excluded2);
		return finalMask;


		static bool isFireworkFor(Digit digit, in CellMap houseCellsExcluded, in Grid grid)
		{
			foreach (var cell in houseCellsExcluded)
			{
				switch (grid.GetDigit(cell))
				{
					case -1 when CandidatesMap[digit].Contains(cell):
					case var cellValue when cellValue == digit:
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
