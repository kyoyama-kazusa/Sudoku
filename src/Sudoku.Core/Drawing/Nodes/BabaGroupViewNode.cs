namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a baba group.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="cell"><inheritdoc cref="Cell" path="/summary"/></param>
/// <param name="unknownValueChar"><inheritdoc cref="UnknownValueChar" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
[method: JsonConstructor]
public sealed partial class BabaGroupViewNode(ColorIdentifier identifier, Cell cell, char unknownValueChar, Mask digitsMask) :
	BasicViewNode(identifier)
{
	/// <summary>
	/// Initializes a <see cref="BabaGroupViewNode"/> instance via the specified values.
	/// </summary>
	/// <inheritdoc cref="BabaGroupViewNode(ColorIdentifier, Cell, char, Mask)"/>
	public BabaGroupViewNode(Cell cell, char unknownValueChar, Mask digitsMask) :
		this(ColorIdentifier.Normal, cell, unknownValueChar, digitsMask)
	{
	}


	/// <summary>
	/// Indicates the cell used.
	/// </summary>
	[HashCodeMember]
	public int Cell { get; } = cell;

	/// <summary>
	/// Indicates the character that represents the baba group name.
	/// </summary>
	[StringMember]
	public char UnknownValueChar { get; } = unknownValueChar;

	/// <summary>
	/// Indicates a mask that hold digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the cell string.
	/// </summary>
	[StringMember(nameof(Cell))]
	private string CellString => CoordinateConverter.InvariantCultureInstance.CellConverter(in Cell.AsCellMap());

	/// <summary>
	/// Indicates the digits mask string.
	/// </summary>
	[StringMember(nameof(DigitsMask))]
	private string DigitsMaskString => Convert.ToString(DigitsMask, 2).ToString();


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out ColorIdentifier identifier, out Cell cell, out char unknownValueChar)
		=> (identifier, cell, unknownValueChar) = (Identifier, Cell, UnknownValueChar);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out ColorIdentifier identifier, out Cell cell, out Mask digitsMask, out char unknownValueChar)
		=> ((identifier, cell, unknownValueChar), digitsMask) = (this, DigitsMask);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is BabaGroupViewNode comparer && Cell == comparer.Cell;

	/// <inheritdoc/>
	public override BabaGroupViewNode Clone() => new(Identifier, Cell, UnknownValueChar, DigitsMask);
}
