namespace Sudoku.Drawing;

/// <summary>
/// Represents an item that can be drawn by GDI+ graphics module or UI shape controls.
/// </summary>
/// <param name="identifier"><inheritdoc cref="Identifier" path="/summary"/></param>
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType, TypeDiscriminatorPropertyName = "$typeid")]
[JsonDerivedType(typeof(CellViewNode), 0)]
[JsonDerivedType(typeof(CandidateViewNode), 1)]
[JsonDerivedType(typeof(HouseViewNode), 2)]
[JsonDerivedType(typeof(ChuteViewNode), 3)]
[JsonDerivedType(typeof(BabaGroupViewNode), 4)]
[JsonDerivedType(typeof(ChainLinkViewNode), 5)]
[JsonDerivedType(typeof(CellLinkViewNode), 6)]
[JsonDerivedType(typeof(ConjugateLinkViewNode), 7)]
[JsonDerivedType(typeof(CircleViewNode), 10)]
[JsonDerivedType(typeof(CrossViewNode), 11)]
[JsonDerivedType(typeof(TriangleViewNode), 12)]
[JsonDerivedType(typeof(DiamondViewNode), 13)]
[JsonDerivedType(typeof(StarViewNode), 14)]
[JsonDerivedType(typeof(SquareViewNode), 15)]
[JsonDerivedType(typeof(HeartViewNode), 16)]
[JsonDerivedType(typeof(TruthSpaceViewNode), 20)]
[JsonDerivedType(typeof(LinkSpaceViewNode), 21)]
public closed class ViewNode(ColorDescriptor identifier) :
	ICloneable,
	IEquatable<ViewNode>,
	IEqualityOperators<ViewNode, ViewNode, bool>
{
	/// <summary>
	/// Indicates an instance providing with data for describing coloring.
	/// </summary>
	public ColorDescriptor Identifier { get; } = identifier;

	/// <summary>
	/// Indicates the inner identifier to distinct the different types that is derived from <see cref="ViewNode"/>.
	/// </summary>
	/// <seealso cref="ViewNode"/>
	protected string TypeIdentifier => GetType().Name;


	/// <inheritdoc/>
	public sealed override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ViewNode);

	/// <inheritdoc/>
	public virtual bool Equals([NotNullWhen(true)] ViewNode? other) => other is not null && Identifier == other.Identifier;

	/// <inheritdoc/>
	public abstract override int GetHashCode();

	/// <inheritdoc/>
	public abstract override string ToString();

	/// <inheritdoc cref="ICloneable.Clone"/>
	public abstract ViewNode Clone();

	/// <inheritdoc/>
	object ICloneable.Clone() => Clone();


	/// <inheritdoc/>
	public static bool operator ==(ViewNode? left, ViewNode? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(ViewNode? left, ViewNode? right) => !(left == right);
}
