#define JUNIOR_EXOCET
#define SENIOR_EXOCET
#define WEAK_EXOCET
#define DOUBLE_EXOCET
#define COMPLEX
#define COMPLEX_JUNIOR_EXOCET
#define COMPLEX_SENIOR_EXOCET
#define ADVANCED
#define ADVANCED_SENIOR_EXOCET
#define ADVANCED_COMPLEX_JUNIOR_EXOCET
#define ADVANCED_COMPLEX_SENIOR_EXOCET
#undef SIZE_ONLY_THREE
#undef BASE_SIZE_ONLY_TWO
#if (COMPLEX_JUNIOR_EXOCET || COMPLEX_SENIOR_EXOCET) && !COMPLEX
#line 1 "ExocetStepSearcher.cs"
#warning 'COMPLEX' should be set if 'COMPLEX_JUNIOR_EXOCET' or 'COMPLEX_SENIOR_EXOCET' is set.
#line default
#endif
#if (ADVANCED_COMPLEX_JUNIOR_EXOCET || ADVANCED_COMPLEX_SENIOR_EXOCET) && !ADVANCED
#line 1 "ExocetStepSearcher.cs"
#warning 'ADVANCED' should be set if 'ADVANCED_COMPLEX_JUNIOR_EXOCET' or 'ADVANCED_COMPLEX_SENIOR_EXOCET' is set.
#line default
#endif

// TODO: There are some types that are not implemented.
// * True base for Complex Senior Exocet
// * Double Exocet the other type
// * Advanced Senior Exocet
// * Weak Exocet with some other value cells

namespace Sudoku.Analytics.StepSearchers.Exocets;

using TargetCellsGroup = CellMapOrCandidateMapGrouping<CellMap, Cell, House>;

/// <summary>
/// Provides with an <b>Exocet</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Junior Exocet</item>
/// <item>Weak Exocet</item>
/// <item>Lame Weak Exocet</item>
/// <item>Double Exocet</item>
/// <item>Senior Exocet</item>
/// <item>Complex Junior Exocet</item>
/// <item>Complex Senior Exocet</item>
/// <item>Advanced Complex Junior Exocet</item>
/// <item>Advanced Complex Senior Exocet</item>
/// <!--<item>Pattern-Locked Quadruple</item>-->
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_ExocetStepSearcher",

	// Junior Exocets
	Technique.JuniorExocet,
	Technique.JuniorExocetConjugatePair, Technique.JuniorExocetMirrorConjugatePair, Technique.JuniorExocetAdjacentTarget,
	Technique.JuniorExocetIncompatiblePair, Technique.JuniorExocetTargetPair, Technique.JuniorExocetGeneralizedFish,
	Technique.JuniorExocetMirrorAlmostHiddenSet, Technique.JuniorExocetLockedMember,

	// Senior Exocets
	Technique.SeniorExocet,
	Technique.SeniorExocetMirrorConjugatePair, Technique.SeniorExocetLockedMember, Technique.SeniorExocetTrueBase,

	// Weak Exocets
	Technique.WeakExocet, Technique.WeakExocetAdjacentTarget, Technique.WeakExocetSlash, Technique.WeakExocetBzRectangle,
	Technique.LameWeakExocet,

	// Double Exocets
	Technique.DoubleExocet, Technique.DoubleExocetGeneralizedFish,

	// Complex Exocets
	Technique.FrankenJuniorExocet, Technique.FrankenSeniorExocet, Technique.MutantJuniorExocet, Technique.MutantSeniorExocet,
	Technique.FrankenJuniorExocetLockedMember, Technique.MutantJuniorExocetLockedMember,
	Technique.FrankenSeniorExocetLockedMember, Technique.MutantSeniorExocetLockedMember,
	Technique.FrankenJuniorExocetAdjacentTarget, Technique.MutantJuniorExocetAdjacentTarget,

	// Advanced Exocets
	Technique.AdvancedFrankenSeniorExocet, Technique.AdvancedMutantSeniorExocet,

	// Miscellaneous
	Technique.PatternLockedQuadruple,

	RuntimeFlags = StepSearcherRuntimeFlags.TimeComplexity)]
public sealed partial class ExocetStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		var chuteIndexBox = (stackalloc int[3]);
		foreach (var isRow in (true, false))
		{
#if !SIZE_ONLY_THREE && (COMPLEX_SENIOR_EXOCET || COMPLEX_JUNIOR_EXOCET)
			// Iterate by size of houses to be iterated. The possible values should be 3 or 4,
			// but we can allow on size = 2 for Complex Senior Exocets.
			for (var size = 2; size <= 4; size++)
#endif
			{
#if SIZE_ONLY_THREE
				const int size = 3;
#endif
				// Iterate on each combination of houses as basic cross-line cells used.
				foreach (var houses in (isRow ? HouseMaskOperations.AllRowsMask : HouseMaskOperations.AllColumnsMask).AllSets.GetSubsets(size))
				{
					var (housesEmptyCells, housesCells, housesMask) = (CellMap.Empty, CellMap.Empty, HouseMaskOperations.Create(houses));
					foreach (var house in houses)
					{
						housesEmptyCells |= HousesMap[house] & EmptyCells;
						housesCells |= HousesMap[house];
					}

					// We manually disable the case that 3 of 3 (or 4) houses in a same chute.
					chuteIndexBox.Clear();
					foreach (var house in houses)
					{
						chuteIndexBox[(isRow ? house - 9 : house - 18) / 3]++;
					}
					if (chuteIndexBox.Contains(3))
					{
						// This case is invalid.
						continue;
					}

					// Iterate on each chute (mega rows or columns) in order to check for each empty cell,
					// determining whether it can be used as a base.
					for (var (i, timesOfI) = (isRow ? 3 : 0, 0); timesOfI < 3; i++, timesOfI++)
					{
						var (_, _, chuteHouses) = Chutes[i];
						ref readonly var chuteCells = ref ChuteMaps[i];

						// Now iterate by size of base cells. The minimum value is 1, e.g.:
						//   ..64.....1....39.7.5.............3..2....1.89....59....4......83....2....126...7.
						// For digits 4 & 5 in houses r258, with base cell r9c7 and target cell r2c8 => r2c8 must be 4 or 5.
#if !BASE_SIZE_ONLY_TWO
						for (var baseSize = 1; baseSize <= 2; baseSize++)
#endif
						{
#if BASE_SIZE_ONLY_TWO
							const int baseSize = 2;
#endif
							// Iterate on each empty cells, or a cell group whose length is equal to iteration variable 'baseCellsSize'.
							foreach (ref readonly var minilineBaseCells in Miniline.MinilinesGroupedByChuteIndex[i].AsReadOnlySpan())
							{
								// Set cancellation token handling logic here.
								context.CancellationToken.ThrowIfCancellationRequested();

								if ((minilineBaseCells & EmptyCells) is not (var baseEmptyCellsToBeIterated and not []))
								{
									// No cells can be iterated.
									continue;
								}

								// Iterate on each miniline, to get all possible cases.
								foreach (ref readonly var baseCells in baseEmptyCellsToBeIterated & baseSize)
								{
									if (housesEmptyCells & baseCells)
									{
										// Base cells shouldn't be located in the current list of houses being iterated.
										continue;
									}

									// Now we should check for target cells.
									// The target cells must be located in houses being iterated, and intersects with the current chute.
									var chuteEmptyCells = chuteCells & EmptyCells;
									var targetCells = chuteEmptyCells & housesEmptyCells & ~baseCells.PeerIntersection;
									var (baseCellsDigitsMask, targetCellsDigitsMask) = (grid[baseCells], grid[targetCells]);

									// Check whether all cross-line lines contains at least one digit appeared in base cells.
									var crossline = housesCells & ~chuteCells;
									var atLeastOneLineContainNoDigitsAppearedInBase = false;
									foreach (var line in isRow ? crossline.RowMask << 9 : crossline.ColumnMask << 18)
									{
										var lineMask = grid[HousesMap[line] & crossline & EmptyCells];
										if ((lineMask & baseCellsDigitsMask) == 0)
										{
											atLeastOneLineContainNoDigitsAppearedInBase = true;
											break;
										}
									}
									if (atLeastOneLineContainNoDigitsAppearedInBase)
									{
										continue;
									}

									// Check whether escape cells contain any digits appeared in base. If so, invalid.
									var escapeCellsContainValueCellsDigitAppearedInBaseCells = false;
									foreach (var cell in housesCells & ~crossline & ~EmptyCells)
									{
										if ((baseCellsDigitsMask >> grid.GetDigit(cell) & 1) != 0)
										{
											escapeCellsContainValueCellsDigitAppearedInBaseCells = true;
											break;
										}
									}
									if (escapeCellsContainValueCellsDigitAppearedInBaseCells)
									{
										continue;
									}

									var groupsOfTargetCells = targetCells.GroupTargets(housesMask);

									// Check whether all groups of target cells don't exceed the maximum limit, 2 cells.
									var containsAtLeastOneGroupMoreThanTwoCells = false;
									foreach (ref readonly var element in groupsOfTargetCells)
									{
										if (element.Count > 2)
										{
											containsAtLeastOneGroupMoreThanTwoCells = true;
											break;
										}
									}
									if (containsAtLeastOneGroupMoreThanTwoCells)
									{
										continue;
									}

									// Check whether target cells don't share a same block.
									if (!(targetCells.Count == 1 || targetCells.Count == 2 && !Mask.IsPow2(targetCells.BlockMask)))
									{
										continue;
									}

									// Collect exocets by types.
									// We separate exocets with 3 parts:
									//   * Junior Exocet, Double Exocet, Weak Exocet, (Advanced) Complex Junior Exocet
									//   * Senior Exocet
									//   * (Advanced) Complex Senior Exocet
									// because the pre-condition are not same with each other.

#if SENIOR_EXOCET || COMPLEX_SENIOR_EXOCET
									if (baseCells.Count == 2)
									{
										foreach (var targetCell in targetCells)
										{
#if SENIOR_EXOCET
											if (CollectSeniorExocets(
												ref context, grid, baseCells, targetCell, groupsOfTargetCells, crossline,
												baseCellsDigitsMask, housesMask, isRow, size, i
											) is { } seniorExocet)
											{
												return seniorExocet;
											}
#endif
#if COMPLEX_SENIOR_EXOCET
											if (CollectComplexSeniorExocets(
												ref context, grid, baseCells, targetCell, crossline,
												baseCellsDigitsMask, housesMask, isRow, size, i, in housesCells
											) is { } complexSeniorExocet)
											{
												return complexSeniorExocet;
											}
#endif
										}
									}
#endif

#if WEAK_EXOCET || JUNIOR_EXOCET || DOUBLE_EXOCET || COMPLEX_JUNIOR_EXOCET
									if (groupsOfTargetCells.Length == baseSize)
									{
										if (!grid.CheckTargetCellsDigitsValidity(targetCells, baseCellsDigitsMask))
										{
											continue;
										}

#if WEAK_EXOCET
										if (CollectWeakExocets(
											ref context, grid, baseCells, targetCells, groupsOfTargetCells, crossline,
											baseCellsDigitsMask, housesMask, isRow, size, i
										) is { } weakExocet)
										{
											return weakExocet;
										}
#endif
#if JUNIOR_EXOCET
										if (CollectJuniorExocets(
											ref context, grid, baseCells, targetCells, groupsOfTargetCells, crossline,
											baseCellsDigitsMask, housesMask, isRow, size, i
										) is { } juniorExocet)
										{
											return juniorExocet;
										}
#endif
#if DOUBLE_EXOCET
										if (CollectDoubleExocets(
											ref context, grid, baseCells, targetCells, groupsOfTargetCells, crossline,
											housesEmptyCells, baseCellsDigitsMask, housesMask, isRow, size, i
										) is { } doubleExocet)
										{
											return doubleExocet;
										}
#endif
#if COMPLEX_JUNIOR_EXOCET
										if (CollectComplexJuniorExocets(
											ref context, grid, baseCells, targetCells, groupsOfTargetCells, crossline,
											baseCellsDigitsMask, housesMask, isRow, size, i, in housesCells
										) is { } complexJuniorExocet)
										{
											return complexJuniorExocet;
										}
#endif
									}
#endif
								}
							}
						}
					}
				}
			}
		}

		return null;
	}

	//
	// Sub-type Entry
	//
#if JUNIOR_EXOCET
	/// <summary>
	/// The core method to check for Junior Exocet sub-types.
	/// </summary>
	/// <param name="context">The analysis context to be used for accumulate steps.</param>
	/// <param name="grid">The grid as the candidate referencing object.</param>
	/// <param name="baseCells">The cells of the base.</param>
	/// <param name="targetCells">The cells of the target. The target cell only contains empty cells; non-empty ones will be ignored.</param>
	/// <param name="groupsOfTargetCells">
	/// The grouped cells for the argument <paramref name="targetCells"/>, by is containing crossline house.
	/// </param>
	/// <param name="crossline">
	/// The cells of cross-line. The cross-line cells contain 18 cells regardless of its state: empty or non-empty.
	/// </param>
	/// <param name="baseCellsDigitsMask">The mask that holds a list of digits that are appeared in base cells.</param>
	/// <param name="housesMask">The mask that holds a list of houses being iterated.</param>
	/// <param name="isRow">Indicates whether the exocet pattern is row-ish. The direction is same as cross-line cells.</param>
	/// <param name="size">
	/// The size of houses used in the cross-line cells. The value can be 3 or 4; higher-sized cases cannot be determined.
	/// I don't know. :(
	/// </param>
	/// <param name="chuteIndex">The chute index to be used. Valid values are between 0 and 6.</param>
	/// <returns>A valid <see cref="ExocetStep"/> instance calculated and found.</returns>
	private static ExocetStep? CollectJuniorExocets(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		bool isRow,
		int size,
		int chuteIndex
	)
	{
		// Check for maximum times can be appeared in cross-line cells,
		// and collect all possible digits can be filled in cross-line for (size - 1) times at most.
		// Here will cause an conflict: Locked Members will make digits appeared in base cells
		// appear in cross-line cells as values (i.e. non-empty cells).
		// However, sometimes, we may encounter a case that digits appeared in base cells indeed appears
		// in cross-line cells as values, but they are non-locked members.
		// Here is an example:
		//   98.7.....6.....97...7.....54...3..2...86..4.......4..1..68..5......1...4.....2.3.
		// We should treat the digit as a non-locked member.
		if (!CheckValidityAndLockedMembersExistence(
			grid, baseCellsDigitsMask, baseCells, targetCells, crossline, size - 1, out var digitsMaskExactlySizeMinusOneTimes,
			out var digitsMaskAppearedInCrossline, out var lockedMembers, out var lockedMemberDigitsMask))
		{
			return null;
		}

		// Check whether locked members and normal digits satisfied the (size - 1) rule equal to base cells digits mask.
		if ((Mask)(digitsMaskExactlySizeMinusOneTimes | lockedMemberDigitsMask) != baseCellsDigitsMask)
		{
			return null;
		}

		if (CheckMirrorSync(
			ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
			housesMask, chuteIndex, digitsMaskAppearedInCrossline
		) is { } mirrorSyncTypeStep)
		{
			return mirrorSyncTypeStep;
		}

		switch (BitOperations.PopCount(lockedMemberDigitsMask))
		{
			case 1 or 2:
			{
				if (CheckJuniorLockedMember(
					ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
					lockedMembers, chuteIndex, groupsOfTargetCells, out _, out var lockedDigitsMask
				) is { } lockedMemberTypeStep)
				{
					return lockedMemberTypeStep;
				}

				// Check whether the locked members are really used.
				// If not, we should check for them, whether they are appeared in cross-line cells in (size - 1) times.
				var lockedMembersAreSatisfySizeMinusOneRule = true;
				foreach (var digit in lockedDigitsMask)
				{
					if (!grid.IsExactAppearingTimesOf(digit, crossline, size - 1))
					{
						// The current digit can be filled in cross-line cells at most (size - 1) times.
						lockedMembersAreSatisfySizeMinusOneRule = false;
						break;
					}
				}
				if (lockedMembersAreSatisfySizeMinusOneRule)
				{
					goto case 0;
				}
				break;
			}
			case 0:
			{
				if (CheckJuniorOrSeniorBase(
					ref context, grid, baseCells, targetCells, -1, crossline, [], baseCellsDigitsMask,
					housesMask, out var inferredTargetConjugatePairs
				) is { } baseTypeStep)
				{
					return baseTypeStep;
				}

				if (CheckMirrorConjugatePair(
					ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
					isRow, chuteIndex, housesMask
				) is { } mirrorTypeStep)
				{
					return mirrorTypeStep;
				}

				if (CheckAdjacentTarget(
					ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
					isRow, chuteIndex, housesMask
				) is { } singleMirrorTypeStep)
				{
					return singleMirrorTypeStep;
				}

				if (CheckIncompatiblePair(
					ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
					housesMask, digitsMaskAppearedInCrossline, out var inferredTargetPairMask
				) is { } incompatiblePairTypeStep)
				{
					return incompatiblePairTypeStep;
				}

				if (CheckTargetPair(
					ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
					inferredTargetPairMask, housesMask, inferredTargetConjugatePairs
				) is { } targetPairTypeStep)
				{
					return targetPairTypeStep;
				}

				if (CheckGeneralizedFish(
					ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
					inferredTargetPairMask, isRow, housesMask
				) is { } generalizedFishTypeStep)
				{
					return generalizedFishTypeStep;
				}

				if (CheckMirrorAlmostHiddenSet(
					ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
					isRow, housesMask, chuteIndex
				) is { } mirrorAhsTypeStep)
				{
					return mirrorAhsTypeStep;
				}

				break;
			}
		}

		return null;
	}
#endif

