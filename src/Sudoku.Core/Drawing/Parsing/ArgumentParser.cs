namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents a parser type that parses arguments for one kind of drawing item.
/// </summary>
internal abstract class ArgumentParser
{
	/// <summary>
	/// Try to parse the arguments and return a list of <see cref="ViewNode"/> instances indicating the result,
	/// assigned to <paramref name="result"/>.
	/// </summary>
	/// <param name="arguments">The raw arguments.</param>
	/// <param name="grid">The reference to the grid. The reference can be a <see langword="null"/> reference.</param>
	/// <param name="colorIdentifier">The color identifier.</param>
	/// <param name="coordinateParser">The coordinate parser.</param>
	/// <param name="result">A list of <see cref="ViewNode"/> instances.</param>
	/// <returns>
	/// A <see cref="bool"/> result indicating whether the operation is successfully handled, and arguments are valid.
	/// </returns>
	public bool TryParse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid,
		ColorIdentifier colorIdentifier,
		CoordinateParser coordinateParser,
		out ReadOnlySpan<ViewNode> result
	)
	{
		try
		{
			result = Parse(arguments, in grid, colorIdentifier, coordinateParser);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Parses the arguments and returns a list of <see cref="ViewNode"/> instances indicating the result.
	/// </summary>
	/// <param name="arguments">The raw arguments.</param>
	/// <param name="grid">The reference to the grid. The reference can be a <see langword="null"/> reference.</param>
	/// <param name="colorIdentifier">The color identifier.</param>
	/// <param name="coordinateParser">The coordinate parser.</param>
	/// <returns>A list of <see cref="ViewNode"/> instances.</returns>
	/// <exception cref="FormatException">Throws when the arguments are invalid.</exception>
	public abstract ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid,
		ColorIdentifier colorIdentifier,
		CoordinateParser coordinateParser
	);
}
