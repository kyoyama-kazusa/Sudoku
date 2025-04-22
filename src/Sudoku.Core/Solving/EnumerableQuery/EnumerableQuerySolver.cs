namespace Sudoku.Solving.EnumerableQuery;

/// <summary>
/// Defines a solver that can solve a sudoku puzzle, using LINQ.
/// </summary>
public sealed class EnumerableQuerySolver : ISolver
{
	/// <summary>
	/// Indicates the characters for 1 to 9.
	/// </summary>
	private const string DigitCharacters = "123456789";


	/// <inheritdoc/>
	public string? UriLink => null;


	/// <inheritdoc/>
	public bool? Solve(in Grid grid, out Grid result)
	{
		(result, var @return) = SolveCore(grid.ToString()) switch
		{
			[] => (Grid.Undefined, default(bool?)),
			[var resultString] => (Grid.Parse(resultString), true),
			_ => (Grid.Undefined, false)
		};
		return @return;
	}


	/// <summary>
	/// The core method to solve puzzles.
	/// </summary>
	/// <param name="puzzle">Indicates the puzzles.</param>
	/// <returns>A list of puzzles found.</returns>
	private static ReadOnlySpan<string> SolveCore(string puzzle)
	{
		var result = (ReadOnlySpan<string>)(string[])[puzzle];
		while (result is [var r, ..] && r.Contains('.'))
		{
			result =
				from solution in result
				let index = solution.IndexOf('.')
				let column = index % 9
				let block = index - index % 27 + column - index % 3
				from digit in DigitCharacters.Span
				let duplicateCases =
					from pos in Digits
					let rowContainsDuplicateDigits = solution[index - column + pos] == digit
					let columnContainsDuplicateDigits = solution[column + pos * 9] == digit
					let blockContainsDuplicateDigits = solution[block + pos % 3 + pos / 3 * 9] == digit
					where rowContainsDuplicateDigits || columnContainsDuplicateDigits || blockContainsDuplicateDigits
					select pos
				where duplicateCases.Length == 0
				select $"{solution[..index]}{digit}{solution[(index + 1)..]}";
		}
		return result;
	}
}
