namespace Sudoku.Drawing;

/// <summary>
/// Provides with a data structure that displays a view for basic information.
/// </summary>
public sealed class View :
	HashSet<ViewNode>,
	IEquatable<View>,
	IExceptMethod<View, ViewNode>,
	IEqualityOperators<View, View, bool>,
	IFirstLastMethod<View, ViewNode>,
	IInfiniteSet<View, ViewNode>,
	IOfTypeMethod<View, ViewNode>,
	ISelectMethod<View, ViewNode>,
	IWhereMethod<View, ViewNode>
{
	/// <summary>
	/// Indicates an empty <see cref="View"/> instance. You can use this property to create a new instance.
	/// </summary>
	public static View Empty => [];

	/// <summary>
	/// Represents a <see cref="IEqualityComparer"/> of <see cref="View"/> type that can compares equality of a whole set
	/// with specified equality comparison rules on each element of type <see cref="ViewNode"/>.
	/// </summary>
	/// <seealso cref="IEqualityComparer{T}"/>
	/// <seealso cref="ViewNode"/>
	public static IEqualityComparer<View> SetComparer => field ??= CreateSetComparer();


	/// <summary>
	/// Adds a list of <see cref="ViewNode"/>s into the collection.
	/// </summary>
	/// <typeparam name="TViewNode">The type of each element.</typeparam>
	/// <param name="nodes">A list of <see cref="ViewNode"/> instance.</param>
	public void AddRange<TViewNode>(ReadOnlySpan<TViewNode> nodes) where TViewNode : ViewNode
	{
		foreach (var node in nodes)
		{
			Add(node);
		}
	}

	/// <summary>
	/// Try to find the candidate whose cell is specified one.
	/// </summary>
	/// <param name="cell">The cell to be found.</param>
	/// <returns>The found node; or <see langword="null"/> if none found.</returns>
	public CellViewNode? FindCell(Cell cell)
	{
		foreach (var node in OfType<CellViewNode>())
		{
			if (node.Cell == cell)
			{
				return node;
			}
		}
		return null;
	}

	/// <summary>
	/// Try to find the candidate whose candidate is specified one.
	/// </summary>
	/// <param name="candidate">The candidate to be found.</param>
	/// <returns>The found node; or <see langword="null"/> if none found.</returns>
	public CandidateViewNode? FindCandidate(Candidate candidate)
	{
		foreach (var node in OfType<CandidateViewNode>())
		{
			if (node.Candidate == candidate)
			{
				return node;
			}
		}
		return null;
	}

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as View);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] View? other) => SetComparer.Equals(this, other);

	/// <inheritdoc/>
	public override int GetHashCode() => SetComparer.GetHashCode(this);

	/// <inheritdoc cref="IExceptMethod{TSelf, TSource}.Except(IEnumerable{TSource})"/>
	public View ExceptWith(View other)
	{
		var result = Clone(ViewCloningOption.Default);
		result.ExceptWith(other.AsEnumerable());
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="View"/> instance with same values as the current instance,
	/// with all nodes cloned with new instances.
	/// </summary>
	/// <returns>A new <see cref="View"/> instance.</returns>
	public View Clone() => Clone(ViewCloningOption.IncludingNodes);

	/// <summary>
	/// Creates a new <see cref="View"/> instance with same values as the current instance,
	/// with specified option to define the cloning behavior.
	/// </summary>
	/// <param name="option">The option to define the cloning behavior.</param>
	/// <returns>A new <see cref="View"/> instance.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Throws when argument <paramref name="option"/> isn't defined.</exception>
	public View Clone(ViewCloningOption option)
		=> Count == 0
			? Empty
			: option switch
			{
				ViewCloningOption.Default => [.. this],
				ViewCloningOption.IncludingNodes => [.. from node in this select node.Clone()],
				_ => throw new ArgumentOutOfRangeException(nameof(option))
			};

	/// <summary>
	/// Try to convert this collection as a <see cref="ReadOnlySpan{T}"/> instance.
	/// </summary>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> instance.</returns>
	public ReadOnlySpan<ViewNode> AsSpan() => from node in this select node;

	/// <summary>
	/// Projects with a new transform of elements.
	/// </summary>
	/// <typeparam name="T">The type of target element.</typeparam>
	/// <param name="selector">The method to transform each element.</param>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> of <typeparamref name="T"/> elements.</returns>
	public ReadOnlySpan<T> Select<T>(Func<ViewNode, T> selector)
	{
		var result = new List<T>(Count);
		foreach (var element in this)
		{
			result.AddRef(selector(element));
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Filters a <see cref="View"/>, only reserves the <see cref="ViewNode"/> instances satisfying the specified condition.
	/// </summary>
	/// <param name="predicate">The filter.</param>
	/// <returns>A list of <see cref="ViewNode"/> filtered.</returns>
	public ReadOnlySpan<ViewNode> Where(Func<ViewNode, bool> predicate)
	{
		var result = new List<ViewNode>(Count);
		foreach (var element in this)
		{
			if (predicate(element))
			{
				result.Add(element);
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Filters the view nodes, only returns nodes of type <typeparamref name="TViewNode"/>.
	/// </summary>
	/// <typeparam name="TViewNode">The type of the node.</typeparam>
	/// <returns>The target collection of element type <typeparamref name="TViewNode"/>.</returns>
	public ReadOnlySpan<TViewNode> OfType<TViewNode>() where TViewNode : ViewNode
	{
		var result = new List<TViewNode>();
		foreach (var node in this)
		{
			if (node is TViewNode casted)
			{
				result.Add(casted);
			}
		}
		return result.AsSpan();
	}

	/// <returns>
	/// The first element that matches the conditions defined by the specified predicate, if found;
	/// otherwise, throw an <see cref="InvalidOperationException"/>.
	/// </returns>
	/// <exception cref="InvalidOperationException">
	/// Throws when the sequence has no elements satisfying the specified rule.
	/// </exception>
	/// <inheritdoc cref="FirstOrDefault(Func{ViewNode, bool})"/>
	public ViewNode First(Func<ViewNode, bool> match) => FirstOrDefault(match)!;

	/// <summary>
	/// Searches for an element that matches the conditions defined by the specified predicate,
	/// and returns the first occurrence within the entire <see cref="View"/>.
	/// </summary>
	/// <param name="match">The <see cref="Func{T, TResult}"/> delegate that defines the conditions of the element to search for.</param>
	/// <returns>
	/// The first element that matches the conditions defined by the specified predicate, if found; otherwise, <see langword="null"/>.
	/// </returns>
	public ViewNode? FirstOrDefault(Func<ViewNode, bool> match)
	{
		foreach (var element in this)
		{
			if (match(element))
			{
				return element;
			}
		}
		return null;
	}

	/// <inheritdoc/>
	ViewNode IFirstLastMethod<View, ViewNode>.First() => this[0];

	/// <inheritdoc/>
	ViewNode IFirstLastMethod<View, ViewNode>.First(Func<ViewNode, bool> predicate) => First(predicate);

	/// <inheritdoc/>
	ViewNode? IFirstLastMethod<View, ViewNode>.FirstOrDefault() => this.FirstOrDefault();

	/// <inheritdoc/>
	ViewNode IFirstLastMethod<View, ViewNode>.FirstOrDefault(ViewNode defaultValue) => this.FirstOrDefault() ?? defaultValue;

	/// <inheritdoc/>
	ViewNode? IFirstLastMethod<View, ViewNode>.FirstOrDefault(Func<ViewNode, bool> predicate) => FirstOrDefault(predicate);

	/// <inheritdoc/>
	ViewNode IFirstLastMethod<View, ViewNode>.FirstOrDefault(Func<ViewNode, bool> predicate, ViewNode defaultValue)
		=> FirstOrDefault(predicate) ?? defaultValue;

	/// <inheritdoc/>
	IEnumerable<ViewNode> IWhereMethod<View, ViewNode>.Where(Func<ViewNode, bool> predicate) => Where(predicate).ToArray();

	/// <inheritdoc/>
	IEnumerable<ViewNode> IExceptMethod<View, ViewNode>.Except(IEnumerable<ViewNode> second) => ExceptWith([.. second]);

	/// <inheritdoc/>
	IEnumerable<ViewNode> IExceptMethod<View, ViewNode>.Except(IEnumerable<ViewNode> second, IEqualityComparer<ViewNode>? comparer)
		=> ((IExceptMethod<View, ViewNode>)this).Except(second);

	/// <inheritdoc/>
	IEnumerable<TResult> ISelectMethod<View, ViewNode>.Select<TResult>(Func<ViewNode, TResult> selector)
		=> Select(selector).ToArray();

	/// <inheritdoc/>
	IEnumerable<TResult> IOfTypeMethod<View, ViewNode>.OfType<TResult>() => OfType<TResult>().ToArray();


	/// <summary>
	/// Performs bitwise-and operation and assign the value to the current instance.
	/// </summary>
	/// <param name="value">The instance.</param>
	public void operator &=(View value) => IntersectWith(value);

	/// <summary>
	/// Performs bitwise-or operation and assign the value to the current instance.
	/// </summary>
	/// <param name="value">The instance.</param>
	public void operator |=(View value) => UnionWith(value);

	/// <summary>
	/// Performs bitwise-exclusive-or operation and assign the value to the current instance.
	/// </summary>
	/// <param name="value">The instance.</param>
	public void operator ^=(View value) => SymmetricExceptWith(value);


	/// <inheritdoc/>
	public static bool operator ==(View? left, View? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(View? left, View? right) => !(left == right);

	/// <summary>
	/// Creates a <see cref="View"/> whose elements contains both <paramref name="left"/> and <paramref name="right"/>.
	/// </summary>
	/// <param name="left">The left-side <see cref="View"/> instance.</param>
	/// <param name="right">The right-side <see cref="View"/> instance.</param>
	/// <returns>A <see cref="View"/> result created.</returns>
	public static View operator &(View left, View right)
	{
		var result = left.Clone(ViewCloningOption.Default);
		result.IntersectWith(right);
		return result;
	}

	/// <summary>
	/// Merges two <see cref="View"/> instances into one <see cref="View"/>.
	/// </summary>
	/// <param name="left">Indicates the left-side <see cref="View"/> instance.</param>
	/// <param name="right">Indicates the right-side <see cref="View"/> instance.</param>
	/// <returns>A <see cref="View"/> result merged.</returns>
	public static View operator |(View left, View right)
	{
		var result = left.Clone(ViewCloningOption.Default);
		result.UnionWith(right);
		return result;
	}

	/// <summary>
	/// Creates a <see cref="View"/> instance, whose elements is from two <see cref="View"/> collections
	/// <paramref name="left"/> and <paramref name="right"/>, with only one-side containing this element.
	/// </summary>
	/// <param name="left">The left-side <see cref="View"/> instance.</param>
	/// <param name="right">The right-side <see cref="View"/> instance.</param>
	/// <returns>A <see cref="View"/> result created.</returns>
	public static View operator ^(View left, View right)
	{
		var result = left.Clone(ViewCloningOption.Default);
		result.SymmetricExceptWith(right);
		return result;
	}
}
