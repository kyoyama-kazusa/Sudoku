namespace Sudoku.Analytics.StepSearchers.Uniqueness;

public partial class UniqueRectangleStepSearcher
{
	/// <summary>
	/// Check UR + 3x/1SL and UR + 3X/1SL.
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
	/// <para>
	/// The pattern:
	/// <code><![CDATA[
	///   ↓ cornerCell
	///  (ab ) | abX
	///        |  |
	///        |  |a
	///        |  |
	///   abz  | abY  a/bz
	/// ]]></code>
	/// Suppose cell <c>abX</c> is filled with digit <c>b</c>, then a deadly pattern will be formed:
	/// <code><![CDATA[
	/// a | b
	/// b | a  z
	/// ]]></code>
	/// The pattern is called UR + 3x/1SL.
	/// </para>
	/// <para>
	/// The pattern can be extended with cell <c>a/bz</c> to a pair of cells <c>a/bS</c>,
	/// and cell <c>abz</c> extends to <c>abS</c>, which will become UR + 3X/1SL (where <c>S</c> is a subset of digits).
	/// </para>
	/// </remarks>
	private partial void Check3X1SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		// Test examples:
		// UR + 3x/1SL
		// 3+8.+75.+96+4......2+38.+49.+3871+5.....73.+68.+3.6...9..65.+3...+5273.+68.+19+34...+6..+16+8.7...3:142 244 548 552 156 268 288
		// .46.2.13....+1+4.....2.386.7...2.9+38+1+5.3..5..9...5.1.6+4+3.5.861.2.+2...+3...+1.14.7.35.:821 829 641 751 851 753 254 754 256 756 861 977 984 986 487 991 999
		// 6..1..2.9.+9.+6...5.....896+139...361.+5..65.49....529..+678.936.....7.+9....+62+63..7.+91:512 812 713 223 723 425 731 433 443 843 351 152 867 488
		// +51+628.+9.379.6....+5+48..95.+6.2....+95..+9+5..2.1.6+1.85...296.59.....+8.+9...6573.1.+56.9.:442 742 343 744 345 353 754 462 762 365 766 175 375 176 376
		// +2+863+975+1+4+97415623+81+5+3..+2+9+765+4.+6...+29+62.....8.+3+9..2..+6.7+6+2...+8+53+4+15+73+86+9+2+8+392651+4+7:156 456
		// 6.+7+9+8134...+4+7....9.+39+42.7.1.6..+57.+94+7...4+98..+495....+7+3...6+7+49+3.37.5+92+4+1.+94......7:521 522 227 528 628 144 844 253 258 259 667 579 193 893 394 297 898
		//
		// UR + 3X/1SL
		// ..7348+26+5+6+3+2...+7+4845+867+2+31+928..3+6.7..+7+6+814.23.+4+3+2.+7..+63+2.76....+8..+4+2+36.+7+7+64.8..32:543 147 967 173 177 578

		var cornerDigitsMask = grid.GetCandidates(cornerCell);
		if ((cornerDigitsMask & ~comparer) != 0)
		{
			// :( The corner cell can only contain the digits appeared in UR.
			return;
		}

		// Determine target cell, same-block cell (as corner) and the last cell.
		Unsafe.SkipInit(out int targetCell);
		Unsafe.SkipInit(out int sameBlockCell);
		var cells = urCells.AsCellMap();
		foreach (var cell in cells - cornerCell)
		{
			if (HousesMap[cornerCell.ToHouse(HouseType.Block)].Contains(cell))
			{
				sameBlockCell = cell;
			}
			else if (PeersMap[cornerCell].Contains(cell))
			{
				targetCell = cell;
			}
		}

