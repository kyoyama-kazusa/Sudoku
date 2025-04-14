namespace Sudoku.Analytics.StepSearchers.Uniqueness;

using TargetCandidatesGroup = CellMapOrCandidateMapGrouping<CandidateMap, Candidate, Cell>;

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

	SupportedSudokuTypes = SudokuType.Standard,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
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
		=> CandidatesCount > (EmptyCells.Count << 1) + PeersCount + 8
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
					new SingletonArray<Conclusion>(new(Assignment, trueCandidate)),
					[[new CandidateViewNode(ColorIdentifier.Normal, trueCandidate)]],
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
					new SingletonArray<Conclusion>(new(Elimination, cell, digit)),
					[[.. from multiValueCell in multivalueCells select new CellViewNode(ColorIdentifier.Normal, multiValueCell)]],
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
						housesCount[(int)houseType] = (candidatesMap[digit] & HousesMap[cell.ToHouse(houseType)]).Count;
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
			[[.. from candidate in trueCandidates select new CandidateViewNode(ColorIdentifier.Normal, candidate)]],
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
		if (map.FirstSharedHouse == 32)
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
				if (Mask.PopCount(mask) != cells.Count + 1)
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
					candidateOffsets.Add(new(ColorIdentifier.Normal, cand));
				}
				foreach (var cell in cells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}
				}

				var step = new BivalueUniversalGraveType3Step(
					conclusions.AsMemory(),
					[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, house)]],
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
				if (Mask.PopCount(mask) != 2)
				{
					continue;
				}

				// Check whether the conjugate pair lies in current two cells.
				var first = Mask.TrailingZeroCount(mask);
				var second = mask.GetNextSet(first);
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
							.. from candidate in trueCandidates select new CandidateViewNode(ColorIdentifier.Normal, candidate),
							new CandidateViewNode(ColorIdentifier.Auxiliary1, cell1 * 9 + conjugatePairDigit),
							new CandidateViewNode(ColorIdentifier.Auxiliary1, cell2 * 9 + conjugatePairDigit),
							new ConjugateLinkViewNode(ColorIdentifier.Normal, cell1, cell2, conjugatePairDigit)
						]
					],
					context.Options,
					MaskOperations.Create(digits),
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
		// Test examples:
		// BUG + 2
		// 000000273300000004270004580069210030000070000050043920082100047700000009941000000
		// 6+2+3+4+587+1+9+71+89+2+3+46+5+9+54+67+1+3+820800+97+240+40006+250020003+4+100000+7498+2+1+19+72+8+5+6+348+4+2+3+1+6+95+7
		// 20+40+10080030+4600050090+7010+404+38+26+590005+1+9480+30000300063009+4+170+8+4080500000+100+83002:512 917 721 226 831 632 534 536 238 752 161 761 262 762 763 268 768 672 282 682 987 691 791 697 698
		// +46+1080+75+9+7+894+6+5+1+2+3300001+4+688300+142900040203+80900800+5+1+400700+86+4+500+36+40+8+71+6+4+8+15+7+93+2:234 272 974 282
		// +20008+3+76+9009072+1+343000005+2+800013+7+2+967+9+2+8+564+1+3+1002+9+4+85+702004+50+716003+2+89+4+5005+70008+2:412 522 132 133 634 841 442 192
		// 
		// BUG + 3
		// 3+1+6005+84+90704+100+26004+6801+70+1300+5+4+6906008300+140+471+9+605070+1+34+89+60+480+961030+9+6+3500+4+81:523 532 236 249 252 253
		// 100008500+504600020+600050+4072+5+690031+8+74+13+82+900+398+5+16+2+74+4020+936008000+60+7+9+2+9600+2004+3:312 714 126 332 174 186 194 796
		// 
		// BUG + 4
		// 7+9+30+402+80+100090+3+7005+83+70+100+3400109+5860+10+5+9+432+509+400+617+8109+640+23000007+800900080060:616 223 624 226 626 629 439 244 266 282 283 483 284 489 589 292 593 793 296 599
		// 
		// BUG + 5
		// +13+6000+85+9+5+79+13+80042+4+8+6+597+3+130+1002000+60+250010+3+75+4+30108+29+1+507+6+308+46+380+501+7+8+270+13000:414 444 945 647 947 955 697 997
		// ...356+7...+678+943+1..4.+2+7+1+86.87+5+96+2+4319..548+2+7642+6+13+7+958.5.+4.+3.2...26.95....+4725..+3:913 919 192
		// 8..597..6.+6+71+23....+51+68+47..34+2+7+5+9+8616+18+34+29+7557+9+8+6+1+342+1.6+43+52.+7...9+76...7..218+6.4:318 928 938
		// .2.+13+5.9.58+9+67+4+213.+3.2+98....+53+74+21+6.64.9+13.52.+12+58+63...+6.4+29.+3.29+5+36+7+481+37+4+85+1+92+6:719 733
		// ..+4.9.+2+5+3+7+354+128+6+9.92+3..71+4.2..4+9.3.4.+98.3.+27.6.+12.+49.+276+9+3+458+1+9+415+863+7+2...+27+1+9+4+6
		// x:...789...+8.4+62+37...9+7+4+5+1.8+34..5+78+9+3673+5+2+9+6+8419+8+61+34+5+27.7.+8+1+2.5.+1+48+96+53+7+2...347+1.+8:611 617 691
		// 
		// BUG + 6
		// .3.+8+9+2+65.5..6+34..8+6+8.571..+4+8457+6912+3..3+4+5+89+7+6+9671+2384+5.+5.986...3.+62.5.+89.9+8+3.+7+56.:122 223 277 777
		// ..7+18+39.+2..+17+29...8+2+9456+7+13.13+9.+852.9.2+3..8.1.86+2+1.37+91..892..7.+9.6.1+2..+2.8+53.1+9.:411 612 421 622 429 629 445 452 755 456 472 572 478 678 481 581 488 489 492
		// +9+125+364+8+7+68+4.+2.+53+97+5+38+94..21.5+9..7.4+4.+9.7....8+76.+4.9.32+6+73.9.+45+34+1.+5.+29.+5+984.23+7.:856 659
		// 
		// BUG + 7
		// 20040+6000+1+40090+2600+690+12+4+353+8+6+529+1+740701+64300010+3870500200705000010+480+23600+200040:313 513 717 818 819 723 823 971 873 981 592 393 593 396 997 799 899
		// ..+32+76..+5+28.5+13+47.+7.5+4+8+92+3.37+2+89+1+654..+17+34+9+2+889+4+62+5+713.+27+3.+85...3.9.7.4+2...1.2+3.+7:111 971 679 998

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
			[[.. from candidate in trueCandidates select new CandidateViewNode(ColorIdentifier.Normal, candidate)]],
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
		foreach (var cell in (PeersMap[c1] ^ PeersMap[c2]) & BivalueCells)
		{
			if (grid.GetCandidates(cell) != mask)
			{
				continue;
			}

			// BUG-XZ found.
			var conclusions = new List<Conclusion>();
			var condition = (c1.AsCellMap() + cell).FirstSharedHouse != 32;
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
						new CellViewNode(ColorIdentifier.Normal, cell),
						.. from candidate in trueCandidates select new CandidateViewNode(ColorIdentifier.Normal, candidate)
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