#if SENIOR_EXOCET
	/// <summary>
	/// The core method to check for Senior Exocet sub-types.
	/// </summary>
	/// <param name="context">The analysis context to be used for accumulate steps.</param>
	/// <param name="grid">The grid as the candidate referencing object.</param>
	/// <param name="baseCells">The cells of the base.</param>
	/// <param name="targetCell">The cell of the target. The target cell only contains empty cells; non-empty ones will be ignored.</param>
	/// <param name="groupsOfTargetCells">The grouped cells for target cells, by is containing crossline house.</param>
	/// <param name="crossline">
	/// The cells of cross-line. The cross-line cells contain 18 cells regardless of its state: empty or non-empty.
	/// </param>
	/// <param name="baseCellsDigitsMask">The mask that holds a list of digits that are appeared in base cells.</param>
	/// <param name="housesMask">The mask that holds a list of houses being iterated.</param>
	/// <param name="isRow">Indicates whether the exocet pattern is row-ish. The direction is same as cross-line cells.</param>
	/// <param name="size">
	/// The size of houses used in the cross-line cells. The value can be 3 or 4; higher-sized cases cannot be determined
	/// because I don't know. :(
	/// </param>
	/// <param name="chuteIndex">The chute index to be used. Valid values are between 0 and 6.</param>
	/// <returns>A valid <see cref="ExocetStep"/> instance calculated and found.</returns>
	private static ExocetStep? CollectSeniorExocets(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		bool isRow,
		int size,
		int chuteIndex
	)
	{
		var crosslineIncludingTarget = CellMap.Empty;
		foreach (var house in housesMask)
		{
			crosslineIncludingTarget |= HousesMap[house] & ~baseCells.PeerIntersection;
		}

		// Try to fetch all possible endo-target cells if worth.
		// Check whether cross-line non-empty cells contains digits appeared in base cells.
		// If so, they will be endo-target cells.
		// The maximum possible number of appearing times is 2, corresponding to the real target cells count.
		var endoTargetValueDigitsMask = grid.GetValueDigitsAppearedInCrossline(crosslineIncludingTarget - targetCell, baseCellsDigitsMask);
		switch (BitOperations.PopCount(endoTargetValueDigitsMask))
		{
			case 0:
			{
				if (CheckSeniorExocetNoValueCells(
					ref context, grid, baseCells, targetCell, groupsOfTargetCells, crossline, crosslineIncludingTarget,
					baseCellsDigitsMask, housesMask, size, chuteIndex
				) is { } endoTargetMustBeTrueTypeStep)
				{
					return endoTargetMustBeTrueTypeStep;
				}
				break;
			}
			case 1:
			{
				// Check for maximum times can be appeared in cross-line cells.
				// Due to consideration on locked members, we may not handle for them.
				var allDigitsCanBeFilledExactlySizeMinusOneTimes = true;
				foreach (var digit in (Mask)(baseCellsDigitsMask & ~endoTargetValueDigitsMask))
				{
					if (!grid.IsExactAppearingTimesOf(digit, crosslineIncludingTarget - targetCell, size - 1))
					{
						allDigitsCanBeFilledExactlySizeMinusOneTimes = false;
						break;
					}
				}
				if (!allDigitsCanBeFilledExactlySizeMinusOneTimes)
				{
					// All digits should strictly appear (size - 1) times at most in cross-line cells.
					// For example, if the size = 3, digits should only appear 2 times at most in cross-line cells.
					// If greater (times > size - 1), an exocet cannot be formed;
					// and if less (times < size - 1), we cannot conclude which digits are the target cells.
					return null;
				}

				if (CheckSeniorTrueBase(
					ref context, grid, baseCells, targetCell, crossline, crosslineIncludingTarget, baseCellsDigitsMask,
					BitOperations.TrailingZeroCount(endoTargetValueDigitsMask)
				) is { } lockedMemberTypeStep)
				{
					return lockedMemberTypeStep;
				}

				break;
			}
		}

		return null;
	}
#endif

#if COMPLEX_JUNIOR_EXOCET
	/// <summary>
	/// The core method to check for Complex Junior Exocet sub-types.
	/// </summary>
	/// <param name="context">The analysis context to be used for accumulate steps.</param>
	/// <param name="grid">The grid as the candidate referencing object.</param>
	/// <param name="baseCells">The cells of the base.</param>
	/// <param name="targetCells">The cells of the target. The target cell only contains empty cells; non-empty ones will be ignored.</param>
	/// <param name="groupsOfTargetCells">
	/// The grouped cells for the argument <paramref name="targetCells"/>, by is containing crossline house.
	/// </param>
	/// <param name="crossline">
	/// The cells of cross-line. The cross-line cells contain 18 cells regardless of its state: empty or non-empty.
	/// </param>
	/// <param name="baseCellsDigitsMask">The mask that holds a list of digits that are appeared in base cells.</param>
	/// <param name="housesMask">The mask that holds a list of houses being iterated.</param>
	/// <param name="isRow">Indicates whether the exocet pattern is row-ish. The direction is same as cross-line cells.</param>
	/// <param name="size">
	/// The size of houses used in the cross-line cells. The value can be 3 or 4; higher-sized cases cannot be determined
	/// because I don't know. :(
	/// </param>
	/// <param name="chuteIndex">The chute index to be used. Valid values are between 0 and 6.</param>
	/// <param name="housesCells">The houses cells to be used for the base houses.</param>
	/// <returns>A valid <see cref="ExocetStep"/> instance calculated and found.</returns>
	private static ExocetStep? CollectComplexJuniorExocets(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		bool isRow,
		int size,
		int chuteIndex,
		in CellMap housesCells
	)
	{
		if (GetComplexHouses(isRow, baseCells) is not (var extraHousesMask and not 0))
		{
			// No houses will be iterated.
			return null;
		}

		if (!grid.CheckTargetCellsDigitsValidity(targetCells, baseCellsDigitsMask))
		{
			// The target cells don't contain enough candidates that can match base cells.
			return null;
		}

		// Now iterate on each combination of extra houses.
		foreach (var extraHouse in extraHousesMask.AllSets)
		{
			// Create a map that contains both cross-line cells and extra houses cells.
			var expandedCrosslineIncludingTarget = (housesCells | HousesMap[extraHouse]) & ~baseCells.PeerIntersection;
			var intersectedCellsBase = housesCells & HousesMap[extraHouse] & ~targetCells & ~baseCells.PeerIntersection;

			// Check whether all intersected cells by original cross-line cells and extra house cells are non-empty,
			// except endo-target cells; and cannot be of value appeared in base cells.
			if (grid.CheckCrossLineIntersectionLeaveEmpty(intersectedCellsBase, baseCellsDigitsMask))
			{
				continue;
			}

			// Check for locked members.
			CheckValidityAndLockedMembersExistence(
				grid,
				baseCellsDigitsMask,
				baseCells,
				targetCells,
				expandedCrosslineIncludingTarget & ~targetCells,
				size,
				out var digitsMaskExactlySizeMinusOneTimes,
				out _,
				out var lockedMembers,
				out var lockedMemberDigitsMask
			);

			// Check whether the satisfied digits and locked member digits are strictly equal to base cells digits mask.
			if ((Mask)(lockedMemberDigitsMask | digitsMaskExactlySizeMinusOneTimes) != baseCellsDigitsMask)
			{
				continue;
			}

			// Check for basic cases here.
			// Check for locked members and determine the next step.
			switch (BitOperations.PopCount(lockedMemberDigitsMask))
			{
				case 1:
				{
					if (CheckComplexJuniorLockedMember(
						ref context, grid, baseCells, targetCells, crossline, baseCellsDigitsMask,
						housesMask, 1 << extraHouse, size, in expandedCrosslineIncludingTarget, lockedMembers, chuteIndex,
						groupsOfTargetCells, out _, out var lockedDigitsMask
					) is { } complexJuniorLockedMemberTypeStep)
					{
						return complexJuniorLockedMemberTypeStep;
					}

					// Check whether the locked members are really used.
					// If not, we should check for them, whether they are appeared in cross-line cells in (size) times.
					var lockedMembersAreSatisfySizeMinusOneRule = true;
					foreach (var digit in lockedDigitsMask)
					{
						if (!grid.IsExactAppearingTimesOf(digit, expandedCrosslineIncludingTarget, size))
						{
							// The current digit can be filled in cross-line cells at most (size) times.
							lockedMembersAreSatisfySizeMinusOneRule = false;
							break;
						}
					}
					if (lockedMembersAreSatisfySizeMinusOneRule)
					{
						goto case 0;
					}
					break;
				}
				case 0:
				{
					if (CheckComplexJuniorBase(
						ref context, grid, baseCells, targetCells, crossline,
						baseCellsDigitsMask, housesMask, 1 << extraHouse, size, in expandedCrosslineIncludingTarget
					) is { } complexJuniorTypeStep)
					{
						return complexJuniorTypeStep;
					}

					if (CheckComplexJuniorAdjacentTarget(
						ref context, grid, baseCells, targetCells, expandedCrosslineIncludingTarget,
						baseCellsDigitsMask, isRow, chuteIndex, housesMask, 1 << extraHouse, groupsOfTargetCells
					) is { } complexJuniorAdjacentTargetTypeStep)
					{
						return complexJuniorAdjacentTargetTypeStep;
					}

					if (CheckComplexJuniorMirrorConjugatePair(
						ref context, grid, baseCells, targetCells, expandedCrosslineIncludingTarget,
						baseCellsDigitsMask, isRow, chuteIndex, housesMask, 1 << extraHouse, groupsOfTargetCells
					) is { } complexJuniorMirrorConjugatePairTypeStep)
					{
						return complexJuniorMirrorConjugatePairTypeStep;
					}
					break;
				}
			}
		}

		return null;
	}
#endif

#if COMPLEX_SENIOR_EXOCET
	/// <summary>
	/// The core method to check for Complex Senior Exocet sub-types.
	/// </summary>
	/// <param name="context">The analysis context to be used for accumulate steps.</param>
	/// <param name="grid">The grid as the candidate referencing object.</param>
	/// <param name="baseCells">The cells of the base.</param>
	/// <param name="targetCell">The cell of the target. The target cell only contains empty cells; non-empty ones will be ignored.</param>
	/// <param name="crossline">
	/// The cells of cross-line. The cross-line cells contain 18 cells regardless of its state: empty or non-empty.
	/// </param>
	/// <param name="baseCellsDigitsMask">The mask that holds a list of digits that are appeared in base cells.</param>
	/// <param name="housesMask">The mask that holds a list of houses being iterated.</param>
	/// <param name="isRow">Indicates whether the exocet pattern is row-ish. The direction is same as cross-line cells.</param>
	/// <param name="size">
	/// The size of houses used in the cross-line cells. The value can be 3 or 4; higher-sized cases cannot be determined
	/// because I don't know. :(
	/// </param>
	/// <param name="chuteIndex">The chute index to be used. Valid values are between 0 and 6.</param>
	/// <param name="housesCells">The houses cells to be used for the base houses.</param>
	/// <returns>A valid <see cref="ExocetStep"/> instance calculated and found.</returns>
	private static ExocetStep? CollectComplexSeniorExocets(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		bool isRow,
		int size,
		int chuteIndex,
		in CellMap housesCells
	)
	{
		if (GetComplexHouses(isRow, baseCells) is not (var extraHousesMask and not 0))
		{
			// No houses will be iterated.
			return null;
		}

		// Now iterate on each combination of extra houses.
		foreach (var extraHouse in extraHousesMask.AllSets)
		{
			// Create a map that contains both cross-line cells and extra houses cells.
			var expandedCrossline = (crossline | HousesMap[extraHouse]) & ~baseCells.PeerIntersection;
			var expandedCrosslineIncludingTarget = (housesCells | HousesMap[extraHouse]) & ~baseCells.PeerIntersection;
			var intersectedCellsBase = (housesCells & HousesMap[extraHouse]) - targetCell & ~baseCells.PeerIntersection;

			// Iterate on each empty cells in the above map, to get the other target cell.
			foreach (var endoTargetCell in expandedCrossline & EmptyCells)
			{
				if (Mask.IsPow2((targetCell.AsCellMap() + endoTargetCell).BlockMask))
				{
					// The target selected endo-target cell cannot share a same block with target cell.
					continue;
				}

				if (!grid.CheckTargetCellsDigitsValidity([targetCell, endoTargetCell], baseCellsDigitsMask))
				{
					continue;
				}

				// Check whether all intersected cells by original cross-line cells and extra house cells are non-empty,
				// except endo-target cells; and cannot be of value appeared in base cells.
				if (grid.CheckCrossLineIntersectionLeaveEmpty(intersectedCellsBase - endoTargetCell, baseCellsDigitsMask))
				{
					continue;
				}

				// Check for locked members.
				CheckValidityAndLockedMembersExistence(
					grid,
					baseCellsDigitsMask,
					baseCells,
					in targetCell.AsCellMap(),
					expandedCrosslineIncludingTarget - targetCell - endoTargetCell,
					size,
					out var digitsMaskExactlySizeMinusOneTimes,
					out _,
					out var lockedMembers,
					out var lockedMemberDigitsMask
				);

				// Check whether the satisfied digits and locked member digits are strictly equal to base cells digits mask.
				if ((Mask)(lockedMemberDigitsMask | digitsMaskExactlySizeMinusOneTimes) == baseCellsDigitsMask)
				{
					// Check for basic cases here.
					// Check for locked members and determine the next step.
					switch (BitOperations.PopCount(lockedMemberDigitsMask))
					{
						case 1:
						{
							if (CheckComplexSeniorLockedMember(
								ref context, grid, baseCells, targetCell, endoTargetCell, crossline, baseCellsDigitsMask,
								housesMask, 1 << extraHouse, size, in expandedCrosslineIncludingTarget, lockedMembers, chuteIndex,
								out _, out var lockedDigitsMask
							) is { } complexSeniorLockedMemberTypeStep)
							{
								return complexSeniorLockedMemberTypeStep;
							}

							// Check whether the locked members are really used.
							// If not, we should check for them, whether they are appeared in cross-line cells in (size) times.
							var lockedMembersAreSatisfySizeMinusOneRule = true;
							foreach (var digit in lockedDigitsMask)
							{
								if (!grid.IsExactAppearingTimesOf(digit, expandedCrosslineIncludingTarget - endoTargetCell, size))
								{
									// The current digit can be filled in cross-line cells at most (size) times.
									lockedMembersAreSatisfySizeMinusOneRule = false;
									break;
								}
							}
							if (lockedMembersAreSatisfySizeMinusOneRule)
							{
								goto case 0;
							}
							break;
						}
						case 0:
						{
							if (CheckComplexSeniorBase(
								ref context, grid, baseCells, targetCell, endoTargetCell, crossline,
								baseCellsDigitsMask, housesMask, 1 << extraHouse, size, in expandedCrosslineIncludingTarget
							) is { } complexSeniorTypeStep)
							{
								return complexSeniorTypeStep;
							}
							break;
						}
					}
				}
				else
				{
					// Consider for the case that complex senior exocet will use extra target cells to form an AHS or conjugate pair.
					// We should here append an loop for checking for it.
					// Check for extra digits appeared in current selected endo-target cell.
					// This will help us searching for possible AHSes or conjugate pairs.
					// Iterate on each digit to check on any AHSes or conjugate pairs.
					var collectedDigitsMask = (Mask)0; // This field is used for remove searching for duplicate digits.
					foreach (var selectedDigit in (Mask)(grid.GetCandidates(endoTargetCell) & ~baseCellsDigitsMask))
					{
						// Check for all possible direction for the endo-target cell, to get intersected cells with cross-line cells.
						foreach (var houseType in HouseTypes)
						{
							var endoTargetCellHouse = endoTargetCell.ToHouse(houseType);
							if ((housesMask >> endoTargetCellHouse & 1) == 0 && extraHouse != endoTargetCellHouse)
							{
								// The current house is not valid to be iterated.
								continue;
							}

							switch (expandedCrosslineIncludingTarget & HousesMap[endoTargetCellHouse] & CandidatesMap[selectedDigit])
							{
								case { Count: < 2 }:
								{
									// This digit does not contain any possible cell to form an AHS or a conjugate pair.
									continue;
								}
								case { Count: 2 } endoTargetCellsGroup when !endoTargetCellsGroup.PeerIntersection.Contains(targetCell):
								{
									// This will include a conjugate pair.
									if ((HousesMap[endoTargetCellHouse] & CandidatesMap[selectedDigit]) != endoTargetCellsGroup)
									{
										continue;
									}

									// Because we have already modified the endo-target cells to a list of cells,
									// we should re-check here.
									CheckValidityAndLockedMembersExistence(
										grid,
										baseCellsDigitsMask,
										baseCells,
										in targetCell.AsCellMap(),
										expandedCrosslineIncludingTarget - targetCell & ~endoTargetCellsGroup,
										size,
										out digitsMaskExactlySizeMinusOneTimes,
										out _,
										out lockedMembers,
										out lockedMemberDigitsMask
									);

									if ((Mask)(digitsMaskExactlySizeMinusOneTimes | lockedMemberDigitsMask) != baseCellsDigitsMask)
									{
										continue;
									}

									if (CheckComplexSeniorLockedMemberGrouped(
										ref context, grid, baseCells, targetCell, endoTargetCellsGroup, crossline, baseCellsDigitsMask,
										(Mask)(1 << selectedDigit), housesMask, 1 << extraHouse, size, in expandedCrosslineIncludingTarget,
										lockedMembers, chuteIndex, out var lockedDigitsMask, out var inferredBaseDigitsMask
									) is { } complexSeniorLockedMemberTypeStep)
									{
										return complexSeniorLockedMemberTypeStep;
									}

#if ADVANCED_COMPLEX_SENIOR_EXOCET
									if (CheckAdvancedComplexSenior(
										ref context, grid, baseCells, targetCell, endoTargetCellsGroup, crossline,
										baseCellsDigitsMask, inferredBaseDigitsMask, housesMask, 1 << extraHouse, size,
										(Mask)(1 << selectedDigit), in expandedCrosslineIncludingTarget
									) is { } complexSeniorAhsTypeStep)
									{
										return complexSeniorAhsTypeStep;
									}
#endif

									collectedDigitsMask |= (Mask)(1 << selectedDigit);
									break;
								}
								case { Count: > 2 } endoTargetCellsGroup when !endoTargetCellsGroup.PeerIntersection.Contains(targetCell):
								{
									// This will include an AHS.
									var disappearedDigitsMask = (Mask)(grid[endoTargetCellsGroup] & ~baseCellsDigitsMask);
									if (BitOperations.PopCount(disappearedDigitsMask) < endoTargetCellsGroup.Count - 1)
									{
										// Endo-target cells are not enough to form an AHS.
										continue;
									}

									foreach (var digitCombination in disappearedDigitsMask.AllSets.GetSubsets(endoTargetCellsGroup.Count - 1))
									{
										var appearingMap = CellMap.Empty;
										foreach (var digit in digitCombination)
										{
											appearingMap |= CandidatesMap[digit] & HousesMap[endoTargetCellHouse];
										}

										if (appearingMap != endoTargetCellsGroup)
										{
											// Other cells will be used, which is invalid for the checking.
											continue;
										}

										CheckValidityAndLockedMembersExistence(
											grid,
											baseCellsDigitsMask,
											baseCells,
											in targetCell.AsCellMap(),
											expandedCrosslineIncludingTarget - targetCell & ~endoTargetCellsGroup,
											size,
											out digitsMaskExactlySizeMinusOneTimes,
											out _,
											out lockedMembers,
											out lockedMemberDigitsMask
										);

										if ((Mask)(digitsMaskExactlySizeMinusOneTimes | lockedMemberDigitsMask) != baseCellsDigitsMask)
										{
											continue;
										}

										var currentDigitsMask = MaskOperations.Create(digitCombination);
										if (CheckComplexSeniorLockedMemberGrouped(
											ref context, grid, baseCells, targetCell, endoTargetCellsGroup, crossline, baseCellsDigitsMask,
											currentDigitsMask, housesMask, 1 << extraHouse, size, in expandedCrosslineIncludingTarget,
											lockedMembers, chuteIndex, out var lockedDigitsMask, out var inferredBaseDigitsMask
										) is { } complexSeniorLockedMemberTypeStep)
										{
											return complexSeniorLockedMemberTypeStep;
										}

#if ADVANCED_COMPLEX_SENIOR_EXOCET
										if (CheckAdvancedComplexSenior(
											ref context, grid, baseCells, targetCell, endoTargetCellsGroup, crossline,
											baseCellsDigitsMask, inferredBaseDigitsMask, housesMask, 1 << extraHouse, size,
											currentDigitsMask, in expandedCrosslineIncludingTarget
										) is { } complexSeniorAhsTypeStep)
										{
											return complexSeniorAhsTypeStep;
										}
#endif

										collectedDigitsMask |= currentDigitsMask;
									}
									break;
								}
							}
						}
					}
				}
			}
		}

		return null;
	}
