namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents a domino loop pattern.
/// </summary>
/// <param name="cells">Indicates the cells used.</param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class DominoLoopPattern([Property] Cell[] cells) : Pattern
{
	/// <summary>
	/// Indicateds all possible patterns.
	/// </summary>
	internal static readonly DominoLoopPattern[] Patterns;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static DominoLoopPattern()
	{
		Patterns = new DominoLoopPattern[729];

		var s = (stackalloc int[4]);
		for (var (a, n) = (9, 0); a < 18; a++)
		{
			for (var b = 9; b < 18; b++)
			{
				if (a / 3 == b / 3 || b < a)
				{
					continue;
				}

				for (var c = 18; c < 27; c++)
				{
					for (var d = 18; d < 27; d++)
					{
						if (c / 3 == d / 3 || d < c)
						{
							continue;
						}

						var all = HousesMap[a] | HousesMap[b] | HousesMap[c] | HousesMap[d];
						var overlap = (HousesMap[a] | HousesMap[b]) & (HousesMap[c] | HousesMap[d]);
						var blockMask = overlap.BlockMask;
						for (var (i, count) = (0, 0); count < 4 && i < 16; i++)
						{
							if ((blockMask >> i & 1) != 0)
							{
								s[count++] = i;
							}
						}

						all &= HousesMap[s[0]] | HousesMap[s[1]] | HousesMap[s[2]] | HousesMap[s[3]];
						all &= ~overlap;

						var patternCells = new Cell[16];
						var pos = 0;
						foreach (var cell in all & HousesMap[a])
						{
							patternCells[pos++] = cell;
						}
						foreach (var cell in all & HousesMap[d])
						{
							patternCells[pos++] = cell;
						}
						var cells1 = (Cell[])[.. all & HousesMap[b]];
						patternCells[pos++] = cells1[2];
						patternCells[pos++] = cells1[3];
						patternCells[pos++] = cells1[0];
						patternCells[pos++] = cells1[1];
						var cells2 = (Cell[])[.. all & HousesMap[c]];
						patternCells[pos++] = cells2[2];
						patternCells[pos++] = cells2[3];
						patternCells[pos++] = cells2[0];
						patternCells[pos++] = cells2[1];

						Patterns[n++] = new(patternCells);
					}
				}
			}
		}
	}


	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.DominoLoop;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	[HashCodeMember]
	public CellMap Map => [.. Cells];

	[StringMember(nameof(Map))]
	private string MapString => Map.ToString();


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other) => other is DominoLoopPattern comparer && Map == comparer.Map;

	/// <inheritdoc/>
	public override DominoLoopPattern Clone() => new(Cells);
}
