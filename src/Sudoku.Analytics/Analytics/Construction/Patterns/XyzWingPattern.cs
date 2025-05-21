namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents an XYZ-Wing pattern.
/// </summary>
/// <param name="pivot"><inheritdoc cref="Pivot" path="/summary"/></param>
/// <param name="leafCell1"><inheritdoc cref="LeafCell1" path="/summary"/></param>
/// <param name="leafCell2"><inheritdoc cref="LeafCell2" path="/summary"/></param>
/// <param name="house1"><inheritdoc cref="House1" path="/summary"/></param>
/// <param name="house2"><inheritdoc cref="House2" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="zDigit"><inheritdoc cref="ZDigit" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class XyzWingPattern(
	Cell pivot,
	Cell leafCell1,
	Cell leafCell2,
	House house1,
	House house2,
	Mask digitsMask,
	Digit zDigit
) :
	Pattern,
	IFormattable
{
	/// <inheritdoc/>
	public override bool IsChainingCompatible => true;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.XyzWing;

	/// <summary>
	/// Indicates the full pattern of cells.
	/// </summary>
	public CellMap Cells => Pivot.AsCellMap() + LeafCell1 + LeafCell2;

	/// <summary>
	/// Indicates the pivot cell.
	/// </summary>
	[HashCodeMember]
	public Cell Pivot { get; } = pivot;

	/// <summary>
	/// Indicates the leaf cell 1.
	/// </summary>
	[HashCodeMember]
	public Cell LeafCell1 { get; } = leafCell1;

	/// <summary>
	/// Indicates the leaf cell 2.
	/// </summary>
	[HashCodeMember]
	public Cell LeafCell2 { get; } = leafCell2;

	/// <summary>
	/// Indicates the house 1.
	/// </summary>
	[HashCodeMember]
	public House House1 { get; } = house1;

	/// <summary>
	/// Indicates the house 2.
	/// </summary>
	[HashCodeMember]
	public House House2 { get; } = house2;

	/// <summary>
	/// Indicates all digits.
	/// </summary>
	[HashCodeMember]
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the digit Z.
	/// </summary>
	public Digit ZDigit { get; } = zDigit;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out CellMap cells, out Mask digitsMask, out Digit zDigit)
		=> (cells, digitsMask, zDigit) = (Cells, DigitsMask, ZDigit);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Cell pivot, out Cell leafCell1, out Cell leafCell2, out House house1, out House house2, out Mask digitsMask, out Digit zDigit)
		=> (pivot, leafCell1, leafCell2, house1, house2, (_, digitsMask, zDigit)) = (Pivot, LeafCell1, LeafCell2, House1, House2, this);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is XyzWingPattern comparer && Cells == comparer.Cells && DigitsMask == comparer.DigitsMask
		&& House1 == comparer.House1 && House2 == comparer.House2;

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var zDigitStr = converter.DigitConverter((Mask)(1 << ZDigit));
		return $@"{converter.CellConverter(Pivot.AsCellMap() + LeafCell1 + LeafCell2)}({DigitsMask}, {zDigitStr})";
	}

	/// <inheritdoc/>
	public override XyzWingPattern Clone() => new(Pivot, LeafCell1, LeafCell2, House1, House2, DigitsMask, ZDigit);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);
}
