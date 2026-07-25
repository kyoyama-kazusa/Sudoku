namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines an icon view node that applies to a cell, indicating the icon of the cell. The icons can be used on some sudoku variants.
/// </summary>
/// <param name="identifier"><inheritdoc cref="ViewNode(ColorDescriptor)"/></param>
/// <param name="cell"><inheritdoc cref="Cell" path="/summary"/></param>
public closed class IconViewNode(ColorDescriptor identifier, Cell cell) : ViewNode(identifier)
{
	/// <summary>
	/// The cell.
	/// </summary>
	public Cell Cell { get; } = cell;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruct-method']/target[@name='method']"/>
	public void Deconstruct(out Cell cell) => cell = Cell;

	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is IconViewNode comparer && Cell == comparer.Cell;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Cell, TypeIdentifier);

	/// <inheritdoc/>
	public override string ToString()
		=> $"{nameof(IconViewNode)} {{ {nameof(Cell)} = {Cell}, {nameof(Identifier)} = {Identifier} }}";
}
