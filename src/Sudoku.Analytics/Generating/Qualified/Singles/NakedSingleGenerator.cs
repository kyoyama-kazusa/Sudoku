namespace Sudoku.Generating.Qualified;

/// <summary>
/// Represents a generator that supports generating for puzzles that can be solved by only using Naked Singles.
/// </summary>
/// <seealso cref="Technique.NakedSingle"/>
public sealed class NakedSingleGenerator : SingleGenerator
{
	/// <inheritdoc/>
	public override TechniqueSet SupportedTechniques => [Technique.NakedSingle];


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

			if (!puzzle.CanPrimaryNakedSingle())
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
		// Randomly select a cell as target cell.
		var targetCell = Alignment switch
		{
			ConclusionCellAlignment.NotLimited => Rng.NextCell(),
			ConclusionCellAlignment.CenterHouse => (PeersMap[40] + 40)[Rng.Next(PeersCount + 1)],
			ConclusionCellAlignment.CenterBlock => HousesMap[4][Rng.NextDigit()],
			_ => 40
		};
		var peerCells = PeersMap[targetCell].ToArray();

		// Generate extra digits.
		// Different with other techniques, naked single can be failed to be generated - it'll produce a puzzle that has no solution.
		// We should check for this. If so, we should generate puzzle again.
		ShuffleSequence(peerCells);
		ShuffleSequence(DigitSeed);

		// Update values and create a grid.
		var (puzzle, digitsMask, i) = (Grid.Empty, Grid.MaxCandidatesMask, 0);
		foreach (var cell in peerCells[..8])
		{
			var digit = DigitSeed[i++];
			puzzle.SetDigit(cell, digit);
			digitsMask &= (Mask)~(1 << digit);
		}

		var targetDigit = BitOperations.Log2(digitsMask);
		step = new NakedSingleStep(
			null!,
			null,
			null!,
			targetCell,
			targetDigit,
			TechniqueNaming.Single.GetNakedSingleSubtype(puzzle, targetCell),
			Lasting.GetLastingAllHouses(puzzle, targetCell, out var lastingHouse),
			lastingHouse.ToHouseType()
		);
		result = puzzle.FixedGrid;
		return true;
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
					foreach (var (currentGrid, s) in StepMarshal.Combine(grids, steps))
					{
						if (s is not NakedSingleStep { Cell: var cell, Digit: var digit })
						{
							continue;
						}

						var excluderCells = Excluder.GetNakedSingleExcluderCells(currentGrid, cell, digit, out _);
						var extractedGrid = currentGrid;
						extractedGrid.Unfix();

						for (var c = 0; c < 81; c++)
						{
							if (cell != c && !excluderCells.Contains(c))
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
