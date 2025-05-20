namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a conjugate pair.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="start"><inheritdoc cref="Start" path="/summary"/></param>
/// <param name="end"><inheritdoc cref="End" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
[method: JsonConstructor]
public sealed partial class ConjugateLinkViewNode(ColorIdentifier identifier, Cell start, Cell end, Digit digit) :
	ViewNode(identifier),
	ILinkViewNode
{
	/// <summary>
	/// Indicates the start point.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public Cell Start { get; } = start;

	/// <summary>
	/// Indicates the end point.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public Cell End { get; } = end;

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	[HashCodeMember]
	[StringMember]
	public Digit Digit { get; } = digit;

	/// <summary>
	/// Indicates the target conjugate pair.
	/// </summary>
	public Conjugate ConjugatePair => new(Start, End, Digit);

	/// <inheritdoc/>
	object ILinkViewNode.Start => Start * 9 + Digit;

	/// <inheritdoc/>
	object ILinkViewNode.End => End * 9 + Digit;

	/// <inheritdoc/>
	LinkShape ILinkViewNode.Shape => LinkShape.ConjugatePair;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out ColorIdentifier identifier, out Cell start, out Cell end, out Digit digit)
		=> (identifier, start, end, digit) = (Identifier, Start, End, Digit);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other)
		&& other is ConjugateLinkViewNode comparer
		&& Start == comparer.Start
		&& End == comparer.End
		&& Digit == comparer.Digit;

	/// <inheritdoc/>
	public override ConjugateLinkViewNode Clone() => new(Identifier, Start, End, Digit);
}
