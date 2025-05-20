namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Represents a supersymmetric space. This type can also be used as representation for truth or link concept
/// defined in another project called <see href="https://sudoku.allanbarker.com/index.html">XSudo</see>.
/// </summary>
/// <param name="mask"><inheritdoc cref="_mask" path="/summary"/></param>
[TypeImpl(TypeImplFlags.AllObjectMethods | TypeImplFlags.Equatable | TypeImplFlags.EqualityOperators)]
public readonly partial struct Space(Mask mask) : IEquatable<Space>, IEqualityOperators<Space, Space, bool>
{
	/// <summary>
	/// Indicates the backing mask.
	/// </summary>
	[HashCodeMember]
	[EquatableMember]
	private readonly Mask _mask = mask;


	/// <summary>
	/// Indicates whether the space is house-related.
	/// </summary>
	public bool IsHouseRelated => Type is SpaceType.RowNumber or SpaceType.ColumnNumber or SpaceType.BlockNumber;

	/// <summary>
	/// Indicates whether the space is cell-related.
	/// </summary>
	public bool IsCellRelated => Type == SpaceType.RowColumn;

	/// <summary>
	/// Indicates the space type.
	/// </summary>
	public SpaceType Type => (SpaceType)(_mask >> 8 & 3);

	/// <summary>
	/// Indicates the row value,
	/// or -1 if <see cref="Type"/> is not <see cref="SpaceType.RowColumn"/> or <see cref="SpaceType.RowNumber"/>.
	/// </summary>
	public RowIndex Row => Type switch { SpaceType.RowColumn => Secondary, SpaceType.RowNumber => Primary, _ => -1 };

	/// <summary>
	/// Indicates the column value,
	/// or -1 if <see cref="Type"/> is not <see cref="SpaceType.RowColumn"/> or <see cref="SpaceType.ColumnNumber"/>.
	/// </summary>
	public ColumnIndex Column => Type switch { SpaceType.RowColumn => Primary, SpaceType.ColumnNumber => Primary, _ => -1 };

	/// <summary>
	/// Indicates the block value, or -1 if <see cref="Type"/> is not <see cref="SpaceType.BlockNumber"/>.
	/// </summary>
	public BlockIndex Block => Type switch { SpaceType.BlockNumber => Primary, _ => -1 };

	/// <summary>
	/// Indicates the target cell, or -1 if <see cref="Type"/> is not <see cref="SpaceType.RowColumn"/>.
	/// </summary>
	public Cell Cell => Type switch { SpaceType.RowColumn => Row * 9 + Column, _ => -1 };

	/// <summary>
	/// Indicates the target digit, or -1 if <see cref="Type"/> is <see cref="SpaceType.RowColumn"/>.
	/// </summary>
	public Digit Digit => Type switch { not SpaceType.RowColumn => Secondary, _ => -1 };

	/// <summary>
	/// Indicates the target house, or -1 if <see cref="Type"/> is <see cref="SpaceType.RowColumn"/>.
	/// </summary>
	public House House
		=> Type switch
		{
			SpaceType.RowNumber => Row + 9,
			SpaceType.ColumnNumber => Column + 18,
			SpaceType.BlockNumber => Block,
			_ => -1
		};

	/// <summary>
	/// Indicates the represented letter.
	/// </summary>
	private char Letter
		=> Type switch
		{
			SpaceType.RowColumn => 'n',
			SpaceType.RowNumber => 'r',
			SpaceType.ColumnNumber => 'c',
			SpaceType.BlockNumber => 'b'
		};

	/// <summary>
	/// Indicates the primary value, written after letter.
	/// </summary>
	private Digit Primary => _mask & 15;

	/// <summary>
	/// Indicates the secondary value, written before letter.
	/// </summary>
	private Digit Secondary => _mask >> 4 & 15;

	[StringMember]
	private string FinalText => $"{Secondary + 1}{Letter}{Primary + 1}";


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out int primary, out int secondary) => (primary, secondary) = (Primary, Secondary);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out SpaceType type, out int primary, out int secondary) => (type, (primary, secondary)) = (Type, this);


	/// <summary>
	/// Creates a <see cref="Space"/> for row-number space.
	/// </summary>
	/// <param name="row">Indicates the row index.</param>
	/// <param name="digit">Indicates the number.</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Space RowNumber(RowIndex row, Digit digit)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(digit, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(digit, 0);
		return new((Mask)(row | digit << 4 | (int)SpaceType.RowNumber << 8));
	}

	/// <summary>
	/// Creates a <see cref="Space"/> for column-number space.
	/// </summary>
	/// <param name="column">Indicates the column index.</param>
	/// <param name="digit">Indicates the number.</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Space ColumnNumber(ColumnIndex column, Digit digit)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(digit, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(digit, 0);
		return new((Mask)(column | digit << 4 | (int)SpaceType.ColumnNumber << 8));
	}

	/// <summary>
	/// Creates a <see cref="Space"/> for block-number space.
	/// </summary>
	/// <param name="block">Indicates the block index.</param>
	/// <param name="digit">Indicates the number.</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Space BlockNumber(BlockIndex block, Digit digit)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(block, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(block, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(digit, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(digit, 0);
		return new((Mask)(block | digit << 4 | (int)SpaceType.BlockNumber << 8));
	}

	/// <summary>
	/// Creates a <see cref="Space"/> for row-column space.
	/// </summary>
	/// <param name="row">Indicates the row index.</param>
	/// <param name="column">Indicates the number.</param>
	/// <returns>The <see cref="Space"/> instance created.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument is greater than 9.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Space RowColumn(RowIndex row, ColumnIndex column)
	{
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(row, 0);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, 9);
		ArgumentOutOfRangeException.ThrowIfLessThan(column, 0);
		return new((Mask)(column | row << 4 | (int)SpaceType.RowColumn << 8));
	}
}
