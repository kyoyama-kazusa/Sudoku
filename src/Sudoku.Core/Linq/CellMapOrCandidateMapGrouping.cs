namespace Sudoku.Linq;

/// <summary>
/// Represents a map group for <see cref="CandidateMap"/> and <see cref="CellMap"/>.
/// </summary>
/// <typeparam name="TMap">The type of the map that stores the <see cref="Values"/>.</typeparam>
/// <typeparam name="TElement">The type of elements stored in <see cref="Values"/>.</typeparam>
/// <typeparam name="TKey">The type of the key in the group.</typeparam>
/// <param name="key">Indicates the key used.</param>
/// <param name="values">Indicates the candidates.</param>
/// <seealso cref="CellMap"/>
/// <seealso cref="CandidateMap"/>
[TypeImpl(
	TypeImplFlags.Object_Equals | TypeImplFlags.Object_GetHashCode | TypeImplFlags.EqualityOperators | TypeImplFlags.Equatable,
	IsLargeStructure = true)]
public readonly partial struct CellMapOrCandidateMapGrouping<TMap, TElement, TKey>(
	[Property] TKey key,
	[Property, HashCodeMember] in TMap values
) :
	IEnumerable<TElement>,
	IEquatable<CellMapOrCandidateMapGrouping<TMap, TElement, TKey>>,
	IEqualityOperators<CellMapOrCandidateMapGrouping<TMap, TElement, TKey>, CellMapOrCandidateMapGrouping<TMap, TElement, TKey>, bool>,
	IGrouping<TKey, TElement>,
	ISelectMethod<TMap, TElement>,
	IWhereMethod<TMap, TElement>
	where TMap : unmanaged, ICellMapOrCandidateMap<TMap, TElement>
	where TElement : unmanaged, IBinaryInteger<TElement>
	where TKey : notnull
{
	/// <summary>
	/// Indicates the number of values stored in <see cref="Values"/>, i.e. the shorthand of expression <c>Values.Count</c>.
	/// </summary>
	/// <seealso cref="Values"/>
	public int Count => Values.Count;

	[EquatableMember]
	private TMap ValuesEntry => Values;


	/// <inheritdoc cref="ICellMapOrCandidateMap{TSelf, TElement}.this[TElement]"/>
	public TElement this[TElement index] => Values[index];


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Deconstruct(out TKey key, out TMap values) => (key, values) = (Key, Values);

	/// <summary>
	/// Returns an enumerator that iterates through a collection.
	/// </summary>
	/// <returns>An enumerator object that can be used to iterate through the collection.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnonymousSpanEnumerator<TElement> GetEnumerator() => Values.GetEnumerator();

	/// <summary>
	/// Makes a <see cref="CellMap"/> instance that is concatenated by a list of groups
	/// of type <see cref="CellMapOrCandidateMapGrouping{TMap, TElement, TKey}"/>, adding their keys.
	/// </summary>
	/// <param name="groups">The groups.</param>
	/// <returns>A <see cref="CellMap"/> instance.</returns>
	public static CellMap CreateMapByKeys(ReadOnlySpan<CellMapOrCandidateMapGrouping<TMap, TElement, Cell>> groups)
	{
		var result = CellMap.Empty;
		foreach (ref readonly var group in groups)
		{
			result.Add(group.Key);
		}
		return result;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Values.AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<TElement> IWhereMethod<TMap, TElement>.Where(Func<TElement, bool> predicate) => this.Where(predicate).ToArray();

	/// <inheritdoc/>
	IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator() => Values.AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<TMap, TElement>.Select<TResult>(Func<TElement, TResult> selector)
		=> this.Select(selector).ToArray();
}
