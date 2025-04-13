namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents an icon argument parser.
/// </summary>
internal sealed class IconArgumentParser : ArgumentParser
{
	/// <inheritdoc/>
	public override ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid, 
		ColorIdentifier colorIdentifier,
		CoordinateParser coordinateParser
	)
	{
		if (arguments is not [var iconKindString, .. var iconArgs])
		{
			throw new FormatException($"Incorrect syntax of icon view node '{string.Join(' ', arguments)}'.");
		}

		Func<Cell, IconViewNode> creator = iconKindString.ToLower() switch
		{
			"circle" => cell => new CircleViewNode(colorIdentifier, cell),
			"cross" => cell => new CrossViewNode(colorIdentifier, cell),
			"diamond" => cell => new DiamondViewNode(colorIdentifier, cell),
			"heart" => cell => new HeartViewNode(colorIdentifier, cell),
			"square" => cell => new SquareViewNode(colorIdentifier, cell),
			"star" => cell => new StarViewNode(colorIdentifier, cell),
			"triangle" => cell => new TriangleViewNode(colorIdentifier, cell),
			_ => throw new FormatException($"Invalid icon kind string: '{iconKindString}'.")
		};
		return from cell in new CellMap(iconArgs, coordinateParser) select creator(cell);
	}
}
