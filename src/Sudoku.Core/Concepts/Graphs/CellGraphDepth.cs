namespace Sudoku.Concepts.Graphs;

/// <summary>
/// Represents a node that describes for a graph node inside a <see cref="CellGraph"/>, with depth from a node.
/// </summary>
/// <param name="Depth">Indicates the depth.</param>
/// <param name="Cell">The cell.</param>
/// <seealso cref="CellGraph"/>
public readonly record struct CellGraphDepth(int Depth, Cell Cell) : IEqualityOperators<CellGraphDepth, CellGraphDepth, bool>
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(Depth)} = ");
		builder.Append(Depth);
		builder.Append($", {nameof(Cell)} = ");
		builder.Append($@"""{CoordinateConverter.InvariantCultureInstance.CellConverter(in Cell.AsCellMap())}""");
		return true;
	}
}
