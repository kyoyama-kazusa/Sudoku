namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents a chromatic pattern.
/// </summary>
/// <param name="block1Cells"><inheritdoc cref="Block1Cells" path="/summary"/></param>
/// <param name="block2Cells"><inheritdoc cref="Block2Cells" path="/summary"/></param>
/// <param name="block3Cells"><inheritdoc cref="Block3Cells" path="/summary"/></param>
/// <param name="block4Cells"><inheritdoc cref="Block4Cells" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode)]
public sealed partial class ChromaticPatternPattern(Cell[] block1Cells, Cell[] block2Cells, Cell[] block3Cells, Cell[] block4Cells) :
	Pattern
{
	/// <summary>
	/// All possible blocks combinations being reserved for chromatic pattern searcher's usages.
	/// </summary>
	internal static readonly Mask[] ChromaticPatternBlocksCombinations = [
		0b000_011_011, 0b000_101_101, 0b000_110_110,
		0b011_000_011, 0b101_000_101, 0b110_000_110,
		0b011_011_000, 0b101_101_000, 0b110_110_000
	];

	/// <summary>
	/// The possible pattern offsets.
	/// </summary>
	internal static readonly ChromaticPatternPattern[] Patterns;

	/// <summary>
	/// Indicates the possible offset values for diagonal cases.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly Cell[][] DiagonalCases = [[0, 10, 20], [1, 11, 18], [2, 9, 19]];

	/// <summary>
	/// Indicates the possible offset values for anti-diagonal cases.
	/// </summary>
	/// <remarks>
	/// <include file="../../global-doc-comments.xml" path="g/requires-static-constructor-invocation" />
	/// </remarks>
	private static readonly Cell[][] AntidiagonalCases = [[0, 11, 19], [1, 9, 20], [2, 10, 18]];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static ChromaticPatternPattern()
	{
		var patternOffsetsList = new List<ChromaticPatternPattern>();
		foreach (var (aCase, bCase, cCase, dCase) in (
			(true, false, false, false),
			(false, true, false, false),
			(false, false, true, false),
			(false, false, false, true)
		))
		{
			// Phase 1.
			foreach (var a in aCase ? DiagonalCases : AntidiagonalCases)
			{
				foreach (var b in bCase ? DiagonalCases : AntidiagonalCases)
				{
					foreach (var c in cCase ? DiagonalCases : AntidiagonalCases)
					{
						foreach (var d in dCase ? DiagonalCases : AntidiagonalCases)
						{
							patternOffsetsList.Add(new(a, b, c, d));
						}
					}
				}
			}

			// Phase 2.
			foreach (var a in aCase ? AntidiagonalCases : DiagonalCases)
			{
				foreach (var b in bCase ? AntidiagonalCases : DiagonalCases)
				{
					foreach (var c in cCase ? AntidiagonalCases : DiagonalCases)
					{
						foreach (var d in dCase ? AntidiagonalCases : DiagonalCases)
						{
							patternOffsetsList.Add(new(a, b, c, d));
						}
					}
				}
			}
		}
		Patterns = [.. patternOffsetsList];
	}


	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.ChromaticPattern;

	/// <summary>
	/// Indicates the cells used in first block.
	/// </summary>
	public Cell[] Block1Cells { get; } = block1Cells;

	/// <summary>
	/// Indicates the cells used in second block.
	/// </summary>
	public Cell[] Block2Cells { get; } = block2Cells;

	/// <summary>
	/// Indicates the cells used in third block.
	/// </summary>
	public Cell[] Block3Cells { get; } = block3Cells;

	/// <summary>
	/// Indicates the cells used in fourth block.
	/// </summary>
	public Cell[] Block4Cells { get; } = block4Cells;

	/// <summary>
	/// Indicates all cells used.
	/// </summary>
	public CellMap Map => [.. Block1Cells, .. Block2Cells, .. Block3Cells, .. Block4Cells];

	[HashCodeMember]
	private int HashCode => Map.GetHashCode();


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out Cell[] block1Cells, out Cell[] block2Cells, out Cell[] block3Cells, out Cell[] block4Cells)
		=> (block1Cells, block2Cells, block3Cells, block4Cells) = (Block1Cells, Block2Cells, Block3Cells, Block4Cells);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is ChromaticPatternPattern comparer && Map == comparer.Map;

	/// <inheritdoc/>
	public override ChromaticPatternPattern Clone() => new(Block1Cells, Block2Cells, Block3Cells, Block4Cells);
}