#endif

#if WEAK_EXOCET
	/// <summary>
	/// The core method to check for Weak Exocet sub-types.
	/// </summary>
	/// <param name="context">The analysis context to be used for accumulate steps.</param>
	/// <param name="grid">The grid as the candidate referencing object.</param>
	/// <param name="baseCells">The cells of the base.</param>
	/// <param name="targetCells">The cells of the target. The target cell only contains empty cells; non-empty ones will be ignored.</param>
	/// <param name="groupsOfTargetCells">
	/// The grouped cells for the argument <paramref name="targetCells"/>, by is containing crossline house.
	/// </param>
	/// <param name="crossline">
	/// The cells of cross-line. The cross-line cells contain 18 cells regardless of its state: empty or non-empty.
	/// </param>
	/// <param name="baseCellsDigitsMask">The mask that holds a list of digits that are appeared in base cells.</param>
	/// <param name="housesMask">The mask that holds a list of houses being iterated.</param>
	/// <param name="isRow">Indicates whether the exocet pattern is row-ish. The direction is same as cross-line cells.</param>
	/// <param name="size">
	/// The size of houses used in the cross-line cells. The value can be 3 or 4; higher-sized cases cannot be determined
	/// because I don't know. :(
	/// </param>
	/// <param name="chuteIndex">The chute index to be used. Valid values are between 0 and 6.</param>
	/// <returns>A valid <see cref="ExocetStep"/> instance calculated and found.</returns>
	private static ExocetStep? CollectWeakExocets(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		bool isRow,
		int size,
		int chuteIndex
	)
	{
		#region Description for Weak Exocets
		// See following links to learn more information about this technique:
		//
		//   * http://forum.enjoysudoku.com/weak-exocet-t39651.html
		//   * https://tieba.baidu.com/p/7637653732
		//
		// The conditions of this technique is very hard to be described so... here is an example:
		//
		//   ,--------------------------,------------------------,-----------------------,
		//   | 1278    237     m1237    | 23468   5      3468    | 2678   2478   9       |
		//   | a4      2579    279      | *1      268    689     | 25678  a3     568     |
		//   | 2589    a6      239      | 7       2348   3489    | a1     2458   458     |
		//   :--------------------------+------------------------+-----------------------:
		//   | 1267    2347    5        | 9       13468  13468   | 2368   1248   T1346-8 |
		//   | 1269    8       123469   | B346    7      B1346   | 23569  12459  13456   |
		//   | 169     349     T1346-9  | 5       13468  2       | 3689   1489   7       |
		//   :--------------------------+------------------------+-----------------------:
		//   | a3      2579-4  2479     | 248     1248   14578   | 5789   a6     158     |
		//   | 567     457     8        | 346     9      1346-57 | 357    157    2       |
		//   | 2579-6  a1      2679     | 2368    2368   35678   | a4     5789   358     |
		//   '--------------------------'------------------------'-----------------------'
		//                     crossline          crossline                      crossline
		//
		// (Single-line text code: ....5...94..1...3..6.7..1....59......8..7.......5.2..73......6...8.9...2.1....4..)
		// Here is an exocet with digits [1, 3, 4, 6], base: r5c46, target: r4c9 & r6c3, crossline cells: r123789c359.
		//
		// If we don't have the value digit 1 from the cell r2c4 (marked as '*'), the cell r1c3 must be an value digit not be 1, 3, 4 and 6
		// to ensure every digit [1, 3, 4, 6] appearing at most 2 times (size = 3, 3 - 1 = 2).
		// However, this cell is an empty cell, which means it may cause an extra chance to arrange the filling the digit 1,
		// so here the digit 1 may contain 3 times to be filled with the cross-line cells if so.
		// In this way a standard JE may not be formed because of the digit 1.
		// We should use an extra digit 1 to make this rule to be "balanced" again.
		// If the value digit 1 (r2c4) exists in a different block as cell r1c3, the JE can also be formed,
		// with all standard JE rule being obeyed (even adjacent target, i.e. the mirror rules).
		// In addition, this exocet pattern may raise extra eliminations to be used, which will not introduced by a standard JE.
		// This technique is called "Weak Exocet", having been found and proved by Borescoper, my friend.
		//
		// However, a weak exocet should rely on some extra conditions:
		//
		//   1) All four digits in base should contain value representations in 4 blocks cross-line cells used.
		//   2) All value cells using such four digits should appear twice for each.
		//   3) For each value digit, it should be paired with another digit to make a group; the last digit will be the other group.
		//   4) Such two groups should be filled into four blocks mentioned in condition 1) with diagonal-distributed blocks.
		//   5) The missing-value cell shouldn't lie in any blocks mentioned in condition 1) and 4).
		//   6) An extra value digit should be placed in a block that 4 blocks mentioned above not covered,
		//      but not in a same chute with base cells.
		//   7) The missing-value cell cannot lie in a same line with that given value digit condition 6) mentioned.
		//
		// A weak exocet may contain some extra eliminations that a normal JE pattern (even not containing the missing-value cell) doesn't have.
		#endregion

		// Target cells must be 2.
		if (groupsOfTargetCells.Length != 2)
		{
			return null;
		}

		// Try to get cross-line cells, count up the positions of the value cells in the cross-line cells.
		// This will miss some weak exocets. One exception is:
		//
		//   98.7..6..7...5..9...4+9....73..2.9....1..3.......5...3.2...9..1..7...5..4..6...8..:967
		//   ,-------------------,----------------------,----------------------,
		//   | 9     8     1235  | 7      124    1234   | 6      245     1235  |
		//   | 7     236   123   | 13468  5      123468 | 1234   9       1238  |
		//   | 156   2356  4     | 9      1268   12368  | 1235   258     7     |
		//   :-------------------+----------------------+----------------------:
		//   | 3     456   578   | 2      14678  9      | 1457   45678   1568  |
		//   | 4568  1     25789 | 468    3      4678   | 24579  245678  25689 |
		//   | 468   2469  2789  | 5      14678  14678  | 1247   3       12689 |
		//   :-------------------+----------------------+----------------------:
		//   | 2     345   358   | 3468   9      34678  | 357    1       356   |
		//   | 18    7     1389  | 1368   1268   5      | 239    26      4     |
		//   | 145   3459  6     | 134    1247   12347  | 8      257     2359  |
		//   '-------------------'----------------------'----------------------'
		//
		// r1c1 has a value digit that will make cross-line c158 to be 7 values, failed to be checked.
		// Today I don't know which, where and how values influence the elimination, so I don't allow for this.
		if ((crossline & ~EmptyCells) is not { Count: 5 or 6, RowMask: var rowsCovered, ColumnMask: var columnsCovered })
		{
			return null;
		}

		// Check whether such 5 and 6 cells are in a 2 * 3 "rectangle".
		if ((isRow, BitOperations.PopCount(rowsCovered), BitOperations.PopCount(columnsCovered)) is not ((false, 2, 3) or (true, 3, 2)))
		{
			return null;
		}

		// Check whether the rows or columns are spanned 3 different chute in the same direction of the cross-line cells.
		var (spanningLinesChute3, spanningLinesChute2, spanningLinesChute1) = (isRow ? rowsCovered : columnsCovered).SplitMask();
		if ((BitOperations.PopCount(spanningLinesChute1), BitOperations.PopCount(spanningLinesChute2), BitOperations.PopCount(spanningLinesChute3)) is not (1, 1, 1))
		{
			return null;
		}

		// Try to fetch the missing-value cell.
		// Please note here is an exception: If the missing-value cell isn't missing the digit, it can also be a weak exocet
		// if the digit belongs to digits appeared in base cells, and such cells should only contain 1. 2 or more cells is invalid.
		var missingValueCell = -1;
		foreach (var row in rowsCovered)
		{
			foreach (var column in columnsCovered)
			{
				var cell = row * 9 + column;
				if (grid.GetState(cell) == CellState.Empty || (baseCellsDigitsMask >> grid.GetDigit(cell) & 1) != 0)
				{
					if (missingValueCell != -1)
					{
						// At least 2 cells satisfied this condition, which is invalid in a weak exocet.
						return null;
					}

					missingValueCell = cell;
				}
			}
		}
		if (missingValueCell == -1)
		{
			return null;
		}

		// Check the outside value digit, whether the digit doesn't share a same house as the missing-value cell.
		var (baseCellUncoveredBlocksMaskCoveringCrossline, baseCellCoveredBlocksMaskCoveringCrossline) = ((Mask)0, (Mask)0);
		var (baseCellUncoveredBlockCells, baseCellCoveredBlockCells) = (CellMap.Empty, CellMap.Empty);
		foreach (ref readonly var chuteCells in ChuteMaps[isRow ? ..3 : 3..])
		{
			if (chuteCells & baseCells)
			{
				foreach (var block in baseCellCoveredBlocksMaskCoveringCrossline = (chuteCells & crossline).BlockMask)
				{
					baseCellCoveredBlockCells |= HousesMap[block];
				}
				foreach (var block in baseCellUncoveredBlocksMaskCoveringCrossline = (Mask)(crossline.BlockMask & ~baseCellCoveredBlocksMaskCoveringCrossline))
				{
					baseCellUncoveredBlockCells |= HousesMap[block];
				}
				break;
			}
		}

		// Check whether the missing-value cell isn't in the chute that base cells can cover.
		if (baseCellCoveredBlockCells.Contains(missingValueCell))
		{
			return null;
		}

		// Now fetch the value cell outside the blocks of the missing-value cell.
		const Cell invalidPos = -2;
		Cell valueDigitCell;
		var valueDigitsPos = new List<Cell>(6);
		foreach (var block in baseCellCoveredBlocksMaskCoveringCrossline)
		{
			foreach (var cell in HousesMap[block] & ~EmptyCells & ~crossline)
			{
				if ((baseCellsDigitsMask >> grid.GetDigit(cell) & 1) != 0 && !PeersMap[missingValueCell].Contains(cell))
				{
					if (valueDigitsPos.Count == 6)
					{
						// Exceeds the adding limit.
						return null;
					}

					valueDigitsPos.Add(cell);
				}
			}
		}

		valueDigitCell = valueDigitsPos switch { [] => -1, [var vdc] => vdc, _ => invalidPos };
		if (valueDigitCell == invalidPos)
		{
			// This case is invalid to be checked.
			return null;
		}

		// Check whether the missing-value cell has a value, and they are same.
		if (valueDigitCell != -1 && grid.GetDigit(missingValueCell) is var missingValueCellDigit and not -1
			&& missingValueCellDigit != grid.GetDigit(valueDigitCell))
		{
			return null;
		}

		// Check for cross-line (size - 1) rule.
		// Due to the reason of the exocet pattern forming rule, the missing-value cell may cause some digits appeared in base cells
		// exceed the (size - 1) times appearing.
		var sizeMinusOneRule = true;
		var exceptionDigit = valueDigitCell == -1 ? -1 : grid.GetDigit(valueDigitCell);
		foreach (var digit in baseCellsDigitsMask)
		{
			var limitCount = exceptionDigit != -1 && exceptionDigit == digit ? size - 1 : size;
			if (grid.IsExactAppearingTimesOf(digit, crossline - missingValueCell, limitCount))
			{
				// The current digit can be filled in cross-line cells at most (size - 1) times.
				sizeMinusOneRule = false;
				break;
			}
		}
		if (!sizeMinusOneRule)
		{
			return null;
		}

		// Check whether the value cell digit isn't covered in the same line as value cells in cross-line.
		var coveredLinesForValueCells = (isRow ? columnsCovered << 18 : rowsCovered << 9).AllSets;
		var intersectedLinesForSuchLastCells = (HousesMap[coveredLinesForValueCells[0]] | HousesMap[coveredLinesForValueCells[1]]) & ~crossline;
		if (valueDigitCell != -1 && intersectedLinesForSuchLastCells.Contains(valueDigitCell))
		{
			return null;
		}

		// Check whether lines of value cells don't contain the digits appeared in base cells, of value representation.
		var intersectedLinesContainAnyValueCellDigitsAppearedInBaseCells = false;
		foreach (var cell in intersectedLinesForSuchLastCells)
		{
			if ((baseCellsDigitsMask >> grid.GetDigit(cell) & 1) != 0)
			{
				intersectedLinesContainAnyValueCellDigitsAppearedInBaseCells = true;
				break;
			}
		}
		if (intersectedLinesContainAnyValueCellDigitsAppearedInBaseCells)
		{
			return null;
		}

		// Then check for digits of values in houses that base cell chute does not cover.
		// All four blocks should contain 4 kinds of digits of value representation, with diagonal-distributed rule.
		var blocks = baseCellUncoveredBlocksMaskCoveringCrossline.AllSets;
		var lastSixteenCells = (HousesMap[blocks[0]] | HousesMap[blocks[1]] | HousesMap[blocks[2]] | HousesMap[blocks[3]]) & ~crossline & ~intersectedLinesForSuchLastCells;
		var isDiagonallyDistributed = true;
		foreach (var ((a1, da1), (a2, da2)) in (((0, 3), (1, 2)), ((1, 2), (0, 3))))
		{
			var ca1 = (Mask)(grid[HousesMap[blocks[a1]] & lastSixteenCells & ~EmptyCells, true] & baseCellsDigitsMask);
			var cda1 = (Mask)(grid[HousesMap[blocks[da1]] & lastSixteenCells & ~EmptyCells, true] & baseCellsDigitsMask);
			var ca2 = (Mask)(grid[HousesMap[blocks[a2]] & lastSixteenCells & ~EmptyCells, true] & baseCellsDigitsMask);
			var cda2 = (Mask)(grid[HousesMap[blocks[da2]] & lastSixteenCells & ~EmptyCells, true] & baseCellsDigitsMask);
			if (ca1 != cda1 || ca2 != cda2
				|| (Mask)(ca1 | ca2) != baseCellsDigitsMask
				|| (BitOperations.PopCount(ca1), BitOperations.PopCount(cda1), BitOperations.PopCount(ca2), BitOperations.PopCount(cda2)) is not (2, 2, 2, 2))
			{
				isDiagonallyDistributed = false;
				break;
			}
		}
		if (!isDiagonallyDistributed)
		{
			return null;
		}

		// A weak exocet is formed. Phew! Now check for eliminations.
		if (CheckBaseWeak(
			ref context, grid, baseCells, groupsOfTargetCells, crossline, valueDigitCell, missingValueCell,
			baseCellsDigitsMask
		) is { } baseWeakTypeStep)
		{
			return baseWeakTypeStep;
		}

		if (CheckWeakAdjacentTarget(
			ref context, grid, baseCells, groupsOfTargetCells, crossline, valueDigitCell, missingValueCell,
			baseCellsDigitsMask, chuteIndex
		) is { } adjacentTargetTypeStep)
		{
			return adjacentTargetTypeStep;
		}

		if (CheckWeakExocetSlash(
			ref context, grid, baseCells, groupsOfTargetCells, crossline, lastSixteenCells, valueDigitCell, missingValueCell,
			baseCellsDigitsMask, isRow, chuteIndex, out var cellsCanBeEliminated
		) is { } slashTypeStep)
		{
			return slashTypeStep;
		}

		if (CheckWeakExocetBzRectangle(
			ref context, grid, baseCells, groupsOfTargetCells, crossline, lastSixteenCells, cellsCanBeEliminated,
			valueDigitCell, missingValueCell, baseCellsDigitsMask, isRow, chuteIndex
		) is { } bzRectangleTypeStep)
		{
			return bzRectangleTypeStep;
		}

		return null;
	}
