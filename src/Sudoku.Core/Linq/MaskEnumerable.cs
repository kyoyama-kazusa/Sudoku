namespace Sudoku.Linq;

/// <summary>
/// Represents a list of methods for iteration on a <see cref="Mask"/>.
/// </summary>
/// <seealso cref="Mask"/>
public static class MaskEnumerable
{
	/// <summary>
	/// Projects each bit from a specified mask, converting it into a (an) <typeparamref name="T"/> instance,
	/// with specified method to be called.
	/// </summary>
	/// <typeparam name="T">The target type of values for each bit converted.</typeparam>
	/// <param name="this">A mask instance.</param>
	/// <param name="selector">The selector method to be converted.</param>
	/// <returns>A list of converted result, encapsulated by a <see cref="ReadOnlySpan{T}"/> type.</returns>
	public static ReadOnlySpan<T> Select<T>(this Mask @this, Func<int, T> selector)
	{
		var (result, i) = (new T[BitOperations.PopCount(@this)], 0);
		foreach (var bit in @this)
		{
			result[i++] = selector(bit);
		}
		return result;
	}

	/// <summary>
	/// Projects each <see cref="Digit"/> of a <see cref="Mask"/> to a <see cref="CellMap"/>, flattens the resulting sequence into one sequence,
	/// and invokes a result selector function on each element therein.
	/// </summary>
	/// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
	/// <param name="source">A sequence of values to project.</param>
	/// <param name="collectionSelector">A transform function to apply to each element of the input <see cref="Mask"/>.</param>
	/// <param name="resultSelector">A transform function to apply to each element of the intermediate <see cref="CellMap"/>.</param>
	/// <returns>
	/// A <see cref="ReadOnlySpan{T}"/> whose elements are the result of invoking the one-to-many transform function
	/// <paramref name="collectionSelector"/> on each element of <paramref name="source"/> and then mapping each of those sequences
	/// and their corresponding source element to a result element.
	/// </returns>
	public static ReadOnlySpan<TResult> SelectMany<TResult>(
		this Mask source,
		Func<Digit, CellMap> collectionSelector,
		Func<Digit, Cell, TResult> resultSelector
	)
	{
		var result = new List<TResult>(BitOperations.PopCount(source) << 1);
		foreach (var digit in source)
		{
			foreach (var cell in collectionSelector(digit))
			{
				result.AddRef(resultSelector(digit, cell));
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Filters bits via the specified condition.
	/// </summary>
	/// <param name="this">The mask type of bits.</param>
	/// <param name="predicate">The condition that filters bits, removing bits not satisfying the condition.</param>
	/// <returns>A new <see cref="Mask"/> result.</returns>
	public static Mask Where(this Mask @this, Func<int, bool> predicate)
	{
		var result = (Mask)0;
		foreach (var bitPos in @this)
		{
			if (predicate(bitPos))
			{
				result |= (Mask)(1 << bitPos);
			}
		}
		return result;
	}
}
