namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Defines a pattern that is a Borescoper's Deadly Pattern technique pattern in theory. The sketch is like:
/// <code><![CDATA[
/// .-------.-------.-------.
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// :-------+-------+-------:
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | P P . |
/// :-------+-------+-------:
/// | . . . | . . Q | S S . |
/// | . . . | . . Q | S(S). |
/// | . . . | . . . | . . . |
/// '-------'-------'-------'
/// ]]></code>
/// Where:
/// <list type="table">
/// <item><term>P</term><description>The first group of cells.</description></item>
/// <item><term>Q</term><description>The second group of cells.</description></item>
/// <item>
/// <term>S</term>
/// <description>
/// The square cells of size 3 or 4. The cell with the bracket (r8c8 in the picture)
/// is optional.
/// </description>
/// </item>
/// </list>
/// </summary>
/// <param name="mask"><inheritdoc cref="_mask" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class BorescoperDeadlyPatternPattern(long mask) : Pattern
{
	/// <summary>
	/// Indicates all possible patterns to iterate.
	/// </summary>
	/// <remarks>
	/// Please note that all possible heptagons and octagons are in here.
	/// </remarks>
	internal static readonly BorescoperDeadlyPatternPattern[] Patterns;

	/// <summary>
	/// Indicates the quadruple list that describes the chosen cells in the target block.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly BlockIndex[][] OffsetQuadruples = [
		[0, 1, 3, 4], [1, 2, 4, 5], [3, 4, 6, 7],
		[4, 5, 7, 8], [0, 2, 3, 5], [3, 5, 6, 8],
		[0, 1, 6, 7], [1, 2, 7, 8], [0, 2, 6, 8]
	];


	/// <summary>
	/// <para>Indicates the internal mask.</para>
	/// <para>
	/// This mask is of type <see cref="long"/>, where the distribution of each bit is as follows:
	/// <code><![CDATA[
	/// 0      7     14     21     28     35     42     49     56
	/// ↓      ↓      ↓      ↓      ↓      ↓      ↓      ↓      ↓
	/// |-------|-------|-------|-------|-------|-------|-------|-------|
	/// ↑       ↑       ↑       ↑       ↑       ↑       ↑       ↑       ↑
	/// 0       8      16      24      32      40      48      56      64
	/// ]]></code>
	/// where the bit <c>[0..56]</c> is for 8 cells, the last 7 bits determine the pattern is a
	/// heptagon or a octagon. If the value is 127 (not available), the pattern will be a heptagon.
	/// </para>
	/// <para>
	/// Due to the drawing API, you have to check this file rather than the tip window.
	/// </para>
	/// </summary>
	[HashCodeMember]
	private readonly long _mask = mask;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static BorescoperDeadlyPatternPattern()
	{
		var count = 0;
		Patterns = new BorescoperDeadlyPatternPattern[14580];
		for (var block = 0; block < 9; block++)
		{
			for (var i = 0; i < 9; i++) // 9 cases.
			{
				var quadruple = OffsetQuadruples[i];
				var tempQuadruple = new Cell[4];
				for (var j = 0; j < 4; j++)
				{
					// Set all indices to cell offsets.
					tempQuadruple[j] = (block / 3 * 3 + quadruple[j] / 3) * 9 + block % 3 * 3 + quadruple[j] % 3;
				}

				collectHeptagonPatterns(block, i, tempQuadruple, ref count);
				collectOctagonPatterns(block, i, tempQuadruple, ref count);
			}
		}


		static void collectHeptagonPatterns(House block, int i, Cell[] quadruple, ref int count)
		{
			if (quadruple is not [var q1, var q2, var q3, var q4])
			{
				return;
			}

			var blockTriplets = (ReadOnlySpan<(Cell, Cell, Cell)>)[(q1, q2, q3), (q2, q1, q4), (q3, q1, q4), (q4, q2, q3)];
			for (var j = 0; j < 4; j++)
			{
				_ = blockTriplets[j] is (var t1, var t2, var t3) triplet;
				var house1 = (t1.AsCellMap() + t2).SharedLine;
				var house2 = (t1.AsCellMap() + t3).SharedLine;
				var pair1 = new Cell[6, 2];
				var pair2 = new Cell[6, 2];
				var (o1, o2) = i switch { >= 0 and <= 3 => (9, 1), 4 or 5 => (9, 2), 6 or 7 => (18, 1), 8 => (18, 2) };
				if (house1 is >= 9 and < 18)
				{
					// 'house1' is a row and 'house2' is a column.
					r(block, house1, pair1, o1, j);
					r(block, house2, pair2, o2, j);
				}
				else
				{
					// 'house1' is a column and 'house2' is a row.
					r(block, house1, pair1, o2, j);
					r(block, house2, pair2, o1, j);
				}

				for (var i1 = 0; i1 < 6; i1++)
				{
					for (var i2 = 0; i2 < 6; i2++)
					{
						// Now check extra digits.
						var allCells = (ReadOnlySpan<Cell>)[.. triplet, pair1[i1, 0], pair1[i1, 1], pair2[i2, 0], pair2[i2, 1]];
						var v = 0L;
						for (var z = 0; z < allCells.Length; z++)
						{
							v |= (long)allCells[z];

							if (z != allCells.Length - 1)
							{
								v <<= 7;
							}
							if (z == 2)
							{
								v |= 127;
								v <<= 7;
							}
						}

						Patterns[count++] = new(v);
					}
				}
			}
		}

		static void collectOctagonPatterns(House block, int i, Cell[] quad, ref int count)
		{
			if (quad is not [var t1, var t2, var t3, _])
			{
				return;
			}

			var house1 = (t1.AsCellMap() + t2).SharedLine;
			var house2 = (t1.AsCellMap() + t3).SharedLine;
			var pair1 = new Cell[6, 2];
			var pair2 = new Cell[6, 2];
			var (o1, o2) = i switch { >= 0 and <= 3 => (9, 1), 4 or 5 => (9, 2), 6 or 7 => (18, 1), 8 => (18, 2) };
			if (house1 is >= 9 and < 18)
			{
				// 'house1' is a row and 'house2' is a column.
				r(block, house1, pair1, o1, 0);
				r(block, house2, pair2, o2, 0);
			}
			else
			{
				// 'house1' is a column and 'house2' is a row.
				r(block, house1, pair1, o2, 0);
				r(block, house2, pair2, o1, 0);
			}

			for (var i1 = 0; i1 < 6; i1++)
			{
				for (var i2 = 0; i2 < 6; i2++)
				{
					// Now check extra digits.
					var allCells = (ReadOnlySpan<Cell>)[.. quad, pair1[i1, 0], pair1[i1, 1], pair2[i2, 0], pair2[i2, 1]];
					var v = 0L;
					for (var z = 0; z < allCells.Length; z++)
					{
						var cell = allCells[z];
						v |= (long)cell;
						if (z != allCells.Length - 1)
						{
							v <<= 7;
						}
					}

					Patterns[count++] = new(v);
				}
			}
		}

		static void r(House block, House houseIndex, Cell[,] pair, int increment, int index)
		{
			for (var (i, cur) = (0, 0); i < 9; i++)
			{
				var cell = HousesCells[houseIndex][i];
				if (block == cell.ToHouse(HouseType.Block))
				{
					continue;
				}

				(pair[cur, 0], pair[cur, 1]) = index switch
				{
					0 => (cell, cell + increment),
					1 => houseIndex is >= 18 and < 27 ? (cell - increment, cell) : (cell, cell + increment),
					2 => houseIndex is >= 9 and < 18 ? (cell - increment, cell) : (cell, cell + increment),
					3 => (cell - increment, cell)
				};
				cur++;
			}
		}
	}


	/// <summary>
	/// Indicates whether the specified pattern is a heptagon.
	/// </summary>
	public bool IsHeptagon => (_mask >> 28 & 127) == 127;

	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.BorescoperDeadlyPattern;

	/// <summary>
	/// Indicates the map of pair 1 cells.
	/// </summary>
	public CellMap Pair1Map => [Pair1.A, Pair1.B];

	/// <summary>
	/// Indicates the map of pair 2 cells.
	/// </summary>
	public CellMap Pair2Map => [Pair2.A, Pair2.B];

	/// <summary>
	/// The map of other three (or four) cells.
	/// </summary>
	public CellMap CenterCellsMap
		=> this switch
		{
			{ CenterCells: var (a, b, c, _), IsHeptagon: true } => [a, b, c],
			{ CenterCells: var (a, b, c, d), IsHeptagon: false } => [a, b, c, d]
		};

	/// <summary>
	/// Indicates the full map of all cells used in this pattern.
	/// </summary>
	public CellMap Map => Pair1Map | Pair2Map | CenterCellsMap;

	/// <summary>
	/// Indicates the pair 1.
	/// </summary>
	private (Cell A, Cell B) Pair1 => ((Cell)(_mask >> 7 & 127), (Cell)(_mask & 127));

	/// <summary>
	/// Indicates the pair 2.
	/// </summary>
	private (Cell A, Cell B) Pair2 => ((Cell)(_mask >> 21 & 127), (Cell)(_mask >> 14 & 127));

	/// <summary>
	/// Indicates the other three (or four) cells.
	/// </summary>
	/// <remarks>
	/// <b>If and only if</b> the fourth value in the returned quadruple is available.
	/// </remarks>
	private (Cell A, Cell B, Cell C, Cell D) CenterCells
		=> ((Cell)(_mask >> 49 & 127), (Cell)(_mask >> 42 & 127), (Cell)(_mask >> 35 & 127), (Cell)(_mask >> 28 & 127));


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is BorescoperDeadlyPatternPattern comparer && _mask == comparer._mask;

	/// <inheritdoc/>
	public override BorescoperDeadlyPatternPattern Clone() => new(_mask);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Deconstruct(out (Cell A, Cell B) pair1, out (Cell A, Cell B) pair2, out (Cell A, Cell B, Cell C, Cell D) centerCells)
		=> (pair1, pair2, centerCells) = (Pair1, Pair2, CenterCells);
}
