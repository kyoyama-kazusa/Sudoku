namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents an extended rectangle pattern.
/// </summary>
/// <param name="isFat">Indicates whether the pattern is fat.</param>
/// <param name="patternCells">Indicates the cells used.</param>
/// <param name="pairCells">Indicates a list of pairs of cells used.</param>
/// <param name="size">Indicates the size of the pattern.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class ExtendedRectanglePattern(
	[Property] bool isFat,
	[Property, HashCodeMember] in CellMap patternCells,
	[Property] (Cell Left, Cell Right)[] pairCells,
	[Property] int size
) : Pattern
{
	/// <summary>
	/// Indicates all possible extended rectangle pattern combinations.
	/// </summary>
	/// <remarks>
	/// <para>The list contains two types of <b>Extended Rectangle</b>s:</para>
	/// <para>
	/// Fit type (2 blocks spanned):
	/// <code><![CDATA[
	/// ab | ab
	/// bc | bc
	/// ac | ac
	/// ]]></code>
	/// </para>
	/// <para>
	/// Fat type (3 blocks spanned):
	/// <code><![CDATA[
	/// ab | ac | bc
	/// ab | ac | bc
	/// ]]></code>
	/// </para>
	/// </remarks>
	public static readonly ReadOnlyMemory<ExtendedRectanglePattern> AllPatterns;

	/// <summary>
	/// Indicates all possible combinations of houses.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly House[][] HouseCombinations = [
		[9, 10], [9, 11], [10, 11], [12, 13], [12, 14], [13, 14],
		[15, 16], [15, 17], [16, 17], [18, 19], [18, 20], [19, 20],
		[21, 22], [21, 23], [22, 23], [24, 25], [24, 26], [25, 26]
	];

	/// <summary>
	/// Indicates the row combinations for rows.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly Cell[][] FitTableRow = [
		[0, 3], [0, 4], [0, 5], [0, 6], [0, 7], [0, 8],
		[1, 3], [1, 4], [1, 5], [1, 6], [1, 7], [1, 8],
		[2, 3], [2, 4], [2, 5], [2, 6], [2, 7], [2, 8],
		[3, 6], [3, 7], [3, 8],
		[4, 6], [4, 7], [4, 8],
		[5, 6], [5, 7], [5, 8]
	];

	/// <summary>
	/// Indicates the column combinations of columns.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly Cell[][] FitTableColumn = [
		[0, 27], [0, 36], [0, 45], [0, 54], [0, 63], [0, 72],
		[9, 27], [9, 36], [9, 45], [9, 54], [9, 63], [9, 72],
		[18, 27], [18, 36], [18, 45], [18, 54], [18, 63], [18, 72],
		[27, 54], [27, 63], [27, 72],
		[36, 54], [36, 63], [36, 72],
		[45, 54], [45, 63], [45, 72]
	];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static ExtendedRectanglePattern()
	{
		var result = new List<ExtendedRectanglePattern>();

		// Initializes fit types.
		for (var j = 0; j < 3; j++)
		{
			for (var i = 0; i < FitTableRow.Length; i++)
			{
				var c11 = FitTableRow[i][0] + j * 27;
				var c21 = FitTableRow[i][1] + j * 27;
				var c12 = c11 + 9;
				var c22 = c21 + 9;
				var c13 = c11 + 18;
				var c23 = c21 + 18;
				result.Add(new(false, [c11, c12, c13, c21, c22, c23], [(c11, c21), (c12, c22), (c13, c23)], 3));
			}
		}
		for (var j = 0; j < 3; j++)
		{
			for (var i = 0; i < FitTableColumn.Length; i++)
			{
				var c11 = FitTableColumn[i][0] + j * 3;
				var c21 = FitTableColumn[i][1] + j * 3;
				var c12 = c11 + 1;
				var c22 = c21 + 1;
				var c13 = c11 + 2;
				var c23 = c21 + 2;
				result.Add(new(false, [c11, c12, c13, c21, c22, c23], [(c11, c21), (c12, c22), (c13, c23)], 3));
			}
		}

		// Initializes fat types.
		for (var size = 3; size <= 7; size++)
		{
			for (var i = 0; i < HouseCombinations.Length; i++)
			{
				var (house1, house2) = (HouseCombinations[i][0], HouseCombinations[i][1]);
				foreach (var mask in Mask.EnumerateOf(9, size))
				{
					// Check whether all cells are in same house. If so, continue the loop immediately.
					if (size == 3 && mask.SplitMask is not (not 7, not 7, not 7))
					{
						continue;
					}

					var (map, pairs) = (CellMap.Empty, (List<(Cell, Cell)>)[]);
					foreach (var pos in mask)
					{
						var (cell1, cell2) = (HousesCells[house1][pos], HousesCells[house2][pos]);
						map.Add(cell1);
						map.Add(cell2);
						pairs.Add((cell1, cell2));
					}
					result.Add(new(true, map, [.. pairs], size));
				}
			}
		}
		AllPatterns = result.AsMemory();
	}


	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.ExtendedRectangle;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out bool isFat, out CellMap patternCells, out (Cell Left, Cell Right)[] pairCells, out int size)
		=> (isFat, patternCells, pairCells, size) = (IsFat, PatternCells, PairCells, Size);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is ExtendedRectanglePattern comparer && PatternCells == comparer.PatternCells;

	/// <inheritdoc/>
	public override ExtendedRectanglePattern Clone() => new(IsFat, PatternCells, PairCells, Size);
}
