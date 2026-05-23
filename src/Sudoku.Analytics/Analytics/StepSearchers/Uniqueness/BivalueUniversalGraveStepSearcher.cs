namespace Sudoku.Analytics.StepSearchers;

using TargetCandidatesGroup = BitStateMapGrouping<CandidateMap, Candidate, Cell>;

/// <summary>
/// Provides with a <b>Bi-value Universal Grave</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Basic types:
/// <list type="bullet">
/// <item>Bi-value Universal Grave Type 1</item>
/// <item>Bi-value Universal Grave Type 2</item>
/// <item>Bi-value Universal Grave Type 3</item>
/// <item>Bi-value Universal Grave Type 4</item>
/// </list>
/// </item>
/// <item>
/// Extended types:
/// <list type="bullet">
/// <item>Bi-value Universal Grave + n</item>
/// <item>Bi-value Universal Grave XZ</item>
/// </list>
/// </item>
/// <item>Bi-value Universal Grave False Candidate Type</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_BivalueUniversalGraveStepSearcher",

	// Basic types
	Technique.BivalueUniversalGraveType1, Technique.BivalueUniversalGraveType2, Technique.BivalueUniversalGraveType3,
	Technique.BivalueUniversalGraveType4,

	// Extra types
	Technique.BivalueUniversalGraveXzRule, Technique.BivalueUniversalGravePlusN,
	Technique.BivalueUniversalGraveFalseCandidateType,

	SupportsSukaku = false,
	SupportsAnalyzingPuzzleHavingMultipleSolutions = false)]
