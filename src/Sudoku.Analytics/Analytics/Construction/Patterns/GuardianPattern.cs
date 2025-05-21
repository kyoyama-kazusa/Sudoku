namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents for a data set that describes the complete information about a guardian technique.
/// </summary>
/// <param name="loopCells"><inheritdoc cref="LoopCells" path="/summary"/></param>
/// <param name="guardians"><inheritdoc cref="Guardians" path="/summary"/></param>
/// <param name="digit"><inheritdoc cref="Digit" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class GuardianPattern(in CellMap loopCells, in CellMap guardians, Digit digit) : Pattern
{
	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.Guardian;

	/// <summary>
	/// Indicates the cells used in this whole guardian loop.
	/// </summary>
	[HashCodeMember]
	public CellMap LoopCells { get; } = loopCells;

	/// <summary>
	/// Indicates the extra cells that is used as guardians.
	/// </summary>
	[HashCodeMember]
	public CellMap Guardians { get; } = guardians;

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	[HashCodeMember]
	public Digit Digit { get; } = digit;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out CellMap loopCells, out CellMap guardians, out Digit digit)
		=> (loopCells, guardians, digit) = (LoopCells, Guardians, Digit);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is GuardianPattern comaprer
		&& LoopCells == comaprer.LoopCells && Guardians == comaprer.Guardians && Digit == comaprer.Digit;

	/// <inheritdoc/>
	public override GuardianPattern Clone() => new(LoopCells, Guardians, Digit);
}
