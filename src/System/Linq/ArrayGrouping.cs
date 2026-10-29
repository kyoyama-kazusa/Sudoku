namespace System.Linq;

/// <summary>
/// Represents a type that enumerates elements of type <typeparamref name="TSource"/>[],
/// grouped by the specified key of type <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TSource">The type of each element.</typeparam>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <param name="elements"><inheritdoc cref="_elements" path="/summary"/></param>
/// <param name="key"><inheritdoc cref="Key" path="/summary"/></param>
public sealed class ArrayGrouping<TSource, TKey>(TSource[] elements, TKey key) : ISequenceGrouping<ArrayGrouping<TSource, TKey>, TKey, TSource>
	where TKey : notnull
{
	/// <summary>
	/// Indicates the elements.
	/// </summary>
	private readonly TSource[] _elements = elements;


	/// <summary>
	/// Indicates the key that can compare each element.
	/// </summary>
	public TKey Key { get; } = key;

	/// <inheritdoc/>
	ReadOnlySpan<TSource> ISequenceGrouping<ArrayGrouping<TSource, TKey>, TKey, TSource>.Elements => _elements;


	/// <summary>
	/// Gets the element at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The reference to the element at the specified index.</returns>
	public ref readonly TSource this[int index] => ref _elements[index];


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as ArrayGrouping<TSource, TKey>);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] ArrayGrouping<TSource, TKey>? other)
		=> other is not null && ReferenceEquals(_elements, other._elements) && Key.Equals(other.Key);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Key, (nint)Unsafe.AsPointer(ref _elements[0]));

	/// <inheritdoc/>
	public override string ToString()
		=> $"{nameof(ArrayGrouping<,>)} {{ {nameof(Key)} = {Key}, FirstElementString = {_elements[0]} }}";

	/// <summary>
	/// Projects elements into a new form.
	/// </summary>
	/// <typeparam name="TResult">The type of each element in result collection.</typeparam>
	/// <param name="selector">The selector method that transform the object into new one.</param>
	/// <returns>A list of <typeparamref name="TResult"/> values.</returns>
	public TResult[] Select<TResult>(Func<TSource, TResult> selector)
	{
		var result = new List<TResult>(_elements.Length);
		foreach (var element in _elements)
		{
			result.AddRef(selector(element));
		}
		return [.. result];
	}

	/// <summary>
	/// Filters the collection, only reserving elements satisfying the specified condition.
	/// </summary>
	/// <param name="predicate">The condition that checks for each element.</param>
	/// <returns>A list of <typeparamref name="TSource"/> elements satisfying the condition.</returns>
	public TSource[] Where(Func<TSource, bool> predicate)
	{
		var result = new List<TSource>(_elements.Length);
		foreach (var element in _elements)
		{
			if (predicate(element))
			{
				result.AddRef(element);
			}
		}
		return [.. result];
	}

	/// <inheritdoc cref="SequenceExtensions.AsReadOnlySpan{T}(T[])"/>
	public ReadOnlySpan<TSource> AsReadOnlySpan() => _elements;

	/// <summary>
	/// Creates an enumerator that can enumerate each element in the source collection.
	/// </summary>
	/// <returns>An enumerator instance.</returns>
	public AnonymousSpanEnumerator<TSource> GetEnumerator() => new(_elements);

	/// <inheritdoc cref="ReadOnlySpan{T}.GetPinnableReference"/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ref readonly TSource GetPinnableReference() => ref _elements[0];

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<ArrayGrouping<TSource, TKey>, TSource>.Select<TResult>(Func<TSource, TResult> selector)
		=> Select(selector);

	/// <inheritdoc/>
	IEnumerable<TSource> IWhereMethod<ArrayGrouping<TSource, TKey>, TSource>.Where(Func<TSource, bool> predicate)
		=> Where(predicate);


	/// <inheritdoc/>
	public static bool operator ==(ArrayGrouping<TSource, TKey>? left, ArrayGrouping<TSource, TKey>? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(ArrayGrouping<TSource, TKey>? left, ArrayGrouping<TSource, TKey>? right)
		=> !(left == right);
}