public sealed partial class BivalueUniversalGraveStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether the searcher should call the extended BUG checker
	/// to search for all true candidates no matter how difficult searching.
	/// </summary>
	[SettingItemName(SettingItemNames.SearchExtendedBivalueUniversalGraveTypes)]
	public bool SearchExtendedTypes { get; set; }


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
		=> CandidatesCount > (EmptyCells.Count << 1) + Peer.PeersCountCandidates
			? null
			: CheckForTrueCandidateTypes(ref context) is { } trueCandidateTypeFirstFoundStep
				? trueCandidateTypeFirstFoundStep
				: SearchExtendedTypes && CheckForFalseCandidateTypes(ref context) is { } falseCandidateTypeFirstFoundStep
					? falseCandidateTypeFirstFoundStep
					: null;

	/// <summary>
	/// Check for types that uses true candidates.
	/// </summary>
	/// <param name="context"><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <returns><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	[InterceptorMethodCaller]
	private Step? CheckForTrueCandidateTypes(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		switch (TrueCandidate.GetAllTrueCandidates(grid))
		{
			case []:
			{
				return null;
			}
			case [var trueCandidate]:
			{
				// BUG + 1 found.
				var step = new BivalueUniversalGraveType1Step(
					Array.Single(new Conclusion(Assignment, trueCandidate)),
					[[new CandidateViewNode(ColorDescriptorAlias.Normal, trueCandidate)]],
					context.Options
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
				break;
			}
			case var trueCandidates:
			{
				if (checkForSingleDigit(trueCandidates))
				{
					if (CheckType2(ref context, trueCandidates) is { } type2Step)
					{
						return type2Step;
					}
				}
				else
				{
					if (SearchExtendedTypes)
					{
						if (CheckMultiple(ref context, trueCandidates) is { } typeMultipleStep)
						{
							return typeMultipleStep;
						}
						if (CheckXz(ref context, trueCandidates) is { } typeXzStep)
						{
							return typeXzStep;
						}
					}

					if (CheckType3Naked(ref context, trueCandidates) is { } type3Step)
					{
						return type3Step;
					}
					if (CheckType4(ref context, trueCandidates) is { } type4Step)
					{
						return type4Step;
					}
				}
				break;
			}
		}
		return null;


		static bool checkForSingleDigit(in CandidateMap trueCandidates)
		{
			var i = 0;
			Unsafe.SkipInit<Digit>(out var comparer);
			foreach (var trueCandidate in trueCandidates)
			{
				if (i++ == 0)
				{
					comparer = trueCandidate % 9;
					continue;
				}

				if (comparer != trueCandidate % 9)
				{
					return false;
				}
			}
			return true;
		}
	}

	/// <summary>
	/// Check for types that uses false candidates.
	/// </summary>
	/// <param name="context"><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <returns><inheritdoc cref="Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private BivalueUniversalGraveFalseCandidateTypeStep? CheckForFalseCandidateTypes(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var multivalueCells = EmptyCells & ~BivalueCells;
		if ((multivalueCells.PeerIntersection & EmptyCells) is not (var falseCandidatePossibleCells and not []))
		{
			// Optimization: The false candidates must lie in the intersection of all multi-value cells.
			// If the multi-value cells cannot confluent into any cells, no possible false candidates
			// will be found.
			return null;
		}

		foreach (var cell in falseCandidatePossibleCells)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				var copied = grid;
				copied.SetDigit(cell, digit);

				if (!formsDeadlyPattern(copied))
				{
					continue;
				}

				var step = new BivalueUniversalGraveFalseCandidateTypeStep(
					Array.Single(new Conclusion(Elimination, cell, digit)),
					[[.. from multiValueCell in multivalueCells select new CellViewNode(ColorDescriptorAlias.Normal, multiValueCell)]],
					context.Options,
					cell * 9 + digit
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
			}
		}

		return null;


		static bool formsDeadlyPattern(in Grid grid)
		{
			_ = grid is { BivalueCells: var bivalueCells, EmptyCells: var emptyCells, CandidatesMap: var candidatesMap };
			if (bivalueCells != emptyCells)
			{
				return false;
			}

			var housesCount = (stackalloc int[3]);
			foreach (var cell in emptyCells)
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					housesCount.Clear();
					foreach (var houseType in HouseTypes)
					{
						housesCount[(int)houseType] = (candidatesMap[digit] & HousesMap[cell.GetHouse(houseType)]).Count;
					}

					if (housesCount is not [2, 2, 2])
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	/// <summary>
	/// Check for type 2.
	/// </summary>
	private BivalueUniversalGraveType2Step? CheckType2(ref StepAnalysisContext context, in CandidateMap trueCandidates)
	{
		var cells = (stackalloc Cell[trueCandidates.Count]);
		var i = 0;
		foreach (var candidate in trueCandidates)
		{
			cells[i++] = candidate / 9;
		}

		var cellsMap = cells.AsCellMap();
		if (cellsMap.PeerIntersection is not (var map and not []))
		{
			return null;
		}

		var digit = trueCandidates[0] % 9;
		if ((map & CandidatesMap[digit]) is not (var elimMap and not []))
		{
			return null;
		}

		// BUG type 2.
		var step = new BivalueUniversalGraveType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, digit)).ToArray(),
			[[.. from candidate in trueCandidates select new CandidateViewNode(ColorDescriptorAlias.Normal, candidate)]],
			context.Options,
			digit,
			cellsMap
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for type 3 with naked subsets.
	/// </summary>
	private BivalueUniversalGraveType3Step? CheckType3Naked(ref StepAnalysisContext context, in CandidateMap trueCandidates)
	{
		ref readonly var grid = ref context.Grid;

		// Check whether all true candidates lie in a same house.
		var map = TargetCandidatesGroup.CreateMapByKeys(from c in trueCandidates group c by c / 9);
		if (map.FirstSharedHouse == FallbackConstants.@int)
		{
			return null;
		}

		// Get the digit mask.
		var digitsMask = (Mask)0;
		foreach (var candidate in trueCandidates)
		{
			digitsMask |= (Mask)(1 << candidate % 9);
		}

		// Iterate on each house that the true candidates lying on.
		foreach (var house in map.SharedHouses)
		{
			var houseMap = HousesMap[house];
			if ((houseMap & EmptyCells & ~map) is not (var otherCellsMap and not []))
			{
				continue;
			}

			// Iterate on each size.
			foreach (ref readonly var cells in otherCellsMap | otherCellsMap.Count - 1)
			{
				var mask = (Mask)(digitsMask | grid[cells]);
				if (BitOperations.PopCount((uint)mask) != cells.Count + 1)
				{
					continue;
				}

				if ((houseMap & ~cells & ~map & EmptyCells) is not (var elimMap and not []))
				{
					continue;
				}

				var conclusions = new List<Conclusion>();
				foreach (var cell in elimMap)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if ((mask >> digit & 1) != 0)
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
				foreach (var cand in trueCandidates)
				{
					candidateOffsets.Add(new(ColorDescriptorAlias.Normal, cand));
				}
				foreach (var cell in cells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorDescriptorAlias.Auxiliary1, cell * 9 + digit));
					}
				}

				var step = new BivalueUniversalGraveType3Step(
					conclusions.AsMemory(),
					[[.. candidateOffsets, new HouseViewNode(ColorDescriptorAlias.Normal, house)]],
					context.Options,
					trueCandidates,
					digitsMask,
					cells,
					true
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
			}
		}
		return null;
	}

	/// <summary>
	/// Check for type 4.
	/// </summary>
	private BivalueUniversalGraveType4Step? CheckType4(ref StepAnalysisContext context, in CandidateMap trueCandidates)
	{
		// Conjugate pairs should lie in two cells.
		var candsGroupByCell = from candidate in trueCandidates group candidate by candidate / 9;
		if (candsGroupByCell.Length != 2)
		{
			return null;
		}

		// Check two cell has same house.
		var cells = TargetCandidatesGroup.CreateMapByKeys(candsGroupByCell);
		if (cells is not ([var cell1, var cell2] and { SharedHouses: var houses and not 0 }))
		{
			return null;
		}

		// Check for each house.
		foreach (var house in houses)
		{
			// Add up all digits.
			var digits = (Mask)0;
			foreach (var candGroupByCell in candsGroupByCell)
			{
				foreach (var cand in candGroupByCell)
				{
					digits |= (Mask)(1 << cand % 9);
				}
			}

			// Check whether exists a conjugate pair in this house.
			for (var conjugatePairDigit = 0; conjugatePairDigit < 9; conjugatePairDigit++)
			{
				// Check whether forms a conjugate pair.
				var mask = (HousesMap[house] & CandidatesMap[conjugatePairDigit]) / house;
				if (BitOperations.PopCount((uint)mask) != 2)
				{
					continue;
				}

				// Check whether the conjugate pair lies in current two cells.
				var first = BitOperations.PopTwo((uint)mask, out var second);
				if (HousesCells[house][first] != cell1 || HousesCells[house][second] != cell2)
				{
					continue;
				}

				// Check whether all digits contain that digit.
				if ((digits >> conjugatePairDigit & 1) != 0)
				{
					continue;
				}

				// BUG type 4 found.
				// Now add up all eliminations.
				var conclusions = new List<Conclusion>();
				foreach (var candGroupByCell in candsGroupByCell)
				{
					var cell = candGroupByCell.Key;
					var digitMask = (Mask)0;
					foreach (var cand in candGroupByCell)
					{
						digitMask |= (Mask)(1 << cand % 9);
					}

					// Bitwise not.
					foreach (var d in digitMask = (Mask)(Grid.MaxCandidatesMask & ~digitMask))
					{
						if (conjugatePairDigit != d && CandidatesMap[d].Contains(cell))
						{
							conclusions.Add(new(Elimination, cell, d));
						}
					}
				}

				// Check eliminations.
				if (conclusions.Count == 0)
				{
					continue;
				}

				// BUG type 4.
				var step = new BivalueUniversalGraveType4Step(
					conclusions.AsMemory(),
					[
						[
							.. from candidate in trueCandidates select new CandidateViewNode(ColorDescriptorAlias.Normal, candidate),
							new CandidateViewNode(ColorDescriptorAlias.Auxiliary1, cell1 * 9 + conjugatePairDigit),
							new CandidateViewNode(ColorDescriptorAlias.Auxiliary1, cell2 * 9 + conjugatePairDigit),
							new ConjugateLinkViewNode(ColorDescriptorAlias.Normal, cell1, cell2, conjugatePairDigit)
						]
					],
					context.Options,
					Mask.Create(digits),
					cells,
					new(cell1, cell2, conjugatePairDigit)
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
			}
		}
		return null;
	}

	/// <summary>
	/// Check for BUG + n.
	/// </summary>
	private BivalueUniversalGraveMultipleStep? CheckMultiple(ref StepAnalysisContext context, in CandidateMap trueCandidates)
	{
		if (trueCandidates.Count > 18)
		{
			return null;
		}

		if (trueCandidates.PeerIntersection is not { Count: var mapCount and not 0 } map)
		{
			return null;
		}

		// BUG + n found.
		// Check eliminations.
		var conclusions = new List<Conclusion>(mapCount);
		foreach (var candidate in map)
		{
			if (CandidatesMap[candidate % 9].Contains(candidate / 9))
			{
				conclusions.Add(new(Elimination, candidate));
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		// BUG + n.
		var step = new BivalueUniversalGraveMultipleStep(
			conclusions.AsMemory(),
			[[.. from candidate in trueCandidates select new CandidateViewNode(ColorDescriptorAlias.Normal, candidate)]],
			context.Options,
			trueCandidates
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for BUG-XZ.
	/// </summary>
	private BivalueUniversalGraveXzStep? CheckXz(ref StepAnalysisContext context, in CandidateMap trueCandidates)
	{
		if (trueCandidates is not [var cand1, var cand2])
		{
			return null;
		}

		ref readonly var grid = ref context.Grid;
		var (c1, d1, c2, d2) = (cand1 / 9, cand1 % 9, cand2 / 9, cand2 % 9);
		var mask = (Mask)(1 << d1 | 1 << d2);
		foreach (var cell in (Peer.PeersMap[c1] ^ Peer.PeersMap[c2]) & BivalueCells)
		{
			if (grid.GetCandidates(cell) != mask)
			{
				continue;
			}

			// BUG-XZ found.
			var conclusions = new List<Conclusion>();
			var condition = (c1.AsCellMap() + cell).FirstSharedHouse != FallbackConstants.@int;
			var anotherCell = condition ? c2 : c1;
			var anotherDigit = condition ? d2 : d1;
			foreach (var peer in (cell.AsCellMap() + anotherCell).PeerIntersection)
			{
				if (CandidatesMap[anotherDigit].Contains(peer))
				{
					conclusions.Add(new(Elimination, peer, anotherDigit));
				}
			}
			if (conclusions.Count == 0)
			{
				continue;
			}

			var step = new BivalueUniversalGraveXzStep(
				conclusions.AsMemory(),
				[
					[
						new CellViewNode(ColorDescriptorAlias.Normal, cell),
						.. from candidate in trueCandidates select new CandidateViewNode(ColorDescriptorAlias.Normal, candidate)
					]
				],
				context.Options,
				mask,
				c1.AsCellMap() + c2,
				cell
			);
			if (context.OnlyFindOne)
			{
				return step;
			}

			context.Accumulator.Add(step);
		}
		return null;
	}
}
