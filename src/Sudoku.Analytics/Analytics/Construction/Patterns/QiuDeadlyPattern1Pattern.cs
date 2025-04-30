namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Defines a pattern that is a Qiu's deadly pattern technique pattern in theory. The sketch is like:
/// <code><![CDATA[
/// .-------.-------.-------.
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | P P . | . . . | . . . |
/// :-------+-------+-------:
/// | S S B | B B B | B B B |
/// | S S B | B B B | B B B |
/// | . . . | . . . | . . . |
/// :-------+-------+-------:
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// '-------'-------'-------'
/// ]]></code>
/// Where:
/// <list type="table">
/// <item><term>P</term><description>Corner Cells.</description></item>
/// <item><term>S</term><description>Cross-line Cells.</description></item>
/// <item><term>B</term><description>Base-line Cells.</description></item>
/// </list>
/// </summary>
/// <param name="Corner">The corner cells that is <c>P</c> in that sketch.</param>
/// <param name="Lines">The base-line cells that is <c>B</c> in that sketch.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class QiuDeadlyPattern1Pattern([Property] in CellMap Corner, [Property] HouseMask Lines) : Pattern
{
	/// <summary>
	/// Indicates the line offsets of the patterns.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	internal static readonly RowIndex[][] LineOffsets = [[0, 1, 2], [0, 2, 1], [1, 2, 0], [3, 4, 5], [3, 5, 4], [4, 5, 3], [6, 7, 8], [6, 8, 7], [7, 8, 6]];

	/// <summary>
	/// Indicates the patterns for case 1.
	/// </summary>
	internal static readonly QiuDeadlyPattern1Pattern[] Patterns;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static QiuDeadlyPattern1Pattern()
	{
		// Case 1: 2 lines + 2 cells.
		var patterns = new List<QiuDeadlyPattern1Pattern>();
		foreach (var isRow in (true, false))
		{
			var (@base, fullHousesMask) = isRow ? (9, HouseMaskOperations.AllRowsMask) : (18, HouseMaskOperations.AllColumnsMask);
			foreach (var lineOffsetPair in LineOffsets)
			{
				var (l1, l2, l3) = (lineOffsetPair[0] + @base, lineOffsetPair[1] + @base, lineOffsetPair[2] + @base);
				var linesMask = 1 << l1 | 1 << l2;
				foreach (var cornerHouse in fullHousesMask & ~linesMask & ~(1 << l3))
				{
					foreach (var posPair in LineOffsets)
					{
						patterns.Add(new([HousesCells[cornerHouse][posPair[0]], HousesCells[cornerHouse][posPair[1]]], linesMask));
					}
				}
			}
		}
		Patterns = [.. patterns];
	}


	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.QiuDeadlyPattern1;

	/// <summary>
	/// Indicates the crossline cells.
	/// </summary>
	public CellMap Crossline
	{
		get
		{
			var l1 = BitOperations.TrailingZeroCount(Lines);
			var l2 = Lines.GetNextSet(l1);
			return (HousesMap[l1] | HousesMap[l2]) & PeersMap[Corner[0]] | (HousesMap[l1] | HousesMap[l2]) & PeersMap[Corner[1]];
		}
	}

	/// <summary>
	/// Indicates the mirror cells.
	/// </summary>
	public CellMap Mirror
	{
		get
		{
			var block = Crossline.FirstSharedHouse;
			var l1 = BitOperations.TrailingZeroCount(Lines);
			var l2 = Lines.GetNextSet(l1);
			return HousesMap[block] & ~(HousesMap[l1] | HousesMap[l2]);
		}
	}


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is QiuDeadlyPattern1Pattern comparer && Crossline == comparer.Crossline && Lines == comparer.Lines;

	/// <inheritdoc/>
	public override QiuDeadlyPattern1Pattern Clone() => new(Corner, Lines);
}