#endif

#if DOUBLE_EXOCET
	/// <summary>
	/// The core method to check for Double Exocet sub-types.
	/// </summary>
	/// <param name="context">The analysis context to be used for accumulate steps.</param>
	/// <param name="grid">The grid as the candidate referencing object.</param>
	/// <param name="baseCells">The cells of the base.</param>
	/// <param name="targetCells">The cells of the target. The target cell only contains empty cells; non-empty ones will be ignored.</param>
	/// <param name="groupsOfTargetCells">
	/// The grouped cells for the argument <paramref name="targetCells"/>, by is containing crossline house.
	/// </param>
	/// <param name="crossline">
	/// The cells of cross-line. The cross-line cells contain 18 cells regardless of its state: empty or non-empty.
	/// </param>
	/// <param name="housesEmptyCells">The empty cells in the houses.</param>
	/// <param name="baseCellsDigitsMask">The mask that holds a list of digits that are appeared in base cells.</param>
	/// <param name="housesMask">The mask that holds a list of houses being iterated.</param>
	/// <param name="isRow">Indicates whether the exocet pattern is row-ish. The direction is same as cross-line cells.</param>
	/// <param name="size">
	/// The size of houses used in the cross-line cells. The value can be 3 or 4; higher-sized cases cannot be determined
	/// because I don't know. :(
	/// </param>
	/// <param name="chuteIndex">The chute index to be used. Valid values are between 0 and 6.</param>
	/// <returns>A valid <see cref="ExocetStep"/> instance calculated and found.</returns>
	private static ExocetStep? CollectDoubleExocets(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		in CellMap housesEmptyCells,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		bool isRow,
		int size,
		int chuteIndex
	)
	{
		// Before the checking we have a normal JE pattern. Now we should check for the other one, to form a double JE.
		// A double JE is not two combined JEs. A double JE can contain extra eliminations that won't be formed in a single JE pattern.

		// Check whether the number of target cells is 2.
		if (baseCells.Count != targetCells.Count || targetCells.Count != 2)
		{
			return null;
		}

		// Check for the other pair of base cells.
		var blocksMask = HouseMaskOperations.AllBlocksMask & ~baseCells.BlockMask & ~crossline.BlockMask;
		var lastCells = CellMap.Empty;
		foreach (var block in blocksMask)
		{
			lastCells |= HousesMap[block];
		}
		lastCells &= ~baseCells.PeerIntersection;
		foreach (var line in isRow ? crossline.RowMask << 9 : crossline.ColumnMask << 18)
		{
			lastCells &= ~HousesMap[line];
		}

		// Now we have a possible double JE. However, we haven't checked for validity. Now check for it.
		// We should only check for one base cell because the other one is ensured to be correct.
		if (!CheckValidityAndLockedMembersExistence(
			grid, baseCellsDigitsMask, baseCells, targetCells, crossline,
			size - 1, out _, out _, out _, out _))
		{
			// The pattern does not satisfy basic (size - 1) rule.
			return null;
		}

		// Iterate on each intersection to get the other side of base cells.
		foreach (ref readonly var intersection in Miniline.MinilinesGroupedByChuteIndex[chuteIndex].AsReadOnlySpan())
		{
			var theOtherBaseCells = intersection & lastCells;
			if (theOtherBaseCells.Count != 2)
			{
				// The other side of cells should contain the same number of the current base cells.
				continue;
			}

			var theOtherBaseCellsDigitsMask = grid[theOtherBaseCells];
			if (theOtherBaseCellsDigitsMask != baseCellsDigitsMask)
			{
				// The other side of base cells should hold same digits.
				continue;
			}

			// Try to calculate the target cells.
			var theOtherTargetCells = Chutes[chuteIndex].Cells & housesEmptyCells & EmptyCells & ~theOtherBaseCells.PeerIntersection;
			var theOtherTargetCellsDigitsMask = grid[theOtherTargetCells];

			// Check whether all digits appeared in base cells can be filled in target empty cells.
			if ((theOtherTargetCellsDigitsMask & baseCellsDigitsMask) != baseCellsDigitsMask)
			{
				// They are out of relation.
				continue;
			}

			if (targetCells & theOtherTargetCells)
			{
				// Two target cells pair cannot intersect with each other.
				continue;
			}

			// If the current accumulator has already collected for the same-cell steps, we won't add it.
			if (!context.OnlyFindOne)
			{
				var alreadyContain = false;
				foreach (var s in context.Accumulator)
				{
					if (s is IDoubleExocet d
						&& (d.BaseCells | d.BaseCellsTheOther | d.TargetCells | d.TargetCellsTheOther) is var a
						&& (baseCells | theOtherBaseCells | targetCells | theOtherTargetCells) is var b
						&& a == b)
					{
						alreadyContain = true;
					}
				}
				if (alreadyContain)
				{
					return null;
				}
			}

			// A Double JE is found. Now check for eliminations.
			if (CheckDoubleBase(
				ref context, grid, baseCells, targetCells, theOtherBaseCells, theOtherTargetCells, crossline,
				baseCellsDigitsMask, housesMask
			) is { } doubleBaseTypeStep)
			{
				return doubleBaseTypeStep;
			}

			if (CheckDoubleGeneralizedFish(
				ref context, grid, baseCells, targetCells, theOtherBaseCells, theOtherTargetCells, crossline,
				size, isRow, baseCellsDigitsMask, housesMask
			) is { } generalizedFishTypeStep)
			{
				return generalizedFishTypeStep;
			}
		}

		return null;
	}
#endif

#if JUNIOR_EXOCET || SENIOR_EXOCET
	private static NormalExocetStep? CheckJuniorOrSeniorBase(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		Cell endoTargetCell,
		in CellMap crossline,
		in CellMap crosslineIncludingTarget,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		out ReadOnlySpan<Conjugate> inferredTargetConjugatePairs
	)
	{
		var (conclusions, conjugatePairs) = (new List<Conclusion>(), new List<Conjugate>(2));
		switch (baseCells, targetCells, endoTargetCell)
		{
			case ({ Count: 1 }, [var targetCell], -1):
			{
				foreach (var digit in (Mask)(grid.GetCandidates(targetCell) & ~baseCellsDigitsMask))
				{
					conclusions.Add(new(Elimination, targetCell, digit));
				}
				break;
			}
			case ({ Count: 1 }, { Count: 2 }, -1):
			{
				var digitsMask = (Mask)(grid[targetCells, false, MaskAggregator.And] & ~baseCellsDigitsMask);
				if (digitsMask == 0)
				{
					break;
				}

				foreach (var coveredLine in targetCells.SharedHouses)
				{
					foreach (var conjugatePairDigit in digitsMask)
					{
						if ((HousesMap[coveredLine] & CandidatesMap[conjugatePairDigit]) != targetCells)
						{
							continue;
						}

						// This digit is a conjugate pair.
						foreach (var cell in targetCells)
						{
							foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask & ~(1 << conjugatePairDigit)))
							{
								conclusions.Add(new(Elimination, cell, digit));
							}
						}

						conjugatePairs.Add(new(targetCells, conjugatePairDigit));
						break;
					}
				}
				break;
			}
			case (_, [var targetCell], not -1):
			{
				foreach (var cell in targetCells + endoTargetCell)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask))
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
				break;
			}
			case (_, { Count: 2 }, -1):
			{
				foreach (var cell in targetCells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask))
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
				break;
			}
			case (_, { Count: 3 or 4 }, -1):
			{
				foreach (var (_, cellsInThisBlock) in targetCells.GroupTargets(housesMask))
				{
					switch (cellsInThisBlock.Count)
					{
						case 1:
						{
							foreach (var cell in cellsInThisBlock)
							{
								foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask))
								{
									conclusions.Add(new(Elimination, cell, digit));
								}
							}
							break;
						}
						case 2:
						{
							var digitsMask = (Mask)(grid[cellsInThisBlock, false, MaskAggregator.And] & ~baseCellsDigitsMask);
							if (digitsMask == 0)
							{
								break;
							}

							foreach (var coveredLine in cellsInThisBlock.SharedHouses)
							{
								foreach (var conjugatePairDigit in digitsMask)
								{
									if ((HousesMap[coveredLine] & CandidatesMap[conjugatePairDigit]) != cellsInThisBlock)
									{
										continue;
									}

									// This digit is a conjugate pair.
									foreach (var cell in cellsInThisBlock)
									{
										foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask & ~(1 << conjugatePairDigit)))
										{
											conclusions.Add(new(Elimination, cell, digit));
										}
									}

									conjugatePairs.Add(new(cellsInThisBlock, conjugatePairDigit));
									break;
								}
							}

							break;
						}
					}
				}
				break;
			}
		}

		// Check for the number of target cells and their bound conjugate pairs.
		if ((targetCells.Count, conjugatePairs.Count, endoTargetCell) is not ((_, _, not -1) or (2, 0, _) or (3, 1, _) or (4, 2, _)))
		{
			inferredTargetConjugatePairs = [];
			return null;
		}

		// Try to get conjugate pairs in target cells.
		inferredTargetConjugatePairs = conjugatePairs.AsSpan();

		if (conclusions.Count == 0)
		{
			// No eliminations found.
			return null;
		}

		var crosslineFinal = endoTargetCell == -1 ? crossline : crosslineIncludingTarget;
		var step = new NormalExocetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. endoTargetCell != -1 ? [new CellViewNode(ColorIdentifier.Auxiliary1, endoTargetCell)] : (ViewNode[])[],
					.. from cell in crosslineFinal - endoTargetCell select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from conjugatePair in conjugatePairs
					from cell in conjugatePair.Map
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + conjugatePair.Digit),
					..
					from cell in baseCells
					from digit in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + digit),
					..
					from cell in crosslineFinal - endoTargetCell
					where grid.GetState(cell) == CellState.Empty
					from digit in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + digit),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			endoTargetCell != -1 ? [endoTargetCell] : [],
			crossline,
			[.. conjugatePairs]
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if SENIOR_EXOCET
	private static ExocetStep? CheckSeniorExocetNoValueCells(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		in CellMap crosslineIncludingTarget,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		int size,
		int chuteIndex
	)
	{
		foreach (var endoTargetCell in crossline)
		{
			if (grid.GetState(endoTargetCell) != CellState.Empty)
			{
				continue;
			}

			// Endo-target cells must contain at least one of all digits appeared in base cells.
			if (!grid.CheckTargetCellsDigitsValidity([targetCell, endoTargetCell], baseCellsDigitsMask))
			{
				continue;
			}

			CheckValidityAndLockedMembersExistence(
				grid,
				baseCellsDigitsMask,
				baseCells,
				[targetCell],
				crosslineIncludingTarget - targetCell - endoTargetCell,
				size - 1,
				out var digitsMaskExactlySizeMinusOneTimes,
				out _,
				out var lockedMembers,
				out var lockedMemberDigitsMask
			);

			if ((Mask)(lockedMemberDigitsMask | digitsMaskExactlySizeMinusOneTimes) != baseCellsDigitsMask)
			{
				continue;
			}

			switch (BitOperations.PopCount(lockedMemberDigitsMask))
			{
				case 1:
				{
					if (CheckSeniorLockedMember(
						ref context, grid, baseCells, targetCell, endoTargetCell, crossline, crosslineIncludingTarget,
						baseCellsDigitsMask, lockedMembers, chuteIndex, groupsOfTargetCells, size, out _,
						out var lockedDigitsMask
					) is { } seniorLockedMemberTypeStep)
					{
						return seniorLockedMemberTypeStep;
					}

					// Check whether the locked members are really used.
					// If not, we should check for them, whether they are appeared in cross-line cells in (size) times.
					var lockedMembersAreSatisfySizeMinusOneRule = true;
					foreach (var digit in lockedDigitsMask)
					{
						if (!grid.IsExactAppearingTimesOf(digit, crosslineIncludingTarget - endoTargetCell, size - 1))
						{
							// The current digit can be filled in cross-line cells at most (size) times.
							lockedMembersAreSatisfySizeMinusOneRule = false;
							break;
						}
					}
					if (lockedMembersAreSatisfySizeMinusOneRule)
					{
						goto case 0;
					}
					break;
				}
				case 0:
				{
					// Check for base SE type.
					if (CheckJuniorOrSeniorBase(
						ref context, grid, baseCells, [targetCell], endoTargetCell, crossline, crosslineIncludingTarget,
						baseCellsDigitsMask, housesMask, out _
					) is { } baseTypeStep)
					{
						return baseTypeStep;
					}
					break;
				}
			}
		}

		return null;
	}
#endif

	//
	// Sub-type Declaration
	//
