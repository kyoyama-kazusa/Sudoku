namespace Sudoku.Analytics.StepSearchers.Uniqueness;

/// <summary>
/// Provides with an <b>Anonymous Deadly Pattern</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Anonymous Deadly Pattern Type 1</item>
/// <item>Anonymous Deadly Pattern Type 2</item>
/// <item>Anonymous Deadly Pattern Type 3</item>
/// <item>Anonymous Deadly Pattern Type 4</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_AnonymousDeadlyPatternStepSearcher",
	Technique.AnonymousDeadlyPatternType1, Technique.AnonymousDeadlyPatternType2,
	Technique.AnonymousDeadlyPatternType3, Technique.AnonymousDeadlyPatternType4,
	RuntimeFlags = StepSearcherRuntimeFlags.TimeComplexity,
	SupportedSudokuTypes = SudokuType.Standard,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class AnonymousDeadlyPatternStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates all eight-cell patterns.
	/// </summary>
	private static readonly ReadOnlyMemory<CellMap> EightCellPatterns;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static AnonymousDeadlyPatternStepSearcher()
	{
		// Construct patterns of 8 cells.
		// We can unify two patterns of 8 cells into one, by using extended rectangles.
		// Enumerate all possible XR patterns with 6 cells, and enumerate all cells.
		// For each cell, we will remove it and insert a unique rectangle pattern that covers the current cell.
		var eightCellPatterns = new HashSet<CellMap>();
		foreach (var pattern in ExtendedRectanglePattern.AllPatterns[..1620]) // [0..1620] -> XR size 6
		{
			var patternCells = pattern.PatternCells;
			foreach (var missingCell in patternCells)
			{
				// Determine which side has more cells (row or column).
				var lastPatternCells = patternCells - missingCell;
				var row = missingCell.ToHouse(HouseType.Row);
				var column = missingCell.ToHouse(HouseType.Column);
				var rowCells = patternCells & HousesMap[row];
				var columnCells = patternCells & HousesMap[column];
				var isRow = rowCells.Count > columnCells.Count;
				var targetHouse = pattern.IsFat ? isRow ? column : row : isRow ? row : column;
				var chute = default(Chute);
				foreach (ref readonly var c in Chutes)
				{
					if ((c.HousesMask >> targetHouse & 1) != 0)
					{
						chute = c;
						break;
					}
				}

				// Iterate the other two houses that exclude the current cell, and find all possible cells.
				// For each cell, we should treat it as the diagonal cell of UR pattern with the missing cell.
				// If two cells are RxCy and RzCw, all 4 UR cells are RxCy, RxCw, RzCy and RzCw.
				var (x, y) = (missingCell / 9, missingCell % 9);
				foreach (var house in chute.HousesMask & ~(1 << targetHouse))
				{
					foreach (var cell in HousesMap[house])
					{
						if ((missingCell.AsCellMap() + cell).FirstSharedHouse != 32)
						{
							continue;
						}

						// Add it into the whole pattern.
						var (z, w) = (cell / 9, cell % 9);
						var extraCells = cell.AsCellMap() + (x * 9 + w) + z * 9 + y;
						if (lastPatternCells & extraCells)
						{
							continue;
						}

						eightCellPatterns.Add(lastPatternCells | extraCells);
					}
				}
			}
		}
		EightCellPatterns = eightCellPatterns.ToArray();
	}


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (Collect_8Cells(ref context) is { } eightCellsStep)
		{
			return eightCellsStep;
		}
		if (Collect_8Cells_Rotating(ref context) is { } eightCells2Step)
		{
			return eightCells2Step;
		}
		if (Collect_9Cells(ref context) is { } nineCellsStep)
		{
			return nineCellsStep;
		}
		return null;
	}

	/// <remarks>
	/// There're 2 patterns:
	/// <code><![CDATA[
	/// (1) UL + XR, 3 digits
	/// 12 .  .  |  23 .  .  |  13 .  .
	/// .  12 .  |  23 .  .  |  13 .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 12 12 .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	///
	/// (2) XR + UR, 3 digits
	/// 12 23 .  |  13 .  .  |  .  .  .
	/// .  .  13 |  13 .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 12 23 13 |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ]]></code>
	/// </remarks>
	private AnonymousDeadlyPatternStep? Collect_8Cells(ref StepAnalysisContext context)
	{
		// Test examples:
		// Type 1
		// 31.+84.+6.+768.732.+1+5..7...+3...4+6+2+51..31..+36+4.+525..9+7+8+14+6+2.....5.....523.61....8.+239:913 932 947 848 772 972 473 873 973 674 481 981 887 792 493
		// +4+6.+2.+9+37.7+91+4+8+3256.+2+36..49.....32.+6+42.9+5+64..+76.+41.+85..+95+2.4..1+3347......+18+632..4.:847 987 889 989
		// +45+1+6..8+2.+6+8+21..+3..79+3+8+2456+12+3+549+8+7+1+68+7+6.....5+1+4+9+567+2+83.649+8.+172..+7..6.+5+8..8+7..+63.:356 195 596
		// +4+6+739158+2.9+5+26.+1.+4.+1+25...+6+9.83..9.1+6.+7+9.....3.+46.+32..79+5+4..+3628731.+2...+56+2+8+9....+1:735 445 551 854 455 456 856 457 484 486 795 796 497 498
		// ..9..+8+4+7+6..4.+69+531...+45.+9824.1.8+2.9+5..+2+9...4+8..85+4+62+1+7...+69+5+12..1+5+8+247+6.+2.61+738+5.:311 312 722 331 731 732 342 351 751 352 371 772
		// +4+168+35.9.+3+8..9.1.62+9.....3.9.+4.1+8..3+56+1374.2.8.+3+95...1.3+9....+851+52+78+3....4+85.93+1.:728 634 737 739 747 767 668 274 276 477 677 487
		// 9+534.+6+27...+8+7356+9+4+647.+9.+5....+96.482.7.....+4.9...3.915.+362+9+4.+7.+5.+7..239..8+915+6+7+34+2:252
		// +2+48..+7.....12.+47....+7...2.4.+5+9+67+8+3.+2.+8+65+3+2.+797+23+491+8+566+7+2+14+953+889+572+3..+13+1+48..+9+2+7:535 635 638 938
		//
		// Type 2
		// 4.+28.+7.+31+7.3..+2+8.4.68...+72+52.....+4.......61......9.37..+8.+3452+1+7+5+24+7+81..337+1..+9+548:542 648 948 552 958 562
		// 41.+62...+526+3+958+4+1+7.5..+416+2.+6+2+4.+9517..+8.4.+2.96.3..7+6.+4+2+172+5+6+4...+3+452+8+97618+9+6...+254:817 379 394
		// 75+9.8.6..2+3.7+9+651...+6+5+23....2.64+9...6..31+5...+9.3+8+72.5+687.+9..2+6..9..+67.85.+6...+8+791:418 419 432 437 439 839 453 459 859 483
		// +6+17+438..+9.2.....4+7.9.7..3....+9+3.5.+81..128..+93.+38.7.6.+4..659...21.....97+595...+7...:521 124 625 126 527 531 635 636 238 538 871 493 694 695 197
		//
		// Type 3
		// ..5..+73+6..3+78+6...26.9.3...7+89..54+1+7.+3+746..5+2.+5+1.7...4.2+6+3.4.785+9+4+8+5+762+3+1+751...6+9+4:214 126 234 136 956 865 869 969
		// ..+6+295+3+7+8..8.+3.+2+9+1...7+18654+629+54+718+3...9+8+2+5+4+6..4+163+927....2+18+3.3.+1+8+5.+7+62...+3+7.41.:421 422 571 572 972 591 592
		// 15+7+83+2+9+6+4+6+849..3+1+22+9+3+6+4158+7932..+6..+1..+6+2....+3..+5..+3629+3485+2.+1.6+5+61+3.42.+8+7+2+9+16+8+435:745 748 755 756 758 765
		// .9+6+7+234+5.7+2+5..+4+6+9+3...56+927.+9.16.832...2...+9..5..9.+2..62.73.+65.+9..82+9..+34..+9..1.6+2:131 831 332 432 351 451 452 752 455 555 755 159 859 462 465 767 492 495 895
		// ...258..+7...+39+1......7+46...4+3.+8+6..+21+2.8+1+3.6.+45+6+1429+8+73+9145+7328+6...+91+4+7+3+575+3+68+2+149:611 917 222 822 831 932 233 538
		// +7.31.6.4+8+8.+6......5.+1+3+8.6271.+2.45+8+39+4.+98..+2.53+8+59..4..+6.8+4.71.+22+17....8+4+9.421+8+7..:922 225 926 528 575 385 985 587
		// .....+6..+9+6....957.+9.27.3..632+5+9+1+8..7..6+24+795+3+79436+5.8.26.+5.+4..8+5.1+892..4...6.+1..+5:112 312 812 313 117 217 137 378 398
		// 2...+94+6.1+9..+21845..+4.+6.329+8...+4.+6319.9..+3..6+4634+1.+9....253.+1+9.+6+41986+5+7..3..9..+1.5:545 745 551 751 753 265 765 868 793 298
		//
		// Type 4
		// .+2+38.+5.4..4+5..123+8...3+4+2.579+5..+3+87.+4+43...7.8.2....4....12....6..6.......5.418+6...:611 915 617 119 725 831 133 633 937 157 957 559 659 163 663 167 967 169 569 969 771 974 975 377 577 977 379 781 883 784 984 785 985 187 387 587 987 288 289 389
		// ...+52318+4+324+1.+85..+1+85+6.+4.+23+41+238+7..5+8..+9+524.....+4162.+82..735...7..+2.+9.....98.+1..+2:612 352 753 658 758 659 362 762 968 673 677 178 682 383 683 687 388 398
		// +1+79+23+6+5+48....+47.+2+1.24.+513......6548+22+6.+489+7.5+54+8.......+8....1.63..6+1.8...1+6.9+8+2..:744 368 378 778 394
		// 6+147+5+3.+8.+39+7.+2.+1652+5+8+9+61+4+7+37..6..8..+8....9.....1.8..4..+7.2...3..2+6..579..8+3+1...+24:149 557 657 159 259 567 667 269 969 571 971 679
		// 7..6..3+41.46....9.2...4.+68...+74.3..8..4.6.1..5.+28+17....2..8...3.7....81.4.8..9..6:321 124 224 724 225 226 526 729 134 334 641 947 248 959 962 176 576 577 777 286 586 289 589 294 794 295 798
		// ..5..+73+6..3+78+6...26.9.3...7+89..54+1+7.+3+746..5+2.+5+1.7...4.2+6+3.4.785+9+4+8+5+762+3+1+751...6+9+4:214 126 234 136 956 865 869 969
		// .9+6+7+234+5.7+2+5..+4+6+9+3...56+927.+9.16.832...2...+9..5..9.+2..62.73.+65.+9..82+9..+34..+9..1.6+2:131 831 332 432 351 451 452 752 455 555 755 159 859 462 465 767 492 495 895
		// 3....+6..9.8+6.+3.7.15.24....+3.+2+3.....6...6235+9.+6+59.....+2..5....18..436...+52..5.1...:817 418 818 835 836 141 841 447 451 751 467 772 972 776 976 277 477 977 781 981 286 792 992 495 397 697 498 798
		// 65..23...9....+7...+3.4+1.9.8.+5....+2.712..+7863..7.3+54+18..+8...15..7..52........6..9..:222 227 627 528 529 637 539 642 942 943 952 272 472 273 474 677 378 678 182 382 482 682 385 487 488 489 689 192 492 193 298 398 498 299 399 499
		// 76+94+3+15+2+85+129+8+6..34..57+2+91+6........9...695...6+9.......+94+6+758+1+322..+1.98.5+8+51+2.3.97:342 343 445 447 448 352 353 457 458 363 763 465 467 468
		// .5.+8.1+3+727.1+2.+3+85+4+28+34+5+7+619+3.86+7...5...+53..+8+75.+7+1.8.+36.+7+5+3.+6.4+8.36+7+8.5.18.29.5+763:911 951 452 965

		ref readonly var grid = ref context.Grid;
		foreach (ref readonly var pattern in EightCellPatterns)
		{
			if ((EmptyCells & pattern) != pattern)
			{
				// The pattern may contain non-empty cells. Skip it.
				continue;
			}

			// Check the number of digits appeared in the pattern, and determine which type it would be.
			var digitsMask = grid[pattern];

			// The pattern may be very complex to be checked, so we should append an extra check here:
			// determine which digits can be used as a deadly pattern, and which are not.
			// Obviously, both patterns of size 8 should use 3 digits, and two of them must use >= 6 times,
			// and the last one must use >= 4 times.
			// We should check which digits can be used in pattern. If the number of found digits are not enough 3,
			// the pattern will also be ignored to be checked.
			var (greaterThan6Digits, greaterThan4Digits) = ((Mask)0, (Mask)0);
			foreach (var digit in digitsMask)
			{
				var m = (pattern & CandidatesMap[digit]).Count;
				if (m >= 6)
				{
					greaterThan6Digits |= (Mask)(1 << digit);
				}
				else if (m >= 4)
				{
					greaterThan4Digits |= (Mask)(1 << digit);
				}
			}
			if (BitOperations.PopCount(greaterThan6Digits) < 2
				|| (Mask)(greaterThan4Digits | greaterThan6Digits) is var possiblePatternDigitsMask
				&& BitOperations.PopCount(possiblePatternDigitsMask) < 3)
			{
				continue;
			}

			// Iterate on each combination.
			foreach (var combination in possiblePatternDigitsMask.AllSets.GetSubsets(3))
			{
				// Try to get all cells that holds extra digits.
				var currentCombinationDigitsMask = MaskOperations.Create(combination);
				var extraDigitsMask = (Mask)(digitsMask & ~currentCombinationDigitsMask);
				var extraCells = CellMap.Empty;
				foreach (var cell in pattern)
				{
					if ((grid.GetCandidates(cell) & extraDigitsMask) != 0)
					{
						extraCells.Add(cell);
					}
				}
				if (extraCells.Count >= 2 && extraCells.FirstSharedHouse == 32)
				{
					// All extra cells must share with a same house.
					continue;
				}

				if (!VerifyPattern(grid, pattern, extraDigitsMask, out var p))
				{
					// The pattern cannot be passed to be verified.
					continue;
				}

				switch (BitOperations.PopCount(extraDigitsMask))
				{
					case 0:
					{
						throw new PuzzleInvalidException(grid, typeof(AnonymousDeadlyPatternStep));
					}
					case 1:
					{
						var extraDigit = Mask.Log2(extraDigitsMask);
						if (CheckType1Or2(
							ref context, grid, pattern, currentCombinationDigitsMask, extraDigit, p,
							(pattern & CandidatesMap[extraDigit]).Count == 1
								? Technique.AnonymousDeadlyPatternType1
								: Technique.AnonymousDeadlyPatternType2
						) is { } type1Or2Step)
						{
							return type1Or2Step;
						}
						continue;
					}
					case 2:
					{
						if (CheckType3(
							ref context, grid, pattern, currentCombinationDigitsMask,
							extraDigitsMask, extraCells, p, Technique.AnonymousDeadlyPatternType3) is { } type3Step)
						{
							return type3Step;
						}
						goto default;
					}
					default:
					{
						if (CheckType4(
							ref context, grid, pattern, currentCombinationDigitsMask,
							extraDigitsMask, extraCells, p, Technique.AnonymousDeadlyPatternType4) is { } type4Step)
						{
							return type4Step;
						}
						break;
					}
				}
			}
		}
		return null;
	}

	/// <remarks>
	/// There's only 1 pattern:
	/// <code><![CDATA[
	/// 12 .  .  |  24 .  .  |  14 .  .
	/// 23 .  .  |  23 .  .  |  .  .  .
	/// 13 .  .  |  34 .  .  |  14 .  .
	/// ]]></code>
	/// </remarks>
	private AnonymousDeadlyPatternStep? Collect_8Cells_Rotating(ref StepAnalysisContext context)
	{
		// Test examples:
		// Type 1
		// 17..9..83389......6..38..7..931287..8.1673...72695431891...2.37.3....29.26..39.51
		// +6+4+7...+2+89+2+914+687+35+5382+7+9..+1+726+8+9...+3..+31.79+2+6.+1..+2..783..+7....+2.72...+3.4+16+49+3285+7:564 466 566 575 176 576 678 586
		// 1+9.7.56.+4+658...3+79.74+6.9....+8.3..1.....892..+6..6+5+17..+8...4+5+896.8.9...54...59.3..2:215 235 238 838 241 441 741 261 268 273 182 282 189
		// +65249+78+1+3+843+2157+69.+7.+8+63....9..4+8.2..+8..+2...12+653+7+1+9...+2..8.1+3..+16.+3....+73+81+5+2496:539 547 557 458 976 589 789
		// .....+73....3.59+4.8+189..4+75.8..9..6..4+9+5...2713..7..8...18+4.6+52.+6+32+571+9+84......1..:212 612 614 115 222 245 345 346 265 665 792 296
		// 72+8.+13+4.6349.+2.+1......+7.2+3....7.2+6+1+5....+5938.5..1..+7..+8..2+4196+323+68+9+7+541...+365+8..:534 634 636 451 152 453 962 463 866 191 792 793
		// +6.5..........+369+51....+54867....1+2.+7....3..2..27...+56.+3.5...+3+1.9..35.+97.+6+964+781...:212 312 412 214 215 418 823 444 944 851 952 853 953 455 458 858 963 468 868 274 278 288
		// ..3...9..9.+83.1..4...7+98....3...+2.1+9+8.61594..1.9+4+7+38.6.8.+93+6.4....217.....7...6..:411 511 212 412 512 518 519 222 132 232 632 433 533 238 538 139 239 573 179 581 588 389 491 591 292 492 592 598 898 399 599 899
		//
		// Type 2
		// ..9+62+7+8+54+546+1+3+897+228+7.5....+9+5+4+763+21+87.+82..6+9..+6+28+9..47.9.3..+7...2..+7.4...+7..8...9:171 176 576 678 181 381 586 191 391 593 196 596
		// 3..2+86+5.9+9.6+54+72.+35..+9+3+1.+67.3..2+4.5..+481+957+3..+5..6+3...1+6.4+78..58.53196.4+4+9.+6+5+2...:113 743 263 763 167 168 268 897 198
		// 9+8+5...+6.4+16+2....5.734...281..+1843.....8.7.5..+3+9+7625+4+1+8216...845.7....+16.8.......2:116 924 925 926 727 748 949 958 959 384 984 385 985 986 394 794 994 395 995 796 996 997
		// 2.....4.78+4361+7....7.+29+4+8..+4+3..2.+78.+7+2+8.....4.617+4+8...+6.+24..1.+83.+4...9.2.87..+2.+4.:512 513 318 518 918 538 539 944 357 358 568 584 591 394 594
		//
		// Type 3
		// ...+92743+5+7+925+34..+14+5+31+8+6..............12....637+5+861+2+49.3+4+6....856......+4...49....:841 745 346 347 747 748 387 396
		// +6153+7428+9+4........79.....549+56.2.8.3+3+7..5....1.4...5.7+86+9.+1.+43+52+47...1+68+5+3+1648..+2:124 824 126 926 834 154 954 656 158 958 865 965 966
		//
		// Type 4
		// ...+92743+5+7+925+34..+14+5+31+8+6..............12....637+5+861+2+49.3+4+6....856......+4...49....:841 745 346 347 747 748 387 396
		// +59+1..7.+8.78+435..1.+3+6+218+945+7.57..+1..8.38+5..1..9+1+6+8.4.+2...5.1.29..23..+58....+96+2....:367 471 479 481 785 491 397
		// .12..8+3+6.48+6.7+3...9+3.6.+ 2.+ 8..+2+4+3.6...67..+25+84+3..3+7.+ 421+6+34+82+6+7+1+59+2....1.38...+839...:515 924 539 541 945 582 583 487
		// 74.+ 2.....+9.+ 5+4...+26+62+8+5194+7+359.+ 3412...+8.69+2...2..7+5+8...85..+2..+3+4....359..+3....451.:818 119 819 153 453 157 759 662 167 668 673 773 182 683 783 693 793 895 799
		// 4.6..+29+87....9....7.+ 9.843.2.....36...4+7.2.15+3.+ 631..+2..+69+82....1124.6....+37+5...8+26:521 522 524 526 428 542 475 778 984 786 986 788

		ref readonly var grid = ref context.Grid;

		// Iterate on each pattern. Here the pattern cells is same as patterns used in Unique Matrix pattern.
		// We should remove one cell from the pattern, to make the pattern become a possible Rotating Deadly Pattern.
		foreach (ref readonly var cells in UniqueMatrixStepSearcher.Patterns)
		{
			// Iterate on each cell as missing cell.
			foreach (var missingCell in cells)
			{
				// Check whether all last 8 cells are empty cells.
				var pattern = cells - missingCell;
				if ((EmptyCells & pattern) != pattern)
				{
					// The pattern may contain non-empty cells. Skip it.
					continue;
				}

				// Check the number of digits appeared in the pattern, and determine which type it would be.
				var digitsMask = grid[pattern];

				// Adds an filter nearly same as anonymous deadly pattern, but with 4 different digits.
				var possiblePatternDigitsMask = (Mask)0;
				foreach (var digit in digitsMask)
				{
					if ((pattern & CandidatesMap[digit]).Count >= 4)
					{
						possiblePatternDigitsMask |= (Mask)(1 << digit);
					}
				}
				if (BitOperations.PopCount(possiblePatternDigitsMask) < 4)
				{
					continue;
				}

				// Iterate on each combination.
				foreach (var combination in possiblePatternDigitsMask.AllSets.GetSubsets(4))
				{
					// Try to get all cells that holds extra digits.
					var currentCombinationDigitsMask = MaskOperations.Create(combination);
					var extraDigitsMask = (Mask)(digitsMask & ~currentCombinationDigitsMask);
					var extraCells = CellMap.Empty;
					foreach (var cell in pattern)
					{
						if ((grid.GetCandidates(cell) & extraDigitsMask) != 0)
						{
							extraCells.Add(cell);
						}
					}
					if (extraCells.Count >= 2 && extraCells.FirstSharedHouse == 32)
					{
						// All extra cells must share with a same house.
						continue;
					}

					if (!VerifyPattern(grid, pattern, extraDigitsMask, out var p))
					{
						// The pattern cannot be passed to be verified.
						continue;
					}

					switch (BitOperations.PopCount(extraDigitsMask))
					{
						case 0:
						{
							throw new PuzzleInvalidException(grid, typeof(AnonymousDeadlyPatternStep));
						}
						case 1:
						{
							var extraDigit = Mask.Log2(extraDigitsMask);
							if (CheckType1Or2(
								ref context, grid, pattern, currentCombinationDigitsMask, extraDigit, p,
								(pattern & CandidatesMap[extraDigit]).Count == 1
									? Technique.RotatingDeadlyPatternType1
									: Technique.RotatingDeadlyPatternType2
							) is { } type1Or2Step)
							{
								return type1Or2Step;
							}
							continue;
						}
						case 2:
						{
							if (CheckType3(
								ref context, grid, pattern, currentCombinationDigitsMask,
								extraDigitsMask, extraCells, p, Technique.RotatingDeadlyPatternType3) is { } type3Step)
							{
								return type3Step;
							}
							goto default;
						}
						default:
						{
							if (CheckType4(
								ref context, grid, pattern, currentCombinationDigitsMask,
								extraDigitsMask, extraCells, p, Technique.RotatingDeadlyPatternType4) is { } type4Step)
							{
								return type4Step;
							}
							break;
						}
					}
				}
			}
		}
		return null;
	}

	/// <remarks>
	/// There're 3 patterns:
	/// <code><![CDATA[
	/// (1) 2URs + XR, 3 digits
	/// 12 .  .  |  12 .  .  |  .  .  .
	/// .  23 .  |  23 .  .  |  .  .  .
	/// .  .  13 |  13 .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 12 23 13 |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	///
	/// (2) 2XRs, 4 digits
	/// 12 24 .  |  14 .  .  |  .  .  .
	/// 13 .  34 |  14 .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ---------+-----------+---------
	/// 23 24 34 |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	///
	/// (3) BDP + UR, 3 digits
	/// 12 .  .  |  12 .  .  |  .  .  .
	/// 23 .  .  |  .  .  .  |  23 .  .
	/// .  13 .  |  12 .  .  |  23 .  .
	/// ---------+-----------+---------
	/// 13 13 .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// .  .  .  |  .  .  .  |  .  .  .
	/// ]]></code>
	/// </remarks>
	private AnonymousDeadlyPatternStep? Collect_9Cells(ref StepAnalysisContext context)
	{
		return null;
	}

	/// <summary>
	/// Check for type 1 and 2.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digitsMask">The digits used.</param>
	/// <param name="targetDigit">The target digit.</param>
	/// <param name="p">All candidates used.</param>
	/// <param name="technique">The technique.</param>
	/// <returns>The found step.</returns>
	private AnonymousDeadlyPatternStep? CheckType1Or2(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap pattern,
		Mask digitsMask,
		Digit targetDigit,
		in CandidateMap p,
		Technique technique
	)
	{
		// You have found a deadly pattern with 8 cells without any names! Now check eliminations.
		var extraCells = CandidatesMap[targetDigit] & pattern;
		var eliminations = extraCells is [var extraCell]
			?
			from digit in (Mask)(grid.GetCandidates(extraCell) & ~(1 << targetDigit))
			select new Conclusion(Elimination, extraCell, digit)
			: from cell in extraCells % CandidatesMap[targetDigit] select new Conclusion(Elimination, cell, targetDigit);
		if (eliminations.Length == 0)
		{
			return null;
		}

		if (extraCells.Count == 1)
		{
			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in pattern - extraCells[0])
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
				}
			}

			var step = new AnonymousDeadlyPatternType1Step(
				eliminations.ToArray(),
				[[.. candidateOffsets]],
				context.Options,
				p,
				extraCells[0],
				digitsMask,
				technique
			);
			if (context.OnlyFindOne)
			{
				return step;
			}
			context.Accumulator.Add(step);
		}
		else
		{
			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in pattern)
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					candidateOffsets.Add(
						new(
							digit == targetDigit ? ColorIdentifier.Auxiliary1 : ColorIdentifier.Normal,
							cell * 9 + digit
						)
					);
				}
			}

			var step = new AnonymousDeadlyPatternType2Step(
				eliminations.ToArray(),
				[[.. candidateOffsets]],
				context.Options,
				p,
				extraCells,
				targetDigit,
				technique
			);
			if (context.OnlyFindOne)
			{
				return step;
			}
			context.Accumulator.Add(step);
		}
		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digitsMask">The digits used.</param>
	/// <param name="extraDigitsMask">The extra digits.</param>
	/// <param name="extraCells">Indicates the extra cells used.</param>
	/// <param name="p">All candidates used.</param>
	/// <param name="technique">The technique.</param>
	/// <returns>The found step.</returns>
	private AnonymousDeadlyPatternType3Step? CheckType3(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap pattern,
		Mask digitsMask,
		Mask extraDigitsMask,
		in CellMap extraCells,
		in CandidateMap p,
		Technique technique
	)
	{
		// Iterate on each shared house, to find a subset.
		foreach (var house in extraCells.SharedHouses)
		{
			var availableCells = HousesMap[house] & EmptyCells & ~pattern;
			if (availableCells.Count <= 2)
			{
				continue;
			}

			// Iterate each combination.
			foreach (ref readonly var subsetCells in availableCells | availableCells.Count - 1)
			{
				var subsetDigitsMask = (Mask)(grid[subsetCells] | extraDigitsMask);
				if (BitOperations.PopCount(subsetDigitsMask) != subsetCells.Count + 1)
				{
					// The (n) digits should be inside (n - 1) cells.
					continue;
				}

				var eliminations = new List<Conclusion>();
				foreach (var cell in availableCells & ~subsetCells)
				{
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & subsetDigitsMask))
					{
						eliminations.Add(new(Elimination, cell, digit));
					}
				}
				if (eliminations.Count == 0)
				{
					// No eliminations found.
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in pattern)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								HousesMap[house].Contains(cell) && (subsetDigitsMask >> digit & 1) != 0
									? ColorIdentifier.Auxiliary1
									: ColorIdentifier.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in subsetCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
					}
				}

				var step = new AnonymousDeadlyPatternType3Step(
					eliminations.AsMemory(),
					[[.. candidateOffsets, new HouseViewNode(ColorIdentifier.Normal, house)]],
					context.Options,
					p,
					pattern,
					subsetCells,
					subsetDigitsMask,
					technique
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
	/// <param name="context">The context.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digitsMask">The digits used.</param>
	/// <param name="extraDigitsMask">The extra digits.</param>
	/// <param name="extraCells">Indicates the extra cells used.</param>
	/// <param name="p">All candidates used.</param>
	/// <param name="technique">The technique.</param>
	/// <returns>The found step.</returns>
	private AnonymousDeadlyPatternType4Step? CheckType4(
		ref StepAnalysisContext context,
		in Grid grid,
		in CellMap pattern,
		Mask digitsMask,
		Mask extraDigitsMask,
		in CellMap extraCells,
		in CandidateMap p,
		Technique technique
	)
	{
		if (extraCells.Count is 0 or 1)
		{
			return null;
		}

		var house = extraCells.FirstSharedHouse;

		// Check whether at least one digit hold a conjugate pair inside the extra cells.
		var (conclusions, conjugatePairDigitsMask) = (new List<Conclusion>(), (Mask)0);
		foreach (var digit in (Mask)(grid[extraCells] & digitsMask))
		{
			var conjugatePairCells = extraCells & CandidatesMap[digit];
			if (conjugatePairCells == (CandidatesMap[digit] & HousesMap[house]))
			{
				conjugatePairDigitsMask |= (Mask)(1 << digit);
			}
		}
		if (BitOperations.PopCount(conjugatePairDigitsMask) != extraCells.Count - 1)
		{
			// The number of conjugate pairs must be less than the number of extra cells of 1.
			return null;
		}

		foreach (var digit in (Mask)(digitsMask & ~conjugatePairDigitsMask))
		{
			foreach (var cell in extraCells & CandidatesMap[digit])
			{
				conclusions.Add(new(Elimination, cell, digit));
			}
		}
		if (conclusions.Count == 0)
		{
			// No conclusions found.
			return null;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		var conjugatePairs = new List<ConjugateLinkViewNode>();
		foreach (var cell in pattern & ~extraCells)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(ColorIdentifier.Normal, cell * 9 + digit));
			}
		}
		foreach (var digit in conjugatePairDigitsMask)
		{
			var map = extraCells & CandidatesMap[digit];
			foreach (var cell in map)
			{
				candidateOffsets.Add(new(ColorIdentifier.Auxiliary1, cell * 9 + digit));
			}

			conjugatePairs.Add(new(ColorIdentifier.Normal, map[0], map[1], digit));
		}

		var step = new AnonymousDeadlyPatternType4Step(
			conclusions.AsMemory(),
			[[.. candidateOffsets, .. conjugatePairs]],
			context.Options,
			p,
			house,
			conjugatePairDigitsMask,
			technique
		);
		if (context.OnlyFindOne)
		{
			return step;
		}
		context.Accumulator.Add(step);
		return null;
	}


	/// <summary>
	/// Try to verify whether a pattern is a deadly pattern without the specified digits from a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="pattern">The pattern.</param>
	/// <param name="digits">The digits.</param>
	/// <param name="c">Indicates all candidates used.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private static bool VerifyPattern(in Grid grid, in CellMap pattern, Mask digits, out CandidateMap c)
	{
		var emptyGrid = Grid.Empty;
		foreach (var cell in pattern)
		{
			emptyGrid.SetCandidates(cell, (Mask)(grid.GetCandidates(cell) & ~digits));
		}

		var flag = DeadlyPatternInferrer.TryInfer(emptyGrid, pattern, out var result);
		c = result.PatternCandidates;
		return flag && result.IsDeadlyPattern;
	}
}
