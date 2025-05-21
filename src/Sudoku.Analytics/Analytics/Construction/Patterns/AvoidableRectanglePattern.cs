namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents an avoidable rectangle.
/// </summary>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="valuesMap"><inheritdoc cref="ValuesMap" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class AvoidableRectanglePattern(in CellMap cells, Mask digitsMask, in CellMap valuesMap) : Pattern
{
	/// <inheritdoc/>
	public override bool IsChainingCompatible => true;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.AvoidableRectangle;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	[HashCodeMember]
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Indicates the digits used.
	/// </summary>
	[HashCodeMember]
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the value cells.
	/// </summary>
	[HashCodeMember]
	public CellMap ValuesMap { get; } = valuesMap;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is AvoidableRectanglePattern comparer
		&& Cells == comparer.Cells && DigitsMask == comparer.DigitsMask && ValuesMap == comparer.ValuesMap;

	/// <inheritdoc/>
	public override AvoidableRectanglePattern Clone() => new(Cells, DigitsMask, ValuesMap);
}
