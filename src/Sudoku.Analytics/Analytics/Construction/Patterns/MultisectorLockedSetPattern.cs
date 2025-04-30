namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents a pattern for multi-sector locked sets.
/// </summary>
/// <param name="map">The map of cells used.</param>
/// <param name="rowCount">The number of rows used.</param>
/// <param name="columnCount">The number of columns used.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class MultisectorLockedSetPattern(
	[Property, HashCodeMember] in CellMap map,
	[Property] RowIndex rowCount,
	[Property] ColumnIndex columnCount
) : Pattern
{
	/// <summary>
	/// Indicates the list initialized with the static constructor.
	/// </summary>
	internal static readonly MultisectorLockedSetPattern[] Patterns;

	/// <summary>
	/// Indicates the possible size (the number of rows and columns) in an MSLS.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly (RowIndex Rows, ColumnIndex Columns)[] PossibleSizes = [(3, 3), (3, 4), (4, 3), (4, 4), (4, 5), (5, 4)];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static MultisectorLockedSetPattern()
	{
		const HouseMask a = ~7, b = ~56, c = ~448;
		var result = new MultisectorLockedSetPattern[74601];
		var digitsSpan = Digits.AsReadOnlySpan();
		var i = 0;
		for (var l = 0; l < PossibleSizes.Length; l++)
		{
			var (rows, columns) = PossibleSizes[l];
			foreach (var rowList in digitsSpan.GetSubsets(rows))
			{
				var (rowMask, rowMap) = ((Mask)0, CellMap.Empty);
				foreach (var row in rowList)
				{
					rowMask |= (Mask)(1 << row);
					rowMap |= HousesMap[row + 9];
				}
				if ((rowMask & a) == 0 || (rowMask & b) == 0 || (rowMask & c) == 0)
				{
					continue;
				}

				foreach (var columnList in digitsSpan.GetSubsets(columns))
				{
					var (columnMask, columnMap) = ((Mask)0, CellMap.Empty);
					foreach (var column in columnList)
					{
						columnMask |= (Mask)(1 << column);
						columnMap |= HousesMap[column + 18];
					}
					if ((columnMask & a) == 0 || (columnMask & b) == 0 || (columnMask & c) == 0)
					{
						continue;
					}

					result[i++] = new(rowMap & columnMap, rowList.Length, columnList.Length);
				}
			}
		}

		Patterns = result;
	}


	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.MultisectorLockedSet;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out CellMap map, out RowIndex rowCount, out ColumnIndex columnCount)
		=> (map, rowCount, columnCount) = (Map, RowCount, ColumnCount);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is MultisectorLockedSetPattern comparer && Map == comparer.Map;

	/// <inheritdoc/>
	public override MultisectorLockedSetPattern Clone() => new(Map, RowCount, ColumnCount);
}
