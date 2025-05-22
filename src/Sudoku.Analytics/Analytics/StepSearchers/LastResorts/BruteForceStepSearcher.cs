namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Brute Force</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Brute Force</item>
/// </list>
/// </summary>
[StepSearcher(
	"StepSearcherName_BruteForceStepSearcher",
	Technique.BruteForce,
	IsCachingSafe = true,
	IsOrderingFixed = true,
	SupportAnalyzingMultipleSolutionsPuzzle = false)]
public sealed partial class BruteForceStepSearcher : StepSearcher
{
	/// <summary>
	/// The order of cell offsets to get values.
	/// </summary>
	/// <remarks>
	/// For example, the first value is 40, which means the first cell to be tried to be filled
	/// is the 40th cell in the grid (i.e. the cell <c>r5c5</c>).
	/// </remarks>
	private static readonly Cell[] BruteForceTryAndErrorOrder;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static BruteForceStepSearcher()
	{
		var (table, currentRow, currentColumn, k) = ((List<int>)[40], 4, 4, 1);
		var directions = ((int, int)[])[(0, 1), (-1, 0), (0, -1), (1, 0)];
		while (table.Count < 81)
		{
			for (var i = 0; i < 4; i++)
			{
				var steps = i < 2 ? k : k + 1;
				var (dx, dy) = directions[i];
				for (var s = 0; s < steps; s++)
				{
					var newRow = currentRow + dx;
					var newColumn = currentColumn + dy;
					if (newRow < 0 || newRow >= 9 || newColumn < 0 || newColumn >= 9)
					{
						break;
					}

					currentRow = newRow;
					currentColumn = newColumn;
					table.Add(currentRow * 9 + currentColumn);

					if (table.Count == 81)
					{
						break;
					}
				}
				if (table.Count == 81)
				{
					break;
				}
			}
			k += 2;
		}

		BruteForceTryAndErrorOrder = [.. table];
	}


	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		if (Solution.IsUndefined)
		{
			goto ReturnNull;
		}

		ref readonly var grid = ref context.Grid;
		foreach (var offset in BruteForceTryAndErrorOrder)
		{
			if (grid.GetState(offset) == CellState.Empty)
			{
				var step = new BruteForceStep(
					new SingletonArray<Conclusion>(new(Assignment, offset * 9 + Solution.GetDigit(offset))),
					null,
					context.Options
				);
				if (context.OnlyFindOne)
				{
					return step;
				}

				context.Accumulator.Add(step);
			}
		}

	ReturnNull:
		return null;
	}
}
