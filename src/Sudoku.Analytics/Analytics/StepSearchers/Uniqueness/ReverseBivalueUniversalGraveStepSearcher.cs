namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Reverse Bivalue Universal Grave</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Reverse Bivalue Universal Grave Type 1</item>
/// <item>Reverse Bivalue Universal Grave Type 2</item>
/// <item>Reverse Bivalue Universal Grave Type 3</item>
/// <item>Reverse Bivalue Universal Grave Type 4</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_ReverseBivalueUniversalGraveStepSearcher",
	Technique.ReverseBivalueUniversalGraveType1, Technique.ReverseBivalueUniversalGraveType2,
	Technique.ReverseBivalueUniversalGraveType3, Technique.ReverseBivalueUniversalGraveType4,
	SupportedSudokuTypes = SudokuType.Standard,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class ReverseBivalueUniversalGraveStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the global maps. The length of this field is always 7.
	/// </summary>
	private static readonly CellMap[] GlobalMaps = [CellMap.Full, .. from chute in Chutes select chute.Cells];


	/// <summary>
	/// Indicates whether the searcher can search for partially-used types, meaning the formed reverse BUG pattern may <b>not</b> be required
	/// occupying <b>all</b> value cells of two digits. The default value is <see langword="true"/>.
	/// </summary>
	[SettingItemName(SettingItemNames.SearchForReverseBugPartiallyUsedTypes)]
	public bool AllowPartiallyUsedTypes { get; set; } = true;

	/// <summary>
	/// Indicates the maximum number of cells the step searcher will be searched for. This value controls the complexity of the technique.
	/// The maximum value is 4, and recommend value is 2. This value cannot be negative. The default value is 4.
	/// </summary>
	[SettingItemName(SettingItemNames.ReverseBugMaxSearchingEmptyCellsCount)]
	public int MaxSearchingEmptyCellsCount { get; set; } = 4;


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Collect all possible digits can be used for the final construction of reverse BUGs.
		ref readonly var grid = ref context.Grid;
		Mask digits;
		if (AllowPartiallyUsedTypes)
		{
			digits = Grid.MaxCandidatesMask;
		}
		else
		{
			digits = 0;
			for (var digit = 0; digit < 9; digit++)
			{
				// Check whether the digit can be used.
				// If the number of values for the current digit is 8 or 9, it may not be used for searching for reverse BUGs.
				if (ValuesMap[digit].Count <= 7)
				{
					digits |= (Mask)(1 << digit);
				}
			}
		}

		// Remove naked singles.
		var emptyCells = EmptyCells;
		foreach (var cell in EmptyCells)
		{
			if (BitOperations.IsPow2(context.Grid.GetCandidates(cell)))
			{
				emptyCells.Remove(cell);
			}
		}

		var accumulator = new HashSet<ReverseBivalueUniversalGraveStep>();
		var globalMapUpperBound = AllowPartiallyUsedTypes ? GlobalMaps.Length : 1;

		// Iterates on all combinations of digits, with length of each combination 2.
		foreach (var digitPair in digits.AllSets.GetSubsets(2))
		{
			var d1 = digitPair[0];
			var d2 = digitPair[1];
			var comparer = (Mask)(1 << d1 | 1 << d2);

			for (var i = 0; i < globalMapUpperBound; i++)
			{
				ref readonly var globalMap = ref GlobalMaps[i];
				var baseValuesMap = ValuesMap[d1] | ValuesMap[d2];
				if (baseValuesMap.Count >= 16)
				{
					continue;
				}

				var valuesMap = i == 0 ? baseValuesMap : globalMap & baseValuesMap;

				// Extra check: If the global map doesn't use all cells of a grid, we should check the other 2 floors/towers.
				// If those floors/towers are not filled one of two digits, the pattern won't be formed, neither.
				var d1Map = ValuesMap[d1] & ~globalMap;
				var d2Map = ValuesMap[d2] & ~globalMap;
				var d1Counter = d1Map.Count;
				var d2Counter = d2Map.Count;
				switch (i)
				{
					case 0 when (d1Counter, d2Counter) is ( >= 7, _) or (_, >= 7):
					{
						continue;
					}
					case not 0:
					{
						var flag = true;
						foreach (var c in d1Map | d2Map)
						{
							if (grid.GetState(c) == CellState.Given)
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							// The last cells cannot contain givens of digit 1 or 2.
							continue;
						}

						break;
					}
				}

				// The following loop is used for appending new empty cells into the variable 'valuesMap'.
				// Reverse BUGs can be split into two parts: Reverse URs and Reverse ULs.
				// Both of them are used cells, of length being a even number.
				// If the variable 'valuesMap' holds an odd number of cells,
				// we should append 2 or 4 cells; otherwise, 1 or 3 cells, to achieve this.
				// The total number of empty cells chosen may not be greater than 4,
				// because eliminations in such constructed pattern may not exist. In addition, only type 2 will use at most 4 empty cells;
				// other types will only use 1 or 2 empty cells.
				for (
					var incrementStep = (valuesMap.Count & 1) == 0 ? 2 : 1;
					incrementStep <= Math.Min(18 - d1Counter - d2Counter, MaxSearchingEmptyCellsCount);
					incrementStep += 2
				)
				{
					foreach (ref readonly var cellsChosen in emptyCells & incrementStep)
					{
						var completePattern = valuesMap | cellsChosen;
						if (!IsGeneralizedUniqueLoop(completePattern))
						{
							// This pattern is invalid.
							continue;
						}

						if (GetSeparatedHamiltonianCycles(new(completePattern)) is not [var firstSeparatedLoopSolution, ..])
						{
							continue;
						}

						if (CheckType1(
							accumulator, ref context, d1, d2, comparer, completePattern,
							cellsChosen, firstSeparatedLoopSolution
						) is { } type1Step)
						{
							return type1Step;
						}
						if (CheckType2(
							accumulator, ref context, d1, d2, comparer, completePattern,
							cellsChosen, firstSeparatedLoopSolution
						) is { } type2Step)
						{
							return type2Step;
						}
						if (CheckType3(
							accumulator, ref context, d1, d2, comparer, completePattern,
							cellsChosen, firstSeparatedLoopSolution
						) is { } type3Step)
						{
							return type3Step;
						}
						if (CheckType4(
							accumulator, ref context, d1, d2, comparer, completePattern,
							cellsChosen, firstSeparatedLoopSolution
						) is { } type4Step)
						{
							return type4Step;
						}
					}
				}
			}
		}

		if (!context.OnlyFindOne && accumulator.Count != 0)
		{
			context.Accumulator.AddRange(accumulator);
		}
		return null;
	}

	/// <summary>
	/// Check for type 1.
	/// </summary>
	/// <param name="accumulator">The accumulator.</param>
	/// <param name="context"><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="d1">The first digit used.</param>
	/// <param name="d2">The second digit used.</param>
	/// <param name="comparer">A mask that contains the digits <paramref name="d1"/> and <paramref name="d2"/>.</param>
	/// <param name="completePattern">The complete pattern.</param>
	/// <param name="cellsChosen">The empty cells chosen.</param>
	/// <param name="separatedLoops">The separated hamiltonian cycles to be used to render loop links.</param>
	/// <returns><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private ReverseBivalueUniversalGraveType1Step? CheckType1(
		HashSet<ReverseBivalueUniversalGraveStep> accumulator,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		Mask comparer,
		in CellMap completePattern,
		in CellMap cellsChosen,
		ReadOnlySpan<HamiltonianCycle> separatedLoops
	)
	{
		if (cellsChosen is not [var extraCell])
		{
			return null;
		}

		var mask = context.Grid.GetCandidates(extraCell);
		if ((Mask)(mask & comparer) is not (var elimDigitsMask and not 0))
		{
			return null;
		}

		var cellOffsets = new List<CellViewNode>(completePattern.Count);
		foreach (var cell in completePattern)
		{
			cellOffsets.Add(new(cellsChosen.Contains(cell) ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell));
		}

		var step = new ReverseBivalueUniversalGraveType1Step(
			new SingletonArray<Conclusion>(new(Elimination, extraCell, BitOperations.TrailingZeroCount(elimDigitsMask))),
			[[.. cellOffsets, .. GetLinkViewNodes(separatedLoops)]],
			context.Options,
			d1,
			d2,
			completePattern,
			cellsChosen
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for type 2.
	/// </summary>
	/// <param name="accumulator">The step accumulator.</param>
	/// <param name="context"><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="d1">The first digit used.</param>
	/// <param name="d2">The second digit used.</param>
	/// <param name="comparer">A mask that contains the digits <paramref name="d1"/> and <paramref name="d2"/>.</param>
	/// <param name="completePattern">The complete pattern.</param>
	/// <param name="cellsChosen">The empty cells chosen.</param>
	/// <param name="separatedLoops">The separated hamiltonian cycles to be used to render loop links.</param>
	/// <returns><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private ReverseBivalueUniversalGraveType2Step? CheckType2(
		HashSet<ReverseBivalueUniversalGraveStep> accumulator,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		Mask comparer,
		in CellMap completePattern,
		in CellMap cellsChosen,
		ReadOnlySpan<HamiltonianCycle> separatedLoops
	)
	{
		var lastDigitsMask = (Mask)(context.Grid[cellsChosen] & ~comparer);
		if (!BitOperations.IsPow2(lastDigitsMask))
		{
			return null;
		}

		var extraDigit = BitOperations.TrailingZeroCount(lastDigitsMask);
		var elimMap = cellsChosen.PeerIntersection & EmptyCells & CandidatesMap[extraDigit];
		if (!elimMap)
		{
			return null;
		}

		var cellOffsets = new List<CellViewNode>(completePattern.Count);
		foreach (var cell in completePattern)
		{
			cellOffsets.Add(new(cellsChosen.Contains(cell) ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell));
		}

		var step = new ReverseBivalueUniversalGraveType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
			[
				[
					.. cellOffsets,
					..
					from cell in cellsChosen
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + extraDigit),
					.. GetLinkViewNodes(separatedLoops)
				]
			],
			context.Options,
			d1,
			d2,
			extraDigit,
			completePattern,
			cellsChosen
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);

		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	/// <param name="accumulator">The step accumulator.</param>
	/// <param name="context"><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="d1">The first digit used.</param>
	/// <param name="d2">The second digit used.</param>
	/// <param name="comparer">A mask that contains the digits <paramref name="d1"/> and <paramref name="d2"/>.</param>
	/// <param name="completePattern">The complete pattern.</param>
	/// <param name="cellsChosen">The empty cells chosen.</param>
	/// <param name="separatedLoops">The separated hamiltonian cycles to be used to render loop links.</param>
	/// <returns><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private ReverseBivalueUniversalGraveType3Step? CheckType3(
		HashSet<ReverseBivalueUniversalGraveStep> accumulator,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		Mask comparer,
		in CellMap completePattern,
		in CellMap cellsChosen,
		ReadOnlySpan<HamiltonianCycle> separatedLoops
	)
	{
		// Test examples:
		// +7.+2+4.36+5+9..9..741+3.3+45.+92+8+7..89...6.+9....5..827..3....821.+5+4..6.9.1..+8......9.5.+1:824 145 449 155 458 681 683 285 694 696
		// 85....+3+4...+234...54+3...5...+38.6.+29+54+9.5...6+2+3+62+49+5+3+18+729+85+3+7+4+61......+53+8+54+3+8+6127+9:915 135 235 735 755

		if (cellsChosen is not [var cell1, var cell2])
		{
			return null;
		}

		if (cellsChosen.FirstSharedHouse == FallbackConstants.@int)
		{
			return null;
		}

		ref readonly var grid = ref context.Grid;
		var (digitsMask1, digitsMask2) = (grid.GetCandidates(cell1), grid.GetCandidates(cell2));
		var otherDigitsMask = (Mask)((digitsMask1 | digitsMask2) & ~comparer);
		if (BitOperations.IsPow2(otherDigitsMask))
		{
			// Only one digit is categorized as "other digits". In this case we can only use an extra cell to form a type 3.
			// However, the extra cell is a naked single. The naked single must be handled before this technique.
			return null;
		}

		var numbersOfOtherDigits = BitOperations.PopCount(otherDigitsMask);
		foreach (var house in cellsChosen.SharedHouses)
		{
			var otherEmptyCells = EmptyCells & HousesMap[house] & ~cellsChosen;
			if (otherEmptyCells.Count <= numbersOfOtherDigits - 1)
			{
				// No conclusion will be created.
				continue;
			}

			foreach (ref readonly var cells in otherEmptyCells & numbersOfOtherDigits - 1)
			{
				if (grid[cells] != otherDigitsMask)
				{
					// The subset is not matched.
					continue;
				}

				// Type 3 found. Now check eliminations.
				var elimMap = otherEmptyCells & ~cells;
				if (!elimMap)
				{
					continue;
				}

				var conclusions = new List<Conclusion>(elimMap.Count * numbersOfOtherDigits);
				foreach (var cell in elimMap)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & otherDigitsMask))
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
				if (conclusions.Count == 0)
				{
					// No eliminations.
					continue;
				}

				var cellOffsets = new List<CellViewNode>(completePattern.Count);
				foreach (var cell in completePattern)
				{
					cellOffsets.Add(new(cellsChosen.Contains(cell) ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell));
				}

				var candidateOffsets = new List<CandidateViewNode>(cellsChosen.Count);
				foreach (var cell in cellsChosen)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & comparer))
					{
						candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
					}
				}
				foreach (var cell in cells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}
				}

				var step = new ReverseBivalueUniversalGraveType3Step(
					conclusions.AsMemory(),
					[
						[
							.. cellOffsets,
							.. candidateOffsets,
							new HouseViewNode(ColorIdentifier.Normal, house),
							.. GetLinkViewNodes(separatedLoops)
						]
					],
					context.Options,
					d1,
					d2,
					house,
					otherDigitsMask,
					completePattern,
					cellsChosen
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				accumulator.Add(step);
			}
		}

		return null;
	}

	/// <summary>
	/// Check for type 4.
	/// </summary>
	/// <param name="accumulator">The step accumulator.</param>
	/// <param name="context"><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="d1">The first digit used.</param>
	/// <param name="d2">The second digit used.</param>
	/// <param name="comparer">A mask that contains the digits <paramref name="d1"/> and <paramref name="d2"/>.</param>
	/// <param name="completePattern">The complete pattern.</param>
	/// <param name="cellsChosen">The empty cells chosen.</param>
	/// <param name="separatedLoops">The separated hamiltonian cycles to be used to render loop links.</param>
	/// <returns><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private ReverseBivalueUniversalGraveType4Step? CheckType4(
		HashSet<ReverseBivalueUniversalGraveStep> accumulator,
		ref StepAnalysisContext context,
		Digit d1,
		Digit d2,
		Mask comparer,
		in CellMap completePattern,
		in CellMap cellsChosen,
		ReadOnlySpan<HamiltonianCycle> separatedLoops
	)
	{
		if (cellsChosen is not [var cell1, var cell2])
		{
			return null;
		}

		if (cellsChosen.FirstSharedHouse != FallbackConstants.@int)
		{
			return null;
		}

		ref readonly var grid = ref context.Grid;
		var cell1Digit = (Mask)(grid.GetCandidates(cell1) & comparer);
		var cell2Digit = (Mask)(grid.GetCandidates(cell2) & comparer);
		var mergedDigitMask = (Mask)(cell1Digit | cell2Digit);
		if (!BitOperations.IsPow2(mergedDigitMask))
		{
			return null;
		}

		var selectedDigit = BitOperations.TrailingZeroCount(mergedDigitMask);
		if (!((grid.Exists(cell1, selectedDigit) ?? false) && (grid.Exists(cell2, selectedDigit) ?? false)))
		{
			// We should ensure all chosen cells (empty cells) contain the selected digit.
			return null;
		}

		foreach (var house in cellsChosen.Houses)
		{
			var possibleConjugatePairCells = CandidatesMap[selectedDigit] & HousesMap[house];
			if (possibleConjugatePairCells.Count != 2)
			{
				continue;
			}

			var conjugatePairCellOuterPattern = (possibleConjugatePairCells & ~cellsChosen)[0];
			var conjugatePairCellInnerPattern = (possibleConjugatePairCells - conjugatePairCellOuterPattern)[0];
			var anotherCell = (cellsChosen - conjugatePairCellInnerPattern)[0];
			if ((anotherCell.AsCellMap() + conjugatePairCellOuterPattern).FirstSharedHouse == FallbackConstants.@int)
			{
				continue;
			}

			var cellOffsets = new List<CellViewNode>(completePattern.Count);
			foreach (var cell in completePattern)
			{
				cellOffsets.Add(new(cellsChosen.Contains(cell) ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal, cell));
			}

			var step = new ReverseBivalueUniversalGraveType4Step(
				new SingletonArray<Conclusion>(new(Elimination, anotherCell, selectedDigit)),
				[
					[
						.. cellOffsets,
						new CandidateViewNode(ColorIdentifier.Auxiliary1, conjugatePairCellInnerPattern * 9 + selectedDigit),
						new CandidateViewNode(ColorIdentifier.Auxiliary1, conjugatePairCellOuterPattern * 9 + selectedDigit),
						new ConjugateLinkViewNode(
							ColorIdentifier.Normal,
							possibleConjugatePairCells[0],
							possibleConjugatePairCells[1],
							selectedDigit
						),
						new ChainLinkViewNode(
							ColorIdentifier.Normal,
							[conjugatePairCellInnerPattern * 9 + selectedDigit],
							[anotherCell * 9 + selectedDigit],
							false
						),
						new ChainLinkViewNode(
							ColorIdentifier.Normal,
							[conjugatePairCellOuterPattern * 9 + selectedDigit],
							[anotherCell * 9 + selectedDigit],
							false
						),
						.. GetLinkViewNodes(separatedLoops)
					]
				],
				context.Options,
				d1,
				d2,
				completePattern,
				cellsChosen,
				new(possibleConjugatePairCells, selectedDigit)
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			accumulator.Add(step);
		}

		return null;
	}


	/// <summary>
	/// Determine whether a loop is a generalized unique loop.
	/// </summary>
	/// <param name="loop">The loop to be checked.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	/// <remarks>
	/// <para>This method uses another way to check for unique loops.</para>
	/// <para>
	/// <i>
	/// However, this method contains a little bug for checking loops, leading to returning <see langword="true"/> for this method,
	/// and returning <see langword="false"/> for the method <see cref="UniqueLoopPattern.IsValid{T}(T)"/>
	/// above this.
	/// If a pattern is like:
	/// </i>
	/// <code><![CDATA[
	/// .-----.-----.
	/// | x   | x   |
	/// |   x |   x |
	/// |-----+-----|
	/// | x   |   x |
	/// |   x | x   |
	/// '-----'-----'
	/// ]]></code>
	/// <i>This pattern isn't a valid unique loop, because the pattern has no suitable way to filling digits, without conflict.</i>
	/// </para>
	/// <para>
	/// This method can also check for separated ones, e.g.:
	/// <code><![CDATA[
	/// .-------.-------.-------.
	/// | x     | x     |       |
	/// | x     | x     |       |
	/// |       |       |       |
	/// |-------+-------+-------|
	/// |       |       |       |
	/// |       |     x |     x |
	/// |       |     x |     x |
	/// ~~~~~~~~~~~~~~~~~~~~~~~~~
	/// ]]></code>
	/// </para>
	/// <para><inheritdoc cref="UniqueLoopPattern.IsValid{T}(T)" path="//remarks/para[2]"/></para>
	/// </remarks>
	/// <seealso cref="UniqueLoopPattern.IsValid{T}(T)"/>
	private static bool IsGeneralizedUniqueLoop(in CellMap loop)
	{
		// The length of the loop pattern must be at least 4, and an even.
		_ = loop is { Count: var length, Houses: var houses, RowMask: var r, ColumnMask: var c, BlockMask: var b };
		if ((length & 1) != 0 || length is < 4 or > 14)
		{
			return false;
		}

		// The pattern must span n/2 rows, n/2 columns and n/2 blocks, and n is the length of the pattern).
		var halfLength = length >> 1;
		if (BitOperations.PopCount(r) != halfLength || BitOperations.PopCount(c) != halfLength || BitOperations.PopCount(b) != halfLength)
		{
			return false;
		}

		// All houses spanned should contain only 2 cells of the pattern.
		foreach (var house in houses)
		{
			if ((HousesMap[house] & loop).Count != 2)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Try to find cycles that uses all cells, without checking they are in a same loop.
	/// </summary>
	/// <param name="this">The instance.</param>
	/// <returns>All found loops.</returns>
	private static ReadOnlySpan<HamiltonianCycle[]> GetSeparatedHamiltonianCycles(in CellGraph @this)
	{
		ref readonly var originalCells = ref @this.Map;
		if (originalCells)
		{
			var comparer = EqualityComparer<HamiltonianCycle>.Create(
				static (left, right) => left.Equals(right, HamiltonianCycleComparison.IgnoreDirection),
				static obj => obj.GetHashCode(HamiltonianCycleComparison.IgnoreDirection)
			);
			var paths = new HashSet<HamiltonianCycle[]>();
			dfs(originalCells, originalCells[1..], originalCells[0], [originalCells[0]], new(comparer), paths);
			return paths.ToArray();
		}
		return [];


		static void dfs(
			in CellMap originalCells,
			CellMap lastCells,
			Cell current,
			List<Cell> path,
			HashSet<HamiltonianCycle> internalLoops,
			HashSet<HamiltonianCycle[]> paths
		)
		{
			if (!lastCells && path is [var f, .., var l] && PeersMap[l].Contains(f))
			{
				paths.Add([new([.. path]), .. internalLoops]);
				internalLoops.Clear();
				return;
			}

			if (path is [var pathFirstCell, .. { Count: >= 3 }])
			{
				// Check whether we can connect back to the first cell.
				foreach (var cell in originalCells & PeersMap[current])
				{
					if (cell == pathFirstCell
						&& path.AsSpan().AsCellMap() is var excludedCells
						&& IsGeneralizedUniqueLoop(excludedCells))
					{
						internalLoops.Add(new([.. path]));

						var tempLastCells = lastCells & ~excludedCells;
						dfs(tempLastCells, tempLastCells[1..], tempLastCells[0], [tempLastCells[0]], internalLoops, paths);
					}
				}
			}

			foreach (var next in lastCells & PeersMap[current])
			{
				path.Add(next);
				dfs(originalCells, lastCells - next, next, path, internalLoops, paths);
				path.Remove(next);
			}
		}
	}

	/// <summary>
	/// Construct link view nodes.
	/// </summary>
	/// <param name="separatedLoops">The separated loops.</param>
	/// <returns>A list of <see cref="CellLinkViewNode"/> instances.</returns>
	private static ReadOnlySpan<CellLinkViewNode> GetLinkViewNodes(ReadOnlySpan<HamiltonianCycle> separatedLoops)
	{
		var links = new List<CellLinkViewNode>();
		foreach (var hamiltonianLoop in separatedLoops)
		{
			foreach (var (first, second) in hamiltonianLoop.EnumerateAdjacentCells())
			{
				links.Add(new(ColorIdentifier.Normal, first, second));
			}
		}
		return links.AsSpan();
	}
}
