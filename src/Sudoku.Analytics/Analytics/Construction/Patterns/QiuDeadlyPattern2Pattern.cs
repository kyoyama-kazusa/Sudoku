namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Defines a pattern that is a Qiu's deadly pattern technique pattern in theory. The sketch is like:
/// <code><![CDATA[
/// .-------.-------.-------.
/// | B B . | . . . | . . . |
/// | B B . | . . . | . . . |
/// | B B . | . . . | . . . |
/// :-------+-------+-------:
/// | S S B | B B B | B B B |
/// | S S B | B B B | B B B |
/// | B B . | . . . | . . . |
/// :-------+-------+-------:
/// | B B . | . . . | . . . |
/// | B B . | . . . | . . . |
/// | B B . | . . . | . . . |
/// '-------'-------'-------'
/// ]]></code>
/// Where:
/// <list type="table">
/// <item><term>S</term><description>Cross-line Cells.</description></item>
/// <item><term>B</term><description>Base-line Cells.</description></item>
/// </list>
/// </summary>
/// <param name="lines1"><inheritdoc cref="Lines1" path="/summary"/></param>
/// <param name="lines2"><inheritdoc cref="Lines2" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class QiuDeadlyPattern2Pattern(HouseMask lines1, HouseMask lines2) : Pattern
{
	/// <summary>
	/// Indicates the patterns for case 2.
	/// </summary>
	internal static readonly QiuDeadlyPattern2Pattern[] Patterns;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static QiuDeadlyPattern2Pattern()
	{
		// Case 2: 2 rows + 2 columns.
		var patterns = new List<QiuDeadlyPattern2Pattern>();
		var rows = HouseMaskOperations.AllRowsMask.AllSets;
		var columns = HouseMaskOperations.AllColumnsMask.AllSets;
		foreach (var lineOffsetPairRow in QiuDeadlyPattern1Pattern.LineOffsets)
		{
			var rowsMask = 1 << rows[lineOffsetPairRow[0]] | 1 << rows[lineOffsetPairRow[1]];
			foreach (var lineOffsetPairColumn in QiuDeadlyPattern1Pattern.LineOffsets)
			{
				var columnsMask = 1 << columns[lineOffsetPairColumn[0]] | 1 << columns[lineOffsetPairColumn[1]];
				patterns.Add(new(rowsMask, columnsMask));
			}
		}
		Patterns = [.. patterns];
	}


	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.QiuDeadlyPattern2;

	/// <summary>
	/// Indicates the crossline cells.
	/// </summary>
	public CellMap Crossline
	{
		get
		{
			var l11 = BitOperations.TrailingZeroCount(Lines1);
			var l21 = Lines1.GetNextSet(l11);
			var l12 = BitOperations.TrailingZeroCount(Lines2);
			var l22 = Lines2.GetNextSet(l12);
			var result = CellMap.Empty;
			foreach (var (a, b) in ((l11, l12), (l11, l22), (l21, l12), (l21, l22)))
			{
				result |= HousesMap[a] & HousesMap[b];
			}
			return result;
		}
	}

	/// <summary>
	/// The first pair of lines.
	/// </summary>
	public HouseMask Lines1 { get; } = lines1;

	/// <summary>
	/// The second pair of lines.
	/// </summary>
	public HouseMask Lines2 { get; } = lines2;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is QiuDeadlyPattern2Pattern comparer && Lines1 == comparer.Lines1 && Lines2 == comparer.Lines2;

	/// <inheritdoc/>
	public override QiuDeadlyPattern2Pattern Clone() => new(Lines1, Lines2);
}
