namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Represents a rank set (truth or link) view node.
/// </summary>
/// <param name="identifier"><inheritdoc cref="ViewNode.Identifier" path="/summary"/></param>
/// <param name="space"><inheritdoc cref="Space" path="/summary"/></param>
public closed class RankSetViewNode(ColorDescriptor identifier, Space space) : ViewNode(identifier), ILinkViewNode
{
	/// <summary>
	/// Indicates whether the rank set is a truth.
	/// </summary>
	public abstract bool IsTruth { get; }

	/// <summary>
	/// Indicates the space.
	/// </summary>
	public Space Space { get; } = space;

	/// <inheritdoc/>
	object ILinkViewNode.Start => null!;

	/// <inheritdoc/>
	object ILinkViewNode.End => null!;

	/// <inheritdoc/>
	LinkShape ILinkViewNode.Shape => LinkShape.RankSet;


	/// <inheritdoc/>
	public sealed override int GetHashCode() => HashCode.Combine(Space, TypeIdentifier);

	/// <inheritdoc/>
	public sealed override string ToString()
		=> $$"""{{nameof(RankSetViewNode)}} { {{nameof(Space)}} = {{Space}}, {{nameof(Identifier)}} = {{Identifier}}, EqualityContract = {{TypeIdentifier}} }""";
}
