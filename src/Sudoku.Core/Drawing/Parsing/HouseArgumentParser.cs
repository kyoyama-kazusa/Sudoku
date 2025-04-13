namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents a house argument parser.
/// </summary>
internal sealed class HouseArgumentParser : ArgumentParser
{
	/// <inheritdoc/>
	public override ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid, 
		ColorIdentifier colorIdentifier,
		CoordinateParser coordinateParser
	)
	{
		var houses = arguments.Aggregate(0, (interim, next) => interim | coordinateParser.HouseParser(next));
		return from house in houses select new HouseViewNode(colorIdentifier, house);
	}
}
