namespace Sudoku.Generating.Qualified;

/// <summary>
/// Represents a generator type that can generate puzzles with complex single techniques.
/// </summary>
public abstract class ComplexSingleGenerator : TechniqueGenerator, IJustOneCellGenerator
{
	/// <summary>
	/// Indicates the backing analyzer.
	/// </summary>
	protected static readonly Analyzer Analyzer = Analyzer.Default
		.WithStepSearchers(
			new SingleStepSearcher { EnableFullHouse = true, HiddenSinglesInBlockFirst = true },
			new DirectIntersectionStepSearcher { AllowDirectClaiming = true, AllowDirectPointing = true },
			new DirectSubsetStepSearcher
			{
				AllowDirectHiddenSubset = true,
				AllowDirectLockedHiddenSubset = true,
				AllowDirectLockedSubset = true,
				AllowDirectNakedSubset = true,
				DirectHiddenSubsetMaxSize = 4,
				DirectNakedSubsetMaxSize = 4
			}
		)
		.WithUserDefinedOptions(new() { IsDirectMode = true });


	/// <summary>
	/// Indicates the creator instance that creates a list of cells indicating pattern interim cells.
	/// </summary>
	protected abstract InterimCellsCreator InterimCellsCreator { get; }

	/// <summary>
	/// Indicates the local step filter.
	/// </summary>
	protected abstract StepFilter StepFilter { get; }


	/// <inheritdoc/>
	public sealed override bool TryGenerateUnique(out Grid result, CancellationToken cancellationToken = default)
	{
		var generator = new Generator();
		while (true)
		{
			var puzzle = generator.Generate(cancellationToken: cancellationToken);
			if (puzzle.IsUndefined)
			{
				result = Grid.Undefined;
				return false;
			}

			switch (Analyzer.Analyze(puzzle, cancellationToken: cancellationToken))
			{
				case { FailedReason: FailedReason.UserCancelled }:
				{
					result = Grid.Undefined;
					return false;
				}
				case { IsSolved: true, StepsSpan: var steps } when steps.Any(StepFilter.Invoke):
				{
					result = puzzle;
					return true;
				}
				default:
				{
					cancellationToken.ThrowIfCancellationRequested();
					break;
				}
			}
		}
	}

	/// <inheritdoc/>
	public bool TryGenerateJustOneCell(out Grid result, CancellationToken cancellationToken = default)
		=> TryGenerateJustOneCell(out result, out _, out _, cancellationToken);

	/// <inheritdoc/>
	public bool TryGenerateJustOneCell(out Grid result, [NotNullWhen(true)] out Step? step, CancellationToken cancellationToken = default)
		=> TryGenerateJustOneCell(out result, out _, out step, cancellationToken);

	/// <inheritdoc/>
	public bool TryGenerateJustOneCell(out Grid result, out Grid phasedGrid, [NotNullWhen(true)] out Step? step, CancellationToken cancellationToken = default)
	{
		var generator = new Generator();
		while (true)
		{
			var puzzle = generator.Generate(cancellationToken: cancellationToken);
			switch (Analyzer.Analyze(puzzle, cancellationToken: cancellationToken))
			{
				case { FailedReason: FailedReason.UserCancelled }:
				{
					(result, phasedGrid, step) = (Grid.Undefined, Grid.Undefined, null);
					return false;
				}
				case { IsSolved: true, StepsSpan: var steps, GridsSpan: var grids }:
				{
					foreach (var (g, s) in Step.Combine(grids, steps))
					{
						if (StepFilter(s))
						{
							// Reserves the given cells that are used in the pattern.
							var reservedCells = InterimCellsCreator(g, s);
							var r = Grid.Empty;
							foreach (var cell in reservedCells)
							{
								r.SetDigit(cell, g.GetDigit(cell));
								r.SetState(cell, CellState.Given);
							}

							(result, phasedGrid, step) = (r, g, s);
							return true;
						}
					}
					goto default;
				}
				default:
				{
					cancellationToken.ThrowIfCancellationRequested();
					break;
				}
			}
		}
	}
}
