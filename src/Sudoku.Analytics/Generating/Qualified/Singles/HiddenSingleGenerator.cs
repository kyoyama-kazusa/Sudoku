namespace Sudoku.Generating.Qualified;

/// <summary>
/// Represents a generator that supports generating for puzzles that can be solved by only using Hidden Singles and Full Houses.
/// </summary>
/// <seealso cref="Technique.CrosshatchingBlock"/>
/// <seealso cref="Technique.CrosshatchingRow"/>
/// <seealso cref="Technique.CrosshatchingColumn"/>
public sealed class HiddenSingleGenerator : SingleGenerator
{
	/// <summary>
	/// Indicates whether the generator allows using block excluders in a Hidden Single in Row/Column.
	/// </summary>
	public bool AllowsBlockExcluders { get; set; }

	/// <inheritdoc/>
	public override TechniqueSet SupportedTechniques
		=> [Technique.CrosshatchingBlock, Technique.CrosshatchingRow, Technique.CrosshatchingColumn];


	/// <inheritdoc/>
	public override bool TryGenerateUnique(out Grid result, CancellationToken cancellationToken = default)
	{
		var emptyCellsCount = GetValidEmptyCellsCount() is var z and not -1 ? z : 82;
		var generator = new Generator();
		while (true)
		{
			var puzzle = generator.Generate(81 - emptyCellsCount, SymmetricType, cancellationToken: cancellationToken);
			if (puzzle.IsUndefined)
			{
				result = Grid.Undefined;
				return false;
			}

			if (!puzzle.CanPrimaryHiddenSingle(true))
			{
				goto NextLoop;
			}

			if (!AllowsBlockExcluders && Analyzer.Analyze(puzzle, cancellationToken: cancellationToken).HasBlockExcluders)
			{
				goto NextLoop;
			}

			result = puzzle;
			return true;

		NextLoop:
			if (cancellationToken.IsCancellationRequested)
			{
				result = Grid.Undefined;
				return false;
			}
		}
	}

	/// <inheritdoc/>
	public override bool GeneratePrimary(out Grid result, CancellationToken cancellationToken)
		=> TryGenerateUnique(out result, cancellationToken);

