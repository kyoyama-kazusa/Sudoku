namespace Sudoku.Linq;

/// <summary>
/// Represents a list of LINQ methods that can operate with <see cref="CellMap"/> instances.
/// </summary>
/// <seealso cref="CellMap"/>
public static class CellMapEnumerable
{
	/// <summary>
	/// Finds the first cell that satisfies the specified condition.
	/// </summary>
	/// <param name="this">Indicates the current instance.</param>
	/// <param name="match">The condition to be used.</param>
	/// <returns>The first found cell or -1 if none found.</returns>
	public static Cell First(this in CellMap @this, Func<Cell, bool> match)
	{
		foreach (var cell in @this.Offsets)
		{
			if (match(cell))
			{
				return cell;
			}
		}
		return -1;
	}

	/// <summary>
	/// Finds the first cell that satisfies the specified condition.
	/// </summary>
	/// <param name="this">Indicates the current instance.</param>
	/// <param name="grid">The grid to be used.</param>
	/// <param name="match">The condition to be used.</param>
	/// <returns>The first found cell or -1 if none found.</returns>
	public static Cell First(this in CellMap @this, in Grid grid, CellMapOrCandidateMapPredicate<CellMap, Cell> match)
	{
		foreach (var cell in @this.Offsets)
		{
			if (match(cell, grid))
			{
				return cell;
			}
		}
		return -1;
	}

	/// <summary>
	/// Projects each element in the current instance into the target-typed <typeparamref name="TResult"/> array,
	/// using the specified function to convert.
	/// </summary>
	/// <typeparam name="TResult">The type of target value.</typeparam>
	/// <param name="this">The current instance.</param>
	/// <param name="selector">The selector.</param>
	/// <returns>An array of <typeparamref name="TResult"/> elements.</returns>
	public static ReadOnlySpan<TResult> Select<TResult>(this scoped in CellMap @this, Func<Cell, TResult> selector)
	{
		var (result, i) = (new TResult[@this.Count], 0);
		foreach (var cell in @this.Offsets)
		{
			result[i++] = selector(cell);
		}
		return result;
	}

	/// <summary>
	/// Filters a <see cref="CellMap"/> collection based on a predicate.
	/// </summary>
	/// <param name="this">The current instance.</param>
	/// <param name="predicate">A function to test each element for a condition.</param>
	/// <returns>
	/// A <see cref="CellMap"/> that contains elements from the input <see cref="CellMap"/> satisfying the condition.
	/// </returns>
	public static CellMap Where(this in CellMap @this, Func<Cell, bool> predicate)
	{
		var result = @this;
		foreach (var cell in @this.Offsets)
		{
			if (!predicate(cell))
			{
				result.Remove(cell);
			}
		}
		return result;
	}

	/// <summary>
	/// Groups the elements of a sequence according to a specified key selector function.
	/// </summary>
	/// <typeparam name="TKey">
	/// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" path="/typeparam[@name='TKey']"/>
	/// </typeparam>
	/// <param name="this">The current instance.</param>
	/// <param name="keySelector">
	/// <inheritdoc cref="Enumerable.GroupBy{TSource, TKey}(IEnumerable{TSource}, Func{TSource, TKey})" path="/param[@name='keySelector']"/>
	/// </param>
	/// <returns>
	/// A list of <see cref="CellMapOrCandidateMapGrouping{TMap, TElement, TKey}"/> instances where each value object contains a sequence of objects and a key.
	/// </returns>
	/// <seealso cref="CellMapOrCandidateMapGrouping{TMap, TElement, TKey}"/>
	public static ReadOnlySpan<CellMapOrCandidateMapGrouping<CellMap, Cell, TKey>> GroupBy<TKey>(
		this scoped in CellMap @this,
		Func<Cell, TKey> keySelector
	) where TKey : notnull
	{
		var dictionary = new Dictionary<TKey, CellMap>();
		foreach (var cell in @this)
		{
			var key = keySelector(cell);
			if (!dictionary.TryAdd(key, cell.AsCellMap()))
			{
				var originalElement = dictionary[key];
				originalElement.Add(cell);
				dictionary[key] = originalElement;
			}
		}

		var result = new CellMapOrCandidateMapGrouping<CellMap, Cell, TKey>[dictionary.Count];
		var i = 0;
		foreach (var kvp in dictionary)
		{
			ref readonly var key = ref kvp.KeyRef;
			ref readonly var value = ref kvp.ValueRef;
			result[i++] = new(key, value);
		}
		return result;
	}

	/// <summary>
	/// Projects each cell (of type <see cref="Cell"/>) of a <see cref="CellMap"/> to a mask (of type <see cref="Mask"/>),
	/// flattens the resulting sequence into one sequence, and invokes a result selector function on each element therein.
	/// </summary>
	/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
	/// <param name="this">A sequence of values to project.</param>
	/// <param name="collectionSelector">A transform function to apply to each element of the input <see cref="CellMap"/>.</param>
	/// <param name="resultSelector">A transform function to apply to each element of the intermediate mask (of type <see cref="Mask"/>).</param>
	/// <returns>
	/// A <see cref="ReadOnlySpan{T}"/> of <typeparamref name="TResult"/> whose elements are the result
	/// of invoking the one-to-many transform function <paramref name="collectionSelector"/> on each element of <paramref name="this"/>
	/// and then mapping each of those sequences and their corresponding source element to a result element.
	/// </returns>
	public static ReadOnlySpan<TResult> SelectMany<TResult>(
		this in CellMap @this,
		Func<Cell, Mask> collectionSelector,
		Func<Cell, Digit, TResult> resultSelector
	)
	{
		var result = new List<TResult>(@this.Count << 1);
		foreach (var cell in @this)
		{
			foreach (var digit in collectionSelector(cell))
			{
				result.AddRef(resultSelector(cell, digit));
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Indicates whether at least one element satisfies the specified condition.
	/// </summary>
	/// <param name="this">The cell to be checked.</param>
	/// <param name="match">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating whether at least one element satisfies the specified condition.</returns>
	public static bool Any(this in CellMap @this, Func<Cell, bool> match)
	{
		foreach (var cell in @this)
		{
			if (match(cell))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Determine whether all <see cref="Cell"/>s satisfy the specified condition.
	/// </summary>
	/// <param name="this">The candidate to be checked.</param>
	/// <param name="match">The match method.</param>
	/// <returns>A <see cref="bool"/> result indicating whether all elements satisfy the specified condition.</returns>
	public static bool All(this in CellMap @this, Func<Cell, bool> match)
	{
		foreach (var cell in @this)
		{
			if (!match(cell))
			{
				return false;
			}
		}
		return true;
	}
}
