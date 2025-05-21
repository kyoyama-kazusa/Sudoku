namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Represents for a data set that describes the complete information about a unique loop technique.
/// </summary>
/// <param name="loop"><inheritdoc cref="Loop" path="/summary"/></param>
/// <param name="path"><inheritdoc cref="Path" path="/summary"/></param>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
[TypeImpl(TypeImplFlags.Object_GetHashCode | TypeImplFlags.Object_ToString)]
public sealed partial class UniqueLoopPattern(in CellMap loop, Cell[] path, Mask digitsMask) : Pattern
{
	/// <inheritdoc/>
	public override bool IsChainingCompatible => false;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.UniqueLoop;

	/// <summary>
	/// Indicates the cells used in this whole unique loop.
	/// </summary>
	public CellMap Loop { get; } = loop;

	/// <summary>
	/// Indicates the detail path of the loop.
	/// </summary>
	public Cell[] Path { get; } = path;

	/// <summary>
	/// Indicates the digits used, represented as a mask.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	[HashCodeMember]
	private int LoopHashCode => Loop.GetHashCode();

	[StringMember]
	private string PatternString
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append($$"""{{nameof(UniqueLoopPattern)}} {""");
			sb.Append($"{nameof(Loop)} = {Loop}");
			sb.Append(", ");
			sb.Append($"{nameof(Path)} = [");
			for (var i = 0; i < Path.Length; i++)
			{
				sb.Append(Path[i]);
				if (i != Path.Length - 1)
				{
					sb.Append(", ");
				}
			}
			sb.Append("], ");
			sb.Append($$"""{{nameof(DigitsMask)}} = {{DigitsMask}} ({{DigitsMask.ToBinaryString()}}) }""");
			return sb.ToString();
		}
	}


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out CellMap loop, out Cell[] path, out Mask digitsMask)
		=> (loop, path, digitsMask) = (Loop, Path, DigitsMask);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other) => other is UniqueLoopPattern comparer && Loop == comparer.Loop;

	/// <inheritdoc/>
	public override UniqueLoopPattern Clone() => new(Loop, Path, DigitsMask);


	/// <summary>
	/// Determine whether the specified loop is a valid unique loop or unique rectangle pattern.
	/// </summary>
	/// <typeparam name="TCollection">The type of the collection that can iterate on each <see cref="Cell"/> instances.</typeparam>
	/// <param name="loopPath">The path of the loop.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	/// <remarks>
	/// <para>
	/// This method uses a trick way (node-coloring) to check whether a loop is a unique loop.
	/// The parameter <paramref name="loopPath"/> holds a list of cells, which are nodes of the loop.
	/// Then we begin to colorize each node with 2 different colors <b><i>A</i></b> and <b><i>B</i></b>.
	/// The only point we should notice is that the color between 2 adjacent nodes should be different
	/// (i.e. one is colored with <b><i>A</i></b> and the other should be colored with <b><i>B</i></b>).
	/// If at least one pair of cells in a same house are colored with a same color, it won't be a valid unique loop.
	/// Those colors stands for the final filling digits. Therefore, "Two cells in a same house are colored same"
	/// means "Two cells in a same house are filled with a same digit", which disobeys the basic sudoku rule.
	/// </para>
	/// <para>
	/// This method won't check for whether the loop is a unique rectangle (of length 4). It means, a valid unique rectangle
	/// can also make this method return <see langword="true"/>.
	/// </para>
	/// </remarks>
	public static bool IsValid<TCollection>(TCollection loopPath) where TCollection : IEnumerable<Cell>, allows ref struct
	{
		var (visitedOdd, visitedEven, isOdd) = (0, 0, false);
		foreach (var cell in loopPath)
		{
			foreach (var houseType in HouseTypes)
			{
				var house = cell.ToHouse(houseType);
				if (isOdd)
				{
					if ((visitedOdd >> house & 1) != 0)
					{
						return false;
					}
					visitedOdd |= 1 << house;
				}
				else
				{
					if ((visitedEven >> house & 1) != 0)
					{
						return false;
					}
					visitedEven |= 1 << house;
				}
			}
			isOdd = !isOdd;
		}
		return visitedEven == visitedOdd;
	}
}
