namespace Sudoku.Generating;

public partial class GeneratorHub
{
	private static partial bool TransformChecker_MissingDigit(in Grid grid, [NotNullWhen(true)] out object? result)
	{
		var digitsDistribution = new Dictionary<Digit, Cell>(9);
		foreach (var cell in grid.GivenCells)
		{
			var digit = grid.GetDigit(cell);
			if (!digitsDistribution.TryAdd(digit, 1))
			{
				digitsDistribution[digit]++;
			}
		}

		foreach (var digit in digitsDistribution.Keys)
		{
			if (digitsDistribution[digit] == 0)
			{
				result = digit;
				return true;
			}
		}

		result = null;
		return false;
	}

	private static partial void Transformer_MissingDigit(ref Grid grid, ConstraintCollection constraints, object? variable)
	{
		var digit = (int)variable!;
		var desiredDigit = constraints.OfTypeSingle<MissingDigitConstraint>().Digit;
		if (desiredDigit != digit)
		{
			grid.SwapDigit(digit, desiredDigit);
		}
	}
}
