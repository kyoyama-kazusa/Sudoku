namespace Sudoku.Drawing;

/// <summary>
/// Defines a <see cref="ColorIdentifier"/> derived type that uses well-known kinds to distinct with colors.
/// </summary>
/// <param name="kind"><inheritdoc cref="Kind" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString, ToStringBehavior = ToStringBehavior.RecordLike)]
[method: JsonConstructor]
public sealed partial class WellKnownColorIdentifier(WellKnownColorIdentifierKind kind) :
	ColorIdentifier
{
	/// <summary>
	/// The well-known identifier kind to be assigned.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public WellKnownColorIdentifierKind Kind { get; } = kind;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ColorIdentifier? other)
		=> other is WellKnownColorIdentifier comparer && Kind == comparer.Kind;
}
