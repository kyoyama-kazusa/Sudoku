namespace Sudoku.Categorization;

/// <summary>
/// Represents a group of <see cref="SingleSubtype"/> instances.
/// </summary>
/// <param name="values">Indicates the array to be initialized.</param>
/// <seealso cref="SingleSubtype"/>
[CollectionBuilder(typeof(SingleSubtypeGroup), nameof(Create))]
public readonly partial struct SingleSubtypeGroup([Field] ReadOnlyMemory<SingleSubtype> values) :
	IEnumerable<SingleSubtype>,
	IReadOnlyCollection<SingleSubtype>,
	ISliceMethod<SingleSubtypeGroup, SingleSubtype>,
	IToArrayMethod<SingleSubtypeGroup, SingleSubtype>
{
	/// <summary>
	/// Indicates the empty instance.
	/// </summary>
	public static readonly SingleSubtypeGroup Empty = default;


	/// <summary>
	/// Initializes a <see cref="SingleSubtypeGroup"/> instance via an array of <see cref="SingleSubtype"/> instances.
	/// </summary>
	/// <param name="array">An array of <see cref="SingleSubtype"/> instances.</param>
	private SingleSubtypeGroup(SingleSubtype[] array) : this((ReadOnlyMemory<SingleSubtype>)array)
	{
	}


	/// <summary>
	/// Indicates the number of <see cref="SingleSubtype"/> instances in this collection.
	/// </summary>
	public int Length => _values.Length;

	/// <summary>
	/// Gets a span from the current collection.
	/// </summary>
	public ReadOnlySpan<SingleSubtype> Span => _values.Span;

	/// <summary>
	/// Indicates the techniques that all flags covered.
	/// </summary>
	public TechniqueSet Techniques
	{
		get
		{
			TechniqueSet result = [];
			foreach (var element in _values)
			{
				result.Add(element.RelatedTechnique);
			}
			return result;
		}
	}

	/// <inheritdoc/>
	int IReadOnlyCollection<SingleSubtype>.Count => Length;


	/// <summary>
	/// Gets the element at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>A <see cref="SingleSubtype"/> instance as result.</returns>
	public SingleSubtype this[int index] => _values.ElementAt(index);


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SingleSubtype[] ToArray() => _values.ToArray();

	/// <inheritdoc cref="ISliceMethod{TSelf, TSource}.Slice(int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SingleSubtypeGroup Slice(int start, int count) => new(_values.Slice(start, count));

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AnonymousSpanEnumerator<SingleSubtype> GetEnumerator() => new(_values.Span);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _values.ToArray().GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<SingleSubtype> IEnumerable<SingleSubtype>.GetEnumerator() => _values.ToArray().AsEnumerable().GetEnumerator();

	/// <inheritdoc/>
	IEnumerable<SingleSubtype> ISliceMethod<SingleSubtypeGroup, SingleSubtype>.Slice(int start, int count) => Slice(start, count);


	/// <summary>
	/// Creates a <see cref="SingleSubtypeGroup"/> instance.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <returns>A <see cref="SingleSubtypeGroup"/> instance.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SingleSubtypeGroup Create(ReadOnlySpan<SingleSubtype> values) => values.IsEmpty ? Empty : new(values.ToArray());
}
