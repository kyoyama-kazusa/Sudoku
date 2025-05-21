namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Defines a temporary type that records a pair of data for a bi-value oddagon.
/// </summary>
/// <param name="loopCells"><inheritdoc cref="LoopCells" path="/summary"/></param>
/// <param name="extraCells"><inheritdoc cref="ExtraCells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class BivalueOddagonPattern(in CellMap loopCells, in CellMap extraCells, Mask digitsMask) : Pattern
{
	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.BivalueOddagon;

	/// <summary>
	/// Indicates the cells of the whole loop.
	/// </summary>
	[HashCodeMember]
	public CellMap LoopCells { get; } = loopCells;

	/// <summary>
	/// Indicates the extra cells.
	/// </summary>
	public CellMap ExtraCells { get; } = extraCells;

	/// <summary>
	/// Indicates the mask of digits that the loop used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out CellMap loopCells, out CellMap extraCells, out Mask digitsMask)
		=> (loopCells, extraCells, digitsMask) = (LoopCells, ExtraCells, DigitsMask);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is BivalueOddagonPattern comparer && LoopCells == comparer.LoopCells;

	/// <inheritdoc/>
	public override BivalueOddagonPattern Clone() => new(LoopCells, ExtraCells, DigitsMask);
}
