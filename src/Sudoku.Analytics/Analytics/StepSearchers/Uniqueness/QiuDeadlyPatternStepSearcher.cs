namespace Sudoku.Analytics.StepSearchers.Uniqueness;

/// <summary>
/// Provides with a <b>Qiu's Deadly Pattern</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Standard types:
/// <list type="bullet">
/// <item>Qiu's Deadly Pattern Type 1</item>
/// <item>Qiu's Deadly Pattern Type 2</item>
/// <item>Qiu's Deadly Pattern Type 3</item>
/// <item>Qiu's Deadly Pattern Type 4</item>
/// <item>Qiu's Deadly Pattern Locked Type</item>
/// </list>
/// </item>
/// <item>
/// External types:
/// <list type="bullet">
/// <item>Qiu's Deadly Pattern External Type 1</item>
/// <item>Qiu's Deadly Pattern External Type 2</item>
/// </list>
/// </item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_QiuDeadlyPatternStepSearcher",
	Technique.QiuDeadlyPatternType1, Technique.QiuDeadlyPatternType2,
	Technique.QiuDeadlyPatternType3, Technique.QiuDeadlyPatternType4, Technique.LockedQiuDeadlyPattern,
	Technique.QiuDeadlyPatternExternalType1, Technique.QiuDeadlyPatternExternalType2,
	SupportedSudokuTypes = SudokuType.Standard,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class QiuDeadlyPatternStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the patterns for case 1.
	/// </summary>
	private static readonly QiuDeadlyPattern1Pattern[] PatternsForCase1;

	/// <summary>
	/// Indicates the patterns for case 2.
	/// </summary>
	private static readonly QiuDeadlyPattern2Pattern[] PatternsForCase2;

	/// <summary>
	/// Indicates the line offsets of the patterns.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly RowIndex[][] LineOffsets = [[0, 1, 2], [0, 2, 1], [1, 2, 0], [3, 4, 5], [3, 5, 4], [4, 5, 3], [6, 7, 8], [6, 8, 7], [7, 8, 6]];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static QiuDeadlyPatternStepSearcher()
	{
		// Case 1: 2 lines + 2 cells.
		var patternsForCase1 = new List<QiuDeadlyPattern1Pattern>();
		foreach (var isRow in (true, false))
		{
			var (@base, fullHousesMask) = isRow ? (9, HouseMaskOperations.AllRowsMask) : (18, HouseMaskOperations.AllColumnsMask);
			foreach (var lineOffsetPair in LineOffsets)
			{
				var (l1, l2, l3) = (lineOffsetPair[0] + @base, lineOffsetPair[1] + @base, lineOffsetPair[2] + @base);
				var linesMask = 1 << l1 | 1 << l2;
				foreach (var cornerHouse in fullHousesMask & ~linesMask & ~(1 << l3))
				{
					foreach (var posPair in LineOffsets)
					{
						patternsForCase1.Add(new([HousesCells[cornerHouse][posPair[0]], HousesCells[cornerHouse][posPair[1]]], linesMask));
					}
				}
			}
		}
		PatternsForCase1 = [.. patternsForCase1];

		// Case 2: 2 rows + 2 columns.
		var patternsForCase2 = new List<QiuDeadlyPattern2Pattern>();
		var rows = HouseMaskOperations.AllRowsMask.AllSets;
		var columns = HouseMaskOperations.AllColumnsMask.AllSets;
		foreach (var lineOffsetPairRow in LineOffsets)
		{
			var rowsMask = 1 << rows[lineOffsetPairRow[0]] | 1 << rows[lineOffsetPairRow[1]];
			foreach (var lineOffsetPairColumn in LineOffsets)
			{
				var columnsMask = 1 << columns[lineOffsetPairColumn[0]] | 1 << columns[lineOffsetPairColumn[1]];
				patternsForCase2.Add(new(rowsMask, columnsMask));
			}
		}
		PatternsForCase2 = [.. patternsForCase2];
	}


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		// Handle for case 1.
		foreach (var pattern in PatternsForCase1)
		{
			if (Collect(ref context, pattern) is { } step)
			{
				return step;
			}
		}

		// Handle for case 2.
		foreach (var pattern in PatternsForCase2)
		{
			if (Collect(ref context, pattern) is { } step)
			{
				return step;
			}
		}
		return null;
	}

	/// <summary>
	/// <inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/summary"/>
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="pattern">The target pattern.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternStep? Collect(ref StepAnalysisContext context, QiuDeadlyPattern1Pattern pattern)
	{
		ref readonly var grid = ref context.Grid;

		// We should check for the distinction for the lines.
		var lines = pattern.Lines;
		var l1 = BitOperations.TrailingZeroCount(lines);
		var l2 = lines.GetNextSet(l1);
		var valueCellsInBothLines = CellMap.Empty;
		foreach (var cell in HousesMap[l1] | HousesMap[l2])
		{
			if (grid.GetState(cell) != CellState.Empty)
			{
				valueCellsInBothLines.Add(cell);
			}
		}

		if (pattern.Corner & ~EmptyCells)
		{
			// Corners cannot be non-empty.
			return null;
		}

		var isRow = l1 is >= 9 and < 18;
		if (CheckForBaseType(ref context, grid, pattern, valueCellsInBothLines, isRow) is { } type1Step)
		{
			return type1Step;
		}
		return null;
	}

	/// <summary>
	/// <inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/summary"/>
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="pattern">The target pattern.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternStep? Collect(ref StepAnalysisContext context, QiuDeadlyPattern2Pattern pattern)
	{
		// TODO: Re-implement later.
		return null;
	}

	/// <summary>
	/// Check for base type.
	/// </summary>
	private QiuDeadlyPatternStep? CheckForBaseType(
		ref StepAnalysisContext context,
		in Grid grid,
		QiuDeadlyPattern1Pattern pattern,
		in CellMap valueCellsInBothLines,
		bool isRow
	)
	{
		var lines = pattern.Lines;
		var l1 = BitOperations.TrailingZeroCount(lines);
		var l2 = lines.GetNextSet(l1);

		// Check whether both two lines are finished.
		if (((HousesMap[l1] | HousesMap[l2]) & ~EmptyCells).Count == 18)
		{
			return null;
		}

		var alignedPosMask = (Mask)0;
		var (l1AlignedMask, l2AlignedMask) = ((Mask)0, (Mask)0);
		for (var pos = 0; pos < 9; pos++)
		{
			if (valueCellsInBothLines.Contains(HousesCells[l1][pos]))
			{
				alignedPosMask |= (Mask)(1 << pos);
				l1AlignedMask |= (Mask)(1 << pos);
			}
			if (valueCellsInBothLines.Contains(HousesCells[l2][pos]))
			{
				alignedPosMask |= (Mask)(1 << pos);
				l2AlignedMask |= (Mask)(1 << pos);
			}
		}
		if (BitOperations.PopCount((Mask)(l1AlignedMask | l2AlignedMask)) > Math.Max(BitOperations.PopCount(l1AlignedMask), BitOperations.PopCount(l2AlignedMask)))
		{
			// Distinction is not 1.
			return null;
		}

		// Check whether the paired cells contain at least 2 empty cells and not in a block.
		var emptyCellsInPairedCells = CellMap.Empty;
		foreach (var pos in alignedPosMask)
		{
			if (HousesCells[l1][pos] is var cell1 && grid.GetState(cell1) == CellState.Empty)
			{
				emptyCellsInPairedCells.Add(cell1);
			}
			if (HousesCells[l2][pos] is var cell2 && grid.GetState(cell2) == CellState.Empty)
			{
				emptyCellsInPairedCells.Add(cell2);
			}
		}
		if (emptyCellsInPairedCells.Count >= 2)
		{
			// Distinction is not 1.
			return null;
		}

		// Check whether the digits contain only 1 digit different.
		var nonEmptyCellsDigitsMaskForLine1 = grid[valueCellsInBothLines & HousesMap[l1]];
		var nonEmptyCellsDigitsMaskForLine2 = grid[valueCellsInBothLines & HousesMap[l2]];
		if (BitOperations.PopCount((Mask)(nonEmptyCellsDigitsMaskForLine1 ^ nonEmptyCellsDigitsMaskForLine2)) >= 2)
		{
			return null;
		}

		// Check whether at least 2 cells is empty in cross-line.
		var crossline = pattern.Crossline;
		if ((crossline & ~EmptyCells).Count >= 3)
		{
			return null;
		}

		// Check whether the number of locked digits appeared in cross-line is at least 2.
		var block = crossline.FirstSharedHouse;
		var allDigitsMaskNotAppearedInCrossline = grid[HousesMap[block] & ~crossline];
		var allDigitsMaskAppearedInCrossline = grid[crossline];
		var cornerLockedDigitsMask = (Mask)(allDigitsMaskAppearedInCrossline & ~allDigitsMaskNotAppearedInCrossline);
		if (BitOperations.PopCount(cornerLockedDigitsMask) < 2)
		{
			return null;
		}

		var corner = pattern.Corner;

		// Check whether corner cells intersect with at least 1 digit. e.g. Digits [1, 2, 3] & [1, 4].
		// This is necessary because we should guarantee the target eliminations should cover at least one digit.
		// For example, digits [1, 2, 3] and [1, 4] intersect with digit [1]. If the cross-line locks the digit [1, 2],
		// then we can conclude that if 1 is set in [1, 4] and 2 is set in [1, 2, 3], the target pattern will form a deadly pattern.
		var cornerDigitsMaskIntersected = grid[corner, false, MaskAggregator.And];
		if (cornerDigitsMaskIntersected == 0)
		{
			return null;
		}

		// Check whether cross-line cells contain the base digits.
		var cornerDigitsMask = grid[corner];
		if ((grid[crossline & ~EmptyCells, true] & cornerDigitsMask) != 0)
		{
			return null;
		}

		// Check whether at least one cross-line has fully covered by value digits.
		// Counter-example (Wrong):
		//   .+9.5+17...51.9+4.+7.....38.+59124+5+7+6+1+839.....+9.+5.96...+5.1773..54.......+93.75.+5.2+78...:411 611 413 417 353 853 173 873 883 687
		var atLeastOneCrosslineIsFullyCoveredByValueDigits = false;
		foreach (var baseCell in pattern.Corner)
		{
			var lineType = BitOperations.TrailingZeroCount(pattern.Lines) < 18 ? HouseType.Row : HouseType.Column;
			var lineTypeTransposed = lineType == HouseType.Row ? HouseType.Column : HouseType.Row;
			var coveredCrossline = PeersMap[baseCell] & HousesMap[baseCell.ToHouse(lineTypeTransposed)] & pattern.Crossline;
			if (!(coveredCrossline & EmptyCells))
			{
				atLeastOneCrosslineIsFullyCoveredByValueDigits = true;
				break;
			}
		}
		if (atLeastOneCrosslineIsFullyCoveredByValueDigits)
		{
			return null;
		}

		// Okay. Now we have a rigorous pattern. Now check for mask and determine its sub-type that can be categorized.

		//
		// Part I - Corner-cell Types (Type 1-4)
		//
		var maskIntersected = cornerLockedDigitsMask & cornerDigitsMaskIntersected;
		if (maskIntersected == cornerDigitsMaskIntersected)
		{
			// One cell only holds those two and the other doesn't only hold them.
			var cornerExtraDigitsMask = (Mask)(cornerDigitsMask & ~cornerDigitsMaskIntersected);
			var cornerContainingExtraDigit = corner;
			var tempMap = CellMap.Empty;
			foreach (var digit in cornerExtraDigitsMask)
			{
				tempMap |= CandidatesMap[digit];
			}
			cornerContainingExtraDigit &= tempMap;

			if (BaseType_Type1(
				ref context, corner, crossline, grid, l1, l2,
				cornerDigitsMaskIntersected, cornerContainingExtraDigit) is { } type1Step)
			{
				return type1Step;
			}

			if (BaseType_Type2(
				ref context, corner, crossline, grid, l1, l2, cornerDigitsMaskIntersected,
				cornerExtraDigitsMask, cornerContainingExtraDigit) is { } type2Step)
			{
				return type2Step;
			}

			if (BaseType_Type3(
				ref context, corner, crossline, grid, l1, l2, cornerDigitsMaskIntersected,
				cornerExtraDigitsMask, cornerContainingExtraDigit) is { } type3Step)
			{
				return type3Step;
			}

			if (BaseType_Type4(ref context, corner, crossline, l1, l2, cornerDigitsMaskIntersected, cornerContainingExtraDigit)
				is { } type4Step)
			{
				return type4Step;
			}
		}
		else if (maskIntersected != 0)
		{
			if (BaseType_TypeLocked(ref context, corner, crossline, grid, l1, l2, cornerLockedDigitsMask) is { } typeLockedStep)
			{
				return typeLockedStep;
			}
		}

		//
		// Part II - Guardian Types (External Type 1-2)
		//
		if ((grid[crossline] & cornerDigitsMask) == cornerDigitsMask)
		{
			// Check whether the number of empty cross-line cells are same as the number of locked digits.
			// Counter-example:
			//   .....+8936+95+31+64+7+8+2.+6+8..+9+5+1+46.9.+1+78+2+5...+8.+6+3+9..+8.+9.5+64..+9.68+1+4+5+3+81+6..+3+27+9...+792+1+6+8:211 711 412 213 713 215 235 251 451 751 452 261 761
			if (BitOperations.PopCount(cornerDigitsMask) > (crossline & EmptyCells).Count)
			{
				return null;
			}

			// Now check for subtypes.
			var mirror = pattern.Mirror;
			if (BaseType_ExternalType1(ref context, corner, crossline, mirror, grid, l1, l2, cornerDigitsMask) is { } externalType1Step)
			{
				return externalType1Step;
			}

			if (BaseType_ExternalType2(ref context, corner, crossline, mirror, grid, l1, l2, cornerDigitsMask) is { } externalType2Step)
			{
				return externalType2Step;
			}
		}

		// No valid steps found. Return null.
		return null;
	}

	/// <summary>
	/// Check for base type (type 1).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType1Step? BaseType_Type1(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		in CellMap cornerContainingExtraDigit
	)
	{
		// Test examples:
		// 12...+4+35..731.5..+4+4...23..1...8..41.+8+145.2..3...4.+158.....17.+49.913+48..+574..+5.+13.:715 632 833 341 641 543 643 645 955 661 663 665 271 671 672 673 299
		// .48.163.736...2.4.5..3.....+4.....+1.8...451+7..1.....+4..+7....9..4.2.1...736.478.21+9:918 938 862 574
		// +4+6+2+9+517+38.7.+843..6.+83+62+7..474+8+3+6+25.....7+89+6+4...6+4+1+5.878.+7+13+64.+53..57+4+86.6+542+9+8.+7.:928 938 252 159 262 967 189
		// 1+6.72+48.....65.....57....6+4.2647+59+1....+1.+2+6...+71.8654+2+79..+6.42+1+6...19+7...+18.47+3+96:326 329 331 931 359 374 583 584 884
		// 5....4+762..+4...5+9.....6.41.2.+7.5..34..6+48+32+7.43..7...9.72.9..+4...1.+4....84.3.+6..7:321 126 331 942 144 867 677 379 879 687 588 389 889
		// 1+85+43.62.+9.4.8....7....1...6..8....+1+2186+9473+5+5....3..6+8..3....4+4...2.3..+351+74.9.2:222 234 537 938 942 746 962 467 672 973 578 983 688
		// +412..39+5+7+3+97.4+5..86+8+5..+7.4+3.+2..7.38..+3.5.1....69+32.....5..+3...19+7..5.8+3..+437..59.:624 227 161 274 876 284
		// ..+3......+2.+7...659+95..27..3+4398+16+5+27.2.975.3.1+7+52+4398+67+8+246+1+39+5561......+3+9+4....+6.:815 816 117 417 418 119 419 137
		// .+2.75.....9.+3...2.5+8392...1+47+2+59+3186.+3.+8...+5.856+21..3.2+1.+4356.8+36+8+1+7+9.4..+4+5+682.+1.:416 616 426 626 727 729 457 957 959
		// 4.5+8+1+6..9.7.94.+56116+9+5....+4+9+5+736+14+28+6+8+1...+3+5+72+43+785+1+9+6.......1571..58+94+35..+1..6.2:737 738 273 274 276
		// +8.....7219...7.34+6..+7..4+5+8+9.+7+92.+54383...9..755.......+2+49+36812+5+77...5.8...........:232 135 154 662 663 164 192 292 692 193 293 693 394 296

		if (cornerContainingExtraDigit is not [var targetCell])
		{
			return null;
		}

		var elimDigitsMask = (Mask)(grid.GetCandidates(targetCell) & digitsMaskAppearedInCorner);
		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var digit in digitsMaskAppearedInCorner)
		{
			foreach (var cell in CandidatesMap[digit] & crossline)
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}

		var theOtherCornerCellNoElimination = (corner - targetCell)[0];
		foreach (var digit in grid.GetCandidates(theOtherCornerCellNoElimination))
		{
			candidateOffsets.Add(new(ColorIdentifier.Normal, theOtherCornerCellNoElimination * 9 + digit));
		}

		var step = new QiuDeadlyPatternType1Step(
			(from digit in elimDigitsMask select new Conclusion(Elimination, targetCell, digit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			targetCell,
			elimDigitsMask
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (type 2).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="extraDigitsMask">Indicates a mask value that holds extra digits.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType2Step? BaseType_Type2(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		Mask extraDigitsMask,
		in CellMap cornerContainingExtraDigit
	)
	{
		if (!Mask.IsPow2(extraDigitsMask))
		{
			return null;
		}

		var extraDigit = Mask.Log2(extraDigitsMask);
		var elimMap = corner % CandidatesMap[extraDigit];
		if (!elimMap)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in cornerContainingExtraDigit)
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
		}
		foreach (var digit in digitsMaskAppearedInCorner)
		{
			foreach (var cell in CandidatesMap[digit] & crossline)
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}

		var step = new QiuDeadlyPatternType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, extraDigit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			extraDigit
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (type 3).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <param name="extraDigitsMask">Indicates a mask value that holds extra digits.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType3Step? BaseType_Type3(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		Mask extraDigitsMask,
		in CellMap cornerContainingExtraDigit
	)
	{
		// Test examples:
		// 1.2..+6348.4....5+76.6....+9122.6..4....3.6......1..5.+6......+61..9.+2.93.76.6....2..5:324 824 825 925 348 368

		if (Mask.IsPow2(digitsMaskAppearedInCorner))
		{
			return null;
		}

		if (Mask.IsPow2(extraDigitsMask))
		{
			return null;
		}

		if (cornerContainingExtraDigit != corner)
		{
			return null;
		}

		var size = BitOperations.PopCount(digitsMaskAppearedInCorner);
		foreach (var cornerCellCoveredHouse in corner.SharedHouses)
		{
			var emptyCellsInCurrentHouse = HousesMap[cornerCellCoveredHouse] & EmptyCells;
			var availableCellsToBeIterated = emptyCellsInCurrentHouse & ~corner;
			if (availableCellsToBeIterated.Count - 1 < size)
			{
				// No more cells or eliminations to exist.
				continue;
			}

			foreach (ref readonly var extraCells in emptyCellsInCurrentHouse & size)
			{
				var currentDigitsMask = grid[extraCells];
				if (currentDigitsMask != extraDigitsMask)
				{
					continue;
				}

				var elimMap = emptyCellsInCurrentHouse & ~extraCells & ~corner;
				var conclusions = new List<Conclusion>();
				foreach (var digit in currentDigitsMask)
				{
					foreach (var cell in elimMap & CandidatesMap[digit])
					{
						conclusions.Add(new(Elimination, cell, digit));
					}
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in corner)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								(extraDigitsMask >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var d in digitsMaskAppearedInCorner)
				{
					foreach (var cell in CandidatesMap[d] & crossline)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d));
					}
				}
				foreach (var cell in extraCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary3, cell * 9 + digit));
					}
				}

				var step = new QiuDeadlyPatternType3Step(
					conclusions.AsMemory(),
					[
						[
							.. candidateOffsets,
							new HouseViewNode(ColorIdentifier.Normal, l1),
							new HouseViewNode(ColorIdentifier.Normal, l2),
							new HouseViewNode(ColorIdentifier.Auxiliary1, cornerCellCoveredHouse),
							new CellViewNode(ColorIdentifier.Normal, corner[0]),
							new CellViewNode(ColorIdentifier.Normal, corner[1])
						]
					],
					context.Options,
					true,
					1 << l1 | 1 << l2,
					corner[0],
					corner[1],
					extraCells,
					extraDigitsMask,
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
	/// Check for base type (type 4).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="digitsMaskAppearedInCorner">A mask value that holds a list of digits appeared in corner cells.</param>
	/// <param name="cornerContainingExtraDigit">Indicates the cells which are corner cells and contain extra digit.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternType4Step? BaseType_Type4(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		House l1,
		House l2,
		Mask digitsMaskAppearedInCorner,
		in CellMap cornerContainingExtraDigit
	)
	{
		// Test examples:
		// 83+59+71+6+4+27.4+63+28+5..+2+6.5....4+5.+2+869+1...2.1.5.+8.+815+9...4....4......9.2.4+86+2+4+81+69+735:139 939 171 172 376
		// ..2...1..+15..3.+87.83+67.1.29..+8.6.....2.514.8.....8....98.6.3+251.6.+12..4.+2.1...6..:914 415 916 947 348 349 749 359 361 962 368 369 669 769 781 783 586 786 387 389 796 996 799

		foreach (var digit in digitsMaskAppearedInCorner)
		{
			foreach (var cornerCellCoveredHouse in (corner & CandidatesMap[digit]).SharedHouses)
			{
				if ((CandidatesMap[digit] & HousesMap[cornerCellCoveredHouse]) == corner)
				{
					var conclusions = new List<Conclusion>();
					foreach (var elimDigit in (Mask)(digitsMaskAppearedInCorner & ~(1 << digit)))
					{
						foreach (var cell in CandidatesMap[elimDigit] & corner)
						{
							conclusions.Add(new(Elimination, cell, elimDigit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var d in digitsMaskAppearedInCorner)
					{
						foreach (var cell in CandidatesMap[d] & crossline)
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + d));
						}
					}
					foreach (var cell in corner)
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}

					var step = new QiuDeadlyPatternType4Step(
						conclusions.AsMemory(),
						[
							[
								.. candidateOffsets,
								new HouseViewNode(ColorIdentifier.Normal, l1),
								new HouseViewNode(ColorIdentifier.Normal, l2),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, corner[0], corner[1], digit),
								new CellViewNode(ColorIdentifier.Normal, corner[0]),
								new CellViewNode(ColorIdentifier.Normal, corner[1])
							]
						],
						context.Options,
						true,
						1 << l1 | 1 << l2,
						corner[0],
						corner[1],
						new(corner, digit)
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Check for base type (locked type).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="cornerLockedDigitsMask">A mask value that holds digits locked in cross-line cells.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternLockedTypeStep? BaseType_TypeLocked(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in Grid grid,
		House l1,
		House l2,
		Mask cornerLockedDigitsMask
	)
	{
		// Test examples:
		// .....+2+185..591..23.12.+5+3.7.3+8.52+1.46.....93+1.+14..3..5...12........1.....6.83.5.+9+1:613 713 913 931 751 652 752 259 677 783 983 687
		// ...45....+6+58...+43...+46+8..+5.....7+6+9+4+5....+4178+646+7+8+9+5.+1......+4.2....9+3.5.4.+43..8...:224 129 171 871 172 872 173 177 677 179 779 979 191 991 194 294 195
		// .+7+6.84+932+9.27.+68+5+4+4.8.+29..16.........87+26.34+9+3.......68..6+9+31+2+7+7+63..24+9.29+147...+3:542 543 144 544 145 546 846 748 462 164 564 165 584

		var currentDigitsMask = grid[corner];
		if (BitOperations.PopCount(currentDigitsMask) > 4)
		{
			// Corner cells hold at least 5 digits, which is disallowed in the pattern.
			return null;
		}

		var digitsShouldBeLocked = (Mask)(cornerLockedDigitsMask & currentDigitsMask);
		var currentDigitsNotLocked = (Mask)(currentDigitsMask & ~digitsShouldBeLocked);
		if (!Mask.IsPow2(currentDigitsNotLocked))
		{
			return null;
		}

		var elimDigit = Mask.Log2(currentDigitsNotLocked);
		var elimMap = crossline & CandidatesMap[elimDigit];
		if (!elimMap)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var digit in digitsShouldBeLocked)
		{
			foreach (var cell in CandidatesMap[digit] & crossline)
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}
		foreach (var cell in corner)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(
					new(
						(digitsShouldBeLocked >> digit & 1) != 0 ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
						cell * 9 + digit
					)
				);
			}
		}

		var house = crossline.FirstSharedHouse;
		var lockedMap = CandidateMap.Empty;
		foreach (var digit in currentDigitsMask)
		{
			foreach (var cell in HousesMap[house] & CandidatesMap[digit])
			{
				lockedMap.Add(cell * 9 + digit);
			}
		}

		var step = new QiuDeadlyPatternLockedTypeStep(
			(from cell in elimMap select new Conclusion(Elimination, cell, elimDigit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new HouseViewNode(ColorIdentifier.Auxiliary1, house),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			lockedMap
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (external type 1).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="mirror">Indicates the mirror cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="externalDigitsMaskToBeChecked">A mask value that holds digits should be checked.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternExternalType1Step? BaseType_ExternalType1(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in CellMap mirror,
		in Grid grid,
		House l1,
		House l2,
		Mask externalDigitsMaskToBeChecked
	)
	{
		// Test examples:
		// ...45....+6+58...+43...+46+8..+5.....7+6+9+4+5....+4178+646+7+8+9+5.+1......+4.2....9+3.5.4.+43..8...:224 129 171 871 172 872 173 177 677 179 779 979 191 991 194 294 195

		if ((mirror & EmptyCells) is not [var targetCell])
		{
			return null;
		}

		// Check whether the block where cross-line cells is lying,
		// contains empty cells if it is not categorized as cross-line or the target cell (where eliminations are raised).
		// Counter-example:
		//   +67+13..9..+3.+85..+61.52.+1.6+38.8+6......+1.35+61.72.+1.......6+28.7..+163+71+3.+62....+56.+31+27.:425 426 443 943 245 446 948 956 459 463 963 864 964 265 965 466 866 966 467 476
		var cellsShouldNotBeEmpty = (HousesMap[Mask.Log2(crossline.BlockMask)] & ~crossline & ~mirror) - targetCell;
		if (cellsShouldNotBeEmpty & EmptyCells)
		{
			return null;
		}

		var elimDigits = (Mask)(grid.GetCandidates(targetCell) & ~externalDigitsMaskToBeChecked);
		if (elimDigits == 0)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in crossline & EmptyCells)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & externalDigitsMaskToBeChecked))
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}
		foreach (var cell in corner)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
			}
		}

		var step = new QiuDeadlyPatternExternalType1Step(
			(from digit in elimDigits select new Conclusion(Elimination, targetCell, digit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			targetCell,
			externalDigitsMaskToBeChecked
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}

	/// <summary>
	/// Check for base type (external type 2).
	/// </summary>
	/// <param name="context"><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/param[@name='context']"/></param>
	/// <param name="corner">Indicates the corner cells used in a pattern.</param>
	/// <param name="crossline">Indicates the cross-line cells used in a pattern.</param>
	/// <param name="mirror">Indicates the mirror cells used in a pattern.</param>
	/// <param name="grid">The grid used.</param>
	/// <param name="l1">The line 1.</param>
	/// <param name="l2">The line 2.</param>
	/// <param name="externalDigitsMaskToBeChecked">A mask value that holds digits should be checked.</param>
	/// <returns><inheritdoc cref="StepSearcher.Collect(ref StepAnalysisContext)" path="/returns"/></returns>
	private QiuDeadlyPatternExternalType2Step? BaseType_ExternalType2(
		ref StepAnalysisContext context,
		in CellMap corner,
		in CellMap crossline,
		in CellMap mirror,
		in Grid grid,
		House l1,
		House l2,
		Mask externalDigitsMaskToBeChecked
	)
	{
		// Test examples:
		// ...45....+6+58...+43...+46+8..+5.....7+6+9+4+5....+4178+646+7+8+9+5.+1......+4.2....9+3.5.4.+43..8...:224 129 171 871 172 872 173 177 677 179 779 979 191 991 194 294 195

		var elimDigits = (Mask)(grid[mirror] & externalDigitsMaskToBeChecked);
		if (!Mask.IsPow2(elimDigits))
		{
			return null;
		}

		var elimDigit = Mask.Log2(elimDigits);
		var range = HousesMap[BitOperations.TrailingZeroCount(mirror.BlockMask)] & EmptyCells & ~crossline;
		var truth = range & CandidatesMap[elimDigit];
		var elimMap = range % CandidatesMap[elimDigit];
		if (!elimMap)
		{
			return null;
		}

		// Check whether the block where cross-line cells is lying,
		// contains empty cells if it is not categorized as cross-line or the target cell (where eliminations are raised).
		var cellsShouldNotBeEmpty = HousesMap[Mask.Log2(crossline.BlockMask)] & ~crossline & ~mirror & ~truth;
		if (cellsShouldNotBeEmpty & EmptyCells)
		{
			return null;
		}

		// Check whether other digits cannot be eliminated are locked in the cross-line cells.
		// Counter-example:
		//     .+5243+19+6..3+67...1519.....2.2.......694.......+5+6....74+93.9......6.+5.1.....8.5..69.:825 826 833 837 839 844 254 854 255 256 864 477 877 779 789 495 496 499 799
		var allOtherDigitsAreLockedInCrosslineCells = true;
		var crosslineBlock = HousesMap[Mask.Log2(crossline.BlockMask)] & EmptyCells;
		foreach (var digit in (Mask)(externalDigitsMaskToBeChecked & ~(1 << elimDigit)))
		{
			if (crosslineBlock & CandidatesMap[digit] & ~crossline)
			{
				allOtherDigitsAreLockedInCrosslineCells = false;
				break;
			}
		}
		if (!allOtherDigitsAreLockedInCrosslineCells)
		{
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in crossline & EmptyCells)
		{
			foreach (var digit in (Mask)(grid.GetCandidates(cell) & externalDigitsMaskToBeChecked))
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, cell * 9 + digit));
			}
		}
		foreach (var cell in corner)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
			}
		}
		foreach (var cell in truth)
		{
			candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + elimDigit));
		}

		var step = new QiuDeadlyPatternExternalType2Step(
			(from cell in elimMap select new Conclusion(Elimination, cell, elimDigit)).ToArray(),
			[
				[
					.. candidateOffsets,
					new HouseViewNode(ColorIdentifier.Normal, l1),
					new HouseViewNode(ColorIdentifier.Normal, l2),
					new CellViewNode(ColorIdentifier.Normal, corner[0]),
					new CellViewNode(ColorIdentifier.Normal, corner[1])
				]
			],
			context.Options,
			true,
			1 << l1 | 1 << l2,
			corner[0],
			corner[1],
			mirror,
			elimDigit
		);
		if (context.OnlyFindOne)
		{
			return step;
		}

		context.Accumulator.Add(step);
		return null;
	}
}