	/// <inheritdoc/>
	public override bool TryGenerateJustOneCell(out Grid result, [NotNullWhen(true)] out Step? step, CancellationToken cancellationToken = default)
	{
		var house = RandomlySelectHouse(Alignment);
		var subtype = RandomlySelectSubtype(house, s => AllowsBlockExcluders || s.GetExcludersCount(HouseType.Block) == 0);
		var a = forBlock;
		var b = forLine;
		result = (house < 9 ? a : b)(house, out step);
		return !result.IsUndefined;


		Grid forBlock(House house, out Step? step)
		{
			while (true)
			{
				// Placeholders may not necessary. Just check for excluders.
				var cellsInHouse = HousesMap[house];
				var (excluderRows, excluderColumns) = (0, 0);
				var (rows, columns) = (cellsInHouse.RowMask << 9, cellsInHouse.ColumnMask << 18);
				for (var i = 0; i < subtype.GetExcludersCount(HouseType.Row); i++)
				{
					House excluderHouse;
					do
					{
						excluderHouse = Rng.Next(9, 18);
					} while ((rows >> excluderHouse & 1) == 0);
					_ = (excluderRows |= 1 << excluderHouse, rows &= ~(1 << excluderHouse));
				}
				for (var i = 0; i < subtype.GetExcludersCount(HouseType.Column); i++)
				{
					House excluderHouse;
					do
					{
						excluderHouse = Rng.Next(18, 27);
					} while ((columns >> excluderHouse & 1) == 0);
					_ = (excluderColumns |= 1 << excluderHouse, columns &= ~(1 << excluderHouse));
				}
				var excluders = CellMap.Empty;
				foreach (var r in excluderRows)
				{
					var lastCellsAvailable = HousesMap[r] & ~cellsInHouse & ~excluders.ExpandedPeers;
					excluders.Add(lastCellsAvailable[Rng.Next(0, lastCellsAvailable.Count)]);
				}
				foreach (var c in excluderColumns)
				{
					var lastCellsAvailable = HousesMap[c] & ~cellsInHouse & ~excluders.ExpandedPeers;
					excluders.Add(lastCellsAvailable[Rng.Next(0, lastCellsAvailable.Count)]);
				}
				if (!excluders)
				{
					// Try again if no excluders found.
					continue;
				}

				// Now checks for uncovered cells in the target house, one of the uncovered cells is the target cell,
				// and the others should be placeholders.
				ShuffleSequence(DigitSeed);
				var targetDigit = DigitSeed[Rng.NextDigit()];
				var puzzle = Grid.Empty;
				var uncoveredCells = cellsInHouse & ~excluders.ExpandedPeers;
				var targetCell = uncoveredCells[Rng.Next(0, uncoveredCells.Count)];
				var tempIndex = 0;
				foreach (var placeholderCell in uncoveredCells - targetCell)
				{
					if (DigitSeed[tempIndex] == targetDigit)
					{
						tempIndex++;
					}

					puzzle.SetDigit(placeholderCell, DigitSeed[tempIndex]);
					puzzle.SetState(placeholderCell, CellState.Given);
					tempIndex++;
				}
				foreach (var excluder in excluders)
				{
					puzzle.SetDigit(excluder, targetDigit);
					puzzle.SetState(excluder, CellState.Given);
				}

				// We should adjust the givens and target cells to the specified position.
				switch (Alignment)
				{
					case ConclusionCellAlignment.CenterHouse or ConclusionCellAlignment.CenterBlock when house != 4:
					{
						adjustToBlock5(ref house, ref targetCell, ref puzzle);
						break;
					}
					case ConclusionCellAlignment.CenterCell when targetCell != 40:
					{
						adjustToBlock5(ref house, ref targetCell, ref puzzle);
						adjustToCenterCell(ref targetCell, ref puzzle);
						break;
					}
					default:
					{
						break;
					}
				}

				step = new HiddenSingleStep(
					null!,
					null,
					null!,
					targetCell,
					targetDigit,
					house,
					false,
					Lasting.GetLasting(puzzle, targetCell, house),
					subtype,
					null
				);
				return puzzle;
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void adjustToBlock5(ref House house, ref Cell targetCell, ref Grid puzzle)
			{
				if (house == 4)
				{
					return;
				}

				var (c1s, c1e, c2s, c2e) = house switch
				{
					0 => (0, 1, 3, 4),
					1 => (0, 1, -1, -1),
					2 => (0, 1, 4, 5),
					3 => (3, 4, -1, -1),
					5 => (4, 5, -1, -1),
					6 => (1, 2, 3, 4),
					7 => (1, 2, -1, -1),
					8 => (1, 2, 4, 5)
				};

				puzzle.SwapChute(c1s, c1e);
				if (c2s != -1)
				{
					puzzle.SwapChute(c2s, c2e);
				}

				house = 4;
				targetCell = HousesCells[4][BlockPositionOf(targetCell)];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void adjustToCenterCell(ref Cell targetCell, ref Grid puzzle)
			{
				var pos = BlockPositionOf(targetCell);
				if (targetCell.ToHouse(HouseType.Block) == 4 && pos == 4)
				{
					return;
				}

				var (c1s, c1e, c2s, c2e) = pos switch
				{
					0 => (12, 13, 21, 22),
					1 => (12, 13, -1, -1),
					2 => (12, 13, 22, 23),
					3 => (21, 22, -1, -1),
					5 => (22, 23, -1, -1),
					6 => (13, 14, 21, 22),
					7 => (13, 14, -1, -1),
					8 => (13, 14, 22, 23)
				};

				puzzle.SwapHouse(c1s, c1e);
				if (c2s != -1)
				{
					puzzle.SwapHouse(c2s, c2e);
				}

				targetCell = 40;
			}
		}

		Grid forLine(House house, out Step? step)
		{
			// Adjust if values are invalid.
			house = (subtype.RelatedTechnique, house) switch
			{
				(Technique.CrosshatchingRow, >= 18) => house - 9,
				(Technique.CrosshatchingColumn, < 18) => house + 9,
				_ => house
			};

			var cellsInHouse = HousesMap[house];
			while (true)
			{
				var (excluderBlocks, excluderLines) = (0, 0);
				var (blocks, lines) = ((HouseMask)cellsInHouse.BlockMask, house < 18 ? cellsInHouse.ColumnMask << 18 : cellsInHouse.RowMask << 9);
				for (var i = 0; i < subtype.GetExcludersCount(HouseType.Block); i++)
				{
					House excluderHouse;
					do
					{
						excluderHouse = Rng.NextDigit();
					} while ((blocks >> excluderHouse & 1) == 0);
					_ = (excluderBlocks |= 1 << excluderHouse, blocks &= ~(1 << excluderHouse));
				}
				for (var i = 0; i < subtype.GetExcludersCount(house < 18 ? HouseType.Column : HouseType.Row); i++)
				{
					House excluderHouse;
					do
					{
						excluderHouse = house < 18 ? Rng.Next(18, 27) : Rng.Next(9, 18);
					} while ((lines >> excluderHouse & 1) == 0);
					_ = (excluderLines |= 1 << excluderHouse, lines &= ~(1 << excluderHouse));
				}
				var excluders = CellMap.Empty;
				foreach (var r in excluderBlocks)
				{
					var lastCellsAvailable = HousesMap[r] & ~cellsInHouse & ~excluders.ExpandedPeers;
					excluders.Add(lastCellsAvailable[Rng.Next(0, lastCellsAvailable.Count)]);
				}
				foreach (var c in excluderLines)
				{
					var lastCellsAvailable = HousesMap[c] & ~cellsInHouse & ~excluders.ExpandedPeers;
					excluders.Add(lastCellsAvailable[Rng.Next(0, lastCellsAvailable.Count)]);
				}
				if (!excluders)
				{
					// Try again if no excluders found.
					continue;
				}

				// Now checks for uncovered cells in the target house, one of the uncovered cells is the target cell,
				// and the others should be placeholders.
				ShuffleSequence(DigitSeed);
				var targetDigit = DigitSeed[Rng.NextDigit()];
				var puzzle = Grid.Empty;
				var uncoveredCells = cellsInHouse & ~excluders.ExpandedPeers;
				var targetCell = uncoveredCells[Rng.Next(0, uncoveredCells.Count)];
				var tempIndex = 0;
				foreach (var placeholderCell in uncoveredCells - targetCell)
				{
					if (DigitSeed[tempIndex] == targetDigit)
					{
						tempIndex++;
					}

					puzzle.SetDigit(placeholderCell, DigitSeed[tempIndex]);
					puzzle.SetState(placeholderCell, CellState.Given);
					tempIndex++;
				}
				if (!AllowsBlockExcluders)
				{
					var emptyCellsRelatedBlocksContainAnyExcluder = false;
					foreach (var block in (HousesMap[house] & ~puzzle.GivenCells).BlockMask)
					{
						if (HousesMap[block] & excluders)
						{
							emptyCellsRelatedBlocksContainAnyExcluder = true;
							break;
						}
					}
					if (emptyCellsRelatedBlocksContainAnyExcluder)
					{
						// Invalid case. Try again.
						continue;
					}
				}
				foreach (var excluder in excluders)
				{
					puzzle.SetDigit(excluder, targetDigit);
					puzzle.SetState(excluder, CellState.Given);
				}

				// We should adjust the givens and target cells to the specified position.
				switch (Alignment)
				{
					case ConclusionCellAlignment.CenterHouse or ConclusionCellAlignment.CenterBlock when targetCell.ToHouse(HouseType.Block) is var b && b != 4:
					{
						adjustToBlock5(b, ref house, ref targetCell, ref puzzle);
						break;
					}
					case ConclusionCellAlignment.CenterCell when targetCell != 40:
					{
						adjustToBlock5(targetCell.ToHouse(HouseType.Block), ref house, ref targetCell, ref puzzle);
						adjustToCenterCell(ref house, ref targetCell, ref puzzle);
						break;
					}
					default:
					{
						break;
					}
				}

				step = new HiddenSingleStep(
					null!,
					null,
					null!,
					targetCell,
					targetDigit,
					house,
					false,
					Lasting.GetLasting(puzzle, targetCell, house),
					subtype,
					null
				);
				return puzzle;
			}


			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void adjustToBlock5(House cellHouse, ref House house, ref Cell targetCell, ref Grid puzzle)
			{
				if (cellHouse == 4)
				{
					return;
				}

				var (c1s, c1e, c2s, c2e) = cellHouse switch
				{
					0 => (0, 1, 3, 4),
					1 => (0, 1, -1, -1),
					2 => (0, 1, 4, 5),
					3 => (3, 4, -1, -1),
					5 => (4, 5, -1, -1),
					6 => (1, 2, 3, 4),
					7 => (1, 2, -1, -1),
					8 => (1, 2, 4, 5)
				};

				puzzle.SwapChute(c1s, c1e);
				if (c2s != -1)
				{
					puzzle.SwapChute(c2s, c2e);
				}

				var isRow = house < 18;
				var blockPos = BlockPositionOf(targetCell);
				house = blockPos switch
				{
					0 => house + 3,
					1 => isRow ? house + 3 : house,
					2 => isRow ? house + 3 : house - 3,
					3 => isRow ? house : house + 3,
					4 => house,
					5 => isRow ? house : house - 3,
					6 => isRow ? house - 3 : house + 3,
					7 => isRow ? house - 3 : house,
					8 => house - 3
				};
				targetCell = HousesCells[4][blockPos];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static void adjustToCenterCell(ref House house, ref Cell targetCell, ref Grid puzzle)
			{
				var pos = BlockPositionOf(targetCell);
				if (targetCell.ToHouse(HouseType.Block) == 4 && pos == 4)
				{
					return;
				}

				var (c1s, c1e, c2s, c2e) = pos switch
				{
					0 => (12, 13, 21, 22),
					1 => (12, 13, -1, -1),
					2 => (12, 13, 22, 23),
					3 => (21, 22, -1, -1),
					5 => (22, 23, -1, -1),
					6 => (13, 14, 21, 22),
					7 => (13, 14, -1, -1),
					8 => (13, 14, 22, 23)
				};

				puzzle.SwapHouse(c1s, c1e);
				if (c2s != -1)
				{
					puzzle.SwapHouse(c2s, c2e);
				}

				var isRow = house < 18;
				targetCell = 40;
				house = pos switch
				{
					0 => house + 1,
					1 => isRow ? house + 1 : house,
					2 => isRow ? house + 1 : house - 1,
					3 => isRow ? house : house + 1,
					4 => house,
					5 => isRow ? house : house - 1,
					6 => isRow ? house - 1 : house + 1,
					7 => isRow ? house - 1 : house,
					8 => house - 1
				};
			}
		}
	}

	/// <inheritdoc/>
	public override bool TryGenerateJustOneCell(out Grid result, out Grid phasedGrid, [NotNullWhen(true)] out Step? step, CancellationToken cancellationToken = default)
	{
		var generator = new Generator();
		while (true)
		{
			var puzzle = generator.Generate(cancellationToken: cancellationToken);
			if (puzzle.IsUndefined)
			{
				(result, phasedGrid, step) = (Grid.Undefined, Grid.Undefined, null);
				return false;
			}

			switch (Analyzer.Analyze(puzzle, cancellationToken: cancellationToken))
			{
				case { FailedReason: FailedReason.UserCancelled }:
				{
					(result, phasedGrid, step) = (Grid.Undefined, Grid.Undefined, null);
					return false;
				}
				case { IsSolved: true, GridsSpan: var grids, StepsSpan: var steps }:
				{
					foreach (var (currentGrid, s) in Step.Combine(grids, steps))
					{
						if (s is not HiddenSingleStep { Cell: var cell, Digit: var digit, House: var house })
						{
							continue;
						}

						if (ExcluderInfo.TryCreate(currentGrid, digit, house, cell.AsCellMap()) is not var (baseCells, _, _))
						{
							continue;
						}

						var extractedGrid = currentGrid;
						extractedGrid.Unfix();

						for (var c = 0; c < 81; c++)
						{
							if (!HousesMap[house].Contains(c) && !baseCells.Contains(c))
							{
								extractedGrid.SetDigit(c, -1);
							}
						}

						(result, phasedGrid, step) = (extractedGrid.FixedGrid, currentGrid, s);
						return true;
					}
					break;
				}
				default:
				{
					if (cancellationToken.IsCancellationRequested)
					{
						(result, phasedGrid, step) = (Grid.Undefined, Grid.Undefined, null);
						return false;
					}
					break;
				}
			}
		}
	}
}
