namespace Sudoku.Generating.Qualified;

/// <summary>
/// Represents a generator that supports generating for puzzles that can be solved by only using Direct Subsets.
/// </summary>
public sealed class DirectSubsetGenerator : ComplexSingleGenerator
{
	/// <inheritdoc/>
	public override TechniqueSet SupportedTechniques
		=> [
			Technique.ComplexFullHouse, Technique.ComplexCrosshatchingBlock, Technique.ComplexCrosshatchingRow,
			Technique.ComplexCrosshatchingColumn, Technique.ComplexNakedSingle
		];

	/// <inheritdoc/>
	protected override InterimCellsCreator InterimCellsCreator
		=> static (in g, s) =>
		{
			var step = (DirectSubsetStep)s;
			var result = CellMap.Empty;
			switch (step.SubsetTechnique)
			{
				case Technique.LockedPair or Technique.LockedTriple:
				case Technique.NakedPair or Technique.NakedTriple or Technique.NakedQuadruple:
				case Technique.NakedPairPlus or Technique.NakedTriplePlus or Technique.NakedQuadruplePlus:
				{
					// Find all excluders from all peer cells of each subset cell.
					foreach (var cell in step.SubsetCells)
					{
						var digitsToCover = (Mask)(Grid.MaxCandidatesMask & ~g.GetCandidates(cell));
						foreach (var peerCell in PeersMap[cell])
						{
							if ((digitsToCover >> g.GetDigit(peerCell) & 1) != 0)
							{
								result.Add(peerCell);
							}
						}
					}

					// Remove excluders that are not necessary.
					// An unnecessary excluder can be found if a digit has already excluded from subset house.
					foreach (var digit in (Mask)(Grid.MaxCandidatesMask & ~step.SubsetDigitsMask))
					{
						foreach (var cell in HousesMap[step.SubsetHouse])
						{
							if (g.GetDigit(cell) == digit)
							{
								// Remove unnecessary excluders.
								foreach (var c in result.ToArrayUnsafe())
								{
									if (g.GetDigit(c) == digit)
									{
										result.Remove(c);
									}
								}
								break;
							}
						}
					}
					break;
				}
			}

			switch (step.BasedOn)
			{
				case Technique.FullHouse or Technique.NakedSingle:
				{
					var targetCell = step.Cell;
					var targetDigit = step.Digit;
					for (var digit = 0; digit < 9; digit++)
					{
						if (digit == targetDigit)
						{
							continue;
						}

						// Check whether the digit has already marked according to the former steps.
						var cellsToCheck = PeersMap[targetCell] & result;
						var digitHasAlreadyMarked = false;
						foreach (var cell in cellsToCheck)
						{
							if (g.GetDigit(cell) == digit)
							{
								digitHasAlreadyMarked = true;
								break;
							}
						}
						if (digitHasAlreadyMarked)
						{
							continue;
						}

						foreach (var cell in PeersMap[targetCell])
						{
							if (g.GetState(cell) != CellState.Empty && g.GetDigit(cell) == digit)
							{
								result.Add(cell);
								break;
							}
						}
					}
					goto default;
				}
				case var basedOn and (Technique.CrosshatchingBlock or Technique.CrosshatchingRow or Technique.CrosshatchingColumn):
				{
					var targetHouse = step.Cell.ToHouse(
						basedOn switch
						{
							Technique.CrosshatchingBlock => HouseType.Block,
							Technique.CrosshatchingRow => HouseType.Row,
							_ => HouseType.Column
						}
					);
					foreach (var cell in HousesMap[targetHouse])
					{
						if (g.GetState(cell) != CellState.Empty)
						{
							result.Add(cell);
						}
					}
					goto default;
				}
				default:
				{
					foreach (var cell in HousesMap[step.SubsetHouse])
					{
						if (g.GetState(cell) != CellState.Empty)
						{
							result.Add(cell);
						}
					}
					foreach (var node in s.Views![0])
					{
						if (node is CircleViewNode { Cell: var cell } && g.GetState(cell) != CellState.Empty)
						{
							result.Add(cell);
						}
					}
					break;
				}
			}
			return result;
		};

	/// <inheritdoc/>
	protected override StepFilter StepFilter => static step => step is DirectSubsetStep;
}
