namespace Sudoku.Analytics.StepSearchers.Intersections;

/// <summary>
/// Provides with an <b>Law of Leftover</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Law of leftover</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_LawOfLeftoverStepSearcher",
	Technique.LawOfLeftover,
	IsOrderingFixed = true,
	IsAvailabilityReadOnly = true,
	RuntimeFlags = StepSearcherRuntimeFlags.DirectTechniquesOnly | StepSearcherRuntimeFlags.SkipVerification)]
public sealed partial class LawOfLeftoverStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;
		foreach (var ((line, block), (a, b, c, _)) in Miniline.Map)
		{
			// Try to check for value cells from two different sets.
			var (lineSetDigitsMask, blockSetDigitsMask) = ((Mask)0, (Mask)0);
			var mergedCells = CellMap.Empty;
			foreach (var cell in a & ~EmptyCells)
			{
				lineSetDigitsMask |= (Mask)(1 << grid.GetDigit(cell));
				mergedCells.Add(cell);
			}
			foreach (var cell in b & ~EmptyCells)
			{
				blockSetDigitsMask |= (Mask)(1 << grid.GetDigit(cell));
				mergedCells.Add(cell);
			}

			// The LoL technique requires value digits from two different sets should be merged into a big set containing 6 digits.
			// Check whether merged mask contain 6 on bits.
			var mergedDigitsMask = (Mask)(lineSetDigitsMask | blockSetDigitsMask);
			if (BitOperations.PopCount(mergedDigitsMask) != 6 || mergedCells.Count != 6)
			{
				continue;
			}

			// Check whether both two sides don't hold all 6 digits.
			if ((a & ~EmptyCells).Count == 6 || (b & ~EmptyCells).Count == 6)
			{
				continue;
			}

			// A LoL is found. Now check for eliminations.
			var conclusions = new List<Conclusion>();
			foreach (var (houseCells, digitsMaskTheOtherSide) in ((a & EmptyCells, blockSetDigitsMask), (b & EmptyCells, lineSetDigitsMask)))
			{
				switch (houseCells)
				{
					case [var targetCell]:
					{
						// The cell can be filled with the digit from the other side.
						var disappearedDigit = Mask.TrailingZeroCount((Mask)(mergedDigitsMask & digitsMaskTheOtherSide));
						conclusions.Add(new(Assignment, targetCell, disappearedDigit));
						break;
					}
					default:
					{
						var disappearedDigitsMask = (Mask)(mergedDigitsMask & digitsMaskTheOtherSide);
						foreach (var cell in houseCells)
						{
							foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~disappearedDigitsMask))
							{
								conclusions.Add(new(Elimination, cell, digit));
							}
						}
						break;
					}
				}
			}
			if (conclusions.Count == 0)
			{
				continue;
			}

			var step = new LawOfLeftoverStep(
				conclusions.AsMemory(),
				[
					[
						.. from cell in a select new CircleViewNode(ColorIdentifier.Normal, cell),
						.. from cell in b select new TriangleViewNode(ColorIdentifier.Auxiliary2, cell),
						.. from cell in c select new DiamondViewNode(ColorIdentifier.Auxiliary3, cell)
					]
				],
				context.Options,
				line,
				block,
				a,
				b,
				mergedDigitsMask
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