		// Check pattern.
		// According to pattern, there should be a strong link of digit 'a' between 'targetCell' and 'lastCell',
		// and 'sameBlockCell' can only contain one extra digit,
		// and one peer intersection cell of 'targetCell' and 'sameBlockCell' should only contain that extra digit,
		// and only digits appeared in UR pattern.
		var mapOfDigit1And2 = CandidatesMap[d1] | CandidatesMap[d2];
		var lastCell = (cells - cornerCell - targetCell - sameBlockCell)[0];
		foreach (var (conjugatePairDigit, elimDigit) in ((d1, d2), (d2, d1)))
		{
			if ((grid.GetCandidates(targetCell) >> elimDigit & 1) == 0 || (grid.GetCandidates(sameBlockCell) >> elimDigit & 1) == 0)
			{
				// :( Both cells 'targetCell' and 'sameBlockCell' must contain the elimination digit.
				continue;
			}

			var pairMap = targetCell.AsCellMap() + lastCell;
			var conjugatePairHouse = pairMap.FirstSharedHouse;
			if (!IsConjugatePair(conjugatePairDigit, pairMap, conjugatePairHouse))
			{
				// :( Strong link of digit 'a' is required.
				continue;
			}

			// Check for cells in line of cell 'same-block', which doesn't include cell 'cornerCell'.
			// Then we should check for empty cells that doesn't overlap with UR pattern, to determine existence of subsets.
			var sameBlockHouses = 1 << sameBlockCell.ToHouse(HouseType.Row) | 1 << sameBlockCell.ToHouse(HouseType.Column);
			foreach (var house in sameBlockHouses)
			{
				if (HousesMap[house].Contains(cornerCell))
				{
					sameBlockHouses &= ~(1 << house);
					break;
				}
			}

			// Then iterate empty cells lying in the target house, to determine whether a subset can be formed.
			var subsetHouse = BitOperations.Log2(sameBlockHouses);
			var outsideCellsRange = HousesMap[subsetHouse] // Subset house that:
				& ~HousesMap[sameBlockCell.ToHouse(HouseType.Block)] // won't overlap the block with same-block cell
				& ~cells // and won't overlap with UR pattern
				& mapOfDigit1And2; // and must contain either digit 1 or digit 2
			foreach (ref readonly var outsideCells in outsideCellsRange | outsideCellsRange.Count)
			{
				var outsideCellDigitsMask = grid[outsideCells];
				var extraDigitsMaskInOutsideCell = (Mask)(outsideCellDigitsMask & ~comparer);
				if (extraDigitsMaskInOutsideCell == 0)
				{
					// :( The cell contains at least one extra digit.
					continue;
				}

				if (BitOperations.PopCount(extraDigitsMaskInOutsideCell) != outsideCells.Count)
				{
					// :( The size of the extra cell must be equal to the number of extra digits.
					continue;
				}

				var extraDigitsMaskInSameBlockCell = (Mask)(grid.GetCandidates(sameBlockCell) & ~comparer);
				if (extraDigitsMaskInSameBlockCell != extraDigitsMaskInOutsideCell)
				{
					// :( The cell 'sameBlockCell' must hold the exactly number of extra digits
					//    with the number of cells 'outsideCells'.
					continue;
				}

				var subsetCellsContainingElimDigit = outsideCells & CandidatesMap[elimDigit];
				if ((subsetCellsContainingElimDigit.SharedHouses >> conjugatePairHouse & 1) == 0)
				{
					// :( All cells in outside cells containing elimination digit
					//    should share same block with conjugate pair shared.
					continue;
				}

				// Now pattern is formed. Collect view nodes.
				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in cells | outsideCells)
				{
					foreach (var digit in comparer)
					{
						if ((grid.GetCandidates(cell) >> digit & 1) != 0 && (cell != targetCell || digit != elimDigit))
						{
							candidateOffsets.Add(
								new(
									(cell == targetCell || cell == lastCell) && digit == conjugatePairDigit
										? ColorIdentifier.Auxiliary1
										: ColorIdentifier.Normal,
									cell * 9 + digit
								)
							);
						}
					}
				}
				foreach (var outsideCell in outsideCells)
				{
					foreach (var extraDigitInOutsideCell in (Mask)(grid.GetCandidates(outsideCell) & extraDigitsMaskInOutsideCell))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, outsideCell * 9 + extraDigitInOutsideCell));
					}
				}
				if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out _))
				{
					continue;
				}

				accumulator.Add(
					new UniqueRectangleConjugatePairExtraStep(
						new SingletonArray<Conclusion>(new(Elimination, targetCell, elimDigit)),
						[
							[
								.. candidateOffsets,
								..
								from outsideCell in outsideCells
								select new CellViewNode(ColorIdentifier.Auxiliary2, outsideCell),
								..
								from extraDigitInOutsideCell in extraDigitsMaskInOutsideCell
								let extraCandidate = sameBlockCell * 9 + extraDigitInOutsideCell
								select new CandidateViewNode(ColorIdentifier.Auxiliary2, extraCandidate),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, pairMap[0], pairMap[1], conjugatePairDigit),
								new HouseViewNode(ColorIdentifier.Auxiliary2, subsetHouse)
							]
						],
						context.Options,
						outsideCells.Count == 1 ? Technique.UniqueRectangle3X1L : Technique.UniqueRectangle3X1U,
						d1,
						d2,
						cells,
						arMode,
						[new(pairMap, conjugatePairDigit)],
						outsideCells,
						extraDigitsMaskInOutsideCell,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 4x/1SL and UR + 4X/1SL.
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
	/// Case 1:
	/// cornerCell
	///     ↓       .--------------------.
	///   (abW)-----+-abX                |
	///           a |                    |
	///             |                    |
	///   ab(yz)    | ab(yz) b(yz) b(yz) |
	///             '--------------------'
	/// 
	/// Case 2:
	/// .--------------------.
	/// |   cornerCell→(abW)-+-----abX
	/// |                    | a
	/// |                    |
	/// | a(yz) a(yz) ab(yz) |    ab(yz)
	/// '--------------------'
	/// 
	/// Case 3:
	/// .--------------.   .--------------.
	/// |      cornerCell  |              |
	/// |          ↓   |   |              |
	/// |        (abW)-+---+-abX          |
	/// |              | a |              |
	/// |              |   |              |
	/// | a(yz) ab(yz) |   | ab(yz) b(yz) |
	/// '--------------'   '--------------'
	/// ]]></code>
	/// Then <c>b</c> can be removed from cell <c>abX</c>.
	/// </remarks>
	private partial void Check4X1SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell cornerCell, in CellMap otherCellsMap, int index)
	{
		// Test examples:
		// UR + 4x/1SL
		// 7.....4....2.......95.8...2....4..+27..61..845.....591.1+2+8+659+73+4....725.+1..741.+2..:351 355 382
		// 6+92+8+7+4+5+3+1+1+743.+5.8+983+5+91...4.1+82.+9.45..9.5+13.+8.....89+1...+1.97+8.3.8.5..+1....71+8.4.6:637 238 652 664 665 572 281 385 685 291 591
		// 6+92+8+7+4+5+3+1+1+743.+5.8+983+5+91...4.1+82.+9.45..9.5+13.+8.....89+1+2..+1.97+8.3.8.5..+1.+7..71+8.4.6:637 238 652 461 664 665 572 281 385 685 291 591
		// .8.492.6...3+71+68....+9+3+5+8+4.+183+72+65+914..+4+93+7+2+8+692+61+84+537....+4......2.7.6...9.623.4.:521 731 171 571 771 172 572 777 579 181 581
		//
		// UR + 4X/1SL
		// ..8.+53....7.62.8.+39..+48+7+51.+75+23+18+4+69+369+2+7+418+51+8+45+96+32+7.9..3...4..7.65.3....7+4.6..:217 918 289 591 196 299
		// 2.8.+7.+5+93+7.+593+28.+1+93.5....7.+5726..1....7.9..+5+69..514+7.5..+6.7.3.+3+76.95.+8.1.+9...7+56:412 435 436 638 452 852 453 257 277 279 494

		// Determine target cell, same-block cell and the last cell.
		var cells = urCells.AsCellMap();
		var sameBlockCell = (cells - cornerCell & HousesMap[cornerCell.ToHouse(HouseType.Block)])[0];
		Unsafe.SkipInit(out int targetCell);
		foreach (var cell in cells - cornerCell - sameBlockCell)
		{
			if ((cornerCell.AsCellMap() + cell).SharedLine != 32)
			{
				targetCell = cell;
				break;
			}
		}
		if (HousesMap[cornerCell.ToHouse(HouseType.Block)].Contains(targetCell))
		{
			// :( Cells 'cornerCell' and 'targetCell' shouldn't in a same block.
			return;
		}

		var mapOfDigit1And2 = CandidatesMap[d1] | CandidatesMap[d2];
		var lastCell = (cells - cornerCell - sameBlockCell - targetCell)[0];
		var pairMap = cornerCell.AsCellMap() + targetCell;
		foreach (var (conjugatePairDigit, elimDigit) in ((d1, d2), (d2, d1)))
		{
			if (!IsConjugatePair(conjugatePairDigit, pairMap, pairMap.SharedLine))
			{
				// :( There should be a conjugate pair between 'cornerCell' and 'targetCell'.
				continue;
			}

			if ((grid.GetCandidates(targetCell) >> elimDigit & 1) == 0 || (grid.GetCandidates(sameBlockCell) >> elimDigit & 1) == 0)
			{
				// :( Target cell and same-block cell must hold elimination digit.
				continue;
			}

			var cornerCellBlock = cornerCell.ToHouse(HouseType.Block);
			var targetCellBlock = targetCell.ToHouse(HouseType.Block);
			var line = (sameBlockCell.AsCellMap() + lastCell).SharedLine;
			var outsideCellsRange = HousesMap[line] & mapOfDigit1And2 & ~cells;
			foreach (ref readonly var outsideCells in outsideCellsRange | outsideCellsRange.Count)
			{
				if (outsideCells.Count == 1)
				{
					continue;
				}

				// Group them up, grouped them by block they are in.
				var cellsGroupedByBlock =
					from cell in outsideCells.ToArrayUnsafe().AsReadOnlySpan()
					group cell by cell.ToHouse(HouseType.Block) into cellsGroup
					let block = cellsGroup.Key
					select (Block: block, Cells: cellsGroup.AsSpan().AsCellMap());
				var ocCorner = from p in cellsGroupedByBlock where p.Block == cornerCellBlock select p.Cells;
				var ocTarget = from p in cellsGroupedByBlock where p.Block == targetCellBlock select p.Cells;
				ref readonly var outsideCellsSameCornerCell = ref ocCorner.Length != 0 ? ref ocCorner[0] : ref CellMap.Empty;
				ref readonly var outsideCellsSameTargetCell = ref ocTarget.Length != 0 ? ref ocTarget[0] : ref CellMap.Empty;
				var otherCells = outsideCells & ~outsideCellsSameCornerCell & ~outsideCellsSameTargetCell;
				var extraDigitsMask = (Mask)(grid[outsideCells + lastCell + sameBlockCell] & ~comparer);
				if (BitOperations.PopCount(extraDigitsMask) != outsideCells.Count)
				{
					// :( The number of extra digits appeared in subset cells should be equal to the number of subset cells.
					continue;
				}

				var outsideCellsContainingConjugatePairDigit = outsideCells & CandidatesMap[conjugatePairDigit];
				var outsideCellsContainingElimDigit = outsideCells & CandidatesMap[elimDigit];
				if ((outsideCellsContainingConjugatePairDigit.SharedHouses >> cornerCellBlock & 1) == 0
					|| (outsideCellsContainingElimDigit.SharedHouses >> targetCellBlock & 1) == 0)
				{
					// :( All cells in subset cells containing conjugate pair digit should be inside block same as corner cell,
					//    and all cells in subset cells containing elimination digit should be inside block same as target cell.
					continue;
				}

				var extraDigitsMaskInSameBlockCell = (Mask)(grid.GetCandidates(sameBlockCell) & ~comparer);
				var extraDigitsMaskInLastCell = (Mask)(grid.GetCandidates(lastCell) & ~comparer);
				if ((extraDigitsMask & extraDigitsMaskInSameBlockCell) != extraDigitsMaskInSameBlockCell
					|| (extraDigitsMask & extraDigitsMaskInLastCell) != extraDigitsMaskInLastCell)
				{
					// :( The extra digits appeared in same-block cell and last cell should be a subset of all subset digits.
					continue;
				}

				// Now pattern is formed. Collect view nodes.
				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in cells | outsideCells)
				{
					foreach (var digit in comparer)
					{
						if ((grid.GetCandidates(cell) >> digit & 1) != 0 && (cell != targetCell || digit != elimDigit))
						{
							candidateOffsets.Add(
								new(
									(cell == targetCell || cell == cornerCell) && digit == conjugatePairDigit
										? ColorIdentifier.Auxiliary1
										: ColorIdentifier.Normal,
									cell * 9 + digit
								)
							);
						}
					}
				}
				foreach (var outsideCell in outsideCells)
				{
					foreach (var extraDigitInOutsideCell in (Mask)(grid.GetCandidates(outsideCell) & extraDigitsMask))
					{
						candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, outsideCell * 9 + extraDigitInOutsideCell));
					}
				}
				if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out _))
				{
					continue;
				}

				accumulator.Add(
					new UniqueRectangleConjugatePairExtraStep(
						new SingletonArray<Conclusion>(new(Elimination, targetCell, elimDigit)),
						[
							[
								.. candidateOffsets,
								.. from outsideCell in outsideCells select new CellViewNode(ColorIdentifier.Auxiliary2, outsideCell),
								..
								from extraDigitInOutsideCell in (Mask)(grid.GetCandidates(sameBlockCell) & extraDigitsMask)
								let extraCandidate = sameBlockCell * 9 + extraDigitInOutsideCell
								select new CandidateViewNode(ColorIdentifier.Auxiliary2, extraCandidate),
								new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, pairMap[0], pairMap[1], conjugatePairDigit),
								new HouseViewNode(ColorIdentifier.Auxiliary2, outsideCells.SharedLine)
							]
						],
						context.Options,
						outsideCells.Count == 2 && !BitOperations.IsPow2(outsideCells.BlockMask)
							? Technique.UniqueRectangle4X1L
							: Technique.UniqueRectangle4X1U,
						d1,
						d2,
						cells,
						arMode,
						[new(pairMap, conjugatePairDigit)],
						outsideCells,
						extraDigitsMask,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 4x/2SL and UR + 4X/2SL.
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
	/// <para>
	/// The pattern:
	/// <code><![CDATA[
	/// corner1   corner2
	///    ↓    a    ↓
	///  (abZ)--|--(abX)
	///         |    |
	///         |    |a
	///         |    |
	///   abz   |   abY  a/bz
	/// ]]></code>
	/// Suppose cell <c>abX</c> is filled with digit <c>b</c>, then a deadly pattern will be formed:
	/// <code><![CDATA[
	/// a | b
	/// b | a  z
	/// ]]></code>
	/// The pattern is called UR + 4x/2SL.
	/// </para>
	/// <para>
	/// The pattern can be extended with cell <c>a/bz</c> to a pair of cells <c>a/bS</c>,
	/// and cell <c>abz</c> extends to <c>abS</c>, which will become UR + 4X/2SL (where <c>S</c> is a subset of digits).
	/// </para>
	/// </remarks>
	private partial void Check4X2SL(SortedSet<UniqueRectangleStep> accumulator, in Grid grid, ref StepAnalysisContext context, Cell[] urCells, bool arMode, Mask comparer, Digit d1, Digit d2, Cell corner1, Cell corner2, in CellMap otherCellsMap, int index)
	{
		// Test examples:
		// UR + 4x/2SL
		// .+23.+5.1675..2.......8.7.+29+5...684..2.8.5..6.............1.2+5+4792.4.6.+51..5..+4..+2+6:422 325 326 826 347 151 356 161 162 364 366 367 867
		// .93.5.218.42...+7+5.+5+1.+2..+46..2+6+5.3+1.7+1+3+9647+58+2.5...2+3.6+9+7+5...+82+4+284..+5+6+3+1+361.2.975:924 925 835 935 175
		// 7.5....+36..+3...89...+9..+3.+75+83+7+45+9+6+2+1+29+4+3+16+587+1+56+782+9+4+3548.3..6.3+6+2....1.+9+7+126+4+3+58:814 124 134 984 786
		// .+4.3.+6.+28.+2..4+83.+686+3..+19....48.2.6+5+61+85+79.3..5+26.48....6..+5.71..1.6+7+5.+34+7+5+1.3+6..:711 911 721 921 961 972
		// 6.+7+9+8134...+4+7....9.+39+42.7.1.6..+57.+94+7...4+98..+495....+7+3...6+7+49+3.37.5+92+4+1.+94......7:521 522 227 528 628 243 144 844 253 258 259 667 271 272 579 193 893 394 297 898
		// 
		// UR + 4X/2SL
		// 85+9+26...364..+93.+52..+25...6+9+2.843.+59.5..7.9+2...9..52...+9+8..+2...5..39.+5.2842+5+3..91.:153 458 663 167 467 867 468 669
		// +56....+7...+4.+7.59.+6.73+6...5..2+63+57...+7+5.+86...238...26+75+6152+7+8...8.+2.4.+567+4.7+5.+62+18:457 458
		// .+85+71+4..2..46395+1+89+1..+8.7+4..3..+4.+8.....+3.8..45+4+8..73..8+5.4...6.49....+28..61+89.+4..:233 641 144 246 546 149 649 251 751 653 655 957 969 373 276 779 586 396 399
		// .+9+5+4+6..2...4....+56.1+6..54+9..41+5+8+69..5.+8...+6.+46.+97+4..+8+51+62+3+78+54+9+45+7..98+6+39+8365+4...:116 717 719 326 127 367

		if ((corner1.AsCellMap() + corner2).IsInIntersection)
		{
			// :( Two corner cells shouldn't inside one same block.
			return;
		}

		foreach (var (targetCell, cornerCell) in ((corner1, corner2), (corner2, corner1)))
		{
			// Determine target cell, same-block cell (as corner) and the last cell.
			Unsafe.SkipInit(out int sameBlockCell);
			var cells = urCells.AsCellMap();
			foreach (var cell in cells - cornerCell)
			{
				if (HousesMap[cornerCell.ToHouse(HouseType.Block)].Contains(cell))
				{
					sameBlockCell = cell;
					break;
				}
			}

			var mapOfDigit1And2 = CandidatesMap[d1] | CandidatesMap[d2];
			var lastCell = (cells - cornerCell - targetCell - sameBlockCell)[0];
			var pairMap1 = targetCell.AsCellMap() + cornerCell;
			var pairMap2 = targetCell.AsCellMap() + lastCell;
			foreach (var (conjugatePairDigit, elimDigit) in ((d1, d2), (d2, d1)))
			{
				if ((grid.GetCandidates(targetCell) >> elimDigit & 1) == 0
					|| (grid.GetCandidates(sameBlockCell) >> elimDigit & 1) == 0)
				{
					// :( Both cells 'targetCell' and 'sameBlockCell' must contain the elimination digit.
					continue;
				}

				// Determine whether there're two conjugate pairs, with both connected with cell 'targetCell', of same digit.
				if (!IsConjugatePair(conjugatePairDigit, pairMap1, pairMap1.SharedLine)
					|| !IsConjugatePair(conjugatePairDigit, pairMap2, BitOperations.TrailingZeroCount(pairMap2.SharedHouses)))
				{
					continue;
				}

				// Check for cells in line of cell 'same-block', which doesn't include cell 'cornerCell'.
				// Then we should check for empty cells that doesn't overlap with UR pattern, to determine existence of subsets.
				var sameBlockHouses = 1 << sameBlockCell.ToHouse(HouseType.Row) | 1 << sameBlockCell.ToHouse(HouseType.Column);
				foreach (var house in sameBlockHouses)
				{
					if (HousesMap[house].Contains(cornerCell))
					{
						sameBlockHouses &= ~(1 << house);
						break;
					}
				}

				// Then iterate empty cells lying in the target house, to determine whether a subset can be formed.
				var conjugatePairHouse = BitOperations.TrailingZeroCount(pairMap2.SharedHouses);
				var subsetHouse = BitOperations.Log2(sameBlockHouses);
				var outsideCellsRange = HousesMap[subsetHouse] // Subset house that:
					& ~HousesMap[sameBlockCell.ToHouse(HouseType.Block)] // won't overlap the block with same-block cell
					& ~cells // and won't overlap with UR pattern
					& mapOfDigit1And2; // and must contain either digit 1 or digit 2
				foreach (ref readonly var outsideCells in outsideCellsRange | outsideCellsRange.Count)
				{
					var outsideCellDigitsMask = grid[outsideCells];
					var extraDigitsMaskInOutsideCell = (Mask)(outsideCellDigitsMask & ~comparer);
					if (extraDigitsMaskInOutsideCell == 0)
					{
						// :( The cell contains at least one extra digit.
						continue;
					}

					if (BitOperations.PopCount(extraDigitsMaskInOutsideCell) != outsideCells.Count)
					{
						// :( The size of the extra cell must be equal to the number of extra digits.
						continue;
					}

					var extraDigitsMaskInSameBlockCell = (Mask)(grid.GetCandidates(sameBlockCell) & ~comparer);
					if (extraDigitsMaskInSameBlockCell != extraDigitsMaskInOutsideCell)
					{
						// :( The cell 'sameBlockCell' must hold the exactly number of extra digits
						//    with the number of cells 'outsideCells'.
						continue;
					}

					var subsetCellsContainingElimDigit = outsideCells & CandidatesMap[elimDigit];
					if ((subsetCellsContainingElimDigit.SharedHouses >> conjugatePairHouse & 1) == 0)
					{
						// :( All cells in outside cells containing elimination digit
						//    should share same block with conjugate pair shared.
						continue;
					}

					// Now pattern is formed. Collect view nodes.
					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var cell in cells | outsideCells)
					{
						foreach (var digit in comparer)
						{
							if ((grid.GetCandidates(cell) >> digit & 1) != 0 && (cell != targetCell || digit != elimDigit))
							{
								candidateOffsets.Add(
									new(
										(cell == targetCell || cell == lastCell || cell == cornerCell) && digit == conjugatePairDigit
											? ColorIdentifier.Auxiliary1
											: ColorIdentifier.Normal,
										cell * 9 + digit
									)
								);
							}
						}
					}
					foreach (var outsideCell in outsideCells)
					{
						foreach (var extraDigitInOutsideCell in (Mask)(grid.GetCandidates(outsideCell) & extraDigitsMaskInOutsideCell))
						{
							candidateOffsets.Add(new(ColorIdentifier.Auxiliary2, outsideCell * 9 + extraDigitInOutsideCell));
						}
					}
					if (!IsIncompleteValid(arMode, AllowIncompleteUniqueRectangles, candidateOffsets, out _))
					{
						continue;
					}

					accumulator.Add(
						new UniqueRectangleConjugatePairExtraStep(
							new SingletonArray<Conclusion>(new(Elimination, targetCell, elimDigit)),
							[
								[
									.. candidateOffsets,
									..
									from outsideCell in outsideCells
									select new CellViewNode(ColorIdentifier.Auxiliary2, outsideCell),
									..
									from extraDigitInOutsideCell in extraDigitsMaskInOutsideCell
									let extraCandidate = sameBlockCell * 9 + extraDigitInOutsideCell
									select new CandidateViewNode(ColorIdentifier.Auxiliary2, extraCandidate),
									new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, pairMap1[0], pairMap1[1], conjugatePairDigit),
									new ConjugateLinkViewNode(ColorIdentifier.Auxiliary1, pairMap2[0], pairMap2[1], conjugatePairDigit),
									new HouseViewNode(ColorIdentifier.Auxiliary2, subsetHouse)
								]
							],
							context.Options,
							outsideCells.Count == 1 ? Technique.UniqueRectangle4X2L : Technique.UniqueRectangle4X2U,
							d1,
							d2,
							cells,
							arMode,
							[new(pairMap1, conjugatePairDigit), new(pairMap2, conjugatePairDigit)],
							outsideCells,
							extraDigitsMaskInOutsideCell,
							index
						)
					);
				}
			}
		}
	}
}
