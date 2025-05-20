namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a locked member. This type is only used by searching for exocets.
/// </summary>
/// <param name="lockedCells"><inheritdoc cref="LockedCells" path="/summary"/></param>
/// <param name="lockedBlock"><inheritdoc cref="LockedBlock" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.EqualityOperators)]
public sealed partial class LockedMember(in CellMap lockedCells, House lockedBlock) :
	IComponent,
	IEquatable<LockedMember>,
	IEqualityOperators<LockedMember, LockedMember, bool>
{
	/// <summary>
	/// Indicates the locked cells.
	/// </summary>
	[HashCodeMember]
	public CellMap LockedCells { get; } = lockedCells;

	/// <summary>
	/// Indicates the locked block.
	/// </summary>
	[HashCodeMember]
	public House LockedBlock { get; } = lockedBlock;

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.LockedMember;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out CellMap lockedCells, out House lockedBlock)
		=> (lockedCells, lockedBlock) = (LockedCells, LockedBlock);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] LockedMember? other)
		=> other is not null && LockedCells == other.LockedCells && LockedBlock == other.LockedBlock;
}
