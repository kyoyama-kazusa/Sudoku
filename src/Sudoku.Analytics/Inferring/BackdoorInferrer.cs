namespace Sudoku.Inferring;

/// <summary>
/// Defines a backdoor searcher.
/// </summary>
public sealed class BackdoorInferrer : IInferrable<BackdoorInferredResult>
{
	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TryInfer(in Grid grid, out BackdoorInferredResult result)
	{
		if (grid.PuzzleType != SudokuType.Standard || grid.IsSolved || !grid.IsValid)
		{
			result = default;
			return false;
		}

		var sstsChecker = Analyzer.SstsOnly;
		result = new(
			sstsChecker.Analyze(grid).IsSolved && grid.SolutionGrid is var solution
				?
				from candidate in grid
				let digit = solution.GetDigit(candidate / 9)
				where digit != -1
				select new Conclusion(digit == candidate % 9 ? Assignment : Elimination, candidate)
				: g(grid)
		);
		return true;


		ReadOnlySpan<Conclusion> g(in Grid grid)
		{
			var (assignment, elimination, solution) = (new List<Conclusion>(81), new List<Conclusion>(729), grid.SolutionGrid);
			foreach (var cell in grid.EmptyCells)
			{
				// Case 1: Assignments.
				var case1Playground = grid;
				case1Playground.SetDigit(cell, solution.GetDigit(cell));

				if (sstsChecker.Analyze(case1Playground).IsSolved)
				{
					assignment.Add(new(Assignment, cell, solution.GetDigit(cell)));

					// Case 2: Eliminations.
					foreach (var digit in (Mask)(grid.GetCandidates(cell) & ~(1 << solution.GetDigit(cell))))
					{
						var case2Playground = grid;
						case2Playground.SetExistence(cell, digit, false);
						if (sstsChecker.Analyze(case2Playground).IsSolved)
						{
							elimination.Add(new(Elimination, cell, digit));
						}
					}
				}
			}
			return (Conclusion[])[.. assignment, .. elimination];
		}
	}
}
