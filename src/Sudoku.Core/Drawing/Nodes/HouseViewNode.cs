namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a house.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="house"><inheritdoc cref="House" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
[method: JsonConstructor]
public sealed partial class HouseViewNode(ColorIdentifier identifier, House house) : BasicViewNode(identifier)
{
	/// <summary>
	/// Indicates the house highlighted.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public House House { get; } = house;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out ColorIdentifier identifier, out House house) => (identifier, house) = (Identifier, House);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is HouseViewNode comparer && House == comparer.House;

	/// <inheritdoc/>
	public override HouseViewNode Clone() => new(Identifier, House);
}
