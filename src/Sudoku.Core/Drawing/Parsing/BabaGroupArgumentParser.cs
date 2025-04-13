namespace Sudoku.Drawing.Parsing;

/// <summary>
/// Represents a baba group argument parser.
/// </summary>
internal sealed class BabaGroupArgumentParser : ArgumentParser
{
	/// <inheritdoc/>
	public override ReadOnlySpan<ViewNode> Parse(
		ReadOnlySpan<string> arguments,
		[AllowNull] ref readonly Grid grid,
		ColorIdentifier colorIdentifier,
		CoordinateParser coordinateParser
	)
	{
		if (arguments is not [[var babaGroupingChar], .. var babaGroupingArgs])
		{
			throw new FormatException("Baba grouping character expected.");
		}

		return
			from cell in new CellMap(arguments, coordinateParser)
			select new BabaGroupViewNode(colorIdentifier, cell, babaGroupingChar, Grid.MaxCandidatesMask);
	}
}
