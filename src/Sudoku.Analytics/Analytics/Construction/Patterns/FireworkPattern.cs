namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Indicates a firework pattern. The pattern will be like:
/// <code><![CDATA[
/// .-------.-------.-------.
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// :-------+-------+-------:
/// | . . . | B . . | . C . |
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// :-------+-------+-------:
/// | . . . | . . . | . . . |
/// | . . . | . . . | . . . |
/// | . . . | A . . | .(D). |
/// '-------'-------'-------'
/// ]]></code>
/// </summary>
/// <param name="map">Indicates the full map of all cells used.</param>
/// <param name="pivot">The pivot cell. This property can be <see langword="null"/> if four cells are used.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class FireworkPattern([Property, HashCodeMember] in CellMap map, [Property, HashCodeMember] Cell? pivot) :
	Pattern
{
	/// <summary>
	/// Indicates the patterns used.
	/// </summary>
	internal static readonly FireworkPattern[] Patterns;

	/// <summary>
	/// Indicates the house combinations.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly BlockIndex[][] HouseCombinations = [
		[0, 1, 3, 4], [0, 2, 3, 5], [1, 2, 4, 5],
		[0, 1, 6, 7], [0, 2, 6, 8], [1, 2, 7, 8],
		[3, 4, 6, 7], [3, 5, 6, 8], [4, 5, 7, 8]
	];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static FireworkPattern()
	{
		Patterns = new FireworkPattern[3645];

		var i = 0;
		foreach (var houseQuad in HouseCombinations)
		{
			// Collection for pattern triples.
			foreach (var triple in houseQuad.AsReadOnlySpan().GetSubsets(3))
			{
				foreach (var a in HousesMap[triple[0]])
				{
					foreach (var b in HousesMap[triple[1]])
					{
						foreach (var c in HousesMap[triple[2]])
						{
							if ((a.AsCellMap() + b).FirstSharedHouse != 32 && (a.AsCellMap() + c).FirstSharedHouse != 32)
							{
								Patterns[i++] = new([a, b, c], a);
								continue;
							}

							if ((a.AsCellMap() + b).FirstSharedHouse != 32 && (b.AsCellMap() + c).FirstSharedHouse != 32)
							{
								Patterns[i++] = new([a, b, c], b);
								continue;
							}

							if ((a.AsCellMap() + c).FirstSharedHouse != 32 && (b.AsCellMap() + c).FirstSharedHouse != 32)
							{
								Patterns[i++] = new([a, b, c], c);
							}
						}
					}
				}
			}

			// Collection for pattern quadruples.
			foreach (var a in HousesMap[houseQuad[0]])
			{
				foreach (var b in HousesMap[houseQuad[1]])
				{
					foreach (var c in HousesMap[houseQuad[2]])
					{
						foreach (var d in HousesMap[houseQuad[3]])
						{
							if ((a.AsCellMap() + b).FirstSharedHouse == 32 || (a.AsCellMap() + c).FirstSharedHouse == 32
								|| (b.AsCellMap() + d).FirstSharedHouse == 32 || (c.AsCellMap() + d).FirstSharedHouse == 32)
							{
								continue;
							}

							Patterns[i++] = new([a, b, c, d], null);
						}
					}
				}
			}
		}
	}


	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.Firework;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is FireworkPattern comparer && Map == comparer.Map && Pivot == comparer.Pivot;

	/// <inheritdoc/>
	public override FireworkPattern Clone() => new(Map, Pivot);
}
