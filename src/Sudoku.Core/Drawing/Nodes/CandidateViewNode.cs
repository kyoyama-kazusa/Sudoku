namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a candidate.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="candidate"><inheritdoc cref="Candidate" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
[method: JsonConstructor]
public sealed partial class CandidateViewNode(ColorIdentifier identifier, Candidate candidate) : BasicViewNode(identifier)
{
	/// <summary>
	/// Indicates the candidate highlighted.
	/// </summary>
	[HashCodeMember]
	public Candidate Candidate { get; } = candidate;

	/// <summary>
	/// Indicates the target cell.
	/// </summary>
	public Cell Cell => Candidate / 9;

	/// <summary>
	/// Indicates the candidate string.
	/// </summary>
	[StringMember(nameof(Candidate))]
	private string CandidateString => CoordinateConverter.InvariantCultureInstance.CandidateConverter(Candidate.AsCandidateMap());


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out ColorIdentifier identifier, out Candidate candidate) => (identifier, candidate) = (Identifier, Candidate);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is CandidateViewNode comparer && Candidate == comparer.Candidate;

	/// <inheritdoc/>
	public override CandidateViewNode Clone() => new(Identifier, Candidate);
}