#if JUNIOR_EXOCET
	private static ExocetMirrorConjugatePairStep? CheckMirrorConjugatePair(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		bool isRow,
		int chuteIndex,
		HouseMask housesMask
	)
	{
		// Mirror conjugate pair cannot be used for same-side target cells.
		if (targetCells.Count != 2 || targetCells.FirstSharedHouse != 32)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		var conjugatePairs = new List<Conjugate>(2);
		var cellGroups = targetCells.GroupTargets(housesMask);
		if (cellGroups.Length != 2)
		{
			return null;
		}

		foreach (ref readonly var cellGroup in cellGroups)
		{
			if (cellGroup.Count == 2)
			{
				// If the number of target cells in one side is 2, we cannot determine which one is correct.
				continue;
			}

			foreach (var targetCell in cellGroup)
			{
				var theOtherTwoCells = GetMirrorCells(targetCell, chuteIndex, out _);
				var theOtherEmptyCells = theOtherTwoCells & EmptyCells;
				if (!theOtherEmptyCells)
				{
					// The current miniline cannot contain any eliminations.
					continue;
				}

				var otherCellsDigitsMask = grid[theOtherEmptyCells];
				foreach (var house in theOtherEmptyCells.SharedHouses)
				{
					// Check whether the current house has a conjugate pair in the current cells.
					foreach (var digit in otherCellsDigitsMask)
					{
						var cellsContainingDigit = (CandidatesMap[digit] & HousesMap[house]) - targetCell;
						if ((theOtherEmptyCells & cellsContainingDigit) != cellsContainingDigit)
						{
							continue;
						}

						// Here a conjugate pair will be formed.
						// Now check for eliminations.
						foreach (var elimCell in theOtherEmptyCells)
						{
							foreach (var elimDigit in (Mask)(grid.GetCandidates(elimCell) & ~baseCellsDigitsMask & ~(1 << digit)))
							{
								conclusions.Add(new(Elimination, elimCell, elimDigit));
							}
						}

						conjugatePairs.Add(new(theOtherEmptyCells, digit));
					}
				}
			}
		}
		if (conclusions.Count == 0)
		{
			// No eliminations found.
			return null;
		}

		var step = new ExocetMirrorConjugatePairStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					..
					from conjugatePair in conjugatePairs
					from cell in conjugatePair.Map
					where grid.GetState(cell) == CellState.Empty && (grid.GetCandidates(cell) >> conjugatePair.Digit & 1) != 0
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + conjugatePair.Digit),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			CellMap.Empty,
			crossline,
			[.. conjugatePairs]
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if JUNIOR_EXOCET
	private static JuniorExocetAdjacentTargetStep? CheckAdjacentTarget(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		bool isRow,
		int chuteIndex,
		HouseMask housesMask
	)
	{
		// Adjacent target cannot be used for same-side target cells.
		if (targetCells.FirstSharedHouse != 32)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		var singleMirrors = CellMap.Empty;
		foreach (ref readonly var cellGroup in targetCells.GroupTargets(housesMask))
		{
			if (cellGroup.Count == 2)
			{
				// This side contain 2 target empty cells. We cannot conclude for this case.
				continue;
			}

			foreach (var targetCell in cellGroup)
			{
				var mirrorCells = GetMirrorCells(targetCell, chuteIndex, out _);
				if ((mirrorCells & EmptyCells) is not [var theOnlyMirrorCell])
				{
					// The mirror cells contain not 1 cell, it may not be included in this type.
					continue;
				}

				// Try to get the target cell that is not share with a same block with this mirror cell.
				var theOtherTargetCells = targetCells - targetCell;
				if (theOtherTargetCells is not [var theOtherTargetCell])
				{
					// The number of the other side of target cell is not 1, we cannot conclude for that.
					continue;
				}

				// Check for the only mirror cell, determining whether the cell contains an arbitrary extra digits.
				var digitsInMirrorCell = grid.GetCandidates(theOnlyMirrorCell);
				var elimDigitsFromTheOnlyMirrorCell = (Mask)(digitsInMirrorCell & ~baseCellsDigitsMask);

				// Check for the containing digits in mirror cells, and fetch which digits are appeared in base cells.
				// Such digits will be synchronized with the other target cell.
				var containedDigitsAppearedInBaseCellsInMirror = (Mask)(digitsInMirrorCell & baseCellsDigitsMask);
				var elimDigitsFromTheOtherTargetCell = (Mask)(grid.GetCandidates(theOtherTargetCell) & ~containedDigitsAppearedInBaseCellsInMirror);

				// Try to fetch eliminations.
				if (elimDigitsFromTheOnlyMirrorCell != 0)
				{
					foreach (var elimDigit in elimDigitsFromTheOnlyMirrorCell)
					{
						conclusions.Add(new(Elimination, theOnlyMirrorCell, elimDigit));
					}
				}
				if (elimDigitsFromTheOtherTargetCell != 0)
				{
					foreach (var elimDigit in elimDigitsFromTheOtherTargetCell)
					{
						conclusions.Add(new(Elimination, theOtherTargetCell, elimDigit));
					}
				}

				singleMirrors.Add(theOnlyMirrorCell);
			}
		}

		if (conclusions.Count == 0 || !singleMirrors)
		{
			// No eliminations found.
			return null;
		}

		var step = new JuniorExocetAdjacentTargetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. from cell in singleMirrors select new CellViewNode(ColorIdentifier.Auxiliary3, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			CellMap.Empty,
			crossline,
			singleMirrors
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if JUNIOR_EXOCET
	private static JuniorExocetIncompatiblePairStep? CheckIncompatiblePair(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		Mask digitsMaskAppearedInCrossline,
		out Mask inferredTargetPairMask
	)
	{
		inferredTargetPairMask = 0;

		if (digitsMaskAppearedInCrossline != 0)
		{
			return null;
		}

		if (baseCells is not [var base1, var base2])
		{
			return null;
		}

		// Now try to fetch the defining cells. First, try to get uncovered 4 blocks that the final cells should be located in.
		var cellsDoNotCover = CellMap.Empty;
		foreach (ref readonly var chuteCells in ChuteMaps)
		{
			if (chuteCells & baseCells)
			{
				cellsDoNotCover |= chuteCells;
			}
		}
		var lastFourBlocks = CellMap.Full & ~cellsDoNotCover;
		var lastFourBlocksNotIntersectedWithCrossline = lastFourBlocks & ~crossline;
		var lastFourBlocksIntersectedWithCrossline = lastFourBlocks & crossline;

		// Try to check for cross-line cells, get all values and its containing cells.
		var valueCellsInLastFourBlocksIntersectedWithCrossline = lastFourBlocksIntersectedWithCrossline & ~EmptyCells;

		// Determine whether the value cells only span with 2 rows and columns, forming a square shape:
		//
		//   1 . . | 2 . .
		//   . . . | . . .
		//   . . . | . . .
		//   ------+------
		//   2 . . | 1 . .
		//   . . . | . . .
		//   . . . | . . .
		//
		// If not, the rule cannot be formed.
		if (valueCellsInLastFourBlocksIntersectedWithCrossline.Count != 4
			|| BitOperations.PopCount(valueCellsInLastFourBlocksIntersectedWithCrossline.RowMask) != 2
			|| BitOperations.PopCount(valueCellsInLastFourBlocksIntersectedWithCrossline.ColumnMask) != 2)
		{
			return null;
		}

		// Try to fetch the last 16 cells to be checked.
		var lastSixteenCells = lastFourBlocksNotIntersectedWithCrossline;
		foreach (var house in valueCellsInLastFourBlocksIntersectedWithCrossline.RowMask << 9)
		{
			lastSixteenCells &= ~HousesMap[house];
		}
		foreach (var house in valueCellsInLastFourBlocksIntersectedWithCrossline.ColumnMask << 18)
		{
			lastSixteenCells &= ~HousesMap[house];
		}

		// Now we have the 16 cells to be checked. Such cells are called "X Region".
		// We should check for value cells, determining which combinations of digits have spanned all 4 blocks the current 16 cells located in.
		var valuesGroupedByBlock = (stackalloc Mask[2]);
		var valueCellsBlocks = lastSixteenCells.BlockMask.AllSets;
		foreach (var blockIndex in (0, 1))
		{
			var valueCellsFromBlock1 = lastSixteenCells & HousesMap[valueCellsBlocks[blockIndex]] & ~EmptyCells;
			var valueCellsFromBlock2 = lastSixteenCells & HousesMap[valueCellsBlocks[3 - blockIndex]] & ~EmptyCells;
			var valuesFromBlock1 = MaskOperations.Create(from cell in valueCellsFromBlock1 select grid.GetDigit(cell));
			var valuesFromBlock2 = MaskOperations.Create(from cell in valueCellsFromBlock2 select grid.GetDigit(cell));
			var valuesFromBothBlocks = (Mask)(valuesFromBlock1 & valuesFromBlock2);
			if (valuesFromBothBlocks == 0)
			{
				// It seems no digits will be intersected with two blocks in diagonal direction...
				// The current "X Region" may not contain any possible conclusions. Just return.
				return null;
			}

			// Write down such digits.
			valuesGroupedByBlock[blockIndex] = valuesFromBothBlocks;
		}

		// Then we should check for combinations, determining which combinations are not correct.
		// The rule is, if two different digits from two groups of blocks in a diagonal direction are found,
		// they will not be the final pair of digits appeared in base cells.
		// For example, if the following diagram exists:
		//
		//   1 . . | 2 . .
		//   . . . | . . .
		//   . . . | . . .
		//   ------+------
		//   2 . . | 1 . .
		//   . . . | . . .
		//   . . . | . . .
		//
		// The pair of digits 1 and 2 cannot be the final pair in base cells,
		// meaning the base cells cannot be filled with 1 and 2 at the same time. They are incompatible.
		// Here is the prove (Chinese version):
		//
		//   https://tieba.baidu.com/p/5916787916
		//
		// Which tells us that, if so, the last pattern of exocet may form a deadly pattern.
		var incompatibleCombinationsGroupedByDigit = (stackalloc Mask[9]);
		incompatibleCombinationsGroupedByDigit.Clear();
		foreach (var value1 in valuesGroupedByBlock[0])
		{
			foreach (var value2 in valuesGroupedByBlock[1])
			{
				incompatibleCombinationsGroupedByDigit[value1] |= (Mask)(1 << value2);
				incompatibleCombinationsGroupedByDigit[value2] |= (Mask)(1 << value1);
			}
		}

		// Now check for eliminations.
		var incompatibleCandidates = CandidateMap.Empty;
		var conclusions = new List<Conclusion>();
		var targetCellsDigitsMask = grid[targetCells];
		foreach (var (elimCell, theOtherCell) in ((base1, base2), (base2, base1)))
		{
			var allDigits = grid.GetCandidates(theOtherCell);
			foreach (var elimDigit in grid.GetCandidates(elimCell))
			{
				if (incompatibleCombinationsGroupedByDigit[elimDigit] == (Mask)(allDigits & ~(1 << elimDigit)))
				{
					conclusions.Add(new(Elimination, elimCell, elimDigit));
					incompatibleCandidates.Add(elimCell * 9 + elimDigit);
				}
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		// Temporarily remove the eliminated digits, and check whether the last digits in target cells form a pair.
		// Because we know a JE pattern will lead to a conclusion:
		//
		//   "The target cells should be a pair of different digits if a JE formed."
		//
		// we can conclude that the target cells will form a distributed disjointed pair (from concept "Distributed Disjointed Subset", DDS).
		// Then we can remove all digits from the cells that both target cells can see.
		var baseCellsLastDigitsMask = baseCellsDigitsMask;
		foreach (var digitCanBeRemoved in targetCellsDigitsMask)
		{
			if (baseCells == (from conclusion in conclusions where conclusion.Digit == digitCanBeRemoved select conclusion.Cell))
			{
				baseCellsLastDigitsMask &= (Mask)~(1 << digitCanBeRemoved);
			}
		}
		if (BitOperations.PopCount(baseCellsLastDigitsMask) == 2)
		{
			// The JE has formed a distribution disjointed pair.
			inferredTargetPairMask = baseCellsLastDigitsMask;
		}

		var step = new JuniorExocetIncompatiblePairStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. from cell in lastSixteenCells & ~EmptyCells select new CellViewNode(ColorIdentifier.Auxiliary3, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			incompatibleCandidates,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if JUNIOR_EXOCET
	private static JuniorExocetTargetPairStep? CheckTargetPair(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		Mask inferredTargetPairMask,
		HouseMask housesMask,
		scoped ReadOnlySpan<Conjugate> inferredTargetConjugatePairs
	)
	{
		if (inferredTargetPairMask == 0)
		{
			return null;
		}

		if (baseCells.Count != 2)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		foreach (var cell in baseCells)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~inferredTargetPairMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		foreach (ref readonly var cellGroup in targetCells.GroupTargets(housesMask))
		{
			var (_, values) = cellGroup;
			switch (values.Count)
			{
				case 1:
				{
					foreach (var cell in values)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~inferredTargetPairMask))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					break;
				}
				case 2 when inferredTargetConjugatePairs.First(conj => conj.Map == values) is { Digit: var conjDigit }:
				{
					foreach (var cell in values)
					{
						foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~inferredTargetPairMask & ~(1 << conjDigit)))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					break;
				}
			}
		}
		if (targetCells.Count == 2)
		{
			foreach (var cell in targetCells.PeerIntersection)
			{
				foreach (var digit in (Mask)(grid.GetCandidates(cell) & inferredTargetPairMask))
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
		}
		foreach (var cell in baseCells.PeerIntersection)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & inferredTargetPairMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new JuniorExocetTargetPairStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			inferredTargetPairMask,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if JUNIOR_EXOCET
	private static JuniorExocetGeneralizedFishStep? CheckGeneralizedFish(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		Mask inferredTargetPairMask,
		bool isRow,
		HouseMask housesMask
	)
	{
		if (inferredTargetPairMask == 0)
		{
			return null;
		}

		if (baseCells.Count != 2)
		{
			return null;
		}

		var inferredTargetPairMaskDigit1 = BitOperations.TrailingZeroCount(inferredTargetPairMask);
		var inferredTargetPairMaskDigit2 = inferredTargetPairMask.GetNextSet(inferredTargetPairMaskDigit1);

		var conclusions = new List<Conclusion>();
		foreach (var line in isRow ? crossline.ColumnMask << 18 : crossline.RowMask << 9)
		{
			var crosslineCellsIntersectedWithLine = HousesMap[line] & crossline & (CandidatesMap[inferredTargetPairMaskDigit1] | CandidatesMap[inferredTargetPairMaskDigit2]);
			if (!crosslineCellsIntersectedWithLine)
			{
				// The current line does not contain any eliminations because all intersected cells in cross-line are not empty.
				continue;
			}

			var elimCells = HousesMap[line] & EmptyCells & ~crossline;
			foreach (var cell in elimCells)
			{
				foreach (var digit in (Mask)(grid.GetCandidates(cell) & inferredTargetPairMask))
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new JuniorExocetGeneralizedFishStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in targetCells
					from d in grid.GetCandidates(cell)
					where (inferredTargetPairMask >> d & 1) != 0
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					let isSwordfishDigit = (inferredTargetPairMask >> d & 1) != 0
					let colorIdentifier = isSwordfishDigit ? ColorIdentifier.Auxiliary3 : ColorIdentifier.Auxiliary2
					select new CandidateViewNode(colorIdentifier, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			inferredTargetPairMask,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if JUNIOR_EXOCET
	private static JuniorExocetMirrorAlmostHiddenSetStep? CheckMirrorAlmostHiddenSet(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		bool isRow,
		HouseMask housesMask,
		int chuteIndex
	)
	{
		// AHS cannot be used on same-side target cells.
		if (targetCells.FirstSharedHouse != 32)
		{
			return null;
		}

		// If one side of target holds 2 cells, we won't check for it.
		if (targetCells.Count >= 3)
		{
			return null;
		}

		foreach (ref readonly var cellGroup in targetCells.GroupTargets(housesMask))
		{
			if (cellGroup is not (_, [var targetCell]))
			{
				continue;
			}

			// Try to fetch the miniline of the current target cell located in.
			var mirrorCells = GetMirrorCells(targetCell, chuteIndex, out var miniline);
			var mirrorEmptyCells = mirrorCells & EmptyCells;
			if (!mirrorEmptyCells)
			{
				// The current miniline cannot contain any eliminations.
				continue;
			}

			// Now check for empty cells in this house, removing all cells located in the miniline that the target cell located in.
			foreach (var coveredHouse in mirrorEmptyCells.SharedHouses)
			{
				var otherCells = HousesMap[coveredHouse] & EmptyCells & ~miniline;
				if (otherCells.Count < 2)
				{
					// The target house does not contain enough cells to form an AHS.
					continue;
				}

				for (var size = 2; size <= otherCells.Count - 1; size++)
				{
					foreach (ref readonly var extraCells in otherCells & size - 1)
					{
						var ahsCells = extraCells | mirrorEmptyCells;
						foreach (var digitsMaskGroup in ((Mask)(grid[ahsCells] & ~baseCellsDigitsMask)).AllSets.GetSubsets(size))
						{
							var extraDigitsMask = MaskOperations.Create(digitsMaskGroup);
							var lastHoldingMap = CellMap.Empty;
							foreach (var digit in digitsMaskGroup)
							{
								lastHoldingMap |= CandidatesMap[digit];
							}

							lastHoldingMap &= HousesMap[coveredHouse];
							if (lastHoldingMap - targetCell != ahsCells)
							{
								// Final map does not match.
								continue;
							}

							var conclusions = new List<Conclusion>();
							foreach (var cell in extraCells)
							{
								foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~extraDigitsMask))
								{
									conclusions.Add(new(Elimination, cell, digit));
								}
							}
							if (conclusions.Count == 0)
							{
								// No valid conclusions exist.
								continue;
							}

							var step = new JuniorExocetMirrorAlmostHiddenSetStep(
								conclusions.AsMemory(),
								[
									[
										.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
										.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
										.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
										.. from cell in extraCells select new CellViewNode(ColorIdentifier.Auxiliary3, cell),
										.. from cell in mirrorEmptyCells select new CellViewNode(ColorIdentifier.Auxiliary3, cell),
										..
										from cell in baseCells
										from d in grid.GetCandidates(cell)
										select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
										..
										from cell in mirrorEmptyCells
										from d in (Mask)(grid.GetCandidates(cell) & extraDigitsMask)
										select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
										..
										from cell in extraCells
										from d in (Mask)(grid.GetCandidates(cell) & extraDigitsMask)
										select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
										..
										from cell in crossline
										where grid.GetState(cell) == CellState.Empty
										from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
										select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
										//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
									]
								],
								context.Options,
								baseCellsDigitsMask,
								baseCells,
								targetCells,
								crossline,
								extraCells,
								extraDigitsMask
							);
							if (context.OnlyFindOne)
							{
								return step;
							}

							context.Accumulator.Add(step);
						}
					}
				}
			}
		}

		return null;
	}
#endif

#if JUNIOR_EXOCET
	private static ExocetLockedMemberStep? CheckJuniorLockedMember(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		scoped ReadOnlySpan<LockedMember?> lockedMembers,
		int chuteIndex,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		out Mask inferredLastTargetDigitsMask,
		out Mask lockedDigitsMask
	)
	{
		// Same-side target cells cannot be used in this case, because locked member will check for mirror cells,
		// invalid for mirror checking on same-side cells.
		if (targetCells.FirstSharedHouse != 32)
		{
			(inferredLastTargetDigitsMask, lockedDigitsMask) = (0, 0);
			return null;
		}

		if (baseCells.Count != 2)
		{
			// No conclusions when the number of base cells is not 2.
			(inferredLastTargetDigitsMask, lockedDigitsMask) = (0, 0);
			return null;
		}

		// Check whether the digit is a real locked member. Locked member:
		//
		//   B B / | . . . | L L L
		//   . . . | T . . | V / /
		//   . . . | V . . | T / /
		//
		// Symbols:
		//   B: Base cells that contain candidate 'a'.
		//   T: Target cells that are empty.
		//   V: A cell that is filled with a value that is not appeared in base cells.
		//   L: Cells forming a locked member of digit 'a'.
		//   /: Cells don't contain candidate 'a'.
		if (lockedMembers.IsEmpty || groupsOfTargetCells is not [(_, [var targetCell1]), (_, [var targetCell2])])
		{
			goto AssignBaseMaskByDefault;
		}

		var (cellOffsets, candidateOffsets, houseOffsets) = (new List<CellViewNode>(4), new List<CandidateViewNode>(), new List<HouseViewNode>(2));
		(lockedDigitsMask, inferredLastTargetDigitsMask, var conclusions) = (0, 0, new List<Conclusion>());

		// Collect eliminations via mirror cells.
		// For each locked member, we should check for its mirror cells,
		// determining whether the mirror cells can make this target cell and the other target cell any conclusions.
		// Rule:
		//
		//   1)
		//   If all mirror cells of a target cell are non-empty, check its containing values and determine
		//   whether one of the digits is concluded as a locked member.
		//   If so, the other side of the target cell can be fixed with that digit (Mirror Synchronization) -
		//   all digits non-fixed can be removed.
		//
		//   2)
		//   If one of two mirror cells is non-empty, check it, determining whether it is a locked member digit.
		//   If so, record all digits appeared in the other empty mirror cells, and this locked member digit,
		//   they are possible candidates in the other side of target cell. Other digits can be removed.
		//
		// Here is an example:
		//
		//   98.7..6..5...9..7...7..4...4...8...3.3....4.6..54...9.2.....1...5..12.....89...6.
		var lockedDigitElimTimes = 0;
		foreach (var lockedDigit in baseCellsDigitsMask)
		{
			if (lockedMembers[lockedDigit] is var (lockedMemberMap, lockedBlock))
			{
				var (thisTargetCell, theOtherTargetCell) = HousesMap[lockedBlock].Contains(targetCell1)
					? (targetCell1, targetCell2)
					: (targetCell2, targetCell1);
				var mirrorCellsThisTarget = GetMirrorCells(thisTargetCell, chuteIndex, out _);
				var finalDigitsMask = (mirrorCellsThisTarget & ~EmptyCells) switch
				{
					[] when mirrorCellsThisTarget is [var a, var b]
						=> (Mask)((grid.GetCandidates(a) | grid.GetCandidates(b)) & baseCellsDigitsMask),
					[var a] when mirrorCellsThisTarget - a is [var b]
						=> (Mask)(((Mask)(1 << grid.GetDigit(a)) | grid.GetCandidates(b)) & baseCellsDigitsMask),
					[var a, var b]
						=> (Mask)((1 << grid.GetDigit(a) | 1 << grid.GetDigit(b)) & baseCellsDigitsMask)
				};
				foreach (var digit in (Mask)(grid.GetCandidates(theOtherTargetCell) & ~finalDigitsMask))
				{
					conclusions.Add(new(Elimination, theOtherTargetCell, digit));
				}

				cellOffsets.AddRange(from cell in mirrorCellsThisTarget select new CellViewNode(ColorIdentifier.Auxiliary3, cell));
				candidateOffsets.AddRange(
					from cell in lockedMemberMap
					select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + lockedDigit)
				);
				houseOffsets.Add(new(ColorIdentifier.Auxiliary1, lockedBlock));

				lockedDigitsMask |= (Mask)(1 << lockedDigit);
				inferredLastTargetDigitsMask |= (Mask)(grid.GetCandidates(theOtherTargetCell) & finalDigitsMask);
				lockedDigitElimTimes++;
			}
		}

		if (lockedDigitElimTimes == 2)
		{
			// Sync candidates in base cells from target cells.
			foreach (var cell in baseCells)
			{
				foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~inferredLastTargetDigitsMask))
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
		}

		if (conclusions.Count == 0)
		{
			// No eliminations found.
			goto AssignBaseMaskByDefault;
		}

		var ld = lockedDigitsMask;
		var step = new ExocetLockedMemberStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. cellOffsets,
					.. candidateOffsets,
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					let colorIdentifier = (ld >> d & 1) != 0 ? ColorIdentifier.Auxiliary2 : ColorIdentifier.Normal
					select new CandidateViewNode(colorIdentifier, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					.. houseOffsets,
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			lockedDigitsMask,
			baseCells,
			targetCells,
			[],
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;

	AssignBaseMaskByDefault:
		(inferredLastTargetDigitsMask, lockedDigitsMask) = (baseCellsDigitsMask, 0);
		return null;
	}
#endif

#if JUNIOR_EXOCET
	private static JuniorExocetMirrorSyncStep? CheckMirrorSync(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		int chuteIndex,
		Mask digitsMaskAppearedInCrossline
	)
	{
		if (digitsMaskAppearedInCrossline != 0)
		{
			// Mirror rule requires the digit cannot be appeared in cross-line as values.
			return null;
		}

		if (targetCells.Count != 2 || targetCells.FirstSharedHouse != 32)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		foreach (var block in targetCells.BlockMask)
		{
			var targetCell = (HousesMap[block] & targetCells)[0];
			var theOtherTargetCell = (targetCells - targetCell)[0];
			collectFor(conclusions, grid, targetCell, theOtherTargetCell);
		}

		if (conclusions.Count == 0)
		{
			// No conclusions found.
			return null;
		}

		var step = new JuniorExocetMirrorSyncStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;


		void collectFor(List<Conclusion> conclusions, in Grid grid, Cell targetCell, Cell theOtherTargetCell)
		{
			Unsafe.SkipInit<CellMap>(out var miniline);
			foreach (var tempMiniline in Miniline.MinilinesGroupedByChuteIndex[chuteIndex])
			{
				if (tempMiniline.Contains(theOtherTargetCell))
				{
					miniline = tempMiniline;
					break;
				}
			}

			switch (miniline - theOtherTargetCell & EmptyCells)
			{
				case [var mirrorEmptyCellFromTheOtherTargetCell]:
				{
					var digitsMaskForMirrorFromTheOtherTargetCell = (Mask)(grid.GetCandidates(mirrorEmptyCellFromTheOtherTargetCell) & baseCellsDigitsMask);
					if (digitsMaskForMirrorFromTheOtherTargetCell == baseCellsDigitsMask)
					{
						// They are same. Don't need to sync candidates.
						return;
					}

					var digitsMaskTargetCell = (Mask)(grid.GetCandidates(targetCell) & ~digitsMaskForMirrorFromTheOtherTargetCell);
					if (digitsMaskTargetCell == 0)
					{
						// No candidates should be synchronized.
						return;
					}

					// Sync for candidates for the other side of target cell.
					var lastDigitsMaskForTarget = grid.GetCandidates(targetCell) & baseCellsDigitsMask;
					foreach (var digit in digitsMaskTargetCell)
					{
						lastDigitsMaskForTarget &= (Mask)~(1 << digit);
						conclusions.Add(new(Elimination, targetCell, digit));
					}

					// Sync for mirror cells.
					foreach (var digit in (Mask)(grid.GetCandidates(mirrorEmptyCellFromTheOtherTargetCell) & ~lastDigitsMaskForTarget))
					{
						conclusions.Add(new(Elimination, mirrorEmptyCellFromTheOtherTargetCell, digit));
					}

					break;
				}
			}
		}
	}
#endif

#if SENIOR_EXOCET
	private static SeniorExocetTrueBaseStep? CheckSeniorTrueBase(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		in CellMap crossline,
		in CellMap crosslineIncludingTarget,
		Mask baseCellsDigitsMask,
		Digit trueBaseDigit
	)
	{
		// Check whether the endo-target cell only holds one.
		var endoTargetCell = -1;
		var multipleEndoTargetCellsFound = false;
		foreach (var cell in crossline)
		{
			if (grid.GetDigit(cell) == trueBaseDigit)
			{
				if (endoTargetCell != -1)
				{
					// Multiple endo-target cells found.
					multipleEndoTargetCellsFound = true;
					break;
				}

				endoTargetCell = cell;
			}
		}
		if (multipleEndoTargetCellsFound)
		{
			// Invalid.
			return null;
		}

		var conclusions = new List<Conclusion>();

		// First, check for elimination on target cell.
		foreach (var digit in (Mask)(grid.GetCandidates(targetCell) & ~(baseCellsDigitsMask & ~(1 << trueBaseDigit))))
		{
			conclusions.Add(new(Elimination, targetCell, digit));
		}

		// Second, check for locked candidates for base cells - base cells form a locked candidates of digit 'lockedDigit'.
		foreach (var cell in baseCells % CandidatesMap[trueBaseDigit])
		{
			conclusions.Add(new(Elimination, cell, trueBaseDigit));
		}

		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new SeniorExocetTrueBaseStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					new CellViewNode(ColorIdentifier.Auxiliary1, targetCell),
					.. from cell in crosslineIncludingTarget - endoTargetCell select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					let colorIdentifier = trueBaseDigit != d ? ColorIdentifier.Normal : ColorIdentifier.Auxiliary1
					select new CandidateViewNode(colorIdentifier, cell * 9 + d),
					..
					from cell in crosslineIncludingTarget & ~EmptyCells
					where grid.GetDigit(cell) == trueBaseDigit
					select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					..
					from cell in crosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			trueBaseDigit,
			baseCells,
			targetCell.AsCellMap(),
			endoTargetCell.AsCellMap(),
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if SENIOR_EXOCET
	private static ExocetLockedMemberStep? CheckSeniorLockedMember(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		Cell endoTargetCell,
		in CellMap crossline,
		in CellMap crosslineIncludingTarget,
		Mask baseCellsDigitsMask,
		scoped ReadOnlySpan<LockedMember?> lockedMembers,
		int chuteIndex,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		int size,
		out Mask inferredLastTargetDigitsMask,
		out Mask lockedDigitsMask
	)
	{
		if (baseCells.Count != 2)
		{
			// No conclusions when the number of base cells is not 2.
			(inferredLastTargetDigitsMask, lockedDigitsMask) = (0, 0);
			return null;
		}

		// Check whether the digit is a real locked member.
		if (lockedMembers.IsEmpty || groupsOfTargetCells is not [(_, [var targetCell1]), (_, [var targetCell2])])
		{
			goto AssignBaseMaskByDefault;
		}

		var (cellOffsets, candidateOffsets, houseOffsets) = (new List<CellViewNode>(4), new List<CandidateViewNode>(), new List<HouseViewNode>(2));
		(lockedDigitsMask, inferredLastTargetDigitsMask, var conclusions) = (0, 0, new List<Conclusion>());

		// Collect eliminations via mirror cells.
		foreach (var lockedDigit in baseCellsDigitsMask)
		{
			if (lockedMembers[lockedDigit] is var (lockedMemberMap, lockedBlock) && HousesMap[lockedBlock].Contains(targetCell))
			{
				foreach (var digit in (Mask)(grid.GetCandidates(endoTargetCell) & ~(baseCellsDigitsMask & ~(1 << lockedDigit))))
				{
					conclusions.Add(new(Elimination, endoTargetCell, digit));
				}
				foreach (var digit in (Mask)(grid.GetCandidates(targetCell) & ~baseCellsDigitsMask))
				{
					conclusions.Add(new(Elimination, targetCell, digit));
				}

				// Cannibalism rule:
				// If the maximum appearing times of the digit is less than the minimum possible limit,
				// the digit will make exocet pattern unstable. Therefore, the digit cannot be the one appeared in base cells.
				// We can safely remove it from base cells.
				if (grid.AppearingTimesOf(lockedDigit, crosslineIncludingTarget - targetCell - endoTargetCell) < size - 1)
				{
					foreach (var cell in baseCells + endoTargetCell + targetCell & CandidatesMap[lockedDigit])
					{
						conclusions.Add(new(Elimination, cell, lockedDigit));
					}
				}

				candidateOffsets.AddRange(
					from cell in lockedMemberMap
					select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + lockedDigit)
				);
				houseOffsets.Add(new(ColorIdentifier.Auxiliary1, lockedBlock));

				lockedDigitsMask |= (Mask)(1 << lockedDigit);
				inferredLastTargetDigitsMask |= (Mask)((grid.GetCandidates(endoTargetCell) | grid.GetCandidates(targetCell)) & baseCellsDigitsMask);
			}
		}
		if (conclusions.Count == 0)
		{
			// No eliminations found.
			goto AssignBaseMaskByDefault;
		}

		var ld = lockedDigitsMask;
		var step = new ExocetLockedMemberStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCell.AsCellMap() + endoTargetCell select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crosslineIncludingTarget select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. cellOffsets,
					.. candidateOffsets,
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					let colorIdentifier = (ld >> d & 1) != 0 ? ColorIdentifier.Auxiliary2 : ColorIdentifier.Normal
					select new CandidateViewNode(colorIdentifier, cell * 9 + d),
					..
					from cell in crosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					.. houseOffsets,
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			lockedDigitsMask,
			baseCells,
			targetCell.AsCellMap(),
			endoTargetCell.AsCellMap(),
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;

	AssignBaseMaskByDefault:
		(inferredLastTargetDigitsMask, lockedDigitsMask) = (baseCellsDigitsMask, 0);
		return null;
	}
#endif

#if WEAK_EXOCET
	private static WeakExocetStep? CheckBaseWeak(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		Cell valueDigitCell,
		Cell missingValueCell,
		Mask baseCellsDigitsMask
	)
	{
		var conclusions = new List<Conclusion>();
		var targetCells = CellMap.Empty;
		foreach (var groupOfTargetCells in groupsOfTargetCells)
		{
			foreach (var cell in groupOfTargetCells)
			{
				targetCells.Add(cell);

				if (valueDigitCell != -1 || !PeersMap[cell].Contains(missingValueCell))
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask))
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new WeakExocetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline - missingValueCell select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. valueDigitCell != -1 ? [new CellViewNode(ColorIdentifier.Auxiliary3, valueDigitCell)] : (ViewNode[])[],
					new CellViewNode(ColorIdentifier.Auxiliary3, missingValueCell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			valueDigitCell,
			missingValueCell,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if WEAK_EXOCET
	private static WeakExocetAdjacentTargetStep? CheckWeakAdjacentTarget(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		Cell valueDigitCell,
		Cell missingValueCell,
		Mask baseCellsDigitsMask,
		int chuteIndex
	)
	{
		if (valueDigitCell == -1)
		{
			// The value-digit cell shouldn't be missing.
			return null;
		}

		var conclusions = new List<Conclusion>();
		var cellOffsets = new List<CellViewNode>();
		var targetCells = groupsOfTargetCells[0][0].AsCellMap() + groupsOfTargetCells[1][0];
		foreach (var (thisTargetCell, theOtherTargetCell) in ((groupsOfTargetCells[0][0], groupsOfTargetCells[1][0]), (groupsOfTargetCells[1][0], groupsOfTargetCells[0][0])))
		{
			var mirrorCellsThisTarget = GetMirrorCells(thisTargetCell, chuteIndex, out _);
			var finalDigitsMask = (mirrorCellsThisTarget & ~EmptyCells) switch
			{
				[] when mirrorCellsThisTarget is [var a, var b]
					=> (Mask)((grid.GetCandidates(a) | grid.GetCandidates(b)) & baseCellsDigitsMask),
				[var a] when mirrorCellsThisTarget - a is [var b]
					=> (Mask)(((Mask)(1 << grid.GetDigit(a)) | grid.GetCandidates(b)) & baseCellsDigitsMask),
				[var a, var b]
					=> (Mask)((1 << grid.GetDigit(a) | 1 << grid.GetDigit(b)) & baseCellsDigitsMask)
			};
			foreach (var digit in (Mask)(grid.GetCandidates(theOtherTargetCell) & ~finalDigitsMask))
			{
				conclusions.Add(new(Elimination, theOtherTargetCell, digit));
			}

			cellOffsets.AddRange(from cell in mirrorCellsThisTarget select new CellViewNode(ColorIdentifier.Auxiliary3, cell));
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new WeakExocetAdjacentTargetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. cellOffsets,
					.. from cell in crossline - missingValueCell select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					new CellViewNode(ColorIdentifier.Auxiliary3, valueDigitCell),
					new CellViewNode(ColorIdentifier.Auxiliary3, missingValueCell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			valueDigitCell,
			missingValueCell,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if WEAK_EXOCET
	private static WeakExocetSlashStep? CheckWeakExocetSlash(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		in CellMap lastSixteenCells,
		Cell valueDigitCell,
		Cell missingValueCell,
		Mask baseCellsDigitsMask,
		bool isRow,
		int chuteIndex,
		out CellMap cellsCanBeEliminated
	)
	{
		if (valueDigitCell == -1)
		{
			// The value-digit cell shouldn't be missing.
			cellsCanBeEliminated = [];
			return null;
		}

		Unsafe.SkipInit<Chute>(out var sharedChute);
		foreach (var chute in Chutes[isRow ? 3.. : ..3])
		{
			if (chute.Cells.Contains(valueDigitCell))
			{
				sharedChute = chute;
				break;
			}
		}

		var blocks = ((Mask)(lastSixteenCells.BlockMask & ~sharedChute.Cells.BlockMask)).AllSets;
		cellsCanBeEliminated = lastSixteenCells & (HousesMap[blocks[0]] | HousesMap[blocks[1]]);

		var conclusions = new List<Conclusion>();
		foreach (var block in blocks)
		{
			// Check for digits appeared in this block.
			var valueCells = cellsCanBeEliminated & HousesMap[block] & ~EmptyCells;
			var valueCellsBaseDigitsMask = (Mask)0;
			foreach (var digit in baseCellsDigitsMask)
			{
				if (valueCells & ValuesMap[digit])
				{
					valueCellsBaseDigitsMask |= (Mask)(1 << digit);
				}
			}

			foreach (var cell in cellsCanBeEliminated & HousesMap[block] & EmptyCells)
			{
				foreach (var digit in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask))
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var targetCells = from @group in groupsOfTargetCells select @group[0];
		var step = new WeakExocetSlashStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline - missingValueCell select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					new CellViewNode(ColorIdentifier.Auxiliary3, valueDigitCell),
					new CellViewNode(ColorIdentifier.Auxiliary3, missingValueCell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			valueDigitCell,
			missingValueCell,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if WEAK_EXOCET
	private static WeakExocetBzRectangleStep? CheckWeakExocetBzRectangle(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		in CellMap crossline,
		in CellMap lastSixteenCells,
		in CellMap cellsCanBeEliminated,
		Cell valueDigitCell,
		Cell missingValueCell,
		Mask baseCellsDigitsMask,
		bool isRow,
		int chuteIndex
	)
	{
		if (valueDigitCell == -1)
		{
			// The value-digit cell shouldn't be missing.
			return null;
		}

		var elimLines = (isRow ? baseCells.RowMask << 9 : baseCells.ColumnMask << 18).AllSets;
		var elimLinesMap = HousesMap[elimLines[0]] | HousesMap[elimLines[1]];
		var intersectedLines = (isRow ? cellsCanBeEliminated.ColumnMask << 18 : cellsCanBeEliminated.RowMask << 9).AllSets;
		var intersectedLinesMap = HousesMap[intersectedLines[0]] | HousesMap[intersectedLines[1]];
		var finalIntersectedFourCells = elimLinesMap & intersectedLinesMap;

		var conclusions = new List<Conclusion>();
		foreach (var cell in HousesMap[BitOperations.TrailingZeroCount(finalIntersectedFourCells.BlockMask)] & ~crossline & ~finalIntersectedFourCells)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var targetCells = from @group in groupsOfTargetCells select @group[0];
		var step = new WeakExocetBzRectangleStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline - missingValueCell select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					new CellViewNode(ColorIdentifier.Auxiliary3, valueDigitCell),
					new CellViewNode(ColorIdentifier.Auxiliary3, missingValueCell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			valueDigitCell,
			missingValueCell,
			baseCells,
			targetCells,
			in crossline
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if DOUBLE_EXOCET
	private static NormalDoubleExocetStep? CheckDoubleBase(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap theOtherBaseCells,
		in CellMap theOtherTargetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask
	)
	{
		var conclusions = new List<Conclusion>();
		foreach (var cell in baseCells.PeerIntersection & theOtherBaseCells.PeerIntersection)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		foreach (var cell in (targetCells | theOtherTargetCells).PeerIntersection)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		if (conclusions.Count == 0)
		{
			// No eliminations found.
			return null;
		}

		var step = new NormalDoubleExocetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells | theOtherBaseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells | theOtherTargetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells | theOtherBaseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			crossline,
			theOtherBaseCells,
			in theOtherTargetCells
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if DOUBLE_EXOCET
	private static DoubleExocetGeneralizedFishStep? CheckDoubleGeneralizedFish(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap theOtherBaseCells,
		in CellMap theOtherTargetCells,
		in CellMap crossline,
		int size,
		bool isRow,
		Mask baseCellsDigitsMask,
		HouseMask housesMask
	)
	{
		var conclusions = new List<Conclusion>();
		var lockedDigitsMask = (Mask)0;
		foreach (var digit in baseCellsDigitsMask)
		{
			var digitDistribution = CandidatesMap[digit] & crossline;
			if (BitOperations.PopCount(isRow ? digitDistribution.ColumnMask : digitDistribution.RowMask) != size - 1)
			{
				// Cannot form a generalized fish.
				continue;
			}

			foreach (var line in isRow ? digitDistribution.ColumnMask << 18 : digitDistribution.RowMask << 9)
			{
				var cells = HousesMap[line] & CandidatesMap[digit] & ~crossline;
				if (!cells)
				{
					continue;
				}

				lockedDigitsMask |= (Mask)(1 << digit);
				foreach (var cell in cells)
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new DoubleExocetGeneralizedFishStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells | theOtherBaseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells | theOtherTargetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in crossline select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells | theOtherBaseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in targetCells | theOtherTargetCells
					from d in grid.GetCandidates(cell)
					where (lockedDigitsMask >> d & 1) != 0
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in crossline
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					let colorIdentifier = (lockedDigitsMask >> d & 1) != 0 ? ColorIdentifier.Auxiliary3 : ColorIdentifier.Auxiliary2
					select new CandidateViewNode(colorIdentifier, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			crossline,
			theOtherBaseCells,
			in theOtherTargetCells
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if COMPLEX_JUNIOR_EXOCET
	private static ComplexExocetLockedMemberStep? CheckComplexJuniorLockedMember(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		int size,
		in CellMap expandedCrosslineIncludingTarget,
		scoped ReadOnlySpan<LockedMember?> lockedMembers,
		int chuteIndex,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells,
		out Mask lockedDigitsMask,
		out Mask inferredLastTargetDigitsMask
	)
	{
		if (baseCells.Count != 2)
		{
			// No conclusions when the number of base cells is not 2.
			(inferredLastTargetDigitsMask, lockedDigitsMask) = (0, 0);
			return null;
		}

		// Check whether the digit is a real locked member.
		if (lockedMembers.IsEmpty || groupsOfTargetCells is not [(_, [var targetCell1]), (_, [var targetCell2])])
		{
			goto AssignBaseMaskByDefault;
		}

		var (cellOffsets, candidateOffsets, houseOffsets) = (new List<CellViewNode>(4), new List<CandidateViewNode>(), new List<HouseViewNode>(2));
		(lockedDigitsMask, inferredLastTargetDigitsMask, var conclusions) = (0, 0, new List<Conclusion>());

		// Collect eliminations via mirror cells.
		// For each locked member, we should check for its mirror cells,
		// determining whether the mirror cells can make this target cell and the other target cell any conclusions.
		var lockedDigitElimTimes = 0;
		foreach (var lockedDigit in baseCellsDigitsMask)
		{
			if (lockedMembers[lockedDigit] is not var (lockedMemberMap, lockedBlock))
			{
				continue;
			}

			var (thisTargetCell, theOtherTargetCell) = HousesMap[lockedBlock].Contains(targetCell1)
				? (targetCell1, targetCell2)
				: (targetCell2, targetCell1);

			// Check whether the target cell is lying in two cross-line cells houses, and lying in the block for the locked member.
			// If so, we should treat this as an invalid case.
			// This is because the intersection cell (this target cell) will be counted twice instead of once.
			// This will fix issue #603 https://github.com/KyouyamaKazusa0805/Sudoku/issues/603
			// and #605 https://github.com/KyouyamaKazusa0805/Sudoku/issues/605
			// This rule is just like an endo-fin. 
			//
			// Example:
			//   +71..+8...4..27..83.+3+85..6+19+7+1.8.+625+7+35+2.13+7.+8..37+85..1..+7.5.836.2+5.6..+7+48+86..7....:
			//     214 425 426 175 975
			var endofinTargetCounter = 0;
			foreach (var house in housesMask | extraHousesMask)
			{
				if (HousesMap[house].Contains(targetCell1))
				{
					endofinTargetCounter++;
				}
			}
			if (endofinTargetCounter >= 2)
			{
				continue;
			}

			var mirrorCellsThisTarget = GetMirrorCells(thisTargetCell, chuteIndex, out _);
			var finalDigitsMask = (mirrorCellsThisTarget & ~EmptyCells) switch
			{
				[] when mirrorCellsThisTarget is [var a, var b]
					=> (Mask)((grid.GetCandidates(a) | grid.GetCandidates(b)) & baseCellsDigitsMask),
				[var a] when mirrorCellsThisTarget - a is [var b]
					=> (Mask)(((Mask)(1 << grid.GetDigit(a)) | grid.GetCandidates(b)) & baseCellsDigitsMask),
				[var a, var b]
					=> (Mask)((1 << grid.GetDigit(a) | 1 << grid.GetDigit(b)) & baseCellsDigitsMask)
			};
			foreach (var digit in (Mask)(grid.GetCandidates(theOtherTargetCell) & ~finalDigitsMask))
			{
				conclusions.Add(new(Elimination, theOtherTargetCell, digit));
			}

			candidateOffsets.AddRange(
				from cell in lockedMemberMap
				select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + lockedDigit)
			);
			houseOffsets.Add(new(ColorIdentifier.Auxiliary1, lockedBlock));

			lockedDigitsMask |= (Mask)(1 << lockedDigit);
			inferredLastTargetDigitsMask |= (Mask)(grid.GetCandidates(theOtherTargetCell) & finalDigitsMask);
			lockedDigitElimTimes++;
		}

		if (lockedDigitElimTimes == 2)
		{
			// Sync candidates in base cells from target cells.
			foreach (var cell in baseCells)
			{
				foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~inferredLastTargetDigitsMask))
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
		}

		if (conclusions.Count == 0)
		{
			goto AssignBaseMaskByDefault;
		}

		var step = new ComplexExocetLockedMemberStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in expandedCrosslineIncludingTarget select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. cellOffsets,
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in targetCells
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					.. candidateOffsets,
					.. houseOffsets,
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			[],
			crossline,
			housesMask,
			extraHousesMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;

	AssignBaseMaskByDefault:
		(inferredLastTargetDigitsMask, lockedDigitsMask) = (baseCellsDigitsMask, 0);
		return null;
	}
#endif

#if COMPLEX_SENIOR_EXOCET
	private static ComplexExocetLockedMemberStep? CheckComplexSeniorLockedMember(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		Cell endoTargetCell,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		int size,
		in CellMap expandedCrosslineIncludingTarget,
		scoped ReadOnlySpan<LockedMember?> lockedMembers,
		int chuteIndex,
		out Mask lockedDigitsMask,
		out Mask inferredLastTargetDigitsMask
	)
	{
		if (baseCells.Count != 2)
		{
			// No conclusions when the number of base cells is not 2.
			(inferredLastTargetDigitsMask, lockedDigitsMask) = (0, 0);
			return null;
		}

		// Check whether the digit is a real locked member.
		if (lockedMembers.IsEmpty)
		{
			goto AssignBaseMaskByDefault;
		}

		// Check whether complex senior exocet patterns contain one cell
		// which is the intersection of the houses chosen in cross-line.
		// If not, the number of the locked member digit cannot determine the maximal times is less than the target times,
		// therefore, the conclusion cannot be raised.
		// This will fix issue #660: https://github.com/KyouyamaKazusa0805/Sudoku/issues/660
		// Counter-example:
		//   6..2...9...5.6.3...9.1.5..7.5...46.......2.7....91...5..1.2.9...6......13..4..2..
		// Technique reference link https://github.com/KyouyamaKazusa0805/Sudoku/discussions/497#discussioncomment-7369213
		var cellsInExtraHouse = CellMap.Empty;
		var cellsInCrossline = CellMap.Empty;
		foreach (var house in extraHousesMask)
		{
			cellsInExtraHouse |= HousesMap[house] & crossline;
		}
		foreach (var house in housesMask)
		{
			cellsInCrossline |= HousesMap[house] & crossline;
		}
		var intersectionCells = cellsInExtraHouse & cellsInCrossline;
		if (intersectionCells.Count == 0)
		{
			goto AssignBaseMaskByDefault;
		}

		var (cellOffsets, candidateOffsets, houseOffsets) = (new List<CellViewNode>(4), new List<CandidateViewNode>(), new List<HouseViewNode>(2));
		(lockedDigitsMask, inferredLastTargetDigitsMask, var conclusions) = (0, 0, new List<Conclusion>());

		// Collect eliminations via mirror cells.
		// For each locked member, we should check for its mirror cells,
		// determining whether the mirror cells can make this target cell and the other target cell any conclusions. Rule:
		//   1) If all mirror cells of a target cell are non-empty, check its containing values and determine
		//      whether one of the digits is concluded as a locked member.
		//      If so, the other side of the target cell can be fixed with that digit (Mirror Synchronization) -
		//      all digits non-fixed can be removed.
		//   2) If one of two mirror cells is non-empty, check it, determining whether it is a locked member digit.
		//      If so, record all digits appeared in the other empty mirror cells, and this locked member digit,
		//      they are possible candidates in the other side of target cell. Other digits can be removed.
		foreach (var lockedDigit in baseCellsDigitsMask)
		{
			if (lockedMembers[lockedDigit] is not var (lockedMemberMap, lockedBlock) || !HousesMap[lockedBlock].Contains(targetCell))
			{
				continue;
			}

			// Check whether the target cell is lying in two cross-line cells houses, and lying in the block for the locked member.
			// If so, we should treat this as an invalid case.
			// This is because the intersection cell (this target cell) will be counted twice instead of once.
			// This will fix issue #603 https://github.com/KyouyamaKazusa0805/Sudoku/issues/603
			// and #605 https://github.com/KyouyamaKazusa0805/Sudoku/issues/605
			// This rule is just like an endo-fin.
			// Counter-example:
			//   +71..+8...4..27..83.+3+85..6+19+7+1.8.+625+7+35+2.13+7.+8..37+85..1..+7.5.836.2+5.6..+7+48+86..7....:214 425 426 175 975
			var endofinTargetCounter = 0;
			foreach (var house in housesMask | extraHousesMask)
			{
				if (HousesMap[house].Contains(targetCell))
				{
					endofinTargetCounter++;
				}
			}
			if (endofinTargetCounter >= 2)
			{
				continue;
			}

			foreach (var digit in (Mask)(grid.GetCandidates(endoTargetCell) & ~(baseCellsDigitsMask & ~(1 << lockedDigit))))
			{
				conclusions.Add(new(Elimination, endoTargetCell, digit));
			}
			foreach (var digit in (Mask)(grid.GetCandidates(targetCell) & ~baseCellsDigitsMask))
			{
				conclusions.Add(new(Elimination, targetCell, digit));
			}

			// Cannibalism rule:
			// If the maximum appearing times of the digit is less than the minimum possible limit,
			// the digit will make exocet pattern unstable. Therefore, the digit cannot be the one appeared in base cells.
			// We can safely remove it from base cells.
			if (grid.AppearingTimesOf(lockedDigit, expandedCrosslineIncludingTarget - targetCell - endoTargetCell) < size)
			{
				foreach (var cell in baseCells + endoTargetCell + targetCell & CandidatesMap[lockedDigit])
				{
					conclusions.Add(new(Elimination, cell, lockedDigit));
				}
			}

			candidateOffsets.AddRange(
				from cell in lockedMemberMap
				select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + lockedDigit)
			);
			houseOffsets.Add(new(ColorIdentifier.Auxiliary1, lockedBlock));

			lockedDigitsMask |= (Mask)(1 << lockedDigit);
			inferredLastTargetDigitsMask |= (Mask)((grid.GetCandidates(endoTargetCell) | grid.GetCandidates(targetCell)) & baseCellsDigitsMask);
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new ComplexExocetLockedMemberStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCell.AsCellMap() + endoTargetCell select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in expandedCrosslineIncludingTarget select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. cellOffsets,
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in targetCell.AsCellMap() + endoTargetCell
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					.. candidateOffsets,
					.. houseOffsets,
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			in targetCell.AsCellMap(),
			in endoTargetCell.AsCellMap(),
			crossline,
			housesMask,
			extraHousesMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;

	AssignBaseMaskByDefault:
		(inferredLastTargetDigitsMask, lockedDigitsMask) = (baseCellsDigitsMask, 0);
		return null;
	}
#endif

#if COMPLEX_SENIOR_EXOCET
	private static ComplexExocetLockedMemberStep? CheckComplexSeniorLockedMemberGrouped(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		in CellMap endoTargetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		Mask almostHiddenSetDigitsMask,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		int size,
		in CellMap expandedCrosslineIncludingTarget,
		scoped ReadOnlySpan<LockedMember?> lockedMembers,
		int chuteIndex,
		out Mask lockedDigitsMask,
		out Mask inferredBaseDigitsMask
	)
	{
		if (baseCells.Count != 2)
		{
			// No conclusions when the number of base cells is not 2.
			(inferredBaseDigitsMask, lockedDigitsMask) = (0, 0);
			return null;
		}

		// Check whether the digit is a real locked member.
		if (lockedMembers.IsEmpty)
		{
			goto AssignBaseMaskByDefault;
		}

		var (cellOffsets, candidateOffsets, houseOffsets) = (new List<CellViewNode>(4), new List<CandidateViewNode>(), new List<HouseViewNode>(2));
		(lockedDigitsMask, inferredBaseDigitsMask, var conclusions) = (0, 0, new List<Conclusion>());

		// Collect eliminations via mirror cells.
		// For each locked member, we should check for its mirror cells,
		// determining whether the mirror cells can make this target cell and the other target cell any conclusions.
		foreach (var lockedDigit in baseCellsDigitsMask)
		{
			if (lockedMembers[lockedDigit] is var (lockedMemberMap, lockedBlock) && HousesMap[lockedBlock].Contains(targetCell))
			{
				foreach (var endoTargetCell in endoTargetCells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(endoTargetCell) & ~almostHiddenSetDigitsMask & ~(baseCellsDigitsMask & ~(1 << lockedDigit))))
					{
						conclusions.Add(new(Elimination, endoTargetCell, digit));
					}
					foreach (var digit in (Mask)(grid.GetCandidates(targetCell) & ~baseCellsDigitsMask))
					{
						conclusions.Add(new(Elimination, targetCell, digit));
					}
				}

				// Cannibalism rule:
				// If the maximum appearing times of the digit is less than the minimum possible limit,
				// the digit will make exocet pattern unstable. Therefore, the digit cannot be the one appeared in base cells.
				// We can safely remove it from base cells.
				if (grid.AppearingTimesOf(lockedDigit, expandedCrosslineIncludingTarget - targetCell & ~endoTargetCells) < size)
				{
					foreach (var cell in (baseCells | endoTargetCells) + targetCell & CandidatesMap[lockedDigit])
					{
						conclusions.Add(new(Elimination, cell, lockedDigit));
					}
				}

				candidateOffsets.AddRange(
					from cell in lockedMemberMap
					select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + lockedDigit)
				);
				houseOffsets.Add(new(ColorIdentifier.Auxiliary1, lockedBlock));

				lockedDigitsMask |= (Mask)(1 << lockedDigit);
				inferredBaseDigitsMask |= (Mask)((grid[endoTargetCells] | grid.GetCandidates(targetCell)) & baseCellsDigitsMask);
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new ComplexExocetLockedMemberStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in endoTargetCells + targetCell select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in expandedCrosslineIncludingTarget select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. cellOffsets,
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in endoTargetCells + targetCell
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in endoTargetCells
					from d in (Mask)(grid.GetCandidates(cell) & almostHiddenSetDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					.. candidateOffsets,
					.. houseOffsets,
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCell.AsCellMap(),
			endoTargetCells,
			crossline,
			housesMask,
			extraHousesMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;

	AssignBaseMaskByDefault:
		(inferredBaseDigitsMask, lockedDigitsMask) = (baseCellsDigitsMask, 0);
		return null;
	}
#endif

#if COMPLEX_JUNIOR_EXOCET
	private static NormalComplexExocetStep? CheckComplexJuniorBase(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		int size,
		in CellMap expandedCrosslineIncludingTarget
	)
	{
		// Check whether the number of digits appeared in base cells should be satisfied (size) rule.
		var allDigitsSatisfySizeRule = true;
		foreach (var digit in baseCellsDigitsMask)
		{
			var lastMap = expandedCrosslineIncludingTarget & ~targetCells;
			if (!grid.IsExactAppearingTimesOf(digit, lastMap, size))
			{
				allDigitsSatisfySizeRule = false;
				break;
			}
		}
		if (!allDigitsSatisfySizeRule)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		foreach (var cell in targetCells)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new NormalComplexExocetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in expandedCrosslineIncludingTarget select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in targetCells
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			[],
			crossline,
			housesMask,
			extraHousesMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if COMPLEX_SENIOR_EXOCET
	private static NormalComplexExocetStep? CheckComplexSeniorBase(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		Cell endoTargetCell,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		int size,
		in CellMap expandedCrosslineIncludingTarget
	)
	{
		// Check whether the number of digits appeared in base cells should be satisfied (size) rule.
		var allDigitsSatisfySizeRule = true;
		foreach (var digit in baseCellsDigitsMask)
		{
			var lastMap = expandedCrosslineIncludingTarget - targetCell - endoTargetCell;
			if (!grid.IsExactAppearingTimesOf(digit, lastMap, size))
			{
				allDigitsSatisfySizeRule = false;
				break;
			}
		}
		if (!allDigitsSatisfySizeRule)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		foreach (var cell in targetCell.AsCellMap() + endoTargetCell)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~baseCellsDigitsMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new NormalComplexExocetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCell.AsCellMap() + endoTargetCell select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					.. from cell in expandedCrosslineIncludingTarget select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in targetCell.AsCellMap() + endoTargetCell
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCell.AsCellMap(),
			endoTargetCell.AsCellMap(),
			crossline,
			housesMask,
			extraHousesMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if COMPLEX_SENIOR_EXOCET
	private static AdvancedComplexSeniorExocetStep? CheckAdvancedComplexSenior(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		Cell targetCell,
		in CellMap endoTargetCellsGroup,
		in CellMap crossline,
		Mask baseCellsDigitsMask,
		Mask inferredBaseDigitsMask,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		int size,
		Mask almostHiddenSetMask,
		in CellMap expandedCrosslineIncludingTarget
	)
	{
		var conclusions = new List<Conclusion>();
		foreach (var cell in endoTargetCellsGroup)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~inferredBaseDigitsMask & ~almostHiddenSetMask))
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		foreach (var digit in (Mask)(grid.GetCandidates(targetCell) & ~inferredBaseDigitsMask))
		{
			conclusions.Add(new(Elimination, targetCell, digit));
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var step = new AdvancedComplexSeniorExocetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in endoTargetCellsGroup + targetCell select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					..
					from cell in expandedCrosslineIncludingTarget
					select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in endoTargetCellsGroup
					from digit in (Mask)(grid.GetCandidates(cell) & almostHiddenSetMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary1, cell * 9 + digit),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in endoTargetCellsGroup + targetCell
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCell.AsCellMap(),
			endoTargetCellsGroup,
			crossline,
			housesMask,
			extraHousesMask,
			almostHiddenSetMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);

		return null;
	}
#endif

#if COMPLEX_JUNIOR_EXOCET
	private static ComplexJuniorExocetAdjacentTargetStep? CheckComplexJuniorAdjacentTarget(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap expandedCrosslineIncludingTarget,
		Mask baseCellsDigitsMask,
		bool isRow,
		int chuteIndex,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells
	)
	{
		// Adjacent target cannot be used for same-side target cells.
		if (targetCells.FirstSharedHouse != 32)
		{
			return null;
		}

		var (conclusions, singleMirrors) = (new List<Conclusion>(), CellMap.Empty);
		foreach (ref readonly var cellGroup in groupsOfTargetCells)
		{
			if (cellGroup.Count == 2)
			{
				// This side contain 2 target empty cells. We cannot conclude for this case.
				continue;
			}

			foreach (var targetCell in cellGroup)
			{
				var mirrorCells = GetMirrorCells(targetCell, chuteIndex, out _);
				if ((mirrorCells & EmptyCells) is not [var theOnlyMirrorCell])
				{
					// The mirror cells contain not 1 cell, it may not be included in this type.
					continue;
				}

				// Try to get the target cell that is not share with a same block with this mirror cell.
				var theOtherTargetCells = targetCells - targetCell;
				if (theOtherTargetCells is not [var theOtherTargetCell])
				{
					// The number of the other side of target cell is not 1, we cannot conclude for that.
					continue;
				}

				// Check for the only mirror cell, determining whether the cell contains an arbitrary extra digits.
				var digitsInMirrorCell = grid.GetCandidates(theOnlyMirrorCell);
				var elimDigitsFromTheOnlyMirrorCell = (Mask)(digitsInMirrorCell & ~baseCellsDigitsMask);

				// Check for the containing digits in mirror cells, and fetch which digits are appeared in base cells.
				// Such digits will be synchronized with the other target cell.
				var containedDigitsAppearedInBaseCellsInMirror = (Mask)(digitsInMirrorCell & baseCellsDigitsMask);
				var elimDigitsFromTheOtherTargetCell = (Mask)(grid.GetCandidates(theOtherTargetCell) & ~containedDigitsAppearedInBaseCellsInMirror);

				// Try to fetch eliminations.
				if (elimDigitsFromTheOnlyMirrorCell != 0)
				{
					foreach (var elimDigit in elimDigitsFromTheOnlyMirrorCell)
					{
						conclusions.Add(new(Elimination, theOnlyMirrorCell, elimDigit));
					}
				}
				if (elimDigitsFromTheOtherTargetCell != 0)
				{
					foreach (var elimDigit in elimDigitsFromTheOtherTargetCell)
					{
						conclusions.Add(new(Elimination, theOtherTargetCell, elimDigit));
					}
				}

				singleMirrors.Add(theOnlyMirrorCell);
			}
		}

		if (conclusions.Count == 0 || !singleMirrors)
		{
			// No eliminations found.
			return null;
		}

		var step = new ComplexJuniorExocetAdjacentTargetStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					..
					from cell in expandedCrosslineIncludingTarget & ~targetCells
					select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					.. from cell in singleMirrors select new CellViewNode(ColorIdentifier.Auxiliary3, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget & ~targetCells
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			expandedCrosslineIncludingTarget,
			housesMask,
			extraHousesMask,
			in singleMirrors
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

#if COMPLEX_JUNIOR_EXOCET
	private static ComplexJuniorExocetMirrorConjugatePairStep? CheckComplexJuniorMirrorConjugatePair(
		ref StepAnalysisContext context,
		Grid grid,
		in CellMap baseCells,
		in CellMap targetCells,
		in CellMap expandedCrosslineIncludingTarget,
		Mask baseCellsDigitsMask,
		bool isRow,
		int chuteIndex,
		HouseMask housesMask,
		HouseMask extraHousesMask,
		scoped ReadOnlySpan<TargetCellsGroup> groupsOfTargetCells
	)
	{
		// Mirror conjugate pair cannot be used for same-side target cells.
		if (targetCells.Count != 2 || targetCells.FirstSharedHouse != 32)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		var conjugatePairs = new List<Conjugate>(2);
		if (groupsOfTargetCells.Length != 2)
		{
			return null;
		}

		foreach (ref readonly var cellGroup in groupsOfTargetCells)
		{
			if (cellGroup.Count == 2)
			{
				// If the number of target cells in one side is 2, we cannot determine which one is correct.
				continue;
			}

			foreach (var targetCell in cellGroup)
			{
				var theOtherTwoCells = GetMirrorCells(targetCell, chuteIndex, out _);
				var theOtherEmptyCells = theOtherTwoCells & EmptyCells;
				if (!theOtherEmptyCells)
				{
					// The current miniline cannot contain any eliminations.
					continue;
				}

				var otherCellsDigitsMask = grid[theOtherEmptyCells];
				foreach (var house in theOtherEmptyCells.SharedHouses)
				{
					// Check whether the current house has a conjugate pair in the current cells.
					foreach (var digit in otherCellsDigitsMask)
					{
						var cellsContainingDigit = (CandidatesMap[digit] & HousesMap[house]) - targetCell;
						if ((theOtherEmptyCells & cellsContainingDigit) != cellsContainingDigit)
						{
							continue;
						}

						// Here a conjugate pair will be formed.
						// Now check for eliminations.
						foreach (var elimCell in theOtherEmptyCells)
						{
							foreach (var elimDigit in (Mask)(grid.GetCandidates(elimCell) & ~baseCellsDigitsMask & ~(1 << digit)))
							{
								conclusions.Add(new(Elimination, elimCell, elimDigit));
							}
						}

						conjugatePairs.Add(new(theOtherEmptyCells, digit));
					}
				}
			}
		}
		if (conclusions.Count == 0)
		{
			// No eliminations found.
			return null;
		}

		var step = new ComplexJuniorExocetMirrorConjugatePairStep(
			conclusions.AsMemory(),
			[
				[
					.. from cell in baseCells select new CellViewNode(ColorIdentifier.Normal, cell),
					.. from cell in targetCells select new CellViewNode(ColorIdentifier.Auxiliary1, cell),
					..
					from cell in expandedCrosslineIncludingTarget & ~targetCells
					select new CellViewNode(ColorIdentifier.Auxiliary2, cell),
					..
					from cell in baseCells
					from d in grid.GetCandidates(cell)
					select new CandidateViewNode(ColorIdentifier.Normal, cell * 9 + d),
					..
					from cell in expandedCrosslineIncludingTarget & ~targetCells
					where grid.GetState(cell) == CellState.Empty
					from d in (Mask)(grid.GetCandidates(cell) & baseCellsDigitsMask)
					select new CandidateViewNode(ColorIdentifier.Auxiliary2, cell * 9 + d),
					..
					from conjugatePair in conjugatePairs
					from cell in conjugatePair.Map
					where grid.GetState(cell) == CellState.Empty && (grid.GetCandidates(cell) >> conjugatePair.Digit & 1) != 0
					select new CandidateViewNode(ColorIdentifier.Auxiliary3, cell * 9 + conjugatePair.Digit),
					//.. from house in housesMask select new HouseViewNode(ColorIdentifier.Auxiliary2, house)
				]
			],
			context.Options,
			baseCellsDigitsMask,
			baseCells,
			targetCells,
			expandedCrosslineIncludingTarget,
			housesMask,
			extraHousesMask,
			[.. conjugatePairs]
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
#endif

	/// <summary>
	/// Try to fetch locked members in the pattern, and verify validity of the pattern.
	/// </summary>
	/// <param name="grid">The grid to be checked.</param>
	/// <param name="baseCellsDigitsMask">The mask that holds a list of digits appeared in base cells.</param>
	/// <param name="baseCells">The base cells.</param>
	/// <param name="targetCells">The target cells.</param>
	/// <param name="fullCrossline">The cross-line cells. The value can cover extra houses on complex exocets.</param>
	/// <param name="times">The desired times of appearing of digits <paramref name="baseCellsDigitsMask"/>.</param>
	/// <param name="digitsMaskExactlySizeMinusOneTimes">The digits that appeared exactly (<paramref name="times"/>) times.</param>
	/// <param name="digitsMaskAppearedInCrossline">The digits whose value representations are appeared in cross-line cells.</param>
	/// <param name="lockedMembers">The locked members returned.</param>
	/// <param name="lockedMemberDigitsMask">The found digits as locked members.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the pattern is valid.</returns>
	private static bool CheckValidityAndLockedMembersExistence(
		in Grid grid,
		Mask baseCellsDigitsMask,
		in CellMap baseCells,
		in CellMap targetCells,
		scoped in CellMap fullCrossline,
		int times,
		out Mask digitsMaskExactlySizeMinusOneTimes,
		out Mask digitsMaskAppearedInCrossline,
		out ReadOnlySpan<LockedMember?> lockedMembers,
		out Mask lockedMemberDigitsMask
	)
	{
		(digitsMaskExactlySizeMinusOneTimes, digitsMaskAppearedInCrossline) = (0, 0);
		foreach (var digit in baseCellsDigitsMask)
		{
			if (grid.IsExactAppearingTimesOf(digit, fullCrossline, times))
			{
				// The current digit can be filled in cross-line cells at most (size) times.
				digitsMaskExactlySizeMinusOneTimes |= (Mask)(1 << digit);
			}

			foreach (var cell in fullCrossline)
			{
				if (grid.GetDigit(cell) == digit)
				{
					digitsMaskAppearedInCrossline |= (Mask)(1 << digit);
					break;
				}
			}
		}

		lockedMemberDigitsMask = baseCellsDigitsMask;

		// Filters the digits they may not be locked members when they matches exactly appeared (size) times.
		lockedMembers = GetLockedMembers(baseCells, targetCells, ref lockedMemberDigitsMask);

		// Check whether at least one digit appeared in base cells isn't locked and doesn't satisfy the (size) rule.
		var atLeastOneDigitIsNotLockedAndNotSatisfyCrosslineAppearingRule = false;
		foreach (var filteredDigit in (Mask)(baseCellsDigitsMask & ~lockedMemberDigitsMask))
		{
			if ((digitsMaskExactlySizeMinusOneTimes >> filteredDigit & 1) == 0)
			{
				atLeastOneDigitIsNotLockedAndNotSatisfyCrosslineAppearingRule = true;
				break;
			}
		}
		return !atLeastOneDigitIsNotLockedAndNotSatisfyCrosslineAppearingRule;
	}

	/// <summary>
	/// Try to get the mirror cells for the specified target cell at the specified index of the chute.
	/// </summary>
	/// <param name="chuteIndex">The chute index (in range 0..6).</param>
	/// <param name="targetCell">The target cell.</param>
	/// <param name="miniline">The miniline cells the target cell and mirror cells lie in.</param>
	/// <returns>The mirror cells that may contain non-empty cells.</returns>
	private static CellMap GetMirrorCells(Cell targetCell, int chuteIndex, out CellMap miniline)
	{
		Unsafe.SkipInit(out miniline);
		foreach (ref readonly var temp in Miniline.MinilinesGroupedByChuteIndex[chuteIndex].AsReadOnlySpan())
		{
			if (temp.Contains(targetCell))
			{
				miniline = temp;
				break;
			}
		}
		return miniline - targetCell;
	}

	/// <summary>
	/// Try to fetch all possible complex houses.
	/// </summary>
	/// <param name="isRow">Indicates whether the cross-line cells are at row direction.</param>
	/// <param name="baseCells">The base cells.</param>
	/// <returns>A mask that holds a list of houses that can be complex houses.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static HouseMask GetComplexHouses(bool isRow, in CellMap baseCells)
		=> HouseMaskOperations.AllHousesMask
			& ~(isRow ? HouseMaskOperations.AllRowsMask : HouseMaskOperations.AllColumnsMask)
			& ~baseCells.SharedHouses;

	/// <summary>
	/// Try to create a <see cref="ReadOnlySpan{T}"/> of <see cref="CellMap"/> instances
	/// indicating the locked members for the digit at the specified index.
	/// </summary>
	/// <param name="baseCells">The base cells.</param>
	/// <param name="targetCells">The target cells.</param>
	/// <param name="lockedDigitsMask">A mask that holds a list of digits that may be locked members.</param>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of <see cref="CellMap"/> instances.</returns>
	private static ReadOnlySpan<LockedMember?> GetLockedMembers(
		in CellMap baseCells,
		in CellMap targetCells,
		scoped ref Mask lockedDigitsMask
	)
	{
		var (flag, r, realLockedDigitsMask) = (false, new LockedMember?[9], (Mask)0);
		foreach (var lockedDigit in lockedDigitsMask)
		{
			var (lockedMemberMap, lockedBlock) = (CellMap.Empty, -1);
			foreach (var block in targetCells.BlockMask)
			{
				var lastMap = HousesMap[block] & ~targetCells & CandidatesMap[lockedDigit];
				if (!!lastMap && (HousesMap[baseCells.SharedLine] & lastMap) == lastMap)
				{
					(lockedMemberMap, lockedBlock) = (lastMap, block);
					break;
				}
			}
			if (lockedBlock == -1)
			{
				continue;
			}

			r[lockedDigit] = new(lockedMemberMap, lockedBlock);
			realLockedDigitsMask |= (Mask)(1 << lockedDigit);
			flag = true;
		}

		(var result, lockedDigitsMask) = flag ? (r, realLockedDigitsMask) : ([], 0);
		return result;
	}
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Try to get the maximum times that the specified digit, describing it can be filled with the specified houses in maximal case.
	/// </summary>
	/// <param name="this">The grid to be checked.</param>
	/// <param name="digit">The digit to be checked.</param>
	/// <param name="cells">The cells to be checked.</param>
	/// <returns>The maximum possible times of the appearing.</returns>
	public static int AppearingTimesOf(this in Grid @this, Digit digit, in CellMap cells)
	{
		var (activeCells, inactiveCells) = (CandidatesMap[digit] & cells, ValuesMap[digit] & cells);
		for (var i = Math.Min(9, activeCells.Count); i >= 1; i--)
		{
			foreach (ref readonly var cellsCombination in activeCells & i)
			{
				if (!cellsCombination.CanSeeEachOther && ((cellsCombination.ExpandedPeers | cellsCombination) & activeCells) == activeCells)
				{
					return i + inactiveCells.Count;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Try to get the maximum times that the specified digit, describing it can be filled with the specified houses in maximal case.
	/// </summary>
	/// <param name="this">The grid to be checked.</param>
	/// <param name="digit">The digit to be checked.</param>
	/// <param name="cells">The cells to be checked.</param>
	/// <param name="limitCount">The number of times that the digit can be filled with the specified cells.</param>
	/// <returns>A <see cref="bool"/> result indicating whether the argument <paramref name="limitCount"/> is exactly correct.</returns>
	public static bool IsExactAppearingTimesOf(this in Grid @this, Digit digit, in CellMap cells, int limitCount)
	{
		var (activeCells, inactiveCells) = (CandidatesMap[digit] & cells, ValuesMap[digit] & cells);
		if (!activeCells && limitCount == inactiveCells.Count)
		{
			return true;
		}

		for (var i = Math.Min(9, activeCells.Count); i >= 1; i--)
		{
			foreach (ref readonly var cellsCombination in activeCells & i)
			{
				if (!cellsCombination.CanSeeEachOther && ((cellsCombination.ExpandedPeers | cellsCombination) & activeCells) == activeCells)
				{
					return i + inactiveCells.Count == limitCount;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Try to group up with target cells, separating into multiple parts, grouped by its containing row or column.
	/// </summary>
	/// <param name="this">The target cells to be split.</param>
	/// <param name="houses">The mask value holding a list of houses to be matched.</param>
	/// <returns>
	/// A list of <see cref="CellMap"/> grouped, representing as a <see cref="TargetCellsGroup"/>.
	/// </returns>
	/// <seealso cref="TargetCellsGroup"/>
	public static ReadOnlySpan<TargetCellsGroup> GroupTargets(this in CellMap @this, HouseMask houses)
	{
		var (result, i) = (new TargetCellsGroup[BitOperations.PopCount(houses)], 0);
		foreach (var house in houses)
		{
			if ((@this & HousesMap[house]) is var map and not [])
			{
				result[i++] = new(house, map);
			}
		}
		return result.AsReadOnlySpan()[..i];
	}
}
